using System.Collections.Generic;
using BE;

namespace BLL.Contracts
{
    public interface IRolService
    {
        List<Familia> GetFamilias();
        HashSet<Permiso> GetPatentes();
        List<Familia> GetFamiliasUsuario(int idUsuario);
        List<Familia> ListarRoles();
        bool CrearFamilia(Familia f);
        bool ActualizarFamilia(Familia f);
        bool EliminarFamilia(Familia f);
        bool AgregarPatenteAFamilia(Familia f, Permiso p);
        bool QuitarPatenteAFamilia(Familia f, Permiso p);
        bool SetPatentesFamilia(int idFamilia, IEnumerable<int> idsPatentesSeleccionadas);
        HashSet<Permiso> GetPatentesPorSector(int sector);
        bool AsignarFamiliaAUsuario(int idUsuario, int idFamilia);
        bool SetFamiliasDeUsuario(int idUsuario, IEnumerable<int> familiasIds);
        Familia GetFamilia(int idFamilia);
        List<Patente> GetPatentesDeFamilia(int idFamilia);
        void SetPatentesDeFamilia(int idFamilia, IEnumerable<int> patentesIds);

    }
}
