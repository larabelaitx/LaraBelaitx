using System.Collections.Generic;

public interface IRolService
{
    // ===== Roles (Familias)
    IEnumerable<BE.Familia> ListarRoles();
    BE.Familia GetFamilia(int id);
    int CrearFamilia(BE.Familia f);
    bool ActualizarFamilia(BE.Familia f);
    bool EliminarFamilia(int idFamilia);

    // ===== Patentes
    List<BE.Patente> GetPatentes();
    List<BE.Patente> GetPatentesDeFamilia(int idFamilia);
    bool SetPatentesDeFamilia(int idFamilia, IEnumerable<int> idsPatentes);

    // ===== Asignación de familias a usuarios
    List<BE.Familia> GetFamiliasUsuario(int idUsuario);
    bool SetFamiliasDeUsuario(int idUsuario, IEnumerable<int> familiasIds);

    // ===== Patentes por usuario
    List<BE.Patente> GetPatentesDirectasDeUsuario(int idUsuario);   // solo directas
    List<BE.Patente> GetPatentesEfectivasDeUsuario(int idUsuario);  // directas ∪ heredadas
    bool SetPatentesDeUsuario(int idUsuario, IEnumerable<int> idsPatentes);
    List<BE.Patente> GetPatentesDeUsuario(int idUsuario);

}
