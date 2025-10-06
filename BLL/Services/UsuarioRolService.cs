using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;
using DAL;

namespace BLL.Services
{
    public class UsuarioRolService
    {
        private readonly UsuarioRolDao _dao;

        public UsuarioRolService()
        {
            _dao = new UsuarioRolDao();
        }

        public List<Familia> ObtenerFamiliasDisponibles(int idUsuario)
        {
            return _dao.ObtenerFamiliasDisponibles(idUsuario);
        }

        public List<Familia> ObtenerFamiliasAsignadas(int idUsuario)
        {
            return _dao.ObtenerFamiliasAsignadas(idUsuario);
        }

        public void AsignarFamilias(int idUsuario, List<int> familiasIds)
        {
            _dao.AsignarFamilias(idUsuario, familiasIds);
        }
    }

}
