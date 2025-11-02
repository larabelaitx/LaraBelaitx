using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using BE;
using DAL.Mappers;

namespace DAL
{
    public class UsuarioDao : ICRUD<Usuario>
    {
        #region Singleton
        private static UsuarioDao _instance;
        public static UsuarioDao GetInstance() => _instance ?? (_instance = new UsuarioDao());
        private UsuarioDao() { }
        #endregion

        private static string Cnn => ConnectionFactory.Current;

        private static object DbOrNull<T>(T? value) where T : struct
            => value.HasValue ? (object)value.Value : DBNull.Value;

        private static object DbOrNull(string value)
            => string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value;

        #region CRUD USUARIO (lecturas)
        public Usuario GetById(int idUsuario)
        {
            const string query = @"
                SELECT IdUsuario, IdIdioma, IdEstado, Nombre, Usuario, Apellido,
                       Mail, Documento, PasswordHash, PasswordSalt, PasswordIterations,
                       NroIntentos, DVH, BloqueadoHastaUtc, UltimoLoginUtc,
                       DebeCambiarContrasena
                FROM Usuario
                WHERE IdUsuario = @idUsuario";

            var param = new List<SqlParameter> { new SqlParameter("@idUsuario", idUsuario) };

            return MPUsuario.GetInstance()
                .MapUser(SqlHelpers.GetInstance(Cnn).GetDataTable(query, param));
        }

        public Usuario GetByUserName(string username)
        {
            const string query = @"
                SELECT IdUsuario, IdIdioma, IdEstado, Nombre, Usuario, Apellido,
                       Mail, Documento, PasswordHash, PasswordSalt, PasswordIterations,
                       NroIntentos, DVH, BloqueadoHastaUtc, UltimoLoginUtc,
                       DebeCambiarContrasena
                FROM Usuario
                WHERE Usuario = @user";

            var param = new List<SqlParameter> { new SqlParameter("@user", username) };

            return MPUsuario.GetInstance()
                .MapUser(SqlHelpers.GetInstance(Cnn).GetDataTable(query, param));
        }

        public List<Usuario> GetAll()
        {
            const string query = @"
            SELECT IdUsuario, IdIdioma, IdEstado, Nombre, Usuario, Apellido,
                   Mail, Documento, PasswordHash, PasswordSalt, PasswordIterations,
                   NroIntentos, DVH, BloqueadoHastaUtc, UltimoLoginUtc,
                   DebeCambiarContrasena
            FROM Usuario";

            return MPUsuario.GetInstance()
                .Map(SqlHelpers.GetInstance(Cnn).GetDataTable(query));
        }

        public List<Usuario> GetAllActive()
        {
            const string query = @"
                SELECT IdUsuario, IdIdioma, IdEstado, Nombre, Usuario, Apellido,
                       Mail, Documento, PasswordHash, PasswordSalt, PasswordIterations,
                       NroIntentos, DVH, BloqueadoHastaUtc, UltimoLoginUtc,
                       DebeCambiarContrasena
                FROM Usuario
                WHERE IdEstado <> 3";

            return MPUsuario.GetInstance()
                .Map(SqlHelpers.GetInstance(Cnn).GetDataTable(query));
        }
        #endregion

        #region CRUD USUARIO (altas / modificaciones / bajas)
        public bool Add(Usuario usuario, DVH dvh)
        {
            const string checkPrev = @"
                SELECT COUNT(*)
                FROM dbo.Usuario
                WHERE Usuario = @username
                   OR (Mail = @mail AND @mail IS NOT NULL)
                   OR (Documento = @doc AND @doc IS NOT NULL);";

            const string insert = @"
                INSERT INTO dbo.Usuario
                    (IdEstado, IdIdioma, Usuario,
                     PasswordHash, PasswordSalt, PasswordIterations,
                     Nombre, Apellido, Mail, Documento,
                     NroIntentos, DVH, DebeCambiarContrasena)
                VALUES
                    (@IdEstado, @IdIdioma, @Usuario,
                     @PasswordHash, @PasswordSalt, @PasswordIterations,
                     @Nombre, @Apellido, @Mail, @Documento,
                     @NroIntentos, @DVH, @DebeCambiarContrasena);";

            var exists = SqlHelpers.GetInstance(Cnn).ExecuteScalar(
                checkPrev,
                new List<SqlParameter>
                {
                    new SqlParameter("@username", usuario.UserName),
                    new SqlParameter("@mail", (object)usuario.Email ?? DBNull.Value),
                    new SqlParameter("@doc",  (object)usuario.Documento ?? DBNull.Value)
                });

            if (Convert.ToInt32(exists) > 0)
                throw new Exception("Usuario, Mail o Documento ya existente.");

            var ps = new List<SqlParameter>
            {
                new SqlParameter("@IdEstado", usuario.EstadoUsuarioId > 0 ? usuario.EstadoUsuarioId : 1),
                new SqlParameter("@IdIdioma", (object)(usuario.IdiomaId ?? 1)),
                new SqlParameter("@Usuario", usuario.UserName),

                VarBinaryParamFromB64("@PasswordHash", usuario.PasswordHash, 32),
                VarBinaryParamFromB64("@PasswordSalt", usuario.PasswordSalt, 16),
                new SqlParameter("@PasswordIterations", usuario.PasswordIterations),

                new SqlParameter("@Nombre",    DbOrNull(usuario.Name)),
                new SqlParameter("@Apellido",  DbOrNull(usuario.LastName)),
                new SqlParameter("@Mail",      DbOrNull(usuario.Email)),
                new SqlParameter("@Documento", DbOrNull(usuario.Documento)),

                new SqlParameter("@NroIntentos", usuario.Tries),
                new SqlParameter("@DVH", dvh?.dvh ?? (object)DBNull.Value),
                new SqlParameter("@DebeCambiarContrasena", usuario.DebeCambiarContraseña)
            };

            int rows = SqlHelpers.GetInstance(Cnn).ExecuteQuery(insert, ps);
            if (rows > 0)
            {
                DVVDao.GetInstance().AddUpdateDVV(
                    new DVV { tabla = "Usuario", dvv = DVVDao.GetInstance().CalculateDVV("Usuario") });
                return true;
            }
            return false;
        }

