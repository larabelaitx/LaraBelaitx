using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Mappers
{
    internal class MPDVV
    {
        private static MPDVV _instance;
        //Singleton
        public static MPDVV GetInstance()
        {
            if (_instance == null)
            {
                _instance = new MPDVV();
            }

            return _instance;
        }

        public BE.DVV Map(DataTable dt)
        {
            try
            {
                BE.DVV dvv = new BE.DVV()
                {
                    idDVV = dt.Rows[0].Field<int>("IdDVV"),
                    tabla = dt.Rows[0].Field<string>("Tabla"),
                    dvv = dt.Rows[0].Field<string>("DVV")
                };

                return dvv;

            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public List<BE.DVV> MapDVVs(DataTable dt)
        {
            List<BE.DVV> dvvs = new List<BE.DVV>();
            try
            {
                foreach (DataRow dr in dt.Rows)
                {
                    dvvs.Add(new BE.DVV()
                    {
                        idDVV = dr.Field<int>("IdDVV"),
                        tabla = dr.Field<string>("Tabla"),
                        dvv = dr.Field<string>("DVV")
                    });
                }
                return dvvs;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
