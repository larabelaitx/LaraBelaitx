namespace UI
{
    partial class MainURP
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainURP));
            this.kryptonGroupBox2 = new Krypton.Toolkit.KryptonGroupBox();
            this.btnPermisos = new System.Windows.Forms.Button();
            this.btnRoles = new System.Windows.Forms.Button();
            this.btnUsuarios = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnVolver = new System.Windows.Forms.Button();
            this.labelTitulo = new System.Windows.Forms.Label();
            this.btnPerfil = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonGroupBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonGroupBox2.Panel)).BeginInit();
            this.kryptonGroupBox2.Panel.SuspendLayout();
            this.kryptonGroupBox2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // kryptonGroupBox2
            // 
            this.kryptonGroupBox2.CaptionStyle = Krypton.Toolkit.LabelStyle.AlternateControl;
            this.kryptonGroupBox2.Dock = System.Windows.Forms.DockStyle.Left;
            this.kryptonGroupBox2.Location = new System.Drawing.Point(0, 56);
            // 
            // kryptonGroupBox2.Panel
            // 
            this.kryptonGroupBox2.Panel.Controls.Add(this.btnPermisos);
            this.kryptonGroupBox2.Panel.Controls.Add(this.btnRoles);
            this.kryptonGroupBox2.Panel.Controls.Add(this.btnUsuarios);
            this.kryptonGroupBox2.Size = new System.Drawing.Size(285, 471);
            this.kryptonGroupBox2.TabIndex = 0;
            // 
            // btnPermisos
            // 
            this.btnPermisos.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnPermisos.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnPermisos.Location = new System.Drawing.Point(0, 120);
            this.btnPermisos.Name = "btnPermisos";
            this.btnPermisos.Size = new System.Drawing.Size(281, 60);
            this.btnPermisos.TabIndex = 0;
            this.btnPermisos.Text = "Permisos";
            this.btnPermisos.UseVisualStyleBackColor = true;
            this.btnPermisos.Click += new System.EventHandler(this.btnPermisos_Click);
            // 
            // btnRoles
            // 
            this.btnRoles.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnRoles.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnRoles.Location = new System.Drawing.Point(0, 60);
            this.btnRoles.Name = "btnRoles";
            this.btnRoles.Size = new System.Drawing.Size(281, 60);
            this.btnRoles.TabIndex = 1;
            this.btnRoles.Text = "Roles";
            this.btnRoles.UseVisualStyleBackColor = true;
            this.btnRoles.Click += new System.EventHandler(this.btnRoles_Click);
            // 
            // btnUsuarios
            // 
            this.btnUsuarios.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnUsuarios.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnUsuarios.Location = new System.Drawing.Point(0, 0);
            this.btnUsuarios.Name = "btnUsuarios";
            this.btnUsuarios.Size = new System.Drawing.Size(281, 60);
            this.btnUsuarios.TabIndex = 2;
            this.btnUsuarios.Text = "Usuarios";
            this.btnUsuarios.UseVisualStyleBackColor = true;
            this.btnUsuarios.Click += new System.EventHandler(this.btnUsuarios_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.CornflowerBlue;
            this.panel1.Controls.Add(this.btnVolver);
            this.panel1.Controls.Add(this.labelTitulo);
            this.panel1.Controls.Add(this.btnPerfil);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1114, 56);
            this.panel1.TabIndex = 1;
            // 
            // btnVolver
            // 
            this.btnVolver.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnVolver.Location = new System.Drawing.Point(957, 12);
            this.btnVolver.Name = "btnVolver";
            this.btnVolver.Size = new System.Drawing.Size(148, 38);
            this.btnVolver.TabIndex = 20;
            this.btnVolver.Text = "Volver";
            this.btnVolver.UseVisualStyleBackColor = true;
            // 
            // labelTitulo
            // 
            this.labelTitulo.AutoSize = true;
            this.labelTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTitulo.Location = new System.Drawing.Point(10, 12);
            this.labelTitulo.Name = "labelTitulo";
            this.labelTitulo.Size = new System.Drawing.Size(418, 38);
            this.labelTitulo.TabIndex = 0;
            this.labelTitulo.Text = "ITX - Usuarios, Roles y Permisos";
            // 
            // btnPerfil
            // 
            this.btnPerfil.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPerfil.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPerfil.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            this.btnPerfil.Location = new System.Drawing.Point(1804, 12);
            this.btnPerfil.Name = "btnPerfil";
            this.btnPerfil.Size = new System.Drawing.Size(190, 32);
            this.btnPerfil.TabIndex = 1;
            this.btnPerfil.Text = "Mi Perfil";
            this.btnPerfil.UseVisualStyleBackColor = true;
            // 
            // MainURP
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1114, 527);
            this.Controls.Add(this.kryptonGroupBox2);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainURP";
            this.Text = "Usuarios, Roles y Permisos";
            ((System.ComponentModel.ISupportInitialize)(this.kryptonGroupBox2.Panel)).EndInit();
            this.kryptonGroupBox2.Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.kryptonGroupBox2)).EndInit();
            this.kryptonGroupBox2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Krypton.Toolkit.KryptonGroupBox kryptonGroupBox2;
        private System.Windows.Forms.Button btnPermisos;
        private System.Windows.Forms.Button btnRoles;
        private System.Windows.Forms.Button btnUsuarios;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelTitulo;
        private System.Windows.Forms.Button btnPerfil;
        private System.Windows.Forms.Button btnVolver;
    }
}
