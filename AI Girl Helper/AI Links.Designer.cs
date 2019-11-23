namespace AI_Helper
{
    partial class LinksForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LinksForm));
            this.BooruLinkLabel = new System.Windows.Forms.LinkLabel();
            this.LinksCharactersGroupBox = new System.Windows.Forms.GroupBox();
            this.KenzatoLinkLabel = new System.Windows.Forms.LinkLabel();
            this.IllusionDiscordLinkLabel = new System.Windows.Forms.LinkLabel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.IllusionOficialUploaderLinkLabel = new System.Windows.Forms.LinkLabel();
            this.LinksCharactersGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BooruLinkLabel
            // 
            this.BooruLinkLabel.AutoSize = true;
            this.BooruLinkLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.BooruLinkLabel.Location = new System.Drawing.Point(6, 17);
            this.BooruLinkLabel.Name = "BooruLinkLabel";
            this.BooruLinkLabel.Size = new System.Drawing.Size(35, 13);
            this.BooruLinkLabel.TabIndex = 0;
            this.BooruLinkLabel.TabStop = true;
            this.BooruLinkLabel.Text = "Booru";
            this.BooruLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.BooruLinkLabel_LinkClicked);
            // 
            // LinksCharactersGroupBox
            // 
            this.LinksCharactersGroupBox.Controls.Add(this.KenzatoLinkLabel);
            this.LinksCharactersGroupBox.Controls.Add(this.IllusionDiscordLinkLabel);
            this.LinksCharactersGroupBox.Controls.Add(this.BooruLinkLabel);
            this.LinksCharactersGroupBox.ForeColor = System.Drawing.Color.Black;
            this.LinksCharactersGroupBox.Location = new System.Drawing.Point(12, 12);
            this.LinksCharactersGroupBox.Name = "LinksCharactersGroupBox";
            this.LinksCharactersGroupBox.Size = new System.Drawing.Size(98, 83);
            this.LinksCharactersGroupBox.TabIndex = 1;
            this.LinksCharactersGroupBox.TabStop = false;
            this.LinksCharactersGroupBox.Text = "Characters";
            // 
            // KenzatoLinkLabel
            // 
            this.KenzatoLinkLabel.AutoSize = true;
            this.KenzatoLinkLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.KenzatoLinkLabel.Location = new System.Drawing.Point(6, 30);
            this.KenzatoLinkLabel.Name = "KenzatoLinkLabel";
            this.KenzatoLinkLabel.Size = new System.Drawing.Size(46, 13);
            this.KenzatoLinkLabel.TabIndex = 2;
            this.KenzatoLinkLabel.TabStop = true;
            this.KenzatoLinkLabel.Text = "Kenzato";
            this.KenzatoLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.KenzatoLinkLabel_LinkClicked);
            // 
            // IllusionDiscordLinkLabel
            // 
            this.IllusionDiscordLinkLabel.AutoSize = true;
            this.IllusionDiscordLinkLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.IllusionDiscordLinkLabel.Location = new System.Drawing.Point(6, 43);
            this.IllusionDiscordLinkLabel.Name = "IllusionDiscordLinkLabel";
            this.IllusionDiscordLinkLabel.Size = new System.Drawing.Size(78, 13);
            this.IllusionDiscordLinkLabel.TabIndex = 1;
            this.IllusionDiscordLinkLabel.TabStop = true;
            this.IllusionDiscordLinkLabel.Text = "Illusion Discord";
            this.IllusionDiscordLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.IllusionDiscordLinkLabel_LinkClicked);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(13, 102);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(84, 13);
            this.linkLabel1.TabIndex = 2;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Mods (Pastebin)";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel1_LinkClicked);
            // 
            // IllusionOficialUploaderLinkLabel
            // 
            this.IllusionOficialUploaderLinkLabel.AutoSize = true;
            this.IllusionOficialUploaderLinkLabel.Location = new System.Drawing.Point(13, 119);
            this.IllusionOficialUploaderLinkLabel.Name = "IllusionOficialUploaderLinkLabel";
            this.IllusionOficialUploaderLinkLabel.Size = new System.Drawing.Size(146, 13);
            this.IllusionOficialUploaderLinkLabel.TabIndex = 3;
            this.IllusionOficialUploaderLinkLabel.TabStop = true;
            this.IllusionOficialUploaderLinkLabel.Text = "IllusionOficialUploader(Mega)";
            this.IllusionOficialUploaderLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.IllusionOficialUploader_LinkClicked);
            // 
            // LinksForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.ClientSize = new System.Drawing.Size(354, 138);
            this.Controls.Add(this.IllusionOficialUploaderLinkLabel);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.LinksCharactersGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LinksForm";
            this.Text = "Links";
            this.LinksCharactersGroupBox.ResumeLayout(false);
            this.LinksCharactersGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.LinkLabel BooruLinkLabel;
        private System.Windows.Forms.GroupBox LinksCharactersGroupBox;
        private System.Windows.Forms.LinkLabel IllusionDiscordLinkLabel;
        private System.Windows.Forms.LinkLabel KenzatoLinkLabel;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.LinkLabel IllusionOficialUploaderLinkLabel;
    }
}