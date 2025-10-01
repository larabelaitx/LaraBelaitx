using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class BitacoraEntry
    {
        public long Id { get; set; }
        public DateTime Fecha { get; set; }
        public int? UsuarioId { get; set; }
        public string Usuario { get; set; }
        public string Modulo { get; set; }
        public string Accion { get; set; }
        public byte Severidad { get; set; }
        public string Mensaje { get; set; }
        public string IP { get; set; }
        public string Host { get; set; }
    }
}
