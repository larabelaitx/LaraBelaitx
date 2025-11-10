namespace UI
{
    partial class AltaFamilia
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AltaFamilia));
            this.txtNombre = new Krypton.Toolkit.KryptonTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnVolver = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.chkActiva = new Krypton.Toolkit.KryptonCheckBox();
            this.txtDescripcion = new Krypton.Toolkit.KryptonRichTextBox();
            this.lstAsignadas = new Krypton.Toolkit.KryptonListBox();
            this.lstDisponibles = new Krypton.Toolkit.KryptonListBox();
            this.btnAdd = new Krypton.Toolkit.KryptonButton();
            this.btnAddAll = new Krypton.Toolkit.KryptonButton();
            this.btnRemove = new Krypton.Toolkit.KryptonButton();
            this.btnRemoveAll = new Krypton.Toolkit.KryptonButton();
            this.btnGuardar = new Krypton.Toolkit.KryptonButton();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.btnCancelar = new Krypton.Toolkit.KryptonButton();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtNombre
            // 
            this.txtNombre.Location = new System.Drawing.Point(17, 98);
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.Size = new System.Drawing.Size(352, 31);
            this.txtNombre.StateCommon.Border.Rounding = 5F;
            this.txtNombre.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 67);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 28);
            this.label1.TabIndex = 13;
            this.label1.Text = "Nombre:";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.CornflowerBlue;
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btnVolver);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Location = new System.Drawing.Point(-1, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1085, 54);
            this.panel1.TabIndex = 14;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.DarkTurquoise;
            this.panel2.Location = new System.Drawing.Point(411, 48);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(10, 394);
            this.panel2.TabIndex = 20;
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
            this.label2.Text = "ITX - Familias";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(13, 132);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(122, 28);
            this.label3.TabIndex = 15;
            this.label3.Text = "Descripción:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(13, 287);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 28);
            this.label4.TabIndex = 17;
            this.label4.Text = "Activa:";
            // 
            // chkActiva
            // 
            this.chkActiva.LabelStyle = Krypton.Toolkit.LabelStyle.BoldControl;
            this.chkActiva.Location = new System.Drawing.Point(90, 295);
            this.chkActiva.Name = "chkActiva";
            this.chkActiva.PaletteMode = Krypton.Toolkit.PaletteMode.Office2013White;
            this.chkActiva.Size = new System.Drawing.Size(22, 16);
            this.chkActiva.TabIndex = 18;
            this.chkActiva.Values.Text = "";
            // 
            // txtDescripcion
            // 
            this.txtDescripcion.Location = new System.Drawing.Point(18, 163);
            this.txtDescripcion.Name = "txtDescripcion";
            this.txtDescripcion.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtDescripcion.Size = new System.Drawing.Size(351, 121);
            this.txtDescripcion.StateCommon.Border.Rounding = 5F;
            this.txtDescripcion.TabIndex = 19;
            this.txtDescripcion.Text = "";
            // 
            // lstAsignadas
            // 
            this.lstAsignadas.Location = new System.Drawing.Point(776, 98);
            this.lstAsignadas.Name = "lstAsignadas";
            this.lstAsignadas.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstAsignadas.Size = new System.Drawing.Size(300, 220);
            this.lstAsignadas.StateCommon.Border.Rounding = 5F;
            this.lstAsignadas.TabIndex = 20;
            // 
            // lstDisponibles
            // 
            this.lstDisponibles.Location = new System.Drawing.Point(431, 98);
            this.lstDisponibles.Name = "lstDisponibles";
            this.lstDisponibles.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstDisponibles.Size = new System.Drawing.Size(300, 220);
            this.lstDisponibles.StateCommon.Border.Rounding = 5F;
            this.lstDisponibles.TabIndex = 21;
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(737, 114);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.PaletteMode = Krypton.Toolkit.PaletteMode.Office2013White;
            this.btnAdd.Size = new System.Drawing.Size(33, 28);
            this.btnAdd.StateCommon.Border.Rounding = 5F;
            this.btnAdd.TabIndex = 22;
            this.btnAdd.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnAdd.Values.Text = ">";
            // 
            // btnAddAll
            // 
            this.btnAddAll.Location = new System.Drawing.Point(737, 159);
            this.btnAddAll.Name = "btnAddAll";
            this.btnAddAll.PaletteMode = Krypton.Toolkit.PaletteMode.Office2013White;
            this.btnAddAll.Size = new System.Drawing.Size(33, 28);
            this.btnAddAll.StateCommon.Border.Rounding = 5F;
            this.btnAddAll.TabIndex = 23;
            this.btnAddAll.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnAddAll.Values.Text = ">>";
            // 
            // btnRemove
            // 
            this.btnRemove.Location = new System.Drawing.Point(737, 204);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.PaletteMode = Krypton.Toolkit.PaletteMode.Office2013White;
            this.btnRemove.Size = new System.Drawing.Size(33, 28);
            this.btnRemove.StateCommon.Border.Rounding = 5F;
            this.btnRemove.TabIndex = 24;
            this.btnRemove.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnRemove.Values.Text = "<";
            // 
            // btnRemoveAll
            // 
            this.btnRemoveAll.Location = new System.Drawing.Point(737, 252);
            this.btnRemoveAll.Name = "btnRemoveAll";
            this.btnRemoveAll.PaletteMode = Krypton.Toolkit.PaletteMode.Office2013White;
            this.btnRemoveAll.Size = new System.Drawing.Size(33, 28);
            this.btnRemoveAll.StateCommon.Border.Rounding = 5F;
            this.btnRemoveAll.TabIndex = 25;
            this.btnRemoveAll.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnRemoveAll.Values.Text = "<<";
            // 
            // btnGuardar
            // 
            this.btnGuardar.Location = new System.Drawing.Point(289, 363);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.PaletteMode = Krypton.Toolkit.PaletteMode.Office2013White;
            this.btnGuardar.Size = new System.Drawing.Size(155, 48);
            this.btnGuardar.StateCommon.Back.Color1 = System.Drawing.SystemColors.Window;
            this.btnGuardar.StateCommon.Back.Color2 = System.Drawing.Color.White;
            this.btnGuardar.StateCommon.Back.ColorAlign = Krypton.Toolkit.PaletteRectangleAlign.Form;
            this.btnGuardar.StateCommon.Back.ColorStyle = Krypton.Toolkit.PaletteColorStyle.Dashed;
            this.btnGuardar.StateCommon.Border.Rounding = 5F;
            this.btnGuardar.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.Black;
            this.btnGuardar.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGuardar.TabIndex = 26;
            this.btnGuardar.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnGuardar.Values.Text = "Guardar";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(426, 70);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(194, 25);
            this.label5.TabIndex = 27;
            this.label5.Text = "Patentes Disponibles:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(771, 67);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(183, 25);
            this.label6.TabIndex = 28;
            this.label6.Text = "Patentes Asignadas:";
            // 
            // btnCancelar
            // 
            this.btnCancelar.Location = new System.Drawing.Point(515, 363);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.PaletteMode = Krypton.Toolkit.PaletteMode.Office2013White;
            this.btnCancelar.Size = new System.Drawing.Size(155, 48);
            this.btnCancelar.StateCommon.Back.Color1 = System.Drawing.SystemColors.Window;
            this.btnCancelar.StateCommon.Back.Color2 = System.Drawing.Color.White;
            this.btnCancelar.StateCommon.Back.ColorAlign = Krypton.Toolkit.PaletteRectangleAlign.Form;
            this.btnCancelar.StateCommon.Back.ColorStyle = Krypton.Toolkit.PaletteColorStyle.Dashed;
            this.btnCancelar.StateCommon.Border.Rounding = 5F;
            this.btnCancelar.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.Black;
            this.btnCancelar.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.TabIndex = 29;
            this.btnCancelar.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnCancelar.Values.Text = "Cancelar";
            // 
            // AltaFamilia
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(1094, 436);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnRemoveAll);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnAddAll);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.lstDisponibles);
            this.Controls.Add(this.lstAsignadas);
            this.Controls.Add(this.txtDescripcion);
            this.Controls.Add(this.chkActiva);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtNombre);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AltaFamilia";
            this.Text = "AltaFamilia";
            this.Load += new System.EventHandler(this.AltaFamilia_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Krypton.Toolkit.KryptonTextBox txtNombre;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnVolver;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private Krypton.Toolkit.KryptonCheckBox chkActiva;
        private Krypton.Toolkit.KryptonRichTextBox txtDescripcion;
        private System.Windows.Forms.Panel panel2;
        private Krypton.Toolkit.KryptonListBox lstAsignadas;
        private Krypton.Toolkit.KryptonListBox lstDisponibles;
        private Krypton.Toolkit.KryptonButton btnAdd;
        private Krypton.Toolkit.KryptonButton btnAddAll;
        private Krypton.Toolkit.KryptonButton btnRemove;
        private Krypton.Toolkit.KryptonButton btnRemoveAll;
        private Krypton.Toolkit.KryptonButton btnGuardar;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private Krypton.Toolkit.KryptonButton btnCancelar;
    }
}