using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class Patente : Permiso
    {
        public override bool TienePermiso(Permiso permiso)
        {
            return permiso != null && permiso.Id == this.Id;
        }
    }
}
