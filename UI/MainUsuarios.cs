using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Krypton.Toolkit;
using BE;
using BLL.Contracts;

namespace UI
{
    public partial class MainUsuarios : KryptonForm
    {
        private readonly IUsuarioService _svcUsuarios;
        private readonly IRolService _svcRoles;
        private readonly int? _currentUserId;

        private BindingList<UsuarioVM> _view = new BindingList<UsuarioVM>();

        public MainUsuarios(IUsuarioService usuarios, IRolService roles, int? currentUserId = null)
        {
            InitializeComponent();
            _svcUsuarios = usuarios ?? throw new ArgumentNullException(nameof(usuarios));
            _svcRoles = roles ?? throw new ArgumentNullException(nameof(roles));
            _currentUserId = currentUserId;

            Load += MainUsuarios_Load;

            btnBuscar.Click += (s, e) => Refrescar();
            btnLimpiar.Click += (s, e) => LimpiarFiltros();
            btnDescargar.Click += (s, e) => DescargarCsv();
            btnVolver.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            btnAgregarr.Click += (s, e) => AbrirAltaUsuario(ModoForm.Alta, null);

            dgvUsuarios.CellFormatting += dgvUsuarios_CellFormatting;
            dgvUsuarios.CellContentClick += dgvUsuarios_CellContentClick;

            ConfigurarGrid();
        }

        private void MainUsuarios_Load(object sender, EventArgs e)
        {
            CargarCombos();
            MapearColumnas();
            Refrescar();
        }

