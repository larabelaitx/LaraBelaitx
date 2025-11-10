// IUsuarioService.cs
using System.Collections.Generic;
using BE;

public interface IUsuarioService
{
    Usuario GetById(int id);
    Usuario GetByUserName(string user);
    List<Usuario> GetAll();
    List<Usuario> GetAllActive();

    int CrearConPassword(Usuario u, string plainPassword);

    bool Crear(Usuario u);
    bool Actualizar(Usuario u);
    bool BajaLogica(Usuario u);

    bool Bloquear(int idUsuario);
    bool Desbloquear(int idUsuario);

    bool Login(string userName, string plainPassword, out Usuario usuario);
    void DesbloquearUsuario(int idUsuario);
    void ActualizarPassword(int idUsuario, string nuevaClave, bool obligarCambio = false);

    Usuario ObtenerPorId(int id);
    bool ExisteUsername(string username);
    bool ExisteEmail(string email);
    bool ExisteDocumento(string documento);

    bool PuedeEliminarUsuarios(IEnumerable<int> idsAEliminar, out string motivo);
    bool TryBajaUsuarios(IEnumerable<int> ids, out string motivo);

    bool CambiarEstado(int idUsuario, int nuevoEstado);
    bool Reactivar(int idUsuario);     
    bool DarDeBaja(int idUsuario);   

    bool Update(Usuario u);         

    bool BajaLogicaSegura(int idUsuario, out string mensaje);
}
