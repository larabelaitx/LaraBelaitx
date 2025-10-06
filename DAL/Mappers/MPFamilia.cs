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
                var idFam = dt.Rows[0].Field<int>("IdFamilia");
                return new BE.Familia
                {
                    Id = idFam,
                    Name = dt.Rows[0].Field<string>("Nombre"),
                    Permisos = DAL.PatenteDao.GetInstance()
                                .GetPatentesFamilia(idFam)
                                .ToList<BE.Permiso>()
                };
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
                    var idFam = dr.Field<int>("IdFamilia");
                    familias.Add(new BE.Familia
                    {
                        Id = idFam,
                        Name = dr.Field<string>("Nombre"),
                        Permisos = DAL.PatenteDao.GetInstance()
                                     .GetPatentesFamilia(idFam)
                                     .ToList<BE.Permiso>()
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
