// UI/Menu.cs
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using Krypton.Toolkit;
using BE;
using BLL.Contracts;
using BLL.Services;
using UI.Seguridad;   // Perms
using Services;       // SecurityContext
using DAL;

namespace UI
{
    public partial class Menu : KryptonForm
    {
        private const bool FORCE_SHOW_MENU = false;

        private readonly IUsuarioService _usrSvc;
        private readonly IRolService _rolSvc;
        private readonly Usuario _usuarioActual;

        private readonly IClienteService _cliSvc = new ClienteService();
        private readonly ICuentaService _ctaSvc = new CuentaService();
        private readonly ITarjetaService _tarSvc = new TarjetaService();

        public Menu() : this(new UsuarioService(), new RolService(), null) { }
        public Menu(Usuario usuarioActual) : this(new UsuarioService(), new RolService(), usuarioActual) { }
        public Menu(IUsuarioService usrSvc, IRolService rolSvc, Usuario usuarioActual = null)
        {
            InitializeComponent();

            _usrSvc = usrSvc ?? throw new ArgumentNullException(nameof(usrSvc));
            _rolSvc = rolSvc ?? throw new ArgumentNullException(nameof(rolSvc));
            _usuarioActual = usuarioActual;

            var lbl = Controls.Find("lblUsuario", true).FirstOrDefault() as Label;
            if (lbl != null && _usuarioActual != null)
                lbl.Text = _usuarioActual.NombreCompleto ?? _usuarioActual.UserName ?? lbl.Text;

            TryWire("btnUsuariosRolesPermisos", btnUsuariosRolesPermisos_Click);
            TryWire("btnFamilias", btnFamilias_Click);
            TryWire("btnPermisos", btnPermisos_Click);
            TryWire("btnBackupRestore", btnBackupRestore_Click);
            TryWire("btnClientes", btnClientes_Click);
            TryWire("btnCuentas", btnCuentas_Click);
            TryWire("btnTarjetas", btnTarjetas_Click);
            TryWire("btnBitacora", btnBitacora_Click);
            TryWire("btnDigitosVerificadores", btnDigitosVerificadores_Click);
            TryWire("btnCerrarSesion", btnCerrarSesion_Click);
            TryWire("btnSalir", (s, e) => Close());

            Load += Menu_Load;
        }

        private int CurrentUserId => SecurityContext.CurrentUser?.Id ?? _usuarioActual?.Id ?? 0;

        private void Menu_Load(object sender, EventArgs e)
        {
            if (SecurityContext.CurrentUser == null && _usuarioActual != null)
                SecurityContext.CurrentUser = _usuarioActual;

            ApplySecurity();
        }

        private void ApplySecurity()
        {
            if (FORCE_SHOW_MENU)
            {
                foreach (Control c in Controls.Cast<Control>().SelectMany(AllControls))
                    if (c is ButtonBase || c is KryptonButton) { c.Visible = true; c.Enabled = true; }
                SetVisibleIf("btnCerrarSesion", true);
                SetVisibleIf("btnSalir", true);
                return;
            }

            bool hasUser = CurrentUserId > 0;

            SetVisibleIf("btnUsuariosRolesPermisos", hasUser && _rolSvc.TieneAlguna(CurrentUserId,
                Perms.Usuario_Listar, Perms.Familia_Listar, Perms.Patente_Listar));

            SetVisibleIf("btnFamilias", hasUser && _rolSvc.TienePatente(CurrentUserId, Perms.Familia_Listar));
            SetVisibleIf("btnPermisos", hasUser && _rolSvc.TienePatente(CurrentUserId, Perms.Patente_Listar));

            SetVisibleIf("btnBackupRestore", hasUser && _rolSvc.TieneAlguna(CurrentUserId,
                Perms.Backup_Crear, Perms.Backup_Restaurar));

            SetVisibleIf("btnClientes", hasUser && _rolSvc.TienePatente(CurrentUserId, Perms.Cliente_Listar));
            SetVisibleIf("btnCuentas", hasUser && _rolSvc.TienePatente(CurrentUserId, Perms.Cuenta_Listar));
            SetVisibleIf("btnTarjetas", hasUser && _rolSvc.TienePatente(CurrentUserId, Perms.Tarjeta_Listar));

            SetVisibleIf("btnBitacora", hasUser && _rolSvc.TienePatente(CurrentUserId, Perms.Bitacora_Listar));
            SetVisibleIf("btnDigitosVerificadores", hasUser && _rolSvc.TienePatente(CurrentUserId, Perms.DV_Verificar));

            SetVisibleIf("btnCerrarSesion", true);
            SetVisibleIf("btnSalir", true);
        }

