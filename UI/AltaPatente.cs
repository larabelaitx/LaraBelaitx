using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Krypton.Toolkit;
using BE;
using BLL.Contracts;
using UI.Seguridad; // <- Perms

namespace UI
{
    public partial class AltaPatente : KryptonForm
    {
        private readonly IUsuarioService _usuarios;
        private readonly IRolService _roles;
        private readonly int? _currentUserId;

        private Usuario _usuario;

        private readonly BindingList<Permiso> _disponibles = new BindingList<Permiso>();
        private readonly BindingList<Permiso> _asignadas = new BindingList<Permiso>(); // directas
        private readonly BindingList<Permiso> _heredadas = new BindingList<Permiso>(); // por familias

        // Constante alineada con tu tabla Patente
        private const string P_USU_PAT_EDITAR = Perms.Patente_AsignarAUsuario;

        public AltaPatente(IUsuarioService usuarios, IRolService roles, int? currentUserId = null)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterParent;

            _usuarios = usuarios ?? throw new ArgumentNullException(nameof(usuarios));
            _roles = roles ?? throw new ArgumentNullException(nameof(roles));
            _currentUserId = currentUserId;

            Load += AltaPatente_Load;

            btnAdd.Click += (s, e) => MoverSeleccion(lstDisponibles, _disponibles, _asignadas);
            btnAddAll.Click += (s, e) => MoverTodos(_disponibles, _asignadas);
            btnRemove.Click += (s, e) => MoverSeleccion(lstAsignadas, _asignadas, _disponibles);
            btnRemoveAll.Click += (s, e) => MoverTodos(_asignadas, _disponibles);

            btnBuscar.Click += btnBuscar_Click;
            btnGuardar.Click += btnGuardar_Click;
            btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
        }

        private void AltaPatente_Load(object sender, EventArgs e)
        {
            // Guard de seguridad (sin depender de ThrowIfNotAllowed)
            if (_currentUserId.HasValue && !_roles.TienePatente(_currentUserId.Value, P_USU_PAT_EDITAR))
            {
                KryptonMessageBox.Show(this,
                    $"No tenés permisos para administrar patentes.\n({P_USU_PAT_EDITAR})",
                    "Acceso denegado", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                Close();
                return;
            }

            lstDisponibles.DataSource = _disponibles;
            lstDisponibles.DisplayMember = nameof(Permiso.Name);
            lstDisponibles.ValueMember = nameof(Permiso.Id);
            lstDisponibles.SelectionMode = SelectionMode.MultiExtended;

            lstAsignadas.DataSource = _asignadas;
            lstAsignadas.DisplayMember = nameof(Permiso.Name);
            lstAsignadas.ValueMember = nameof(Permiso.Id);
            lstAsignadas.SelectionMode = SelectionMode.MultiExtended;

            lstHeredadas.DataSource = _heredadas;
            lstHeredadas.DisplayMember = nameof(Permiso.Name);
            lstHeredadas.ValueMember = nameof(Permiso.Id);
            lstHeredadas.SelectionMode = SelectionMode.MultiExtended;

            lstHeredadas.DoubleClick += (s, ev) => PromoverHeredadas();
            if (Controls.Find("btnPromover", true).FirstOrDefault() is KryptonButton btnPromover)
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

            var todas = (_roles.GetPatentes() ?? Enumerable.Empty<Permiso>())
                        .OrderBy(p => p.Name)
                        .ToList();

            var directas = new HashSet<int>(
                (_roles.GetPatentesDirectasDeUsuario(idUsuario) ?? Enumerable.Empty<Permiso>())
                .Select(p => p.Id));

            var familias = _roles.GetFamiliasUsuario(idUsuario) ?? new List<Familia>();
            var heredadasIds = new HashSet<int>(
                familias.SelectMany(f => _roles.GetPatentesDeFamilia(f.Id) ?? Enumerable.Empty<Permiso>())
                        .Select(p => p.Id));

            foreach (var p in todas)
            {
                if (directas.Contains(p.Id)) _asignadas.Add(p);
                else if (heredadasIds.Contains(p.Id)) _heredadas.Add(p);
                else _disponibles.Add(p);
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
                "Patentes", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
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

            // Revalidación antes de guardar
            if (_currentUserId.HasValue && !_roles.TienePatente(_currentUserId.Value, P_USU_PAT_EDITAR))
            {
                KryptonMessageBox.Show(this,
                    $"No tenés permisos para guardar cambios de patentes.\n({P_USU_PAT_EDITAR})",
                    "Acceso denegado", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                return;
            }

            try
            {
                var idsDirectas = _asignadas.Select(p => p.Id).Distinct().ToList();
                _roles.SetPatentesDeUsuario(_usuario.Id, idsDirectas);

                // Bitácora
                BLL.Bitacora.Info(_currentUserId,
                    $"Patentes directas seteadas para UsuarioId={_usuario.Id}: [{string.Join(",", idsDirectas)}]",
                    "Usuarios", "SetPatentesUsuario", host: Environment.MachineName);

                KryptonMessageBox.Show("Asignación guardada correctamente.", "Patentes",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (InvalidOperationException ex)
            {
                KryptonMessageBox.Show(ex.Message, "Validación",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                CargarListas(_usuario.Id);
            }
            catch (Exception ex)
            {
                BLL.Bitacora.Error(_usuario?.Id, $"Error guardando patentes: {ex.Message}",
                    "UI", "Patentes_Guardar", host: Environment.MachineName);

                KryptonMessageBox.Show("No se pudo guardar la asignación.\n\n" + ex.Message,
                    "Error", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
                CargarListas(_usuario.Id);
            }
        }
    }
}
