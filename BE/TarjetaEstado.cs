using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public enum TarjetaEstado
    {
        Solicitada = 1,
        Rechazada = 2,
        Emitida = 3,
        EnTransito = 4,
        Entregada = 5,
        BloqueadaTemporal = 6,
        BloqueadaDefinitiva = 7,
        Baja = 8,
        Reemplazada = 9,
        Vencida = 10
    }
}

