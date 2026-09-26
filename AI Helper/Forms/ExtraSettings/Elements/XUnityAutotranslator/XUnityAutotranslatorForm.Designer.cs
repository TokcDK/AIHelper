namespace AIHelper.Forms.ExtraSettings.Elements
{
    internal partial class XUnityAutotranslatorForm
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
            this.XUASettingsPanel = new System.Windows.Forms.Panel();
            this.XUAGroupBox = new System.Windows.Forms.GroupBox();
            this.XUASettingsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.XUALanguageLabel = new System.Windows.Forms.Label();
            this.XUAHelpLinkLabel = new System.Windows.Forms.LinkLabel();
            this.XUAFromLanguageComboBox = new System.Windows.Forms.ComboBox();
            this.XUAFromLanguageArrowLabel = new System.Windows.Forms.Label();
            this.XUALanguageComboBox = new System.Windows.Forms.ComboBox();
            this.XUALanguageArrowLabel = new System.Windows.Forms.Label();
            this.XUATranslateServiceLabel = new System.Windows.Forms.Label();
            this.XUAEndpointComboBox = new System.Windows.Forms.ComboBox();
            this.XUAcfgFileOpenLinkLabel = new System.Windows.Forms.LinkLabel();
            this.XUASettingsPanel.SuspendLayout();
            this.XUAGroupBox.SuspendLayout();
            this.XUASettingsTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // XUASettingsPanel
            // 
            this.XUASettingsPanel.Controls.Add(this.XUAGroupBox);
            this.XUASettingsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.XUASettingsPanel.Location = new System.Drawing.Point(0, 0);
            this.XUASettingsPanel.Margin = new System.Windows.Forms.Padding(0);
            this.XUASettingsPanel.Name = "XUASettingsPanel";
            this.XUASettingsPanel.Size = new System.Drawing.Size(380, 190);
            this.XUASettingsPanel.TabIndex = 0;
            // 
            // XUAGroupBox
            // 
            this.XUAGroupBox.Controls.Add(this.XUASettingsTableLayoutPanel);
            this.XUAGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.XUAGroupBox.Location = new System.Drawing.Point(0, 0);
            this.XUAGroupBox.Name = "XUAGroupBox";
            this.XUAGroupBox.Padding = new System.Windows.Forms.Padding(10, 6, 10, 10);
            this.XUAGroupBox.Size = new System.Drawing.Size(380, 190);
            this.XUAGroupBox.TabIndex = 0;
            this.XUAGroupBox.TabStop = false;
            this.XUAGroupBox.Text = "XUnity.AutoTranslator";
            // 
            // XUASettingsTableLayoutPanel
            // 
            // Column 0 holds the captions, column 1 the fields, and column 2 the small
            // controls which belong beside them: the two arrows and the help link.
            this.XUASettingsTableLayoutPanel.ColumnCount = 3;
            this.XUASettingsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.XUASettingsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.XUASettingsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.XUASettingsTableLayoutPanel.Controls.Add(this.XUALanguageLabel, 0, 0);
            this.XUASettingsTableLayoutPanel.Controls.Add(this.XUAHelpLinkLabel, 2, 0);
            this.XUASettingsTableLayoutPanel.Controls.Add(this.XUAFromLanguageComboBox, 0, 1);
            this.XUASettingsTableLayoutPanel.Controls.Add(this.XUAFromLanguageArrowLabel, 2, 1);
            this.XUASettingsTableLayoutPanel.Controls.Add(this.XUALanguageComboBox, 0, 2);
            this.XUASettingsTableLayoutPanel.Controls.Add(this.XUALanguageArrowLabel, 2, 2);
            this.XUASettingsTableLayoutPanel.Controls.Add(this.XUATranslateServiceLabel, 0, 3);
            this.XUASettingsTableLayoutPanel.Controls.Add(this.XUAEndpointComboBox, 0, 4);
            this.XUASettingsTableLayoutPanel.Controls.Add(this.XUAcfgFileOpenLinkLabel, 2, 6);
            this.XUASettingsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.XUASettingsTableLayoutPanel.Location = new System.Drawing.Point(10, 6);
            this.XUASettingsTableLayoutPanel.Name = "XUASettingsTableLayoutPanel";
            this.XUASettingsTableLayoutPanel.RowCount = 7;
            this.XUASettingsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.XUASettingsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.XUASettingsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.XUASettingsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.XUASettingsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.XUASettingsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.XUASettingsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.XUASettingsTableLayoutPanel.Size = new System.Drawing.Size(360, 174);
            this.XUASettingsTableLayoutPanel.TabIndex = 0;
            this.XUASettingsTableLayoutPanel.SetColumnSpan(this.XUALanguageLabel, 2);
            this.XUASettingsTableLayoutPanel.SetColumnSpan(this.XUAFromLanguageComboBox, 2);
            this.XUASettingsTableLayoutPanel.SetColumnSpan(this.XUALanguageComboBox, 2);
            this.XUASettingsTableLayoutPanel.SetColumnSpan(this.XUATranslateServiceLabel, 3);
            this.XUASettingsTableLayoutPanel.SetColumnSpan(this.XUAEndpointComboBox, 3);
            // 
            // XUALanguageLabel
            // 
            this.XUALanguageLabel.AutoSize = true;
            this.XUALanguageLabel.Location = new System.Drawing.Point(0, 0);
            this.XUALanguageLabel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.XUALanguageLabel.Name = "XUALanguageLabel";
            this.XUALanguageLabel.Size = new System.Drawing.Size(63, 15);
            this.XUALanguageLabel.TabIndex = 0;
            this.XUALanguageLabel.Text = "Language:";
            // 
            // XUAHelpLinkLabel
            // 
            this.XUAHelpLinkLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.XUAHelpLinkLabel.AutoSize = true;
            this.XUAHelpLinkLabel.Location = new System.Drawing.Point(347, 0);
            this.XUAHelpLinkLabel.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.XUAHelpLinkLabel.Name = "XUAHelpLinkLabel";
            this.XUAHelpLinkLabel.Size = new System.Drawing.Size(13, 15);
            this.XUAHelpLinkLabel.TabIndex = 1;
            this.XUAHelpLinkLabel.TabStop = true;
            this.XUAHelpLinkLabel.Text = "?";
            this.XUAHelpLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.XUAHelpLinkLabel_LinkClicked);
            // 
            // XUAFromLanguageComboBox
            // 
            // The language the text is translated from.
            this.XUAFromLanguageComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.XUAFromLanguageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.XUAFromLanguageComboBox.FormattingEnabled = true;
            this.XUAFromLanguageComboBox.Location = new System.Drawing.Point(0, 19);
            this.XUAFromLanguageComboBox.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.XUAFromLanguageComboBox.Name = "XUAFromLanguageComboBox";
            this.XUAFromLanguageComboBox.Size = new System.Drawing.Size(331, 23);
            this.XUAFromLanguageComboBox.TabIndex = 2;
            this.XUAFromLanguageComboBox.SelectedIndexChanged += new System.EventHandler(this.XUAFromLanguageComboBox_SelectedIndexChanged);
            // 
            // XUAFromLanguageArrowLabel
            // 
            // The two arrows read as one direction: out of the source language, into the
            // target language.
            this.XUAFromLanguageArrowLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.XUAFromLanguageArrowLabel.AutoSize = true;
            this.XUAFromLanguageArrowLabel.Location = new System.Drawing.Point(341, 22);
            this.XUAFromLanguageArrowLabel.Margin = new System.Windows.Forms.Padding(10, 0, 0, 6);
            this.XUAFromLanguageArrowLabel.Name = "XUAFromLanguageArrowLabel";
            this.XUAFromLanguageArrowLabel.Size = new System.Drawing.Size(19, 15);
            this.XUAFromLanguageArrowLabel.TabIndex = 3;
            this.XUAFromLanguageArrowLabel.Text = ">";
            // 
            // XUALanguageComboBox
            // 
            // The language the text is translated into.
            this.XUALanguageComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.XUALanguageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.XUALanguageComboBox.FormattingEnabled = true;
            this.XUALanguageComboBox.Location = new System.Drawing.Point(0, 48);
            this.XUALanguageComboBox.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.XUALanguageComboBox.Name = "XUALanguageComboBox";
            this.XUALanguageComboBox.Size = new System.Drawing.Size(331, 23);
            this.XUALanguageComboBox.TabIndex = 4;
            this.XUALanguageComboBox.SelectedIndexChanged += new System.EventHandler(this.XUALanguageComboBox_SelectedIndexChanged);
            // 
            // XUALanguageArrowLabel
            // 
            this.XUALanguageArrowLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.XUALanguageArrowLabel.AutoSize = true;
            this.XUALanguageArrowLabel.Location = new System.Drawing.Point(341, 51);
            this.XUALanguageArrowLabel.Margin = new System.Windows.Forms.Padding(10, 0, 0, 6);
            this.XUALanguageArrowLabel.Name = "XUALanguageArrowLabel";
            this.XUALanguageArrowLabel.Size = new System.Drawing.Size(19, 15);
            this.XUALanguageArrowLabel.TabIndex = 5;
            this.XUALanguageArrowLabel.Text = "<";
            // 
            // XUATranslateServiceLabel
            // 
            this.XUATranslateServiceLabel.AutoSize = true;
            this.XUATranslateServiceLabel.Location = new System.Drawing.Point(0, 85);
            this.XUATranslateServiceLabel.Margin = new System.Windows.Forms.Padding(0, 8, 0, 4);
            this.XUATranslateServiceLabel.Name = "XUATranslateServiceLabel";
            this.XUATranslateServiceLabel.Size = new System.Drawing.Size(102, 15);
            this.XUATranslateServiceLabel.TabIndex = 6;
            this.XUATranslateServiceLabel.Text = "Translate service:";
            // 
            // XUAEndpointComboBox
            // 
            this.XUAEndpointComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.XUAEndpointComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.XUAEndpointComboBox.FormattingEnabled = true;
            this.XUAEndpointComboBox.Location = new System.Drawing.Point(0, 104);
            this.XUAEndpointComboBox.Margin = new System.Windows.Forms.Padding(0);
            this.XUAEndpointComboBox.Name = "XUAEndpointComboBox";
            this.XUAEndpointComboBox.Size = new System.Drawing.Size(360, 23);
            this.XUAEndpointComboBox.TabIndex = 7;
            this.XUAEndpointComboBox.SelectedIndexChanged += new System.EventHandler(this.XUAEndpointComboBox_SelectedIndexChanged);
            // 
            // XUAcfgFileOpenLinkLabel
            // 
            this.XUAcfgFileOpenLinkLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.XUAcfgFileOpenLinkLabel.AutoSize = true;
            this.XUAcfgFileOpenLinkLabel.Location = new System.Drawing.Point(307, 159);
            this.XUAcfgFileOpenLinkLabel.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.XUAcfgFileOpenLinkLabel.Name = "XUAcfgFileOpenLinkLabel";
            this.XUAcfgFileOpenLinkLabel.Size = new System.Drawing.Size(53, 15);
            this.XUAcfgFileOpenLinkLabel.TabIndex = 8;
            this.XUAcfgFileOpenLinkLabel.TabStop = true;
            this.XUAcfgFileOpenLinkLabel.Text = "Settings";
            this.XUAcfgFileOpenLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.XUAcfgFileOpenLinkLabel_LinkClicked);
            // 
            // XUnityAutotranslatorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(380, 190);
            this.Controls.Add(this.XUASettingsPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "XUnityAutotranslatorForm";
            this.Text = "XunityAutotranslator";
            this.Load += new System.EventHandler(this.XUnityAutotranslatorForm_Load);
            this.XUASettingsPanel.ResumeLayout(false);
            this.XUAGroupBox.ResumeLayout(false);
            this.XUASettingsTableLayoutPanel.ResumeLayout(false);
            this.XUASettingsTableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox XUAGroupBox;
        private System.Windows.Forms.TableLayoutPanel XUASettingsTableLayoutPanel;
        private System.Windows.Forms.LinkLabel XUAHelpLinkLabel;
        private System.Windows.Forms.LinkLabel XUAcfgFileOpenLinkLabel;
        private System.Windows.Forms.ComboBox XUAEndpointComboBox;
        private System.Windows.Forms.Label XUATranslateServiceLabel;
        private System.Windows.Forms.Label XUAFromLanguageArrowLabel;
        private System.Windows.Forms.Label XUALanguageArrowLabel;
        private System.Windows.Forms.ComboBox XUALanguageComboBox;
        private System.Windows.Forms.Label XUALanguageLabel;
        private System.Windows.Forms.ComboBox XUAFromLanguageComboBox;
        private System.Windows.Forms.Panel XUASettingsPanel;
    }
}
