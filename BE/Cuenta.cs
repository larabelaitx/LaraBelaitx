using System;

namespace BE
{
    public class Cuenta
    {
        public int IdCuenta { get; set; }
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        public string NumeroCuenta { get; set; }
        public string CBU { get; set; }
        public string Alias { get; set; }
        public string TipoCuenta { get; set; }
        public string Moneda { get; set; }

        public decimal Saldo { get; set; }
        public DateTime FechaApertura { get; set; }
        public Estado Estado { get; set; }

        // 👇 NUEVO: texto para la grilla
        public string EstadoTexto => Estado.ToString();

        // 👇 NUEVO: nombre completo del cliente para la grilla y orden
        public string ClienteNombre
        {
            get
            {
                // Ajustá los nombres si tu Cliente usa Name/LastName
                var ape = Cliente?.Apellido ?? string.Empty;
                var nom = Cliente?.Nombre ?? string.Empty;
                var full = (ape + " " + nom).Trim();
                return full.Length == 0 ? "(sin cliente)" : full;
            }
        }

        public override string ToString() => $"{TipoCuenta} {NumeroCuenta} ({Moneda})";
    }
}
