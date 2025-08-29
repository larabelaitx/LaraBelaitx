using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Mappers;
using Services;
using BE;

namespace DAL
{
    public class EstadoDao : ICRUD<BE.Estado>
    {

        private static string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigFile.txt");
        private static string _connString = Crypto.Decript(FileHelper.GetInstance(configFilePath).ReadFile());

        #region Singleton
        private static EstadoDao _instance;
        public static EstadoDao GetInstance()
        {
            if (_instance == null)
            {
                _instance = new EstadoDao();
            }

            return _instance;
        }

        #endregion
        public bool Add(Estado alta)
        {
            throw new NotImplementedException();
        }

        public bool Delete(Estado delete)
        {
            throw new NotImplementedException();
        }

        public List<Estado> GetAll()
        {
            string SelectAll = "SELECT IdEstado, Descripcion FROM Estado";
            return Mappers.MPEstado.GetInstance().MapEstados(Services.SqlHelpers.GetInstance(_connString).GetDataTable(SelectAll));
        }

        public Estado GetById(int idEstado)
        {
            string SelectId = "SELECT IdEstado, Descripcion FROM Estado WHERE IdEstado = {0}";
            SelectId = string.Format(SelectId, idEstado);
            return Mappers.MPEstado.GetInstance().Map(Services.SqlHelpers.GetInstance(_connString).GetDataTable(SelectId));
        }

        public bool Update(Estado update)
        {
            throw new NotImplementedException();
        }

        public BE.Estado GetEstadoUsuario(int idUsuario)
        {
            string SelectJoin = "SELECT E.IdEstado, E.Descripcion from Estado as E INNER JOIN Usuario as U on E.IdEstado = U.IdEstado WHERE U.IdUsuario = {0}";
            SelectJoin = string.Format(SelectJoin, idUsuario);
            return Mappers.MPEstado.GetInstance().Map(Services.SqlHelpers.GetInstance(_connString).GetDataTable(SelectJoin));
        }
    }
}
