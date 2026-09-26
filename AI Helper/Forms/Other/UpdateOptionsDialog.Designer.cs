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
            this.UpdateOptionsFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.UpdateZipmodsCheckBox = new System.Windows.Forms.CheckBox();
            this.BleadingEdgeZipmodsCheckBox = new System.Windows.Forms.CheckBox();
            this.UpdatePluginsCheckBox = new System.Windows.Forms.CheckBox();
            this.CheckEnabledModsOnlyCheckBox = new System.Windows.Forms.CheckBox();
            this.ButtonsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.OpenOldVersionsDirButton = new System.Windows.Forms.Button();
            this.btnUpdateMods = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.UpdateOptionsPanel.SuspendLayout();
            this.UpdateOptionsTableLayoutPanel.SuspendLayout();
            this.UpdateOptionsFlowLayoutPanel.SuspendLayout();
            this.ButtonsTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // UpdateOptionsPanel
            // 
            this.UpdateOptionsPanel.Controls.Add(this.UpdateOptionsTableLayoutPanel);
            this.UpdateOptionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UpdateOptionsPanel.Location = new System.Drawing.Point(0, 0);
            this.UpdateOptionsPanel.Margin = new System.Windows.Forms.Padding(0);
            this.UpdateOptionsPanel.Name = "UpdateOptionsPanel";
            this.UpdateOptionsPanel.Padding = new System.Windows.Forms.Padding(12);
            this.UpdateOptionsPanel.Size = new System.Drawing.Size(400, 220);
            this.UpdateOptionsPanel.TabIndex = 0;
            // 
            // UpdateOptionsTableLayoutPanel
            // 
            this.UpdateOptionsTableLayoutPanel.ColumnCount = 1;
            this.UpdateOptionsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.UpdateOptionsTableLayoutPanel.Controls.Add(this.UpdateOptionsFlowLayoutPanel, 0, 0);
            this.UpdateOptionsTableLayoutPanel.Controls.Add(this.ButtonsTableLayoutPanel, 0, 1);
            this.UpdateOptionsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UpdateOptionsTableLayoutPanel.Location = new System.Drawing.Point(12, 12);
            this.UpdateOptionsTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.UpdateOptionsTableLayoutPanel.Name = "UpdateOptionsTableLayoutPanel";
            this.UpdateOptionsTableLayoutPanel.RowCount = 2;
            this.UpdateOptionsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.UpdateOptionsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.UpdateOptionsTableLayoutPanel.Size = new System.Drawing.Size(376, 196);
            this.UpdateOptionsTableLayoutPanel.TabIndex = 0;
            // 
            // UpdateOptionsFlowLayoutPanel
            // 
            // The checkboxes are added and removed here while the dialog runs, so the
            // panel scrolls and lays them out from the top down rather than in a row.
            this.UpdateOptionsFlowLayoutPanel.AutoScroll = true;
            this.UpdateOptionsFlowLayoutPanel.Controls.Add(this.UpdateZipmodsCheckBox);
            this.UpdateOptionsFlowLayoutPanel.Controls.Add(this.BleadingEdgeZipmodsCheckBox);
            this.UpdateOptionsFlowLayoutPanel.Controls.Add(this.UpdatePluginsCheckBox);
            this.UpdateOptionsFlowLayoutPanel.Controls.Add(this.CheckEnabledModsOnlyCheckBox);
            this.UpdateOptionsFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UpdateOptionsFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.UpdateOptionsFlowLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.UpdateOptionsFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.UpdateOptionsFlowLayoutPanel.Name = "UpdateOptionsFlowLayoutPanel";
            this.UpdateOptionsFlowLayoutPanel.Size = new System.Drawing.Size(376, 152);
            this.UpdateOptionsFlowLayoutPanel.TabIndex = 0;
            this.UpdateOptionsFlowLayoutPanel.WrapContents = false;
            // 
            // UpdateZipmodsCheckBox
            // 
            this.UpdateZipmodsCheckBox.AutoSize = true;
            this.UpdateZipmodsCheckBox.Checked = true;
            this.UpdateZipmodsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.UpdateZipmodsCheckBox.Location = new System.Drawing.Point(0, 0);
            this.UpdateZipmodsCheckBox.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.UpdateZipmodsCheckBox.Name = "UpdateZipmodsCheckBox";
            this.UpdateZipmodsCheckBox.Size = new System.Drawing.Size(116, 19);
            this.UpdateZipmodsCheckBox.TabIndex = 0;
            this.UpdateZipmodsCheckBox.Text = "Update zipmods";
            this.UpdateZipmodsCheckBox.CheckedChanged += new System.EventHandler(this.UpdateZipmodsCheckBox_CheckedChanged);
            // 
            // BleadingEdgeZipmodsCheckBox
            // 
            this.BleadingEdgeZipmodsCheckBox.AutoSize = true;
            this.BleadingEdgeZipmodsCheckBox.Location = new System.Drawing.Point(0, 27);
            this.BleadingEdgeZipmodsCheckBox.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.BleadingEdgeZipmodsCheckBox.Name = "BleadingEdgeZipmodsCheckBox";
            this.BleadingEdgeZipmodsCheckBox.Size = new System.Drawing.Size(163, 19);
            this.BleadingEdgeZipmodsCheckBox.TabIndex = 1;
            this.BleadingEdgeZipmodsCheckBox.Text = "Check test zipmods pack";
            this.BleadingEdgeZipmodsCheckBox.CheckedChanged += new System.EventHandler(this.BleadingEdgeZipmodsCheckBox_CheckedChanged);
            // 
            // UpdatePluginsCheckBox
            // 
            this.UpdatePluginsCheckBox.AutoSize = true;
            this.UpdatePluginsCheckBox.Checked = true;
            this.UpdatePluginsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.UpdatePluginsCheckBox.Location = new System.Drawing.Point(0, 54);
            this.UpdatePluginsCheckBox.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.UpdatePluginsCheckBox.Name = "UpdatePluginsCheckBox";
            this.UpdatePluginsCheckBox.Size = new System.Drawing.Size(110, 19);
            this.UpdatePluginsCheckBox.TabIndex = 2;
            this.UpdatePluginsCheckBox.Text = "Update plugins";
            this.UpdatePluginsCheckBox.CheckedChanged += new System.EventHandler(this.UpdatePluginsCheckBox_CheckedChanged);
            // 
            // CheckEnabledModsOnlyCheckBox
            // 
            this.CheckEnabledModsOnlyCheckBox.AutoSize = true;
            this.CheckEnabledModsOnlyCheckBox.Checked = true;
            this.CheckEnabledModsOnlyCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CheckEnabledModsOnlyCheckBox.Location = new System.Drawing.Point(0, 81);
            this.CheckEnabledModsOnlyCheckBox.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.CheckEnabledModsOnlyCheckBox.Name = "CheckEnabledModsOnlyCheckBox";
            this.CheckEnabledModsOnlyCheckBox.Size = new System.Drawing.Size(141, 19);
            this.CheckEnabledModsOnlyCheckBox.TabIndex = 3;
            this.CheckEnabledModsOnlyCheckBox.Text = "Only enabled plugins";
            this.CheckEnabledModsOnlyCheckBox.CheckedChanged += new System.EventHandler(this.CheckEnabledModsOnlyCheckBox_CheckedChanged);
            // 
            // ButtonsTableLayoutPanel
            // 
            // The first column takes the slack, so the three buttons keep their own size
            // and sit against the right edge however wide the dialog is.
            this.ButtonsTableLayoutPanel.ColumnCount = 4;
            this.ButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ButtonsTableLayoutPanel.Controls.Add(this.OpenOldVersionsDirButton, 1, 0);
            this.ButtonsTableLayoutPanel.Controls.Add(this.btnUpdateMods, 2, 0);
            this.ButtonsTableLayoutPanel.Controls.Add(this.btnCancel, 3, 0);
            this.ButtonsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ButtonsTableLayoutPanel.Location = new System.Drawing.Point(0, 152);
            this.ButtonsTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.ButtonsTableLayoutPanel.Name = "ButtonsTableLayoutPanel";
            this.ButtonsTableLayoutPanel.RowCount = 1;
            this.ButtonsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ButtonsTableLayoutPanel.Size = new System.Drawing.Size(376, 44);
            this.ButtonsTableLayoutPanel.TabIndex = 1;
            // 
            // OpenOldVersionsDirButton
            // 
            this.OpenOldVersionsDirButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.OpenOldVersionsDirButton.AutoSize = true;
            this.OpenOldVersionsDirButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.OpenOldVersionsDirButton.Location = new System.Drawing.Point(40, 6);
            this.OpenOldVersionsDirButton.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.OpenOldVersionsDirButton.MinimumSize = new System.Drawing.Size(96, 32);
            this.OpenOldVersionsDirButton.Name = "OpenOldVersionsDirButton";
            this.OpenOldVersionsDirButton.Padding = new System.Windows.Forms.Padding(14, 6, 14, 6);
            this.OpenOldVersionsDirButton.Size = new System.Drawing.Size(96, 32);
            this.OpenOldVersionsDirButton.TabIndex = 0;
            this.OpenOldVersionsDirButton.Text = "Old versions";
            this.OpenOldVersionsDirButton.Click += new System.EventHandler(this.OpenOldVersionsDirButton_Click);
            // 
            // btnUpdateMods
            // 
            this.btnUpdateMods.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnUpdateMods.AutoSize = true;
            this.btnUpdateMods.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnUpdateMods.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnUpdateMods.Location = new System.Drawing.Point(144, 6);
            this.btnUpdateMods.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.btnUpdateMods.MinimumSize = new System.Drawing.Size(96, 32);
            this.btnUpdateMods.Name = "btnUpdateMods";
            this.btnUpdateMods.Padding = new System.Windows.Forms.Padding(14, 6, 14, 6);
            this.btnUpdateMods.Size = new System.Drawing.Size(96, 32);
            this.btnUpdateMods.TabIndex = 1;
            this.btnUpdateMods.Text = "Start";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnCancel.AutoSize = true;
            this.btnCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(248, 6);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.btnCancel.MinimumSize = new System.Drawing.Size(96, 32);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Padding = new System.Windows.Forms.Padding(14, 6, 14, 6);
            this.btnCancel.Size = new System.Drawing.Size(96, 32);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            // 
            // UpdateOptionsDialogForm
            // 
            this.AcceptButton = this.btnUpdateMods;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(400, 220);
            this.Controls.Add(this.UpdateOptionsPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MinimumSize = new System.Drawing.Size(360, 200);
            this.Name = "UpdateOptionsDialogForm";
            this.Text = "Update options";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UpdateOptionsDialogForm_FormClosing);
            this.Load += new System.EventHandler(this.UpdateOptionsDialogForm_Load);
            this.UpdateOptionsPanel.ResumeLayout(false);
            this.UpdateOptionsTableLayoutPanel.ResumeLayout(false);
            this.UpdateOptionsFlowLayoutPanel.ResumeLayout(false);
            this.UpdateOptionsFlowLayoutPanel.PerformLayout();
            this.ButtonsTableLayoutPanel.ResumeLayout(false);
            this.ButtonsTableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel UpdateOptionsPanel;
        private System.Windows.Forms.TableLayoutPanel UpdateOptionsTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel ButtonsTableLayoutPanel;
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
