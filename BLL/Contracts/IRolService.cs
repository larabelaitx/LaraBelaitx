using System.Collections.Generic;

public interface IRolService
{
    IEnumerable<BE.Permiso> ListarRoles();
    BE.Familia GetFamilia(int id);
    int CrearFamilia(BE.Familia f);
    bool ActualizarFamilia(BE.Familia f);

    // Patentes (permisos simples)
    List<BE.Patente> GetPatentes();
    List<BE.Patente> GetPatentesDeFamilia(int idFamilia);
    bool SetPatentesDeFamilia(int idFamilia, IEnumerable<int> idsPatentes);

    // Asignación de familias a usuarios
    List<BE.Familia> GetFamiliasUsuario(int idUsuario);
    bool SetFamiliasDeUsuario(int idUsuario, IEnumerable<int> familiasIds);

    // ➕ NUEVO: patentes asignadas directamente a un usuario
    List<BE.Permiso> GetPatentesDeUsuario(int idUsuario);
    bool SetPatentesDeUsuario(int idUsuario, IEnumerable<int> idsPatentes);
}