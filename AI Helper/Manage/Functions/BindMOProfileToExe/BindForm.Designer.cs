namespace AIHelper.Manage.Functions.BindMOProfileToExe
{
    partial class BindForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.ProfileLabel = new System.Windows.Forms.Label();
            this.ProfilesComboBox = new System.Windows.Forms.ComboBox();
            this.ExesLabel = new System.Windows.Forms.Label();
            this.ExesComboBox = new System.Windows.Forms.ComboBox();
            this.BoundExesListBox = new System.Windows.Forms.ListBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.AddExeButton = new System.Windows.Forms.Button();
            this.RemoveExeButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(539, 242);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            this.ProfileLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ProfileLabel.AutoSize = true;
            this.ProfileLabel.Location = new System.Drawing.Point(3, 7);
            this.ProfileLabel.Name = "label1";
            this.ProfileLabel.Size = new System.Drawing.Size(41, 13);
            this.ProfileLabel.TabIndex = 0;
            this.ProfileLabel.Text = "Profiles";
            this.ProfileLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // ProfilesComboBox
            // 
            this.ProfilesComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProfilesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ProfilesComboBox.FormattingEnabled = true;
            this.ProfilesComboBox.Location = new System.Drawing.Point(3, 23);
            this.ProfilesComboBox.Name = "ProfilesComboBox";
            this.ProfilesComboBox.Size = new System.Drawing.Size(521, 21);
            this.ProfilesComboBox.TabIndex = 1;
            this.ProfilesComboBox.SelectedIndexChanged += new System.EventHandler(this.ProfilesComboBox_SelectedIndexChanged);
            // 
            // label2
            // 
            this.ExesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ExesLabel.AutoSize = true;
            this.ExesLabel.Location = new System.Drawing.Point(3, 47);
            this.ExesLabel.Name = "label2";
            this.ExesLabel.Size = new System.Drawing.Size(40, 13);
            this.ExesLabel.TabIndex = 2;
            this.ExesLabel.Text = "Games";
            this.ExesLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // ExesComboBox
            // 
            this.ExesComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ExesComboBox.FormattingEnabled = true;
            this.ExesComboBox.Location = new System.Drawing.Point(3, 63);
            this.ExesComboBox.Name = "ExesComboBox";
            this.ExesComboBox.Size = new System.Drawing.Size(521, 21);
            this.ExesComboBox.TabIndex = 3;
            // 
            // BoundExesListBox
            // 
            this.BoundExesListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BoundExesListBox.FormattingEnabled = true;
            this.BoundExesListBox.Location = new System.Drawing.Point(3, 137);
            this.BoundExesListBox.Name = "BoundExesListBox";
            this.BoundExesListBox.Size = new System.Drawing.Size(527, 88);
            this.BoundExesListBox.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.AddExeButton, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.RemoveExeButton, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 100);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(527, 31);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // AddExeButton
            // 
            this.AddExeButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AddExeButton.Location = new System.Drawing.Point(3, 3);
            this.AddExeButton.Name = "AddExeButton";
            this.AddExeButton.Size = new System.Drawing.Size(257, 25);
            this.AddExeButton.TabIndex = 4;
            this.AddExeButton.Text = "↓";
            this.AddExeButton.UseVisualStyleBackColor = true;
            this.AddExeButton.Click += new System.EventHandler(this.AddExeButton_Click);
            // 
            // RemoveExeButton
            // 
            this.RemoveExeButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RemoveExeButton.Location = new System.Drawing.Point(266, 3);
            this.RemoveExeButton.Name = "RemoveExeButton";
            this.RemoveExeButton.Size = new System.Drawing.Size(258, 25);
            this.RemoveExeButton.TabIndex = 6;
            this.RemoveExeButton.Text = "↑";
            this.RemoveExeButton.UseVisualStyleBackColor = true;
            this.RemoveExeButton.Click += new System.EventHandler(this.RemoveExeButton_Click);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel4, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.BoundExesListBox, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 3;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 97F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(533, 228);
            this.tableLayoutPanel3.TabIndex = 3;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.ExesComboBox, 0, 3);
            this.tableLayoutPanel4.Controls.Add(this.ExesLabel, 0, 2);
            this.tableLayoutPanel4.Controls.Add(this.ProfilesComboBox, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.ProfileLabel, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 4;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(527, 91);
            this.tableLayoutPanel4.TabIndex = 4;
            // 
            // BindForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(539, 242);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "BindForm";
            this.Text = "BindForm";
            this.Load += new System.EventHandler(this.BindForm_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label ProfileLabel;
        private System.Windows.Forms.ComboBox ProfilesComboBox;
        private System.Windows.Forms.Label ExesLabel;
        private System.Windows.Forms.ComboBox ExesComboBox;
        private System.Windows.Forms.Button AddExeButton;
        private System.Windows.Forms.ListBox BoundExesListBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button RemoveExeButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
    }
}