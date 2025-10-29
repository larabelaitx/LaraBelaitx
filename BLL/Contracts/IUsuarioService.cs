using System.Collections.Generic;
using BE;

namespace BLL.Contracts
{
    public interface IUsuarioService
    {
        Usuario GetById(int id);
        Usuario GetByUserName(string user);
        List<Usuario> GetAll();
        List<Usuario> GetAllActive();

        // crea hasheando una contraseña en claro
        bool CrearConPassword(Usuario u, string plainPassword);

        bool Crear(Usuario u);
        bool Actualizar(Usuario u);
        bool BajaLogica(Usuario u);

        bool Login(string userName, string plainPassword, out Usuario usuario);
        void DesbloquearUsuario(int idUsuario);

        Usuario ObtenerPorId(int id);

        bool ExisteUsername(string username);
        bool ExisteEmail(string email);
        bool ExisteDocumento(string documento);              // <-- NUEVO

        void ActualizarPassword(int idUsuario, string nuevaClave, bool obligarCambio = false); // <-- NUEVO
    }
}
