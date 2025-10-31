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
            if (dt == null || dt.Rows.Count == 0) return null; // <- NEW

            try
            {
                return new BE.Patente
                {
                    Id = dt.Rows[0].Field<int>("IdPatente"),
                    Name = dt.Rows[0].Field<string>("Nombre"),
                };
            }
            catch
            {
                throw; // <- en lugar de "throw e;" para no perder el stack original
            }
        }
        public HashSet<BE.Permiso> MapPatentes(DataTable dt)
        {
            var patentes = new HashSet<BE.Permiso>();
            if (dt == null || dt.Rows.Count == 0) return patentes; // <- NEW

            try
            {
                foreach (DataRow dr in dt.Rows)
                {
                    patentes.Add(new BE.Patente
                    {
                        Id = dr.Field<int>("IdPatente"),
                        Name = dr.Field<string>("Nombre"),
                    });
                }
                return patentes;
            }
            catch
            {
                throw; // <- idem
            }
        }

    }
}
