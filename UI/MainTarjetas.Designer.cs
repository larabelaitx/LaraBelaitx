namespace UI
{
    partial class MainTarjetas
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainTarjetas));
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.btnVolver = new System.Windows.Forms.Button();
            this.lblCliente = new System.Windows.Forms.Label();
            this.txtCliente = new Krypton.Toolkit.KryptonTextBox();
            this.lblCuenta = new System.Windows.Forms.Label();
            this.txtCuenta = new Krypton.Toolkit.KryptonTextBox();
            this.chkSoloVigentes = new Krypton.Toolkit.KryptonCheckBox();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.btnLimpiar = new System.Windows.Forms.Button();
            this.btnAlta = new System.Windows.Forms.Button();
            this.btnExportar = new System.Windows.Forms.Button();
            this.dgvTarjetas = new Krypton.Toolkit.KryptonDataGridView();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTarjetas)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.CornflowerBlue;
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.btnVolver);
            this.panel1.Location = new System.Drawing.Point(2, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1098, 58);
            this.panel1.TabIndex = 16;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(3, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(180, 38);
            this.label2.TabIndex = 7;
            this.label2.Text = "ITX - Tarjetas";
            // 
            // btnVolver
            // 
            this.btnVolver.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVolver.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnVolver.Location = new System.Drawing.Point(885, 13);
            this.btnVolver.Name = "btnVolver";
            this.btnVolver.Size = new System.Drawing.Size(193, 31);
            this.btnVolver.TabIndex = 0;
            this.btnVolver.Text = "Volver";
            this.btnVolver.UseVisualStyleBackColor = true;
            // 
            // lblCliente
            // 
            this.lblCliente.AutoSize = true;
            this.lblCliente.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCliente.Location = new System.Drawing.Point(7, 72);
            this.lblCliente.Name = "lblCliente";
            this.lblCliente.Size = new System.Drawing.Size(80, 28);
            this.lblCliente.TabIndex = 17;
            this.lblCliente.Text = "Cliente:";
            // 
            // txtCliente
            // 
            this.txtCliente.Location = new System.Drawing.Point(93, 74);
            this.txtCliente.Name = "txtCliente";
            this.txtCliente.Size = new System.Drawing.Size(236, 27);
            this.txtCliente.TabIndex = 18;
            // 
            // lblCuenta
            // 
            this.lblCuenta.AutoSize = true;
            this.lblCuenta.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCuenta.Location = new System.Drawing.Point(409, 73);
            this.lblCuenta.Name = "lblCuenta";
            this.lblCuenta.Size = new System.Drawing.Size(81, 28);
            this.lblCuenta.TabIndex = 19;
            this.lblCuenta.Text = "Cuenta:";
            // 
            // txtCuenta
            // 
            this.txtCuenta.Location = new System.Drawing.Point(495, 74);
            this.txtCuenta.Name = "txtCuenta";
            this.txtCuenta.Size = new System.Drawing.Size(236, 27);
            this.txtCuenta.TabIndex = 20;
            // 
            // chkSoloVigentes
            // 
            this.chkSoloVigentes.Location = new System.Drawing.Point(783, 77);
            this.chkSoloVigentes.Name = "chkSoloVigentes";
            this.chkSoloVigentes.Size = new System.Drawing.Size(119, 24);
            this.chkSoloVigentes.TabIndex = 21;
            this.chkSoloVigentes.Values.Text = "Solo vigentes";
            // 
            // btnBuscar
            // 
            this.btnBuscar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBuscar.Location = new System.Drawing.Point(12, 123);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Size = new System.Drawing.Size(124, 37);
            this.btnBuscar.TabIndex = 22;
            this.btnBuscar.Text = "Buscar";
            this.btnBuscar.UseVisualStyleBackColor = true;
            // 
            // btnLimpiar
            // 
            this.btnLimpiar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLimpiar.Location = new System.Drawing.Point(159, 123);
            this.btnLimpiar.Name = "btnLimpiar";
            this.btnLimpiar.Size = new System.Drawing.Size(124, 37);
            this.btnLimpiar.TabIndex = 23;
            this.btnLimpiar.Text = "Limpiar";
            this.btnLimpiar.UseVisualStyleBackColor = true;
            // 
            // btnAlta
            // 
            this.btnAlta.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAlta.Location = new System.Drawing.Point(313, 123);
            this.btnAlta.Name = "btnAlta";
            this.btnAlta.Size = new System.Drawing.Size(124, 37);
            this.btnAlta.TabIndex = 24;
            this.btnAlta.Text = "Alta";
            this.btnAlta.UseVisualStyleBackColor = true;
            // 
            // btnExportar
            // 
            this.btnExportar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExportar.Location = new System.Drawing.Point(474, 123);
            this.btnExportar.Name = "btnExportar";
            this.btnExportar.Size = new System.Drawing.Size(124, 37);
            this.btnExportar.TabIndex = 25;
            this.btnExportar.Text = "Exportar";
            this.btnExportar.UseVisualStyleBackColor = true;
            // 
            // dgvTarjetas
            // 
            this.dgvTarjetas.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvTarjetas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTarjetas.Location = new System.Drawing.Point(12, 183);
            this.dgvTarjetas.Name = "dgvTarjetas";
            this.dgvTarjetas.RowHeadersWidth = 51;
            this.dgvTarjetas.RowTemplate.Height = 24;
            this.dgvTarjetas.Size = new System.Drawing.Size(1068, 241);
            this.dgvTarjetas.TabIndex = 26;
            // 
            // MainTarjetas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1098, 436);
            this.Controls.Add(this.dgvTarjetas);
            this.Controls.Add(this.btnExportar);
            this.Controls.Add(this.btnAlta);
            this.Controls.Add(this.btnLimpiar);
            this.Controls.Add(this.btnBuscar);
            this.Controls.Add(this.chkSoloVigentes);
            this.Controls.Add(this.txtCuenta);
            this.Controls.Add(this.lblCuenta);
            this.Controls.Add(this.txtCliente);
            this.Controls.Add(this.lblCliente);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainTarjetas";
            this.Text = "MainTarjetas";
            this.Load += new System.EventHandler(this.MainTarjetas_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTarjetas)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnVolver;
        private System.Windows.Forms.Label lblCliente;
        private Krypton.Toolkit.KryptonTextBox txtCliente;
        private System.Windows.Forms.Label lblCuenta;
        private Krypton.Toolkit.KryptonTextBox txtCuenta;
        private Krypton.Toolkit.KryptonCheckBox chkSoloVigentes;
        private System.Windows.Forms.Button btnBuscar;
        private System.Windows.Forms.Button btnLimpiar;
        private System.Windows.Forms.Button btnAlta;
        private System.Windows.Forms.Button btnExportar;
        private Krypton.Toolkit.KryptonDataGridView dgvTarjetas;
    }
}