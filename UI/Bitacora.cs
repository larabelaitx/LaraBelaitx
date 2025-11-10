using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BE;
using BLL.Services;
using Krypton.Toolkit;

namespace UI
{
    public partial class Bitacora : KryptonForm
    {
        private readonly BitacoraService _svc;
        private int _page = 1;
        private const int PageSize = 100;
        private IList<BitacoraEntry> _current = new List<BitacoraEntry>();

        public Bitacora(string cnn)
        {
            InitializeComponent();
            _svc = new BitacoraService(cnn);

            LoadLookups();
            ApplyDefaults();

            btnAplicar.Click += btnAplicar_Click;
            btnLimpiar.Click += btnLimpiar_Click;
            btnImprimir.Click += btnImprimir_Click;
        }

        private void LoadLookups()
        {
            var usuarios = _svc.Usuarios().ToList();
            usuarios.Insert(0, "(Todos)");
            cboUsuario.DataSource = usuarios;

            cboOrden.DataSource = new[]
            {
                "Fecha DESC",
                "Fecha ASC",
                "Severidad DESC, Fecha DESC",
                "Usuario ASC, Fecha DESC",
                "Módulo ASC, Acción ASC, Fecha DESC"
            };
            cboOrden.SelectedIndex = 0;
        }

        private void ApplyDefaults()
        {
            dtpDesde.ShowCheckBox = true;
            dtpHasta.ShowCheckBox = true;
            dtpDesde.Checked = false;
            dtpHasta.Checked = false;
        }

        private void btnAplicar_Click(object sender, EventArgs e)
        {
            _page = 1;
            Buscar();
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            cboUsuario.SelectedIndex = 0;
            cboOrden.SelectedIndex = 0;
            dtpDesde.Checked = false;
            dtpHasta.Checked = false;
            _page = 1;
            Buscar();
        }

        private void Buscar()
        {
            string usuario = cboUsuario.SelectedItem as string;
            if (usuario == "(Todos)") usuario = null;

            DateTime? desde = dtpDesde.Checked ? dtpDesde.Value.Date : (DateTime?)null;
            DateTime? hasta = dtpHasta.Checked ? dtpHasta.Value.Date.AddDays(1).AddMilliseconds(-1) : (DateTime?)null;

            if (desde.HasValue && hasta.HasValue && desde > hasta)
            {
                KryptonMessageBox.Show("El rango de fechas es inválido (Desde > Hasta).", "Filtros",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                return;
            }

            string ordenar = cboOrden.SelectedItem != null ? cboOrden.SelectedItem.ToString() : "Fecha DESC";

            var res = _svc.Search(desde, hasta, usuario, _page, PageSize, ordenar);
            var rows = res.Rows ?? new BitacoraEntry[0];
            _current = rows.ToList();

            grdBitacora.DataSource = _current;

            if (grdBitacora.Columns.Count > 0)
            {
                if (grdBitacora.Columns.Contains("Id")) grdBitacora.Columns["Id"].Visible = false;
                if (grdBitacora.Columns.Contains("UsuarioId")) grdBitacora.Columns["UsuarioId"].Visible = false;
                grdBitacora.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }

            btnImprimir.Enabled = _current.Any();
        }

        private void btnImprimir_Click(object sender, EventArgs e)
        {
            if (_current.Count == 0)
            {
                KryptonMessageBox.Show("No hay registros para exportar.", "Imprimir",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                return;
            }

            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "CSV (*.csv)|*.csv";
                dlg.FileName = "Bitacora.csv";

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    System.IO.File.WriteAllLines(dlg.FileName,
                        _current.Select(r => string.Format("{0:yyyy-MM-dd HH:mm};{1};{2};{3};{4};{5}",
                            r.Fecha, r.Usuario, r.Modulo, r.Accion, r.Severidad, r.Mensaje)));
                    KryptonMessageBox.Show("Archivo exportado correctamente.", "Imprimir",
                        KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                }
            }
        }
    }
}
