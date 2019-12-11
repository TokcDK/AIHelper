using AI_Helper.Games;
using AI_Helper.Manage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//using Crc32C;

namespace AI_Helper
{
    public partial class AIHelper : Form
    {
        private readonly bool compressmode = false;

        //constants
        private static string AppResDir { get => Properties.Settings.Default.AppResDir; set => Properties.Settings.Default.AppResDir = value; }

        private static string ModsPath { get => Properties.Settings.Default.ModsPath; set => Properties.Settings.Default.ModsPath = value; }
        private static string DownloadsPath { get => Properties.Settings.Default.DownloadsPath; set => Properties.Settings.Default.DownloadsPath = value; }
        private static string DataPath { get => Properties.Settings.Default.DataPath; set => Properties.Settings.Default.DataPath = value; }
        private static string MODirPath { get => Properties.Settings.Default.MODirPath; set => Properties.Settings.Default.MODirPath = value; }
        private static string MOexePath { get => Properties.Settings.Default.MOexePath; set => Properties.Settings.Default.MOexePath = value; }
        private static string OverwriteFolder { get => Properties.Settings.Default.OverwriteFolder; set => Properties.Settings.Default.OverwriteFolder = value; }
        private static string OverwriteFolderLink { get => Properties.Settings.Default.OverwriteFolderLink; set => Properties.Settings.Default.OverwriteFolderLink = value; }
        private static string SetupXmlPath { get => Properties.Settings.Default.SetupXmlPath; set => Properties.Settings.Default.SetupXmlPath = value; }
        private static string ApplicationStartupPath { /*get => Properties.Settings.Default.ApplicationStartupPath; */set => Properties.Settings.Default.ApplicationStartupPath = value; }

        //private static string ModOrganizerINIpath { get => Properties.Settings.Default.ModOrganizerINIpath; set => Properties.Settings.Default.ModOrganizerINIpath = value; }
        private static string Install2MODirPath { get => Properties.Settings.Default.Install2MODirPath; set => Properties.Settings.Default.Install2MODirPath = value; }

        private static bool MOmode { get => Properties.Settings.Default.MOmode; set => Properties.Settings.Default.MOmode = value; }

        private Game CurrentGame;
        private List<Game> ListOfGames;

        public AIHelper()
        {
            InitializeComponent();

            SetListOfGames();

            VariablesINIT();
            MOmode = true;
            CurrentGame.InitActions();

            SetLocalizationStrings();

            FoldersInit();

            //if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\illusion\AI-Syoujyo\AI-SyoujyoTrial", "INSTALLDIR", null) == null)
            //{
            //    FixRegistryButton.Visible = true;
            //}

            Properties.Settings.Default.INITDone = true;
        }

        private void VariablesINIT()
        {
            ApplicationStartupPath = Application.StartupPath;
            Properties.Settings.Default.CurrentGamePath = CurrentGame.GetGamePath();
            //SettingsManage.SettingsINIT();
            AppResDir = ManageSettings.GetAppResDir();
            ModsPath = ManageSettings.GetModsPath();
            DownloadsPath = ManageSettings.GetDownloadsPath();
            DataPath = ManageSettings.GetDataPath();
            MODirPath = ManageSettings.GetMOdirPath();
            MOexePath = ManageSettings.GetMOexePath();
            Properties.Settings.Default.ModOrganizerINIpath = ManageSettings.GetModOrganizerINIpath();
            Install2MODirPath = ManageSettings.GetInstall2MODirPath();
            OverwriteFolder = ManageSettings.GetOverwriteFolder();
            OverwriteFolderLink = ManageSettings.GetOverwriteFolderLink();
            SetupXmlPath = ManageMO.GetSetupXmlPathForCurrentProfile();
        }

        private void SetListOfGames()
        {
            ListOfGames = ManageSettings.GetListOfExistsGames();

            if (ListOfGames == null || ListOfGames.Count == 0)
            {
                MessageBox.Show(T._("Games not found") + ". " + T._("Exit") + "..");
                Application.Exit();
            }

            foreach (var game in ListOfGames)
            {
                CurrentGameComboBox.Items.Add(game.GetGameFolderName());
            }

            SetSelectedGameIndexAndBasicVariables(
                ManageSettings.GetCurrentGameIndexByFolderName(
                    ListOfGames
                    ,
                    new IniFile(ManageSettings.GetAIHelperINIPath()).ReadINI("Settings", "selected_game")
                    ));
        }

        private void SetSelectedGameIndexAndBasicVariables(int index = 0)
        {
            Properties.Settings.Default.CurrentGameListIndex = index;
            CurrentGame = ListOfGames[Properties.Settings.Default.CurrentGameListIndex];
            CurrentGameComboBox.SelectedIndex = index;

            Properties.Settings.Default.CurrentGameEXEName = CurrentGame.GetGameEXEName();
            Properties.Settings.Default.CurrentGameFolderName = CurrentGame.GetGameFolderName();
            Properties.Settings.Default.StudioEXEName = CurrentGame.GetGameStudioEXEName();
            Properties.Settings.Default.INISettingsEXEName = CurrentGame.GetINISettingsEXEName();
            Properties.Settings.Default.CurrentGamePath = CurrentGame.GetGamePath();
            Properties.Settings.Default.DataPath = Path.Combine(CurrentGame.GetGamePath(), "Data");
            Properties.Settings.Default.ModsPath = Path.Combine(CurrentGame.GetGamePath(), "Mods");

            //set checkbox
            Properties.Settings.Default.AutoShortcutRegistryCheckBoxChecked = bool.Parse(ManageINI.GetINIValueIfExist(ManageSettings.GetAIHelperINIPath(), "autoCreateShortcutAndFixRegystry", "Settings", "False"));
            AutoShortcutRegistryCheckBox.Checked = Properties.Settings.Default.AutoShortcutRegistryCheckBoxChecked;
        }

        private void SetLocalizationStrings()
        {
            this.Text = T._("AI Helper for Organized ModPack");
            InstallInModsButton.Text = T._("Install from 2MO");
            //button1.Text = T._("Prepare the game");
            SettingsPage.Text = T._("Settings");
            CreateShortcutButton.Text = T._("Shortcut");
            FixRegistryButton.Text = T._("Registry");
            groupBox1.Text = T._("Display");
            FullScreenCheckBox.Text = T._("fullscreen");
            AutoShortcutRegistryCheckBox.Text = T._("Auto");
            SettingsFoldersGroupBox.Text = T._("Folders");
            OpenGameFolderLinkLabel.Text = T._("Game");
            OpenModsFolderLinkLabel.Text = T._("Mods");
            MainPage.Text = T._("Info");
            LaunchTabPage.Text = T._("Launch");
            LaunchTabLaunchLabel.Text = T._("Launch");
            ToolsTabPage.Text = T._("Tools");
            StudioButton.Text = T._("Studio");
            GameButton.Text = T._("Game");
            MOButton.Text = T._("Manager");
            SettingsButton.Text = T._("Settings");
            QualityComboBox.Items.Add(T._("Perfomance"));
            QualityComboBox.Items.Add(T._("Normal"));
            QualityComboBox.Items.Add(T._("Quality"));
        }

        private int mode = 0;

        private void Button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;

            switch (mode)
            {
                case 0:
                    CompressingMode();
                    break;

                case 1:
                    AIGirlHelperTabControl.SelectedTab = LaunchTabPage;
                    break;

                case 2:
                    ExtractingMode();
                    break;

                default:
                    break;
            }

            button1.Enabled = true;
        }

        private async void ExtractingMode()
        {
            button1.Text = T._("Extracting") + "..";

            //https://ru.stackoverflow.com/questions/222414/%d0%9a%d0%b0%d0%ba-%d0%bf%d1%80%d0%b0%d0%b2%d0%b8%d0%bb%d1%8c%d0%bd%d0%be-%d0%b2%d1%8b%d0%bf%d0%be%d0%bb%d0%bd%d0%b8%d1%82%d1%8c-%d0%bc%d0%b5%d1%82%d0%be%d0%b4-%d0%b2-%d0%be%d1%82%d0%b4%d0%b5%d0%bb%d1%8c%d0%bd%d0%be%d0%bc-%d0%bf%d0%be%d1%82%d0%be%d0%ba%d0%b5
            await Task.Run(() => UnpackGame());
            await Task.Run(() => UnpackMO());
            await Task.Run(() => UnpackMods());

            //BepinExLoadingFix();//добавлено в folderinit

            ManageOther.CreateShortcuts();

            ManageMO.MakeDummyFiles();

            button1.Text = T._("Game Ready");
            FoldersInit();
        }

