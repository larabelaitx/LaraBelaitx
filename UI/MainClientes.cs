using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using BLL.Services;   
using BE;          

namespace UI
{
    public partial class MainClientes : Form
    {
        private readonly IClienteService _clienteService;
        private bool _gridConfigured;

        public MainClientes()
        {
            InitializeComponent();

            // Instancia del servicio real 
            _clienteService = new ClienteService();

            ConfigureGrid();
            ApplySearch(); // carga inicial (todos)

            // Eventos de botones
            btnBuscar.Click += btnBuscar_Click;
            btnLimpiar.Click += btnLimpiar_Click;
            btnAgregar.Click += btnAgregar_Click;
            btnVolver.Click += btnVolver_Click;

            // Enter en filtros -> Buscar
            txtNombre.KeyDown += Filtros_KeyDown_Enter_Buscar;
            txtApellido.KeyDown += Filtros_KeyDown_Enter_Buscar;
            txtDocumento.KeyDown += Filtros_KeyDown_Enter_Buscar;

            // Eventos de grilla
            dgvClientes.CellContentClick += dgvClientes_CellContentClick;
            dgvClientes.DataBindingComplete += dgvClientes_DataBindingComplete;
        }

        private void ConfigureGrid()
        {
            if (_gridConfigured) return;

            dgvClientes.AutoGenerateColumns = false;
            dgvClientes.MultiSelect = false;
            dgvClientes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvClientes.ReadOnly = true; // edición por formulario
            dgvClientes.AllowUserToAddRows = false;
            dgvClientes.AllowUserToDeleteRows = false;
            dgvClientes.AllowUserToOrderColumns = true;
            dgvClientes.RowHeadersVisible = false;
            dgvClientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvClientes.Columns.Clear();

            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colNombre",
                HeaderText = "Nombre",
                DataPropertyName = "Nombre"
            });
            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colApellido",
                HeaderText = "Apellido",
                DataPropertyName = "Apellido"
            });
            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCorreo",
                HeaderText = "Correo",
                DataPropertyName = "CorreoElectronico"
            });
            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDocumento",
                HeaderText = "Documento",
                DataPropertyName = "DocumentoIdentidad" 
            });

            // Botón Editar
            dgvClientes.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "colEditar",
                HeaderText = "Editar",
                Text = "Editar",
                UseColumnTextForButtonValue = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
            });

            // Botón Ver
            dgvClientes.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "colVer",
                HeaderText = "Ver",
                Text = "Ver",
                UseColumnTextForButtonValue = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
            });

            _gridConfigured = true;
        }

        private void btnBuscar_Click(object sender, EventArgs e) => ApplySearch();

        /// <summary>
        /// Si no hay filtros, trae todo. Si hay alguno, filtra.
        /// </summary>
        private void ApplySearch()
        {
            string nombre = (txtNombre.Text ?? "").Trim();
            string apellido = (txtApellido.Text ?? "").Trim();
            string documento = (txtDocumento.Text ?? "").Trim();

            bool hayFiltros = !string.IsNullOrEmpty(nombre)
                           || !string.IsNullOrEmpty(apellido)
                           || !string.IsNullOrEmpty(documento);

            List<Cliente> datos = hayFiltros
                ? _clienteService.Buscar(nombre, apellido, documento)
                : _clienteService.ObtenerTodos();

            dgvClientes.DataSource = null;
            dgvClientes.DataSource = datos;

            if (dgvClientes.Columns.Contains("colApellido"))
                dgvClientes.Sort(dgvClientes.Columns["colApellido"], ListSortDirection.Ascending);
        }
        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            ClearFilters();
            ApplySearch(); // recarga todo
        }

        private void ClearFilters()
        {
            txtNombre.Clear();
            txtApellido.Clear();
            txtDocumento.Clear();
            txtNombre.Focus();
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            using (var frm = new AltaCliente(_clienteService, modo: "ALTA", cliente: null))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    ApplySearch();
            }
        }
        private void btnVolver_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void dgvClientes_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            string col = dgvClientes.Columns[e.ColumnIndex].Name;
            var cliente = dgvClientes.Rows[e.RowIndex].DataBoundItem as Cliente;
            if (cliente == null) return;

            if (col == "colVer")
            {
                using (var frm = new AltaCliente(_clienteService, modo: "VER", cliente: cliente))
                    frm.ShowDialog(this);
            }
            else if (col == "colEditar")
            {
                using (var frm = new AltaCliente(_clienteService, modo: "MODIFICAR", cliente: cliente))
                {
                    if (frm.ShowDialog(this) == DialogResult.OK)
                        ApplySearch();
                }
            }
        }

        private void dgvClientes_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvClientes.ClearSelection();
            foreach (DataGridViewColumn c in dgvClientes.Columns)
            {
                c.DefaultCellStyle.Alignment =
                    c is DataGridViewButtonColumn ? DataGridViewContentAlignment.MiddleCenter
                                                 : DataGridViewContentAlignment.MiddleLeft;
            }
        }

        // Enter en filtros
        private void Filtros_KeyDown_Enter_Buscar(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                ApplySearch();
            }
        }
    }
}
