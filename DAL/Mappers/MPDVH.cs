using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Mappers
{
    internal class MPDVH
    {
        #region SINGLETON
        private static MPDVH _instance;
        public static MPDVH GetInstance()
        {
            if (_instance == null)
            {
                _instance = new MPDVH();
            }

            return _instance;
        }
        #endregion

        public List<BE.DVH> MapDVHs(DataTable dt, string table)
        {
            List<BE.DVH> dvh = new List<BE.DVH>();
            try
            {
                foreach (DataRow dr in dt.Rows)
                {
                    dvh.Add(new BE.DVH()
                    {
                        tabla = table,
                        dvh = dr.Field<string>("DVH")

                    });
                }
                return dvh;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
