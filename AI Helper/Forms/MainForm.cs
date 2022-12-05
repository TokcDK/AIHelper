using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AIHelper.Install.Types;
using AIHelper.Install.Types.Directories;
using AIHelper.Install.UpdateMaker;
using AIHelper.Manage;
using AIHelper.Manage.FoldersTab.Folders;
using AIHelper.Manage.Functions;
using AIHelper.Manage.Update;
using AIHelper.Manage.Update.Targets;
using CheckForEmptyDir;
using INIFileMan;
using NLog;
using static AIHelper.Manage.ManageModOrganizer;

//using Crc32C;

namespace AIHelper
{
    internal partial class MainForm : Form
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        //internal bool _compressmode;

        /// <summary>
        /// Get or set path to setup.xml graphic settings for current game
        /// </summary>
        private static string SetupXmlPath { get => ManageSettings.SetupXmlPath; set => ManageSettings.SetupXmlPath = value; }
        /// <summary>
        /// Get or set MO mode for current game
        /// </summary>
        private static bool MOmode { get => ManageSettings.IsMoMode; set => ManageSettings.IsMoMode = value; }

        public MainForm()
        {
            InitializeComponent();

            var resolution = Screen.PrimaryScreen.Bounds;
            this.Size = new Size((int)(resolution.Width / 3.5), (int)(resolution.Height / 4.5));

            ManageSettings.ApplicationStartupPath = Application.StartupPath;
            ManageSettings.ApplicationProductName = Application.ProductName;

            if (!ManageOther.SetListOfAddedGames(this))
            {
                this.Enabled = false;
                Application.Exit();
                return;
            }

            ManageSettings.MainForm = this; // set reference to the form for controls use

            CheckMoAndEndInit();

            ThemesLoader.ApplyDarkTheme();
        }

        private async void CheckMoAndEndInit()
        {
            //MO data parse
            Directory.CreateDirectory(ManageSettings.AppModOrganizerDirPath);

            //FixOldMODirPath();

            if (!File.Exists(ManageSettings.AppMOexePath))
            {
                var moDownloadOffer = MessageBox.Show(T._("Mod Organizer is missing in the app dir. Need to download latest version for to be able to manage mods by MO. Download latest?"), T._("Mod Organizer not found!"), MessageBoxButtons.OKCancel);
                if (DialogResult.OK != moDownloadOffer)
                {
                    MessageBox.Show(T._("Application will exit now. You can manually put Mod Organizer in MO folder nex to the program."));
                    Directory.CreateDirectory("MO");
                    using (var process = new Process())
                    {
                        try
                        {
                            process.StartInfo.UseShellExecute = true;
                            process.StartInfo.FileName = "https://github.com/Modorganizer2/modorganizer/releases";
                            process.Start();
                        }
                        catch (Exception e)
                        {
                            _log.Error($"Failed to open link to MO github page. Error:{e.Message}");
                        }
                    }

                    Application.Exit();
                    return;
                }

                await new Updater(null).Update(new List<UpdateTargetBase>() { new Mo(new UpdateInfo(null)) }).ConfigureAwait(true);
            }

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

            //if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\illusion\AI-Syoujyo\AI-SyoujyoTrial", "INSTALLDIR", null) == null)
            //{
            //    FixRegistryButton.Visible = true;
            //}

            ManageSettings.INITDone = true;
        }

        //private void FixOldMODirPath()
        //{
        //    return;

        //if (Directory.Exists(ManageSettings.GetAppOldModOrganizerDirPath()))
        //{
        //    foreach(var possibleFileSymlink in new[]
        //    {
        //        Path.Combine(ManageSettings.GetAppOldModOrganizerDirPath(), ManageSettings.MoCategoriesFileName()),
        //        Path.Combine(ManageSettings.GetAppOldModOrganizerDirPath(), ManageSettings.MoIniFileName()),
        //    })
        //    {
        //        // clean symlinks, they will be restored later
        //        if (File.Exists(possibleFileSymlink))
        //        {
        //            if (possibleFileSymlink.IsSymlink())
        //            {
        //                File.Delete(possibleFileSymlink);
        //            }
        //        }
        //    }

        //    new DirectoryInfo(ManageSettings.GetAppOldModOrganizerDirPath()).MoveAll(new DirectoryInfo(ManageSettings.GetAppModOrganizerDirPath()), overwriteFiles: true);
        //}
        //}

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

