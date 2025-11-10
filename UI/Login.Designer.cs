using System.Windows.Forms;
using Krypton.Toolkit;

namespace UI
{
    partial class Login
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblUsuario;
        private Label lblContraseña;
        private KryptonTextBox txtUsuario;
        private KryptonTextBox txtContraseña;
        private KryptonButton btnIngresar;
        private KryptonButton btnOlvideContraseña;
        private Label lblIdioma;
        private KryptonComboBox cboIdioma;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Login));
            this.lblUsuario = new System.Windows.Forms.Label();
            this.lblContraseña = new System.Windows.Forms.Label();
            this.txtUsuario = new Krypton.Toolkit.KryptonTextBox();
            this.txtContraseña = new Krypton.Toolkit.KryptonTextBox();
            this.btnIngresar = new Krypton.Toolkit.KryptonButton();
            this.btnOlvideContraseña = new Krypton.Toolkit.KryptonButton();
            this.lblIdioma = new System.Windows.Forms.Label();
            this.cboIdioma = new Krypton.Toolkit.KryptonComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.cboIdioma)).BeginInit();
            this.SuspendLayout();
            // 
            // lblUsuario
            // 
            this.lblUsuario.AutoSize = true;
            this.lblUsuario.BackColor = System.Drawing.Color.Transparent;
            this.lblUsuario.Font = new System.Drawing.Font("Segoe UI", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsuario.Location = new System.Drawing.Point(194, 394);
            this.lblUsuario.Name = "lblUsuario";
            this.lblUsuario.Size = new System.Drawing.Size(103, 31);
            this.lblUsuario.TabIndex = 0;
            this.lblUsuario.Text = "Usuario:";
            // 
            // lblContraseña
            // 
            this.lblContraseña.AutoSize = true;
            this.lblContraseña.BackColor = System.Drawing.Color.Transparent;
            this.lblContraseña.Font = new System.Drawing.Font("Segoe UI", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblContraseña.Location = new System.Drawing.Point(194, 447);
            this.lblContraseña.Name = "lblContraseña";
            this.lblContraseña.Size = new System.Drawing.Size(140, 31);
            this.lblContraseña.TabIndex = 1;
            this.lblContraseña.Text = "Contraseña:";
            // 
            // txtUsuario
            // 
            this.txtUsuario.Location = new System.Drawing.Point(340, 398);
            this.txtUsuario.Name = "txtUsuario";
            this.txtUsuario.Size = new System.Drawing.Size(242, 27);
            this.txtUsuario.TabIndex = 2;
            // 
            // txtContraseña
            // 
            this.txtContraseña.Location = new System.Drawing.Point(340, 451);
            this.txtContraseña.Name = "txtContraseña";
            this.txtContraseña.PasswordChar = '*';
            this.txtContraseña.Size = new System.Drawing.Size(242, 27);
            this.txtContraseña.TabIndex = 3;
            // 
            // btnIngresar
            // 
            this.btnIngresar.Location = new System.Drawing.Point(200, 498);
            this.btnIngresar.Name = "btnIngresar";
            this.btnIngresar.Size = new System.Drawing.Size(171, 32);
            this.btnIngresar.TabIndex = 4;
            this.btnIngresar.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnIngresar.Values.Text = "Ingresar";
            this.btnIngresar.Click += new System.EventHandler(this.btnIngresar_Click);
            // 
            // btnOlvideContraseña
            // 
            this.btnOlvideContraseña.Location = new System.Drawing.Point(403, 498);
            this.btnOlvideContraseña.Name = "btnOlvideContraseña";
            this.btnOlvideContraseña.Size = new System.Drawing.Size(179, 32);
            this.btnOlvideContraseña.TabIndex = 5;
            this.btnOlvideContraseña.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnOlvideContraseña.Values.Text = "Olvidé clave";
            this.btnOlvideContraseña.Click += new System.EventHandler(this.btnOlvideContraseña_Click);
            // 
            // lblIdioma
            // 
            this.lblIdioma.AutoSize = true;
            this.lblIdioma.BackColor = System.Drawing.Color.Transparent;
            this.lblIdioma.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIdioma.Location = new System.Drawing.Point(194, 356);
            this.lblIdioma.Name = "lblIdioma";
            this.lblIdioma.Size = new System.Drawing.Size(139, 21);
            this.lblIdioma.TabIndex = 7;
            this.lblIdioma.Text = "Idioma / Language:";
            // 
            // cboIdioma
            // 
            this.cboIdioma.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboIdioma.DropDownWidth = 150;
            this.cboIdioma.Items.AddRange(new object[] {
            "Español",
            "English"});
            this.cboIdioma.Location = new System.Drawing.Point(340, 353);
            this.cboIdioma.Name = "cboIdioma";
            this.cboIdioma.Size = new System.Drawing.Size(242, 26);
            this.cboIdioma.TabIndex = 4;
            this.cboIdioma.SelectedIndexChanged += new System.EventHandler(this.cboIdioma_SelectedIndexChanged);
            // 
            // Login
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(814, 567);
            this.Controls.Add(this.btnOlvideContraseña);
            this.Controls.Add(this.btnIngresar);
            this.Controls.Add(this.txtContraseña);
            this.Controls.Add(this.txtUsuario);
            this.Controls.Add(this.lblContraseña);
            this.Controls.Add(this.lblUsuario);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Login";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login";
            this.Load += new System.EventHandler(this.Login_Load);
            ((System.ComponentModel.ISupportInitialize)(this.cboIdioma)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
    }
}
