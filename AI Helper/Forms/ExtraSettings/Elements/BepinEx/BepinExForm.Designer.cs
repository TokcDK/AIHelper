namespace AIHelper.Forms.ExtraSettings.Elements.BepinEx
{
    internal partial class BepinExForm
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
            this.BepInExSettingsGroupBox = new System.Windows.Forms.GroupBox();
            this.BepInExSettingsDisplayedLogLevelLabel = new System.Windows.Forms.Label();
            this.OpenLogLinkLabel = new System.Windows.Forms.LinkLabel();
            this.BepInExSettingsLogCheckBox = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.BepinExHelpLinkLabel = new System.Windows.Forms.LinkLabel();
            this.BepinExLogOpenConfigFileLinkLabel = new System.Windows.Forms.LinkLabel();
            this.BepinExLogTargetLinkLabel = new System.Windows.Forms.LinkLabel();
            this.BepInExSettingsLogCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.BepInExLogLevelsSourceLabel = new System.Windows.Forms.Label();
            this.BepInExLogLevelsLabel = new System.Windows.Forms.Label();
            this.BepinExSettingsPanel.SuspendLayout();
            this.BepInExSettingsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BepinExSettingsPanel
            // 
            this.BepinExSettingsPanel.Controls.Add(this.BepInExSettingsGroupBox);
            this.BepinExSettingsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BepinExSettingsPanel.Location = new System.Drawing.Point(0, 0);
            this.BepinExSettingsPanel.Name = "BepinExSettingsPanel";
            this.BepinExSettingsPanel.Size = new System.Drawing.Size(170, 180);
            this.BepinExSettingsPanel.TabIndex = 0;
            // 
            // BepInExSettingsGroupBox
            // 
            this.BepInExSettingsGroupBox.Controls.Add(this.BepInExSettingsDisplayedLogLevelLabel);
            this.BepInExSettingsGroupBox.Controls.Add(this.OpenLogLinkLabel);
            this.BepInExSettingsGroupBox.Controls.Add(this.BepInExSettingsLogCheckBox);
            this.BepInExSettingsGroupBox.Controls.Add(this.label2);
            this.BepInExSettingsGroupBox.Controls.Add(this.label1);
            this.BepInExSettingsGroupBox.Controls.Add(this.BepinExHelpLinkLabel);
            this.BepInExSettingsGroupBox.Controls.Add(this.BepinExLogOpenConfigFileLinkLabel);
            this.BepInExSettingsGroupBox.Controls.Add(this.BepinExLogTargetLinkLabel);
            this.BepInExSettingsGroupBox.Controls.Add(this.BepInExSettingsLogCheckedListBox);
            this.BepInExSettingsGroupBox.Controls.Add(this.BepInExLogLevelsSourceLabel);
            this.BepInExSettingsGroupBox.Controls.Add(this.BepInExLogLevelsLabel);
            this.BepInExSettingsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BepInExSettingsGroupBox.ForeColor = System.Drawing.Color.White;
            this.BepInExSettingsGroupBox.Location = new System.Drawing.Point(0, 0);
            this.BepInExSettingsGroupBox.Name = "BepInExSettingsGroupBox";
            this.BepInExSettingsGroupBox.Size = new System.Drawing.Size(170, 180);
            this.BepInExSettingsGroupBox.TabIndex = 2;
            this.BepInExSettingsGroupBox.TabStop = false;
            this.BepInExSettingsGroupBox.Text = "BepInEx";
            // 
            // BepInExSettingsDisplayedLogLevelLabel
            // 
            this.BepInExSettingsDisplayedLogLevelLabel.AutoSize = true;
            this.BepInExSettingsDisplayedLogLevelLabel.ForeColor = System.Drawing.SystemColors.ScrollBar;
            this.BepInExSettingsDisplayedLogLevelLabel.Location = new System.Drawing.Point(40, 160);
            this.BepInExSettingsDisplayedLogLevelLabel.Name = "BepInExSettingsDisplayedLogLevelLabel";
            this.BepInExSettingsDisplayedLogLevelLabel.Size = new System.Drawing.Size(27, 13);
            this.BepInExSettingsDisplayedLogLevelLabel.TabIndex = 27;
            this.BepInExSettingsDisplayedLogLevelLabel.Text = "Info";
            this.BepInExSettingsDisplayedLogLevelLabel.Visible = false;
            this.BepInExSettingsDisplayedLogLevelLabel.Click += new System.EventHandler(this.BepInExSettingsDisplayedLogLevelLabel_Click);
            // 
            // OpenLogLinkLabel
            // 
            this.OpenLogLinkLabel.AutoSize = true;
            this.OpenLogLinkLabel.LinkColor = System.Drawing.Color.WhiteSmoke;
            this.OpenLogLinkLabel.Location = new System.Drawing.Point(3, 160);
            this.OpenLogLinkLabel.Name = "OpenLogLinkLabel";
            this.OpenLogLinkLabel.Size = new System.Drawing.Size(21, 13);
            this.OpenLogLinkLabel.TabIndex = 21;
            this.OpenLogLinkLabel.TabStop = true;
            this.OpenLogLinkLabel.Text = "log";
            this.OpenLogLinkLabel.VisitedLinkColor = System.Drawing.Color.Gainsboro;
            this.OpenLogLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OpenLogLinkLabel_LinkClicked);
            // 
            // BepInExSettingsLogCheckBox
            // 
            this.BepInExSettingsLogCheckBox.AutoSize = true;
            this.BepInExSettingsLogCheckBox.Location = new System.Drawing.Point(25, 161);
            this.BepInExSettingsLogCheckBox.Name = "BepInExSettingsLogCheckBox";
            this.BepInExSettingsLogCheckBox.Size = new System.Drawing.Size(15, 14);
            this.BepInExSettingsLogCheckBox.TabIndex = 20;
            this.BepInExSettingsLogCheckBox.UseVisualStyleBackColor = true;
            this.BepInExSettingsLogCheckBox.CheckedChanged += new System.EventHandler(this.BepInExSettingsLogCheckBox_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(65, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(11, 13);
            this.label2.TabIndex = 19;
            this.label2.Text = "-";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(49, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(15, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "+";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // BepinExHelpLinkLabel
            // 
            this.BepinExHelpLinkLabel.AutoSize = true;
            this.BepinExHelpLinkLabel.Location = new System.Drawing.Point(47, 0);
            this.BepinExHelpLinkLabel.Name = "BepinExHelpLinkLabel";
            this.BepinExHelpLinkLabel.Size = new System.Drawing.Size(12, 13);
            this.BepinExHelpLinkLabel.TabIndex = 17;
            this.BepinExHelpLinkLabel.TabStop = true;
            this.BepinExHelpLinkLabel.Text = "?";
            this.BepinExHelpLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.BepinExHelpLinkLabel_LinkClicked);
            // 
            // BepinExLogOpenConfigFileLinkLabel
            // 
            this.BepinExLogOpenConfigFileLinkLabel.AutoSize = true;
            this.BepinExLogOpenConfigFileLinkLabel.Location = new System.Drawing.Point(100, 160);
            this.BepinExLogOpenConfigFileLinkLabel.Name = "BepinExLogOpenConfigFileLinkLabel";
            this.BepinExLogOpenConfigFileLinkLabel.Size = new System.Drawing.Size(46, 13);
            this.BepinExLogOpenConfigFileLinkLabel.TabIndex = 16;
            this.BepinExLogOpenConfigFileLinkLabel.TabStop = true;
            this.BepinExLogOpenConfigFileLinkLabel.Text = "Settings";
            this.BepinExLogOpenConfigFileLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.BepinExLogOpenConfigFileLinkLabel_LinkClicked);
            // 
            // BepinExLogTargetLinkLabel
            // 
            this.BepinExLogTargetLinkLabel.AutoSize = true;
            this.BepinExLogTargetLinkLabel.Location = new System.Drawing.Point(47, 17);
            this.BepinExLogTargetLinkLabel.Name = "BepinExLogTargetLinkLabel";
            this.BepinExLogTargetLinkLabel.Size = new System.Drawing.Size(45, 13);
            this.BepinExLogTargetLinkLabel.TabIndex = 8;
            this.BepinExLogTargetLinkLabel.TabStop = true;
            this.BepinExLogTargetLinkLabel.Text = "Console";
            this.BepinExLogTargetLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.BepinExLogTargetLinkLabel_LinkClicked);
            // 
            // BepInExSettingsLogCheckedListBox
            // 
            this.BepInExSettingsLogCheckedListBox.BackColor = System.Drawing.Color.Gray;
            this.BepInExSettingsLogCheckedListBox.CheckOnClick = true;
            this.BepInExSettingsLogCheckedListBox.ForeColor = System.Drawing.SystemColors.Window;
            this.BepInExSettingsLogCheckedListBox.FormattingEnabled = true;
            this.BepInExSettingsLogCheckedListBox.Items.AddRange(new object[] {
            "Fatal",
            "Error",
            "Warning",
            "Message",
            "Info",
            "Debug"});
            this.BepInExSettingsLogCheckedListBox.Location = new System.Drawing.Point(3, 57);
            this.BepInExSettingsLogCheckedListBox.Name = "BepInExSettingsLogCheckedListBox";
            this.BepInExSettingsLogCheckedListBox.Size = new System.Drawing.Size(75, 100);
            this.BepInExSettingsLogCheckedListBox.TabIndex = 5;
            this.BepInExSettingsLogCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.BepInExSettingsLogCheckedListBox_ItemCheck);
            this.BepInExSettingsLogCheckedListBox.SelectedIndexChanged += new System.EventHandler(this.BepInExSettingsLogCheckedListBox_SelectedIndexChanged);
            // 
            // BepInExLogLevelsSourceLabel
            // 
            this.BepInExLogLevelsSourceLabel.AutoSize = true;
            this.BepInExLogLevelsSourceLabel.ForeColor = System.Drawing.Color.White;
            this.BepInExLogLevelsSourceLabel.Location = new System.Drawing.Point(6, 17);
            this.BepInExLogLevelsSourceLabel.Name = "BepInExLogLevelsSourceLabel";
            this.BepInExLogLevelsSourceLabel.Size = new System.Drawing.Size(37, 13);
            this.BepInExLogLevelsSourceLabel.TabIndex = 7;
            this.BepInExLogLevelsSourceLabel.Text = "target";
            // 
            // BepInExLogLevelsLabel
            // 
            this.BepInExLogLevelsLabel.AutoSize = true;
            this.BepInExLogLevelsLabel.ForeColor = System.Drawing.Color.White;
            this.BepInExLogLevelsLabel.Location = new System.Drawing.Point(6, 41);
            this.BepInExLogLevelsLabel.Name = "BepInExLogLevelsLabel";
            this.BepInExLogLevelsLabel.Size = new System.Drawing.Size(34, 13);
            this.BepInExLogLevelsLabel.TabIndex = 6;
            this.BepInExLogLevelsLabel.Text = "levels";
            // 
            // BepinExForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.ClientSize = new System.Drawing.Size(170, 180);
            this.Controls.Add(this.BepinExSettingsPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "BepinExForm";
            this.Text = "BepinExForm";
            this.Load += new System.EventHandler(this.BepinExForm_Load);
            this.BepinExSettingsPanel.ResumeLayout(false);
            this.BepInExSettingsGroupBox.ResumeLayout(false);
            this.BepInExSettingsGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel BepinExSettingsPanel;
        private System.Windows.Forms.GroupBox BepInExSettingsGroupBox;
        private System.Windows.Forms.LinkLabel BepinExLogTargetLinkLabel;
        private System.Windows.Forms.CheckedListBox BepInExSettingsLogCheckedListBox;
        private System.Windows.Forms.Label BepInExLogLevelsSourceLabel;
        private System.Windows.Forms.Label BepInExLogLevelsLabel;
        private System.Windows.Forms.LinkLabel BepinExLogOpenConfigFileLinkLabel;
        private System.Windows.Forms.LinkLabel BepinExHelpLinkLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox BepInExSettingsLogCheckBox;
        private System.Windows.Forms.LinkLabel OpenLogLinkLabel;
        private System.Windows.Forms.Label BepInExSettingsDisplayedLogLevelLabel;
    }
}