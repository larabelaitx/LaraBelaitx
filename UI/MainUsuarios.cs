using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BLL;
using BLL.Contracts;
using Krypton.Toolkit;
using BE;



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
            _svcUsuarios = usuarios;
            _svcRoles = roles;

            this.Load += MainUsuarios_Load;
            btnBuscar.Click += (s, e) => Refrescar();
            btnLimpiar.Click += (s, e) => LimpiarFiltros();
            btnAgregar.Click += (s, e) => AbrirAltaUsuario(ModoForm.Alta, null);
            btnDescargar.Click += (s, e) => DescargarCsv();
            btnVolver.Click += (s, e) => this.Close();

            dgvUsuarios.CellContentClick += dgvUsuarios_CellContentClick;

            ConfigurarGrid();
        }
        private void AbrirAltaUsuario(ModoForm modo, int? idUsuario)
        {
            using (var frm = new AltaUsuario(modo, idUsuario, _svcUsuarios, _svcRoles))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    Refrescar();
            }
        }
        private void MainUsuarios_Load(object sender, EventArgs e)
        {
            CargarCombos();
            Refrescar();
            dgvUsuarios.CellFormatting += dgvUsuarios_CellFormatting;

        }

        private void CargarCombos()
        {
            var estados = new List<ItemComboEstado>();
            estados.Add(new ItemComboEstado { Texto = "Todos", Valor = null });
            estados.Add(new ItemComboEstado { Texto = "Activo", Valor = true });
            estados.Add(new ItemComboEstado { Texto = "Inactivo", Valor = false });

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

            if (!dgvUsuarios.Columns.Contains("btnVer"))
                dgvUsuarios.Columns.Add(new DataGridViewButtonColumn { Name = "btnVer", HeaderText = "Ver", Text = "Ver", UseColumnTextForButtonValue = true });
            if (!dgvUsuarios.Columns.Contains("btnEditar"))
                dgvUsuarios.Columns.Add(new DataGridViewButtonColumn { Name = "btnEditar", HeaderText = "Editar", Text = "Editar", UseColumnTextForButtonValue = true });
            if (!dgvUsuarios.Columns.Contains("btnBaja"))
                dgvUsuarios.Columns.Add(new DataGridViewButtonColumn { Name = "btnBaja", HeaderText = "Baja", Text = "Baja", UseColumnTextForButtonValue = true });
            if (!dgvUsuarios.Columns.Contains("btnReactivar"))
                dgvUsuarios.Columns.Add(new DataGridViewButtonColumn { Name = "btnReactivar", HeaderText = "Reactivar", Text = "Reactivar", UseColumnTextForButtonValue = true });
           
            if (!dgvUsuarios.Columns.Contains("colRoles"))
                dgvUsuarios.Columns.Add(new DataGridViewButtonColumn
                {
                    Name = "colRoles",
                    HeaderText = "Roles",
                    Text = "Roles (0)",     
                    UseColumnTextForButtonValue = false, 
                    Width = 100
                });
        }

        private void Refrescar()
        {
            var modelo = _svcUsuarios.GetAll().Select(u => new UsuarioVM
            {
                Id = u.Id,
                Usuario = u.UserName,
                Apellido = u.LastName,
                Nombre = u.Name,
                Mail = u.Email,
                Rol = ExtraerRolPrincipal(u),
                Estado = u.EstadoDisplay
            });

            var filtrado = AplicarFiltros(modelo);
            _view = new BindingList<UsuarioVM>(filtrado.ToList());
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
            bool? fActivo = (cboEstado.SelectedItem as ItemComboEstado)?.Valor; // true/false/null
            int? fRolId = (cboRol.SelectedItem as ItemComboRol)?.Id;

            var q = origen;

            if (!string.IsNullOrEmpty(fUsuario)) q = q.Where(x => Match(x.Usuario, fUsuario));
            if (!string.IsNullOrEmpty(fNombre)) q = q.Where(x => Match(x.Nombre, fNombre));
            if (!string.IsNullOrEmpty(fApellido)) q = q.Where(x => Match(x.Apellido, fApellido));
            if (!string.IsNullOrEmpty(fMail)) q = q.Where(x => Match(x.Mail, fMail));
            if (fActivo.HasValue)
            {
                string esperado = fActivo.Value ? "Habilitado" : "Baja";
                q = q.Where(x => string.Equals(x.Estado, esperado, StringComparison.OrdinalIgnoreCase));
            }
            if (fRolId.HasValue)
            {
                q = q.Where(x =>
                {
                    var familias = _svcRoles.GetFamiliasUsuario(x.Id) ?? new List<BE.Familia>();
                    return familias.Any(f => f.Id == fRolId.Value);
                });
            }

            return q;
        }

        private string ExtraerRolPrincipal(BE.Usuario u)
        {
            if (u == null || u.Permisos == null || u.Permisos.Count == 0) return "(sin rol)";
            var familia = u.Permisos.FirstOrDefault(p => p != null && p.GetType().Name == "Familia");
            return familia != null ? familia.Name : "(sin rol)";
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
                    // Desbloquea si está bloqueado o reactiva si estaba dado de baja
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

            // Columna Roles (contador)
            if (col == "colRoles")
            {
                var familias = _svcRoles.GetFamiliasUsuario(vm.Id) ?? new List<Familia>();
                dgvUsuarios.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = $"Roles ({familias.Count})";
                return;
            }

            // Cambiar texto del botón según estado
            if (col == "btnReactivar")
            {
                var cell = dgvUsuarios.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewButtonCell;
                if (cell != null)
                {
                    // Estado viene del VM (u.EstadoDisplay): "Habilitado", "Bloqueado" o "Baja"
                    cell.Value = vm.Estado.Equals("Bloqueado", StringComparison.OrdinalIgnoreCase)
                        ? "Desbloquear"
                        : "Reactivar";
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
                FileName = string.Format("Usuarios_{0:yyyyMMdd_HHmm}.csv", DateTime.Now)
            })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;

                var sb = new StringBuilder();
                var sep = ",";
                sb.AppendLine(string.Join(sep, new[] { "Usuario", "Apellido", "Nombre", "Mail", "Rol", "Estado" }));
                foreach (var u in _view)
                {
                    Func<string, string> Esc = s => "\"" + (s ?? string.Empty).Replace("\"", "\"\"") + "\"";
                    sb.AppendLine(string.Join(sep, new[] { Esc(u.Usuario), Esc(u.Apellido), Esc(u.Nombre), Esc(u.Mail), Esc(u.Rol), Esc(u.Estado) }));
                }
                System.IO.File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
            }

            KryptonMessageBox.Show("Archivo exportado correctamente.", "Éxito");
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

    public enum ModoForm { Alta, Ver, Editar }
}

