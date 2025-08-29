using System;
using System.Windows.Forms;
using Krypton.Toolkit;
using BLL.Services;  
using BE;


namespace UI
{
    public partial class AltaCliente : KryptonForm
    {
        public enum FormMode { Alta, Edicion, Consulta }
        private readonly IClienteService _service;
        private Cliente _cliente;              
        public FormMode Modo { get; private set; } = FormMode.Alta;
        public int? IdCliente { get; private set; }
        public AltaCliente()
        {
            InitializeComponent();
            this.Load += AltaCliente_Load;
        }
        public AltaCliente(IClienteService service, FormMode modo, int? idCliente = null, Cliente cliente = null) : this()
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            Modo = modo;
            IdCliente = idCliente;
            _cliente = cliente;
            btnGuardar.Click += btnGuardar_Click;
            btnVolver.Click += btnVolver_Click;
        }
        private void AltaCliente_Load(object sender, EventArgs e)
        {
            try
            {
                CargarCombos();

                if (_cliente == null && IdCliente.HasValue && IdCliente.Value > 0)
                    _cliente = _service.GetById(IdCliente.Value);

                if (_cliente != null)
                    CargarDesdeEntidad(_cliente);
                else
                    SetDefaults();

                AjustarUIxModo();
            }
            catch (Exception)
            {
                KryptonMessageBox.Show(this, "Cliente creado con éxito.", "OK");
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void CargarCombos()
        {
            // Estado Civil
            cboEstadoCivil.Items.Clear();
            cboEstadoCivil.Items.AddRange(new object[]
            {
                "Soltero/a", "Casado/a", "Divorciado/a", "Viudo/a", "Unión convivencial"
            });

            // Situación Fiscal
            cboSituacionFiscal.Items.Clear();
            cboSituacionFiscal.Items.AddRange(new object[]
            {
                "Monotributo", "Responsable Inscripto", "Exento", "Consumidor Final", "No Responsable"
            });

            // Es PEP
            cboEsPep.Items.Clear();
            cboEsPep.Items.AddRange(new object[] { "No", "Sí" });
        }

        private void SetDefaults()
        {
            dtpFechaNacimiento.Value = new DateTime(1990, 1, 1);
            if (cboEstadoCivil.Items.Count > 0) cboEstadoCivil.SelectedIndex = 0;
            if (cboSituacionFiscal.Items.Count > 0) cboSituacionFiscal.SelectedIndex = 0;
            if (cboEsPep.Items.Count > 0) cboEsPep.SelectedIndex = 0; // "No"
        }

        private void AjustarUIxModo()
        {
            switch (Modo)
            {
                case FormMode.Alta:
                    Text = "ITX - Alta de Cliente";
                    this.AcceptButton = btnGuardar;
                    break;

                case FormMode.Edicion:
                    Text = "ITX - Editar Cliente";
                    this.AcceptButton = btnGuardar;
                    break;

                case FormMode.Consulta:
                    Text = "ITX - Ver Cliente";
                    btnGuardar.Visible = false;
                    DeshabilitarCampos();
                    break;
            }
        }

        private void DeshabilitarCampos()
        {
            txtNombre.ReadOnly = true;
            txtApellido.ReadOnly = true;
            dtpFechaNacimiento.Enabled = false;
            txtLugarNacimiento.ReadOnly = true;
            txtNacionalidad.ReadOnly = true;
            cboEstadoCivil.Enabled = false;
            txtDocumento.ReadOnly = true;
            txtCUIT.ReadOnly = true;
            txtDomicilio.ReadOnly = true;
            txtTelefono.ReadOnly = true;
            txtCorreo.ReadOnly = true;
            txtOcupacion.ReadOnly = true;
            cboSituacionFiscal.Enabled = false;
            cboEsPep.Enabled = false;
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
            try
            {
                if (!Validar()) return;

                if (Modo == FormMode.Alta)
                {
                    var nuevo = new Cliente();
                    VolcarAEntidad(nuevo);

                    var id = _service.Crear(nuevo); 
                    if (id > 0)
                    {
                        KryptonMessageBox.Show(this, "Cliente creado con éxito.");
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        KryptonMessageBox.Show(this, "No se pudo crear el cliente.");
                    }
                }
                else if (Modo == FormMode.Edicion)
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
                        KryptonMessageBox.Show(this, "Cliente actualizado.");
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        KryptonMessageBox.Show(this, "No se pudo actualizar el cliente.");
                    }
                }
                else
                {
                    // Consulta: no guarda
                    Close();
                }
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(this, $"Error al guardar: {ex.Message}");
            }
        }

        private void btnVolver_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
