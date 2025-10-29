using System;
using System.Windows.Forms;
using Krypton.Toolkit;

namespace UI
{
    public partial class ChangePasswordDialog : KryptonForm
    {
        public string NewPassword { get; private set; }

        private KryptonTextBox txtNueva;
        private KryptonTextBox txtRepetir;
        private KryptonCheckBox chkMostrar;
        private KryptonButton btnOk;
        private KryptonButton btnCancel;

        public ChangePasswordDialog()
        {
            InitializeComponent();
            BuildUi();

            // ⬇️ Agregados
            this.Load += ChangePasswordDialog_Load;
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }

        private void BuildUi()
        {
            Text = "Cambiar contraseña";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            Width = 520; Height = 220;

            var lbl1 = new KryptonLabel { Left = 20, Top = 25, Width = 180, Text = "Ingresá tu nueva contraseña:" };
            var lbl2 = new KryptonLabel { Left = 20, Top = 75, Width = 180, Text = "Repetí la contraseña:" };

            txtNueva = new KryptonTextBox { Left = 220, Top = 22, Width = 260, PasswordChar = '•' };
            txtRepetir = new KryptonTextBox { Left = 220, Top = 72, Width = 260, PasswordChar = '•' };

            chkMostrar = new KryptonCheckBox { Left = 220, Top = 110, Text = "Mostrar" };
            chkMostrar.CheckedChanged += (s, e) =>
            {
                char ch = chkMostrar.Checked ? '\0' : '•';
                txtNueva.PasswordChar = ch;
                txtRepetir.PasswordChar = ch;
            };

            btnOk = new KryptonButton { Left = 300, Top = 140, Width = 80, Text = "OK" };
            btnCancel = new KryptonButton { Left = 400, Top = 140, Width = 80, Text = "Cancelar" };

            btnOk.Click += (s, e) =>
            {
                var p1 = txtNueva.Text.Trim();
                var p2 = txtRepetir.Text.Trim();

                if (string.IsNullOrWhiteSpace(p1))
                {
                    KryptonMessageBox.Show("La contraseña no puede estar vacía.", "Validación",
                        KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                    txtNueva.Focus(); return;
                }
                if (p1 != p2)
                {
                    KryptonMessageBox.Show("Las contraseñas no coinciden.", "Validación",
                        KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                    txtRepetir.Focus(); return;
                }
                NewPassword = p1;
                DialogResult = DialogResult.OK;
                Close();
            };

            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(lbl1);
            Controls.Add(lbl2);
            Controls.Add(txtNueva);
            Controls.Add(txtRepetir);
            Controls.Add(chkMostrar);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
        }

        private void ChangePasswordDialog_Load(object sender, EventArgs e)
        {
            try { txtNueva?.Focus(); } catch { }
        }
    }
}
