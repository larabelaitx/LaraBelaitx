using System;
using System.Data.SqlClient;
using Services;

namespace DAL
{
    /// <summary>
    /// Fábrica de conexiones centralizada que usa la configuración segura de AppConn.
    /// Evita dependencias a archivos locales o App.config.
    /// </summary>
    public static class ConnectionFactory
    {
        /// <summary>
        /// Abre y devuelve una conexión SqlConnection lista para usar.
        /// </summary>
        public static SqlConnection Open()
        {
            string cnn = AppConn.Get(); // obtiene la cadena guardada desde ConfigStringConn
            if (string.IsNullOrWhiteSpace(cnn))
                throw new InvalidOperationException("No hay cadena de conexión configurada. Ejecutá la configuración primero.");

            var cn = new SqlConnection(cnn);
            cn.Open();
            return cn;
        }

        /// <summary>
        /// Devuelve la cadena actual (sin abrir conexión).
        /// </summary>
        public static string Current => AppConn.Get();
    }
}