        public bool Update(Usuario usuario, DVH dvh)
        {
            if (!string.IsNullOrWhiteSpace(usuario.Documento) &&
                ExisteDocumento(usuario.Documento.Trim(), excluirId: usuario.Id))
                throw new Exception("El documento ya está en uso por otro usuario.");

            if (!string.IsNullOrWhiteSpace(usuario.Email) &&
                ExisteEmail(usuario.Email.Trim(), excluirId: usuario.Id))
                throw new Exception("El mail ya está en uso por otro usuario.");

            const string sql = @"
    UPDATE dbo.Usuario
       SET Usuario                = @Usuario,
           Nombre                 = @Nombre,
           Apellido               = @Apellido,
           Mail                   = @Mail,
           Documento              = @Documento,
           PasswordHash           = COALESCE(@PasswordHash,       PasswordHash),
           PasswordSalt           = COALESCE(@PasswordSalt,       PasswordSalt),
           PasswordIterations     = COALESCE(@PasswordIterations, PasswordIterations),
           IdEstado               = @IdEstado,
           IdIdioma               = COALESCE(@IdIdioma, IdIdioma),
           NroIntentos            = @NroIntentos,
           DVH                    = @DVH,
           DebeCambiarContrasena  = @DebeCambiarContrasena
     WHERE IdUsuario = @IdUsuario;";

            var ps = new List<SqlParameter>
            {
                new SqlParameter("@IdUsuario", usuario.Id),
                new SqlParameter("@Usuario",   usuario.UserName),
                new SqlParameter("@Nombre",    DbOrNull(usuario.Name)),
                new SqlParameter("@Apellido",  DbOrNull(usuario.LastName)),
                new SqlParameter("@Mail",      DbOrNull(usuario.Email)),
                new SqlParameter("@Documento", DbOrNull(usuario.Documento)),

                VarBinaryParam("@PasswordHash", usuario.PasswordHash, 32),
                VarBinaryParam("@PasswordSalt", usuario.PasswordSalt, 16),
                new SqlParameter("@PasswordIterations",
                    usuario.PasswordIterations > 0 ? (object)usuario.PasswordIterations : DBNull.Value),

                new SqlParameter("@IdEstado",    usuario.EstadoUsuarioId),
                new SqlParameter("@IdIdioma",    DbOrNull(usuario.IdiomaId)),
                new SqlParameter("@NroIntentos", usuario.Tries),
                new SqlParameter("@DVH",         dvh?.dvh ?? (object)DBNull.Value),
                new SqlParameter("@DebeCambiarContrasena", usuario.DebeCambiarContraseña)
            };

            int rows = SqlHelpers.GetInstance(Cnn).ExecuteQuery(sql, ps);
            if (rows > 0)
            {
                DVVDao.GetInstance().AddUpdateDVV(
                    new DVV { tabla = "Usuario", dvv = DVVDao.GetInstance().CalculateDVV("Usuario") });
                return true;
            }
            return false;
        }

