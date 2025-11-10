namespace UI
{
    partial class ConfigStringConn
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigStringConn));
            this.panel1 = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtServidor = new Krypton.Toolkit.KryptonTextBox();
            this.txtBase = new Krypton.Toolkit.KryptonTextBox();
            this.txtUsuarioDb = new Krypton.Toolkit.KryptonTextBox();
            this.txtPassDb = new Krypton.Toolkit.KryptonTextBox();
            this.chkWindows = new Krypton.Toolkit.KryptonCheckBox();
            this.btnProbar = new Krypton.Toolkit.KryptonButton();
            this.btnGuardar = new Krypton.Toolkit.KryptonButton();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.CornflowerBlue;
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Location = new System.Drawing.Point(-3, -5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(253, 445);
            this.panel1.TabIndex = 12;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI Semibold", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(89, 146);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(54, 38);
            this.label6.TabIndex = 14;
            this.label6.Text = "DB";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(44, 108);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(158, 38);
            this.label2.TabIndex = 13;
            this.label2.Text = "CONEXION";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(287, 45);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 25);
            this.label3.TabIndex = 39;
            this.label3.Text = "Server";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(289, 103);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(129, 25);
            this.label1.TabIndex = 40;
            this.label1.Text = "Base de Datos";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semibold", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(289, 161);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(105, 25);
            this.label4.TabIndex = 41;
            this.label4.Text = "Usuario DB";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semibold", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(289, 219);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(106, 25);
            this.label5.TabIndex = 42;
            this.label5.Text = "Contraseña";
            // 
            // txtServidor
            // 
            this.txtServidor.Location = new System.Drawing.Point(292, 73);
            this.txtServidor.Name = "txtServidor";
            this.txtServidor.Size = new System.Drawing.Size(218, 27);
            this.txtServidor.TabIndex = 43;
            // 
            // txtBase
            // 
            this.txtBase.Location = new System.Drawing.Point(292, 131);
            this.txtBase.Name = "txtBase";
            this.txtBase.Size = new System.Drawing.Size(218, 27);
            this.txtBase.TabIndex = 44;
            // 
            // txtUsuarioDb
            // 
            this.txtUsuarioDb.Location = new System.Drawing.Point(292, 189);
            this.txtUsuarioDb.Name = "txtUsuarioDb";
            this.txtUsuarioDb.Size = new System.Drawing.Size(218, 27);
            this.txtUsuarioDb.TabIndex = 45;
            // 
            // txtPassDb
            // 
            this.txtPassDb.Location = new System.Drawing.Point(292, 247);
            this.txtPassDb.Name = "txtPassDb";
            this.txtPassDb.Size = new System.Drawing.Size(218, 27);
            this.txtPassDb.TabIndex = 46;
            // 
            // chkWindows
            // 
            this.chkWindows.Location = new System.Drawing.Point(294, 297);
            this.chkWindows.Name = "chkWindows";
            this.chkWindows.Size = new System.Drawing.Size(146, 24);
            this.chkWindows.TabIndex = 48;
            this.chkWindows.Values.Text = "Usuario Windows";
            // 
            // btnProbar
            // 
            this.btnProbar.Location = new System.Drawing.Point(313, 339);
            this.btnProbar.Name = "btnProbar";
            this.btnProbar.Size = new System.Drawing.Size(163, 34);
            this.btnProbar.TabIndex = 49;
            this.btnProbar.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnProbar.Values.Text = "Probar Conexión";
            // 
            // btnGuardar
            // 
            this.btnGuardar.Location = new System.Drawing.Point(313, 390);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(163, 34);
            this.btnGuardar.TabIndex = 50;
            this.btnGuardar.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnGuardar.Values.Text = "Guardar";
            // 
            // ConfigStringConn
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(604, 425);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnProbar);
            this.Controls.Add(this.chkWindows);
            this.Controls.Add(this.txtPassDb);
            this.Controls.Add(this.txtUsuarioDb);
            this.Controls.Add(this.txtBase);
            this.Controls.Add(this.txtServidor);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ConfigStringConn";
            this.Load += new System.EventHandler(this.ConfigStringConn_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private Krypton.Toolkit.KryptonTextBox txtServidor;
        private Krypton.Toolkit.KryptonTextBox txtBase;
        private Krypton.Toolkit.KryptonTextBox txtUsuarioDb;
        private Krypton.Toolkit.KryptonTextBox txtPassDb;
        private Krypton.Toolkit.KryptonCheckBox chkWindows;
        private Krypton.Toolkit.KryptonButton btnProbar;
        private Krypton.Toolkit.KryptonButton btnGuardar;
    }
}