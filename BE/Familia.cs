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
        public string Descripcion { get; set; }
        [Browsable(false)]
        public HashSet<Permiso> Patentes { get; set; }
        [DisplayName("Patentes")]
        public string PatentesNames { get => PatentesNombres(); }
        public string PatentesNombres()
        {
            string patentes = "";
            foreach (Patente p in Patentes)
            {
                patentes += string.Format("{0}, ", p.Name);
            }
            return patentes;
        }
        public override bool TienePermiso(Permiso permiso)
        {
            foreach (Patente patente in Patentes)
            {
                if (patente.TienePermiso(permiso))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
