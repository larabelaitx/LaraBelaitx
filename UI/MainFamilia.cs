using System;
using System.Linq;
using System.Windows.Forms;
using Krypton.Toolkit;
using BLL.Contracts;
using DAL;

namespace UI
{
    public partial class MainFamilia : KryptonForm
    {
        private readonly IRolService _rolSvc;
        private readonly FamiliaDao _famDao = FamiliaDao.GetInstance();

        public MainFamilia(IRolService rolSvc)
        {
            InitializeComponent();
            _rolSvc = rolSvc ?? throw new ArgumentNullException(nameof(rolSvc));

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
            EnsureGridConfigured();
            CargarFamilias();
        }

        private void EnsureGridConfigured()
        {
            if (dgvFamilias.Columns.Count > 0) return;

            dgvFamilias.AutoGenerateColumns = false;

            dgvFamilias.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId", DataPropertyName = "Id", Visible = false });
            dgvFamilias.Columns.Add(new DataGridViewTextBoxColumn { Name = "colNombre", HeaderText = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvFamilias.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDescripcion", HeaderText = "Descripción", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvFamilias.Columns.Add(new DataGridViewCheckBoxColumn { Name = "colActiva", HeaderText = "Activa", Width = 65 });

            dgvFamilias.Columns.Add(new DataGridViewButtonColumn { Name = "colVer", HeaderText = "Ver", Text = "Ver", UseColumnTextForButtonValue = true, Width = 70 });
            dgvFamilias.Columns.Add(new DataGridViewButtonColumn { Name = "colEditar", HeaderText = "Editar", Text = "Editar", UseColumnTextForButtonValue = true, Width = 80 });
            dgvFamilias.Columns.Add(new DataGridViewButtonColumn { Name = "colEliminar", HeaderText = "Eliminar", Text = "Eliminar", UseColumnTextForButtonValue = true, Width = 90 });
        }

        private static bool BoolFromAny(object f)
        {
            if (f == null) return true;
            var t = f.GetType();
            foreach (var n in new[] { "Activa", "Activo", "IsEnabled" })
            {
                var p = t.GetProperty(n);
                if (p != null && p.PropertyType == typeof(bool))
                    return (bool)p.GetValue(f);
            }
            return true;
        }

        private void CargarFamilias()
        {
            try
            {
                dgvFamilias.Rows.Clear();

                var familias = _famDao.GetAll();

                // filtros simples
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

                if (Controls.Find("lblTotal", true).FirstOrDefault() is Label lblTotal)
                    lblTotal.Text = $"Total: {familias.Count}";
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show($"Error al cargar familias:\n{ex.Message}", "Familias",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }

        // ----- ABRIR ALTAFAMILIA -----

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            // Alta (id null)
            using (var frm = new AltaFamilia(_rolSvc, null))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    CargarFamilias();
            }
        }

        private void AbrirFamilia(int idFamilia)
        {
            using (var frm = new AltaFamilia(_rolSvc, idFamilia))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    CargarFamilias();
            }
        }

        // ----- GRID: Ver / Editar / Eliminar -----

        private void dgvFamilias_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var grid = (DataGridView)sender;
            var col = grid.Columns[e.ColumnIndex].Name;
            int id = Convert.ToInt32(grid.Rows[e.RowIndex].Cells["colId"].Value);

            if (col == "colVer" || col == "colEditar")
            {
                AbrirFamilia(id); // usa el mismo form (tu AltaFamilia maneja edición por id)
                return;
            }

            if (col == "colEliminar")
            {
                var nombre = grid.Rows[e.RowIndex].Cells["colNombre"].Value?.ToString();
                var dr = KryptonMessageBox.Show(
                    $"¿Eliminar la familia '{nombre}'?",
                    "Confirmar", KryptonMessageBoxButtons.YesNo, KryptonMessageBoxIcon.Question);

                if (dr != DialogResult.Yes) return;

                try
                {
                    var fam = _famDao.GetById(id);
                    _famDao.Delete(fam);

                    KryptonMessageBox.Show("Familia eliminada correctamente.", "Familias",
                        KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                    CargarFamilias();
                }
                catch (Exception ex)
                {
                    KryptonMessageBox.Show($"No se pudo eliminar:\n{ex.Message}", "Familias",
                        KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                }
            }
        }
    }
}
