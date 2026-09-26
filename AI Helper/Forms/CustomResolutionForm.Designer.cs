namespace AIHelper.Forms
{
    internal partial class CustomResolutionForm
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
            this.MainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.PromptLabel = new System.Windows.Forms.Label();
            this.HeightTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.HeightLabel = new System.Windows.Forms.Label();
            this.HeightTextBox = new System.Windows.Forms.TextBox();
            this.WidthTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.WidthLabel = new System.Windows.Forms.Label();
            this.WidthTextBox = new System.Windows.Forms.TextBox();
            this.ButtonsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.ApplyButton = new System.Windows.Forms.Button();
            this.MainTableLayoutPanel.SuspendLayout();
            this.HeightTableLayoutPanel.SuspendLayout();
            this.WidthTableLayoutPanel.SuspendLayout();
            this.ButtonsTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTableLayoutPanel
            // 
            // Every row is as tall as what it holds, so the dialog is as tall as the
            // question, the two fields and the button need and no taller.
            this.MainTableLayoutPanel.ColumnCount = 1;
            this.MainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainTableLayoutPanel.Controls.Add(this.PromptLabel, 0, 0);
            this.MainTableLayoutPanel.Controls.Add(this.HeightTableLayoutPanel, 0, 1);
            this.MainTableLayoutPanel.Controls.Add(this.WidthTableLayoutPanel, 0, 2);
            this.MainTableLayoutPanel.Controls.Add(this.ButtonsTableLayoutPanel, 0, 3);
            this.MainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainTableLayoutPanel.Location = new System.Drawing.Point(12, 12);
            this.MainTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.MainTableLayoutPanel.Name = "MainTableLayoutPanel";
            this.MainTableLayoutPanel.RowCount = 4;
            this.MainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.MainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.MainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.MainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.MainTableLayoutPanel.Size = new System.Drawing.Size(236, 166);
            this.MainTableLayoutPanel.TabIndex = 0;
            // 
            // PromptLabel
            // 
            this.PromptLabel.AutoSize = true;
            this.PromptLabel.Location = new System.Drawing.Point(0, 0);
            this.PromptLabel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.PromptLabel.Name = "PromptLabel";
            this.PromptLabel.Size = new System.Drawing.Size(205, 15);
            this.PromptLabel.TabIndex = 0;
            this.PromptLabel.Text = "Please input your custom resolution.";
            // 
            // HeightTableLayoutPanel
            // 
            this.HeightTableLayoutPanel.ColumnCount = 2;
            this.HeightTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.HeightTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.HeightTableLayoutPanel.Controls.Add(this.HeightLabel, 0, 0);
            this.HeightTableLayoutPanel.Controls.Add(this.HeightTextBox, 1, 0);
            this.HeightTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HeightTableLayoutPanel.Location = new System.Drawing.Point(0, 27);
            this.HeightTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.HeightTableLayoutPanel.Name = "HeightTableLayoutPanel";
            this.HeightTableLayoutPanel.RowCount = 1;
            this.HeightTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.HeightTableLayoutPanel.Size = new System.Drawing.Size(236, 29);
            this.HeightTableLayoutPanel.TabIndex = 1;
            // 
            // HeightLabel
            // 
            this.HeightLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.HeightLabel.AutoSize = true;
            this.HeightLabel.Location = new System.Drawing.Point(0, 7);
            this.HeightLabel.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.HeightLabel.Name = "HeightLabel";
            this.HeightLabel.Size = new System.Drawing.Size(46, 15);
            this.HeightLabel.TabIndex = 0;
            this.HeightLabel.Text = "Height:";
            // 
            // HeightTextBox
            // 
            this.HeightTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.HeightTextBox.Location = new System.Drawing.Point(58, 0);
            this.HeightTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.HeightTextBox.Name = "HeightTextBox";
            this.HeightTextBox.Size = new System.Drawing.Size(178, 23);
            this.HeightTextBox.TabIndex = 1;
            this.HeightTextBox.Text = "1024";
            // 
            // WidthTableLayoutPanel
            // 
            this.WidthTableLayoutPanel.ColumnCount = 2;
            this.WidthTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.WidthTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.WidthTableLayoutPanel.Controls.Add(this.WidthLabel, 0, 0);
            this.WidthTableLayoutPanel.Controls.Add(this.WidthTextBox, 1, 0);
            this.WidthTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WidthTableLayoutPanel.Location = new System.Drawing.Point(0, 64);
            this.WidthTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.WidthTableLayoutPanel.Name = "WidthTableLayoutPanel";
            this.WidthTableLayoutPanel.RowCount = 1;
            this.WidthTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.WidthTableLayoutPanel.Size = new System.Drawing.Size(236, 29);
            this.WidthTableLayoutPanel.TabIndex = 2;
            // 
            // WidthLabel
            // 
            this.WidthLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.WidthLabel.AutoSize = true;
            this.WidthLabel.Location = new System.Drawing.Point(0, 7);
            this.WidthLabel.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.WidthLabel.Name = "WidthLabel";
            this.WidthLabel.Size = new System.Drawing.Size(42, 15);
            this.WidthLabel.TabIndex = 0;
            this.WidthLabel.Text = "Width:";
            // 
            // WidthTextBox
            // 
            this.WidthTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.WidthTextBox.Location = new System.Drawing.Point(54, 0);
            this.WidthTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.WidthTextBox.Name = "WidthTextBox";
            this.WidthTextBox.Size = new System.Drawing.Size(182, 23);
            this.WidthTextBox.TabIndex = 1;
            this.WidthTextBox.Text = "768";
            // 
            // ButtonsTableLayoutPanel
            // 
            this.ButtonsTableLayoutPanel.ColumnCount = 2;
            this.ButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ButtonsTableLayoutPanel.Controls.Add(this.ApplyButton, 1, 0);
            this.ButtonsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ButtonsTableLayoutPanel.Location = new System.Drawing.Point(0, 122);
            this.ButtonsTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.ButtonsTableLayoutPanel.Name = "ButtonsTableLayoutPanel";
            this.ButtonsTableLayoutPanel.RowCount = 1;
            this.ButtonsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ButtonsTableLayoutPanel.Size = new System.Drawing.Size(236, 44);
            this.ButtonsTableLayoutPanel.TabIndex = 3;
            // 
            // ApplyButton
            // 
            this.ApplyButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.ApplyButton.AutoSize = true;
            this.ApplyButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ApplyButton.Location = new System.Drawing.Point(140, 6);
            this.ApplyButton.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.ApplyButton.MinimumSize = new System.Drawing.Size(96, 32);
            this.ApplyButton.Name = "ApplyButton";
            this.ApplyButton.Padding = new System.Windows.Forms.Padding(14, 6, 14, 6);
            this.ApplyButton.Size = new System.Drawing.Size(96, 32);
            this.ApplyButton.TabIndex = 0;
            this.ApplyButton.Text = "Apply";
            // 
            // CustomResolution
            // 
            this.AcceptButton = this.ApplyButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(260, 190);
            this.Controls.Add(this.MainTableLayoutPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "CustomResolution";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.Text = "Custom Resolution";
            this.MainTableLayoutPanel.ResumeLayout(false);
            this.MainTableLayoutPanel.PerformLayout();
            this.HeightTableLayoutPanel.ResumeLayout(false);
            this.HeightTableLayoutPanel.PerformLayout();
            this.WidthTableLayoutPanel.ResumeLayout(false);
            this.WidthTableLayoutPanel.PerformLayout();
            this.ButtonsTableLayoutPanel.ResumeLayout(false);
            this.ButtonsTableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label HeightLabel;
        private System.Windows.Forms.Label WidthLabel;
        private System.Windows.Forms.TextBox HeightTextBox;
        private System.Windows.Forms.TextBox WidthTextBox;
        private System.Windows.Forms.Label PromptLabel;
        private System.Windows.Forms.TableLayoutPanel MainTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel WidthTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel HeightTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel ButtonsTableLayoutPanel;
        private System.Windows.Forms.Button ApplyButton;
    }
}
