// DAL/FamiliaDao.cs
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using BE;
using System.Linq;

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

        // (legacy)
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
                    bool activa = GetBool(f, true);
                    string fila = $"{0}|{f.Name}|{f.Descripcion}|{(activa ? 1 : 0)}";
                    string dvh = Services.DV.GetDV(fila);

                    int nuevoId;
                    using (var cmd = new SqlCommand(ins, cn, tx))
                    {
                        cmd.CommandTimeout = 120;
                        cmd.Parameters.AddWithValue("@n", (object)f.Name ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@d", (object)f.Descripcion ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@a", activa);
                        cmd.Parameters.AddWithValue("@dvh", (object)dvh ?? DBNull.Value);
                        nuevoId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    var dvv = DVVDao.GetInstance().CalculateDVV("Familia", useNoLock: true);
                    UpsertDVV_tx(cn, tx, "Familia", dvv, timeout: 120);
                    tx.Commit();
                    return nuevoId;
                }
                catch { tx.Rollback(); throw; }
            }
        }

        public bool Update(Familia f)
        {
            if (f == null) throw new ArgumentNullException(nameof(f));
            const string up = @"
                UPDATE Familia
                   SET Nombre=@n, Descripcion=@d, Activa=@a, DVH=@dvh
                 WHERE IdFamilia=@id";
            using (var cn = ConnectionFactory.Open())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    bool activa = GetBool(f, true);
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

                    var dvv = DVVDao.GetInstance().CalculateDVV("Familia");
                    UpsertDVV_tx(cn, tx, "Familia", dvv);
                    tx.Commit();
                    return true;
                }
                catch { tx.Rollback(); throw; }
            }
        }

        // ===== Atómicos + anti-duplicado =====
        public int AddWithPatentesAtomic(Familia f, IList<int> patentesIds)
        {
            if (f == null) throw new ArgumentNullException(nameof(f));
            if (patentesIds == null || patentesIds.Count == 0)
                throw new InvalidOperationException("No se puede crear una familia sin patentes.");

            using (var cn = ConnectionFactory.Open())
            using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    const string qDup = @"
                        SELECT COUNT(*) FROM dbo.Familia WITH (UPDLOCK, HOLDLOCK)
                        WHERE UPPER(LTRIM(RTRIM(Nombre))) = UPPER(LTRIM(RTRIM(@n)));";
                    using (var chk = new SqlCommand(qDup, cn, tx))
                    {
                        chk.Parameters.AddWithValue("@n", (object)f.Name ?? DBNull.Value);
                        if (Convert.ToInt32(chk.ExecuteScalar()) > 0)
                            throw new InvalidOperationException("Ya existe una familia con ese nombre.");
                    }

                    bool activa = GetBool(f, true);
                    string fila = $"{0}|{f.Name}|{f.Descripcion}|{(activa ? 1 : 0)}";
                    string dvh = Services.DV.GetDV(fila);

                    int idFamilia;
                    const string qInsFam = @"
                        INSERT INTO dbo.Familia (Nombre, Descripcion, Activa, DVH)
                        VALUES (@n,@d,@a,@dvh);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    using (var cmd = new SqlCommand(qInsFam, cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@n", (object)f.Name ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@d", (object)f.Descripcion ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@a", activa);
                        cmd.Parameters.AddWithValue("@dvh", (object)dvh ?? DBNull.Value);
                        idFamilia = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    const string qInsFP = "INSERT INTO dbo.FamiliaPatente (IdFamilia, IdPatente) VALUES (@f,@p);";
                    foreach (var idP in patentesIds.Distinct())
                    {
                        using (var cmd = new SqlCommand(qInsFP, cn, tx))
                        {
                            cmd.Parameters.AddWithValue("@f", idFamilia);
                            cmd.Parameters.AddWithValue("@p", idP);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    var dvvF = DVVDao.GetInstance().CalculateDVV_tx(cn, tx, "dbo.Familia");
                    DVVDao.UpsertDVV_tx(cn, tx, "Familia", dvvF);

                    var dvvFP = DVVDao.GetInstance().CalculateDVV_tx(cn, tx, "dbo.FamiliaPatente");
                    DVVDao.UpsertDVV_tx(cn, tx, "FamiliaPatente", dvvFP);

                    tx.Commit();
                    return idFamilia;
                }
                catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
                { tx.Rollback(); throw new InvalidOperationException("Ya existe una familia con ese nombre.", ex); }
                catch { tx.Rollback(); throw; }
            }
        }

        public bool UpdateWithPatentesAtomic(Familia f, IList<int> patentesIds)
        {
            if (f == null) throw new ArgumentNullException(nameof(f));
            if (patentesIds == null || patentesIds.Count == 0)
                throw new InvalidOperationException("No se puede dejar la familia sin patentes.");

            using (var cn = ConnectionFactory.Open())
            using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    bool activa = GetBool(f, true);
                    string fila = $"{f.Id}|{f.Name}|{f.Descripcion}|{(activa ? 1 : 0)}";
                    string dvh = Services.DV.GetDV(fila);

                    const string qUpd = @"
                        UPDATE dbo.Familia
                           SET Nombre=@n, Descripcion=@d, Activa=@a, DVH=@dvh
                         WHERE IdFamilia=@id;";
                    using (var cmd = new SqlCommand(qUpd, cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@id", f.Id);
                        cmd.Parameters.AddWithValue("@n", (object)f.Name ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@d", (object)f.Descripcion ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@a", activa);
                        cmd.Parameters.AddWithValue("@dvh", (object)dvh ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }

                    const string qDel = "DELETE FROM dbo.FamiliaPatente WHERE IdFamilia=@f;";
                    using (var del = new SqlCommand(qDel, cn, tx))
                    {
                        del.Parameters.AddWithValue("@f", f.Id);
                        del.ExecuteNonQuery();
                    }

                    const string qIns = "INSERT INTO dbo.FamiliaPatente (IdFamilia, IdPatente) VALUES (@f,@p);";
                    foreach (var idP in patentesIds.Distinct())
                    {
                        using (var ins = new SqlCommand(qIns, cn, tx))
                        {
                            ins.Parameters.AddWithValue("@f", f.Id);
                            ins.Parameters.AddWithValue("@p", idP);
                            ins.ExecuteNonQuery();
                        }
                    }

                    var dvvF = DVVDao.GetInstance().CalculateDVV_tx(cn, tx, "dbo.Familia");
                    DVVDao.UpsertDVV_tx(cn, tx, "Familia", dvvF);

                    var dvvFP = DVVDao.GetInstance().CalculateDVV_tx(cn, tx, "dbo.FamiliaPatente");
                    DVVDao.UpsertDVV_tx(cn, tx, "FamiliaPatente", dvvFP);

                    tx.Commit();
                    return true;
                }
                catch { tx.Rollback(); throw; }
            }
        }

        public bool Delete(Familia familia, DVH dvh)
        {
            using (var cn = ConnectionFactory.Open())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    // 1) existencia
                    int existe;
                    using (var cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM dbo.Familia WITH (UPDLOCK, HOLDLOCK) WHERE IdFamilia=@id;", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@id", familia.Id);
                        existe = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    if (existe == 0) throw new Exception("La familia a eliminar no existe.");

                    // 2) ¿Es la familia Administrador?
                    string nombreFam;
                    using (var cmd = new SqlCommand(
                        "SELECT Nombre FROM dbo.Familia WITH (HOLDLOCK) WHERE IdFamilia=@id;", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@id", familia.Id);
                        nombreFam = cmd.ExecuteScalar() as string;
                    }
                    bool esAdmin = !string.IsNullOrWhiteSpace(nombreFam) &&
                                   (nombreFam.Equals("ADMIN", StringComparison.OrdinalIgnoreCase) ||
                                    nombreFam.Equals("ADMINISTRADOR", StringComparison.OrdinalIgnoreCase));

                    // 3) Solo si es Admin, verificar que no deje usuarios sin permisos
                    if (esAdmin)
                    {
                        var usuariosAfectados = new List<int>();
                        using (var cmd = new SqlCommand(
                            "SELECT IdUsuario FROM dbo.UsuarioFamilia WHERE IdFamilia=@id;", cn, tx))
                        {
                            cmd.Parameters.AddWithValue("@id", familia.Id);
                            using (var rd = cmd.ExecuteReader())
                                while (rd.Read()) usuariosAfectados.Add(rd.GetInt32(0));
                        }

                        foreach (var idUsuario in usuariosAfectados)
                        {
                            int otrasFamilias;
                            using (var cmd = new SqlCommand(
                                "SELECT COUNT(*) FROM dbo.UsuarioFamilia WHERE IdUsuario=@u AND IdFamilia<>@f;", cn, tx))
                            {
                                cmd.Parameters.AddWithValue("@u", idUsuario);
                                cmd.Parameters.AddWithValue("@f", familia.Id);
                                otrasFamilias = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            int patentesDirectas;
                            using (var cmd = new SqlCommand(
                                "SELECT COUNT(*) FROM dbo.UsuarioPatente WHERE IdUsuario=@u;", cn, tx))
                            {
                                cmd.Parameters.AddWithValue("@u", idUsuario);
                                patentesDirectas = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            if (otrasFamilias == 0 && patentesDirectas == 0)
                                throw new Exception($"No se puede eliminar la familia '{nombreFam}': dejaría usuarios sin permisos.");
                        }
                    }

                    // 4) Siempre validar patentes huérfanas
                    const string qH = @"
                        SELECT COUNT(*)
                        FROM dbo.FamiliaPatente fp
                        WHERE fp.IdFamilia=@f
                          AND NOT EXISTS (SELECT 1 FROM dbo.UsuarioPatente up WHERE up.IdPatente=fp.IdPatente)
                          AND NOT EXISTS (SELECT 1 FROM dbo.FamiliaPatente fp2
                                          WHERE fp2.IdPatente=fp.IdPatente AND fp2.IdFamilia<>fp.IdFamilia);";
                    using (var cmd = new SqlCommand(qH, cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@f", familia.Id);
                        if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                            throw new Exception($"No se puede eliminar la familia '{nombreFam ?? familia.Name}' porque dejaría patentes huérfanas.");
                    }

                    // 5) Eliminar relaciones + familia
                    new SqlCommand("DELETE FROM dbo.FamiliaPatente WHERE IdFamilia=@id", cn, tx)
                    { Parameters = { new SqlParameter("@id", familia.Id) } }.ExecuteNonQuery();

                    new SqlCommand("DELETE FROM dbo.UsuarioFamilia WHERE IdFamilia=@id", cn, tx)
                    { Parameters = { new SqlParameter("@id", familia.Id) } }.ExecuteNonQuery();

                    using (var cmd = new SqlCommand("DELETE FROM dbo.Familia WHERE IdFamilia=@id;", cn, tx))
                    { cmd.Parameters.AddWithValue("@id", familia.Id); cmd.ExecuteNonQuery(); }

                    // 6) DVV
                    var dvvF = DVVDao.GetInstance().CalculateDVV_tx(cn, tx, "dbo.Familia");
                    DVVDao.UpsertDVV_tx(cn, tx, "Familia", dvvF);
                    var dvvFP = DVVDao.GetInstance().CalculateDVV_tx(cn, tx, "dbo.FamiliaPatente");
                    DVVDao.UpsertDVV_tx(cn, tx, "FamiliaPatente", dvvFP);
                    var dvvUF = DVVDao.GetInstance().CalculateDVV_tx(cn, tx, "dbo.UsuarioFamilia");
                    DVVDao.UpsertDVV_tx(cn, tx, "UsuarioFamilia", dvvUF);

                    tx.Commit();
                    return true;
                }
                catch (Exception ex) { tx.Rollback(); throw new Exception("Error al eliminar familia: " + ex.Message, ex); }
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
            => SetPatentesDeFamilia(familia?.Id ?? 0, patentesIds);

        public int SetPatentes(int idFamilia, IEnumerable<int> patentesIds)
            => SetPatentesDeFamilia(idFamilia, patentesIds);

        public int SetPatentesDeFamilia(int idFamilia, IEnumerable<int> patentesIds)
        {
            var nuevas = (patentesIds ?? Enumerable.Empty<int>()).Distinct().ToList();

            using (var cn = ConnectionFactory.Open())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    if (!nuevas.Any())
                        throw new Exception($"No se permite dejar la familia (Id={idFamilia}) sin patentes.");

                    var actuales = new List<int>();
                    using (var cmd = new SqlCommand(
                        "SELECT IdPatente FROM FamiliaPatente WITH (UPDLOCK) WHERE IdFamilia = @f", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@f", idFamilia);
                        using (var dr = cmd.ExecuteReader())
                            while (dr.Read()) actuales.Add(dr.GetInt32(0));
                    }

                    var removidas = actuales.Except(nuevas).ToList();
                    foreach (var idPat in removidas)
                    {
                        int restantes = PatenteDao.GetInstance()
                            .CountAsignacionesExcluyendoFamilia_tx(cn, tx, idPat, idFamilia);
                        if (restantes <= 0)
                            throw new Exception($"No se puede quitar la patente (Id={idPat}) de la familia porque quedaría sin asignar.");
                    }

                    using (var del = new SqlCommand("DELETE FROM FamiliaPatente WHERE IdFamilia=@f", cn, tx))
                    { del.Parameters.AddWithValue("@f", idFamilia); del.ExecuteNonQuery(); }

                    const string ins = "INSERT INTO FamiliaPatente (IdFamilia, IdPatente) VALUES (@f, @p)";
                    foreach (var idPat in nuevas)
                    {
                        using (var cmd = new SqlCommand(ins, cn, tx))
                        {
                            cmd.Parameters.AddWithValue("@f", idFamilia);
                            cmd.Parameters.AddWithValue("@p", idPat);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    var dvvDao = DVVDao.GetInstance();
                    UpsertDVV_tx(cn, tx, "FamiliaPatente", dvvDao.CalculateDVV("FamiliaPatente"));

                    tx.Commit();
                    return nuevas.Count;
                }
                catch { tx.Rollback(); throw; }
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
        private static void UpsertDVV_tx(SqlConnection cn, SqlTransaction tx, string tabla, string dvv, int timeout = 120)
        {
            const string check = "SELECT COUNT(*) FROM DVV WHERE Tabla = @t";
            using (var cmd = new SqlCommand(check, cn, tx))
            {
                cmd.CommandTimeout = timeout;
                cmd.Parameters.AddWithValue("@t", tabla);
                int c = Convert.ToInt32(cmd.ExecuteScalar());

                string sql = c > 0 ? "UPDATE DVV SET DVV=@d WHERE Tabla=@t"
                                   : "INSERT INTO DVV (Tabla, DVV) VALUES (@t,@d)";
                using (var up = new SqlCommand(sql, cn, tx))
                {
                    up.CommandTimeout = timeout;
                    up.Parameters.AddWithValue("@t", tabla);
                    up.Parameters.AddWithValue("@d", (object)dvv ?? DBNull.Value);
                    up.ExecuteNonQuery();
                }
            }
        }
    }
}
