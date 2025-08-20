using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Mappers
{
    internal class MPIdioma
    {
        private static MPIdioma _instance;
        //Singleton
        public static MPIdioma GetInstance()
        {
            if (_instance == null)
            {
                _instance = new MPIdioma();
            }

            return _instance;
        }

        public BE.Idioma Map(DataTable dt)
        {
            try
            {
                BE.Idioma idioma = new BE.Idioma()
                {
                    Id = dt.Rows[0].Field<int>("IdIdioma"),
                    Name = dt.Rows[0].Field<string>("Nombre"),
                };

                return idioma;

            }
            catch (Exception e)
            {

                throw e;
            }
        }
        public List<BE.Idioma> MapIdiomas(DataTable dt)
        {
            List<BE.Idioma> idiomas = new List<BE.Idioma>();
            try
            {
                foreach (DataRow dr in dt.Rows)
                {

                    idiomas.Add(new BE.Idioma()
                    {
                        Id = dr.Field<int>("IdIdioma"),
                        Name = dr.Field<string>("Nombre"),
                    });
                }

                return idiomas;

            }
            catch (Exception e)
            {

                throw e;
            }
        }
    }
}
