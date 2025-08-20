using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class DVHDao
    {
        private static string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigFile.txt");
        private static string _connString = Services.Security.Crypto.Decript(Services.Helpers.FileHelper.GetInstance(configFilePath).ReadFile());
        #region SINGLETON

        private static DVHDao _instance;
        public static DVHDao GetInstance()
        {
            if (_instance == null)
            {
                _instance = new DVHDao();
            }

            return _instance;
        }
        #endregion

        public List<BE.DVH> GetDvhs(string table)
        {
            string querySelect = string.Format("SELECT DVH FROM {0}", table);
            return Mappers.MPDVH.GetInstance().MapDVHs(Services.SqlHelpers.GetInstance(_connString).GetDataTable(querySelect), table);
        }
    }
}
