using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using DAL.Mappers;
using BE;

namespace DAL
{
    public class PatenteDao : ICRUD<Permiso>
    {
        // ------------------ Singleton ------------------
        private static PatenteDao _instance;
        public static PatenteDao GetInstance() => _instance ?? (_instance = new PatenteDao());
        private PatenteDao() { }

        // ------------------ Lecturas ------------------
        public HashSet<Permiso> GetAll()
        {
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
                if (dt.Rows.Count == 0) return null;
                return MPPatente.GetInstance().Map(dt);
            }
        }

        public HashSet<Permiso> GetPatentesUsuario(int idUsuario)
        {
            const string sql = @"
                SELECT P.IdPatente, P.Nombre
                FROM Patente P 
                INNER JOIN UsuarioPatente UP ON P.IdPatente = UP.IdPatente 
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
                FROM Patente P 
                INNER JOIN FamiliaPatente FP ON P.IdPatente = FP.IdPatente 
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

        // Si no usás “sector”, devolvemos todas.
        public HashSet<Permiso> GetBySector(int sector) => GetAll();

        // ------------------ Conteos SIN transacción ------------------
        private int CountUsuarioPatentes(int idPatente)
        {
            const string sql = "SELECT COUNT(*) FROM UsuarioPatente WHERE IdPatente = @p";
            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@p", idPatente);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private int CountFamiliasPatente(int idPatente)
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
            => CountUsuarioPatentes(idPatente) + CountFamiliasPatente(idPatente);

        public int CountAsignacionesPatenteExcluyendoUsuario(int idPatente, int idUsuario)
        {
            const string sqlU = "SELECT COUNT(*) FROM UsuarioPatente WHERE IdPatente=@p AND IdUsuario<>@u";
            int enUsuariosExceptoEste;
            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sqlU, cn))
            {
                cmd.Parameters.AddWithValue("@p", idPatente);
                cmd.Parameters.AddWithValue("@u", idUsuario);
                enUsuariosExceptoEste = Convert.ToInt32(cmd.ExecuteScalar());
            }
            int enFamilias = CountFamiliasPatente(idPatente);
            return enUsuariosExceptoEste + enFamilias;
        }

        public int CountAsignacionesPatenteExcluyendoFamilia(int idPatente, int idFamilia)
        {
            const string sqlF = "SELECT COUNT(*) FROM FamiliaPatente WHERE IdPatente=@p AND IdFamilia<>@f";
            int enFamiliasExceptoEsta;
            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sqlF, cn))
            {
                cmd.Parameters.AddWithValue("@p", idPatente);
                cmd.Parameters.AddWithValue("@f", idFamilia);
                enFamiliasExceptoEsta = Convert.ToInt32(cmd.ExecuteScalar());
            }
            int enUsuarios = CountUsuarioPatentes(idPatente);
            return enUsuarios + enFamiliasExceptoEsta;
        }

        // ------------------ Conteos CON transacción (únicos) ------------------
        // Nota: nombres únicos para evitar CS0121 / CS0111
        public int CountAsignacionesExcluyendoUsuario_tx(SqlConnection cn, SqlTransaction tx, int idPatente, int idUsuario)
            => CountUsuarioPatentesExcepto_tx(cn, tx, idPatente, idUsuario)
             + CountFamiliasPatente_tx(cn, tx, idPatente);

        public int CountAsignacionesExcluyendoFamilia_tx(SqlConnection cn, SqlTransaction tx, int idPatente, int idFamilia)
            => CountUsuarioPatentes_tx(cn, tx, idPatente)
             + CountFamiliasPatenteExcepto_tx(cn, tx, idPatente, idFamilia);

        // ----------- helpers privados (reusan misma cn/tx) -----------
        private int CountUsuarioPatentes_tx(SqlConnection cn, SqlTransaction tx, int idPatente)
        {
            const string sql = "SELECT COUNT(*) FROM UsuarioPatente WHERE IdPatente = @p";
            using (var cmd = new SqlCommand(sql, cn, tx) { CommandTimeout = 60 })
            {
                cmd.Parameters.AddWithValue("@p", idPatente);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private int CountUsuarioPatentesExcepto_tx(SqlConnection cn, SqlTransaction tx, int idPatente, int idUsuario)
        {
            const string sql = "SELECT COUNT(*) FROM UsuarioPatente WHERE IdPatente=@p AND IdUsuario<>@u";
            using (var cmd = new SqlCommand(sql, cn, tx) { CommandTimeout = 60 })
            {
                cmd.Parameters.AddWithValue("@p", idPatente);
                cmd.Parameters.AddWithValue("@u", idUsuario);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private int CountFamiliasPatente_tx(SqlConnection cn, SqlTransaction tx, int idPatente)
        {
            const string sql = "SELECT COUNT(*) FROM FamiliaPatente WHERE IdPatente = @p";
            using (var cmd = new SqlCommand(sql, cn, tx) { CommandTimeout = 60 })
            {
                cmd.Parameters.AddWithValue("@p", idPatente);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private int CountFamiliasPatenteExcepto_tx(SqlConnection cn, SqlTransaction tx, int idPatente, int idFamilia)
        {
            const string sql = "SELECT COUNT(*) FROM FamiliaPatente WHERE IdPatente=@p AND IdFamilia<>@f";
            using (var cmd = new SqlCommand(sql, cn, tx) { CommandTimeout = 60 })
            {
                cmd.Parameters.AddWithValue("@p", idPatente);
                cmd.Parameters.AddWithValue("@f", idFamilia);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // ------------------ ICRUD<Permiso> ------------------
        bool ICRUD<Permiso>.Add(Permiso alta) => throw new NotImplementedException();
        bool ICRUD<Permiso>.Update(Permiso update) => throw new NotImplementedException();
        bool ICRUD<Permiso>.Delete(Permiso delete) => throw new NotImplementedException();
        List<Permiso> ICRUD<Permiso>.GetAll() => new List<Permiso>(GetAll());
        Permiso ICRUD<Permiso>.GetById(int id) => GetById(id);
    }
}
