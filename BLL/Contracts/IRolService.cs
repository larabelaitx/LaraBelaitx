// BLL.Contracts/IRolService.cs
using System.Collections.Generic;

public interface IRolService
{
    // ===== Roles (Familias)
    IEnumerable<BE.Familia> ListarRoles();
    BE.Familia GetFamilia(int id);
    int CrearFamilia(BE.Familia f);
    bool ActualizarFamilia(BE.Familia f);
    bool EliminarFamilia(int idFamilia);

    // ===== Patentes globales
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

    // ===== Chequeos de permisos
    bool TienePatente(int idUsuario, string codigoPatente);
    void ThrowIfNotAllowed(int idUsuario, string codigoPatente);
    bool TieneAlguna(int idUsuario, params string[] codigos);
    bool TieneTodas(int idUsuario, params string[] codigos);

    // ===== Wrappers SEGUROS (requieren id del solicitante)
    bool SetPatentesDeUsuarioSecure(int requesterUserId, int targetUserId, IEnumerable<int> idsPatentes);
    bool SetPatentesDeFamiliaSecure(int requesterUserId, int idFamilia, IEnumerable<int> idsPatentes);
    bool SetFamiliasDeUsuarioSecure(int requesterUserId, int targetUserId, IEnumerable<int> familiasIds);
    bool EliminarFamiliaSecure(int requesterUserId, int idFamilia);
    int CrearFamiliaSecure(int requesterUserId, BE.Familia f);
    bool ActualizarFamiliaSecure(int requesterUserId, BE.Familia f);
}
