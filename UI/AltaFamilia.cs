using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            this.Load += AltaFamilia_Load;

            btnAdd.Click += (s, e) => MoverSeleccion(lstDisponibles, _disponibles, _asignadas);
            btnAddAll.Click += (s, e) => MoverTodos(_disponibles, _asignadas);
            btnRemove.Click += (s, e) => MoverSeleccion(lstAsignadas, _asignadas, _disponibles);
            btnRemoveAll.Click += (s, e) => MoverTodos(_asignadas, _disponibles);

            btnGuardar.Click += btnGuardar_Click;
        }

        private void AltaFamilia_Load(object sender, EventArgs e)
        {
            lstDisponibles.DataSource = _disponibles;
            lstDisponibles.DisplayMember = nameof(Patente.Name);
            lstDisponibles.ValueMember = nameof(Patente.Id);
            lstDisponibles.SelectionMode = SelectionMode.MultiExtended;

            lstAsignadas.DataSource = _asignadas;
            lstAsignadas.DisplayMember = nameof(Patente.Name);
            lstAsignadas.ValueMember = nameof(Patente.Id);
            lstAsignadas.SelectionMode = SelectionMode.MultiExtended;

            if (_idFamilia.HasValue)
            {
                _familia = _roles.GetFamilia(_idFamilia.Value);
                if (_familia == null)
                {
                    KryptonMessageBox.Show("No se encontró la familia indicada.", "Familias");
                    Close();
                    return;
                }

                txtNombre.Text = _familia.Name ?? _familia.GetType().GetProperty("Nombre")?.GetValue(_familia)?.ToString();
                var desc = _familia.GetType().GetProperty("Descripcion")?.GetValue(_familia)?.ToString();
                txtDescripcion.Text = desc ?? (_familia.GetType().GetProperty("Description")?.GetValue(_familia)?.ToString() ?? string.Empty);

                var propActiva = _familia.GetType().GetProperty("Activa");
                chkActiva.Checked = propActiva != null && (bool)propActiva.GetValue(_familia);
            }
            else
            {
                chkActiva.Checked = true;
            }

            var todas = _roles.GetPatentes() ?? new List<Patente>();

            var setAsignadas = (_idFamilia.HasValue
                                ? _roles.GetPatentesDeFamilia(_idFamilia.Value).Select(p => p.Id)
                                : Enumerable.Empty<int>())
                                .ToHashSet();

            foreach (var p in todas.Where(p => !setAsignadas.Contains(p.Id)).OrderBy(p => p.Name))
                _disponibles.Add(p);
            foreach (var p in todas.Where(p => setAsignadas.Contains(p.Id)).OrderBy(p => p.Name))
                _asignadas.Add(p);

            ActualizarHabilitadoComposicion();
            chkActiva.CheckedChanged += (s, a) => ActualizarHabilitadoComposicion();
        }

        private void ActualizarHabilitadoComposicion()
        {
            bool habilitar = chkActiva.Checked;
            lstDisponibles.Enabled = lstAsignadas.Enabled = habilitar;
            btnAdd.Enabled = btnAddAll.Enabled = btnRemove.Enabled = btnRemoveAll.Enabled = habilitar;
        }

        private static void MoverSeleccion(KryptonListBox origenList,
                                           BindingList<Patente> origen,
                                           BindingList<Patente> destino)
        {
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

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            var nombre = (txtNombre.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(nombre))
            {
                KryptonMessageBox.Show("El nombre es obligatorio.", "Familias");
                txtNombre.Focus();
                return;
            }

            var entidad = _familia ?? new Familia();

            var propNombre = entidad.GetType().GetProperty("Name") ?? entidad.GetType().GetProperty("Nombre");
            propNombre?.SetValue(entidad, nombre);

            var propDesc = entidad.GetType().GetProperty("Descripcion") ?? entidad.GetType().GetProperty("Description");
            propDesc?.SetValue(entidad, txtDescripcion.Text?.Trim());

            var propActiva = entidad.GetType().GetProperty("Activa");
            propActiva?.SetValue(entidad, chkActiva.Checked);

            var patentesIds = _asignadas.Select(p => p.Id).ToList();

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

            KryptonMessageBox.Show("Familia guardada correctamente.", "Éxito");
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
