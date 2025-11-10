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

                // 🔐 Dinámicos para NO truncar
                VarBinaryParamDyn("@PasswordHash", usuario.PasswordHash),
                VarBinaryParamDyn("@PasswordSalt", usuario.PasswordSalt),
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

        public int AddReturnId(Usuario usuario, DVH dvh)
        {
            const string qCheck = @"
                SELECT COUNT(*) FROM dbo.Usuario
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
                     @NroIntentos, @DVH, @DebeCambiarContrasena);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var ps = new List<SqlParameter>
            {
                new SqlParameter("@IdEstado", usuario.EstadoUsuarioId > 0 ? usuario.EstadoUsuarioId : 1),
                new SqlParameter("@IdIdioma", (object)(usuario.IdiomaId ?? 1)),
                new SqlParameter("@Usuario", usuario.UserName),

                // 🔐 Dinámicos para NO truncar
                VarBinaryParamDyn("@PasswordHash", usuario.PasswordHash),
                VarBinaryParamDyn("@PasswordSalt", usuario.PasswordSalt),
                new SqlParameter("@PasswordIterations", usuario.PasswordIterations > 0 ? usuario.PasswordIterations : 100000),

                new SqlParameter("@Nombre",    DbOrNull(usuario.Name)),
                new SqlParameter("@Apellido",  DbOrNull(usuario.LastName)),
                new SqlParameter("@Mail",      DbOrNull(usuario.Email)),
                new SqlParameter("@Documento", DbOrNull(usuario.Documento)),
                new SqlParameter("@NroIntentos", usuario.Tries),
                new SqlParameter("@DVH", dvh?.dvh ?? (object)DBNull.Value),
                new SqlParameter("@DebeCambiarContrasena", usuario.DebeCambiarContraseña)
            };

            var newId = Convert.ToInt32(SqlHelpers.GetInstance(Cnn).ExecuteScalar(qIns, ps));

            // Recalcular DVV
            var dvv = DVVDao.GetInstance().CalculateDVV("dbo.Usuario");
            DVVDao.GetInstance().AddUpdateDVV(new DVV { tabla = "Usuario", dvv = dvv });

            return newId;
        }

        public bool Update(Usuario usuario, DVH dvh)
        {
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

                // 🔐 Dinámicos para NO truncar (se respetan nulls con COALESCE)
                VarBinaryParamDyn("@PasswordHash", usuario.PasswordHash),
                VarBinaryParamDyn("@PasswordSalt", usuario.PasswordSalt),
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
            using (var cn = ConnectionFactory.Open())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    int existe;
                    using (var cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM dbo.Usuario WITH (UPDLOCK, HOLDLOCK) WHERE IdUsuario = @id;", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@id", usuario.Id);
                        existe = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    if (existe == 0)
                        throw new Exception("El usuario a eliminar no existe.");

                    int vivos;
                    using (var cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM dbo.Usuario WITH (UPDLOCK, HOLDLOCK) WHERE IdEstado <> 3 AND IdUsuario <> @id;", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@id", usuario.Id);
                        vivos = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    if (vivos <= 0)
                        throw new Exception("No se puede eliminar: quedaría el sistema sin usuarios.");

                    using (var cmd = new SqlCommand(
                        "UPDATE dbo.Usuario SET IdEstado = 3, DVH = @DVH WHERE IdUsuario = @Id;", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@Id", usuario.Id);
                        cmd.Parameters.AddWithValue("@DVH", (object)dvh?.dvh ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }

                    var dvv = DVVDao.GetInstance().CalculateDVV_tx(cn, tx, "dbo.Usuario");
                    DVVDao.UpsertDVV_tx(cn, tx, "Usuario", dvv);

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

        public bool CambiarEstado(int idUsuario, int nuevoEstado, DVH dvh = null)
        {
            const string sql = @"
                UPDATE dbo.Usuario
                   SET IdEstado = @Estado,
                       DVH      = @DVH
                 WHERE IdUsuario = @Id;";

            var rows = SqlHelpers.GetInstance(Cnn).ExecuteQuery(
                sql,
                new List<SqlParameter> {
            new SqlParameter("@Estado", nuevoEstado),
            new SqlParameter("@DVH", (object)dvh?.dvh ?? DBNull.Value),
            new SqlParameter("@Id", idUsuario)
                });

            // Recalcular DVV de la tabla Usuario después de la actualización
            var dvv = DVVDao.GetInstance().CalculateDVV("dbo.Usuario");
            DVVDao.GetInstance().AddUpdateDVV(new DVV { tabla = "Usuario", dvv = dvv });

            return rows > 0;
        }

        public bool BajaLogicaSegura(int idUsuario, out string mensaje)
        {
            mensaje = null;

            using (var cn = ConnectionFactory.Open())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    // 1️⃣ Verifica existencia
                    using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Usuario WITH (UPDLOCK, HOLDLOCK) WHERE IdUsuario = @id", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@id", idUsuario);
                        if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                        {
                            mensaje = "El usuario no existe.";
                            tx.Rollback();
                            return false;
                        }
                    }

                    // 2️⃣ Evita dejar al sistema sin Administrador activo
                    const string qCheckAdmin = @"
                        SELECT COUNT(*) 
                        FROM Usuario u
                        JOIN UsuarioFamilia uf ON uf.IdUsuario = u.IdUsuario
                        JOIN Familia f ON f.IdFamilia = uf.IdFamilia
                        WHERE u.IdEstado = 1
                          AND (UPPER(f.Nombre) = 'ADMIN' OR UPPER(f.Nombre) = 'ADMINISTRADOR')
                          AND u.IdUsuario <> @id;";


                    using (var cmd = new SqlCommand(qCheckAdmin, cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@id", idUsuario);
                        if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                        {
                            mensaje = "Debe quedar al menos un usuario activo con el rol Administrador.";
                            tx.Rollback();
                            return false;
                        }
                    }

                    // 3️⃣ Evita patentes huérfanas
                    const string qCheckPatentes = @"
                        SELECT COUNT(*) 
                        FROM UsuarioPatente up
                        WHERE up.IdUsuario = @id
                          AND NOT EXISTS (SELECT 1 FROM UsuarioPatente up2 WHERE up2.IdPatente = up.IdPatente AND up2.IdUsuario <> @id)
                          AND NOT EXISTS (SELECT 1 FROM FamiliaPatente fp WHERE fp.IdPatente = up.IdPatente);";

                    using (var cmd = new SqlCommand(qCheckPatentes, cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@id", idUsuario);
                        if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                        {
                            mensaje = "La baja dejaría patentes huérfanas. Reasigná esas patentes a otra familia o usuario.";
                            tx.Rollback();
                            return false;
                        }
                    }

                    // 4️⃣ Baja lógica + limpieza
                    using (var cmd = new SqlCommand("UPDATE Usuario SET IdEstado = 3 WHERE IdUsuario = @id", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@id", idUsuario);
                        cmd.ExecuteNonQuery();
                    }
                    new SqlCommand("DELETE FROM UsuarioFamilia WHERE IdUsuario = @id", cn, tx)
                    { Parameters = { new SqlParameter("@id", idUsuario) } }.ExecuteNonQuery();
                    new SqlCommand("DELETE FROM UsuarioPatente WHERE IdUsuario = @id", cn, tx)
                    { Parameters = { new SqlParameter("@id", idUsuario) } }.ExecuteNonQuery();

                    tx.Commit();
                    mensaje = "Usuario dado de baja correctamente.";
                    return true;
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    mensaje = $"Error interno: {ex.Message}";
                    return false;
                }
            }
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
            var nuevas = (familiasIds ?? Enumerable.Empty<int>()).Distinct().ToList();

            using (var cn = ConnectionFactory.Open())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    // 0) ¿Existe el usuario?
                    using (var cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM dbo.Usuario WITH (UPDLOCK, HOLDLOCK) WHERE IdUsuario=@u", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@u", idUsuario);
                        if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                            throw new Exception("El usuario no existe.");
                    }

                    // 1) Patentes directas actuales (cuentan como permisos efectivos)
                    int patentesDirectas;
                    using (var cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM dbo.UsuarioPatente WITH (UPDLOCK) WHERE IdUsuario=@u", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@u", idUsuario);
                        patentesDirectas = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // 2) VALIDACIÓN ESTRICTA: cada familia debe existir y tener al menos 1 patente
                    int patentesHeredadasTotales = 0;
                    foreach (var idF in nuevas)
                    {
                        // existe la familia
                        using (var cmd = new SqlCommand(
                            "SELECT COUNT(*) FROM dbo.Familia WITH (UPDLOCK, HOLDLOCK) WHERE IdFamilia=@f", cn, tx))
                        {
                            cmd.Parameters.AddWithValue("@f", idF);
                            if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                                throw new Exception($"La familia Id={idF} no existe.");
                        }

                        // cantidad de patentes de la familia
                        int patFam;
                        using (var cmd = new SqlCommand(
                            "SELECT COUNT(*) FROM dbo.FamiliaPatente WITH (UPDLOCK) WHERE IdFamilia=@f", cn, tx))
                        {
                            cmd.Parameters.AddWithValue("@f", idF);
                            patFam = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        if (patFam == 0)
                            throw new Exception($"No se permite asignar familias sin patentes (IdFamilia={idF}).");

                        patentesHeredadasTotales += patFam;
                    }

                    // 3) El resultado NO puede dejar al usuario sin permisos efectivos
                    //    (directas + heredadas) > 0
                    if ((patentesDirectas + patentesHeredadasTotales) == 0)
                        throw new Exception("Operación inválida: el usuario quedaría sin permisos. Asigná al menos una familia con patentes o alguna patente directa.");

                    // 4) Reemplazo total de familias (sin auto-asignar 'Admin' ni nada “por defecto”)
                    using (var del = new SqlCommand(
                        "DELETE FROM dbo.UsuarioFamilia WHERE IdUsuario=@u", cn, tx))
                    {
                        del.Parameters.AddWithValue("@u", idUsuario);
                        del.ExecuteNonQuery();
                    }

                    if (nuevas.Any())
                    {
                        const string ins = "INSERT INTO dbo.UsuarioFamilia (IdUsuario, IdFamilia) VALUES (@u, @f)";
                        foreach (var idF in nuevas)
                        {
                            using (var cmd = new SqlCommand(ins, cn, tx))
                            {
                                cmd.Parameters.AddWithValue("@u", idUsuario);
                                cmd.Parameters.AddWithValue("@f", idF);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    // 5) DVV de UsuarioFamilia
                    var dvv = DVVDao.GetInstance().CalculateDVV_tx(cn, tx, "dbo.UsuarioFamilia");
                    DVVDao.UpsertDVV_tx(cn, tx, "UsuarioFamilia", dvv);

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

        #region Usuario–Patente (incluye R2)
        public bool SetPatentesDeUsuario(int idUsuario, IList<int> patentesIds)
        {
            patentesIds = patentesIds?.Distinct().ToList() ?? new List<int>();

            using (var cn = ConnectionFactory.Open())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    var actuales = new List<int>();
                    using (var cmd = new SqlCommand(
                        "SELECT IdPatente FROM dbo.UsuarioPatente WITH (UPDLOCK) WHERE IdUsuario = @u;", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@u", idUsuario);
                        using (var rd = cmd.ExecuteReader())
                            while (rd.Read()) actuales.Add(rd.GetInt32(0));
                    }

                    var removidas = actuales.Except(patentesIds).ToList();

                    // 🔹 Validación corregida: se bloquea solo si la patente quedaría huérfana globalmente
                    foreach (var idPat in removidas)
                    {
                        int asignacionesRestantes = PatenteDao.GetInstance()
                            .CountAsignacionesExcluyendoUsuario_tx(cn, tx, idPat, idUsuario);

                        if (asignacionesRestantes <= 0)
                            throw new Exception($"No se puede quitar la patente (Id={idPat}) porque quedaría sin asignar en el sistema.");
                    }

                    // 🔹 Ya no es necesario chequear si el usuario la conserva por familia;
                    // si la conserva por familia, simplemente no estará en 'removidas'

                    // Limpia todas las relaciones y vuelve a insertar las vigentes
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

                    // 🔹 Recalcular DVV de la tabla
                    var dvv = DVVDao.GetInstance().CalculateDVV_tx(cn, tx, "dbo.UsuarioPatente");
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

        private bool UsuarioConservaPorFamilia_tx(SqlConnection cn, SqlTransaction tx, int idUsuario, int idPatente)
        {
            const string sql = @"
                SELECT TOP 1 1
                  FROM dbo.UsuarioFamilia uf WITH (UPDLOCK)
                  JOIN dbo.FamiliaPatente fp WITH (UPDLOCK)
                    ON fp.IdFamilia = uf.IdFamilia
                 WHERE uf.IdUsuario = @u
                   AND fp.IdPatente = @p;";

            using (var cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.Parameters.AddWithValue("@u", idUsuario);
                cmd.Parameters.AddWithValue("@p", idPatente);
                var r = cmd.ExecuteScalar();
                return r != null;
            }
        }
        #endregion

        #region Helpers VARBINARY (incluye versión dinámica)
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

        private static SqlParameter VarBinaryParamDyn(string name, object value)
        {
            byte[] bytes = null;
            if (value is byte[] b) bytes = b;
            else if (value is string s && !string.IsNullOrWhiteSpace(s))
            { try { bytes = Convert.FromBase64String(s); } catch { bytes = null; } }

            var p = new SqlParameter(name, System.Data.SqlDbType.VarBinary, bytes?.Length ?? -1);
            p.Value = (object)bytes ?? DBNull.Value;
            return p;
        }
        #endregion

        // ICRUD no usado directamente
        bool ICRUD<Usuario>.Add(Usuario alta) => throw new NotImplementedException();
        bool ICRUD<Usuario>.Update(Usuario update) => throw new NotImplementedException();
        bool ICRUD<Usuario>.Delete(Usuario baja) => throw new NotImplementedException();
    }
}
