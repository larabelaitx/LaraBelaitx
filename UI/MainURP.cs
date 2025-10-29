using System;
using System.Windows.Forms;
using Krypton.Toolkit;
using BLL.Services;

namespace UI
{
    public partial class MainURP : KryptonForm
    {
        private readonly UsuarioService _usrSvc = new UsuarioService();
        private readonly RolService _rolSvc = new RolService();

        public MainURP()
        {
            InitializeComponent();
        }

        private void MainURP_Load(object sender, EventArgs e) { }

        private void btnUsuarios_Click(object sender, EventArgs e)
        {
            using (var frm = new MainUsuarios(_usrSvc, _rolSvc))
                frm.ShowDialog(this);
        }

        private void btnRoles_Click(object sender, EventArgs e)
        {
            using (var frm = new MainFamilia()) // usa el nombre real
                frm.ShowDialog(this);
        }

        private void btnPermisos_Click(object sender, EventArgs e)
        {
            using (var frm = new MainPatente()) // o AltaPatente
                frm.ShowDialog(this);
        }
    }
}
