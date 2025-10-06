using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;
using Services;

namespace DAL
{
    public class BitacoraDao
    {
        private static string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigFile.txt");
        private static string _connString = Crypto.Decript(FileHelper.GetInstance(configFilePath).ReadFile());
        private static BitacoraDao _instance;

        #region Singleton
        public static BitacoraDao GetInstance()
        {
            if (_instance == null)
                _instance = new BitacoraDao();
            return _instance;
        }

        private BitacoraDao() { }
        #endregion

        #region ADD
        public bool Add(BE.Bitacora entry, BE.DVH dvh)
        {
            const string sql = @"
                INSERT INTO Bitacora
                    (Fecha, UsuarioId, Modulo, Accion, Severidad, Mensaje, IP, Host, DVH)
                VALUES
                    (@Fecha, @UsuarioId, @Modulo, @Accion, @Severidad, @Mensaje, @IP, @Host, @DVH)";

            try
            {
                var ps = new List<SqlParameter>
                {
                    new SqlParameter("@Fecha", entry.Fecha),
                    new SqlParameter("@UsuarioId", (object)(entry.Usuario?.Id ?? (int?)null) ?? DBNull.Value),
                    new SqlParameter("@Modulo", (object)entry?.strCritc ?? DBNull.Value),   // si no usás Modulo, podés dejar NULL
                    new SqlParameter("@Accion", DBNull.Value),
                    new SqlParameter("@Severidad", entry.Criticidad?.Id ?? (object)DBNull.Value),
                    new SqlParameter("@Mensaje", entry.Descripcion ?? (object)DBNull.Value),
                    new SqlParameter("@IP", DBNull.Value),
                    new SqlParameter("@Host", DBNull.Value),
                    new SqlParameter("@DVH", (object)dvh?.dvh ?? DBNull.Value)
                };

                int rows = SqlHelpers.GetInstance(_connString).ExecuteQuery(sql, ps);

                if (rows > 0)
                {
                    DVVDao.GetInstance().AddUpdateDVV(new BE.DVV
                    {
                        tabla = "Bitacora",
                        dvv = DVVDao.GetInstance().CalculateDVV("Bitacora")
                    });
                    return true;
                }

                return false;
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region SEARCH
        /// <summary>
        /// Búsqueda paginada con filtros opcionales.
        /// </summary>
        public (IList<BitacoraEntry> rows, int total) Search(
     DateTime? desde, DateTime? hasta, string modulo,
     string accion, string mensaje, string usuario,
     byte? severidad, int page, int pageSize, string ordenarPor)
        {
            var list = new List<BitacoraEntry>();
            int total = 0;

            string selectCore = @"
                SELECT B.IdBitacora, B.Fecha, B.UsuarioId, U.Usuario AS Usuario, 
               B.Modulo, B.Accion, B.Severidad, B.Mensaje, B.IP, B.Host
                FROM Bitacora B
             LEFT JOIN Usuario U ON B.UsuarioId = U.IdUsuario
              WHERE 1=1";

            var where = new System.Text.StringBuilder();
            var ps = new List<SqlParameter>();

            if (desde.HasValue)
            {
                where.Append(" AND B.Fecha >= @desde");
                ps.Add(new SqlParameter("@desde", desde.Value));
            }
            if (hasta.HasValue)
            {
                where.Append(" AND B.Fecha <= @hasta");
                ps.Add(new SqlParameter("@hasta", hasta.Value));
            }
            if (!string.IsNullOrWhiteSpace(usuario))
            {
                where.Append(" AND (U.Usuario LIKE @usuario OR U.Mail LIKE @usuario)");
                ps.Add(new SqlParameter("@usuario", $"%{usuario}%"));
            }
            if (!string.IsNullOrWhiteSpace(mensaje))
            {
                where.Append(" AND B.Mensaje LIKE @mensaje");
                ps.Add(new SqlParameter("@mensaje", $"%{mensaje}%"));
            }
            if (severidad.HasValue)
            {
                where.Append(" AND B.Severidad = @sev");
                ps.Add(new SqlParameter("@sev", severidad.Value));
            }

            // Sanitizar ORDER BY (whitelist simple)
            string orderBy = "B.Fecha DESC";
            if (!string.IsNullOrWhiteSpace(ordenarPor))
            {
                var allowed = new HashSet<string>
                {
            "B.Fecha ASC","B.Fecha DESC",
            "U.Usuario ASC","U.Usuario DESC",
            "B.Severidad ASC","B.Severidad DESC"
                 };
                    var wanted = ordenarPor.Trim();
                if (allowed.Contains(wanted)) orderBy = wanted;
            }

            // Paginación con CTE
            int start = (page - 1) * pageSize + 1;
            int end = page * pageSize;

            string sqlPaged = $@"
                WITH Paged AS (
                SELECT ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, *
                FROM ({selectCore} {where}) AS InnerQuery
                )
                SELECT * FROM Paged WHERE RowNum BETWEEN @start AND @end";

            var psPaged = new List<SqlParameter>(ps)
                 {
                 new SqlParameter("@start", start),
                  new SqlParameter("@end", end)
                };

            var dtRows = Services.SqlHelpers.GetInstance(_connString).GetDataTable(sqlPaged, psPaged);

            foreach (DataRow dr in dtRows.Rows)
            {
                list.Add(new BitacoraEntry
                {
                    Id = Convert.ToInt64(dr["IdBitacora"]),
                    Fecha = Convert.ToDateTime(dr["Fecha"]),
                    UsuarioId = dr["UsuarioId"] == DBNull.Value ? null : (int?)Convert.ToInt32(dr["UsuarioId"]),
                    Usuario = dr["Usuario"]?.ToString(),
                    Modulo = dr["Modulo"]?.ToString(),
                    Accion = dr["Accion"]?.ToString(),
                    Severidad = Convert.ToByte(dr["Severidad"]),
                    Mensaje = dr["Mensaje"]?.ToString(),
                    IP = dr["IP"]?.ToString(),
                    Host = dr["Host"]?.ToString()
                });
            }
            string sqlCount = $"SELECT COUNT(1) FROM ({selectCore} {where}) AS C";
            var dtCount = Services.SqlHelpers.GetInstance(_connString).GetDataTable(sqlCount, ps);
            if (dtCount.Rows.Count > 0)
                total = Convert.ToInt32(dtCount.Rows[0][0]);

            return (list, total);
        }
        #endregion

        #region LOOKUP USUARIOS
        public IList<string> GetUsuarios()
        {
            const string sql = "SELECT DISTINCT U.Usuario FROM Usuario U INNER JOIN Bitacora B ON U.IdUsuario = B.UsuarioId ORDER BY U.Usuario";
            var dt = SqlHelpers.GetInstance(_connString).GetDataTable(sql);

            var result = new List<string>();
            foreach (DataRow r in dt.Rows)
                result.Add(r["Usuario"].ToString());
            return result;
        }
        #endregion
    }
}
