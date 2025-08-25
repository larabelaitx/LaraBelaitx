using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    //Clase Componente del Composite, construido por Permiso(Hoja) y Familia (Compuesto/Rama)
    public abstract class Permiso
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public abstract bool TienePermiso(Permiso permiso);
    }
}
