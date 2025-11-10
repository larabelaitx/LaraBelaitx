using System;
using System.Windows.Forms;
using BE;
using Krypton.Toolkit;
using BLL.Contracts;
using BLL.Services;
using UI.Common; // ModoForm
using UI.Seguridad; // Perms

namespace UI
{
    public partial class AltaCuenta : KryptonForm
    {
        private readonly ICuentaService _svc;
        private readonly IRolService _roles;
        private readonly int? _currentUserId;

        private ModoForm _modo;        // Alta | Editar | Ver
        private Cuenta _cuenta;         // usado en Editar/Ver
        private readonly Cliente _cliente; // sólo en Alta

        private const string P_CTA_ALTA = Perms.Cuenta_Alta;
        private const string P_CTA_EDITAR = Perms.Cuenta_Editar;
        private const string P_CTA_VER = Perms.Cuenta_Listar;

        public AltaCuenta(ICuentaService svc, Cliente cliente, IRolService roles = null, int? currentUserId = null)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterParent;

            _svc = svc ?? throw new ArgumentNullException(nameof(svc));
            _roles = roles ?? new RolService();
            _currentUserId = currentUserId;

            _cliente = cliente ?? throw new ArgumentNullException(nameof(cliente));
            _modo = ModoForm.Alta;

            Load += Alta_Load;
            btnGuardar.Click += btnGuardar_Click;
            btnVolver.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            cboTipo.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMoneda.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        public AltaCuenta(ICuentaService svc, int idCuenta, bool modoConsulta, IRolService roles = null, int? currentUserId = null)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterParent;

            _svc = svc ?? throw new ArgumentNullException(nameof(svc));
            _roles = roles ?? new RolService();
            _currentUserId = currentUserId;

            _modo = modoConsulta ? ModoForm.Ver : ModoForm.Editar;

            Load += (s, e) => Edit_Load(idCuenta);
            btnGuardar.Click += btnGuardar_Click;
            btnVolver.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            cboTipo.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMoneda.DropDownStyle = ComboBoxStyle.DropDownList;
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
            cboTipo.Items.Clear();
            cboTipo.Items.AddRange(CatalogosCuenta.TiposCuenta);
            if (cboTipo.Items.Count > 0) cboTipo.SelectedIndex = 0;

            cboMoneda.Items.Clear();
            cboMoneda.Items.AddRange(CatalogosCuenta.Monedas);
            if (cboMoneda.Items.Count > 0) cboMoneda.SelectedIndex = 0;
        }

        private void Alta_Load(object sender, EventArgs e)
        {
            if (!CheckAllowed(P_CTA_ALTA)) { Close(); return; }

            Text = "ITX - Alta de Cuenta";
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
            if (_modo == ModoForm.Editar && !CheckAllowed(P_CTA_EDITAR)) { Close(); return; }
            if (_modo == ModoForm.Ver && !CheckAllowed(P_CTA_VER)) { Close(); return; }

            Text = (_modo == ModoForm.Editar) ? "ITX - Editar Cuenta" : "ITX - Ver Cuenta";
            CargarCombos();

            _cuenta = _svc.GetById(idCuenta);
            if (_cuenta == null)
            {
                KryptonMessageBox.Show(this, "No se encontró la cuenta.", "Aviso",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
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

            if (_modo == ModoForm.Ver)
            {
                cboTipo.Enabled = false;
                cboMoneda.Enabled = false;
                dtpApertura.Enabled = false;
                btnGuardar.Visible = false;
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (_modo == ModoForm.Editar && !CheckAllowed(P_CTA_EDITAR)) return;
            if (_modo == ModoForm.Alta && !CheckAllowed(P_CTA_ALTA)) return;

            try
            {
                if (cboTipo.SelectedItem == null)
                {
                    KryptonMessageBox.Show(this, "Seleccioná el tipo de cuenta.", "Aviso");
                    return;
                }
                if (cboMoneda.SelectedItem == null)
                {
                    KryptonMessageBox.Show(this, "Seleccioná la moneda.", "Aviso");
                    return;
                }

                if (_modo == ModoForm.Alta)
                {
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
                        // Bitácora
                        BLL.Bitacora.Info(_currentUserId,
                            $"Cuenta creada: {nueva.NumeroCuenta} | ClienteId={nueva.ClienteId} | {nueva.TipoCuenta} {nueva.Moneda}",
                            "Cuentas", "Crear", host: Environment.MachineName);

                        KryptonMessageBox.Show(this, "Cuenta creada con éxito.", "OK");
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        KryptonMessageBox.Show(this, "No se pudo crear la cuenta.", "Aviso");
                    }
                }
                else if (_modo == ModoForm.Editar)
                {
                    _cuenta.TipoCuenta = cboTipo.SelectedItem.ToString();
                    _cuenta.Moneda = cboMoneda.SelectedItem.ToString();
                    _cuenta.FechaApertura = dtpApertura.Value.Date;

                    if (_svc.Actualizar(_cuenta))
                    {
                        // Bitácora
                        BLL.Bitacora.Info(_currentUserId,
                            $"Cuenta actualizada: Id={_cuenta.IdCuenta} | Tipo={_cuenta.TipoCuenta} | Moneda={_cuenta.Moneda}",
                            "Cuentas", "Actualizar", host: Environment.MachineName);

                        KryptonMessageBox.Show(this, "Cuenta actualizada.", "OK");
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        KryptonMessageBox.Show(this, "No se pudo actualizar la cuenta.", "Aviso");
                    }
                }
                else
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(this, "Error al guardar: " + ex.Message, "Error",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }

        private void AltaCuenta_Load(object sender, EventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
    }
}
