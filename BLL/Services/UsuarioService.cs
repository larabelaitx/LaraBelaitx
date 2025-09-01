using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using BE;
using BLL.Services;
using DAL;

namespace BLL
{
    public class UsuarioService : IUsuarioService
    {
        private readonly UsuarioDao _usuarioDao;
        private readonly FamiliaDao _familiaDao;
        private readonly IDVService _dv;

        public UsuarioService(UsuarioDao usuarioDao, FamiliaDao familiaDao, IDVService dv)
        {
            _usuarioDao = usuarioDao;
            _familiaDao = familiaDao;
            _dv = dv;
        }

        public static UsuarioService CreateWithSingletons(IDVService dv)
            => new UsuarioService(UsuarioDao.GetInstance(), FamiliaDao.GetInstance(), dv);

        public IEnumerable<Usuario> Listar() => _usuarioDao.GetAll();

        public Usuario ObtenerPorId(int id) => _usuarioDao.GetById(id);

        public void Crear(Usuario u)
        {
            ValidarUsuario(u, esAlta: true);
            if (ExisteUserName(u.UserName)) throw new Exception("El nombre de usuario ya existe.");

            var dvh = _dv.CalcularDVHUsuario(u);
            if (!_usuarioDao.Add(u, dvh)) throw new Exception("No se pudo crear el usuario.");
        }

        public void Actualizar(Usuario u)
        {
            if (u == null || u.Id <= 0) throw new Exception("Usuario inválido.");
            ValidarUsuario(u, esAlta: false);

            var actual = _usuarioDao.GetById(u.Id);
            if (actual == null) throw new Exception("Usuario inexistente.");

            if (!string.Equals(actual.UserName, u.UserName, StringComparison.OrdinalIgnoreCase) &&
                ExisteUserName(u.UserName))
                throw new Exception("El nombre de usuario ya existe.");

            var dvh = _dv.CalcularDVHUsuario(u);
            if (!_usuarioDao.Update(u, dvh)) throw new Exception("No se pudo actualizar el usuario.");
        }

        public void DarDeBaja(int idUsuario)
        {
            var u = _usuarioDao.GetById(idUsuario);
            if (u == null) throw new Exception("Usuario inexistente.");

            u.Enabled = new Estado { Id = 3, Name = "Baja" };

            var dvh = _dv.CalcularDVHUsuario(u);
      
            if (!_usuarioDao.Update(u, dvh)) throw new Exception("No se pudo dar de baja al usuario.");

        }

        public void Reactivar(int idUsuario)
        {
            var u = _usuarioDao.GetById(idUsuario);
            if (u == null) throw new Exception("Usuario inexistente.");

            u.Enabled = new Estado { Id = 1, Name = "Habilitado" };

            var dvh = _dv.CalcularDVHUsuario(u);
            if (!_usuarioDao.Update(u, dvh)) throw new Exception("No se pudo reactivar al usuario.");
        }

        public bool UsuarioTieneRolId(int idUsuario, int idRol)
        {
            var familias = _familiaDao.GetFamiliasUsuario(idUsuario) ?? new List<Familia>();
            return familias.Any(f => f.Id == idRol);
        }

        public bool ExisteUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName)) return false;
            return _usuarioDao.GetByUserName(userName.Trim()) != null;
        }
        private void ValidarUsuario(Usuario u, bool esAlta)
        {
            if (u == null) throw new Exception("Objeto usuario nulo.");

            if (string.IsNullOrWhiteSpace(u.Name))
                throw new Exception("El nombre es obligatorio.");
            if (string.IsNullOrWhiteSpace(u.LastName))
                throw new Exception("El apellido es obligatorio.");
            if (string.IsNullOrWhiteSpace(u.UserName))
                throw new Exception("El usuario (login) es obligatorio.");
            if (!EsEmailValido(u.Email))
                throw new Exception("El email no es válido.");
        }

        private bool EsEmailValido(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                var addr = new MailAddress(email.Trim());
                return addr.Address == email.Trim();
            }
            catch { return false; }
        }
        public bool ExisteUsername(string username, int? excludeId)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;

            var usuario = _usuarioDao.GetByUserName(username.Trim());
            return usuario != null && (!excludeId.HasValue || usuario.Id != excludeId.Value);
        }

        public bool ExisteEmail(string email, int? excludeId)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            var usuarios = _usuarioDao.GetAll();
            return usuarios.Any(u => string.Equals(u.Email, email.Trim(), StringComparison.OrdinalIgnoreCase) &&
                                     (!excludeId.HasValue || u.Id != excludeId.Value));
        }
    }
}
