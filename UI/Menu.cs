using System;
using System.Linq;
using System.Windows.Forms;
using Krypton.Toolkit;

using BE;
using BLL.Contracts;
using BLL.Services;


namespace UI
{
    public partial class Menu : KryptonForm
    {
        private readonly IUsuarioService _usrSvc;
        private readonly IRolService _rolSvc;
        private readonly Usuario _usuarioActual;

        public Menu() : this(new UsuarioService(), new RolService(), null) { }
        public Menu(Usuario usuarioActual) : this(new UsuarioService(), new RolService(), usuarioActual) { }
        public Menu(IUsuarioService usrSvc, IRolService rolSvc, Usuario usuarioActual = null)
        {
            InitializeComponent();


            _usrSvc = usrSvc ?? throw new ArgumentNullException(nameof(usrSvc));
            _rolSvc = rolSvc ?? throw new ArgumentNullException(nameof(rolSvc));
            _usuarioActual = usuarioActual;

            var lbl = this.Controls.Find("lblUsuario", true).FirstOrDefault() as Label;
            if (lbl != null && _usuarioActual != null)
                lbl.Text = _usuarioActual?.NombreCompleto ?? _usuarioActual?.UserName ?? lbl.Text;

            TryWire("btnUsuariosRolesPermisos", btnUsuariosRolesPermisos_Click);
            TryWire("btnFamilias", btnFamilias_Click);
            TryWire("btnPermisos", btnPermisos_Click);
            TryWire("btnBackupRestore", btnBackupRestore_Click);
            TryWire("btnClientes", btnClientes_Click);
            TryWire("btnCuentas", btnCuentas_Click);
            TryWire("btnTarjetas", btnTarjetas_Click);
            TryWire("btnBitacora", btnBitacora_Click);
            TryWire("btnDigitosVerificadores", btnDigitosVerificadores_Click);
            TryWire("btnCerrarSesion", btnCerrarSesion_Click);
            TryWire("btnSalir", (s, e) => Close());
        }

        private void TryWire(string buttonName, EventHandler handler)
        {
            var btn = this.Controls.Find(buttonName, true).FirstOrDefault() as Control;
            if (btn != null) btn.Click += handler;
        }

        private void OpenModal(Form frm)
        {
            try
            {
                using (frm) frm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                BLL.Bitacora.Error(_usuarioActual?.Id, $"Error abriendo {frm?.GetType()?.Name}: {ex.Message}",
                    "UI", "OpenModal", host: Environment.MachineName);

                KryptonMessageBox.Show(this, ex.Message, "Error",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }

        private void btnUsuariosRolesPermisos_Click(object sender, EventArgs e)
        {
            OpenModal(new MainURP(_usrSvc, _rolSvc));
        }

        private void btnFamilias_Click(object sender, EventArgs e)
        {
            OpenModal(new MainFamilia(_rolSvc));
        }

        private void btnPermisos_Click(object sender, EventArgs e)
        {
            OpenModal(new AltaPatente(_usrSvc, _rolSvc));
        }

        private void btnBackupRestore_Click(object sender, EventArgs e)
        {
            try
            {
                var user = _usuarioActual;
                string lang =
                    (user?.IdiomaId == 2) ? "en-US" :
                    (user?.IdiomaId == 3) ? "pt-BR" :
                    "es-AR";

                OpenModal(new BackupRestore(user, lang));
            }
            catch (Exception ex)
            {
                BLL.Bitacora.Error(_usuarioActual?.Id, $"Error al abrir Backup/Restore: {ex.Message}",
                    "UI", "Abrir_BackupRestore", host: Environment.MachineName);

                KryptonMessageBox.Show(
                    $"Error al abrir el módulo de Backup/Restore:\n{ex.Message}",
                    "Error",
                    KryptonMessageBoxButtons.OK,
                    KryptonMessageBoxIcon.Error);
            }
        }

        private void btnClientes_Click(object sender, EventArgs e)
        {
            OpenModal(new MainClientes());
        }

        private void btnCuentas_Click(object sender, EventArgs e)
        {
            OpenModal(new MainCuentas());
        }

        private void btnTarjetas_Click(object sender, EventArgs e)
        {
            try
            {
                var frmType = Type.GetType("UI.MainTarjetas");
                if (frmType == null)
                {
                    KryptonMessageBox.Show(this,
                        "La pantalla de Tarjetas aún no está implementada (UI.MainTarjetas).",
                        "Información", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                    return;
                }
                var frm = (Form)Activator.CreateInstance(frmType);
                OpenModal(frm);
            }
            catch (Exception ex)
            {
                BLL.Bitacora.Error(_usuarioActual?.Id, $"Error al abrir Tarjetas: {ex.Message}",
                    "UI", "Abrir_Tarjetas", host: Environment.MachineName);

                KryptonMessageBox.Show(this, ex.Message, "Error",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }

        private void btnBitacora_Click(object sender, EventArgs e)
        {
            try
            {
                string cnn;
                using (var cn = DAL.ConnectionFactory.Open())
                {
                    cnn = cn.ConnectionString;
                }

                using (var frm = new Bitacora(cnn))
                {
                    frm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                BLL.Bitacora.Error(_usuarioActual?.Id, $"No se pudo abrir Bitácora: {ex.Message}",
                    "UI", "Abrir_Bitacora", host: Environment.MachineName);

                KryptonMessageBox.Show(this,
                    "No se pudo abrir Bitácora.\n\n" + ex.Message,
                    "Error", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }

        private void btnDigitosVerificadores_Click(object sender, EventArgs e)
        {
            OpenModal(new MainDV());
        }

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            // Usar owner seguro (null) para evitar ObjectDisposedException
            var res = KryptonMessageBox.Show(
                /* owner */ null,
                "¿Cerrar sesión?",
                "Confirmar",
                KryptonMessageBoxButtons.YesNo,
                KryptonMessageBoxIcon.Question
            );

            if (res == DialogResult.Yes)
            {
                // Si querés que el Login vuelva al frente de inmediato:
                try
                {
                    var login = Application.OpenForms.OfType<Login>().FirstOrDefault();
                    if (login != null)
                    {
                        login.WindowState = FormWindowState.Normal;
                        login.Activate();
                    }
                }
                catch { /* tolerante */ }

                Close(); // cierra el menú
            }
        }
    }
}
