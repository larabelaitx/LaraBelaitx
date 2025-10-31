using System;
using System.Collections.Generic;
using System.Data;
using BE;

namespace DAL.Mappers
{
    public class MPUsuario
    {
        private static MPUsuario _instance;
        public static MPUsuario GetInstance() => _instance ?? (_instance = new MPUsuario());
        private MPUsuario() { }

        public Usuario MapUser(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0) return null;
            return MapRow(dt.Rows[0]);
        }

        public List<Usuario> Map(DataTable dt)
        {
            var list = new List<Usuario>();
            if (dt == null) return list;
            foreach (DataRow r in dt.Rows) list.Add(MapRow(r));
            return list;
        }

        private Usuario MapRow(DataRow r)
        {
            var hashBytes = GetBytes(r, "PasswordHash");
            var saltBytes = GetBytes(r, "PasswordSalt");

            return new Usuario
            {
                Id = Get<int>(r, "IdUsuario"),
                UserName = Get<string>(r, "Usuario"),
                Name = Get<string>(r, "Nombre"),
                LastName = Get<string>(r, "Apellido"),
                Email = Get<string>(r, "Mail"),
                Documento = Get<string>(r, "Documento"),

                EstadoUsuarioId = GetFlex<int>(r, 0, "EstadoUsuarioId", "IdEstado"),
                IdiomaId = GetFlex<int?>(r, null, "IdIdioma", "IdiomaId"),

                Tries = Get<int>(r, "NroIntentos"),

                PasswordHash = hashBytes,
                PasswordSalt = saltBytes,
                PasswordIterations = Get<int>(r, "PasswordIterations"),

                DVH = Get<string>(r, "DVH"),
                UltimoLoginUtc = Get<DateTime?>(r, "UltimoLoginUtc"),
                BloqueadoHastaUtc = Get<DateTime?>(r, "BloqueadoHastaUtc"),

                DebeCambiarContraseña = Get<bool>(r, "DebeCambiarContrasena", false)
            };
        }

        // ===== helpers =====
        private static bool HasCol(DataRow r, string col)
            => r?.Table?.Columns?.Contains(col) == true;

        private static T ConvertTo<T>(object v, T def = default)
        {
            if (v == null || v == DBNull.Value) return def;
            var t = typeof(T);
            var underlying = Nullable.GetUnderlyingType(t);
            return (T)System.Convert.ChangeType(v, underlying ?? t);
        }

        private static T Get<T>(DataRow r, string col, T def = default)
            => HasCol(r, col) ? ConvertTo<T>(r[col], def) : def;

        private static T GetFlex<T>(DataRow r, T def, params string[] cols)
        {
            foreach (var c in cols)
            {
                if (HasCol(r, c))
                {
                    var v = r[c];
                    if (v != DBNull.Value) return ConvertTo<T>(v, def);
                }
            }
            return def;
        }

        private static byte[] GetBytes(DataRow r, string col)
            => HasCol(r, col) && r[col] != DBNull.Value ? (byte[])r[col] : null;
    }
}