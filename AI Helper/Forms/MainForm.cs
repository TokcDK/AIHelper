using AIHelper.Forms.Other;
using AIHelper.Manage;
using AIHelper.Manage.FoldersTab.Folders;
using AIHelper.Manage.ui;
using AIHelper.Manage.ui.themes;
using AIHelper.Manage.Update;
using AIHelper.Manage.Update.Targets;
using CheckForEmptyDir;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AIHelper.Manage.ManageModOrganizer;

namespace AIHelper
{
    internal partial class MainForm : Form, IContainerControl
    {
        #region Resizable borderless form Constants

        // Windows Messages
        private const int WM_NCHITTEST = 0x0084;

        // Hit Test Results
        private const int HTCLIENT = 1;
        private const int HTCAPTION = 2;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;

        #endregion

        #region Resizable borderless form Properties

        /// <summary>
        /// Gets or sets the border thickness for resize detection.
        /// </summary>
        public int ResizeBorderThickness { get; set; } = 6;

        #endregion

        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        internal bool IsDebug;
        internal bool IsBetaTest;

        /// <summary>
        /// Get or set MO mode for current game
        /// </summary>
        private static bool IsMoMode { get => ManageSettings.IsMoMode; set => ManageSettings.IsMoMode = value; }
        public static MainForm Instance { get; private set; }

        public MainForm()
        {
            InitializeComponent();

            ManageSettings.ApplicationStartupPath = Application.StartupPath;
            ManageSettings.ApplicationProductName = Application.ProductName;

            Instance = this; // set instance for static access

            //--- Resizable borderless form settings  
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.MinimumSize = new Size(520, 360);
            //---

            // the launch page builds its function row while the application runs and
            // colours it to match the theme, so it has to be rebuilt when the theme
            // changes rather than just recoloured
            ThemesLoader.ThemeApplied += OnThemeApplied;

            ManageMainFormService.CalcSizeDependOnDesktop(this);

            if (!ManageOther.SetListOfAddedGames(this))
            {
                this.Enabled = false;
                Application.Exit();
                this.Close();
                return;
            }

            CheckMoAndEndInit();
        }

