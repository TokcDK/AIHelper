namespace AIHelper.Forms.Other
{
    partial class DialogFormGeneral
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
            this.InfoOptionsPanel = new System.Windows.Forms.Panel();
            this.flpOptions = new System.Windows.Forms.FlowLayoutPanel();
            this.ChoiceTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.MainPanel.SuspendLayout();
            this.DialogLayoutTableLayoutPanel.SuspendLayout();
            this.InfoOptionsPanel.SuspendLayout();
            this.ChoiceTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            // The whole content of the dialog sits here so the padding is applied once
            // instead of on every row.
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
            // Row 0 is the options the caller fills in, row 1 is the button bar. The
            // button bar keeps a fixed height so it does not move when options are
            // added or removed.
            this.DialogLayoutTableLayoutPanel.ColumnCount = 1;
            this.DialogLayoutTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.DialogLayoutTableLayoutPanel.Controls.Add(this.InfoOptionsPanel, 0, 0);
            this.DialogLayoutTableLayoutPanel.Controls.Add(this.ChoiceTableLayoutPanel, 0, 1);
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
            // InfoOptionsPanel
            // 
            this.InfoOptionsPanel.Controls.Add(this.flpOptions);
            this.InfoOptionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InfoOptionsPanel.Location = new System.Drawing.Point(0, 0);
            this.InfoOptionsPanel.Margin = new System.Windows.Forms.Padding(0);
            this.InfoOptionsPanel.Name = "InfoOptionsPanel";
            this.InfoOptionsPanel.Size = new System.Drawing.Size(376, 152);
            this.InfoOptionsPanel.TabIndex = 0;
            // 
            // flpOptions
            // 
            // Filled in by the caller, so it scrolls rather than clips when there are
            // more options than fit.
            this.flpOptions.AutoScroll = true;
            this.flpOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpOptions.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpOptions.Location = new System.Drawing.Point(0, 0);
            this.flpOptions.Margin = new System.Windows.Forms.Padding(0);
            this.flpOptions.Name = "flpOptions";
            this.flpOptions.Size = new System.Drawing.Size(376, 152);
            this.flpOptions.TabIndex = 0;
            this.flpOptions.WrapContents = false;
            // 
            // ChoiceTableLayoutPanel
            // 
            // The first column takes the slack, so the two buttons stay their own size
            // and sit against the right edge however wide the dialog is.
            this.ChoiceTableLayoutPanel.ColumnCount = 3;
            this.ChoiceTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ChoiceTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ChoiceTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ChoiceTableLayoutPanel.Controls.Add(this.btnStart, 1, 0);
            this.ChoiceTableLayoutPanel.Controls.Add(this.btnCancel, 2, 0);
            this.ChoiceTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ChoiceTableLayoutPanel.Location = new System.Drawing.Point(0, 152);
            this.ChoiceTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.ChoiceTableLayoutPanel.Name = "ChoiceTableLayoutPanel";
            this.ChoiceTableLayoutPanel.RowCount = 1;
            this.ChoiceTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ChoiceTableLayoutPanel.Size = new System.Drawing.Size(376, 44);
            this.ChoiceTableLayoutPanel.TabIndex = 1;
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
            this.btnStart.Text = "Start";
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
            // DialogFormGeneral
            // 
            this.AcceptButton = this.btnStart;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(400, 220);
            this.Controls.Add(this.MainPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MinimumSize = new System.Drawing.Size(320, 200);
            this.Name = "DialogFormGeneral";
            this.Text = "Options";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Closing);
            this.Load += new System.EventHandler(this.Form_Load);
            this.MainPanel.ResumeLayout(false);
            this.DialogLayoutTableLayoutPanel.ResumeLayout(false);
            this.InfoOptionsPanel.ResumeLayout(false);
            this.ChoiceTableLayoutPanel.ResumeLayout(false);
            this.ChoiceTableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel MainPanel;
        private System.Windows.Forms.TableLayoutPanel DialogLayoutTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel ChoiceTableLayoutPanel;
        private System.Windows.Forms.Panel InfoOptionsPanel;
        public System.Windows.Forms.FlowLayoutPanel flpOptions;
        public System.Windows.Forms.Button btnCancel;
        public System.Windows.Forms.Button btnStart;
    }
}
