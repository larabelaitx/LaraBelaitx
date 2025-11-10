using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Krypton.Toolkit;
using BE;
using DAL;
using BLL;
using BLL.Contracts;

namespace UI
{
    public partial class BackupRestore : KryptonForm
    {
        private readonly Usuario _userSession;
        private readonly IRolService _rolSvc;
        private readonly int? _currentUserId;
        private const string P_BACKUP_RESTORE = "BACKUP_RESTORE";

        private string _backupPath;
        private string[] _restorePath;
        private Translator _translator;

        public BackupRestore(Usuario user, string lang)
            : this(user, lang, null, null) { }

        public BackupRestore(Usuario user, string lang, IRolService rolSvc, int? currentUserId)
        {
            InitializeComponent();
            _userSession = user;
            _rolSvc = rolSvc;
            _currentUserId = currentUserId;

            LoadLanguage(lang);

            this.Load += BackupRestore_Load;
            btnSelectSave.Click += btnSelectSave_Click;
            btnSave.Click += btnSave_Click;
            btnSelectRestore.Click += btnSelectRestore_Click;
            btnRestore.Click += btnRestore_Click;
        }

        private void BackupRestore_Load(object sender, EventArgs e)
        {
            if (_rolSvc != null && _currentUserId.HasValue &&
                !_rolSvc.TienePatente(_currentUserId.Value, P_BACKUP_RESTORE))
            {
                KryptonMessageBox.Show(this,
                    "No tenés permiso para acceder a esta funcionalidad.\n(BACKUP_RESTORE)",
                    "Acceso denegado", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                Close();
                return;
            }

            txtBxRestoreFolder.Text = "";
            txtBxSaveFolder.Text = "";

            cbPart.DropDownStyle = ComboBoxStyle.DropDownList;
            cbPart.Items.Clear();
            cbPart.Items.AddRange(new object[] { "2", "3", "4", "5", "6" });
            cbPart.SelectedIndex = 0;
        }

        private void btnSelectSave_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = _translator != null
                    ? _translator.Translate("lblSelectSave")
                    : "Elegí la carpeta para guardar los backups";
                dlg.ShowNewFolderButton = true;

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _backupPath = dlg.SelectedPath;
                    txtBxSaveFolder.Text = _backupPath;

                    string lower = _backupPath.Replace('\\', '/').ToLowerInvariant();
                    if (lower.Contains("/onedrive") || lower.Contains("/desktop"))
                    {
                        KryptonMessageBox.Show(
                            "⚠️ Sugerencia: usá una carpeta local (p.ej., C:\\SQLBackups) y asegurá permisos al servicio SQL.\n" +
                            "Rutas de OneDrive/Escritorio suelen causar 'Access denied' en backups.",
                            "Sugerencia", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                    }
                }
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_backupPath))
            {
                KryptonMessageBox.Show(
                    _translator != null ? _translator.Translate("alertFolder") : "Seleccioná una carpeta.",
                    "Alerta", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (!Directory.Exists(_backupPath)) Directory.CreateDirectory(_backupPath);
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show("Error al crear carpeta: " + ex.Message,
                    "Error", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
                return;
            }

            int partitions;
            if (!int.TryParse(cbPart.Text, out partitions)) partitions = 2;
            partitions = Math.Max(2, Math.Min(6, partitions));

            ToggleUi(false, true);
            try
            {
                await System.Threading.Tasks.Task.Run(() => DbMaintenance.Backup(_backupPath, partitions));

                KryptonMessageBox.Show("Backup realizado correctamente.", "Información",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);

                try
                {
                    BLL.Bitacora.Info(_userSession != null ? _userSession.Id : (int?)null,
                        "Backup realizado por: " + (_userSession != null ? _userSession.UserName : "Desconocido"),
                        "DB", "Backup");
                }
                catch { }

                BackupRestore_Load(sender, e);
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show("Error al realizar el backup: " + ex.Message, "Error",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
            finally
            {
                ToggleUi(true, false);
            }
        }

        private void btnSelectRestore_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "Seleccioná los archivos .bak";
                dlg.Filter = "Backup (*.bak)|*.bak|Todos los archivos (*.*)|*.*";
                dlg.Multiselect = true;

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _restorePath = dlg.FileNames != null
                        ? dlg.FileNames.Where(File.Exists).ToArray()
                        : new string[0];
                    txtBxRestoreFolder.Text = string.Join("; ", _restorePath);
                }
            }
        }

        private async void btnRestore_Click(object sender, EventArgs e)
        {
            if (_restorePath == null || _restorePath.Length == 0)
            {
                KryptonMessageBox.Show("Seleccioná archivo(s) de backup.", "Alerta",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                return;
            }

            DialogResult ask = KryptonMessageBox.Show(
                "Esto restaurará la base de datos con el/los backup(s) seleccionado(s).\n¿Deseás continuar?",
                "Confirmar", KryptonMessageBoxButtons.YesNo, KryptonMessageBoxIcon.Warning);
            if (ask != DialogResult.Yes) return;

            ToggleUi(false, true);
            try
            {
                await System.Threading.Tasks.Task.Run(() => DbMaintenance.Restore(_restorePath));

                KryptonMessageBox.Show("Restore completado.", "Información",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);

                try
                {
                    BLL.Bitacora.Warn(_userSession != null ? _userSession.Id : (int?)null,
                        "Restore realizado por: " + (_userSession != null ? _userSession.UserName : "Desconocido"),
                        "DB", "Restore");
                }
                catch { }

                BackupRestore_Load(sender, e);
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show("Error al restaurar: " + ex.Message, "Error",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
            finally
            {
                ToggleUi(true, false);
            }
        }

        private void LoadLanguage(string languageCode)
        {
            _translator = new Translator(languageCode);
            translateControls(this.Controls);
        }

        private void translateControls(Control.ControlCollection controls)
        {
            if (_translator == null) return;

            foreach (Control c in controls)
            {
                if (c is KryptonButton || c is KryptonLabel)
                    c.Text = _translator.Translate(c.Name);

                var group = c as KryptonGroupBox;
                if (group != null)
                {
                    group.Text = _translator.Translate(group.Name);
                    if (group.Controls.Count > 0 && group.Controls[0] is Control inner)
                    {
                        foreach (Control conIn in inner.Controls)
                        {
                            if (conIn is KryptonButton || conIn is KryptonLabel || conIn is KryptonCheckBox)
                                conIn.Text = _translator.Translate(conIn.Name);
                        }
                    }
                }
            }
        }

        private void ToggleUi(bool enable, bool waitCursor)
        {
            btnSelectSave.Enabled = enable;
            btnSave.Enabled = enable;
            btnSelectRestore.Enabled = enable;
            btnRestore.Enabled = enable;
            Cursor.Current = waitCursor ? Cursors.WaitCursor : Cursors.Default;
        }
    }
}
