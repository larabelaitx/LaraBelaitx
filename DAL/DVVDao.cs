using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using BE;
using Services;

namespace DAL
{
    public class DVVDao
    {
        private static string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigFile.txt");
        private static string _connString = Crypto.Decript(FileHelper.GetInstance(configFilePath).ReadFile());

        #region Singleton
        private static DVVDao _instance;

        public static DVVDao GetInstance()
        {
            if (_instance == null)
            {
                _instance = new DVVDao();
            }

            return _instance;
        }
        #endregion
        public bool AddUpdateDVV(DVV dvv)
        {
            bool returnValue = false;

            string queryCheckExistence = "SELECT COUNT(*) FROM DVV WHERE Tabla = @Tabla";
            string queryInsert = "INSERT INTO DVV (Tabla, DVV) VALUES (@Tabla, @DVV)";
            string queryUpdate = "UPDATE DVV SET DVV = @DVV WHERE Tabla = @Tabla";

            try
            {
                if (dvv != null)
                {
                    List<SqlParameter> sqlParams = new List<SqlParameter>
                    {
                        new SqlParameter("@Tabla", dvv.tabla),
                        new SqlParameter("@DVV", dvv.dvv)
                    };

                    int count = (int)Services.SqlHelpers.GetInstance(_connString).ExecuteScalar(queryCheckExistence, new List<SqlParameter> { new SqlParameter("@Tabla", dvv.tabla) });

                    if (count > 0)
                    {
                        if (Services.SqlHelpers.GetInstance(_connString).ExecuteQuery(queryUpdate, sqlParams) > 0)
                        {
                            returnValue = true;
                        }
                    }
                    else
                    {
                        if (Services.SqlHelpers.GetInstance(_connString).ExecuteQuery(queryInsert, sqlParams) > 0)
                        {
                            returnValue = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return returnValue;
        }
        public List<BE.DVV> GetAllDVV()
        {
            string selectDVV = "SELECT idDVV, Tabla, DVV FROM DVV";
            List<DVV> dvvs = Mappers.MPDVV.GetInstance().MapDVVs(Services.SqlHelpers.GetInstance(_connString).GetDataTable(selectDVV));
            return dvvs;
        }

        public string CalculateDVV(string table)
        {
            List<DVH> dvhs = DVHDao.GetInstance().GetDvhs(table);
            StringBuilder sb = new StringBuilder();
            foreach (DVH dvh in dvhs)
            {
                sb.Append(dvh.dvh);
            }
            return DV.GetDV(sb.ToString());
        }
    }
}
