using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using BE;
using Services;

namespace DAL
{
    public class DVVDao
    {
        // Singleton
        private static DVVDao _instance;
        public static DVVDao GetInstance() => _instance ?? (_instance = new DVVDao());
        private DVVDao() { }

        // ÚNICO cambio importante: conexión siempre desde DBDao
        private static string Conn => Services.AppConn.Get();

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
        public string CalculateDVV(string tabla, bool useNoLock = true, int timeout = 120, string pkColumn = null)
        {
            if (string.IsNullOrWhiteSpace(tabla))
                throw new ArgumentException("tabla requerida", nameof(tabla));

            // Separar esquema.nombre (default dbo)
            string schema, name;
            var parts = tabla.Split(new[] { '.' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2) { schema = parts[0]; name = parts[1]; }
            else { schema = "dbo"; name = parts[0]; }

            using (var cn = ConnectionFactory.Open())
            {
                // 1) Detectar PK real (soporta compuesta)
                var pkCols = new List<string>();
                const string pkSql = @"
                    SELECT c.name
                    FROM sys.key_constraints k
                    JOIN sys.index_columns ic
                      ON ic.object_id = k.parent_object_id AND ic.index_id = k.unique_index_id
                    JOIN sys.columns c
                      ON c.object_id = ic.object_id AND c.column_id = ic.column_id
                    JOIN sys.tables t
                      ON t.object_id = k.parent_object_id
                    JOIN sys.schemas s
                      ON s.schema_id = t.schema_id
                    WHERE k.type = 'PK' AND s.name = @schema AND t.name = @table
                    ORDER BY ic.key_ordinal;";

                using (var cmd = new SqlCommand(pkSql, cn))
                {
                    cmd.Parameters.AddWithValue("@schema", schema);
                    cmd.Parameters.AddWithValue("@table", name);
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read()) pkCols.Add(rd.GetString(0));
                }

                // 2) Validar si me pasaron pkColumn y existe; si no, usar la PK detectada
                string orderBy;
                if (!string.IsNullOrWhiteSpace(pkColumn))
                {
                    // normalizar y validar contra columnas reales de la tabla
                    var given = pkColumn.Split(',')
                                        .Select(s => s.Trim().Trim('[', ']'))
                                        .Where(s => !string.IsNullOrWhiteSpace(s))
                                        .ToList();

                    var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    const string colsSql = @"
                        SELECT c.name
                        FROM sys.columns c
                        JOIN sys.tables t ON t.object_id = c.object_id
                        JOIN sys.schemas s ON s.schema_id = t.schema_id
                        WHERE s.name = @schema AND t.name = @table;";

                    using (var cmd = new SqlCommand(colsSql, cn))
                    {
                        cmd.Parameters.AddWithValue("@schema", schema);
                        cmd.Parameters.AddWithValue("@table", name);
                        using (var rd = cmd.ExecuteReader())
                            while (rd.Read()) cols.Add(rd.GetString(0));
                    }

                    bool allExist = given.All(g => cols.Contains(g));
                    if (allExist)
                        orderBy = string.Join(", ", given.Select(g => $"[{g}]"));
                    else if (pkCols.Count > 0)
                        orderBy = string.Join(", ", pkCols.Select(c => $"[{c}]"));
                    else
                        orderBy = cols.Any() ? $"[{cols.First()}]" : "[(select 1)]"; // fallback extremo
                }
                else
                {
                    // no vino pkColumn: usar la PK detectada; si no hay PK, primer columna
                    if (pkCols.Count > 0)
                        orderBy = string.Join(", ", pkCols.Select(c => $"[{c}]"));
                    else
                    {
                        string firstCol = null;
                        const string firstColSql = @"
                            SELECT TOP(1) c.name
                            FROM sys.columns c
                            JOIN sys.tables t ON t.object_id = c.object_id
                            JOIN sys.schemas s ON s.schema_id = t.schema_id
                            WHERE s.name = @schema AND t.name = @table
                            ORDER BY c.column_id;";

                        using (var cmd = new SqlCommand(firstColSql, cn))
                        {
                            cmd.Parameters.AddWithValue("@schema", schema);
                            cmd.Parameters.AddWithValue("@table", name);
                            firstCol = (string)cmd.ExecuteScalar();
                        }
                        orderBy = !string.IsNullOrWhiteSpace(firstCol) ? $"[{firstCol}]" : "[(select 1)]";
                    }
                }

                // 3) Armar DVV determinístico
                var hint = useNoLock ? "WITH (NOLOCK)" : "";
                var sql = $@"
                    DECLARE @concat nvarchar(max);

                    SELECT @concat =
                        STRING_AGG(CONVERT(nvarchar(max), DVH), N'|')
                            WITHIN GROUP (ORDER BY {orderBy})
                    FROM [{schema}].[{name}] {hint};

                    IF @concat IS NULL SET @concat = N'';
                    SELECT CONVERT(varchar(64), HASHBYTES('SHA2_256', @concat), 2);";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.CommandTimeout = timeout;
                    var result = cmd.ExecuteScalar();
                    return result?.ToString() ?? "";
                }
            }
        }


        // Upsert DVV con timeout configurable
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
