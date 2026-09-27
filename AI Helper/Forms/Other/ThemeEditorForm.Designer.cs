namespace AIHelper.Forms.Other
{
    partial class ThemeEditorForm
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
            if (disposing)
            {
                // the tooltip is created without a container, so components.Dispose does not
                // reach it; releasing it here as well as on close covers the caller in Edit,
                // which disposes the dialog through a using block
                DisposeTooltips();

                if (components != null)
                {
                    components.Dispose();
                }
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
            this.MainPanel = new System.Windows.Forms.Panel();
            this.LayoutTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lblElement = new System.Windows.Forms.Label();
            this.lblColour = new System.Windows.Forms.Label();
            this.RolesOwnColorListBox = new System.Windows.Forms.ListBox();
            this.EditLayoutTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.PreviewOwnColorPanel = new System.Windows.Forms.Panel();
            this.txtColourHex = new AIHelper.Manage.ui.ThemedTextBox();
            this.ColourButtonsFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.btnPick = new System.Windows.Forms.Button();
            this.btnFromScreen = new System.Windows.Forms.Button();
            this.btnResetElement = new System.Windows.Forms.Button();
            this.ColourOwnColorSwatchGrid = new AIHelper.Manage.ui.ColourSwatchGrid();
            this.ButtonsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lblHintMutedLabel = new System.Windows.Forms.Label();
            this.btnResetAll = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.MainPanel.SuspendLayout();
            this.LayoutTableLayoutPanel.SuspendLayout();
            this.EditLayoutTableLayoutPanel.SuspendLayout();
            this.ColourButtonsFlowLayoutPanel.SuspendLayout();
            this.ButtonsTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            this.MainPanel.Controls.Add(this.LayoutTableLayoutPanel);
            this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainPanel.Location = new System.Drawing.Point(0, 0);
            this.MainPanel.Margin = new System.Windows.Forms.Padding(0);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Padding = new System.Windows.Forms.Padding(12);
            this.MainPanel.Size = new System.Drawing.Size(700, 500);
            this.MainPanel.TabIndex = 0;
            // 
            // LayoutTableLayoutPanel
            // 
            this.LayoutTableLayoutPanel.ColumnCount = 2;
            this.LayoutTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 46F));
            this.LayoutTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 54F));
            this.LayoutTableLayoutPanel.Controls.Add(this.lblElement, 0, 0);
            this.LayoutTableLayoutPanel.Controls.Add(this.lblColour, 1, 0);
            this.LayoutTableLayoutPanel.Controls.Add(this.RolesOwnColorListBox, 0, 1);
            this.LayoutTableLayoutPanel.Controls.Add(this.EditLayoutTableLayoutPanel, 1, 1);
            this.LayoutTableLayoutPanel.Controls.Add(this.ButtonsTableLayoutPanel, 0, 2);
            this.LayoutTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LayoutTableLayoutPanel.Location = new System.Drawing.Point(12, 12);
            this.LayoutTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.LayoutTableLayoutPanel.Name = "LayoutTableLayoutPanel";
            this.LayoutTableLayoutPanel.RowCount = 3;
            this.LayoutTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.LayoutTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.LayoutTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.LayoutTableLayoutPanel.SetColumnSpan(this.ButtonsTableLayoutPanel, 2);
            this.LayoutTableLayoutPanel.Size = new System.Drawing.Size(676, 476);
            this.LayoutTableLayoutPanel.TabIndex = 0;
            // 
            // lblElement
            // 
            this.lblElement.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblElement.Location = new System.Drawing.Point(0, 0);
            this.lblElement.Margin = new System.Windows.Forms.Padding(0);
            this.lblElement.Name = "lblElement";
            this.lblElement.Size = new System.Drawing.Size(311, 22);
            this.lblElement.TabIndex = 0;
            this.lblElement.Text = "Element";
            this.lblElement.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // lblColour
            // 
            this.lblColour.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblColour.Location = new System.Drawing.Point(317, 0);
            this.lblColour.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.lblColour.Name = "lblColour";
            this.lblColour.Size = new System.Drawing.Size(359, 22);
            this.lblColour.TabIndex = 1;
            this.lblColour.Text = "Colour";
            this.lblColour.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // RolesOwnColorListBox
            // 
            this.RolesOwnColorListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.RolesOwnColorListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RolesOwnColorListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.RolesOwnColorListBox.IntegralHeight = false;
            this.RolesOwnColorListBox.ItemHeight = 22;
            this.RolesOwnColorListBox.Location = new System.Drawing.Point(0, 22);
            this.RolesOwnColorListBox.Margin = new System.Windows.Forms.Padding(0, 0, 6, 6);
            this.RolesOwnColorListBox.Name = "RolesOwnColorListBox";
            this.RolesOwnColorListBox.Size = new System.Drawing.Size(305, 408);
            this.RolesOwnColorListBox.TabIndex = 2;
            this.RolesOwnColorListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.RolesListBox_DrawItem);
            this.RolesOwnColorListBox.SelectedIndexChanged += new System.EventHandler(this.RolesListBox_SelectedIndexChanged);
            // 
            // EditLayoutTableLayoutPanel
            // 
            this.EditLayoutTableLayoutPanel.ColumnCount = 1;
            this.EditLayoutTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.EditLayoutTableLayoutPanel.Controls.Add(this.PreviewOwnColorPanel, 0, 0);
            this.EditLayoutTableLayoutPanel.Controls.Add(this.txtColourHex, 0, 1);
            this.EditLayoutTableLayoutPanel.Controls.Add(this.ColourButtonsFlowLayoutPanel, 0, 2);
            this.EditLayoutTableLayoutPanel.Controls.Add(this.ColourOwnColorSwatchGrid, 0, 3);
            this.EditLayoutTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EditLayoutTableLayoutPanel.Location = new System.Drawing.Point(317, 22);
            this.EditLayoutTableLayoutPanel.Margin = new System.Windows.Forms.Padding(6, 0, 0, 6);
            this.EditLayoutTableLayoutPanel.Name = "EditLayoutTableLayoutPanel";
            this.EditLayoutTableLayoutPanel.RowCount = 4;
            this.EditLayoutTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72F));
            this.EditLayoutTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.EditLayoutTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.EditLayoutTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.EditLayoutTableLayoutPanel.Size = new System.Drawing.Size(365, 408);
            this.EditLayoutTableLayoutPanel.TabIndex = 3;
            // 
            // PreviewOwnColorPanel
            // 
            this.PreviewOwnColorPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PreviewOwnColorPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PreviewOwnColorPanel.Location = new System.Drawing.Point(0, 0);
            this.PreviewOwnColorPanel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.PreviewOwnColorPanel.Name = "PreviewOwnColorPanel";
            this.PreviewOwnColorPanel.Size = new System.Drawing.Size(365, 64);
            this.PreviewOwnColorPanel.TabIndex = 0;
            // 
            // txtColourHex
            // 
            this.txtColourHex.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtColourHex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtColourHex.Location = new System.Drawing.Point(0, 72);
            this.txtColourHex.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.txtColourHex.MaxLength = 9;
            this.txtColourHex.Name = "txtColourHex";
            this.txtColourHex.Size = new System.Drawing.Size(365, 23);
            this.txtColourHex.TabIndex = 1;
            this.txtColourHex.TextChanged += new System.EventHandler(this.TxtColourHex_TextChanged);
            // 
            // ColourButtonsFlowLayoutPanel
            // 
            this.ColourButtonsFlowLayoutPanel.Controls.Add(this.btnPick);
            this.ColourButtonsFlowLayoutPanel.Controls.Add(this.btnFromScreen);
            this.ColourButtonsFlowLayoutPanel.Controls.Add(this.btnResetElement);
            this.ColourButtonsFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ColourButtonsFlowLayoutPanel.Location = new System.Drawing.Point(0, 102);
            this.ColourButtonsFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.ColourButtonsFlowLayoutPanel.Name = "ColourButtonsFlowLayoutPanel";
            this.ColourButtonsFlowLayoutPanel.Size = new System.Drawing.Size(365, 40);
            this.ColourButtonsFlowLayoutPanel.TabIndex = 2;
            this.ColourButtonsFlowLayoutPanel.WrapContents = false;
            // 
            // btnPick
            // 
            this.btnPick.AutoSize = true;
            this.btnPick.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnPick.Location = new System.Drawing.Point(0, 0);
            this.btnPick.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.btnPick.MinimumSize = new System.Drawing.Size(0, 32);
            this.btnPick.Name = "btnPick";
            this.btnPick.Padding = new System.Windows.Forms.Padding(12, 4, 12, 4);
            this.btnPick.Size = new System.Drawing.Size(96, 32);
            this.btnPick.TabIndex = 0;
            this.btnPick.Text = "Pick...";
            this.btnPick.Click += new System.EventHandler(this.BtnPick_Click);
            // 
            // btnFromScreen
            // 
            this.btnFromScreen.AutoSize = true;
            this.btnFromScreen.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnFromScreen.Location = new System.Drawing.Point(102, 0);
            this.btnFromScreen.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.btnFromScreen.MinimumSize = new System.Drawing.Size(0, 32);
            this.btnFromScreen.Name = "btnFromScreen";
            this.btnFromScreen.Padding = new System.Windows.Forms.Padding(12, 4, 12, 4);
            this.btnFromScreen.Size = new System.Drawing.Size(120, 32);
            this.btnFromScreen.TabIndex = 1;
            this.btnFromScreen.Text = "From screen";
            this.btnFromScreen.Click += new System.EventHandler(this.BtnFromScreen_Click);
            // 
            // btnResetElement
            // 
            this.btnResetElement.AutoSize = true;
            this.btnResetElement.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnResetElement.Location = new System.Drawing.Point(228, 0);
            this.btnResetElement.Margin = new System.Windows.Forms.Padding(0);
            this.btnResetElement.MinimumSize = new System.Drawing.Size(0, 32);
            this.btnResetElement.Name = "btnResetElement";
            this.btnResetElement.Padding = new System.Windows.Forms.Padding(12, 4, 12, 4);
            this.btnResetElement.Size = new System.Drawing.Size(120, 32);
            this.btnResetElement.TabIndex = 2;
            this.btnResetElement.Text = "Reset element";
            this.btnResetElement.Click += new System.EventHandler(this.BtnResetElement_Click);
            // 
            // ColourOwnColorSwatchGrid
            // 
            this.ColourOwnColorSwatchGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ColourOwnColorSwatchGrid.Location = new System.Drawing.Point(0, 142);
            this.ColourOwnColorSwatchGrid.Margin = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.ColourOwnColorSwatchGrid.MinimumSize = new System.Drawing.Size(0, 178);
            this.ColourOwnColorSwatchGrid.Name = "ColourOwnColorSwatchGrid";
            this.ColourOwnColorSwatchGrid.Size = new System.Drawing.Size(365, 272);
            this.ColourOwnColorSwatchGrid.TabIndex = 3;
            this.ColourOwnColorSwatchGrid.ColourPicked += new System.Action<System.Drawing.Color>(this.ColourSwatchGrid_ColourPicked);
            // 
            // ButtonsTableLayoutPanel
            // 
            this.ButtonsTableLayoutPanel.ColumnCount = 3;
            this.ButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ButtonsTableLayoutPanel.Controls.Add(this.lblHintMutedLabel, 0, 0);
            this.ButtonsTableLayoutPanel.Controls.Add(this.btnResetAll, 1, 0);
            this.ButtonsTableLayoutPanel.Controls.Add(this.btnClose, 2, 0);
            this.ButtonsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ButtonsTableLayoutPanel.Location = new System.Drawing.Point(0, 436);
            this.ButtonsTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.ButtonsTableLayoutPanel.Name = "ButtonsTableLayoutPanel";
            this.ButtonsTableLayoutPanel.RowCount = 1;
            this.ButtonsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ButtonsTableLayoutPanel.Size = new System.Drawing.Size(676, 40);
            this.ButtonsTableLayoutPanel.TabIndex = 4;
            // 
            // lblHintMutedLabel
            // 
            this.lblHintMutedLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblHintMutedLabel.AutoSize = true;
            this.lblHintMutedLabel.Location = new System.Drawing.Point(0, 12);
            this.lblHintMutedLabel.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.lblHintMutedLabel.Name = "lblHintMutedLabel";
            this.lblHintMutedLabel.Size = new System.Drawing.Size(0, 15);
            this.lblHintMutedLabel.TabIndex = 0;
            this.lblHintMutedLabel.Text = "Changes apply at once. Close the window, then use Save to keep them.";
            // 
            // btnResetAll
            // 
            this.btnResetAll.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnResetAll.AutoSize = true;
            this.btnResetAll.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnResetAll.Location = new System.Drawing.Point(462, 4);
            this.btnResetAll.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.btnResetAll.MinimumSize = new System.Drawing.Size(0, 32);
            this.btnResetAll.Name = "btnResetAll";
            this.btnResetAll.Padding = new System.Windows.Forms.Padding(12, 4, 12, 4);
            this.btnResetAll.Size = new System.Drawing.Size(96, 32);
            this.btnResetAll.TabIndex = 1;
            this.btnResetAll.Text = "Reset all";
            this.btnResetAll.Click += new System.EventHandler(this.BtnResetAll_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnClose.AutoSize = true;
            this.btnClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnClose.Location = new System.Drawing.Point(566, 4);
            this.btnClose.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.btnClose.MinimumSize = new System.Drawing.Size(96, 32);
            this.btnClose.Name = "btnClose";
            this.btnClose.Padding = new System.Windows.Forms.Padding(12, 4, 12, 4);
            this.btnClose.Size = new System.Drawing.Size(110, 32);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // ThemeEditorForm
            // 
            this.AcceptButton = this.btnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(700, 500);
            this.Controls.Add(this.MainPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MinimumSize = new System.Drawing.Size(620, 460);
            this.Name = "ThemeEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit theme colours";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ThemeEditorForm_FormClosing);
            this.Load += new System.EventHandler(this.ThemeEditorForm_Load);
            this.MainPanel.ResumeLayout(false);
            this.LayoutTableLayoutPanel.ResumeLayout(false);
            this.LayoutTableLayoutPanel.PerformLayout();
            this.EditLayoutTableLayoutPanel.ResumeLayout(false);
            this.EditLayoutTableLayoutPanel.PerformLayout();
            this.ColourButtonsFlowLayoutPanel.ResumeLayout(false);
            this.ColourButtonsFlowLayoutPanel.PerformLayout();
            this.ButtonsTableLayoutPanel.ResumeLayout(false);
            this.ButtonsTableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel MainPanel;
        private System.Windows.Forms.TableLayoutPanel LayoutTableLayoutPanel;
        private System.Windows.Forms.Label lblElement;
        private System.Windows.Forms.Label lblColour;
        private System.Windows.Forms.ListBox RolesOwnColorListBox;
        private System.Windows.Forms.TableLayoutPanel EditLayoutTableLayoutPanel;
        private System.Windows.Forms.Panel PreviewOwnColorPanel;
        private AIHelper.Manage.ui.ThemedTextBox txtColourHex;
        private System.Windows.Forms.FlowLayoutPanel ColourButtonsFlowLayoutPanel;
        private System.Windows.Forms.Button btnPick;
        private System.Windows.Forms.Button btnFromScreen;
        private System.Windows.Forms.Button btnResetElement;
        private AIHelper.Manage.ui.ColourSwatchGrid ColourOwnColorSwatchGrid;
        private System.Windows.Forms.TableLayoutPanel ButtonsTableLayoutPanel;
        private System.Windows.Forms.Label lblHintMutedLabel;
        private System.Windows.Forms.Button btnResetAll;
        private System.Windows.Forms.Button btnClose;
    }
}
