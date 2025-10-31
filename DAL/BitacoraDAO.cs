using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Services;

namespace DAL
{
    public class BitacoraDao
    {
        private static BitacoraDao _inst;
        private BitacoraDao() { }
        public static BitacoraDao GetInstance() => _inst ?? (_inst = new BitacoraDao());

        private static string Cnn() => ConnectionFactory.Current;

        // ---------- ALTAS ----------
        public bool Add(int? usuarioId, string modulo, string accion, int? severidad, string mensaje, string ip, string host, DateTime? fecha = null)
        {
            DateTime f = fecha ?? DateTime.Now;

            // DVH: Fecha|UsuarioId|Modulo|Accion|Severidad|Mensaje|IP|Host
            string fila = string.Join("|", new[] {
                f.ToString("O"), usuarioId?.ToString(), modulo, accion,
                severidad?.ToString(), mensaje, ip, host
            });
            string dvh = DV.GetDV(fila);

            const string ins = @"
                INSERT INTO dbo.Bitacora
                    (Fecha, UsuarioId, Modulo, Accion, Severidad, Mensaje, IP, Host, DVH)
                VALUES
                    (@Fecha, @UsuarioId, @Modulo, @Accion, @Severidad, @Mensaje, @IP, @Host, @DVH);";

            using (var cn = new SqlConnection(Cnn()))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(ins, cn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Fecha", f);
                            cmd.Parameters.AddWithValue("@UsuarioId", (object)usuarioId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Modulo", (object)modulo ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Accion", (object)accion ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Severidad", (object)severidad ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Mensaje", (object)mensaje ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@IP", (object)ip ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Host", (object)host ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@DVH", (object)dvh ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }

                        string dvv = DVVDao.GetInstance().CalculateDVV("Bitacora");
                        UpsertDVV_tx(cn, tx, "Bitacora", dvv);

                        tx.Commit();
                        return true;
                    }
                    catch
                    {
                        tx.Rollback();
                        return false; // nunca romper el flujo por Bitácora
                    }
                }
            }
        }

        // Helpers prácticos
        public bool AddLoginOk(int? usuarioId, string ip = null, string host = null)
            => Add(usuarioId, "Seguridad", "Login", 1, "Login OK", ip, host);

        public bool AddLoginFail(string userNameOrMail, string ip = null, string host = null)
            => Add(null, "Seguridad", "LoginFail", 2, $"Intento fallido de login: {userNameOrMail}", ip, host);

        public bool AddError(string modulo, string accion, string mensaje, int? usuarioId = null, string ip = null, string host = null)
            => Add(usuarioId, modulo, accion, 3, mensaje, ip, host);

        // ---------- CONSULTAS ----------
        public (DataTable Rows, int Total) Search(DateTime? desde, DateTime? hasta, string usuario, int page, int pageSize, string ordenar)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 50;

            var ps = new List<SqlParameter>();
            var where = new StringBuilder("WHERE 1=1 ");

            if (desde.HasValue) { where.Append("AND b.Fecha >= @d "); ps.Add(new SqlParameter("@d", desde.Value)); }
            if (hasta.HasValue) { where.Append("AND b.Fecha <= @h "); ps.Add(new SqlParameter("@h", hasta.Value)); }
            if (!string.IsNullOrWhiteSpace(usuario))
            {
                // 👇 Ajustado a tu esquema: u.Usuario y u.Mail
                where.Append("AND (u.Usuario = @u OR u.Mail = @u) ");
                ps.Add(new SqlParameter("@u", usuario));
            }

            string orderBy = string.IsNullOrWhiteSpace(ordenar) ? "b.Fecha DESC" : ordenar;

            string sqlCount = $@"
                SELECT COUNT(*)
                FROM dbo.Bitacora b LEFT JOIN dbo.Usuario u ON u.IdUsuario = b.UsuarioId
                {where}";

