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
            this.infoTable = new System.Windows.Forms.TableLayoutPanel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.progressOverall = new System.Windows.Forms.ProgressBar();
            this.lblStep = new System.Windows.Forms.Label();
            this.progressStep = new System.Windows.Forms.ProgressBar();
            this.lblFolder = new System.Windows.Forms.Label();
            this.txtLog = new AIHelper.Manage.ui.ThemedTextBox();
            this.buttonsTable = new System.Windows.Forms.TableLayoutPanel();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnOpenFolder = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.logFlushTimer = new System.Windows.Forms.Timer(this.components);
            this.mainPanel.SuspendLayout();
            this.mainTable.SuspendLayout();
            this.infoTable.SuspendLayout();
            this.buttonsTable.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainPanel
            // 
            this.mainPanel.Controls.Add(this.mainTable);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(0, 0);
            this.mainPanel.Margin = new System.Windows.Forms.Padding(0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Padding = new System.Windows.Forms.Padding(12);
            this.mainPanel.Size = new System.Drawing.Size(704, 521);
            this.mainPanel.TabIndex = 0;
            // 
            // mainTable
            // 
            // The status block is as tall as its lines need, the log takes everything
            // that is left and the buttons keep a fixed height at the bottom.
            this.mainTable.ColumnCount = 1;
            this.mainTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTable.Controls.Add(this.infoTable, 0, 0);
            this.mainTable.Controls.Add(this.txtLog, 0, 1);
            this.mainTable.Controls.Add(this.buttonsTable, 0, 2);
            this.mainTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTable.Location = new System.Drawing.Point(12, 12);
            this.mainTable.Margin = new System.Windows.Forms.Padding(0);
            this.mainTable.Name = "mainTable";
            this.mainTable.RowCount = 3;
            this.mainTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.mainTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.mainTable.Size = new System.Drawing.Size(680, 497);
            this.mainTable.TabIndex = 0;
            // 
            // infoTable
            // 
            this.infoTable.ColumnCount = 1;
            this.infoTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.infoTable.Controls.Add(this.lblStatus, 0, 0);
            this.infoTable.Controls.Add(this.progressOverall, 0, 1);
            this.infoTable.Controls.Add(this.lblStep, 0, 2);
            this.infoTable.Controls.Add(this.progressStep, 0, 3);
            this.infoTable.Controls.Add(this.lblFolder, 0, 4);
            this.infoTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infoTable.Location = new System.Drawing.Point(0, 0);
            this.infoTable.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.infoTable.Name = "infoTable";
            this.infoTable.RowCount = 5;
            this.infoTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.infoTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.infoTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.infoTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.infoTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.infoTable.Size = new System.Drawing.Size(680, 112);
            this.infoTable.TabIndex = 0;
            // 
            // lblStatus
            // 
            // The one line which says what is happening right now, so it is the one line
            // which is set in a larger bold face.
            this.lblStatus.AutoEllipsis = true;
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblStatus.Location = new System.Drawing.Point(0, 0);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(680, 18);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "_";
            // 
            // progressOverall
            // 
            this.progressOverall.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressOverall.Location = new System.Drawing.Point(0, 24);
            this.progressOverall.Margin = new System.Windows.Forms.Padding(0);
            this.progressOverall.Name = "progressOverall";
            this.progressOverall.Size = new System.Drawing.Size(680, 22);
            this.progressOverall.TabIndex = 1;
            // 
            // lblStep
            // 
            this.lblStep.AutoEllipsis = true;
            this.lblStep.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStep.Location = new System.Drawing.Point(0, 46);
            this.lblStep.Margin = new System.Windows.Forms.Padding(0);
            this.lblStep.Name = "lblStep";
            this.lblStep.Size = new System.Drawing.Size(680, 22);
            this.lblStep.TabIndex = 2;
            this.lblStep.Text = "_";
            this.lblStep.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progressStep
            // 
            this.progressStep.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressStep.Location = new System.Drawing.Point(0, 68);
            this.progressStep.Margin = new System.Windows.Forms.Padding(0);
            this.progressStep.Name = "progressStep";
            this.progressStep.Size = new System.Drawing.Size(680, 22);
            this.progressStep.TabIndex = 3;
            // 
            // lblFolder
            // 
            this.lblFolder.AutoEllipsis = true;
            this.lblFolder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFolder.Location = new System.Drawing.Point(0, 90);
            this.lblFolder.Margin = new System.Windows.Forms.Padding(0);
            this.lblFolder.Name = "lblFolder";
            this.lblFolder.Size = new System.Drawing.Size(680, 22);
            this.lblFolder.TabIndex = 4;
            this.lblFolder.Text = "2MO";
            this.lblFolder.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtLog
            // 
            // Read only and monospaced, so it reads as output rather than as a field.
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtLog.HideSelection = false;
            this.txtLog.Location = new System.Drawing.Point(0, 124);
            this.txtLog.Margin = new System.Windows.Forms.Padding(0);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(680, 329);
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
            this.buttonsTable.Location = new System.Drawing.Point(0, 453);
            this.buttonsTable.Margin = new System.Windows.Forms.Padding(0);
            this.buttonsTable.Name = "buttonsTable";
            this.buttonsTable.RowCount = 1;
            this.buttonsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.buttonsTable.Size = new System.Drawing.Size(680, 44);
            this.buttonsTable.TabIndex = 2;
            // 
            // btnStart
            // 
            this.btnStart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStart.Location = new System.Drawing.Point(0, 6);
            this.btnStart.Margin = new System.Windows.Forms.Padding(0, 6, 6, 6);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(225, 32);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Install";
            this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);
            // 
            // btnOpenFolder
            // 
            this.btnOpenFolder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOpenFolder.Location = new System.Drawing.Point(237, 6);
            this.btnOpenFolder.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnOpenFolder.Name = "btnOpenFolder";
            this.btnOpenFolder.Size = new System.Drawing.Size(212, 32);
            this.btnOpenFolder.TabIndex = 1;
            this.btnOpenFolder.Text = "Open install folder";
            this.btnOpenFolder.Click += new System.EventHandler(this.BtnOpenFolder_Click);
            // 
            // btnClose
            // 
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClose.Location = new System.Drawing.Point(461, 6);
            this.btnClose.Margin = new System.Windows.Forms.Padding(6, 6, 0, 6);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(219, 32);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // logFlushTimer
            // 
            this.logFlushTimer.Interval = 200;
            this.logFlushTimer.Tick += new System.EventHandler(this.LogFlushTimer_Tick);
            // 
            // InstallProgressForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(704, 521);
            this.Controls.Add(this.mainPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
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
            this.infoTable.ResumeLayout(false);
            this.buttonsTable.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.TableLayoutPanel mainTable;
        private System.Windows.Forms.TableLayoutPanel infoTable;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ProgressBar progressOverall;
        private System.Windows.Forms.Label lblStep;
        private System.Windows.Forms.ProgressBar progressStep;
        private System.Windows.Forms.Label lblFolder;
        private AIHelper.Manage.ui.ThemedTextBox txtLog;
        private System.Windows.Forms.TableLayoutPanel buttonsTable;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnOpenFolder;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Timer logFlushTimer;
    }
}
