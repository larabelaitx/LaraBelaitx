using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public static class CatalogosCuenta
    {
        public static readonly string[] TiposCuenta =
        {
            "Caja de Ahorro", "Cuenta Corriente"
        };

        public static readonly string[] Monedas =
        {
            "ARS"
        };

        public static readonly string[] EstadosCuenta =
        {
            "Abierta", "Suspendida", "Cerrada"
        };
    }
}
