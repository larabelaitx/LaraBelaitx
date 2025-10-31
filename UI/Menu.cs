using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Krypton.Toolkit;
using BE;
using BLL.Services;
using Services;

namespace UI
{
    public partial class Menu : KryptonForm
    {
        private readonly Usuario _usuario;
        private readonly UsuarioService _usrSvc = new UsuarioService();
        private readonly RolService _rolSvc = new RolService();
        private readonly ClienteService _cliSvc = new ClienteService();
        private readonly CuentaService _ctaSvc = new CuentaService();
        private readonly DVService _dvSvc = new DVService();

        public Menu(Usuario usuario)
        {
            InitializeComponent();
            _usuario = usuario ?? new Usuario { UserName = "Desconocido" };

            SafeHook(btnClientes, btnClientes_Click);
            SafeHook(btnCuentas, btnCuentas_Click);
            SafeHook(btnTarjetas, btnTarjetas_Click);
            SafeHook(btnUsuariosRolesPermisos, btnUsuariosRolesPermisos_Click);
            SafeHook(btnBitacora, btnBitacora_Click);
            SafeHook(btnBackupRestore, btnBackupRestore_Click);
            SafeHook(btnDigitosVerificadores, btnDigitosVerificadores_Click);
            SafeHook(btnCerrarSesion, btnCerrarSesion_Click);
            SafeHook(btnMiPerfil, btnMiPerfil_Click);

            this.Load += Menu_Load;
        }

        private void SafeHook(Control c, EventHandler handler)
        {
            if (c != null) c.Click += handler;
        }

        private void Menu_Load(object sender, EventArgs e)
        {
            try
            {
                RenderAlertas();
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(
                    "Ocurrió un error al cargar el menú:\n" + ex.Message,
                    "Menú",
                    KryptonMessageBoxButtons.OK,
                    KryptonMessageBoxIcon.Error);
            }
        }

        private void btnClientes_Click(object sender, EventArgs e)
        {
            using (var frm = new MainClientes())
                frm.ShowDialog(this);
        }

        private void btnCuentas_Click(object sender, EventArgs e)
        {
            using (var frm = new MainCuentas())
                frm.ShowDialog(this);
        }

        private void btnTarjetas_Click(object sender, EventArgs e)
        {
            KryptonMessageBox.Show("El módulo de Tarjetas está en construcción.", "Tarjetas");
        }

        private void btnBitacora_Click(object sender, EventArgs e)
        {
            try
            {
                string cnn = AppConn.Get(); // 🔹 ahora lee de tu archivo cifrado db.conn.sec

                if (string.IsNullOrWhiteSpace(cnn))
                    throw new Exception("No se encontró cadena de conexión configurada.");

                using (var frm = new Bitacora(cnn))
                    frm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(
                    "No se pudo abrir Bitácora:\n" + ex.Message,
                    "Bitácora",
                    KryptonMessageBoxButtons.OK,
                    KryptonMessageBoxIcon.Error);
            }
        }

        private void btnBackupRestore_Click(object sender, EventArgs e)
        {
            using (var frm = new BackupRestore(_usuario, "es"))
                frm.ShowDialog(this);
        }

        private void btnMiPerfil_Click(object sender, EventArgs e)
        {
            try
            {
                var u = _usrSvc.GetById(_usuario.Id);
                if (u == null)
                {
                    KryptonMessageBox.Show(
                        "No se pudo cargar el usuario actual.",
                        "Mi Perfil",
                        KryptonMessageBoxButtons.OK,
                        KryptonMessageBoxIcon.Warning);
                    return;
                }

                using (var frm = new AltaUsuario(ModoForm.Editar, u.Id, _usrSvc, _rolSvc))
                    frm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(
                    "Error al abrir Mi Perfil:\n" + ex.Message,
                    "Mi Perfil",
                    KryptonMessageBoxButtons.OK,
                    KryptonMessageBoxIcon.Error);
            }
        }

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            try { BLL.Bitacora.Info(_usuario.Id, "Cierre de sesión"); } catch { }

            this.Hide();
            using (var login = new Login())
                login.ShowDialog();
            this.Close();
        }

        private void RenderAlertas()
        {
            var rtb = this.Controls.Find("rtbAlertas", true).FirstOrDefault() as KryptonRichTextBox;
            var lbl = this.Controls.Find("lblAlertas", true).FirstOrDefault() as KryptonLabel;
            if (lbl != null) lbl.Values.Text = "Alertas";

            var sb = new StringBuilder();

            try
            {
                var bloqueados = _usrSvc.GetAll()
                                        .Where(u => string.Equals(u.EstadoDisplay, "Bloqueado", StringComparison.OrdinalIgnoreCase))
                                        .ToList();
                if (bloqueados.Any())
                {
                    sb.AppendLine("• Usuarios bloqueados:");
                    foreach (var b in bloqueados)
                        sb.AppendLine($"   - {b.UserName} ({b.Email})");
                    sb.AppendLine();
                }
            }
            catch { }

            try
            {
                string[] tablas = { "Usuario", "Cliente", "Cuenta", "Familia", "Patente", "Bitacora" };
                var inconsistentes = tablas.Where(t => !_dvSvc.VerificarTabla(t, out _, out _)).ToList();
                if (inconsistentes.Any())
                {
                    sb.AppendLine("• Inconsistencias DVV:");
                    foreach (var t in inconsistentes) sb.AppendLine($"   - {t}");
                    sb.AppendLine();
                }
            }
            catch { }

            if (sb.Length == 0)
                sb.AppendLine("Sin alertas por el momento.");

            if (rtb != null) rtb.Text = sb.ToString();
        }

        private void btnDigitosVerificadores_Click(object sender, EventArgs e)
        {
            using (var frm = new MainDV())
                frm.ShowDialog(this);
        }

        private void btnUsuariosRolesPermisos_Click(object sender, EventArgs e)
        {
            using (var frm = new MainURP())
                frm.ShowDialog(this);
        }
    }
}