        #region Resizable borderless form WndProc Override

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_NCHITTEST && (int)m.Result == HTCLIENT)
            {
                m.Result = (IntPtr)GetHitTestResult(PointToClient(Cursor.Position));
            }
        }

        private int GetHitTestResult(Point cursor)
        {
            int resizeBorder = ResizeBorderThickness;
            int clientWidth = ClientSize.Width;
            int clientHeight = ClientSize.Height;

            bool isTop = cursor.Y < resizeBorder;
            bool isBottom = cursor.Y >= clientHeight - resizeBorder;
            bool isLeft = cursor.X < resizeBorder;
            bool isRight = cursor.X >= clientWidth - resizeBorder;

            if (isTop)
            {
                if (isLeft) return HTTOPLEFT;
                if (isRight) return HTTOPRIGHT;
                return HTTOP;
            }

            if (isBottom)
            {
                if (isLeft) return HTBOTTOMLEFT;
                if (isRight) return HTBOTTOMRIGHT;
                return HTBOTTOM;
            }

            if (isLeft) return HTLEFT;
            if (isRight) return HTRIGHT;

            return HTCLIENT;
        }

        #endregion

        private async void CheckMoAndEndInit()
        {
            //MO data parse
            Directory.CreateDirectory(ManageSettings.AppModOrganizerDirPath);

            await CheckUpdateModOrganizer();

            ManageSettings.MOIsNew = IsMo23OrNever();

            ManageModOrganizer.RedefineGameMoData();

            ManageModOrganizer.CleanMoFolder();
            //
            ManageModOrganizer.CheckBaseGamesPy();
            //
            ManageSettings.Games.Game.GameName = ManageModOrganizer.GetMoBasicGamePluginGameName();

            CleanLog();

            SetMoMode(false);
            ManageSettings.Games.Game.InitActions();

            SetLocalizationStrings();

            UpdateData();

            ManageSettings.INITDone = true;
        }

        private async Task CheckUpdateModOrganizer()
        {
            if (File.Exists(ManageSettings.AppMOexePath))
            {
                return;
            }

            var moDownloadOffer = MessageBox.Show(T._("Mod Organizer is missing in the app dir. Need to download latest version for to be able to manage mods by MO. Download latest?"), T._("Mod Organizer not found!"), MessageBoxButtons.OKCancel);
            if (DialogResult.OK != moDownloadOffer)
            {
                MessageBox.Show(T._("Application will exit now. You can manually put Mod Organizer in MO folder next to the program."));
                Directory.CreateDirectory("MO");

                ManageModOrganizer.OpenModOrganizerWebPage();

                this.Close();
                Application.Exit();
                return;
            }

            await new Updater(null).Update(new List<UpdateTargetBase>() { new Mo(new UpdateInfo(null)) }).ConfigureAwait(true);

        }

        private static void CleanLog()
        {
            try
            {
                if (!File.Exists(ManageLogs.LogFilePath)) return;

                var logsPath = Path.Combine(Path.GetDirectoryName(ManageLogs.LogFilePath), "logs", "old");
                Directory.CreateDirectory(logsPath);
                File.Move(ManageLogs.LogFilePath, Path.Combine(logsPath, Path.GetFileName(ManageLogs.LogFilePath)));
                _log.Info("Old log file moved to logs");
            }
            catch (IOException ex)
            {
                _log.Error("Error while old log file move:" + ex);
            }
        }

        private void SetLocalizationStrings()
        {
            this.Text = "AI Helper" + " | " + ManageSettings.Games.Game.GameDisplayingName;
            this.LaunchTabPage.Text = string.Format("{0} {1}", T._("🚀"), T._("Launch"));
            this.GameButton.Text = string.Format("{0} {1}", T._("▶"), T._("Game"));
            this.StudioButton.Text = string.Format("{0} {1}", T._("🎨"), T._("Studio"));
            this.MOButton.Text = string.Format("{0} {1}", T._("📦"), T._("Manager"));
            this.SettingsButton.Text = string.Format("{0} {1}", T._("⚙"), T._("Settings"));
            this.LaunchTabTheAppTitleAccentLabel.Text = T._("-  Ằ🌶Ḩelper  -");
            this.SelectedGameTitleMutedLabel.Text = T._("GameTitle");
            this.SettingsTabPage.Text = string.Format("{0} {1}", T._("🔧"), T._("Settings"));
            this.SettingsTabGeneralTabPage.Text = string.Format("{0} {1}", T._("★"), T._("General"));
            this.CurrentGameLabel.Text = T._("Current Game:");
            this.AddGameButton.Text = T._("+");
            this.ThemeSelectLabel.Text = T._("Theme:");
            this.SaveThemeButton.Text = T._("Save");
            this.RenameThemeButton.Text = T._("Rename");
            this.DeleteThemeButton.Text = T._("Delete");
            this.OpenLogLinkLabel.Text = T._("log");
            this.VRGameCheckBox.Text = T._("vr");
            this.JPLauncherRunLinkLabel.Text = T._("JP Launcher");
            this.ExtraSettingsLinkLabel.Text = T._("Extra Settings");
            this.FixRegistryLinkLabel.Text = T._("Fix registry");
            this.CreateShortcutLinkLabel.Text = T._("Shortcut");
            this.AutoShortcutRegistryCheckBox.Text = T._("Autoshortcut");
            this.SettingsTabDisplayTabPage.Text = string.Format("{0} {1}", T._("🔳"), T._("Display"));
            this.ToolsTabPage.Text = string.Format("{0} {1}", T._("🔨"), T._("Tools"));
            this.FoldersTabPage.Text = string.Format("{0} {1}", T._("🗁"), T._("Folders"));
            // the window chrome uses symbols rather than letters: they read the same in
            // every language and they are what a window is expected to look like
            this.FormMinimizeButton.Text = "–";
            this.FormCloseButton.Text = "✕";
            this.FormTitleLabel.Text = T._("AIHelper");
            this.Text = string.Format("{0} [{1}]", T._("AI Helper"), T._("Organized modpack"));
        }

        /// <summary>
        /// private int _mode;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void MainService_Click(object sender, EventArgs e)
        {
            AIGirlHelperTabControl.SelectedTab = LaunchTabPage;
            return;
        }

        /// <summary>
        /// The theme the list is showing, when it is one the user made.
        /// </summary>
        /// <remarks>
        /// Only a theme the user made can be renamed or deleted: the others are compiled
        /// into the application and would come back on the next start.
        /// </remarks>
        private CustomTheme SelectedCustomTheme
        {
            get { return SelectThemeComboBox.SelectedItem as CustomTheme; }
        }

        /// <summary>
        /// Keeps the colours which are on screen now as a theme of the user's own.
        /// </summary>
        /// <remarks>
        /// This is what the random theme is for: it draws a new set every time it is
        /// applied, and this is how a set worth keeping is kept.
        /// </remarks>
        private void SaveThemeButton_Click(object sender, EventArgs e)
        {
            var name = ThemeNameDialogForm.Ask(
                this,
                T._("Save theme"),
                CustomTheme.SuggestName(),
                candidate => DescribeThemeNameProblem(candidate, null));

            if (name == null) return;

            if (ThemesLoader.SaveCurrentAsCustomTheme(name) == null)
            {
                MessageBox.Show(this,
                    T._("The theme could not be saved. See the log for details."),
                    T._("Save theme"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            UpdateCustomThemeButtons();
        }

        /// <summary>
        /// Renames the theme the list is showing.
        /// </summary>
        private void RenameThemeButton_Click(object sender, EventArgs e)
        {
            var theme = SelectedCustomTheme;
            if (theme == null) return;

            var name = ThemeNameDialogForm.Ask(
                this,
                T._("Rename theme"),
                theme.Name,
                candidate => DescribeThemeNameProblem(candidate, theme));

            if (name == null) return;

            if (!ThemesLoader.RenameCustomTheme(theme, name))
            {
                MessageBox.Show(this,
                    T._("The theme could not be renamed. See the log for details."),
                    T._("Rename theme"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            UpdateCustomThemeButtons();
        }

        /// <summary>
        /// Deletes the theme the list is showing, after asking.
        /// </summary>
        private void DeleteThemeButton_Click(object sender, EventArgs e)
        {
            var theme = SelectedCustomTheme;
            if (theme == null) return;

            var answer = MessageBox.Show(this,
                string.Format(T._("Delete the theme \"{0}\"?"), theme.Name),
                T._("Delete theme"),
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning);

            if (answer != DialogResult.OK) return;

            if (!ThemesLoader.DeleteCustomTheme(theme))
            {
                MessageBox.Show(this,
                    T._("The theme could not be deleted. See the log for details."),
                    T._("Delete theme"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            UpdateCustomThemeButtons();
        }

        /// <summary>
        /// Returns why a theme cannot be given a name, or <c>null</c> when it can.
        /// </summary>
        /// <param name="except">
        /// The theme being renamed, which is allowed to keep the name it already has.
        /// </param>
        private static string DescribeThemeNameProblem(string name, CustomTheme except)
        {
            if (string.IsNullOrEmpty(name)) return T._("A theme needs a name.");

            if (CustomThemeStore.IsNameTaken(name, except))
            {
                return string.Format(T._("There is already a theme named \"{0}\". Please use another name."), name);
            }

            return null;
        }

        /// <summary>
        /// Lets renaming and deleting be used only while a theme the user made is selected.
        /// </summary>
        /// <remarks>
        /// Called whenever the selection moves, including when it is moved by the theme
        /// loader rather than by the user.
        /// </remarks>
        private void UpdateCustomThemeButtons()
        {
            var isCustom = SelectedCustomTheme != null;

            RenameThemeButton.Enabled = isCustom;
            DeleteThemeButton.Enabled = isCustom;
        }

        private void SelectThemeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCustomThemeButtons();
        }

        /// <summary>
        /// remove all tooltips and dispose resources
        /// </summary>
        internal void DisposeTooltips()
        {
            if (_thToolTip == null) return;

            _thToolTip.Dispose();
            _thToolTip = null;
        }

        /// <summary>
        /// App's tooltips
        /// </summary>
        private UiTooltip _thToolTip;
        internal void SetTooltips()
        {
            DisposeTooltips();

            _thToolTip = new UiTooltip();

            if (AIGirlHelperTabControl.SelectedTab == LaunchTabPage)
            {
                _thToolTip.Set(SelectedGameTitleMutedLabel, T._("Selected game title"));

                //Launch
                _thToolTip.Set(VRGameCheckBox, T._("Check to run VR exe instead on standart"));

                _thToolTip.Set(GameButton, IsMoMode ? T._("Will execute the Game")
                    + T._(" from Mod Organizer with attached mods")
                    : T._("Will execute the Game")
                    );
                _thToolTip.Set(StudioButton, IsMoMode ? T._("Will execute Studio")
                    + T._(" from Mod Organizer with attached mods")
                    : T._("Will execute Studio")
                    );
                _thToolTip.Set(MOButton, T._("Will execute Mod Organizer mod manager where you can manage your mods"));
                _thToolTip.Set(JPLauncherRunLinkLabel, IsMoMode ?
                      T._("Will execute original game launcher")
                    + T._(" from Mod Organizer with attached mods")
                    : T._("Will execute original game launcher")
                    );
                _thToolTip.Set(SettingsButton, T._("Will be opened Settings tab"));
                _thToolTip.Set(OpenLogLinkLabel, T._("Open BepinEx log if found"));
            }
            else if (AIGirlHelperTabControl.SelectedTab == SettingsTabPage)
            {
                _thToolTip.Set(AutoShortcutRegistryCheckBox, T._("When checked will create shortcut for the AI Helper on Desktop and will fix registry if need"));
                _thToolTip.Set(CreateShortcutLinkLabel, T._("Will create shortcut in Desktop if not exist"));
                _thToolTip.Set(FixRegistryLinkLabel, T._("Will set Data dir with game files as install dir in registry"));

                _thToolTip.Set(ExtraSettingsLinkLabel, T._("Open extra setting window for plugins and etc"));

                _thToolTip.Set(SelectThemeComboBox, T._("Colours of the window. Themes you saved yourself are in this list too"));
                _thToolTip.Set(SaveThemeButton, T._("Save the colours which are on screen now as a new theme"));
                _thToolTip.Set(RenameThemeButton, T._("Give the selected theme another name. Only a theme you saved yourself can be renamed"));
                _thToolTip.Set(DeleteThemeButton, T._("Remove the selected theme. Only a theme you saved yourself can be deleted"));

                _thToolTip.Set(CurrentGameComboBox, T._("List of found games. Current") + ": " + ManageSettings.Games.Game.GameDisplayingName);
            }
        }

        /// <summary>
        /// reinit some parameters
        /// </summary>
        internal void UpdateData()
        {
            EnableDisableSomeTools();

            SetMoMode();

            Directory.CreateDirectory(ManageSettings.CurrentGameDataDirPath);
            if (IsMoMode && !Directory.Exists(ManageSettings.CurrentGameModsDirPath))
            {
                Directory.CreateDirectory(ManageSettings.CurrentGameModsDirPath);
            }

            Directory.CreateDirectory(ManageSettings.CurrentGameDataDirPath);

            if (IsMoMode)
            {
                MOModeSpecificSetup();
            }
            else
            {
                CommonModeSpecificSetup();
            }

            AIGirlHelperTabControl.SelectedTab = LaunchTabPage;
            CurrentGameComboBox.Text = ManageSettings.Games.Game.GameDirName;
            CurrentGameComboBox.SelectedIndex = ManageSettings.CurrentGameIndex;

            GetEnableDisableLaunchTabButtons();

            SetTooltips();

            if (AutoShortcutRegistryCheckBox.Checked) ManageOther.AutoShortcutAndRegystry();

            SelectedGameTitleMutedLabel.Text = ManageSettings.Games.Game.GameDisplayingName + "❤";
            this.FormTitleLabel.Text = "AI Helper" + " | " + ManageSettings.Games.Game.GameDisplayingName;

            ThemesLoader.SetTheme();

            ManageTabs.LoadContent();
        }

        /// <summary>
        /// Rebuilds the parts of the window which colour themselves, so they pick up
        /// the theme which was just applied.
        /// </summary>
        private void OnThemeApplied(object sender, EventArgs e)
        {
            ManageTabs.LoadContent();

            // the tab headers are measured while the theme is applied, and a longer
            // caption can make the strip wider than the window is
            EnsureWindowFitsContent();
        }

        /// <summary>
        /// Makes sure the window is wide enough for what it has to show.
        /// </summary>
        /// <remarks>
        /// Every tab header is as wide as the longest caption of the strip, so a caption
        /// which grew - a different language, or a wider font - can make the strip wider
        /// than the window, and a tab strip does not scroll: the last header is simply
        /// cut off. The window is grown instead. It is only ever grown and never shrunk,
        /// so this cannot undo a size the user chose.
        /// </remarks>
        private void EnsureWindowFitsContent()
        {
            if (!IsHandleCreated) return;

            var required = GetRequiredClientWidth();
            if (required <= 0) return;

            // a window wider than the screen would put its own window buttons out of
            // reach, so the screen wins when the two disagree
            var desktop = Screen.FromControl(this).WorkingArea.Width;
            if (required > desktop) required = desktop;

            // nothing on this window, but a borderless window is not assumed to be the
            // only kind there will ever be
            var frame = Width - ClientSize.Width;

            if (MinimumSize.Width < required + frame)
            {
                MinimumSize = new Size(required + frame, MinimumSize.Height);
            }

            if (ClientSize.Width < required)
            {
                Width = required + frame;
            }
        }

        /// <summary>
        /// Width the window body needs so that neither the tab strip nor the launch page
        /// is clipped.
        /// </summary>
        private int GetRequiredClientWidth()
        {
            var strip = ThemesLoader.MeasureTabStripWidth(AIGirlHelperTabControl);

            // the pages sit inside the tab strip, inset by its own frame
            var pageInset = AIGirlHelperTabControl.ClientSize.Width - AIGirlHelperTabControl.DisplayRectangle.Width;

            // the four launch buttons sit in a grid of a fixed width, and the page clips
            // them rather than scrolling
            var launchPage = LaunchTabSubMainButtonsTableLayoutPanel.PreferredSize.Width
                           + LaunchTabTableLayoutPanel.Padding.Horizontal
                           + Math.Max(0, pageInset);

            return Math.Max(strip, launchPage) + Padding.Horizontal;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // the first theme pass can run before the window has a handle, and the sizes
            // it needs are only final once the window is on screen
            EnsureWindowFitsContent();
        }

        private void CommonModeSpecificSetup()
        {
            StudioButton.Enabled = false;
        }

        private void MOModeSpecificSetup()
        {
            ManageModOrganizer.RestoreModlist();

            ManageModOrganizer.CheckBaseGamesPy();


            if (!ManageSettings.CurrentGameModsDirPath.IsNullOrEmptyDirectory("*", new string[1] { "_separator" }))
            {
                AIGirlHelperTabControl.SelectedTab = LaunchTabPage;
            }

            ManageModOrganizer.DummyFiles();

            ManageModOrganizer.MoIniFixes();

            ManageModOrganizer.MakeLinks();

            //try start in another thread for perfomance purposes
            new Thread(obj => RunSlowActions()).Start();
        }

        private static void SetMoMode(bool setText = true)
        {
            IsMoMode = !File.Exists(ManageSettings.CurrentGameMoToStandartConvertationOperationsListFilePath);
        }

        private void EnableDisableSomeTools()
        {
            IsDebug = Path.GetFileName(ManageSettings.ApplicationStartupPath) == "Debug" && File.Exists(Path.Combine(ManageSettings.ApplicationStartupPath, "IsDevDebugMode.txt"));
            IsBetaTest = File.Exists(Path.Combine(ManageSettings.ApplicationStartupPath, "IsBetaTest.txt"));

        }

        private static void RunSlowActions()
        {
            MOUSFSLoadingFix(true); // remove old fix files
            PreloadingSetup(); // instead of old patch

            ManageModOrganizer.SetModOrganizerIniSettingsForTheGame();
        }

        private void GetEnableDisableLaunchTabButtons()
        {
            if (AIGirlHelperTabControl.SelectedTab.Name != "LaunchTabPage") return;

            JPLauncherRunLinkLabel.Enabled = File.Exists(Path.Combine(ManageSettings.CurrentGameDataDirPath, ManageSettings.IniSettingsExeName + ".exe"));
            GameButton.Enabled = File.Exists(Path.Combine(ManageSettings.CurrentGameDataDirPath, ManageSettings.CurrentGameExeName + ".exe"));
            StudioButton.Enabled = File.Exists(Path.Combine(ManageSettings.CurrentGameDataDirPath, ManageSettings.StudioExeName + ".exe"));

            //Set BepInEx log data
            if (ManageSettings.BepInExCfgFilePath.Length > 0 && File.Exists(ManageSettings.BepInExCfgFilePath))
            {
                BepInExConsoleCheckBox.Enabled = true;
                try
                {
                    BepInExConsoleCheckBox.Checked = bool.Parse(ManageCfg.GetCfgValueIfExist(ManageSettings.BepInExCfgFilePath, "Enabled", "Logging.Console", "False")); // немного тормозит
                }
                catch (Exception ex)
                {
                    _log.Debug("An error occured while GetEnableDisableLaunchTabButtons. error:\r\n" + ex);
                    BepInExConsoleCheckBox.Checked = false;
                }
            }

            //VR
            VRGameCheckBox.Visible = ManageSettings.CurrentGameIsHaveVr;
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            ManageOther.CheckBoxChangeColor(sender as CheckBox);
            ManageSettings.AutoShortcutRegistryCheckBoxChecked = AutoShortcutRegistryCheckBox.Checked;
            ManageIni.GetINIFile(ManageSettings.AiHelperIniPath).SetKey("Settings", "autoCreateShortcutAndFixRegystry", ManageSettings.AutoShortcutRegistryCheckBoxChecked.ToString(CultureInfo.InvariantCulture));
        }

        private void FixRegistryButton_Click(object sender, EventArgs e)
        {
            FixRegistryLinkLabel.Enabled = false;

            ManageRegistry.FixRegistry(false);

            FixRegistryLinkLabel.Enabled = true;
        }

        private async void MOButton_Click(object sender, EventArgs e)
        {
            OnOffButtons(false);

            await Task.Run(() => ManageOther.WaitIfGameIsChanging()).ConfigureAwait(true);

            if (IsMoMode)
            {
                ManageProcess.KillProcessesByName(Path.GetFileNameWithoutExtension(ManageSettings.AppMOexePath));
                ManageProcess.RunProgramAndWaitHidden(ManageSettings.AppMOexePath, string.Empty);
            }
            else
            {
                MessageBox.Show(T._("Game in Common mode now.\n To execute Mod Organizer convert game back\n to MO mode by button in Tools tab"));
            }
            OnOffButtons();
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            AIGirlHelperTabControl.SelectedTab = SettingsTabPage;
        }

        private async void GameButton_Click(object sender, EventArgs e)
        {
            await ManageProcess.RunMainGameStudioExe(true);
        }

        private async void StudioButton_Click(object sender, EventArgs e)
        {
            await ManageProcess.RunMainGameStudioExe(false);
        }

        internal void OnOffButtons(bool switchOn = true)
        {
            AIGirlHelperTabControl.Invoke((Action)(() => AIGirlHelperTabControl.Enabled = switchOn));
        }

        private void AIHelper_LocationChanged(object sender, EventArgs e)
        {
            if (ManageSettings.Games.Game == null) return;

            //move second form with main form
            //https://stackoverflow.com/questions/3429445/how-to-move-two-windows-forms-together
            if (_extraSettingsForm != null && !_extraSettingsForm.IsDisposed)
            {
                if (_extraSettingsForm.WindowState == FormWindowState.Minimized) _extraSettingsForm.WindowState = FormWindowState.Normal;

                _extraSettingsForm.Location = new Point(Bounds.Location.X + (Bounds.Width / 2) - (_extraSettingsForm.Width / 2),
                    Bounds.Location.Y + /*(Bounds.Height / 2) - (f2.Height / 2) +*/ Bounds.Height);
            }
        }

        private void CreateShortcutButton_Click(object sender, EventArgs e)
        {
            ManageOther.CreateShortcuts(true, false);
        }

        private void OpenLogLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenBepinexLog();
        }

        private void CurrentGameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ManageSettings.INITDone && !ManageSettings.CurrentGameIsChanging && !ManageSettings.SetModOrganizerINISettingsForTheGame)
            {
                ManageSettings.CurrentGameIsChanging = true;

                ManageSettings.Games.Game = ManageSettings.Games.Games[(sender as ComboBox).SelectedIndex];
                ManageOther.SetSelectedGameIndexAndBasicVariables(this);
                ActionsOnGameChanged();

                ManageIni.GetINIFile(ManageSettings.AiHelperIniPath).SetKey("Settings", "selected_game", ManageSettings.CurrentGameDirName);

                UpdateData();

                ManageSettings.CurrentGameIsChanging = false;
            }
            else
            {
                CurrentGameComboBox.SelectedItem = ManageSettings.CurrentGameDirName;
            }
        }

        private void ActionsOnGameChanged()
        {
            CloseExtraForms();
            ManageModOrganizer.RedefineGameMoData();
            ManageSettings.BepInExCfgFilePath = string.Empty;
            ManageSettings.MOSelectedProfileDirName = string.Empty;

            ManageModOrganizer.CheckBaseGamesPy();

            ManageSettings.Games.Game.InitActions();
            CurrentGameTitleTextBox.Text = ManageSettings.Games.Game.GameDisplayingName;
        }

        private void CloseExtraForms()
        {
            if (_extraSettingsForm != null && !_extraSettingsForm.IsDisposed) _extraSettingsForm.Close();
        }

        private void ConsoleCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var bepinExcfg = ManageSettings.BepInExCfgFilePath;
            ManageCfg.WriteCfgValue(bepinExcfg, "Logging.Console", "Enabled", /*" " +*/ (sender as CheckBox).Checked.ToString(CultureInfo.InvariantCulture));

        }

        internal ExtraSettingsForm _extraSettingsForm;
        private void ExtraSettingsLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_extraSettingsForm == null || _extraSettingsForm.IsDisposed)
            {
                //show and reposition of form
                //https://stackoverflow.com/questions/31492787/how-to-set-position-second-form-depend-on-first-form
                _extraSettingsForm = new ExtraSettingsForm
                {
                    StartPosition = FormStartPosition.Manual
                };
                _extraSettingsForm.Load += new EventHandler((o, ea) =>
                {
                    _extraSettingsForm.Location = ManageSettings.MainForm.Location;
                    _extraSettingsForm.Size = ManageSettings.MainForm.Size;
                });

                ThemesLoader.ApplyTheme(_extraSettingsForm);
                _extraSettingsForm.Show();
                _extraSettingsForm.TopMost = true;
            }
            else
            {
                _extraSettingsForm.Close();
            }
        }

        private async void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OnOffButtons(false);

            await Task.Run(() => ManageOther.WaitIfGameIsChanging()).ConfigureAwait(true);

            if (IsMoMode)
            {
                ManageProcess.RunProgramAndWaitHidden(ManageSettings.AppMOexePath, "moshortcut://:" + ManageSettings.IniSettingsExeName);
            }
            else
            {
                ManageProcess.RunProgramAndWaitHidden(Path.Combine(ManageSettings.CurrentGameDataDirPath, ManageSettings.IniSettingsExeName + ".exe"), string.Empty);
            }
            OnOffButtons();
        }

        private void AI_Helper_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (ManageSettings.Games.Game != null)
                {
                    ManageIni.GetINIFile(ManageSettings.AiHelperIniPath)
                        .SetKey(ManageSettings.IniSettingsSectionName, "selected_game", ManageSettings.CurrentGameDirName);
                }
            }
            catch (Exception ex)
            {
                _log.Debug("An error occurred in time of ini settings save while the app closing . error:\r\n" + ex);
            }

            // the size is written whenever the user stops resizing the window as well;
            // this covers a window closed before that write had a chance to happen
            ManageMainFormService.SaveSize(this);
        }

        private void AIGirlHelperTabControl_Selected(object sender, TabControlEventArgs e)
        {
            if (AIGirlHelperTabControl.SelectedTab.Name == "ToolsTabPage")
            {
            }
            else if (AIGirlHelperTabControl.SelectedTab.Name == "LaunchTabPage")
            {
                GetEnableDisableLaunchTabButtons();
            }
        }

        private void AIGirlHelperTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetTooltips();
            ManageTabs.LoadContent();
        }

        private void AddGameLabel_Click(object sender, EventArgs e)
        {
            ManageOther.AddNewGame(this);
        }

        private void SettingsTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (SettingsTabControl.SelectedIndex)
            {
                case 1:
                    ManageSettings.ReloadGameSettings(SettingsTabDisplayTabPageGameBackgroundPanel);

                    // the settings control of the game is created while the application
                    // runs, so it has to be themed after it is put in place
                    ThemesLoader.ApplyTheme(SettingsTabDisplayTabPageGameBackgroundPanel);
                    break;
                default:
                    break;
            }
        }
    }
}