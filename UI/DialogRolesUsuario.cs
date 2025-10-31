using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Krypton.Toolkit;
using BE;
using BLL.Services;   // RolService
using BLL.Contracts;  // IRolService

namespace UI
{
    /// <summary>
    /// Diálogo simple para asignar Familias (roles) a un Usuario.
    /// - Muestra un CheckedListBox con todas las Familias.
    /// - Marca las familias iniciales recibidas por ctor.
    /// - Devuelve la selección en FamiliasSeleccionadasIds cuando se pulsa Guardar.
    /// </summary>
    public partial class DialogRolesUsuario : KryptonForm
    {
        // ==== API expuesta (usada por MainUsuarios) ====
        public List<int> FamiliasSeleccionadasIds { get; private set; } = new List<int>();

        // ==== Dependencias ====
        private readonly Usuario _usuario;
        private readonly IRolService _rolSvc;

        // ==== Controles (creados por código para evitar Designer) ====
        private CheckedListBox chkListFamilias;
        private KryptonButton btnGuardar;
        private KryptonButton btnCancelar;
        private KryptonTextBox txtFiltro;
        private KryptonLabel lblFiltro;
        private KryptonLabel lblTitulo;

        // Lista en memoria para filtrar sin perder items
        private List<Familia> _todasFamilias = new List<Familia>();
        private HashSet<int> _iniciales = new HashSet<int>();

        // ==== Constructores compatibles con tu uso actual ====
        public DialogRolesUsuario()
        {
            InitializeComponent();
            // Si alguien usa el ctor vacío desde el Designer, proveemos defaults seguros
            _rolSvc = new RolService();
            _usuario = null;
            BuildUi();
            this.Load += DialogRolesUsuario_Load;
        }

        public DialogRolesUsuario(Usuario usuario, IEnumerable<int> familiasInicialesIds = null)
            : this()
        {
            _usuario = usuario;
            _iniciales = new HashSet<int>(familiasInicialesIds ?? Enumerable.Empty<int>());
        }

        // (Opcional) Si en algún momento querés inyectar el service:
        public DialogRolesUsuario(Usuario usuario, IRolService rolService, IEnumerable<int> familiasInicialesIds = null)
        {
            InitializeComponent();
            _usuario = usuario;
            _rolSvc = rolService ?? new RolService();
            _iniciales = new HashSet<int>(familiasInicialesIds ?? Enumerable.Empty<int>());
            BuildUi();
            this.Load += DialogRolesUsuario_Load;
        }

        // ==== UI por código ====
        private void BuildUi()
        {
            this.Text = "Asignar Roles (Familias) a Usuario";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Width = 520;
            this.Height = 520;

            lblTitulo = new KryptonLabel
            {
                Left = 20,
                Top = 12,
                Width = 460,
                Text = "Seleccioná las Familias (roles) a asignar"
            };

            lblFiltro = new KryptonLabel
            {
                Left = 20,
                Top = 44,
                Width = 80,
                Text = "Filtrar:"
            };

            txtFiltro = new KryptonTextBox
            {
                Left = 80,
                Top = 40,
                Width = 400
            };
            txtFiltro.TextChanged += (s, e) => AplicarFiltro(txtFiltro.Text);

            chkListFamilias = new CheckedListBox
            {
                Left = 20,
                Top = 75,
                Width = 460,
                Height = 340,
                CheckOnClick = true
            };

            btnGuardar = new KryptonButton
            {
                Left = 280,
                Top = 430,
                Width = 95,
                Text = "Guardar"
            };
            btnGuardar.Click += btnGuardar_Click;

            btnCancelar = new KryptonButton
            {
                Left = 385,
                Top = 430,
                Width = 95,
                Text = "Cancelar"
            };
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblFiltro);
            this.Controls.Add(txtFiltro);
            this.Controls.Add(chkListFamilias);
            this.Controls.Add(btnGuardar);
            this.Controls.Add(btnCancelar);
        }

        private void DialogRolesUsuario_Load(object sender, EventArgs e)
        {
            try
            {
                // Cargar todas las familias desde el servicio
                var svc = _rolSvc ?? new RolService();
                var roles = svc.ListarRoles() ?? Enumerable.Empty<BE.Permiso>();
                _todasFamilias = roles.OfType<Familia>()
                                      .OrderBy(f => f.Name ?? string.Empty)
                                      .ToList();

                // Render inicial
                RefrescarLista(_todasFamilias, _iniciales);
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(
                    "No se pudieron cargar las familias.\n" + ex.Message,
                    "Roles", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }

        private void RefrescarLista(IEnumerable<Familia> familias, HashSet<int> marcar)
        {
            // Mantener selección previa del control
            var yaMarcadas = new HashSet<int>(
                chkListFamilias.CheckedItems.Cast<ItemFamilia>().Select(i => i.Id));

            chkListFamilias.BeginUpdate();
            chkListFamilias.Items.Clear();

            foreach (var f in familias)
            {
                var item = new ItemFamilia(f.Id, f.Name);
                int index = chkListFamilias.Items.Add(item);

                bool debeMarcar = marcar.Contains(f.Id) || yaMarcadas.Contains(f.Id);
                if (debeMarcar)
                    chkListFamilias.SetItemChecked(index, true);
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

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // Wrapper simple para mostrar texto en CheckedListBox y guardar Id
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