        private static IEnumerable<Control> AllControls(Control root)
        {
            foreach (Control child in root.Controls)
            {
                yield return child;
                foreach (var sub in AllControls(child)) yield return sub;
            }
        }

        private void SetVisibleIf(string controlName, bool visible)
        {
            var ctrl = Controls.Find(controlName, true).FirstOrDefault();
            if (ctrl != null) { ctrl.Visible = visible; ctrl.Enabled = visible; }
        }

        private void TryWire(string buttonName, EventHandler handler)
        {
            var btn = Controls.Find(buttonName, true).FirstOrDefault() as Control;
            if (btn != null) btn.Click += handler;
        }

        // ====== Navegación ======
        private void btnUsuariosRolesPermisos_Click(object sender, EventArgs e)
        {
            if (!_rolSvc.TieneAlguna(CurrentUserId, Perms.Usuario_Listar, Perms.Familia_Listar, Perms.Patente_Listar)) return;
            using (var frm = new MainURP(_usrSvc, _rolSvc, CurrentUserId)) frm.ShowDialog(this);
        }

        private void btnFamilias_Click(object sender, EventArgs e)
        {
            if (!_rolSvc.TienePatente(CurrentUserId, Perms.Familia_Listar)) return;
            using (var frm = new MainFamilia(_rolSvc, CurrentUserId)) frm.ShowDialog(this);
        }

        private void btnPermisos_Click(object sender, EventArgs e)
        {
            if (!_rolSvc.TienePatente(CurrentUserId, Perms.Patente_Listar)) return;
            using (var frm = new AltaPatente(_usrSvc, _rolSvc, CurrentUserId)) frm.ShowDialog(this);
        }

        private void btnBackupRestore_Click(object sender, EventArgs e)
        {
            if (!_rolSvc.TieneAlguna(CurrentUserId, Perms.Backup_Crear, Perms.Backup_Restaurar)) return;

            var user = SecurityContext.CurrentUser ?? _usuarioActual;
            var lang = (user != null && user.IdiomaId == 2) ? "en-US" :
                       (user != null && user.IdiomaId == 3) ? "pt-BR" : "es-AR";
            using (var frm = new BackupRestore(user, lang)) frm.ShowDialog(this);
        }

        private void btnClientes_Click(object sender, EventArgs e)
        {
            if (!_rolSvc.TienePatente(CurrentUserId, Perms.Cliente_Listar)) return;
            using (var frm = new MainClientes(new ClienteService(), new RolService(), CurrentUserId)) frm.ShowDialog(this);
        }

        private void btnCuentas_Click(object sender, EventArgs e)
        {
            if (!_rolSvc.TienePatente(CurrentUserId, Perms.Cuenta_Listar)) return;
            using (var frm = new MainCuentas(new CuentaService(), new RolService(), CurrentUserId, null)) frm.ShowDialog(this);
        }

        private void btnTarjetas_Click(object sender, EventArgs e)
        {
            if (!_rolSvc.TienePatente(CurrentUserId, Perms.Tarjeta_Listar)) return;
            using (var frm = new MainTarjetas()) frm.ShowDialog(this);
        }

        private void btnBitacora_Click(object sender, EventArgs e)
        {
            if (!_rolSvc.TienePatente(CurrentUserId, Perms.Bitacora_Listar)) return;
            string cnn;
            using (var cn = ConnectionFactory.Open()) cnn = cn.ConnectionString;
            using (var frm = new Bitacora(cnn)) frm.ShowDialog(this);
        }

        private void btnDigitosVerificadores_Click(object sender, EventArgs e)
        {
            if (!_rolSvc.TienePatente(CurrentUserId, Perms.DV_Verificar)) return;
            using (var frm = new MainDV(new DVService(), new RolService(), CurrentUserId)) frm.ShowDialog(this);
        }

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            SecurityContext.Clear();
            var login = Application.OpenForms.OfType<Login>().FirstOrDefault();
            if (login != null) { login.WindowState = FormWindowState.Normal; login.Activate(); }
            Close();
        }
    }
}
