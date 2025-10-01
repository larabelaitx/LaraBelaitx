using System;
using System.Windows.Forms;
using Krypton.Toolkit;
using BLL.Services;     
using BE;
using System.Linq;
using Services;


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

                if (u.DebeCambiarContraseña)
                {
                    var data1 = new KryptonInputBoxData
                    {
                        Caption = "Cambiar contraseña",
                        Prompt = "Ingresá tu nueva contraseña:",
                        DefaultResponse = ""
                    };
                    string p1 = KryptonInputBox.Show(data1);
                    if (string.IsNullOrWhiteSpace(p1))
                    {
                        MessageBox.Show("Operación cancelada. Debés establecer una nueva contraseña para continuar.",
                            "Cambio de contraseña", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    var data2 = new KryptonInputBoxData
                    {
                        Caption = "Cambiar contraseña",
                        Prompt = "Repetí la nueva contraseña:",
                        DefaultResponse = ""
                    };
                    string p2 = KryptonInputBox.Show(data2);

                    if (p1 != p2)
                    {
                        MessageBox.Show("Las contraseñas no coinciden.",
                            "Cambio de contraseña", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var hp = PasswordService.Hash(p1);
                    u.PasswordHash = hp.Hash;
                    u.PasswordSalt = hp.Salt;
                    u.PasswordIterations = hp.Iterations;
                    u.DebeCambiarContraseña = false;
                    u.Tries = 0; 

                    _usuarios.Actualizar(u);

                    MessageBox.Show("Contraseña actualizada correctamente.",
                        "Cambio de contraseña", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                MessageBox.Show($"¡Bienvenido {u?.NombreCompleto ?? usuario}!",
                    "Acceso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                var menu = new Menu(/* u si querés pasarlo */);
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
            try
            {
                string userOrMail = txtUsuario.Text.Trim();
                if (string.IsNullOrWhiteSpace(userOrMail))
                {
                    MessageBox.Show("Ingresá usuario o email en el campo Usuario para recuperar.", "Recuperar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtUsuario.Focus();
                    return;
                }

                var svc = new BLL.Services.UsuarioService();
                BE.Usuario u = svc.GetByUserName(userOrMail);
                if (u == null)
                {
                    var todos = svc.GetAll();
                    u = todos.FirstOrDefault(x =>
                        !string.IsNullOrWhiteSpace(x.Email) &&
                        string.Equals(x.Email.Trim(), userOrMail, StringComparison.OrdinalIgnoreCase));
                }

                if (u == null)
                {
                    MessageBox.Show("No se encontró el usuario.", "Recuperar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string temp = Crypto.GenPassword(); 
                var hp = PasswordService.Hash(temp);
                u.PasswordHash = hp.Hash;
                u.PasswordSalt = hp.Salt;
                u.PasswordIterations = hp.Iterations;
                u.DebeCambiarContraseña = true;
                u.Tries = 0;
                u.EstadoUsuarioId = BE.EstadosUsuario.Habilitado;

                var ok = svc.Actualizar(u);
                if (!ok) throw new Exception("No se pudo actualizar el usuario.");

                MessageBox.Show($"Contraseña temporal: {temp}\nAl ingresar se te pedirá cambiarla.", "Recuperación", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en la recuperación: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
