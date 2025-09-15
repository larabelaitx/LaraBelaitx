using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Krypton.Toolkit;
using BLL;
using BE;
using BLL.Contracts; 

namespace UI
{
    public partial class AltaUsuario : KryptonForm
    {
        private readonly ModoForm _modo;
        private readonly int? _idUsuario;
        private readonly IUsuarioService _svcUsuarios;
        private readonly IRolService _svcRoles;

        private ErrorProvider _ep;
        private List<RolDto> _roles;   // DataSource de cboRol
        private BE.Usuario _usuarioExistente; // para Editar/Ver
        private bool _cargando;

        public AltaUsuario()
        {
            InitializeComponent();
        }

        public AltaUsuario(ModoForm modo, int? idUsuario, IUsuarioService svcUsuarios, IRolService svcRoles)
            : this()
        {
            _modo = modo;
            _idUsuario = idUsuario;
            _svcUsuarios = svcUsuarios ?? throw new ArgumentNullException(nameof(svcUsuarios));
            _svcRoles = svcRoles ?? throw new ArgumentNullException(nameof(svcRoles));
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            CargarCombos();

            if (_idUsuario.HasValue)
            {
                var u = _svcUsuarios.ObtenerPorId(_idUsuario.Value);
                if (u != null)
                    MapearBeAControles(u);
            }

            if (_modo == ModoForm.Ver)
            {
                // deshabilitar campos/botón Guardar
                btnGuardar.Enabled = false;
                foreach (Control c in this.Controls)
                    c.Enabled = false;
                btnVolver.Enabled = true;
            }
        }

        private void CargarCombos()
        {
            var roles = _svcRoles.ListarRoles() ?? Enumerable.Empty<BE.Permiso>();

            cboRol.DisplayMember = "Name";  
            cboRol.ValueMember = "Id";
            cboRol.DataSource = roles.ToList();

            cboEstado.Items.Clear();
            cboEstado.Items.Add("Activo");
            cboEstado.Items.Add("Inactivo");
            cboEstado.SelectedIndex = 0;
        }

        private void CablearEventos()
        {
            txtDocumento.KeyPress += (s, ev) =>
            {
                if (!char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar))
                    ev.Handled = true;
            };

            txtNombre.Leave += (s, ev) => AutogenerarUsuario();
            txtApellido.Leave += (s, ev) => AutogenerarUsuario();

            btnGuardar.Click += BtnGuardar_Click;
            btnVolver.Click += BtnVolver_Click;
        }

        private void AplicarModo(ModoForm modo)
        {
            Text = modo == ModoForm.Alta ? "Alta de Usuario"
                 : modo == ModoForm.Editar ? "Editar Usuario"
                 : "Ver Usuario";

            bool lectura = (modo == ModoForm.Ver);

            foreach (Control c in Controls.OfType<Control>())
                c.Enabled = !lectura;
            btnVolver.Enabled = true;

            if (lectura)
            {
                btnGuardar.Enabled = false;
                btnGuardar.Visible = false;
            }
        }

        private void AutogenerarUsuario()
        {
            if (_cargando) return;
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtApellido.Text))
                return;

            string n = txtNombre.Text.Trim();
            string a = txtApellido.Text.Trim();
            string username = (n[0].ToString() + a).ToLower();

            if (username.Length > 40)
                username = username.Substring(0, 40);

            txtUsuario.Text = username;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            var be = MapearControlesABe();

            try
            {
                if (_modo == ModoForm.Alta)
                {
                    _svcUsuarios.Crear(be);
                }
                else
                {
                    _svcUsuarios.Actualizar(be);
                }

                KryptonMessageBox.Show(_modo == ModoForm.Alta ? "Usuario creado correctamente." : "Usuario actualizado correctamente.", "Usuarios", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show($"Ocurrió un error en la operación.\n\nDetalle: {ex.Message}", "Usuarios", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }

        private void BtnVolver_Click(object sender, EventArgs e)
        {
            if (HayDatosCargados() && _modo != ModoForm.Ver)
            {
                var r = KryptonMessageBox.Show(
                    "Los datos ingresados se perderán. ¿Volver sin guardar?","Confirmar", KryptonMessageBoxButtons.YesNo, KryptonMessageBoxIcon.Warning);

                if (r == DialogResult.No) return;
            }
            Close();
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

            if (ok)
            {
                int? excludeId = _idUsuario;
                if (_svcUsuarios.ExisteUsername(txtUsuario.Text.Trim(), excludeId))
                {
                    _ep.SetError(txtUsuario, "El nombre de usuario ya existe.");
                    ok = false;
                }
                if (_svcUsuarios.ExisteEmail(txtEmail.Text.Trim(), excludeId))
                {
                    _ep.SetError(txtEmail, "El email ya está registrado.");
                    ok = false;
                }
            }

            return ok;
        }

        private bool Requerido(Control c, string msg)
        {
            if (c is KryptonTextBox t && string.IsNullOrWhiteSpace(t.Text))
            {
                _ep.SetError(t, msg);
                return false;
            }
            return true;
        }

        private bool EsEmailValido(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
        }

        private bool HayDatosCargados()
        {
            return new[] { txtNombre.Text, txtApellido.Text, txtDocumento.Text, txtEmail.Text, txtUsuario.Text }
                   .Any(x => !string.IsNullOrWhiteSpace(x));
        }

        private void MapearBeAControles(BE.Usuario u)
        {
            txtNombre.Text = u.Name;
            txtApellido.Text = u.LastName;
            txtDocumento.Text = u.Documento;
            txtEmail.Text = u.Email;
            txtUsuario.Text = u.UserName;

            if (cboRol.ValueMember == "Id") cboRol.SelectedValue = u.RolId;

            var estaActivo = u.Estado; 
            cboEstado.SelectedItem = estaActivo ? "Activo" : "Inactivo";
        }

        private BE.Usuario MapearControlesABe()
        {
            return new BE.Usuario
            {
                Id = _idUsuario ?? 0, // si tu BE usa 0 como "sin id", ajustá
                Name = txtNombre.Text.Trim(),
                LastName = txtApellido.Text.Trim(),
                Documento = txtDocumento.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                UserName = txtUsuario.Text.Trim(),
                RolId = (int)(cboRol.SelectedValue ?? 0),
                Estado = (cboEstado.SelectedItem?.ToString() == "Activo")
            };
        }

        private void AltaUsuario_Load(object sender, EventArgs e)
        {

        }
    }

    public class RolDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }

    public class UsuarioDto
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Documento { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public int RolId { get; set; }
        public bool Activo { get; set; }
    }
}
