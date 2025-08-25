using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                CuentaId = Get<int>(r, "CuentaId"),
                NumeroTarjeta = Get<string>(r, "NumeroTarjeta"),
                BIN = Get<string>(r, "BIN"),
                Titular = Get<string>(r, "Titular"),
                FechaEmision = Get<DateTime?>(r, "FechaEmision") ?? DateTime.MinValue,
                FechaVencimiento = Get<DateTime?>(r, "FechaVencimiento") ?? DateTime.MinValue,
                Marca = Get<string>(r, "Marca"),
                Tipo = Get<string>(r, "Tipo")
            };
        }

        private static bool Has(DataRow r, string c) => r.Table.Columns.Contains(c);
        private static T Get<T>(DataRow r, string c, T def = default)
        {
            if (!Has(r, c) || r[c] == DBNull.Value) return def;
            return (T)Convert.ChangeType(r[c], typeof(T));
        }
    }
}
