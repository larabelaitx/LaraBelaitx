using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servicios.Helpers
{
    public class FileHelper
    {
        private string _path;
        private static FileHelper _instance;

        private FileHelper(string filePath)
        {

            this._path = filePath;

        }
        //Singleton
        public static FileHelper GetInstance(string ruta)
        {
            if (_instance == null)
            {
                _instance = new FileHelper(ruta);
            }
            return _instance;
        }

        public string ReadFile()
        {

            if (File.Exists(_path))
            {

                using (StreamReader reader = new StreamReader(_path))
                {

                    string content = reader.ReadToEnd();

                    return content;

                }

            }
            else
            {

                throw new FileNotFoundException("No existe el archivo", _path);

            }

        }


        public void WriteFile(string content)
        {

            using (StreamWriter writer = new StreamWriter(_path, false))
            {

                writer.Write(content);

            }

        }


        public void AppendFile(string content)
        {

            using (StreamWriter writer = new StreamWriter(_path, true))
            {

                writer.Write(content);

            }

        }

        public bool helpFile(string path)
        {
            string fileName = path;
            string rutaPDF = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", fileName);

            try
            {
                if (System.IO.File.Exists(rutaPDF))
                {
                    System.Diagnostics.Process.Start(rutaPDF);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }
    }
}
