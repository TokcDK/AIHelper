namespace AIHelper.Forms.ExtraSettings.Elements.BepinEx
{
    partial class BepinExForm
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
            this.BepinExSettingsPanel = new System.Windows.Forms.Panel();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.BepInExLogLevelsLabel = new System.Windows.Forms.Label();
            this.BepInExSettingsGroupBox = new System.Windows.Forms.GroupBox();
            this.BepInExLogLevelsSourceLabel = new System.Windows.Forms.Label();
            this.BepInExLogLevelsGroupBox = new System.Windows.Forms.GroupBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.BepinExSettingsPanel.SuspendLayout();
            this.BepInExSettingsGroupBox.SuspendLayout();
            this.BepInExLogLevelsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BepinExSettingsPanel
            // 
            this.BepinExSettingsPanel.Controls.Add(this.BepInExSettingsGroupBox);
            this.BepinExSettingsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BepinExSettingsPanel.Location = new System.Drawing.Point(0, 0);
            this.BepinExSettingsPanel.Name = "BepinExSettingsPanel";
            this.BepinExSettingsPanel.Size = new System.Drawing.Size(350, 200);
            this.BepinExSettingsPanel.TabIndex = 0;
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.BackColor = System.Drawing.Color.Gray;
            this.checkedListBox1.ForeColor = System.Drawing.SystemColors.Window;
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Items.AddRange(new object[] {
            "Fatal",
            "Error",
            "Warning",
            "Message",
            "Info",
            "Debug"});
            this.checkedListBox1.Location = new System.Drawing.Point(6, 54);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(72, 100);
            this.checkedListBox1.TabIndex = 0;
            // 
            // BepInExLogLevelsLabel
            // 
            this.BepInExLogLevelsLabel.AutoSize = true;
            this.BepInExLogLevelsLabel.Location = new System.Drawing.Point(21, 38);
            this.BepInExLogLevelsLabel.Name = "BepInExLogLevelsLabel";
            this.BepInExLogLevelsLabel.Size = new System.Drawing.Size(34, 13);
            this.BepInExLogLevelsLabel.TabIndex = 1;
            this.BepInExLogLevelsLabel.Text = "levels";
            // 
            // BepInExSettingsGroupBox
            // 
            this.BepInExSettingsGroupBox.Controls.Add(this.BepInExLogLevelsGroupBox);
            this.BepInExSettingsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BepInExSettingsGroupBox.ForeColor = System.Drawing.Color.White;
            this.BepInExSettingsGroupBox.Location = new System.Drawing.Point(0, 0);
            this.BepInExSettingsGroupBox.Name = "BepInExSettingsGroupBox";
            this.BepInExSettingsGroupBox.Size = new System.Drawing.Size(350, 200);
            this.BepInExSettingsGroupBox.TabIndex = 2;
            this.BepInExSettingsGroupBox.TabStop = false;
            this.BepInExSettingsGroupBox.Text = "BepInEx";
            // 
            // BepInExLogLevelsSourceLabel
            // 
            this.BepInExLogLevelsSourceLabel.AutoSize = true;
            this.BepInExLogLevelsSourceLabel.Location = new System.Drawing.Point(6, 17);
            this.BepInExLogLevelsSourceLabel.Name = "BepInExLogLevelsSourceLabel";
            this.BepInExLogLevelsSourceLabel.Size = new System.Drawing.Size(37, 13);
            this.BepInExLogLevelsSourceLabel.TabIndex = 2;
            this.BepInExLogLevelsSourceLabel.Text = "target";
            // 
            // BepInExLogLevelsGroupBox
            // 
            this.BepInExLogLevelsGroupBox.Controls.Add(this.linkLabel1);
            this.BepInExLogLevelsGroupBox.Controls.Add(this.checkedListBox1);
            this.BepInExLogLevelsGroupBox.Controls.Add(this.BepInExLogLevelsSourceLabel);
            this.BepInExLogLevelsGroupBox.Controls.Add(this.BepInExLogLevelsLabel);
            this.BepInExLogLevelsGroupBox.ForeColor = System.Drawing.Color.White;
            this.BepInExLogLevelsGroupBox.Location = new System.Drawing.Point(3, 34);
            this.BepInExLogLevelsGroupBox.Name = "BepInExLogLevelsGroupBox";
            this.BepInExLogLevelsGroupBox.Size = new System.Drawing.Size(137, 160);
            this.BepInExLogLevelsGroupBox.TabIndex = 3;
            this.BepInExLogLevelsGroupBox.TabStop = false;
            this.BepInExLogLevelsGroupBox.Text = "log";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(47, 17);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(31, 13);
            this.linkLabel1.TabIndex = 4;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "none";
            // 
            // BepinExForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.ClientSize = new System.Drawing.Size(350, 200);
            this.Controls.Add(this.BepinExSettingsPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "BepinExForm";
            this.Text = "BepinExForm";
            this.BepinExSettingsPanel.ResumeLayout(false);
            this.BepInExSettingsGroupBox.ResumeLayout(false);
            this.BepInExLogLevelsGroupBox.ResumeLayout(false);
            this.BepInExLogLevelsGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel BepinExSettingsPanel;
        private System.Windows.Forms.GroupBox BepInExSettingsGroupBox;
        private System.Windows.Forms.Label BepInExLogLevelsSourceLabel;
        private System.Windows.Forms.Label BepInExLogLevelsLabel;
        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.GroupBox BepInExLogLevelsGroupBox;
        private System.Windows.Forms.LinkLabel linkLabel1;
    }
}