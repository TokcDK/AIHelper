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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.ChoiceTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.InfoOptionsPanel = new System.Windows.Forms.Panel();
            this.flpOptions = new System.Windows.Forms.FlowLayoutPanel();
            this.MainPanel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.ChoiceTableLayoutPanel.SuspendLayout();
            this.InfoOptionsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            this.MainPanel.Controls.Add(this.tableLayoutPanel1);
            this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainPanel.Location = new System.Drawing.Point(0, 0);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = new System.Drawing.Size(354, 191);
            this.MainPanel.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.ChoiceTableLayoutPanel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.InfoOptionsPanel, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 78.01047F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 21.98953F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(354, 191);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // ChoiceTableLayoutPanel
            // 
            this.ChoiceTableLayoutPanel.ColumnCount = 2;
            this.ChoiceTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ChoiceTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ChoiceTableLayoutPanel.Controls.Add(this.btnCancel, 1, 0);
            this.ChoiceTableLayoutPanel.Controls.Add(this.btnStart, 0, 0);
            this.ChoiceTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ChoiceTableLayoutPanel.Location = new System.Drawing.Point(0, 148);
            this.ChoiceTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.ChoiceTableLayoutPanel.Name = "ChoiceTableLayoutPanel";
            this.ChoiceTableLayoutPanel.RowCount = 1;
            this.ChoiceTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ChoiceTableLayoutPanel.Size = new System.Drawing.Size(354, 43);
            this.ChoiceTableLayoutPanel.TabIndex = 0;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCancel.Location = new System.Drawing.Point(180, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(171, 37);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnStart
            // 
            this.btnStart.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnStart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStart.Location = new System.Drawing.Point(3, 3);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(171, 37);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            // 
            // InfoOptionsPanel
            // 
            this.InfoOptionsPanel.Controls.Add(this.flpOptions);
            this.InfoOptionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InfoOptionsPanel.Location = new System.Drawing.Point(0, 0);
            this.InfoOptionsPanel.Margin = new System.Windows.Forms.Padding(0);
            this.InfoOptionsPanel.Name = "InfoOptionsPanel";
            this.InfoOptionsPanel.Size = new System.Drawing.Size(354, 148);
            this.InfoOptionsPanel.TabIndex = 1;
            // 
            // flpOptions
            // 
            this.flpOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpOptions.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpOptions.Location = new System.Drawing.Point(0, 0);
            this.flpOptions.Margin = new System.Windows.Forms.Padding(0);
            this.flpOptions.Name = "flpOptions";
            this.flpOptions.Size = new System.Drawing.Size(354, 148);
            this.flpOptions.TabIndex = 0;
            // 
            // DialogFormGeneral
            // 
            this.AcceptButton = this.btnStart;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(354, 191);
            this.Controls.Add(this.MainPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "DialogFormGeneral";
            this.Text = "_";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Closing);
            this.Load += new System.EventHandler(this.Form_Load);
            this.MainPanel.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ChoiceTableLayoutPanel.ResumeLayout(false);
            this.InfoOptionsPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel MainPanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel ChoiceTableLayoutPanel;
        private System.Windows.Forms.Panel InfoOptionsPanel;
        public System.Windows.Forms.FlowLayoutPanel flpOptions;
        public System.Windows.Forms.Button btnCancel;
        public System.Windows.Forms.Button btnStart;
    }
}