using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public static class CatalogosTarjeta
    {
        public static readonly string[] Marcas =
        {
            "Visa", "Mastercard", "Amex", "Maestro", "Cabal"
        };

        public static readonly string[] Tipos =
        {
            "Débito"
        };

        public static readonly string[] EstadosTarjeta =
        {
            "Activa", "Bloqueada", "Expirada", "Reemplazada"
        };
    }
}
