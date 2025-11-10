namespace UI
{
    partial class AltaUsuario
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AltaUsuario));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnVolver = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.lblNombre = new System.Windows.Forms.Label();
            this.lblApellido = new System.Windows.Forms.Label();
            this.lblDocumento = new System.Windows.Forms.Label();
            this.lblEmail = new System.Windows.Forms.Label();
            this.lblUsuario = new System.Windows.Forms.Label();
            this.lblRol = new System.Windows.Forms.Label();
            this.lblEstado = new System.Windows.Forms.Label();
            this.cboEstado = new Krypton.Toolkit.KryptonComboBox();
            this.cboRol = new Krypton.Toolkit.KryptonComboBox();
            this.txtUsuario = new Krypton.Toolkit.KryptonTextBox();
            this.txtDocumento = new Krypton.Toolkit.KryptonTextBox();
            this.txtNombre = new Krypton.Toolkit.KryptonTextBox();
            this.txtEmail = new Krypton.Toolkit.KryptonTextBox();
            this.txtApellido = new Krypton.Toolkit.KryptonTextBox();
            this.btnGuardar = new Krypton.Toolkit.KryptonButton();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboEstado)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboRol)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.CornflowerBlue;
            this.panel1.Controls.Add(this.btnVolver);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Location = new System.Drawing.Point(2, -4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1085, 57);
            this.panel1.TabIndex = 15;
            // 
            // btnVolver
            // 
            this.btnVolver.Location = new System.Drawing.Point(934, 12);
            this.btnVolver.Name = "btnVolver";
            this.btnVolver.Size = new System.Drawing.Size(148, 38);
            this.btnVolver.TabIndex = 19;
            this.btnVolver.Text = "Volver";
            this.btnVolver.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(181, 38);
            this.label2.TabIndex = 7;
            this.label2.Text = "ITX - Usuario";
            // 
            // lblNombre
            // 
            this.lblNombre.AutoSize = true;
            this.lblNombre.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNombre.Location = new System.Drawing.Point(12, 56);
            this.lblNombre.Name = "lblNombre";
            this.lblNombre.Size = new System.Drawing.Size(92, 28);
            this.lblNombre.TabIndex = 20;
            this.lblNombre.Text = "Nombre:";
            // 
            // lblApellido
            // 
            this.lblApellido.AutoSize = true;
            this.lblApellido.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblApellido.Location = new System.Drawing.Point(471, 56);
            this.lblApellido.Name = "lblApellido";
            this.lblApellido.Size = new System.Drawing.Size(92, 28);
            this.lblApellido.TabIndex = 21;
            this.lblApellido.Text = "Apellido:";
            this.lblApellido.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblDocumento
            // 
            this.lblDocumento.AutoSize = true;
            this.lblDocumento.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDocumento.Location = new System.Drawing.Point(7, 122);
            this.lblDocumento.Name = "lblDocumento";
            this.lblDocumento.Size = new System.Drawing.Size(124, 28);
            this.lblDocumento.TabIndex = 22;
            this.lblDocumento.Text = "Documento:";
            this.lblDocumento.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblEmail
            // 
            this.lblEmail.AutoSize = true;
            this.lblEmail.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEmail.Location = new System.Drawing.Point(471, 122);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new System.Drawing.Size(65, 28);
            this.lblEmail.TabIndex = 23;
            this.lblEmail.Text = "Email:";
            this.lblEmail.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblUsuario
            // 
            this.lblUsuario.AutoSize = true;
            this.lblUsuario.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsuario.Location = new System.Drawing.Point(12, 184);
            this.lblUsuario.Name = "lblUsuario";
            this.lblUsuario.Size = new System.Drawing.Size(86, 28);
            this.lblUsuario.TabIndex = 24;
            this.lblUsuario.Text = "Usuario:";
            this.lblUsuario.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblRol
            // 
            this.lblRol.AutoSize = true;
            this.lblRol.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRol.Location = new System.Drawing.Point(471, 184);
            this.lblRol.Name = "lblRol";
            this.lblRol.Size = new System.Drawing.Size(45, 28);
            this.lblRol.TabIndex = 25;
            this.lblRol.Text = "Rol:";
            this.lblRol.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblEstado
            // 
            this.lblEstado.AutoSize = true;
            this.lblEstado.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEstado.Location = new System.Drawing.Point(12, 255);
            this.lblEstado.Name = "lblEstado";
            this.lblEstado.Size = new System.Drawing.Size(77, 28);
            this.lblEstado.TabIndex = 26;
            this.lblEstado.Text = "Estado:";
            this.lblEstado.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cboEstado
            // 
            this.cboEstado.DropDownWidth = 322;
            this.cboEstado.Location = new System.Drawing.Point(12, 286);
            this.cboEstado.Name = "cboEstado";
            this.cboEstado.Size = new System.Drawing.Size(322, 26);
            this.cboEstado.StateCommon.ComboBox.Content.TextH = Krypton.Toolkit.PaletteRelativeAlign.Near;
            this.cboEstado.TabIndex = 27;
            // 
            // cboRol
            // 
            this.cboRol.DropDownWidth = 266;
            this.cboRol.Location = new System.Drawing.Point(476, 216);
            this.cboRol.Name = "cboRol";
            this.cboRol.Size = new System.Drawing.Size(322, 26);
            this.cboRol.StateCommon.ComboBox.Content.TextH = Krypton.Toolkit.PaletteRelativeAlign.Near;
            this.cboRol.TabIndex = 28;
            // 
            // txtUsuario
            // 
            this.txtUsuario.Location = new System.Drawing.Point(12, 215);
            this.txtUsuario.Name = "txtUsuario";
            this.txtUsuario.Size = new System.Drawing.Size(322, 27);
            this.txtUsuario.TabIndex = 29;
            // 
            // txtDocumento
            // 
            this.txtDocumento.Location = new System.Drawing.Point(12, 150);
            this.txtDocumento.Name = "txtDocumento";
            this.txtDocumento.Size = new System.Drawing.Size(322, 27);
            this.txtDocumento.TabIndex = 30;
            // 
            // txtNombre
            // 
            this.txtNombre.Location = new System.Drawing.Point(12, 87);
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.Size = new System.Drawing.Size(322, 27);
            this.txtNombre.TabIndex = 31;
            // 
            // txtEmail
            // 
            this.txtEmail.Location = new System.Drawing.Point(476, 154);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(322, 27);
            this.txtEmail.TabIndex = 32;
            // 
            // txtApellido
            // 
            this.txtApellido.Location = new System.Drawing.Point(476, 87);
            this.txtApellido.Name = "txtApellido";
            this.txtApellido.Size = new System.Drawing.Size(322, 27);
            this.txtApellido.TabIndex = 33;
            // 
            // btnGuardar
            // 
            this.btnGuardar.Location = new System.Drawing.Point(426, 352);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(201, 56);
            this.btnGuardar.TabIndex = 34;
            this.btnGuardar.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnGuardar.Values.Text = "Guardar";
            // 
            // AltaUsuario
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1094, 422);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.txtApellido);
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.txtNombre);
            this.Controls.Add(this.txtDocumento);
            this.Controls.Add(this.txtUsuario);
            this.Controls.Add(this.cboRol);
            this.Controls.Add(this.cboEstado);
            this.Controls.Add(this.lblEstado);
            this.Controls.Add(this.lblRol);
            this.Controls.Add(this.lblUsuario);
            this.Controls.Add(this.lblEmail);
            this.Controls.Add(this.lblDocumento);
            this.Controls.Add(this.lblApellido);
            this.Controls.Add(this.lblNombre);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AltaUsuario";
            this.Text = "AltaUsuario";
            this.Load += new System.EventHandler(this.AltaUsuario_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboEstado)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboRol)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnVolver;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.Label lblApellido;
        private System.Windows.Forms.Label lblDocumento;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.Label lblUsuario;
        private System.Windows.Forms.Label lblRol;
        private System.Windows.Forms.Label lblEstado;
        private Krypton.Toolkit.KryptonComboBox cboEstado;
        private Krypton.Toolkit.KryptonComboBox cboRol;
        private Krypton.Toolkit.KryptonTextBox txtUsuario;
        private Krypton.Toolkit.KryptonTextBox txtDocumento;
        private Krypton.Toolkit.KryptonTextBox txtNombre;
        private Krypton.Toolkit.KryptonTextBox txtEmail;
        private Krypton.Toolkit.KryptonTextBox txtApellido;
        private Krypton.Toolkit.KryptonButton btnGuardar;
    }
}