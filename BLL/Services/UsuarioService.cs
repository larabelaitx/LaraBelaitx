using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using DAL;
using BLL.Contracts;
using Svc = global::Services;   // PasswordService y DV
using Tx = System.Transactions; // alias para transacciones

namespace BLL.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly UsuarioDao _dao = UsuarioDao.GetInstance();
        private readonly FamiliaDao _famDao = FamiliaDao.GetInstance();

        public int MaxTries { get; set; } = 3;

        public Usuario GetById(int id) => _dao.GetById(id);
        public Usuario GetByUserName(string user) => _dao.GetByUserName(user);
        public List<Usuario> GetAll() => _dao.GetAllActive();
        public List<Usuario> GetAllActive() => _dao.GetAllActive();

        // ===========================
        //  ALTA: devuelve el Id nuevo
        // ===========================
        public int CrearConPassword(Usuario u, string plainPassword)
        {
            (byte[] hash, byte[] salt, int iters) = Svc.PasswordService.Hash(plainPassword);
            u.PasswordHash = hash;
            u.PasswordSalt = salt;
            u.PasswordIterations = iters;
            u.DebeCambiarContraseña = true;
            u.Tries = 0;
            if (u.EstadoUsuarioId == 0) u.EstadoUsuarioId = EstadosUsuario.Habilitado;

            var dvh = new DVH { dvh = Svc.DV.GetDV(DvhString(u)) };
            int newId = _dao.AddReturnId(u, dvh);

            BLL.Bitacora.Info(newId,
                $"Alta de usuario '{u.UserName}'",
                "Usuarios", "Usuario_Alta", host: Environment.MachineName);

            return newId;
        }

        public bool Crear(Usuario u)
        {
            var dvh = new DVH { dvh = Svc.DV.GetDV(DvhString(u)) };
            var ok = _dao.Add(u, dvh);
            if (ok)
                BLL.Bitacora.Info(u.Id, $"Creación de usuario '{u.UserName}'",
                    "Usuarios", "Usuario_Crear", host: Environment.MachineName);
            return ok;
        }

        public bool Actualizar(Usuario u)
        {
            if (!string.IsNullOrWhiteSpace(u.Documento) &&
                _dao.ExisteDocumento(u.Documento.Trim(), excluirId: u.Id))
                throw new Exception("El documento ya está en uso por otro usuario.");

            if (!string.IsNullOrWhiteSpace(u.Email) &&
                _dao.ExisteEmail(u.Email.Trim(), excluirId: u.Id))
                throw new Exception("El mail ya está en uso por otro usuario.");

            var dvh = new DVH { dvh = Svc.DV.GetDV(DvhString(u)) };
            var ok = _dao.Update(u, dvh);
            if (ok)
                BLL.Bitacora.Info(u.Id, $"Actualización de usuario '{u.UserName}'",
                    "Usuarios", "Usuario_Actualizar", host: Environment.MachineName);
            return ok;
        }

        // Alias para compatibilidad con la UI
        public bool Update(Usuario u) => Actualizar(u);

        public bool BajaLogica(Usuario u)
        {
            u.EstadoUsuarioId = EstadosUsuario.Baja;
            var dvh = new DVH { dvh = Svc.DV.GetDV(DvhString(u)) };
            var ok = _dao.Update(u, dvh);
            if (ok)
                BLL.Bitacora.Warn(u.Id, $"Baja lógica de usuario '{u.UserName}'",
                    "Usuarios", "Usuario_Baja", host: Environment.MachineName);
            return ok;
        }

        // ===============================
        // NUEVO: Cambiar estado (Baja / Reactivar)
        // ===============================
        public bool CambiarEstado(int idUsuario, int nuevoEstado)
        {
            var u = _dao.GetById(idUsuario);
            if (u == null) throw new Exception("Usuario inexistente.");

            u.EstadoUsuarioId = nuevoEstado;
            var dvh = new DVH { dvh = Svc.DV.GetDV(DvhString(u)) };
            var ok = _dao.CambiarEstado(idUsuario, nuevoEstado, dvh);

            if (ok)
            {
                string accion = nuevoEstado == EstadosUsuario.Habilitado ? "Reactivación" : "Baja";
                BLL.Bitacora.Info(idUsuario, $"{accion} de usuario '{u.UserName}'",
                    "Usuarios", $"Usuario_{accion}", host: Environment.MachineName);
            }
            return ok;
        }

        public bool Reactivar(int idUsuario) => CambiarEstado(idUsuario, EstadosUsuario.Habilitado);
        public bool DarDeBaja(int idUsuario) => CambiarEstado(idUsuario, EstadosUsuario.Baja);

        // =====================================
        // LOGIN / BLOQUEO / PASSWORD / CONTROL
        // =====================================
        public bool Login(string userName, string plainPassword, out Usuario usuario)
        {
            usuario = _dao.GetByUserName(userName);

            if (usuario == null)
            {
                BLL.Bitacora.Warn(null, $"Login fallido (usuario inexistente): {userName}",
                    "Seguridad", "LoginFail", host: Environment.MachineName);
                return false;
            }

            if (usuario.EstadoUsuarioId == EstadosUsuario.Bloqueado)
            {
                BLL.Bitacora.Warn(usuario.Id, "Intento de login con usuario bloqueado",
                    "Seguridad", "LoginBlocked", host: Environment.MachineName);
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

                BLL.Bitacora.Warn(usuario.Id, $"Intento de login fallido (tries={usuario.Tries}/{MaxTries})",
                    "Seguridad", "LoginFail", host: Environment.MachineName);
                return false;
            }

            usuario.Tries = 0;
            if (usuario.EstadoUsuarioId == EstadosUsuario.Bloqueado)
                usuario.EstadoUsuarioId = EstadosUsuario.Habilitado;

            var dvhOk = new DVH { dvh = Svc.DV.GetDV($"{usuario.Id}|{usuario.Tries}|{usuario.EstadoUsuarioId}") };
            _dao.MarcarUltimoLoginExitoso(usuario.Id, DateTime.UtcNow, dvhOk);

            BLL.Bitacora.Info(usuario.Id, "Inicio de sesión correcto",
                "Seguridad", "LoginOK", host: Environment.MachineName);

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

            BLL.Bitacora.Info(u.Id, $"Usuario '{u.UserName}' desbloqueado",
                "Usuarios", "Usuario_Desbloquear", host: Environment.MachineName);
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

            BLL.Bitacora.Info(u.Id, $"Password actualizado para '{u.UserName}'",
                "Usuarios", "Usuario_PasswordActualizar", host: Environment.MachineName);
        }

        // ======================
        // VALIDACIONES Y BAJAS
        // ======================
        public Usuario ObtenerPorId(int id) => GetById(id);
        public bool ExisteUsername(string username) => _dao.GetByUserName(username) != null;
        public bool ExisteEmail(string email)
            => !string.IsNullOrWhiteSpace(email) && _dao.ExisteEmail(email.Trim(), excluirId: null);
        public bool ExisteDocumento(string documento)
            => !string.IsNullOrWhiteSpace(documento) && _dao.ExisteDocumento(documento.Trim(), excluirId: null);

        private static string DvhString(Usuario u)
            => $"{u.Id}|{u.UserName}|{u.Email}|{u.EstadoUsuarioId}|{u.Tries}";

        private int CountHabilitadosBLL() => _dao.GetAllActive().Count;
        private int CountHabilitadosBLL(IEnumerable<int> ids)
        {
            var set = new HashSet<int>((ids ?? Enumerable.Empty<int>()).Distinct());
            return _dao.GetAllActive().Count(u => set.Contains(u.Id));
        }

        private int CountAdminsActivosBLL() =>
            _dao.GetAllActive().Count(u =>
                (_famDao.GetFamiliasUsuario(u.Id) ?? new List<Familia>())
                .Any(f => string.Equals(f.Name, "Administrador", StringComparison.OrdinalIgnoreCase)));

        private int CountAdminsActivosBLL(IEnumerable<int> ids)
        {
            var set = new HashSet<int>((ids ?? Enumerable.Empty<int>()).Distinct());
            return _dao.GetAllActive()
                       .Where(u => set.Contains(u.Id))
                       .Count(u => (_famDao.GetFamiliasUsuario(u.Id) ?? new List<Familia>())
                           .Any(f => string.Equals(f.Name, "Administrador", StringComparison.OrdinalIgnoreCase)));
        }

        public bool PuedeEliminarUsuarios(IEnumerable<int> idsAEliminar, out string motivo)
        {
            motivo = null;
            var lista = (idsAEliminar ?? new List<int>()).Distinct().ToList();
            if (lista.Count == 0) { motivo = "No se seleccionaron usuarios."; return false; }

            var totalHabilitados = CountHabilitadosBLL();
            var habAEliminar = CountHabilitadosBLL(lista);

            if (totalHabilitados - habAEliminar < 1)
            {
                motivo = "No se puede eliminar: quedaría el sistema sin usuarios.";
                return false;
            }
            return true;
        }

        // ===== BAJA TRANSACCIONAL =====
        public bool TryBajaUsuarios(IEnumerable<int> ids, out string motivo)
        {
            motivo = null;
            var lista = (ids ?? new List<int>()).Distinct().ToList();
            if (lista.Count == 0)
            {
                motivo = "No se seleccionaron usuarios para eliminar.";
                return false;
            }

            int totalHabilitados = CountHabilitadosBLL();
            int habAEliminar = CountHabilitadosBLL(lista);
            if (totalHabilitados - habAEliminar < 1)
            {
                motivo = "No se puede eliminar: quedaría el sistema sin usuarios habilitados.";
                return false;
            }

            int adminsActivos = CountAdminsActivosBLL();
            int adminsAEliminar = CountAdminsActivosBLL(lista);
            if (adminsActivos - adminsAEliminar < 1)
            {
                motivo = "Debe quedar al menos un Administrador activo.";
                return false;
            }

            var txOpts = new Tx.TransactionOptions { IsolationLevel = Tx.IsolationLevel.ReadCommitted };
            using (var scope = new Tx.TransactionScope(Tx.TransactionScopeOption.Required, txOpts))
            {
                try
                {
                    foreach (int id in lista)
                    {
                        _dao.SetUsuarioFamilias(id, new List<int>());
                        _dao.SetPatentesDeUsuario(id, new List<int>());

                        var u = _dao.GetById(id);
                        if (u != null)
                        {
                            u.EstadoUsuarioId = EstadosUsuario.Baja;
                            var dvh = new DVH { dvh = Svc.DV.GetDV(DvhString(u)) };
                            _dao.Update(u, dvh);
                        }

                        BLL.Bitacora.Warn(id, $"Baja lógica de usuario Id={id}",
                            "Usuarios", "Usuario_Baja", host: Environment.MachineName);
                    }

                    var dvvU = DAL.DVVDao.GetInstance().CalculateDVV("Usuario");
                    DAL.DVVDao.GetInstance().AddUpdateDVV(new BE.DVV { tabla = "Usuario", dvv = dvvU });

                    var dvvUF = DAL.DVVDao.GetInstance().CalculateDVV("UsuarioFamilia");
                    DAL.DVVDao.GetInstance().AddUpdateDVV(new BE.DVV { tabla = "UsuarioFamilia", dvv = dvvUF });

                    var dvvUP = DAL.DVVDao.GetInstance().CalculateDVV("UsuarioPatente");
                    DAL.DVVDao.GetInstance().AddUpdateDVV(new BE.DVV { tabla = "UsuarioPatente", dvv = dvvUP });

                    scope.Complete();
                    return true;
                }
                catch (Exception ex)
                {
                    BLL.Bitacora.Error(null, $"Error en baja transaccional: {ex.Message}",
                        "Usuarios", "Usuario_Baja", host: Environment.MachineName);
                    motivo = "Error interno: no se pudo completar la transacción.";
                    return false;
                }
            }
        }

        public bool BajaLogicaSegura(int idUsuario, out string mensaje)
        {
            var ok = _dao.BajaLogicaSegura(idUsuario, out mensaje);
            if (ok)
            {
                BLL.Bitacora.Warn(idUsuario, $"Baja lógica de usuario Id={idUsuario}",
                    "Usuarios", "Usuario_Baja", host: Environment.MachineName);
            }
            else
            {
                BLL.Bitacora.Warn(idUsuario, $"Intento de baja fallido: {mensaje}",
                    "Usuarios", "Usuario_Baja", host: Environment.MachineName);
            }
            return ok;
        }

        public bool Bloquear(int idUsuario)
        {
            var u = _dao.GetById(idUsuario);
            if (u == null) throw new Exception("Usuario inexistente.");

            u.EstadoUsuarioId = EstadosUsuario.Bloqueado;
            var dvh = new DVH { dvh = Svc.DV.GetDV($"{u.Id}|{u.UserName}|{u.EstadoUsuarioId}") };
            var ok = _dao.Update(u, dvh);

            if (ok)
                BLL.Bitacora.Warn(u.Id, $"Usuario '{u.UserName}' bloqueado manualmente.",
                    "Usuarios", "Usuario_Bloquear", host: Environment.MachineName);

            return ok;
        }

        public bool Desbloquear(int idUsuario)
        {
            var u = _dao.GetById(idUsuario);
            if (u == null) throw new Exception("Usuario inexistente.");

            u.EstadoUsuarioId = EstadosUsuario.Habilitado;
            u.Tries = 0;
            var dvh = new DVH { dvh = Svc.DV.GetDV($"{u.Id}|{u.UserName}|{u.EstadoUsuarioId}") };
            var ok = _dao.Update(u, dvh);

            if (ok)
                BLL.Bitacora.Info(u.Id, $"Usuario '{u.UserName}' desbloqueado.",
                    "Usuarios", "Usuario_Desbloquear", host: Environment.MachineName);

            return ok;
        }

    }
}
