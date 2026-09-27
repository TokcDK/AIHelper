namespace AIHelper.Forms
{
    internal partial class LogLevelselectionForm
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
            this.LevelsFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.checkBox1 = new AIHelper.Manage.ui.ThemedCheckBox();
            this.checkBox2 = new AIHelper.Manage.ui.ThemedCheckBox();
            this.checkBox3 = new AIHelper.Manage.ui.ThemedCheckBox();
            this.checkBox4 = new AIHelper.Manage.ui.ThemedCheckBox();
            this.LevelsFlowLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // LevelsFlowLayoutPanel
            // 
            // The levels are a list, so they are laid out from the top down and the
            // panel scrolls when there are more of them than fit.
            this.LevelsFlowLayoutPanel.AutoScroll = true;
            this.LevelsFlowLayoutPanel.Controls.Add(this.checkBox1);
            this.LevelsFlowLayoutPanel.Controls.Add(this.checkBox2);
            this.LevelsFlowLayoutPanel.Controls.Add(this.checkBox3);
            this.LevelsFlowLayoutPanel.Controls.Add(this.checkBox4);
            this.LevelsFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LevelsFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.LevelsFlowLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.LevelsFlowLayoutPanel.Name = "LevelsFlowLayoutPanel";
            this.LevelsFlowLayoutPanel.Padding = new System.Windows.Forms.Padding(12);
            this.LevelsFlowLayoutPanel.Size = new System.Drawing.Size(170, 150);
            this.LevelsFlowLayoutPanel.TabIndex = 0;
            this.LevelsFlowLayoutPanel.WrapContents = false;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(12, 12);
            this.checkBox1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(41, 19);
            this.checkBox1.TabIndex = 0;
            this.checkBox1.Text = "all";
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(12, 39);
            this.checkBox2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(62, 19);
            this.checkBox2.TabIndex = 1;
            this.checkBox2.Text = "debug";
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(12, 66);
            this.checkBox3.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(47, 19);
            this.checkBox3.TabIndex = 2;
            this.checkBox3.Text = "info";
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Location = new System.Drawing.Point(12, 93);
            this.checkBox4.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(43, 19);
            this.checkBox4.TabIndex = 3;
            this.checkBox4.Text = "bla";
            // 
            // LogLevelselectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(170, 150);
            this.Controls.Add(this.LevelsFlowLayoutPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "LogLevelselectionForm";
            this.Text = "LogLevelselectionForm";
            this.LevelsFlowLayoutPanel.ResumeLayout(false);
            this.LevelsFlowLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel LevelsFlowLayoutPanel;
        private AIHelper.Manage.ui.ThemedCheckBox checkBox1;
        private AIHelper.Manage.ui.ThemedCheckBox checkBox2;
        private AIHelper.Manage.ui.ThemedCheckBox checkBox3;
        private AIHelper.Manage.ui.ThemedCheckBox checkBox4;
    }
}
