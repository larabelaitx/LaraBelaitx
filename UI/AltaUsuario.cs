using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Krypton.Toolkit;
using BE;
using BLL.Contracts;
using Services;

namespace UI
{
    public partial class AltaUsuario : KryptonForm
    {
        private readonly ModoForm _modo;
        private readonly int? _idUsuario;
        private readonly IUsuarioService _svcUsuarios;
        private readonly IRolService _svcRoles;

        private ErrorProvider _ep;
        private bool _cargando;
        private Usuario _usuarioExistente;

        public AltaUsuario(ModoForm modo, int? idUsuario, IUsuarioService svcUsuarios, IRolService svcRoles)
        {
            InitializeComponent();

            _modo = modo;
            _idUsuario = idUsuario;
            _svcUsuarios = svcUsuarios ?? throw new ArgumentNullException(nameof(svcUsuarios));
            _svcRoles = svcRoles ?? throw new ArgumentNullException(nameof(svcRoles));

            this.Load += AltaUsuario_Load;
            if (btnGuardar != null) btnGuardar.Click += btnGuardar_Click;
            if (btnVolver != null) btnVolver.Click += btnVolver_Click;
            if (txtNombre != null) txtNombre.TextChanged += AutogenerarUsuario;
            if (txtApellido != null) txtApellido.TextChanged += AutogenerarUsuario;
        }

        private void btnGuardar_Click(object sender, EventArgs e) => BtnGuardar_Click(sender, e);
        private void btnVolver_Click(object sender, EventArgs e) => BtnVolver_Click(sender, e);
        private void AutogenerarUsuario(object sender, EventArgs e) => AutogenerarUsuario();
        private void CargarCombos(object sender, EventArgs e) => CargarCombos();
        private void MapearBeAControles(object sender, EventArgs e)
        {
            if (_idUsuario.HasValue)
            {
                var u = _svcUsuarios.GetById(_idUsuario.Value);
                if (u != null) MapearBeAControles(u);
            }
        }
        private void AplicarModo(object sender, EventArgs e) => AplicarModo(_modo);

        private void AltaUsuario_Load(object sender, EventArgs e)
        {
            _ep = new ErrorProvider(this);

            try
            {
                CargarCombos();

                if (_idUsuario.HasValue)
                {
                    var u = _svcUsuarios.GetById(_idUsuario.Value);
                    if (u != null) MapearBeAControles(u);
                }

                AplicarModo(_modo);
            }
            catch (Exception ex)
            {
                BLL.Bitacora.Error(null, $"Error cargando AltaUsuario: {ex.Message}",
                    "UI", "AltaUsuario_Load", host: Environment.MachineName);

                KryptonMessageBox.Show(this,
                    "Error cargando la pantalla:\n" + ex.Message,
                    "Usuarios",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
                Close();
            }
        }

        private void CargarCombos()
        {
            var roles = _svcRoles.ListarRoles() ?? Enumerable.Empty<BE.Permiso>();
            cboRol.DisplayMember = "Name";
            cboRol.ValueMember = "Id";
            cboRol.DataSource = roles.ToList();

            cboEstado.Items.Clear();
            cboEstado.Items.Add("Activo");   // Habilitado
            cboEstado.Items.Add("Inactivo"); // Baja
            cboEstado.SelectedIndex = 0;
        }

        private void AplicarModo(ModoForm modo)
        {
            Text = modo == ModoForm.Alta ? "Alta de Usuario"
                 : modo == ModoForm.Editar ? "Editar Usuario"
                 : "Ver Usuario";

            bool lectura = (modo == ModoForm.Ver);

            foreach (Control c in Controls) c.Enabled = !lectura;
            if (btnVolver != null) btnVolver.Enabled = true;

            if (lectura && btnGuardar != null)
            {
                btnGuardar.Enabled = false;
                btnGuardar.Visible = false;
            }
        }

        private void AutogenerarUsuario()
        {
            if (_cargando) return;
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtApellido.Text)) return;

            string n = txtNombre.Text.Trim();
            string a = txtApellido.Text.Trim();
            string username = (n[0] + a).ToLower();

            if (username.Length > 40) username = username.Substring(0, 40);
            txtUsuario.Text = username;
        }

        private void MapearBeAControles(Usuario u)
        {
            _cargando = true;
            _usuarioExistente = u;

            txtNombre.Text = u.Name;
            txtApellido.Text = u.LastName;
            txtDocumento.Text = u.Documento;
            txtEmail.Text = u.Email;
            txtUsuario.Text = u.UserName;

            if (cboRol.ValueMember == "Id") cboRol.SelectedValue = u.RolId;
            cboEstado.SelectedItem = u.IsEnabled ? "Activo" : "Inactivo";

            _cargando = false;
        }

        private Usuario MapearControlesABe()
        {
            return new Usuario
            {
                Id = _idUsuario ?? 0,
                Name = txtNombre.Text.Trim(),
                LastName = txtApellido.Text.Trim(),
                Documento = txtDocumento.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                UserName = txtUsuario.Text.Trim(),
                RolId = (int)(cboRol.SelectedValue ?? 0),
                EstadoUsuarioId = (cboEstado.SelectedItem?.ToString() == "Activo")
                    ? EstadosUsuario.Habilitado
                    : EstadosUsuario.Baja
            };
        }

