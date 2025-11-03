// BLL.Contracts / IUsuarioService.cs
using System.Collections.Generic;
using BE;

public interface IUsuarioService
{
    Usuario GetById(int id);
    Usuario GetByUserName(string user);
    List<Usuario> GetAll();
    List<Usuario> GetAllActive();

    bool CrearConPassword(Usuario u, string plainPassword);
    bool Crear(Usuario u);
    bool Actualizar(Usuario u);
    bool BajaLogica(Usuario u);

    bool Login(string userName, string plainPassword, out Usuario usuario);
    void DesbloquearUsuario(int idUsuario);
    void ActualizarPassword(int idUsuario, string nuevaClave, bool obligarCambio = false);

    Usuario ObtenerPorId(int id);
    bool ExisteUsername(string username);
    bool ExisteEmail(string email);
    bool ExisteDocumento(string documento);

    // Validación previa (opcional)
    bool PuedeEliminarUsuarios(IEnumerable<int> idsAEliminar, out string motivo);

    // ✅ La que falta y usa MainUsuarios.cs
    bool TryBajaUsuarios(IEnumerable<int> ids, out string motivo);
}
