using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BE;
using BLL.Contracts;
using Krypton.Toolkit;

namespace UI
{
    public partial class MainUsuarios : KryptonForm
    {
        private readonly IUsuarioService _svcUsuarios;
        private readonly IRolService _svcRoles;
        private BindingList<UsuarioVM> _view = new BindingList<UsuarioVM>();

        public MainUsuarios(IUsuarioService usuarios, IRolService roles)
        {
            InitializeComponent();

            _svcUsuarios = usuarios ?? throw new ArgumentNullException(nameof(usuarios));
            _svcRoles = roles ?? throw new ArgumentNullException(nameof(roles));

            Load += MainUsuarios_Load;
            btnBuscar.Click += (s, e) => Refrescar();
            btnLimpiar.Click += (s, e) => LimpiarFiltros();
            btnDescargar.Click += (s, e) => DescargarCsv();
            btnVolver.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            btnAgregarr.Click += (s, e) => AbrirAltaUsuario(ModoForm.Alta, null);

            dgvUsuarios.CellFormatting += dgvUsuarios_CellFormatting;
            dgvUsuarios.CellContentClick += dgvUsuarios_CellContentClick;

            FixScrollListado();
            ConfigurarGrid();
        }

        private void MainUsuarios_Load(object sender, EventArgs e)
        {
            try
            {
                CargarCombos();
                MapearColumnas();
                Refrescar();
            }
            catch (Exception ex)
            {
                BLL.Bitacora.Error(null, $"Error cargando MainUsuarios: {ex.Message}",
                    "UI", "MainUsuarios_Load", host: Environment.MachineName);

                KryptonMessageBox.Show(this,
                    "Error cargando pantalla Usuarios:\n" + ex.Message,
                    "Error", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }

        private void FixScrollListado()
        {
            (dgvUsuarios?.Parent as Panel)?.Let(p => p.AutoScroll = true);
            (dgvUsuarios?.Parent as KryptonPanel)?.Let(p => p.AutoScroll = true);

            dgvUsuarios.Dock = DockStyle.Fill;
            dgvUsuarios.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvUsuarios.ScrollBars = ScrollBars.Both;
            dgvUsuarios.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            dgvUsuarios.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
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

            AddBtn("btnVer", "Ver", 60);
            AddBtn("btnEditar", "Editar", 70);
            AddBtn("btnBaja", "Baja", 60);
            AddBtn("btnReactivar", "Reactivar", 90);

            if (!g.Columns.Contains("colRoles"))
            {
                var col = new DataGridViewButtonColumn
                {
                    Name = "colRoles",
                    HeaderText = "Roles",
                    UseColumnTextForButtonValue = false,
                    Width = 110,
                    MinimumWidth = 110
                };
                g.Columns.Add(col);
            }

            void AddBtn(string name, string text, int w)
            {
                if (g.Columns.Contains(name)) return;
                g.Columns.Add(new DataGridViewButtonColumn
                {
                    Name = name,
                    HeaderText = text,
                    Text = text,
                    UseColumnTextForButtonValue = true,
                    Width = w,
                    MinimumWidth = w
                });
            }
        }

        private void MapearColumnas()
        {
            SetDP("Usuario", nameof(UsuarioVM.Usuario));
            SetDP("Apellido", nameof(UsuarioVM.Apellido));
            SetDP("Nombre", nameof(UsuarioVM.Nombre));
            SetDP("Mail", nameof(UsuarioVM.Mail));
            SetDP("Estado", nameof(UsuarioVM.Estado));
            SetDP("Rol", nameof(UsuarioVM.Rol));

            void SetDP(string headerText, string prop)
            {
                var col = dgvUsuarios.Columns
                    .Cast<DataGridViewColumn>()
                    .FirstOrDefault(c => string.Equals((c.HeaderText ?? "").Trim(), headerText, StringComparison.OrdinalIgnoreCase));
                if (col != null) col.DataPropertyName = prop;
            }
        }

        private void CargarCombos()
        {
            var estados = new List<ItemComboEstado>
            {
                new ItemComboEstado { Texto = "Todos",    Valor = null },
                new ItemComboEstado { Texto = "Activo",   Valor = true },
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
                    Rol = ExtraerRolPrincipal(u),
                    Estado = u.EstadoDisplay
                });

            var filtrado = AplicarFiltros(modelo).ToList();
            _view = new BindingList<UsuarioVM>(filtrado);
            dgvUsuarios.DataSource = _view;

            if (dgvUsuarios.Columns.Contains("Mail"))
                dgvUsuarios.Columns["Mail"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private IEnumerable<UsuarioVM> AplicarFiltros(IEnumerable<UsuarioVM> origen)
        {
            string norm(string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return string.Empty;
                var n = s.Normalize(NormalizationForm.FormD);
                var sb = new StringBuilder(s.Length);
                foreach (var ch in n)
                    if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark) sb.Append(ch);
                return sb.ToString().ToLowerInvariant().Trim();
            }

            bool Match(string fuente, string filtro) =>
                string.IsNullOrEmpty(filtro) || norm(fuente).Contains(filtro);

            var fUsuario = norm(txtUsuario.Text);
            var fNombre = norm(txtNombre.Text);
            var fApellido = norm(txtApellido.Text);
            var fMail = norm(txtMail.Text);

            bool? fActivo = (cboEstado.SelectedItem as ItemComboEstado)?.Valor;
            int? fRolId = (cboRol.SelectedItem as ItemComboRol)?.Id;

            var q = origen;

            if (!string.IsNullOrEmpty(fUsuario)) q = q.Where(x => Match(x.Usuario, fUsuario));
            if (!string.IsNullOrEmpty(fNombre)) q = q.Where(x => Match(x.Nombre, fNombre));
            if (!string.IsNullOrEmpty(fApellido)) q = q.Where(x => Match(x.Apellido, fApellido));
            if (!string.IsNullOrEmpty(fMail)) q = q.Where(x => Match(x.Mail, fMail));

            if (fActivo.HasValue)
            {
                if (fActivo.Value)
                    q = q.Where(x => x.Estado.Equals("Habilitado", StringComparison.OrdinalIgnoreCase));
                else
                    q = q.Where(x => x.Estado.Equals("Inactivo", StringComparison.OrdinalIgnoreCase) ||
                                     x.Estado.Equals("Bloqueado", StringComparison.OrdinalIgnoreCase));
            }

            if (fRolId.HasValue)
                q = q.Where(x =>
                {
                    var familias = _svcRoles.GetFamiliasUsuario(x.Id) ?? new List<Familia>();
                    return familias.Any(f => f.Id == fRolId.Value);
                });

            return q;
        }

        private static string ExtraerRolPrincipal(Usuario u)
        {
            if (u?.Permisos == null || u.Permisos.Count == 0) return "(sin rol)";
            var familia = u.Permisos.FirstOrDefault(p => p != null && p.GetType().Name == "Familia");
            return familia != null ? familia.Name : "(sin rol)";
        }

        private void AbrirAltaUsuario(ModoForm modo, int? idUsuario)
        {
            try
            {
                using (var frm = new AltaUsuario(modo, idUsuario, _svcUsuarios, _svcRoles))
                {
                    frm.StartPosition = FormStartPosition.CenterParent;
                    if (frm.ShowDialog(this) == DialogResult.OK)
                        Refrescar();
                }
            }
            catch (Exception ex)
            {
                BLL.Bitacora.Error(null, $"No se pudo abrir AltaUsuario: {ex.Message}",
                    "UI", "MainUsuarios_AbrirAlta", host: Environment.MachineName);

                KryptonMessageBox.Show("No se pudo abrir AltaUsuario.\n\n" + ex.Message,
                    "Error", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
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

                case "btnBaja":
                    {
                        if (KryptonMessageBox.Show(this,
                                "¿Dar de baja al usuario seleccionado?",
                                "Confirmar", KryptonMessageBoxButtons.YesNo,
                                KryptonMessageBoxIcon.Question) != DialogResult.Yes)
                            return;

                        try
                        {
                            // ✔ ahora usamos la baja transaccional del servicio
                            if (!_svcUsuarios.TryBajaUsuarios(new[] { vm.Id }, out var motivo))
                            {
                                ShowWarn("No se pudo dar de baja el usuario.\n\n" + motivo);
                                return;
                            }

                            ShowInfo("Usuario dado de baja correctamente.");
                            Refrescar();
                        }
                        catch (Exception ex)
                        {
                            BLL.Bitacora.Error(null, $"Baja de usuario {vm.Usuario} falló: {ex.Message}",
                                "UI", "MainUsuarios_Baja", host: Environment.MachineName);

                            KryptonMessageBox.Show(this,
                                "Ocurrió un error realizando la baja.\n\n" + ex.Message,
                                "Error", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
                        }
                        break;
                    }

                case "btnReactivar":
                    if (KryptonMessageBox.Show("¿Desbloquear / reactivar al usuario?", "Confirmar",
                        KryptonMessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        _svcUsuarios.DesbloquearUsuario(vm.Id);
                        Refrescar();
                    }
                    break;

                case "colRoles":
                    {
                        var usuario = _svcUsuarios.GetById(vm.Id);
                        var idsActuales = (_svcRoles.GetFamiliasUsuario(vm.Id) ?? new List<Familia>()).Select(f => f.Id);
                        using (var dlg = new DialogRolesUsuario(usuario, idsActuales))
                        {
                            if (dlg.ShowDialog(this) == DialogResult.OK)
                            {
                                _svcRoles.SetFamiliasDeUsuario(vm.Id, dlg.FamiliasSeleccionadasIds);
                                Refrescar();
                            }
                        }
                        break;
                    }
            }
        }

        // ---- Helpers UI ----
        private void ShowInfo(string msg) =>
            KryptonMessageBox.Show(this, msg, "Información",
                KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);

        private void ShowWarn(string msg) =>
            KryptonMessageBox.Show(this, msg, "Aviso",
                KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);

        private void dgvUsuarios_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var name = dgvUsuarios.Columns[e.ColumnIndex].Name;
            var vm = dgvUsuarios.Rows[e.RowIndex].DataBoundItem as UsuarioVM;
            if (vm == null) return;

            if (name == "colRoles")
            {
                var familias = _svcRoles.GetFamiliasUsuario(vm.Id) ?? new List<Familia>();
                dgvUsuarios.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = $"Roles ({familias.Count})";
                return;
            }

            if (name == "btnReactivar")
            {
                if (dgvUsuarios.Rows[e.RowIndex].Cells[e.ColumnIndex] is DataGridViewButtonCell cell)
                {
                    bool mostrar =
                        vm.Estado.Equals("Bloqueado", StringComparison.OrdinalIgnoreCase) ||
                        vm.Estado.Equals("Inactivo", StringComparison.OrdinalIgnoreCase);

                    cell.Value = mostrar ? "Reactivar" : "—";
                    cell.ReadOnly = !mostrar;
                }
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
                KryptonMessageBox.Show("No hay datos para exportar.", "Información");
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
                    string Esc(string s) => "\"" + (s ?? string.Empty).Replace("\"", "\"\"") + "\"";
                    sb.AppendLine(string.Join(sep, new[]
                    {
                        Esc(u.Usuario), Esc(u.Apellido), Esc(u.Nombre), Esc(u.Mail), Esc(u.Rol), Esc(u.Estado)
                    }));
                }
                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
            }

            KryptonMessageBox.Show("Archivo exportado correctamente.", "Éxito");
        }
    }

    // ===== VMs / combos =====
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

    public class ItemComboEstado
    {
        public string Texto { get; set; }
        public bool? Valor { get; set; }
    }

    public class ItemComboRol
    {
        public int? Id { get; set; }
        public string Nombre { get; set; }
    }

    // helper de extensión para sintaxis más limpia
    internal static class ObjExt
    {
        public static void Let<T>(this T obj, Action<T> act) where T : class
        {
            if (obj != null) act(obj);
        }
    }
}
