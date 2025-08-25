using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        [Browsable(false)]
        public string Password { get; set; }
        public int Tries { get; set; }
        [Browsable(false)]
        public Estado Enabled { get; set; }
        public List<Permiso> Permisos { get; set; }

        [Browsable(false)]
        public Idioma Language { get; set; }
        [DisplayName("Language")]
        public string IdiomaNombre { get => Language.Name; }
        [DisplayName("Enabled")]
        public bool IsEnabled
        {
            get => Enabled.Id == 1;
        }
    }
}
