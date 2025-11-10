namespace UI
{
    partial class AltaCuenta
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AltaCuenta));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnVolver = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.txtNumero = new Krypton.Toolkit.KryptonTextBox();
            this.txtCBU = new Krypton.Toolkit.KryptonTextBox();
            this.txtAlias = new Krypton.Toolkit.KryptonTextBox();
            this.cboTipo = new Krypton.Toolkit.KryptonComboBox();
            this.cboMoneda = new Krypton.Toolkit.KryptonComboBox();
            this.dtpApertura = new Krypton.Toolkit.KryptonDateTimePicker();
            this.txtObs = new Krypton.Toolkit.KryptonTextBox();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.txtCliente = new Krypton.Toolkit.KryptonTextBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboTipo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboMoneda)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.CornflowerBlue;
            this.panel1.Controls.Add(this.btnVolver);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Location = new System.Drawing.Point(0, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1081, 54);
            this.panel1.TabIndex = 10;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // btnVolver
            // 
            this.btnVolver.Location = new System.Drawing.Point(930, 6);
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
            this.label2.Size = new System.Drawing.Size(183, 38);
            this.label2.TabIndex = 7;
            this.label2.Text = "ITX - Clientes";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(14, 72);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 28);
            this.label1.TabIndex = 21;
            this.label1.Text = "Cliente:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(463, 72);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(181, 28);
            this.label3.TabIndex = 22;
            this.label3.Text = "Número de Cuenta:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(14, 115);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 28);
            this.label4.TabIndex = 23;
            this.label4.Text = "CBU:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(463, 115);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(57, 28);
            this.label5.TabIndex = 24;
            this.label5.Text = "Alias:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(14, 155);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(148, 28);
            this.label6.TabIndex = 25;
            this.label6.Text = "Tipo de Cuenta:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(463, 155);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(94, 28);
            this.label7.TabIndex = 26;
            this.label7.Text = "Moneda: ";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(14, 242);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(143, 28);
            this.label8.TabIndex = 27;
            this.label8.Text = "Observaciones:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(14, 199);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(175, 28);
            this.label9.TabIndex = 28;
            this.label9.Text = "Fecha de Apertura:";
            // 
            // txtNumero
            // 
            this.txtNumero.Location = new System.Drawing.Point(650, 73);
            this.txtNumero.Name = "txtNumero";
            this.txtNumero.Size = new System.Drawing.Size(217, 27);
            this.txtNumero.TabIndex = 29;
            // 
            // txtCBU
            // 
            this.txtCBU.Location = new System.Drawing.Point(195, 116);
            this.txtCBU.Name = "txtCBU";
            this.txtCBU.Size = new System.Drawing.Size(217, 27);
            this.txtCBU.TabIndex = 30;
            // 
            // txtAlias
            // 
            this.txtAlias.Location = new System.Drawing.Point(650, 116);
            this.txtAlias.Name = "txtAlias";
            this.txtAlias.Size = new System.Drawing.Size(217, 27);
            this.txtAlias.TabIndex = 31;
            // 
            // cboTipo
            // 
            this.cboTipo.DropDownWidth = 222;
            this.cboTipo.Location = new System.Drawing.Point(195, 155);
            this.cboTipo.Name = "cboTipo";
            this.cboTipo.Size = new System.Drawing.Size(222, 26);
            this.cboTipo.StateCommon.ComboBox.Content.TextH = Krypton.Toolkit.PaletteRelativeAlign.Near;
            this.cboTipo.TabIndex = 32;
            // 
            // cboMoneda
            // 
            this.cboMoneda.DropDownWidth = 222;
            this.cboMoneda.Location = new System.Drawing.Point(650, 157);
            this.cboMoneda.Name = "cboMoneda";
            this.cboMoneda.Size = new System.Drawing.Size(222, 26);
            this.cboMoneda.StateCommon.ComboBox.Content.TextH = Krypton.Toolkit.PaletteRelativeAlign.Near;
            this.cboMoneda.TabIndex = 33;
            // 
            // dtpApertura
            // 
            this.dtpApertura.Location = new System.Drawing.Point(195, 202);
            this.dtpApertura.Name = "dtpApertura";
            this.dtpApertura.Size = new System.Drawing.Size(222, 25);
            this.dtpApertura.TabIndex = 34;
            // 
            // txtObs
            // 
            this.txtObs.Location = new System.Drawing.Point(195, 243);
            this.txtObs.Name = "txtObs";
            this.txtObs.Size = new System.Drawing.Size(672, 27);
            this.txtObs.TabIndex = 35;
            // 
            // btnGuardar
            // 
            this.btnGuardar.Location = new System.Drawing.Point(409, 309);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(148, 38);
            this.btnGuardar.TabIndex = 36;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = true;
            // 
            // txtCliente
            // 
            this.txtCliente.Location = new System.Drawing.Point(195, 72);
            this.txtCliente.Name = "txtCliente";
            this.txtCliente.Size = new System.Drawing.Size(217, 27);
            this.txtCliente.TabIndex = 37;
            // 
            // AltaCuenta
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1092, 437);
            this.Controls.Add(this.txtCliente);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.txtObs);
            this.Controls.Add(this.dtpApertura);
            this.Controls.Add(this.cboMoneda);
            this.Controls.Add(this.cboTipo);
            this.Controls.Add(this.txtAlias);
            this.Controls.Add(this.txtCBU);
            this.Controls.Add(this.txtNumero);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AltaCuenta";
            this.Text = "AltaCuenta";
            this.Load += new System.EventHandler(this.AltaCuenta_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboTipo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboMoneda)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnVolver;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private Krypton.Toolkit.KryptonTextBox txtNumero;
        private Krypton.Toolkit.KryptonTextBox txtCBU;
        private Krypton.Toolkit.KryptonTextBox txtAlias;
        private Krypton.Toolkit.KryptonComboBox cboTipo;
        private Krypton.Toolkit.KryptonComboBox cboMoneda;
        private Krypton.Toolkit.KryptonDateTimePicker dtpApertura;
        private Krypton.Toolkit.KryptonTextBox txtObs;
        private System.Windows.Forms.Button btnGuardar;
        private Krypton.Toolkit.KryptonTextBox txtCliente;
    }
}