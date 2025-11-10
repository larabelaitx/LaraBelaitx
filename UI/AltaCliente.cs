using System;
using System.Windows.Forms;
using Krypton.Toolkit;
using BE;
using BLL.Contracts;
using BLL.Services;
using UI.Common; // ModoForm
using UI.Seguridad; // Perms

namespace UI
{
    public partial class AltaCliente : KryptonForm
    {
        private readonly IClienteService _service;
        private readonly IRolService _roles;
        private readonly int? _currentUserId;

        private Cliente _cliente;
        public ModoForm Modo { get; private set; } = ModoForm.Alta;
        public int? IdCliente { get; private set; }

        private const string P_CLIENTE_ALTA = Perms.Cliente_Alta;
        private const string P_CLIENTE_EDITAR = Perms.Cliente_Editar;
        private const string P_CLIENTE_VER = Perms.Cliente_Listar;

        public AltaCliente()
            : this(new ClienteService(), new RolService(), ModoForm.Alta, null, null, null) { }

        public AltaCliente(IClienteService service, ModoForm modo, int? idCliente = null, Cliente cliente = null, int? currentUserId = null)
            : this(service, new RolService(), modo, idCliente, cliente, currentUserId) { }

        public AltaCliente(IClienteService service, IRolService roles, ModoForm modo,
                           int? idCliente = null, Cliente cliente = null, int? currentUserId = null)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterParent;

            _service = service ?? throw new ArgumentNullException(nameof(service));
            _roles = roles ?? throw new ArgumentNullException(nameof(roles));
            _currentUserId = currentUserId;

            Modo = modo;
            IdCliente = idCliente;
            _cliente = cliente;

