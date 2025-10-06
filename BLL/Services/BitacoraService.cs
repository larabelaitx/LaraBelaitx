using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;
using DAL;

namespace BLL.Services
{
    public class BitacoraService
    {
        private readonly BitacoraDao _dao;
        public BitacoraService()
        {
            _dao = BitacoraDao.GetInstance();
        }
        public BitacoraService(string cnn)
        {
            _dao = BitacoraDao.GetInstance();
        }

        public (IList<BitacoraEntry> rows, int total) Buscar(
            DateTime? desde, DateTime? hasta, string usuario,
            int page, int pageSize, string ordenarPor)
        {
            return _dao.Search(desde, hasta, null, null, null, usuario, null, page, pageSize, ordenarPor);
        }
        public IList<string> Usuarios() => _dao.GetUsuarios();
    }
}
