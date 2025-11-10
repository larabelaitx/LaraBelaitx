using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using BE;
using Services;

namespace DAL
{
    public class DVVDao
    {
        // ===== Singleton =====
        private static DVVDao _instance;
        public static DVVDao GetInstance() => _instance ?? (_instance = new DVVDao());
        private DVVDao() { }

        // Si usás AppConn:
        private static string Conn => Services.AppConn.Get();

        // ===== Upsert DVV (fuera de tx) =====
        public bool AddUpdateDVV(DVV dvv)
        {
            const string qCheck = "SELECT COUNT(*) FROM DVV WHERE Tabla=@Tabla";
            const string qIns = "INSERT INTO DVV (Tabla, DVV) VALUES (@Tabla, @DVV)";
            const string qUpd = "UPDATE DVV SET DVV=@DVV WHERE Tabla=@Tabla";

            if (dvv == null) return false;

            var p = new List<SqlParameter> {
                new SqlParameter("@Tabla", dvv.tabla),
                new SqlParameter("@DVV",   dvv.dvv ?? (object)DBNull.Value)
            };

            int count = Convert.ToInt32(SqlHelpers.GetInstance(Conn)
                            .ExecuteScalar(qCheck, new List<SqlParameter> { new SqlParameter("@Tabla", dvv.tabla) }));

            var sql = (count > 0) ? qUpd : qIns;
            return SqlHelpers.GetInstance(Conn).ExecuteQuery(sql, p) > 0;
        }

        public List<DVV> GetAllDVV()
        {
            const string q = "SELECT idDVV, Tabla, DVV FROM DVV";
            return DAL.Mappers.MPDVV.GetInstance()
                   .MapDVVs(SqlHelpers.GetInstance(Conn).GetDataTable(q));
        }

        // ===== Cálculo NO transaccional (instancia) =====
        public string CalculateDVV(string tabla, bool useNoLock = true, int timeout = 120, string pkColumn = null)
        {
            if (string.IsNullOrWhiteSpace(tabla))
                throw new ArgumentException("tabla requerida", nameof(tabla));

            string schema, name;
            var parts = tabla.Split(new[] { '.' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2) { schema = parts[0]; name = parts[1]; }
            else { schema = "dbo"; name = parts[0]; }

            using (var cn = ConnectionFactory.Open())
            {
                return CalculateDVV_tx(cn, null, $"{schema}.{name}", useNoLock, timeout, pkColumn);
            }
        }

        // ===== Cálculo transaccional (instancia) =====
        public string CalculateDVV_tx(SqlConnection cn, SqlTransaction tx, string tabla,
                                      bool useNoLock = false, int timeout = 120, string pkColumn = null)
        {
            if (string.IsNullOrWhiteSpace(tabla))
                throw new ArgumentException("tabla requerida", nameof(tabla));

            string schema, name;
            var parts = tabla.Split(new[] { '.' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2) { schema = parts[0]; name = parts[1]; }
            else { schema = "dbo"; name = parts[0]; }

            // 1) PK real
            var pkCols = new List<string>();
            const string pkSql = @"
                SELECT c.name
                FROM sys.key_constraints k
                JOIN sys.index_columns ic ON ic.object_id = k.parent_object_id AND ic.index_id = k.unique_index_id
                JOIN sys.columns c        ON c.object_id = ic.object_id AND c.column_id = ic.column_id
                JOIN sys.tables t         ON t.object_id = k.parent_object_id
                JOIN sys.schemas s        ON s.schema_id = t.schema_id
                WHERE k.type = 'PK' AND s.name = @schema AND t.name = @table
                ORDER BY ic.key_ordinal;";

            using (var cmd = new SqlCommand(pkSql, cn, tx))
            {
                cmd.Parameters.AddWithValue("@schema", schema);
                cmd.Parameters.AddWithValue("@table", name);
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read()) pkCols.Add(rd.GetString(0));
            }

            // 2) ORDER BY determinístico
            string orderBy;
            if (!string.IsNullOrWhiteSpace(pkColumn))
            {
                var given = pkColumn.Split(',')
                                    .Select(s => s.Trim().Trim('[', ']'))
                                    .Where(s => !string.IsNullOrWhiteSpace(s));

                var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                const string colsSql = @"
                    SELECT c.name
                    FROM sys.columns c
                    JOIN sys.tables t ON t.object_id = c.object_id
                    JOIN sys.schemas s ON s.schema_id = t.schema_id
                    WHERE s.name = @schema AND t.name = @table;";
                using (var cmd = new SqlCommand(colsSql, cn, tx))
                {
                    cmd.Parameters.AddWithValue("@schema", schema);
                    cmd.Parameters.AddWithValue("@table", name);
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read()) cols.Add(rd.GetString(0));
                }

                var ok = true;
                var lst = new List<string>();
                foreach (var g in given)
                {
                    if (!cols.Contains(g)) { ok = false; break; }
                    lst.Add($"[{g}]");
                }
                orderBy = ok
                    ? string.Join(", ", lst)
                    : (pkCols.Count > 0 ? string.Join(", ", pkCols.ConvertAll(c => $"[{c}]")) : "[Id]");
            }
            else
            {
                orderBy = pkCols.Count > 0 ? string.Join(", ", pkCols.ConvertAll(c => $"[{c}]")) : "[Id]";
            }

            // 3) HASH sobre DVH en orden estable
            var hint = useNoLock ? "WITH (NOLOCK)" : "";
            var sql = $@"
                DECLARE @concat nvarchar(max);
                SELECT @concat =
                    STRING_AGG(CONVERT(nvarchar(max), DVH), N'|')
                        WITHIN GROUP (ORDER BY {orderBy})
                FROM [{schema}].[{name}] {hint};

                IF @concat IS NULL SET @concat = N'';
                SELECT CONVERT(varchar(64), HASHBYTES('SHA2_256', @concat), 2);";

            using (var cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.CommandTimeout = timeout;
                var result = cmd.ExecuteScalar();
                return result?.ToString() ?? "";
            }
        }

        // ===== Upsert DVV (transaccional) — estático =====
        public static void UpsertDVV_tx(SqlConnection cn, SqlTransaction tx, string tabla, string dvv, int timeout = 120)
        {
            const string check = "SELECT COUNT(*) FROM DVV WHERE Tabla = @t";
            using (var cmd = new SqlCommand(check, cn, tx))
            {
                cmd.CommandTimeout = timeout;
                cmd.Parameters.AddWithValue("@t", tabla);
                int c = Convert.ToInt32(cmd.ExecuteScalar());

                string sql = c > 0
                    ? "UPDATE DVV SET DVV = @d WHERE Tabla = @t"
                    : "INSERT INTO DVV (Tabla, DVV) VALUES (@t, @d)";

                using (var up = new SqlCommand(sql, cn, tx))
                {
                    up.CommandTimeout = timeout;
                    up.Parameters.AddWithValue("@t", tabla);
                    up.Parameters.AddWithValue("@d", (object)dvv ?? DBNull.Value);
                    up.ExecuteNonQuery();
                }
            }
        }
    }
}
