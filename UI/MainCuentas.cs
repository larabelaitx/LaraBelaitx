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
    public partial class MainCuentas : KryptonForm
    {
        private readonly ICuentaService _svc;
        private readonly IRolService _roles;
        private readonly int? _currentUserId;
        private readonly int? _idClienteContexto;

        private BindingList<CuentaVM> _view = new BindingList<CuentaVM>();

        private const string P_CTA_LISTAR = Perms.Cuenta_Listar;
        private const string P_CTA_ALTA = Perms.Cuenta_Alta;
        private const string P_CTA_EDITAR = Perms.Cuenta_Editar;

        public MainCuentas() : this(new CuentaService(), new RolService(), null, null) { }

        public MainCuentas(int idCliente, IRolService roles, int? currentUserId)
            : this(new CuentaService(), roles ?? new RolService(), currentUserId, idCliente) { }

        public MainCuentas(ICuentaService svc, IRolService roles, int? currentUserId, int? idClienteContexto)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterParent;

            _svc = svc ?? throw new ArgumentNullException(nameof(svc));
            _roles = roles ?? throw new ArgumentNullException(nameof(roles));
            _currentUserId = currentUserId;
            _idClienteContexto = idClienteContexto;

            Load += MainCuentas_Load;

            TryWire("btnBuscar", (s, e) => Refrescar());
            TryWire("btnLimpiar", (s, e) => { LimpiarFiltros(); Refrescar(); });
            TryWire("btnAgregar", btnAgregar_Click);
            TryWire("btnVolver", (s, e) => Close());

            TryFind<DataGridView>("dgvCuentas", g =>
            {
                g.AutoGenerateColumns = false;
                g.ReadOnly = true;
                g.AllowUserToAddRows = g.AllowUserToDeleteRows = false;
                g.MultiSelect = false;
                g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                g.RowHeadersVisible = false;

                AddTextCol(g, "colId", "Id", "IdCuenta", false, 60);
                AddTextCol(g, "colNumero", "N° Cuenta", "NumeroCuenta", true, 140);
                AddTextCol(g, "colCBU", "CBU", "CBU", true, 170);
                AddTextCol(g, "colAlias", "Alias", "Alias", true, 150);
                AddTextCol(g, "colTipo", "Tipo", "TipoCuenta", true, 110);
                AddTextCol(g, "colMoneda", "Moneda", "Moneda", true, 90);
                AddTextCol(g, "colApertura", "Apertura", "FechaAperturaStr", true, 100);

                AddBtnCol(g, "colVer", "Ver", 60);
                AddBtnCol(g, "colEditar", "Editar", 70);

                g.CellContentClick += dgvCuentas_CellContentClick;
            });
        }

        private void MainCuentas_Load(object sender, EventArgs e)
        {
            if (!Allowed(P_CTA_LISTAR)) { Close(); return; }

            if (_idClienteContexto.HasValue)
            {
                SetText("lblCliente", "Cliente Id: " + _idClienteContexto.Value);
                var txt = Find<TextBox>("txtIdCliente");
                if (txt != null)
                {
                    txt.Text = _idClienteContexto.Value.ToString();
                    txt.ReadOnly = true;
                }
            }

            Refrescar();
        }

        private bool Allowed(string patente)
            => _currentUserId.HasValue && _currentUserId.Value > 0 && _roles.TienePatente(_currentUserId.Value, patente);

        private void Refrescar()
        {
            try
            {
                var q = _svc.GetAll() ?? new List<Cuenta>();

                int idCli;
                var fIdCliTxt = GetText("txtIdCliente");
                if (!string.IsNullOrWhiteSpace(fIdCliTxt) && int.TryParse(fIdCliTxt, out idCli))
                    q = q.Where(c => GetInt(c, "ClienteId", "IdCliente") == idCli).ToList();

                var fNum = GetText("txtNumero");
                if (!string.IsNullOrWhiteSpace(fNum))
                    q = q.Where(c => (GetStr(c, "NumeroCuenta") ?? "").IndexOf(fNum, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

                var fAlias = GetText("txtAlias");
                if (!string.IsNullOrWhiteSpace(fAlias))
                    q = q.Where(c => (GetStr(c, "Alias") ?? "").IndexOf(fAlias, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

                var lista = q.Select(c => new CuentaVM
                {
                    IdCuenta = GetInt(c, "Id", "IdCuenta", "CuentaId"),
                    NumeroCuenta = GetStr(c, "NumeroCuenta") ?? "",
                    CBU = GetStr(c, "CBU") ?? "",
                    Alias = GetStr(c, "Alias") ?? "",
                    TipoCuenta = GetStr(c, "TipoCuenta") ?? "",
                    Moneda = GetStr(c, "Moneda") ?? "",
                    FechaAperturaStr = GetDate(c, "FechaApertura") == DateTime.MinValue
                        ? ""
                        : GetDate(c, "FechaApertura").ToString("yyyy-MM-dd")
                })
                .OrderBy(x => x.NumeroCuenta)
                .ToList();

                _view = new BindingList<CuentaVM>(lista);
                TryFind<DataGridView>("dgvCuentas", g => g.DataSource = _view);

                SetText("lblTotal", "Total: " + lista.Count.ToString());
            }
            catch
            {
                // Silencioso
            }
        }

        private void LimpiarFiltros()
        {
            if (!_idClienteContexto.HasValue) SetText("txtIdCliente", "");
            SetText("txtNumero", "");
            SetText("txtAlias", "");
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!Allowed(P_CTA_ALTA)) return;

            try
            {
                int idCli;
                if (!_idClienteContexto.HasValue)
                {
                    var fIdCliTxt = GetText("txtIdCliente");
                    if (string.IsNullOrWhiteSpace(fIdCliTxt) || !int.TryParse(fIdCliTxt, out idCli) || idCli <= 0)
                        return; // silencioso si no hay cliente
                }
                else idCli = _idClienteContexto.Value;

                var clienteSvc = new ClienteService();
                var cli = clienteSvc.GetById(idCli);
                if (cli == null) return;

                using (var frm = new AltaCuenta(_svc, cli, _roles, _currentUserId))
                {
                    if (frm.ShowDialog(this) == DialogResult.OK)
                        Refrescar();
                }
            }
            catch { /* silencioso */ }
        }

        private void dgvCuentas_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            var grid = (DataGridView)sender;
            var col = grid.Columns[e.ColumnIndex].Name;
            var vm = grid.Rows[e.RowIndex].DataBoundItem as CuentaVM;
            if (vm == null) return;

            if (col == "colVer")
            {
                using (var frm = new AltaCuenta(_svc, vm.IdCuenta, true, _roles, _currentUserId))
                    frm.ShowDialog(this);
                return;
            }

            if (col == "colEditar")
            {
                if (!Allowed(P_CTA_EDITAR)) return;

                using (var frm = new AltaCuenta(_svc, vm.IdCuenta, false, _roles, _currentUserId))
                {
                    if (frm.ShowDialog(this) == DialogResult.OK)
                        Refrescar();
                }
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
        private T Find<T>(string name) where T : Control
        {
            return Controls.Find(name, true).FirstOrDefault() as T;
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

        // ==== Reflect helpers ====
        private static string GetStr(object o, params string[] names)
        {
            if (o == null) return null;
            foreach (var n in names)
            {
                var p = o.GetType().GetProperty(n);
                if (p != null) return (p.GetValue(o, null) ?? "").ToString();
            }
            return null;
        }
        private static int GetInt(object o, params string[] names)
        {
            if (o == null) return 0;
            foreach (var n in names)
            {
                var p = o.GetType().GetProperty(n);
                if (p != null)
                {
                    var v = p.GetValue(o, null);
                    if (v is int i) return i;
                    if (v != null && int.TryParse(v.ToString(), out var r)) return r;
                }
            }
            return 0;
        }
        private static DateTime GetDate(object o, params string[] names)
        {
            if (o == null) return DateTime.MinValue;
            foreach (var n in names)
            {
                var p = o.GetType().GetProperty(n);
                if (p != null)
                {
                    var v = p.GetValue(o, null);
                    if (v is DateTime d1) return d1;
                    if (v != null && DateTime.TryParse(v.ToString(), out var d2)) return d2;
                }
            }
            return DateTime.MinValue;
        }

        // VM
        private class CuentaVM
        {
            public int IdCuenta { get; set; }
            public string NumeroCuenta { get; set; }
            public string CBU { get; set; }
            public string Alias { get; set; }
            public string TipoCuenta { get; set; }
            public string Moneda { get; set; }
            public string FechaAperturaStr { get; set; }
        }
    }
}
