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
using BE;
using DAL;
using BLL.Services;

namespace UI
{
    public partial class DialogRolesUsuario : KryptonForm
    {
        public List<int> FamiliasSeleccionadasIds { get; private set; } = new List<int>();

        private Usuario _usuario;

        public DialogRolesUsuario()
        {
            InitializeComponent();
        }

        public DialogRolesUsuario(Usuario usuario, IEnumerable<int> familiasInicialesIds = null)
            : this()
        {
            _usuario = usuario;
            if (familiasInicialesIds != null)
                FamiliasSeleccionadasIds = familiasInicialesIds.ToList();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
