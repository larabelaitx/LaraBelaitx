using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Mappers
{
    public class MPBitacora
    {
        #region Singleton
        private static MPBitacora _instance;
        public static MPBitacora GetInstance()
        {
            if (_instance == null)
            {
                _instance = new MPBitacora();
            }

            return _instance;
        }
        #endregion

        public List<BE.Bitacora> Map(DataTable dt)
        {
            try
            {
                List<BE.Bitacora> bitacora = new List<BE.Bitacora>();
                foreach (DataRow dr in dt.Rows)
                {
                    BE.Bitacora registro = (new BE.Bitacora()
                    {
                        Id = dr.Field<int>("IdEvento"),
                        Descripcion = dr.Field<string>("Descripcion"),
                        Criticidad = new BE.Criticidad(dr.Field<int>("IdCriticidad")),
                        Usuario = DAL.UsuarioDao.GetInstance().GetById(dr.Field<int>("IdUsuario")),
                        Fecha = dr.Field<DateTime>("Fecha")
                    });
                    bitacora.Add(registro);
                }
                return bitacora;
            }
            catch (Exception e)
            {

                throw e;
            }
        }
    }
}
