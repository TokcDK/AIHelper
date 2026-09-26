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
            this.ExtraSettingsPanel.Margin = new System.Windows.Forms.Padding(0);
            this.ExtraSettingsPanel.Name = "ExtraSettingsPanel";
            this.ExtraSettingsPanel.Padding = new System.Windows.Forms.Padding(12);
            this.ExtraSettingsPanel.Size = new System.Drawing.Size(420, 440);
            this.ExtraSettingsPanel.TabIndex = 0;
            // 
            // ExtraSettingsFlowLayoutPanel
            // 
            // The settings panels of the found plugins are docked in here while the
            // window is shown, so the panel scrolls rather than clips when there are
            // more of them than fit.
            this.ExtraSettingsFlowLayoutPanel.AutoScroll = true;
            this.ExtraSettingsFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExtraSettingsFlowLayoutPanel.Location = new System.Drawing.Point(12, 12);
            this.ExtraSettingsFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.ExtraSettingsFlowLayoutPanel.Name = "ExtraSettingsFlowLayoutPanel";
            this.ExtraSettingsFlowLayoutPanel.Size = new System.Drawing.Size(396, 416);
            this.ExtraSettingsFlowLayoutPanel.TabIndex = 0;
            // 
            // ExtraSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(420, 440);
            this.Controls.Add(this.ExtraSettingsPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(420, 300);
            this.Name = "ExtraSettings";
            this.Text = "Extra";
            this.Load += new System.EventHandler(this.ExtraSettings_Load);
            this.ExtraSettingsPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel ExtraSettingsPanel;
        private System.Windows.Forms.FlowLayoutPanel ExtraSettingsFlowLayoutPanel;
    }
}
