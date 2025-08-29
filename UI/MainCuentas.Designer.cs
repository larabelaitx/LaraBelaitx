namespace UI
{
    partial class MainCuentas
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnVolver = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtCliente = new System.Windows.Forms.TextBox();
            this.cboEstado = new Krypton.Toolkit.KryptonComboBox();
            this.cboTipo = new Krypton.Toolkit.KryptonComboBox();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.btnLimpiar = new System.Windows.Forms.Button();
            this.btnAgregar = new System.Windows.Forms.Button();
            this.btnDescargar = new System.Windows.Forms.Button();
            this.dgvCuentas = new Krypton.Toolkit.KryptonDataGridView();
            this.NumeroCuenta = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TipoCuenta = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Moneda = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Estado = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ClienteNombre = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Ver = new System.Windows.Forms.DataGridViewButtonColumn();
            this.Editar = new System.Windows.Forms.DataGridViewButtonColumn();
            this.Reactivar = new System.Windows.Forms.DataGridViewButtonColumn();
            this.Baja = new System.Windows.Forms.DataGridViewButtonColumn();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboEstado)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboTipo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCuentas)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.CornflowerBlue;
            this.panel1.Controls.Add(this.btnVolver);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Location = new System.Drawing.Point(2, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1081, 54);
            this.panel1.TabIndex = 9;
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
            this.label2.Size = new System.Drawing.Size(185, 38);
            this.label2.TabIndex = 7;
            this.label2.Text = "ITX - Cuentas";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(16, 71);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 28);
            this.label1.TabIndex = 10;
            this.label1.Text = "Cliente:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(365, 71);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(156, 28);
            this.label3.TabIndex = 11;
            this.label3.Text = "Tipo de Cuenta:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(16, 149);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 28);
            this.label4.TabIndex = 12;
            this.label4.Text = "Estado:";
            // 
            // txtCliente
            // 
            this.txtCliente.Location = new System.Drawing.Point(21, 102);
            this.txtCliente.Name = "txtCliente";
            this.txtCliente.Size = new System.Drawing.Size(267, 22);
            this.txtCliente.TabIndex = 13;
            // 
            // cboEstado
            // 
            this.cboEstado.DropDownWidth = 267;
            this.cboEstado.Location = new System.Drawing.Point(21, 180);
            this.cboEstado.Name = "cboEstado";
            this.cboEstado.Size = new System.Drawing.Size(267, 26);
            this.cboEstado.StateCommon.ComboBox.Content.TextH = Krypton.Toolkit.PaletteRelativeAlign.Near;
            this.cboEstado.TabIndex = 14;
            // 
            // cboTipo
            // 
            this.cboTipo.DropDownWidth = 228;
            this.cboTipo.Location = new System.Drawing.Point(370, 102);
            this.cboTipo.Name = "cboTipo";
            this.cboTipo.Size = new System.Drawing.Size(267, 26);
            this.cboTipo.StateCommon.ComboBox.Content.TextH = Krypton.Toolkit.PaletteRelativeAlign.Near;
            this.cboTipo.TabIndex = 15;
          //  this.cboTipo.SelectedIndexChanged += new System.EventHandler(this.kryptonComboBox2_SelectedIndexChanged);
            // 
            // btnBuscar
            // 
            this.btnBuscar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBuscar.Location = new System.Drawing.Point(370, 158);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Size = new System.Drawing.Size(124, 48);
            this.btnBuscar.TabIndex = 17;
            this.btnBuscar.Text = "Buscar";
            this.btnBuscar.UseVisualStyleBackColor = true;
            // 
            // btnLimpiar
            // 
            this.btnLimpiar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLimpiar.Location = new System.Drawing.Point(513, 158);
            this.btnLimpiar.Name = "btnLimpiar";
            this.btnLimpiar.Size = new System.Drawing.Size(124, 48);
            this.btnLimpiar.TabIndex = 18;
            this.btnLimpiar.Text = "Limpiar";
            this.btnLimpiar.UseVisualStyleBackColor = true;
            // 
            // btnAgregar
            // 
            this.btnAgregar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAgregar.Location = new System.Drawing.Point(739, 85);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Size = new System.Drawing.Size(148, 48);
            this.btnAgregar.TabIndex = 19;
            this.btnAgregar.Text = "Agregar";
            this.btnAgregar.UseVisualStyleBackColor = true;
            // 
            // btnDescargar
            // 
            this.btnDescargar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDescargar.Location = new System.Drawing.Point(903, 85);
            this.btnDescargar.Name = "btnDescargar";
            this.btnDescargar.Size = new System.Drawing.Size(148, 48);
            this.btnDescargar.TabIndex = 20;
            this.btnDescargar.Text = "Descargar";
            this.btnDescargar.UseVisualStyleBackColor = true;
            // 
            // dgvCuentas
            // 
            this.dgvCuentas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvCuentas.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvCuentas.ColumnHeadersHeight = 36;
            this.dgvCuentas.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.NumeroCuenta,
            this.TipoCuenta,
            this.Moneda,
            this.Estado,
            this.ClienteNombre,
            this.Ver,
            this.Editar,
            this.Reactivar,
            this.Baja});
            this.dgvCuentas.Location = new System.Drawing.Point(21, 250);
            this.dgvCuentas.Name = "dgvCuentas";
            this.dgvCuentas.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.dgvCuentas.RowHeadersVisible = false;
            this.dgvCuentas.RowHeadersWidth = 51;
            this.dgvCuentas.RowTemplate.Height = 24;
            this.dgvCuentas.Size = new System.Drawing.Size(1051, 276);
            this.dgvCuentas.TabIndex = 21;
            //this.dgvCuentas.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.kryptonDataGridView1_CellContentClick);
            // 
            // NumeroCuenta
            // 
            this.NumeroCuenta.FillWeight = 80F;
            this.NumeroCuenta.HeaderText = "Número de Cuenta";
            this.NumeroCuenta.MinimumWidth = 3;
            this.NumeroCuenta.Name = "NumeroCuenta";
            // 
            // TipoCuenta
            // 
            this.TipoCuenta.HeaderText = "Tipo Cuenta";
            this.TipoCuenta.MinimumWidth = 6;
            this.TipoCuenta.Name = "TipoCuenta";
            // 
            // Moneda
            // 
            this.Moneda.HeaderText = "Moneda";
            this.Moneda.MinimumWidth = 6;
            this.Moneda.Name = "Moneda";
            // 
            // Estado
            // 
            this.Estado.HeaderText = "Estado";
            this.Estado.MinimumWidth = 6;
            this.Estado.Name = "Estado";
            // 
            // ClienteNombre
            // 
            this.ClienteNombre.HeaderText = "Cliente";
            this.ClienteNombre.MinimumWidth = 6;
            this.ClienteNombre.Name = "ClienteNombre";
            // 
            // Ver
            // 
            this.Ver.HeaderText = "Ver";
            this.Ver.MinimumWidth = 6;
            this.Ver.Name = "Ver";
            // 
            // Editar
            // 
            this.Editar.HeaderText = "Editar";
            this.Editar.MinimumWidth = 6;
            this.Editar.Name = "Editar";
            // 
            // Reactivar
            // 
            this.Reactivar.HeaderText = "Reactivar";
            this.Reactivar.MinimumWidth = 6;
            this.Reactivar.Name = "Reactivar";
            // 
            // Baja
            // 
            this.Baja.HeaderText = "Baja";
            this.Baja.MinimumWidth = 6;
            this.Baja.Name = "Baja";
            // 
            // MainCuentas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1084, 538);
            this.Controls.Add(this.dgvCuentas);
            this.Controls.Add(this.btnDescargar);
            this.Controls.Add(this.btnAgregar);
            this.Controls.Add(this.btnLimpiar);
            this.Controls.Add(this.btnBuscar);
            this.Controls.Add(this.cboTipo);
            this.Controls.Add(this.cboEstado);
            this.Controls.Add(this.txtCliente);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel1);
            this.Name = "MainCuentas";
            this.Text = "MainCuentas";
        //    this.Load += new System.EventHandler(this.MainCuentas_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboEstado)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboTipo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCuentas)).EndInit();
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
        private System.Windows.Forms.TextBox txtCliente;
        private Krypton.Toolkit.KryptonComboBox cboEstado;
        private Krypton.Toolkit.KryptonComboBox cboTipo;
        private System.Windows.Forms.Button btnBuscar;
        private System.Windows.Forms.Button btnLimpiar;
        private System.Windows.Forms.Button btnAgregar;
        private System.Windows.Forms.Button btnDescargar;
        private Krypton.Toolkit.KryptonDataGridView dgvCuentas;
        private System.Windows.Forms.DataGridViewTextBoxColumn NumeroCuenta;
        private System.Windows.Forms.DataGridViewTextBoxColumn TipoCuenta;
        private System.Windows.Forms.DataGridViewTextBoxColumn Moneda;
        private System.Windows.Forms.DataGridViewTextBoxColumn Estado;
        private System.Windows.Forms.DataGridViewTextBoxColumn ClienteNombre;
        private System.Windows.Forms.DataGridViewButtonColumn Ver;
        private System.Windows.Forms.DataGridViewButtonColumn Editar;
        private System.Windows.Forms.DataGridViewButtonColumn Reactivar;
        private System.Windows.Forms.DataGridViewButtonColumn Baja;
    }
}