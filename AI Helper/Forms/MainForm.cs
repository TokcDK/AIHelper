using AIHelper.Install.Types;
using AIHelper.Install.Types.Directories;
using AIHelper.Install.UpdateMaker;
using AIHelper.Manage;
using AIHelper.Manage.Update;
using AIHelper.SharedData;
using CheckForEmptyDir;
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
using static AIHelper.Manage.ManageModOrganizer;

//using Crc32C;

namespace AIHelper
{
    internal partial class MainForm : Form
    {
        internal bool _compressmode;

        /// <summary>
        /// Get or set path to setup.xml graphic settings for current game
        /// </summary>
        private static string SetupXmlPath { get => Properties.Settings.Default.SetupXmlPath; set => Properties.Settings.Default.SetupXmlPath = value; }
        /// <summary>
        /// Get or set MO mode for current game
        /// </summary>
        private static bool MOmode { get => ManageSettings.IsMoMode(); set => Properties.Settings.Default.MOmode = value; }

        public MainForm()
        {
            InitializeComponent();

            Properties.Settings.Default.ApplicationStartupPath = Application.StartupPath;
            Properties.Settings.Default.ApplicationProductName = Application.ProductName;

            if (!SetListOfAddedGames())
            {
                Application.Exit();
                this.Enabled = false;
                return;
            }

            GameData.MainForm = this; // set reference to the form for controls use

            CheckMoAndEndInit();
        }

        private async void CheckMoAndEndInit()
        {
            //MO data parse
            if (!Directory.Exists(ManageSettings.GetCurrentGameModOrganizerIniPath()))
            {
                Directory.CreateDirectory(ManageSettings.GetCurrentGameModOrganizerIniPath());
            }
            if (!File.Exists(ManageSettings.GetAppMOexePath()))
            {
                await new Updater().Update().ConfigureAwait(true);
            }

            Properties.Settings.Default.MOIsNew = ManageModOrganizer.IsMo23OrNever();

            ManageModOrganizer.RedefineGameMoData();

            ManageModOrganizer.CleanMoFolder();
            //
            GameData.CurrentGame.GameName = ManageModOrganizer.GetMoBasicGamePluginGameName();
            //
            ManageModOrganizer.CheckBaseGamesPy();

            CleanLog();

            SetMoMode(false);
            GameData.CurrentGame.InitActions();

            SetLocalizationStrings();

            FoldersInit();

            //if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\illusion\AI-Syoujyo\AI-SyoujyoTrial", "INSTALLDIR", null) == null)
            //{
            //    FixRegistryButton.Visible = true;
            //}

            Properties.Settings.Default.INITDone = true;
        }

        private static void CleanLog()
        {
            if (File.Exists(ManageLogs.LogFilePath) && new FileInfo(ManageLogs.LogFilePath).Length > 10000000)
            {
                try
                {
                    File.Delete(ManageLogs.LogFilePath);
                }
                catch (Exception ex)
                {
                    ManageLogs.Log("An error occured whil tried to CleanLog. error:" + ex);
                }
            }
        }

        private bool SetListOfAddedGames()
        {
            try
            {
                GameData.ListOfGames = ManageSettings.GetListOfExistsGames();

                if (GameData.ListOfGames == null || GameData.ListOfGames.Count == 0)
                {
                    MessageBox.Show(T._("Games not found") + "."
                        + Environment.NewLine + T._("Need atleast one game in subfolder in Games folder") + "."
                        + Environment.NewLine + "----------------"
                        + Environment.NewLine + T._("List of games") + ":"
                        + Environment.NewLine + ManageSettings.GetFolderNamesOfFoundGame()
                        + Environment.NewLine + "----------------"
                        + Environment.NewLine + T._("The game folder must contain") + ":"
                        + Environment.NewLine + "Data" + " - " + T._("main game data")
                        + Environment.NewLine + "Mods" + " - " + T._("game mods in subfolders")
                        + Environment.NewLine + "MO" + " - " + T._("Mod Organizer folder with next data") + ":"
                        + Environment.NewLine + "  " + "profiles" + " - " + T._("profiles folder with mod combinations")
                        + Environment.NewLine + "  " + "categories.dat" + " - " + T._("list of categories for mods")
                        + Environment.NewLine + "  " + "ModOrganizer.ini" + " - " + T._("Mod Organizer settings for the game")
                        + Environment.NewLine + "  " + T._("in place of any not exists MO data files will be created empty.")
                        );
                    //Application.Exit();
                    return false;
                }

                foreach (var game in GameData.ListOfGames)
                {
                    CurrentGameComboBox.Items.Add(game.GetGameFolderName());
                }
                if (CurrentGameComboBox.Items.Count == 1)
                {
                    //CurrentGameComboBox.Items[0] = ListOfGames[0].GetGameDisplayingName();
                    CurrentGameComboBox.Enabled = false;
                }

                var ini = ManageIni.GetINIFile(ManageSettings.GetAiHelperIniPath());

                string selected_game;
                if (ini.Configuration == null)
                {
                    selected_game = "";
                }
                else
                {
                    selected_game = ini.GetKey("Settings", "selected_game");
                    if (string.IsNullOrWhiteSpace(selected_game))
                    {
                        var game = GameData.ListOfGames[0];
                        selected_game = game.GetGameFolderName();
                    }
                }

                SetSelectedGameIndexAndBasicVariables(ManageSettings.GetCurrentGameIndexByFolderName(
                        GameData.ListOfGames
                        ,
                        selected_game
                        ));

                try
                {
                    CurrentGameTitleTextBox.Text = GameData.CurrentGame.GetGameDisplayingName();
                    CurrentGameTitleTextBox.Enabled = false;
                }
                catch { }
            }
            catch (Exception ex)
            {
                ManageLogs.Log("An error occured while SetListOfGames.path=" + ManageSettings.GetAiHelperIniPath() + "\r\n error:\r\n" + ex);
                return false;
            }


            return true;
        }

