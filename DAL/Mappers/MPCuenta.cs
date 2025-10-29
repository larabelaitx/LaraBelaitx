using System;
using System.Collections.Generic;
using System.Data;

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
            bool Has(string c) => r.Table.Columns.Contains(c);
            T Get<T>(string c, T def = default)
            {
                if (!Has(c) || r[c] == DBNull.Value) return def;
                return (T)Convert.ChangeType(r[c], typeof(T));
            }

            var cuenta = new BE.Cuenta
            {
                IdCuenta = Get<int>("IdCuenta"),
                ClienteId = Get<int>("ClienteId"),
                NumeroCuenta = Get<string>("NumeroCuenta"),
                CBU = Get<string>("CBU"),
                Alias = Get<string>("Alias"),
                TipoCuenta = Get<string>("TipoCuenta"),
                Moneda = Get<string>("Moneda"),
                Saldo = Get<decimal?>("Saldo") ?? 0m,
                FechaApertura = Get<DateTime?>("FechaApertura") ?? DateTime.MinValue
            };

            if (Has("Estado"))
            {
                var idEstado = Get<int>("Estado");
                cuenta.Estado = new BE.Estado { Id = idEstado, Name = Get<string>("EstadoNombre") };
            }

            if (cuenta.Cliente == null && Has("ClienteId"))
            {
                cuenta.Cliente = new BE.Cliente { IdCliente = cuenta.ClienteId };

            }

            return cuenta;
        }
    }
}
