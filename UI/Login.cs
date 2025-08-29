using System;
using System.Windows.Forms;
using Krypton.Toolkit;

namespace UI
{
    public partial class Login : KryptonForm
    {
        public Login()
        {
            InitializeComponent();
        }

        private void Login_Load(object sender, EventArgs e)
        {

        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string contraseña = txtContraseña.Text.Trim();

            if (usuario == "admin" && contraseña == "1234")
            {
                MessageBox.Show("Login exitoso", "Acceso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                Menu menu = new Menu();
                this.Hide();
                menu.ShowDialog();
                this.Close();
            }
            else
            {
                MessageBox.Show("Usuario o contraseña incorrectos", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtContraseña.Clear();
                txtContraseña.Focus();
            }
        }

        private void btnOlvideContraseña_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Funcionalidad para recuperar contraseña", "Recuperar", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
