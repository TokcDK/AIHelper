namespace AI_Helper
{
    partial class ExtraSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExtraSettings));
            this.XUAGroupBox = new System.Windows.Forms.GroupBox();
            this.XUAcfgFileOpenLinkLabel = new System.Windows.Forms.LinkLabel();
            this.XUAEndpointComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.XUALanguageComboBox = new System.Windows.Forms.ComboBox();
            this.XUALanguageLabel = new System.Windows.Forms.Label();
            this.XUAFromLanguageComboBox = new System.Windows.Forms.ComboBox();
            this.XUAGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // XUAGroupBox
            // 
            this.XUAGroupBox.Controls.Add(this.XUAcfgFileOpenLinkLabel);
            this.XUAGroupBox.Controls.Add(this.XUAEndpointComboBox);
            this.XUAGroupBox.Controls.Add(this.label3);
            this.XUAGroupBox.Controls.Add(this.label2);
            this.XUAGroupBox.Controls.Add(this.label1);
            this.XUAGroupBox.Controls.Add(this.XUALanguageComboBox);
            this.XUAGroupBox.Controls.Add(this.XUALanguageLabel);
            this.XUAGroupBox.Controls.Add(this.XUAFromLanguageComboBox);
            this.XUAGroupBox.ForeColor = System.Drawing.Color.White;
            this.XUAGroupBox.Location = new System.Drawing.Point(12, 12);
            this.XUAGroupBox.Name = "XUAGroupBox";
            this.XUAGroupBox.Size = new System.Drawing.Size(152, 163);
            this.XUAGroupBox.TabIndex = 0;
            this.XUAGroupBox.TabStop = false;
            this.XUAGroupBox.Text = "XUnity.AutoTranslator";
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
            this.XUAEndpointComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.XUAEndpointComboBox.FormattingEnabled = true;
            this.XUAEndpointComboBox.Location = new System.Drawing.Point(6, 100);
            this.XUAEndpointComboBox.Name = "XUAEndpointComboBox";
            this.XUAEndpointComboBox.Size = new System.Drawing.Size(140, 21);
            this.XUAEndpointComboBox.TabIndex = 14;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(4, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Translate service:";
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
            this.XUALanguageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.XUALanguageComboBox.FormattingEnabled = true;
            this.XUALanguageComboBox.Location = new System.Drawing.Point(6, 60);
            this.XUALanguageComboBox.Name = "XUALanguageComboBox";
            this.XUALanguageComboBox.Size = new System.Drawing.Size(121, 21);
            this.XUALanguageComboBox.TabIndex = 10;
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
            this.XUAFromLanguageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.XUAFromLanguageComboBox.FormattingEnabled = true;
            this.XUAFromLanguageComboBox.Location = new System.Drawing.Point(6, 33);
            this.XUAFromLanguageComboBox.Name = "XUAFromLanguageComboBox";
            this.XUAFromLanguageComboBox.Size = new System.Drawing.Size(121, 21);
            this.XUAFromLanguageComboBox.TabIndex = 8;
            // 
            // ExtraSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.ClientSize = new System.Drawing.Size(354, 187);
            this.Controls.Add(this.XUAGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ExtraSettings";
            this.Text = "Extra";
            this.Load += new System.EventHandler(this.ExtraSettings_Load);
            this.XUAGroupBox.ResumeLayout(false);
            this.XUAGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox XUAGroupBox;
        private System.Windows.Forms.Label XUALanguageLabel;
        private System.Windows.Forms.ComboBox XUAFromLanguageComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox XUALanguageComboBox;
        private System.Windows.Forms.ComboBox XUAEndpointComboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.LinkLabel XUAcfgFileOpenLinkLabel;
    }
}