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
        public List<Usuario> GetAllActive() => _dao.GetAllActive();

        // ====== FIX: usar HashBytes (devuelve byte[]), no strings ======
        public bool CrearConPassword(Usuario u, string plainPassword)
        {
            // Hash PBKDF2; tu PasswordService.Hash devuelve (byte[] hash, byte[] salt, int iters)
            (byte[] hash, byte[] salt, int iters) = Svc.PasswordService.Hash(plainPassword);

            u.PasswordHash = hash;
            u.PasswordSalt = salt;
            u.PasswordIterations = iters;

            // primera vez: forzar cambio
            u.DebeCambiarContraseña = true;

            return Crear(u);
        }

        public bool ExisteDocumento(string documento)
        {
            if (string.IsNullOrWhiteSpace(documento)) return false;
            var all = _dao.GetAll();
            var doc = documento.Trim();
            return all.Exists(x => !string.IsNullOrWhiteSpace(x.Documento) &&
                                   x.Documento.Trim().Equals(doc, System.StringComparison.OrdinalIgnoreCase));
        }

        public void ActualizarPassword(int idUsuario, string nuevaClave, bool obligarCambio = false)
        {
            var u = _dao.GetById(idUsuario);
            if (u == null) return;

            (byte[] hash, byte[] salt, int iters) = Svc.PasswordService.Hash(nuevaClave);
            u.PasswordHash = hash;
            u.PasswordSalt = salt;
            u.PasswordIterations = iters;
            u.DebeCambiarContraseña = obligarCambio;
            u.Tries = 0;

            var dvh = new DVH { dvh = Svc.DV.GetDV($"{u.Id}|{u.Tries}|{u.EstadoUsuarioId}") };
            _dao.Update(u, dvh);
        }
        // ===============================================================

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

        public bool Login(string userName, string plainPassword, out Usuario usuario)
        {
            usuario = _dao.GetByUserName(userName);

            // Usuario inexistente: no reveles nada
            if (usuario == null)
            {
                Log(severidad: 2, userId: null,
                    mensaje: $"Login fallido (usuario inexistente): {userName}",
                    accion: "LoginFail");
                return false;
            }

            // Si ya está bloqueado, no permitas seguir
            if (usuario.EstadoUsuarioId == EstadosUsuario.Bloqueado)
            {
                Log(severidad: 2, userId: usuario.Id,
                    mensaje: "Intento de login con usuario bloqueado",
                    accion: "LoginBlocked");
                return false;
            }

            // Validación de password
            var ok = _dao.Login(userName, plainPassword);
            if (!ok)
            {
                // incrementar intentos (nunca negativo)
                usuario.Tries = Math.Max(0, usuario.Tries) + 1;

                // bloquear si alcanzó el máximo
                if (usuario.Tries >= MaxTries)
                    usuario.EstadoUsuarioId = EstadosUsuario.Bloqueado;

                // persistir (DVH mínimo para los campos que tocamos)
                var dvh = new DVH { dvh = Svc.DV.GetDV($"{usuario.Id}|{usuario.Tries}|{usuario.EstadoUsuarioId}") };
                _dao.Update(usuario, dvh);

                Log(severidad: 2, userId: usuario.Id,
                    mensaje: $"Intento de login fallido (tries={usuario.Tries}/{MaxTries})",
                    accion: "LoginFail");

                return false;
            }

            // Password OK: resetear intentos y mantener estado habilitado
            usuario.Tries = 0;
            if (usuario.EstadoUsuarioId == EstadosUsuario.Bloqueado)
                usuario.EstadoUsuarioId = EstadosUsuario.Habilitado;

            var dvhOk = new DVH { dvh = Svc.DV.GetDV($"{usuario.Id}|{usuario.Tries}|{usuario.EstadoUsuarioId}") };
            _dao.Update(usuario, dvhOk);

            Log(severidad: 1, userId: usuario.Id,
                mensaje: "Login exitoso",
                accion: "Login");

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

            Log(severidad: 1, userId: u.Id,
                mensaje: "Usuario desbloqueado por administrador",
                accion: "Unblock");
        }

        private void Log(int severidad, int? userId, string mensaje, string accion, string modulo = "Seguridad")
        {
            _bitacora.Add(
                usuarioId: userId,
                modulo: modulo,
                accion: accion,
                severidad: severidad,
                mensaje: mensaje,
                ip: null,
                host: Environment.MachineName,
                fecha: DateTime.Now
            );
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
