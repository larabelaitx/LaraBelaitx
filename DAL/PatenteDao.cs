using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DAL.Mappers;
using BE;

namespace DAL
{
    public class PatenteDao : ICRUD<Permiso>
    {
        // Singleton
        private static PatenteDao _instance;
        public static PatenteDao GetInstance() => _instance ?? (_instance = new PatenteDao());
        private PatenteDao() { }

        // ------------------ Lecturas ------------------

        public HashSet<Permiso> GetAll()
        {
            const string sql = "SELECT IdPatente, Nombre FROM Patente";
            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                da.Fill(dt);
                return MPPatente.GetInstance().MapPatentes(dt);
            }
        }

        public Permiso GetById(int idPatente)
        {
            const string sql = "SELECT IdPatente, Nombre FROM Patente WHERE IdPatente = @id";
            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", idPatente);
                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return MPPatente.GetInstance().Map(dt);
                }
            }
        }

        public HashSet<Permiso> GetPatentesUsuario(int idUsuario)
        {
            const string sql = @"
                SELECT P.IdPatente, P.Nombre
                FROM Patente AS P 
                INNER JOIN UsuarioPatente AS UP ON P.IdPatente = UP.IdPatente 
                WHERE UP.IdUsuario = @u";
            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@u", idUsuario);
                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return MPPatente.GetInstance().MapPatentes(dt);
                }
            }
        }

        public HashSet<Permiso> GetPatentesFamilia(int idFamilia)
        {
            const string sql = @"
                SELECT P.IdPatente, P.Nombre
                FROM Patente AS P 
                INNER JOIN FamiliaPatente AS FP ON P.IdPatente = FP.IdPatente 
                WHERE FP.IdFamilia = @f";
            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@f", idFamilia);
                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return MPPatente.GetInstance().MapPatentes(dt);
                }
            }
        }

        public HashSet<Permiso> GetBySector(int sector)
        {
            // Si tenés columna Sector en Patente, filtrá; si no, devolvé todo.
            // const string sql = "SELECT IdPatente, Nombre, Sector FROM Patente WHERE Sector=@s";
            const string sql = "SELECT IdPatente, Nombre FROM Patente";
            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@s", sector);
                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return MPPatente.GetInstance().MapPatentes(dt);
                }
            }
        }

        // ------------------ Relaciones / util ------------------

        public bool DeletePatenteFamilia(int idFamilia)
        {
            const string sql = "DELETE FROM FamiliaPatente WHERE IdFamilia = @f";
            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@f", idFamilia);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool CheckPatenteAsing(int idPatente)
        {
            int familias = GetCountFamiliasPatente(idPatente);
            int usuarios = GetCountUsuarioPatentes(idPatente);
            // Si la patente está asignada al menos a un lugar (familia/usuario) => true
            return (familias + usuarios) > 0;
        }

        private int GetCountUsuarioPatentes(int idPatente)
        {
            const string sql = "SELECT COUNT(*) FROM UsuarioPatente WHERE IdPatente = @p";
            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@p", idPatente);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private int GetCountFamiliasPatente(int idPatente)
        {
            const string sql = "SELECT COUNT(*) FROM FamiliaPatente WHERE IdPatente = @p";
            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@p", idPatente);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // ------------------ ICRUD<Permiso> ------------------
        // Si NO usás altas/bajas/modificaciones de Patentes desde la app,
        // dejamos NotImplemented para no introducir lógica que no usás.

        bool ICRUD<Permiso>.Add(Permiso alta) => throw new NotImplementedException();
        bool ICRUD<Permiso>.Update(Permiso update) => throw new NotImplementedException();
        bool ICRUD<Permiso>.Delete(Permiso delete) => throw new NotImplementedException();

        // La interfaz pide List<Permiso>. Convertimos el HashSet a List para cumplir la firma.
        List<Permiso> ICRUD<Permiso>.GetAll() => new List<Permiso>(GetAll());

        Permiso ICRUD<Permiso>.GetById(int id) => GetById(id);
    }
}
