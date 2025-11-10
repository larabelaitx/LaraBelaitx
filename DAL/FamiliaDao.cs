using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using BE;
using System.Linq;
using DAL.Mappers;

namespace DAL
{
    public sealed class FamiliaDao
    {
        private static FamiliaDao _inst;
        public static FamiliaDao GetInstance() => _inst ?? (_inst = new FamiliaDao());
        private FamiliaDao() { }

        // ----------------------- CRUD -----------------------

        public List<Familia> GetAll()
        {
            const string sql = "SELECT IdFamilia, Nombre, Descripcion, Activa FROM Familia";
            var list = new List<Familia>();

            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            using (var dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    var fam = new Familia
                    {
                        Id = dr.GetInt32(0),
                        Name = dr.IsDBNull(1) ? null : dr.GetString(1),
                        Descripcion = dr.IsDBNull(2) ? null : dr.GetString(2)
                    };

                    bool dbActiva = !dr.IsDBNull(3) && dr.GetBoolean(3);
                    TrySetBool(fam, "Activa", dbActiva);
                    TrySetBool(fam, "Activo", dbActiva);
                    TrySetBool(fam, "IsEnabled", dbActiva);

                    list.Add(fam);
                }
            }
            return list;
        }

        public Familia GetById(int id)
        {
            const string sql = "SELECT IdFamilia, Nombre, Descripcion, Activa FROM Familia WHERE IdFamilia = @id";
            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (var dr = cmd.ExecuteReader())
                {
                    if (!dr.Read()) return null;

                    var fam = new Familia
                    {
                        Id = dr.GetInt32(0),
                        Name = dr.IsDBNull(1) ? null : dr.GetString(1),
                        Descripcion = dr.IsDBNull(2) ? null : dr.GetString(2)
                    };

                    bool dbActiva = !dr.IsDBNull(3) && dr.GetBoolean(3);
                    TrySetBool(fam, "Activa", dbActiva);
                    TrySetBool(fam, "Activo", dbActiva);
                    TrySetBool(fam, "IsEnabled", dbActiva);

                    return fam;
                }
            }
        }

        public int Add(Familia f)
        {
            if (f == null) throw new ArgumentNullException(nameof(f));

            const string ins = @"
        INSERT INTO Familia (Nombre, Descripcion, Activa, DVH)
        VALUES (@n, @d, @a, @dvh);
        SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var cn = ConnectionFactory.Open())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    bool activa = GetBool(f, defaultValue: true);
                    string fila = $"{0}|{f.Name}|{f.Descripcion}|{(activa ? 1 : 0)}";
                    string dvh = Services.DV.GetDV(fila);

                    int nuevoId;
                    using (var cmd = new SqlCommand(ins, cn, tx))
                    {
                        cmd.CommandTimeout = 120; // ⏱️ subir timeout
                        cmd.Parameters.AddWithValue("@n", (object)f.Name ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@d", (object)f.Descripcion ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@a", activa);
                        cmd.Parameters.AddWithValue("@dvh", (object)dvh ?? DBNull.Value);
                        nuevoId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // 🔐 Calcular DVV con NOLOCK para no quedar esperando si otra sesión lee DVH
                    var dvv = DAL.DVVDao.GetInstance().CalculateDVV("Familia", useNoLock: true);
                    UpsertDVV_tx(cn, tx, "Familia", dvv, timeout: 120);

                    tx.Commit();
                    return nuevoId;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public bool Update(Familia f)
        {
            if (f == null) throw new ArgumentNullException(nameof(f));

            const string up = @"
                UPDATE Familia
                   SET Nombre = @n,
                       Descripcion = @d,
                       Activa = @a,
                       DVH = @dvh
                 WHERE IdFamilia = @id";

            using (var cn = ConnectionFactory.Open())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    bool activa = GetBool(f, defaultValue: true);
                    string fila = $"{f.Id}|{f.Name}|{f.Descripcion}|{(activa ? 1 : 0)}";
                    string dvh = Services.DV.GetDV(fila);

                    using (var cmd = new SqlCommand(up, cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@id", f.Id);
                        cmd.Parameters.AddWithValue("@n", (object)f.Name ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@d", (object)f.Descripcion ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@a", activa);
                        cmd.Parameters.AddWithValue("@dvh", (object)dvh ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }

                    var dvv = DAL.DVVDao.GetInstance().CalculateDVV("Familia");
                    UpsertDVV_tx(cn, tx, "Familia", dvv);

                    tx.Commit();
                    return true;
                }
                catch
                {
                    tx.Rollback();
                    tx.Rollback();
                    throw;
                }
            }
        }

        public bool Delete(Familia familia, DVH dvh)
        {
            using (var cn = ConnectionFactory.Open())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    // 1️⃣ Verifica existencia de la familia
                    int existe;
                    using (var cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM dbo.Familia WITH (UPDLOCK, HOLDLOCK) WHERE IdFamilia = @id;", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@id", familia.Id);
                        existe = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    if (existe == 0)
                        throw new Exception("La familia a eliminar no existe.");

                    // 2️⃣ Verifica usuarios asignados a la familia
                    var usuariosAfectados = new List<int>();
                    using (var cmd = new SqlCommand(
                        "SELECT IdUsuario FROM dbo.UsuarioFamilia WHERE IdFamilia = @id;", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@id", familia.Id);
                        using (var rd = cmd.ExecuteReader())
                            while (rd.Read()) usuariosAfectados.Add(rd.GetInt32(0));
                    }

                    // 3️⃣ Verifica si al menos un usuario quedaría sin permisos
                    foreach (var idUsuario in usuariosAfectados)
                    {
                        // ¿Tiene otras familias?
                        int otrasFamilias;
                        using (var cmd = new SqlCommand(
                            "SELECT COUNT(*) FROM dbo.UsuarioFamilia WHERE IdUsuario = @u AND IdFamilia <> @f;", cn, tx))
                        {
                            cmd.Parameters.AddWithValue("@u", idUsuario);
                            cmd.Parameters.AddWithValue("@f", familia.Id);
                            otrasFamilias = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // ¿Tiene patentes directas?
                        int patentesDirectas;
                        using (var cmd = new SqlCommand(
                            "SELECT COUNT(*) FROM dbo.UsuarioPatente WHERE IdUsuario = @u;", cn, tx))
                        {
                            cmd.Parameters.AddWithValue("@u", idUsuario);
                            patentesDirectas = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        if (otrasFamilias == 0 && patentesDirectas == 0)
                            throw new Exception($"No se puede eliminar la familia '{familia.Name}': el usuario (Id={idUsuario}) quedaría sin permisos.");
                    }

                    // 4️⃣ Verifica patentes huérfanas (sin usuarios ni otras familias)
                    const string qHuérfanas = @"
                SELECT COUNT(*)
                FROM dbo.FamiliaPatente fp
                WHERE fp.IdFamilia = @f
                  AND NOT EXISTS (SELECT 1 FROM dbo.UsuarioPatente up WHERE up.IdPatente = fp.IdPatente)
                  AND NOT EXISTS (SELECT 1 FROM dbo.FamiliaPatente fp2 WHERE fp2.IdPatente = fp.IdPatente AND fp2.IdFamilia <> fp.IdFamilia);";
                    using (var cmd = new SqlCommand(qHuérfanas, cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@f", familia.Id);
                        if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                            throw new Exception($"No se puede eliminar la familia '{familia.Name}' porque dejaría patentes huérfanas.");
                    }

                    // 5️⃣ Elimina relaciones (en orden seguro)
                    new SqlCommand("DELETE FROM dbo.FamiliaPatente WHERE IdFamilia = @id", cn, tx)
                    { Parameters = { new SqlParameter("@id", familia.Id) } }.ExecuteNonQuery();

                    new SqlCommand("DELETE FROM dbo.UsuarioFamilia WHERE IdFamilia = @id", cn, tx)
                    { Parameters = { new SqlParameter("@id", familia.Id) } }.ExecuteNonQuery();

                    // 6️⃣ Baja lógica o eliminación directa de la familia
                    using (var cmd = new SqlCommand(
                        "DELETE FROM dbo.Familia WHERE IdFamilia = @id;", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@id", familia.Id);
                        cmd.ExecuteNonQuery();
                    }

                    // 7️⃣ Recalcula DVV
                    var dvvF = DVVDao.GetInstance().CalculateDVV_tx(cn, tx, "dbo.Familia");
                    DVVDao.UpsertDVV_tx(cn, tx, "Familia", dvvF);

                    var dvvFP = DVVDao.GetInstance().CalculateDVV_tx(cn, tx, "dbo.FamiliaPatente");
                    DVVDao.UpsertDVV_tx(cn, tx, "FamiliaPatente", dvvFP);

                    var dvvUF = DVVDao.GetInstance().CalculateDVV_tx(cn, tx, "dbo.UsuarioFamilia");
                    DVVDao.UpsertDVV_tx(cn, tx, "UsuarioFamilia", dvvUF);

                    tx.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    throw new Exception("Error al eliminar familia: " + ex.Message, ex);
                }
            }
        }


        private static void UpsertDVV_tx(SqlConnection cn, SqlTransaction tx, string tabla, string dvv, int timeout = 120)
        {
            const string check = "SELECT COUNT(*) FROM DVV WHERE Tabla = @t";
            using (var cmd = new SqlCommand(check, cn, tx))
            {
                cmd.CommandTimeout = timeout;
                cmd.Parameters.AddWithValue("@t", tabla);
                int c = Convert.ToInt32(cmd.ExecuteScalar());

                string sql = c > 0
                    ? "UPDATE DVV SET DVV = @d WHERE Tabla = @t"
                    : "INSERT INTO DVV (Tabla, DVV) VALUES (@t, @d)";

                using (var up = new SqlCommand(sql, cn, tx))
                {
                    up.CommandTimeout = timeout;
                    up.Parameters.AddWithValue("@t", tabla);
                    up.Parameters.AddWithValue("@d", (object)dvv ?? DBNull.Value);
                    up.ExecuteNonQuery();
                }
            }
        }


        // ------------------- Relaciones --------------------

        public List<Familia> GetFamiliasUsuario(int idUsuario)
        {
            const string sql = @"
                SELECT f.IdFamilia, f.Nombre, f.Descripcion, f.Activa
                FROM UsuarioFamilia uf
                INNER JOIN Familia f ON f.IdFamilia = uf.IdFamilia
                WHERE uf.IdUsuario = @u";

            var list = new List<Familia>();
            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@u", idUsuario);
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var fam = new Familia
                        {
                            Id = dr.GetInt32(0),
                            Name = dr.IsDBNull(1) ? null : dr.GetString(1),
                            Descripcion = dr.IsDBNull(2) ? null : dr.GetString(2)
                        };
                        bool dbActiva = !dr.IsDBNull(3) && dr.GetBoolean(3);
                        TrySetBool(fam, "Activa", dbActiva);
                        TrySetBool(fam, "Activo", dbActiva);
                        TrySetBool(fam, "IsEnabled", dbActiva);

                        list.Add(fam);
                    }
                }
            }
            return list;
        }

        public int SetPatentes(Familia familia, IEnumerable<int> patentesIds)
        {
            if (familia == null) throw new ArgumentNullException(nameof(familia));
            return SetPatentesDeFamilia(familia.Id, patentesIds);
        }

        public int SetPatentes(int idFamilia, IEnumerable<int> patentesIds)
            => SetPatentesDeFamilia(idFamilia, patentesIds);

        public int SetPatentesDeFamilia(int idFamilia, IEnumerable<int> patentesIds)
        {
            var nuevas = (patentesIds ?? Enumerable.Empty<int>()).Distinct().ToList();
            int affected = 0;

            using (var cn = ConnectionFactory.Open())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    // 1) Leer asignaciones actuales con UPDLOCK (evita lecturas sucias sin mantener bloqueo de rango)
                    var actuales = new List<int>();
                    using (var cmd = new SqlCommand(
                        "SELECT IdPatente FROM FamiliaPatente WITH (UPDLOCK) WHERE IdFamilia = @f", cn, tx)
                    { CommandTimeout = 60 })
                    {
                        cmd.Parameters.AddWithValue("@f", idFamilia);
                        using (var dr = cmd.ExecuteReader())
                            while (dr.Read()) actuales.Add(dr.GetInt32(0));
                    }

                    // 2) Validación anti-huérfanas (solo para las que se quitarían)
                    var removidas = actuales.Except(nuevas).ToList();
                    foreach (var idPat in removidas)
                    {
                        int restantes = PatenteDao.GetInstance()
                            .CountAsignacionesExcluyendoFamilia_tx(cn, tx, idPat, idFamilia);

                        if (restantes <= 0)
                            throw new Exception(
                                $"No se puede quitar la patente (Id={idPat}) de la familia porque quedaría sin asignar.");
                    }

                    // 3) Reemplazo completo
                    using (var del = new SqlCommand(
                        "DELETE FROM FamiliaPatente WHERE IdFamilia = @f", cn, tx)
                    { CommandTimeout = 60 })
                    {
                        del.Parameters.AddWithValue("@f", idFamilia);
                        affected += del.ExecuteNonQuery();
                    }

                    if (nuevas.Any())
                    {
                        const string ins = "INSERT INTO FamiliaPatente (IdFamilia, IdPatente) VALUES (@f, @p)";
                        foreach (var idPat in nuevas)
                        {
                            using (var cmd = new SqlCommand(ins, cn, tx) { CommandTimeout = 60 })
                            {
                                cmd.Parameters.AddWithValue("@f", idFamilia);
                                cmd.Parameters.AddWithValue("@p", idPat);
                                affected += cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    // 4) DVV
                    var dvvDao = DAL.DVVDao.GetInstance();
                    UpsertDVV_tx(cn, tx, "FamiliaPatente", dvvDao.CalculateDVV("FamiliaPatente"));

                    tx.Commit();
                    return affected;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }




        // ---------------------- Helpers --------------------

        private static bool GetBool(object target, bool defaultValue)
        {
            if (target == null) return defaultValue;
            foreach (var name in new[] { "Activa", "Activo", "IsEnabled" })
            {
                var p = target.GetType().GetProperty(name);
                if (p != null && p.PropertyType == typeof(bool))
                    return (bool)p.GetValue(target);
            }
            return defaultValue;
        }

        private static void TrySetBool(object target, string propName, bool value)
        {
            var p = target?.GetType().GetProperty(propName);
            if (p != null && p.CanWrite && p.PropertyType == typeof(bool))
                p.SetValue(target, value);
        }

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
