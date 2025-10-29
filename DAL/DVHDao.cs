using System;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Acceso a DVH por tabla. Usa ConnectionFactory para la cadena de conexión.
    /// </summary>
    public sealed class DVHDao
    {
        // --- Singleton ---
        private static DVHDao _instance;
        public static DVHDao GetInstance() => _instance ?? (_instance = new DVHDao());
        private DVHDao() { }

        /// <summary>
        /// Devuelve los DVH de la tabla indicada.
        /// </summary>
        public System.Collections.Generic.List<BE.DVH> GetDvhs(string table)
        {
            string safeTable = SanitizeTableName(table);

            // NOTA: no se puede parametrizar el nombre de tabla.
            // Usamos brackets + sanitizado para reducir riesgos.
            string sql = $"SELECT DVH FROM [{safeTable}]";

            using (var cn = ConnectionFactory.Open())
            using (var cmd = new SqlCommand(sql, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                da.Fill(dt);
                return DAL.Mappers.MPDVH.GetInstance().MapDVHs(dt, safeTable);
            }
        }

        /// <summary>
        /// Sanea el nombre de tabla para que sólo permita letras, números, _ y .
        /// Lanza excepción si queda vacío o cambia el nombre.
        /// </summary>
        private static string SanitizeTableName(string table)
        {
            if (string.IsNullOrWhiteSpace(table))
                throw new ArgumentException("El nombre de la tabla es requerido.", nameof(table));

            // Permitimos: letras, números, guion bajo y punto (para esquemas tipo dbo.Tabla)
            var chars = table.Trim();
            var filtered = new System.Text.StringBuilder(chars.Length);
            foreach (char c in chars)
            {
                if (char.IsLetterOrDigit(c) || c == '_' || c == '.')
                    filtered.Append(c);
            }

            var result = filtered.ToString();
            if (string.IsNullOrWhiteSpace(result))
                throw new ArgumentException("El nombre de la tabla no es válido.", nameof(table));

            // Evitar doble punto o finalizar con punto
            if (result.StartsWith(".") || result.EndsWith(".") || result.Contains(".."))
                throw new ArgumentException("El nombre de la tabla no es válido.", nameof(table));

            return result;
        }
    }
}
