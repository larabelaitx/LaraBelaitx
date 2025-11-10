using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Krypton.Toolkit;
using BLL.Contracts;   // IDVServices, IRolService
using BLL.Services;    // DVService, RolService

namespace UI
{
    public partial class MainDV : KryptonForm
    {
        // 🔓 Modo entrega: habilita SIEMPRE los botones del DV.
        // Cuando termines, ponelo en false para volver al chequeo real por patentes.
        private const bool FORCE_ENABLE_DV_BUTTONS = true;

        private readonly IDVServices _svc;
        private readonly IRolService _rolSvc;
        private readonly int? _currentUserId;

        private readonly KryptonPanel panel = new KryptonPanel();
        private readonly KryptonButton btnVerificarTodas = new KryptonButton();
        private readonly KryptonButton btnRecalcDVH = new KryptonButton();
        private readonly KryptonButton btnRecalcDVV = new KryptonButton();
        private readonly KryptonButton btnAyuda = new KryptonButton();
        private readonly KryptonDataGridView grid = new KryptonDataGridView();

        private readonly BindingList<FilaResumen> _view = new BindingList<FilaResumen>();

        private static readonly string[] Tablas =
        {
            "Usuario",
            "Familia",
            "UsuarioFamilia",
            "UsuarioPatente",
            "FamiliaPatente"
        };

        private static class Patentes
        {
            public const string DV_CONSULTAR = "DV_CONSULTAR";
            public const string DV_RECALCULAR = "DV_RECALCULAR";
            public const string INTEGRIDAD_VER = "INTEGRIDAD_VER";
            public const string INTEGRIDAD_REP = "INTEGRIDAD_REP";
        }

        // Ctor por defecto (para diseñador)
        public MainDV() : this(new DVService(), new RolService(), null) { }

        // Ctor completo (lo usa tu Menu.cs con 3 args)
        public MainDV(IDVServices service, IRolService rolService, int? currentUserId)
        {
            _svc = service ?? throw new ArgumentNullException(nameof(service));
            _rolSvc = rolService ?? throw new ArgumentNullException(nameof(rolService));
            _currentUserId = currentUserId;

            InitializeComponent();

            Text = "Verificación de Dígitos Verificadores (DVH / DVV)";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(760, 520);

            panel.Dock = DockStyle.Fill;
            Controls.Add(panel);

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

            var bar = new TableLayoutPanel();
            bar.Dock = DockStyle.Top;
            bar.Height = 58;
            bar.ColumnCount = 5;
            bar.RowCount = 1;
            bar.Padding = new Padding(10);
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            btnVerificarTodas.Text = "Verificar TODAS";
            btnRecalcDVH.Text = "Recalcular DVH (sel.)";
            btnRecalcDVV.Text = "Recalcular DVV (sel.)";
            btnAyuda.Text = "Ayuda";

            btnVerificarTodas.Click += (s, e) => BtnVerificarTodas_Click();
            btnRecalcDVH.Click += (s, e) => BtnRecalcDVH_Click();
            btnRecalcDVV.Click += (s, e) => BtnRecalcDVV_Click();
            btnAyuda.Click += (s, e) => MostrarAyuda();

            bar.Controls.Add(btnVerificarTodas, 0, 0);
            bar.Controls.Add(btnRecalcDVH, 1, 0);
            bar.Controls.Add(btnRecalcDVV, 2, 0);
            bar.Controls.Add(btnAyuda, 3, 0);

            panel.Controls.Add(grid);
            panel.Controls.Add(bar);

            CargarFilasVacias();
            Load += MainDV_Load;
        }

        private void AgregarColTexto(string header, string prop, int width)
        {
            var c = new DataGridViewTextBoxColumn
            {
                HeaderText = header,
                DataPropertyName = prop,
                Width = width,
                MinimumWidth = width,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            grid.Columns.Add(c);
        }

        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= grid.Rows.Count) return;
            var row = grid.Rows[e.RowIndex].DataBoundItem as FilaResumen;
            if (row == null) return;

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
            foreach (var t in Tablas)
                _view.Add(new FilaResumen { Tabla = t, DVHEstado = "—", DVVEstado = "—", DVHTexto = "—", DVVTexto = "—" });
        }

        private string TablaSeleccionada()
        {
            if (grid.CurrentRow == null) return null;
            var fr = grid.CurrentRow.DataBoundItem as FilaResumen;
            return fr?.Tabla;
        }

        private int? CurrentUserId => _currentUserId;

