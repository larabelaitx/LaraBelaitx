namespace UI
{
    partial class AltaTarjeta
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AltaTarjeta));
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.btnVolver = new System.Windows.Forms.Button();
            this.txtIdTarjeta = new Krypton.Toolkit.KryptonTextBox();
            this.lblIdTarjeta = new System.Windows.Forms.Label();
            this.txtNumero = new Krypton.Toolkit.KryptonTextBox();
            this.lblNumero = new System.Windows.Forms.Label();
            this.txtCVV = new Krypton.Toolkit.KryptonTextBox();
            this.lblCVV = new System.Windows.Forms.Label();
            this.txtClienteId = new Krypton.Toolkit.KryptonTextBox();
            this.lblClienteId = new System.Windows.Forms.Label();
            this.txtCuentaId = new Krypton.Toolkit.KryptonTextBox();
            this.lblCuentaId = new System.Windows.Forms.Label();
            this.btnBuscarCliente = new Krypton.Toolkit.KryptonButton();
            this.btnBuscarCuenta = new Krypton.Toolkit.KryptonButton();
            this.lblMarca = new System.Windows.Forms.Label();
            this.txtMarca = new Krypton.Toolkit.KryptonTextBox();
            this.lblTitular = new System.Windows.Forms.Label();
            this.txtTitular = new Krypton.Toolkit.KryptonTextBox();
            this.lblEmision = new System.Windows.Forms.Label();
            this.dtpEmision = new Krypton.Toolkit.KryptonDateTimePicker();
            this.lblVencimiento = new System.Windows.Forms.Label();
            this.dtpVencimiento = new Krypton.Toolkit.KryptonDateTimePicker();
            this.btnGuardar = new Krypton.Toolkit.KryptonButton();
            this.btnCancelar = new Krypton.Toolkit.KryptonButton();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.CornflowerBlue;
            this.panel1.Controls.Add(this.lblTitulo);
            this.panel1.Controls.Add(this.btnVolver);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1098, 58);
            this.panel1.TabIndex = 16;
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitulo.Location = new System.Drawing.Point(3, 8);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(180, 38);
            this.lblTitulo.TabIndex = 7;
            this.lblTitulo.Text = "ITX - Tarjetas";
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
            // txtIdTarjeta
            // 
            this.txtIdTarjeta.Location = new System.Drawing.Point(144, 81);
            this.txtIdTarjeta.Name = "txtIdTarjeta";
            this.txtIdTarjeta.ReadOnly = true;
            this.txtIdTarjeta.Size = new System.Drawing.Size(236, 27);
            this.txtIdTarjeta.TabIndex = 20;
            this.txtIdTarjeta.TabStop = false;
            // 
            // lblIdTarjeta
            // 
            this.lblIdTarjeta.AutoSize = true;
            this.lblIdTarjeta.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIdTarjeta.Location = new System.Drawing.Point(12, 80);
            this.lblIdTarjeta.Name = "lblIdTarjeta";
            this.lblIdTarjeta.Size = new System.Drawing.Size(35, 28);
            this.lblIdTarjeta.TabIndex = 19;
            this.lblIdTarjeta.Text = "Id:";
            this.lblIdTarjeta.Click += new System.EventHandler(this.lblIdTarjeta_Click);
            // 
            // txtNumero
            // 
            this.txtNumero.Location = new System.Drawing.Point(709, 135);
            this.txtNumero.Name = "txtNumero";
            this.txtNumero.ReadOnly = true;
            this.txtNumero.Size = new System.Drawing.Size(236, 27);
            this.txtNumero.TabIndex = 22;
            this.txtNumero.TabStop = false;
            // 
            // lblNumero
            // 
            this.lblNumero.AutoSize = true;
            this.lblNumero.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNumero.Location = new System.Drawing.Point(603, 133);
            this.lblNumero.Name = "lblNumero";
            this.lblNumero.Size = new System.Drawing.Size(100, 28);
            this.lblNumero.TabIndex = 21;
            this.lblNumero.Text = "N° Tarjeta";
            // 
            // txtCVV
            // 
            this.txtCVV.Location = new System.Drawing.Point(709, 81);
            this.txtCVV.Name = "txtCVV";
            this.txtCVV.PasswordChar = '●';
            this.txtCVV.ReadOnly = true;
            this.txtCVV.Size = new System.Drawing.Size(236, 27);
            this.txtCVV.TabIndex = 24;
            this.txtCVV.TabStop = false;
            this.txtCVV.UseSystemPasswordChar = true;
            // 
            // lblCVV
            // 
            this.lblCVV.AutoSize = true;
            this.lblCVV.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCVV.Location = new System.Drawing.Point(603, 80);
            this.lblCVV.Name = "lblCVV";
            this.lblCVV.Size = new System.Drawing.Size(50, 28);
            this.lblCVV.TabIndex = 23;
            this.lblCVV.Text = "CVV";
            // 
            // txtClienteId
            // 
            this.txtClienteId.Location = new System.Drawing.Point(144, 133);
            this.txtClienteId.MaxLength = 9;
            this.txtClienteId.Name = "txtClienteId";
            this.txtClienteId.Size = new System.Drawing.Size(236, 27);
            this.txtClienteId.TabIndex = 26;
            // 
            // lblClienteId
            // 
            this.lblClienteId.AutoSize = true;
            this.lblClienteId.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblClienteId.Location = new System.Drawing.Point(12, 134);
            this.lblClienteId.Name = "lblClienteId";
            this.lblClienteId.Size = new System.Drawing.Size(99, 28);
            this.lblClienteId.TabIndex = 25;
            this.lblClienteId.Text = "Cliente Id";
            // 
            // txtCuentaId
            // 
            this.txtCuentaId.Location = new System.Drawing.Point(144, 195);
            this.txtCuentaId.MaxLength = 9;
            this.txtCuentaId.Name = "txtCuentaId";
            this.txtCuentaId.Size = new System.Drawing.Size(236, 27);
            this.txtCuentaId.TabIndex = 28;
            // 
            // lblCuentaId
            // 
            this.lblCuentaId.AutoSize = true;
            this.lblCuentaId.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCuentaId.Location = new System.Drawing.Point(12, 194);
            this.lblCuentaId.Name = "lblCuentaId";
            this.lblCuentaId.Size = new System.Drawing.Size(100, 28);
            this.lblCuentaId.TabIndex = 27;
            this.lblCuentaId.Text = "Cuenta Id";
            // 
            // btnBuscarCliente
            // 
            this.btnBuscarCliente.Location = new System.Drawing.Point(392, 132);
            this.btnBuscarCliente.Name = "btnBuscarCliente";
            this.btnBuscarCliente.Size = new System.Drawing.Size(118, 29);
            this.btnBuscarCliente.TabIndex = 29;
            this.btnBuscarCliente.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnBuscarCliente.Values.Text = "Buscar Cliente";
            // 
            // btnBuscarCuenta
            // 
            this.btnBuscarCuenta.Location = new System.Drawing.Point(392, 192);
            this.btnBuscarCuenta.Name = "btnBuscarCuenta";
            this.btnBuscarCuenta.Size = new System.Drawing.Size(118, 29);
            this.btnBuscarCuenta.TabIndex = 30;
            this.btnBuscarCuenta.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnBuscarCuenta.Values.Text = "Buscar Cuenta";
            // 
            // lblMarca
            // 
            this.lblMarca.AutoSize = true;
            this.lblMarca.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMarca.Location = new System.Drawing.Point(603, 192);
            this.lblMarca.Name = "lblMarca";
            this.lblMarca.Size = new System.Drawing.Size(66, 28);
            this.lblMarca.TabIndex = 31;
            this.lblMarca.Text = "Marca";
            // 
            // txtMarca
            // 
            this.txtMarca.Location = new System.Drawing.Point(709, 195);
            this.txtMarca.Name = "txtMarca";
            this.txtMarca.ReadOnly = true;
            this.txtMarca.Size = new System.Drawing.Size(236, 27);
            this.txtMarca.TabIndex = 32;
            this.txtMarca.TabStop = false;
            // 
            // lblTitular
            // 
            this.lblTitular.AutoSize = true;
            this.lblTitular.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitular.Location = new System.Drawing.Point(12, 259);
            this.lblTitular.Name = "lblTitular";
            this.lblTitular.Size = new System.Drawing.Size(69, 28);
            this.lblTitular.TabIndex = 33;
            this.lblTitular.Text = "Titular";
            // 
            // txtTitular
            // 
            this.txtTitular.Location = new System.Drawing.Point(144, 260);
            this.txtTitular.MaxLength = 9;
            this.txtTitular.Name = "txtTitular";
            this.txtTitular.Size = new System.Drawing.Size(236, 27);
            this.txtTitular.TabIndex = 34;
            // 
            // lblEmision
            // 
            this.lblEmision.AutoSize = true;
            this.lblEmision.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEmision.Location = new System.Drawing.Point(603, 256);
            this.lblEmision.Name = "lblEmision";
            this.lblEmision.Size = new System.Drawing.Size(83, 28);
            this.lblEmision.TabIndex = 35;
            this.lblEmision.Text = "Emision";
            // 
            // dtpEmision
            // 
            this.dtpEmision.Location = new System.Drawing.Point(709, 259);
            this.dtpEmision.Name = "dtpEmision";
            this.dtpEmision.Size = new System.Drawing.Size(236, 25);
            this.dtpEmision.TabIndex = 36;
            // 
            // lblVencimiento
            // 
            this.lblVencimiento.AutoSize = true;
            this.lblVencimiento.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVencimiento.Location = new System.Drawing.Point(12, 319);
            this.lblVencimiento.Name = "lblVencimiento";
            this.lblVencimiento.Size = new System.Drawing.Size(126, 28);
            this.lblVencimiento.TabIndex = 37;
            this.lblVencimiento.Text = "Vencimiento";
            // 
            // dtpVencimiento
            // 
            this.dtpVencimiento.Location = new System.Drawing.Point(144, 322);
            this.dtpVencimiento.Name = "dtpVencimiento";
            this.dtpVencimiento.Size = new System.Drawing.Size(236, 25);
            this.dtpVencimiento.TabIndex = 38;
            // 
            // btnGuardar
            // 
            this.btnGuardar.Location = new System.Drawing.Point(353, 384);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(157, 52);
            this.btnGuardar.TabIndex = 39;
            this.btnGuardar.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnGuardar.Values.Text = "Guardar";
            // 
            // btnCancelar
            // 
            this.btnCancelar.Location = new System.Drawing.Point(546, 384);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(157, 52);
            this.btnCancelar.TabIndex = 40;
            this.btnCancelar.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnCancelar.Values.Text = "Cancelar";
            // 
            // AltaTarjeta
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1098, 448);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.dtpVencimiento);
            this.Controls.Add(this.lblVencimiento);
            this.Controls.Add(this.dtpEmision);
            this.Controls.Add(this.lblEmision);
            this.Controls.Add(this.txtTitular);
            this.Controls.Add(this.lblTitular);
            this.Controls.Add(this.txtMarca);
            this.Controls.Add(this.lblMarca);
            this.Controls.Add(this.btnBuscarCuenta);
            this.Controls.Add(this.btnBuscarCliente);
            this.Controls.Add(this.txtCuentaId);
            this.Controls.Add(this.lblCuentaId);
            this.Controls.Add(this.txtClienteId);
            this.Controls.Add(this.lblClienteId);
            this.Controls.Add(this.txtCVV);
            this.Controls.Add(this.lblCVV);
            this.Controls.Add(this.txtNumero);
            this.Controls.Add(this.lblNumero);
            this.Controls.Add(this.txtIdTarjeta);
            this.Controls.Add(this.lblIdTarjeta);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AltaTarjeta";
            this.Text = "AltaTarjeta";
            this.Load += new System.EventHandler(this.AltaTarjeta_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Button btnVolver;
        private Krypton.Toolkit.KryptonTextBox txtIdTarjeta;
        private System.Windows.Forms.Label lblIdTarjeta;
        private Krypton.Toolkit.KryptonTextBox txtNumero;
        private System.Windows.Forms.Label lblNumero;
        private Krypton.Toolkit.KryptonTextBox txtCVV;
        private System.Windows.Forms.Label lblCVV;
        private Krypton.Toolkit.KryptonTextBox txtClienteId;
        private System.Windows.Forms.Label lblClienteId;
        private Krypton.Toolkit.KryptonTextBox txtCuentaId;
        private System.Windows.Forms.Label lblCuentaId;
        private Krypton.Toolkit.KryptonButton btnBuscarCliente;
        private Krypton.Toolkit.KryptonButton btnBuscarCuenta;
        private System.Windows.Forms.Label lblMarca;
        private Krypton.Toolkit.KryptonTextBox txtMarca;
        private System.Windows.Forms.Label lblTitular;
        private Krypton.Toolkit.KryptonTextBox txtTitular;
        private System.Windows.Forms.Label lblEmision;
        private Krypton.Toolkit.KryptonDateTimePicker dtpEmision;
        private System.Windows.Forms.Label lblVencimiento;
        private Krypton.Toolkit.KryptonDateTimePicker dtpVencimiento;
        private Krypton.Toolkit.KryptonButton btnGuardar;
        private Krypton.Toolkit.KryptonButton btnCancelar;
    }
}