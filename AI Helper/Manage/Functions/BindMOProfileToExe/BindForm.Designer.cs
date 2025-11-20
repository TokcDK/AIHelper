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
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.ProfilesComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ExesComboBox = new System.Windows.Forms.ComboBox();
            this.AddExeButton = new System.Windows.Forms.Button();
            this.SaveButton = new System.Windows.Forms.Button();
            this.BoundExesListBox = new System.Windows.Forms.ListBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.RemoveExeButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 89.43662F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.56338F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 235F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.BoundExesListBox, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(492, 167);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.label1);
            this.flowLayoutPanel1.Controls.Add(this.ProfilesComboBox);
            this.flowLayoutPanel1.Controls.Add(this.label2);
            this.flowLayoutPanel1.Controls.Add(this.ExesComboBox);
            this.flowLayoutPanel1.Controls.Add(this.SaveButton);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(223, 161);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Profiles";
            // 
            // ProfilesComboBox
            // 
            this.ProfilesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ProfilesComboBox.FormattingEnabled = true;
            this.ProfilesComboBox.Location = new System.Drawing.Point(3, 16);
            this.ProfilesComboBox.Name = "ProfilesComboBox";
            this.ProfilesComboBox.Size = new System.Drawing.Size(208, 21);
            this.ProfilesComboBox.TabIndex = 1;
            this.ProfilesComboBox.SelectedIndexChanged += new System.EventHandler(this.ProfilesComboBox_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Games";
            // 
            // ExesComboBox
            // 
            this.ExesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ExesComboBox.FormattingEnabled = true;
            this.ExesComboBox.Location = new System.Drawing.Point(3, 56);
            this.ExesComboBox.Name = "ExesComboBox";
            this.ExesComboBox.Size = new System.Drawing.Size(208, 21);
            this.ExesComboBox.TabIndex = 3;
            // 
            // AddExeButton
            // 
            this.AddExeButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AddExeButton.Location = new System.Drawing.Point(3, 3);
            this.AddExeButton.Name = "AddExeButton";
            this.AddExeButton.Size = new System.Drawing.Size(15, 74);
            this.AddExeButton.TabIndex = 4;
            this.AddExeButton.Text = ">";
            this.AddExeButton.UseVisualStyleBackColor = true;
            this.AddExeButton.Click += new System.EventHandler(this.AddExeButton_Click);
            // 
            // SaveButton
            // 
            this.SaveButton.Location = new System.Drawing.Point(3, 83);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(75, 23);
            this.SaveButton.TabIndex = 5;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = true;
            // 
            // BoundExesListBox
            // 
            this.BoundExesListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BoundExesListBox.FormattingEnabled = true;
            this.BoundExesListBox.Location = new System.Drawing.Point(259, 3);
            this.BoundExesListBox.Name = "BoundExesListBox";
            this.BoundExesListBox.Size = new System.Drawing.Size(230, 161);
            this.BoundExesListBox.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.AddExeButton, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.RemoveExeButton, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(232, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(21, 161);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // RemoveExeButton
            // 
            this.RemoveExeButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RemoveExeButton.Location = new System.Drawing.Point(3, 83);
            this.RemoveExeButton.Name = "RemoveExeButton";
            this.RemoveExeButton.Size = new System.Drawing.Size(15, 75);
            this.RemoveExeButton.TabIndex = 6;
            this.RemoveExeButton.Text = "<";
            this.RemoveExeButton.UseVisualStyleBackColor = true;
            this.RemoveExeButton.Click += new System.EventHandler(this.RemoveExeButton_Click);
            // 
            // BindForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(492, 167);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "BindForm";
            this.Text = "BindForm";
            this.Load += new System.EventHandler(this.BindForm_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ProfilesComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox ExesComboBox;
        private System.Windows.Forms.Button AddExeButton;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.ListBox BoundExesListBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button RemoveExeButton;
    }
}