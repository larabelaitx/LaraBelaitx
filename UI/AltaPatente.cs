using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Krypton.Toolkit;
using BE;
using BLL.Contracts;

namespace UI
{
    public partial class AltaPatente : KryptonForm
    {
        private readonly IUsuarioService _usuarios;
        private readonly IRolService _roles;

        private Usuario _usuario;

        private readonly BindingList<Permiso> _disponibles = new BindingList<Permiso>();
        private readonly BindingList<Permiso> _asignadas = new BindingList<Permiso>();
        private readonly BindingList<Permiso> _heredadas = new BindingList<Permiso>();

        public AltaPatente(IUsuarioService usuarios, IRolService roles)
        {
            InitializeComponent();
            _usuarios = usuarios ?? throw new ArgumentNullException(nameof(usuarios));
            _roles = roles ?? throw new ArgumentNullException(nameof(roles));

            Load += AltaPatente_Load;

            btnAdd.Click += (s, e) => MoverSeleccion(lstDisponibles, _disponibles, _asignadas);
            btnAddAll.Click += (s, e) => MoverTodos(_disponibles, _asignadas);
            btnRemove.Click += (s, e) => MoverSeleccion(lstAsignadas, _asignadas, _disponibles);
            btnRemoveAll.Click += (s, e) => MoverTodos(_asignadas, _disponibles);

            btnBuscar.Click += btnBuscar_Click;
            btnGuardar.Click += btnGuardar_Click;
            btnCancelar.Click += (s, e) => Close();
        }

        private void AltaPatente_Load(object sender, EventArgs e)
        {
            lstDisponibles.DataSource = _disponibles;
            lstDisponibles.DisplayMember = nameof(Permiso.Name);
            lstDisponibles.ValueMember = nameof(Permiso.Id);
            lstDisponibles.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;

            lstAsignadas.DataSource = _asignadas;
            lstAsignadas.DisplayMember = nameof(Permiso.Name);
            lstAsignadas.ValueMember = nameof(Permiso.Id);
            lstAsignadas.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;

            lstHeredadas.DataSource = _heredadas;
            lstHeredadas.DisplayMember = nameof(Permiso.Name);
            lstHeredadas.ValueMember = nameof(Permiso.Id);
            lstHeredadas.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;

            lstHeredadas.DoubleClick += (s, ev) => PromoverHeredadas();

            if (this.Controls.Find("btnPromover", true).FirstOrDefault() is KryptonButton btnPromover)
                btnPromover.Click += (s, ev) => PromoverHeredadas();

            AlternarEdicion(false);
        }

        private void AlternarEdicion(bool habilitar)
        {
            grpAsignacion.Enabled = habilitar;
            btnGuardar.Enabled = habilitar;
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            var ingreso = (txtUsuario.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(ingreso))
            {
                KryptonMessageBox.Show("Ingresá un usuario para buscar.", "Patentes",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                return;
            }

            try
            {
                _usuario = _usuarios.GetByUserName(ingreso);
                if (_usuario == null)
                {
                    KryptonMessageBox.Show("No se encontró el usuario indicado.", "Patentes",
                        KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                    AlternarEdicion(false);
                    return;
                }

                lblUsuarioSeleccionado.Values.Text = $"{_usuario.UserName} ({_usuario.LastName} {_usuario.Name})";
                CargarListas(_usuario.Id);
                AlternarEdicion(true);
            }
            catch (Exception ex)
            {
                BLL.Bitacora.Error(null, $"Error al buscar usuario en Patentes: {ex.Message}",
                    "UI", "Patentes_Buscar", host: Environment.MachineName);

                KryptonMessageBox.Show("No se pudo realizar la búsqueda.\n\n" + ex.Message,
                    "Error", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }

        private void CargarListas(int idUsuario)
        {
            _disponibles.Clear();
            _asignadas.Clear();
            _heredadas.Clear();

            var todas = (_roles.GetPatentes() ?? Enumerable.Empty<Patente>())
                        .OrderBy(p => p.Name)
                        .ToList();

            var directas = new HashSet<int>(
                (_roles.GetPatentesDeUsuario(idUsuario) ?? Enumerable.Empty<Permiso>())
                .Select(p => p.Id));

            var familias = _roles.GetFamiliasUsuario(idUsuario) ?? new List<Familia>();
            var heredadas = new HashSet<int>(
                familias.SelectMany(f => _roles.GetPatentesDeFamilia(f.Id) ?? new List<Patente>())
                        .Select(p => p.Id));

            foreach (var p in todas)
            {
                if (directas.Contains(p.Id))
                    _asignadas.Add(p);
                else if (heredadas.Contains(p.Id))
                    _heredadas.Add(p);
                else
                    _disponibles.Add(p);
            }
        }

        private void PromoverHeredadas()
        {
            var sel = lstHeredadas.SelectedItems.Cast<Permiso>().ToList();
            if (!sel.Any()) return;

            foreach (var p in sel)
            {
                if (!_asignadas.Any(x => x.Id == p.Id))
                    _asignadas.Add(p);

                var h = _heredadas.FirstOrDefault(x => x.Id == p.Id);
                if (h != null) _heredadas.Remove(h);

                var d = _disponibles.FirstOrDefault(x => x.Id == p.Id);
                if (d != null) _disponibles.Remove(d);
            }

            KryptonMessageBox.Show(
                "Se promovieron patentes heredadas a directas para este usuario.",
                "Patentes",
                KryptonMessageBoxButtons.OK,
                KryptonMessageBoxIcon.Information);
        }

        private static void MoverSeleccion(KryptonListBox origenList,
                                           BindingList<Permiso> origen,
                                           BindingList<Permiso> destino)
        {
            var items = origenList.SelectedItems.Cast<Permiso>().ToList();
            if (!items.Any()) return;

            foreach (var it in items)
            {
                if (!destino.Any(p => p.Id == it.Id))
                    destino.Add(it);

                var match = origen.FirstOrDefault(p => p.Id == it.Id);
                if (match != null) origen.Remove(match);
            }
        }

        private static void MoverTodos(BindingList<Permiso> origen, BindingList<Permiso> destino)
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
            if (_usuario == null)
            {
                KryptonMessageBox.Show("Primero seleccione un usuario.", "Patentes",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                return;
            }

            try
            {
                var idsDirectas = _asignadas.Select(p => p.Id).Distinct().ToList();
                _roles.SetPatentesDeUsuario(_usuario.Id, idsDirectas);

                KryptonMessageBox.Show("Asignación guardada correctamente.", "Patentes",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);

                DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                BLL.Bitacora.Error(_usuario?.Id, $"Error guardando patentes: {ex.Message}",
                    "UI", "Patentes_Guardar", host: Environment.MachineName);

                KryptonMessageBox.Show("No se pudo guardar la asignación.\n\n" + ex.Message, "Error",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }
    }
}