        private bool Check(string patente)
        {
            // 🔓 Modo entrega: no bloquea por patentes
            if (FORCE_ENABLE_DV_BUTTONS) return true;

            try
            {
                if (CurrentUserId == null)
                    throw new UnauthorizedAccessException("Sesión no disponible.");
                _rolSvc.ThrowIfNotAllowed(CurrentUserId.Value, patente);
                return true;
            }
            catch
            {
                KryptonMessageBox.Show(this,
                    $"No contás con el permiso requerido: {patente}",
                    "Permisos",
                    KryptonMessageBoxButtons.OK,
                    KryptonMessageBoxIcon.Warning);
                return false;
            }
        }

        private void LogDV(string operacion, string detalle)
        {
            BLL.Bitacora.Info(CurrentUserId, detalle, "Integridad", operacion, host: Environment.MachineName);
        }

        private void RefreshPermisosUI()
        {
            if (FORCE_ENABLE_DV_BUTTONS)
            {
                btnVerificarTodas.Enabled = true;
                btnRecalcDVH.Enabled = true;
                btnRecalcDVV.Enabled = true;
                return;
            }

            var uid = CurrentUserId;
            if (uid == null)
            {
                btnVerificarTodas.Enabled = btnRecalcDVH.Enabled = btnRecalcDVV.Enabled = false;
                return;
            }

            btnVerificarTodas.Enabled = _rolSvc.TienePatente(uid.Value, Patentes.INTEGRIDAD_REP);
            btnRecalcDVH.Enabled = _rolSvc.TienePatente(uid.Value, Patentes.DV_RECALCULAR);
            btnRecalcDVV.Enabled = _rolSvc.TienePatente(uid.Value, Patentes.INTEGRIDAD_VER);
        }

        private void MainDV_Load(object sender, EventArgs e) => RefreshPermisosUI();

        private void BtnVerificarTodas_Click()
        {
            if (!Check(Patentes.INTEGRIDAD_REP)) return;
            LogDV(Patentes.INTEGRIDAD_REP, "Verificar integridad (todas las tablas DVH/DVV)");
            VerificarTodas();
        }

        private void BtnRecalcDVH_Click()
        {
            if (!Check(Patentes.DV_RECALCULAR)) return;

            var tabla = TablaSeleccionada();
            if (string.IsNullOrWhiteSpace(tabla))
            {
                KryptonMessageBox.Show(this, "Seleccioná una tabla del resumen.", "Info",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                return;
            }

            var confirm = KryptonMessageBox.Show(this,
                $"¿Recalcular DVH de todas las filas de '{tabla}'?\n(Luego verificá las tablas o recalculá DVV si corresponde).",
                "Confirmar", KryptonMessageBoxButtons.YesNo, KryptonMessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                LogDV(Patentes.DV_RECALCULAR, $"Recalcular DVH en tabla '{tabla}'");
                int cant = _svc.RecalcularDVH(tabla);

                KryptonMessageBox.Show(this, $"DVH recalculado. Filas actualizadas: {cant}.",
                    "DVH", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);

                VerificarTodas();
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(this, "Error recalculando DVH: " + ex.Message, "Error",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }

        private void BtnRecalcDVV_Click()
        {
            if (!Check(Patentes.INTEGRIDAD_VER)) return;

            var tabla = TablaSeleccionada();
            if (string.IsNullOrWhiteSpace(tabla))
            {
                KryptonMessageBox.Show(this, "Seleccioná una tabla del resumen.", "Info",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                return;
            }

            try
            {
                LogDV(Patentes.INTEGRIDAD_VER, $"Recalcular y guardar DVV en tabla '{tabla}'");
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

        private void VerificarTodas()
        {
            try
            {
                _view.Clear();

                foreach (var t in Tablas)
                {
                    var dif = _svc.VerificarDVH(t);
                    var dvhOk = dif == null || dif.Count == 0;
                    var dvhTxt = dvhOk ? "OK" : $"Error ({dif.Count})";

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

                var resumen = string.Join(Environment.NewLine,
                    _view.Select(r => $"{r.Tabla}: DVH {r.DVHTexto} | DVV {r.DVVTexto}"));

                KryptonMessageBox.Show(this, resumen, "Verificar Todas",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(this, "Error verificando: " + ex.Message, "Error",
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
                "• No se exponen valores de DVH/DVV reales (solo estados).\n" +
                "• Las acciones de recálculo no modifican datos de negocio, únicamente integridad (DVH/DVV).";

            KryptonMessageBox.Show(this, msg, "Ayuda • DVH/DVV",
                KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
        }

        private class FilaResumen
        {
            public string Tabla { get; set; }
            public string DVHEstado { get; set; }
            public string DVVEstado { get; set; }
            public string DVHTexto { get; set; }
            public string DVVTexto { get; set; }
        }
    }
}