        public bool Delete(Usuario usuario, DVH dvh)
        {
            const string qCountActivos = "SELECT COUNT(*) FROM dbo.Usuario WHERE IdEstado <> 3";
            int activos = Convert.ToInt32(SqlHelpers.GetInstance(Cnn).ExecuteScalar(qCountActivos));
            if (activos <= 1)
                throw new Exception("No se puede eliminar: quedaría el sistema sin usuarios.");

            const string query = "UPDATE Usuario SET IdEstado = 3, DVH = @DVH WHERE IdUsuario = @IdUsuario";
            var ps = new List<SqlParameter>
            {
                new SqlParameter("@IdUsuario", usuario.Id),
                new SqlParameter("@DVH", dvh?.dvh ?? (object)DBNull.Value)
            };

            int rows = SqlHelpers.GetInstance(Cnn).ExecuteQuery(query, ps);
            if (rows > 0)
            {
                DVVDao.GetInstance().AddUpdateDVV(
                    new DVV { tabla = "Usuario", dvv = DVVDao.GetInstance().CalculateDVV("Usuario") });
                return true;
            }
            return false;
        }
        #endregion

        #region LOGIN (verificación PBKDF2)
        public bool Login(string user, string plainPassword)
        {
            const string query = @"
                SELECT PasswordHash, PasswordSalt, PasswordIterations
                FROM dbo.Usuario
                WHERE Usuario = @user AND IdEstado = 1";

            var dt = SqlHelpers.GetInstance(Cnn).GetDataTable(
                query,
                new List<SqlParameter> { new SqlParameter("@user", user) });

            if (dt.Rows.Count == 0) return false;

            var hash = dt.Rows[0]["PasswordHash"] as byte[];
            var salt = dt.Rows[0]["PasswordSalt"] as byte[];
            var iters = Convert.ToInt32(dt.Rows[0]["PasswordIterations"]);

            return Services.PasswordService.Verify(plainPassword, hash, salt, iters);
        }
        #endregion

