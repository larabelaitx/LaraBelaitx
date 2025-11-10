using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Krypton.Toolkit;
using BE;
using BLL.Contracts;

namespace UI
{
    public partial class AltaFamilia : KryptonForm
    {
        private readonly IRolService _roles;
        private readonly int? _idFamilia;
        private readonly int? _currentUserId;
        private readonly bool _soloLectura;

        private readonly BindingList<Patente> _disponibles = new BindingList<Patente>();
        private readonly BindingList<Patente> _asignadas = new BindingList<Patente>();

        private Familia _familia;

        private const string P_FAM_ABM = "FAMILIAS_ABM";

        // >>> NUEVO: parametro soloLectura <<<
        public AltaFamilia(IRolService roles, int? idFamilia = null, int? currentUserId = null, bool soloLectura = false)
        {
            InitializeComponent();

            StartPosition = FormStartPosition.CenterParent;

            _roles = roles ?? throw new ArgumentNullException(nameof(roles));
            _idFamilia = idFamilia;
            _currentUserId = currentUserId;
            _soloLectura = soloLectura;

            Load += AltaFamilia_Load;

            btnAdd.Click += (s, e) => MoverSeleccion(lstDisponibles, _disponibles, _asignadas);
            btnAddAll.Click += (s, e) => MoverTodos(_disponibles, _asignadas);
            btnRemove.Click += (s, e) => MoverSeleccion(lstAsignadas, _asignadas, _disponibles);
            btnRemoveAll.Click += (s, e) => MoverTodos(_asignadas, _disponibles);

            btnGuardar.Click += btnGuardar_Click;
            btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            btnVolver.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
        }

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
                { p.SetValue(o, value); return; }
            }
        }

        private bool CheckAllowed(string patente)
        {
            if (!_currentUserId.HasValue || _currentUserId.Value <= 0 || !_roles.TienePatente(_currentUserId.Value, patente))
            {
                KryptonMessageBox.Show(this,
                    $"No tenés permiso para acceder a esta funcionalidad.\n({patente})",
                    "Acceso denegado",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
            }
            return _currentUserId.HasValue && _currentUserId.Value > 0 && _roles.TienePatente(_currentUserId.Value, patente);
        }

        private void AltaFamilia_Load(object sender, EventArgs e)
        {
            // En modo Ver, permitimos abrir SIN exigir P_FAM_ABM (sólo lectura);
            // en modos Alta/Editar, sí exigimos P_FAM_ABM.
            if (!_soloLectura)
            {
                if (!CheckAllowed(P_FAM_ABM)) { Close(); return; }
            }

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
                            KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                        Close();
                        return;
                    }

                    txtNombre.Text = GetStr(_familia, "Name", "Nombre") ?? "";
                    txtDescripcion.Text = GetStr(_familia, "Descripcion") ?? "";
                    chkActiva.Checked = true;
                }
                else
                {
                    chkActiva.Checked = true;
                }

                var todas = (_roles.GetPatentes() ?? new List<Patente>()).ToList();
                var idsAsignadas = (_idFamilia.HasValue
                                    ? (_roles.GetPatentesDeFamilia(_idFamilia.Value) ?? new List<Patente>()).Select(p => p.Id)
                                    : Enumerable.Empty<int>())
                                   .ToHashSet();

                _disponibles.Clear();
                _asignadas.Clear();

                foreach (var p in todas.Where(p => !idsAsignadas.Contains(p.Id)).OrderBy(p => p.Name))
                    _disponibles.Add(p);

                foreach (var p in todas.Where(p => idsAsignadas.Contains(p.Id)).OrderBy(p => p.Name))
                    _asignadas.Add(p);

                // Si es SOLO LECTURA, deshabilitar edición y ocultar Guardar
                if (_soloLectura)
                {
                    SetSoloLecturaUI(true);
                }
                else
                {
                    ActualizarHabilitadoComposicion();
                    chkActiva.CheckedChanged += (s, a) => ActualizarHabilitadoComposicion();
                }
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(this,
                    "Error cargando datos: " + ex.Message,
                    "Familias",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
                Close();
            }
        }

        private void SetSoloLecturaUI(bool ro)
        {
            // Campos de texto
            txtNombre.ReadOnly = true;
            txtDescripcion.ReadOnly = true;
            chkActiva.Enabled = false;

            // Listas y botones de composición
            lstDisponibles.Enabled = false;
            lstAsignadas.Enabled = false;
            btnAdd.Enabled = btnAddAll.Enabled = btnRemove.Enabled = btnRemoveAll.Enabled = false;

            // Guardar NO visible en lectura
            btnGuardar.Visible = false;

            // Cancelar / Volver visibles para salir
            btnCancelar.Visible = true;
            btnVolver.Visible = true;

            // Cambiamos el título para mayor claridad
            this.Text = "Familia (solo lectura)";
        }

        private void ActualizarHabilitadoComposicion()
        {
            bool habilitar = chkActiva.Checked;
            lstDisponibles.Enabled = lstAsignadas.Enabled = habilitar;
            btnAdd.Enabled = btnAddAll.Enabled = btnRemove.Enabled = btnRemoveAll.Enabled = habilitar;
        }

        private static void MoverSeleccion(KryptonListBox origenList, BindingList<Patente> origen, BindingList<Patente> destino)
        {
            var items = origenList.SelectedItems.Cast<Patente>().ToList();
            if (!items.Any()) return;

            foreach (var it in items)
            {
                if (!destino.Any(p => p.Id == it.Id)) destino.Add(it);
                var match = origen.FirstOrDefault(p => p.Id == it.Id);
                if (match != null) origen.Remove(match);
            }
        }

        private static void MoverTodos(BindingList<Patente> origen, BindingList<Patente> destino)
        {
            var items = origen.ToList();
            foreach (var it in items)
            {
                if (!destino.Any(p => p.Id == it.Id)) destino.Add(it);
                origen.Remove(it);
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            // Bloqueo definitivo por si alguien llama Guardar en modo Ver
            if (_soloLectura) return;

            if (!CheckAllowed(P_FAM_ABM)) return;

            try
            {
                var nombre = (txtNombre.Text ?? "").Trim();
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    KryptonMessageBox.Show(this, "El nombre es obligatorio.", "Familias",
                        KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                    txtNombre.Focus();
                    return;
                }

                var entidad = _familia ?? new Familia();

                SetStr(entidad, nombre, "Name", "Nombre");
                SetStr(entidad, (txtDescripcion.Text ?? "").Trim(), "Descripcion");

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

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (InvalidOperationException ex)
            {
                KryptonMessageBox.Show(this,
                    "Validación de patentes:\n" + ex.Message,
                    "Familias",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(this,
                    "No se pudo guardar la familia:\n" + ex.Message,
                    "Error",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }
    }
}
