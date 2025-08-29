using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services;

namespace DAL
{
    public class BitacoraDao
    {
        private static string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigFile.txt");
        private static string _connString = Crypto.Decript(FileHelper.GetInstance(configFilePath).ReadFile());

        #region Singleton
        private static BitacoraDao _instance;
        public static BitacoraDao GetInstance()
        {
            if (_instance == null)
            {
                _instance = new BitacoraDao();
            }

            return _instance;
        }
        #endregion

        public List<BE.Bitacora> GetAllBitacora()
        {
            string selectAll = "SELECT TOP (1000) IdEvento,IdCriticidad,IdUsuario,Descripcion,Fecha,DVH FROM Bitacora";
            return Mappers.MPBitacora.GetInstance().Map(Services.SqlHelpers.GetInstance(_connString).GetDataTable(selectAll));
        }
        public bool Add(BE.Bitacora bitacora, BE.DVH dvh)
        {
            bool result = false;
            string queryInsert = "INSERT INTO Bitacora (IdCriticidad,IdUsuario,Descripcion,Fecha,DVH) VALUES (@IdCriticidad,@IdUsuario,@Descripcion,@Fecha,@DVH)";
            try
            {
                if (bitacora != null)
                {
                    List<SqlParameter> parameters = new List<SqlParameter>()
                    {
                        new SqlParameter("@IdCriticidad", bitacora.Criticidad.Id),
                        new SqlParameter("@IdUsuario", bitacora.Usuario.Id),
                        new SqlParameter("@Descripcion", bitacora.Descripcion),
                        new SqlParameter("@Fecha", bitacora.Fecha),
                        new SqlParameter("@DVH", dvh.dvh)
                    };
                    if (Services.SqlHelpers.GetInstance(_connString).ExecuteQuery(queryInsert, parameters) > 0)
                    {
                        result = true;
                        BE.DVV dvv = new BE.DVV()
                        {
                            tabla = "Bitacora",
                            dvv = DVVDao.GetInstance().CalculateDVV("Bitacora")
                        };
                        DVVDao.GetInstance().AddUpdateDVV(dvv);
                    }
                }
            }
            catch (Exception e)
            {

                throw e;
            }

            return result;
        }
    }
}
