namespace AIHelper.Forms.Other
{
    partial class CleanOptionsDialogForm
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
            this.MainPanel = new System.Windows.Forms.Panel();
            this.OptionsLayoutTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.OptionsFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.cbxMoveIntoNewMod = new AIHelper.Manage.ui.ThemedCheckBox();
            this.cbxIgnoreSymlinks = new AIHelper.Manage.ui.ThemedCheckBox();
            this.cbxIgnoreShortcuts = new AIHelper.Manage.ui.ThemedCheckBox();
            this.ButtonsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.MainPanel.SuspendLayout();
            this.OptionsLayoutTableLayoutPanel.SuspendLayout();
            this.OptionsFlowLayoutPanel.SuspendLayout();
            this.ButtonsTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            this.MainPanel.Controls.Add(this.OptionsLayoutTableLayoutPanel);
            this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainPanel.Location = new System.Drawing.Point(0, 0);
            this.MainPanel.Margin = new System.Windows.Forms.Padding(0);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Padding = new System.Windows.Forms.Padding(12);
            this.MainPanel.Size = new System.Drawing.Size(400, 220);
            this.MainPanel.TabIndex = 0;
            // 
            // OptionsLayoutTableLayoutPanel
            // 
            this.OptionsLayoutTableLayoutPanel.ColumnCount = 1;
            this.OptionsLayoutTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.OptionsLayoutTableLayoutPanel.Controls.Add(this.OptionsFlowLayoutPanel, 0, 0);
            this.OptionsLayoutTableLayoutPanel.Controls.Add(this.ButtonsTableLayoutPanel, 0, 1);
            this.OptionsLayoutTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OptionsLayoutTableLayoutPanel.Location = new System.Drawing.Point(12, 12);
            this.OptionsLayoutTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.OptionsLayoutTableLayoutPanel.Name = "OptionsLayoutTableLayoutPanel";
            this.OptionsLayoutTableLayoutPanel.RowCount = 2;
            this.OptionsLayoutTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.OptionsLayoutTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.OptionsLayoutTableLayoutPanel.Size = new System.Drawing.Size(376, 196);
            this.OptionsLayoutTableLayoutPanel.TabIndex = 0;
            // 
            // OptionsFlowLayoutPanel
            // 
            this.OptionsFlowLayoutPanel.AutoScroll = true;
            this.OptionsFlowLayoutPanel.Controls.Add(this.cbxMoveIntoNewMod);
            this.OptionsFlowLayoutPanel.Controls.Add(this.cbxIgnoreSymlinks);
            this.OptionsFlowLayoutPanel.Controls.Add(this.cbxIgnoreShortcuts);
            this.OptionsFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OptionsFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.OptionsFlowLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.OptionsFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.OptionsFlowLayoutPanel.Name = "OptionsFlowLayoutPanel";
            this.OptionsFlowLayoutPanel.Size = new System.Drawing.Size(376, 152);
            this.OptionsFlowLayoutPanel.TabIndex = 0;
            this.OptionsFlowLayoutPanel.WrapContents = false;
            // 
            // cbxMoveIntoNewMod
            // 
            this.cbxMoveIntoNewMod.AutoSize = true;
            this.cbxMoveIntoNewMod.Location = new System.Drawing.Point(0, 0);
            this.cbxMoveIntoNewMod.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.cbxMoveIntoNewMod.Name = "cbxMoveIntoNewMod";
            this.cbxMoveIntoNewMod.Size = new System.Drawing.Size(150, 19);
            this.cbxMoveIntoNewMod.TabIndex = 0;
            this.cbxMoveIntoNewMod.Text = "Move to the new mod";
            // 
            // cbxIgnoreSymlinks
            // 
            this.cbxIgnoreSymlinks.AutoSize = true;
            this.cbxIgnoreSymlinks.Checked = true;
            this.cbxIgnoreSymlinks.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxIgnoreSymlinks.Location = new System.Drawing.Point(0, 27);
            this.cbxIgnoreSymlinks.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.cbxIgnoreSymlinks.Name = "cbxIgnoreSymlinks";
            this.cbxIgnoreSymlinks.Size = new System.Drawing.Size(104, 19);
            this.cbxIgnoreSymlinks.TabIndex = 1;
            this.cbxIgnoreSymlinks.Text = "Ignore symlinks";
            // 
            // cbxIgnoreShortcuts
            // 
            this.cbxIgnoreShortcuts.AutoSize = true;
            this.cbxIgnoreShortcuts.Location = new System.Drawing.Point(0, 54);
            this.cbxIgnoreShortcuts.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.cbxIgnoreShortcuts.Name = "cbxIgnoreShortcuts";
            this.cbxIgnoreShortcuts.Size = new System.Drawing.Size(108, 19);
            this.cbxIgnoreShortcuts.TabIndex = 2;
            this.cbxIgnoreShortcuts.Text = "Ignore shortcuts";
            // 
            // ButtonsTableLayoutPanel
            // 
            this.ButtonsTableLayoutPanel.ColumnCount = 3;
            this.ButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ButtonsTableLayoutPanel.Controls.Add(this.btnStart, 1, 0);
            this.ButtonsTableLayoutPanel.Controls.Add(this.btnCancel, 2, 0);
            this.ButtonsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ButtonsTableLayoutPanel.Location = new System.Drawing.Point(0, 152);
            this.ButtonsTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.ButtonsTableLayoutPanel.Name = "ButtonsTableLayoutPanel";
            this.ButtonsTableLayoutPanel.RowCount = 1;
            this.ButtonsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ButtonsTableLayoutPanel.Size = new System.Drawing.Size(376, 44);
            this.ButtonsTableLayoutPanel.TabIndex = 1;
            // 
            // btnStart
            // 
            this.btnStart.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnStart.AutoSize = true;
            this.btnStart.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnStart.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnStart.Location = new System.Drawing.Point(160, 6);
            this.btnStart.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.btnStart.MinimumSize = new System.Drawing.Size(96, 32);
            this.btnStart.Name = "btnStart";
            this.btnStart.Padding = new System.Windows.Forms.Padding(14, 6, 14, 6);
            this.btnStart.Size = new System.Drawing.Size(96, 32);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "OK";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnCancel.AutoSize = true;
            this.btnCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(264, 6);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.btnCancel.MinimumSize = new System.Drawing.Size(96, 32);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Padding = new System.Windows.Forms.Padding(14, 6, 14, 6);
            this.btnCancel.Size = new System.Drawing.Size(96, 32);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            // 
            // CleanOptionsDialogForm
            // 
            this.AcceptButton = this.btnStart;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(400, 220);
            this.Controls.Add(this.MainPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "CleanOptionsDialogForm";
            this.Text = "Cleaning options";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CleanOptionsDialogForm_FormClosing);
            this.Load += new System.EventHandler(this.CleanOptionsDialogForm_Load);
            this.MainPanel.ResumeLayout(false);
            this.OptionsLayoutTableLayoutPanel.ResumeLayout(false);
            this.OptionsFlowLayoutPanel.ResumeLayout(false);
            this.OptionsFlowLayoutPanel.PerformLayout();
            this.ButtonsTableLayoutPanel.ResumeLayout(false);
            this.ButtonsTableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel MainPanel;
        private System.Windows.Forms.TableLayoutPanel OptionsLayoutTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel ButtonsTableLayoutPanel;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.FlowLayoutPanel OptionsFlowLayoutPanel;
        internal AIHelper.Manage.ui.ThemedCheckBox cbxMoveIntoNewMod;
        internal AIHelper.Manage.ui.ThemedCheckBox cbxIgnoreSymlinks;
        internal AIHelper.Manage.ui.ThemedCheckBox cbxIgnoreShortcuts;
    }
}
