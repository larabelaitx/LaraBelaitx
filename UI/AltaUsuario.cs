using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Krypton.Toolkit;
using System.Collections.Generic;
using BE;
using BLL.Contracts;
using UI.Seguridad; // Perms

namespace UI
{
    public partial class AltaUsuario : KryptonForm
    {
        private readonly ModoForm _modo;
        private readonly int? _idUsuario;
        private readonly IUsuarioService _svcUsuarios;
        private readonly IRolService _svcRoles;
        private readonly int? _currentUserId;

        private const string P_USU_ALTA = Perms.Usuario_Alta;
        private const string P_USU_EDITAR = Perms.Usuario_Editar;
        private const string P_USU_VER = Perms.Usuario_Listar;

        private ErrorProvider _ep;
        private bool _cargando;
        private Usuario _usuarioExistente;

        public AltaUsuario(
            ModoForm modo,
            int? idUsuario,
            IUsuarioService svcUsuarios,
            IRolService svcRoles,
            int? currentUserId = null)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterParent;

            _modo = modo;
            _idUsuario = idUsuario;
            _svcUsuarios = svcUsuarios ?? throw new ArgumentNullException(nameof(svcUsuarios));
            _svcRoles = svcRoles ?? throw new ArgumentNullException(nameof(svcRoles));
            _currentUserId = currentUserId;

            Load += AltaUsuario_Load;

            btnGuardar.Click += BtnGuardar_Click;
            btnVolver.Click += (s, e) => BtnVolver_Click(s, e);

            txtNombre.TextChanged += AutogenerarUsuario;
            txtApellido.TextChanged += AutogenerarUsuario;

            cboRol.DropDownStyle = ComboBoxStyle.DropDownList;
            cboEstado.DropDownStyle = ComboBoxStyle.DropDownList;

            // Enter confirma
            AcceptButton = btnGuardar;
        }

        private void AltaUsuario_Load(object sender, EventArgs e)
        {
            // Permisos por modo
            if (_modo == ModoForm.Alta && !CheckAllowed(P_USU_ALTA)) { Close(); return; }
            if (_modo == ModoForm.Editar && !CheckAllowed(P_USU_EDITAR)) { Close(); return; }
            if (_modo == ModoForm.Ver && !CheckAllowed(P_USU_VER)) { Close(); return; }

            _ep = new ErrorProvider(this);

            CargarCombos();

            if (_idUsuario.HasValue)
            {
                var u = _svcUsuarios.GetById(_idUsuario.Value);
                if (u != null) MapearBeAControles(u);
            }

            AplicarModo(_modo);
        }

