using System;
using System.Linq;
using System.Windows.Forms;
using Krypton.Toolkit;
using BLL.Services;
using BE;
using Services;
using DAL;
using System.IO;
using System.Text;

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

            try
            {
                var svc = new UsuarioService();
                var u = svc.GetByUserName("admin");

                if (u == null)
                {
                    var nuevo = new BE.Usuario
                    {
                        UserName = "admin",
                        Name = "Administrador",
                        LastName = "Sistema",
                        Email = null,
                        EstadoUsuarioId = BE.EstadosUsuario.Habilitado,
                        IdiomaId = 1,
                        Tries = 0
                        // DebeCambiarContraseña = true; // si querés forzar al primer login
                    };

                    // crea con "1234"
                    svc.CrearConPassword(nuevo, "1234");

                    // si NO querés forzar cambio al admin inicial:
                    var creado = svc.GetByUserName("admin");
                    if (creado != null)
                    {
                        creado.DebeCambiarContraseña = false; // poné true si querés forzar
                        svc.Actualizar(creado);
                    }

                    MessageBox.Show("✅ Usuario 'admin' creado con contraseña '1234'.",
                        "Semilla creada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("El usuario 'admin' ya existe.",
                        "Semilla", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al crear usuario admin:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string contraseña = txtContraseña.Text;
            string host = Environment.MachineName;

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
                var ok = _usuarios.Login(usuario, contraseña, out BE.Usuario u);

                if (!ok)
                {
                    BitacoraDao.GetInstance().AddLoginFail(usuario, null, host);

                    // Si el service ya lo bloqueó, avisamos explícito
                    if (u != null && u.EstadoUsuarioId == EstadosUsuario.Bloqueado)
                    {
                        MessageBox.Show(
                            "Tu usuario fue bloqueado por intentos fallidos.\nContactá a un administrador para desbloquearlo.",
                            "Usuario bloqueado",
                            MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                    else
                    {
                        // mostrar intentos restantes si tenemos el usuario
                        int usados = Math.Max(0, u?.Tries ?? 0);
                        int restantes = Math.Max(0, _usuarios.MaxTries - usados);

                        MessageBox.Show(
                            $"Usuario o contraseña incorrectos.\nIntentos restantes: {restantes}",
                            "Error de acceso",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    txtContraseña.Clear();
                    txtContraseña.Focus();
                    return;
                }

                // ---- Cambio de contraseña obligatorio ----
                if (u.DebeCambiarContraseña)
                {
                    using (var dlg = new ChangePasswordDialog())
                    {
                        if (dlg.ShowDialog(this) != DialogResult.OK)
                        {
                            // No aceptó el cambio -> quedate en login
                            return;
                        }

                        var hp = Services.PasswordService.Hash(dlg.NewPassword);
                        u.PasswordHash = hp.Hash;
                        u.PasswordSalt = hp.Salt;
                        u.PasswordIterations = hp.Iterations;
                        u.DebeCambiarContraseña = false;
                        u.Tries = 0;

                        _usuarios.Actualizar(u);

                        // Log opcional del cambio de clave
                        DAL.BitacoraDao.GetInstance().Add(u?.Id, "Seguridad", "PasswordChange", 1,
                            "Cambio de contraseña OK", null, host);

                        // === Punto clave de seguridad ===
                        // No seguimos con el ingreso. Forzamos re-login con la nueva clave.
                        KryptonMessageBox.Show(
                            "Contraseña actualizada correctamente.\nVolvé a iniciar sesión con tu nueva contraseña.",
                            "Cambio de contraseña",
                            KryptonMessageBoxButtons.OK,
                            KryptonMessageBoxIcon.Information);

                        txtUsuario.Text = u.UserName;   // lo dejamos precargado para que solo escriba la nueva clave
                        txtContraseña.Clear();
                        txtContraseña.Focus();
                        return; // <- no se abre el menú
                    }
                }

                // ---- Login OK (sin cambio pendiente) ----
                DAL.BitacoraDao.GetInstance().AddLoginOk(u?.Id, null, host);

                MessageBox.Show($"¡Bienvenido {u?.NombreCompleto ?? usuario}!",
                    "Acceso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                var menu = new Menu(u);
                this.Hide();

                // Al cerrar el menú, volvemos al Login (sin cerrar la app)
                menu.FormClosed += (s2, e2) =>
                {
                    txtContraseña.Clear();
          
                    this.WindowState = FormWindowState.Normal;
                    this.Activate();
                };

                menu.Show();
            }
            catch (Exception ex)
            {
                DAL.BitacoraDao.GetInstance().AddError("Seguridad", "Login", ex.Message, null, null, host);
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
                    MessageBox.Show(
                        "Ingresá usuario o email en el campo Usuario para recuperar.",
                        "Recuperar",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    txtUsuario.Focus();
                    return;
                }

                var svc = new UsuarioService();

                // 1) Buscar por username; si no, por email
                Usuario u = svc.GetByUserName(userOrMail);
                if (u == null)
                {
                    var todos = svc.GetAll();
                    u = todos.FirstOrDefault(x =>
                        !string.IsNullOrWhiteSpace(x.Email) &&
                        string.Equals(x.Email.Trim(), userOrMail, StringComparison.OrdinalIgnoreCase));
                }

                if (u == null)
                {
                    MessageBox.Show(
                        "No se encontró el usuario.",
                        "Recuperar",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // 2) Generar password temporal y forzar cambio
                string temp = Crypto.GenPassword();
                var hp = PasswordService.Hash(temp);
                u.PasswordHash = hp.Hash;
                u.PasswordSalt = hp.Salt;
                u.PasswordIterations = hp.Iterations;
                u.DebeCambiarContraseña = true;
                u.Tries = 0;
                u.EstadoUsuarioId = EstadosUsuario.Habilitado;

                var ok = svc.Actualizar(u);
                if (!ok) throw new Exception("No se pudo actualizar el usuario.");

                // 3) Descargar TXT con los datos (igual que en Alta)
                using (var sfd = new SaveFileDialog
                {
                    Filter = "Archivo de texto|*.txt",
                    FileName = $"Recuperacion_{u.UserName}_{DateTime.Now:yyyyMMdd_HHmm}.txt"
                })
                {
                    if (sfd.ShowDialog(this) == DialogResult.OK)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("ITX – Recuperación de contraseña");
                        sb.AppendLine("================================");
                        sb.AppendLine($"Usuario:   {u.UserName}");
                        if (!string.IsNullOrWhiteSpace(u.Email)) sb.AppendLine($"Email:     {u.Email}");
                        if (!string.IsNullOrWhiteSpace(u.Documento)) sb.AppendLine($"Documento: {u.Documento}");
                        sb.AppendLine();
                        sb.AppendLine("Contraseña temporal (válida hasta el próximo inicio):");
                        sb.AppendLine($"    {temp}");
                        sb.AppendLine();
                        sb.AppendLine("Al iniciar sesión se te pedirá cambiarla por una nueva.");
                        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    }
                }

                MessageBox.Show(
                    "Se generó una contraseña temporal.\nGuardá el archivo TXT con la información.",
                    "Recuperación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en la recuperación: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}