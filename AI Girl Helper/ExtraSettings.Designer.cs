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
            this.CurrentGameComboBox = new System.Windows.Forms.ComboBox();
            this.XUALanguageLabel = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.XUAcfgFileOpenLinkLabel = new System.Windows.Forms.LinkLabel();
            this.XUAGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // XUAGroupBox
            // 
            this.XUAGroupBox.Controls.Add(this.XUAcfgFileOpenLinkLabel);
            this.XUAGroupBox.Controls.Add(this.comboBox2);
            this.XUAGroupBox.Controls.Add(this.label3);
            this.XUAGroupBox.Controls.Add(this.label2);
            this.XUAGroupBox.Controls.Add(this.label1);
            this.XUAGroupBox.Controls.Add(this.comboBox1);
            this.XUAGroupBox.Controls.Add(this.XUALanguageLabel);
            this.XUAGroupBox.Controls.Add(this.CurrentGameComboBox);
            this.XUAGroupBox.ForeColor = System.Drawing.Color.White;
            this.XUAGroupBox.Location = new System.Drawing.Point(12, 12);
            this.XUAGroupBox.Name = "XUAGroupBox";
            this.XUAGroupBox.Size = new System.Drawing.Size(152, 163);
            this.XUAGroupBox.TabIndex = 0;
            this.XUAGroupBox.TabStop = false;
            this.XUAGroupBox.Text = "XUnity.AutoTranslator";
            // 
            // CurrentGameComboBox
            // 
            this.CurrentGameComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CurrentGameComboBox.FormattingEnabled = true;
            this.CurrentGameComboBox.Location = new System.Drawing.Point(6, 33);
            this.CurrentGameComboBox.Name = "CurrentGameComboBox";
            this.CurrentGameComboBox.Size = new System.Drawing.Size(121, 21);
            this.CurrentGameComboBox.TabIndex = 8;
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
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(6, 60);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 21);
            this.comboBox1.TabIndex = 10;
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
            // comboBox2
            // 
            this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(6, 100);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(121, 21);
            this.comboBox2.TabIndex = 14;
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
        private System.Windows.Forms.ComboBox CurrentGameComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.LinkLabel XUAcfgFileOpenLinkLabel;
    }
}