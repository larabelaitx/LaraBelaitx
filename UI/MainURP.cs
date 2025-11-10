using System;
using System.Windows.Forms;
using Krypton.Toolkit;
using BLL.Contracts;
using UI.Seguridad; // <- Perms

namespace UI
{
    public partial class MainURP : KryptonForm
    {
        private readonly IUsuarioService _usrSvc;
        private readonly IRolService _rolSvc;
        private readonly int? _currentUserId;

        public MainURP(IUsuarioService usrSvc, IRolService rolSvc, int? currentUserId = null)
        {
            InitializeComponent();
            _usrSvc = usrSvc ?? throw new ArgumentNullException(nameof(usrSvc));
            _rolSvc = rolSvc ?? throw new ArgumentNullException(nameof(rolSvc));
            _currentUserId = currentUserId;

            btnRoles.Click += btnRoles_Click;
            btnPermisos.Click += btnPermisos_Click;
            btnVolver.Click += btnVolver_Click;
            btnUsuarios.Click += btnUsuarios_Click;
        }

        private bool CheckAllowed(string patente)
        {
            if (!_currentUserId.HasValue || _currentUserId.Value <= 0 ||
                !_rolSvc.TienePatente(_currentUserId.Value, patente))
            {
                KryptonMessageBox.Show(this,
                    $"No tenés permiso para acceder a esta funcionalidad.\n({patente})",
                    "Acceso denegado", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void btnRoles_Click(object sender, EventArgs e)
        {
            if (!CheckAllowed(Perms.Familia_Listar)) return;
            try
            {
                using (var frm = new MainFamilia(_rolSvc, _currentUserId ?? 0))
                    frm.ShowDialog(this);
            }
            catch (Exception ex) { ShowErr("Familias", ex); }
        }

        private void btnPermisos_Click(object sender, EventArgs e)
        {
            // Patente real de tu BD para administrar patentes de usuarios
            if (!CheckAllowed(Perms.Patente_AsignarAUsuario)) return;
            try
            {
                using (var frm = new AltaPatente(_usrSvc, _rolSvc, _currentUserId))
                    frm.ShowDialog(this);
            }
            catch (Exception ex) { ShowErr("Patentes", ex); }
        }

        private void btnUsuarios_Click(object sender, EventArgs e)
        {
            if (!CheckAllowed(Perms.Usuario_Listar)) return;
            try
            {
                using (var frm = new MainUsuarios(_usrSvc, _rolSvc, _currentUserId))
                    frm.ShowDialog(this);
            }
            catch (Exception ex) { ShowErr("Usuarios", ex); }
        }

        private void btnVolver_Click(object sender, EventArgs e) => Close();

        private void ShowErr(string modulo, Exception ex)
        {
            KryptonMessageBox.Show(this,
                $"No se pudo abrir la gestión de {modulo}.\n\n{ex.Message}",
                "Error", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
        }
    }
}
