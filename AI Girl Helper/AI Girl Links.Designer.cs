namespace AI_Girl_Helper
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
            this.LinksCharactersGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BooruLinkLabel
            // 
            this.BooruLinkLabel.AutoSize = true;
            this.BooruLinkLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.BooruLinkLabel.Location = new System.Drawing.Point(6, 17);
            this.BooruLinkLabel.Name = "BooruLinkLabel";
            this.BooruLinkLabel.Size = new System.Drawing.Size(100, 13);
            this.BooruLinkLabel.TabIndex = 0;
            this.BooruLinkLabel.TabStop = true;
            this.BooruLinkLabel.Text = "Booru Illusion cards";
            this.BooruLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.BooruLinkLabel_LinkClicked);
            // 
            // LinksCharactersGroupBox
            // 
            this.LinksCharactersGroupBox.Controls.Add(this.BooruLinkLabel);
            this.LinksCharactersGroupBox.ForeColor = System.Drawing.Color.Black;
            this.LinksCharactersGroupBox.Location = new System.Drawing.Point(12, 12);
            this.LinksCharactersGroupBox.Name = "LinksCharactersGroupBox";
            this.LinksCharactersGroupBox.Size = new System.Drawing.Size(113, 114);
            this.LinksCharactersGroupBox.TabIndex = 1;
            this.LinksCharactersGroupBox.TabStop = false;
            this.LinksCharactersGroupBox.Text = "Characters";
            // 
            // LinksForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.ClientSize = new System.Drawing.Size(354, 138);
            this.Controls.Add(this.LinksCharactersGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LinksForm";
            this.Text = "Links";
            this.LinksCharactersGroupBox.ResumeLayout(false);
            this.LinksCharactersGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.LinkLabel BooruLinkLabel;
        private System.Windows.Forms.GroupBox LinksCharactersGroupBox;
    }
}