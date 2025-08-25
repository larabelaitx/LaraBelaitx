using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Mappers
{
    internal class MPCuenta
    {
        private static MPCuenta _instance;
        public static MPCuenta GetInstance() => _instance ?? (_instance = new MPCuenta());
        private MPCuenta() { }

        public BE.Cuenta Map(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0) return null;
            return MapRow(dt.Rows[0]);
        }

        public List<BE.Cuenta> MapCuentas(DataTable dt)
        {
            var list = new List<BE.Cuenta>(dt?.Rows.Count ?? 0);
            if (dt == null) return list;
            foreach (DataRow r in dt.Rows) list.Add(MapRow(r));
            return list;
        }

        private static BE.Cuenta MapRow(DataRow r)
        {
            return new BE.Cuenta
            {
                IdCuenta = Get<int>(r, "IdCuenta"),
                ClienteId = Get<int>(r, "ClienteId"),
                NumeroCuenta = Get<string>(r, "NumeroCuenta"),
                CBU = Get<string>(r, "CBU"),
                Alias = Get<string>(r, "Alias"),
                TipoCuenta = Get<string>(r, "TipoCuenta"),
                Moneda = Get<string>(r, "Moneda"),
                Saldo = Get<decimal?>(r, "Saldo") ?? 0m,
                FechaApertura = Get<DateTime?>(r, "FechaApertura") ?? DateTime.MinValue
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
