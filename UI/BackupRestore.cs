using System;
using System.Linq;
using System.Windows.Forms;
using Krypton.Toolkit;
using BE;
using DAL;   // usa DbMaintenance
using BLL;

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

            btnSelectSave.Click += btnSelectSave_Click;
            btnSave.Click += btnSave_Click;
            btnSelectRestore.Click += btnSelectRestore_Click;
            btnRestore.Click += btnRestore_Click;

            this.Load += BackupRestore_Load;
        }

        private void btnSelectSave_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = _translator.Translate("lblSelectSave");
                folderDialog.ShowNewFolderButton = true;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    _backupPath = folderDialog.SelectedPath;
                    txtBxSaveFolder.Text = _backupPath;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_backupPath))
            {
                KryptonMessageBox.Show(_translator.Translate("alertFolder"), "Alerta");
                return;
            }

            try
            {
                int partitions = !string.IsNullOrWhiteSpace(cbPart.Text) && int.TryParse(cbPart.Text, out var p) ? p : 1;

                // 👉 nuevo llamado
                DbMaintenance.Backup(_backupPath, partitions);

                KryptonMessageBox.Show(_translator.Translate("infoBackupOk"), "Información");
                BLL.Bitacora.Info(_userSession?.Id, $"Backup realizado por: {_userSession?.UserName} {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                BackupRestore_Load(sender, e);
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show($"{_translator.Translate("infoBackupE")} {ex.Message}", "Error");
            }
        }

        private void btnSelectRestore_Click(object sender, EventArgs e)
        {
            using (var fileDialog = new OpenFileDialog())
            {
                fileDialog.Title = _translator.Translate("selectBackupFileTitle");
                fileDialog.Filter = "Backup (*.bak)|*.bak|All files (*.*)|*.*";
                fileDialog.Multiselect = true;

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    _restorePath = fileDialog.FileNames;
                    txtBxRestoreFolder.Text = string.Join("; ", _restorePath ?? Array.Empty<string>());
                }
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            if (_restorePath == null || _restorePath.Length == 0 || _restorePath.Any(string.IsNullOrWhiteSpace))
            {
                KryptonMessageBox.Show(_translator.Translate("alertFolder"), "Alerta");
                return;
            }

            try
            {
                // 👉 nuevo llamado
                DbMaintenance.Restore(_restorePath);

                KryptonMessageBox.Show(_translator.Translate("infoRestoreOk"), "Información");
                BLL.Bitacora.Warn(_userSession?.Id, $"Restore realizado por: {_userSession?.UserName} {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                BackupRestore_Load(sender, e);
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show($"{_translator.Translate("infoRestoreE")}: {ex.Message}.", "Error");
            }
        }

        private void BackupRestore_Load(object sender, EventArgs e)
        {
            txtBxRestoreFolder.Text = string.Empty;
            txtBxSaveFolder.Text = string.Empty;
            cbPart.Text = string.Empty;
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
    }
}
