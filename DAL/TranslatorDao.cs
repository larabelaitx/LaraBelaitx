using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DAL
{
    public class TranslatorDao
    {
        // Singleton básico
        private static TranslatorDao _instance;
        public static TranslatorDao GetInstance() => _instance ?? (_instance = new TranslatorDao());
        private TranslatorDao() { }

        /// <summary>
        /// Carga todas las traducciones para un código de idioma dado (p.ej. "es", "en").
        /// </summary>
        public Dictionary<string, string> LoadTranslations(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
                languageCode = "es";

            const string sql = @"SELECT KeyId, TextDescription
                                 FROM MultiIdioma
                                 WHERE LanguageCode = @Lang";

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@Lang", languageCode);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var key = reader.IsDBNull(0) ? null : reader.GetString(0);
                        var val = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        if (!string.IsNullOrEmpty(key))
                            map[key] = val ?? "";
                    }
                }
            }

            return map;
        }

        /// <summary>
        /// Traduce una clave puntual. Si no existe, devuelve la clave tal cual.
        /// </summary>
        public string Translate(string languageCode, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return key;

            var dict = LoadTranslations(languageCode);
            return dict.TryGetValue(key, out var value) ? value : key;
        }
    }
}
