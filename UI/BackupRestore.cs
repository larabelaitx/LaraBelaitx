using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Krypton.Toolkit;
using BE;
using DAL;   // DbMaintenance
using BLL;
using BitacoraLog = BLL.Bitacora;

namespace UI
{
    public partial class BackupRestore : KryptonForm
    {
        private readonly Usuario _userSession;
        private string _backupPath;
        private string[] _restorePath;
        private Translator _translator;

        public BackupRestore(Usuario user, string lang)
        {
            InitializeComponent();
            _userSession = user;
            LoadLanguage(lang);

            this.Load += BackupRestore_Load;
            btnSelectSave.Click += btnSelectSave_Click;
            btnSave.Click += btnSave_Click;
            btnSelectRestore.Click += btnSelectRestore_Click;
            btnRestore.Click += btnRestore_Click;
        }

        private void BackupRestore_Load(object sender, EventArgs e)
        {
            txtBxRestoreFolder.Text = string.Empty;
            txtBxSaveFolder.Text = string.Empty;

            // Particiones 2..6 (DropDownList)
            cbPart.DropDownStyle = ComboBoxStyle.DropDownList;
            cbPart.Items.Clear();
            cbPart.Items.AddRange(new object[] { "2", "3", "4", "5", "6" });
            if (cbPart.Items.Count > 0) cbPart.SelectedIndex = 0; // valor por defecto = 2
        }

        private void btnSelectSave_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = _translator.Translate("lblSelectSave");
                dlg.ShowNewFolderButton = true;

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _backupPath = dlg.SelectedPath;
                    txtBxSaveFolder.Text = _backupPath;

                    // Aviso si es una ruta problemática (OneDrive, Desktop)
                    var lower = _backupPath.Replace('\\', '/').ToLowerInvariant();
                    if (lower.Contains("/onedrive") || lower.Contains("/desktop"))
                    {
                        KryptonMessageBox.Show(
                            "⚠️ Recomendado: usá una carpeta local como C:\\SQLBackups y otorgá permisos al servicio SQL.\n" +
                            "Las rutas de OneDrive o Escritorio suelen generar 'Access denied' al hacer backup.",
                            "Sugerencia",
                            KryptonMessageBoxButtons.OK,
                            KryptonMessageBoxIcon.Information);
                    }
                }
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_backupPath))
            {
                KryptonMessageBox.Show(_translator.Translate("alertFolder"), "Alerta",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                return;
            }

            try { if (!Directory.Exists(_backupPath)) Directory.CreateDirectory(_backupPath); }
            catch (Exception ex)
            {
                KryptonMessageBox.Show($"{_translator.Translate("infoBackupE")} {ex.Message}", "Error",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
                return;
            }

            // Validar particiones entre 2 y 6
            int partitions = 2;
            if (!int.TryParse(cbPart.Text, out partitions)) partitions = 2;
            partitions = Math.Max(2, Math.Min(6, partitions));

            ToggleUi(false, true);
            try
            {
                await System.Threading.Tasks.Task.Run(() => DbMaintenance.Backup(_backupPath, partitions));

                KryptonMessageBox.Show(_translator.Translate("infoBackupOk"), "Información",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);

                // Bitácora (tolerante)
                try { BitacoraLog.Info(_userSession?.Id, $"Backup realizado por: {_userSession?.UserName} {DateTime.Now:yyyy-MM-dd HH:mm:ss}"); } catch { }

                BackupRestore_Load(sender, e);
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show($"{_translator.Translate("infoBackupE")} {ex.Message}", "Error",
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
                dlg.Title = _translator.Translate("selectBackupFileTitle");
                dlg.Filter = "Backup (*.bak)|*.bak|All files (*.*)|*.*";
                dlg.Multiselect = true;

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _restorePath = dlg.FileNames?.Where(File.Exists).ToArray() ?? Array.Empty<string>();
                    txtBxRestoreFolder.Text = string.Join("; ", _restorePath);
                }
            }
        }

        private async void btnRestore_Click(object sender, EventArgs e)
        {
            if (_restorePath == null || _restorePath.Length == 0)
            {
                KryptonMessageBox.Show(_translator.Translate("alertFolder"), "Alerta",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                return;
            }

            var ask = KryptonMessageBox.Show(
                _translator.Translate("confirmRestore") ??
                "Esto restaurará la base de datos con el/los backup(s) seleccionado(s).\n¿Deseás continuar?",
                "Confirmar",
                KryptonMessageBoxButtons.YesNo,
                KryptonMessageBoxIcon.Warning);
            if (ask != DialogResult.Yes) return;

            ToggleUi(false, true);
            try
            {
                await System.Threading.Tasks.Task.Run(() => DbMaintenance.Restore(_restorePath));

                KryptonMessageBox.Show(_translator.Translate("infoRestoreOk"), "Información",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);

                // Bitácora (tolerante)
                try { BitacoraLog.Warn(_userSession?.Id, $"Restore realizado por: {_userSession?.UserName} {DateTime.Now:yyyy-MM-dd HH:mm:ss}"); } catch { }

                BackupRestore_Load(sender, e);
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show($"{_translator.Translate("infoRestoreE")}: {ex.Message}", "Error",
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

        private void translateControls(Control.ControlCollection control)
        {
            foreach (Control c in control)
            {
                if (c is KryptonButton || c is KryptonLabel)
                    c.Text = _translator.Translate(c.Name);

                if (c is KryptonGroupBox)
                {
                    c.Text = _translator.Translate(c.Name);
                    if (c.Controls.Count > 0 && c.Controls[0] is Control inner)
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
