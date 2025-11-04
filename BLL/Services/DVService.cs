using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BE;
using BLL.Contracts;
using DAL;

namespace BLL.Services
{
    public class DVService : IDVServices
    {
        private readonly DVVDao _dvvDao = DVVDao.GetInstance();

        // ===== API original =====
        public string RecalcularDVV(string tabla) => _dvvDao.CalculateDVV(tabla);

        public string ObtenerDVV(string tabla)
        {
            var todos = _dvvDao.GetAllDVV();
            var row = todos.Find(d => string.Equals(d.tabla, tabla, StringComparison.OrdinalIgnoreCase));
            return row != null ? row.dvv : null;
        }

        public bool VerificarTabla(string tabla, out string dvvCalculado, out string dvvGuardado)
        {
            dvvCalculado = _dvvDao.CalculateDVV(tabla);
            dvvGuardado = ObtenerDVV(tabla);
            return !string.IsNullOrEmpty(dvvGuardado) &&
                   string.Equals(dvvCalculado, dvvGuardado, StringComparison.Ordinal);
        }

        // ===== Métodos extra =====
        public bool GuardarDVV(string tabla, string dvv)
            => _dvvDao.AddUpdateDVV(new DVV { tabla = tabla, dvv = dvv });

        public List<DVDiscrepancia> VerificarDVH(string tabla)
        {
            var result = new List<DVDiscrepancia>();
            DataTable dt = GetTable(tabla);

            var colsDv = dt.Columns.Cast<DataColumn>()
                                   .Where(c => !string.Equals(c.ColumnName, "DVH", StringComparison.OrdinalIgnoreCase))
                                   .ToList();

            var pkCols = GetPkColumns(tabla);
            if (pkCols.Count == 0) pkCols = GuessFirstColumnAsPk(dt);

            foreach (DataRow row in dt.Rows)
            {
                string actual = (dt.Columns.Contains("DVH") && row["DVH"] != DBNull.Value)
                    ? Convert.ToString(row["DVH"])
                    : null;

                string calculado = ComputeDVH(row, colsDv);

                if (!string.Equals(actual, calculado, StringComparison.Ordinal))
                {
                    result.Add(new DVDiscrepancia
                    {
                        Tabla = tabla,
                        PK = BuildPkDisplay(row, pkCols),
                        DVHActual = actual,
                        DVHCalculado = calculado
                    });
                }
            }

            return result;
        }

        public int RecalcularDVH(string tabla)
        {
            DataTable dt = GetTable(tabla);

            var colsDv = dt.Columns.Cast<DataColumn>()
                                   .Where(c => !string.Equals(c.ColumnName, "DVH", StringComparison.OrdinalIgnoreCase))
                                   .ToList();

            var pkCols = GetPkColumns(tabla);
            if (pkCols.Count == 0) pkCols = GuessFirstColumnAsPk(dt);

            int updated = 0;

            using (var cn = ConnectionFactory.Open())
            {
                foreach (DataRow row in dt.Rows)
                {
                    string nuevo = ComputeDVH(row, colsDv);
                    string actual = (dt.Columns.Contains("DVH") && row["DVH"] != DBNull.Value)
                        ? Convert.ToString(row["DVH"])
                        : null;

                    if (!string.Equals(actual, nuevo, StringComparison.Ordinal))
                    {
                        var sql = new StringBuilder();
                        sql.Append("UPDATE ").Append(EscapeTable(tabla)).Append(" SET DVH=@dvh WHERE ");
                        for (int i = 0; i < pkCols.Count; i++)
                        {
                            if (i > 0) sql.Append(" AND ");
                            sql.Append('[').Append(pkCols[i]).Append("]=").Append("@pk").Append(i);
                        }

                        using (var cmd = new SqlCommand(sql.ToString(), cn))
                        {
                            cmd.Parameters.AddWithValue("@dvh", (object)nuevo ?? DBNull.Value);
                            for (int i = 0; i < pkCols.Count; i++)
                                cmd.Parameters.AddWithValue("@pk" + i, row[pkCols[i]] ?? DBNull.Value);

                            updated += cmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            return updated;
        }

        public bool RecalcularYGuardarDVV(string tabla, out string dvv)
        {
            dvv = _dvvDao.CalculateDVV(tabla);
            if (string.IsNullOrEmpty(dvv))
                return false;

            return _dvvDao.AddUpdateDVV(new DVV { tabla = tabla, dvv = dvv });
        }

        // ===== Helpers =====
        private static DataTable GetTable(string tabla)
        {
            var dt = new DataTable();
            using (var cn = ConnectionFactory.Open())
            using (var da = new SqlDataAdapter("SELECT * FROM " + EscapeTable(tabla), cn))
            {
                da.Fill(dt);
            }
            return dt;
        }

        private static string EscapeTable(string tabla)
        {
            var parts = tabla.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2) return "[" + parts[0] + "].[" + parts[1] + "]";
            return "[" + parts[0] + "]";
        }

        private static List<string> GuessFirstColumnAsPk(DataTable dt)
        {
            var list = new List<string>();
            if (dt.Columns.Count > 0) list.Add(dt.Columns[0].ColumnName);
            return list;
        }

        private static string BuildPkDisplay(DataRow row, List<string> pkCols)
        {
            var parts = new List<string>();
            foreach (var name in pkCols)
            {
                var v = row.Table.Columns.Contains(name) ? row[name] : null;
                parts.Add(name + "=" + (v == null || v == DBNull.Value ? "NULL" : v.ToString()));
            }
            return string.Join("; ", parts);
        }

        private static string ComputeDVH(DataRow row, IList<DataColumn> cols)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < cols.Count; i++)
            {
                var c = cols[i];
                var v = row[c] == DBNull.Value ? string.Empty : Convert.ToString(row[c]);
                if (i > 0) sb.Append('|');
                sb.Append(v);
            }

            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(sb.ToString());
                var hash = sha.ComputeHash(bytes);
                var hex = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++) hex.Append(hash[i].ToString("X2"));
                return hex.ToString();
            }
        }

        private static List<string> GetPkColumns(string tabla)
        {
            string schema = "dbo";
            string name = tabla;
            var parts = tabla.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2) { schema = parts[0]; name = parts[1]; }

            var list = new List<string>();
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

            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(pkSql, cn))
            {
                cmd.Parameters.AddWithValue("@schema", schema);
                cmd.Parameters.AddWithValue("@table", name);
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read()) list.Add(rd.GetString(0));
            }
            return list;
        }
    }
}
