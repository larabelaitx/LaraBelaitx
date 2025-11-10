using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Krypton.Toolkit;
using BE;
using BLL.Contracts;
using BLL.Services;

namespace UI
{
    public partial class MainTarjetas : KryptonForm
    {
        private readonly ITarjetaService _svc;
        private readonly ICuentaService _ctas;
        private readonly IClienteService _clis;

        private List<Tarjeta> _modelo = new List<Tarjeta>();

        public MainTarjetas() : this(new TarjetaService(), new CuentaService(), new ClienteService()) { }
        public MainTarjetas(ITarjetaService svc, ICuentaService ctas, IClienteService clis)
        {
            InitializeComponent();
            _svc = svc ?? new TarjetaService();
            _ctas = ctas ?? new CuentaService();
            _clis = clis ?? new ClienteService();

            Load += MainTarjetas_Load;

            btnBuscar.Click += (s, e) => Refrescar();
            btnLimpiar.Click += (s, e) =>
            {
                txtCliente.Clear();
                txtCuenta.Clear();
                if (chkSoloVigentes != null) chkSoloVigentes.Checked = false;
                Refrescar();
            };

            btnAlta.Click += (s, e) => AbrirAlta();

            dgvTarjetas.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) AbrirVer(); };
            dgvTarjetas.CellContentClick += dgvTarjetas_CellContentClick;
        }

        private void MainTarjetas_Load(object sender, EventArgs e)
        {
            ConfigurarGrid();
            Refrescar();
        }

        private void ConfigurarGrid()
        {
            var g = dgvTarjetas;
            g.AutoGenerateColumns = false;
            g.ReadOnly = true;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.RowHeadersVisible = false;

            if (g.Columns.Count == 0)
            {
                g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId", HeaderText = "Id", DataPropertyName = "IdTarjeta", Width = 60 });
                g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCuenta", HeaderText = "Cuenta", DataPropertyName = "CuentaId", Width = 90 });
                g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colNumero", HeaderText = "Tarjeta", DataPropertyName = "NumeroEnmascarado", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
                g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTitular", HeaderText = "Titular", DataPropertyName = "Titular", Width = 180 });
                g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMarca", HeaderText = "Marca", DataPropertyName = "Marca", Width = 90 });
                g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colEmision", HeaderText = "Emisión", DataPropertyName = "FechaEmision", Width = 95 });
                g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colVto", HeaderText = "Vencimiento", DataPropertyName = "FechaVencimiento", Width = 110 });

                g.Columns.Add(new DataGridViewButtonColumn { Name = "btnVer", HeaderText = "Ver", Text = "Ver", UseColumnTextForButtonValue = true, Width = 70 });
                g.Columns.Add(new DataGridViewButtonColumn { Name = "btnEditar", HeaderText = "Editar", Text = "Editar", UseColumnTextForButtonValue = true, Width = 80 });
                g.Columns.Add(new DataGridViewButtonColumn { Name = "btnBaja", HeaderText = "Baja", Text = "Baja", UseColumnTextForButtonValue = true, Width = 70 });
            }
        }

        private void Refrescar()
        {
            _modelo = _svc.GetAll() ?? new List<Tarjeta>();
            var q = _modelo.AsEnumerable();

            // Filtro por Nº de cuenta
            if (!string.IsNullOrWhiteSpace(txtCuenta.Text))
            {
                if (int.TryParse(txtCuenta.Text.Trim(), out var idCta))
                    q = q.Where(t => t.CuentaId == idCta);
            }

            // Filtro por Cliente (Apellido/Nombre contiene)
            if (!string.IsNullOrWhiteSpace(txtCliente.Text))
            {
                var filtro = txtCliente.Text.Trim().ToLowerInvariant();
                q = q.Where(t =>
                {
                    var cta = _ctas.GetById(t.CuentaId);
                    var cli = (cta != null) ? _clis.GetById(cta.ClienteId) : null;
                    var nom = (cli == null) ? "" : $"{cli.Apellido ?? ""} {cli.Nombre ?? ""}";
                    return nom.ToLowerInvariant().Contains(filtro);
                });
            }

            // Sólo vigentes (no vencidas)
            if (chkSoloVigentes != null && chkSoloVigentes.Checked)
                q = q.Where(t => t.FechaVencimiento > DateTime.Today);

            dgvTarjetas.DataSource = q.Select(t => new
            {
                t.IdTarjeta,
                t.CuentaId,
                t.NumeroEnmascarado,
                t.Titular,
                t.Marca,
                FechaEmision = t.FechaEmision == default ? "" : t.FechaEmision.ToString("dd/MM/yyyy"),
                FechaVencimiento = t.FechaVencimiento == default ? "" : t.FechaVencimiento.ToString("dd/MM/yyyy")
            }).ToList();
        }

        private int? IdSeleccionado()
        {
            if (dgvTarjetas.CurrentRow == null) return null;
            var id = dgvTarjetas.CurrentRow.Cells["colId"].Value;
            return (id != null && int.TryParse(id.ToString(), out var v)) ? v : (int?)null;
        }

        private void dgvTarjetas_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var col = dgvTarjetas.Columns[e.ColumnIndex].Name;
            if (col == "btnVer") { AbrirVer(); return; }
            if (col == "btnEditar") { AbrirEditar(); return; }
            if (col == "btnBaja") { Eliminar(); return; }
        }

        private void AbrirAlta()
        {
            using (var frm = new AltaTarjeta(_svc, _ctas, _clis))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    Refrescar();
            }
        }

        private void AbrirEditar()
        {
            var id = IdSeleccionado();
            if (!id.HasValue) { KryptonMessageBox.Show(this, "Seleccioná una tarjeta.", "Info"); return; }

            using (var frm = new AltaTarjeta(_svc, _ctas, _clis, id.Value, false))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    Refrescar();
            }
        }

        private void AbrirVer()
        {
            var id = IdSeleccionado();
            if (!id.HasValue) return;

            using (var frm = new AltaTarjeta(_svc, _ctas, _clis, id.Value, true))
                frm.ShowDialog(this);
        }

        private void Eliminar()
        {
            var id = IdSeleccionado();
            if (!id.HasValue) return;

            if (KryptonMessageBox.Show(this, "¿Eliminar la tarjeta seleccionada?", "Confirmar",
                    KryptonMessageBoxButtons.YesNo, KryptonMessageBoxIcon.Question) != DialogResult.Yes) return;

            if (_svc.Eliminar(id.Value))
            {
                KryptonMessageBox.Show(this, "Tarjeta eliminada.", "OK");
                Refrescar();
            }
            else
            {
                KryptonMessageBox.Show(this, "No se pudo eliminar.", "Aviso");
            }
        }
    }
}
