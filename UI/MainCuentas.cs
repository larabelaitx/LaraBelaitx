using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using BLL.Services;
using BE;
using BLL.Contracts;
using Krypton.Toolkit;

namespace UI
{
    public partial class MainCuentas : Form
    {
        private readonly ICuentaService _svc;

        public MainCuentas()
        {
            InitializeComponent();
            _svc = new CuentaService();

            ConfigureGrid();
            LoadCombos();
            WireEvents();
            ApplySearch();
        }

        private void WireEvents()
        {
            btnBuscar.Click += (s, e) => ApplySearch();
            btnLimpiar.Click += (s, e) => { txtCliente.Clear(); cboTipo.SelectedIndex = 0; cboEstado.SelectedIndex = 0; ApplySearch(); };
            btnAgregar.Click += btnAgregar_Click;

            dgvCuentas.CellContentClick += dgvCuentas_CellContentClick;
            dgvCuentas.DataBindingComplete += (s, e) => dgvCuentas.ClearSelection();

            txtCliente.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; ApplySearch(); } };
        }

        private void LoadCombos()
        {
            cboTipo.Items.Clear();
            cboTipo.Items.AddRange(new object[] { "Todas", "Caja de Ahorro", "Cuenta Corriente" });
            cboTipo.SelectedIndex = 0;

            cboEstado.Items.Clear();
            // Si NO usás columna Estado en DB, dejá solo “Todas”
            cboEstado.Items.AddRange(new object[] { "Todas", "Abierta", "Suspendida", "Cerrada" });
            cboEstado.SelectedIndex = 0;
        }

        private void ConfigureGrid()
        {
            dgvCuentas.AutoGenerateColumns = false;
            dgvCuentas.MultiSelect = false;
            dgvCuentas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCuentas.ReadOnly = true;
            dgvCuentas.AllowUserToAddRows = false;
            dgvCuentas.AllowUserToDeleteRows = false;
            dgvCuentas.RowHeadersVisible = false;
            dgvCuentas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCuentas.Columns.Clear();

            dgvCuentas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colNumero", HeaderText = "N° Cuenta", DataPropertyName = "NumeroCuenta" });
            dgvCuentas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTipo", HeaderText = "Tipo", DataPropertyName = "TipoCuenta" });
            dgvCuentas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMoneda", HeaderText = "Moneda", DataPropertyName = "Moneda" });
            dgvCuentas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colEstado", HeaderText = "Estado", DataPropertyName = "EstadoTexto" });
            dgvCuentas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCliente", HeaderText = "Cliente", DataPropertyName = "ClienteNombre" });

            dgvCuentas.Columns.Add(new DataGridViewButtonColumn { Name = "colVer", HeaderText = "Ver", Text = "Ver", UseColumnTextForButtonValue = true });
            dgvCuentas.Columns.Add(new DataGridViewButtonColumn { Name = "colEditar", HeaderText = "Editar", Text = "Editar", UseColumnTextForButtonValue = true });
            dgvCuentas.Columns.Add(new DataGridViewButtonColumn { Name = "colBaja", HeaderText = "Baja", Text = "Baja", UseColumnTextForButtonValue = true });
            dgvCuentas.Columns.Add(new DataGridViewButtonColumn { Name = "colReactivar", HeaderText = "Reactivar", Text = "Reactivar", UseColumnTextForButtonValue = true });
        }

        private void ApplySearch()
        {
            var cuentas = _svc.Buscar(txtCliente.Text, cboTipo.Text, cboEstado.Text);

            if (cuentas == null || cuentas.Count == 0)
            {
                MessageBox.Show("No hay cuentas para mostrar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dgvCuentas.DataSource = null;
                return;
            }

            // 🔹 Ordena antes de enlazar
            var cuentasOrdenadas = cuentas
                .OrderBy(c => c.ClienteNombre ?? string.Empty)
                .ThenByDescending(c => c.IdCuenta)
                .ToList();
            var binding = new BindingList<BE.Cuenta>(cuentasOrdenadas);
            var bs = new BindingSource { DataSource = binding };

            dgvCuentas.AutoGenerateColumns = false;
            dgvCuentas.DataSource = bs;

            foreach (DataGridViewColumn col in dgvCuentas.Columns)
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            var clientes = new ClienteService().ObtenerTodos();

            if (clientes == null || clientes.Count == 0)
            {
                MessageBox.Show("No hay clientes para asociar.", "Aviso");
                return;
            }

            var data = new KryptonInputBoxData
            {
                Caption = "Seleccionar Cliente",
                Prompt = "Ingrese el Documento del cliente para vincular la cuenta:",
                DefaultResponse = ""
            };

            string seleccion = KryptonInputBox.Show(data);

            if (string.IsNullOrWhiteSpace(seleccion))
                return;

            var cliente = clientes.FirstOrDefault(c => (c.DocumentoIdentidad ?? "").Trim() == seleccion.Trim());
            if (cliente == null)
            {
                MessageBox.Show("Cliente no encontrado.", "Aviso");
                return;
            }

            using (var frm = new AltaCuenta(new CuentaService(), cliente))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    ApplySearch();
            }
        }

        private void dgvCuentas_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvCuentas.Rows[e.RowIndex].DataBoundItem;
            if (row == null) return;

            int idCuenta = (int)row.GetType().GetProperty("IdCuenta").GetValue(row, null);

            var colName = dgvCuentas.Columns[e.ColumnIndex].Name;
            if (colName == "colVer")
            {
                using (var frm = new AltaCuenta(new CuentaService(), idCuenta, modoConsulta: true))
                    frm.ShowDialog(this);
            }
            else if (colName == "colEditar")
            {
                using (var frm = new AltaCuenta(new CuentaService(), idCuenta, modoConsulta: false))
                {
                    if (frm.ShowDialog(this) == DialogResult.OK)
                        ApplySearch();
                }
            }
            else if (colName == "colBaja")
            {
                if (MessageBox.Show(this, "¿Confirmás dar de baja la cuenta?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    MessageBox.Show(this, "Implementá SetEstado/SetActiva según tu regla.", "Aviso");
                }
            }
            else if (colName == "colReactivar")
            {
                if (MessageBox.Show(this, "¿Confirmás reactivar la cuenta?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    MessageBox.Show(this, "Implementá SetEstado/SetActiva según tu regla.", "Aviso");
                }
            }
        }

        private void MainCuentas_Load(object sender, EventArgs e)
        {

        }
    }
}
