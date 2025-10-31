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
        // ------------------ Singleton ------------------
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
                        cmd.Parameters.AddWithValue("@n", (object)f.Name ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@d", (object)f.Descripcion ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@a", activa);
                        cmd.Parameters.AddWithValue("@dvh", (object)dvh ?? DBNull.Value);
                        nuevoId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    var dvv = DAL.DVVDao.GetInstance().CalculateDVV("Familia");
                    UpsertDVV_tx(cn, tx, "Familia", dvv);

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
                    throw;
                }
            }
        }

        public int Delete(Familia f)
        {
            if (f == null) throw new ArgumentNullException(nameof(f));

            using (var cn = ConnectionFactory.Open())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    // 1) Patentes de la familia
                    var patentes = new List<int>();
                    using (var cmd = new SqlCommand(
                        "SELECT IdPatente FROM FamiliaPatente WHERE IdFamilia = @f", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@f", f.Id);
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read()) patentes.Add(dr.GetInt32(0));
                        }
                    }

                    // 2) Validación anti-huérfanas: cada patente debe seguir asignada
                    foreach (var idPat in patentes)
                    {
                        int restantes = PatenteDao.GetInstance()
                            .CountAsignacionesPatenteExcluyendoFamilia(idPat, f.Id);

                        if (restantes <= 0)
                            throw new Exception(
                                $"No se puede eliminar la familia. La patente Id={idPat} quedaría sin asignar.");
                    }

                    // 3) Borrar relaciones (UsuarioFamilia y FamiliaPatente)
                    using (var delUF = new SqlCommand(
                        "DELETE FROM UsuarioFamilia WHERE IdFamilia = @f", cn, tx))
                    {
                        delUF.Parameters.AddWithValue("@f", f.Id);
                        delUF.ExecuteNonQuery();
                    }

                    using (var delFP = new SqlCommand(
                        "DELETE FROM FamiliaPatente WHERE IdFamilia = @f", cn, tx))
                    {
                        delFP.Parameters.AddWithValue("@f", f.Id);
                        delFP.ExecuteNonQuery();
                    }

                    // 4) Borrar familia
                    int affected;
                    using (var delFam = new SqlCommand(
                        "DELETE FROM Familia WHERE IdFamilia = @id", cn, tx))
                    {
                        delFam.Parameters.AddWithValue("@id", f.Id);
                        affected = delFam.ExecuteNonQuery();
                    }

                    // 5) DVV de tablas afectadas
                    var dvv = DAL.DVVDao.GetInstance();
                    UpsertDVV_tx(cn, tx, "UsuarioFamilia", dvv.CalculateDVV("UsuarioFamilia"));
                    UpsertDVV_tx(cn, tx, "FamiliaPatente", dvv.CalculateDVV("FamiliaPatente"));
                    UpsertDVV_tx(cn, tx, "Familia", dvv.CalculateDVV("Familia"));

                    tx.Commit();
                    return affected; // 1 si borró la familia, 0 si no
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public int DeleteSafe(Familia f)
        {
            if (f == null) throw new ArgumentNullException(nameof(f));

            using (var cn = ConnectionFactory.Open())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    // 1) Patentes de la familia
                    var patentes = new List<int>();
                    using (var cmd = new SqlCommand("SELECT IdPatente FROM FamiliaPatente WHERE IdFamilia=@f", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@f", f.Id);
                        using (var dr = cmd.ExecuteReader())
                            while (dr.Read()) patentes.Add(dr.GetInt32(0));
                    }

                    // 2) Validar que cada patente quede asignada en algún otro lado
                    foreach (var idPat in patentes)
                    {
                        int restantes = PatenteDao.GetInstance()
                            .CountAsignacionesPatenteExcluyendoFamilia(idPat, f.Id);

                        if (restantes <= 0)
                            throw new Exception($"No se puede borrar la familia '{f.Name}': la patente (Id={idPat}) quedaría sin asignar.");
                    }

                    // 3) Borrar relaciones Usuario-Familia y Familia-Patente
                    using (var cmd = new SqlCommand("DELETE FROM UsuarioFamilia WHERE IdFamilia=@f", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@f", f.Id);
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new SqlCommand("DELETE FROM FamiliaPatente WHERE IdFamilia=@f", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@f", f.Id);
                        cmd.ExecuteNonQuery();
                    }

                    // 4) Borrar la familia
                    int rows;
                    using (var cmd = new SqlCommand("DELETE FROM Familia WHERE IdFamilia=@f", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@f", f.Id);
                        rows = cmd.ExecuteNonQuery();
                    }

                    // 5) DVV
                    var dvv1 = DVVDao.GetInstance().CalculateDVV("UsuarioFamilia");
                    UpsertDVV_tx(cn, tx, "UsuarioFamilia", dvv1);
                    var dvv2 = DVVDao.GetInstance().CalculateDVV("FamiliaPatente");
                    UpsertDVV_tx(cn, tx, "FamiliaPatente", dvv2);
                    var dvv3 = DVVDao.GetInstance().CalculateDVV("Familia");
                    UpsertDVV_tx(cn, tx, "Familia", dvv3);

                    tx.Commit();
                    return rows;
                }
                catch
                {
                    tx.Rollback();
                    throw;
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

        // Compatibilidad: a veces llaman con objeto Familia
        public int SetPatentes(Familia familia, IEnumerable<int> patentesIds)
        {
            if (familia == null) throw new ArgumentNullException(nameof(familia));
            return SetPatentesDeFamilia(familia.Id, patentesIds);
        }

        // …y a veces con id
        public int SetPatentes(int idFamilia, IEnumerable<int> patentesIds)
            => SetPatentesDeFamilia(idFamilia, patentesIds);

        public int SetPatentesDeFamilia(int idFamilia, IEnumerable<int> patentesIds)
        {
            patentesIds = patentesIds?.Distinct() ?? Enumerable.Empty<int>();
            int affected = 0;

            using (var cn = ConnectionFactory.Open())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    // 1) Leer actuales de la familia
                    var actuales = new List<int>();
                    using (var cmd = new SqlCommand("SELECT IdPatente FROM FamiliaPatente WHERE IdFamilia=@f", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@f", idFamilia);
                        using (var dr = cmd.ExecuteReader())
                            while (dr.Read()) actuales.Add(dr.GetInt32(0));
                    }

                    // 2) Determinar removidas
                    var removidas = actuales.Except(patentesIds).ToList();

                    // 3) Validación anti-huérfanas (si saco X de la familia, ¿sigue existiendo en otro lado?)
                    foreach (var idPat in removidas)
                    {
                        int restantes = PatenteDao.GetInstance()
                            .CountAsignacionesPatenteExcluyendoFamilia(idPat, idFamilia);

                        if (restantes <= 0)
                            throw new Exception($"No se puede quitar la patente (Id={idPat}) de la familia porque quedaría sin asignar.");
                    }

                    // 4) Borrar e insertar
                    using (var del = new SqlCommand("DELETE FROM FamiliaPatente WHERE IdFamilia = @f", cn, tx))
                    {
                        del.Parameters.AddWithValue("@f", idFamilia);
                        affected += del.ExecuteNonQuery();
                    }

                    if (patentesIds.Any())
                    {
                        const string ins = "INSERT INTO FamiliaPatente (IdFamilia, IdPatente) VALUES (@f, @p)";
                        foreach (var idPat in patentesIds)
                        {
                            using (var cmd = new SqlCommand(ins, cn, tx))
                            {
                                cmd.Parameters.AddWithValue("@f", idFamilia);
                                cmd.Parameters.AddWithValue("@p", idPat);
                                affected += cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    var dvv = DAL.DVVDao.GetInstance().CalculateDVV("FamiliaPatente");
                    UpsertDVV_tx(cn, tx, "FamiliaPatente", dvv);

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
