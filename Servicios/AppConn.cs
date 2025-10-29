using System;
using System.Configuration; // <-- para ConfigurationManager
using System.Data.SqlClient;
using System.IO;

namespace Services
{
    public static class AppConn
    {
        private static readonly string _path =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "db.conn.sec");

        private static string _cached;

        /// <summary>
        /// Devuelve la connection string:
        /// 1) cache si ya se leyó
        /// 2) archivo cifrado Resources\db.conn.sec (si existe)
        /// 3) <connectionStrings name="ITX_DB"> de App.config (fallback)
        /// 4) null si no hay nada
        /// </summary>
        public static string Get()
        {
            if (!string.IsNullOrWhiteSpace(_cached)) return _cached;

            // 1) archivo cifrado (preferencia)
            if (File.Exists(_path))
            {
                var enc = File.ReadAllText(_path);
                if (!string.IsNullOrWhiteSpace(enc))
                {
                    var plain = Crypto.Decript(enc); // tu AES
                    if (!string.IsNullOrWhiteSpace(plain))
                        return _cached = plain;
                }
            }

            // 2) fallback a App.config
            var cs = ConfigurationManager.ConnectionStrings["ITX_DB"]?.ConnectionString;
            if (!string.IsNullOrWhiteSpace(cs))
                return _cached = cs;

            return null; // no hay cadena disponible
        }

        /// <summary>Guarda (cifrada) la connection string en Resources\db.conn.sec.</summary>
        public static void Save(string plainConnString)
        {
            var dir = Path.GetDirectoryName(_path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var enc = Crypto.Encript(plainConnString);
            File.WriteAllText(_path, enc);
            _cached = plainConnString;
        }

        /// <summary>Intenta abrir la conexión; true=OK, false=error (devuelve mensaje).</summary>
        public static bool TryTest(out string error)
        {
            error = null;
            try
            {
                var cs = Get();
                if (string.IsNullOrWhiteSpace(cs))
                {
                    error = "No se encontró cadena de conexión (ni archivo cifrado ni App.config).";
                    return false;
                }

                using (var cn = new SqlConnection(cs))
                {
                    cn.Open();
                    using (var cmd = new SqlCommand("SELECT 1", cn))
                    {
                        cmd.ExecuteScalar();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
