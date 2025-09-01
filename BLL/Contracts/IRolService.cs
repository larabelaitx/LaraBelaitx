using System.Collections.Generic;
using BE;

namespace BLL
{
    public interface IRolService
    {
        IEnumerable<Permiso> ListarRoles(); // Familias
        Permiso ObtenerPorId(int idRol);
    }
}
