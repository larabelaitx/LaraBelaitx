using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace BE
{
    //Clase Componente del Composite, construido por Permiso(Hoja) y Familia (Compuesto/Rama)
    public abstract class Permiso
    {
        public int Id { get; set; }

        [DisplayName("Nombre")]
        public string Name { get; set; }

        [Browsable(false)]
        public Sector Sector { get; set; }

        [Browsable(false)]
        public bool Activo { get; set; } = true;

        [Browsable(false)]
        public string DVH { get; set; }
        public abstract bool TienePermiso(Permiso permiso);
    }
}
