using AIHelper.Manage;
using AIHelper.Manage.FoldersTab.Folders;
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
        /// Get or set path to setup.xml graphic settings for current game
        /// </summary>
        private static string SetupXmlPath { get => ManageSettings.SetupXmlPath; set => ManageSettings.SetupXmlPath = value; }
        /// <summary>
        /// Get or set MO mode for current game
        /// </summary>
        private static bool IsMoMode { get => ManageSettings.IsMoMode; set => ManageSettings.IsMoMode = value; }

        public MainForm()
        {
            InitializeComponent();

            ManageSettings.ApplicationStartupPath = Application.StartupPath;
            ManageSettings.ApplicationProductName = Application.ProductName;

            ManageSettings.MainForm = this; // set reference to the form for controls use

            //--- Resizable borderless form settings  
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.MinimumSize = new Size(480, 260);
            //---

            ManageMainFormService.CalcSizeDependOnDesktop(this);

            if (!ManageOther.SetListOfAddedGames(this))
            {
                this.Enabled = false;
                Application.Exit();
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
            QualityComboBox.Items.Add(T._("Perfomance"));
            QualityComboBox.Items.Add(T._("Normal"));
            QualityComboBox.Items.Add(T._("Quality"));

            this.LaunchTabPage.Text = string.Format("{0} {1}", T._("🚀"), T._("Launch"));
            this.GameButton.Text = string.Format("{0} {1}", T._("▶"), T._("Game"));
            this.StudioButton.Text = string.Format("{0} {1}", T._("🎨"), T._("Studio"));
            this.MOButton.Text = string.Format("{0} {1}", T._("📦"), T._("Manager"));
            this.SettingsButton.Text = string.Format("{0} {1}", T._("⚙"), T._("Settings"));
            this.LaunchTabTheAppTitleLabel.Text = T._("-  Ằ🌶Ḩelper  -");
            this.SelectedGameLabelOwnColor.Text = T._("GameTitle");
            this.SettingsTabPage.Text = string.Format("{0} {1}", T._("🔧"), T._("Settings"));
            this.SettingsTabGeneralTabPage.Text = string.Format("{0} {1}", T._("★"), T._("General"));
            this.CurrentGameLabel.Text = T._("Current Game:");
            this.AddGameButton.Text = T._("+");
            this.ThemeSelectLabel.Text = T._("Theme:");
            this.OpenLogLinkLabel.Text = T._("log");
            this.VRGameCheckBox.Text = T._("vr");
            this.JPLauncherRunLinkLabel.Text = T._("JP Launcher");
            this.ExtraSettingsLinkLabel.Text = T._("Extra Settings");
            this.FixRegistryLinkLabel.Text = T._("Fix registry");
            this.CreateShortcutLinkLabel.Text = T._("Shortcut");
            this.AutoShortcutRegistryCheckBox.Text = T._("Autoshortcut");
            this.SettingsTabDisplayTabPage.Text = string.Format("{0} {1}", T._("🔳"), T._("Display"));
            this.OpenSetupXmlLinkLabel.Text = T._("Open game setup file");
            this.ResolutionLabel.Text = T._("Resolution:");
            this.QualityLabel.Text = T._("Quality:");
            this.FullScreenCheckBox.Text = T._("fullscreen");
            this.ToolsTabPage.Text = string.Format("{0} {1}", T._("🔨"), T._("Tools"));
            this.FoldersTabPage.Text = string.Format("{0} {1}", T._("🗁"), T._("Folders"));
            this.FormMinimizeButton.Text = T._("_");
            this.FormCloseButton.Text = T._("X");
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
        /// remove all tooltips and dispose resources
        /// </summary>
        internal void DisposeTooltips()
        {
            try
            {
                if (_thToolTip != null)
                {
                    _thToolTip.RemoveAll();
                    _thToolTip.Dispose();
                }
            }
            catch (Exception ex)
            {
                _log.Debug("An error occured while SetTooltips. error:\r\n" + ex);
            }
        }

        /// <summary>
        /// App's tooltips
        /// </summary>
        private ToolTip _thToolTip;
        internal void SetTooltips()
        {
            DisposeTooltips();

            //http://qaru.site/questions/47162/c-how-do-i-add-a-tooltip-to-a-control
            //THMainResetTableButton
            _thToolTip = new ToolTip
            {
                // Set up the delays for the ToolTip.
                AutoPopDelay = 32000,
                InitialDelay = 1000,
                ReshowDelay = 500,
                UseAnimation = true,
                UseFading = true,
                // Force the ToolTip text to be displayed whether or not the form is active.
                ShowAlways = true
            };

            if (AIGirlHelperTabControl.SelectedTab == LaunchTabPage)
            {
                _thToolTip.SetToolTip(SelectedGameLabelOwnColor, T._("Selected game title"));

                //Launch
                _thToolTip.SetToolTip(VRGameCheckBox, T._("Check to run VR exe instead on standart"));

                _thToolTip.SetToolTip(GameButton, IsMoMode ? T._("Will execute the Game")
                    + T._(" from Mod Organizer with attached mods")
                    : T._("Will execute the Game")
                    );
                _thToolTip.SetToolTip(StudioButton, IsMoMode ? T._("Will execute Studio")
                    + T._(" from Mod Organizer with attached mods")
                    : T._("Will execute Studio")
                    );
                _thToolTip.SetToolTip(MOButton, T._("Will execute Mod Organizer mod manager where you can manage your mods"));
                _thToolTip.SetToolTip(JPLauncherRunLinkLabel, IsMoMode ?
                      T._("Will execute original game launcher")
                    + T._(" from Mod Organizer with attached mods")
                    : T._("Will execute original game launcher")
                    );
                _thToolTip.SetToolTip(SettingsButton, T._("Will be opened Settings tab"));
                _thToolTip.SetToolTip(OpenLogLinkLabel, T._("Open BepinEx log if found"));
            }
            else if (AIGirlHelperTabControl.SelectedTab == SettingsTabPage)
            {
                _thToolTip.SetToolTip(AutoShortcutRegistryCheckBox, T._("When checked will create shortcut for the AI Helper on Desktop and will fix registry if need"));
                _thToolTip.SetToolTip(ResolutionComboBox, T._("Select preferred screen resolution"));
                _thToolTip.SetToolTip(FullScreenCheckBox, T._("When checked game will be in fullscreen mode"));
                _thToolTip.SetToolTip(QualityComboBox, T._("Select preferred graphics quality"));
                _thToolTip.SetToolTip(CreateShortcutLinkLabel, T._("Will create shortcut in Desktop if not exist"));
                _thToolTip.SetToolTip(FixRegistryLinkLabel, T._("Will set Data dir with game files as install dir in registry"));

                _thToolTip.SetToolTip(ExtraSettingsLinkLabel, T._("Open extra setting window for plugins and etc"));

                _thToolTip.SetToolTip(CurrentGameComboBox, T._("List of found games. Current") + ": " + ManageSettings.Games.Game.GameDisplayingName);


            }
            else if (AIGirlHelperTabControl.SelectedTab == ToolsTabPage)
            {

            }
            else if (AIGirlHelperTabControl.SelectedTab == FoldersTabPage)
            {
            }
            ////////////////////////////
        }

        private void SetScreenSettings()
        {
            if (!IsMoMode) SetupXmlPath = ManageSettings.CurrentGameSetupXmlFilePathinData;

            //set Settings
            if (!File.Exists(SetupXmlPath)) CreateSetupXmlPath();

            ResolutionComboBox.Text = ManageXml.ReadXmlValue(SetupXmlPath, "Setting/Size", ResolutionComboBox.Text);
            FullScreenCheckBox.Checked = bool.Parse(ManageXml.ReadXmlValue(SetupXmlPath, "Setting/FullScreen", FullScreenCheckBox.Checked + ""));

            string quality = ManageXml.ReadXmlValue(SetupXmlPath, "Setting/Quality", "2");
            //если качество будет за пределами диапазона 0-2, тогда будет равно 1 - нормально
            if (quality != "0" && quality != "1" && quality != "2") quality = "1";

            QualityComboBox.SelectedIndex = int.Parse(quality, CultureInfo.InvariantCulture);
        }

        private void CreateSetupXmlPath()
        {
            string screenWidth = Screen.PrimaryScreen.Bounds.Width.ToString(CultureInfo.InvariantCulture);
            
            int[] width = { 1280, 1366, 1536, 1600, 1920, 2048, 2560, 3200, 3840 };
            if (int.Parse(screenWidth, CultureInfo.InvariantCulture) > width[width.Length - 1])
            {
                ResolutionComboBox.SelectedItem = width.Length - 1;
                SetScreenResolution(ResolutionComboBox.Items[width.Length - 1].ToString());
            }
            else
            {
                for (int w = 0; w < width.Length; w++)
                {
                    if (int.Parse(screenWidth, CultureInfo.InvariantCulture) > width[w]) continue;

                    string selectedRes = ResolutionComboBox.Items[w].ToString();
                    ResolutionComboBox.Text = selectedRes;
                    SetScreenResolution(selectedRes);
                    break;
                }
            }
        }

        private static void SetScreenResolution(string resolution)
        {
            ManageModOrganizer.CheckMoUserdata();

            if (!File.Exists(SetupXmlPath))
            {
                // write default setup.xml
                Directory.CreateDirectory(Path.GetDirectoryName(SetupXmlPath));
                File.WriteAllText(SetupXmlPath, ManageSettings.DefaultSetupXmlValue, Encoding.GetEncoding("UTF-16"));
            }

            ManageXml.ChangeSetupXmlValue(SetupXmlPath, "Setting/Size", resolution);
            string[] wh = resolution.Replace("(16 : 9)", string.Empty).Trim().Split('x');
            ManageXml.ChangeSetupXmlValue(SetupXmlPath, "Setting/Width", wh[0].Trim());
            ManageXml.ChangeSetupXmlValue(SetupXmlPath, "Setting/Height", wh[1].Trim());
        }

        private static void SetGraphicsQuality(string quality)
        {
            ManageModOrganizer.CheckMoUserdata();

            ManageXml.ChangeSetupXmlValue(SetupXmlPath, "Setting/Quality", quality);
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

            SetScreenSettings();

            SetTooltips();

            if (AutoShortcutRegistryCheckBox.Checked) ManageOther.AutoShortcutAndRegystry();

            SelectedGameLabelOwnColor.Text = ManageSettings.Games.Game.GameDisplayingName + "❤";
            this.FormTitleLabel.Text = "AI Helper" + " | " + ManageSettings.Games.Game.GameDisplayingName;

            ThemesLoader.SetTheme();

            ManageTabs.LoadContent();
        }

        private void CommonModeSpecificSetup()
        {
            SetupXmlPath = ManageSettings.CurrentGameSetupXmlFilePath;

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

            SetupXmlPath = ManageModOrganizer.GetSetupXmlPathForCurrentProfile();
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

        private void ResolutionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetScreenResolution((sender as ComboBox).SelectedItem.ToString());
        }

        private void FullScreenCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ManageOther.CheckBoxChangeColor(sender as CheckBox);
            ManageXml.ChangeSetupXmlValue(SetupXmlPath, "Setting/FullScreen", (sender as CheckBox).Checked.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
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
            OnOffButtons(false);

            await Task.Run(() => ManageOther.WaitIfGameIsChanging()).ConfigureAwait(true);

            bool isVr = ManageSettings.CurrentGameIsHaveVr && ManageSettings.MainForm.VRGameCheckBox.Checked;

            string exePath;
            string arguments = string.Empty;
            string oldMOProfileName = "";
            if (IsMoMode)
            {
                var currentGameExemoProfileName = ManageSettings.CurrentGameExemoProfileName;
                var customExeTitleName = currentGameExemoProfileName + (isVr ? "VR" : "");
                exePath = ManageSettings.AppMOexePath; // set Mod organizer exe path

                ManageProcess.KillProcessesByName(ManageModOrganizer.GetExeNameByTitle(customExeTitleName));

                if (ManageModOrganizer.TryGetMOProfileNameByExeTitle(customExeTitleName, out string profileNameToRun))
                {
                    oldMOProfileName = ManageModOrganizer.SetCurrentProfileByName(profileNameToRun);
                }

                arguments = "moshortcut://:\"" + customExeTitleName + "\"";
            }
            else
            {
                exePath = Path.Combine(ManageSettings.CurrentGameDataDirPath, (isVr ? ManageSettings.CurrentGame.GameExeNameVr : ManageSettings.CurrentGameExeName) + ".exe");
            }

            ManageProcess.KillProcessesByName(Path.GetFileNameWithoutExtension(exePath));
            ManageProcess.RunProgramAndWaitHidden(exePath, arguments);

            if (IsMoMode && !string.IsNullOrEmpty(oldMOProfileName))
            {
                // return last profile
                ManageModOrganizer.SetCurrentProfileByName(oldMOProfileName);
            }
            OnOffButtons();
        }

        internal void OnOffButtons(bool switchOn = true)
        {
            AIGirlHelperTabControl.Invoke((Action)(() => AIGirlHelperTabControl.Enabled = switchOn));
        }

        private async void StudioButton_Click(object sender, EventArgs e)
        {
            OnOffButtons(false);

            await Task.Run(() => ManageOther.WaitIfGameIsChanging()).ConfigureAwait(true);

            string oldMOProfileName = "";
            if (IsMoMode)
            {
                ManageProcess.KillProcessesByName(ManageSettings.StudioExeName);
                ManageProcess.KillProcessesByName(Path.GetFileNameWithoutExtension(ManageSettings.AppMOexePath));

                var studio = ManageModOrganizer.GetMOcustomExecutableTitleByExeName(ManageSettings.StudioExeName);
                if (ManageModOrganizer.TryGetMOProfileNameByExeTitle(studio, out string profileNameToRun))
                {
                    oldMOProfileName = ManageModOrganizer.SetCurrentProfileByName(profileNameToRun);
                }
                ManageProcess.RunProgramAndWaitHidden(ManageSettings.AppMOexePath, "moshortcut://:" + studio);
            }
            else
            {
                ManageProcess.KillProcessesByName(ManageSettings.StudioExeName);

                var exe = Path.Combine(ManageSettings.CurrentGameDataDirPath, ManageSettings.StudioExeName + ".exe");
                ManageProcess.RunProgramAndWaitHidden(exe, string.Empty);
            }
            if (IsMoMode && !string.IsNullOrEmpty(oldMOProfileName))
            {
                // return last profile
                ManageModOrganizer.SetCurrentProfileByName(oldMOProfileName);
            }
            OnOffButtons();
        }

        private void QualityComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetGraphicsQuality((sender as ComboBox).SelectedIndex.ToString(CultureInfo.InvariantCulture));
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

                ThemesLoader.SetTheme(ManageSettings.CurrentTheme, _extraSettingsForm);
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
            var ini = ManageIni.GetINIFile(ManageSettings.AiHelperIniPath);

            foreach (var (condition, sectionName, keyName, value) in new (bool condition, string sectionName, string keyName, string value)[]
            {
                (ManageSettings.Games.Game != null, "Settings", "selected_game", ManageSettings.CurrentGameDirName),
                (this.Width > 0, "Settings", "Width", this.Width.ToString()),
                (this.Height > 0, "Settings", "Height", this.Height.ToString()),
            })
            {
                try
                {
                    if (!condition)
                    {
                        _log.Debug($"Skipping ini setting save for section '{sectionName}', key '{keyName}' due to condition being false.");
                        continue;
                    }
                    ini.SetKey(sectionName, keyName, value);
                }
                catch (Exception ex)
                {
                    _log.Debug("An error occurred in time of ini settings save while the app closing . error:\r\n" + ex);
                }
            }
        }

        private void SetupXmlPathLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(SetupXmlPath)) Process.Start("notepad.exe", SetupXmlPath);
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
    }
}