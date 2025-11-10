using System;

namespace BE
{
    public class Tarjeta
    {
        public int IdTarjeta { get; set; }

        // La tabla tiene IdCuenta e IdCliente
        public int CuentaId { get; set; }
        public Cuenta Cuenta { get; set; }
        public int ClienteId { get; set; }

        // La tabla tiene NroTarjeta, pero mantenemos la propiedad NumeroTarjeta.
        public string NumeroTarjeta { get; set; }

        // En tu tabla no existe BIN; lo dejamos por compatibilidad (se puede no usar).
        public string BIN { get; set; }

        // La tabla tiene NombreTitular; mantenemos Titular.
        public string Titular { get; set; }

        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }

        // La tabla usa "Banco". Mantenemos "Marca" y lo mapeamos a Banco.
        public string Marca { get; set; }

        // La tabla no tiene Tipo; lo mantenemos opcional (no se persiste).
        public string Tipo { get; set; }

        // CVV existe en la tabla (varbinary); lo exponemos como byte[].
        public byte[] CVV { get; set; }

        // Tenías Estado (BE.Estado); tu tabla aún no tiene columna de estado de tarjeta.
        // Lo dejamos opcional/no persistente por ahora.
        public Estado Estado { get; set; }

        public string NumeroEnmascarado =>
            string.IsNullOrEmpty(NumeroTarjeta) || NumeroTarjeta.Length < 4
                ? string.Empty
                : $"**** {NumeroTarjeta.Substring(NumeroTarjeta.Length - 4)}";

        public override string ToString() => $"{Marca} {NumeroEnmascarado}";
    }
}
