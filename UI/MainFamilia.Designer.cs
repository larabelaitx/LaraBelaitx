namespace UI
{
    partial class MainFamilia
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
            this.label2 = new System.Windows.Forms.Label();
            this.btnVolver = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnAgregar = new Krypton.Toolkit.KryptonButton();
            this.cboActivo = new Krypton.Toolkit.KryptonComboBox();
            this.txtNombre = new Krypton.Toolkit.KryptonTextBox();
            this.btnLimpiar = new Krypton.Toolkit.KryptonButton();
            this.label3 = new System.Windows.Forms.Label();
            this.btnAplicar = new Krypton.Toolkit.KryptonButton();
            this.label1 = new System.Windows.Forms.Label();
            this.dgvFamilias = new Krypton.Toolkit.KryptonDataGridView();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboActivo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFamilias)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.CornflowerBlue;
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.btnVolver);
            this.panel1.Location = new System.Drawing.Point(0, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1098, 58);
            this.panel1.TabIndex = 15;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(3, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(173, 38);
            this.label2.TabIndex = 7;
            this.label2.Text = "ITX - Familia";
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
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.groupBox1.Controls.Add(this.btnAgregar);
            this.groupBox1.Controls.Add(this.cboActivo);
            this.groupBox1.Controls.Add(this.txtNombre);
            this.groupBox1.Controls.Add(this.btnLimpiar);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.btnAplicar);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(0, 65);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1098, 103);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Filtros";
            // 
            // btnAgregar
            // 
            this.btnAgregar.Location = new System.Drawing.Point(923, 29);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Size = new System.Drawing.Size(155, 29);
            this.btnAgregar.TabIndex = 29;
            this.btnAgregar.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnAgregar.Values.Text = "Agregar";
            // 
            // cboActivo
            // 
            this.cboActivo.DropDownWidth = 243;
            this.cboActivo.Location = new System.Drawing.Point(469, 43);
            this.cboActivo.Name = "cboActivo";
            this.cboActivo.Size = new System.Drawing.Size(243, 26);
            this.cboActivo.StateCommon.ComboBox.Content.TextH = Krypton.Toolkit.PaletteRelativeAlign.Near;
            this.cboActivo.TabIndex = 28;
            // 
            // txtNombre
            // 
            this.txtNombre.Location = new System.Drawing.Point(104, 42);
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.Size = new System.Drawing.Size(243, 27);
            this.txtNombre.TabIndex = 27;
            // 
            // btnLimpiar
            // 
            this.btnLimpiar.Location = new System.Drawing.Point(923, 64);
            this.btnLimpiar.Name = "btnLimpiar";
            this.btnLimpiar.Size = new System.Drawing.Size(155, 29);
            this.btnLimpiar.TabIndex = 26;
            this.btnLimpiar.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnLimpiar.Values.Text = "Limpiar";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(390, 41);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 28);
            this.label3.TabIndex = 19;
            this.label3.Text = "Activo:";
            // 
            // btnAplicar
            // 
            this.btnAplicar.Location = new System.Drawing.Point(739, 29);
            this.btnAplicar.Name = "btnAplicar";
            this.btnAplicar.Size = new System.Drawing.Size(155, 29);
            this.btnAplicar.TabIndex = 18;
            this.btnAplicar.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnAplicar.Values.Text = "Aplicar";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(6, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 28);
            this.label1.TabIndex = 12;
            this.label1.Text = "Nombre:";
            // 
            // dgvFamilias
            // 
            this.dgvFamilias.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvFamilias.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFamilias.Location = new System.Drawing.Point(0, 174);
            this.dgvFamilias.Name = "dgvFamilias";
            this.dgvFamilias.RowHeadersWidth = 51;
            this.dgvFamilias.RowTemplate.Height = 24;
            this.dgvFamilias.Size = new System.Drawing.Size(1098, 286);
            this.dgvFamilias.TabIndex = 17;
            // 
            // MainFamilia
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1104, 444);
            this.Controls.Add(this.dgvFamilias);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel1);
            this.Name = "MainFamilia";
            this.Text = "MainFamilia";
            this.Load += new System.EventHandler(this.MainFamilia_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboActivo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFamilias)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnVolver;
        private System.Windows.Forms.GroupBox groupBox1;
        private Krypton.Toolkit.KryptonButton btnLimpiar;
        private System.Windows.Forms.Label label3;
        private Krypton.Toolkit.KryptonButton btnAplicar;
        private System.Windows.Forms.Label label1;
        private Krypton.Toolkit.KryptonComboBox cboActivo;
        private Krypton.Toolkit.KryptonTextBox txtNombre;
        private Krypton.Toolkit.KryptonDataGridView dgvFamilias;
        private Krypton.Toolkit.KryptonButton btnAgregar;
    }
}