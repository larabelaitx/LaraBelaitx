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
            const string sql = "SELECT IdPatente, Nombre FROM Patente ORDER BY Nombre"; // <-- NEW
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
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddWithValue("@id", idPatente);
                var dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count == 0) return null;        // <-- NEW
                return MPPatente.GetInstance().Map(dt);
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
            // Si NO hay columna Sector, devolvé todo ordenado (o eliminá este método si no se usa).
            const string sql = "SELECT IdPatente, Nombre FROM Patente ORDER BY Nombre";
            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                da.Fill(dt);
                return MPPatente.GetInstance().MapPatentes(dt);
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
        public int CountAsignacionesPatente(int idPatente)
        {
            return GetCountUsuarioPatentes(idPatente) + GetCountFamiliasPatente(idPatente);
        }

        // NEW: Conteo excluyendo un usuario (para evaluar si al quitar una patente al usuario, sigue viva en otro lado)
        public int CountAsignacionesPatenteExcluyendoUsuario(int idPatente, int idUsuario)
        {
            int enUsuariosExceptoEste = GetCountUsuarioPatentesExcepto(idPatente, idUsuario);
            int enFamilias = GetCountFamiliasPatente(idPatente);
            return enUsuariosExceptoEste + enFamilias;
        }

        // NEW: Conteo excluyendo una familia (para evaluar si al quitar de familia, sigue viva en otro lado)
        public int CountAsignacionesPatenteExcluyendoFamilia(int idPatente, int idFamilia)
        {
            int enUsuarios = GetCountUsuarioPatentes(idPatente);
            int enFamiliasExceptoEsta = GetCountFamiliasPatenteExcepto(idPatente, idFamilia);
            return enUsuarios + enFamiliasExceptoEsta;
        }

        // NEW (helpers privados)
        private int GetCountUsuarioPatentesExcepto(int idPatente, int idUsuario)
        {
            const string sql = "SELECT COUNT(*) FROM UsuarioPatente WHERE IdPatente=@p AND IdUsuario<>@u";
            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@p", idPatente);
                cmd.Parameters.AddWithValue("@u", idUsuario);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private int GetCountFamiliasPatenteExcepto(int idPatente, int idFamilia)
        {
            const string sql = "SELECT COUNT(*) FROM FamiliaPatente WHERE IdPatente=@p AND IdFamilia<>@f";
            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@p", idPatente);
                cmd.Parameters.AddWithValue("@f", idFamilia);
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
