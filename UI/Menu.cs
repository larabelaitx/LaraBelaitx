using System;
using System.Linq;
using System.Windows.Forms;
using Krypton.Toolkit;

using BE;
using BLL.Contracts;
using BLL.Services;   // UsuarioService, RolService

namespace UI
{
    public partial class Menu : KryptonForm
    {
        // Servicios compartidos por el menú y los formularios que abre
        private readonly IUsuarioService _usrSvc;
        private readonly IRolService _rolSvc;

        // Usuario autenticado (opcional)
        private readonly Usuario _usuarioActual;

        // ---- Constructores ----
        public Menu() : this(new UsuarioService(), new RolService(), null) { }
        public Menu(Usuario usuarioActual) : this(new UsuarioService(), new RolService(), usuarioActual) { }
        public Menu(IUsuarioService usrSvc, IRolService rolSvc, Usuario usuarioActual = null)
        {
            InitializeComponent();

            _usrSvc = usrSvc ?? throw new ArgumentNullException(nameof(usrSvc));
            _rolSvc = rolSvc ?? throw new ArgumentNullException(nameof(rolSvc));
            _usuarioActual = usuarioActual;

            // Etiqueta de bienvenida si existe
            var lbl = this.Controls.Find("lblUsuario", true).FirstOrDefault() as Label;
            if (lbl != null && _usuarioActual != null)
                lbl.Text = _usuarioActual?.NombreCompleto ?? _usuarioActual?.UserName ?? lbl.Text;

            // Wire-up de botones
            TryWire("btnUsuariosRolesPermisos", btnUsuariosRolesPermisos_Click); // URP hub
            TryWire("btnFamilias", btnFamilias_Click);             // Roles/Familias
            TryWire("btnPermisos", btnPermisos_Click);             // Patentes directas
            TryWire("btnBackupRestore", btnBackupRestore_Click);
            TryWire("btnClientes", btnClientes_Click);
            TryWire("btnCuentas", btnCuentas_Click);
            TryWire("btnTarjetas", btnTarjetas_Click);
            TryWire("btnBitacora", btnBitacora_Click);
            TryWire("btnDigitosVerificadores", btnDigitosVerificadores_Click);
            TryWire("btnCerrarSesion", btnCerrarSesion_Click);
            TryWire("btnSalir", (s, e) => Close());
        }

        // Helper para asociar eventos sin romper si el control no está
        private void TryWire(string buttonName, EventHandler handler)
        {
            var btn = this.Controls.Find(buttonName, true).FirstOrDefault() as Control;
            if (btn != null) btn.Click += handler;
        }

        // Helper para abrir modal con manejo de errores
        private void OpenModal(Form frm)
        {
            try
            {
                using (frm) frm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(this, ex.Message, "Error", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }

        // === Handlers ===

        // Hub URP (desde acá podés entrar a Usuarios, Familias, Patentes)
        private void btnUsuariosRolesPermisos_Click(object sender, EventArgs e)
        {
            OpenModal(new MainURP(_usrSvc, _rolSvc));
        }

        // Mantenimiento de Familias/Roles compuestos
        private void btnFamilias_Click(object sender, EventArgs e)
        {
            OpenModal(new MainFamilia(_rolSvc));
        }

        // Asignar patentes directas a un usuario
        private void btnPermisos_Click(object sender, EventArgs e)
        {
            OpenModal(new AltaPatente(_usrSvc, _rolSvc));
        }

        // Backup / Restore
        private void btnBackupRestore_Click(object sender, EventArgs e)
        {
            try
            {
                var user = _usuarioActual;
                string lang =
                    (user?.IdiomaId == 2) ? "en-US" :
                    (user?.IdiomaId == 3) ? "pt-BR" :
                    "es-AR";

                OpenModal(new BackupRestore(user, lang));
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(
                    $"Error al abrir el módulo de Backup/Restore:\n{ex.Message}",
                    "Error",
                    KryptonMessageBoxButtons.OK,
                    KryptonMessageBoxIcon.Error);
            }
        }

        // Clientes (archivo: MainClientes.cs)
        private void btnClientes_Click(object sender, EventArgs e)
        {
            // Si tu clase se llama distinto, cambiá aquí.
            OpenModal(new MainClientes());
        }

        // Cuentas (archivo: MainCuentas.cs)
        private void btnCuentas_Click(object sender, EventArgs e)
        {
            OpenModal(new MainCuentas());
        }

        // Tarjetas (si tu form existe como MainTarjetas; si no, mostramos aviso)
        private void btnTarjetas_Click(object sender, EventArgs e)
        {
            try
            {
                var frmType = Type.GetType("UI.MainTarjetas");
                if (frmType == null)
                {
                    KryptonMessageBox.Show(this,
                        "La pantalla de Tarjetas aún no está implementada (UI.MainTarjetas).",
                        "Información", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                    return;
                }
                var frm = (Form)Activator.CreateInstance(frmType);
                OpenModal(frm);
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(this, ex.Message, "Error", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }

        // Bitácora (archivo: Bitacora.cs, clase Bitacora)
        private void btnBitacora_Click(object sender, EventArgs e)
        {
            try
            {
                // Tomo el connection string actual desde la misma factory que usa tu DAL
                string cnn;
                using (var cn = DAL.ConnectionFactory.Open())
                {
                    cnn = cn.ConnectionString;
                }

                using (var frm = new Bitacora(cnn))
                {
                    frm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show(this,
                    "No se pudo abrir Bitácora.\n\n" + ex.Message,
                    "Error", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
        }


        // Dígitos Verificadores (archivo: MainDV.cs)
        private void btnDigitosVerificadores_Click(object sender, EventArgs e)
        {
            OpenModal(new MainDV());
        }

        // Cerrar sesión (volver al Login si tu app lo maneja así)
        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            if (KryptonMessageBox.Show(this, "¿Cerrar sesión?", "Confirmar",
                KryptonMessageBoxButtons.YesNo, KryptonMessageBoxIcon.Question) == DialogResult.Yes)
            {
                Close(); // el Login que te abrió el menú se vuelve a activar (según tu implementación actual)
            }
        }
    }
}
