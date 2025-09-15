using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using BE;
using DAL.Mappers;
using Services;

namespace DAL
{
    public class UsuarioDao : ICRUD<Usuario>
    {
        private static string configFilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigFile.txt");
        private static string _connString =
            Crypto.Decript(FileHelper.GetInstance(configFilePath).ReadFile());

        #region Singleton
        private static UsuarioDao _instance;
        public static UsuarioDao GetInstance()
        {
            if (_instance == null) _instance = new UsuarioDao();
            return _instance;
        }
        #endregion

        // helper para DBNull
        private static object DbOrNull<T>(T? value) where T : struct
            => value.HasValue ? (object)value.Value : DBNull.Value;

        private static object DbOrNull(string value)
            => string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value;

        #region CRUD USUARIO
        public Usuario GetById(int idUsuario)
        {
            string query = @"
                SELECT IdUsuario, IdIdioma, IdEstado, Nombre, Usuario, Apellido,
                       Mail, PasswordHash, PasswordSalt, PasswordIterations,
                       NroIntentos, Documento, DebeCambiarContraseña, DVH
                FROM Usuario
                WHERE IdUsuario = @idUsuario";

            List<SqlParameter> param = new List<SqlParameter>
            {
                new SqlParameter("@idUsuario", idUsuario)
            };

            return MPUsuario.GetInstance()
                .MapUser(SqlHelpers.GetInstance(_connString).GetDataTable(query, param));
        }

        public Usuario GetByUserName(string username)
        {
            string query = @"
                SELECT IdUsuario, IdIdioma, IdEstado, Nombre, Usuario, Apellido,
                       Mail, PasswordHash, PasswordSalt, PasswordIterations,
                       NroIntentos, Documento, DebeCambiarContraseña, DVH
                FROM Usuario
                WHERE Usuario = @user";

            List<SqlParameter> param = new List<SqlParameter>
            {
                new SqlParameter("@user", username)
            };

            return MPUsuario.GetInstance()
                .MapUser(SqlHelpers.GetInstance(_connString).GetDataTable(query, param));
        }

        public List<Usuario> GetAll()
        {
            string query = @"
                SELECT IdUsuario, IdIdioma, IdEstado, Nombre, Usuario, Apellido,
                       Mail, PasswordHash, PasswordSalt, PasswordIterations,
                       NroIntentos, Documento, DebeCambiarContraseña, DVH
                FROM Usuario";

            return MPUsuario.GetInstance()
                .Map(SqlHelpers.GetInstance(_connString).GetDataTable(query));
        }

        public List<Usuario> GetAllActive()
        {
            string query = @"
                SELECT IdUsuario, IdIdioma, IdEstado, Nombre, Usuario, Apellido,
                       Mail, PasswordHash, PasswordSalt, PasswordIterations,
                       NroIntentos, Documento, DebeCambiarContraseña, DVH
                FROM Usuario
                WHERE IdEstado <> 3";

            return MPUsuario.GetInstance()
                .Map(SqlHelpers.GetInstance(_connString).GetDataTable(query));
        }

