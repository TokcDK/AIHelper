namespace AIHelper.Forms.Other
{
    partial class SwitchModeDialogForm
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
            this.DialogLayoutTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.MessageLayoutTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.MessageTextTextBox = new System.Windows.Forms.TextBox();
            this.MakeBuckupCheckBox = new System.Windows.Forms.CheckBox();
            this.DoNotSwitchCheckBox = new System.Windows.Forms.CheckBox();
            this.ButtonsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.btnSwitchMode = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.MainPanel.SuspendLayout();
            this.DialogLayoutTableLayoutPanel.SuspendLayout();
            this.MessageLayoutTableLayoutPanel.SuspendLayout();
            this.ButtonsTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            this.MainPanel.Controls.Add(this.DialogLayoutTableLayoutPanel);
            this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainPanel.Location = new System.Drawing.Point(0, 0);
            this.MainPanel.Margin = new System.Windows.Forms.Padding(0);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Padding = new System.Windows.Forms.Padding(12);
            this.MainPanel.Size = new System.Drawing.Size(400, 220);
            this.MainPanel.TabIndex = 0;
            // 
            // DialogLayoutTableLayoutPanel
            // 
            this.DialogLayoutTableLayoutPanel.ColumnCount = 1;
            this.DialogLayoutTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.DialogLayoutTableLayoutPanel.Controls.Add(this.MessageLayoutTableLayoutPanel, 0, 0);
            this.DialogLayoutTableLayoutPanel.Controls.Add(this.ButtonsTableLayoutPanel, 0, 1);
            this.DialogLayoutTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DialogLayoutTableLayoutPanel.Location = new System.Drawing.Point(12, 12);
            this.DialogLayoutTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.DialogLayoutTableLayoutPanel.Name = "DialogLayoutTableLayoutPanel";
            this.DialogLayoutTableLayoutPanel.RowCount = 2;
            this.DialogLayoutTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.DialogLayoutTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.DialogLayoutTableLayoutPanel.Size = new System.Drawing.Size(376, 196);
            this.DialogLayoutTableLayoutPanel.TabIndex = 0;
            // 
            // MessageLayoutTableLayoutPanel
            // 
            // The message takes the room there is, the two options sit in a row under it
            // and the third column keeps them from being spread over the whole width.
            this.MessageLayoutTableLayoutPanel.ColumnCount = 3;
            this.MessageLayoutTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.MessageLayoutTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.MessageLayoutTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MessageLayoutTableLayoutPanel.Controls.Add(this.MessageTextTextBox, 0, 0);
            this.MessageLayoutTableLayoutPanel.Controls.Add(this.MakeBuckupCheckBox, 0, 1);
            this.MessageLayoutTableLayoutPanel.Controls.Add(this.DoNotSwitchCheckBox, 1, 1);
            this.MessageLayoutTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MessageLayoutTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.MessageLayoutTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.MessageLayoutTableLayoutPanel.Name = "MessageLayoutTableLayoutPanel";
            this.MessageLayoutTableLayoutPanel.RowCount = 2;
            this.MessageLayoutTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MessageLayoutTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.MessageLayoutTableLayoutPanel.Size = new System.Drawing.Size(376, 152);
            this.MessageLayoutTableLayoutPanel.TabIndex = 0;
            // 
            // MessageTextTextBox
            // 
            // A read only box rather than a label, because the message is long and has to
            // be selectable so it can be copied out.
            this.MessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MessageLayoutTableLayoutPanel.SetColumnSpan(this.MessageTextTextBox, 3);
            this.MessageTextTextBox.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MessageTextTextBox.Location = new System.Drawing.Point(0, 0);
            this.MessageTextTextBox.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.MessageTextTextBox.Multiline = true;
            this.MessageTextTextBox.Name = "MessageTextTextBox";
            this.MessageTextTextBox.ReadOnly = true;
            this.MessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.MessageTextTextBox.Size = new System.Drawing.Size(376, 119);
            this.MessageTextTextBox.TabIndex = 0;
            this.MessageTextTextBox.Text = "Game mode will be changed to normal when ";
            this.MessageTextTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // MakeBuckupCheckBox
            // 
            this.MakeBuckupCheckBox.AutoSize = true;
            this.MakeBuckupCheckBox.Checked = true;
            this.MakeBuckupCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.MakeBuckupCheckBox.Location = new System.Drawing.Point(0, 129);
            this.MakeBuckupCheckBox.Margin = new System.Windows.Forms.Padding(0, 0, 20, 0);
            this.MakeBuckupCheckBox.Name = "MakeBuckupCheckBox";
            this.MakeBuckupCheckBox.Size = new System.Drawing.Size(101, 19);
            this.MakeBuckupCheckBox.TabIndex = 1;
            this.MakeBuckupCheckBox.Text = "Make backup";
            // 
            // DoNotSwitchCheckBox
            // 
            this.DoNotSwitchCheckBox.AutoSize = true;
            this.DoNotSwitchCheckBox.Location = new System.Drawing.Point(121, 129);
            this.DoNotSwitchCheckBox.Margin = new System.Windows.Forms.Padding(0);
            this.DoNotSwitchCheckBox.Name = "DoNotSwitchCheckBox";
            this.DoNotSwitchCheckBox.Size = new System.Drawing.Size(101, 19);
            this.DoNotSwitchCheckBox.TabIndex = 2;
            this.DoNotSwitchCheckBox.Text = "Do not switch";
            // 
            // ButtonsTableLayoutPanel
            // 
            this.ButtonsTableLayoutPanel.ColumnCount = 3;
            this.ButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ButtonsTableLayoutPanel.Controls.Add(this.btnSwitchMode, 1, 0);
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
            // btnSwitchMode
            // 
            this.btnSwitchMode.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnSwitchMode.AutoSize = true;
            this.btnSwitchMode.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSwitchMode.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSwitchMode.Location = new System.Drawing.Point(152, 6);
            this.btnSwitchMode.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.btnSwitchMode.MinimumSize = new System.Drawing.Size(96, 32);
            this.btnSwitchMode.Name = "btnSwitchMode";
            this.btnSwitchMode.Padding = new System.Windows.Forms.Padding(14, 6, 14, 6);
            this.btnSwitchMode.Size = new System.Drawing.Size(104, 32);
            this.btnSwitchMode.TabIndex = 0;
            this.btnSwitchMode.Text = "Switch mode";
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
            // SwitchModeDialogForm
            // 
            this.AcceptButton = this.btnSwitchMode;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(400, 220);
            this.Controls.Add(this.MainPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SwitchModeDialogForm";
            this.Text = "Mode switch";
            this.Load += new System.EventHandler(this.SwitchModeDialogForm_Load);
            this.MainPanel.ResumeLayout(false);
            this.DialogLayoutTableLayoutPanel.ResumeLayout(false);
            this.MessageLayoutTableLayoutPanel.ResumeLayout(false);
            this.MessageLayoutTableLayoutPanel.PerformLayout();
            this.ButtonsTableLayoutPanel.ResumeLayout(false);
            this.ButtonsTableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel MainPanel;
        private System.Windows.Forms.TableLayoutPanel DialogLayoutTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel MessageLayoutTableLayoutPanel;
        private System.Windows.Forms.TextBox MessageTextTextBox;
        private System.Windows.Forms.TableLayoutPanel ButtonsTableLayoutPanel;
        private System.Windows.Forms.Button btnSwitchMode;
        private System.Windows.Forms.Button btnCancel;
        public System.Windows.Forms.CheckBox MakeBuckupCheckBox;
        public System.Windows.Forms.CheckBox DoNotSwitchCheckBox;
    }
}
