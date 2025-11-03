using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
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

        private static string Cnn => ConnectionFactory.Current;

        private static object DbOrNull<T>(T? value) where T : struct
            => value.HasValue ? (object)value.Value : DBNull.Value;

        private static object DbOrNull(string value)
            => string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value;

        #region CRUD – lecturas
        public Usuario GetById(int idUsuario)
        {
            const string sql = @"
                SELECT IdUsuario, IdIdioma, IdEstado, Nombre, Usuario, Apellido,
                       Mail, Documento, PasswordHash, PasswordSalt, PasswordIterations,
                       NroIntentos, DVH, BloqueadoHastaUtc, UltimoLoginUtc,
                       DebeCambiarContrasena
                  FROM dbo.Usuario
                 WHERE IdUsuario = @id;";

            var ps = new List<SqlParameter> { new SqlParameter("@id", idUsuario) };
            return Mappers.MPUsuario.GetInstance()
                   .MapUser(SqlHelpers.GetInstance(Cnn).GetDataTable(sql, ps));
        }

        public Usuario GetByUserName(string username)
        {
            const string sql = @"
                SELECT IdUsuario, IdIdioma, IdEstado, Nombre, Usuario, Apellido,
                       Mail, Documento, PasswordHash, PasswordSalt, PasswordIterations,
                       NroIntentos, DVH, BloqueadoHastaUtc, UltimoLoginUtc,
                       DebeCambiarContrasena
                  FROM dbo.Usuario
                 WHERE Usuario = @u;";

            var ps = new List<SqlParameter> { new SqlParameter("@u", username) };
            return Mappers.MPUsuario.GetInstance()
                   .MapUser(SqlHelpers.GetInstance(Cnn).GetDataTable(sql, ps));
        }

        public List<Usuario> GetAll()
        {
            const string sql = @"
                SELECT IdUsuario, IdIdioma, IdEstado, Nombre, Usuario, Apellido,
                       Mail, Documento, PasswordHash, PasswordSalt, PasswordIterations,
                       NroIntentos, DVH, BloqueadoHastaUtc, UltimoLoginUtc,
                       DebeCambiarContrasena
                  FROM dbo.Usuario;";

            return Mappers.MPUsuario.GetInstance()
                   .Map(SqlHelpers.GetInstance(Cnn).GetDataTable(sql));
        }

        public List<Usuario> GetAllActive()
        {
            const string sql = @"
                SELECT IdUsuario, IdIdioma, IdEstado, Nombre, Usuario, Apellido,
                       Mail, Documento, PasswordHash, PasswordSalt, PasswordIterations,
                       NroIntentos, DVH, BloqueadoHastaUtc, UltimoLoginUtc,
                       DebeCambiarContrasena
                  FROM dbo.Usuario
                 WHERE IdEstado <> 3;";

            return Mappers.MPUsuario.GetInstance()
                   .Map(SqlHelpers.GetInstance(Cnn).GetDataTable(sql));
        }
        #endregion

        #region CRUD – altas/modificaciones/bajas
        public bool Add(Usuario usuario, DVH dvh)
        {
            // Permite duplicar usuario/mail/doc de registros dados de baja (IdEstado = 3)
            const string qCheck = @"
                SELECT COUNT(*)
                  FROM dbo.Usuario
                 WHERE IdEstado <> 3
                   AND ( Usuario = @u
                      OR (Mail = @m  AND @m  IS NOT NULL)
                      OR (Documento = @d AND @d IS NOT NULL) );";

            var exists = SqlHelpers.GetInstance(Cnn).ExecuteScalar(
                qCheck,
                new List<SqlParameter>{
                    new SqlParameter("@u", usuario.UserName),
                    new SqlParameter("@m", (object)usuario.Email ?? DBNull.Value),
                    new SqlParameter("@d", (object)usuario.Documento ?? DBNull.Value)
                });

            if (Convert.ToInt32(exists) > 0)
                throw new Exception("Usuario, Mail o Documento ya existente.");

            const string qIns = @"
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

            var ps = new List<SqlParameter>
            {
                new SqlParameter("@IdEstado", usuario.EstadoUsuarioId > 0 ? usuario.EstadoUsuarioId : 1),
                new SqlParameter("@IdIdioma", (object)(usuario.IdiomaId ?? 1)),
                new SqlParameter("@Usuario", usuario.UserName),

                VarBinaryParamFromB64("@PasswordHash", usuario.PasswordHash, 32),
                VarBinaryParamFromB64("@PasswordSalt", usuario.PasswordSalt, 16),
                new SqlParameter("@PasswordIterations", usuario.PasswordIterations > 0 ? usuario.PasswordIterations : 100000),

                new SqlParameter("@Nombre",    DbOrNull(usuario.Name)),
                new SqlParameter("@Apellido",  DbOrNull(usuario.LastName)),
                new SqlParameter("@Mail",      DbOrNull(usuario.Email)),
                new SqlParameter("@Documento", DbOrNull(usuario.Documento)),

                new SqlParameter("@NroIntentos", usuario.Tries),
                new SqlParameter("@DVH", dvh?.dvh ?? (object)DBNull.Value),
                new SqlParameter("@DebeCambiarContrasena", usuario.DebeCambiarContraseña)
            };

            var rows = SqlHelpers.GetInstance(Cnn).ExecuteQuery(qIns, ps);
            if (rows > 0)
            {
                var dvv = DVVDao.GetInstance().CalculateDVV("dbo.Usuario");
                DVVDao.GetInstance().AddUpdateDVV(new DVV { tabla = "Usuario", dvv = dvv });
                return true;
            }
            return false;
        }

        public bool Update(Usuario usuario, DVH dvh)
        {
            // Evita colisiones con otros usuarios (los dados de baja NO bloquean)
            if (!string.IsNullOrWhiteSpace(usuario.Documento) &&
                ExisteDocumento(usuario.Documento.Trim(), excluirId: usuario.Id))
                throw new Exception("El documento ya está en uso por otro usuario.");

            if (!string.IsNullOrWhiteSpace(usuario.Email) &&
                ExisteEmail(usuario.Email.Trim(), excluirId: usuario.Id))
                throw new Exception("El mail ya está en uso por otro usuario.");

            const string qUpd = @"
                UPDATE dbo.Usuario
                   SET Usuario               = @Usuario,
                       Nombre                = @Nombre,
                       Apellido              = @Apellido,
                       Mail                  = @Mail,
                       Documento             = @Documento,
                       PasswordHash          = COALESCE(@PasswordHash,       PasswordHash),
                       PasswordSalt          = COALESCE(@PasswordSalt,       PasswordSalt),
                       PasswordIterations    = COALESCE(@PasswordIterations, PasswordIterations),
                       IdEstado              = @IdEstado,
                       IdIdioma              = COALESCE(@IdIdioma, IdIdioma),
                       NroIntentos           = @NroIntentos,
                       DVH                   = @DVH,
                       DebeCambiarContrasena = @DebeCambiarContrasena
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

            var rows = SqlHelpers.GetInstance(Cnn).ExecuteQuery(qUpd, ps);
            if (rows > 0)
            {
                var dvv = DVVDao.GetInstance().CalculateDVV("dbo.Usuario");
                DVVDao.GetInstance().AddUpdateDVV(new DVV { tabla = "Usuario", dvv = dvv });
                return true;
            }
            return false;
        }

        public bool Delete(Usuario usuario, DVH dvh)
        {
            // No permitir dejar el sistema sin usuarios activos
            const string qActivos = "SELECT COUNT(*) FROM dbo.Usuario WHERE IdEstado <> 3;";
            var activos = Convert.ToInt32(SqlHelpers.GetInstance(Cnn).ExecuteScalar(qActivos));
            if (activos <= 1)
                throw new Exception("No se puede eliminar: quedaría el sistema sin usuarios.");

            const string qDel = "UPDATE dbo.Usuario SET IdEstado = 3, DVH = @DVH WHERE IdUsuario = @Id;";
            var ps = new List<SqlParameter>
            {
                new SqlParameter("@Id",  usuario.Id),
                new SqlParameter("@DVH", dvh?.dvh ?? (object)DBNull.Value)
            };

            var rows = SqlHelpers.GetInstance(Cnn).ExecuteQuery(qDel, ps);
            if (rows > 0)
            {
                var dvv = DVVDao.GetInstance().CalculateDVV("dbo.Usuario");
                DVVDao.GetInstance().AddUpdateDVV(new DVV { tabla = "Usuario", dvv = dvv });
                return true;
            }
            return false;
        }
        #endregion

        #region Login / Último login
        public bool Login(string user, string plainPassword)
        {
            const string sql = @"
                SELECT PasswordHash, PasswordSalt, PasswordIterations
                  FROM dbo.Usuario
                 WHERE Usuario = @user AND IdEstado = 1;";

            var dt = SqlHelpers.GetInstance(Cnn).GetDataTable(
                sql, new List<SqlParameter> { new SqlParameter("@user", user) });

            if (dt.Rows.Count == 0) return false;

            var hash = dt.Rows[0]["PasswordHash"] as byte[];
            var salt = dt.Rows[0]["PasswordSalt"] as byte[];
            var iters = Convert.ToInt32(dt.Rows[0]["PasswordIterations"]);

            return PasswordService.Verify(plainPassword, hash, salt, iters);
        }

        public void MarcarUltimoLoginExitoso(int idUsuario, DateTime utcNow, DVH dvh)
        {
            const string sql = @"
                UPDATE dbo.Usuario
                   SET UltimoLoginUtc = @dtUtc,
                       NroIntentos    = 0,
                       IdEstado       = CASE WHEN IdEstado = 2 THEN 1 ELSE IdEstado END,
                       DVH            = @DVH
                 WHERE IdUsuario = @Id;";

            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@dtUtc", utcNow);
                cmd.Parameters.AddWithValue("@DVH", (object)dvh?.dvh ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Id", idUsuario);
                cmd.ExecuteNonQuery();
            }

            var dvv = DVVDao.GetInstance().CalculateDVV("dbo.Usuario");
            DVVDao.GetInstance().AddUpdateDVV(new DVV { tabla = "Usuario", dvv = dvv });
        }
        #endregion

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
                new List<SqlParameter> {
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
                new List<SqlParameter> {
                    new SqlParameter("@mail", (object)email ?? DBNull.Value),
                    new SqlParameter("@id",   (object)excluirId ?? DBNull.Value)
                });

            return Convert.ToInt32(count) > 0;
        }
        #endregion

        #region Relaciones Usuario–Familia
        public bool DeleteUsuarioFamilia(int idFamilia)
        {
            const string sql = "DELETE FROM dbo.UsuarioFamilia WHERE IdFamilia = @IdFamilia;";
            var rows = SqlHelpers.GetInstance(Cnn).ExecuteQuery(
                sql, new List<SqlParameter> { new SqlParameter("@IdFamilia", idFamilia) });

            var dvv = DVVDao.GetInstance().CalculateDVV("dbo.UsuarioFamilia");
            DVVDao.GetInstance().AddUpdateDVV(new DVV { tabla = "UsuarioFamilia", dvv = dvv });
            return rows > 0;
        }

        public bool SetUsuarioFamilia(int idUsuario, int idFamilia)
        {
            const string del = "DELETE FROM dbo.UsuarioFamilia WHERE IdUsuario = @U;";
            SqlHelpers.GetInstance(Cnn).ExecuteQuery(del, new List<SqlParameter> { new SqlParameter("@U", idUsuario) });

            const string ins = "INSERT INTO dbo.UsuarioFamilia (IdUsuario, IdFamilia) VALUES (@U, @F);";
            var rows = SqlHelpers.GetInstance(Cnn).ExecuteQuery(
                ins,
                new List<SqlParameter> {
                    new SqlParameter("@U", idUsuario),
                    new SqlParameter("@F", idFamilia)
                });

            var dvv = DVVDao.GetInstance().CalculateDVV("dbo.UsuarioFamilia");
            DVVDao.GetInstance().AddUpdateDVV(new DVV { tabla = "UsuarioFamilia", dvv = dvv });
            return rows > 0;
        }

        public bool SetUsuarioFamilias(int idUsuario, IEnumerable<int> familiasIds)
        {
            const string del = "DELETE FROM dbo.UsuarioFamilia WHERE IdUsuario = @U;";
            SqlHelpers.GetInstance(Cnn).ExecuteQuery(del, new List<SqlParameter> { new SqlParameter("@U", idUsuario) });

            if (familiasIds != null)
            {
                foreach (var idF in familiasIds.Distinct())
                {
                    const string ins = "INSERT INTO dbo.UsuarioFamilia (IdUsuario, IdFamilia) VALUES (@U, @F);";
                    SqlHelpers.GetInstance(Cnn).ExecuteQuery(
                        ins, new List<SqlParameter> { new SqlParameter("@U", idUsuario), new SqlParameter("@F", idF) });
                }
            }

            var dvv = DVVDao.GetInstance().CalculateDVV("dbo.UsuarioFamilia");
            DVVDao.GetInstance().AddUpdateDVV(new DVV { tabla = "UsuarioFamilia", dvv = dvv });
            return true;
        }
        #endregion

        #region Usuario–Patente y helpers baja transaccional
        public bool SetPatentesDeUsuario(int idUsuario, IList<int> patentesIds)
        {
            patentesIds = patentesIds?.Distinct().ToList() ?? new List<int>();

            using (var cn = ConnectionFactory.Open())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    // Patentes actuales (con UPDLOCK para consistencia)
                    var actuales = new List<int>();
                    using (var cmd = new SqlCommand(
                        "SELECT IdPatente FROM dbo.UsuarioPatente WITH (UPDLOCK) WHERE IdUsuario = @u;", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@u", idUsuario);
                        using (var rd = cmd.ExecuteReader())
                            while (rd.Read()) actuales.Add(rd.GetInt32(0));
                    }

                    // Validación anti-huérfanas: no quitar la última asignación de una patente
                    var removidas = actuales.Except(patentesIds).ToList();
                    foreach (var idPat in removidas)
                    {
                        int restantes = PatenteDao.GetInstance()
                            .CountAsignacionesExcluyendoUsuario_tx(cn, tx, idPat, idUsuario);
                        if (restantes <= 0)
                            throw new Exception($"No se puede quitar la patente (Id={idPat}) porque quedaría sin asignar.");
                    }

                    using (var del = new SqlCommand(
                        "DELETE FROM dbo.UsuarioPatente WHERE IdUsuario = @u;", cn, tx))
                    {
                        del.Parameters.AddWithValue("@u", idUsuario);
                        del.ExecuteNonQuery();
                    }

                    if (patentesIds.Any())
                    {
                        const string ins = "INSERT INTO dbo.UsuarioPatente (IdUsuario, IdPatente) VALUES (@u, @p);";
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

                    // DVV usando las funciones transaccionales
                    var dvv = DVVDao.GetInstance().CalculateDVV("dbo.UsuarioPatente");
                    DVVDao.UpsertDVV_tx(cn, tx, "UsuarioPatente", dvv);

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

        public int CountHabilitados()
        {
            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(@"SELECT COUNT(*) FROM dbo.Usuario WHERE IdEstado = 1;", cn))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public int CountHabilitados(IEnumerable<int> ids)
        {
            var lista = (ids ?? Enumerable.Empty<int>()).Distinct().ToList();
            if (lista.Count == 0) return 0;

            var prm = Enumerable.Range(0, lista.Count).Select(i => "@p" + i).ToList();
            var sql = $"SELECT COUNT(*) FROM dbo.Usuario WHERE IdEstado = 1 AND IdUsuario IN ({string.Join(",", prm)});";

            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                for (int i = 0; i < lista.Count; i++)
                    cmd.Parameters.AddWithValue("@p" + i, lista[i]);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public int CountAdminsActivos()
        {
            const string sql = @"
                SELECT COUNT(DISTINCT u.IdUsuario)
                  FROM dbo.Usuario u
                  JOIN dbo.UsuarioFamilia uf ON uf.IdUsuario = u.IdUsuario
                  JOIN dbo.Familia f        ON f.IdFamilia = uf.IdFamilia
                 WHERE u.IdEstado = 1 AND f.Nombre = 'Administrador';";

            return Convert.ToInt32(SqlHelpers.GetInstance(Cnn).ExecuteScalar(sql));
        }

        public int CountAdminsActivos(IEnumerable<int> ids)
        {
            var lista = (ids ?? Enumerable.Empty<int>()).Distinct().ToList();
            if (lista.Count == 0) return 0;

            var prm = Enumerable.Range(0, lista.Count).Select(i => "@p" + i).ToList();
            var sql = $@"
                SELECT COUNT(DISTINCT u.IdUsuario)
                  FROM dbo.Usuario u
                  JOIN dbo.UsuarioFamilia uf ON uf.IdUsuario = u.IdUsuario
                  JOIN dbo.Familia f        ON f.IdFamilia = uf.IdFamilia
                 WHERE u.IdEstado = 1 AND f.Nombre = 'Administrador'
                   AND u.IdUsuario IN ({string.Join(",", prm)});";

            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                for (int i = 0; i < lista.Count; i++)
                    cmd.Parameters.AddWithValue("@p" + i, lista[i]);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void RemoveFamiliasDeUsuario(int idUsuario)
        {
            const string sql = "DELETE FROM dbo.UsuarioFamilia WHERE IdUsuario = @u;";
            SqlHelpers.GetInstance(Cnn).ExecuteQuery(sql, new List<SqlParameter> { new SqlParameter("@u", idUsuario) });

            var dvv = DVVDao.GetInstance().CalculateDVV("dbo.UsuarioFamilia");
            DVVDao.GetInstance().AddUpdateDVV(new DVV { tabla = "UsuarioFamilia", dvv = dvv });
        }

        public void RemovePatentesDeUsuario(int idUsuario)
        {
            const string sql = "DELETE FROM dbo.UsuarioPatente WHERE IdUsuario = @u;";
            SqlHelpers.GetInstance(Cnn).ExecuteQuery(sql, new List<SqlParameter> { new SqlParameter("@u", idUsuario) });

            var dvv = DVVDao.GetInstance().CalculateDVV("dbo.UsuarioPatente");
            DVVDao.GetInstance().AddUpdateDVV(new DVV { tabla = "UsuarioPatente", dvv = dvv });
        }

        public void BajaLogica(int idUsuario)
        {
            const string sql = "UPDATE dbo.Usuario SET IdEstado = 3 WHERE IdUsuario = @u;";
            SqlHelpers.GetInstance(Cnn).ExecuteQuery(sql, new List<SqlParameter> { new SqlParameter("@u", idUsuario) });

            var dvv = DVVDao.GetInstance().CalculateDVV("dbo.Usuario");
            DVVDao.GetInstance().AddUpdateDVV(new DVV { tabla = "Usuario", dvv = dvv });
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
            { try { bytes = Convert.FromBase64String(s); } catch { bytes = null; } }
            return VarBinaryParam(name, bytes, size);
        }
        #endregion

        // ICRUD no usado directamente
        bool ICRUD<Usuario>.Add(Usuario alta) => throw new NotImplementedException();
        bool ICRUD<Usuario>.Update(Usuario update) => throw new NotImplementedException();
        bool ICRUD<Usuario>.Delete(Usuario baja) => throw new NotImplementedException();
    }
}
