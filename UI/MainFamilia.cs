using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using BE;
using BLL.Contracts;
using Krypton.Toolkit;

namespace UI
{
    public partial class MainFamilia : KryptonForm
    {
        private readonly IRolService _svcRoles;
        private BindingList<FamiliaVM> _view = new BindingList<FamiliaVM>();

        public MainFamilia(IRolService roles)
        {
            InitializeComponent();
            FixLayout();
            _svcRoles = roles ?? throw new ArgumentNullException(nameof(roles));

            Load += MainFamilia_Load;

            // Nombres de controles según tu captura/designer
            btnAplicar.Click += (s, e) => Refrescar();
            btnLimpiar.Click += (s, e) => LimpiarFiltros();
            btnAgregar.Click += (s, e) => AbrirAltaFamilia(null);
            btnVolver.Click += (s, e) => Close();

            dgvFamilias.CellContentClick += dgvFamilias_CellContentClick;
            dgvFamilias.CellFormatting += dgvFamilias_CellFormatting;
        }
        private void FixLayout()
        {
            // Detecta automáticamente un Panel/KryptonPanel y un DataGridView del formulario
            Control panelArriba = null;
            DataGridView grid = null;

            foreach (Control ctrl in this.Controls)
            {
                if (grid == null && ctrl is DataGridView dg) grid = dg;
                if (panelArriba == null && (ctrl is Panel || ctrl is Krypton.Toolkit.KryptonPanel))
                    panelArriba = ctrl;
            }

            if (panelArriba == null || grid == null) return;

            // Panel arriba
            panelArriba.Dock = DockStyle.Top;
            panelArriba.AutoSize = true; // OK para Panel/KryptonPanel

            // Si preferís forzar altura exacta:
            panelArriba.Height = panelArriba.PreferredSize.Height;

            // Grid llena el resto
            grid.Dock = DockStyle.Fill;
            grid.ScrollBars = ScrollBars.Both;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            // Asegura orden (panel al frente)
            if (panelArriba.Parent == grid.Parent)
                panelArriba.Parent.Controls.SetChildIndex(panelArriba, 0);
        }


        private void MainFamilia_Load(object sender, EventArgs e)
        {
            ConfigurarGrid();
            CargarCombos();
            Refrescar();
        }

        private void CargarCombos()
        {
            var src = new List<ItemBoolCombo>
            {
                new ItemBoolCombo { Texto = "Todos", Valor = null },
                new ItemBoolCombo { Texto = "Sí",     Valor = true },
                new ItemBoolCombo { Texto = "No",     Valor = false }
            };
            cboActivo.DisplayMember = nameof(ItemBoolCombo.Texto);
            cboActivo.ValueMember = nameof(ItemBoolCombo.Valor);
            cboActivo.DataSource = src;
            cboActivo.SelectedIndex = 0;
        }

        private void ConfigurarGrid()
        {
            dgvFamilias.Dock = DockStyle.Fill;
            dgvFamilias.ReadOnly = true;
            dgvFamilias.MultiSelect = false;
            dgvFamilias.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvFamilias.AllowUserToAddRows = false;
            dgvFamilias.AllowUserToDeleteRows = false;
            dgvFamilias.RowHeadersVisible = false;
            dgvFamilias.AutoGenerateColumns = false;
            dgvFamilias.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvFamilias.ScrollBars = ScrollBars.Both;

            if (!dgvFamilias.Columns.Contains("colNombre"))
                dgvFamilias.Columns.Add(new DataGridViewTextBoxColumn { Name = "colNombre", HeaderText = "Nombre", DataPropertyName = nameof(FamiliaVM.Nombre) });

            if (!dgvFamilias.Columns.Contains("colDescripcion"))
                dgvFamilias.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDescripcion", HeaderText = "Descripción", DataPropertyName = nameof(FamiliaVM.Descripcion), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

            if (!dgvFamilias.Columns.Contains("colActiva"))
                dgvFamilias.Columns.Add(new DataGridViewTextBoxColumn { Name = "colActiva", HeaderText = "Activa", DataPropertyName = nameof(FamiliaVM.ActivaTexto) });

            if (!dgvFamilias.Columns.Contains("colPatentes"))
                dgvFamilias.Columns.Add(new DataGridViewButtonColumn
                {
                    Name = "colPatentes",
                    HeaderText = "Patentes",
                    Text = "Patentes (0)",
                    UseColumnTextForButtonValue = false,
                    Width = 110
                });

            if (!dgvFamilias.Columns.Contains("btnVer"))
                dgvFamilias.Columns.Add(new DataGridViewButtonColumn { Name = "btnVer", HeaderText = "Ver", Text = "Ver", UseColumnTextForButtonValue = true, Width = 70 });

            if (!dgvFamilias.Columns.Contains("btnEditar"))
                dgvFamilias.Columns.Add(new DataGridViewButtonColumn { Name = "btnEditar", HeaderText = "Editar", Text = "Editar", UseColumnTextForButtonValue = true, Width = 80 });

            if (!dgvFamilias.Columns.Contains("btnEliminar"))
                dgvFamilias.Columns.Add(new DataGridViewButtonColumn { Name = "btnEliminar", HeaderText = "Eliminar", Text = "Eliminar", UseColumnTextForButtonValue = true, Width = 90 });
        }

