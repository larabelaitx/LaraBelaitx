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
        public BE.Criticidad Criticidad { get; set; }
        [DisplayName("Criticidad")]
        public string strCritc { get => Criticidad.Detalle; }
        [Browsable(false)]
        public BE.Usuario Usuario { get; set; }
        [DisplayName("Usuario")]
        public string strUsuario { get => Usuario.Email; }
        public string Descripcion { get; set; }
        public DateTime Fecha { get; set; }
    }
}

