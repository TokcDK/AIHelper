namespace AIHelper.Manage.Functions.BindMOProfileToExe
{
    partial class BindForm
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
            this.ContentTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.SelectorsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.ProfileLabel = new System.Windows.Forms.Label();
            this.ProfilesComboBox = new System.Windows.Forms.ComboBox();
            this.ExesLabel = new System.Windows.Forms.Label();
            this.ExesComboBox = new System.Windows.Forms.ComboBox();
            this.ExeButtonsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.AddExeButton = new System.Windows.Forms.Button();
            this.RemoveExeButton = new System.Windows.Forms.Button();
            this.BoundExesListBox = new System.Windows.Forms.ListBox();
            this.ContentTableLayoutPanel.SuspendLayout();
            this.SelectorsTableLayoutPanel.SuspendLayout();
            this.ExeButtonsTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ContentTableLayoutPanel
            // 
            // Row 0 picks a profile and a game, row 1 moves a game between the two, and
            // the list takes whatever height is left over.
            this.ContentTableLayoutPanel.ColumnCount = 1;
            this.ContentTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ContentTableLayoutPanel.Controls.Add(this.SelectorsTableLayoutPanel, 0, 0);
            this.ContentTableLayoutPanel.Controls.Add(this.ExeButtonsTableLayoutPanel, 0, 1);
            this.ContentTableLayoutPanel.Controls.Add(this.BoundExesListBox, 0, 2);
            this.ContentTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ContentTableLayoutPanel.Location = new System.Drawing.Point(12, 12);
            this.ContentTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.ContentTableLayoutPanel.Name = "ContentTableLayoutPanel";
            this.ContentTableLayoutPanel.RowCount = 3;
            this.ContentTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ContentTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ContentTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ContentTableLayoutPanel.Size = new System.Drawing.Size(536, 276);
            this.ContentTableLayoutPanel.TabIndex = 0;
            // 
            // SelectorsTableLayoutPanel
            // 
            // Every row is as tall as its control, so the two captions and the two fields
            // stay together whatever the font turns out to be.
            this.SelectorsTableLayoutPanel.ColumnCount = 1;
            this.SelectorsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.SelectorsTableLayoutPanel.Controls.Add(this.ProfileLabel, 0, 0);
            this.SelectorsTableLayoutPanel.Controls.Add(this.ProfilesComboBox, 0, 1);
            this.SelectorsTableLayoutPanel.Controls.Add(this.ExesLabel, 0, 2);
            this.SelectorsTableLayoutPanel.Controls.Add(this.ExesComboBox, 0, 3);
            this.SelectorsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SelectorsTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.SelectorsTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.SelectorsTableLayoutPanel.Name = "SelectorsTableLayoutPanel";
            this.SelectorsTableLayoutPanel.RowCount = 4;
            this.SelectorsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.SelectorsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.SelectorsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.SelectorsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.SelectorsTableLayoutPanel.Size = new System.Drawing.Size(536, 112);
            this.SelectorsTableLayoutPanel.TabIndex = 0;
            // 
            // ProfileLabel
            // 
            this.ProfileLabel.AutoSize = true;
            this.ProfileLabel.Location = new System.Drawing.Point(0, 0);
            this.ProfileLabel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.ProfileLabel.Name = "ProfileLabel";
            this.ProfileLabel.Size = new System.Drawing.Size(44, 15);
            this.ProfileLabel.TabIndex = 0;
            this.ProfileLabel.Text = "Profile";
            // 
            // ProfilesComboBox
            // 
            this.ProfilesComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProfilesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ProfilesComboBox.FormattingEnabled = true;
            this.ProfilesComboBox.Location = new System.Drawing.Point(0, 19);
            this.ProfilesComboBox.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.ProfilesComboBox.Name = "ProfilesComboBox";
            this.ProfilesComboBox.Size = new System.Drawing.Size(536, 23);
            this.ProfilesComboBox.TabIndex = 1;
            this.ProfilesComboBox.SelectedIndexChanged += new System.EventHandler(this.ProfilesComboBox_SelectedIndexChanged);
            // 
            // ExesLabel
            // 
            this.ExesLabel.AutoSize = true;
            this.ExesLabel.Location = new System.Drawing.Point(0, 52);
            this.ExesLabel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.ExesLabel.Name = "ExesLabel";
            this.ExesLabel.Size = new System.Drawing.Size(38, 15);
            this.ExesLabel.TabIndex = 2;
            this.ExesLabel.Text = "Exes";
            // 
            // ExesComboBox
            // 
            this.ExesComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ExesComboBox.FormattingEnabled = true;
            this.ExesComboBox.Location = new System.Drawing.Point(0, 71);
            this.ExesComboBox.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.ExesComboBox.Name = "ExesComboBox";
            this.ExesComboBox.Size = new System.Drawing.Size(536, 23);
            this.ExesComboBox.TabIndex = 3;
            // 
            // ExeButtonsTableLayoutPanel
            // 
            // The first column takes the slack, so the two buttons keep their own size
            // and sit against the right edge.
            this.ExeButtonsTableLayoutPanel.ColumnCount = 3;
            this.ExeButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ExeButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ExeButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ExeButtonsTableLayoutPanel.Controls.Add(this.AddExeButton, 1, 0);
            this.ExeButtonsTableLayoutPanel.Controls.Add(this.RemoveExeButton, 2, 0);
            this.ExeButtonsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExeButtonsTableLayoutPanel.Location = new System.Drawing.Point(0, 112);
            this.ExeButtonsTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.ExeButtonsTableLayoutPanel.Name = "ExeButtonsTableLayoutPanel";
            this.ExeButtonsTableLayoutPanel.RowCount = 1;
            this.ExeButtonsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ExeButtonsTableLayoutPanel.Size = new System.Drawing.Size(536, 44);
            this.ExeButtonsTableLayoutPanel.TabIndex = 1;
            // 
            // AddExeButton
            // 
            // The arrows say which way the selected game moves: down into the list, or
            // back up out of it.
            this.AddExeButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.AddExeButton.AutoSize = true;
            this.AddExeButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.AddExeButton.Location = new System.Drawing.Point(344, 6);
            this.AddExeButton.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.AddExeButton.MinimumSize = new System.Drawing.Size(88, 32);
            this.AddExeButton.Name = "AddExeButton";
            this.AddExeButton.Padding = new System.Windows.Forms.Padding(14, 6, 14, 6);
            this.AddExeButton.Size = new System.Drawing.Size(88, 32);
            this.AddExeButton.TabIndex = 0;
            this.AddExeButton.Text = "↓";
            this.AddExeButton.Click += new System.EventHandler(this.AddExeButton_Click);
            // 
            // RemoveExeButton
            // 
            this.RemoveExeButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.RemoveExeButton.AutoSize = true;
            this.RemoveExeButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.RemoveExeButton.Location = new System.Drawing.Point(440, 6);
            this.RemoveExeButton.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.RemoveExeButton.MinimumSize = new System.Drawing.Size(88, 32);
            this.RemoveExeButton.Name = "RemoveExeButton";
            this.RemoveExeButton.Padding = new System.Windows.Forms.Padding(14, 6, 14, 6);
            this.RemoveExeButton.Size = new System.Drawing.Size(88, 32);
            this.RemoveExeButton.TabIndex = 1;
            this.RemoveExeButton.Text = "↑";
            this.RemoveExeButton.Click += new System.EventHandler(this.RemoveExeButton_Click);
            // 
            // BoundExesListBox
            // 
            this.BoundExesListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BoundExesListBox.FormattingEnabled = true;
            this.BoundExesListBox.IntegralHeight = false;
            this.BoundExesListBox.Location = new System.Drawing.Point(0, 156);
            this.BoundExesListBox.Margin = new System.Windows.Forms.Padding(0);
            this.BoundExesListBox.Name = "BoundExesListBox";
            this.BoundExesListBox.Size = new System.Drawing.Size(536, 120);
            this.BoundExesListBox.TabIndex = 2;
            // 
            // BindForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(560, 300);
            this.Controls.Add(this.ContentTableLayoutPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MinimumSize = new System.Drawing.Size(420, 240);
            this.Name = "BindForm";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.Text = "Bind Mod Organizer Profile to Game Executable";
            this.Load += new System.EventHandler(this.BindForm_Load);
            this.SelectorsTableLayoutPanel.ResumeLayout(false);
            this.SelectorsTableLayoutPanel.PerformLayout();
            this.ExeButtonsTableLayoutPanel.ResumeLayout(false);
            this.ExeButtonsTableLayoutPanel.PerformLayout();
            this.ContentTableLayoutPanel.ResumeLayout(false);
            this.ContentTableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel ContentTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel SelectorsTableLayoutPanel;
        private System.Windows.Forms.Label ProfileLabel;
        private System.Windows.Forms.ComboBox ProfilesComboBox;
        private System.Windows.Forms.Label ExesLabel;
        private System.Windows.Forms.ComboBox ExesComboBox;
        private System.Windows.Forms.TableLayoutPanel ExeButtonsTableLayoutPanel;
        private System.Windows.Forms.Button AddExeButton;
        private System.Windows.Forms.Button RemoveExeButton;
        private System.Windows.Forms.ListBox BoundExesListBox;
    }
}
