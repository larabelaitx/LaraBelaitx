using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;

namespace BLL
{
    /// <summary>
    /// Fachada simple para traducciones. Lee desde DAL.TranslatorDao (tabla MultiIdioma).
    /// Si no encuentra la clave, devuelve la clave tal cual como fallback.
    /// </summary>
    public class Translator
    {
        private readonly Dictionary<string, string> _map;

        public Translator(string languageCode)
        {
            try
            {
                _map = TranslatorDao.GetInstance().LoadTranslations(languageCode)
                        ?? new Dictionary<string, string>();
            }
            catch
            {
                _map = new Dictionary<string, string>();
            }
        }

        public string Translate(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return string.Empty;
            return _map.TryGetValue(key, out var text) ? text : key;
        }
    }
}