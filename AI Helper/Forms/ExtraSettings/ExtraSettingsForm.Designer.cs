namespace AIHelper
{
    internal partial class ExtraSettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExtraSettingsForm));
            this.ExtraSettingsPanel = new System.Windows.Forms.Panel();
            this.ExtraSettingsFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.ExtraSettingsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ExtraSettingsPanel
            // 
            this.ExtraSettingsPanel.Controls.Add(this.ExtraSettingsFlowLayoutPanel);
            this.ExtraSettingsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExtraSettingsPanel.Location = new System.Drawing.Point(0, 0);
            this.ExtraSettingsPanel.Name = "ExtraSettingsPanel";
            this.ExtraSettingsPanel.Size = new System.Drawing.Size(354, 187);
            this.ExtraSettingsPanel.TabIndex = 0;
            // 
            // ExtraSettingsFlowLayoutPanel
            // 
            this.ExtraSettingsFlowLayoutPanel.AutoScroll = true;
            this.ExtraSettingsFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExtraSettingsFlowLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.ExtraSettingsFlowLayoutPanel.Name = "ExtraSettingsFlowLayoutPanel";
            this.ExtraSettingsFlowLayoutPanel.Size = new System.Drawing.Size(354, 187);
            this.ExtraSettingsFlowLayoutPanel.TabIndex = 0;
            // 
            // ExtraSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.ClientSize = new System.Drawing.Size(354, 187);
            this.Controls.Add(this.ExtraSettingsPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ExtraSettings";
            this.Text = "Extra";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ExtraSettings_FormClosing);
            this.Load += new System.EventHandler(this.ExtraSettings_Load);
            this.ExtraSettingsPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel ExtraSettingsPanel;
        private System.Windows.Forms.FlowLayoutPanel ExtraSettingsFlowLayoutPanel;
    }
}