        public bool Add(Usuario usuario, DVH dvh)
        {
            const string checkPrev =
                "SELECT COUNT(*) FROM Usuario WHERE Usuario = @username OR Mail = @mail";

            const string insert = @"
                INSERT INTO Usuario
                    (IdEstado, IdIdioma, Usuario, PasswordHash, PasswordSalt, PasswordIterations,
                     Nombre, Apellido, Mail, NroIntentos, Documento, DebeCambiarContraseña, DVH)
                VALUES
                    (@IdEstado, @IdIdioma, @Usuario, @PasswordHash, @PasswordSalt, @PasswordIterations,
                     @Nombre, @Apellido, @Mail, @NroIntentos, @Documento, @DebeCambiar, @DVH)";

            try
            {
                object exists = SqlHelpers.GetInstance(_connString).ExecuteScalar(
                    checkPrev,
                    new List<SqlParameter>
                    {
                        new SqlParameter("@username", usuario.UserName),
                        new SqlParameter("@mail", usuario.Email)
                    });

                if (Convert.ToInt32(exists) > 0)
                    throw new Exception("Usuario o Mail ya existente.");

                List<SqlParameter> sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@IdEstado", usuario.EstadoUsuarioId),
                    new SqlParameter("@IdIdioma", DbOrNull(usuario.IdiomaId)),
                    new SqlParameter("@Usuario", usuario.UserName),
                    new SqlParameter("@PasswordHash", DbOrNull(usuario.PasswordHash)),
                    new SqlParameter("@PasswordSalt", DbOrNull(usuario.PasswordSalt)),
                    new SqlParameter("@PasswordIterations", usuario.PasswordIterations),
                    new SqlParameter("@Nombre", DbOrNull(usuario.Name)),
                    new SqlParameter("@Apellido", DbOrNull(usuario.LastName)),
                    new SqlParameter("@Mail", DbOrNull(usuario.Email)),
                    new SqlParameter("@NroIntentos", usuario.Tries),
                    new SqlParameter("@Documento", DbOrNull(usuario.Documento)),
                    new SqlParameter("@DebeCambiar", usuario.DebeCambiarContraseña),
                    new SqlParameter("@DVH", dvh.dvh ?? (object)DBNull.Value)
                };

                int rows = SqlHelpers.GetInstance(_connString).ExecuteQuery(insert, sqlParams);
                if (rows > 0)
                {
                    DVVDao.GetInstance().AddUpdateDVV(
                        new DVV { tabla = "Usuario", dvv = DVVDao.GetInstance().CalculateDVV("Usuario") });
                    return true;
                }
                return false;
            }
            catch
            {
                throw;
            }
        }

