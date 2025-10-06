using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Mappers;
using Services;
using BE;

namespace DAL
{
    public class PatenteDao : ICRUD<Permiso>
    {
        private static string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigFile.txt");
        private static string _connString = Crypto.Decript(FileHelper.GetInstance(configFilePath).ReadFile());

        private static PatenteDao _instance;
        
        //Singleton
        public static PatenteDao GetInstance()
        {
            if (_instance == null) _instance = new PatenteDao();
            return _instance;
        }

        public bool Add(Permiso alta) { throw new NotImplementedException(); }
        public bool Delete(Permiso delete) { throw new NotImplementedException(); }
        public bool Update(Permiso update) { throw new NotImplementedException(); }

        public HashSet<Permiso> GetAll()
        {
            const string SelectAll = "SELECT IdPatente, Nombre FROM Patente";
            return MPPatente.GetInstance().MapPatentes(SqlHelpers.GetInstance(_connString).GetDataTable(SelectAll));
        }
        public Permiso GetById(int idPatente)
        {
            const string sql = "SELECT IdPatente, Nombre FROM Patente WHERE IdPatente = @id";
            var ps = new List<SqlParameter> { new SqlParameter("@id", idPatente) };
            return MPPatente.GetInstance().Map(SqlHelpers.GetInstance(_connString).GetDataTable(sql, ps));
        }

        public HashSet<BE.Permiso> GetPatentesUsuario(int idUsuario)
        {
            const string SelectJoin = @"SELECT P.* 
                                        FROM Patente AS P 
                                        INNER JOIN UsuarioPatente AS UP ON P.IdPatente = UP.IdPatente 
                                        WHERE UP.IdUsuario = @idUsuario";
            var parameters = new List<SqlParameter> { new SqlParameter("@idUsuario", idUsuario) };
            return MPPatente.GetInstance().MapPatentes(SqlHelpers.GetInstance(_connString).GetDataTable(SelectJoin, parameters));
        }

        public HashSet<BE.Permiso> GetPatentesFamilia(int idFamilia)
        {
            const string SelectJoin = @"SELECT P.* 
                                        FROM Patente AS P 
                                        INNER JOIN FamiliaPatente AS FP ON P.IdPatente = FP.IdPatente 
                                        WHERE FP.IdFamilia = @IdFam";
            var parameters = new List<SqlParameter> { new SqlParameter("@IdFam", idFamilia) };
            return MPPatente.GetInstance().MapPatentes(SqlHelpers.GetInstance(_connString).GetDataTable(SelectJoin, parameters));
        }

        private int GetCountUsuarioPatentes(int idPatente)
        {
            const string sql = "SELECT COUNT(*) FROM UsuarioPatente WHERE IdPatente = @idPatente";
            var parameters = new List<SqlParameter> { new SqlParameter("@idPatente", idPatente) };
            return (int)SqlHelpers.GetInstance(_connString).ExecuteScalar(sql, parameters);
        }
        private int GetCountFamiliasPatente(int idPatente)
        {
            const string sql = "SELECT COUNT(*) FROM FamiliaPatente WHERE IdPatente = @idPatente";
            var ps = new List<SqlParameter> { new SqlParameter("@idPatente", idPatente) };
            return (int)SqlHelpers.GetInstance(_connString).ExecuteScalar(sql, ps);
        }

        public bool DeletePatenteFamilia(int idFamilia)
        {
            bool result = false;
            const string sql = "DELETE FROM FamiliaPatente WHERE IdFamilia = @IdFamilia";
            var ps = new List<SqlParameter> { new SqlParameter("@IdFamilia", idFamilia) };
            try
            {
                if (SqlHelpers.GetInstance(_connString).ExecuteQuery(sql, ps) > 0) result = true;
            }
            catch (Exception e) { throw e; }
            return result;
        }
        public bool CheckPatenteAsing(int idPatente)
        {
            int familias = GetCountFamiliasPatente(idPatente);
            int usuarios = GetCountUsuarioPatentes(idPatente);
            return (familias + usuarios) > 1;
        }

        List<Permiso> ICRUD<Permiso>.GetAll() { throw new NotImplementedException(); }
        public HashSet<BE.Permiso> GetBySector(int sector)
        {
            // Si Patente tiene columna Sector en DB, descomenta la condición
            // const string sql = "SELECT IdPatente, Nombre, Sector FROM Patente WHERE Sector=@s";
            const string sql = "SELECT IdPatente, Nombre FROM Patente"; // fallback sin filtro si no tenés la columna
            var ps = new List<SqlParameter> { new SqlParameter("@s", sector) };
            var dt = Services.SqlHelpers.GetInstance(_connString).GetDataTable(sql, ps);
            return Mappers.MPPatente.GetInstance().MapPatentes(dt);
        }
    }
}