        private bool ValidarFormulario()
        {
            _ep.Clear();
            bool ok = true;

            ok &= Requerido(txtNombre, "El nombre es obligatorio.");
            ok &= Requerido(txtApellido, "El apellido es obligatorio.");
            ok &= Requerido(txtDocumento, "El documento es obligatorio.");
            ok &= Requerido(txtEmail, "El email es obligatorio.");
            ok &= Requerido(txtUsuario, "El usuario es obligatorio.");

            if (ok && !EsEmailValido(txtEmail.Text))
            {
                _ep.SetError(txtEmail, "Formato de email inválido.");
                ok = false;
            }

            if (ok && cboRol.SelectedItem == null)
            {
                _ep.SetError(cboRol, "Debe seleccionar un rol.");
                ok = false;
            }
            if (ok && cboEstado.SelectedItem == null)
            {
                _ep.SetError(cboEstado, "Debe seleccionar el estado.");
                ok = false;
            }

            if (!ok) return false;

            var actual = _idUsuario.HasValue ? _svcUsuarios.GetById(_idUsuario.Value) : null;

            string nuevoUser = (txtUsuario.Text ?? "").Trim();
            string nuevoMail = (txtEmail.Text ?? "").Trim();
            string nuevoDoc = (txtDocumento.Text ?? "").Trim();

            if (actual == null || !string.Equals(actual.UserName ?? "", nuevoUser, StringComparison.OrdinalIgnoreCase))
            {
                if (_svcUsuarios.ExisteUsername(nuevoUser))
                {
                    _ep.SetError(txtUsuario, "El nombre de usuario ya existe.");
                    ok = false;
                }
            }

            if (actual == null || !string.Equals(actual.Email ?? "", nuevoMail, StringComparison.OrdinalIgnoreCase))
            {
                if (_svcUsuarios.ExisteEmail(nuevoMail))
                {
                    _ep.SetError(txtEmail, "El email ya está registrado.");
                    ok = false;
                }
            }

            if (actual == null || !string.Equals(actual.Documento ?? "", nuevoDoc, StringComparison.OrdinalIgnoreCase))
            {
                if (_svcUsuarios.ExisteDocumento(nuevoDoc))
                {
                    _ep.SetError(txtDocumento, "El documento ya está registrado.");
                    ok = false;
                }
            }

            return ok;
        }

        private static bool Requerido(Control c, string msg)
        {
            if (c is KryptonTextBox t && string.IsNullOrWhiteSpace(t.Text))
            {
                var ep = ((AltaUsuario)t.FindForm())._ep;
                ep.SetError(t, msg);
                return false;
            }
            return true;
        }

        private static bool EsEmailValido(string email) =>
            !string.IsNullOrWhiteSpace(email) &&
            Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            var be = MapearControlesABe();

            try
            {
                if (_modo == ModoForm.Alta)
                {
                    string temp = Crypto.GenPassword();
                    var okAlta = _svcUsuarios.CrearConPassword(be, temp);

                    if (!okAlta)
                        throw new Exception("No se pudo crear el usuario.");

                    using (var sfd = new SaveFileDialog
                    {
                        Filter = "Archivo de texto|*.txt",
                        FileName = $"Credenciales_{be.UserName}_{DateTime.Now:yyyyMMdd_HHmm}.txt"
                    })
                    {
                        if (sfd.ShowDialog(this) == DialogResult.OK)
                        {
                            var sb = new System.Text.StringBuilder();
                            sb.AppendLine("ITX – Credenciales de acceso");
                            sb.AppendLine("============================");
                            sb.AppendLine($"Usuario:   {be.UserName}");
                            if (!string.IsNullOrWhiteSpace(be.Email)) sb.AppendLine($"Email:     {be.Email}");
                            if (!string.IsNullOrWhiteSpace(be.Documento)) sb.AppendLine($"Documento: {be.Documento}");
                            sb.AppendLine();
                            sb.AppendLine("Clave temporal (sólo primer inicio):");
                            sb.AppendLine($"    {temp}");
                            sb.AppendLine();
                            sb.AppendLine("Al iniciar sesión se te pedirá cambiarla por una nueva.");
                            System.IO.File.WriteAllText(sfd.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                        }
                    }

                    KryptonMessageBox.Show("Usuario creado correctamente.", "Usuarios",
                        KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                }
                else
                {
                    _svcUsuarios.Actualizar(be);
                    KryptonMessageBox.Show("Usuario actualizado correctamente.", "Usuarios",
                        KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                BLL.Bitacora.Error(null, $"AltaUsuario Guardar: {ex.Message}",
                    "UI", "Usuario_Guardar", host: Environment.MachineName);

                KryptonMessageBox.Show(
                    $"Ocurrió un error en la operación.\n\nDetalle: {ex.Message}",
                    "Usuarios",
                    KryptonMessageBoxButtons.OK,
                    KryptonMessageBoxIcon.Error);
            }
        }

        private void BtnVolver_Click(object sender, EventArgs e)
        {
            if (HayDatosCargados() && _modo != ModoForm.Ver)
            {
                var r = KryptonMessageBox.Show(
                    "Los datos ingresados se perderán. ¿Volver sin guardar?",
                    "Confirmar",
                    KryptonMessageBoxButtons.YesNo,
                    KryptonMessageBoxIcon.Warning);
                if (r == DialogResult.No) return;
            }
            Close();
        }

        private bool HayDatosCargados()
        {
            return new[]
            {
                txtNombre.Text, txtApellido.Text, txtDocumento.Text, txtEmail.Text, txtUsuario.Text
            }.Any(x => !string.IsNullOrWhiteSpace(x));
        }

        private void panel1_Paint(object sender, PaintEventArgs e) { }
    }
}
