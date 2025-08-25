using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class Criticidad
    {
        public int Id { get; private set; }
        public string Detalle { get; private set; }

        private static readonly Dictionary<int, string> _criticidad = new Dictionary<int, string>
        {
            { 1, "Bajo" },
            { 2, "Medio" },
            { 3, "Alto" }
        };

        public Criticidad(int id) { setCriticidad(id); }

        public void setCriticidad(int id)
        {
            if (_criticidad.TryGetValue(id, out string detalle))
            {
                Id = id;
                Detalle = detalle;
            }
        }
    }
}
