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
        private readonly string _cnn;

        public BitacoraDao(string connectionString)
        {
            _cnn = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        private static BitacoraDao _instance;
        private static readonly object _lock = new object();

        public static BitacoraDao GetInstance(string connectionString)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new BitacoraDao(connectionString);
                }
            }
            return _instance;
        }

        public long Add(BitacoraEntry e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));

            using (var cn = new SqlConnection(_cnn))
            using (var cmd = new SqlCommand("seg.Bitacora_Insert", cn) { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("@Fecha", e.Fecha == default ? DateTime.UtcNow : e.Fecha);
                cmd.Parameters.AddWithValue("@UsuarioId", (object)e.UsuarioId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Usuario", (object)e.Usuario ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Modulo", (object)e.Modulo ?? "General");
                cmd.Parameters.AddWithValue("@Accion", (object)e.Accion ?? "SinAccion");
                cmd.Parameters.AddWithValue("@Severidad", e.Severidad);
                cmd.Parameters.AddWithValue("@Mensaje", (object)e.Mensaje ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IP", (object)e.IP ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Host", (object)e.Host ?? DBNull.Value);

                cn.Open();
                var scalar = cmd.ExecuteScalar();
                return (scalar == null || scalar == DBNull.Value) ? 0L : Convert.ToInt64(scalar);
            }
        }

        public (IList<BitacoraEntry> rows, int total) Search(
            DateTime? desde,
            DateTime? hasta,
            byte? severidad,
            string modulo,
            string accion,
            string usuario,
            string texto,
            int page,
            int pageSize,
            string orderBy
        )
        {
            var list = new List<BitacoraEntry>();
            int total = 0;

            using (var cn = new SqlConnection(_cnn))
            using (var cmd = new SqlCommand("seg.Bitacora_Search", cn) { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("@FechaDesde", (object)desde ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaHasta", (object)hasta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Severidad", (object)severidad ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Modulo", (object)modulo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Accion", (object)accion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Usuario", (object)usuario ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Texto", (object)texto ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Page", page);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        if (total == 0 && rd["TotalRows"] != DBNull.Value)
                            total = Convert.ToInt32(rd["TotalRows"]);

                        list.Add(new BitacoraEntry
                        {
                            Id = rd.GetInt64(rd.GetOrdinal("Id")),
                            Fecha = rd.GetDateTime(rd.GetOrdinal("Fecha")),
                            UsuarioId = rd["UsuarioId"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["UsuarioId"]),
                            Usuario = rd["Usuario"] == DBNull.Value ? null : rd["Usuario"].ToString(),
                            Modulo = rd["Modulo"].ToString(),
                            Accion = rd["Accion"].ToString(),
                            Severidad = Convert.ToByte(rd["Severidad"]),
                            Mensaje = rd["Mensaje"] == DBNull.Value ? null : rd["Mensaje"].ToString(),
                            IP = rd["IP"] == DBNull.Value ? null : rd["IP"].ToString(),
                            Host = rd["Host"] == DBNull.Value ? null : rd["Host"].ToString()
                        });
                    }
                }
            }
            return (list, total);
        }

        public IList<string> GetUsuarios()
        {
            var result = new List<string>();
            const string sql = "SELECT DISTINCT Usuario FROM seg.Bitacora WHERE Usuario IS NOT NULL ORDER BY Usuario";

            using (var cn = new SqlConnection(_cnn))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        result.Add(rd.GetString(0));
                }
            }
            return result;
        }
    }
}
