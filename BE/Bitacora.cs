using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class Bitacora
    {
        public int Id { get; set; }

        [Browsable(false)]
        public Criticidad Criticidad { get; set; }

        [DisplayName("Criticidad")]
        public string strCritc => Criticidad?.Detalle ?? "—";

        [Browsable(false)]
        public Usuario Usuario { get; set; }

        [DisplayName("Usuario")]
        public string strUsuario => Usuario?.Email ?? "—";
        public string Descripcion { get; set; }
        public DateTime Fecha { get; set; }
    }
}

