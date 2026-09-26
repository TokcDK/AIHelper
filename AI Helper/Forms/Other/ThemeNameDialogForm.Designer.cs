namespace AIHelper.Forms.Other
{
    partial class ThemeNameDialogForm
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
            if (disposing)
            {
                // the tooltip is created without a container, so components.Dispose does not
                // reach it; releasing it here as well as on close covers the caller in Ask,
                // which disposes the dialog through a using block
                DisposeTooltips();

                if (components != null)
                {
                    components.Dispose();
                }
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
            this.LayoutTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lblThemeName = new System.Windows.Forms.Label();
            this.txtThemeName = new System.Windows.Forms.TextBox();
            this.ButtonsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.MainPanel.SuspendLayout();
            this.LayoutTableLayoutPanel.SuspendLayout();
            this.ButtonsTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            this.MainPanel.Controls.Add(this.LayoutTableLayoutPanel);
            this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainPanel.Location = new System.Drawing.Point(0, 0);
            this.MainPanel.Margin = new System.Windows.Forms.Padding(0);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Padding = new System.Windows.Forms.Padding(12);
            this.MainPanel.Size = new System.Drawing.Size(420, 150);
            this.MainPanel.TabIndex = 0;
            // 
            // LayoutTableLayoutPanel
            // 
            this.LayoutTableLayoutPanel.ColumnCount = 1;
            this.LayoutTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.LayoutTableLayoutPanel.Controls.Add(this.lblThemeName, 0, 0);
            this.LayoutTableLayoutPanel.Controls.Add(this.txtThemeName, 0, 1);
            this.LayoutTableLayoutPanel.Controls.Add(this.ButtonsTableLayoutPanel, 0, 2);
            this.LayoutTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LayoutTableLayoutPanel.Location = new System.Drawing.Point(12, 12);
            this.LayoutTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.LayoutTableLayoutPanel.Name = "LayoutTableLayoutPanel";
            this.LayoutTableLayoutPanel.RowCount = 3;
            this.LayoutTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.LayoutTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.LayoutTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.LayoutTableLayoutPanel.Size = new System.Drawing.Size(396, 126);
            this.LayoutTableLayoutPanel.TabIndex = 0;
            // 
            // lblThemeName
            // 
            this.lblThemeName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblThemeName.Location = new System.Drawing.Point(0, 0);
            this.lblThemeName.Margin = new System.Windows.Forms.Padding(0);
            this.lblThemeName.Name = "lblThemeName";
            this.lblThemeName.Size = new System.Drawing.Size(396, 22);
            this.lblThemeName.TabIndex = 0;
            this.lblThemeName.Text = "Theme name";
            this.lblThemeName.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // txtThemeName
            // 
            this.txtThemeName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtThemeName.Location = new System.Drawing.Point(0, 22);
            this.txtThemeName.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.txtThemeName.Name = "txtThemeName";
            this.txtThemeName.Size = new System.Drawing.Size(396, 23);
            this.txtThemeName.TabIndex = 1;
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
            this.ButtonsTableLayoutPanel.Location = new System.Drawing.Point(0, 52);
            this.ButtonsTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.ButtonsTableLayoutPanel.Name = "ButtonsTableLayoutPanel";
            this.ButtonsTableLayoutPanel.RowCount = 1;
            this.ButtonsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ButtonsTableLayoutPanel.Size = new System.Drawing.Size(396, 74);
            this.ButtonsTableLayoutPanel.TabIndex = 2;
            // 
            // btnStart
            // 
            this.btnStart.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnStart.AutoSize = true;
            this.btnStart.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnStart.Location = new System.Drawing.Point(180, 21);
            this.btnStart.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.btnStart.MinimumSize = new System.Drawing.Size(96, 32);
            this.btnStart.Name = "btnStart";
            this.btnStart.Padding = new System.Windows.Forms.Padding(14, 6, 14, 6);
            this.btnStart.Size = new System.Drawing.Size(96, 32);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "OK";
            this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnCancel.AutoSize = true;
            this.btnCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(284, 21);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.btnCancel.MinimumSize = new System.Drawing.Size(96, 32);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Padding = new System.Windows.Forms.Padding(14, 6, 14, 6);
            this.btnCancel.Size = new System.Drawing.Size(96, 32);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            // 
            // ThemeNameDialogForm
            // 
            this.AcceptButton = this.btnStart;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(420, 150);
            this.Controls.Add(this.MainPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ThemeNameDialogForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Theme name";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ThemeNameDialogForm_FormClosing);
            this.Load += new System.EventHandler(this.ThemeNameDialogForm_Load);
            this.MainPanel.ResumeLayout(false);
            this.LayoutTableLayoutPanel.ResumeLayout(false);
            this.LayoutTableLayoutPanel.PerformLayout();
            this.ButtonsTableLayoutPanel.ResumeLayout(false);
            this.ButtonsTableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel MainPanel;
        private System.Windows.Forms.TableLayoutPanel LayoutTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel ButtonsTableLayoutPanel;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblThemeName;
        private System.Windows.Forms.TextBox txtThemeName;
    }
}
