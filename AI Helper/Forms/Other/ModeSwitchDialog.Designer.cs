namespace AIHelper.Forms.Other
{
    partial class UpdateOptionsDialogForm
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
            this.UpdateOptionsPanel = new System.Windows.Forms.Panel();
            this.UpdateOptionsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.UpdateOptionsFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.UpdateZipmodsCheckBox = new System.Windows.Forms.CheckBox();
            this.BleadingEdgeZipmodsCheckBox = new System.Windows.Forms.CheckBox();
            this.UpdatePluginsCheckBox = new System.Windows.Forms.CheckBox();
            this.CheckEnabledModsOnlyCheckBox = new System.Windows.Forms.CheckBox();
            this.OpenOldVersionsDirButton = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnUpdateMods = new System.Windows.Forms.Button();
            this.UpdateOptionsPanel.SuspendLayout();
            this.UpdateOptionsTableLayoutPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.UpdateOptionsFlowLayoutPanel.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // UpdateOptionsPanel
            // 
            this.UpdateOptionsPanel.Controls.Add(this.UpdateOptionsTableLayoutPanel);
            this.UpdateOptionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UpdateOptionsPanel.Location = new System.Drawing.Point(0, 0);
            this.UpdateOptionsPanel.Name = "UpdateOptionsPanel";
            this.UpdateOptionsPanel.Size = new System.Drawing.Size(354, 191);
            this.UpdateOptionsPanel.TabIndex = 0;
            // 
            // UpdateOptionsTableLayoutPanel
            // 
            this.UpdateOptionsTableLayoutPanel.ColumnCount = 1;
            this.UpdateOptionsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 48.0226F));
            this.UpdateOptionsTableLayoutPanel.Controls.Add(this.panel1, 0, 0);
            this.UpdateOptionsTableLayoutPanel.Controls.Add(this.panel2, 0, 1);
            this.UpdateOptionsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UpdateOptionsTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.UpdateOptionsTableLayoutPanel.Name = "UpdateOptionsTableLayoutPanel";
            this.UpdateOptionsTableLayoutPanel.RowCount = 2;
            this.UpdateOptionsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 77.48691F));
            this.UpdateOptionsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 22.51309F));
            this.UpdateOptionsTableLayoutPanel.Size = new System.Drawing.Size(354, 191);
            this.UpdateOptionsTableLayoutPanel.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.UpdateOptionsFlowLayoutPanel);
            this.panel1.Controls.Add(this.OpenOldVersionsDirButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(348, 141);
            this.panel1.TabIndex = 0;
            // 
            // UpdateOptionsFlowLayoutPanel
            // 
            this.UpdateOptionsFlowLayoutPanel.Controls.Add(this.UpdateZipmodsCheckBox);
            this.UpdateOptionsFlowLayoutPanel.Controls.Add(this.BleadingEdgeZipmodsCheckBox);
            this.UpdateOptionsFlowLayoutPanel.Controls.Add(this.UpdatePluginsCheckBox);
            this.UpdateOptionsFlowLayoutPanel.Controls.Add(this.CheckEnabledModsOnlyCheckBox);
            this.UpdateOptionsFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.UpdateOptionsFlowLayoutPanel.Location = new System.Drawing.Point(9, 3);
            this.UpdateOptionsFlowLayoutPanel.Name = "UpdateOptionsFlowLayoutPanel";
            this.UpdateOptionsFlowLayoutPanel.Size = new System.Drawing.Size(330, 106);
            this.UpdateOptionsFlowLayoutPanel.TabIndex = 5;
            // 
            // UpdateZipmodsCheckBox
            // 
            this.UpdateZipmodsCheckBox.AutoSize = true;
            this.UpdateZipmodsCheckBox.Checked = true;
            this.UpdateZipmodsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.UpdateZipmodsCheckBox.Location = new System.Drawing.Point(3, 3);
            this.UpdateZipmodsCheckBox.Name = "UpdateZipmodsCheckBox";
            this.UpdateZipmodsCheckBox.Size = new System.Drawing.Size(102, 17);
            this.UpdateZipmodsCheckBox.TabIndex = 0;
            this.UpdateZipmodsCheckBox.Text = "Update zipmods";
            this.UpdateZipmodsCheckBox.UseVisualStyleBackColor = true;
            this.UpdateZipmodsCheckBox.CheckedChanged += new System.EventHandler(this.UpdateZipmodsCheckBox_CheckedChanged);
            // 
            // BleadingEdgeZipmodsCheckBox
            // 
            this.BleadingEdgeZipmodsCheckBox.AutoSize = true;
            this.BleadingEdgeZipmodsCheckBox.Location = new System.Drawing.Point(3, 26);
            this.BleadingEdgeZipmodsCheckBox.Name = "BleadingEdgeZipmodsCheckBox";
            this.BleadingEdgeZipmodsCheckBox.Size = new System.Drawing.Size(143, 17);
            this.BleadingEdgeZipmodsCheckBox.TabIndex = 1;
            this.BleadingEdgeZipmodsCheckBox.Text = "Check test zipmods pack";
            this.BleadingEdgeZipmodsCheckBox.UseVisualStyleBackColor = true;
            this.BleadingEdgeZipmodsCheckBox.CheckedChanged += new System.EventHandler(this.BleadingEdgeZipmodsCheckBox_CheckedChanged);
            // 
            // UpdatePluginsCheckBox
            // 
            this.UpdatePluginsCheckBox.AutoSize = true;
            this.UpdatePluginsCheckBox.Checked = true;
            this.UpdatePluginsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.UpdatePluginsCheckBox.Location = new System.Drawing.Point(3, 49);
            this.UpdatePluginsCheckBox.Name = "UpdatePluginsCheckBox";
            this.UpdatePluginsCheckBox.Size = new System.Drawing.Size(97, 17);
            this.UpdatePluginsCheckBox.TabIndex = 2;
            this.UpdatePluginsCheckBox.Text = "Update plugins";
            this.UpdatePluginsCheckBox.UseVisualStyleBackColor = true;
            this.UpdatePluginsCheckBox.CheckedChanged += new System.EventHandler(this.UpdatePluginsCheckBox_CheckedChanged);
            // 
            // CheckEnabledModsOnlyCheckBox
            // 
            this.CheckEnabledModsOnlyCheckBox.AutoSize = true;
            this.CheckEnabledModsOnlyCheckBox.Checked = true;
            this.CheckEnabledModsOnlyCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CheckEnabledModsOnlyCheckBox.Location = new System.Drawing.Point(3, 72);
            this.CheckEnabledModsOnlyCheckBox.Name = "CheckEnabledModsOnlyCheckBox";
            this.CheckEnabledModsOnlyCheckBox.Size = new System.Drawing.Size(125, 17);
            this.CheckEnabledModsOnlyCheckBox.TabIndex = 3;
            this.CheckEnabledModsOnlyCheckBox.Text = "Only enabled plugins";
            this.CheckEnabledModsOnlyCheckBox.UseVisualStyleBackColor = true;
            this.CheckEnabledModsOnlyCheckBox.CheckedChanged += new System.EventHandler(this.CheckEnabledModsOnlyCheckBox_CheckedChanged);
            // 
            // OpenOldVersionsDirButton
            // 
            this.OpenOldVersionsDirButton.Location = new System.Drawing.Point(177, 115);
            this.OpenOldVersionsDirButton.Name = "OpenOldVersionsDirButton";
            this.OpenOldVersionsDirButton.Size = new System.Drawing.Size(168, 23);
            this.OpenOldVersionsDirButton.TabIndex = 4;
            this.OpenOldVersionsDirButton.Text = "Old versions";
            this.OpenOldVersionsDirButton.UseVisualStyleBackColor = true;
            this.OpenOldVersionsDirButton.Click += new System.EventHandler(this.OpenOldVersionsDirButton_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tableLayoutPanel1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 150);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(348, 38);
            this.panel2.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.btnCancel, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnUpdateMods, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(348, 38);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(177, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(168, 32);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnUpdateMods
            // 
            this.btnUpdateMods.Location = new System.Drawing.Point(3, 3);
            this.btnUpdateMods.Name = "btnUpdateMods";
            this.btnUpdateMods.Size = new System.Drawing.Size(168, 32);
            this.btnUpdateMods.TabIndex = 0;
            this.btnUpdateMods.Text = "Start";
            this.btnUpdateMods.UseVisualStyleBackColor = true;
            this.btnUpdateMods.Click += new System.EventHandler(this.btnUpdateMods_Click);
            // 
            // UpdateOptionsDialogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.ClientSize = new System.Drawing.Size(354, 191);
            this.Controls.Add(this.UpdateOptionsPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "UpdateOptionsDialogForm";
            this.Text = "Update options";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UpdateOptionsDialogForm_FormClosing);
            this.Load += new System.EventHandler(this.UpdateOptionsDialogForm_Load);
            this.UpdateOptionsPanel.ResumeLayout(false);
            this.UpdateOptionsTableLayoutPanel.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.UpdateOptionsFlowLayoutPanel.ResumeLayout(false);
            this.UpdateOptionsFlowLayoutPanel.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel UpdateOptionsPanel;
        private System.Windows.Forms.TableLayoutPanel UpdateOptionsTableLayoutPanel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btnCancel;
        public System.Windows.Forms.CheckBox UpdateZipmodsCheckBox;
        public System.Windows.Forms.CheckBox BleadingEdgeZipmodsCheckBox;
        public System.Windows.Forms.CheckBox UpdatePluginsCheckBox;
        public System.Windows.Forms.CheckBox CheckEnabledModsOnlyCheckBox;
        public System.Windows.Forms.Button btnUpdateMods;
        private System.Windows.Forms.Button OpenOldVersionsDirButton;
        private System.Windows.Forms.FlowLayoutPanel UpdateOptionsFlowLayoutPanel;
    }
}