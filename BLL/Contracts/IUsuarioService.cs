using System.Collections.Generic;
using BE;

namespace BLL
{
    public interface IUsuarioService
    {
        IEnumerable<Usuario> Listar();
        Usuario ObtenerPorId(int id);
        void Crear(Usuario usuario);
        void Actualizar(Usuario usuario);
        void DarDeBaja(int idUsuario);  
        void Reactivar(int idUsuario);   
        bool UsuarioTieneRolId(int idUsuario, int idRol);
        bool ExisteUserName(string userName);
        bool ExisteUsername(string username, int? excludeId);
        bool ExisteEmail(string email, int? excludeId);

    }
}