        private void SetSelectedGameIndexAndBasicVariables(int index = 0)
        {
            Properties.Settings.Default.CurrentGameListIndex = index;
            GameData.CurrentGame = GameData.ListOfGames[Properties.Settings.Default.CurrentGameListIndex];
            CurrentGameComboBox.SelectedIndex = index;

            //set checkbox
            Properties.Settings.Default.AutoShortcutRegistryCheckBoxChecked = bool.Parse(ManageIni.GetIniValueIfExist(ManageSettings.GetAiHelperIniPath(), "autoCreateShortcutAndFixRegystry", "Settings", "False"));
            AutoShortcutRegistryCheckBox.Checked = Properties.Settings.Default.AutoShortcutRegistryCheckBoxChecked;
        }

        private void SetLocalizationStrings()
        {
            this.Text = "AI Helper" + " | " + GameData.CurrentGame.GetGameDisplayingName();
            CurrentGameLabel.Text = T._("Current Game") + ":";
            InstallInModsButton.Text = T._("Install");// + " " + ManageSettings.ModsInstallDirName();
            ToolsFixModListButton.Text = T._("Fix modlist");
            btnUpdateMods.Text = T._("Update");
            //button1.Text = T._("Prepare the game");
            SettingsTabPage.Text = T._("Settings");
            //CreateShortcutButton.Text = T._("Shortcut");
            CreateShortcutLinkLabel.Text = T._("Shortcut");
            //FixRegistryButton.Text = T._("Registry");
            FixRegistryLinkLabel.Text = T._("Registry");
            DisplaySettingsGroupBox.Text = T._("Display");
            SetupXmlLinkLabel.Text = DisplaySettingsGroupBox.Text;//Тот же текст
            FullScreenCheckBox.Text = T._("fullscreen");
            AutoShortcutRegistryCheckBox.Text = T._("Auto");
            SettingsFoldersGroupBox.Text = T._("Folders");
            OpenGameFolderLinkLabel.Text = T._("Game");
            OpenModsFolderLinkLabel.Text = T._("Mods");
            MainTabPage.Text = T._("Info");
            LaunchTabPage.Text = T._("Launch");
            LaunchTabLaunchLabel.Text = T._("Launch");
            ToolsTabPage.Text = T._("Tools");
            StudioButton.Text = T._("Studio");
            GameButton.Text = T._("Game");
            MOButton.Text = T._("Manager");
            SettingsButton.Text = T._("Settings");
            ExtraSettingsLinkLabel.Text = T._("Extra Settings");
            JPLauncherRunLinkLabel.Text = T._("Orig Launcher");
            LaunchLinksLinkLabel.Text = T._("Links");
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
                ManageLogs.Log("An error occured while SetTooltips. error:\r\n" + ex);
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
                _thToolTip.SetToolTip(ProgramNameLabelPart2, Properties.Settings.Default.ApplicationProductName + " - " + T._("Illusion games manager.\n\n"
                        + "Move mouse over wished button or text to see info about it"
                        )
                    );
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
                _thToolTip.SetToolTip(LaunchModeInfoLinkLabel, T._("Same as button in Tool tab.\n")
                    + (MOmode ? T._(
                        "Will convert game from MO Mode to Common mode\n" +
                        " when you can run exes from Data folder without Mod Organizer.\n" +
                        " You can convert game back to MO mode\n" +
                        " when it will be need to install new mods or test your mod config"
                    ) : T._(
                        "Will convert the game to MO mode\n when all mod files will be moved back to Mods folder\n" +
                        " in their folders and vanilla files restored"
                    )
                    )
                    );

                _thToolTip.SetToolTip(pbDiscord, T._("Discord page. Info, links, support."));
                _thToolTip.SetToolTip(OpenLogLinkLabel, T._("Open BepinEx log if found"));
                _thToolTip.SetToolTip(BepInExDisplayedLogLevelLabel, T._("Click here to select log level\n" +
                    "Only displays the specified log level and above in the console output"));
            }
            else if (AIGirlHelperTabControl.SelectedTab == SettingsTabPage)
            {
                _thToolTip.SetToolTip(AutoShortcutRegistryCheckBox, T._("When checked will create shortcut for the AI Helper on Desktop and will fix registry if need"));
                _thToolTip.SetToolTip(DisplaySettingsGroupBox, T._("Game Display settings"));
                _thToolTip.SetToolTip(SetupXmlLinkLabel, T._("Open Setup.xml in notepad"));
                _thToolTip.SetToolTip(ResolutionComboBox, T._("Select preferred screen resolution"));
                _thToolTip.SetToolTip(FullScreenCheckBox, T._("When checked game will be in fullscreen mode"));
                _thToolTip.SetToolTip(QualityComboBox, T._("Select preferred graphics quality"));
                //THToolTip.SetToolTip(CreateShortcutButton, T._("Will create shortcut in Desktop if not exist"));
                _thToolTip.SetToolTip(CreateShortcutLinkLabel, T._("Will create shortcut in Desktop if not exist"));
                //THToolTip.SetToolTip(FixRegistryButton, T._("Will set Data dir with game files as install dir in registry"));
                _thToolTip.SetToolTip(FixRegistryLinkLabel, T._("Will set Data dir with game files as install dir in registry"));

                //Open Folders
                _thToolTip.SetToolTip(OpenGameFolderLinkLabel, T._("Open Data folder of selected game"));
                _thToolTip.SetToolTip(OpenModsFolderLinkLabel, T._("Open Mods folder of selected game"));
                _thToolTip.SetToolTip(OpenMOFolderLinkLabel, T._("Open Mod Organizer folder"));
                _thToolTip.SetToolTip(OpenMOOverwriteFolderLinkLabel, T._("Open Overwrite folder of Mod Organizer with possible new generated files for selected game\n\nFiles here have highest priority and will be loaded over any enabled mod files"));
                _thToolTip.SetToolTip(OpenMyUserDataFolderLinkLabel, T._("Open MyUserData folder in Mods if exist\n\nHere placed usual User files of Organized ModPack for selected game"));

                _thToolTip.SetToolTip(LaunchLinksLinkLabel, T._("Open list of links for game resources"));
                _thToolTip.SetToolTip(ExtraSettingsLinkLabel, T._("Open extra setting window for plugins and etc"));

                _thToolTip.SetToolTip(CurrentGameComboBox, T._("List of found games. Current") + ": " + GameData.CurrentGame.GetGameDisplayingName());


                var toMo = ManageSettings.ModsInstallDirName();
                _thToolTip.SetToolTip(SettingsOpen2MOLinkLabel,
                    T._("Open folder, where from mod files can be installed fo selected game") +
                    T._("\n\nHere can be placed mod files which you want to install for selected game in approriate subfolders in mods" +
                    "\nand then can be installed all by one click on") + " " + InstallInModsButton.Text + " " + T._("button") +
                    "\n" + T._("which can be found in") + " " + ToolsTabPage.Text + " " + T._("tab page") +
                    "\n\n" + T._("Helper recognize") + ":"
                    + "\n " + T._(".dll files of BepinEx plugins")
                    + "\n " + T._("Sideloader mod archives")
                    + "\n " + T._("Female character cards")
                    + "\n " + T._("Female character cards in \"f\" subfolder")
                    + "\n " + T._("Male character cards in \"m\" subfolder")
                    + "\n " + T._("Coordinate clothes set cards in \"c\" subfolder")
                    + "\n " + T._("Studio scene cards in \"s\" subfolder")
                    + "\n " + T._("Cardframe Front cards in \"cf\" subfolder")
                    + "\n " + T._("Cardframe Back cards in \"cf\" subfolder")
                    + "\n " + T._("Script loader scripts")
                    + "\n " + T._("Housing plan cards in \"h\\01\", \"h\\02\", \"h\\03\" subfolders")
                    + "\n " + T._("Overlays cards in \"o\" subfolder")
                    + "\n " + T._("folders with overlays cards in \"o\" subfolder")
                    + "\n " + T._("Subfolders with modfiles")
                    + "\n " + T._("Zip archives with mod files")
                    + "\n\n" + T._("Any Rar and 7z archives will be extracted for install") +
                    T._("\nSome recognized mods can be updated instead of be installed as new mod") +
                    T._("\nMost of mods will be automatically activated except .cs scripts" +
                    "\nwhich always optional and often it is cheats or can slowdown/break game")

                    );

            }
            else if (AIGirlHelperTabControl.SelectedTab == ToolsTabPage)
            {
                _thToolTip.SetToolTip(ToolsFixModListButton, T._("Fix problems in current enabled mods list"));

                _thToolTip.SetToolTip(llOpenOldPluginsBuckupFolder,
                    T._("Open older plugins buckup folder")
                    );
                _thToolTip.SetToolTip(btnUpdateMods,
                    T._("Update Mod Organizer and enabled mods") + "\n" +
                    T._("Mod Organizer already have hardcoded info") + "\n" +
                    T._("Mods will be updated if there exist info in meta.ini notes or in updateInfo.txt") + "\n" +
                    T._("After plugins update check will be executed KKManager StandaloneUpdater for Sideloader modpack updates check for games where it is possible")
                    );
                var sideloaderPacksWarning = T._("Warning! More of packs you check more of memory game will consume.") + "\n" +
                    T._("Check only what you really using or you can 16+ gb of memory.");
                _thToolTip.SetToolTip(UseKKmanagerUpdaterLabel, T._("Check if need to run update check for sideloader modpacks.") + "\n\n" +
                    sideloaderPacksWarning
                    );
                _thToolTip.SetToolTip(UpdatePluginsLabel, T._("Check if need to run update check for plugins and Mod Organizer.")
                    );
                _thToolTip.SetToolTip(CheckEnabledModsOnlyLabel, T._("Check updates only for enabled plugins.")
                    );
                _thToolTip.SetToolTip(BleadingEdgeZipmodsLabel,
                    T._("Check also updates of Bleeding Edge Sideloader Modpack in KKManager") + "\n" +
                    T._("Bleeding Edge Sideloader modpack contains test versions of zipmods which is still not added in main modpacks") + "\n\n" +
                    sideloaderPacksWarning
                    );
                _thToolTip.SetToolTip(MOCommonModeSwitchButton, MOmode ? T._(
                        "Will convert game from MO Mode to Common mode\n" +
                        " when you can run exes from Data folder without Mod Organizer.\n You can convert game back to MO mode\n" +
                        " when it will be need to install new mods or test your mod config"
                    ) : T._(
                        "Will convert the game to MO mode\n" +
                        " when all mod files will be moved back to Mods folder\n" +
                        " in their folders and vanilla files restored"
                    )
                    );

                _thToolTip.SetToolTip(InstallInModsButton, T._("Install mods and userdata, placed in") + " " + ManageSettings.ModsInstallDirName()
                     + (MOmode ? T._(
                             " to MO format in Mods when possible"
                         ) : T._(
                             " to the game folder when possible"
                             )));
                _thToolTip.SetToolTip(Install2MODirPathOpenFolderLinkLabel, T._("Open folder where you can drop/download files for autoinstallation"));
            }
            ////////////////////////////
        }

