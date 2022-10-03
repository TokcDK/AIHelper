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
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.cbxMoveIntoNewMod = new System.Windows.Forms.CheckBox();
            this.cbxIgnoreSymlinks = new System.Windows.Forms.CheckBox();
            this.cbxIgnoreShortcuts = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tableLayoutPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(354, 187);
            this.panel1.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 72.19251F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 27.80749F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(354, 187);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 48.27586F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 51.72414F));
            this.tableLayoutPanel2.Controls.Add(this.btnStart, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnCancel, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 138);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(348, 46);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // btnStart
            // 
            this.btnStart.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnStart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStart.Location = new System.Drawing.Point(3, 3);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(162, 40);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "OK";
            this.btnStart.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCancel.Location = new System.Drawing.Point(171, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(174, 40);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.cbxMoveIntoNewMod);
            this.flowLayoutPanel1.Controls.Add(this.cbxIgnoreSymlinks);
            this.flowLayoutPanel1.Controls.Add(this.cbxIgnoreShortcuts);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(348, 129);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // cbxMoveIntoNewMod
            // 
            this.cbxMoveIntoNewMod.AutoSize = true;
            this.cbxMoveIntoNewMod.Location = new System.Drawing.Point(3, 3);
            this.cbxMoveIntoNewMod.Name = "cbxMoveIntoNewMod";
            this.cbxMoveIntoNewMod.Size = new System.Drawing.Size(150, 17);
            this.cbxMoveIntoNewMod.TabIndex = 0;
            this.cbxMoveIntoNewMod.Text = "Remove into the new mod";
            this.cbxMoveIntoNewMod.UseVisualStyleBackColor = true;
            // 
            // cbxIgnoreSymlinks
            // 
            this.cbxIgnoreSymlinks.AutoSize = true;
            this.cbxIgnoreSymlinks.Checked = true;
            this.cbxIgnoreSymlinks.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxIgnoreSymlinks.Location = new System.Drawing.Point(3, 26);
            this.cbxIgnoreSymlinks.Name = "cbxIgnoreSymlinks";
            this.cbxIgnoreSymlinks.Size = new System.Drawing.Size(98, 17);
            this.cbxIgnoreSymlinks.TabIndex = 1;
            this.cbxIgnoreSymlinks.Text = "Ignore symlinks";
            this.cbxIgnoreSymlinks.UseVisualStyleBackColor = true;
            // 
            // cbxIgnoreShortcuts
            // 
            this.cbxIgnoreShortcuts.AutoSize = true;
            this.cbxIgnoreShortcuts.Location = new System.Drawing.Point(3, 49);
            this.cbxIgnoreShortcuts.Name = "cbxIgnoreShortcuts";
            this.cbxIgnoreShortcuts.Size = new System.Drawing.Size(102, 17);
            this.cbxIgnoreShortcuts.TabIndex = 2;
            this.cbxIgnoreShortcuts.Text = "Ignore shortcuts";
            this.cbxIgnoreShortcuts.UseVisualStyleBackColor = true;
            // 
            // CleanOptionsDialogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.ClientSize = new System.Drawing.Size(354, 187);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "CleanOptionsDialogForm";
            this.Text = "Options";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CleanOptionsDialogForm_FormClosing);
            this.Load += new System.EventHandler(this.CleanOptionsDialogForm_Load);
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        public System.Windows.Forms.CheckBox cbxMoveIntoNewMod;
        public System.Windows.Forms.CheckBox cbxIgnoreSymlinks;
        public System.Windows.Forms.CheckBox cbxIgnoreShortcuts;
    }
}