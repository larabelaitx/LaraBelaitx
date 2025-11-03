using System;
using DAL;

namespace BLL
{
    /// <summary>
    /// Logger mínimo y tolerante. Centraliza severidades y SIEMPRE usa fecha UTC.
    /// Nunca deja propagar excepciones de bitácora.
    /// </summary>
    public static class Bitacora
    {
        private static readonly BitacoraDao _dao = BitacoraDao.GetInstance();

        // Severidades estándar (coinciden con tu tabla Criticidad: 1=Info, 2=Warn, 3=Error)
        private enum Sev { Info = 1, Warn = 2, Error = 3 }

        // ---------- API genérica (fecha opcional) ----------
        public static void Add(
            int? userId,
            string modulo,
            string accion,
            int severidad,
            string mensaje,
            string ip = null,
            string host = null,
            DateTime? fechaUtc = null)
        {
            TryAdd(userId, modulo, accion, severidad, mensaje, ip, host, fechaUtc ?? DateTime.UtcNow);
        }

        // ---------- APIs semánticas ----------
        public static void Info(int? userId, string mensaje,
                                string modulo = "App", string accion = "Info",
                                string ip = null, string host = null)
        {
            TryAdd(userId, modulo, accion, (int)Sev.Info, mensaje, ip, host, DateTime.UtcNow);
        }

        public static void Warn(int? userId, string mensaje,
                                string modulo = "App", string accion = "Warn",
                                string ip = null, string host = null)
        {
            TryAdd(userId, modulo, accion, (int)Sev.Warn, mensaje, ip, host, DateTime.UtcNow);
        }

        public static void Error(int? userId, string mensaje,
                                 string modulo = "App", string accion = "Error",
                                 string ip = null, string host = null)
        {
            TryAdd(userId, modulo, accion, (int)Sev.Error, mensaje, ip, host, DateTime.UtcNow);
        }

        // ---------- Sobrecarga legacy (por compatibilidad) ----------
        public static void Add(
            int? userId,
            string modulo,
            string accion,
            int severidad,
            string mensaje,
            DateTime fechaUtc,
            string ip,
            string host)
        {
            TryAdd(userId, modulo, accion, severidad, mensaje, ip, host, fechaUtc);
        }

        // ---------- Núcleo tolerante ----------
        private static void TryAdd(
            int? userId,
            string modulo,
            string accion,
            int severidad,
            string mensaje,
            string ip,
            string host,
            DateTime fechaUtc)
        {
            try
            {
                // Si no pasan host, registramos el de la máquina local (evita NULL innecesario)
                var safeHost = string.IsNullOrWhiteSpace(host) ? Environment.MachineName : host;

                // Usamos SIEMPRE el overload con fecha explícita del DAO
                _dao.Add(
                    usuarioId: userId,
                    modulo: modulo,
                    accion: accion,
                    severidad: severidad,
                    mensaje: mensaje,
                    ip: ip,
                    host: safeHost,
                    fechaUtc: fechaUtc
                );
            }
            catch
            {
                // Nunca permitir que la bitácora rompa la app.
                // (Opcional) Debug: System.Diagnostics.Debug.WriteLine(ex);
            }
        }
    }
}
