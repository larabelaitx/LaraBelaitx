using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Mappers
{
    internal class MPEstado
    {
        private static MPEstado _instance;
        //Singleton
        public static MPEstado GetInstance()
        {
            if (_instance == null)
            {
                _instance = new MPEstado();
            }

            return _instance;
        }

        public BE.Estado Map(DataTable dt)
        {
            try
            {
                BE.Estado estado = new BE.Estado()
                {
                    Id = dt.Rows[0].Field<int>("IdEstado"),
                    Name = dt.Rows[0].Field<string>("Descripcion"),
                };

                return estado;

            }
            catch (Exception e)
            {

                throw e;
            }
        }
        public List<BE.Estado> MapEstados(DataTable dt)
        {
            List<BE.Estado> estados = new List<BE.Estado>();
            try
            {
                foreach (DataRow dr in dt.Rows)
                {

                    estados.Add(new BE.Estado()
                    {
                        Id = dr.Field<int>("IdEstado"),
                        Name = dr.Field<string>("Descripcion"),
                    });
                }

                return estados;

            }
            catch (Exception e)
            {

                throw e;
            }
        }
    }
}
