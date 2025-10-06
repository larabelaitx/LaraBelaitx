using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace DAL
{
    public static class ConnectionFactory
    {
        private static string _cnn;
        private static readonly string _configFilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigFile.txt");
        static ConnectionFactory() => Init();

        public static void Init()
        {
 
            if (File.Exists(_configFilePath))
            {
                try
                {
                    var enc = File.ReadAllText(_configFilePath);
                    if (!string.IsNullOrWhiteSpace(enc))
                    {
                        var plain = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(enc)).Trim();
                        if (!string.IsNullOrWhiteSpace(plain)) { _cnn = plain; return; }
                    }
                }
                catch { /* sigue */ }
            }

            _cnn = ConfigurationManager.ConnectionStrings["ITX_DB"]?.ConnectionString;
            if (!string.IsNullOrWhiteSpace(_cnn)) return;

            _cnn = @"Data Source=LARAB;Initial Catalog=ITX;Integrated Security=True;Encrypt=False;";
        }

        public static SqlConnection Open()
        {
            if (string.IsNullOrWhiteSpace(_cnn))
                throw new InvalidOperationException("ConnectionFactory no inicializado. Llamá a Init().");
            var cn = new SqlConnection(_cnn);
            cn.Open();
            return cn;
        }

        public static string Current => _cnn;
    }
}
