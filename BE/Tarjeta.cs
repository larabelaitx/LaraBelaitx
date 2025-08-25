using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class Tarjeta
    {
        public int IdTarjeta { get; set; }

        public int CuentaId { get; set; }
        public Cuenta Cuenta { get; set; }
        public string NumeroTarjeta { get; set; } 
        public string BIN { get; set; }          
        public string Titular { get; set; }   

        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Marca { get; set; }      
        public string Tipo { get; set; }         

        public Estado Estado { get; set; }

        public string NumeroEnmascarado
    => string.IsNullOrEmpty(NumeroTarjeta) || NumeroTarjeta.Length < 4
       ? string.Empty
       : $"**** {NumeroTarjeta.Substring(NumeroTarjeta.Length - 4)}";


        public override string ToString() => $"{Marca} {NumeroEnmascarado}";
    }
}
