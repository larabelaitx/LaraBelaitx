using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class TranslatorDao
    {
        private static string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigFile.txt");
        private static string _connString = Services.Security.Crypto.Decript(Services.Helpers.FileHelper.GetInstance(configFilePath).ReadFile());
        private static Dictionary<string, string> _translations = new Dictionary<string, string>();

        #region Singleton
        private static TranslatorDao _instance;
        public static TranslatorDao GetInstance()
        {
            if (_instance == null)
            {
                _instance = new TranslatorDao();
            }

            return _instance;
        }
        #endregion

        public Dictionary<string, string> LoadTranslations(string LanguageCode)
        {
            using (SqlConnection connection = new SqlConnection(_connString))
            {
                string query = "SELECT KeyId, TextDescription FROM MultiIdioma WHERE LanguageCode = @LanguageCode";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@LanguageCode", LanguageCode);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string key = reader.GetString(0);
                        string value = reader.GetString(1);
                        _translations[key] = value;
                    }
                }
            }
            return _translations;
        }
    }
}
