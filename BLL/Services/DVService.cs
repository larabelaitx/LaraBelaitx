using BLL.Contracts;
using DAL;

namespace BLL.Services
{
    public class DVService : IDVServices
    {
        private readonly DVVDao _dvv = DVVDao.GetInstance();
        public string RecalcularDVV(string tabla)
            => _dvv.CalculateDVV(tabla);
        public string ObtenerDVV(string tabla)
        {
            var todos = _dvv.GetAllDVV();
            var row = todos.Find(d => d.tabla == tabla);
            return row?.dvv;
        }
        public bool VerificarTabla(string tabla, out string dvvCalculado, out string dvvGuardado)
        {
            dvvCalculado = _dvv.CalculateDVV(tabla);
            dvvGuardado = ObtenerDVV(tabla);
            return !string.IsNullOrEmpty(dvvGuardado) && dvvCalculado == dvvGuardado;
        }
    }
}
