using System;
using System.Windows.Forms;
using Krypton.Toolkit;
using BLL.Contracts;
using UI;

namespace UI
{
    public partial class MainURP : KryptonForm
    {
        private readonly IUsuarioService _usrSvc;
        private readonly IRolService _rolSvc;

        public MainURP(IUsuarioService usrSvc, IRolService rolSvc)
        {
            InitializeComponent();

            _usrSvc = usrSvc ?? throw new ArgumentNullException(nameof(usrSvc));
            _rolSvc = rolSvc ?? throw new ArgumentNullException(nameof(rolSvc));

            // Eventos principales
            btnRoles.Click += btnRoles_Click;
            btnPermisos.Click += btnPermisos_Click;
            btnVolver.Click += btnVolver_Click;
        }

        // --- Abrir gestión de Familias/Roles ---
        private void btnRoles_Click(object sender, EventArgs e)
        {
            try
            {
                using (var frm = new MainFamilia(_rolSvc))
                {
                    frm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(this,
                    "No se pudo abrir la gestión de Familias.\n\n" + ex.Message,
                    "Error",
                    KryptonMessageBoxButtons.OK,
                    KryptonMessageBoxIcon.Error);
            }
        }

        // --- Abrir gestión de Patentes ---
        private void btnPermisos_Click(object sender, EventArgs e)
        {
            try
            {
                using (var frm = new AltaPatente(_usrSvc, _rolSvc))
                {
                    frm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(this,
                    "No se pudo abrir la gestión de Patentes.\n\n" + ex.Message,
                    "Error",
                    KryptonMessageBoxButtons.OK,
                    KryptonMessageBoxIcon.Error);
            }
        }

        // --- Volver / Cerrar ---
        private void btnVolver_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnUsuarios_Click(object sender, EventArgs e)
        {
            try
            {
                // ✅ Abrimos MainUsuarios con los services
                using (var frm = new MainUsuarios(_usrSvc, _rolSvc))
                    frm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(this,
                    "No se pudo abrir la gestión de Usuarios.\n\n" + ex.Message,
                    "Error", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }
    }
}
