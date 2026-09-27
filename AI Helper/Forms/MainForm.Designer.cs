namespace AIHelper
{
    internal partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // the colour editor is a window of its own, so components.Dispose does not
                // reach it. It is closed here, before the controls it reports to are gone,
                // so that closing it cannot find the theme row already disposed.
                if (_themeEditor != null && !_themeEditor.IsDisposed)
                {
                    _themeEditor.Close();
                }

                _themeEditor = null;

                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.MainFormBackgroundPanel = new System.Windows.Forms.Panel();
            this.FormRootTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.FormTitleBarTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.FormTitleLabel = new System.Windows.Forms.Label();
            this.FormMinimizeButton = new System.Windows.Forms.Button();
            this.FormCloseButton = new System.Windows.Forms.Button();
            this.AIGirlHelperTabControl = new AIHelper.Manage.ui.ThemedTabControl();
            this.LaunchTabPage = new System.Windows.Forms.TabPage();
            this.LaunchTabPageBackgroundPanel = new System.Windows.Forms.Panel();
            this.LaunchTabTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.LaunchTabTopTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.LaunchTabTopAppGameTitleTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.LaunchTabTheAppTitleAccentLabel = new System.Windows.Forms.Label();
            this.SelectedGameTitleMutedLabel = new System.Windows.Forms.Label();
            this.LaunchTabButtonsDownTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.LaunchTabSubMainButtonsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.GameButton = new System.Windows.Forms.Button();
            this.MOButton = new System.Windows.Forms.Button();
            this.StudioButton = new System.Windows.Forms.Button();
            this.SettingsButton = new System.Windows.Forms.Button();
            this.FunctionsForFLPTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.SettingsTabPage = new System.Windows.Forms.TabPage();
            this.SettingsTabPageBackgroundPanel = new System.Windows.Forms.Panel();
            this.SettingsTabControl = new AIHelper.Manage.ui.ThemedTabControl();
            this.SettingsTabGeneralTabPage = new System.Windows.Forms.TabPage();
            this.SettingsTabGeneralTabPageBackgroundPanel = new System.Windows.Forms.Panel();
            this.SettingsTabGeneralTabPageTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.CurrentGameLabel = new System.Windows.Forms.Label();
            this.ManageGameTableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.CurrentGamePanel1 = new System.Windows.Forms.Panel();
            this.CurrentGameTitleTextBox = new AIHelper.Manage.ui.ThemedTextBox();
            this.CurrentGameComboBox = new AIHelper.Manage.ui.ThemedComboBox();
            this.AddGameButton = new System.Windows.Forms.Button();
            this.ThemeSelectLabel = new System.Windows.Forms.Label();
            this.ThemeSelectTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.SelectThemeComboBox = new AIHelper.Manage.ui.ThemedComboBox();
            this.EditThemeButton = new System.Windows.Forms.Button();
            this.SaveThemeButton = new System.Windows.Forms.Button();
            this.RenameThemeButton = new System.Windows.Forms.Button();
            this.DeleteThemeButton = new System.Windows.Forms.Button();
            this.OtherSettingsFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.BepInExLogTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.BepInExConsoleCheckBox = new AIHelper.Manage.ui.ThemedCheckBox();
            this.OpenLogLinkLabel = new System.Windows.Forms.LinkLabel();
            this.VRGameCheckBox = new AIHelper.Manage.ui.ThemedCheckBox();
            this.JPLauncherRunLinkLabel = new System.Windows.Forms.LinkLabel();
            this.ExtraSettingsLinkLabel = new System.Windows.Forms.LinkLabel();
            this.FixRegistryLinkLabel = new System.Windows.Forms.LinkLabel();
            this.CreateShortcutLinkLabel = new System.Windows.Forms.LinkLabel();
            this.AutoShortcutRegistryCheckBox = new AIHelper.Manage.ui.ThemedCheckBox();
            this.SettingsTabDisplayTabPage = new System.Windows.Forms.TabPage();
            this.SettingsTabDisplayTabPageGameBackgroundPanel = new System.Windows.Forms.Panel();
            this.ToolsTabPage = new System.Windows.Forms.TabPage();
            this.ToolsTabPageBackgroundPanel = new System.Windows.Forms.Panel();
            this.FoldersTabPage = new System.Windows.Forms.TabPage();
            this.FoldersTabPageBackgroundPanel = new System.Windows.Forms.Panel();
            this.MainFormBackgroundPanel.SuspendLayout();
            this.FormRootTableLayoutPanel.SuspendLayout();
            this.FormTitleBarTableLayoutPanel.SuspendLayout();
            this.AIGirlHelperTabControl.SuspendLayout();
            this.LaunchTabPage.SuspendLayout();
            this.LaunchTabPageBackgroundPanel.SuspendLayout();
            this.LaunchTabTableLayoutPanel.SuspendLayout();
            this.LaunchTabTopTableLayoutPanel.SuspendLayout();
            this.LaunchTabTopAppGameTitleTableLayoutPanel.SuspendLayout();
            this.LaunchTabButtonsDownTableLayoutPanel.SuspendLayout();
            this.LaunchTabSubMainButtonsTableLayoutPanel.SuspendLayout();
            this.SettingsTabPage.SuspendLayout();
            this.SettingsTabPageBackgroundPanel.SuspendLayout();
            this.SettingsTabControl.SuspendLayout();
            this.SettingsTabGeneralTabPage.SuspendLayout();
            this.SettingsTabGeneralTabPageBackgroundPanel.SuspendLayout();
            this.SettingsTabGeneralTabPageTableLayoutPanel.SuspendLayout();
            this.ManageGameTableLayoutPanel1.SuspendLayout();
            this.CurrentGamePanel1.SuspendLayout();
            this.ThemeSelectTableLayoutPanel.SuspendLayout();
            this.OtherSettingsFlowLayoutPanel.SuspendLayout();
            this.BepInExLogTableLayoutPanel.SuspendLayout();
            this.SettingsTabDisplayTabPage.SuspendLayout();
            this.ToolsTabPage.SuspendLayout();
            this.FoldersTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainFormBackgroundPanel
            // 
            this.MainFormBackgroundPanel.Controls.Add(this.FormRootTableLayoutPanel);
            this.MainFormBackgroundPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainFormBackgroundPanel.Location = new System.Drawing.Point(1, 1);
            this.MainFormBackgroundPanel.Margin = new System.Windows.Forms.Padding(0);
            this.MainFormBackgroundPanel.Name = "MainFormBackgroundPanel";
            this.MainFormBackgroundPanel.Size = new System.Drawing.Size(598, 378);
            this.MainFormBackgroundPanel.TabIndex = 0;
            // 
            // FormRootTableLayoutPanel
            // 
            this.FormRootTableLayoutPanel.ColumnCount = 1;
            this.FormRootTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.FormRootTableLayoutPanel.Controls.Add(this.FormTitleBarTableLayoutPanel, 0, 0);
            this.FormRootTableLayoutPanel.Controls.Add(this.AIGirlHelperTabControl, 0, 1);
            this.FormRootTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FormRootTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.FormRootTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.FormRootTableLayoutPanel.Name = "FormRootTableLayoutPanel";
            this.FormRootTableLayoutPanel.RowCount = 2;
            this.FormRootTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.FormRootTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.FormRootTableLayoutPanel.Size = new System.Drawing.Size(598, 378);
            this.FormRootTableLayoutPanel.TabIndex = 0;
            // 
            // FormTitleBarTableLayoutPanel
            // 
            this.FormTitleBarTableLayoutPanel.ColumnCount = 3;
            this.FormTitleBarTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.FormTitleBarTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.FormTitleBarTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.FormTitleBarTableLayoutPanel.Controls.Add(this.FormTitleLabel, 0, 0);
            this.FormTitleBarTableLayoutPanel.Controls.Add(this.FormMinimizeButton, 1, 0);
            this.FormTitleBarTableLayoutPanel.Controls.Add(this.FormCloseButton, 2, 0);
            this.FormTitleBarTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FormTitleBarTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.FormTitleBarTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.FormTitleBarTableLayoutPanel.Name = "FormTitleBarTableLayoutPanel";
            this.FormTitleBarTableLayoutPanel.RowCount = 1;
            this.FormTitleBarTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.FormTitleBarTableLayoutPanel.Size = new System.Drawing.Size(598, 44);
            this.FormTitleBarTableLayoutPanel.TabIndex = 1;
            // 
            // FormTitleLabel
            // 
            this.FormTitleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FormTitleLabel.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormTitleLabel.Location = new System.Drawing.Point(0, 0);
            this.FormTitleLabel.Margin = new System.Windows.Forms.Padding(0);
            this.FormTitleLabel.Name = "FormTitleLabel";
            this.FormTitleLabel.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
            this.FormTitleLabel.Size = new System.Drawing.Size(510, 44);
            this.FormTitleLabel.TabIndex = 0;
            this.FormTitleLabel.Text = "AIHelper";
            this.FormTitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FormMinimizeButton
            // 
            this.FormMinimizeButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FormMinimizeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FormMinimizeButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormMinimizeButton.Location = new System.Drawing.Point(510, 0);
            this.FormMinimizeButton.Margin = new System.Windows.Forms.Padding(0);
            this.FormMinimizeButton.Name = "FormMinimizeButton";
            this.FormMinimizeButton.Size = new System.Drawing.Size(44, 44);
            this.FormMinimizeButton.TabIndex = 1;
            this.FormMinimizeButton.Text = "–";
            this.FormMinimizeButton.UseVisualStyleBackColor = false;
            // 
            // FormCloseButton
            // 
            this.FormCloseButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FormCloseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FormCloseButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormCloseButton.Location = new System.Drawing.Point(554, 0);
            this.FormCloseButton.Margin = new System.Windows.Forms.Padding(0);
            this.FormCloseButton.Name = "FormCloseButton";
            this.FormCloseButton.Size = new System.Drawing.Size(44, 44);
            this.FormCloseButton.TabIndex = 2;
            this.FormCloseButton.Text = "✕";
            this.FormCloseButton.UseVisualStyleBackColor = false;
            // 
            // AIGirlHelperTabControl
            // 
            this.AIGirlHelperTabControl.Controls.Add(this.LaunchTabPage);
            this.AIGirlHelperTabControl.Controls.Add(this.SettingsTabPage);
            this.AIGirlHelperTabControl.Controls.Add(this.ToolsTabPage);
            this.AIGirlHelperTabControl.Controls.Add(this.FoldersTabPage);
            this.AIGirlHelperTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AIGirlHelperTabControl.HotTrack = true;
            this.AIGirlHelperTabControl.Location = new System.Drawing.Point(0, 44);
            this.AIGirlHelperTabControl.Margin = new System.Windows.Forms.Padding(0);
            this.AIGirlHelperTabControl.Name = "AIGirlHelperTabControl";
            this.AIGirlHelperTabControl.Padding = new System.Drawing.Point(14, 6);
            this.AIGirlHelperTabControl.SelectedIndex = 0;
            this.AIGirlHelperTabControl.Size = new System.Drawing.Size(598, 334);
            this.AIGirlHelperTabControl.TabIndex = 0;
            this.AIGirlHelperTabControl.SelectedIndexChanged += new System.EventHandler(this.AIGirlHelperTabControl_SelectedIndexChanged);
            this.AIGirlHelperTabControl.Selected += new System.Windows.Forms.TabControlEventHandler(this.AIGirlHelperTabControl_Selected);
            // 
            // LaunchTabPage
            // 
            this.LaunchTabPage.Controls.Add(this.LaunchTabPageBackgroundPanel);
            this.LaunchTabPage.Location = new System.Drawing.Point(4, 31);
            this.LaunchTabPage.Margin = new System.Windows.Forms.Padding(0);
            this.LaunchTabPage.Name = "LaunchTabPage";
            this.LaunchTabPage.Size = new System.Drawing.Size(590, 299);
            this.LaunchTabPage.TabIndex = 0;
            this.LaunchTabPage.Text = "🚀Launch";
            // 
            // LaunchTabPageBackgroundPanel
            // 
            this.LaunchTabPageBackgroundPanel.Controls.Add(this.LaunchTabTableLayoutPanel);
            this.LaunchTabPageBackgroundPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LaunchTabPageBackgroundPanel.Location = new System.Drawing.Point(0, 0);
            this.LaunchTabPageBackgroundPanel.Margin = new System.Windows.Forms.Padding(0);
            this.LaunchTabPageBackgroundPanel.Name = "LaunchTabPageBackgroundPanel";
            this.LaunchTabPageBackgroundPanel.Size = new System.Drawing.Size(590, 299);
            this.LaunchTabPageBackgroundPanel.TabIndex = 0;
            // 
            // LaunchTabTableLayoutPanel
            // 
            this.LaunchTabTableLayoutPanel.ColumnCount = 1;
            this.LaunchTabTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.LaunchTabTableLayoutPanel.Controls.Add(this.LaunchTabTopTableLayoutPanel, 0, 0);
            this.LaunchTabTableLayoutPanel.Controls.Add(this.LaunchTabButtonsDownTableLayoutPanel, 0, 1);
            this.LaunchTabTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LaunchTabTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.LaunchTabTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.LaunchTabTableLayoutPanel.Name = "LaunchTabTableLayoutPanel";
            this.LaunchTabTableLayoutPanel.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
            this.LaunchTabTableLayoutPanel.RowCount = 2;
            this.LaunchTabTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 38F));
            this.LaunchTabTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 62F));
            this.LaunchTabTableLayoutPanel.Size = new System.Drawing.Size(590, 299);
            this.LaunchTabTableLayoutPanel.TabIndex = 0;
            // 
            // LaunchTabTopTableLayoutPanel
            // 
            this.LaunchTabTopTableLayoutPanel.ColumnCount = 3;
            this.LaunchTabTopTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12F));
            this.LaunchTabTopTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 76F));
            this.LaunchTabTopTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12F));
            this.LaunchTabTopTableLayoutPanel.Controls.Add(this.LaunchTabTopAppGameTitleTableLayoutPanel, 1, 0);
            this.LaunchTabTopTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LaunchTabTopTableLayoutPanel.Location = new System.Drawing.Point(16, 12);
            this.LaunchTabTopTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.LaunchTabTopTableLayoutPanel.Name = "LaunchTabTopTableLayoutPanel";
            this.LaunchTabTopTableLayoutPanel.RowCount = 1;
            this.LaunchTabTopTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.LaunchTabTopTableLayoutPanel.Size = new System.Drawing.Size(558, 104);
            this.LaunchTabTopTableLayoutPanel.TabIndex = 0;
            // 
            // LaunchTabTopAppGameTitleTableLayoutPanel
            // 
            this.LaunchTabTopAppGameTitleTableLayoutPanel.ColumnCount = 1;
            this.LaunchTabTopAppGameTitleTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.LaunchTabTopAppGameTitleTableLayoutPanel.Controls.Add(this.LaunchTabTheAppTitleAccentLabel, 0, 0);
            this.LaunchTabTopAppGameTitleTableLayoutPanel.Controls.Add(this.SelectedGameTitleMutedLabel, 0, 1);
            this.LaunchTabTopAppGameTitleTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LaunchTabTopAppGameTitleTableLayoutPanel.Location = new System.Drawing.Point(66, 0);
            this.LaunchTabTopAppGameTitleTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.LaunchTabTopAppGameTitleTableLayoutPanel.Name = "LaunchTabTopAppGameTitleTableLayoutPanel";
            this.LaunchTabTopAppGameTitleTableLayoutPanel.RowCount = 2;
            this.LaunchTabTopAppGameTitleTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 64F));
            this.LaunchTabTopAppGameTitleTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 36F));
            this.LaunchTabTopAppGameTitleTableLayoutPanel.Size = new System.Drawing.Size(424, 104);
            this.LaunchTabTopAppGameTitleTableLayoutPanel.TabIndex = 0;
            // 
            // LaunchTabTheAppTitleAccentLabel
            // 
            this.LaunchTabTheAppTitleAccentLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LaunchTabTheAppTitleAccentLabel.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.LaunchTabTheAppTitleAccentLabel.Location = new System.Drawing.Point(0, 0);
            this.LaunchTabTheAppTitleAccentLabel.Margin = new System.Windows.Forms.Padding(0);
            this.LaunchTabTheAppTitleAccentLabel.Name = "LaunchTabTheAppTitleAccentLabel";
            this.LaunchTabTheAppTitleAccentLabel.Size = new System.Drawing.Size(424, 66);
            this.LaunchTabTheAppTitleAccentLabel.TabIndex = 0;
            this.LaunchTabTheAppTitleAccentLabel.Text = "-  Ằ🌶Ḩelper  -";
            this.LaunchTabTheAppTitleAccentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SelectedGameTitleMutedLabel
            // 
            this.SelectedGameTitleMutedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SelectedGameTitleMutedLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.SelectedGameTitleMutedLabel.Location = new System.Drawing.Point(0, 66);
            this.SelectedGameTitleMutedLabel.Margin = new System.Windows.Forms.Padding(0);
            this.SelectedGameTitleMutedLabel.Name = "SelectedGameTitleMutedLabel";
            this.SelectedGameTitleMutedLabel.Size = new System.Drawing.Size(424, 38);
            this.SelectedGameTitleMutedLabel.TabIndex = 1;
            this.SelectedGameTitleMutedLabel.Text = "GameTitle";
            this.SelectedGameTitleMutedLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // LaunchTabButtonsDownTableLayoutPanel
            // 
            this.LaunchTabButtonsDownTableLayoutPanel.ColumnCount = 1;
            this.LaunchTabButtonsDownTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.LaunchTabButtonsDownTableLayoutPanel.Controls.Add(this.LaunchTabSubMainButtonsTableLayoutPanel, 0, 0);
            this.LaunchTabButtonsDownTableLayoutPanel.Controls.Add(this.FunctionsForFLPTableLayoutPanel, 0, 1);
            this.LaunchTabButtonsDownTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LaunchTabButtonsDownTableLayoutPanel.Location = new System.Drawing.Point(16, 116);
            this.LaunchTabButtonsDownTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.LaunchTabButtonsDownTableLayoutPanel.Name = "LaunchTabButtonsDownTableLayoutPanel";
            this.LaunchTabButtonsDownTableLayoutPanel.RowCount = 2;
            this.LaunchTabButtonsDownTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.LaunchTabButtonsDownTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.LaunchTabButtonsDownTableLayoutPanel.Size = new System.Drawing.Size(558, 171);
            this.LaunchTabButtonsDownTableLayoutPanel.TabIndex = 1;
            // 
            // LaunchTabSubMainButtonsTableLayoutPanel
            // 
            this.LaunchTabSubMainButtonsTableLayoutPanel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.LaunchTabSubMainButtonsTableLayoutPanel.AutoSize = true;
            this.LaunchTabSubMainButtonsTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.LaunchTabSubMainButtonsTableLayoutPanel.ColumnCount = 2;
            this.LaunchTabSubMainButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 236F));
            this.LaunchTabSubMainButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 236F));
            this.LaunchTabSubMainButtonsTableLayoutPanel.Controls.Add(this.GameButton, 0, 0);
            this.LaunchTabSubMainButtonsTableLayoutPanel.Controls.Add(this.MOButton, 1, 0);
            this.LaunchTabSubMainButtonsTableLayoutPanel.Controls.Add(this.StudioButton, 0, 1);
            this.LaunchTabSubMainButtonsTableLayoutPanel.Controls.Add(this.SettingsButton, 1, 1);
            this.LaunchTabSubMainButtonsTableLayoutPanel.Location = new System.Drawing.Point(43, 6);
            this.LaunchTabSubMainButtonsTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.LaunchTabSubMainButtonsTableLayoutPanel.Name = "LaunchTabSubMainButtonsTableLayoutPanel";
            this.LaunchTabSubMainButtonsTableLayoutPanel.RowCount = 2;
            this.LaunchTabSubMainButtonsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.LaunchTabSubMainButtonsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.LaunchTabSubMainButtonsTableLayoutPanel.Size = new System.Drawing.Size(472, 120);
            this.LaunchTabSubMainButtonsTableLayoutPanel.TabIndex = 0;
            // 
            // GameButton
            // 
            this.GameButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GameButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.GameButton.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.GameButton.Location = new System.Drawing.Point(6, 6);
            this.GameButton.Margin = new System.Windows.Forms.Padding(6);
            this.GameButton.Name = "GameButton";
            this.GameButton.Size = new System.Drawing.Size(224, 48);
            this.GameButton.TabIndex = 0;
            this.GameButton.Text = "Game";
            this.GameButton.UseVisualStyleBackColor = false;
            this.GameButton.Click += new System.EventHandler(this.GameButton_Click);
            // 
            // MOButton
            // 
            this.MOButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MOButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MOButton.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MOButton.Location = new System.Drawing.Point(242, 6);
            this.MOButton.Margin = new System.Windows.Forms.Padding(6);
            this.MOButton.Name = "MOButton";
            this.MOButton.Size = new System.Drawing.Size(224, 48);
            this.MOButton.TabIndex = 1;
            this.MOButton.Text = "Manager";
            this.MOButton.UseVisualStyleBackColor = false;
            this.MOButton.Click += new System.EventHandler(this.MOButton_Click);
            // 
            // StudioButton
            // 
            this.StudioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StudioButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StudioButton.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.StudioButton.Location = new System.Drawing.Point(6, 66);
            this.StudioButton.Margin = new System.Windows.Forms.Padding(6);
            this.StudioButton.Name = "StudioButton";
            this.StudioButton.Size = new System.Drawing.Size(224, 48);
            this.StudioButton.TabIndex = 2;
            this.StudioButton.Text = "Studio";
            this.StudioButton.UseVisualStyleBackColor = false;
            this.StudioButton.Click += new System.EventHandler(this.StudioButton_Click);
            // 
            // SettingsButton
            // 
            this.SettingsButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SettingsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SettingsButton.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.SettingsButton.Location = new System.Drawing.Point(242, 66);
            this.SettingsButton.Margin = new System.Windows.Forms.Padding(6);
            this.SettingsButton.Name = "SettingsButton";
            this.SettingsButton.Size = new System.Drawing.Size(224, 48);
            this.SettingsButton.TabIndex = 3;
            this.SettingsButton.Text = "Settings";
            this.SettingsButton.UseVisualStyleBackColor = false;
            this.SettingsButton.Click += new System.EventHandler(this.SettingsButton_Click);
            // 
            // FunctionsForFLPTableLayoutPanel
            // 
            this.FunctionsForFLPTableLayoutPanel.ColumnCount = 1;
            this.FunctionsForFLPTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.FunctionsForFLPTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FunctionsForFLPTableLayoutPanel.Location = new System.Drawing.Point(0, 133);
            this.FunctionsForFLPTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.FunctionsForFLPTableLayoutPanel.Name = "FunctionsForFLPTableLayoutPanel";
            this.FunctionsForFLPTableLayoutPanel.RowCount = 1;
            this.FunctionsForFLPTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.FunctionsForFLPTableLayoutPanel.Size = new System.Drawing.Size(558, 38);
            this.FunctionsForFLPTableLayoutPanel.TabIndex = 1;
            // 
            // SettingsTabPage
            // 
            this.SettingsTabPage.Controls.Add(this.SettingsTabPageBackgroundPanel);
            this.SettingsTabPage.Location = new System.Drawing.Point(4, 31);
            this.SettingsTabPage.Margin = new System.Windows.Forms.Padding(0);
            this.SettingsTabPage.Name = "SettingsTabPage";
            this.SettingsTabPage.Size = new System.Drawing.Size(590, 299);
            this.SettingsTabPage.TabIndex = 1;
            this.SettingsTabPage.Text = "🔧Settings";
            // 
            // SettingsTabPageBackgroundPanel
            // 
            this.SettingsTabPageBackgroundPanel.Controls.Add(this.SettingsTabControl);
            this.SettingsTabPageBackgroundPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SettingsTabPageBackgroundPanel.Location = new System.Drawing.Point(0, 0);
            this.SettingsTabPageBackgroundPanel.Margin = new System.Windows.Forms.Padding(0);
            this.SettingsTabPageBackgroundPanel.Name = "SettingsTabPageBackgroundPanel";
            this.SettingsTabPageBackgroundPanel.Size = new System.Drawing.Size(590, 299);
            this.SettingsTabPageBackgroundPanel.TabIndex = 0;
            // 
            // SettingsTabControl
            // 
            this.SettingsTabControl.Controls.Add(this.SettingsTabGeneralTabPage);
            this.SettingsTabControl.Controls.Add(this.SettingsTabDisplayTabPage);
            this.SettingsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SettingsTabControl.Location = new System.Drawing.Point(0, 0);
            this.SettingsTabControl.Margin = new System.Windows.Forms.Padding(0);
            this.SettingsTabControl.Name = "SettingsTabControl";
            this.SettingsTabControl.Padding = new System.Drawing.Point(14, 6);
            this.SettingsTabControl.SelectedIndex = 0;
            this.SettingsTabControl.Size = new System.Drawing.Size(590, 299);
            this.SettingsTabControl.TabIndex = 0;
            this.SettingsTabControl.SelectedIndexChanged += new System.EventHandler(this.SettingsTabControl_SelectedIndexChanged);
            // 
            // SettingsTabGeneralTabPage
            // 
            this.SettingsTabGeneralTabPage.Controls.Add(this.SettingsTabGeneralTabPageBackgroundPanel);
            this.SettingsTabGeneralTabPage.Location = new System.Drawing.Point(4, 31);
            this.SettingsTabGeneralTabPage.Margin = new System.Windows.Forms.Padding(0);
            this.SettingsTabGeneralTabPage.Name = "SettingsTabGeneralTabPage";
            this.SettingsTabGeneralTabPage.Size = new System.Drawing.Size(582, 264);
            this.SettingsTabGeneralTabPage.TabIndex = 0;
            this.SettingsTabGeneralTabPage.Text = "★General";
            // 
            // SettingsTabGeneralTabPageBackgroundPanel
            // 
            this.SettingsTabGeneralTabPageBackgroundPanel.Controls.Add(this.SettingsTabGeneralTabPageTableLayoutPanel);
            this.SettingsTabGeneralTabPageBackgroundPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SettingsTabGeneralTabPageBackgroundPanel.Location = new System.Drawing.Point(0, 0);
            this.SettingsTabGeneralTabPageBackgroundPanel.Margin = new System.Windows.Forms.Padding(0);
            this.SettingsTabGeneralTabPageBackgroundPanel.Name = "SettingsTabGeneralTabPageBackgroundPanel";
            this.SettingsTabGeneralTabPageBackgroundPanel.Padding = new System.Windows.Forms.Padding(16, 14, 16, 14);
            this.SettingsTabGeneralTabPageBackgroundPanel.Size = new System.Drawing.Size(582, 264);
            this.SettingsTabGeneralTabPageBackgroundPanel.TabIndex = 0;
            // 
            // SettingsTabGeneralTabPageTableLayoutPanel
            // 
            this.SettingsTabGeneralTabPageTableLayoutPanel.ColumnCount = 1;
            this.SettingsTabGeneralTabPageTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.SettingsTabGeneralTabPageTableLayoutPanel.Controls.Add(this.CurrentGameLabel, 0, 0);
            this.SettingsTabGeneralTabPageTableLayoutPanel.Controls.Add(this.ManageGameTableLayoutPanel1, 0, 1);
            this.SettingsTabGeneralTabPageTableLayoutPanel.Controls.Add(this.ThemeSelectLabel, 0, 2);
            this.SettingsTabGeneralTabPageTableLayoutPanel.Controls.Add(this.ThemeSelectTableLayoutPanel, 0, 3);
            this.SettingsTabGeneralTabPageTableLayoutPanel.Controls.Add(this.OtherSettingsFlowLayoutPanel, 0, 4);
            this.SettingsTabGeneralTabPageTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SettingsTabGeneralTabPageTableLayoutPanel.Location = new System.Drawing.Point(16, 14);
            this.SettingsTabGeneralTabPageTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.SettingsTabGeneralTabPageTableLayoutPanel.Name = "SettingsTabGeneralTabPageTableLayoutPanel";
            this.SettingsTabGeneralTabPageTableLayoutPanel.RowCount = 5;
            this.SettingsTabGeneralTabPageTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.SettingsTabGeneralTabPageTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.SettingsTabGeneralTabPageTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.SettingsTabGeneralTabPageTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.SettingsTabGeneralTabPageTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.SettingsTabGeneralTabPageTableLayoutPanel.Size = new System.Drawing.Size(550, 236);
            this.SettingsTabGeneralTabPageTableLayoutPanel.TabIndex = 0;
            // 
            // CurrentGameLabel
            // 
            this.CurrentGameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CurrentGameLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.CurrentGameLabel.Location = new System.Drawing.Point(0, 0);
            this.CurrentGameLabel.Margin = new System.Windows.Forms.Padding(0);
            this.CurrentGameLabel.Name = "CurrentGameLabel";
            this.CurrentGameLabel.Size = new System.Drawing.Size(550, 22);
            this.CurrentGameLabel.TabIndex = 0;
            this.CurrentGameLabel.Text = "Current Game:";
            this.CurrentGameLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // ManageGameTableLayoutPanel1
            // 
            this.ManageGameTableLayoutPanel1.ColumnCount = 2;
            this.ManageGameTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ManageGameTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.ManageGameTableLayoutPanel1.Controls.Add(this.CurrentGamePanel1, 0, 0);
            this.ManageGameTableLayoutPanel1.Controls.Add(this.AddGameButton, 1, 0);
            this.ManageGameTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ManageGameTableLayoutPanel1.Location = new System.Drawing.Point(0, 22);
            this.ManageGameTableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.ManageGameTableLayoutPanel1.Name = "ManageGameTableLayoutPanel1";
            this.ManageGameTableLayoutPanel1.RowCount = 1;
            this.ManageGameTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ManageGameTableLayoutPanel1.Size = new System.Drawing.Size(550, 25);
            this.ManageGameTableLayoutPanel1.TabIndex = 1;
            // 
            // CurrentGamePanel1
            // 
            this.CurrentGamePanel1.Controls.Add(this.CurrentGameTitleTextBox);
            this.CurrentGamePanel1.Controls.Add(this.CurrentGameComboBox);
            this.CurrentGamePanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CurrentGamePanel1.Location = new System.Drawing.Point(0, 1);
            this.CurrentGamePanel1.Margin = new System.Windows.Forms.Padding(0, 1, 6, 1);
            this.CurrentGamePanel1.Name = "CurrentGamePanel1";
            this.CurrentGamePanel1.Size = new System.Drawing.Size(504, 23);
            this.CurrentGamePanel1.TabIndex = 0;
            // 
            // CurrentGameTitleTextBox
            // 
            this.CurrentGameTitleTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CurrentGameTitleTextBox.Location = new System.Drawing.Point(0, 0);
            this.CurrentGameTitleTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.CurrentGameTitleTextBox.Name = "CurrentGameTitleTextBox";
            this.CurrentGameTitleTextBox.ReadOnly = true;
            this.CurrentGameTitleTextBox.Size = new System.Drawing.Size(504, 23);
            this.CurrentGameTitleTextBox.TabIndex = 1;
            // 
            // CurrentGameComboBox
            // 
            this.CurrentGameComboBox.Cursor = System.Windows.Forms.Cursors.PanSouth;
            this.CurrentGameComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CurrentGameComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CurrentGameComboBox.FormattingEnabled = true;
            this.CurrentGameComboBox.Location = new System.Drawing.Point(0, 0);
            this.CurrentGameComboBox.Margin = new System.Windows.Forms.Padding(0);
            this.CurrentGameComboBox.Name = "CurrentGameComboBox";
            this.CurrentGameComboBox.Size = new System.Drawing.Size(504, 23);
            this.CurrentGameComboBox.TabIndex = 0;
            this.CurrentGameComboBox.SelectedIndexChanged += new System.EventHandler(this.CurrentGameComboBox_SelectedIndexChanged);
            // 
            // AddGameButton
            // 
            this.AddGameButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AddGameButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AddGameButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.AddGameButton.Location = new System.Drawing.Point(516, 1);
            this.AddGameButton.Margin = new System.Windows.Forms.Padding(6, 1, 0, 1);
            this.AddGameButton.Name = "AddGameButton";
            this.AddGameButton.Size = new System.Drawing.Size(34, 23);
            this.AddGameButton.TabIndex = 1;
            this.AddGameButton.Text = "+";
            this.AddGameButton.UseVisualStyleBackColor = false;
            this.AddGameButton.Click += new System.EventHandler(this.AddGameLabel_Click);
            // 
            // ThemeSelectLabel
            // 
            this.ThemeSelectLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ThemeSelectLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.ThemeSelectLabel.Location = new System.Drawing.Point(0, 47);
            this.ThemeSelectLabel.Margin = new System.Windows.Forms.Padding(0);
            this.ThemeSelectLabel.Name = "ThemeSelectLabel";
            this.ThemeSelectLabel.Size = new System.Drawing.Size(550, 26);
            this.ThemeSelectLabel.TabIndex = 2;
            this.ThemeSelectLabel.Text = "Theme:";
            this.ThemeSelectLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // ThemeSelectTableLayoutPanel
            // 
            this.ThemeSelectTableLayoutPanel.ColumnCount = 5;
            this.ThemeSelectTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ThemeSelectTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ThemeSelectTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ThemeSelectTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ThemeSelectTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ThemeSelectTableLayoutPanel.Controls.Add(this.SelectThemeComboBox, 0, 0);
            this.ThemeSelectTableLayoutPanel.Controls.Add(this.EditThemeButton, 1, 0);
            this.ThemeSelectTableLayoutPanel.Controls.Add(this.SaveThemeButton, 2, 0);
            this.ThemeSelectTableLayoutPanel.Controls.Add(this.RenameThemeButton, 3, 0);
            this.ThemeSelectTableLayoutPanel.Controls.Add(this.DeleteThemeButton, 4, 0);
            this.ThemeSelectTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ThemeSelectTableLayoutPanel.Location = new System.Drawing.Point(0, 73);
            this.ThemeSelectTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.ThemeSelectTableLayoutPanel.Name = "ThemeSelectTableLayoutPanel";
            this.ThemeSelectTableLayoutPanel.RowCount = 1;
            this.ThemeSelectTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ThemeSelectTableLayoutPanel.Size = new System.Drawing.Size(550, 23);
            this.ThemeSelectTableLayoutPanel.TabIndex = 3;
            // 
            // SelectThemeComboBox
            // 
            this.SelectThemeComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SelectThemeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SelectThemeComboBox.FormattingEnabled = true;
            this.SelectThemeComboBox.Location = new System.Drawing.Point(0, 0);
            this.SelectThemeComboBox.Margin = new System.Windows.Forms.Padding(0);
            this.SelectThemeComboBox.Name = "SelectThemeComboBox";
            this.SelectThemeComboBox.Size = new System.Drawing.Size(254, 23);
            this.SelectThemeComboBox.TabIndex = 0;
            this.SelectThemeComboBox.SelectedIndexChanged += new System.EventHandler(this.SelectThemeComboBox_SelectedIndexChanged);
            // 
            // EditThemeButton
            // 
            this.EditThemeButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.EditThemeButton.AutoSize = true;
            this.EditThemeButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.EditThemeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.EditThemeButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.EditThemeButton.Location = new System.Drawing.Point(260, 0);
            this.EditThemeButton.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.EditThemeButton.Name = "EditThemeButton";
            this.EditThemeButton.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.EditThemeButton.Size = new System.Drawing.Size(56, 23);
            this.EditThemeButton.TabIndex = 1;
            this.EditThemeButton.Text = "Edit";
            this.EditThemeButton.UseVisualStyleBackColor = false;
            this.EditThemeButton.Click += new System.EventHandler(this.EditThemeButton_Click);
            // 
            // SaveThemeButton
            // 
            this.SaveThemeButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.SaveThemeButton.AutoSize = true;
            this.SaveThemeButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.SaveThemeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SaveThemeButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.SaveThemeButton.Location = new System.Drawing.Point(322, 0);
            this.SaveThemeButton.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.SaveThemeButton.Name = "SaveThemeButton";
            this.SaveThemeButton.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.SaveThemeButton.Size = new System.Drawing.Size(62, 23);
            this.SaveThemeButton.TabIndex = 1;
            this.SaveThemeButton.Text = "Save";
            this.SaveThemeButton.UseVisualStyleBackColor = false;
            this.SaveThemeButton.Click += new System.EventHandler(this.SaveThemeButton_Click);
            // 
            // RenameThemeButton
            // 
            this.RenameThemeButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.RenameThemeButton.AutoSize = true;
            this.RenameThemeButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.RenameThemeButton.Enabled = false;
            this.RenameThemeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RenameThemeButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.RenameThemeButton.Location = new System.Drawing.Point(390, 0);
            this.RenameThemeButton.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.RenameThemeButton.Name = "RenameThemeButton";
            this.RenameThemeButton.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.RenameThemeButton.Size = new System.Drawing.Size(81, 23);
            this.RenameThemeButton.TabIndex = 2;
            this.RenameThemeButton.Text = "Rename";
            this.RenameThemeButton.UseVisualStyleBackColor = false;
            this.RenameThemeButton.Click += new System.EventHandler(this.RenameThemeButton_Click);
            // 
            // DeleteThemeButton
            // 
            this.DeleteThemeButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.DeleteThemeButton.AutoSize = true;
            this.DeleteThemeButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.DeleteThemeButton.Enabled = false;
            this.DeleteThemeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DeleteThemeButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.DeleteThemeButton.Location = new System.Drawing.Point(477, 0);
            this.DeleteThemeButton.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.DeleteThemeButton.Name = "DeleteThemeButton";
            this.DeleteThemeButton.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.DeleteThemeButton.Size = new System.Drawing.Size(73, 23);
            this.DeleteThemeButton.TabIndex = 3;
            this.DeleteThemeButton.Text = "Delete";
            this.DeleteThemeButton.UseVisualStyleBackColor = false;
            this.DeleteThemeButton.Click += new System.EventHandler(this.DeleteThemeButton_Click);
            // 
            // OtherSettingsFlowLayoutPanel
            // 
            this.OtherSettingsFlowLayoutPanel.AutoScroll = true;
            this.OtherSettingsFlowLayoutPanel.Controls.Add(this.BepInExLogTableLayoutPanel);
            this.OtherSettingsFlowLayoutPanel.Controls.Add(this.VRGameCheckBox);
            this.OtherSettingsFlowLayoutPanel.Controls.Add(this.JPLauncherRunLinkLabel);
            this.OtherSettingsFlowLayoutPanel.Controls.Add(this.ExtraSettingsLinkLabel);
            this.OtherSettingsFlowLayoutPanel.Controls.Add(this.FixRegistryLinkLabel);
            this.OtherSettingsFlowLayoutPanel.Controls.Add(this.CreateShortcutLinkLabel);
            this.OtherSettingsFlowLayoutPanel.Controls.Add(this.AutoShortcutRegistryCheckBox);
            this.OtherSettingsFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OtherSettingsFlowLayoutPanel.Location = new System.Drawing.Point(0, 102);
            this.OtherSettingsFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.OtherSettingsFlowLayoutPanel.Name = "OtherSettingsFlowLayoutPanel";
            this.OtherSettingsFlowLayoutPanel.Size = new System.Drawing.Size(550, 134);
            this.OtherSettingsFlowLayoutPanel.TabIndex = 4;
            this.OtherSettingsFlowLayoutPanel.WrapContents = false;
            // 
            // BepInExLogTableLayoutPanel
            // 
            this.BepInExLogTableLayoutPanel.AutoSize = true;
            this.BepInExLogTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BepInExLogTableLayoutPanel.ColumnCount = 2;
            this.BepInExLogTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.BepInExLogTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.BepInExLogTableLayoutPanel.Controls.Add(this.BepInExConsoleCheckBox, 0, 0);
            this.BepInExLogTableLayoutPanel.Controls.Add(this.OpenLogLinkLabel, 1, 0);
            this.BepInExLogTableLayoutPanel.Location = new System.Drawing.Point(0, 4);
            this.BepInExLogTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.BepInExLogTableLayoutPanel.Name = "BepInExLogTableLayoutPanel";
            this.BepInExLogTableLayoutPanel.RowCount = 1;
            this.BepInExLogTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.BepInExLogTableLayoutPanel.Size = new System.Drawing.Size(88, 15);
            this.BepInExLogTableLayoutPanel.TabIndex = 0;
            // 
            // BepInExConsoleCheckBox
            // 
            this.BepInExConsoleCheckBox.AutoSize = true;
            this.BepInExConsoleCheckBox.Location = new System.Drawing.Point(0, 0);
            this.BepInExConsoleCheckBox.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.BepInExConsoleCheckBox.Name = "BepInExConsoleCheckBox";
            this.BepInExConsoleCheckBox.Size = new System.Drawing.Size(15, 14);
            this.BepInExConsoleCheckBox.TabIndex = 0;
            this.BepInExConsoleCheckBox.UseVisualStyleBackColor = true;
            this.BepInExConsoleCheckBox.CheckedChanged += new System.EventHandler(this.ConsoleCheckBox_CheckedChanged);
            // 
            // OpenLogLinkLabel
            // 
            this.OpenLogLinkLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.OpenLogLinkLabel.AutoSize = true;
            this.OpenLogLinkLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.OpenLogLinkLabel.Location = new System.Drawing.Point(19, 0);
            this.OpenLogLinkLabel.Margin = new System.Windows.Forms.Padding(0);
            this.OpenLogLinkLabel.Name = "OpenLogLinkLabel";
            this.OpenLogLinkLabel.Size = new System.Drawing.Size(69, 15);
            this.OpenLogLinkLabel.TabIndex = 1;
            this.OpenLogLinkLabel.TabStop = true;
            this.OpenLogLinkLabel.Text = "BepInEx log";
            this.OpenLogLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OpenLogLinkLabel_LinkClicked);
            // 
            // VRGameCheckBox
            // 
            this.VRGameCheckBox.AutoSize = true;
            this.VRGameCheckBox.Location = new System.Drawing.Point(98, 4);
            this.VRGameCheckBox.Margin = new System.Windows.Forms.Padding(10, 4, 0, 0);
            this.VRGameCheckBox.Name = "VRGameCheckBox";
            this.VRGameCheckBox.Size = new System.Drawing.Size(36, 19);
            this.VRGameCheckBox.TabIndex = 1;
            this.VRGameCheckBox.Text = "vr";
            this.VRGameCheckBox.UseVisualStyleBackColor = true;
            this.VRGameCheckBox.Visible = false;
            // 
            // JPLauncherRunLinkLabel
            // 
            this.JPLauncherRunLinkLabel.AutoSize = true;
            this.JPLauncherRunLinkLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.JPLauncherRunLinkLabel.Location = new System.Drawing.Point(144, 6);
            this.JPLauncherRunLinkLabel.Margin = new System.Windows.Forms.Padding(10, 6, 0, 0);
            this.JPLauncherRunLinkLabel.Name = "JPLauncherRunLinkLabel";
            this.JPLauncherRunLinkLabel.Size = new System.Drawing.Size(70, 15);
            this.JPLauncherRunLinkLabel.TabIndex = 2;
            this.JPLauncherRunLinkLabel.TabStop = true;
            this.JPLauncherRunLinkLabel.Text = "JP Launcher";
            this.JPLauncherRunLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel1_LinkClicked);
            // 
            // ExtraSettingsLinkLabel
            // 
            this.ExtraSettingsLinkLabel.AutoSize = true;
            this.ExtraSettingsLinkLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ExtraSettingsLinkLabel.Location = new System.Drawing.Point(224, 6);
            this.ExtraSettingsLinkLabel.Margin = new System.Windows.Forms.Padding(10, 6, 0, 0);
            this.ExtraSettingsLinkLabel.Name = "ExtraSettingsLinkLabel";
            this.ExtraSettingsLinkLabel.Size = new System.Drawing.Size(78, 15);
            this.ExtraSettingsLinkLabel.TabIndex = 3;
            this.ExtraSettingsLinkLabel.TabStop = true;
            this.ExtraSettingsLinkLabel.Text = "Extra Settings";
            this.ExtraSettingsLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ExtraSettingsLinkLabel_LinkClicked);
            // 
            // FixRegistryLinkLabel
            // 
            this.FixRegistryLinkLabel.AutoSize = true;
            this.FixRegistryLinkLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.FixRegistryLinkLabel.Location = new System.Drawing.Point(312, 6);
            this.FixRegistryLinkLabel.Margin = new System.Windows.Forms.Padding(10, 6, 0, 0);
            this.FixRegistryLinkLabel.Name = "FixRegistryLinkLabel";
            this.FixRegistryLinkLabel.Size = new System.Drawing.Size(64, 15);
            this.FixRegistryLinkLabel.TabIndex = 4;
            this.FixRegistryLinkLabel.TabStop = true;
            this.FixRegistryLinkLabel.Text = "Fix registry";
            this.FixRegistryLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.FixRegistryButton_Click);
            this.FixRegistryLinkLabel.Click += new System.EventHandler(this.FixRegistryButton_Click);
            // 
            // CreateShortcutLinkLabel
            // 
            this.CreateShortcutLinkLabel.AutoSize = true;
            this.CreateShortcutLinkLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.CreateShortcutLinkLabel.Location = new System.Drawing.Point(386, 6);
            this.CreateShortcutLinkLabel.Margin = new System.Windows.Forms.Padding(10, 6, 0, 0);
            this.CreateShortcutLinkLabel.Name = "CreateShortcutLinkLabel";
            this.CreateShortcutLinkLabel.Size = new System.Drawing.Size(52, 15);
            this.CreateShortcutLinkLabel.TabIndex = 5;
            this.CreateShortcutLinkLabel.TabStop = true;
            this.CreateShortcutLinkLabel.Text = "Shortcut";
            this.CreateShortcutLinkLabel.Click += new System.EventHandler(this.CreateShortcutButton_Click);
            // 
            // AutoShortcutRegistryCheckBox
            // 
            this.AutoShortcutRegistryCheckBox.AutoSize = true;
            this.AutoShortcutRegistryCheckBox.Location = new System.Drawing.Point(448, 4);
            this.AutoShortcutRegistryCheckBox.Margin = new System.Windows.Forms.Padding(10, 4, 0, 0);
            this.AutoShortcutRegistryCheckBox.Name = "AutoShortcutRegistryCheckBox";
            this.AutoShortcutRegistryCheckBox.Size = new System.Drawing.Size(96, 19);
            this.AutoShortcutRegistryCheckBox.TabIndex = 6;
            this.AutoShortcutRegistryCheckBox.Text = "Autoshortcut";
            this.AutoShortcutRegistryCheckBox.UseVisualStyleBackColor = true;
            this.AutoShortcutRegistryCheckBox.Visible = false;
            this.AutoShortcutRegistryCheckBox.CheckedChanged += new System.EventHandler(this.AutoShortcutRegistryCheckBoxCheckBox_CheckedChanged);
            // 
            // SettingsTabDisplayTabPage
            // 
            this.SettingsTabDisplayTabPage.Controls.Add(this.SettingsTabDisplayTabPageGameBackgroundPanel);
            this.SettingsTabDisplayTabPage.Location = new System.Drawing.Point(4, 31);
            this.SettingsTabDisplayTabPage.Margin = new System.Windows.Forms.Padding(0);
            this.SettingsTabDisplayTabPage.Name = "SettingsTabDisplayTabPage";
            this.SettingsTabDisplayTabPage.Size = new System.Drawing.Size(582, 266);
            this.SettingsTabDisplayTabPage.TabIndex = 1;
            this.SettingsTabDisplayTabPage.Text = "🔳Display";
            // 
            // SettingsTabDisplayTabPageGameBackgroundPanel
            // 
            this.SettingsTabDisplayTabPageGameBackgroundPanel.AutoScroll = true;
            this.SettingsTabDisplayTabPageGameBackgroundPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SettingsTabDisplayTabPageGameBackgroundPanel.Location = new System.Drawing.Point(0, 0);
            this.SettingsTabDisplayTabPageGameBackgroundPanel.Margin = new System.Windows.Forms.Padding(0);
            this.SettingsTabDisplayTabPageGameBackgroundPanel.Name = "SettingsTabDisplayTabPageGameBackgroundPanel";
            this.SettingsTabDisplayTabPageGameBackgroundPanel.Padding = new System.Windows.Forms.Padding(10);
            this.SettingsTabDisplayTabPageGameBackgroundPanel.Size = new System.Drawing.Size(582, 266);
            this.SettingsTabDisplayTabPageGameBackgroundPanel.TabIndex = 0;
            // 
            // ToolsTabPage
            // 
            this.ToolsTabPage.Controls.Add(this.ToolsTabPageBackgroundPanel);
            this.ToolsTabPage.Location = new System.Drawing.Point(4, 31);
            this.ToolsTabPage.Margin = new System.Windows.Forms.Padding(0);
            this.ToolsTabPage.Name = "ToolsTabPage";
            this.ToolsTabPage.Size = new System.Drawing.Size(590, 299);
            this.ToolsTabPage.TabIndex = 2;
            this.ToolsTabPage.Text = "🔨Tools";
            // 
            // ToolsTabPageBackgroundPanel
            // 
            this.ToolsTabPageBackgroundPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ToolsTabPageBackgroundPanel.Location = new System.Drawing.Point(0, 0);
            this.ToolsTabPageBackgroundPanel.Margin = new System.Windows.Forms.Padding(0);
            this.ToolsTabPageBackgroundPanel.Name = "ToolsTabPageBackgroundPanel";
            this.ToolsTabPageBackgroundPanel.Size = new System.Drawing.Size(590, 299);
            this.ToolsTabPageBackgroundPanel.TabIndex = 0;
            // 
            // FoldersTabPage
            // 
            this.FoldersTabPage.Controls.Add(this.FoldersTabPageBackgroundPanel);
            this.FoldersTabPage.Location = new System.Drawing.Point(4, 31);
            this.FoldersTabPage.Margin = new System.Windows.Forms.Padding(0);
            this.FoldersTabPage.Name = "FoldersTabPage";
            this.FoldersTabPage.Size = new System.Drawing.Size(590, 299);
            this.FoldersTabPage.TabIndex = 3;
            this.FoldersTabPage.Text = "🗁Folders";
            // 
            // FoldersTabPageBackgroundPanel
            // 
            this.FoldersTabPageBackgroundPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FoldersTabPageBackgroundPanel.Location = new System.Drawing.Point(0, 0);
            this.FoldersTabPageBackgroundPanel.Margin = new System.Windows.Forms.Padding(0);
            this.FoldersTabPageBackgroundPanel.Name = "FoldersTabPageBackgroundPanel";
            this.FoldersTabPageBackgroundPanel.Size = new System.Drawing.Size(590, 299);
            this.FoldersTabPageBackgroundPanel.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 380);
            this.Controls.Add(this.MainFormBackgroundPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.Text = "AI Helper [Organized modpack]";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AI_Helper_FormClosing);
            this.LocationChanged += new System.EventHandler(this.AIHelper_LocationChanged);
            this.MainFormBackgroundPanel.ResumeLayout(false);
            this.FormRootTableLayoutPanel.ResumeLayout(false);
            this.FormTitleBarTableLayoutPanel.ResumeLayout(false);
            this.AIGirlHelperTabControl.ResumeLayout(false);
            this.LaunchTabPage.ResumeLayout(false);
            this.LaunchTabPageBackgroundPanel.ResumeLayout(false);
            this.LaunchTabTableLayoutPanel.ResumeLayout(false);
            this.LaunchTabTopTableLayoutPanel.ResumeLayout(false);
            this.LaunchTabTopAppGameTitleTableLayoutPanel.ResumeLayout(false);
            this.LaunchTabButtonsDownTableLayoutPanel.ResumeLayout(false);
            this.LaunchTabButtonsDownTableLayoutPanel.PerformLayout();
            this.LaunchTabSubMainButtonsTableLayoutPanel.ResumeLayout(false);
            this.SettingsTabPage.ResumeLayout(false);
            this.SettingsTabPageBackgroundPanel.ResumeLayout(false);
            this.SettingsTabControl.ResumeLayout(false);
            this.SettingsTabGeneralTabPage.ResumeLayout(false);
            this.SettingsTabGeneralTabPageBackgroundPanel.ResumeLayout(false);
            this.SettingsTabGeneralTabPageTableLayoutPanel.ResumeLayout(false);
            this.ManageGameTableLayoutPanel1.ResumeLayout(false);
            this.CurrentGamePanel1.ResumeLayout(false);
            this.CurrentGamePanel1.PerformLayout();
            this.ThemeSelectTableLayoutPanel.ResumeLayout(false);
            this.ThemeSelectTableLayoutPanel.PerformLayout();
            this.OtherSettingsFlowLayoutPanel.ResumeLayout(false);
            this.OtherSettingsFlowLayoutPanel.PerformLayout();
            this.BepInExLogTableLayoutPanel.ResumeLayout(false);
            this.BepInExLogTableLayoutPanel.PerformLayout();
            this.SettingsTabDisplayTabPage.ResumeLayout(false);
            this.ToolsTabPage.ResumeLayout(false);
            this.FoldersTabPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel MainFormBackgroundPanel;
        private System.Windows.Forms.TableLayoutPanel FormRootTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel FormTitleBarTableLayoutPanel;
        internal System.Windows.Forms.Label FormTitleLabel;
        internal System.Windows.Forms.Button FormMinimizeButton;
        internal System.Windows.Forms.Button FormCloseButton;
        internal AIHelper.Manage.ui.ThemedTabControl AIGirlHelperTabControl;
        internal System.Windows.Forms.TabPage LaunchTabPage;
        private System.Windows.Forms.Panel LaunchTabPageBackgroundPanel;
        private System.Windows.Forms.TableLayoutPanel LaunchTabTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel LaunchTabTopTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel LaunchTabTopAppGameTitleTableLayoutPanel;
        private System.Windows.Forms.Label LaunchTabTheAppTitleAccentLabel;
        internal System.Windows.Forms.Label SelectedGameTitleMutedLabel;
        private System.Windows.Forms.TableLayoutPanel LaunchTabButtonsDownTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel LaunchTabSubMainButtonsTableLayoutPanel;
        private System.Windows.Forms.Button GameButton;
        private System.Windows.Forms.Button StudioButton;
        private System.Windows.Forms.Button MOButton;
        private System.Windows.Forms.Button SettingsButton;
        internal System.Windows.Forms.TableLayoutPanel FunctionsForFLPTableLayoutPanel;
        internal System.Windows.Forms.TabPage SettingsTabPage;
        private System.Windows.Forms.Panel SettingsTabPageBackgroundPanel;
        private AIHelper.Manage.ui.ThemedTabControl SettingsTabControl;
        private System.Windows.Forms.TabPage SettingsTabGeneralTabPage;
        private System.Windows.Forms.Panel SettingsTabGeneralTabPageBackgroundPanel;
        private System.Windows.Forms.TableLayoutPanel SettingsTabGeneralTabPageTableLayoutPanel;
        private System.Windows.Forms.Label CurrentGameLabel;
        private System.Windows.Forms.TableLayoutPanel ManageGameTableLayoutPanel1;
        private System.Windows.Forms.Panel CurrentGamePanel1;
        internal AIHelper.Manage.ui.ThemedTextBox CurrentGameTitleTextBox;
        internal AIHelper.Manage.ui.ThemedComboBox CurrentGameComboBox;
        private System.Windows.Forms.Button AddGameButton;
        private System.Windows.Forms.Label ThemeSelectLabel;
        internal AIHelper.Manage.ui.ThemedComboBox SelectThemeComboBox;
        private System.Windows.Forms.TableLayoutPanel ThemeSelectTableLayoutPanel;
        private System.Windows.Forms.Button EditThemeButton;
        private System.Windows.Forms.Button SaveThemeButton;
        private System.Windows.Forms.Button RenameThemeButton;
        private System.Windows.Forms.Button DeleteThemeButton;
        private System.Windows.Forms.FlowLayoutPanel OtherSettingsFlowLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel BepInExLogTableLayoutPanel;
        private AIHelper.Manage.ui.ThemedCheckBox BepInExConsoleCheckBox;
        private System.Windows.Forms.LinkLabel OpenLogLinkLabel;
        internal AIHelper.Manage.ui.ThemedCheckBox VRGameCheckBox;
        private System.Windows.Forms.LinkLabel JPLauncherRunLinkLabel;
        private System.Windows.Forms.LinkLabel ExtraSettingsLinkLabel;
        private System.Windows.Forms.LinkLabel FixRegistryLinkLabel;
        private System.Windows.Forms.LinkLabel CreateShortcutLinkLabel;
        internal AIHelper.Manage.ui.ThemedCheckBox AutoShortcutRegistryCheckBox;
        private System.Windows.Forms.TabPage SettingsTabDisplayTabPage;
        private System.Windows.Forms.Panel SettingsTabDisplayTabPageGameBackgroundPanel;
        internal System.Windows.Forms.TabPage ToolsTabPage;
        internal System.Windows.Forms.Panel ToolsTabPageBackgroundPanel;
        internal System.Windows.Forms.TabPage FoldersTabPage;
        internal System.Windows.Forms.Panel FoldersTabPageBackgroundPanel;
    }
}
