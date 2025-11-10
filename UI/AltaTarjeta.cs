using System;
using System.Linq;
using System.Windows.Forms;
using Krypton.Toolkit;
using BE;
using BLL.Contracts;

namespace UI
{
    public partial class AltaTarjeta : KryptonForm
    {
        private readonly ITarjetaService _svc;
        private readonly ICuentaService _ctas;
        private readonly IClienteService _clis;

        private Tarjeta _tarjeta;
        private bool _soloLectura;

        // Alta
        public AltaTarjeta(ITarjetaService svc, ICuentaService ctas, IClienteService clis)
        {
            InitializeComponent();
            _svc = svc; _ctas = ctas; _clis = clis;
            _soloLectura = false;

            Load += Alta_Load;
            btnGuardar.Click += btnGuardar_Click;
            btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            // Marca fija (sin combo)
            txtMarca.ReadOnly = true;
            txtMarca.Text = "Visa";
        }

        // Editar / Ver
        public AltaTarjeta(ITarjetaService svc, ICuentaService ctas, IClienteService clis, int idTarjeta, bool modoSoloLectura)
        {
            InitializeComponent();
            _svc = svc; _ctas = ctas; _clis = clis;
            _soloLectura = modoSoloLectura;

            Load += (s, e) => Edit_Load(idTarjeta);
            btnGuardar.Click += btnGuardar_Click;
            btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            // Marca fija (sin combo)
            txtMarca.ReadOnly = true;
            txtMarca.Text = "Visa";
        }

        private void Alta_Load(object sender, EventArgs e)
        {
            Text = "ITX - Alta de Tarjeta";

            // Campos mínimos para alta: Cliente, Cuenta, Marca (fija), Titular opcional, fechas
            txtIdTarjeta.ReadOnly = true;
            txtNumero.ReadOnly = true; // se genera solo
            txtCVV.ReadOnly = true;    // solo display enmascarado
        }

        private void Edit_Load(int idTarjeta)
        {
            _tarjeta = _svc.GetById(idTarjeta);
            if (_tarjeta == null)
            {
                KryptonMessageBox.Show(this, "No se encontró la tarjeta.", "Aviso");
                Close();
                return;
            }

            Text = _soloLectura ? "ITX - Ver Tarjeta" : "ITX - Editar Tarjeta";

            txtIdTarjeta.Text = _tarjeta.IdTarjeta.ToString();
            txtClienteId.Text = _tarjeta.ClienteId.ToString();
            txtCuentaId.Text = _tarjeta.CuentaId.ToString();
            txtNumero.Text = _tarjeta.NumeroEnmascarado;
            txtTitular.Text = _tarjeta.Titular ?? "";
            txtMarca.Text = string.IsNullOrWhiteSpace(_tarjeta.Marca) ? "Visa" : _tarjeta.Marca;
            dtpEmision.Value = _tarjeta.FechaEmision == default(DateTime) ? DateTime.Today : _tarjeta.FechaEmision;
            dtpVencimiento.Value = _tarjeta.FechaVencimiento == default(DateTime) ? DateTime.Today.AddYears(5) : _tarjeta.FechaVencimiento;
            txtCVV.Text = (_tarjeta.CVV == null || _tarjeta.CVV.Length == 0) ? "" : "***";

            SetReadOnly(_soloLectura);
        }

        private void SetReadOnly(bool ro)
        {
            txtClienteId.ReadOnly = ro;
            txtCuentaId.ReadOnly = ro;
            txtTitular.ReadOnly = ro;
            // txtMarca es siempre ReadOnly; no hace falta tocarlo
            dtpEmision.Enabled = !ro;
            dtpVencimiento.Enabled = !ro;
            btnGuardar.Visible = !ro;
        }

        private bool ValidarDatos(out int idCliente, out int idCuenta)
        {
            idCliente = 0; idCuenta = 0;

            if (!int.TryParse(txtClienteId.Text.Trim(), out idCliente) || idCliente <= 0)
            { KryptonMessageBox.Show(this, "ClienteId inválido."); return false; }

            if (!int.TryParse(txtCuentaId.Text.Trim(), out idCuenta) || idCuenta <= 0)
            { KryptonMessageBox.Show(this, "CuentaId inválido."); return false; }

            if (string.IsNullOrWhiteSpace(txtMarca.Text))
            { KryptonMessageBox.Show(this, "La marca es obligatoria."); return false; }

            if (dtpVencimiento.Value.Date <= dtpEmision.Value.Date)
            { KryptonMessageBox.Show(this, "La fecha de vencimiento debe ser posterior a la de emisión."); return false; }

            string motivo;
            if (!_svc.PuedeCrearParaCuenta(idCliente, idCuenta, out motivo) && _tarjeta == null) // solo en alta
            { KryptonMessageBox.Show(this, motivo, "Reglas de negocio"); return false; }

            return true;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            int idCliente, idCuenta;
            if (!ValidarDatos(out idCliente, out idCuenta)) return;

            try
            {
                if (_tarjeta == null)
                {
                    var t = new Tarjeta
                    {
                        ClienteId = idCliente,
                        CuentaId = idCuenta,
                        Marca = txtMarca.Text.Trim(),       // ← marca fija
                        Titular = (txtTitular.Text ?? "").Trim(),
                        FechaEmision = dtpEmision.Value.Date,
                        FechaVencimiento = dtpVencimiento.Value.Date
                    };

                    // Generación automática de Nro y CVV
                    t.NumeroTarjeta = _svc.GenerarNroTarjeta("450799");
                    t.CVV = _svc.GenerarCVV();

                    var id = _svc.Crear(t);
                    KryptonMessageBox.Show(this, "Tarjeta creada. Id=" + id, "OK");
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    _tarjeta.ClienteId = idCliente;
                    _tarjeta.CuentaId = idCuenta;
                    _tarjeta.Marca = txtMarca.Text.Trim();   // ← marca fija
                    _tarjeta.Titular = (txtTitular.Text ?? "").Trim();
                    _tarjeta.FechaEmision = dtpEmision.Value.Date;
                    _tarjeta.FechaVencimiento = dtpVencimiento.Value.Date;

                    if (_svc.Actualizar(_tarjeta))
                    {
                        KryptonMessageBox.Show(this, "Tarjeta actualizada.", "OK");
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        KryptonMessageBox.Show(this, "No se pudo actualizar.", "Aviso");
                    }
                }
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(this, "Error al guardar:\n" + ex.Message, "Error",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }

        // Handlers vacíos si el designer los referencia (no molestan)
        private void lblIdTarjeta_Click(object sender, EventArgs e) { }
        private void AltaTarjeta_Load(object sender, EventArgs e) { }
    }
}
