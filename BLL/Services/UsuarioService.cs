using System;
using System.Collections.Generic;
using BE;
using DAL;
using BLL.Contracts;
using Svc = global::Services;

namespace BLL.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly UsuarioDao _dao = UsuarioDao.GetInstance();
        private readonly BitacoraDao _bitacora = BitacoraDao.GetInstance();

        public int MaxTries { get; set; } = 3;
        public Usuario GetById(int id) => _dao.GetById(id);
        public Usuario GetByUserName(string user) => _dao.GetByUserName(user);
        public List<Usuario> GetAll() => _dao.GetAllActive();
        public bool CrearConPassword(Usuario u, string plainPassword)
        {
            (string hash, string salt, int iters) = Svc.PasswordService.Hash(plainPassword);
            u.PasswordHash = hash;
            u.PasswordSalt = salt;
            u.PasswordIterations = iters;
            return Crear(u);
        }

        public bool Crear(Usuario u)
        {
            var dvh = new DVH { dvh = Svc.DV.GetDV(DvhString(u)) };
            return _dao.Add(u, dvh);
        }

        public bool Actualizar(Usuario u)
        {
            var dvh = new DVH { dvh = Svc.DV.GetDV(DvhString(u)) };
            return _dao.Update(u, dvh);
        }

        public bool BajaLogica(Usuario u)
        {
            var dvh = new DVH { dvh = Svc.DV.GetDV(DvhString(u)) };
            return _dao.Delete(u, dvh);
        }

        private static string DvhString(Usuario u)
            => $"{u.Id}|{u.UserName}|{u.Email}|{u.EstadoUsuarioId}|{u.Tries}";

        // --- Autenticación / Seguridad ---
        public bool Login(string userName, string plainPassword, out Usuario usuario)
        {
            usuario = _dao.GetByUserName(userName);
            if (usuario == null)
            {
                Log(1, null, $"Login fallido (usuario inexistente): {userName}");
                return false;
            }

            var ok = _dao.Login(userName, plainPassword);
            if (!ok)
            {
                usuario.Tries = (usuario.Tries <= 0 ? 0 : usuario.Tries) + 1;
                if (usuario.Tries >= MaxTries) usuario.EstadoUsuarioId = EstadosUsuario.Bloqueado;

                var dvh = new DVH { dvh = Svc.DV.GetDV($"{usuario.Id}|{usuario.Tries}|{usuario.EstadoUsuarioId}") };
                _dao.Update(usuario, dvh);

                Log(2, usuario.Id, $"Intento de login fallido (tries={usuario.Tries})");
                return false;
            }

            usuario.Tries = 0;
            var dvhOk = new DVH { dvh = Svc.DV.GetDV($"{usuario.Id}|{usuario.Tries}|{usuario.EstadoUsuarioId}") };
            _dao.Update(usuario, dvhOk);

            Log(1, usuario.Id, "Login exitoso");
            return true;
        }

        public void DesbloquearUsuario(int idUsuario)
        {
            var u = _dao.GetById(idUsuario);
            if (u == null) return;

            u.EstadoUsuarioId = EstadosUsuario.Habilitado;
            u.Tries = 0;
            var dvh = new DVH { dvh = Svc.DV.GetDV($"{u.Id}|{u.Tries}|{u.EstadoUsuarioId}") };
            _dao.Update(u, dvh);

            Log(1, u.Id, "Usuario desbloqueado por administrador");
        }

        private void Log(int criticidad, int? userId, string desc)
        {
            _bitacora.Add(new BE.Bitacora
            {
                Criticidad = new BE.Criticidad(criticidad),
                Usuario = userId.HasValue ? new BE.Usuario { Id = userId.Value } : null,
                Descripcion = desc,
                Fecha = DateTime.UtcNow
            }, new DVH { dvh = Svc.DV.GetDV($"{criticidad}|{userId}|{desc}|{DateTime.UtcNow:O}") });
        }
    
        public Usuario ObtenerPorId(int id) => GetById(id);

        public bool ExisteUsername(string username)
        {
            var u = _dao.GetByUserName(username);
            return u != null;
        }

        public bool ExisteEmail(string email)
        {
            var all = _dao.GetAll();
            return all.Exists(x => !string.IsNullOrWhiteSpace(x.Email) &&
                                   x.Email.Trim().ToLower() == email?.Trim().ToLower());
        }

    }
}