        private void UnpackMO()
        {
            if (MOmode)
            {
                string MO7zip = Path.Combine(AppResDir, "MO.7z");
                if (File.Exists(MO7zip) && !File.Exists(Path.Combine(MODirPath, "ModOrganizer.exe")))
                {
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Visible = true));
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Marquee));
                    _ = DataInfoLabel.Invoke((Action)(() => DataInfoLabel.Text = T._("Extracting")));
                    _ = ModsInfoLabel.Invoke((Action)(() => ModsInfoLabel.Text = T._("MO archive") + ": " + Path.GetFileNameWithoutExtension(MO7zip)));
                    Compressor.Decompress(MO7zip, MODirPath);
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Blocks));
                }
            }
        }

        private void UnpackGame()
        {
            if (Directory.Exists(DataPath))
            {
                string AIGirlTrial = Path.Combine(AppResDir, "AIGirlTrial.7z");
                string AIGirl = Path.Combine(AppResDir, "AIGirl.7z");
                if (File.Exists(AIGirlTrial) && !File.Exists(Path.Combine(DataPath, "AI-SyoujyoTrial.exe")))
                {
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Visible = true));
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Marquee));
                    _ = DataInfoLabel.Invoke((Action)(() => DataInfoLabel.Text = T._("Extracting")));
                    _ = ModsInfoLabel.Invoke((Action)(() => ModsInfoLabel.Text = T._("Game archive") + ": " + Path.GetFileNameWithoutExtension(AIGirlTrial)));
                    Compressor.Decompress(AIGirlTrial, DataPath);
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Blocks));
                }
                else if (File.Exists(AIGirl) && !File.Exists(Path.Combine(DataPath, "AI-Syoujyo.exe")))
                {
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Visible = true));
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Marquee));
                    _ = DataInfoLabel.Invoke((Action)(() => DataInfoLabel.Text = T._("Extracting")));
                    _ = ModsInfoLabel.Invoke((Action)(() => ModsInfoLabel.Text = T._("Game archive") + ": " + Path.GetFileNameWithoutExtension(AIGirl)));
                    Compressor.Decompress(AIGirl, DataPath);
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Blocks));
                }
            }
        }

        private void UnpackMods()
        {
            if (Directory.Exists(ModsPath) && Directory.Exists(DownloadsPath))
            {
                if (Directory.Exists(DownloadsPath))
                {
                    string[] ModDirs = Directory.GetDirectories(ModsPath, "*").Where(name => !name.EndsWith("_separator", StringComparison.OrdinalIgnoreCase)).ToArray();
                    string[] files = Directory.GetFiles(DownloadsPath, "*.7z", SearchOption.AllDirectories).Where(name => !ModDirs.Contains(Path.Combine(ModsPath, Path.GetFileNameWithoutExtension(name)))).ToArray();
                    if (files.Length == 0)
                    {
                    }
                    else
                    {
                        int i = 1;
                        progressBar1.Invoke((Action)(() => progressBar1.Visible = true));
                        progressBar1.Invoke((Action)(() => progressBar1.Maximum = files.Length));
                        progressBar1.Invoke((Action)(() => progressBar1.Value = i));
                        foreach (string file in files)
                        {
                            string filename = Path.GetFileNameWithoutExtension(file);
                            DataInfoLabel.Invoke((Action)(() => DataInfoLabel.Text = T._("Extracting") + " " + +i + "/" + files.Length));
                            ModsInfoLabel.Invoke((Action)(() => ModsInfoLabel.Text = T._("Mod") + ": " + filename));
                            string moddirpath = Path.Combine(ModsPath, filename);
                            if (!Directory.Exists(moddirpath))
                            {
                                Compressor.Decompress(file, moddirpath);
                            }
                            progressBar1.Invoke((Action)(() => progressBar1.Value = i));
                            i++;
                        }
                        progressBar1.Invoke((Action)(() => progressBar1.Visible = false));
                    }
                }

                //Unpack separators
                string separators = Path.Combine(AppResDir, "MOModsSeparators.7z");
                if (File.Exists(separators))
                {
                    Compressor.Decompress(separators, ModsPath);
                }
            }
        }

        private async void CompressingMode()
        {
            if (compressmode)
            {
                button1.Text = "Compressing..";

                //https://ru.stackoverflow.com/questions/222414/%d0%9a%d0%b0%d0%ba-%d0%bf%d1%80%d0%b0%d0%b2%d0%b8%d0%bb%d1%8c%d0%bd%d0%be-%d0%b2%d1%8b%d0%bf%d0%be%d0%bb%d0%bd%d0%b8%d1%82%d1%8c-%d0%bc%d0%b5%d1%82%d0%be%d0%b4-%d0%b2-%d0%be%d1%82%d0%b4%d0%b5%d0%bb%d1%8c%d0%bd%d0%be%d0%bc-%d0%bf%d0%be%d1%82%d0%be%d0%ba%d0%b5
                //await Task.Run(() => PackGame());
                //await Task.Run(() => PackMO());
                await Task.Run(() => PackMods());
                await Task.Run(() => PackSeparators());

                ////http://www.sql.ru/forum/1149655/kak-peredat-parametr-s-metodom-delegatom
                //Thread open = new Thread(new ParameterizedThreadStart((obj) => PackMods()));
                //open.Start();

                button1.Text = T._("Prepare the game");
                FoldersInit();
            }
            else
            {
                AIGirlHelperTabControl.SelectedTab = LaunchTabPage;
            }
        }

        private void PackMO()
        {
            if (Directory.Exists(MODirPath) && Directory.Exists(AppResDir))
            {
                _ = progressBar1.Invoke((Action)(() => progressBar1.Visible = true));
                _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Marquee));
                _ = DataInfoLabel.Invoke((Action)(() => DataInfoLabel.Text = "Compressing"));
                _ = ModsInfoLabel.Invoke((Action)(() => ModsInfoLabel.Text = "MO archive.."));
                Compressor.Compress(MODirPath, AppResDir);
                _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Blocks));
            }
        }

        private void PackGame()
        {
            if (Directory.Exists(DataPath) && Directory.Exists(AppResDir))
            {
                string AIGirlTrial = Path.Combine(AppResDir, "AIGirlTrial.7z");
                string AIGirl = Path.Combine(AppResDir, "AIGirl.7z");
                if (!File.Exists(AIGirlTrial)
                    && (File.Exists(Path.Combine(DataPath, ManageSettings.GetCurrentGameEXEName() + ".exe"))))
                {
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Visible = true));
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Marquee));
                    _ = DataInfoLabel.Invoke((Action)(() => DataInfoLabel.Text = "Compressing"));
                    _ = ModsInfoLabel.Invoke((Action)(() => ModsInfoLabel.Text = "Game archive: " + Path.GetFileNameWithoutExtension(AIGirlTrial)));
                    Compressor.Compress(DataPath, AppResDir);
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Blocks));
                }
            }
        }

        private void PackMods()
        {
            if (Directory.Exists(ModsPath))
            {
                string[] dirs = Directory.GetDirectories(ModsPath, "*").Where(name => !name.EndsWith("_separator", StringComparison.OrdinalIgnoreCase)).ToArray();//с игнором сепараторов
                if (dirs.Length == 0)
                {
                }
                else
                {
                    //Read categories.dat
                    List<CategoriesList> categories = new List<CategoriesList>();
                    foreach (string line in File.ReadAllLines(ManageSettings.GetMOcategoriesPath()))
                    {
                        if (line.Length == 0)
                        {
                        }
                        else
                        {
                            string[] linevalues = line.Split('|');
                            categories.Add(new CategoriesList(linevalues[0], linevalues[1], linevalues[2], linevalues[3]));
                        }
                    }

                    int i = 1;
                    progressBar1.Invoke((Action)(() => progressBar1.Visible = true));
                    progressBar1.Invoke((Action)(() => progressBar1.Maximum = dirs.Length));
                    progressBar1.Invoke((Action)(() => progressBar1.Value = i));
                    foreach (string dir in dirs)
                    {
                        DataInfoLabel.Invoke((Action)(() => DataInfoLabel.Text = "Compressing " + i + "/" + dirs.Length));
                        ModsInfoLabel.Invoke((Action)(() => ModsInfoLabel.Text = "Folder: " + Path.GetFileNameWithoutExtension(dir)));

                        Compressor.Compress(dir, GetResultTargetName(categories, dir));

                        progressBar1.Invoke((Action)(() => progressBar1.Value = i));
                        i++;
                    }
                    progressBar1.Invoke((Action)(() => progressBar1.Visible = false));
                }
            }
        }

        private void PackSeparators()
        {
            if (Directory.Exists(ModsPath))
            {
                string[] dirs = Directory.GetDirectories(ModsPath, "*").Where(name => name.EndsWith("_separator", StringComparison.OrdinalIgnoreCase)).ToArray();//с игнором сепараторов
                if (dirs.Length == 0)
                {
                }
                else
                {
                    //progressBar1.Invoke((Action)(() => progressBar1.Visible = true));
                    //progressBar1.Invoke((Action)(() => progressBar1.Maximum = dirs.Length));
                    //progressBar1.Invoke((Action)(() => progressBar1.Value = 0));
                    string tempdir = Path.Combine(ModsPath, "MOModsSeparators");

                    DataInfoLabel.Invoke((Action)(() => DataInfoLabel.Text = "Compressing"));
                    ModsInfoLabel.Invoke((Action)(() => ModsInfoLabel.Text = "Folder: " + Path.GetFileNameWithoutExtension(tempdir)));

                    Directory.CreateDirectory(tempdir);
                    foreach (string dir in dirs)
                    {
                        CopyFolder.Copy(dir, Path.Combine(tempdir, Path.GetFileName(dir)));
                    }

                    Compressor.Compress(tempdir, AppResDir);
                    Directory.Delete(tempdir, true);

                    //progressBar1.Invoke((Action)(() => progressBar1.Visible = false));
                }
            }
        }

        /// <summary>
        /// Get result subfolder in Downloads dir as set for mod in categories.dat
        /// </summary>
        /// <param name="categories"></param>
        /// <param name="inputmoddir"></param>
        /// <returns></returns>
        private string GetResultTargetName(List<CategoriesList> categories, string inputmoddir)
        {
            string targetdir = DownloadsPath;

            //Sideloader mods
            if (Directory.Exists(Path.Combine(inputmoddir, "mods")))
            {
                targetdir = Path.Combine(targetdir, "Sideloader");
            }

            string categoryvalue = ManageMO.GetMetaParameterValue(Path.Combine(inputmoddir, "meta.ini"), "category").Replace("\"", string.Empty);
            if (categoryvalue.Length == 0)
            {
            }
            else
            {
                //Subcategory from meta
                categoryvalue = categoryvalue.Split(',')[0];//взять только первое значение
                int categiryindex = int.Parse(categoryvalue) - 1;//В List индекс идет от нуля
                if (categiryindex > 0)
                {
                    if (categiryindex < categories.Count)//на всякий, защита от ошибки выхода за диапазон
                    {
                        //Проверить родительскую категорию
                        int ParentIDindex = int.Parse(categories[categiryindex].ParentID) - 1;//В List индекс идет от нуля
                        if (ParentIDindex > 0 && ParentIDindex < categories.Count)
                        {
                            targetdir = Path.Combine(targetdir, categories[ParentIDindex].Name);
                        }

                        targetdir = Path.Combine(targetdir, categories[categiryindex].Name);

                        _ = Directory.CreateDirectory(targetdir);
                    }
                }
            }

            return targetdir;
        }

        private void SetTooltips()
        {
            //http://qaru.site/questions/47162/c-how-do-i-add-a-tooltip-to-a-control
            //THMainResetTableButton
            ToolTip THToolTip = new ToolTip
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

            //Main
            //THToolTip.SetToolTip(button1, T._("Unpacking mods and resources from 'Downloads' and 'RES' folders for game when they are not installed"));
            THToolTip.SetToolTip(InstallInModsButton, T._("Automatically get required mod data, converts and moves files from 2MO folder")
                + (MOmode ? T._(
                        " to MO format in Mods when possible"
                    ) : T._(
                        " to the game folder when possible"
                        )));
            THToolTip.SetToolTip(Install2MODirPathOpenFolderLinkLabel, T._("Open folder where you can drop/download files for autoinstallation"));
            THToolTip.SetToolTip(AutoShortcutRegistryCheckBox, T._("When checked will create shortcut for the AI Helper on Desktop and will fix registry if need"));
            THToolTip.SetToolTip(groupBox1, T._("Game Display settings"));
            THToolTip.SetToolTip(ResolutionComboBox, T._("Select preferred screen resolution"));
            THToolTip.SetToolTip(FullScreenCheckBox, T._("When checked game will be in fullscreen mode"));
            THToolTip.SetToolTip(QualityComboBox, T._("Select preferred graphics quality"));
            THToolTip.SetToolTip(CreateShortcutButton, T._("Will create shortcut in Desktop if not exist"));
            THToolTip.SetToolTip(FixRegistryButton, T._("Will set Data dir with game files as install dir in registry"));
            THToolTip.SetToolTip(GameButton, MOmode ? T._("Will execute the Game")
                + T._(" from Mod Organizer with attached mods")
                : T._("Will execute the Game")
                );
            THToolTip.SetToolTip(StudioButton, MOmode ? T._("Will execute Studio")
                + T._(" from Mod Organizer with attached mods")
                : T._("Will execute Studio")
                );
            THToolTip.SetToolTip(MOButton, T._("Will execute Mod Organizer mod manager where you can manage your mods"));
            THToolTip.SetToolTip(SettingsButton, MOmode ?
                  T._("Will execute original game launcher")
                + T._(" from Mod Organizer with attached mods")
                : T._("Will execute original game launcher")
                );
            THToolTip.SetToolTip(MOCommonModeSwitchButton, MOmode ? T._(
                    "Will convert game from MO Mode to Common mode\n" +
                    " when you can run exes from Data folder without Mod Organizer.\n You can convert game back to MO mode\n" +
                    " when it will be need to install new mods or test your mod config"
                ) : T._(
                    "Will convert the game to MO mode\n" +
                    " when all mod files will be moved back to Mods folder\n" +
                    " in their folders and vanilla files restored"
                ));
            THToolTip.SetToolTip(LanchModeInfoLinkLabel, T._("Same as button in Tool tab.\n")
                + (MOmode ? T._(
                    "Will convert game from MO Mode to Common mode\n" +
                    " when you can run exes from Data folder without Mod Organizer.\n" +
                    " You can convert game back to MO mode\n" +
                    " when it will be need to install new mods or test your mod config"
                ) : T._(
                    "Will convert the game to MO mode\n when all mod files will be moved back to Mods folder\n" +
                    " in their folders and vanilla files restored"
                )));

            //Open Folders
            THToolTip.SetToolTip(OpenGameFolderLinkLabel, T._("Opens Data folder of selected game"));
            THToolTip.SetToolTip(OpenModsFolderLinkLabel, T._("Opens Mods folder of selected game"));
            THToolTip.SetToolTip(OpenMOFolderLinkLabel, T._("Opens Mod Organizer folder"));
            THToolTip.SetToolTip(OpenMOOverwriteFolderLinkLabel, T._("Opens Overwrite folder of Mod Organizer with possible new generated files for selected game\n\nFiles here have highest priority and will be loaded over any enabled mod files"));
            THToolTip.SetToolTip(OpenMyUserDataFolderLinkLabel, T._("Opens MyUserData folder in Mods if exist\n\nHere placed usual User files of Organized ModPack for selected game"));

            THToolTip.SetToolTip(Open2MOLinkLabel,
                T._("Opens 2MO folder fo selected game" +
                "\n\nHere can be placed mod files which you want to install for selected game in approriate subfolders in mods" +
                "\nand then can be installed all by one click on") + " " + InstallInModsButton.Text + " " + T._("button") +
                "\n" + T._("which can be found in") + " " + ToolsTabPage.Text + " " + T._("tab page") +
                "\n\n" + T._("Helper recognize") + ":"
                + "\n " + T._(".dll files of BepinEx plugins in \"2MO\" folder")
                + "\n " + T._("Sideloader mod archives in \"2MO\" folder")
                + "\n " + T._("Female character cards in \"2MO\" folder")
                + "\n " + T._("Female character cards in \"f\" subfolder")
                + "\n " + T._("Male character cards in \"m\" subfolder")
                + "\n " + T._("Coordinate clothes set cards in \"c\" subfolder")
                + "\n " + T._("Studio scene cards in \"s\" subfolder")
                + "\n " + T._("Cardframe Front cards in \"cf\" subfolder")
                + "\n " + T._("Cardframe Back cards in \"cf\" subfolder")
                + "\n " + T._("Script loader scripts in \"2MO\" folder")
                + "\n " + T._("Housing plan cards in \"h\\01\", \"h\\02\", \"h\\03\" subfolders")
                + "\n " + T._("Overlays cards in \"o\" subfolder")
                + "\n " + T._("folders with overlays cards in \"o\" subfolder")
                + "\n " + T._("Subfolders with modfiles in \"2MO\" folder")
                + "\n " + T._("Zip archives with mod files in \"2MO\" folder")
                + "\n " + T._("Zip archives with mod files in \"2MO\" folder")
                + "\n\n" + T._("Any Rar and 7z archives in \"2MO\" folder will be extracted" +
                "\nSome recognized mods can be updated instead of be installed as new mod" +
                "\nMost of mods will be automatically activated except .cs scripts" +
                "\nwhich always optional and often it is cheats or can slowdown/break game")

                );
            ////////////////////////////
        }

        private void SetScreenSettings()
        {
            //set Settings
            if (File.Exists(SetupXmlPath))
            {
            }
            else
            {
                string screenWidth = Screen.PrimaryScreen.Bounds.Width.ToString();
                //string screenHeight = Screen.PrimaryScreen.Bounds.Height.ToString();
                int[] width = { 1280, 1366, 1536, 1600, 1920, 2048, 2560, 3200, 3840 };
                if (int.Parse(screenWidth) > width[width.Length - 1])
                {
                    ResolutionComboBox.SelectedItem = width.Length - 1;
                    SetScreenResolution(ResolutionComboBox.Items[width.Length - 1].ToString());
                }
                else
                {
                    for (int w = 0; w < width.Length; w++)
                    {
                        if (int.Parse(screenWidth) <= width[w])
                        {
                            string SelectedRes = ResolutionComboBox.Items[w].ToString();
                            ResolutionComboBox.Text = SelectedRes;
                            SetScreenResolution(SelectedRes);
                            break;
                        }
                    }
                }
            }

            ResolutionComboBox.Text = ManageXML.ReadXmlValue(SetupXmlPath, "Setting/Size", ResolutionComboBox.Text);
            FullScreenCheckBox.Checked = bool.Parse(ManageXML.ReadXmlValue(SetupXmlPath, "Setting/FullScreen", FullScreenCheckBox.Checked.ToString().ToLower()));

            QualityComboBox.SelectedIndex = int.Parse(ManageXML.ReadXmlValue(SetupXmlPath, "Setting/Quality", "2"));
        }

        private void SetScreenResolution(string Resolution)
        {
            if (MOmode)
            {
                if (Directory.Exists(OverwriteFolder))
                {
                }
                else
                {
                    Directory.CreateDirectory(OverwriteFolder);
                }
                if (!Directory.Exists(OverwriteFolderLink))
                {
                    CreateSymlink.Folder(OverwriteFolder, OverwriteFolderLink);
                }
            }

            ManageXML.ChangeXmlValue(SetupXmlPath, "Setting/Size", Resolution);
            ManageXML.ChangeXmlValue(SetupXmlPath, "Setting/Width", Resolution.Replace("(16 : 9)", string.Empty).Trim().Split('x')[0].Trim());
            ManageXML.ChangeXmlValue(SetupXmlPath, "Setting/Height", Resolution.ToString().Replace("(16 : 9)", string.Empty).Trim().Split('x')[1].Trim());
        }

        private void SetGraphicsQuality(string quality)
        {
            if (Directory.Exists(OverwriteFolder))
            {
            }
            else
            {
                Directory.CreateDirectory(OverwriteFolder);
            }
            if (!Directory.Exists(OverwriteFolderLink))
            {
                CreateSymlink.Folder(OverwriteFolder, OverwriteFolderLink);
            }

            ManageXML.ChangeXmlValue(SetupXmlPath, "Setting/Quality", quality);
        }

        private void FoldersInit()
        {
            if (File.Exists(ManageSettings.GetMOToStandartConvertationOperationsListFilePath()))
            {
                MOmode = false;
                button1.Text = T._("Common mode");
            }
            else
            {
                MOmode = true;
                button1.Text = T._("MO mode");
            }

            if (!Directory.Exists(DataPath))
            {
                Directory.CreateDirectory(DataPath);
            }
            if (MOmode && !Directory.Exists(ModsPath))
            {
                Directory.CreateDirectory(ModsPath);
                ModsInfoLabel.Text = T._("Mods dir created");
            }

            string AIGirl = ManageSettings.GetCurrentGameEXEName();
            string AIGirlTrial = ManageSettings.GetCurrentGameEXEName();
            if (File.Exists(Path.Combine(ManageSettings.GetDataPath(), AIGirlTrial + ".exe")))
            {
                DataInfoLabel.Text = string.Format(T._("{0} game installed in {1}"), AIGirlTrial, "Data");
            }
            else if (File.Exists(Path.Combine(DataPath, AIGirl + ".exe")))
            {
                DataInfoLabel.Text = string.Format(T._("{0} game installed in {1}"), AIGirl, "Data");
            }
            else if (File.Exists(Path.Combine(AppResDir, AIGirlTrial + ".7z")))
            {
                DataInfoLabel.Text = string.Format(T._("{0} archive in {1}"), AIGirlTrial, "Data");
            }
            else if (File.Exists(Path.Combine(AppResDir, AIGirl + ".7z")))
            {
                DataInfoLabel.Text = string.Format(T._("{0} archive in {1}"), "AIGirl", "Data");
            }
            else if (Directory.Exists(DataPath))
            {
                DataInfoLabel.Text = string.Format(T._("{0} files not in {1}. Move {0} game files there."), AIGirl, "Data");
            }
            else
            {
                Directory.CreateDirectory(DataPath);
                DataInfoLabel.Text = string.Format(T._("{0} dir created. Move {1} game files there."), "Data", AIGirl);
            }

            if (MOmode)
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

                if (!Manage.ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Manage.ManageSettings.GetModsPath(), "*", new string[1] { "_separator" }))
                {
                    ModsInfoLabel.Text = T._("Found mod folders in Mods");
                    //button1.Enabled = false;
                    mode = 1;
                    button1.Text = T._("Mods Ready");
                    //MO2StandartButton.Enabled = true;
                    GetEnableDisableLaunchButtons();
                    AIGirlHelperTabControl.SelectedTab = LaunchTabPage;

                    //if (File.Exists(Path.Combine(DataPath, ManageSettings.GetCurrentGameEXEName() + ".exe")))
                    //{
                    //}
                    //GetEnableDisableLaunchButtons();
                    MOCommonModeSwitchButton.Text = T._("MOToCommon");
                }

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

                LanchModeInfoLinkLabel.Text = T._("MO mode");

                //создание ссылок на файлы bepinex
                ManageMOMods.BepinExLoadingFix();

                //создание exe-болванки
                ManageMO.MakeDummyFiles();

                ManageMO.SetModOrganizerINISettingsForTheGame();
                ManageMOMods.SetMOModsVariables();
            }
            else
            {
                ModsInfoLabel.Visible = false;

                StudioButton.Enabled = false;
                //MO2StandartButton.Enabled = false;
                MOCommonModeSwitchButton.Text = T._("CommonToMO");
                button1.Text = T._("Common mode");
                LanchModeInfoLinkLabel.Text = T._("Common mode");
                button1.Enabled = false;
                //AIGirlHelperTabControl.SelectedTab = LaunchTabPage;
            }

            //Обновление пути к setup.xml с настройками графики
            VariablesINIT();

            AIGirlHelperTabControl.SelectedTab = LaunchTabPage;
            CurrentGameComboBox.Text = CurrentGame.GetGameFolderName();
            CurrentGameComboBox.SelectedIndex = ManageSettings.GetCurrentGameIndex();

            SetScreenSettings();

            SetTooltips();

            if (AutoShortcutRegistryCheckBox.Checked)
            {
                ManageOther.AutoShortcutAndRegystry();
            }

            SelectedGameLabel1.Text = ManageSettings.GetCurrentGameFolderName();
        }

        private void GetEnableDisableLaunchButtons()
        {
            MOButton.Enabled = File.Exists(ManageSettings.GetMOexePath());
            SettingsButton.Enabled = File.Exists(Path.Combine(DataPath, ManageSettings.GetINISettingsEXEName() + ".exe"));
            GameButton.Enabled = File.Exists(Path.Combine(DataPath, ManageSettings.GetCurrentGameEXEName() + ".exe"));
            StudioButton.Enabled = File.Exists(Path.Combine(DataPath, ManageSettings.GetStudioEXEName() + ".exe"));
            try
            {
                BepInExConsoleCheckBox.Checked = bool.Parse(ManageINI.GetINIValueIfExist(ManageSettings.GetBepInExCfgFilePath(), "Enabled", "Logging.Console", "False"));
            }
            catch
            {
                BepInExConsoleCheckBox.Checked = false;
            }
            BepInExConsoleCheckBox.Enabled = ManageSettings.GetBepInExCfgFilePath().Length > 0;
            if (BepInExConsoleCheckBox.Checked)
            {
                BepInExDisplayedLogLevelLabel.Visible = true;
                ManageSettings.SwitchBepInExDisplayedLogLevelValue(BepInExConsoleCheckBox, BepInExDisplayedLogLevelLabel, true);
            }
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            ManageOther.CheckBoxChangeColor(sender as CheckBox);
            Properties.Settings.Default.AutoShortcutRegistryCheckBoxChecked = AutoShortcutRegistryCheckBox.Checked;
            new IniFile(ManageSettings.GetAIHelperINIPath()).WriteINI("Settings", "autoCreateShortcutAndFixRegystry", Properties.Settings.Default.AutoShortcutRegistryCheckBoxChecked.ToString());
        }

        private void ResolutionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetScreenResolution((sender as ComboBox).SelectedItem.ToString());
        }

        private void FullScreenCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ManageOther.CheckBoxChangeColor(sender as CheckBox);
            ManageXML.ChangeXmlValue(SetupXmlPath, "Setting/FullScreen", (sender as CheckBox).Checked.ToString().ToLower());
        }

        private void FixRegistryButton_Click(object sender, EventArgs e)
        {
            FixRegistryButton.Enabled = false;

            ManageRegistry.FixRegistry(false);

            FixRegistryButton.Enabled = true;
        }

        private void MOButton_Click(object sender, EventArgs e)
        {
            OnOffButtons(false);
            if (MOmode)
            {
                RunProgram(MOexePath, string.Empty);
            }
            else
            {
                MOButton.Enabled = false;
                MessageBox.Show(T._("Game in Common mode now.\n To execute Mod Organizer convert game back\n to MO mode by button in Tools tab"));
            }
            OnOffButtons();
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            OnOffButtons(false);
            if (MOmode)
            {
                RunProgram(MOexePath, "moshortcut://:" + ManageSettings.GetINISettingsEXEName());
            }
            else
            {
                RunProgram(Path.Combine(DataPath, ManageSettings.GetINISettingsEXEName() + ".exe"), string.Empty);
            }
            OnOffButtons();
        }

        private void GameButton_Click(object sender, EventArgs e)
        {
            OnOffButtons(false);
            if (MOmode)
            {
                RunProgram(MOexePath, "moshortcut://:" + ManageSettings.GetCurrentGameEXEName());
            }
            else
            {
                RunProgram(Path.Combine(DataPath, ManageSettings.GetCurrentGameEXEName() + ".exe"), string.Empty);
            }
            OnOffButtons();
        }

        private void OnOffButtons(bool SwitchOn = true)
        {
            AIGirlHelperTabControl.Enabled = SwitchOn;
        }

        private void StudioButton_Click(object sender, EventArgs e)
        {
            OnOffButtons(false);
            if (MOmode)
            {
                RunProgram(MOexePath, "moshortcut://:" + ManageSettings.GetStudioEXEName());
            }
            else
            {
                RunProgram(Path.Combine(DataPath, ManageSettings.GetStudioEXEName() + ".exe"), string.Empty);
            }
            OnOffButtons();
        }

        private void RunProgram(string ProgramPath, string Arguments)
        {
            if (File.Exists(ProgramPath))
            {
                using (Process Program = new Process())
                {
                    //MessageBox.Show("outdir=" + outdir);
                    Program.StartInfo.FileName = ProgramPath;
                    if (Arguments.Length > 0)
                    {
                        Program.StartInfo.Arguments = Arguments;
                    }
                    Program.StartInfo.WorkingDirectory = Path.GetDirectoryName(ProgramPath);

                    //http://www.cyberforum.ru/windows-forms/thread31052.html
                    // свернуть
                    WindowState = FormWindowState.Minimized;
                    if (LinksForm == null || LinksForm.IsDisposed)
                    {
                    }
                    else
                    {
                        LinksForm.WindowState = FormWindowState.Minimized;
                    }

                    _ = Program.Start();
                    Program.WaitForExit();

                    // Показать
                    WindowState = FormWindowState.Normal;
                    if (LinksForm == null || LinksForm.IsDisposed)
                    {
                    }
                    else
                    {
                        LinksForm.WindowState = FormWindowState.Normal;
                    }
                }
            }
        }

        private readonly Dictionary<string, string> qualitylevels = new Dictionary<string, string>(3);

        private void QualityComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetGraphicsQuality((sender as ComboBox).SelectedIndex.ToString());
        }

        private LinksForm LinksForm;

        private void NewformButton_Click(object sender, EventArgs e)
        {
            if (LinksForm == null || LinksForm.IsDisposed)
            {
                //show and reposition of form
                //https://stackoverflow.com/questions/31492787/how-to-set-position-second-form-depend-on-first-form
                LinksForm = new LinksForm
                {
                    //LinksForm.Text = T._("Links");
                    StartPosition = FormStartPosition.Manual
                };
                LinksForm.Load += delegate (object s2, EventArgs e2)
                {
                    LinksForm.Location = new Point(Bounds.Location.X + (Bounds.Width / 2) - (LinksForm.Width / 2),
                        Bounds.Location.Y + /*(Bounds.Height / 2) - (f2.Height / 2) +*/ Bounds.Height);
                };
                LinksForm.Text = T._("Links");
                newformButton.Text = @"/\";
                if (extraSettingsForm != null && !extraSettingsForm.IsDisposed)
                {
                    extraSettingsForm.Close();
                }
                LinksForm.Show();
            }
            else
            {
                newformButton.Text = @"\/";
                LinksForm.Close();
            }
        }

        private void AIHelper_LocationChanged(object sender, EventArgs e)
        {
            //move second form with main form
            //https://stackoverflow.com/questions/3429445/how-to-move-two-windows-forms-together
            if (LinksForm == null || LinksForm.IsDisposed)
            {
            }
            else
            {
                if (LinksForm.WindowState == FormWindowState.Minimized)
                {
                    LinksForm.WindowState = FormWindowState.Normal;
                }
                LinksForm.Location = new Point(Bounds.Location.X + (Bounds.Width / 2) - (LinksForm.Width / 2),
                    Bounds.Location.Y + /*(Bounds.Height / 2) - (f2.Height / 2) +*/ Bounds.Height);
            }
            if (extraSettingsForm == null || extraSettingsForm.IsDisposed)
            {
            }
            else
            {
                if (extraSettingsForm.WindowState == FormWindowState.Minimized)
                {
                    extraSettingsForm.WindowState = FormWindowState.Normal;
                }
                extraSettingsForm.Location = new Point(Bounds.Location.X + (Bounds.Width / 2) - (extraSettingsForm.Width / 2),
                    Bounds.Location.Y + /*(Bounds.Height / 2) - (f2.Height / 2) +*/ Bounds.Height);
            }
        }

        private async void InstallInModsButton_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(Install2MODirPath) && (Directory.GetFiles(Install2MODirPath, "*.rar").Length > 0 || Directory.GetFiles(Install2MODirPath, "*.7z").Length > 0 || Directory.GetFiles(Install2MODirPath, "*.png").Length > 0 || Directory.GetFiles(Install2MODirPath, "*.cs").Length > 0 || Directory.GetFiles(Install2MODirPath, "*.dll").Length > 0 || Directory.GetFiles(Install2MODirPath, "*.zipmod").Length > 0 || Directory.GetFiles(Install2MODirPath, "*.zip").Length > 0 || Directory.GetDirectories(Install2MODirPath, "*").Length > 0))
            {
                OnOffButtons(false);

                await Task.Run((Action)(() => InstallModFilesAndCleanEmptyFolder()));

                InstallInModsButton.Text = T._("Install from 2MO");

                OnOffButtons();

                //обновление информации о конфигурации папок игры
                FoldersInit();

                MessageBox.Show(T._("All possible mods installed. Install all rest in 2MO folder manually."));
            }
            else
            {
                MessageBox.Show(T._("No compatible for installation formats found in 2MO folder.\nFormats: zip, zipmod, png, png in subfolder, unpacked mod in subfolder"));
            }
            Process.Start("explorer.exe", Install2MODirPath);
        }

        private void InstallModFilesAndCleanEmptyFolder()
        {
            string InstallMessage = T._("Installing");
            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage));
            ManageArchive.UnpackArchivesToSubfoldersWithSameName(Install2MODirPath, ".rar");
            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage + "."));
            ManageArchive.UnpackArchivesToSubfoldersWithSameName(Install2MODirPath, ".7z");
            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage + ".."));
            ManageMOMods.InstallCsScriptsForScriptLoader();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage + "..."));
            ManageMOMods.InstallZipArchivesToMods();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage));
            ManageMOMods.InstallBepinExModsToMods();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage + "."));
            ManageMOMods.InstallZipModsToMods();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage + ".."));
            ManageMOMods.InstallCardsFrom2MO();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage + "..."));
            ManageMOMods.InstallModFilesFromSubfolders();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage));
            ManageMOMods.InstallImagesFromSubfolders();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage + "."));
            ManageFilesFolders.DeleteEmptySubfolders(Install2MODirPath, false);

            if (!Directory.Exists(Install2MODirPath))
            {
                Directory.CreateDirectory(Install2MODirPath);
            }

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = T._("Install from 2MO")));
        }

        private void CreateShortcutButton_Click(object sender, EventArgs e)
        {
            ManageOther.CreateShortcuts(true, false);
        }

        private void MO2StandartButton_Click(object sender, EventArgs e)
        {
            OnOffButtons(false);
            if (MOmode)
            {
                DialogResult result = MessageBox.Show(T._("Attention") + "\n\n" + T._("Conversation to") + " " + T._("Common mode") + "\n\n" + T._("This will move all mod files from Mods folder to Data folder to make it like common installation variant.\nYou can restore it later back to MO mode.\n\nContinue?"), T._("Confirmation"), MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    MOmode = false;
                    MOCommonModeSwitchButton.Enabled = false;
                    LanchModeInfoLinkLabel.Enabled = false;

                    string[] EnabledModsList = ManageMO.GetModNamesListFromActiveMOProfile();
                    int EnabledModsLength = EnabledModsList.Length;

                    if (EnabledModsList.Length == 0)
                    {
                        MOmode = true;
                        MOCommonModeSwitchButton.Enabled = true;
                        MessageBox.Show(T._("There is no enabled mods in Mod Organizer"));
                        return;
                    }

                    ManageMOMods.CleanBepInExLinksFromData();

                    if (File.Exists(ManageSettings.GetDummyFilePath()))
                    {
                        File.Delete(ManageSettings.GetDummyFilePath());
                    }

                    if (!Directory.Exists(ManageSettings.GetMOmodeDataFilesBakDirPath()))
                    {
                        Directory.CreateDirectory(ManageSettings.GetMOmodeDataFilesBakDirPath());
                    }
                    StringBuilder Operations = new StringBuilder();

                    StringBuilder EmptyFoldersPaths = ManageFilesFolders.GetEmptySubfoldersPaths(ManageSettings.GetDataPath(), new StringBuilder());

                    //получение всех файлов из Data
                    string[] DataFolderFilesPaths = Directory.GetFiles(DataPath, "*.*", SearchOption.AllDirectories);

                    //получение всех файлов из папки Overwrite и их обработка
                    string[] FilesInOverwrite = Directory.GetFiles(OverwriteFolder, "*.*", SearchOption.AllDirectories);
                    if (FilesInOverwrite.Length > 0)
                    {
                        string FileInDataFolder;
                        int FilesInOverwriteLength = FilesInOverwrite.Length;
                        for (int N = 0; N < FilesInOverwriteLength; N++)
                        {
                            FileInDataFolder = FilesInOverwrite[N].Replace(OverwriteFolder, DataPath);
                            if (File.Exists(FileInDataFolder))
                            {
                                string FileInBakFolderWhichIsInRES = FileInDataFolder.Replace(DataPath, ManageSettings.GetMOmodeDataFilesBakDirPath());
                                if (!File.Exists(FileInBakFolderWhichIsInRES) && DataFolderFilesPaths.Contains(FileInDataFolder))
                                {
                                    string bakfolder = Path.GetDirectoryName(FileInBakFolderWhichIsInRES);

                                    if (!Directory.Exists(bakfolder))
                                    {
                                        Directory.CreateDirectory(bakfolder);
                                    }

                                    try
                                    {
                                        File.Move(FileInDataFolder, FileInBakFolderWhichIsInRES);//перенос файла из Data в Bak, если там не было
                                        File.Move(FilesInOverwrite[N], FileInDataFolder);//перенос файла из папки Overwrite в Data
                                        Operations.AppendLine(FilesInOverwrite[N] + "|MovedTo|" + FileInDataFolder);//запись об операции будет пропущена, если будет какая-то ошибка
                                    }
                                    catch
                                    {
                                        //когда файла в дата нет, файл в бак есть и есть файл в папке Overwrite - вернуть файл из bak назад
                                        if (!File.Exists(FileInDataFolder))
                                        {
                                            if (File.Exists(FileInBakFolderWhichIsInRES))
                                            {
                                                if (File.Exists(FilesInOverwrite[N]))
                                                {
                                                    File.Move(FileInBakFolderWhichIsInRES, FileInDataFolder);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    for (int N = 0; N < EnabledModsLength; N++)
                    {
                        string ModFolder = Path.Combine(ModsPath, EnabledModsList[N]);
                        if (ModFolder.Length > 0 && Directory.Exists(ModFolder))
                        {
                            string[] ModFiles = Directory.GetFiles(ModFolder, "*.*", SearchOption.AllDirectories);
                            if (ModFiles.Length > 0)
                            {
                                int ModFilesLength = ModFiles.Length;
                                string FileInDataFolder;

                                bool metaskipped = false;
                                for (int f = 0; f < ModFilesLength; f++)
                                {
                                    //skip meta.ini
                                    if (metaskipped)
                                    {
                                    }
                                    else if (ModFiles[f].Length >= 8 && string.Compare(ModFiles[f].Substring(ModFiles[f].Length - 8, 8), "meta.ini", true) == 0)
                                    {
                                        metaskipped = true;//для ускорения проверки, когда meta будет найден, будет делать быструю проверку bool переменной
                                        continue;
                                    }

                                    MOCommonModeSwitchButton.Text = "..." + EnabledModsLength + "/" + N + ": " + f + "/" + ModFilesLength;
                                    FileInDataFolder = ModFiles[f].Replace(ModFolder, DataPath);

                                    if (File.Exists(FileInDataFolder))
                                    {
                                        string FileInBakFolderWhichIsInRES = FileInDataFolder.Replace(DataPath, ManageSettings.GetMOmodeDataFilesBakDirPath());
                                        if (!File.Exists(FileInBakFolderWhichIsInRES) && DataFolderFilesPaths.Contains(FileInDataFolder))
                                        {
                                            string bakfolder = Path.GetDirectoryName(FileInBakFolderWhichIsInRES);

                                            if (!Directory.Exists(bakfolder))
                                            {
                                                Directory.CreateDirectory(bakfolder);
                                            }

                                            try
                                            {
                                                File.Move(FileInDataFolder, FileInBakFolderWhichIsInRES);//перенос файла из Data в Bak, если там не было
                                                File.Move(ModFiles[f], FileInDataFolder);//перенос файла из папки мода в Data
                                                Operations.AppendLine(ModFiles[f] + "|MovedTo|" + FileInDataFolder);//запись об операции будет пропущена, если будет какая-то ошибка
                                            }
                                            catch
                                            {
                                                //когда файла в дата нет, файл в бак есть и есть файл в папке мода - вернуть файл из bak назад
                                                if (!File.Exists(FileInDataFolder))
                                                {
                                                    if (File.Exists(FileInBakFolderWhichIsInRES))
                                                    {
                                                        if (File.Exists(ModFiles[f]))
                                                        {
                                                            File.Move(FileInBakFolderWhichIsInRES, FileInDataFolder);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        string destFolder = Path.GetDirectoryName(FileInDataFolder);
                                        if (!Directory.Exists(destFolder))
                                        {
                                            Directory.CreateDirectory(destFolder);
                                        }
                                        try
                                        {
                                            File.Move(ModFiles[f], FileInDataFolder);//перенос файла из папки мода в Data
                                            Operations.AppendLine(ModFiles[f] + "|MovedTo|" + FileInDataFolder);//запись об операции будет пропущена, если будет какая-то ошибка
                                        }
                                        catch
                                        {
                                        }
                                    }

                                    //MoveWithReplace(ModFiles[f], DestFilePath[f]);
                                }
                                //Directory.Delete(ModFolder, true);
                            }
                        }
                    }

                    File.WriteAllText(ManageSettings.GetMOToStandartConvertationOperationsListFilePath(), Operations.ToString());
                    string[] DataWithModsFileslist = Directory.GetFiles(DataPath, "*.*", SearchOption.AllDirectories);
                    File.WriteAllLines(ManageSettings.GetModdedDataFilesListFilePath(), DataWithModsFileslist);
                    File.WriteAllLines(ManageSettings.GetVanillaDataFilesListFilePath(), DataWithModsFileslist);

                    //записать пути до пустых папок, чтобы при восстановлении восстановить и их
                    if (EmptyFoldersPaths.ToString().Length > 0)
                    {
                        File.WriteAllText(ManageSettings.GetVanillaDataEmptyFoldersListFilePath(), EmptyFoldersPaths.ToString());
                    }

                    //Directory.Delete(ModsPath, true);
                    //Directory.Move(MODirPath, Path.Combine(AppResDir, Path.GetFileName(MODirPath)));
                    MOCommonModeSwitchButton.Text = T._("CommonToMO");
                    MOCommonModeSwitchButton.Enabled = true;
                    LanchModeInfoLinkLabel.Enabled = true;
                    //File.Move(Path.Combine(MODirPath, "ModOrganizer.exe"), Path.Combine(MODirPath, "ModOrganizer.exe.GameInCommonModeNow"));
                    //обновление информации о конфигурации папок игры
                    FoldersInit();
                    MessageBox.Show(T._("All mod files now in Data folder! You can restore MO mode by same button."));
                }
            }
            else
            {
                DialogResult result = MessageBox.Show(T._("Attention") + "\n\n" + T._("Conversation to") + " " + T._("MO mode") + "\n\n" + T._("This will move all mod files back to Mods folder from Data and will switch to MO mode.\n\nContinue?"), T._("Confirmation"), MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    MOmode = true;
                    MOCommonModeSwitchButton.Enabled = false;
                    LanchModeInfoLinkLabel.Enabled = false;

                    string[] Operations = File.ReadAllLines(ManageSettings.GetMOToStandartConvertationOperationsListFilePath());
                    string[] VanillaDataFiles = File.ReadAllLines(ManageSettings.GetVanillaDataFilesListFilePath());
                    string[] ModdedDataFiles = File.ReadAllLines(ManageSettings.GetModdedDataFilesListFilePath());

                    StringBuilder FilesWhichAlreadyHaveSameDestFileInMods = new StringBuilder();
                    bool FilesWhichAlreadyHaveSameDestFileInModsIsNotEmpty = false;

                    //Перемещение файлов модов по списку
                    int OperationsLength = Operations.Length;
                    for (int o = 0; o < OperationsLength; o++)
                    {
                        string[] MovePaths = Operations[o].Split(new string[] { "|MovedTo|" }, StringSplitOptions.None);

                        bool FilePathInModsExists = File.Exists(MovePaths[0]);
                        bool FilePathInDataExists = File.Exists(MovePaths[1]);

                        if (FilePathInDataExists)
                        {
                            if (!FilePathInModsExists)
                            {
                                string modsubfolder = Path.GetDirectoryName(MovePaths[0]);
                                if (!Directory.Exists(modsubfolder))
                                {
                                    Directory.CreateDirectory(modsubfolder);
                                }

                                File.Move(MovePaths[1], MovePaths[0]);
                            }
                            else
                            {
                                //если в Mods на месте планируемого для перемещения назад в Mods файла появился новый файл, то записать информацию о нем в новый мод, чтобы перенести его в новый мод
                                FilesWhichAlreadyHaveSameDestFileInMods.AppendLine(MovePaths[1] + "|MovedTo|" + MovePaths[0]);
                                FilesWhichAlreadyHaveSameDestFileInModsIsNotEmpty = true;
                            }
                        }
                    }

                    //string destFolderForNewFiles = Path.Combine(ModsPath, "NewAddedFiles");

                    //получение даты и времени для дальнейшего использования
                    string DateTimeInFormat = DateTime.Now.ToString("yyyyMMddHHmmss");

                    if (FilesWhichAlreadyHaveSameDestFileInModsIsNotEmpty)
                    {
                        foreach (string FromToPathsLine in FilesWhichAlreadyHaveSameDestFileInMods.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            string[] FromToPaths = FromToPathsLine.Split(new string[] { "|MovedTo|" }, StringSplitOptions.None);

                            string TargetFolderPath = Path.GetDirectoryName(FromToPaths[1]);

                            bool IsForOverwriteFolder = ManageStrings.IsStringAContainsStringB(TargetFolderPath, OverwriteFolder);
                            //поиск имени мода с учетом обработки файлов папки Overwrite
                            string ModName = TargetFolderPath;
                            if (IsForOverwriteFolder)
                            {
                                ModName = Path.GetFileName(OverwriteFolder);
                            }
                            else
                            {
                                while (Path.GetDirectoryName(ModName) != ModsPath)
                                {
                                    ModName = Path.GetDirectoryName(ModName);
                                }
                                ModName = Path.GetFileName(ModName);
                            }

                            //Новое имя для новой целевой папки мода
                            string OriginalModPath = IsForOverwriteFolder ? OverwriteFolder : Path.Combine(ModsPath, ModName);
                            string NewModName = ModName + "_" + DateTimeInFormat;
                            string NewModPath = Path.Combine(ModsPath, NewModName);
                            TargetFolderPath = TargetFolderPath.Replace(OriginalModPath, NewModPath);

                            string TargetFileName = Path.GetFileNameWithoutExtension(FromToPaths[1]);
                            string TargetFileExtension = Path.GetExtension(FromToPaths[1]);
                            string TargetPath = Path.Combine(TargetFolderPath, TargetFileName + TargetFileExtension);

                            //создать подпапку для файла
                            if (!Directory.Exists(TargetFolderPath))
                            {
                                Directory.CreateDirectory(TargetFolderPath);
                            }

                            //переместить файл в новую для него папку
                            File.Move(FromToPaths[0], TargetPath);

                            //записать в папку мода замечание с объяснением наличия этого мода
                            string note = T._(
                                "Files in same paths already exist in original mod folder!\n\n" +
                                " This folder was created in time of conversion from Common mode to MO mode\n" +
                                " and because in destination place\n" +
                                " where mod file must be moved already was other file with same name.\n" +
                                " It could happen if content of the mod folder was updated\n" +
                                " when game was in common mode and was made same file in same place.\n" +
                                " Please check files here and if this files need for you\n" +
                                " then activate this mod or move files to mod folder with same name\n" +
                                " and if this files obsolete or just not need anymore then delete this mod folder."
                                );
                            File.WriteAllText(Path.Combine(NewModPath, "NOTE!.txt"), note);

                            //запись meta.ini с замечанием
                            ManageMO.WriteMetaINI(
                                NewModPath
                                ,
                                string.Empty
                                ,
                                "0." + DateTimeInFormat
                                ,
                                string.Empty
                                ,
                                note.Replace("\n", "<br>")
                                );
                            ManageMO.ActivateInsertModIfPossible(NewModName, false, ModName, false);
                        }
                    }

                    //Перемещение новых файлов
                    //
                    //добавление всех файлов из дата, которых нет в списке файлов модов и игры в дата, что был создан сразу после перехода в обычный режим
                    string[] addedFiles = Directory.GetFiles(DataPath, "*.*", SearchOption.AllDirectories).Where(line => !ModdedDataFiles.Contains(line)).ToArray();
                    //задание имени целевой папки для новых модов
                    string addedFilesFolderName = "[added]UseFiles_" + DateTimeInFormat;
                    string DestFolderPath = Path.Combine(ModsPath, addedFilesFolderName);

                    int addedFilesLength = addedFiles.Length;
                    for (int f = 0; f < addedFilesLength; f++)
                    {
                        string DestFileName = addedFiles[f].Replace(DataPath, DestFolderPath);
                        string DestFileFolder = Path.GetDirectoryName(DestFileName);
                        if (!Directory.Exists(DestFileFolder))
                        {
                            Directory.CreateDirectory(DestFileFolder);
                        }
                        File.Move(addedFiles[f], DestFileName);
                    }

                    //подключить новый мод, если он существует
                    if (Directory.Exists(DestFolderPath))
                    {
                        //запись meta.ini
                        ManageMO.WriteMetaINI(
                            DestFolderPath
                            ,
                            "53,"
                            ,
                            DateTimeInFormat
                            ,
                            T._("Sort files if need")
                            ,
                            T._("<br>This files was added in Common mode<br>and moved as mod after convertation in MO mode.<br>Date: ") + DateTimeInFormat
                            );

                        ManageMO.ActivateInsertModIfPossible(addedFilesFolderName);
                    }

                    //перемещение ванильных файлов назад в дата
                    if (Directory.Exists(ManageSettings.GetMOmodeDataFilesBakDirPath()))
                    {
                        string[] FilesInMOmodeDataFilesBak = Directory.GetFiles(ManageSettings.GetMOmodeDataFilesBakDirPath(), "*.*", SearchOption.AllDirectories);
                        int FilesInMOmodeDataFilesBakLength = FilesInMOmodeDataFilesBak.Length;
                        for (int f = 0; f < FilesInMOmodeDataFilesBakLength; f++)
                        {
                            string DestFileInDataFolderPath = FilesInMOmodeDataFilesBak[f].Replace(ManageSettings.GetMOmodeDataFilesBakDirPath(), DataPath);
                            if (!File.Exists(DestFileInDataFolderPath))
                            {
                                string DestFileInDataFolderPathFolder = Path.GetDirectoryName(DestFileInDataFolderPath);
                                if (!Directory.Exists(DestFileInDataFolderPathFolder))
                                {
                                    Directory.CreateDirectory(DestFileInDataFolderPathFolder);
                                }
                                File.Move(FilesInMOmodeDataFilesBak[f], DestFileInDataFolderPath);
                            }
                        }

                        //удаление папки, где хранились резервные копии ванильных файлов
                        ManageFilesFolders.DeleteEmptySubfolders(ManageSettings.GetMOmodeDataFilesBakDirPath());
                    }

                    //очистка пустых папок в Data
                    if (File.Exists(ManageSettings.GetVanillaDataEmptyFoldersListFilePath()))
                    {
                        //удалить все, за исключением добавленных ранее путей до пустых папок
                        ManageFilesFolders.DeleteEmptySubfolders(DataPath, false, File.ReadAllLines(ManageSettings.GetVanillaDataEmptyFoldersListFilePath()));
                    }
                    else
                    {
                        ManageFilesFolders.DeleteEmptySubfolders(DataPath, false);
                    }

                    //чистка файлов-списков
                    File.Delete(ManageSettings.GetMOToStandartConvertationOperationsListFilePath());
                    File.Delete(ManageSettings.GetVanillaDataFilesListFilePath());
                    File.Delete(ManageSettings.GetVanillaDataEmptyFoldersListFilePath());
                    File.Delete(ManageSettings.GetModdedDataFilesListFilePath());

                    //try
                    //{
                    //    //восстановление 2х папок, что были по умолчанию сначала пустыми
                    //    Directory.CreateDirectory(Path.Combine(DataPath, "UserData", "audio"));
                    //    Directory.CreateDirectory(Path.Combine(DataPath, "UserData", "coordinate", "male"));
                    //}
                    //catch
                    //{
                    //}

                    //File.Move(Path.Combine(MODirPath, "ModOrganizer.exe.GameInCommonModeNow"), Path.Combine(MODirPath, "ModOrganizer.exe"));

                    //создание ссылок на файлы bepinex
                    //BepinExLoadingFix();
                    //обновление информации о конфигурации папок игры
                    FoldersInit();

                    MOCommonModeSwitchButton.Text = T._("MOToCommon");
                    MOCommonModeSwitchButton.Enabled = true;
                    LanchModeInfoLinkLabel.Enabled = true;
                    MessageBox.Show(T._("Mod Organizer mode restored! All mod files moved back to Mods folder. If in Data folder was added new files they also moved in Mods folder as new mod, check and sort it if need"));
                }
            }
            OnOffButtons();
        }

        private void OpenGameFolderLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("explorer.exe", DataPath);
        }

        private void OpenMOFolderLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("explorer.exe", MODirPath);
        }

        private void OpenModsFolderLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("explorer.exe", ModsPath);
        }

        private void Install2MODirPathOpenFolderLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("explorer.exe", Install2MODirPath);
        }

        private void OpenMyUserDataFolderLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string UserFilesFolder = Path.Combine(ModsPath, "MyUserData");
            if (Directory.Exists(UserFilesFolder))
            {
                Process.Start("explorer.exe", UserFilesFolder);
            }
        }

        private void OpenMOOverwriteFolderLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Directory.Exists(OverwriteFolder))
            {
                Process.Start("explorer.exe", OverwriteFolder);
            }
        }

        private void OpenLogLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //https://stackoverflow.com/questions/9993561/c-sharp-open-file-path-starting-with-userprofile
            var USERPROFILE = Path.Combine("%USERPROFILE%", "appdata", "locallow", "illusion__AI-Syoujyo", "AI-Syoujyo", "output_log.txt");
            var output_log = Environment.ExpandEnvironmentVariables(USERPROFILE);
            Process.Start("explorer.exe", output_log);
        }

        private void CurrentGameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.INITDone)
            {
                File.Delete(ManageSettings.GetModOrganizerINIpath());
                File.Delete(ManageSettings.GetMOcategoriesPath());
                SetSelectedGameIndexAndBasicVariables((sender as ComboBox).SelectedIndex);

                new IniFile(ManageSettings.GetAIHelperINIPath()).WriteINI("Settings", "selected_game", ManageSettings.GetCurrentGameFolderName());

                FoldersInit();
            }
        }

        private void ConsoleCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ManageINI.WriteINIValue(ManageSettings.GetBepInExCfgFilePath(), "Logging.Console", "Enabled", /*" " +*/ (sender as CheckBox).Checked.ToString());
            if ((sender as CheckBox).Checked)
            {
                BepInExDisplayedLogLevelLabel.Visible = true;
            }
            else
            {
                BepInExDisplayedLogLevelLabel.Visible = false;
            }

        }

        private void BepInExDisplayedLogLevelLabel_VisibleChanged(object sender, EventArgs e)
        {
            if (BepInExConsoleCheckBox.Checked)
            {
                ManageSettings.SwitchBepInExDisplayedLogLevelValue(BepInExConsoleCheckBox, BepInExDisplayedLogLevelLabel, true);
            }
        }

        private void BepInExDisplayedLogLevelLabel_Click(object sender, EventArgs e)
        {
            if (BepInExConsoleCheckBox.Checked)
            {
                ManageSettings.SwitchBepInExDisplayedLogLevelValue(BepInExConsoleCheckBox, BepInExDisplayedLogLevelLabel);
            }
        }

        ExtraSettings extraSettingsForm;
        private void ExtraSettingsLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (extraSettingsForm == null || extraSettingsForm.IsDisposed)
            {
                //show and reposition of form
                //https://stackoverflow.com/questions/31492787/how-to-set-position-second-form-depend-on-first-form
                extraSettingsForm = new ExtraSettings
                {
                    //LinksForm.Text = T._("Links");
                    StartPosition = FormStartPosition.Manual
                };
                extraSettingsForm.Load += delegate (object s2, EventArgs e2)
                {
                    extraSettingsForm.Location = new Point(Bounds.Location.X + (Bounds.Width / 2) - (extraSettingsForm.Width / 2),
                        Bounds.Location.Y + /*(Bounds.Height / 2) - (f2.Height / 2) +*/ Bounds.Height);
                };
                //extraSettings.Text = T._("Links");
                //newformButton.Text = @"/\";
                if (LinksForm != null && !LinksForm.IsDisposed)
                {
                    LinksForm.Close();
                }
                extraSettingsForm.Show();
            }
            else
            {
                //newformButton.Text = @"\/";
                extraSettingsForm.Close();
            }
        }

        private void AIGirlHelperTabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            newformButton.Text = @"\/";
        }

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