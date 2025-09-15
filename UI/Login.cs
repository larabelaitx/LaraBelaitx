using System;
using System.Windows.Forms;
using Krypton.Toolkit;
using BLL.Services;     
using BE;              

namespace UI
{
    public partial class Login : KryptonForm
    {
        private readonly UsuarioService _usuarios = new UsuarioService();

        public Login()
        {
            InitializeComponent();
            this.AcceptButton = btnIngresar;
        }

        private void Login_Load(object sender, EventArgs e)
        {
            txtUsuario.Clear();
            txtContraseña.Clear();
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string contraseña = txtContraseña.Text;

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contraseña))
            {
                MessageBox.Show("Ingresá usuario y contraseña.", "Faltan datos",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (string.IsNullOrWhiteSpace(contraseña)) txtContraseña.Focus(); else txtUsuario.Focus();
                return;
            }

            btnIngresar.Enabled = false;

            try
            {
                var ok = _usuarios.Login(usuario, contraseña, out Usuario u);

                if (!ok)
                {
                    if (u != null && u.EstadoUsuarioId == EstadosUsuario.Bloqueado)
                    {
                        MessageBox.Show("El usuario está bloqueado por intentos fallidos.",
                            "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                    else
                    {
                        MessageBox.Show("Usuario o contraseña incorrectos.",
                            "Error de acceso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    txtContraseña.Clear();
                    txtContraseña.Focus();
                    return;
                }

                MessageBox.Show($"¡Bienvenido {u?.NombreCompleto ?? usuario}!",
                    "Acceso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                var menu = new Menu(/* u */);
                this.Hide();
                menu.ShowDialog();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al intentar iniciar sesión:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnIngresar.Enabled = true;
            }
        }

        private void btnOlvideContraseña_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Funcionalidad para recuperar contraseña (pendiente).",
                "Recuperar", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
