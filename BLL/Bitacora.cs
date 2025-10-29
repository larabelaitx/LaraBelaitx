using System;
using DAL;

namespace BLL
{
    /// <summary>
    /// Logger mínimo para no depender del BitacoraService en este ensamblado.
    /// Es estático y llama directo al DAO.
    /// </summary>
    public static class Bitacora
    {
        private static readonly BitacoraDao _dao = BitacoraDao.GetInstance();

        public static void Info(int? userId, string mensaje,
                                string modulo = "App", string accion = "Info",
                                string ip = null, string host = null)
        {
            // severidad 1 = Info (ajústalo si tenés otra convención)
            _dao.Add(userId, modulo, accion, 1, mensaje, ip, host, DateTime.UtcNow);
        }

        public static void Warn(int? userId, string mensaje,
                                string modulo = "App", string accion = "Warn",
                                string ip = null, string host = null)
        {
            // severidad 2 = Warning
            _dao.Add(userId, modulo, accion, 2, mensaje, ip, host, DateTime.UtcNow);
        }

        public static void Error(int? userId, string mensaje,
                                 string modulo = "App", string accion = "Error",
                                 string ip = null, string host = null)
        {
            // severidad 3 = Error
            _dao.Add(userId, modulo, accion, 3, mensaje, ip, host, DateTime.UtcNow);
        }
    }
}
