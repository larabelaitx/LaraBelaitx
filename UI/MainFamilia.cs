// UI/MainFamilia.cs
using System;
using System.Linq;
using System.Windows.Forms;
using Krypton.Toolkit;
using BLL.Contracts;
using DAL;
using UI.Seguridad;   // Perms

namespace UI
{
    public partial class MainFamilia : KryptonForm
    {
        private readonly IRolService _rolSvc;
        private readonly FamiliaDao _famDao = FamiliaDao.GetInstance();
        private readonly int _uid;

        public MainFamilia(IRolService rolSvc, int currentUserId)
        {
            InitializeComponent();
            _rolSvc = rolSvc ?? throw new ArgumentNullException(nameof(rolSvc));
            _uid = currentUserId;

            Load += MainFamilia_Load;

            btnAplicar.Click += (s, e) => CargarFamilias();
            btnLimpiar.Click += (s, e) =>
            {
                txtNombre.Clear();
                cboActivo.SelectedIndex = -1;
                CargarFamilias();
            };
            btnAgregar.Click += btnAgregar_Click;

            dgvFamilias.CellContentClick += dgvFamilias_CellContentClick;
        }

        private void MainFamilia_Load(object sender, EventArgs e)
        {
            if (!_rolSvc.TienePatente(_uid, Perms.Familia_Listar)) { Close(); return; }

            EnsureGridConfigured();

            btnAgregar.Enabled = _rolSvc.TienePatente(_uid, Perms.Familia_Alta);
            if (dgvFamilias.Columns.Contains("colEditar"))
                dgvFamilias.Columns["colEditar"].Visible = _rolSvc.TienePatente(_uid, Perms.Familia_Editar);
            if (dgvFamilias.Columns.Contains("colEliminar"))
                dgvFamilias.Columns["colEliminar"].Visible = _rolSvc.TienePatente(_uid, Perms.Familia_Eliminar);

            CargarFamilias();

            dgvFamilias.CellDoubleClick += (s, ev) =>
            {
                if (ev.RowIndex >= 0 && dgvFamilias.Columns.Contains("colId"))
                {
                    var val = dgvFamilias.Rows[ev.RowIndex].Cells["colId"].Value;
                    if (val != null && int.TryParse(val.ToString(), out var id))
                        AbrirFamilia(id, soloLectura: true); // doble click = Ver
                }
            };
        }

        private void EnsureGridConfigured()
        {
            if (dgvFamilias.Columns.Count > 0) return;

            dgvFamilias.AutoGenerateColumns = false;
            dgvFamilias.RowHeadersVisible = false;
            dgvFamilias.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvFamilias.MultiSelect = false;
            dgvFamilias.ReadOnly = true;

            dgvFamilias.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId", DataPropertyName = "Id", Visible = false });
            dgvFamilias.Columns.Add(new DataGridViewTextBoxColumn { Name = "colNombre", HeaderText = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvFamilias.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDescripcion", HeaderText = "Descripción", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvFamilias.Columns.Add(new DataGridViewCheckBoxColumn { Name = "colActiva", HeaderText = "Activa", Width = 65 });

            dgvFamilias.Columns.Add(new DataGridViewButtonColumn { Name = "colVer", HeaderText = "Ver", Text = "Ver", UseColumnTextForButtonValue = true, Width = 70 });
            dgvFamilias.Columns.Add(new DataGridViewButtonColumn { Name = "colEditar", HeaderText = "Editar", Text = "Editar", UseColumnTextForButtonValue = true, Width = 80 });
            dgvFamilias.Columns.Add(new DataGridViewButtonColumn { Name = "colEliminar", HeaderText = "Eliminar", Text = "Eliminar", UseColumnTextForButtonValue = true, Width = 90 });

            foreach (DataGridViewColumn c in dgvFamilias.Columns)
                if (c is DataGridViewButtonColumn) c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private static bool BoolFromAny(object f)
        {
            if (f == null) return true;
            var t = f.GetType();
            foreach (var n in new[] { "Activa", "Activo", "IsEnabled" })
            {
                var p = t.GetProperty(n);
                if (p != null && p.PropertyType == typeof(bool))
                    return (bool)p.GetValue(f, null);
            }
            return true;
        }

        private void CargarFamilias()
        {
            dgvFamilias.Rows.Clear();

            var familias = _famDao.GetAll();

            var nombre = (txtNombre.Text ?? "").Trim();
            if (!string.IsNullOrEmpty(nombre))
                familias = familias.Where(f => (f.Name ?? "").IndexOf(nombre, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            if (cboActivo.SelectedIndex >= 0) // 0=Sí, 1=No, -1=Todos
            {
                var want = cboActivo.SelectedIndex == 0;
                familias = familias.Where(f => BoolFromAny(f) == want).ToList();
            }

            foreach (var f in familias)
                dgvFamilias.Rows.Add(f.Id, f.Name, f.Descripcion, BoolFromAny(f));

            var lblTotal = Controls.Find("lblTotal", true).FirstOrDefault() as Label;
            if (lblTotal != null) lblTotal.Text = "Total: " + familias.Count;
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!_rolSvc.TienePatente(_uid, Perms.Familia_Alta)) return;

            using (var frm = new AltaFamilia(_rolSvc, null, _uid, soloLectura: false))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    CargarFamilias();
            }
        }

        private void AbrirFamilia(int idFamilia, bool soloLectura)
        {
            using (var frm = new AltaFamilia(_rolSvc, idFamilia, _uid, soloLectura))
            {
                if (!soloLectura)
                {
                    if (frm.ShowDialog(this) == DialogResult.OK)
                        CargarFamilias();
                }
                else
                {
                    frm.ShowDialog(this);
                }
            }
        }

        private void dgvFamilias_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var grid = (DataGridView)sender;
            var col = grid.Columns[e.ColumnIndex].Name;
            if (!grid.Columns.Contains("colId")) return;

            var rawId = grid.Rows[e.RowIndex].Cells["colId"].Value;
            if (rawId == null || !int.TryParse(rawId.ToString(), out var id)) return;

            if (col == "colVer")
            {
                AbrirFamilia(id, soloLectura: true);
                return;
            }

            if (col == "colEditar")
            {
                if (!_rolSvc.TienePatente(_uid, Perms.Familia_Editar)) return;
                AbrirFamilia(id, soloLectura: false);
                return;
            }

            if (col == "colEliminar")
            {
                if (!_rolSvc.TienePatente(_uid, Perms.Familia_Eliminar)) return;

                var nombre = grid.Rows[e.RowIndex].Cells["colNombre"].Value?.ToString();
                var dr = KryptonMessageBox.Show(
                    $"¿Eliminar la familia '{nombre}'?",
                    "Confirmar", KryptonMessageBoxButtons.YesNo, KryptonMessageBoxIcon.Question);

                if (dr != DialogResult.Yes) return;

                var fam = _famDao.GetById(id);
                if (fam == null) { CargarFamilias(); return; }

                try
                {
                    // Firma nueva: Delete(Familia familia, DVH dvh) -> bool
                    var ok = _famDao.Delete(fam, null);
                    if (!ok)
                        KryptonMessageBox.Show(this, "No se pudo eliminar la familia.", "Familias",
                            KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    KryptonMessageBox.Show(this,
                        "No se pudo eliminar la familia.\n\n" + ex.Message,
                        "Familias", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
                }

                CargarFamilias();
            }
        }
    }
}
