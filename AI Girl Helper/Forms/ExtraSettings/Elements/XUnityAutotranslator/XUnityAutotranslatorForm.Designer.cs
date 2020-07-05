namespace AIHelper.Forms.ExtraSettings.Elements
{
    partial class XUnityAutotranslatorForm
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
            this.XUAGroupBox = new System.Windows.Forms.GroupBox();
            this.XUAHelpLinkLabel = new System.Windows.Forms.LinkLabel();
            this.XUAcfgFileOpenLinkLabel = new System.Windows.Forms.LinkLabel();
            this.XUAEndpointComboBox = new System.Windows.Forms.ComboBox();
            this.XUATranslateServiceLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.XUALanguageComboBox = new System.Windows.Forms.ComboBox();
            this.XUALanguageLabel = new System.Windows.Forms.Label();
            this.XUAFromLanguageComboBox = new System.Windows.Forms.ComboBox();
            this.XUASettingsPanel = new System.Windows.Forms.Panel();
            this.XUAGroupBox.SuspendLayout();
            this.XUASettingsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // XUAGroupBox
            // 
            this.XUAGroupBox.Controls.Add(this.XUAHelpLinkLabel);
            this.XUAGroupBox.Controls.Add(this.XUAcfgFileOpenLinkLabel);
            this.XUAGroupBox.Controls.Add(this.XUAEndpointComboBox);
            this.XUAGroupBox.Controls.Add(this.XUATranslateServiceLabel);
            this.XUAGroupBox.Controls.Add(this.label2);
            this.XUAGroupBox.Controls.Add(this.label1);
            this.XUAGroupBox.Controls.Add(this.XUALanguageComboBox);
            this.XUAGroupBox.Controls.Add(this.XUALanguageLabel);
            this.XUAGroupBox.Controls.Add(this.XUAFromLanguageComboBox);
            this.XUAGroupBox.ForeColor = System.Drawing.Color.White;
            this.XUAGroupBox.Location = new System.Drawing.Point(0, 0);
            this.XUAGroupBox.Name = "XUAGroupBox";
            this.XUAGroupBox.Size = new System.Drawing.Size(152, 163);
            this.XUAGroupBox.TabIndex = 2;
            this.XUAGroupBox.TabStop = false;
            this.XUAGroupBox.Text = "XUnity.AutoTranslator";
            // 
            // XUAHelpLinkLabel
            // 
            this.XUAHelpLinkLabel.AutoSize = true;
            this.XUAHelpLinkLabel.Location = new System.Drawing.Point(115, 0);
            this.XUAHelpLinkLabel.Name = "XUAHelpLinkLabel";
            this.XUAHelpLinkLabel.Size = new System.Drawing.Size(12, 13);
            this.XUAHelpLinkLabel.TabIndex = 16;
            this.XUAHelpLinkLabel.TabStop = true;
            this.XUAHelpLinkLabel.Text = "?";
            this.XUAHelpLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.XUAHelpLinkLabel_LinkClicked);
            // 
            // XUAcfgFileOpenLinkLabel
            // 
            this.XUAcfgFileOpenLinkLabel.AutoSize = true;
            this.XUAcfgFileOpenLinkLabel.Location = new System.Drawing.Point(3, 134);
            this.XUAcfgFileOpenLinkLabel.Name = "XUAcfgFileOpenLinkLabel";
            this.XUAcfgFileOpenLinkLabel.Size = new System.Drawing.Size(124, 13);
            this.XUAcfgFileOpenLinkLabel.TabIndex = 15;
            this.XUAcfgFileOpenLinkLabel.TabStop = true;
            this.XUAcfgFileOpenLinkLabel.Text = "AutoTranslatorConfig.ini";
            this.XUAcfgFileOpenLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.XUAcfgFileOpenLinkLabel_LinkClicked);
            // 
            // XUAEndpointComboBox
            // 
            this.XUAEndpointComboBox.Cursor = System.Windows.Forms.Cursors.PanSouth;
            this.XUAEndpointComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.XUAEndpointComboBox.FormattingEnabled = true;
            this.XUAEndpointComboBox.Location = new System.Drawing.Point(6, 100);
            this.XUAEndpointComboBox.Name = "XUAEndpointComboBox";
            this.XUAEndpointComboBox.Size = new System.Drawing.Size(140, 21);
            this.XUAEndpointComboBox.TabIndex = 14;
            this.XUAEndpointComboBox.SelectedIndexChanged += new System.EventHandler(this.XUAEndpointComboBox_SelectedIndexChanged);
            // 
            // XUATranslateServiceLabel
            // 
            this.XUATranslateServiceLabel.AutoSize = true;
            this.XUATranslateServiceLabel.ForeColor = System.Drawing.Color.Black;
            this.XUATranslateServiceLabel.Location = new System.Drawing.Point(4, 84);
            this.XUATranslateServiceLabel.Name = "XUATranslateServiceLabel";
            this.XUATranslateServiceLabel.Size = new System.Drawing.Size(93, 13);
            this.XUATranslateServiceLabel.TabIndex = 13;
            this.XUATranslateServiceLabel.Text = "Translate service:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(133, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(15, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "<";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(133, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(15, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = ">";
            // 
            // XUALanguageComboBox
            // 
            this.XUALanguageComboBox.Cursor = System.Windows.Forms.Cursors.PanSouth;
            this.XUALanguageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.XUALanguageComboBox.FormattingEnabled = true;
            this.XUALanguageComboBox.Location = new System.Drawing.Point(6, 60);
            this.XUALanguageComboBox.Name = "XUALanguageComboBox";
            this.XUALanguageComboBox.Size = new System.Drawing.Size(121, 21);
            this.XUALanguageComboBox.TabIndex = 10;
            this.XUALanguageComboBox.SelectedIndexChanged += new System.EventHandler(this.XUALanguageComboBox_SelectedIndexChanged);
            // 
            // XUALanguageLabel
            // 
            this.XUALanguageLabel.AutoSize = true;
            this.XUALanguageLabel.ForeColor = System.Drawing.Color.Black;
            this.XUALanguageLabel.Location = new System.Drawing.Point(6, 17);
            this.XUALanguageLabel.Name = "XUALanguageLabel";
            this.XUALanguageLabel.Size = new System.Drawing.Size(58, 13);
            this.XUALanguageLabel.TabIndex = 9;
            this.XUALanguageLabel.Text = "Language:";
            // 
            // XUAFromLanguageComboBox
            // 
            this.XUAFromLanguageComboBox.Cursor = System.Windows.Forms.Cursors.PanSouth;
            this.XUAFromLanguageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.XUAFromLanguageComboBox.FormattingEnabled = true;
            this.XUAFromLanguageComboBox.Location = new System.Drawing.Point(6, 33);
            this.XUAFromLanguageComboBox.Name = "XUAFromLanguageComboBox";
            this.XUAFromLanguageComboBox.Size = new System.Drawing.Size(121, 21);
            this.XUAFromLanguageComboBox.TabIndex = 8;
            this.XUAFromLanguageComboBox.SelectedIndexChanged += new System.EventHandler(this.XUAFromLanguageComboBox_SelectedIndexChanged);
            // 
            // XUASettingsPanel
            // 
            this.XUASettingsPanel.Controls.Add(this.XUAGroupBox);
            this.XUASettingsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.XUASettingsPanel.Location = new System.Drawing.Point(0, 0);
            this.XUASettingsPanel.Name = "XUASettingsPanel";
            this.XUASettingsPanel.Size = new System.Drawing.Size(152, 163);
            this.XUASettingsPanel.TabIndex = 2;
            // 
            // XUnityAutotranslatorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.ClientSize = new System.Drawing.Size(152, 163);
            this.Controls.Add(this.XUASettingsPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "XUnityAutotranslatorForm";
            this.Text = "XunityAutotranslator";
            this.Load += new System.EventHandler(this.XUnityAutotranslatorForm_Load);
            this.XUAGroupBox.ResumeLayout(false);
            this.XUAGroupBox.PerformLayout();
            this.XUASettingsPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox XUAGroupBox;
        private System.Windows.Forms.LinkLabel XUAHelpLinkLabel;
        private System.Windows.Forms.LinkLabel XUAcfgFileOpenLinkLabel;
        private System.Windows.Forms.ComboBox XUAEndpointComboBox;
        private System.Windows.Forms.Label XUATranslateServiceLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox XUALanguageComboBox;
        private System.Windows.Forms.Label XUALanguageLabel;
        private System.Windows.Forms.ComboBox XUAFromLanguageComboBox;
        private System.Windows.Forms.Panel XUASettingsPanel;
    }
}