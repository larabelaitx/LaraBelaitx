using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class Cliente
    {
        public int IdCliente { get; set; }

        public string Nombre { get; set; }
        public string Apellido { get; set; }

        public DateTime FechaNacimiento { get; set; }
        public string LugarNacimiento { get; set; }

        public string Nacionalidad { get; set; }
        public string EstadoCivil { get; set; }  // catálogo hardcodeado
        public string DocumentoIdentidad { get; set; }
        public string CUITCUILCDI { get; set; }

        public string Domicilio { get; set; }
        public string Telefono { get; set; }
        public string CorreoElectronico { get; set; }

        public string Ocupacion { get; set; }
        public string SituacionFiscal { get; set; } // catálogo hardcodeado
        public bool EsPEP { get; set; }

        // Relación con Estado general (ej: Activo/Inactivo)
        public Estado Estado { get; set; }

        public override string ToString() => $"{Nombre} {Apellido} ({DocumentoIdentidad})";
    }
}
