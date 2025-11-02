using System;
using DAL;

namespace BLL
{
    /// <summary>
    /// Logger mínimo y tolerante a fallas. Llama directo al DAO y nunca
    /// deja propagar excepciones de bitácora.
    /// </summary>
    public static class Bitacora
    {
        private static readonly BitacoraDao _dao = BitacoraDao.GetInstance();

        // Centralizamos los códigos de severidad
        private enum Sev { Info = 1, Warn = 2, Error = 3 }

        // ---------- API genérica ----------
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

        // ---------- Sobrecarga legacy (8 parámetros) ----------
        // Hay sitios que históricamente llamaban con 'fecha' explícita.
        // La conservamos para evitar CS1501/CS7036 si existen esos call-sites.
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

        // ---------- núcleo tolerante ----------
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
                // Tu DAO no acepta fecha explícita, así que la omitimos.
                _dao.Add(userId, modulo, accion, severidad, mensaje, ip, host);
            }
            catch
            {
                // Nunca permitir que la bitácora rompa la app.
                // Si querés debuguear algo, podés escribir:
                // System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}
