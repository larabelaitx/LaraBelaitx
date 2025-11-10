using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Krypton.Toolkit;
using BE;
using BLL.Contracts;
using BLL.Services;
using UI.Common;      // ModoForm
using UI.Seguridad;   // Perms

namespace UI
{
    public partial class MainClientes : KryptonForm
    {
        private readonly IClienteService _svc;
        private readonly IRolService _roles;
        private readonly int? _currentUserId;

        private BindingList<ClienteVM> _view = new BindingList<ClienteVM>();

        private const string P_CLIENTE_LISTAR = Perms.Cliente_Listar;
        private const string P_CLIENTE_ALTA = Perms.Cliente_Alta;
        private const string P_CLIENTE_EDITAR = Perms.Cliente_Editar;

        public MainClientes() : this(new ClienteService(), new RolService(), null) { }

        public MainClientes(IClienteService svc, IRolService roles, int? currentUserId)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterParent;

            _svc = svc ?? throw new ArgumentNullException(nameof(svc));
            _roles = roles ?? throw new ArgumentNullException(nameof(roles));
            _currentUserId = currentUserId;

            Load += MainClientes_Load;

            TryWire("btnBuscar", (s, e) => Refrescar());
            TryWire("btnLimpiar", (s, e) => { LimpiarFiltros(); Refrescar(); });
            TryWire("btnAgregar", btnAgregar_Click);
            TryWire("btnVolver", (s, e) => Close());

            TryFind<DataGridView>("dgvClientes", g =>
            {
                g.AutoGenerateColumns = false;
                g.ReadOnly = true;
                g.AllowUserToAddRows = g.AllowUserToDeleteRows = false;
                g.MultiSelect = false;
                g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                g.RowHeadersVisible = false;

                AddTextCol(g, "colId", "Id", "IdCliente", false, 60);
                AddTextCol(g, "colApellido", "Apellido", "Apellido", true, 140);
                AddTextCol(g, "colNombre", "Nombre", "Nombre", true, 140);
                AddTextCol(g, "colDocumento", "Documento", "Documento", true, 120);
                AddTextCol(g, "colCorreo", "Correo", "Correo", true, 200);

                AddBtnCol(g, "colVer", "Ver", 60);
                AddBtnCol(g, "colEditar", "Editar", 70);
                AddBtnCol(g, "colCuentas", "Cuentas", 90);

                g.CellContentClick += dgvClientes_CellContentClick;
            });
        }

        private void MainClientes_Load(object sender, EventArgs e)
        {
            if (!Allowed(P_CLIENTE_LISTAR)) { Close(); return; }
            Refrescar();
        }

        private bool Allowed(string patente)
            => _currentUserId.HasValue && _currentUserId.Value > 0 && _roles.TienePatente(_currentUserId.Value, patente);

        private void Refrescar()
        {
            try
            {
                string fApe = GetText("txtApellido");
                string fNom = GetText("txtNombre");
                string fDoc = GetText("txtDocumento");

                var data = (_svc.GetAll() ?? new List<Cliente>())
                    .Select(c => new ClienteVM
                    {
                        IdCliente = c.IdCliente,
                        Apellido = c.Apellido ?? "",
                        Nombre = c.Nombre ?? "",
                        Documento = c.DocumentoIdentidad ?? "",
                        Correo = c.CorreoElectronico ?? ""
                    });

                if (!string.IsNullOrWhiteSpace(fApe))
                    data = data.Where(x => (x.Apellido ?? "").IndexOf(fApe, StringComparison.OrdinalIgnoreCase) >= 0);
                if (!string.IsNullOrWhiteSpace(fNom))
                    data = data.Where(x => (x.Nombre ?? "").IndexOf(fNom, StringComparison.OrdinalIgnoreCase) >= 0);
                if (!string.IsNullOrWhiteSpace(fDoc))
                    data = data.Where(x => (x.Documento ?? "").IndexOf(fDoc, StringComparison.OrdinalIgnoreCase) >= 0);

                var lista = data.OrderBy(x => x.Apellido).ThenBy(x => x.Nombre).ToList();
                _view = new BindingList<ClienteVM>(lista);

                TryFind<DataGridView>("dgvClientes", g => g.DataSource = _view);
                SetText("lblTotal", "Total: " + lista.Count);
            }
            catch
            {
                // silencioso
            }
        }

