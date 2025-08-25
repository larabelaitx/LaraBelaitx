using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Mappers
{
    internal class MPCliente
    {
        private static MPCliente _instance;
        public static MPCliente GetInstance() => _instance ?? (_instance = new MPCliente());
        private MPCliente() { }

        public BE.Cliente Map(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0) return null;
            return MapRow(dt.Rows[0]);
        }

        public List<BE.Cliente> MapClientes(DataTable dt)
        {
            var list = new List<BE.Cliente>(dt?.Rows.Count ?? 0);
            if (dt == null) return list;
            foreach (DataRow r in dt.Rows) list.Add(MapRow(r));
            return list;
        }

        private static BE.Cliente MapRow(DataRow r)
        {
            return new BE.Cliente
            {
                IdCliente = Get<int>(r, "IdCliente"),
                Nombre = Get<string>(r, "Nombre"),
                Apellido = Get<string>(r, "Apellido"),
                FechaNacimiento = Get<DateTime?>(r, "FechaNacimiento") ?? DateTime.MinValue,
                LugarNacimiento = Get<string>(r, "LugarNacimiento"),
                Nacionalidad = Get<string>(r, "Nacionalidad"),
                EstadoCivil = Get<string>(r, "EstadoCivil"),
                DocumentoIdentidad = Get<string>(r, "DocumentoIdentidad"),
                CUITCUILCDI = Get<string>(r, "CUITCUILCDI"),
                Domicilio = Get<string>(r, "Domicilio"),
                Telefono = Get<string>(r, "Telefono"),
                CorreoElectronico = Get<string>(r, "CorreoElectronico"),
                Ocupacion = Get<string>(r, "Ocupacion"),
                SituacionFiscal = Get<string>(r, "SituacionFiscal"),
                EsPEP = Get<bool>(r, "EsPEP", false)
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