        public void MarcarUltimoLoginExitoso(int idUsuario, DateTime utcNow, DVH dvh)
        {
            const string sql = @"
        UPDATE dbo.Usuario
           SET UltimoLoginUtc = @dtUtc,
               NroIntentos = 0,
               IdEstado = CASE WHEN IdEstado = 2 THEN 1 ELSE IdEstado END,
               DVH = @DVH
         WHERE IdUsuario = @IdUsuario;";

            using (var cn = DAL.ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@dtUtc", utcNow);
                cmd.Parameters.AddWithValue("@DVH", (object)dvh?.dvh ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cmd.ExecuteNonQuery();
            }

            var dvv = DVVDao.GetInstance().CalculateDVV("Usuario");
            DVVDao.GetInstance().AddUpdateDVV(new DVV { tabla = "Usuario", dvv = dvv });
        }

        #region Unicidad (Documento / Email)
        public bool ExisteDocumento(string documento, int? excluirId = null)
        {
            const string sql = @"
        SELECT COUNT(*)
        FROM dbo.Usuario
        WHERE Documento = @doc
          AND @doc IS NOT NULL
          AND (@id IS NULL OR IdUsuario <> @id);";

            var count = SqlHelpers.GetInstance(Cnn).ExecuteScalar(
                sql,
                new List<SqlParameter>
                {
                    new SqlParameter("@doc", (object)documento ?? DBNull.Value),
                    new SqlParameter("@id",  (object)excluirId ?? DBNull.Value)
                });

            return Convert.ToInt32(count) > 0;
        }

        public bool ExisteEmail(string email, int? excluirId = null)
        {
            const string sql = @"
        SELECT COUNT(*)
        FROM dbo.Usuario
        WHERE Mail = @mail
          AND @mail IS NOT NULL
          AND (@id IS NULL OR IdUsuario <> @id);";

            var count = SqlHelpers.GetInstance(Cnn).ExecuteScalar(
                sql,
                new List<SqlParameter>
                {
                    new SqlParameter("@mail", (object)email ?? DBNull.Value),
                    new SqlParameter("@id",   (object)excluirId ?? DBNull.Value)
                });

            return Convert.ToInt32(count) > 0;
        }
        #endregion

        #region Relaciones Usuario-Familia
        public bool DeleteUsuarioFamilia(int idFamilia)
        {
            const string sql = "DELETE FROM UsuarioFamilia WHERE IdFamilia = @IdFamilia";
            var ps = new List<SqlParameter> { new SqlParameter("@IdFamilia", idFamilia) };
            int rows = SqlHelpers.GetInstance(Cnn).ExecuteQuery(sql, ps);
            return rows > 0;
        }

        public bool SetUsuarioFamilia(int idUsuario, int idFamilia)
        {
            const string del = "DELETE FROM UsuarioFamilia WHERE IdUsuario = @U";
            const string ins = "INSERT INTO UsuarioFamilia (IdUsuario, IdFamilia) VALUES (@U, @F)";

            SqlHelpers.GetInstance(Cnn).ExecuteQuery(del, new List<SqlParameter> { new SqlParameter("@U", idUsuario) });
            int rows = SqlHelpers.GetInstance(Cnn).ExecuteQuery(ins, new List<SqlParameter>
            {
                new SqlParameter("@U", idUsuario),
                new SqlParameter("@F", idFamilia)
            });

            DVVDao.GetInstance().AddUpdateDVV(new DVV
            {
                tabla = "UsuarioFamilia",
                dvv = DVVDao.GetInstance().CalculateDVV("UsuarioFamilia")
            });

            return rows > 0;
        }

        public bool SetUsuarioFamilias(int idUsuario, IEnumerable<int> familiasIds)
        {
            const string del = "DELETE FROM UsuarioFamilia WHERE IdUsuario = @U";
            SqlHelpers.GetInstance(Cnn).ExecuteQuery(del, new List<SqlParameter> { new SqlParameter("@U", idUsuario) });

            if (familiasIds != null)
            {
                foreach (var idF in familiasIds.Distinct())
                {
                    const string ins = "INSERT INTO UsuarioFamilia (IdUsuario, IdFamilia) VALUES (@U, @F)";
                    SqlHelpers.GetInstance(Cnn).ExecuteQuery(ins, new List<SqlParameter>
                    {
                        new SqlParameter("@U", idUsuario),
                        new SqlParameter("@F", idF)
                    });
                }
            }

            DVVDao.GetInstance().AddUpdateDVV(new DVV
            {
                tabla = "UsuarioFamilia",
                dvv = DVVDao.GetInstance().CalculateDVV("UsuarioFamilia")
            });

            return true;
        }
        #endregion

        #region Usuario-Patente (no huérfanas)
        public bool SetPatentesDeUsuario(int idUsuario, IList<int> patentesIds)
        {
            patentesIds = patentesIds?.Distinct().ToList() ?? new List<int>();

            using (var cn = DAL.ConnectionFactory.Open())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    var actuales = new List<int>();
                    using (var cmd = new SqlCommand(
                        "SELECT IdPatente FROM UsuarioPatente WITH (UPDLOCK) WHERE IdUsuario=@u", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@u", idUsuario);
                        using (var dr = cmd.ExecuteReader())
                            while (dr.Read()) actuales.Add(dr.GetInt32(0));
                    }

                    var removidas = actuales.Except(patentesIds).ToList();
                    foreach (var idPat in removidas)
                    {
                        int restantes = PatenteDao.GetInstance()
                            .CountAsignacionesExcluyendoUsuario_tx(cn, tx, idPat, idUsuario);
                        if (restantes <= 0)
                            throw new Exception($"No se puede quitar la patente (Id={idPat}) porque quedaría sin asignar.");
                    }

                    using (var del = new SqlCommand(
                        "DELETE FROM UsuarioPatente WHERE IdUsuario=@u", cn, tx))
                    {
                        del.Parameters.AddWithValue("@u", idUsuario);
                        del.ExecuteNonQuery();
                    }

                    if (patentesIds.Any())
                    {
                        const string ins = "INSERT INTO UsuarioPatente (IdUsuario, IdPatente) VALUES (@u, @p)";
                        foreach (var idP in patentesIds)
                        {
                            using (var cmd = new SqlCommand(ins, cn, tx))
                            {
                                cmd.Parameters.AddWithValue("@u", idUsuario);
                                cmd.Parameters.AddWithValue("@p", idP);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    var dvv = DAL.DVVDao.GetInstance().CalculateDVV("UsuarioPatente");
                    DAL.DVVDao.GetInstance().AddUpdateDVV(new BE.DVV { tabla = "UsuarioPatente", dvv = dvv });

                    tx.Commit();
                    return true;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }



        #endregion

        #region Helpers VARBINARY
        private static SqlParameter VarBinaryParam(string name, byte[] value, int size)
        {
            var p = new SqlParameter(name, System.Data.SqlDbType.VarBinary, size);
            p.Value = (object)value ?? DBNull.Value;
            return p;
        }

        private static SqlParameter VarBinaryParamFromB64(string name, object value, int size)
        {
            byte[] bytes = null;
            if (value is byte[] b) bytes = b;
            else if (value is string s && !string.IsNullOrWhiteSpace(s))
            {
                try { bytes = Convert.FromBase64String(s); } catch { bytes = null; }
            }
            return VarBinaryParam(name, bytes, size);
        }
        #endregion

        #region ICRUD (no usados directamente por UI)
        public bool Add(Usuario alta) { throw new NotImplementedException(); }
        public bool Update(Usuario update) { throw new NotImplementedException(); }
        public bool Delete(Usuario baja) { throw new NotImplementedException(); }
        #endregion
    }
}
