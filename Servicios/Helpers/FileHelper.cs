using System;
using System.IO;

namespace Services
{
    /// <summary>
    /// Helper de archivos con Singleton compatible con el código existente.
    /// Mantiene GetInstance(string ruta) para no romper la DAL/UI.
    /// </summary>
    public class FileHelper
    {
        private string _path;
        private static FileHelper _instance;
        private static readonly object _lock = new object();

        private FileHelper(string filePath)
        {
            _path = filePath;
        }

        // === Compatibilidad: conserva el método usado por la DAL ===
        public static FileHelper GetInstance(string ruta)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new FileHelper(ruta);
                }
            }
            else
            {
                // Si cambia la ruta en tiempo de ejecución, la actualizamos.
                _instance._path = ruta;
            }
            return _instance;
        }

        public string ReadFile()
        {
            if (!File.Exists(_path))
                throw new FileNotFoundException("No existe el archivo", _path);

            using (var reader = new StreamReader(_path))
            {
                return reader.ReadToEnd();
            }
        }

        public void WriteFile(string content)
        {
            using (var writer = new StreamWriter(_path, false))
            {
                writer.Write(content);
            }
        }

        public void AppendFile(string content)
        {
            using (var writer = new StreamWriter(_path, true))
            {
                writer.Write(content);
            }
        }

        /// <summary>
        /// Abre un archivo de ayuda ubicado en /Resources/{fileName}
        /// </summary>
        public bool helpFile(string fileName)
        {
            string rutaPDF = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", fileName);

            try
            {
                if (File.Exists(rutaPDF))
                {
                    System.Diagnostics.Process.Start(rutaPDF);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return true;
        }
    }
}
