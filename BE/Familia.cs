using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class Familia : Permiso
    {
        public Familia()
        {
            Permisos = new List<Permiso>();
        }
        public string Descripcion { get; set; }
        [Browsable(false)]
        public List<Permiso> Permisos { get; set; }

        public override bool TienePermiso(Permiso permiso)
        {
            if (permiso == null) return false;
            foreach (var p in Permisos)
                if (p.TienePermiso(permiso)) return true;
            return false;
        }
        public new int Id { get; set; }              
        public string Nombre { get; set; }      
        public override string ToString() => Nombre;
    }
}
