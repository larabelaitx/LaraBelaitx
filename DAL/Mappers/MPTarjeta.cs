using System;
using System.Collections.Generic;
using System.Data;

namespace DAL.Mappers
{
    internal class MPTarjeta
    {
        private static MPTarjeta _instance;
        public static MPTarjeta GetInstance() => _instance ?? (_instance = new MPTarjeta());
        private MPTarjeta() { }

        public BE.Tarjeta Map(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0) return null;
            return MapRow(dt.Rows[0]);
        }

        public List<BE.Tarjeta> MapTarjetas(DataTable dt)
        {
            var list = new List<BE.Tarjeta>(dt?.Rows.Count ?? 0);
            if (dt == null) return list;
            foreach (DataRow r in dt.Rows) list.Add(MapRow(r));
            return list;
        }

        private static BE.Tarjeta MapRow(DataRow r)
        {
            return new BE.Tarjeta
            {
                IdTarjeta = Get<int>(r, "IdTarjeta"),
                // Usamos alias en los SELECT para que estas columnas existan:
                CuentaId = Get<int>(r, "CuentaId"),
                ClienteId = Get<int>(r, "ClienteId"),
                NumeroTarjeta = Get<string>(r, "NumeroTarjeta"),
                Titular = Get<string>(r, "Titular"),
                FechaEmision = Get<DateTime?>(r, "FechaEmision") ?? DateTime.MinValue,
                FechaVencimiento = Get<DateTime?>(r, "FechaVencimiento") ?? DateTime.MinValue,
                Marca = Get<string>(r, "Marca"),
                // Campos opcionales
                BIN = Get<string>(r, "BIN"),
                Tipo = Get<string>(r, "Tipo"),
                CVV = Get<byte[]>(r, "CVV")
            };
        }

        private static bool Has(DataRow r, string c) => r.Table.Columns.Contains(c);

        private static T Get<T>(DataRow r, string c, T def = default)
        {
            if (!Has(r, c) || r[c] == DBNull.Value) return def;

            // Conversión segura a byte[]
            if (typeof(T) == typeof(byte[]))
            {
                if (r[c] == DBNull.Value) return def;
                return (T)(object)((byte[])r[c]);
            }

            return (T)Convert.ChangeType(r[c], typeof(T));
        }
    }
}