        private void LimpiarFiltros()
        {
            SetText("txtApellido", "");
            SetText("txtNombre", "");
            SetText("txtDocumento", "");
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!Allowed(P_CLIENTE_ALTA)) return;

            try
            {
                using (var frm = new AltaCliente(_svc, _roles, ModoForm.Alta, null, null, _currentUserId))
                {
                    if (frm.ShowDialog(this) == DialogResult.OK)
                        Refrescar();
                }
            }
            catch { /* silencioso */ }
        }

        private void dgvClientes_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            var grid = (DataGridView)sender;
            var col = grid.Columns[e.ColumnIndex].Name;
            var vm = grid.Rows[e.RowIndex].DataBoundItem as ClienteVM;
            if (vm == null) return;

            if (col == "colVer")
            {
                using (var frm = new AltaCliente(_svc, _roles, ModoForm.Ver, vm.IdCliente, null, _currentUserId))
                    frm.ShowDialog(this);
                return;
            }

            if (col == "colEditar")
            {
                if (!Allowed(P_CLIENTE_EDITAR)) return;

                using (var frm = new AltaCliente(_svc, _roles, ModoForm.Editar, vm.IdCliente, null, _currentUserId))
                {
                    if (frm.ShowDialog(this) == DialogResult.OK)
                        Refrescar();
                }
                return;
            }

            if (col == "colCuentas")
            {
                try
                {
                    using (var frm = new MainCuentas(vm.IdCliente, _roles, _currentUserId))
                        frm.ShowDialog(this);
                }
                catch { /* silencioso */ }
            }
        }

        // ==== Helpers UI ====
        private void TryWire(string name, EventHandler handler)
        {
            var c = Controls.Find(name, true).FirstOrDefault();
            if (c != null) c.Click += handler;
        }
        private void TryFind<T>(string name, Action<T> act) where T : Control
        {
            var c = Controls.Find(name, true).FirstOrDefault() as T;
            if (c != null) act(c);
        }
        private void AddTextCol(DataGridView g, string name, string header, string prop, bool visible, int width)
        {
            if (g.Columns.Contains(name)) return;
            g.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                DataPropertyName = prop,
                Visible = visible,
                Width = width,
                MinimumWidth = width,
                AutoSizeMode = visible ? DataGridViewAutoSizeColumnMode.DisplayedCells : DataGridViewAutoSizeColumnMode.None
            });
        }
        private void AddBtnCol(DataGridView g, string name, string header, int width)
        {
            if (g.Columns.Contains(name)) return;
            var col = new DataGridViewButtonColumn
            {
                Name = name,
                HeaderText = header,
                Text = header,
                UseColumnTextForButtonValue = true,
                Width = width,
                MinimumWidth = width
            };
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            g.Columns.Add(col);
        }
        private string GetText(string name)
        {
            var c = Controls.Find(name, true).FirstOrDefault() as TextBox;
            return c != null ? (c.Text ?? "").Trim() : "";
        }
        private void SetText(string name, string value)
        {
            var c = Controls.Find(name, true).FirstOrDefault() as Control;
            if (c is TextBox) ((TextBox)c).Text = value;
            else if (c is Label) ((Label)c).Text = value;
        }

        // VM
        private class ClienteVM
        {
            public int IdCliente { get; set; }
            public string Apellido { get; set; }
            public string Nombre { get; set; }
            public string Documento { get; set; }
            public string Correo { get; set; }
        }
    }
}
