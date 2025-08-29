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
            if (_instance == null)
            {
                _instance = new PatenteDao();
            }

            return _instance;
        }

        public bool Add(Permiso alta)
        {
            throw new NotImplementedException();
        }

        public bool Delete(Permiso delete)
        {
            throw new NotImplementedException();
        }

        public HashSet<Permiso> GetAll()
        {
            string SelectAll = "SELECT IdPatente, Nombre FROM Patente";
            return Mappers.MPPatente.GetInstance().MapPatentes(Services.SqlHelpers.GetInstance(_connString).GetDataTable(SelectAll));
        }

        public Permiso GetById(int idPatente)
        {
            string SelectId = "SELECT IdPatente, Nombre FROM Patente WHERE IdPatente = {0}";
            SelectId = string.Format(SelectId, idPatente);
            return Mappers.MPPatente.GetInstance().Map(Services.SqlHelpers.GetInstance(_connString).GetDataTable(SelectId));
        }

        public bool Update(Permiso update)
        {
            throw new NotImplementedException();
        }

        public HashSet<BE.Permiso> GetPatentesUsuario(int idUsuario)
        {
            string SelectJoin = "SELECT P.* from Patente as P INNER JOIN UsuarioPatente as UP on P.IdPatente = UP.IdPatente WHERE UP.IdUsuario = @idUsuario";
            List<SqlParameter> parameters = new List<SqlParameter>()
            {
                new SqlParameter("@idUsuario",idUsuario)
            };
            return Mappers.MPPatente.GetInstance().MapPatentes(Services.SqlHelpers.GetInstance(_connString).GetDataTable(SelectJoin, parameters));
        }
        public HashSet<BE.Permiso> GetPatentesFamilia(int idFamilia)
        {
            string SelectJoin = "SELECT P.* from Patente as P INNER JOIN FamiliaPatente as FP on P.IdPatente = FP.IdPatente WHERE FP.IdFamilia = @IdFam";
            List<SqlParameter> parameters = new List<SqlParameter>()
            {
                new SqlParameter("@IdFam",idFamilia)
            };
            return Mappers.MPPatente.GetInstance().MapPatentes(Services.SqlHelpers.GetInstance(_connString).GetDataTable(SelectJoin, parameters));
        }

        private int GetCountUsuarioPatentes(int idPatente)
        {
            string SelectJoin = "SELECT COUNT(*) FROM UsuarioPatente as UP WHERE UP.IdPatente = @idPatente";
            List<SqlParameter> parameters = new List<SqlParameter>()
            {
                new SqlParameter("@idPatente",idPatente)
            };
            return (int)Services.SqlHelpers.GetInstance(_connString).ExecuteScalar(SelectJoin, parameters);
        }

        private int GetCountFamiliasPatente(int idPatente)
        {
            string querySelect = "SELECT COUNT(*) FROM FamiliaPatente as fp INNER JOIN Patente as p on fp.IdPatente = p.IdPatente INNER JOIN UsuarioFamilia as UF ON UF.IdFamilia = fp.IdFamilia WHERE p.IdPatente = @idPatente";
            List<SqlParameter> sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@idPatente", idPatente)
            };
            return (int)Services.SqlHelpers.GetInstance(_connString).ExecuteScalar(querySelect, sqlParams);
        }

        public bool DeletePatenteFamilia(int idFamilia)
        {
            bool result = false;
            string queryDelUsuarioFamilia = "DELETE FROM FamiliaPatente WHERE IdFamilia = @IdFamilia";
            List<SqlParameter> sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@IdFamilia", idFamilia)
            };
            try
            {
                if (Services.SqlHelpers.GetInstance(_connString).ExecuteQuery(queryDelUsuarioFamilia, sqlParams) > 0)
                {
                    result = true;
                }
            }
            catch (Exception e)
            {

                throw e;
            }

            return result;
        }

        public bool CheckPatenteAsing(int idPatente)
        {
            bool result = false;
            int familiaPatente = GetCountFamiliasPatente(idPatente);
            int usuarioPatente = GetCountUsuarioPatentes(idPatente);

            if (usuarioPatente + familiaPatente > 1)
            {
                result = true;
            }

            return result;
        }

        List<Permiso> ICRUD<Permiso>.GetAll()
        {
            throw new NotImplementedException();
        }
    }
}