            string sqlPage = $@"
                SELECT b.IdBitacora, b.Fecha, b.UsuarioId, u.Usuario AS Usuario,
                       b.Modulo, b.Accion, b.Severidad, b.Mensaje, b.IP, b.Host
                FROM dbo.Bitacora b LEFT JOIN dbo.Usuario u ON u.IdUsuario = b.UsuarioId
                {where}
                ORDER BY {orderBy}
                OFFSET {(page - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY;";

            int total;
            using (var cn = new SqlConnection(Cnn()))
            using (var cmd = new SqlCommand(sqlCount, cn))
            {
                cmd.Parameters.AddRange(ps.ToArray());
                cn.Open();
                total = Convert.ToInt32(cmd.ExecuteScalar());
            }

            var dt = SqlHelpers.GetInstance(Cnn()).GetDataTable(sqlPage, ps);
            return (dt, total);
        }

        public (DataTable Rows, int Total) Search(
            DateTime? desde, DateTime? hasta,
            string usuario, string accion, string modulo, int? severidad,
            int page, int pageSize, string ordenar)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 50;

            var ps = new List<SqlParameter>();
            var where = new StringBuilder("WHERE 1=1 ");

            if (desde.HasValue) { where.Append("AND b.Fecha >= @d "); ps.Add(new SqlParameter("@d", desde.Value)); }
            if (hasta.HasValue) { where.Append("AND b.Fecha <= @h "); ps.Add(new SqlParameter("@h", hasta.Value)); }
            if (!string.IsNullOrWhiteSpace(usuario)) { where.Append("AND (u.Usuario = @u OR u.Mail = @u) "); ps.Add(new SqlParameter("@u", usuario)); }
            if (!string.IsNullOrWhiteSpace(accion)) { where.Append("AND b.Accion = @ac "); ps.Add(new SqlParameter("@ac", accion)); }
            if (!string.IsNullOrWhiteSpace(modulo)) { where.Append("AND b.Modulo = @m "); ps.Add(new SqlParameter("@m", modulo)); }
            if (severidad.HasValue) { where.Append("AND b.Severidad = @sev "); ps.Add(new SqlParameter("@sev", severidad.Value)); }

            string orderBy = string.IsNullOrWhiteSpace(ordenar) ? "b.Fecha DESC" : ordenar;

            string sqlCount = $@"
                SELECT COUNT(*)
                FROM dbo.Bitacora b LEFT JOIN dbo.Usuario u ON u.IdUsuario = b.UsuarioId
                {where}";

            string sqlPage = $@"
                SELECT b.IdBitacora, b.Fecha, b.UsuarioId, u.Usuario AS Usuario,
                       b.Modulo, b.Accion, b.Severidad, b.Mensaje, b.IP, b.Host
                FROM dbo.Bitacora b LEFT JOIN dbo.Usuario u ON u.IdUsuario = b.UsuarioId
                {where}
                ORDER BY {orderBy}
                OFFSET {(page - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY;";

            int total;
            using (var cn = new SqlConnection(Cnn()))
            using (var cmd = new SqlCommand(sqlCount, cn))
            {
                cmd.Parameters.AddRange(ps.ToArray());
                cn.Open();
                total = Convert.ToInt32(cmd.ExecuteScalar());
            }

            var dt = SqlHelpers.GetInstance(Cnn()).GetDataTable(sqlPage, ps);
            return (dt, total);
        }

        // ---------- Para combos/filtros ----------
        public List<string> GetUsuarios()
        {
            // 👇 Ajustado a tu esquema: columna 'Usuario'
            const string sql = @"
                SELECT DISTINCT u.Usuario
                FROM dbo.Usuario u
                WHERE u.Usuario IS NOT NULL AND LEN(u.Usuario) > 0
                ORDER BY u.Usuario;";

            var list = new List<string>();
            using (var cn = new SqlConnection(Cnn()))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                    while (dr.Read())
                        list.Add(dr.GetString(0));
            }
            return list;
        }

        // ---------- Helpers ----------
        private static void UpsertDVV_tx(SqlConnection cn, SqlTransaction tx, string tabla, string dvv)
        {
            const string check = "SELECT COUNT(*) FROM DVV WHERE Tabla = @t";
            using (var cmd = new SqlCommand(check, cn, tx))
            {
                cmd.Parameters.AddWithValue("@t", tabla);
                int c = Convert.ToInt32(cmd.ExecuteScalar());

                string sql = c > 0
                    ? "UPDATE DVV SET DVV = @d WHERE Tabla = @t"
                    : "INSERT INTO DVV (Tabla, DVV) VALUES (@t, @d)";

                using (var up = new SqlCommand(sql, cn, tx))
                {
                    up.Parameters.AddWithValue("@t", tabla);
                    up.Parameters.AddWithValue("@d", (object)dvv ?? DBNull.Value);
                    up.ExecuteNonQuery();
                }
            }
        }
    }
}
