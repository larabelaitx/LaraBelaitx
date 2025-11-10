namespace UI
{
    partial class BackupRestore
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BackupRestore));
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.btnVolver = new System.Windows.Forms.Button();
            this.gruopBoxBackup = new System.Windows.Forms.GroupBox();
            this.btnSelectSave = new Krypton.Toolkit.KryptonButton();
            this.btnSave = new Krypton.Toolkit.KryptonButton();
            this.cbPart = new Krypton.Toolkit.KryptonComboBox();
            this.lblCant = new System.Windows.Forms.Label();
            this.txtBxSaveFolder = new Krypton.Toolkit.KryptonTextBox();
            this.lblSelectSave = new System.Windows.Forms.Label();
            this.groupBoxRestore = new System.Windows.Forms.GroupBox();
            this.btnSelectRestore = new Krypton.Toolkit.KryptonButton();
            this.btnRestore = new Krypton.Toolkit.KryptonButton();
            this.txtBxRestoreFolder = new Krypton.Toolkit.KryptonTextBox();
            this.lblSelectRestore = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.gruopBoxBackup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cbPart)).BeginInit();
            this.groupBoxRestore.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.CornflowerBlue;
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.btnVolver);
            this.panel1.Location = new System.Drawing.Point(2, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1091, 58);
            this.panel1.TabIndex = 12;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(6, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(300, 38);
            this.label2.TabIndex = 7;
            this.label2.Text = "ITX - Backup / Restore";
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
            // gruopBoxBackup
            // 
            this.gruopBoxBackup.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.gruopBoxBackup.Controls.Add(this.btnSelectSave);
            this.gruopBoxBackup.Controls.Add(this.btnSave);
            this.gruopBoxBackup.Controls.Add(this.cbPart);
            this.gruopBoxBackup.Controls.Add(this.lblCant);
            this.gruopBoxBackup.Controls.Add(this.txtBxSaveFolder);
            this.gruopBoxBackup.Controls.Add(this.lblSelectSave);
            this.gruopBoxBackup.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gruopBoxBackup.ForeColor = System.Drawing.Color.Black;
            this.gruopBoxBackup.Location = new System.Drawing.Point(15, 90);
            this.gruopBoxBackup.Name = "gruopBoxBackup";
            this.gruopBoxBackup.Size = new System.Drawing.Size(477, 293);
            this.gruopBoxBackup.TabIndex = 13;
            this.gruopBoxBackup.TabStop = false;
            this.gruopBoxBackup.Text = "Backup";
            // 
            // btnSelectSave
            // 
            this.btnSelectSave.Location = new System.Drawing.Point(165, 109);
            this.btnSelectSave.Name = "btnSelectSave";
            this.btnSelectSave.Size = new System.Drawing.Size(155, 26);
            this.btnSelectSave.TabIndex = 17;
            this.btnSelectSave.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnSelectSave.Values.Text = "Seleccionar";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(165, 246);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(155, 41);
            this.btnSave.TabIndex = 16;
            this.btnSave.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnSave.Values.Text = "Backup";
            // 
            // cbPart
            // 
            this.cbPart.DropDownWidth = 300;
            this.cbPart.Location = new System.Drawing.Point(93, 192);
            this.cbPart.Name = "cbPart";
            this.cbPart.Size = new System.Drawing.Size(300, 26);
            this.cbPart.StateCommon.ComboBox.Content.TextH = Krypton.Toolkit.PaletteRelativeAlign.Near;
            this.cbPart.TabIndex = 15;
            // 
            // lblCant
            // 
            this.lblCant.AutoSize = true;
            this.lblCant.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCant.Location = new System.Drawing.Point(148, 161);
            this.lblCant.Name = "lblCant";
            this.lblCant.Size = new System.Drawing.Size(189, 28);
            this.lblCant.TabIndex = 14;
            this.lblCant.Text = "Cantidad de partes:";
            // 
            // txtBxSaveFolder
            // 
            this.txtBxSaveFolder.Location = new System.Drawing.Point(93, 76);
            this.txtBxSaveFolder.Name = "txtBxSaveFolder";
            this.txtBxSaveFolder.Size = new System.Drawing.Size(300, 27);
            this.txtBxSaveFolder.TabIndex = 13;
            // 
            // lblSelectSave
            // 
            this.lblSelectSave.AutoSize = true;
            this.lblSelectSave.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSelectSave.Location = new System.Drawing.Point(33, 45);
            this.lblSelectSave.Name = "lblSelectSave";
            this.lblSelectSave.Size = new System.Drawing.Size(421, 28);
            this.lblSelectSave.TabIndex = 12;
            this.lblSelectSave.Text = "Seleccionar carpeta donde guardar el Backup";
            // 
            // groupBoxRestore
            // 
            this.groupBoxRestore.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.groupBoxRestore.Controls.Add(this.btnSelectRestore);
            this.groupBoxRestore.Controls.Add(this.btnRestore);
            this.groupBoxRestore.Controls.Add(this.txtBxRestoreFolder);
            this.groupBoxRestore.Controls.Add(this.lblSelectRestore);
            this.groupBoxRestore.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxRestore.Location = new System.Drawing.Point(593, 90);
            this.groupBoxRestore.Name = "groupBoxRestore";
            this.groupBoxRestore.Size = new System.Drawing.Size(477, 293);
            this.groupBoxRestore.TabIndex = 14;
            this.groupBoxRestore.TabStop = false;
            this.groupBoxRestore.Text = "Restore";
            // 
            // btnSelectRestore
            // 
            this.btnSelectRestore.Location = new System.Drawing.Point(179, 109);
            this.btnSelectRestore.Name = "btnSelectRestore";
            this.btnSelectRestore.Size = new System.Drawing.Size(155, 26);
            this.btnSelectRestore.TabIndex = 18;
            this.btnSelectRestore.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnSelectRestore.Values.Text = "Seleccionar";
            // 
            // btnRestore
            // 
            this.btnRestore.Location = new System.Drawing.Point(179, 216);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.Size = new System.Drawing.Size(155, 41);
            this.btnRestore.TabIndex = 17;
            this.btnRestore.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.btnRestore.Values.Text = "Restore";
            // 
            // txtBxRestoreFolder
            // 
            this.txtBxRestoreFolder.Location = new System.Drawing.Point(103, 76);
            this.txtBxRestoreFolder.Name = "txtBxRestoreFolder";
            this.txtBxRestoreFolder.Size = new System.Drawing.Size(300, 27);
            this.txtBxRestoreFolder.TabIndex = 18;
            // 
            // lblSelectRestore
            // 
            this.lblSelectRestore.AutoSize = true;
            this.lblSelectRestore.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSelectRestore.Location = new System.Drawing.Point(34, 45);
            this.lblSelectRestore.Name = "lblSelectRestore";
            this.lblSelectRestore.Size = new System.Drawing.Size(423, 28);
            this.lblSelectRestore.TabIndex = 17;
            this.lblSelectRestore.Text = "Seleccionar carpeta donde guardar el Restore";
            // 
            // BackupRestore
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1098, 429);
            this.Controls.Add(this.groupBoxRestore);
            this.Controls.Add(this.gruopBoxBackup);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BackupRestore";
            this.Text = "Backup / Restore";
            this.Load += new System.EventHandler(this.BackupRestore_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.gruopBoxBackup.ResumeLayout(false);
            this.gruopBoxBackup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cbPart)).EndInit();
            this.groupBoxRestore.ResumeLayout(false);
            this.groupBoxRestore.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnVolver;
        private System.Windows.Forms.GroupBox gruopBoxBackup;
        private System.Windows.Forms.GroupBox groupBoxRestore;
        private System.Windows.Forms.Label lblSelectSave;
        private Krypton.Toolkit.KryptonButton btnSave;
        private Krypton.Toolkit.KryptonComboBox cbPart;
        private System.Windows.Forms.Label lblCant;
        private Krypton.Toolkit.KryptonTextBox txtBxSaveFolder;
        private Krypton.Toolkit.KryptonButton btnRestore;
        private Krypton.Toolkit.KryptonTextBox txtBxRestoreFolder;
        private System.Windows.Forms.Label lblSelectRestore;
        private Krypton.Toolkit.KryptonButton btnSelectSave;
        private Krypton.Toolkit.KryptonButton btnSelectRestore;
    }
}