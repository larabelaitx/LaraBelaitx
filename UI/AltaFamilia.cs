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
    public partial class AltaFamilia : KryptonForm
    {
        private readonly IRolService _roles;
        private readonly int? _idFamilia;

        private readonly BindingList<Patente> _disponibles = new BindingList<Patente>();
        private readonly BindingList<Patente> _asignadas = new BindingList<Patente>();

        private Familia _familia;

        public AltaFamilia(IRolService roles, int? idFamilia = null)
        {
            InitializeComponent();
            _roles = roles ?? throw new ArgumentNullException(nameof(roles));
            _idFamilia = idFamilia;

            // eventos
            this.Load += AltaFamilia_Load;

            btnAdd.Click += (s, e) => MoverSeleccion(lstDisponibles, _disponibles, _asignadas);
            btnAddAll.Click += (s, e) => MoverTodos(_disponibles, _asignadas);
            btnRemove.Click += (s, e) => MoverSeleccion(lstAsignadas, _asignadas, _disponibles);
            btnRemoveAll.Click += (s, e) => MoverTodos(_asignadas, _disponibles);

            btnGuardar.Click += btnGuardar_Click;
            btnCancelar.Click += (s, e) => Close();
            btnVolver.Click += (s, e) => Close();
        }

        // ===== helpers de reflexión tolerante =====
        private static string GetStr(object o, params string[] names)
        {
            if (o == null) return null;
            foreach (var n in names)
            {
                var p = o.GetType().GetProperty(n);
                if (p != null) return p.GetValue(o)?.ToString();
            }
            return null;
        }
        private static void SetStr(object o, string value, params string[] names)
        {
            if (o == null) return;
            foreach (var n in names)
            {
                var p = o.GetType().GetProperty(n);
                if (p != null && p.CanWrite && p.PropertyType == typeof(string))
                {
                    p.SetValue(o, value);
                    return;
                }
            }
        }
        private static bool GetBool(object o, bool def, params string[] names)
        {
            if (o == null) return def;
            foreach (var n in names)
            {
                var p = o.GetType().GetProperty(n);
                if (p != null && p.PropertyType == typeof(bool))
                    return (bool)p.GetValue(o);
            }
            return def;
        }
        private static void SetBool(object o, bool val, params string[] names)
        {
            if (o == null) return;
            foreach (var n in names)
            {
                var p = o.GetType().GetProperty(n);
                if (p != null && p.CanWrite && p.PropertyType == typeof(bool))
                {
                    p.SetValue(o, val);
                    return;
                }
            }
        }

        // ===== carga inicial =====
        private void AltaFamilia_Load(object sender, EventArgs e)
        {
            // listboxes
            lstDisponibles.DataSource = _disponibles;
            lstDisponibles.DisplayMember = nameof(Patente.Name);
            lstDisponibles.ValueMember = nameof(Patente.Id);
            lstDisponibles.SelectionMode = SelectionMode.MultiExtended;

            lstAsignadas.DataSource = _asignadas;
            lstAsignadas.DisplayMember = nameof(Patente.Name);
            lstAsignadas.ValueMember = nameof(Patente.Id);
            lstAsignadas.SelectionMode = SelectionMode.MultiExtended;

            try
            {
                if (_idFamilia.HasValue)
                {
                    _familia = _roles.GetFamilia(_idFamilia.Value);
                    if (_familia == null)
                    {
                        KryptonMessageBox.Show(this, "No se encontró la familia indicada.", "Familias",
                       buttons: KryptonMessageBoxButtons.OK,
                       icon: KryptonMessageBoxIcon.Information);
                        return;
                    }

                    txtNombre.Text = GetStr(_familia, "Name", "Nombre") ?? "";
                    txtDescripcion.Text = GetStr(_familia, "Descripcion", "Description") ?? "";
                    chkActiva.Checked = GetBool(_familia, true, "Activa", "Activo", "IsEnabled");
                }
                else
                {
                    chkActiva.Checked = true;
                }

                var todas = _roles.GetPatentes() ?? new List<Patente>();

                var idsAsignadas = (_idFamilia.HasValue
                                    ? _roles.GetPatentesDeFamilia(_idFamilia.Value).Select(p => p.Id)
                                    : Enumerable.Empty<int>())
                                    .ToHashSet();

                foreach (var p in todas.Where(p => !idsAsignadas.Contains(p.Id)).OrderBy(p => p.Name))
                    _disponibles.Add(p);

                foreach (var p in todas.Where(p => idsAsignadas.Contains(p.Id)).OrderBy(p => p.Name))
                    _asignadas.Add(p);

                ActualizarHabilitadoComposicion();
                chkActiva.CheckedChanged += (s, a) => ActualizarHabilitadoComposicion();
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(this,
                    "Error cargando datos: " + ex.Message,
                    "Familias",
                    buttons: KryptonMessageBoxButtons.OK,
                    icon: KryptonMessageBoxIcon.Error);
                Close();
            }
        }

        private void ActualizarHabilitadoComposicion()
        {
            bool habilitar = chkActiva.Checked;
            lstDisponibles.Enabled = lstAsignadas.Enabled = habilitar;
            btnAdd.Enabled = btnAddAll.Enabled = btnRemove.Enabled = btnRemoveAll.Enabled = habilitar;
        }

        // ===== mover ítems =====
        private static void MoverSeleccion(
            KryptonListBox origenList,              // <- antes era ListBox
            BindingList<Patente> origen,
            BindingList<Patente> destino)
        {
            // KryptonListBox también expone SelectedItems
            var items = origenList.SelectedItems.Cast<Patente>().ToList();
            if (!items.Any()) return;

            foreach (var it in items)
            {
                if (!destino.Any(p => p.Id == it.Id))
                    destino.Add(it);

                var match = origen.FirstOrDefault(p => p.Id == it.Id);
                if (match != null) origen.Remove(match);
            }
        }

        private static void MoverTodos(BindingList<Patente> origen, BindingList<Patente> destino)
        {
            var items = origen.ToList();
            foreach (var it in items)
            {
                if (!destino.Any(p => p.Id == it.Id))
                    destino.Add(it);
                origen.Remove(it);
            }
        }

        // ===== guardar =====
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                var nombre = (txtNombre.Text ?? "").Trim();
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    KryptonMessageBox.Show(this, "El nombre es obligatorio.", "Familias",
                       buttons: KryptonMessageBoxButtons.OK,
                       icon: KryptonMessageBoxIcon.Warning);
                    txtNombre.Focus();
                    return;
                }

                var entidad = _familia ?? new Familia();

                SetStr(entidad, nombre, "Name", "Nombre");
                SetStr(entidad, (txtDescripcion.Text ?? "").Trim(), "Descripcion", "Description");
                SetBool(entidad, chkActiva.Checked, "Activa", "Activo", "IsEnabled");

                var patentesIds = _asignadas.Select(p => p.Id).Distinct().ToList();

                if (_familia == null)
                {
                    int nuevoId = _roles.CrearFamilia(entidad);
                    _roles.SetPatentesDeFamilia(nuevoId, patentesIds);
                }
                else
                {
                    entidad.Id = _familia.Id;
                    _roles.ActualizarFamilia(entidad);
                    _roles.SetPatentesDeFamilia(entidad.Id, patentesIds);
                }

                     KryptonMessageBox.Show(this,
                    "Familia guardada correctamente.",
                    "Éxito",
                    buttons: KryptonMessageBoxButtons.OK,
                    icon: KryptonMessageBoxIcon.Information);

                    DialogResult = DialogResult.OK;
                    Close();
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(this,
                "No se pudo guardar la familia:\n" + ex.Message,
                "Error",
                buttons: KryptonMessageBoxButtons.OK,
                icon: KryptonMessageBoxIcon.Error);
            }
        }
    }
}
