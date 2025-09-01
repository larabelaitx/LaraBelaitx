using System.Collections.Generic;
using System.Linq;
using BE;
using DAL;

namespace BLL
{
    public class RolService : IRolService
    {
        private readonly FamiliaDao _familiaDao;

        public RolService(FamiliaDao familiaDao)
        {
            _familiaDao = familiaDao;
        }

        public static RolService CreateWithSingletons()
            => new RolService(FamiliaDao.GetInstance());
        public IEnumerable<Permiso> ListarRoles()
        {
            return _familiaDao.GetAll().Cast<Permiso>();
        }

        public Permiso ObtenerPorId(int idRol)
        {
            return _familiaDao.GetById(idRol);
        }
    }
}