            Load += AltaCliente_Load;
            btnGuardar.Click += btnGuardar_Click;
            btnVolver.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            cboEstadoCivil.DropDownStyle = ComboBoxStyle.DropDownList;
            cboSituacionFiscal.DropDownStyle = ComboBoxStyle.DropDownList;
            cboEsPep.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void AltaCliente_Load(object sender, EventArgs e)
        {
            if (Modo == ModoForm.Alta && !CheckAllowed(P_CLIENTE_ALTA)) { Close(); return; }
            if (Modo == ModoForm.Editar && !CheckAllowed(P_CLIENTE_EDITAR)) { Close(); return; }
            if (Modo == ModoForm.Ver && !CheckAllowed(P_CLIENTE_VER)) { Close(); return; }

            try
            {
                CargarCombos();

                if (_cliente == null && IdCliente.HasValue && IdCliente.Value > 0)
                    _cliente = _service.GetById(IdCliente.Value);

                if (_cliente != null) CargarDesdeEntidad(_cliente);
                else SetDefaults();

                AjustarUIxModo();
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(this, "No se pudo cargar la pantalla.\n\n" + ex.Message,
                    "Error", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private bool CheckAllowed(string patente)
        {
            if (!_currentUserId.HasValue || _currentUserId.Value <= 0 || !_roles.TienePatente(_currentUserId.Value, patente))
            {
                KryptonMessageBox.Show(this,
                    $"No tenés permiso para acceder a esta funcionalidad.\n({patente})",
                    "Acceso denegado", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void CargarCombos()
        {
            cboEstadoCivil.Items.Clear();
            cboEstadoCivil.Items.AddRange(new object[] { "Soltero/a", "Casado/a", "Divorciado/a", "Viudo/a", "Unión convivencial" });

            cboSituacionFiscal.Items.Clear();
            cboSituacionFiscal.Items.AddRange(new object[] { "Monotributo", "Responsable Inscripto", "Exento", "Consumidor Final", "No Responsable" });

            cboEsPep.Items.Clear();
            cboEsPep.Items.AddRange(new object[] { "No", "Sí" });
        }

        private void SetDefaults()
        {
            dtpFechaNacimiento.Value = new DateTime(1990, 1, 1);
            if (cboEstadoCivil.Items.Count > 0) cboEstadoCivil.SelectedIndex = 0;
            if (cboSituacionFiscal.Items.Count > 0) cboSituacionFiscal.SelectedIndex = 0;
            if (cboEsPep.Items.Count > 0) cboEsPep.SelectedIndex = 0;
        }

        private void AjustarUIxModo()
        {
            switch (Modo)
            {
                case ModoForm.Alta:
                    Text = "ITX - Alta de Cliente";
                    AcceptButton = btnGuardar;
                    btnGuardar.Visible = true;
                    HabilitarCampos(true);
                    break;
                case ModoForm.Editar:
                    Text = "ITX - Editar Cliente";
                    AcceptButton = btnGuardar;
                    btnGuardar.Visible = true;
                    HabilitarCampos(true);
                    break;
                case ModoForm.Ver:
                    Text = "ITX - Ver Cliente";
                    btnGuardar.Visible = false;
                    HabilitarCampos(false);
                    break;
            }
        }

        private void HabilitarCampos(bool enabled)
        {
            txtNombre.ReadOnly = !enabled;
            txtApellido.ReadOnly = !enabled;
            dtpFechaNacimiento.Enabled = enabled;
            txtLugarNacimiento.ReadOnly = !enabled;
            txtNacionalidad.ReadOnly = !enabled;
            cboEstadoCivil.Enabled = enabled;
            txtDocumento.ReadOnly = !enabled;
            txtCUIT.ReadOnly = !enabled;
            txtDomicilio.ReadOnly = !enabled;
            txtTelefono.ReadOnly = !enabled;
            txtCorreo.ReadOnly = !enabled;
            txtOcupacion.ReadOnly = !enabled;
            cboSituacionFiscal.Enabled = enabled;
            cboEsPep.Enabled = enabled;
        }

        private void CargarDesdeEntidad(Cliente c)
        {
            if (c == null) return;
            txtNombre.Text = c.Nombre ?? "";
            txtApellido.Text = c.Apellido ?? "";
            dtpFechaNacimiento.Value = c.FechaNacimiento == default ? DateTime.Today : c.FechaNacimiento;
            txtLugarNacimiento.Text = c.LugarNacimiento ?? "";
            txtNacionalidad.Text = c.Nacionalidad ?? "";
            cboEstadoCivil.SelectedItem = string.IsNullOrWhiteSpace(c.EstadoCivil) ? null : (object)c.EstadoCivil;
            txtDocumento.Text = c.DocumentoIdentidad ?? "";
            txtCUIT.Text = c.CUITCUILCDI ?? "";
            txtDomicilio.Text = c.Domicilio ?? "";
            txtTelefono.Text = c.Telefono ?? "";
            txtCorreo.Text = c.CorreoElectronico ?? "";
            txtOcupacion.Text = c.Ocupacion ?? "";
            cboSituacionFiscal.SelectedItem = string.IsNullOrWhiteSpace(c.SituacionFiscal) ? null : (object)c.SituacionFiscal;
            cboEsPep.SelectedItem = c.EsPEP ? "Sí" : "No";
        }

        private void VolcarAEntidad(Cliente dest)
        {
            dest.Nombre = (txtNombre.Text ?? "").Trim();
            dest.Apellido = (txtApellido.Text ?? "").Trim();
            dest.FechaNacimiento = dtpFechaNacimiento.Value.Date;
            dest.LugarNacimiento = (txtLugarNacimiento.Text ?? "").Trim();
            dest.Nacionalidad = (txtNacionalidad.Text ?? "").Trim();
            dest.EstadoCivil = cboEstadoCivil.SelectedItem?.ToString();
            dest.DocumentoIdentidad = (txtDocumento.Text ?? "").Trim();
            dest.CUITCUILCDI = (txtCUIT.Text ?? "").Trim();
            dest.Domicilio = (txtDomicilio.Text ?? "").Trim();
            dest.Telefono = (txtTelefono.Text ?? "").Trim();
            dest.CorreoElectronico = (txtCorreo.Text ?? "").Trim();
            dest.Ocupacion = (txtOcupacion.Text ?? "").Trim();
            dest.SituacionFiscal = cboSituacionFiscal.SelectedItem?.ToString();
            dest.EsPEP = (cboEsPep.SelectedItem?.ToString() ?? "No") == "Sí";
        }

        private bool Validar()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            { KryptonMessageBox.Show(this, "Nombre es obligatorio."); txtNombre.Focus(); return false; }

            if (string.IsNullOrWhiteSpace(txtApellido.Text))
            { KryptonMessageBox.Show(this, "Apellido es obligatorio."); txtApellido.Focus(); return false; }

            if (string.IsNullOrWhiteSpace(txtDocumento.Text))
            { KryptonMessageBox.Show(this, "Documento es obligatorio."); txtDocumento.Focus(); return false; }

            if (!string.IsNullOrWhiteSpace(txtCorreo.Text) && !txtCorreo.Text.Contains("@"))
            { KryptonMessageBox.Show(this, "Correo electrónico no válido."); txtCorreo.Focus(); return false; }

            return true;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (Modo == ModoForm.Alta && !CheckAllowed(P_CLIENTE_ALTA)) return;
            if (Modo == ModoForm.Editar && !CheckAllowed(P_CLIENTE_EDITAR)) return;

            try
            {
                if (!Validar()) return;

                if (Modo == ModoForm.Alta)
                {
                    var nuevo = new Cliente();
                    VolcarAEntidad(nuevo);

                    var id = _service.Crear(nuevo);
                    if (id > 0)
                    {
                        // Bitácora
                        BLL.Bitacora.Info(_currentUserId,
                            $"Cliente creado: {nuevo.Apellido}, {nuevo.Nombre} (Doc: {nuevo.DocumentoIdentidad})",
                            "Clientes", "Crear", host: Environment.MachineName);

                        KryptonMessageBox.Show(this, "Cliente creado con éxito.", "OK");
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        KryptonMessageBox.Show(this, "No se pudo crear el cliente.", "Aviso");
                    }
                }
                else if (Modo == ModoForm.Editar)
                {
                    if (_cliente == null)
                    {
                        KryptonMessageBox.Show(this, "No hay cliente cargado para modificar.");
                        return;
                    }

                    VolcarAEntidad(_cliente);
                    var ok = _service.Actualizar(_cliente);
                    if (ok)
                    {
                        // Bitácora
                        BLL.Bitacora.Info(_currentUserId,
                            $"Cliente actualizado: {_cliente.Apellido}, {_cliente.Nombre} (Id: {_cliente.IdCliente})",
                            "Clientes", "Actualizar", host: Environment.MachineName);

                        KryptonMessageBox.Show(this, "Cliente actualizado.", "OK");
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        KryptonMessageBox.Show(this, "No se pudo actualizar el cliente.", "Aviso");
                    }
                }
                else
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(this, $"Error al guardar: {ex.Message}", "Error",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e) { }
    }
}
