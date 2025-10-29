namespace UI
{
    partial class AltaPatente
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
            this.txtUsuario = new Krypton.Toolkit.KryptonTextBox();
            this.lblUsuario = new System.Windows.Forms.Label();
            this.btnBuscar = new Krypton.Toolkit.KryptonButton();
            this.lblUsuarioSeleccionado = new Krypton.Toolkit.KryptonLabel();
            this.grpAsignacion = new Krypton.Toolkit.KryptonGroupBox();
            this.lstDisponibles = new Krypton.Toolkit.KryptonListBox();
            this.lstAsignadas = new Krypton.Toolkit.KryptonListBox();
            this.lstHeredadas = new Krypton.Toolkit.KryptonListBox();
            this.lblDisponibles = new System.Windows.Forms.Label();
            this.lblAsignadas = new System.Windows.Forms.Label();
            this.lblHeredadas = new System.Windows.Forms.Label();
            this.btnRemoveAll = new Krypton.Toolkit.KryptonButton();
            this.btnRemove = new Krypton.Toolkit.KryptonButton();
            this.btnAddAll = new Krypton.Toolkit.KryptonButton();
            this.btnAdd = new Krypton.Toolkit.KryptonButton();
            this.btnGuardar = new Krypton.Toolkit.KryptonButton();
            this.btnCancelar = new Krypton.Toolkit.KryptonButton();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grpAsignacion)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grpAsignacion.Panel)).BeginInit();
            this.grpAsignacion.Panel.SuspendLayout();
            this.grpAsignacion.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.CornflowerBlue;
            this.panel1.Controls.Add(this.btnVolver);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Location = new System.Drawing.Point(-9, -1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1096, 60);
            this.panel1.TabIndex = 16;
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
            this.label2.Size = new System.Drawing.Size(185, 38);
            this.label2.TabIndex = 7;
            this.label2.Text = "ITX - Familias";
            // 
            // txtUsuario
            // 
            this.txtUsuario.Location = new System.Drawing.Point(104, 65);
            this.txtUsuario.Name = "txtUsuario";
            this.txtUsuario.Size = new System.Drawing.Size(322, 27);
            this.txtUsuario.TabIndex = 33;
            // 
            // lblUsuario
            // 
            this.lblUsuario.AutoSize = true;
            this.lblUsuario.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsuario.Location = new System.Drawing.Point(12, 62);
            this.lblUsuario.Name = "lblUsuario";
            this.lblUsuario.Size = new System.Drawing.Size(86, 28);
            this.lblUsuario.TabIndex = 32;
            this.lblUsuario.Text = "Usuario:";
            // 
            // btnBuscar
            // 
            this.btnBuscar.Location = new System.Drawing.Point(592, 65);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Size = new System.Drawing.Size(201, 25);
            this.btnBuscar.TabIndex = 35;
            this.btnBuscar.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnBuscar.Values.Text = "Buscar";
            // 
            // lblUsuarioSeleccionado
            // 
            this.lblUsuarioSeleccionado.Location = new System.Drawing.Point(17, 112);
            this.lblUsuarioSeleccionado.Name = "lblUsuarioSeleccionado";
            this.lblUsuarioSeleccionado.Size = new System.Drawing.Size(6, 2);
            this.lblUsuarioSeleccionado.TabIndex = 36;
            this.lblUsuarioSeleccionado.Values.Text = "";
            // 
            // grpAsignacion
            // 
            this.grpAsignacion.Location = new System.Drawing.Point(10, 141);
            // 
            // grpAsignacion.Panel
            // 
            this.grpAsignacion.Panel.Controls.Add(this.btnCancelar);
            this.grpAsignacion.Panel.Controls.Add(this.btnGuardar);
            this.grpAsignacion.Panel.Controls.Add(this.btnRemoveAll);
            this.grpAsignacion.Panel.Controls.Add(this.btnRemove);
            this.grpAsignacion.Panel.Controls.Add(this.btnAddAll);
            this.grpAsignacion.Panel.Controls.Add(this.btnAdd);
            this.grpAsignacion.Panel.Controls.Add(this.lblHeredadas);
            this.grpAsignacion.Panel.Controls.Add(this.lblAsignadas);
            this.grpAsignacion.Panel.Controls.Add(this.lblDisponibles);
            this.grpAsignacion.Panel.Controls.Add(this.lstHeredadas);
            this.grpAsignacion.Panel.Controls.Add(this.lstAsignadas);
            this.grpAsignacion.Panel.Controls.Add(this.lstDisponibles);
            this.grpAsignacion.Size = new System.Drawing.Size(1063, 420);
            this.grpAsignacion.TabIndex = 37;
            // 
            // lstDisponibles
            // 
            this.lstDisponibles.Location = new System.Drawing.Point(41, 36);
            this.lstDisponibles.Name = "lstDisponibles";
            this.lstDisponibles.Size = new System.Drawing.Size(253, 263);
            this.lstDisponibles.TabIndex = 0;
            // 
            // lstAsignadas
            // 
            this.lstAsignadas.Location = new System.Drawing.Point(406, 36);
            this.lstAsignadas.Name = "lstAsignadas";
            this.lstAsignadas.Size = new System.Drawing.Size(253, 263);
            this.lstAsignadas.TabIndex = 1;
            // 
            // lstHeredadas
            // 
            this.lstHeredadas.Location = new System.Drawing.Point(739, 36);
            this.lstHeredadas.Name = "lstHeredadas";
            this.lstHeredadas.Size = new System.Drawing.Size(253, 263);
            this.lstHeredadas.TabIndex = 2;
            // 
            // lblDisponibles
            // 
            this.lblDisponibles.AutoSize = true;
            this.lblDisponibles.BackColor = System.Drawing.Color.Transparent;
            this.lblDisponibles.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDisponibles.Location = new System.Drawing.Point(36, 5);
            this.lblDisponibles.Name = "lblDisponibles";
            this.lblDisponibles.Size = new System.Drawing.Size(118, 28);
            this.lblDisponibles.TabIndex = 38;
            this.lblDisponibles.Text = "Disponibles";
            // 
            // lblAsignadas
            // 
            this.lblAsignadas.AutoSize = true;
            this.lblAsignadas.BackColor = System.Drawing.Color.Transparent;
            this.lblAsignadas.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAsignadas.Location = new System.Drawing.Point(401, 5);
            this.lblAsignadas.Name = "lblAsignadas";
            this.lblAsignadas.Size = new System.Drawing.Size(104, 28);
            this.lblAsignadas.TabIndex = 39;
            this.lblAsignadas.Text = "Asignadas";
            // 
            // lblHeredadas
            // 
            this.lblHeredadas.AutoSize = true;
            this.lblHeredadas.BackColor = System.Drawing.Color.Transparent;
            this.lblHeredadas.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeredadas.Location = new System.Drawing.Point(734, 5);
            this.lblHeredadas.Name = "lblHeredadas";
            this.lblHeredadas.Size = new System.Drawing.Size(110, 28);
            this.lblHeredadas.TabIndex = 40;
            this.lblHeredadas.Text = "Herededas";
            // 
            // btnRemoveAll
            // 
            this.btnRemoveAll.Location = new System.Drawing.Point(331, 219);
            this.btnRemoveAll.Name = "btnRemoveAll";
            this.btnRemoveAll.PaletteMode = Krypton.Toolkit.PaletteMode.Office2013White;
            this.btnRemoveAll.Size = new System.Drawing.Size(33, 28);
            this.btnRemoveAll.StateCommon.Border.Rounding = 5F;
            this.btnRemoveAll.TabIndex = 44;
            this.btnRemoveAll.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnRemoveAll.Values.Text = "<<";
            // 
            // btnRemove
            // 
            this.btnRemove.Location = new System.Drawing.Point(331, 171);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.PaletteMode = Krypton.Toolkit.PaletteMode.Office2013White;
            this.btnRemove.Size = new System.Drawing.Size(33, 28);
            this.btnRemove.StateCommon.Border.Rounding = 5F;
            this.btnRemove.TabIndex = 43;
            this.btnRemove.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnRemove.Values.Text = "<";
            // 
            // btnAddAll
            // 
            this.btnAddAll.Location = new System.Drawing.Point(331, 126);
            this.btnAddAll.Name = "btnAddAll";
            this.btnAddAll.PaletteMode = Krypton.Toolkit.PaletteMode.Office2013White;
            this.btnAddAll.Size = new System.Drawing.Size(33, 28);
            this.btnAddAll.StateCommon.Border.Rounding = 5F;
            this.btnAddAll.TabIndex = 42;
            this.btnAddAll.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnAddAll.Values.Text = ">>";
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(331, 81);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.PaletteMode = Krypton.Toolkit.PaletteMode.Office2013White;
            this.btnAdd.Size = new System.Drawing.Size(33, 28);
            this.btnAdd.StateCommon.Border.Rounding = 5F;
            this.btnAdd.TabIndex = 41;
            this.btnAdd.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnAdd.Values.Text = ">";
            // 
            // btnGuardar
            // 
            this.btnGuardar.Location = new System.Drawing.Point(250, 319);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(201, 52);
            this.btnGuardar.TabIndex = 38;
            this.btnGuardar.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnGuardar.Values.Text = "Guardar";
            // 
            // btnCancelar
            // 
            this.btnCancelar.Location = new System.Drawing.Point(607, 319);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(201, 52);
            this.btnCancelar.TabIndex = 45;
            this.btnCancelar.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnCancelar.Values.Text = "Cancelar";
            // 
            // AltaPatente
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1086, 573);
            this.Controls.Add(this.grpAsignacion);
            this.Controls.Add(this.lblUsuarioSeleccionado);
            this.Controls.Add(this.btnBuscar);
            this.Controls.Add(this.txtUsuario);
            this.Controls.Add(this.lblUsuario);
            this.Controls.Add(this.panel1);
            this.Name = "AltaPatente";
            this.Text = "AltaPatente";
            this.Load += new System.EventHandler(this.AltaPatente_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grpAsignacion.Panel)).EndInit();
            this.grpAsignacion.Panel.ResumeLayout(false);
            this.grpAsignacion.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grpAsignacion)).EndInit();
            this.grpAsignacion.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnVolver;
        private System.Windows.Forms.Label label2;
        private Krypton.Toolkit.KryptonTextBox txtUsuario;
        private System.Windows.Forms.Label lblUsuario;
        private Krypton.Toolkit.KryptonButton btnBuscar;
        private Krypton.Toolkit.KryptonLabel lblUsuarioSeleccionado;
        private Krypton.Toolkit.KryptonGroupBox grpAsignacion;
        private Krypton.Toolkit.KryptonListBox lstDisponibles;
        private System.Windows.Forms.Label lblHeredadas;
        private System.Windows.Forms.Label lblAsignadas;
        private System.Windows.Forms.Label lblDisponibles;
        private Krypton.Toolkit.KryptonListBox lstHeredadas;
        private Krypton.Toolkit.KryptonListBox lstAsignadas;
        private Krypton.Toolkit.KryptonButton btnCancelar;
        private Krypton.Toolkit.KryptonButton btnGuardar;
        private Krypton.Toolkit.KryptonButton btnRemoveAll;
        private Krypton.Toolkit.KryptonButton btnRemove;
        private Krypton.Toolkit.KryptonButton btnAddAll;
        private Krypton.Toolkit.KryptonButton btnAdd;
    }
}