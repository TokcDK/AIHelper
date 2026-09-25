namespace AIHelper.Forms.Other
{
    partial class InstallProgressForm
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
            this.components = new System.ComponentModel.Container();
            this.mainPanel = new System.Windows.Forms.Panel();
            this.mainTable = new System.Windows.Forms.TableLayoutPanel();
            this.infoPanel = new System.Windows.Forms.Panel();
            this.lblFolder = new System.Windows.Forms.Label();
            this.progressStep = new System.Windows.Forms.ProgressBar();
            this.lblStep = new System.Windows.Forms.Label();
            this.progressOverall = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.buttonsTable = new System.Windows.Forms.TableLayoutPanel();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnOpenFolder = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.logFlushTimer = new System.Windows.Forms.Timer(this.components);
            this.mainPanel.SuspendLayout();
            this.mainTable.SuspendLayout();
            this.infoPanel.SuspendLayout();
            this.buttonsTable.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainPanel
            // 
            this.mainPanel.Controls.Add(this.mainTable);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(704, 521);
            this.mainPanel.TabIndex = 0;
            // 
            // mainTable
            // 
            this.mainTable.ColumnCount = 1;
            this.mainTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTable.Controls.Add(this.infoPanel, 0, 0);
            this.mainTable.Controls.Add(this.txtLog, 0, 1);
            this.mainTable.Controls.Add(this.buttonsTable, 0, 2);
            this.mainTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTable.Location = new System.Drawing.Point(0, 0);
            this.mainTable.Name = "mainTable";
            this.mainTable.RowCount = 3;
            this.mainTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainTable.Size = new System.Drawing.Size(704, 521);
            this.mainTable.TabIndex = 0;
            // 
            // infoPanel
            // 
            this.infoPanel.Controls.Add(this.lblFolder);
            this.infoPanel.Controls.Add(this.progressStep);
            this.infoPanel.Controls.Add(this.lblStep);
            this.infoPanel.Controls.Add(this.progressOverall);
            this.infoPanel.Controls.Add(this.lblStatus);
            this.infoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infoPanel.Location = new System.Drawing.Point(3, 3);
            this.infoPanel.Name = "infoPanel";
            this.infoPanel.Size = new System.Drawing.Size(698, 128);
            this.infoPanel.TabIndex = 0;
            // 
            // lblFolder
            // 
            this.lblFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFolder.AutoEllipsis = true;
            this.lblFolder.Location = new System.Drawing.Point(12, 102);
            this.lblFolder.Name = "lblFolder";
            this.lblFolder.Size = new System.Drawing.Size(674, 18);
            this.lblFolder.TabIndex = 4;
            this.lblFolder.Text = "2MO";
            // 
            // progressStep
            // 
            this.progressStep.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressStep.Location = new System.Drawing.Point(12, 78);
            this.progressStep.Name = "progressStep";
            this.progressStep.Size = new System.Drawing.Size(674, 18);
            this.progressStep.TabIndex = 3;
            // 
            // lblStep
            // 
            this.lblStep.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStep.AutoEllipsis = true;
            this.lblStep.Location = new System.Drawing.Point(12, 58);
            this.lblStep.Name = "lblStep";
            this.lblStep.Size = new System.Drawing.Size(674, 16);
            this.lblStep.TabIndex = 2;
            this.lblStep.Text = "_";
            // 
            // progressOverall
            // 
            this.progressOverall.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressOverall.Location = new System.Drawing.Point(12, 32);
            this.progressOverall.Name = "progressOverall";
            this.progressOverall.Size = new System.Drawing.Size(674, 18);
            this.progressOverall.TabIndex = 1;
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.AutoEllipsis = true;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblStatus.Location = new System.Drawing.Point(12, 8);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(674, 20);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "_";
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.SystemColors.Window;
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtLog.HideSelection = false;
            this.txtLog.Location = new System.Drawing.Point(3, 137);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(698, 333);
            this.txtLog.TabIndex = 1;
            this.txtLog.WordWrap = false;
            // 
            // buttonsTable
            // 
            this.buttonsTable.ColumnCount = 3;
            this.buttonsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34F));
            this.buttonsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.buttonsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.buttonsTable.Controls.Add(this.btnStart, 0, 0);
            this.buttonsTable.Controls.Add(this.btnOpenFolder, 1, 0);
            this.buttonsTable.Controls.Add(this.btnClose, 2, 0);
            this.buttonsTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonsTable.Location = new System.Drawing.Point(3, 476);
            this.buttonsTable.Name = "buttonsTable";
            this.buttonsTable.RowCount = 1;
            this.buttonsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.buttonsTable.Size = new System.Drawing.Size(698, 42);
            this.buttonsTable.TabIndex = 2;
            // 
            // btnStart
            // 
            this.btnStart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStart.Location = new System.Drawing.Point(3, 3);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(231, 36);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Install";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);
            // 
            // btnOpenFolder
            // 
            this.btnOpenFolder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOpenFolder.Location = new System.Drawing.Point(240, 3);
            this.btnOpenFolder.Name = "btnOpenFolder";
            this.btnOpenFolder.Size = new System.Drawing.Size(224, 36);
            this.btnOpenFolder.TabIndex = 1;
            this.btnOpenFolder.Text = "Open install folder";
            this.btnOpenFolder.UseVisualStyleBackColor = true;
            this.btnOpenFolder.Click += new System.EventHandler(this.BtnOpenFolder_Click);
            // 
            // btnClose
            // 
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClose.Location = new System.Drawing.Point(470, 3);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(225, 36);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // logFlushTimer
            // 
            this.logFlushTimer.Interval = 200;
            this.logFlushTimer.Tick += new System.EventHandler(this.LogFlushTimer_Tick);
            // 
            // InstallProgressForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.ClientSize = new System.Drawing.Size(704, 521);
            this.Controls.Add(this.mainPanel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(560, 400);
            this.Name = "InstallProgressForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "_";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_FormClosing);
            this.Load += new System.EventHandler(this.Form_Load);
            this.mainPanel.ResumeLayout(false);
            this.mainTable.ResumeLayout(false);
            this.mainTable.PerformLayout();
            this.infoPanel.ResumeLayout(false);
            this.buttonsTable.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.TableLayoutPanel mainTable;
        private System.Windows.Forms.Panel infoPanel;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ProgressBar progressOverall;
        private System.Windows.Forms.Label lblStep;
        private System.Windows.Forms.ProgressBar progressStep;
        private System.Windows.Forms.Label lblFolder;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.TableLayoutPanel buttonsTable;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnOpenFolder;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Timer logFlushTimer;
    }
}