        private void Refrescar()
        {
            var familias = (_svcRoles.ListarRoles() ?? Enumerable.Empty<Permiso>())
                           .OfType<Familia>()
                           .ToList();

            string fNombre = (txtNombre.Text ?? "").Trim().ToLowerInvariant();
            bool? fActiva = (cboActivo.SelectedItem as ItemBoolCombo)?.Valor;

            var vms = familias
                .Where(f => string.IsNullOrWhiteSpace(fNombre) || (f.Name ?? "").ToLowerInvariant().Contains(fNombre))
                .Where(f => !fActiva.HasValue || (GetBool(f) == fActiva.Value))
                .OrderBy(f => f.Name ?? "")
                .Select(f => new FamiliaVM
                {
                    Id = f.Id,
                    Nombre = f.Name,
                    Descripcion = f.Descripcion,
                    Activa = GetBool(f)
                })
                .ToList();

            _view = new BindingList<FamiliaVM>(vms);
            dgvFamilias.DataSource = _view;
        }

        private static bool GetBool(Familia f)
        {
            // soporte para distintos nombres de propiedad booleana
            if (f == null) return true;
            var t = f.GetType();
            foreach (var name in new[] { "Activa", "Activo", "IsEnabled" })
            {
                var p = t.GetProperty(name);
                if (p != null && p.PropertyType == typeof(bool))
                    return (bool)p.GetValue(f);
            }
            return true;
        }

        private void dgvFamilias_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgvFamilias.Columns[e.ColumnIndex].Name != "colPatentes") return;

            var vm = dgvFamilias.Rows[e.RowIndex].DataBoundItem as FamiliaVM;
            if (vm == null) return;

            int cant = 0;
            try { cant = _svcRoles.GetPatentesDeFamilia(vm.Id)?.Count ?? 0; } catch { }
            dgvFamilias.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = $"Patentes ({cant})";
        }

        private void dgvFamilias_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var col = dgvFamilias.Columns[e.ColumnIndex].Name;
            var vm = dgvFamilias.Rows[e.RowIndex].DataBoundItem as FamiliaVM;
            if (vm == null) return;

            switch (col)
            {
                case "btnVer":
                    AbrirAltaFamilia(vm.Id, ModoForm.Ver);
                    break;
                case "btnEditar":
                    AbrirAltaFamilia(vm.Id, ModoForm.Editar);
                    break;
                case "btnEliminar":
                    EliminarFamilia(vm);
                    break;
                case "colPatentes":
                    AbrirDialogoPatentes(vm.Id);
                    break;
            }
        }

        private void LimpiarFiltros()
        {
            txtNombre.Clear();
            cboActivo.SelectedIndex = 0;
            Refrescar();
        }

        private void AbrirAltaFamilia(int? idFamilia, ModoForm modo = ModoForm.Alta)
        {
            // Si tu AltaFamilia tiene ctor (IRolService,int?,ModoForm) usalo;
            // si no, cae al ctor vacío para no romper la compilación.
            KryptonForm frm = null;

            var t = typeof(AltaFamilia);
            var ctor3 = t.GetConstructor(new[] { typeof(IRolService), typeof(int?), typeof(ModoForm) });
            if (ctor3 != null) frm = (KryptonForm)ctor3.Invoke(new object[] { _svcRoles, idFamilia, modo });
            else
            {
                var ctor0 = t.GetConstructor(Type.EmptyTypes);
                if (ctor0 == null)
                {
                    KryptonMessageBox.Show("AltaFamilia no tiene un constructor compatible.", "Familias",
                        KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
                    return;
                }
                frm = (KryptonForm)ctor0.Invoke(null);
            }

            using (frm)
            {
                frm.StartPosition = FormStartPosition.CenterParent;
                if (frm.ShowDialog(this) == DialogResult.OK)
                    Refrescar();
            }
        }

        private void AbrirDialogoPatentes(int idFamilia)
        {
            // Si aún no hiciste el diálogo, dejá este mensaje.
            KryptonMessageBox.Show("Próximamente: gestión de patentes de la familia.", "Familias",
                KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
        }

        private void EliminarFamilia(FamiliaVM vm)
        {
            var r = KryptonMessageBox.Show(
                $"¿Eliminar la familia '{vm.Nombre}'?\nSe validará que ninguna patente quede huérfana.",
                "Confirmar eliminación",
                KryptonMessageBoxButtons.YesNo,
                KryptonMessageBoxIcon.Warning);

            if (r != DialogResult.Yes) return;

            try
            {
                if (_svcRoles.EliminarFamilia(vm.Id))
                {
                    KryptonMessageBox.Show("Familia eliminada.", "Familias");
                    Refrescar();
                }
                else
                {
                    KryptonMessageBox.Show("No se pudo eliminar la familia (sin cambios).", "Familias",
                        KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show("No se pudo eliminar la familia.\n\n" + ex.Message, "Error",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }
    }

    public class FamiliaVM
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Activa { get; set; }
        public string ActivaTexto => Activa ? "Sí" : "No";
    }

    public class ItemBoolCombo
    {
        public string Texto { get; set; }
        public bool? Valor { get; set; }
    }
}
