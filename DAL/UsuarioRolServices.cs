using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                            Nombre = dr.GetString(1),
                            Descripcion = dr.IsDBNull(2) ? "" : dr.GetString(2)
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
                            Nombre = dr.GetString(1),
                            Descripcion = dr.IsDBNull(2) ? "" : dr.GetString(2)
                        });
                    }
                }
            }
            return result;
        }

        public void AsignarFamilias(int idUsuario, List<int> familiasIds)
        {
            using (var cn = ConnectionFactory.Open())
            {
                using (var del = new SqlCommand("DELETE FROM UsuarioFamilia WHERE IdUsuario = @idUsuario", cn))
                {
                    del.Parameters.AddWithValue("@idUsuario", idUsuario);
                    del.ExecuteNonQuery();
                }

                foreach (var idFam in familiasIds)
                {
                    using (var ins = new SqlCommand(
                        "INSERT INTO UsuarioFamilia (IdUsuario, IdFamilia) VALUES (@u, @f)", cn))
                    {
                        ins.Parameters.AddWithValue("@u", idUsuario);
                        ins.Parameters.AddWithValue("@f", idFam);
                        ins.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