        public bool Update(Usuario usuario, DVH dvh)
        {
            const string query = @"
                UPDATE Usuario
                SET Usuario = @Usuario,
                    Nombre = @Nombre,
                    Apellido = @Apellido,
                    Mail = @Mail,
                    PasswordHash = @PasswordHash,
                    PasswordSalt = @PasswordSalt,
                    PasswordIterations = @PasswordIterations,
                    IdEstado = @IdEstado,
                    IdIdioma = @IdIdioma,
                    NroIntentos = @NroIntentos,
                    Documento = @Documento,
                    DebeCambiarContraseña = @DebeCambiar,
                    DVH = @DVH
                WHERE IdUsuario = @IdUsuario";

            try
            {
                List<SqlParameter> sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@IdUsuario", usuario.Id),
                    new SqlParameter("@Usuario", usuario.UserName),
                    new SqlParameter("@Nombre", DbOrNull(usuario.Name)),
                    new SqlParameter("@Apellido", DbOrNull(usuario.LastName)),
                    new SqlParameter("@Mail", DbOrNull(usuario.Email)),
                    new SqlParameter("@PasswordHash", DbOrNull(usuario.PasswordHash)),
                    new SqlParameter("@PasswordSalt", DbOrNull(usuario.PasswordSalt)),
                    new SqlParameter("@PasswordIterations", usuario.PasswordIterations),
                    new SqlParameter("@IdEstado", usuario.EstadoUsuarioId),
                    new SqlParameter("@IdIdioma", DbOrNull(usuario.IdiomaId)),
                    new SqlParameter("@NroIntentos", usuario.Tries),
                    new SqlParameter("@Documento", DbOrNull(usuario.Documento)),
                    new SqlParameter("@DebeCambiar", usuario.DebeCambiarContraseña),
                    new SqlParameter("@DVH", dvh.dvh ?? (object)DBNull.Value)
                };

                int rows = SqlHelpers.GetInstance(_connString).ExecuteQuery(query, sqlParams);
                if (rows > 0)
                {
                    DVVDao.GetInstance().AddUpdateDVV(
                        new DVV { tabla = "Usuario", dvv = DVVDao.GetInstance().CalculateDVV("Usuario") });
                    return true;
                }
                return false;
            }
            catch
            {
                throw;
            }
        }

        public bool Delete(Usuario usuario, DVH dvh)
        {
            const string query =
                "UPDATE Usuario SET IdEstado = 3, DVH = @DVH WHERE IdUsuario = @IdUsuario";

            try
            {
                // Patentes huérfanas
                HashSet<Permiso> patentes =
                    PatenteDao.GetInstance().GetPatentesUsuario(usuario.Id);
                foreach (Permiso p in patentes)
                {
                    if (!PatenteDao.GetInstance().CheckPatenteAsing(p.Id))
                        throw new Exception(
                            string.Format("No se puede eliminar. Patente {0} quedaría sin asignar.", p.Name));
                }

                List<SqlParameter> sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@IdUsuario", usuario.Id),
                    new SqlParameter("@DVH", dvh.dvh ?? (object)DBNull.Value)
                };

                int rows = SqlHelpers.GetInstance(_connString).ExecuteQuery(query, sqlParams);
                if (rows > 0)
                {
                    DVVDao.GetInstance().AddUpdateDVV(
                        new DVV { tabla = "Usuario", dvv = DVVDao.GetInstance().CalculateDVV("Usuario") });
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

        #region LOGIN (con verificación PBKDF2)
        public bool Login(string user, string plainPassword)
        {
            // Traigo hash/salt/iters del usuario habilitado
            const string query = @"
                SELECT PasswordHash, PasswordSalt, PasswordIterations
                FROM Usuario
                WHERE Usuario = @user AND IdEstado = 1";

            try
            {
                List<SqlParameter> sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@user", user)
                };

                var dt = SqlHelpers.GetInstance(_connString).GetDataTable(query, sqlParams);
                if (dt.Rows.Count == 0) return false;

                string storedHash = Convert.ToString(dt.Rows[0]["PasswordHash"]);
                string storedSalt = Convert.ToString(dt.Rows[0]["PasswordSalt"]);
                int iterations = Convert.ToInt32(dt.Rows[0]["PasswordIterations"]);

                return VerifyPassword(plainPassword, storedSalt, iterations, storedHash);
            }
            catch
            {
                throw;
            }
        }

        // PBKDF2 – compatible con C# 7.3
        private static bool VerifyPassword(string plain, string base64Salt, int iterations, string base64Hash)
        {
            if (string.IsNullOrEmpty(base64Salt) || string.IsNullOrEmpty(base64Hash) || iterations <= 0)
                return false;

            byte[] salt = Convert.FromBase64String(base64Salt);
            byte[] expected = Convert.FromBase64String(base64Hash);

            using (var pbkdf2 = new Rfc2898DeriveBytes(plain, salt, iterations, HashAlgorithmName.SHA256))
            {
                byte[] computed = pbkdf2.GetBytes(expected.Length);
                // tiempo constante
                if (computed.Length != expected.Length) return false;
                int diff = 0;
                for (int i = 0; i < expected.Length; i++) diff |= computed[i] ^ expected[i];
                return diff == 0;
            }
        }
        #endregion

        #region NotImplemented
        public bool Add(Usuario alta) { throw new NotImplementedException(); }
        public bool Update(Usuario update) { throw new NotImplementedException(); }
        public bool Delete(Usuario baja) { throw new NotImplementedException(); }
        #endregion
    

    public bool DeleteUsuarioFamilia(int idFamilia)
        {
            const string sql = "DELETE FROM UsuarioFamilia WHERE IdFamilia = @IdFamilia";
            try
            {
                var ps = new List<System.Data.SqlClient.SqlParameter>
        {
            new System.Data.SqlClient.SqlParameter("@IdFamilia", idFamilia)
        };
                int rows = Services.SqlHelpers.GetInstance(_connString).ExecuteQuery(sql, ps);

                return rows > 0;
            }
            catch { throw; }
        }
    }
}
