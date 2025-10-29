using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using BE;

namespace DAL
{
    public class UsuarioRolDao
    {
        public List<Familia> ObtenerFamiliasDisponibles(int idUsuario)
        {
            var result = new List<Familia>();
            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(
                @"SELECT IdFamilia, Nombre, Descripcion 
                  FROM Familia 
                  WHERE IdFamilia NOT IN (
                        SELECT IdFamilia FROM UsuarioFamilia WHERE IdUsuario = @idUsuario)", cn))
            {
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        result.Add(new Familia
                        {
                            Id = dr.GetInt32(0),
                            Name = dr.IsDBNull(1) ? null : dr.GetString(1),
                            Descripcion = dr.IsDBNull(2) ? string.Empty : dr.GetString(2)
                        });
                    }
                }
            }
            return result;
        }

        public List<Familia> ObtenerFamiliasAsignadas(int idUsuario)
        {
            var result = new List<Familia>();
            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(
                @"SELECT f.IdFamilia, f.Nombre, f.Descripcion
                  FROM Familia f
                  INNER JOIN UsuarioFamilia uf ON uf.IdFamilia = f.IdFamilia
                  WHERE uf.IdUsuario = @idUsuario", cn))
            {
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        result.Add(new Familia
                        {
                            Id = dr.GetInt32(0),
                            Name = dr.IsDBNull(1) ? null : dr.GetString(1),
                            Descripcion = dr.IsDBNull(2) ? string.Empty : dr.GetString(2)
                        });
                    }
                }
            }
            return result;
        }

        public bool AsignarFamilias(int idUsuario, List<int> familiasIds)
        {
            familiasIds = familiasIds ?? new List<int>();

            using (var cn = ConnectionFactory.Open())
            using (var tx = cn.BeginTransaction())
            {
                try
                {
                    // 1) Limpiar asignaciones actuales
                    using (var del = new SqlCommand("DELETE FROM UsuarioFamilia WHERE IdUsuario = @u", cn, tx))
                    {
                        del.Parameters.AddWithValue("@u", idUsuario);
                        del.ExecuteNonQuery();
                    }

                    // 2) Insertar nuevas (si hay)
                    if (familiasIds.Count > 0)
                    {
                        const string insSql = "INSERT INTO UsuarioFamilia (IdUsuario, IdFamilia) VALUES (@u, @f)";
                        foreach (var idFam in new HashSet<int>(familiasIds)) // evita duplicados
                        {
                            using (var ins = new SqlCommand(insSql, cn, tx))
                            {
                                ins.Parameters.AddWithValue("@u", idUsuario);
                                ins.Parameters.AddWithValue("@f", idFam);
                                ins.ExecuteNonQuery();
                            }
                        }
                    }

                    // 3) Recalcular DVV de la tabla relacional
                    var dvvDao = DVVDao.GetInstance();
                    var dvv = new DVV { tabla = "UsuarioFamilia", dvv = dvvDao.CalculateDVV("UsuarioFamilia") };
                    dvvDao.AddUpdateDVV(dvv);

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
    }
}
