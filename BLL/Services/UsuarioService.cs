using System;
using System.Collections.Generic;
using BE;
using DAL;
using BLL.Contracts;
using Svc = global::Services;   // alias para PasswordService y DV

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

        // Crea usuario hasheando una contraseña en claro
        public bool CrearConPassword(Usuario u, string plainPassword)
        {
            (byte[] hash, byte[] salt, int iters) = Svc.PasswordService.Hash(plainPassword);
            u.PasswordHash = hash;
            u.PasswordSalt = salt;
            u.PasswordIterations = iters;
            u.DebeCambiarContraseña = true; // primera vez: forzar cambio
            return Crear(u);
        }

        public bool Crear(Usuario u)
        {
            var dvh = new DVH { dvh = Svc.DV.GetDV(DvhString(u)) };
            return _dao.Add(u, dvh);
        }

        public bool Actualizar(Usuario u)
        {
            // Validaciones de unicidad via DAO (sin GetAll)
            if (!string.IsNullOrWhiteSpace(u.Documento) &&
                _dao.ExisteDocumento(u.Documento.Trim(), excluirId: u.Id))
                throw new Exception("El documento ya está en uso por otro usuario.");

            if (!string.IsNullOrWhiteSpace(u.Email) &&
                _dao.ExisteEmail(u.Email.Trim(), excluirId: u.Id))
                throw new Exception("El mail ya está en uso por otro usuario.");

            var dvh = new DVH { dvh = Svc.DV.GetDV(DvhString(u)) };
            return _dao.Update(u, dvh);
        }

        public bool BajaLogica(Usuario u)
        {
            var dvh = new DVH { dvh = Svc.DV.GetDV(DvhString(u)) };
            return _dao.Delete(u, dvh);
        }

        public bool Login(string userName, string plainPassword, out Usuario usuario)
        {
            usuario = _dao.GetByUserName(userName);

            if (usuario == null)
            {
                Log(severidad: 2, userId: null,
                    mensaje: $"Login fallido (usuario inexistente): {userName}",
                    accion: "LoginFail");
                return false;
            }

            if (usuario.EstadoUsuarioId == EstadosUsuario.Bloqueado)
            {
                Log(severidad: 2, userId: usuario.Id,
                    mensaje: "Intento de login con usuario bloqueado",
                    accion: "LoginBlocked");
                return false;
            }

            var ok = _dao.Login(userName, plainPassword);
            if (!ok)
            {
                usuario.Tries = Math.Max(0, usuario.Tries) + 1;

                if (usuario.Tries >= MaxTries)
                    usuario.EstadoUsuarioId = EstadosUsuario.Bloqueado;

                var dvh = new DVH { dvh = Svc.DV.GetDV($"{usuario.Id}|{usuario.Tries}|{usuario.EstadoUsuarioId}") };
                _dao.Update(usuario, dvh);

                Log(severidad: 2, userId: usuario.Id,
                    mensaje: $"Intento de login fallido (tries={usuario.Tries}/{MaxTries})",
                    accion: "LoginFail");

                return false;
            }

            // OK: resetear intentos y (si estaba) habilitar
            usuario.Tries = 0;
            if (usuario.EstadoUsuarioId == EstadosUsuario.Bloqueado)
                usuario.EstadoUsuarioId = EstadosUsuario.Habilitado;

            var dvhOk = new DVH { dvh = Svc.DV.GetDV($"{usuario.Id}|{usuario.Tries}|{usuario.EstadoUsuarioId}") };

            // Graba último login en UTC
            _dao.MarcarUltimoLoginExitoso(usuario.Id, DateTime.UtcNow, dvhOk);

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

        public Usuario ObtenerPorId(int id) => GetById(id);

        public bool ExisteUsername(string username) => _dao.GetByUserName(username) != null;

        // Usar la consulta directa del DAO (mejor que recorrer GetAll)
        public bool ExisteEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return _dao.ExisteEmail(email.Trim(), excluirId: null);
        }

        public bool ExisteDocumento(string documento)
        {
            if (string.IsNullOrWhiteSpace(documento)) return false;
            return _dao.ExisteDocumento(documento.Trim(), excluirId: null);
        }

        // ===== Helpers =====
        private static string DvhString(Usuario u)
            => $"{u.Id}|{u.UserName}|{u.Email}|{u.EstadoUsuarioId}|{u.Tries}";

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
    }
}