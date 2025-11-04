// UI/MainDV.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Krypton.Toolkit;
using BLL.Contracts;
using BLL.Services;

namespace UI
{
    // Si tenés archivo .Designer, esta clase debe ser "partial".
    public partial class MainDV : KryptonForm
    {
        private readonly IDVServices _svc;

        // --- Controles ---
        private readonly KryptonPanel panel = new KryptonPanel();
        private readonly KryptonButton btnVerificarTodas = new KryptonButton();
        private readonly KryptonButton btnRecalcDVH = new KryptonButton();
        private readonly KryptonButton btnRecalcDVV = new KryptonButton();
        private readonly KryptonButton btnAyuda = new KryptonButton();
        private readonly KryptonDataGridView grid = new KryptonDataGridView();

        private readonly BindingList<FilaResumen> _view = new BindingList<FilaResumen>();

        // Tablas sujetas a control de integridad
        private static readonly string[] Tablas =
        {
            "Usuario",
            "Familia",
            "UsuarioFamilia",
            "UsuarioPatente",
            "FamiliaPatente"
        };

        // ===== Constructores =====
        public MainDV() : this(new DVService()) { }

        public MainDV(IDVServices service)
        {
            _svc = service ?? throw new ArgumentNullException(nameof(service));

            InitializeComponent();

            Text = "Verificación de Dígitos Verificadores (DVH / DVV)";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(760, 520);

            // Panel base
            panel.Dock = DockStyle.Fill;
            Controls.Add(panel);

            // Grilla Resumen
            grid.Dock = DockStyle.Fill;
            grid.AutoGenerateColumns = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.RowHeadersVisible = false;
            grid.MultiSelect = false;

            AgregarColTexto("Tabla", nameof(FilaResumen.Tabla), 180);
            AgregarColTexto("DVH", nameof(FilaResumen.DVHTexto), 220);
            AgregarColTexto("DVV", nameof(FilaResumen.DVVTexto), 180);

            grid.DataSource = _view;
            grid.CellFormatting += Grid_CellFormatting;

            // Barra superior
            var bar = new TableLayoutPanel();
            bar.Dock = DockStyle.Top;
            bar.Height = 58;
            bar.ColumnCount = 5;
            bar.RowCount = 1;
            bar.Padding = new Padding(10);
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180)); // Verificar
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160)); // Recalc DVH
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200)); // Recalc DVV
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120)); // Ayuda
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));  // filler

            btnVerificarTodas.Text = "Verificar TODAS";
            btnRecalcDVH.Text = "Recalcular DVH (sel.)";
            btnRecalcDVV.Text = "Recalcular DVV (sel.)";
            btnAyuda.Text = "Ayuda";

            btnVerificarTodas.Click += (s, e) => VerificarTodas();
            btnRecalcDVH.Click += (s, e) => RecalcularDVHSeleccionada();
            btnRecalcDVV.Click += (s, e) => RecalcularDVVSeleccionada();
            btnAyuda.Click += (s, e) => MostrarAyuda();

            bar.Controls.Add(btnVerificarTodas, 0, 0);
            bar.Controls.Add(btnRecalcDVH, 1, 0);
            bar.Controls.Add(btnRecalcDVV, 2, 0);
            bar.Controls.Add(btnAyuda, 3, 0);

            panel.Controls.Add(grid);
            panel.Controls.Add(bar);

            // Cargar resumen inicial
            CargarFilasVacias();
        }

        // ===== UI helpers =====
        private void AgregarColTexto(string header, string prop, int width)
        {
            var c = new DataGridViewTextBoxColumn();
            c.HeaderText = header;
            c.DataPropertyName = prop;
            c.Width = width;
            c.MinimumWidth = width;
            c.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            c.SortMode = DataGridViewColumnSortMode.NotSortable;
            grid.Columns.Add(c);
        }

        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= grid.Rows.Count) return;
            var row = grid.Rows[e.RowIndex].DataBoundItem as FilaResumen;
            if (row == null) return;

            // Coloreamos la fila según estados
            bool dvhOk = string.Equals(row.DVHEstado, "OK", StringComparison.OrdinalIgnoreCase);
            bool dvvOk = string.Equals(row.DVVEstado, "OK", StringComparison.OrdinalIgnoreCase);

            var back = Color.White;
            var fore = Color.Black;

            if (!dvhOk || !dvvOk)
            {
                back = Color.FromArgb(255, 245, 245);
                fore = Color.FromArgb(150, 30, 30);
            }

            grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = back;
            grid.Rows[e.RowIndex].DefaultCellStyle.ForeColor = fore;
        }

        private void CargarFilasVacias()
        {
            _view.Clear();
            for (int i = 0; i < Tablas.Length; i++)
                _view.Add(new FilaResumen { Tabla = Tablas[i], DVHEstado = "—", DVVEstado = "—", DVHTexto = "—", DVVTexto = "—" });
        }

        private string TablaSeleccionada()
        {
            if (grid.CurrentRow == null) return null;
            var fr = grid.CurrentRow.DataBoundItem as FilaResumen;
            return fr != null ? fr.Tabla : null;
        }

        // ===== Acciones =====
        private void VerificarTodas()
        {
            try
            {
                _view.Clear();

                for (int i = 0; i < Tablas.Length; i++)
                {
                    var t = Tablas[i];

                    // DVH: contamos discrepancias (sin exponer hashes ni columnas)
                    var dif = _svc.VerificarDVH(t);
                    var dvhOk = dif == null || dif.Count == 0;
                    var dvhTxt = dvhOk ? "OK" : ("Error (" + dif.Count + ")");

                    // DVV: comparación resumida
                    string calc, guard;
                    bool dvvOk = _svc.VerificarTabla(t, out calc, out guard);
                    var dvvTxt = dvvOk ? "OK" : "Error";

                    _view.Add(new FilaResumen
                    {
                        Tabla = t,
                        DVHEstado = dvhOk ? "OK" : "ERROR",
                        DVVEstado = dvvOk ? "OK" : "ERROR",
                        DVHTexto = dvhTxt,
                        DVVTexto = dvvTxt
                    });
                }

                // Mensaje resumen rápido
                var resumen = string.Join(Environment.NewLine,
                    _view.Select(r => r.Tabla + ": DVH " + r.DVHTexto + " | DVV " + r.DVVTexto).ToArray());

                KryptonMessageBox.Show(this, resumen, "Verificar Todas",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(this, "Error verificando: " + ex.Message, "Error",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }

        private void RecalcularDVHSeleccionada()
        {
            var tabla = TablaSeleccionada();
            if (string.IsNullOrWhiteSpace(tabla))
            {
                KryptonMessageBox.Show(this, "Seleccioná una tabla del resumen.", "Info",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                return;
            }

            var confirm = KryptonMessageBox.Show(this,
                "¿Recalcular DVH de todas las filas de '" + tabla + "'?\n" +
                "(Luego verificá las tablas o recalculá DVV si corresponde).",
                "Confirmar", KryptonMessageBoxButtons.YesNo, KryptonMessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                int cant = _svc.RecalcularDVH(tabla);
                KryptonMessageBox.Show(this, "DVH recalculado. Filas actualizadas: " + cant + ".",
                    "DVH", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                VerificarTodas();
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(this, "Error recalculando DVH: " + ex.Message, "Error",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }

        private void RecalcularDVVSeleccionada()
        {
            var tabla = TablaSeleccionada();
            if (string.IsNullOrWhiteSpace(tabla))
            {
                KryptonMessageBox.Show(this, "Seleccioná una tabla del resumen.", "Info",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                return;
            }

            try
            {
                string dvv;
                bool ok = _svc.RecalcularYGuardarDVV(tabla, out dvv);
                KryptonMessageBox.Show(this,
                    ok ? "DVV actualizado." : "No se pudo actualizar el DVV.",
                    "DVV",
                    KryptonMessageBoxButtons.OK,
                    ok ? KryptonMessageBoxIcon.Information : KryptonMessageBoxIcon.Warning);
                VerificarTodas();
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(this, "Error recalculando DVV: " + ex.Message, "Error",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }

        private void MostrarAyuda()
        {
            var msg =
                "Cómo usar este control de integridad:\n\n" +
                "1) Presioná \"Verificar TODAS\" para chequear DVH y DVV por tabla.\n" +
                "2) Si alguna tabla marca \"DVH Error (...)\", presioná \"Recalcular DVH (sel.)\" con esa tabla seleccionada.\n" +
                "3) Si una tabla marca \"DVV Error\", presioná \"Recalcular DVV (sel.)\" para actualizar su DVV.\n" +
                "4) Volvé a \"Verificar TODAS\" hasta ver todo en OK.\n\n" +
                "Notas:\n" +
                "• No se exponen valores de DVH/DVV reales (solo estados). \n" +
                "• Las acciones de recálculo no modifican datos de negocio, únicamente integridad (DVH/DVV).";

            KryptonMessageBox.Show(this, msg, "Ayuda • DVH/DVV",
                KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
        }

        // ===== Modelo de la grilla =====
        private class FilaResumen
        {
            public string Tabla { get; set; }

            // Estados “OK/ERROR” (para colorear filas)
            public string DVHEstado { get; set; }
            public string DVVEstado { get; set; }

            // Texto visible en la grilla (OK / Error (n))
            public string DVHTexto { get; set; }
            public string DVVTexto { get; set; }
        }
    }
}
