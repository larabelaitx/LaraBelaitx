using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Krypton.Toolkit;

namespace UI
{
    public partial class AltaCliente : KryptonForm
    {
        public enum FormMode { Alta, Edicion, Consulta }

        public FormMode Modo { get; set; } = FormMode.Alta;
        public int? IdCliente { get; set; }
        public AltaCliente()
        {
            InitializeComponent();
        }

        private void AltaCliente_Load(object sender, EventArgs e)
        {

        }
    }
}
