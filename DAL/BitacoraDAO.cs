using System;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public sealed class BitacoraDao
    {
        private static BitacoraDao _inst;
        private static readonly object _lock = new object();

        public static BitacoraDao GetInstance()
        {
            if (_inst != null) return _inst;
            lock (_lock)
            {
                if (_inst == null) _inst = new BitacoraDao();
            }
            return _inst;
        }

        private BitacoraDao() { }

        // Mapea severidad “lógica” (1 Info, 2 Warn, 3 Error) a los IDs existentes en Criticidad
        private static int MapToSeveridad(int severidad)
        {
            if (severidad == 2) return 2; // Warning
            if (severidad == 3) return 3; // Error
            return 1;                     // Info por defecto
        }

        // Overload corto: usa UtcNow
        public int Add(int? usuarioId, string modulo, string accion, int severidad,
                       string mensaje, string ip, string host)
        {
            return Add(usuarioId, modulo, accion, severidad, mensaje, ip, host, DateTime.UtcNow);
        }

        // INSERT + DVH + DVV
        public int Add(int? usuarioId, string modulo, string accion, int severidad,
                       string mensaje, string ip, string host, DateTime fechaUtc)
        {
            using (var cn = ConnectionFactory.Open())
            using (var tx = cn.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    int newId;
                    int sev = MapToSeveridad(severidad);

                    // 1) Insert con DVH placeholder (no NULL para respetar NOT NULL)
                    using (var cmd = new SqlCommand(@"
                        INSERT INTO dbo.Bitacora
                            (Severidad, UsuarioId, Mensaje, Fecha, DVH, Modulo, Accion, IP, Host)
                        VALUES
                            (@sev, @uid, @msg, @fec, @dvhPlaceholder, @mod, @acc, @ip, @host);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@sev", sev);
                        cmd.Parameters.AddWithValue("@uid", (object)usuarioId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@msg", (object)mensaje ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@fec", fechaUtc);
                        cmd.Parameters.AddWithValue("@mod", (object)modulo ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@acc", (object)accion ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ip", (object)ip ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@host", (object)host ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@dvhPlaceholder", string.Empty); // evita NOT NULL
                        newId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // 2) Calcular DVH determinístico
                    var cadena = newId + "|" + sev + "|" + (usuarioId?.ToString() ?? "") + "|" +
                                 (mensaje ?? "") + "|" + fechaUtc.ToString("O") + "|" +
                                 (modulo ?? "") + "|" + (accion ?? "") + "|" +
                                 (ip ?? "") + "|" + (host ?? "");

                    string dvh;
                    using (var dvhCmd = new SqlCommand(
                        "SELECT CONVERT(varchar(64), HASHBYTES('SHA2_256', @s), 2)", cn, tx))
                    {
                        dvhCmd.Parameters.AddWithValue("@s", cadena);
                        dvh = (string)dvhCmd.ExecuteScalar();
                    }

                    using (var upd = new SqlCommand(
                        "UPDATE dbo.Bitacora SET DVH = @dvh WHERE IdBitacora = @id", cn, tx))
                    {
                        upd.Parameters.AddWithValue("@dvh", dvh);
                        upd.Parameters.AddWithValue("@id", newId);
                        upd.ExecuteNonQuery();
                    }

                    // 3) Recalcular DVV
                    var dvv = DVVDao.GetInstance().CalculateDVV("Bitacora");
                    UpsertDVV_tx(cn, tx, "Bitacora", dvv);

                    tx.Commit();
                    return newId;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        // Helpers de uso
        public void AddLoginOk(int? usuarioId, string modulo, string host)
        {
            Add(usuarioId, modulo ?? "Seguridad", "LoginOK", 1,
                "Inicio de sesión correcto", null, host);
        }

        public void AddLoginFail(string userNameIntento, string modulo, string host)
        {
            Add(null, modulo ?? "Seguridad", "LoginFail", 2,
                "Login fallido para '" + userNameIntento + "'", null, host);
        }

        public void AddError(string modulo, string accion, string detalle, int? usuarioId = null,
                             string ip = null, string host = null)
        {
            Add(usuarioId, modulo ?? "App", accion ?? "Error", 3, detalle, ip, host);
        }

        private static void UpsertDVV_tx(SqlConnection cn, SqlTransaction tx, string tabla, string dvv)
        {
            const string check = "SELECT COUNT(*) FROM DVV WHERE Tabla = @t";
            using (var cmd = new SqlCommand(check, cn, tx))
            {
                cmd.Parameters.AddWithValue("@t", tabla);
                var c = Convert.ToInt32(cmd.ExecuteScalar());

                var sql = (c > 0)
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

        // === Consultas para la UI de Bitácora ===
        public DataTable GetUsuarios()
        {
            using (var cn = ConnectionFactory.Open())
            using (var da = new SqlDataAdapter(@"
                SELECT DISTINCT
                    COALESCE(u.Usuario, CAST(b.UsuarioId AS varchar(12))) AS Usuario
                FROM dbo.Bitacora b
                LEFT JOIN dbo.Usuario u ON u.IdUsuario = b.UsuarioId
                ORDER BY 1;", cn))
            {
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public (DataTable dt, int total) Search(
            DateTime? desde, DateTime? hasta, string usuario,
            int page, int pageSize, string ordenar)
        {
            using (var cn = ConnectionFactory.Open())
            {
                var where = new System.Text.StringBuilder(" WHERE 1=1 ");
                var cmdCount = new SqlCommand { Connection = cn };
                var cmdPage = new SqlCommand { Connection = cn };

                if (desde.HasValue)
                {
                    where.Append(" AND b.Fecha >= @desde ");
                    cmdCount.Parameters.AddWithValue("@desde", desde.Value);
                    cmdPage.Parameters.AddWithValue("@desde", desde.Value);
                }
                if (hasta.HasValue)
                {
                    where.Append(" AND b.Fecha <= @hasta ");
                    cmdCount.Parameters.AddWithValue("@hasta", hasta.Value);
                    cmdPage.Parameters.AddWithValue("@hasta", hasta.Value);
                }
                if (!string.IsNullOrWhiteSpace(usuario))
                {
                    where.Append(@" AND (COALESCE(u.Usuario, CAST(b.UsuarioId AS varchar(12))) = @usr) ");
                    cmdCount.Parameters.AddWithValue("@usr", usuario.Trim());
                    cmdPage.Parameters.AddWithValue("@usr", usuario.Trim());
                }

                string ord = (ordenar ?? "").Trim().ToLowerInvariant();
                string orderBy;
                switch (ord)
                {
                    case "fecha asc": orderBy = "b.Fecha ASC"; break;
                    case "fecha desc": orderBy = "b.Fecha DESC"; break;
                    case "severidad asc": orderBy = "b.Severidad ASC, b.Fecha DESC"; break;
                    case "severidad desc": orderBy = "b.Severidad DESC, b.Fecha DESC"; break;
                    default: orderBy = "b.Fecha DESC"; break;
                }

                cmdCount.CommandText = @"
                    SELECT COUNT(*)
                    FROM dbo.Bitacora b
                    LEFT JOIN dbo.Usuario u ON u.IdUsuario = b.UsuarioId
                " + where.ToString();

                int total = Convert.ToInt32(cmdCount.ExecuteScalar());

                int off = Math.Max(0, (page - 1)) * Math.Max(1, pageSize);
                int take = Math.Max(1, pageSize);

                cmdPage.CommandText = @"
                    SELECT
                        b.IdBitacora,
                        b.Fecha,
                        COALESCE(u.Usuario, CAST(b.UsuarioId AS varchar(12))) AS Usuario,
                        b.Modulo,
                        b.Accion,
                        b.Severidad,
                        b.Mensaje,
                        b.IP,
                        b.Host
                    FROM dbo.Bitacora b
                    LEFT JOIN dbo.Usuario u ON u.IdUsuario = b.UsuarioId
                " + where.ToString() + @"
                    ORDER BY " + orderBy + @"
                    OFFSET @off ROWS FETCH NEXT @take ROWS ONLY;";

                cmdPage.Parameters.AddWithValue("@off", off);
                cmdPage.Parameters.AddWithValue("@take", take);

                var dt = new DataTable();
                using (var da = new SqlDataAdapter(cmdPage))
                {
                    da.Fill(dt);
                }

                return (dt, total);
            }
        }
    }
}
