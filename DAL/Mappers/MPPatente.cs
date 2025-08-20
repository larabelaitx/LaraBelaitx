using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Mappers
{
    internal class MPPatente
    {
        private static MPPatente _instance;
        //Singleton
        public static MPPatente GetInstance()
        {
            if (_instance == null)
            {
                _instance = new MPPatente();
            }

            return _instance;
        }

        public BE.Permiso Map(DataTable dt)
        {
            try
            {
                BE.Patente patente = new BE.Patente()
                {
                    Id = dt.Rows[0].Field<int>("IdPatente"),
                    Name = dt.Rows[0].Field<string>("Nombre"),
                };

                return patente;

            }
            catch (Exception e)
            {

                throw e;
            }
        }
        public HashSet<BE.Permiso> MapPatentes(DataTable dt)
        {
            HashSet<BE.Permiso> patentes = new HashSet<BE.Permiso>();
            try
            {
                foreach (DataRow dr in dt.Rows)
                {

                    patentes.Add(new BE.Patente()
                    {
                        Id = dr.Field<int>("IdPatente"),
                        Name = dr.Field<string>("Nombre"),
                    });
                }

                return patentes;

            }
            catch (Exception e)
            {

                throw e;
            }
        }
    }
}
