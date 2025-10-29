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

            // Eventos
            this.Load += MainUsuarios_Load;
            btnBuscar.Click += (s, e) => Refrescar();
            btnLimpiar.Click += (s, e) => LimpiarFiltros();
            btnDescargar.Click += (s, e) => DescargarCsv();
            btnVolver.Click += btnVolver_Click;

            // ¡IMPORTANTE! Usamos tu botón “doble r”
            btnAgregarr.Click += btnAgregarr_Click;

            dgvUsuarios.CellContentClick += dgvUsuarios_CellContentClick;
            dgvUsuarios.CellFormatting += dgvUsuarios_CellFormatting;

            ConfigurarGrid();
        }

        private void MainUsuarios_Load(object sender, EventArgs e)
        {
            CargarCombos();
            MapearColumnas();   // <-- AGREGADO: mapea columnas del grid a UsuarioVM
            Refrescar();
        }

        // ---------- UI helpers ----------
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

        private void ConfigurarGrid()
        {
            dgvUsuarios.AutoGenerateColumns = false;
            dgvUsuarios.ReadOnly = true;
            dgvUsuarios.AllowUserToAddRows = false;
            dgvUsuarios.AllowUserToDeleteRows = false;
            dgvUsuarios.MultiSelect = false;
            dgvUsuarios.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvUsuarios.RowHeadersVisible = false;
            dgvUsuarios.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            // El diseñador ya define Nombre/Apellido/Usuario/Mail/Estado/Rol y los botones.
            // Sólo agregamos la columna "Roles" si no existe.
            if (!dgvUsuarios.Columns.Contains("colRoles"))
            {
                dgvUsuarios.Columns.Add(new DataGridViewButtonColumn
                {
                    Name = "colRoles",
                    HeaderText = "Roles",
                    Text = "Roles (0)",
                    UseColumnTextForButtonValue = false,
                    Width = 100
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
                    .FirstOrDefault(c =>
                        string.Equals((c.HeaderText ?? "").Trim(), headerText, StringComparison.OrdinalIgnoreCase));
                if (col != null) col.DataPropertyName = prop;
            }
        }

        private void Refrescar()
        {
            // Traigo TODOS los usuarios y excluyo únicamente las bajas (IdEstado = 3)
            var modelo = _svcUsuarios.GetAll()
                .Where(u => u.EstadoUsuarioId != 3) // <-- sólo oculto bajas
                .Select(u => new UsuarioVM
                {
                    Id = u.Id,
                    Usuario = u.UserName,
                    Apellido = u.LastName,
                    Nombre = u.Name,
                    Mail = u.Email,
                    Rol = ExtraerRolPrincipal(u),
                    Estado = u.EstadoDisplay   // debe devolver: "Habilitado" / "Bloqueado" / "Baja"
                });

            var filtrado = AplicarFiltros(modelo);
            _view = new BindingList<UsuarioVM>(filtrado.ToList());
            dgvUsuarios.DataSource = _view;

            if (dgvUsuarios.Columns.Contains("Mail"))
                dgvUsuarios.Columns["Mail"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private string RolPrincipal(int idUsuario)
        {
            var familias = _svcRoles.GetFamiliasUsuario(idUsuario) ?? new List<Familia>();
            return familias.Count > 0 ? familias[0].Name : "(sin rol)";
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

            // Combo: Todos / Activo / Inactivo
            bool? fActivo = (cboEstado.SelectedItem as ItemComboEstado)?.Valor;

            // Combo rol
            int? fRolId = (cboRol.SelectedItem as ItemComboRol)?.Id;

            var q = origen;

            if (!string.IsNullOrEmpty(fUsuario)) q = q.Where(x => Match(x.Usuario, fUsuario));
            if (!string.IsNullOrEmpty(fNombre)) q = q.Where(x => Match(x.Nombre, fNombre));
            if (!string.IsNullOrEmpty(fApellido)) q = q.Where(x => Match(x.Apellido, fApellido));
            if (!string.IsNullOrEmpty(fMail)) q = q.Where(x => Match(x.Mail, fMail));

            // ⬇️ Filtrado de estado SIN contemplar "Baja" (ya no viene en GetAllActive)
            if (fActivo.HasValue)
            {
                if (fActivo.Value)
                {
                    // Activo => Habilitado
                    q = q.Where(x => x.Estado.Equals("Habilitado", StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    // Inactivo => incluye Inactivo o Bloqueado
                    q = q.Where(x =>
                        x.Estado.Equals("Inactivo", StringComparison.OrdinalIgnoreCase) ||
                        x.Estado.Equals("Bloqueado", StringComparison.OrdinalIgnoreCase));
                }
            }

            // Filtro por rol (si elegiste uno en el combo)
            if (fRolId.HasValue)
            {
                q = q.Where(x =>
                {
                    var familias = _svcRoles.GetFamiliasUsuario(x.Id) ?? new List<Familia>();
                    return familias.Any(f => f.Id == fRolId.Value);
                });
            }

            return q;
        }

        private string ExtraerRolPrincipal(Usuario u)
        {
            if (u?.Permisos == null || u.Permisos.Count == 0) return "(sin rol)";
            var familia = u.Permisos.FirstOrDefault(p => p != null && p.GetType().Name == "Familia");
            return familia != null ? familia.Name : "(sin rol)";
        }

        private string RolPrincipalPorServicio(int idUsuario)
        {
            var familias = _svcRoles.GetFamiliasUsuario(idUsuario) ?? new List<Familia>();
            return familias.Count > 0 ? familias[0].Name : "(sin rol)";
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
                KryptonMessageBox.Show(
                    "No se pudo abrir AltaUsuario.\n\n" + ex.Message,
                    "Error",
                    KryptonMessageBoxButtons.OK,
                    KryptonMessageBoxIcon.Error
                );
            }
        }

        private void btnAgregarr_Click(object sender, EventArgs e)
        {
            AbrirAltaUsuario(ModoForm.Alta, null);
        }

        private void btnVolver_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;   // o DialogResult.OK si preferís
            this.Close();
        }

        private void dgvUsuarios_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var colName = dgvUsuarios.Columns[e.ColumnIndex].Name;
            var vm = dgvUsuarios.Rows[e.RowIndex].DataBoundItem as UsuarioVM;
            if (vm == null) return;

            switch (colName)
            {
                case "btnVer":
                    AbrirAltaUsuario(ModoForm.Ver, vm.Id);
                    break;

                case "btnEditar":
                    AbrirAltaUsuario(ModoForm.Editar, vm.Id);
                    break;

                case "btnBaja":
                    if (KryptonMessageBox.Show("¿Dar de baja al usuario?", "Confirmar",
                        KryptonMessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        var u = _svcUsuarios.GetById(vm.Id);
                        if (u != null)
                        {
                            _svcUsuarios.BajaLogica(u);
                            Refrescar();
                        }
                    }
                    break;

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
                        var idsActuales = (_svcRoles.GetFamiliasUsuario(vm.Id) ?? new List<Familia>())
                                          .Select(f => f.Id);
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

        private void dgvUsuarios_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var col = dgvUsuarios.Columns[e.ColumnIndex].Name;
            var vm = dgvUsuarios.Rows[e.RowIndex].DataBoundItem as UsuarioVM;
            if (vm == null) return;

            if (col == "colRoles")
            {
                var familias = _svcRoles.GetFamiliasUsuario(vm.Id) ?? new List<Familia>();
                dgvUsuarios.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = $"Roles ({familias.Count})";
                return;
            }

            if (col == "btnReactivar")
            {
                if (dgvUsuarios.Rows[e.RowIndex].Cells[e.ColumnIndex] is DataGridViewButtonCell cell)
                {
                    // ⬇️ Mostrar "Reactivar" tanto para Bloqueado como para Inactivo
                    bool mostrarReactivar =
                        vm.Estado.Equals("Bloqueado", StringComparison.OrdinalIgnoreCase) ||
                        vm.Estado.Equals("Inactivo", StringComparison.OrdinalIgnoreCase);

                    cell.Value = mostrarReactivar ? "Reactivar" : "—";
                    cell.FlatStyle = FlatStyle.Standard;
                    cell.ReadOnly = !mostrarReactivar;
                }
            }
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
                FileName = $"Usuarios_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;

                var sb = new StringBuilder();
                var sep = ",";
                sb.AppendLine(string.Join(sep, new[] { "Usuario", "Apellido", "Nombre", "Mail", "Rol", "Estado" }));
                foreach (var u in _view)
                {
                    string Esc(string s) => "\"" + (s ?? string.Empty).Replace("\"", "\"\"") + "\"";
                    sb.AppendLine(string.Join(sep, new[] { Esc(u.Usuario), Esc(u.Apellido), Esc(u.Nombre), Esc(u.Mail), Esc(u.Rol), Esc(u.Estado) }));
                }
                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
            }

            KryptonMessageBox.Show("Archivo exportado correctamente.", "Éxito");
        }

        private void AplicarModo(ModoForm modo)
        {
            bool soloLectura = (modo == ModoForm.Ver);

            txtNombre.ReadOnly = soloLectura;
            txtApellido.ReadOnly = soloLectura;
            txtUsuario.ReadOnly = soloLectura;
            txtMail.ReadOnly = soloLectura;
            cboEstado.Enabled = !soloLectura;
            cboRol.Enabled = !soloLectura;

            btnVolver.Enabled = true;    // <- SIEMPRE habilitado
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

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
}
