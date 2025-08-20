using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class Bitacora
    {
        #region SINGLETON
        private static Bitacora _instance;

        public static Bitacora GetInstance()
        {
            if (_instance == null)
            {
                _instance = new Bitacora();
            }

            return _instance;

        }
        #endregion

        public List<BE.Bitacora> GetAllBitacora()
        {
            List<BE.Bitacora> bitacora = DAL.BitacoraDao.GetInstance().GetAllBitacora();
            foreach (BE.Bitacora registro in bitacora)
            {
                registro.Descripcion = Services.Security.Crypto.Decript(registro.Descripcion);
            }
            return bitacora;
        }
        public bool AddBitacora(BE.Bitacora registro)
        {
            try
            {
                registro.Descripcion = Services.Security.Crypto.Encript(registro.Descripcion);
                BE.DVH dvh = new BE.DVH() { dvh = CalculateDVHBitacora(registro) };
                return DAL.BitacoraDao.GetInstance().Add(registro, dvh);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        #region DVH
        public string CalculateDVHBitacora(BE.Bitacora bitacora)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(bitacora.Criticidad);
                sb.Append(bitacora.Usuario.Id);
                sb.Append(bitacora.Descripcion);
                sb.Append(bitacora.Fecha);

                return Services.Security.DV.GetDV(sb.ToString());
            }
            catch (NullReferenceException)
            {
                throw new Exception(message: "Usuario Inexistente");
            }

        }
    }
    #endregion
}
