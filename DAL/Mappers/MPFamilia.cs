using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Mappers
{
    internal class MPFamilia
    {
        private static MPFamilia _instance;
        //Singleton
        public static MPFamilia GetInstance()
        {
            if (_instance == null)
            {
                _instance = new MPFamilia();
            }

            return _instance;
        }

        public BE.Familia Map(DataTable dt)
        {
            try
            {
                BE.Familia familia = new BE.Familia()
                {
                    Id = dt.Rows[0].Field<int>("IdFamilia"),
                    Name = dt.Rows[0].Field<string>("Nombre"),
                    Descripcion = dt.Rows[0].Field<string>("Descripcion"),
                    Patentes = PatenteDao.GetInstance().GetPatentesFamilia(dt.Rows[0].Field<int>("IdFamilia"))
                };

                return familia;

            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public List<BE.Familia> MapFamilias(DataTable dt)
        {
            List<BE.Familia> familias = new List<BE.Familia>();
            try
            {
                foreach (DataRow dr in dt.Rows)
                {
                    familias.Add(new BE.Familia()
                    {
                        Id = dr.Field<int>("IdFamilia"),
                        Name = dr.Field<string>("Nombre"),
                        Descripcion = dr.Field<string>("Descripcion"),
                        Patentes = PatenteDao.GetInstance().GetPatentesFamilia(dr.Field<int>("IdFamilia"))
                    });
                }
                return familias;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
