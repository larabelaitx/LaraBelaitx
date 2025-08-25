using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public static class CatalogosCliente
    {
        public static readonly string[] EstadosCiviles =
        {
            "Soltero/a", "Casado/a", "Divorciado/a", "Viudo/a", "Unión convivencial"
        };

        public static readonly string[] SituacionesFiscales =
        {
            "Consumidor Final", "Monotributo", "Responsable Inscripto", "Exento", "No Responsable"
        };

        public static readonly string[] TiposDocumento =
        {
            "DNI", "LE", "LC", "Pasaporte", "CI"
        };

        public static readonly string[] PEP =
        {
            "No", "Sí"
        };
    }
}