            //if (File.Exists(ManageLogs.LogFilePath) && new FileInfo(ManageLogs.LogFilePath).Length > 10000000)
            //{
            //    try
            //    {
            //        File.Delete(ManageLogs.LogFilePath);
            //    }
            //    catch (Exception ex)
            //    {
            //        _log.Debug("An error occured whil tried to CleanLog. error:" + ex);
            //    }
            //}
        }

        private void SetLocalizationStrings()
        {
            this.Text = "AI Helper" + " | " + ManageSettings.Games.Game.GameDisplayingName;
            //CurrentGameLabel.Text = T._("Current Game") + ":";
            //InstallInModsButton.Text = T._("Install");// + " " + ManageSettings.ModsInstallDirName();
            //ToolsFixModListButton.Text = T._("Fix modlist");
            //btnUpdateMods.Text = T._("Update");
            //button1.Text = T._("Prepare the game");
            //CreateShortcutButton.Text = T._("Shortcut");
            //CreateShortcutLinkLabel.Text = T._("Shortcut");
            //FixRegistryButton.Text = T._("Registry");
            //FixRegistryLinkLabel.Text = T._("Registry");
            //DisplaySettingsGroupBox.Text = T._("Display");
            //SetupXmlLinkLabel.Text = DisplaySettingsGroupBox.Text;//Тот же текст
            //FullScreenCheckBox.Text = T._("fullscreen");
            //AutoShortcutRegistryCheckBox.Text = T._("Auto");
            //SettingsFoldersGroupBox.Text = T._("Folders");
            //OpenGameFolderLinkLabel.Text = T._("Game");
            //OpenModsFolderLinkLabel.Text = T._("Mods");
            //MainTabPage.Text = T._("Info");
            //LaunchTabPage.Text = T._("Launch");
            //LaunchTabLaunchLabel.Text = T._("Launch");
            //ToolsTabPage.Text = T._("Tools");
            //SettingsTabPage.Text = T._("Settings");
            //StudioButton.Text = T._("Studio");
            //GameButton.Text = T._("Game");
            //MOButton.Text = T._("Manager");
            //SettingsButton.Text = T._("Settings");
            //ExtraSettingsLinkLabel.Text = T._("Extra Settings");
            //JPLauncherRunLinkLabel.Text = T._("Orig Launcher");
            //LaunchLinksLinkLabel.Text = T._("Links");
            QualityComboBox.Items.Add(T._("Perfomance"));
            QualityComboBox.Items.Add(T._("Normal"));
            QualityComboBox.Items.Add(T._("Quality"));
        }

        private int _mode;

        private void MainService_Click(object sender, EventArgs e)
        {
            AIGirlHelperTabControl.SelectedTab = LaunchTabPage;
            return;
            //MainService.Enabled = false;

            //_mode = ManageGameUnPacker.GetModeValue();

            //switch (_mode)
            //{
            //    case 0:
            //        //ManageGameUnPacker.CompressingMode();
            //        break;

            //    case 1:
            //        AIGirlHelperTabControl.SelectedTab = LaunchTabPage;
            //        break;

            //    case 2:
            //        //ManageGameUnPacker.ExtractingMode();
            //        break;

            //    default:
            //        break;
            //}

            //MainService.Enabled = true;
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

            if (AIGirlHelperTabControl.SelectedTab == MainTabPage)
            {
                //Main
                //THToolTip.SetToolTip(button1, T._("Unpacking mods and resources from 'Downloads' and 'RES' folders for game when they are not installed"));              


            }
            else if (AIGirlHelperTabControl.SelectedTab == LaunchTabPage)
            {
                FunctionsForFlpLoader.Load();

                //_thToolTip.SetToolTip(ProgramNameLabelPart2, ManageSettings.ApplicationProductName + " - " + T._("Illusion games manager.\n\n"
                //        + "Move mouse over wished button or text to see info about it"
                //        )
                //    );
                _thToolTip.SetToolTip(SelectedGameLabel, T._("Selected game title"));

                //Launch
                _thToolTip.SetToolTip(VRGameCheckBox, T._("Check to run VR exe instead on standart"));

                _thToolTip.SetToolTip(GameButton, MOmode ? T._("Will execute the Game")
                    + T._(" from Mod Organizer with attached mods")
                    : T._("Will execute the Game")
                    );
                _thToolTip.SetToolTip(StudioButton, MOmode ? T._("Will execute Studio")
                    + T._(" from Mod Organizer with attached mods")
                    : T._("Will execute Studio")
                    );
                _thToolTip.SetToolTip(MOButton, T._("Will execute Mod Organizer mod manager where you can manage your mods"));
                _thToolTip.SetToolTip(JPLauncherRunLinkLabel, MOmode ?
                      T._("Will execute original game launcher")
                    + T._(" from Mod Organizer with attached mods")
                    : T._("Will execute original game launcher")
                    );
                _thToolTip.SetToolTip(SettingsButton, T._("Will be opened Settings tab"));
                //_thToolTip.SetToolTip(LaunchModeInfoLinkLabel, T._("Same as button in Tool tab.\n")
                //    + (MOmode ? T._(
                //        "Will convert game from MO Mode to Common mode\n" +
                //        " when you can run exes from Data folder without Mod Organizer.\n" +
                //        " You can convert game back to MO mode\n" +
                //        " when it will be need to install new mods or test your mod config"
                //    ) : T._(
                //        "Will convert the game to MO mode\n when all mod files will be moved back to Mods folder\n" +
                //        " in their folders and vanilla files restored"
                //    )
                //    )
                //    );

                //_thToolTip.SetToolTip(pbDiscord, T._("Discord page. Info, links, support."));
                _thToolTip.SetToolTip(OpenLogLinkLabel, T._("Open BepinEx log if found"));
                _thToolTip.SetToolTip(BepInExDisplayedLogLevelLabel, T._("Click here to select log level\n" +
                    "Only displays the specified log level and above in the console output"));
            }
            else if (AIGirlHelperTabControl.SelectedTab == SettingsTabPage)
            {
                _thToolTip.SetToolTip(AutoShortcutRegistryCheckBox, T._("When checked will create shortcut for the AI Helper on Desktop and will fix registry if need"));
                //_thToolTip.SetToolTip(DisplaySettingsGroupBox, T._("Game Display settings"));
                //_thToolTip.SetToolTip(SetupXmlLinkLabel, T._("Open Setup.xml in notepad"));
                _thToolTip.SetToolTip(ResolutionComboBox, T._("Select preferred screen resolution"));
                _thToolTip.SetToolTip(FullScreenCheckBox, T._("When checked game will be in fullscreen mode"));
                _thToolTip.SetToolTip(QualityComboBox, T._("Select preferred graphics quality"));
                //THToolTip.SetToolTip(CreateShortcutButton, T._("Will create shortcut in Desktop if not exist"));
                _thToolTip.SetToolTip(CreateShortcutLinkLabel, T._("Will create shortcut in Desktop if not exist"));
                //THToolTip.SetToolTip(FixRegistryButton, T._("Will set Data dir with game files as install dir in registry"));
                _thToolTip.SetToolTip(FixRegistryLinkLabel, T._("Will set Data dir with game files as install dir in registry"));

                //Open Folders
                //_thToolTip.SetToolTip(OpenGameFolderLinkLabel, T._("Open Data folder of selected game"));
                //_thToolTip.SetToolTip(OpenModsFolderLinkLabel, T._("Open Mods folder of selected game"));
                //_thToolTip.SetToolTip(OpenMOFolderLinkLabel, T._("Open Mod Organizer folder"));
                //_thToolTip.SetToolTip(OpenMOOverwriteFolderLinkLabel, T._("Open Overwrite folder of Mod Organizer with possible new generated files for selected game\n\nFiles here have highest priority and will be loaded over any enabled mod files"));
                //_thToolTip.SetToolTip(OpenMyUserDataFolderLinkLabel, T._("Open MyUserData folder in Mods if exist\n\nHere placed usual User files of Organized ModPack for selected game"));
                //_thToolTip.SetToolTip(OpenPresetDirsLinkLabel, T._("Open dir for character cards"));
                //_thToolTip.SetToolTip(OpenPresetDirsLinkLabelMO, T._("Open dir for character cards using Mod Organizer when game in MO mode."));

                //_thToolTip.SetToolTip(LaunchLinksLinkLabel, T._("Open list of links for game resources"));
                _thToolTip.SetToolTip(ExtraSettingsLinkLabel, T._("Open extra setting window for plugins and etc"));

                _thToolTip.SetToolTip(CurrentGameComboBox, T._("List of found games. Current") + ": " + ManageSettings.Games.Game.GameDisplayingName);


                //var toMo = ManageSettings.ModsInstallDirName;
                //_thToolTip.SetToolTip(SettingsOpen2MOLinkLabel,
                //    T._("Open folder, where from mod files can be installed fo selected game") +
                //    T._("\n\nHere can be placed mod files which you want to install for selected game in approriate subfolders in mods" +
                //    "\nand then can be installed all by one click on") + " " + InstallInModsButton.Text + " " + T._("button") +
                //    "\n" + T._("which can be found in") + " " + ToolsTabPage.Text + " " + T._("tab page") +
                //    "\n\n" + T._("Helper recognize") + ":"
                //    + "\n " + T._(".dll files of BepinEx plugins")
                //    + "\n " + T._("Sideloader mod archives")
                //    + "\n " + T._("Female character cards")
                //    + "\n " + T._("Female character cards in \"f\" subfolder")
                //    + "\n " + T._("Male character cards in \"m\" subfolder")
                //    + "\n " + T._("Coordinate clothes set cards in \"c\" subfolder")
                //    + "\n " + T._("Studio scene cards in \"s\" subfolder")
                //    + "\n " + T._("Cardframe Front cards in \"cf\" subfolder")
                //    + "\n " + T._("Cardframe Back cards in \"cf\" subfolder")
                //    + "\n " + T._("Script loader scripts")
                //    + "\n " + T._("Housing plan cards in \"h\\01\", \"h\\02\", \"h\\03\" subfolders")
                //    + "\n " + T._("Overlays cards in \"o\" subfolder")
                //    + "\n " + T._("folders with overlays cards in \"o\" subfolder")
                //    + "\n " + T._("Subfolders with modfiles")
                //    + "\n " + T._("Zip archives with mod files")
                //    + "\n\n" + T._("Any Rar and 7z archives will be extracted for install") +
                //    T._("\nSome recognized mods can be updated instead of be installed as new mod") +
                //    T._("\nMost of mods will be automatically activated except .cs scripts" +
                //    "\nwhich always optional and often it is cheats or can slowdown/break game")

                //    );

            }
            else if (AIGirlHelperTabControl.SelectedTab == ToolsTabPage)
            {
                //_thToolTip.SetToolTip(ToolsFixModListButton, T._("Fix problems in current enabled mods list"));

                //_thToolTip.SetToolTip(llOpenOldPluginsBuckupFolder,
                //    T._("Open older plugins buckup folder")
                //    );
                //_thToolTip.SetToolTip(btnUpdateMods,
                //    T._("Update Mod Organizer and enabled mods") + "\n" +
                //    T._("Mod Organizer already have hardcoded info") + "\n" +
                //    T._("Mods will be updated if there exist info in meta.ini notes or in updateInfo.txt") + "\n" +
                //    T._("After plugins update check will be executed KKManager StandaloneUpdater for Sideloader modpack updates check for games where it is possible")
                //    );
                //var sideloaderPacksWarning = T._("Warning! More of packs you check more of memory game will consume.") + "\n" +
                //    T._("Check only what you really using or you can 16+ gb of memory.");
                //_thToolTip.SetToolTip(UseKKmanagerUpdaterLabel, T._("Check if need to run update check for sideloader modpacks.") + "\n\n" +
                //    sideloaderPacksWarning
                //    );
                //_thToolTip.SetToolTip(UpdatePluginsLabel, T._("Check if need to run update check for plugins and Mod Organizer.")
                //    );
                //_thToolTip.SetToolTip(CheckEnabledModsOnlyLabel, T._("Check updates only for enabled plugins.")
                //    );
                //_thToolTip.SetToolTip(BleadingEdgeZipmodsLabel,
                //    T._("Check also updates of Bleeding Edge Sideloader Modpack in KKManager") + "\n" +
                //    T._("Bleeding Edge Sideloader modpack contains test versions of zipmods which is still not added in main modpacks") + "\n\n" +
                //    sideloaderPacksWarning
                //    );
                //_thToolTip.SetToolTip(MOCommonModeSwitchButton, MOmode ? T._(
                //        "Will convert game from MO Mode to Common mode\n" +
                //        " when you can run exes from Data folder without Mod Organizer.\n You can convert game back to MO mode\n" +
                //        " when it will be need to install new mods or test your mod config"
                //    ) : T._(
                //        "Will convert the game to MO mode\n" +
                //        " when all mod files will be moved back to Mods folder\n" +
                //        " in their folders and vanilla files restored"
                //    )
                //    );
                //_thToolTip.SetToolTip(ModeSwitchCreateBuckupLabel,
                //    T._("Enables backup creation of selected game before mode switch\n" +
                //    " to be possible to restore Data and Mods dirs.\n" +
                //    "\n" +
                //    "Backup creating using ntfs hardlinks and not consumes any extra space.")
                //    );

                //_thToolTip.SetToolTip(InstallInModsButton, T._("Install mods and userdata, placed in") + " " + ManageSettings.ModsInstallDirName + (MOmode ? T._(
                //             " to MO format in Mods when possible"
                //         ) : T._(
                //             " to the game folder when possible"
                //             )));
                //_thToolTip.SetToolTip(Install2MODirPathOpenFolderLinkLabel, T._("Open folder where you can drop/download files for autoinstallation"));
                ToolsTabButtonsLoader.Load();
            }
            else if (AIGirlHelperTabControl.SelectedTab == FoldersTabPage)
            {
                FoldersTabButtonsLoader.Load();
            }
            ////////////////////////////
        }

        private void SetScreenSettings()
        {
            if (!MOmode) SetupXmlPath = ManageSettings.CurrentGameSetupXmlFilePathinData;

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
            //string screenHeight = Screen.PrimaryScreen.Bounds.Height.ToString();
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
            if (MOmode && !Directory.Exists(ManageSettings.CurrentGameModsDirPath))
            {
                Directory.CreateDirectory(ManageSettings.CurrentGameModsDirPath);
                ModsInfoLabel.Text = T._("Mods dir created");
            }

            if (File.Exists(Path.Combine(ManageSettings.CurrentGameDataDirPath, ManageSettings.CurrentGameExeName + ".exe")))
            {
                DataInfoLabel.Text = string.Format(CultureInfo.InvariantCulture, T._("{0} game installed in {1}"), ManageSettings.CurrentGameDisplayingName, "Data");
            }
            else if (File.Exists(Path.Combine(ManageSettings.AppResDirPath, ManageSettings.CurrentGameExeName + ".7z")))
            {
                DataInfoLabel.Text = string.Format(CultureInfo.InvariantCulture, T._("{0} archive in {1}"), "AIGirl", "Data");
            }
            else if (Directory.Exists(ManageSettings.CurrentGameDataDirPath))
            {
                DataInfoLabel.Text = string.Format(CultureInfo.InvariantCulture, T._("{0} files not in {1}. Move {0} game files there."), ManageSettings.CurrentGameDirName, "Data");
            }
            else
            {
                Directory.CreateDirectory(ManageSettings.CurrentGameDataDirPath);
                DataInfoLabel.Text = string.Format(CultureInfo.InvariantCulture, T._("{0} dir created. Move {1} game files there."), "Data", ManageSettings.CurrentGameDirName);
            }

            if (MOmode)
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

            SelectedGameLabel.Text = ManageSettings.Games.Game.GameDisplayingName + "❤";
            this.Text = "AI Helper" + " | " + ManageSettings.Games.Game.GameDisplayingName;

            // load dinamic elements
            if (AIGirlHelperTabControl.SelectedTab == FoldersTabPage)
            {
                FoldersTabButtonsLoader.Load();
            }
            else if (AIGirlHelperTabControl.SelectedTab == LaunchTabPage)
            {
                FunctionsForFlpLoader.Load();
            }
            else if (AIGirlHelperTabControl.SelectedTab == ToolsTabPage)
            {
                ToolsTabButtonsLoader.Load();
            }
        }

        private void CommonModeSpecificSetup()
        {
            SetupXmlPath = ManageSettings.CurrentGameSetupXmlFilePath;

            ModsInfoLabel.Visible = false;

            StudioButton.Enabled = false;

            //MOCommonModeSwitchButton.Text = T._("CommonToMO");
            MainServiceButton.Text = T._("Common mode");
            //LaunchModeInfoLinkLabel.Text = T._("Common mode");
            MainServiceButton.Enabled = false;
        }

        private void MOModeSpecificSetup()
        {
            ManageModOrganizer.RestoreModlist();

            ManageModOrganizer.CheckBaseGamesPy();

            {
                //string[] Archives7z;
                //string[] ModDirs = Directory.GetDirectories(ModsPath, "*").Where(name => !name.EndsWith("_separator", StringComparison.OrdinalIgnoreCase)).ToArray();

                //Archives7z = Directory.GetFiles(DownloadsPath, "*.7z", SearchOption.AllDirectories);
                //if (ModDirs.Length > 0 && Archives7z.Length > 0)
                //{
                //    bool NotAllModsExtracted = false;
                //    foreach (var Archive in Archives7z)
                //    {
                //        if (ModDirs.Contains(Path.Combine(ModsPath, Path.GetFileNameWithoutExtension(Archive))))
                //        {
                //        }
                //        else
                //        {
                //            NotAllModsExtracted = true;
                //            break;
                //        }
                //    }

                //    if (compressmode && NotAllModsExtracted && ModDirs.Length < Archives7z.Length)
                //    {
                //        ModsInfoLabel.Text = T._("Not all mods in Mods dir");
                //        //button1.Enabled = false;
                //        mode = 2;
                //        button1.Text = T._("Extract missing");
                //    }
                //    else
                //    {
                //        ModsInfoLabel.Text = T._("Found mod folders in Mods");
                //        //button1.Enabled = false;
                //        mode = 1;
                //        button1.Text = T._("Mods Ready");
                //        //MO2StandartButton.Enabled = true;
                //        GetEnableDisableLaunchButtons();
                //        MOCommonModeSwitchButton.Text = T._("MOToCommon");
                //        AIGirlHelperTabControl.SelectedTab = LaunchTabPage;
                //    }
                //}
                //else
                //{
                //    //если нет папок модов но есть архивы в загрузках
                //    if (Archives7z.Length > 0 && ModDirs.Length == 0)
                //    {
                //        ModsInfoLabel.Text = T._("Mods Ready for extract");
                //        mode = 2;
                //        button1.Text = T._("Extract mods");
                //    }
                //}

                ////если нет архивов в загрузках, но есть папки модов
                //if (compressmode && Directory.Exists(DownloadsPath) && Directory.Exists(ModsPath))
                //{
                //    if (ModDirs.Length > 0 && Archives7z.Length == 0)
                //    {
                //        if (Archives7z.Length == 0)
                //        {
                //            ModsInfoLabel.Text = "No archives in downloads";
                //            button1.Text = "Pack mods";
                //            mode = 0;
                //        }
                //    }
                //}
            }


            if (!ManageSettings.CurrentGameModsDirPath.IsNullOrEmptyDirectory("*", new string[1] { "_separator" }))
            {
                ModsInfoLabel.Text = T._("Found mod folders in Mods");

                _mode = 1;
                MainServiceButton.Text = T._("Mods Ready");

                AIGirlHelperTabControl.SelectedTab = LaunchTabPage;

                //MOCommonModeSwitchButton.Text = T._("MOToCommon");
            }

            //LaunchModeInfoLinkLabel.Text = T._("MO mode");

            ManageModOrganizer.DummyFiles();

            ManageModOrganizer.MoIniFixes();

            ManageModOrganizer.MakeLinks();

            //try start in another thread for perfomance purposes
            new Thread(obj => RunSlowActions()).Start();

            SetupXmlPath = ManageModOrganizer.GetSetupXmlPathForCurrentProfile();

            SetMoModsVariables();
        }

        private void SetMoMode(bool setText = true)
        {
            if (File.Exists(ManageSettings.CurrentGameMoToStandartConvertationOperationsListFilePath))
            {
                MOmode = false;
                if (setText) MainServiceButton.Text = T._("Common mode");
            }
            else
            {
                MOmode = true;
                if (setText) MainServiceButton.Text = T._("MO mode");
            }
        }

        internal bool IsDebug;
        internal bool IsBetaTest;
        private void EnableDisableSomeTools()
        {
            IsDebug = Path.GetFileName(ManageSettings.ApplicationStartupPath) == "Debug" && File.Exists(Path.Combine(ManageSettings.ApplicationStartupPath, "IsDevDebugMode.txt"));
            IsBetaTest = File.Exists(Path.Combine(ManageSettings.ApplicationStartupPath, "IsBetaTest.txt"));

            //Debug

            //Beta
            //btnUpdateMods.Visible = true;// IsDebug || IsBetaTest;
        }

        private static void RunSlowActions()
        {
            //создание ссылок на файлы bepinex, НА ЭТО ТРАТИТСЯ МНОГО ВРЕМЕНИ

            MOUSFSLoadingFix(true); // remove old fix files
            PreloadingSetup(); // instead of old patch
            //MOUSFSLoadingFix();
            //GameButton.Enabled = false;
            //Task t1 = new Task(() =>
            //ManageMOMods.BepinExLoadingFix()
            //);
            //t1.Start();
            //t1.ContinueWith(delegate
            //{
            //    Thread.Sleep(1000);//бфло исключение с невозможностью выполнения операции, возможно операция выполнялась до появления окна программы, задержка для исправления
            //    GameButton.Invoke((Action)(() => GameButton.Enabled = true));
            //}, TaskScheduler.Current);


            //НА ЭТО ТРАТИТСЯ БОЛЬШЕ ВСЕГО ВРЕМЕНИ

            ManageModOrganizer.SetModOrganizerIniSettingsForTheGame();
            //await Task.Run(() => ManageMO.SetModOrganizerINISettingsForTheGame()).ConfigureAwait(false);
            //MOButton.Enabled = false;
            //Task t2 = new Task(() =>
            //ManageMO.SetModOrganizerINISettingsForTheGame()
            //);
            //t2.Start();
            //t2.ContinueWith(delegate
            //{
            //    Thread.Sleep(1000);//бфло исключение с невозможностью выполнения операции, возможно операция выполнялась до появления окна программы, задержка для исправления
            //    MOButton.Invoke((Action)(() => MOButton.Enabled = true));
            //}, TaskScheduler.Current);
        }

        private void GetEnableDisableLaunchTabButtons()
        {
            if (AIGirlHelperTabControl.SelectedTab.Name != "LaunchTabPage") return;

            //MOButton.Enabled = /*ManageSettings.IsMoMode && */File.Exists(ManageSettings.GetMOexePath());
            //SettingsButton.Enabled = File.Exists(Path.Combine(DataPath, ManageSettings.GetINISettingsEXEName() + ".exe"));
            JPLauncherRunLinkLabel.Enabled = File.Exists(Path.Combine(ManageSettings.CurrentGameDataDirPath, ManageSettings.IniSettingsExeName + ".exe"));
            GameButton.Enabled = File.Exists(Path.Combine(ManageSettings.CurrentGameDataDirPath, ManageSettings.CurrentGameExeName + ".exe"));
            StudioButton.Enabled = File.Exists(Path.Combine(ManageSettings.CurrentGameDataDirPath, ManageSettings.StudioExeName + ".exe"));

            //Set BepInEx log data
            if (ManageSettings.BepInExCfgFilePath.Length > 0 && File.Exists(ManageSettings.BepInExCfgFilePath))
            {
                BepInExConsoleCheckBox.Enabled = true;
                try
                {
                    //BepInExConsoleCheckBox.Checked = bool.Parse(ManageINI.GetINIValueIfExist(ManageSettings.GetBepInExCfgFilePath(), "Enabled", "Logging.Console", "False"));
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
            //FixRegistryButton.Enabled = false;
            FixRegistryLinkLabel.Enabled = false;

            ManageRegistry.FixRegistry(false);

            //FixRegistryButton.Enabled = true;
            FixRegistryLinkLabel.Enabled = true;
        }

        private async void MOButton_Click(object sender, EventArgs e)
        {
            OnOffButtons(false);

            await Task.Run(() => ManageOther.WaitIfGameIsChanging()).ConfigureAwait(true);

            if (MOmode)
            {
                ManageProcess.KillProcessesByName(Path.GetFileNameWithoutExtension(ManageSettings.AppMOexePath));
                ManageProcess.RunProgram(ManageSettings.AppMOexePath, string.Empty);
            }
            else
            {
                //MOButton.Enabled = false;
                MessageBox.Show(T._("Game in Common mode now.\n To execute Mod Organizer convert game back\n to MO mode by button in Tools tab"));
            }
            OnOffButtons();
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            AIGirlHelperTabControl.SelectedTab = SettingsTabPage;
            //OnOffButtons(false);
            //if (MOmode)
            //{
            //    RunProgram(MOexePath, "moshortcut://:" + ManageSettings.GetINISettingsEXEName());
            //}
            //else
            //{
            //    RunProgram(Path.Combine(DataPath, ManageSettings.GetINISettingsEXEName() + ".exe"), string.Empty);
            //}
            //OnOffButtons();
        }

        private async void GameButton_Click(object sender, EventArgs e)
        {
            OnOffButtons(false);

            await Task.Run(() => ManageOther.WaitIfGameIsChanging()).ConfigureAwait(true);

            bool isVr = ManageSettings.CurrentGameIsHaveVr;

            string exePath;
            string arguments = string.Empty;
            if (MOmode)
            {
                var getCurrentGameExemoProfileName = ManageSettings.CurrentGameExemoProfileName;
                var customExeTitleName = getCurrentGameExemoProfileName + (isVr?"VR":"");
                exePath = ManageSettings.AppMOexePath;

                ManageProcess.KillProcessesByName(ManageModOrganizer.GetExeNameByTitle(customExeTitleName));

                //if (cbxNtlea.Checked)
                //{
                //customExeTitleName += "_NTLEA";

                //var customs = new CustomExecutables();
                //if (!customs.ContainsTitle(customExeTitleName))
                //{
                //    var custom = new CustomExecutables.CustomExecutable
                //    {
                //        Title = customExeTitleName,
                //        Binary = ManageSettings.NtleaExePath(), // ntlea path
                //        Arguments = "\"" + ManageSettings.GetCurrentGameExePath() + "\" \"C932\" \"L0411\"" // ntlea arcuments like [C]odepage  and [L]ocal ID
                //    };
                //    customs.Add(custom, performSave: true);
                //}
                //}

                arguments = "moshortcut://:\"" + customExeTitleName + "\"";
            }
            else
            {
                if (cbxNtlea.Checked)
                {
                    exePath = ManageSettings.NtleaExePath;
                    arguments = "\"" + Path.Combine(ManageSettings.CurrentGameDataDirPath, (isVr ? ManageSettings.CurrentGame.GameExeNameVr : ManageSettings.CurrentGameExeName) + ".exe") + "\"" + " \"C932\" \"L0411\"";
                }
                else
                {
                    exePath = Path.Combine(ManageSettings.CurrentGameDataDirPath, (isVr ? ManageSettings.CurrentGame.GameExeNameVr : ManageSettings.CurrentGameExeName) + ".exe");
                }
            }

            ManageProcess.KillProcessesByName(Path.GetFileNameWithoutExtension(exePath));
            ManageProcess.RunProgram(exePath, arguments);

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

            if (MOmode)
            {
                ManageProcess.KillProcessesByName(ManageSettings.StudioExeName);
                ManageProcess.KillProcessesByName(Path.GetFileNameWithoutExtension(ManageSettings.AppMOexePath));

                var studio = ManageModOrganizer.GetMOcustomExecutableTitleByExeName(ManageSettings.StudioExeName);
                ManageProcess.RunProgram(ManageSettings.AppMOexePath, "moshortcut://:" + studio);
            }
            else
            {
                ManageProcess.KillProcessesByName(ManageSettings.StudioExeName);

                var exe = Path.Combine(ManageSettings.CurrentGameDataDirPath, ManageSettings.StudioExeName + ".exe");
                ManageProcess.RunProgram(exe, string.Empty);
            }
            OnOffButtons();
        }

        private readonly Dictionary<string, string> _qualitylevels = new Dictionary<string, string>(3);

        private void QualityComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetGraphicsQuality((sender as ComboBox).SelectedIndex.ToString(CultureInfo.InvariantCulture));
        }

        private void NewformButton_Click(object sender, EventArgs e)
        {
            ManageReport.ShowReportFromLinks();
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

        private async void InstallInModsButton_Click(object sender, EventArgs e)
        {
        }

        private void CreateShortcutButton_Click(object sender, EventArgs e)
        {
            ManageOther.CreateShortcuts(true, false);
        }

        private void MO2StandartButton_Click(object sender, EventArgs e)
        {
            ManageMOModeSwitch.SwitchBetweenMoAndStandartModes();
        }

        private void Install2MODirPathOpenFolderLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            new Open2MODirButtonData().OpenDir();
        }

        private void OpenLogLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenBepinexLog();
        }

        private void CurrentGameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //bool init = ManageSettings.INITDone;
            //bool change = ManageSettings.CurrentGameIsChanging;

            if (ManageSettings.INITDone && !ManageSettings.CurrentGameIsChanging && !ManageSettings.SetModOrganizerINISettingsForTheGame)
            {
                ManageSettings.CurrentGameIsChanging = true;

                ManageSettings.Games.Game = ManageSettings.Games.Games[(sender as ComboBox).SelectedIndex];
                ManageOther.SetSelectedGameIndexAndBasicVariables(this);
                //ManageSettings.Games.CurrentGameListIndex = (sender as ComboBox).SelectedIndex;
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
            //cleaning previous game data
            //File.Delete(ManageSettings.GetModOrganizerINIpath());
            //File.Delete(ManageSettings.GetMOcategoriesPath());
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
            //ManageINI.WriteINIValue(ManageSettings.GetBepInExCfgFilePath(), "Logging.Console", "Enabled", /*" " +*/ (sender as CheckBox).Checked.ToString(CultureInfo.InvariantCulture));
            var bepinExcfg = ManageSettings.BepInExCfgFilePath;
            ManageCfg.WriteCfgValue(bepinExcfg, "Logging.Console", "Enabled", /*" " +*/ (sender as CheckBox).Checked.ToString(CultureInfo.InvariantCulture));

            if (BepInExDisplayedLogLevelLabel.Visible = (sender as CheckBox).Checked)
            {
                ManageSettings.SwitchBepInExDisplayedLogLevelValue(BepInExConsoleCheckBox, BepInExDisplayedLogLevelLabel, true);
            }
        }

        //private void BepInExDisplayedLogLevelLabel_VisibleChanged(object sender, EventArgs e)
        //{
        //    if (BepInExConsoleCheckBox.Checked)
        //    {
        //        ManageSettings.SwitchBepInExDisplayedLogLevelValue(BepInExConsoleCheckBox, BepInExDisplayedLogLevelLabel, true);
        //    }
        //}

        private void BepInExDisplayedLogLevelLabel_Click(object sender, EventArgs e)
        {
            if (BepInExConsoleCheckBox.Checked)
                ManageSettings.SwitchBepInExDisplayedLogLevelValue(BepInExConsoleCheckBox, BepInExDisplayedLogLevelLabel);
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
                    //LinksForm.Text = T._("Links");
                    StartPosition = FormStartPosition.Manual
                };
                _extraSettingsForm.Load += delegate (object s2, EventArgs e2)
                {
                    _extraSettingsForm.Location = new Point(Bounds.Location.X + (Bounds.Width / 2) - (_extraSettingsForm.Width / 2),
                        Bounds.Location.Y + /*(Bounds.Height / 2) - (f2.Height / 2) +*/ Bounds.Height);
                };
                //extraSettings.Text = T._("Links");
                //newformButton.Text = @"/\";
                _extraSettingsForm.Show();
                //extraSettingsForm.Location = new Point(Bounds.Location.X + (Bounds.Width / 2) - (extraSettingsForm.Width / 2),
                //         Bounds.Location.Y + /*(Bounds.Height / 2) - (f2.Height / 2) +*/ Bounds.Height);
                _extraSettingsForm.TopMost = true;
            }
            else
            {
                //newformButton.Text = @"\/";
                _extraSettingsForm.Close();
            }
        }

        private void AIGirlHelperTabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
        }

        private async void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OnOffButtons(false);

            await Task.Run(() => ManageOther.WaitIfGameIsChanging()).ConfigureAwait(true);

            if (MOmode)
            {
                ManageProcess.RunProgram(ManageSettings.AppMOexePath, "moshortcut://:" + ManageSettings.IniSettingsExeName);
            }
            else
            {
                ManageProcess.RunProgram(Path.Combine(ManageSettings.CurrentGameDataDirPath, ManageSettings.IniSettingsExeName + ".exe"), string.Empty);
            }
            OnOffButtons();
        }

        private void AI_Helper_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ManageSettings.Games.Game == null) return;

            try
            {
                //write last game folder name
                ManageIni.GetINIFile(ManageSettings.AiHelperIniPath).SetKey("Settings", "selected_game", ManageSettings.CurrentGameDirName);
            }
            catch (Exception ex)
            {
                _log.Debug("An error occered in time of the app closing. error:\r\n" + ex);
            }
            //нашел баг, когда при открытии свойства ссылки в проводнике
            //, с последующим закрытием свойств и закрытием AI Helper происходит блокировка папки проводником и при следующем запуске происходит ошибка AI Helper, до разблокировки папки
            //также если пользователь решит запускать МО без помощника, игра не запустится, т.к. фикса бепинекс нет
            //ManageMOMods.BepinExLoadingFix(true);
        }

        private void SetupXmlPathLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(SetupXmlPath)) Process.Start("notepad.exe", SetupXmlPath);
        }

        private void OpenHelpLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show(T._("Move mouse cursor over wished button or text to see info about it"));
            //var HelpFilePath = "FilePath";
            //if (File.Exists(HelpFilePath))
            //    Process.Start("explorer.exe", HelpFilePath);
        }

        private void ToolsFixModListButton_Click(object sender, EventArgs e)
        {
        }

        private void btnUpdateMods_Click(object sender, EventArgs e)
        {
            ManageUpdateMods.UpdateMods();
        }

        private void PbDiscord_Click(object sender, EventArgs e)
        {
            //Process.Start(ManageSettings.DiscordGroupLink);//Program's discord server
        }

        private void AIGirlHelperTabControl_Selected(object sender, TabControlEventArgs e)
        {
            //FoldersInit();

            if (AIGirlHelperTabControl.SelectedTab.Name == "ToolsTabPage")
            {
            }
            else if (AIGirlHelperTabControl.SelectedTab.Name == "LaunchTabPage")
            {
                //newformButton.Text = @"\/";

                GetEnableDisableLaunchTabButtons();
                //set bepinex log cfg
                //BepInExDisplayedLogLevelLabel.Visible = BepInExConsoleCheckBox.Checked = ManageCFG.GetCFGValueIfExist(ManageSettings.GetBepInExCfgFilePath(), "Enabled", "Logging.Console", "").ToUpperInvariant() == "TRUE";
            }
        }

        /// <summary>
        /// update status of update button options and button itself
        /// </summary>
        private void UpdateButtonOptionsRefresh()
        {
            //UseKKmanagerUpdaterLabel.SetCheck();
            //UseKKmanagerUpdaterLabel.Visible = ManageSettings.IsHaveSideloaderMods && File.Exists(ManageSettings.KkManagerStandaloneUpdaterExePath);
            //UseKKmanagerUpdaterCheckBox.Visible = b && ManageSettings.Games.CurrentGame.IsHaveSideloaderMods;
            //BleadingEdgeZipmodsLabel.Visible = UseKKmanagerUpdaterLabel.Visible;
            //cbxBleadingEdgeZipmods.Visible = UseKKmanagerUpdaterCheckBox.Visible;

            //CheckEnabledModsOnlyLabel.Enabled = UpdatePluginsLabel.IsChecked();
            //btnUpdateMods.Enabled = (UpdatePluginsLabel.Visible && UpdatePluginsLabel.IsChecked()) || (UseKKmanagerUpdaterLabel.Visible && UseKKmanagerUpdaterLabel.IsChecked());
            //btnUpdateMods.Enabled = (UpdatePluginsCheckBox.Visible && UpdatePluginsCheckBox.Checked) || (UseKKmanagerUpdaterCheckBox.Visible && UseKKmanagerUpdaterCheckBox.Checked);

            //check bleeding edge txt
            //cbxBleadingEdgeZipmods.Checked = cbxBleadingEdgeZipmods.Visible && File.Exists(ManageSettings.ZipmodsBleedingEdgeMarkFilePath());
            //BleadingEdgeZipmodsLabel.SetCheck(BleadingEdgeZipmodsLabel.Visible && UseKKmanagerUpdaterLabel.IsChecked() && File.Exists(ManageSettings.ZipmodsBleedingEdgeMarkFilePath));
            //BleadingEdgeZipmodsLabel.Enabled = BleadingEdgeZipmodsLabel.Visible && UseKKmanagerUpdaterLabel.IsChecked();
        }

        private void AIGirlHelperTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetTooltips();
        }

        private void RefreshLabelCheckState(object sender)
        {
            //var label = (sender as Label);
            //label.SetCheck(!label.IsChecked());
            //UpdateButtonOptionsRefresh();
        }

        private void Dev_Click(object sender, EventArgs e)
        {
        }

        private void AddGameLabel_Click(object sender, EventArgs e)
        {
            ManageOther.AddNewGame(this);
        }

        private void MO2StandartButton_Click(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        //Disable close window button
        //https://social.msdn.microsoft.com/Forums/en-US/b1f0d913-c603-43e9-8fe3-681fb7286d4c/c-disable-close-button-on-windows-form-application?forum=csharpgeneral
        //[DllImport("user32")]
        //static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        //[DllImport("user32")]
        //static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        //const int MF_BYCOMMAND = 0;
        //const int MF_DISABLED = 2;
        //const int SC_CLOSE = 0xF060;
        //bool DisableMainClose = true;
        //private void button2_Click(object sender, EventArgs e)
        //{
        //    //DisableMainClose = !DisableMainClose;
        //    //var sm = GetSystemMenu(Handle, DisableMainClose);
        //    //EnableMenuItem(sm, SC_CLOSE, MF_BYCOMMAND | MF_DISABLED);
        //    //this.ControlBox = !this.ControlBox;
        //}

        //Материалы
        //Есть пример с загрузкой файла по ссылке:
        //https://github.com/adoconnection/SevenZipExtractor
        //Включение exe или dll в exe проекта
        //https://stackoverflow.com/questions/189549/embedding-dlls-in-a-compiled-executable/20306095#20306095
        //https://github.com/Fody/Costura
        //Решение ошибка Argument exception для библиотек, включаемых в exe с помощью costurafody
        //http://qaru.site/questions/6941424/not-able-to-get-costurafody-to-work-keeps-asking-for-the-dll
    }
}