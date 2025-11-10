using System;
using System.Windows.Forms;
using Krypton.Toolkit;
using BLL.Services;
using Services;
using UI.Infrastructure;

namespace UI
{
    public partial class Login : KryptonForm
    {
        private readonly UsuarioService _usuarios = new UsuarioService();

        public Login()
        {
            InitializeComponent();
            KeyPreview = true;
            AcceptButton = btnIngresar;

            // Ayuda F1 (no invasiva)
            UI.F1Help.Wire(this, "seguridad.login", () => "es-AR");
        }

        private void Login_Load(object sender, EventArgs e)
        {
            // Limpio contexto al abrir login
            SecurityContext.Clear();

            txtUsuario.Clear();
            txtContraseña.Clear();
            txtUsuario.Focus();
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            string usuario = (txtUsuario.Text ?? "").Trim();
            string contraseña = txtContraseña.Text;

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contraseña))
            {
                // Sin mensajes: simplemente no hace nada si faltan datos
                return;
            }

            btnIngresar.Enabled = false;
            try
            {
                var ok = _usuarios.Login(usuario, contraseña, out BE.Usuario u);
                if (!ok)
                {
                    // Silencioso: limpia y vuelve a pedir
                    txtContraseña.Clear();
                    txtContraseña.Focus();
                    return;
                }

                // Cambio de contraseña obligatorio (solo diálogo de cambio, sin mensajes extra)
                if (u.DebeCambiarContraseña)
                {
                    using (var dlg = new ChangePasswordDialog())
                    {
                        if (dlg.ShowDialog(this) != DialogResult.OK) return;

                        var hp = PasswordService.Hash(dlg.NewPassword);
                        u.PasswordHash = hp.Hash;
                        u.PasswordSalt = hp.Salt;
                        u.PasswordIterations = hp.Iterations;
                        u.DebeCambiarContraseña = false;
                        u.Tries = 0;
                        _usuarios.Actualizar(u);

                        // Forzamos re-login
                        txtUsuario.Text = u.UserName;
                        txtContraseña.Clear();
                        txtContraseña.Focus();
                        return;
                    }
                }

                // Seteo de contexto y navegación
                SecurityContext.CurrentUser = u;

                var menu = new Menu(u);
                Hide();
                menu.FormClosed += (s2, e2) =>
                {
                    SecurityContext.Clear();
                    txtContraseña.Clear();
                    WindowState = FormWindowState.Normal;
                    Activate();
                    Show();
                };
                menu.Show();
            }
            catch
            {
                // Silencioso: no mostrar errores en UI; sólo limpiar
                txtContraseña.Clear();
            }
            finally
            {
                btnIngresar.Enabled = true;
            }
        }

        private void btnOlvideContraseña_Click(object sender, EventArgs e)
        {
            // Dejar tu implementación o silenciarla si preferís
        }

        private void cboIdioma_SelectedIndexChanged(object sender, EventArgs e)
        {
            var cb = sender as KryptonComboBox;
            var lang = (cb?.Text ?? "").Trim().StartsWith("EN", StringComparison.OrdinalIgnoreCase) ? "en" : "es";
            LanguageService.SetUICulture(lang);
            ResourceApplier.ApplyTo(this);
        }
    }
}
