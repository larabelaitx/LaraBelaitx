using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BE;
using BLL.Services;
using Krypton.Toolkit;
using BLL.Contracts;

namespace UI
{
    public partial class DialogRolesUsuario : KryptonForm
    {
        public List<int> FamiliasSeleccionadasIds { get; private set; } = new List<int>();

        private readonly Usuario _usuario;
        private readonly IRolService _rolSvc;

        private CheckedListBox chkListFamilias;
        private KryptonButton btnGuardar;
        private KryptonButton btnCancelar;
        private KryptonTextBox txtFiltro;
        private KryptonLabel lblFiltro;
        private KryptonLabel lblTitulo;

        private List<Familia> _todasFamilias = new List<Familia>();
        private HashSet<int> _iniciales = new HashSet<int>();

        public DialogRolesUsuario()
        {
            InitializeComponent();
            _rolSvc = new RolService();
            _usuario = null;
            BuildUi();
            Load += DialogRolesUsuario_Load;
        }

        public DialogRolesUsuario(Usuario usuario, IEnumerable<int> familiasInicialesIds = null)
            : this()
        {
            _usuario = usuario;
            _iniciales = new HashSet<int>(familiasInicialesIds ?? Enumerable.Empty<int>());
        }

        public DialogRolesUsuario(Usuario usuario, IRolService rolService, IEnumerable<int> familiasInicialesIds = null)
        {
            InitializeComponent();
            _usuario = usuario;
            _rolSvc = rolService ?? new RolService();
            _iniciales = new HashSet<int>(familiasInicialesIds ?? Enumerable.Empty<int>());
            BuildUi();
            Load += DialogRolesUsuario_Load;
        }

        private void BuildUi()
        {
            Text = "Asignar Roles (Familias) a Usuario";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Width = 520;
            Height = 520;

            lblTitulo = new KryptonLabel { Left = 20, Top = 12, Width = 460, Text = "Seleccioná las Familias (roles) a asignar" };
            lblFiltro = new KryptonLabel { Left = 20, Top = 44, Width = 80, Text = "Filtrar:" };

            txtFiltro = new KryptonTextBox { Left = 80, Top = 40, Width = 400 };
            txtFiltro.TextChanged += (s, e) => AplicarFiltro(txtFiltro.Text);

            chkListFamilias = new CheckedListBox { Left = 20, Top = 75, Width = 460, Height = 340, CheckOnClick = true };

            btnGuardar = new KryptonButton { Left = 280, Top = 430, Width = 95, Text = "Guardar" };
            btnGuardar.Click += btnGuardar_Click;

            btnCancelar = new KryptonButton { Left = 385, Top = 430, Width = 95, Text = "Cancelar" };
            btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(lblTitulo);
            Controls.Add(lblFiltro);
            Controls.Add(txtFiltro);
            Controls.Add(chkListFamilias);
            Controls.Add(btnGuardar);
            Controls.Add(btnCancelar);
        }

        private void DialogRolesUsuario_Load(object sender, EventArgs e)
        {
            try
            {
                var roles = _rolSvc.ListarRoles() ?? Enumerable.Empty<Familia>();
                _todasFamilias = roles.OrderBy(f => f.Name ?? string.Empty).ToList();
                RefrescarLista(_todasFamilias, _iniciales);
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show("No se pudieron cargar las familias.\n" + ex.Message,
                    "Roles", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }

        private void RefrescarLista(IEnumerable<Familia> familias, HashSet<int> marcar)
        {
            var yaMarcadas = new HashSet<int>(
                chkListFamilias.CheckedItems.Cast<ItemFamilia>().Select(i => i.Id));

            chkListFamilias.BeginUpdate();
            chkListFamilias.Items.Clear();

            foreach (var f in familias)
            {
                var item = new ItemFamilia(f.Id, f.Name);
                int index = chkListFamilias.Items.Add(item);
                bool debeMarcar = marcar.Contains(f.Id) || yaMarcadas.Contains(f.Id);
                if (debeMarcar) chkListFamilias.SetItemChecked(index, true);
            }
            chkListFamilias.EndUpdate();
        }

        private void AplicarFiltro(string texto)
        {
            texto = (texto ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(texto))
            {
                RefrescarLista(_todasFamilias, _iniciales);
                return;
            }

            var filtradas = _todasFamilias
                .Where(f => (f.Name ?? "").ToLowerInvariant().Contains(texto))
                .ToList();

            RefrescarLista(filtradas, _iniciales);
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            FamiliasSeleccionadasIds = chkListFamilias.CheckedItems
                .Cast<ItemFamilia>()
                .Select(i => i.Id)
                .Distinct()
                .ToList();

            DialogResult = DialogResult.OK;
            Close();
        }

        private sealed class ItemFamilia
        {
            public int Id { get; }
            public string Nombre { get; }
            public ItemFamilia(int id, string nombre)
            {
                Id = id;
                Nombre = nombre ?? "(sin nombre)";
            }
            public override string ToString() => Nombre;
        }
    }
}
