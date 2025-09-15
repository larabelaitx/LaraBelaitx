using System.Collections.Generic;
using BE;

namespace BLL.Contracts
{
    public interface IRolService
    {
        List<Familia> GetFamilias();
        HashSet<Permiso> GetPatentes();
        bool CrearFamilia(Familia f);
        bool ActualizarFamilia(Familia f);
        bool EliminarFamilia(Familia f);
        bool AgregarPatenteAFamilia(Familia f, Permiso p);
        bool QuitarPatenteAFamilia(Familia f, Permiso p);
        List<Familia> GetFamiliasUsuario(int idUsuario);
        List<Familia> ListarRoles();
    }
}
