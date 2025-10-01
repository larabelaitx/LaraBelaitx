using System;
using System.Windows.Forms;
using BLL.Services;
using BE;
using Krypton.Toolkit;
using BLL.Contracts; 

namespace UI
{
    public partial class AltaCuenta : KryptonForm
    {
        private readonly ICuentaService _svc;
        private readonly bool _modoConsulta;
        private Cuenta _cuenta;
        private readonly Cliente _cliente; 
        public AltaCuenta(ICuentaService svc, Cliente cliente)
        {
            InitializeComponent();
            _svc = svc ?? throw new ArgumentNullException(nameof(svc));
            _cliente = cliente ?? throw new ArgumentNullException(nameof(cliente));
            _modoConsulta = false;

            this.Load += Alta_Load;
            btnGuardar.Click += btnGuardar_Click;
            btnVolver.Click += (s, e) => Close();
        }
        public AltaCuenta(ICuentaService svc, int idCuenta, bool modoConsulta)
        {
            InitializeComponent();
            _svc = svc ?? throw new ArgumentNullException(nameof(svc));
            _modoConsulta = modoConsulta;

            this.Load += (s, e) => Edit_Load(idCuenta);
            btnGuardar.Click += btnGuardar_Click;
            btnVolver.Click += (s, e) => Close();
        }

        private void CargarCombos()
        {
            cboTipo.Items.Clear();
            cboTipo.Items.AddRange(CatalogosCuenta.TiposCuenta); 
            if (cboTipo.Items.Count > 0) cboTipo.SelectedIndex = 0;

            cboMoneda.Items.Clear();
            cboMoneda.Items.AddRange(CatalogosCuenta.Monedas); 
            if (cboMoneda.Items.Count > 0) cboMoneda.SelectedIndex = 0;
        }

        private void Alta_Load(object sender, EventArgs e)
        {
            CargarCombos();

            txtCliente.Text = $"{_cliente.Apellido}, {_cliente.Nombre} (Doc: {_cliente.DocumentoIdentidad})";

            txtNumero.Text = _svc.GenerarNumeroCuenta(_cliente.IdCliente); 
            txtCBU.Text = _svc.GenerarCBU();
            txtAlias.Text = _svc.GenerarAlias();

            txtNumero.ReadOnly = true;
            txtCBU.ReadOnly = true;
            txtAlias.ReadOnly = true;

            dtpApertura.Value = DateTime.Today;
        }

        private void Edit_Load(int idCuenta)
        {
            CargarCombos();

            _cuenta = _svc.GetById(idCuenta);
            if (_cuenta == null)
            {
                MessageBox.Show(this, "No se encontró la cuenta.", "Aviso");
                Close();
                return;
            }

            txtNumero.Text = _cuenta.NumeroCuenta;
            txtCBU.Text = _cuenta.CBU;
            txtAlias.Text = _cuenta.Alias;
            cboTipo.SelectedItem = _cuenta.TipoCuenta ?? "Caja de Ahorro";
            cboMoneda.SelectedItem = _cuenta.Moneda ?? "ARS";
            dtpApertura.Value = _cuenta.FechaApertura == default(DateTime) ? DateTime.Today : _cuenta.FechaApertura;

            txtNumero.ReadOnly = true;
            txtCBU.ReadOnly = true;
            txtAlias.ReadOnly = true;

            if (_modoConsulta)
            {
                cboTipo.Enabled = false;
                cboMoneda.Enabled = false;
                dtpApertura.Enabled = false;
                btnGuardar.Visible = false;
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (cboTipo.SelectedItem == null)
                {
                    MessageBox.Show(this, "Seleccioná el tipo de cuenta.", "Aviso");
                    return;
                }
                if (cboMoneda.SelectedItem == null)
                {
                    MessageBox.Show(this, "Seleccioná la moneda.", "Aviso");
                    return;
                }

                if (_cuenta == null)
                {
                    // Alta
                    var nueva = new Cuenta
                    {
                        ClienteId = _cliente.IdCliente,
                        NumeroCuenta = txtNumero.Text.Trim(),
                        CBU = txtCBU.Text.Trim(),
                        Alias = txtAlias.Text.Trim(),
                        TipoCuenta = cboTipo.SelectedItem.ToString(),
                        Moneda = cboMoneda.SelectedItem.ToString(),
                        FechaApertura = dtpApertura.Value.Date,
                        Estado = new BE.Estado { Id = 1, Name = "Abierta" }
                    };

                    var id = _svc.Crear(nueva);
                    if (id > 0)
                    {
                        MessageBox.Show(this, "Cuenta creada con éxito.", "OK");
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                        MessageBox.Show(this, "No se pudo crear la cuenta.", "Aviso");
                }
                else
                {
                    // Editar
                    _cuenta.TipoCuenta = cboTipo.SelectedItem.ToString();
                    _cuenta.Moneda = cboMoneda.SelectedItem.ToString();
                    _cuenta.FechaApertura = dtpApertura.Value.Date;

                    if (_svc.Actualizar(_cuenta))
                    {
                        MessageBox.Show(this, "Cuenta actualizada.", "OK");
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                        MessageBox.Show(this, "No se pudo actualizar la cuenta.", "Aviso");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error al guardar: " + ex.Message, "Error");
            }
        }

        private void AltaCuenta_Load(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