        private void SetScreenSettings()
        {
            if (!MOmode)
            {
                SetupXmlPath = ManageSettings.GetCurrentGameSetupXmlFilePathinData();
            }

            //set Settings
            if (!File.Exists(SetupXmlPath))
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
                        if (int.Parse(screenWidth, CultureInfo.InvariantCulture) <= width[w])
                        {
                            string selectedRes = ResolutionComboBox.Items[w].ToString();
                            ResolutionComboBox.Text = selectedRes;
                            SetScreenResolution(selectedRes);
                            break;
                        }
                    }
                }
            }

            ResolutionComboBox.Text = ManageXml.ReadXmlValue(SetupXmlPath, "Setting/Size", ResolutionComboBox.Text);
            FullScreenCheckBox.Checked = bool.Parse(ManageXml.ReadXmlValue(SetupXmlPath, "Setting/FullScreen", FullScreenCheckBox.Checked + ""));

            string quality = ManageXml.ReadXmlValue(SetupXmlPath, "Setting/Quality", "2");
            //если качество будет за пределами диапазона 0-2, тогда будет равно 1 - нормально
            if (quality == "0" || quality == "1" || quality == "2")
            {
            }
            else
            {
                quality = "1";
            }

            QualityComboBox.SelectedIndex = int.Parse(quality, CultureInfo.InvariantCulture);
        }

        private static void SetScreenResolution(string resolution)
        {
            ManageModOrganizer.CheckMoUserdata();

            if (!File.Exists(SetupXmlPath))
            {
                // write default setup.xml
                Directory.CreateDirectory(Path.GetDirectoryName(SetupXmlPath));
                File.WriteAllText(SetupXmlPath, ManageSettings.GetDefaultSetupXmlValue(), Encoding.GetEncoding("UTF-16"));
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
        internal void FoldersInit()
        {
            EnableDisableSomeTools();

            SetMoMode();

            if (!Directory.Exists(ManageSettings.GetCurrentGameDataPath()))
            {
                Directory.CreateDirectory(ManageSettings.GetCurrentGameDataPath());
            }
            if (MOmode && !Directory.Exists(ManageSettings.GetCurrentGameModsDirPath()))
            {
                Directory.CreateDirectory(ManageSettings.GetCurrentGameModsDirPath());
                ModsInfoLabel.Text = T._("Mods dir created");
            }

            if (File.Exists(Path.Combine(ManageSettings.GetCurrentGameDataPath(), ManageSettings.GetCurrentGameExeName() + ".exe")))
            {
                DataInfoLabel.Text = string.Format(CultureInfo.InvariantCulture, T._("{0} game installed in {1}"), ManageSettings.GetCurrentGameDisplayingName(), "Data");
            }
            else if (File.Exists(Path.Combine(ManageSettings.GetAppResDir(), ManageSettings.GetCurrentGameExeName() + ".7z")))
            {
                DataInfoLabel.Text = string.Format(CultureInfo.InvariantCulture, T._("{0} archive in {1}"), "AIGirl", "Data");
            }
            else if (Directory.Exists(ManageSettings.GetCurrentGameDataPath()))
            {
                DataInfoLabel.Text = string.Format(CultureInfo.InvariantCulture, T._("{0} files not in {1}. Move {0} game files there."), ManageSettings.GetCurrentGameFolderName(), "Data");
            }
            else
            {
                Directory.CreateDirectory(ManageSettings.GetCurrentGameDataPath());
                DataInfoLabel.Text = string.Format(CultureInfo.InvariantCulture, T._("{0} dir created. Move {1} game files there."), "Data", ManageSettings.GetCurrentGameFolderName());
            }

            if (MOmode)
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


                if (!ManageSettings.GetCurrentGameModsDirPath().IsNullOrEmptyDirectory("*", new string[1] { "_separator" }))
                {
                    ModsInfoLabel.Text = T._("Found mod folders in Mods");
                    //button1.Enabled = false;
                    _mode = 1;
                    MainService.Text = T._("Mods Ready");
                    //MO2StandartButton.Enabled = true;
                    AIGirlHelperTabControl.SelectedTab = LaunchTabPage;

                    //if (File.Exists(Path.Combine(DataPath, ManageSettings.GetCurrentGameEXEName() + ".exe")))
                    //{
                    //}
                    //GetEnableDisableLaunchButtons();
                    MOCommonModeSwitchButton.Text = T._("MOToCommon");
                }

                {
                    //if (Directory.Exists(Install2MODirPath))
                    //{
                    //    if (Directory.GetFiles(Install2MODirPath, "*.*").Length > 0)
                    //    {
                    //        InstallInModsButton.Visible = true;
                    //        InstallInModsButton.Enabled = true;
                    //    }
                    //    string[] InstallModDirs = Directory.GetDirectories(Install2MODirPath, "*").Where(name => !name.EndsWith("Temp", StringComparison.OrdinalIgnoreCase)).ToArray();
                    //    if (InstallModDirs.Length > 0)
                    //    {
                    //        InstallInModsButton.Visible = true;
                    //        InstallInModsButton.Enabled = true;
                    //    }
                    //}
                }

                LaunchModeInfoLinkLabel.Text = T._("MO mode");

                ManageModOrganizer.DummyFiles();

                ManageModOrganizer.MoIniFixes();

                ManageModOrganizer.MakeLinks();

                //try start in another thread for perfomance purposes
                new Thread(obj => RunSlowActions()).Start();

                SetupXmlPath = ManageModOrganizer.GetSetupXmlPathForCurrentProfile();

                SetMoModsVariables();
            }
            else
            {
                SetupXmlPath = Path.Combine(ManageSettings.GetCurrentGameDataPath(), "UserData", "setup.xml");

                ModsInfoLabel.Visible = false;

                StudioButton.Enabled = false;
                //MO2StandartButton.Enabled = false;
                MOCommonModeSwitchButton.Text = T._("CommonToMO");
                MainService.Text = T._("Common mode");
                LaunchModeInfoLinkLabel.Text = T._("Common mode");
                MainService.Enabled = false;
                //AIGirlHelperTabControl.SelectedTab = LaunchTabPage;
            }

            AIGirlHelperTabControl.SelectedTab = LaunchTabPage;
            CurrentGameComboBox.Text = GameData.CurrentGame.GetGameFolderName();
            CurrentGameComboBox.SelectedIndex = ManageSettings.GetCurrentGameIndex();

            GetEnableDisableLaunchTabButtons();

            SetScreenSettings();

            SetTooltips();

            if (AutoShortcutRegistryCheckBox.Checked)
            {
                ManageOther.AutoShortcutAndRegystry();
            }

            SelectedGameLabel.Text = GameData.CurrentGame.GetGameDisplayingName() + "❤";
            this.Text = "AI Helper" + " | " + GameData.CurrentGame.GetGameDisplayingName();
        }

        private void SetMoMode(bool setText = true)
        {
            if (File.Exists(ManageSettings.GetCurrentGameMoToStandartConvertationOperationsListFilePath()))
            {
                MOmode = false;
                if (setText)
                {
                    MainService.Text = T._("Common mode");
                }
            }
            else
            {
                MOmode = true;
                if (setText)
                {
                    MainService.Text = T._("MO mode");
                }
            }
        }

        internal bool IsDebug;
        internal bool IsBetaTest;
        private void EnableDisableSomeTools()
        {
            IsDebug = Path.GetFileName(Properties.Settings.Default.ApplicationStartupPath) == "Debug" && File.Exists(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "IsDevDebugMode.txt"));
            IsBetaTest = File.Exists(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "IsBetaTest.txt"));

            //Debug

            //Beta
            btnUpdateMods.Visible = true;// IsDebug || IsBetaTest;
        }

        private static void RunSlowActions()
        {
            //создание ссылок на файлы bepinex, НА ЭТО ТРАТИТСЯ МНОГО ВРЕМЕНИ
            MOUSFSLoadingFix();
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
            if (AIGirlHelperTabControl.SelectedTab.Name != "LaunchTabPage")
            {
                return;
            }

            //MOButton.Enabled = /*Properties.Settings.Default.MOmode && */File.Exists(ManageSettings.GetMOexePath());
            //SettingsButton.Enabled = File.Exists(Path.Combine(DataPath, ManageSettings.GetINISettingsEXEName() + ".exe"));
            JPLauncherRunLinkLabel.Enabled = File.Exists(Path.Combine(ManageSettings.GetCurrentGameDataPath(), ManageSettings.GetIniSettingsExeName() + ".exe"));
            GameButton.Enabled = File.Exists(Path.Combine(ManageSettings.GetCurrentGameDataPath(), ManageSettings.GetCurrentGameExeName() + ".exe"));
            StudioButton.Enabled = File.Exists(Path.Combine(ManageSettings.GetCurrentGameDataPath(), ManageSettings.GetStudioExeName() + ".exe"));

            //Set BepInEx log data
            var bepInExCfgPath = ManageSettings.GetBepInExCfgFilePath();
            if (bepInExCfgPath.Length > 0 && File.Exists(bepInExCfgPath))
            {
                BepInExConsoleCheckBox.Enabled = true;
                try
                {
                    //BepInExConsoleCheckBox.Checked = bool.Parse(ManageINI.GetINIValueIfExist(ManageSettings.GetBepInExCfgFilePath(), "Enabled", "Logging.Console", "False"));
                    BepInExConsoleCheckBox.Checked = bool.Parse(ManageCfg.GetCfgValueIfExist(ManageSettings.GetBepInExCfgFilePath(), "Enabled", "Logging.Console", "False"));
                }
                catch (Exception ex)
                {
                    ManageLogs.Log("An error occured while GetEnableDisableLaunchTabButtons. error:\r\n" + ex);
                    BepInExConsoleCheckBox.Checked = false;
                }
            }

            //VR
            VRGameCheckBox.Visible = ManageSettings.GetCurrentGameIsHaveVr();
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            ManageOther.CheckBoxChangeColor(sender as CheckBox);
            Properties.Settings.Default.AutoShortcutRegistryCheckBoxChecked = AutoShortcutRegistryCheckBox.Checked;
            ManageIni.GetINIFile(ManageSettings.GetAiHelperIniPath()).SetKey("Settings", "autoCreateShortcutAndFixRegystry", Properties.Settings.Default.AutoShortcutRegistryCheckBoxChecked.ToString(CultureInfo.InvariantCulture));
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
                ManageProcess.RunProgram(ManageSettings.GetAppMOexePath(), string.Empty);
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

            string vr = string.Empty;
            if (VRGameCheckBox.Visible && VRGameCheckBox.Checked && ManageSettings.GetCurrentGameIsHaveVr())
            {
                vr = "VR";
            }

            string exePath;
            string arguments = string.Empty;
            if (MOmode)
            {
                var getCurrentGameExemoProfileName = ManageSettings.GetCurrentGameExemoProfileName();
                var customExeTitleName = getCurrentGameExemoProfileName + vr;
                exePath = ManageSettings.GetAppMOexePath();

                if (cbxNtlea.Checked)
                {
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
                }

                arguments = "moshortcut://:\"" + customExeTitleName + "\"";
            }
            else
            {
                if (cbxNtlea.Checked)
                {
                    exePath = ManageSettings.NtleaExePath();
                    arguments = "\"" + Path.Combine(ManageSettings.GetCurrentGameDataPath(), ManageSettings.GetCurrentGameExeName() + vr + ".exe") + "\"" + " \"C932\" \"L0411\"";
                }
                else
                {
                    exePath = Path.Combine(ManageSettings.GetCurrentGameDataPath(), ManageSettings.GetCurrentGameExeName() + vr + ".exe");
                }
            }

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
                var studio = ManageModOrganizer.GetMOcustomExecutableTitleByExeName(ManageSettings.GetStudioExeName());
                ManageProcess.RunProgram(ManageSettings.GetAppMOexePath(), "moshortcut://:" + studio);
            }
            else
            {
                ManageProcess.RunProgram(Path.Combine(ManageSettings.GetCurrentGameDataPath(), ManageSettings.GetStudioExeName() + ".exe"), string.Empty);
            }
            OnOffButtons();
        }

        private readonly Dictionary<string, string> _qualitylevels = new Dictionary<string, string>(3);

        private void QualityComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetGraphicsQuality((sender as ComboBox).SelectedIndex.ToString(CultureInfo.InvariantCulture));
        }

        internal LinksForm _linksForm;

        private void NewformButton_Click(object sender, EventArgs e)
        {
            if (_linksForm == null || _linksForm.IsDisposed)
            {
                //show and reposition of form
                //https://stackoverflow.com/questions/31492787/how-to-set-position-second-form-depend-on-first-form
                _linksForm = new LinksForm
                {
                    //LinksForm.Text = T._("Links");
                    StartPosition = FormStartPosition.Manual
                };
                _linksForm.Load += delegate (object s2, EventArgs e2)
                {
                    _linksForm.Location = new Point(Bounds.Location.X + (Bounds.Width / 2) - (_linksForm.Width / 2),
                        Bounds.Location.Y + /*(Bounds.Height / 2) - (f2.Height / 2) +*/ Bounds.Height);
                };
                _linksForm.Text = T._("Links");
                newformButton.Text = @"/\";
                if (_extraSettingsForm != null && !_extraSettingsForm.IsDisposed)
                {
                    _extraSettingsForm.Close();
                }
                _linksForm.Show();
                _linksForm.TopMost = true;
            }
            else
            {
                newformButton.Text = @"\/";
                _linksForm.Close();
            }
        }

        private void AIHelper_LocationChanged(object sender, EventArgs e)
        {
            if (GameData.CurrentGame == null)
            {
                return;
            }

            //move second form with main form
            //https://stackoverflow.com/questions/3429445/how-to-move-two-windows-forms-together
            if (_linksForm == null || _linksForm.IsDisposed)
            {
            }
            else
            {
                if (_linksForm.WindowState == FormWindowState.Minimized)
                {
                    _linksForm.WindowState = FormWindowState.Normal;
                }
                _linksForm.Location = new Point(Bounds.Location.X + (Bounds.Width / 2) - (_linksForm.Width / 2),
                    Bounds.Location.Y + /*(Bounds.Height / 2) - (f2.Height / 2) +*/ Bounds.Height);
            }
            if (_extraSettingsForm == null || _extraSettingsForm.IsDisposed)
            {
            }
            else
            {
                if (_extraSettingsForm.WindowState == FormWindowState.Minimized)
                {
                    _extraSettingsForm.WindowState = FormWindowState.Normal;
                }
                _extraSettingsForm.Location = new Point(Bounds.Location.X + (Bounds.Width / 2) - (_extraSettingsForm.Width / 2),
                    Bounds.Location.Y + /*(Bounds.Height / 2) - (f2.Height / 2) +*/ Bounds.Height);
            }
        }

        private async void InstallInModsButton_Click(object sender, EventArgs e)
        {
            if (new UpdateMaker().MakeUpdate())
            {
                MessageBox.Show("Made update instead");
                return;
            }

            List<ModInstallerBase> installers = GetListOfSubClasses.Inherited.GetListOfinheritedSubClasses<ModInstallerBase>().OrderBy(o => o.Order).ToList();

            //if (Directory.Exists(Install2MoDirPath) && (Directory.GetFiles(Install2MoDirPath, "*.rar").Length > 0 || Directory.GetFiles(Install2MoDirPath, "*.7z").Length > 0 || Directory.GetFiles(Install2MoDirPath, "*.png").Length > 0 || Directory.GetFiles(Install2MoDirPath, "*.cs").Length > 0 || Directory.GetFiles(Install2MoDirPath, "*.dll").Length > 0 || Directory.GetFiles(Install2MoDirPath, "*.zipmod").Length > 0 || Directory.GetFiles(Install2MoDirPath, "*.zip").Length > 0 || Directory.GetDirectories(Install2MoDirPath, "*").Length > 0))
            if (!IsInstallDirHasAnyRequiredFileFrom(installers))
            {
                MessageBox.Show(T._("No compatible for installation formats found in install folder.\n\nIt must be archvives, game files or folders with game files.\n\nWill be opened installation dir where you can drop files for installation."));
            }
            else
            {
                OnOffButtons(false);

                //impossible to correctly update mods in common mode
                if (!ManageSettings.IsMoMode())
                {
                    DialogResult result = MessageBox.Show(T._("Attention") + "\n\n" + T._("Impossible to correctly install/update mods\n\n in standart mode because files was moved in Data.") + "\n\n" + T._("Switch to MO mode?"), T._("Confirmation"), MessageBoxButtons.OKCancel);
                    if (result == DialogResult.OK)
                    {
                        ManageMOModeSwitch.SwitchBetweenMoAndStandartModes();
                    }
                    else
                    {
                        OnOffButtons();
                        return;
                    }
                }

                await Task.Run(() => InstallModFilesAndCleanEmptyFolder(installers)).ConfigureAwait(true);

                InstallInModsButton.Text = T._("Install from") + " " + ManageSettings.ModsInstallDirName();

                OnOffButtons();

                //обновление информации о конфигурации папок игры
                FoldersInit();

                MessageBox.Show(T._("All possible mods installed. Install all rest in install folder manually."));
            }

            Directory.CreateDirectory(ManageSettings.GetInstall2MoDirPath());
            Process.Start("explorer.exe", ManageSettings.GetInstall2MoDirPath());
        }

        private static bool IsInstallDirHasAnyRequiredFileFrom(List<ModInstallerBase> installers)
        {
            foreach (var installer in installers)
            {
                var IsDirInstaller = installer is DirectoriesInstallerBase;
                foreach (var mask in installer.Masks)
                {
                    if ((IsDirInstaller && ManageFilesFoldersExtensions.IsAnySubDirExistsInTheDir(ManageSettings.GetInstall2MoDirPath(), mask))
                        || (!IsDirInstaller && ManageFilesFoldersExtensions.IsAnyFileExistsInTheDir(ManageSettings.GetInstall2MoDirPath(), mask, allDirectories: false)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void InstallModFilesAndCleanEmptyFolder(List<ModInstallerBase> installers)
        {

            foreach (var installer in installers)
            {
                installer.Install();
            }

            //string installMessage = T._("Installing");
            //InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage));
            //new RarExtractor().Install();
            //InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage + "."));
            //new SevenZipExtractor().Install();
            //InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage + ".."));
            //new CsScriptsInstaller().Install();

            //InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage + "..."));
            //new ZipInstaller().Install();

            //InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage));
            //new BebInExDllInstaller().Install();

            //InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage + "."));
            //new SideloaderZipmod().Install();

            //InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage + ".."));
            //new PngInstaller().Install();

            //InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage + "..."));
            //new ModFilesFromDir().Install();

            //InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage));
            //new CardsFromDirsInstaller().Install();

            //InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage + "."));
            ManageFilesFoldersExtensions.DeleteEmptySubfolders(ManageSettings.GetInstall2MoDirPath(), false);

            if (!Directory.Exists(ManageSettings.GetInstall2MoDirPath()))
            {
                Directory.CreateDirectory(ManageSettings.GetInstall2MoDirPath());
            }

            //InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = T._("Install from") + " " + ManageSettings.ModsInstallDirName()));
        }

        private void CreateShortcutButton_Click(object sender, EventArgs e)
        {
            ManageOther.CreateShortcuts(true, false);
        }

        private void MO2StandartButton_Click(object sender, EventArgs e)
        {
            ManageMOModeSwitch.SwitchBetweenMoAndStandartModes();
        }

        private void OpenGameFolderLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Directory.Exists(ManageSettings.GetCurrentGameDataPath()))
                Process.Start("explorer.exe", ManageSettings.GetCurrentGameDataPath());
        }

        private void OpenMOFolderLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Directory.Exists(ManageSettings.GetCurrentGameModOrganizerIniPath()))
                Process.Start("explorer.exe", ManageSettings.GetCurrentGameModOrganizerIniPath());
        }

        private void OpenModsFolderLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Directory.Exists(ManageSettings.GetCurrentGameModsDirPath()))
                Process.Start("explorer.exe", ManageSettings.GetCurrentGameModsDirPath());
        }

        private void Install2MODirPathOpenFolderLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!Directory.Exists(ManageSettings.GetInstall2MoDirPath()))
            {
                Directory.CreateDirectory(ManageSettings.GetInstall2MoDirPath());
            }
            Process.Start("explorer.exe", ManageSettings.GetInstall2MoDirPath());
        }

        private void OpenMyUserDataFolderLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string userFilesFolder = Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), "MyUserData");
            if (!Directory.Exists(userFilesFolder))
            {
                userFilesFolder = Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), "MyUserFiles");
            }
            if (!Directory.Exists(userFilesFolder))
            {
                userFilesFolder = ManageSettings.GetCurrentGameOverwriteFolderPath();
            }
            if (Directory.Exists(userFilesFolder))
            {
                Process.Start("explorer.exe", userFilesFolder);
            }
        }

        private void OpenMOOverwriteFolderLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Directory.Exists(ManageSettings.GetCurrentGameOverwriteFolderPath()))
            {
                Process.Start("explorer.exe", ManageSettings.GetCurrentGameOverwriteFolderPath());
            }
        }

        private void OpenLogLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenBepinexLog();
        }

        private void CurrentGameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //bool init = Properties.Settings.Default.INITDone;
            //bool change = Properties.Settings.Default.CurrentGameIsChanging;

            if (Properties.Settings.Default.INITDone && !Properties.Settings.Default.CurrentGameIsChanging && !Properties.Settings.Default.SetModOrganizerINISettingsForTheGame)
            {
                Properties.Settings.Default.CurrentGameIsChanging = true;

                SetSelectedGameIndexAndBasicVariables((sender as ComboBox).SelectedIndex);
                Properties.Settings.Default.CurrentGameListIndex = (sender as ComboBox).SelectedIndex;
                ActionsOnGameChanged();

                ManageIni.GetINIFile(ManageSettings.GetAiHelperIniPath()).SetKey("Settings", "selected_game", ManageSettings.GetCurrentGameFolderName());

                FoldersInit();

                Properties.Settings.Default.CurrentGameIsChanging = false;
            }
            else
            {
                CurrentGameComboBox.SelectedItem = ManageSettings.GetCurrentGameFolderName();
            }
        }

        private void ActionsOnGameChanged()
        {
            CloseExtraForms();
            //cleaning previous game data
            //File.Delete(ManageSettings.GetModOrganizerINIpath());
            //File.Delete(ManageSettings.GetMOcategoriesPath());
            ManageModOrganizer.RedefineGameMoData();
            Properties.Settings.Default.BepinExCfgPath = string.Empty;
            Properties.Settings.Default.MOSelectedProfileDirName = string.Empty;

            ManageModOrganizer.CheckBaseGamesPy();

            GameData.CurrentGame.InitActions();
            CurrentGameTitleTextBox.Text = GameData.CurrentGame.GetGameDisplayingName();
        }

        private void CloseExtraForms()
        {
            if (_linksForm != null && !_linksForm.IsDisposed)
            {
                _linksForm.Close();
            }
            if (_extraSettingsForm != null && !_extraSettingsForm.IsDisposed)
            {
                _extraSettingsForm.Close();
            }
        }

        private void ConsoleCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            //ManageINI.WriteINIValue(ManageSettings.GetBepInExCfgFilePath(), "Logging.Console", "Enabled", /*" " +*/ (sender as CheckBox).Checked.ToString(CultureInfo.InvariantCulture));
            var bepinExcfg = ManageSettings.GetBepInExCfgFilePath();
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
            {
                ManageSettings.SwitchBepInExDisplayedLogLevelValue(BepInExConsoleCheckBox, BepInExDisplayedLogLevelLabel);
            }
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
                if (_linksForm != null && !_linksForm.IsDisposed)
                {
                    _linksForm.Close();
                }
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
                ManageProcess.RunProgram(ManageSettings.GetAppMOexePath(), "moshortcut://:" + ManageSettings.GetIniSettingsExeName());
            }
            else
            {
                ManageProcess.RunProgram(Path.Combine(ManageSettings.GetCurrentGameDataPath(), ManageSettings.GetIniSettingsExeName() + ".exe"), string.Empty);
            }
            OnOffButtons();
        }

        private void AI_Helper_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (GameData.CurrentGame == null)
            {
                return;
            }

            try
            {
                //write last game folder name
                ManageIni.GetINIFile(ManageSettings.GetAiHelperIniPath()).SetKey("Settings", "selected_game", ManageSettings.GetCurrentGameFolderName());
            }
            catch (Exception ex)
            {
                ManageLogs.Log("An error occered in time of the app closing. error:\r\n" + ex);
            }
            //нашел баг, когда при открытии свойства ссылки в проводнике
            //, с последующим закрытием свойств и закрытием AI Helper происходит блокировка папки проводником и при следующем запуске происходит ошибка AI Helper, до разблокировки папки
            //также если пользователь решит запускать МО без помощника, игра не запустится, т.к. фикса бепинекс нет
            //ManageMOMods.BepinExLoadingFix(true);
        }

        private void SetupXmlPathLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(SetupXmlPath))
            {
                Process.Start("notepad.exe", SetupXmlPath);
            }
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
            OnOffButtons(false);
            //impossible to correctly update mods in common mode
            if (!ManageSettings.IsMoMode())
            {
                DialogResult result = MessageBox.Show(T._("Attention") + "\n\n" + T._("Correct modlist fixes possible only in MO mode") + "\n\n" + T._("Switch to MO mode?"), T._("Confirmation"), MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    ManageMOModeSwitch.SwitchBetweenMoAndStandartModes();
                }
                else
                {
                    OnOffButtons();
                    return;
                }
            }

            new ManageRules.ModList().ModlistFixes();

            OnOffButtons();

            FoldersInit();
        }

        private void btnUpdateMods_Click(object sender, EventArgs e)
        {
            ManageUpdateMods.UpdateMods();
        }

        private void PbDiscord_Click(object sender, EventArgs e)
        {
            Process.Start(ManageSettings.GetDiscordGroupLink());//Program's discord server
        }

        private void LlOpenOldPluginsBuckupFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var modUpdatesBakDir = ManageSettings.GetUpdatedModsOlderVersionsBuckupDirPath();
            if (!Directory.Exists(modUpdatesBakDir))
            {
                Directory.CreateDirectory(modUpdatesBakDir);
            }
            Process.Start("explorer.exe", modUpdatesBakDir);
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
            var b = File.Exists(ManageSettings.KkManagerStandaloneUpdaterExePath());
            //UseKKmanagerUpdaterLabel.SetCheck();
            UseKKmanagerUpdaterLabel.Visible = b && GameData.CurrentGame.IsHaveSideloaderMods;
            //UseKKmanagerUpdaterCheckBox.Visible = b && GameData.CurrentGame.IsHaveSideloaderMods;
            BleadingEdgeZipmodsLabel.Visible = UseKKmanagerUpdaterLabel.Visible;
            //cbxBleadingEdgeZipmods.Visible = UseKKmanagerUpdaterCheckBox.Visible;

            CheckEnabledModsOnlyLabel.Enabled = UpdatePluginsLabel.IsChecked();
            btnUpdateMods.Enabled = (UpdatePluginsLabel.Visible && UpdatePluginsLabel.IsChecked()) || (UseKKmanagerUpdaterLabel.Visible && UseKKmanagerUpdaterLabel.IsChecked());
            //btnUpdateMods.Enabled = (UpdatePluginsCheckBox.Visible && UpdatePluginsCheckBox.Checked) || (UseKKmanagerUpdaterCheckBox.Visible && UseKKmanagerUpdaterCheckBox.Checked);

            //check bleeding edge txt
            //cbxBleadingEdgeZipmods.Checked = cbxBleadingEdgeZipmods.Visible && File.Exists(ManageSettings.ZipmodsBleedingEdgeMarkFilePath());
            BleadingEdgeZipmodsLabel.SetCheck(BleadingEdgeZipmodsLabel.Visible && UseKKmanagerUpdaterLabel.IsChecked() && File.Exists(ManageSettings.ZipmodsBleedingEdgeMarkFilePath()));
            BleadingEdgeZipmodsLabel.Enabled = BleadingEdgeZipmodsLabel.Visible && UseKKmanagerUpdaterLabel.IsChecked();
        }

        private void AIGirlHelperTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetTooltips();
        }

        //private void UseKKmanagerUpdaterCheckBox_MouseClick(object sender, MouseEventArgs e)
        //{
        //    UpdateButtonOptionsRefresh();
        //}

        //private void UpdatePluginsCheckBox_MouseClick(object sender, MouseEventArgs e)
        //{
        //    UpdateButtonOptionsRefresh();
        //}

        //private void cbxBleadingEdgeZipmods_CheckedChanged(object sender, EventArgs e)
        //{
        //    if ((sender as CheckBox).Checked && !File.Exists(ManageSettings.ZipmodsBleedingEdgeMarkFilePath()))
        //    {
        //        Directory.CreateDirectory(Path.GetDirectoryName(ManageSettings.ZipmodsBleedingEdgeMarkFilePath()));
        //        File.WriteAllText(ManageSettings.ZipmodsBleedingEdgeMarkFilePath(), string.Empty);
        //    }
        //    else if (!(sender as CheckBox).Checked && File.Exists(ManageSettings.ZipmodsBleedingEdgeMarkFilePath()))
        //    {
        //        File.Delete(ManageSettings.ZipmodsBleedingEdgeMarkFilePath());
        //    }

        //    UpdateButtonOptionsRefresh();
        //}

        private void UpdatePluginsLabel_Click(object sender, EventArgs e)
        {
            RefreshLabelCheckState(sender);
        }

        private void UseKKmanagerUpdaterLabel_Click(object sender, EventArgs e)
        {
            RefreshLabelCheckState(sender);
        }

        private void BleadingEdgeZipmodsLabel_Click(object sender, EventArgs e)
        {
            (sender as Label).SetCheck(!(sender as Label).IsChecked());

            if ((sender as Label).IsChecked() && !File.Exists(ManageSettings.ZipmodsBleedingEdgeMarkFilePath()))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ManageSettings.ZipmodsBleedingEdgeMarkFilePath()));
                File.WriteAllText(ManageSettings.ZipmodsBleedingEdgeMarkFilePath(), string.Empty);
            }
            else if (!(sender as Label).IsChecked() && File.Exists(ManageSettings.ZipmodsBleedingEdgeMarkFilePath()))
            {
                File.Delete(ManageSettings.ZipmodsBleedingEdgeMarkFilePath());
            }

            UpdateButtonOptionsRefresh();
        }

        private void RefreshLabelCheckState(object sender)
        {
            var label = (sender as Label);
            label.SetCheck(!label.IsChecked());
            UpdateButtonOptionsRefresh();
        }

        private void CheckEnabledModsOnlyLabel_Click(object sender, EventArgs e)
        {
            RefreshLabelCheckState(sender);
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