        private void AbrirAltaUsuario(ModoForm modo, int? idUsuario)
        {
            try
            {
                using (var frm = new AltaUsuario(modo, idUsuario, _svcUsuarios, _svcRoles, _currentUserId))
                {
                    frm.StartPosition = FormStartPosition.CenterParent;
                    if (frm.ShowDialog(this) == DialogResult.OK)
                        Refrescar();
                }
            }
            catch (Exception ex)
            {
                BLL.Bitacora.Error(_currentUserId,
                    "No se pudo abrir AltaUsuario: " + ex.Message,
                    "UI", "MainUsuarios_AbrirAlta", host: Environment.MachineName);

                KryptonMessageBox.Show(this,
                    "No se pudo abrir AltaUsuario.\n\n" + ex.Message,
                    "Error", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }

        private void ConfigurarGrid()
        {
            var g = dgvUsuarios;
            g.AutoGenerateColumns = false;
            g.ReadOnly = true;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.MultiSelect = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.RowHeadersVisible = false;

            if (!g.Columns.Contains("btnVer"))
                g.Columns.Add(new DataGridViewButtonColumn { Name = "btnVer", HeaderText = "Ver", Text = "Ver", UseColumnTextForButtonValue = true, Width = 60 });
            if (!g.Columns.Contains("btnEditar"))
                g.Columns.Add(new DataGridViewButtonColumn { Name = "btnEditar", HeaderText = "Editar", Text = "Editar", UseColumnTextForButtonValue = true, Width = 70 });
            if (!g.Columns.Contains("btnBloquear"))
                g.Columns.Add(new DataGridViewButtonColumn { Name = "btnBloquear", HeaderText = "Bloquear", Text = "Bloquear", UseColumnTextForButtonValue = true, Width = 90 });
            if (!g.Columns.Contains("btnBaja"))
                g.Columns.Add(new DataGridViewButtonColumn { Name = "btnBaja", HeaderText = "Baja", Text = "Baja", UseColumnTextForButtonValue = true, Width = 60 });
            if (!g.Columns.Contains("btnReactivar"))
                g.Columns.Add(new DataGridViewButtonColumn { Name = "btnReactivar", HeaderText = "Reactivar", UseColumnTextForButtonValue = false, Width = 90 });
            if (!g.Columns.Contains("colRoles"))
                g.Columns.Add(new DataGridViewButtonColumn { Name = "colRoles", HeaderText = "Roles", UseColumnTextForButtonValue = false, Width = 110 });
        }

        private void MapearColumnas()
        {
            SetDP("Usuario", nameof(UsuarioVM.Usuario));
            SetDP("Apellido", nameof(UsuarioVM.Apellido));
            SetDP("Nombre", nameof(UsuarioVM.Nombre));
            SetDP("Mail", nameof(UsuarioVM.Mail));
            SetDP("Rol", nameof(UsuarioVM.Rol));
            SetDP("Estado", nameof(UsuarioVM.Estado));
        }

        private void SetDP(string headerText, string prop)
        {
            var col = dgvUsuarios.Columns.Cast<DataGridViewColumn>()
                       .FirstOrDefault(c => (c.HeaderText ?? "").Trim().Equals(headerText, StringComparison.OrdinalIgnoreCase));
            if (col != null) col.DataPropertyName = prop;
        }

        private void CargarCombos()
        {
            var estados = new List<ItemComboEstado>
            {
                new ItemComboEstado { Texto = "Todos", Valor = null },
                new ItemComboEstado { Texto = "Activo", Valor = true },
                new ItemComboEstado { Texto = "Inactivo", Valor = false }
            };
            cboEstado.DisplayMember = nameof(ItemComboEstado.Texto);
            cboEstado.ValueMember = nameof(ItemComboEstado.Valor);
            cboEstado.DataSource = estados;

            var roles = _svcRoles.ListarRoles()
                                 .OrderBy(r => r.Name)
                                 .Select(r => new ItemComboRol { Id = r.Id, Nombre = r.Name })
                                 .ToList();
            roles.Insert(0, new ItemComboRol { Id = null, Nombre = "Todos" });
            cboRol.DisplayMember = nameof(ItemComboRol.Nombre);
            cboRol.ValueMember = nameof(ItemComboRol.Id);
            cboRol.DataSource = roles;
        }

        private void Refrescar()
        {
            var modelo = _svcUsuarios.GetAll()
                .Where(u => u.EstadoUsuarioId != 3)
                .Select(u => new UsuarioVM
                {
                    Id = u.Id,
                    Usuario = u.UserName,
                    Apellido = u.LastName,
                    Nombre = u.Name,
                    Mail = u.Email,
                    Rol = ExtraerRolPrincipal(u, _svcRoles),
                    Estado = u.EstadoDisplay
                });

            _view = new BindingList<UsuarioVM>(AplicarFiltros(modelo).ToList());
            dgvUsuarios.DataSource = _view;

            if (dgvUsuarios.Columns.Contains("Mail"))
                dgvUsuarios.Columns["Mail"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private IEnumerable<UsuarioVM> AplicarFiltros(IEnumerable<UsuarioVM> origen)
        {
            string N(string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return string.Empty;
                var n = s.Normalize(NormalizationForm.FormD);
                var sb = new StringBuilder(s.Length);
                foreach (var ch in n)
                    if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark) sb.Append(ch);
                return sb.ToString().ToLowerInvariant().Trim();
            }

            bool Match(string fuente, string filtro) => string.IsNullOrEmpty(filtro) || N(fuente).Contains(filtro);

            var fUsuario = N(txtUsuario.Text);
            var fNombre = N(txtNombre.Text);
            var fApellido = N(txtApellido.Text);
            var fMail = N(txtMail.Text);

            bool? fActivo = (cboEstado.SelectedItem as ItemComboEstado)?.Valor;
            int? fRolId = (cboRol.SelectedItem as ItemComboRol)?.Id;

            var q = origen;
            if (!string.IsNullOrEmpty(fUsuario)) q = q.Where(x => Match(x.Usuario, fUsuario));
            if (!string.IsNullOrEmpty(fNombre)) q = q.Where(x => Match(x.Nombre, fNombre));
            if (!string.IsNullOrEmpty(fApellido)) q = q.Where(x => Match(x.Apellido, fApellido));
            if (!string.IsNullOrEmpty(fMail)) q = q.Where(x => Match(x.Mail, fMail));

            if (fActivo.HasValue)
                q = fActivo.Value
                    ? q.Where(x => x.Estado.Equals("Habilitado", StringComparison.OrdinalIgnoreCase))
                    : q.Where(x => x.Estado.Equals("Inactivo", StringComparison.OrdinalIgnoreCase) ||
                                   x.Estado.Equals("Bloqueado", StringComparison.OrdinalIgnoreCase) ||
                                   x.Estado.Equals("Baja", StringComparison.OrdinalIgnoreCase));

            if (fRolId.HasValue)
                q = q.Where(x =>
                {
                    var familias = _svcRoles.GetFamiliasUsuario(x.Id) ?? new List<Familia>();
                    return familias.Any(f => f.Id == fRolId.Value);
                });

            return q;
        }

        private static string ExtraerRolPrincipal(Usuario u, IRolService rolSvc)
        {
            if (u == null) return "(sin rol)";
            var familias = rolSvc.GetFamiliasUsuario(u.Id) ?? new List<Familia>();
            var f = familias.FirstOrDefault();
            return f != null ? f.Name : "(sin rol)";
        }

        private void dgvUsuarios_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var col = dgvUsuarios.Columns[e.ColumnIndex].Name;
            var vm = dgvUsuarios.Rows[e.RowIndex].DataBoundItem as UsuarioVM;
            if (vm == null) return;

            switch (col)
            {
                case "btnVer":
                    AbrirAltaUsuario(ModoForm.Ver, vm.Id);
                    break;

                case "btnEditar":
                    AbrirAltaUsuario(ModoForm.Editar, vm.Id);
                    break;

                case "btnBloquear":
                    {
                        var dr = KryptonMessageBox.Show(this,
                            $"¿Bloquear al usuario '{vm.Usuario}'?",
                            "Confirmar", KryptonMessageBoxButtons.YesNo, KryptonMessageBoxIcon.Question);
                        if (dr != DialogResult.Yes) return;

                        try
                        {
                            bool ok = _svcUsuarios.Bloquear(vm.Id);
                            KryptonMessageBox.Show(this,
                                ok ? "Usuario bloqueado correctamente." : "No se pudo bloquear el usuario.",
                                "Usuarios",
                                KryptonMessageBoxButtons.OK,
                                ok ? KryptonMessageBoxIcon.Information : KryptonMessageBoxIcon.Warning);
                            Refrescar();
                        }
                        catch (Exception ex)
                        {
                            KryptonMessageBox.Show(this,
                                "Error al bloquear usuario:\n" + ex.Message,
                                "Usuarios", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
                        }
                        break;
                    }

                case "btnBaja":
                    {
                        var dr = KryptonMessageBox.Show(this,
                            $"¿Dar de baja al usuario '{vm.Usuario}'?",
                            "Confirmar", KryptonMessageBoxButtons.YesNo, KryptonMessageBoxIcon.Question);
                        if (dr != DialogResult.Yes) return;

                        try
                        {
                            string msg;
                            bool ok = _svcUsuarios.BajaLogicaSegura(vm.Id, out msg);
                            KryptonMessageBox.Show(this, msg, "Usuarios",
                                KryptonMessageBoxButtons.OK,
                                ok ? KryptonMessageBoxIcon.Information : KryptonMessageBoxIcon.Warning);
                            Refrescar();
                        }
                        catch (Exception ex)
                        {
                            KryptonMessageBox.Show(this,
                                "Error inesperado al dar de baja:\n" + ex.Message,
                                "Usuarios", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
                        }
                        break;
                    }

                case "btnReactivar":
                    CambiarEstadoUsuario(vm.Id, destinoHabilitado: true);
                    break;
            }
        }

        private void CambiarEstadoUsuario(int idUsuario, bool destinoHabilitado)
        {
            try
            {
                var u = _svcUsuarios.GetById(idUsuario);
                if (u == null)
                {
                    KryptonMessageBox.Show(this, "No se encontró el usuario.", "Usuarios",
                        KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                    return;
                }

                if (destinoHabilitado)
                {
                    _svcUsuarios.Desbloquear(idUsuario);
                    KryptonMessageBox.Show(this, "Usuario reactivado correctamente.", "Usuarios",
                        KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                }

                Refrescar();
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(this,
                    "Error al cambiar estado del usuario:\n" + ex.Message,
                    "Usuarios", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }

        private void dgvUsuarios_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var name = dgvUsuarios.Columns[e.ColumnIndex].Name;
            var vm = dgvUsuarios.Rows[e.RowIndex].DataBoundItem as UsuarioVM;
            if (vm == null) return;

            if (name == "colRoles")
            {
                var familias = _svcRoles.GetFamiliasUsuario(vm.Id) ?? new List<Familia>();
                e.Value = $"Roles ({familias.Count})";
            }
        }

        private void LimpiarFiltros()
        {
            txtUsuario.Clear();
            txtNombre.Clear();
            txtApellido.Clear();
            txtMail.Clear();
            cboEstado.SelectedIndex = 0;
            cboRol.SelectedIndex = 0;
            Refrescar();
        }

        private void DescargarCsv()
        {
            if (_view == null || _view.Count == 0)
            {
                KryptonMessageBox.Show(this, "No hay datos para exportar.", "Información");
                return;
            }

            using (var sfd = new SaveFileDialog
            {
                Filter = "CSV (separado por comas)|*.csv",
                FileName = "Usuarios_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv"
            })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;

                var sb = new StringBuilder();
                var sep = ",";
                sb.AppendLine(string.Join(sep, new[] { "Usuario", "Apellido", "Nombre", "Mail", "Rol", "Estado" }));
                foreach (var u in _view)
                {
                    sb.AppendLine(string.Join(sep, new[]
                    {
                        Esc(u.Usuario), Esc(u.Apellido), Esc(u.Nombre), Esc(u.Mail), Esc(u.Rol), Esc(u.Estado)
                    }));
                }
                System.IO.File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
            }

            KryptonMessageBox.Show(this, "Archivo exportado correctamente.", "Éxito");
        }

        private string Esc(string s)
        {
            if (s == null) s = string.Empty;
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }
    }

    public class UsuarioVM
    {
        public int Id { get; set; }
        public string Usuario { get; set; }
        public string Apellido { get; set; }
        public string Nombre { get; set; }
        public string Mail { get; set; }
        public string Rol { get; set; }
        public string Estado { get; set; }
    }

    public class ItemComboEstado { public string Texto { get; set; } public bool? Valor { get; set; } }
    public class ItemComboRol { public int? Id { get; set; } public string Nombre { get; set; } }
}
