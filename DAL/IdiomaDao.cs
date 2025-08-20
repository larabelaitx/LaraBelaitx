using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Mappers;

namespace DAL
{
    public class IdiomaDao : ICRUD<BE.Idioma>
    {
        private static string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigFile.txt");
        private static string _connString = Services.Security.Crypto.Decript(Services.Helpers.FileHelper.GetInstance(configFilePath).ReadFile());

        private static IdiomaDao _instance;
        //Singleton
        public static IdiomaDao GetInstance()
        {
            if (_instance == null)
            {
                _instance = new IdiomaDao();
            }
            return _instance;
        }

        public bool Add(Idioma alta)
        {
            throw new NotImplementedException();
        }

        public bool Delete(Idioma delete)
        {
            throw new NotImplementedException();
        }

        public List<Idioma> GetAll()
        {
            string SelectAll = "SELECT IdIdioma, Nombre FROM Idioma";
            return Mappers.MPIdioma.GetInstance().MapIdiomas(Services.SqlHelpers.GetInstance(_connString).GetDataTable(SelectAll));
        }

        public Idioma GetById(int idIdioma)
        {
            string SelectId = "SELECT IdIdioma, Nombre FROM Idioma WHERE IdIdioma = {0}";
            SelectId = string.Format(SelectId, idIdioma);
            return Mappers.MPIdioma.GetInstance().Map(Services.SqlHelpers.GetInstance(_connString).GetDataTable(SelectId));
        }

        public bool Update(Idioma update)
        {
            throw new NotImplementedException();
        }

        public BE.Idioma GetIdiomaUsuario(int idUsuario)
        {
            string SelectJoin = "SELECT I.* from Idioma as I INNER JOIN Usuario as U on I.IdIdioma = U.IdIdioma WHERE U.IdUsuario = {0}";
            SelectJoin = string.Format(SelectJoin, idUsuario);
            return Mappers.MPIdioma.GetInstance().Map(Services.SqlHelpers.GetInstance(_connString).GetDataTable(SelectJoin));
        }
    }
}
