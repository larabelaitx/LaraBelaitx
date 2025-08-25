using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public override string ToString() => $"{TipoCuenta} {NumeroCuenta} ({Moneda})";
    }
}