        private bool CheckAllowed(string patente)
        {
            if (!_currentUserId.HasValue || _currentUserId.Value <= 0 ||
                !_svcRoles.TienePatente(_currentUserId.Value, patente))
            {
                KryptonMessageBox.Show(this,
                    $"No tenés permiso para acceder a esta funcionalidad.\n({patente})",
                    "Acceso denegado",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void CargarCombos()
        {
            var roles = (_svcRoles.ListarRoles() ?? Enumerable.Empty<BE.Familia>()).ToList();
            cboRol.DisplayMember = "Name";
            cboRol.ValueMember = "Id";
            cboRol.DataSource = roles;

            cboEstado.Items.Clear();
            cboEstado.Items.Add("Activo");
            cboEstado.Items.Add("Inactivo");
            cboEstado.SelectedIndex = 0;
        }

        private void AplicarModo(ModoForm modo)
        {
            Text = modo == ModoForm.Alta ? "Alta de Usuario"
                 : modo == ModoForm.Editar ? "Editar Usuario"
                 : "Ver Usuario";

            bool lectura = (modo == ModoForm.Ver);
            foreach (Control c in Controls) c.Enabled = !lectura;
            btnVolver.Enabled = true;
            btnGuardar.Visible = !lectura;
            btnGuardar.Enabled = !lectura;
        }

        private void AutogenerarUsuario(object sender, EventArgs e) => AutogenerarUsuario();
        private void AutogenerarUsuario()
        {
            if (_cargando) return;
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtApellido.Text)) return;

            string n = txtNombre.Text.Trim();
            string a = txtApellido.Text.Trim();
            string us = (n[0] + a).ToLower();
            if (us.Length > 40) us = us.Substring(0, 40);
            txtUsuario.Text = us;
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

            // Rol por familias del usuario
            var fams = _svcRoles.GetFamiliasUsuario(u.Id) ?? new List<Familia>();
            var famPrincipal = fams.FirstOrDefault();

            if (cboRol.ValueMember != "Id")
            {
                cboRol.DisplayMember = "Name";
                cboRol.ValueMember = "Id";
            }

            if (famPrincipal != null)
            {
                cboRol.SelectedValue = famPrincipal.Id;
                if (cboRol.SelectedIndex < 0 && cboRol.Items.Count > 0)
                    cboRol.SelectedIndex = 0;
            }
            else if (cboRol.Items.Count > 0)
            {
                cboRol.SelectedIndex = 0;
            }

            cboEstado.SelectedItem =
                (u.EstadoUsuarioId == EstadosUsuario.Habilitado) ? "Activo" : "Inactivo";

            _cargando = false;
        }

        private Usuario MapearControlesABe()
        {
            return new Usuario
            {
                Id = _idUsuario ?? 0,
                Name = (txtNombre.Text ?? "").Trim(),
                LastName = (txtApellido.Text ?? "").Trim(),
                Documento = (txtDocumento.Text ?? "").Trim(),
                Email = (txtEmail.Text ?? "").Trim(),
                UserName = (txtUsuario.Text ?? "").Trim(),
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

            if (ok && !Regex.IsMatch(txtEmail.Text ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase))
            { _ep.SetError(txtEmail, "Formato de email inválido."); ok = false; }

            if (ok && cboRol.SelectedItem == null)
            { _ep.SetError(cboRol, "Debe seleccionar un rol."); ok = false; }

            if (ok && cboEstado.SelectedItem == null)
            { _ep.SetError(cboEstado, "Debe seleccionar el estado."); ok = false; }

            if (!ok) return false;

            // Unicidad (usuario/email/documento)
            var actual = _idUsuario.HasValue ? _svcUsuarios.GetById(_idUsuario.Value) : null;
            string user = (txtUsuario.Text ?? "").Trim();
            string mail = (txtEmail.Text ?? "").Trim();
            string doc = (txtDocumento.Text ?? "").Trim();

            if (actual == null || !string.Equals(actual.UserName ?? "", user, StringComparison.OrdinalIgnoreCase))
                if (_svcUsuarios.ExisteUsername(user)) { _ep.SetError(txtUsuario, "El nombre de usuario ya existe."); ok = false; }

            if (actual == null || !string.Equals(actual.Email ?? "", mail, StringComparison.OrdinalIgnoreCase))
                if (_svcUsuarios.ExisteEmail(mail)) { _ep.SetError(txtEmail, "El email ya está registrado."); ok = false; }

            if (actual == null || !string.Equals(actual.Documento ?? "", doc, StringComparison.OrdinalIgnoreCase))
                if (_svcUsuarios.ExisteDocumento(doc)) { _ep.SetError(txtDocumento, "El documento ya está registrado."); ok = false; }

            return ok;
        }

        private static bool Requerido(Control c, string msg)
        {
            if (c is KryptonTextBox t && string.IsNullOrWhiteSpace(t.Text))
            {
                if (t.FindForm() is AltaUsuario frm) frm._ep.SetError(t, msg);
                return false;
            }
            return true;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (_modo == ModoForm.Alta && !CheckAllowed(P_USU_ALTA)) return;
            if (_modo == ModoForm.Editar && !CheckAllowed(P_USU_EDITAR)) return;

            if (!ValidarFormulario()) return;

            var be = MapearControlesABe();

            // Rol/familia seleccionada
            int famSelId = 0;
            try { famSelId = Convert.ToInt32(cboRol.SelectedValue); } catch { famSelId = 0; }
            if ((_modo == ModoForm.Alta || _modo == ModoForm.Editar) && famSelId <= 0)
            {
                _ep.SetError(cboRol, "Debe seleccionar un rol.");
                return;
            }

            try
            {
                if (_modo == ModoForm.Alta)
                {
                    // Contraseña temporal
                    string temp = Services.Crypto.GenPassword(); // o Guid.NewGuid().ToString("N").Substring(0,10)

                    // Crear usuario (devuelve Id)
                    int newId = _svcUsuarios.CrearConPassword(be, temp);
                    if (newId <= 0) throw new Exception("No se pudo crear el usuario.");

                    var creado = _svcUsuarios.GetById(newId) ?? throw new Exception("No se pudo recuperar el usuario creado.");

                    // Asignar familia/rol
                    _svcRoles.SetFamiliasDeUsuario(creado.Id, new[] { famSelId });

                    // Bitácora (opcional)
                    BLL.Bitacora.Info(_currentUserId,
                        $"Alta de usuario '{creado.UserName}' (Id={creado.Id}).",
                        "Usuarios", "Usuario_Alta", host: Environment.MachineName);
                    BLL.Bitacora.Info(_currentUserId,
                        $"Asignación de rol (FamiliaId={famSelId}) a UsuarioId={creado.Id}.",
                        "Usuarios", "Usuario_AsignarFamilia", host: Environment.MachineName);

                    // (Opcional) Exportar credenciales temporales
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("ITX – Alta de Usuario");
                    sb.AppendLine("=====================");
                    sb.AppendLine($"Fecha:     {DateTime.Now:yyyy-MM-dd HH:mm}");
                    sb.AppendLine($"Usuario:   {creado.UserName}");
                    if (!string.IsNullOrWhiteSpace(creado.Email)) sb.AppendLine($"Email:     {creado.Email}");
                    if (!string.IsNullOrWhiteSpace(creado.Documento)) sb.AppendLine($"Documento: {creado.Documento}");
                    sb.AppendLine();
                    sb.AppendLine("Contraseña temporal:");
                    sb.AppendLine($"    {temp}");
                    sb.AppendLine();
                    sb.AppendLine("Al iniciar sesión se pedirá cambiarla por una nueva.");

                    using (var sfd = new SaveFileDialog
                    {
                        Filter = "Archivo de texto|*.txt",
                        FileName = $"AltaUsuario_{creado.UserName}_{DateTime.Now:yyyyMMdd_HHmm}.txt"
                    })
                    {
                        if (sfd.ShowDialog(this) == DialogResult.OK)
                            System.IO.File.WriteAllText(sfd.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                        else
                            try { Clipboard.SetText(sb.ToString()); } catch { /* ignore */ }
                    }

                    DialogResult = DialogResult.OK;
                    Close();
                }
                else if (_modo == ModoForm.Editar)
                {
                    // Asegurar Id
                    if (!_idUsuario.HasValue || _idUsuario.Value <= 0)
                        throw new InvalidOperationException("Id de usuario inválido para edición.");

                    be.Id = _idUsuario.Value;

                    // Persistir datos
                    _svcUsuarios.Actualizar(be);

                    // Reasignar familia/rol (reemplaza set existente)
                    _svcRoles.SetFamiliasDeUsuario(be.Id, new[] { famSelId });

                    // Bitácora (opcional)
                    BLL.Bitacora.Info(_currentUserId,
                        $"Edición de usuario '{be.UserName}' (Id={be.Id}).",
                        "Usuarios", "Usuario_Editar", host: Environment.MachineName);
                    BLL.Bitacora.Info(_currentUserId,
                        $"Asignación de rol (FamiliaId={famSelId}) a UsuarioId={be.Id}.",
                        "Usuarios", "Usuario_AsignarFamilia", host: Environment.MachineName);

                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    // Ver: no hace nada
                    return;
                }
            }
            catch (Exception ex)
            {
                BLL.Bitacora.Error(_currentUserId, $"AltaUsuario Guardar: {ex.Message}",
                    "UI", "Usuario_Guardar", host: Environment.MachineName);

                KryptonMessageBox.Show(this,
                    $"Ocurrió un error en la operación.\n\nDetalle: {ex.Message}",
                    "Usuarios",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }

        private void BtnVolver_Click(object sender, EventArgs e)
        {
            if (HayDatosCargados() && _modo != ModoForm.Ver)
            {
                var r = KryptonMessageBox.Show(this,
                    "Los datos ingresados se perderán. ¿Volver sin guardar?",
                    "Confirmar",
                    KryptonMessageBoxButtons.YesNo, KryptonMessageBoxIcon.Warning);
                if (r == DialogResult.No) return;
            }
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool HayDatosCargados()
        {
            return new[]
            {
                txtNombre.Text, txtApellido.Text, txtDocumento.Text, txtEmail.Text, txtUsuario.Text
            }.Any(x => !string.IsNullOrWhiteSpace(x));
        }
    }
}
