using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using BE;
using DAL.Mappers;
using Services;

namespace DAL
{
    public class UsuarioDao : ICRUD<Usuario>
    {
        #region Singleton
        private static UsuarioDao _instance;
        public static UsuarioDao GetInstance() => _instance ?? (_instance = new UsuarioDao());
        private UsuarioDao() { }
        #endregion

        // Acceso unificado al connection string actual
        private static string Cnn => ConnectionFactory.Current;

        // helpers para DBNull
        private static object DbOrNull<T>(T? value) where T : struct
            => value.HasValue ? (object)value.Value : DBNull.Value;

        private static object DbOrNull(string value)
            => string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value;

        #region CRUD USUARIO
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

        // Convierte string base64 o null a varbinary (byte[])
        private static object ToDbVarbinary(object value)
        {
            if (value == null) return DBNull.Value;
            if (value is byte[] bytes) return bytes;
            if (value is string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return DBNull.Value;
                try { return Convert.FromBase64String(s); }
                catch { return DBNull.Value; }
            }
            return DBNull.Value;
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
                FROM Usuario;";

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

            // --- Validación de duplicados ---
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

            // --- Parámetros del INSERT ---
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
            // Validación de duplicados (excluyendo al propio usuario)
            const string checkDup = @"
        SELECT COUNT(*)
        FROM dbo.Usuario
        WHERE IdUsuario <> @IdUsuario
          AND (
                (Mail = @Mail AND @Mail IS NOT NULL)
             OR (Documento = @Documento AND @Documento IS NOT NULL)
          );";

            var dup = SqlHelpers.GetInstance(Cnn).ExecuteScalar(
                checkDup,
                new List<SqlParameter>
                {
            new SqlParameter("@IdUsuario", usuario.Id),
            new SqlParameter("@Mail",      (object)usuario.Email ?? DBNull.Value),
            new SqlParameter("@Documento", (object)usuario.Documento ?? DBNull.Value)
                });

            if (Convert.ToInt32(dup) > 0)
                throw new Exception("Mail o Documento ya está en uso por otro usuario.");

            const string sql = @"
        UPDATE dbo.Usuario
        SET
            Usuario                = @Usuario,
            Nombre                 = @Nombre,
            Apellido               = @Apellido,
            Mail                   = @Mail,
            Documento              = @Documento,

            -- Si mandás NULL, conserva lo que ya está en la tabla
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
            const string query =
                "UPDATE Usuario SET IdEstado = 3, DVH = @DVH WHERE IdUsuario = @IdUsuario";

            // Evitar patentes huérfanas
            HashSet<Permiso> patentes = PatenteDao.GetInstance().GetPatentesUsuario(usuario.Id);
            foreach (Permiso p in patentes)
            {
                if (!PatenteDao.GetInstance().CheckPatenteAsing(p.Id))
                    throw new Exception($"No se puede eliminar. Patente {p.Name} quedaría sin asignar.");
            }

            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@IdUsuario", usuario.Id),
                new SqlParameter("@DVH", dvh?.dvh ?? (object)DBNull.Value)
            };

            int rows = SqlHelpers.GetInstance(Cnn).ExecuteQuery(query, sqlParams);
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

            // leer VARBINARY como byte[]
            var hash = dt.Rows[0]["PasswordHash"] as byte[];
            var salt = dt.Rows[0]["PasswordSalt"] as byte[];
            var iters = Convert.ToInt32(dt.Rows[0]["PasswordIterations"]);

            return Services.PasswordService.Verify(plainPassword, hash, salt, iters);
        }

        // PBKDF2 – comparación constante en byte[]
        private static bool VerifyPasswordBytes(string plain, byte[] salt, int iterations, byte[] expected)
        {
            if (salt == null || expected == null || iterations <= 0) return false;

            using (var pbkdf2 = new Rfc2898DeriveBytes(plain, salt, iterations, HashAlgorithmName.SHA256))
            {
                byte[] computed = pbkdf2.GetBytes(expected.Length);
                if (computed.Length != expected.Length) return false;
                int diff = 0;
                for (int i = 0; i < expected.Length; i++)
                    diff |= computed[i] ^ expected[i];
                return diff == 0;
            }
        }
        #endregion

        #region ICRUD (no usados directamente)
        public bool Add(Usuario alta) { throw new NotImplementedException(); }
        public bool Update(Usuario update) { throw new NotImplementedException(); }
        public bool Delete(Usuario baja) { throw new NotImplementedException(); }
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
        public bool SetPatentesDeUsuario(int idUsuario, IList<int> patentesIds)
        {
            using (var cn = DAL.ConnectionFactory.Open())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    // limpia actuales
                    using (var del = new SqlCommand("DELETE FROM UsuarioPatente WHERE IdUsuario=@u", cn, tx))
                    {
                        del.Parameters.AddWithValue("@u", idUsuario);
                        del.ExecuteNonQuery();
                    }

                    // inserta nuevas
                    if (patentesIds != null)
                    {
                        const string ins = "INSERT INTO UsuarioPatente (IdUsuario, IdPatente) VALUES (@u, @p)";
                        foreach (var idP in patentesIds.Distinct())
                        {
                            using (var cmd = new SqlCommand(ins, cn, tx))
                            {
                                cmd.Parameters.AddWithValue("@u", idUsuario);
                                cmd.Parameters.AddWithValue("@p", idP);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    // Recalcular DVV (si lo estás usando)
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

        private static SqlParameter VarBinaryParam(string name, byte[] value, int size)
        {
            var p = new SqlParameter(name, System.Data.SqlDbType.VarBinary, size);
            p.Value = (object)value ?? DBNull.Value;
            return p;
        }
        // si querés aceptar string base64 por las dudas:
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

    }
}
