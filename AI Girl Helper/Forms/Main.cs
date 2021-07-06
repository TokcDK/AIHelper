using AIHelper.Games;
using AIHelper.Manage;
using AIHelper.Manage.Update;
using AIHelper.SharedData;
using INIFileMan;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

//using Crc32C;

namespace AIHelper
{
    public partial class Main : Form
    {
        private bool compressmode;

        //constants
        private static string AppResDir { get => Properties.Settings.Default.AppResDir; set => Properties.Settings.Default.AppResDir = value; }

        private static string ModsPath { get => Properties.Settings.Default.ModsPath; set => Properties.Settings.Default.ModsPath = value; }
        private static string DownloadsPath { get => Properties.Settings.Default.DownloadsPath; set => Properties.Settings.Default.DownloadsPath = value; }
        private static string DataPath { get => ManageSettings.GetCurrentGameDataPath(); /*set => Properties.Settings.Default.DataPath = value;*/ }
        private static string MODirPath { get => Properties.Settings.Default.MODirPath; set => Properties.Settings.Default.MODirPath = value; }
        private static string MOexePath { get => Properties.Settings.Default.MOexePath; set => Properties.Settings.Default.MOexePath = value; }
        private static string OverwriteFolder { get => Properties.Settings.Default.OverwriteFolder; set => Properties.Settings.Default.OverwriteFolder = value; }
        private static string OverwriteFolderLink { get => Properties.Settings.Default.OverwriteFolderLink; set => Properties.Settings.Default.OverwriteFolderLink = value; }
        private static string SetupXmlPath { get => Properties.Settings.Default.SetupXmlPath; set => Properties.Settings.Default.SetupXmlPath = value; }
        private static string ApplicationStartupPath { /*get => Properties.Settings.Default.ApplicationStartupPath; */set => Properties.Settings.Default.ApplicationStartupPath = value; }

        //private static string ModOrganizerINIpath { get => Properties.Settings.Default.ModOrganizerINIpath; set => Properties.Settings.Default.ModOrganizerINIpath = value; }
        private static string Install2MODirPath { get => Properties.Settings.Default.Install2MODirPath; set => Properties.Settings.Default.Install2MODirPath = value; }

        private static bool MOmode { get => Properties.Settings.Default.MOmode; set => Properties.Settings.Default.MOmode = value; }

        private static Game CurrentGame { get => GameData.CurrentGame; set => GameData.CurrentGame = value; }
        private static List<Game> ListOfGames { get => GameData.ListOfGames; set => GameData.ListOfGames = value; }

        public Main()
        {
            InitializeComponent();

            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.ApplicationStartupPath))
            {
                ApplicationStartupPath = Application.StartupPath;
            }

            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.ApplicationProductName))
            {
                Properties.Settings.Default.ApplicationProductName = Application.ProductName;
            }

            if (!SetListOfGames())
            {
                System.Windows.Forms.Application.Exit();
                this.Enabled = false;
                return;
            }

            CheckMOAndEndInit();
        }

        private async void CheckMOAndEndInit()
        {
            //MO data parse
            Properties.Settings.Default.MOIsNew = ManageMO.IsMO23ORNever();

            if (!File.Exists(Path.Combine(ManageSettings.GetMOdirPath(), "ModOrganizer.exe")))
            {
                await new Update().update().ConfigureAwait(true);
            }

            ManageMO.RedefineGameMOData();

            ManageMO.CleanMOFolder();
            //
            ManageMO.CheckBaseGamesPy();

            CleanLog();

            VariablesINIT();
            SetMOMode(false);
            CurrentGame.InitActions();

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
                catch
                {
                }
            }
        }

        private static void VariablesINIT()
        {
            Properties.Settings.Default.CurrentGamePath = CurrentGame.GetGamePath();
            //SettingsManage.SettingsINIT();
            AppResDir = ManageSettings.GetAppResDir();
            ModsPath = ManageSettings.GetCurrentGameModsPath();
            DownloadsPath = ManageSettings.GetDownloadsPath();
            //DataPath = ManageSettings.GetDataPath();
            MODirPath = ManageSettings.GetMOdirPath();
            MOexePath = ManageSettings.GetMOexePath();
            Properties.Settings.Default.ModOrganizerINIpath = ManageSettings.GetModOrganizerINIpath();
            Install2MODirPath = ManageSettings.GetInstall2MODirPath();
            OverwriteFolder = ManageSettings.GetOverwriteFolder();
            OverwriteFolderLink = ManageSettings.GetOverwriteFolderLink();
        }

        private bool SetListOfGames()
        {
            try
            {
                ListOfGames = ManageSettings.GetListOfExistsGames();

                if (ListOfGames == null || ListOfGames.Count == 0)
                {
                    MessageBox.Show(T._("Games not found") + "."
                        + Environment.NewLine + T._("Need atleast one game in subfolder in Games folder") + "."
                        + Environment.NewLine + "----------------"
                        + Environment.NewLine + T._("List of games") + ":"
                        + Environment.NewLine + ManageSettings.GetStringListOfAllGames()
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

                foreach (var game in ListOfGames)
                {
                    CurrentGameComboBox.Items.Add(game.GetGameFolderName());
                }
                if (CurrentGameComboBox.Items.Count == 1)
                {
                    //CurrentGameComboBox.Items[0] = ListOfGames[0].GetGameDisplayingName();
                    CurrentGameComboBox.Enabled = false;
                }

                SetSelectedGameIndexAndBasicVariables(ManageSettings.GetCurrentGameIndexByFolderName(
                        ListOfGames
                        ,
                        new INIFile(ManageSettings.GetAIHelperINIPath()).ReadINI("Settings", "selected_game")
                        ));
                try
                {
                    CurrentGameTitleTextBox.Text = GameData.CurrentGame.GetGameDisplayingName();
                    CurrentGameTitleTextBox.Enabled = false;
                }
                catch { }
            }
            catch
            {
                return false;
            }


            return true;
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
            //Properties.Settings.Default.DataPath = Path.Combine(CurrentGame.GetGamePath(), "Data");
            Properties.Settings.Default.ModsPath = Path.Combine(CurrentGame.GetGamePath(), "Mods");

            //set checkbox
            Properties.Settings.Default.AutoShortcutRegistryCheckBoxChecked = bool.Parse(ManageINI.GetINIValueIfExist(ManageSettings.GetAIHelperINIPath(), "autoCreateShortcutAndFixRegystry", "Settings", "False"));
            AutoShortcutRegistryCheckBox.Checked = Properties.Settings.Default.AutoShortcutRegistryCheckBoxChecked;
        }

        private void SetLocalizationStrings()
        {
            this.Text = "AI Helper" + " | " + CurrentGame.GetGameDisplayingName();
            CurrentGameLabel.Text = T._("Current Game") + ":";
            InstallInModsButton.Text = T._("Install from") + " " + ManageSettings.ModsInstallDirName();
            ToolsFixModListButton.Text = T._("Fix modlist");
            btnUpdateMods.Text = T._("Update");
            //button1.Text = T._("Prepare the game");
            SettingsPage.Text = T._("Settings");
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
            MainPage.Text = T._("Info");
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

            Properties.Settings.Default.CurrentGameDisplayingName = CurrentGame.GetGameDisplayingName();
        }

        private int mode;

        private void MainService_Click(object sender, EventArgs e)
        {
            //MainService.Enabled = false;

            mode = GetModeValue();

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

            //MainService.Enabled = true;
        }

        private int GetModeValue()
        {
            if (File.Exists("PackMe!.txt"))
            {
                compressmode = true;
            }
            else
            {
                compressmode = false;
            }
            return 0;
        }

        private async void ExtractingMode()
        {
            MainService.Text = T._("Extracting") + "..";
            MainService.Enabled = false;

            //https://ru.stackoverflow.com/questions/222414/%d0%9a%d0%b0%d0%ba-%d0%bf%d1%80%d0%b0%d0%b2%d0%b8%d0%bb%d1%8c%d0%bd%d0%be-%d0%b2%d1%8b%d0%bf%d0%be%d0%bb%d0%bd%d0%b8%d1%82%d1%8c-%d0%bc%d0%b5%d1%82%d0%be%d0%b4-%d0%b2-%d0%be%d1%82%d0%b4%d0%b5%d0%bb%d1%8c%d0%bd%d0%be%d0%bc-%d0%bf%d0%be%d1%82%d0%be%d0%ba%d0%b5
            await Task.Run(() => UnpackGame()).ConfigureAwait(true);
            await Task.Run(() => UnpackMO()).ConfigureAwait(true);
            await Task.Run(() => UnpackMods()).ConfigureAwait(true);

            //BepinExLoadingFix();//добавлено в folderinit

            ManageOther.CreateShortcuts();

            ManageMO.DummyFiles();

            MainService.Text = T._("Game Ready");
            FoldersInit();
            MainService.Enabled = true;
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
                MainService.Enabled = false;
                MainService.Text = "Compressing..";

                //https://ru.stackoverflow.com/questions/222414/%d0%9a%d0%b0%d0%ba-%d0%bf%d1%80%d0%b0%d0%b2%d0%b8%d0%bb%d1%8c%d0%bd%d0%be-%d0%b2%d1%8b%d0%bf%d0%be%d0%bb%d0%bd%d0%b8%d1%82%d1%8c-%d0%bc%d0%b5%d1%82%d0%be%d0%b4-%d0%b2-%d0%be%d1%82%d0%b4%d0%b5%d0%bb%d1%8c%d0%bd%d0%be%d0%bc-%d0%bf%d0%be%d1%82%d0%be%d0%ba%d0%b5
                //await Task.Run(() => PackGame());
                //await Task.Run(() => PackMO());
                await Task.Run(() => PackMods()).ConfigureAwait(true);
                await Task.Run(() => PackSeparators()).ConfigureAwait(true);

                ////http://www.sql.ru/forum/1149655/kak-peredat-parametr-s-metodom-delegatom
                //Thread open = new Thread(new ParameterizedThreadStart((obj) => PackMods()));
                //open.Start();

                MainService.Text = T._("Prepare the game");
                FoldersInit();
                MainService.Enabled = true;
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
                        CopyFolder.CopyAll(dir, Path.Combine(tempdir, Path.GetFileName(dir)));
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
        private static string GetResultTargetName(List<CategoriesList> categories, string inputmoddir)
        {
            string targetdir = DownloadsPath;
            targetdir = ManageSettings.GetCurrentGameInstallDirPath();

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
                int categiryindex = int.Parse(categoryvalue, CultureInfo.InvariantCulture) - 1;//В List индекс идет от нуля
                if (categiryindex > 0)
                {
                    if (categiryindex < categories.Count)//на всякий, защита от ошибки выхода за диапазон
                    {
                        //Проверить родительскую категорию
                        int ParentIDindex = int.Parse(categories[categiryindex].ParentID, CultureInfo.InvariantCulture) - 1;//В List индекс идет от нуля
                        if (ParentIDindex > 0 && ParentIDindex < categories.Count)
                        {
                            targetdir = Path.Combine(targetdir, categories[ParentIDindex].Name);
                        }

                        targetdir = Path.Combine(targetdir, categories[categiryindex].Name);

                        Directory.CreateDirectory(targetdir);
                    }
                }
            }

            return targetdir;
        }

        ToolTip THToolTip;
        private void SetTooltips()
        {
            try
            {
                if (THToolTip != null)
                {
                    THToolTip.RemoveAll();
                }
            }
            catch
            {
            }

            //http://qaru.site/questions/47162/c-how-do-i-add-a-tooltip-to-a-control
            //THMainResetTableButton
            THToolTip = new ToolTip
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

            THToolTip.SetToolTip(ProgramNameLabelPart2, Properties.Settings.Default.ApplicationProductName + " - " + T._("Illusion games manager.\n\n"
                    + "Move mouse over wished button or text to see info about it"
                    )
                );
            THToolTip.SetToolTip(SelectedGameLabel, T._("Selected game title"));

            //Launch
            THToolTip.SetToolTip(VRGameCheckBox, T._("Check to run VR exe instead on standart"));

            //Main
            //THToolTip.SetToolTip(button1, T._("Unpacking mods and resources from 'Downloads' and 'RES' folders for game when they are not installed"));
            THToolTip.SetToolTip(InstallInModsButton, T._("Install mods and userdata, placed in") + " " + ManageSettings.ModsInstallDirName()
                + (MOmode ? T._(
                        " to MO format in Mods when possible"
                    ) : T._(
                        " to the game folder when possible"
                        )));
            THToolTip.SetToolTip(ToolsFixModListButton, T._("Fix problems in current enabled mods list"));
            THToolTip.SetToolTip(btnUpdateMods,
                T._("Update Mod Organizer and enabled mods") + "\n" +
                T._("Mod Organizer already have hardcoded info") + "\n" +
                T._("Mods will be updated if there exist info in meta.ini notes or in updateInfo.txt") + "\n" +
                T._("After plugins update check will be executed KKManager StandaloneUpdater for Sideloader modpack updates check for games where it is possible")
                );
            THToolTip.SetToolTip(llOpenOldPluginsBuckupFolder,
                T._("Open older plugins buckup folder")
                );
            THToolTip.SetToolTip(cbxBleadingEdgeZipmods,
                T._("Check also Bleeding Edge SIdeloader Modpack in KKManager") + "\n" +
                T._("Bleeding Edge SIdeloader modpack contains test versions of zipmods which is still not added in main modpacks")
                );
            THToolTip.SetToolTip(Install2MODirPathOpenFolderLinkLabel, T._("Open folder where you can drop/download files for autoinstallation"));
            THToolTip.SetToolTip(AutoShortcutRegistryCheckBox, T._("When checked will create shortcut for the AI Helper on Desktop and will fix registry if need"));
            THToolTip.SetToolTip(DisplaySettingsGroupBox, T._("Game Display settings"));
            THToolTip.SetToolTip(SetupXmlLinkLabel, T._("Open Setup.xml in notepad"));
            THToolTip.SetToolTip(ResolutionComboBox, T._("Select preferred screen resolution"));
            THToolTip.SetToolTip(FullScreenCheckBox, T._("When checked game will be in fullscreen mode"));
            THToolTip.SetToolTip(QualityComboBox, T._("Select preferred graphics quality"));
            //THToolTip.SetToolTip(CreateShortcutButton, T._("Will create shortcut in Desktop if not exist"));
            THToolTip.SetToolTip(CreateShortcutLinkLabel, T._("Will create shortcut in Desktop if not exist"));
            //THToolTip.SetToolTip(FixRegistryButton, T._("Will set Data dir with game files as install dir in registry"));
            THToolTip.SetToolTip(FixRegistryLinkLabel, T._("Will set Data dir with game files as install dir in registry"));
            THToolTip.SetToolTip(GameButton, MOmode ? T._("Will execute the Game")
                + T._(" from Mod Organizer with attached mods")
                : T._("Will execute the Game")
                );
            THToolTip.SetToolTip(StudioButton, MOmode ? T._("Will execute Studio")
                + T._(" from Mod Organizer with attached mods")
                : T._("Will execute Studio")
                );
            THToolTip.SetToolTip(MOButton, T._("Will execute Mod Organizer mod manager where you can manage your mods"));
            THToolTip.SetToolTip(JPLauncherRunLinkLabel, MOmode ?
                  T._("Will execute original game launcher")
                + T._(" from Mod Organizer with attached mods")
                : T._("Will execute original game launcher")
                );
            THToolTip.SetToolTip(SettingsButton, T._("Will be opened Settings tab"));
            THToolTip.SetToolTip(MOCommonModeSwitchButton, MOmode ? T._(
                    "Will convert game from MO Mode to Common mode\n" +
                    " when you can run exes from Data folder without Mod Organizer.\n You can convert game back to MO mode\n" +
                    " when it will be need to install new mods or test your mod config"
                ) : T._(
                    "Will convert the game to MO mode\n" +
                    " when all mod files will be moved back to Mods folder\n" +
                    " in their folders and vanilla files restored"
                )
                );
            THToolTip.SetToolTip(LaunchModeInfoLinkLabel, T._("Same as button in Tool tab.\n")
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

            //Open Folders
            THToolTip.SetToolTip(OpenGameFolderLinkLabel, T._("Open Data folder of selected game"));
            THToolTip.SetToolTip(OpenModsFolderLinkLabel, T._("Open Mods folder of selected game"));
            THToolTip.SetToolTip(OpenMOFolderLinkLabel, T._("Open Mod Organizer folder"));
            THToolTip.SetToolTip(OpenMOOverwriteFolderLinkLabel, T._("Open Overwrite folder of Mod Organizer with possible new generated files for selected game\n\nFiles here have highest priority and will be loaded over any enabled mod files"));
            THToolTip.SetToolTip(OpenMyUserDataFolderLinkLabel, T._("Open MyUserData folder in Mods if exist\n\nHere placed usual User files of Organized ModPack for selected game"));

            THToolTip.SetToolTip(LaunchLinksLinkLabel, T._("Open list of links for game resources"));
            THToolTip.SetToolTip(pbDiscord, T._("Discord page. Info, links, support."));
            THToolTip.SetToolTip(ExtraSettingsLinkLabel, T._("Open extra setting window for plugins and etc"));

            THToolTip.SetToolTip(OpenLogLinkLabel, T._("Open BepinEx log if found"));
            THToolTip.SetToolTip(BepInExDisplayedLogLevelLabel, T._("Click here to select log level\n" +
                "Only displays the specified log level and above in the console output"));


            THToolTip.SetToolTip(CurrentGameComboBox, T._("List of found games. Current") + ": " + GameData.CurrentGame.GetGameDisplayingName());

            var toMO = ManageSettings.ModsInstallDirName();
            THToolTip.SetToolTip(Open2MOLinkLabel,
                T._($"Open {toMO} folder fo selected game" +
                "\n\nHere can be placed mod files which you want to install for selected game in approriate subfolders in mods" +
                "\nand then can be installed all by one click on") + " " + InstallInModsButton.Text + " " + T._("button") +
                "\n" + T._("which can be found in") + " " + ToolsTabPage.Text + " " + T._("tab page") +
                "\n\n" + T._("Helper recognize") + ":"
                + "\n " + T._($".dll files of BepinEx plugins in \"{toMO}\" folder")
                + "\n " + T._($"Sideloader mod archives in \"{toMO}\" folder")
                + "\n " + T._($"Female character cards in \"{toMO}\" folder")
                + "\n " + T._("Female character cards in \"f\" subfolder")
                + "\n " + T._("Male character cards in \"m\" subfolder")
                + "\n " + T._("Coordinate clothes set cards in \"c\" subfolder")
                + "\n " + T._("Studio scene cards in \"s\" subfolder")
                + "\n " + T._("Cardframe Front cards in \"cf\" subfolder")
                + "\n " + T._("Cardframe Back cards in \"cf\" subfolder")
                + "\n " + T._($"Script loader scripts in \"{toMO}\" folder")
                + "\n " + T._("Housing plan cards in \"h\\01\", \"h\\02\", \"h\\03\" subfolders")
                + "\n " + T._("Overlays cards in \"o\" subfolder")
                + "\n " + T._("folders with overlays cards in \"o\" subfolder")
                + "\n " + T._($"Subfolders with modfiles in \"{toMO}\" folder")
                + "\n " + T._($"Zip archives with mod files in \"{toMO}\" folder")
                + "\n " + T._($"Zip archives with mod files in \"{toMO}\" folder")
                + "\n\n" + T._($"Any Rar and 7z archives in \"{toMO}\" folder will be extracted" +
                "\nSome recognized mods can be updated instead of be installed as new mod" +
                "\nMost of mods will be automatically activated except .cs scripts" +
                "\nwhich always optional and often it is cheats or can slowdown/break game")

                );
            ////////////////////////////
        }

        private void SetScreenSettings()
        {
            if (!MOmode)
            {
                SetupXmlPath = Path.Combine(ManageSettings.GetCurrentGameDataPath(), "UserData", "setup.xml");
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
                            string SelectedRes = ResolutionComboBox.Items[w].ToString();
                            ResolutionComboBox.Text = SelectedRes;
                            SetScreenResolution(SelectedRes);
                            break;
                        }
                    }
                }
            }

            ResolutionComboBox.Text = ManageXML.ReadXmlValue(SetupXmlPath, "Setting/Size", ResolutionComboBox.Text);
            FullScreenCheckBox.Checked = bool.Parse(ManageXML.ReadXmlValue(SetupXmlPath, "Setting/FullScreen", FullScreenCheckBox.Checked + ""));

            string Quality = ManageXML.ReadXmlValue(SetupXmlPath, "Setting/Quality", "2");
            //если качество будет за пределами диапазона 0-2, тогда будет равно 1 - нормально
            if (Quality == "0" || Quality == "1" || Quality == "2")
            {
            }
            else
            {
                Quality = "1";
            }

            QualityComboBox.SelectedIndex = int.Parse(Quality, CultureInfo.InvariantCulture);
        }

        private static void SetScreenResolution(string Resolution)
        {
            ManageMO.CheckMOUserdata();

            ManageXML.ChangeSetupXmlValue(SetupXmlPath, "Setting/Size", Resolution);
            string[] WH = Resolution.Replace("(16 : 9)", string.Empty).Trim().Split('x');
            ManageXML.ChangeSetupXmlValue(SetupXmlPath, "Setting/Width", WH[0].Trim());
            ManageXML.ChangeSetupXmlValue(SetupXmlPath, "Setting/Height", WH[1].Trim());
        }

        private static void SetGraphicsQuality(string quality)
        {
            ManageMO.CheckMOUserdata();

            ManageXML.ChangeSetupXmlValue(SetupXmlPath, "Setting/Quality", quality);
        }

        private void FoldersInit()
        {
            EnableDisableSomeTools();

            SetMOMode();

            if (!Directory.Exists(DataPath))
            {
                Directory.CreateDirectory(DataPath);
            }
            if (MOmode && !Directory.Exists(ModsPath))
            {
                Directory.CreateDirectory(ModsPath);
                ModsInfoLabel.Text = T._("Mods dir created");
            }

            if (File.Exists(Path.Combine(DataPath, ManageSettings.GetCurrentGameEXEName() + ".exe")))
            {
                DataInfoLabel.Text = string.Format(CultureInfo.InvariantCulture, T._("{0} game installed in {1}"), ManageSettings.GetCurrentGameDisplayingName(), "Data");
            }
            else if (File.Exists(Path.Combine(AppResDir, ManageSettings.GetCurrentGameEXEName() + ".7z")))
            {
                DataInfoLabel.Text = string.Format(CultureInfo.InvariantCulture, T._("{0} archive in {1}"), "AIGirl", "Data");
            }
            else if (Directory.Exists(DataPath))
            {
                DataInfoLabel.Text = string.Format(CultureInfo.InvariantCulture, T._("{0} files not in {1}. Move {0} game files there."), ManageSettings.GetCurrentGameFolderName(), "Data");
            }
            else
            {
                Directory.CreateDirectory(DataPath);
                DataInfoLabel.Text = string.Format(CultureInfo.InvariantCulture, T._("{0} dir created. Move {1} game files there."), "Data", ManageSettings.GetCurrentGameFolderName());
            }

            if (MOmode)
            {
                ManageMO.RestoreModlist();

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


                if (!Manage.ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Manage.ManageSettings.GetCurrentGameModsPath(), "*", new string[1] { "_separator" }))
                {
                    ModsInfoLabel.Text = T._("Found mod folders in Mods");
                    //button1.Enabled = false;
                    mode = 1;
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

                ManageMO.DummyFiles();

                ManageMO.MOINIFixes();

                //try start in another thread for perfomance purposes
                new Thread(new ParameterizedThreadStart((obj) => RunSlowActions())).Start();

                SetupXmlPath = ManageMO.GetSetupXmlPathForCurrentProfile();

                ManageMOMods.SetMOModsVariables();
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

            //Обновление пути к setup.xml с настройками графики
            VariablesINIT();

            AIGirlHelperTabControl.SelectedTab = LaunchTabPage;
            CurrentGameComboBox.Text = CurrentGame.GetGameFolderName();
            CurrentGameComboBox.SelectedIndex = ManageSettings.GetCurrentGameIndex();

            GetEnableDisableLaunchTabButtons();

            SetScreenSettings();

            SetTooltips();

            if (AutoShortcutRegistryCheckBox.Checked)
            {
                ManageOther.AutoShortcutAndRegystry();
            }

            SelectedGameLabel.Text = CurrentGame.GetGameDisplayingName() + "❤";
            this.Text = "AI Helper" + " | " + CurrentGame.GetGameDisplayingName();
        }

        private void SetMOMode(bool setText = true)
        {
            if (File.Exists(ManageSettings.GetMOToStandartConvertationOperationsListFilePath()))
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

        bool IsDebug;
        bool IsBetaTest;
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
            ManageMOMods.MOUSFSLoadingFix();
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

            ManageMO.SetModOrganizerINISettingsForTheGame();
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
            JPLauncherRunLinkLabel.Enabled = File.Exists(Path.Combine(DataPath, ManageSettings.GetINISettingsEXEName() + ".exe"));
            GameButton.Enabled = File.Exists(Path.Combine(DataPath, ManageSettings.GetCurrentGameEXEName() + ".exe"));
            StudioButton.Enabled = File.Exists(Path.Combine(DataPath, ManageSettings.GetStudioEXEName() + ".exe"));

            //Set BepInEx log data
            var BepInExCFGPath = ManageSettings.GetBepInExCfgFilePath();
            if (BepInExCFGPath.Length > 0 && File.Exists(BepInExCFGPath))
            {
                BepInExConsoleCheckBox.Enabled = true;
                try
                {
                    //BepInExConsoleCheckBox.Checked = bool.Parse(ManageINI.GetINIValueIfExist(ManageSettings.GetBepInExCfgFilePath(), "Enabled", "Logging.Console", "False"));
                    BepInExConsoleCheckBox.Checked = bool.Parse(ManageCFG.GetCFGValueIfExist(ManageSettings.GetBepInExCfgFilePath(), "Enabled", "Logging.Console", "False"));
                }
                catch
                {
                    BepInExConsoleCheckBox.Checked = false;
                }
            }

            //VR
            VRGameCheckBox.Visible = ManageSettings.GetCurrentGameIsHaveVR();
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            ManageOther.CheckBoxChangeColor(sender as CheckBox);
            Properties.Settings.Default.AutoShortcutRegistryCheckBoxChecked = AutoShortcutRegistryCheckBox.Checked;
            new INIFile(ManageSettings.GetAIHelperINIPath()).WriteINI("Settings", "autoCreateShortcutAndFixRegystry", Properties.Settings.Default.AutoShortcutRegistryCheckBoxChecked.ToString(CultureInfo.InvariantCulture));
        }

        private void ResolutionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetScreenResolution((sender as ComboBox).SelectedItem.ToString());
        }

        private void FullScreenCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ManageOther.CheckBoxChangeColor(sender as CheckBox);
            ManageXML.ChangeSetupXmlValue(SetupXmlPath, "Setting/FullScreen", (sender as CheckBox).Checked.ToString().ToLower());
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
                RunProgram(MOexePath, string.Empty);
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
            AIGirlHelperTabControl.SelectedTab = SettingsPage;
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
            if (VRGameCheckBox.Visible && VRGameCheckBox.Checked && ManageSettings.GetCurrentGameIsHaveVR())
            {
                vr = "VR";
            }
            if (MOmode)
            {
                var GetCurrentGameEXEMOProfileName = ManageSettings.GetCurrentGameEXEMOProfileName();
                RunProgram(MOexePath, "moshortcut://:" + GetCurrentGameEXEMOProfileName + vr);
            }
            else
            {
                RunProgram(Path.Combine(DataPath, ManageSettings.GetCurrentGameEXEName() + vr + ".exe"), string.Empty);
            }
            OnOffButtons();
        }

        private void OnOffButtons(bool SwitchOn = true)
        {
            AIGirlHelperTabControl.Invoke((Action)(() => AIGirlHelperTabControl.Enabled = SwitchOn));
        }

        private async void StudioButton_Click(object sender, EventArgs e)
        {
            OnOffButtons(false);

            await Task.Run(() => ManageOther.WaitIfGameIsChanging()).ConfigureAwait(true);

            if (MOmode)
            {
                var studio = ManageMO.GetMOcustomExecutableTitleByExeName(ManageSettings.GetStudioEXEName());
                RunProgram(MOexePath, "moshortcut://:" + studio);
            }
            else
            {
                RunProgram(Path.Combine(DataPath, ManageSettings.GetStudioEXEName() + ".exe"), string.Empty);
            }
            OnOffButtons();
        }

        private void RunProgram(string ProgramPath, string Arguments = "")
        {
            if (File.Exists(ProgramPath))
            {
                GC.Collect();//reduce memory usage before run a program

                //fix mo profile name missing quotes when profile name with spaces
                if (!string.IsNullOrWhiteSpace(Arguments) && Arguments.Contains("moshortcut://:") && !Arguments.Contains("moshortcut://:\""))
                {
                    Arguments = Arguments.Replace("moshortcut://:", "moshortcut://:\"") + "\"";
                }

                if (Path.GetFileNameWithoutExtension(ProgramPath) == "ModOrganizer" && Arguments.Length > 0)
                {
                    Task.Run(() => RunFreezedMOKiller(Arguments.Replace("moshortcut://:", string.Empty))).ConfigureAwait(false);
                }

                using (Process Program = new Process())
                {
                    //MessageBox.Show("outdir=" + outdir);
                    Program.StartInfo.FileName = ProgramPath;
                    if (Arguments.Length > 0)
                    {
                        Program.StartInfo.Arguments = Arguments;
                    }

                    if (!Properties.Settings.Default.MOmode || string.IsNullOrWhiteSpace(Program.StartInfo.Arguments))
                    {
                        Program.StartInfo.WorkingDirectory = Path.GetDirectoryName(ProgramPath);
                    }

                    //http://www.cyberforum.ru/windows-forms/thread31052.html
                    // свернуть
                    ManageOther.SwitchFormMinimizedNormalAll(new Form[3] { this, LinksForm, extraSettingsForm });
                    //WindowState = FormWindowState.Minimized;
                    //if (LinksForm == null || LinksForm.IsDisposed)
                    //{
                    //}
                    //else
                    //{
                    //    LinksForm.WindowState = FormWindowState.Minimized;
                    //}
                    //if (extraSettingsForm == null || extraSettingsForm.IsDisposed)
                    //{
                    //}
                    //else
                    //{
                    //    extraSettingsForm.WindowState = FormWindowState.Minimized;
                    //}

                    _ = Program.Start();
                    Program.WaitForExit();

                    // Показать
                    ManageOther.SwitchFormMinimizedNormalAll(new Form[3] { this, LinksForm, extraSettingsForm });
                    //WindowState = FormWindowState.Normal;
                    //if (LinksForm == null || LinksForm.IsDisposed)
                    //{
                    //}
                    //else
                    //{
                    //    LinksForm.WindowState = FormWindowState.Normal;
                    //}
                    //if (extraSettingsForm == null || extraSettingsForm.IsDisposed)
                    //{
                    //}
                    //else
                    //{
                    //    extraSettingsForm.WindowState = FormWindowState.Normal;
                    //}
                }
            }
        }

        /// <summary>
        /// If Mod Organizer Will freeze in memory after main process will be closed then it will try to kill MO exe
        /// </summary>
        /// <param name="processName"></param>
        private static void RunFreezedMOKiller(string processName)
        {
            //if (string.IsNullOrEmpty(processName))
            //{
            //    return;
            //}

            ////https://stackoverflow.com/questions/262280/how-can-i-know-if-a-process-is-running

            //Thread.Sleep(5000);
            //while (Process.GetProcessesByName(processName).Length != 0)
            //{
            //    Thread.Sleep(5000);
            //}

            //try
            //{
            //    if (Process.GetProcessesByName("ModOrganizer").Length != 0)
            //    {
            //        foreach (var process in Process.GetProcessesByName("ModOrganizer"))
            //        {
            //            process.Kill();
            //        }
            //    }
            //}
            //catch
            //{
            //    MessageBox.Show("AIHelper: Failed to kill one of freezed ModOrganizer.exe. Try to kill it from Task Manager.");
            //}

        }

        private readonly Dictionary<string, string> qualitylevels = new Dictionary<string, string>(3);

        private void QualityComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetGraphicsQuality((sender as ComboBox).SelectedIndex.ToString(CultureInfo.InvariantCulture));
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
                LinksForm.TopMost = true;
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

        readonly string toMO = ManageSettings.ModsInstallDirName();
        private async void InstallInModsButton_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(Install2MODirPath) && (Directory.GetFiles(Install2MODirPath, "*.rar").Length > 0 || Directory.GetFiles(Install2MODirPath, "*.7z").Length > 0 || Directory.GetFiles(Install2MODirPath, "*.png").Length > 0 || Directory.GetFiles(Install2MODirPath, "*.cs").Length > 0 || Directory.GetFiles(Install2MODirPath, "*.dll").Length > 0 || Directory.GetFiles(Install2MODirPath, "*.zipmod").Length > 0 || Directory.GetFiles(Install2MODirPath, "*.zip").Length > 0 || Directory.GetDirectories(Install2MODirPath, "*").Length > 0))
            {
                OnOffButtons(false);

                //impossible to correctly update mods in common mode
                if (!Properties.Settings.Default.MOmode)
                {
                    DialogResult result = MessageBox.Show(T._("Attention") + "\n\n" + T._("Impossible to correctly install/update mods\n\n in standart mode because files was moved in Data.") + "\n\n" + T._("Switch to MO mode?"), T._("Confirmation"), MessageBoxButtons.OKCancel);
                    if (result == DialogResult.OK)
                    {
                        SwitchBetweenMOAndStandartModes();
                    }
                    else
                    {
                        OnOffButtons();
                        return;
                    }
                }

                await Task.Run(() => InstallModFilesAndCleanEmptyFolder()).ConfigureAwait(true);

                InstallInModsButton.Text = T._("Install from") + " " + ManageSettings.ModsInstallDirName();

                OnOffButtons();

                //обновление информации о конфигурации папок игры
                FoldersInit();

                MessageBox.Show(T._($"All possible mods installed. Install all rest in {toMO} folder manually."));
            }
            else
            {
                MessageBox.Show(T._($"No compatible for installation formats found in {toMO} folder.\nFormats: zip, zipmod, png, png in subfolder, unpacked mod in subfolder"));
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

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = T._("Install from") + " " + ManageSettings.ModsInstallDirName()));
        }

        private void CreateShortcutButton_Click(object sender, EventArgs e)
        {
            ManageOther.CreateShortcuts(true, false);
        }

        private void MO2StandartButton_Click(object sender, EventArgs e)
        {
            SwitchBetweenMOAndStandartModes();
        }

        private async void SwitchBetweenMOAndStandartModes()
        {
            OnOffButtons(false);
            if (MOmode)
            {
                DialogResult result = MessageBox.Show(
                    T._("Attention")
                    + "\n\n"
                    + T._("Conversation to")
                    + " " + T._("Common mode")
                    + "\n\n"
                    + T._("This will move using mod files from Mods folder to Data folder to make it like common installation variant.\nYou can restore it later back to MO mode.\n\nContinue?")
                    , T._("Confirmation"), MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    using (ProgressBar MO2CommonProgressBar = new ProgressBar())
                    {
                        MO2CommonProgressBar.Style = ProgressBarStyle.Marquee;
                        MO2CommonProgressBar.MarqueeAnimationSpeed = 50;
                        MO2CommonProgressBar.Dock = DockStyle.Bottom;
                        MO2CommonProgressBar.Height = 10;

                        this.Controls.Add(MO2CommonProgressBar);

                        await Task.Run(() => SwitchToCommonMode()).ConfigureAwait(true);

                        this.Controls.Remove(MO2CommonProgressBar);

                        OnOffButtons();
                    }

                    try
                    {
                        MOCommonModeSwitchButton.Text = MOmode ? T._("MOToCommon") : T._("CommonToMO");
                    }
                    catch
                    {
                    }

                    //Directory.Delete(ModsPath, true);
                    //Directory.Move(MODirPath, Path.Combine(AppResDir, Path.GetFileName(MODirPath)));
                    //MOCommonModeSwitchButton.Enabled = true;
                    //LanchModeInfoLinkLabel.Enabled = true;
                    //File.Move(Path.Combine(MODirPath, "ModOrganizer.exe"), Path.Combine(MODirPath, "ModOrganizer.exe.GameInCommonModeNow"));
                    //обновление информации о конфигурации папок игры
                    FoldersInit();
                }
            }
            else
            {
                DialogResult result = MessageBox.Show(T._("Attention") + "\n\n" + T._("Conversation to") + " " + T._("MO mode") + "\n\n" + T._("This will move all mod files back to Mods folder from Data and will switch to MO mode.\n\nContinue?"), T._("Confirmation"), MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    //MOCommonModeSwitchButton.Enabled = false;
                    //LanchModeInfoLinkLabel.Enabled = false;

                    using (ProgressBar MO2CommonProgressBar = new ProgressBar())
                    {
                        MO2CommonProgressBar.Style = ProgressBarStyle.Marquee;
                        MO2CommonProgressBar.MarqueeAnimationSpeed = 50;
                        MO2CommonProgressBar.Dock = DockStyle.Bottom;
                        MO2CommonProgressBar.Height = 10;

                        this.Controls.Add(MO2CommonProgressBar);

                        await Task.Run(() => SwitchBackToMOMode()).ConfigureAwait(true);

                        this.Controls.Remove(MO2CommonProgressBar);
                    }
                    MOCommonModeSwitchButton.Text = T._("MOToCommon");
                    //MOCommonModeSwitchButton.Enabled = true;
                    //LanchModeInfoLinkLabel.Enabled = true;

                    //создание ссылок на файлы bepinex
                    //BepinExLoadingFix();
                    //обновление информации о конфигурации папок игры
                    FoldersInit();

                }
            }

            OnOffButtons();
        }

        private void SwitchBackToMOMode()
        {
            StringBuilder OperationsMade = new StringBuilder();
            string[] MOToStandartConvertationOperationsList = null;
            try
            {
                MOToStandartConvertationOperationsList = File.ReadAllLines(ManageSettings.GetMOToStandartConvertationOperationsListFilePath());
                ReplaceVarsToPaths(ref MOToStandartConvertationOperationsList);
                var OperationsSplitString = new string[] { "|MovedTo|" };
                var VanillaDataFilesList = File.ReadAllLines(ManageSettings.GetVanillaDataFilesListFilePath());
                ReplaceVarsToPaths(ref VanillaDataFilesList);
                var ModdedDataFilesList = File.ReadAllLines(ManageSettings.GetModdedDataFilesListFilePath());
                ReplaceVarsToPaths(ref ModdedDataFilesList);
                Dictionary<string, string> ZipmodsGUIDList = new Dictionary<string, string>();
                bool ZipmodsGUIDListNotEmpty = false;
                if (File.Exists(ManageSettings.GetZipmodsGUIDListFilePath()))
                {
                    using (var sr = new StreamReader(ManageSettings.GetZipmodsGUIDListFilePath()))
                    {
                        while (!sr.EndOfStream)
                        {
                            var line = sr.ReadLine();

                            if (!string.IsNullOrWhiteSpace(line) && line.Contains("{{ZIPMOD}}"))
                            {
                                var GUIDPathPair = ReplaceVarsToPaths(line).Split(new[] { "{{ZIPMOD}}" }, StringSplitOptions.None);
                                ZipmodsGUIDList.Add(GUIDPathPair[0], GUIDPathPair[1]);
                            }
                        }
                    }
                    ZipmodsGUIDListNotEmpty = ZipmodsGUIDList.Count > 0;
                }

                //remove normal mode identifier
                SwitchNormalModeIdentifier(false);

                StringBuilder FilesWhichAlreadyHaveSameDestFileInMods = new StringBuilder();
                bool FilesWhichAlreadyHaveSameDestFileInModsIsNotEmpty = false;

                //Перемещение файлов модов по списку
                int OperationsLength = MOToStandartConvertationOperationsList.Length;
                for (int o = 0; o < OperationsLength; o++)
                {
                    if (string.IsNullOrWhiteSpace(MOToStandartConvertationOperationsList[o]))
                    {
                        continue;
                    }

                    string[] MovePaths = MOToStandartConvertationOperationsList[o].Split(OperationsSplitString, StringSplitOptions.None);

                    var FilePathInModsExists = File.Exists(MovePaths[0]);
                    var FilePathInDataExists = File.Exists(MovePaths[1]);

                    if (!FilePathInDataExists)
                    {
                        continue;
                    }

                    if (!FilePathInModsExists)
                    {
                        string modsubfolder = Path.GetDirectoryName(MovePaths[0]);
                        if (!Directory.Exists(modsubfolder))
                        {
                            Directory.CreateDirectory(modsubfolder);
                        }

                        try//ignore move file error if file will be locked and write in log about this
                        {
                            File.Move(MovePaths[1], MovePaths[0]);

                            //запись выполненной операции для удаления из общего списка в случае ошибки при переключении из обычного режима
                            OperationsMade.AppendLine(MOToStandartConvertationOperationsList[o]);

                            try
                            {
                                //Move bonemod file both with original
                                if (File.Exists(MovePaths[1] + ".bonemod.txt"))
                                {
                                    File.Move(MovePaths[1] + ".bonemod.txt", MovePaths[0] + ".bonemod.txt");
                                }
                                //запись выполненной операции для удаления из общего списка в случае ошибки при переключении из обычного режима
                                OperationsMade.AppendLine(MovePaths[1] + ".bonemod.txt" + "|MovedTo|" + MovePaths[0] + ".bonemod.txt");
                            }
                            catch
                            {
                            }
                        }
                        catch (Exception ex)
                        {
                            ManageLogs.Log("Failed to move file: '" + Environment.NewLine + MovePaths[1] + "' " + Environment.NewLine + "Error:" + Environment.NewLine + ex);
                        }
                    }
                    else
                    {
                        //если в Mods на месте планируемого для перемещения назад в Mods файла появился новый файл, то записать информацию о нем в новый мод, чтобы перенести его в новый мод
                        FilesWhichAlreadyHaveSameDestFileInMods.AppendLine(MovePaths[1] + "|MovedTo|" + MovePaths[0]);
                        FilesWhichAlreadyHaveSameDestFileInModsIsNotEmpty = true;
                    }
                }

                //string destFolderForNewFiles = Path.Combine(ModsPath, "NewAddedFiles");

                //получение даты и времени для дальнейшего использования
                string DateTimeInFormat = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

                if (FilesWhichAlreadyHaveSameDestFileInModsIsNotEmpty)
                {
                    foreach (string FromToPathsLine in FilesWhichAlreadyHaveSameDestFileInMods.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (string.IsNullOrWhiteSpace(FromToPathsLine))
                        {
                            continue;
                        }

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

                        //ВОЗМОЖНО ЗДЕСЬ ПРОБЛЕМА В КОДЕ, ПРИ КОТОРОЙ ДЛЯ КАЖДОГО ФАЙЛА БУДЕТ СОЗДАНА ОТДЕЛЬНАЯ ПАПКА С МОДОМ
                        //НУЖНО ДОБАВИТЬ ЗАПИСЬ И ПОДКЛЮЧЕНИЕ НОВОГО МОДА ТОЛЬКО ПОСЛЕ ТОГО, КАК ВСЕ ФАЙЛЫ ИЗ НЕГО ПЕРЕМЕЩЕНЫ

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
                        ManageMO.ActivateDeactivateInsertMod(NewModName, false, ModName, false);
                    }
                }

                //Перемещение новых файлов
                //
                //добавление всех файлов из дата, которых нет в списке файлов модов и игры в дата, что был создан сразу после перехода в обычный режим
                string[] addedFiles = Directory.GetFiles(DataPath, "*.*", SearchOption.AllDirectories).Where(line => !ModdedDataFilesList.Contains(line)).ToArray();
                //задание имени целевой папки для новых модов
                string addedFilesFolderName = "[added]UseFiles_" + DateTimeInFormat;
                string DestFolderPath = Path.Combine(ModsPath, addedFilesFolderName);

                int addedFilesLength = addedFiles.Length;
                for (int f = 0; f < addedFilesLength; f++)
                {
                    string DestFileName = null;
                    try
                    {
                        //если zipmod guid присутствует в сохраненных, переместить его на место удаленного
                        string ext;
                        string guid;
                        if (ZipmodsGUIDListNotEmpty
                            && addedFiles[f].ToUpperInvariant().Contains("SIDELOADER MODPACK")
                            && ((ext = Path.GetExtension(addedFiles[f]).ToUpperInvariant()) == ".ZIPMOD" || ext == ".ZIP")
                            && !string.IsNullOrWhiteSpace(guid = ManageArchive.GetZipmodGUID(addedFiles[f]))
                            && ZipmodsGUIDList.ContainsKey(guid)
                            )
                        {
                            if (ZipmodsGUIDList[guid].Contains("%"))//temp check
                            {
                                ManageLogs.Log("zipmod contains %VAR%:" + ZipmodsGUIDList[guid]);
                            }

                            var zipmod = ReplaceVarsToPaths(ZipmodsGUIDList[guid]);

                            if (Path.GetFileName(addedFiles[f]) == Path.GetFileName(zipmod))//when zipmod has same name but moved
                            {
                                var targetfolder = zipmod.IsInOverwriteFolder() ?
                                    ManageSettings.GetOverwriteFolder() : ManageSettings.GetCurrentGameModsPath();
                                DestFileName = addedFiles[f].Replace(ManageSettings.GetCurrentGameDataPath(), targetfolder
                                    );
                            }
                            else//when mod was renamed
                            {
                                if (zipmod.IsInOverwriteFolder())//zipmod in overwrite
                                {
                                    var NewFilePath = addedFiles[f].Replace(ManageSettings.GetCurrentGameDataPath(), ManageSettings.GetOverwriteFolder());
                                    if (Directory.Exists(Path.GetDirectoryName(NewFilePath)) && NewFilePath != addedFiles[f])
                                    {
                                        DestFileName = NewFilePath;
                                    }
                                }
                                else//zipmod in Mods
                                {
                                    var ModPath = ManageMOMods.GetMOModPathInMods(zipmod);
                                    if (Path.GetFileName(ModPath).ToUpperInvariant() != "MODS" && Directory.Exists(ModPath))
                                    {
                                        DestFileName = addedFiles[f].Replace(ManageSettings.GetCurrentGameDataPath(), ModPath);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ManageLogs.Log("Error occured while to MO mode switch:" + Environment.NewLine + ex);
                    }

                    if (string.IsNullOrEmpty(DestFileName))
                    {
                        DestFileName = addedFiles[f].Replace(DataPath, DestFolderPath);
                    }
                    Directory.CreateDirectory(Path.GetDirectoryName(DestFileName));
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

                    ManageMO.ActivateDeactivateInsertMod(addedFilesFolderName);
                }

                //перемещение ванильных файлов назад в дата
                MoveVanillaFIlesBackToData();

                //очистка пустых папок в Data
                if (File.Exists(ManageSettings.GetVanillaDataEmptyFoldersListFilePath()))
                {
                    //удалить все, за исключением добавленных ранее путей до пустых папок
                    string[] VanillaDataEmptyFoldersList = File.ReadAllLines(ManageSettings.GetVanillaDataEmptyFoldersListFilePath());
                    ReplaceVarsToPaths(ref VanillaDataEmptyFoldersList);
                    ManageFilesFolders.DeleteEmptySubfolders(DataPath, false, VanillaDataEmptyFoldersList);
                }
                else
                {
                    ManageFilesFolders.DeleteEmptySubfolders(DataPath, false);
                }

                //restore sideloader mods from Mods\Mods
                var modsmods = Path.Combine(ManageSettings.GetCurrentGameModsPath(), "Mods");
                var targetmodpath = modsmods;
                if (false && Directory.Exists(modsmods)) //need to update overwrite check and scan only active mods or just move content to overwrite
                {
                    try//skip if was error
                    {
                        //scan Mods\Mods subdir
                        foreach (var dir in new DirectoryInfo(modsmods).EnumerateDirectories())
                        {
                            //parse only sideloader folders
                            if (!dir.Name.ToUpperInvariant().Contains("SIDELOADER MODPACK"))
                            {
                                continue;
                            }

                            //search target mod path where exists 'dir' folder
                            foreach (var moddir in Directory.EnumerateDirectories(ManageSettings.GetCurrentGameModsPath()))
                            {
                                //skip separators
                                if (moddir.EndsWith("_separator", StringComparison.InvariantCulture) || moddir == modsmods)
                                {
                                    continue;
                                }

                                //check if subfolder dir.name exists
                                if (Directory.Exists(Path.Combine(moddir, dir.Name)))
                                {
                                    targetmodpath = moddir;
                                    break;
                                }
                            }

                            //move files from 'dir' to target mod
                            if (targetmodpath != modsmods)//skip if target mod folder not changed
                                foreach (var file in Directory.EnumerateFiles(dir.FullName))
                                {
                                    var targetfilepath = file.Replace(dir.FullName, targetmodpath);
                                    if (!File.Exists(targetfilepath))//skip if exists target file path
                                    {
                                        File.Move(file, targetfilepath);
                                    }
                                }
                        }

                        //cleanup empty dirs
                        ManageFilesFolders.DeleteEmptySubfolders(modsmods, true);
                    }
                    catch (Exception ex)
                    {
                        ManageLogs.Log("error occured while Mods\\Mods file sorting. Error:\r\n" + ex);
                    }
                }

                //чистка файлов-списков
                File.Delete(ManageSettings.GetMOToStandartConvertationOperationsListFilePath());
                File.Delete(ManageSettings.GetVanillaDataFilesListFilePath());
                File.Delete(ManageSettings.GetVanillaDataEmptyFoldersListFilePath());
                File.Delete(ManageSettings.GetModdedDataFilesListFilePath());
                if (File.Exists(ManageSettings.GetZipmodsGUIDListFilePath()))
                {
                    File.Delete(ManageSettings.GetZipmodsGUIDListFilePath());
                }

                MOmode = true;

                MessageBox.Show(T._("Mod Organizer mode restored! All mod files moved back to Mods folder. If in Data folder was added new files they also moved in Mods folder as new mod, check and sort it if need"));

            }
            catch (Exception ex)
            {
                //обновление списка операций с файлами, для удаления уже выполненных и записи обновленного списка
                if (OperationsMade.ToString().Length > 0 && MOToStandartConvertationOperationsList != null && MOToStandartConvertationOperationsList.Length > 0)
                {
                    foreach (string OperationsMadeLine in OperationsMade.ToString().SplitToLines())
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(OperationsMadeLine))
                            {
                                continue;
                            }

                            MOToStandartConvertationOperationsList = MOToStandartConvertationOperationsList.Where(OperationsLine => OperationsLine != OperationsMadeLine).ToArray();
                        }
                        catch
                        {
                        }
                    }

                    File.WriteAllLines(ManageSettings.GetMOToStandartConvertationOperationsListFilePath(), MOToStandartConvertationOperationsList);
                }

                //recreate normal mode identifier if failed
                SwitchNormalModeIdentifier();

                MessageBox.Show("Failed to switch in MO mode. Error:" + Environment.NewLine + ex);
            }
        }

        /// <summary>
        /// normal mode identifier switcher
        /// </summary>
        /// <param name="Create">true=Create/false=Delete</param>
        private static void SwitchNormalModeIdentifier(bool Create = true)
        {
            if (Create)
            {
                if (!File.Exists(Path.Combine(ManageSettings.GetCurrentGameDataPath(), "normal.mode")))
                {
                    File.WriteAllText(Path.Combine(ManageSettings.GetCurrentGameDataPath(), "normal.mode"), "The game is in normal mode");
                }
            }
            else
            {
                if (File.Exists(Path.Combine(ManageSettings.GetCurrentGameDataPath(), "normal.mode")))
                {
                    File.Delete(Path.Combine(ManageSettings.GetCurrentGameDataPath(), "normal.mode"));
                }
            }
        }

        private void SwitchToCommonMode()
        {
            var EnabledModsList = ManageMO.GetModNamesListFromActiveMOProfile();

            if (EnabledModsList.Length == 0)
            {
                MessageBox.Show(T._("There is no enabled mods in Mod Organizer"));
                return;
            }

            //список выполненных операций с файлами.
            var MOToStandartConvertationOperationsList = new StringBuilder();
            //список пустых папок в data до ереноса файлов модов
            StringBuilder VanillaDataEmptyFoldersList = new StringBuilder();
            //список файлов в data без модов
            string[] VanillaDataFilesList;
            //список guid zipmod-ов
            Dictionary<string, string> ZipmodsGUIDList = new Dictionary<string, string>();
            List<string> LongPaths = new List<string>();


            var EnabledModsLength = EnabledModsList.Length;

            var debufStr = "";
            try
            {
                ManageMOMods.CleanBepInExLinksFromData();

                if (!ManageSettings.MOIsNew)
                {
                    if (File.Exists(ManageSettings.GetDummyFilePath()) && /*Удалил TESV.exe, который был лаунчером, а не болванкой*/new FileInfo(ManageSettings.GetDummyFilePath()).Length < 10000)
                    {
                        File.Delete(ManageSettings.GetDummyFilePath());
                    }
                }

                debufStr = ManageSettings.GetMOmodeDataFilesBakDirPath();
                Directory.CreateDirectory(ManageSettings.GetMOmodeDataFilesBakDirPath());
                MOToStandartConvertationOperationsList = new StringBuilder();

                VanillaDataEmptyFoldersList = ManageFilesFolders.GetEmptySubfoldersPaths(ManageSettings.GetCurrentGameDataPath(), new StringBuilder());

                //получение всех файлов из Data
                VanillaDataFilesList = Directory.GetFiles(DataPath, "*.*", SearchOption.AllDirectories);


                //получение всех файлов из папки Overwrite и их обработка
                var FilesInOverwrite = Directory.GetFiles(OverwriteFolder, "*.*", SearchOption.AllDirectories);
                if (FilesInOverwrite.Length > 0)
                {
                    //if (Path.GetFileName(FilesInOverwrite[0]).Contains("Overwrite"))
                    //{
                    //    OverwriteFolder = OverwriteFolder.Replace("overwrite", "Overwrite");
                    //}

                    string FileInDataFolder;
                    var FilesInOverwriteLength = FilesInOverwrite.Length;

                    using (var frmProgress = new Form())
                    {
                        frmProgress.Text = T._("Move files from Overwrite folder");
                        frmProgress.Size = new Size(200, 50);
                        frmProgress.StartPosition = FormStartPosition.CenterScreen;
                        frmProgress.FormBorderStyle = FormBorderStyle.FixedToolWindow;
                        using (var pbProgress = new ProgressBar())
                        {
                            pbProgress.Maximum = FilesInOverwriteLength;
                            pbProgress.Dock = DockStyle.Bottom;
                            frmProgress.Controls.Add(pbProgress);
                            frmProgress.Show();
                            for (int N = 0; N < FilesInOverwriteLength; N++)
                            {
                                if (N < pbProgress.Maximum)
                                {
                                    pbProgress.Value = N;
                                }

                                var FileInOverwrite = FilesInOverwrite[N];

                                if (ManageStrings.CheckForLongPath(ref FileInOverwrite))
                                {
                                    LongPaths.Add(FileInOverwrite.Remove(0, 4));
                                }
                                //if (FileInOverwrite.Length > 259)
                                //{
                                //    if (OfferToSkipTheFileConfirmed(FileInOverwrite))
                                //    {
                                //        continue;
                                //    }
                                //    //    FileInOverwrite = @"\\?\" + FileInOverwrite;
                                //}

                                FileInDataFolder = FileInOverwrite.Replace(OverwriteFolder, DataPath);
                                if (ManageStrings.CheckForLongPath(ref FileInDataFolder))
                                {
                                    LongPaths.Add(FileInDataFolder.Remove(0, 4));
                                }
                                //if (FileInDataFolder.Length > 259)
                                //{
                                //    FileInDataFolder = @"\\?\" + FileInDataFolder;
                                //}
                                if (File.Exists(FileInDataFolder))
                                {
                                    var FileInBakFolderWhichIsInRES = FileInDataFolder.Replace(DataPath, ManageSettings.GetMOmodeDataFilesBakDirPath());
                                    //if (FileInBakFolderWhichIsInRES.Length > 259)
                                    //{
                                    //    FileInBakFolderWhichIsInRES = @"\\?\" + FileInBakFolderWhichIsInRES;
                                    //}
                                    if (!File.Exists(FileInBakFolderWhichIsInRES) && VanillaDataFilesList.Contains(FileInDataFolder))
                                    {

                                        var bakfolder = Path.GetDirectoryName(FileInBakFolderWhichIsInRES);
                                        try
                                        {
                                            if (IsDebug)
                                                debufStr = bakfolder + ":bakfolder,l2043";
                                            Directory.CreateDirectory(bakfolder);

                                            File.Move(FileInDataFolder, FileInBakFolderWhichIsInRES);//перенос файла из Data в Bak, если там не было

                                            ManageMOMods.SaveGUIDIfZipMod(FileInOverwrite, ZipmodsGUIDList);

                                            File.Move(FileInOverwrite, FileInDataFolder);//перенос файла из папки Overwrite в Data
                                            MOToStandartConvertationOperationsList.AppendLine(FileInOverwrite + "|MovedTo|" + FileInDataFolder);//запись об операции будет пропущена, если будет какая-то ошибка

                                        }
                                        catch (Exception ex)
                                        {
                                            //когда файла в дата нет, файл в бак есть и есть файл в папке Overwrite - вернуть файл из bak назад
                                            if (!File.Exists(FileInDataFolder) && File.Exists(FileInBakFolderWhichIsInRES) && File.Exists(FileInOverwrite))
                                            {
                                                File.Move(FileInBakFolderWhichIsInRES, FileInDataFolder);
                                            }

                                            ManageLogs.Log("Error occured while to common mode switch:" + Environment.NewLine + ex + "\r\nparent dir=" + bakfolder + "\r\nData path=" + FileInDataFolder + "\r\nOverwrite path=" + FileInOverwrite);
                                        }
                                    }
                                }
                                else
                                {
                                    var destFolder = Path.GetDirectoryName(FileInDataFolder);
                                    try
                                    {
                                        if (IsDebug)
                                            debufStr = destFolder + ":destFolder,l2068";
                                        Directory.CreateDirectory(destFolder);

                                        ManageMOMods.SaveGUIDIfZipMod(FileInOverwrite, ZipmodsGUIDList);

                                        File.Move(FileInOverwrite, FileInDataFolder);//перенос файла из папки мода в Data
                                        MOToStandartConvertationOperationsList.AppendLine(FileInOverwrite + "|MovedTo|" + FileInDataFolder);//запись об операции будет пропущена, если будет какая-то ошибка
                                    }
                                    catch (Exception ex)
                                    {
                                        ManageLogs.Log("Error occured while to common mode switch:" + Environment.NewLine + ex + "\r\npath=" + destFolder + "\r\nData path=" + FileInDataFolder + "\r\nOverwrite path=" + FileInOverwrite);
                                    }
                                }
                            }
                        }
                    }
                }
                using (var frmProgress = new Form())
                {
                    frmProgress.Text = T._("Move files from Mods folder");
                    frmProgress.Size = new Size(200, 50);
                    frmProgress.StartPosition = FormStartPosition.CenterScreen;
                    frmProgress.FormBorderStyle = FormBorderStyle.FixedToolWindow;
                    using (var pbProgress = new ProgressBar())
                    {
                        pbProgress.Maximum = EnabledModsLength;
                        pbProgress.Dock = DockStyle.Bottom;
                        frmProgress.Controls.Add(pbProgress);
                        frmProgress.Show();
                        for (int N = 0; N < EnabledModsLength; N++)
                        {
                            if (N < pbProgress.Maximum)
                            {
                                pbProgress.Value = N;
                            }

                            var ModFolder = Path.Combine(ModsPath, EnabledModsList[N]);
                            if (ModFolder.Length > 0 && Directory.Exists(ModFolder))
                            {
                                var ModFiles = Directory.GetFiles(ModFolder, "*.*", SearchOption.AllDirectories);
                                if (ModFiles.Length > 0)
                                {
                                    var ModFilesLength = ModFiles.Length;
                                    string FileInDataFolder;

                                    var metaskipped = false;
                                    for (int f = 0; f < ModFilesLength; f++)
                                    {
                                        //"\\?\" - prefix to ignore 260 path cars limit

                                        var FileOfMod = ModFiles[f];
                                        if (ManageStrings.CheckForLongPath(ref FileOfMod))
                                        {
                                            LongPaths.Add(FileOfMod.Remove(0, 4));
                                        }
                                        //if (FileOfMod.Length > 259)
                                        //{
                                        //    if (OfferToSkipTheFileConfirmed(FileOfMod))
                                        //    {
                                        //        continue;
                                        //    }
                                        //    //FileOfMod = @"\\?\" + FileOfMod;
                                        //}

                                        try
                                        {
                                            //skip images and txt in mod root folder
                                            var FileExtension = Path.GetExtension(FileOfMod);
                                            if (FileExtension == ".txt" || FileExtension.IsPictureExtension())
                                            {
                                                if (Path.GetFileName(FileOfMod.Replace(Path.DirectorySeparatorChar + Path.GetFileName(FileOfMod), string.Empty)) == EnabledModsList[N])
                                                {
                                                    //пропускать картинки и txt в корне папки мода
                                                    continue;
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            ManageLogs.Log("error while image skip. error:" + ex);
                                        }

                                        //skip meta.ini
                                        if (metaskipped)
                                        {
                                        }
                                        else if (Path.GetFileName(FileOfMod) == "meta.ini" /*ModFile.Length >= 8 && string.Compare(ModFile.Substring(ModFile.Length - 8, 8), "meta.ini", true) == 0*/)
                                        {
                                            metaskipped = true;//для ускорения проверки, когда meta будет найден, будет делать быструю проверку bool переменной
                                            continue;
                                        }

                                        //MOCommonModeSwitchButton.Text = "..." + EnabledModsLength + "/" + N + ": " + f + "/" + ModFilesLength;
                                        FileInDataFolder = FileOfMod.Replace(ModFolder, DataPath);
                                        if (ManageStrings.CheckForLongPath(ref FileInDataFolder))
                                        {
                                            LongPaths.Add(FileInDataFolder.Remove(0, 4));
                                        }
                                        //if (FileInDataFolder.Length > 259)
                                        //{
                                        //    FileInDataFolder = @"\\?\" + FileInDataFolder;
                                        //}

                                        if (File.Exists(FileInDataFolder))
                                        {
                                            var FileInBakFolderWhichIsInRES = FileInDataFolder.Replace(DataPath, ManageSettings.GetMOmodeDataFilesBakDirPath());
                                            //if (FileInBakFolderWhichIsInRES.Length > 259)
                                            //{
                                            //    FileInBakFolderWhichIsInRES = @"\\?\" + FileInBakFolderWhichIsInRES;
                                            //}
                                            if (!File.Exists(FileInBakFolderWhichIsInRES) && VanillaDataFilesList.Contains(FileInDataFolder))
                                            {
                                                var bakfolder = Path.GetDirectoryName(FileInBakFolderWhichIsInRES);
                                                try
                                                {
                                                    if (IsDebug)
                                                        debufStr = bakfolder + ":bakfolder,l2183";
                                                    Directory.CreateDirectory(bakfolder);

                                                    File.Move(FileInDataFolder, FileInBakFolderWhichIsInRES);//перенос файла из Data в Bak, если там не было

                                                    ManageMOMods.SaveGUIDIfZipMod(FileOfMod, ZipmodsGUIDList);

                                                    File.Move(FileOfMod, FileInDataFolder);//перенос файла из папки мода в Data
                                                    MOToStandartConvertationOperationsList.AppendLine(FileOfMod + "|MovedTo|" + FileInDataFolder);//запись об операции будет пропущена, если будет какая-то ошибка
                                                }
                                                catch (Exception ex)
                                                {
                                                    //когда файла в дата нет, файл в бак есть и есть файл в папке мода - вернуть файл из bak назад
                                                    if (!File.Exists(FileInDataFolder) && File.Exists(FileInBakFolderWhichIsInRES) && File.Exists(FileOfMod))
                                                    {
                                                        File.Move(FileInBakFolderWhichIsInRES, FileInDataFolder);
                                                    }

                                                    ManageLogs.Log("Error occured while to common mode switch:" + Environment.NewLine + ex + "\r\npath=" + bakfolder + "\r\nData path=" + FileInDataFolder + "\r\nMods path=" + FileOfMod);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var destFolder = Path.GetDirectoryName(FileInDataFolder);
                                            try
                                            {
                                                if (IsDebug)
                                                    debufStr = destFolder + ":destFolder,l2208";
                                                Directory.CreateDirectory(destFolder);

                                                ManageMOMods.SaveGUIDIfZipMod(FileOfMod, ZipmodsGUIDList);

                                                File.Move(FileOfMod, FileInDataFolder);//перенос файла из папки мода в Data
                                                MOToStandartConvertationOperationsList.AppendLine(FileOfMod + "|MovedTo|" + FileInDataFolder);//запись об операции будет пропущена, если будет какая-то ошибка
                                            }
                                            catch (Exception ex)
                                            {
                                                ManageLogs.Log("Error occured while to common mode switch:" + Environment.NewLine + ex + "\r\npath=" + destFolder + "\r\nData path=" + FileInDataFolder + "\r\nMods path=" + FileOfMod);
                                            }
                                        }

                                        //MoveWithReplace(ModFile, DestFilePath[f]);
                                    }
                                    //Directory.Delete(ModFolder, true);
                                }
                            }
                        }
                    }
                }

                ReplacePathsToVars(ref MOToStandartConvertationOperationsList);
                File.WriteAllText(ManageSettings.GetMOToStandartConvertationOperationsListFilePath(), MOToStandartConvertationOperationsList.ToString());
                MOToStandartConvertationOperationsList.Clear();

                var DataWithModsFileslist = Directory.GetFiles(DataPath, "*.*", SearchOption.AllDirectories);
                ReplacePathsToVars(ref DataWithModsFileslist);
                File.WriteAllLines(ManageSettings.GetModdedDataFilesListFilePath(), DataWithModsFileslist);

                ReplacePathsToVars(ref VanillaDataFilesList);
                File.WriteAllLines(ManageSettings.GetVanillaDataFilesListFilePath(), VanillaDataFilesList);

                if (ZipmodsGUIDList.Count > 0)
                {
                    //using (var file = new StreamWriter(ManageSettings.GetZipmodsGUIDListFilePath()))
                    //{
                    //    foreach (var entry in ZipmodsGUIDList)
                    //    {
                    //        file.WriteLine("{0}{{ZIPMOD}}{1}", entry.Key, entry.Value);
                    //    }
                    //}
                    File.WriteAllLines(ManageSettings.GetZipmodsGUIDListFilePath(),
                        ZipmodsGUIDList.Select(x => x.Key + "{{ZIPMOD}}" + x.Value).ToArray());
                }
                DataWithModsFileslist = null;

                //create normal mode identifier
                SwitchNormalModeIdentifier();


                //записать пути до пустых папок, чтобы при восстановлении восстановить и их
                if (VanillaDataEmptyFoldersList.ToString().Length > 0)
                {
                    ReplacePathsToVars(ref VanillaDataEmptyFoldersList);
                    File.WriteAllText(ManageSettings.GetVanillaDataEmptyFoldersListFilePath(), VanillaDataEmptyFoldersList.ToString());
                }

                MOmode = false;

                MessageBox.Show(T._("All mod files now in Data folder! You can restore MO mode by same button."));
            }
            catch (Exception ex)
            {
                //восстановление файлов в первоначальные папки
                RestoreMovedFilesLocation(MOToStandartConvertationOperationsList);

                //clean empty folders except whose was already in Data
                ManageFilesFolders.DeleteEmptySubfolders(DataPath, false, VanillaDataEmptyFoldersList.ToString().SplitToLines().ToArray());

                //сообщить об ошибке
                MessageBox.Show("Mode was not switched. Error:" + Environment.NewLine + ex + "\r\n/debufStr=" + debufStr);
            }
        }

        /// <summary>
        /// replace variable to path in string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string ReplaceVarsToPaths(string str)
        {
            return str
            .Replace(ManageSettings.VarCurrentGameDataPath(), ManageSettings.GetCurrentGameDataPath())
            .Replace(ManageSettings.VarCurrentGameModsPath(), ManageSettings.GetCurrentGameModsPath())
            .Replace(ManageSettings.VarCurrentGameMOOverwritePath(), ManageSettings.GetCurrentGameMOOverwritePath());
        }

        /// <summary>
        /// replace path to variable in string array
        /// </summary>
        /// <param name="sarr"></param>
        private static void ReplacePathsToVars(ref string[] sarr)
        {
            for (int i = 0; i < sarr.Length; i++)
            {
                sarr[i] = sarr[i]
                .Replace(ManageSettings.GetCurrentGameDataPath(), ManageSettings.VarCurrentGameDataPath())
                .Replace(ManageSettings.GetCurrentGameModsPath(), ManageSettings.VarCurrentGameModsPath())
                .Replace(ManageSettings.GetCurrentGameMOOverwritePath(), ManageSettings.VarCurrentGameMOOverwritePath());
            }
        }

        /// <summary>
        /// replace variable to path in string array
        /// </summary>
        /// <param name="sarr"></param>
        private static void ReplaceVarsToPaths(ref string[] sarr)
        {
            for (int i = 0; i < sarr.Length; i++)
            {
                sarr[i] = sarr[i]
                .Replace(ManageSettings.VarCurrentGameDataPath(), ManageSettings.GetCurrentGameDataPath())
                .Replace(ManageSettings.VarCurrentGameModsPath(), ManageSettings.GetCurrentGameModsPath())
                .Replace(ManageSettings.VarCurrentGameMOOverwritePath(), ManageSettings.GetCurrentGameMOOverwritePath());
            }
        }

        /// <summary>
        /// replace path to variable in string builder
        /// </summary>
        /// <param name="sb"></param>
        private static void ReplacePathsToVars(ref StringBuilder sb)
        {
            sb = sb
                .Replace(ManageSettings.GetCurrentGameDataPath(), ManageSettings.VarCurrentGameDataPath())
                .Replace(ManageSettings.GetCurrentGameModsPath(), ManageSettings.VarCurrentGameModsPath())
                .Replace(ManageSettings.GetCurrentGameMOOverwritePath(), ManageSettings.VarCurrentGameMOOverwritePath());
        }

        /// <summary>
        /// replace variable to path in string builder
        /// </summary>
        /// <param name="sb"></param>
        private static void ReplaceVarsToPaths(ref StringBuilder sb)
        {
            sb = sb
                .Replace(ManageSettings.VarCurrentGameDataPath(), ManageSettings.GetCurrentGameDataPath())
                .Replace(ManageSettings.VarCurrentGameModsPath(), ManageSettings.GetCurrentGameModsPath())
                .Replace(ManageSettings.VarCurrentGameMOOverwritePath(), ManageSettings.GetCurrentGameMOOverwritePath());
        }

        private static bool OfferToSkipTheFileConfirmed(string file)
        {
            DialogResult result = MessageBox.Show(
                T._("Path to file is too long!") + Environment.NewLine
                + "(" + file + ")" + Environment.NewLine
                + T._("Long Path can cause mode switch error and mode will not be switched.") + Environment.NewLine
                + T._("Skip it?") + Environment.NewLine
                , T._("Too long file path"), MessageBoxButtons.YesNo);

            return result == DialogResult.Yes;
        }

        private void RestoreMovedFilesLocation(StringBuilder Operations)
        {
            if (Operations.Length > 0)
            {
                foreach (string record in Operations.ToString().SplitToLines())
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(record))
                        {
                            continue;
                        }

                        var MovePaths = record.Split(new string[] { "|MovedTo|" }, StringSplitOptions.None);

                        if (MovePaths.Length != 2)
                        {
                            continue;
                        }

                        var FilePathInMods = MovePaths[0];
                        var FilePathInData = MovePaths[1];

                        if (File.Exists(FilePathInData))
                        {
                            if (!File.Exists(FilePathInMods))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(FilePathInMods));

                                File.Move(FilePathInData, FilePathInMods);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ManageLogs.Log("error while RestoreMovedFilesLocation. error:\r\n" + ex);
                    }
                }

                //возврат возможных ванильных резервных копий
                MoveVanillaFIlesBackToData();
            }
        }

        private static void MoveVanillaFIlesBackToData()
        {
            var MOmodeDataFilesBakDirPath = ManageSettings.GetMOmodeDataFilesBakDirPath();
            if (Directory.Exists(MOmodeDataFilesBakDirPath))
            {
                var FilesInMOmodeDataFilesBak = Directory.GetFiles(MOmodeDataFilesBakDirPath, "*.*", SearchOption.AllDirectories);
                int FilesInMOmodeDataFilesBakLength = FilesInMOmodeDataFilesBak.Length;
                for (int f = 0; f < FilesInMOmodeDataFilesBakLength; f++)
                {
                    if (string.IsNullOrWhiteSpace(FilesInMOmodeDataFilesBak[f]))
                    {
                        continue;
                    }

                    var DestFileInDataFolderPath = FilesInMOmodeDataFilesBak[f].Replace(MOmodeDataFilesBakDirPath, DataPath);
                    if (!File.Exists(DestFileInDataFolderPath))
                    {
                        var DestFileInDataFolderPathFolder = Path.GetDirectoryName(DestFileInDataFolderPath);
                        if (!Directory.Exists(DestFileInDataFolderPathFolder))
                        {
                            Directory.CreateDirectory(DestFileInDataFolderPathFolder);
                        }
                        File.Move(FilesInMOmodeDataFilesBak[f], DestFileInDataFolderPath);
                    }
                }

                //удаление папки, где хранились резервные копии ванильных файлов
                ManageFilesFolders.DeleteEmptySubfolders(MOmodeDataFilesBakDirPath);
            }
        }

        private void OpenGameFolderLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Directory.Exists(DataPath))
                Process.Start("explorer.exe", DataPath);
        }

        private void OpenMOFolderLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Directory.Exists(MODirPath))
                Process.Start("explorer.exe", MODirPath);
        }

        private void OpenModsFolderLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Directory.Exists(ModsPath))
                Process.Start("explorer.exe", ModsPath);
        }

        private void Install2MODirPathOpenFolderLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!Directory.Exists(Install2MODirPath))
            {
                Directory.CreateDirectory(Install2MODirPath);
            }
            Process.Start("explorer.exe", Install2MODirPath);
        }

        private void OpenMyUserDataFolderLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string UserFilesFolder = Path.Combine(ModsPath, "MyUserData");
            if (!Directory.Exists(UserFilesFolder))
            {
                UserFilesFolder = Path.Combine(ModsPath, "MyUserFiles");
            }
            if (!Directory.Exists(UserFilesFolder))
            {
                UserFilesFolder = ManageSettings.GetOverwriteFolder();
            }
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
            ManageMOMods.OpenBepinexLog();
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

                new INIFile(ManageSettings.GetAIHelperINIPath()).WriteINI("Settings", "selected_game", ManageSettings.GetCurrentGameFolderName());

                FoldersInit();

                Properties.Settings.Default.CurrentGameIsChanging = false;
            }
            else
            {
                CurrentGameComboBox.SelectedItem = Properties.Settings.Default.CurrentGameFolderName;
            }
        }

        private void ActionsOnGameChanged()
        {
            CloseExtraForms();
            //cleaning previous game data
            //File.Delete(ManageSettings.GetModOrganizerINIpath());
            //File.Delete(ManageSettings.GetMOcategoriesPath());
            ManageMO.RedefineGameMOData();
            Properties.Settings.Default.BepinExCfgPath = string.Empty;
            Properties.Settings.Default.MOSelectedProfileDirName = string.Empty;

            ManageMO.CheckBaseGamesPy();

            CurrentGame.InitActions();
            CurrentGameTitleTextBox.Text = CurrentGame.GetGameDisplayingName();
            Properties.Settings.Default.CurrentGameDisplayingName = CurrentGame.GetGameDisplayingName();
        }

        private void CloseExtraForms()
        {
            if (LinksForm != null && !LinksForm.IsDisposed)
            {
                LinksForm.Close();
            }
            if (extraSettingsForm != null && !extraSettingsForm.IsDisposed)
            {
                extraSettingsForm.Close();
            }
        }

        private void ConsoleCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            //ManageINI.WriteINIValue(ManageSettings.GetBepInExCfgFilePath(), "Logging.Console", "Enabled", /*" " +*/ (sender as CheckBox).Checked.ToString(CultureInfo.InvariantCulture));
            var BepinExcfg = ManageSettings.GetBepInExCfgFilePath();
            ManageCFG.WriteCFGValue(BepinExcfg, "Logging.Console", "Enabled", /*" " +*/ (sender as CheckBox).Checked.ToString(CultureInfo.InvariantCulture));

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
                //extraSettingsForm.Location = new Point(Bounds.Location.X + (Bounds.Width / 2) - (extraSettingsForm.Width / 2),
                //         Bounds.Location.Y + /*(Bounds.Height / 2) - (f2.Height / 2) +*/ Bounds.Height);
                extraSettingsForm.TopMost = true;
            }
            else
            {
                //newformButton.Text = @"\/";
                extraSettingsForm.Close();
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
                RunProgram(MOexePath, "moshortcut://:" + ManageSettings.GetINISettingsEXEName());
            }
            else
            {
                RunProgram(Path.Combine(DataPath, ManageSettings.GetINISettingsEXEName() + ".exe"), string.Empty);
            }
            OnOffButtons();
        }

        private void AI_Helper_FormClosing(object sender, FormClosingEventArgs e)
        {
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
            if (!Properties.Settings.Default.MOmode)
            {
                DialogResult result = MessageBox.Show(T._("Attention") + "\n\n" + T._("Correct modlist fixes possible only in MO mode") + "\n\n" + T._("Switch to MO mode?"), T._("Confirmation"), MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    SwitchBetweenMOAndStandartModes();
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

        private async void btnUpdateMods_Click(object sender, EventArgs e)
        {
            AIGirlHelperTabControl.Enabled = false;

            //update plugins in mo mode
            if (MOmode)
            {
                await new Update().update().ConfigureAwait(true);
            }

            //run zipmod's check if updater found and only for KK, AI, HS2
            if (CurrentGame.isHaveSideloaderMods && File.Exists(ManageSettings.KKManagerStandaloneUpdaterEXEPath()))
            {
                if (MOmode)
                {
                    ManageMO.CleanMOFolder();
                    //
                    ManageMO.CheckBaseGamesPy();

                    ManageMO.RedefineGameMOData();

                    //add updater as new exe in mo list if not exists
                    if (!ManageMO.IsMOcustomExecutableTitleByExeNameExists("StandaloneUpdater"))
                    {
                        ManageMO.InsertCustomExecutable(new string[]
                        {
                            "KKManagerStandaloneUpdater",
                            ManageSettings.KKManagerStandaloneUpdaterEXEPath(),
                            ManageSettings.GetCurrentGameDataPath()
                        });
                    }

                    var ZipmodsGUIDList = new Dictionary<string, string>();

                    //activate all mods with Sideloader modpack inside
                    ActivateSideloaderMods(ZipmodsGUIDList);

                    RunProgram(MOexePath, "moshortcut://:" + ManageMO.GetMOcustomExecutableTitleByExeName("StandaloneUpdater"));

                    //restore modlist
                    ManageMO.RestoreModlist();

                    //restore zipmods to source mods
                    MoveZipModsFromOverwriteToSourceMod(ZipmodsGUIDList);
                }
                else
                {
                    //run updater normal
                    RunProgram(ManageSettings.KKManagerStandaloneUpdaterEXEPath(), "\"" + ManageSettings.GetCurrentGameDataPath() + "\"");
                }
            }

            AIGirlHelperTabControl.Enabled = true;
        }

        private static void MoveZipModsFromOverwriteToSourceMod(Dictionary<string, string> zipmodsGUIDList)
        {
            if (!Directory.Exists(Path.Combine(ManageSettings.GetOverwriteFolder(), "mods")))
            {
                return;
            }

            //var modpackFilters = new Dictionary<string, string>
            //{
            //    { "Sideloader Modpack - KK_UncensorSelector", @"^\[KK\]\[Female\].*$" }//sideloader dir name /files regex filter
            //};

            var dirs = Directory.GetDirectories(Path.Combine(ManageSettings.GetOverwriteFolder(), "mods")).Where(dirpath => dirpath.ToUpperInvariant().Contains("SIDELOADER MODPACK"));

            if (!dirs.Any())
            {
                return;
            }

            var ProgressForm = new Form
            {
                StartPosition = FormStartPosition.CenterScreen,
                Size = new Size(400, 50),
                Text = T._("Sideloader dirs sorting") + "..",
                FormBorderStyle = FormBorderStyle.FixedToolWindow
            };
            var PBar = new ProgressBar
            {
                Dock = DockStyle.Bottom
            };

            ProgressForm.Controls.Add(PBar);
            ProgressForm.Show();

            var modpacks = ManageMOMods.GetSideloaderModpackTargetDirs();

            PBar.Maximum = dirs.Count();
            PBar.Value = 0;

            var TimeInfo = DateTime.Now.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture);

            foreach (var dir in dirs)
            {
                //try
                {
                    if (PBar.Value < PBar.Maximum)
                    {
                        PBar.Value++;
                    }

                    //if (!dir.ToUpperInvariant().Contains("SIDELOADER MODPACK"))
                    //{
                    //    continue;
                    //}

                    //set sideloader dir name
                    var sideloadername = Path.GetFileName(dir);

                    ProgressForm.Text = T._("Sorting") + ":" + sideloadername;

                    var IsUnc = ManageMOMods.IsUncensorSelector(sideloadername);
                    var IsMaleUnc = IsUnc && modpacks.ContainsKey(sideloadername + "M");
                    var IsFeMaleUnc = IsUnc && modpacks.ContainsKey(sideloadername + "F");
                    var IsSortingModPack = modpacks.ContainsKey(sideloadername) || IsMaleUnc || IsFeMaleUnc;
                    foreach (var f in Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories))
                    {
                        var file = f;

                        // Check if TargetIsInSideloader by guid
                        var guid = ManageArchive.GetZipmodGUID(file);
                        bool Isguid = guid.Length > 0 && zipmodsGUIDList.ContainsKey(guid);
                        string TargetModPath = Isguid ? ManageMOMods.GetMOModPathInMods(zipmodsGUIDList[guid]) : "";
                        var pathElements = !string.IsNullOrWhiteSpace(TargetModPath) ? file.Replace(TargetModPath, "").Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries) : null;
                        var TargetzipModDirName = pathElements != null && pathElements.Length > 1 ? pathElements[1] : ""; // %modpath%\mods\%sideloadermodpackdir%
                        var TargetIsInSideloader = TargetzipModDirName.ToUpperInvariant().Contains("SIDELOADER MODPACK"); // dir in mods is sideloader

                        if (Isguid && !TargetIsInSideloader/*do not touch sideloader files and let them be updated properly*/)//move by guid
                        {
                            var target = file.Replace(ManageSettings.GetOverwriteFolder(), TargetModPath);


                            bool IsTargetExists = File.Exists(target);
                            bool IsSourceExists = File.Exists(file);

                            ManageStrings.CheckForLongPath(ref file);
                            ManageStrings.CheckForLongPath(ref target);

                            if (!IsSourceExists)
                            {
                                continue;
                            }
                            else if (IsTargetExists && IsSourceExists)
                            {
                                var targetinfo = new FileInfo(target);
                                var sourceinfo = new FileInfo(file);
                                if (targetinfo.Length == sourceinfo.Length && targetinfo.GetCrc32() == sourceinfo.GetCrc32())
                                {
                                    sourceinfo.Delete();
                                    continue;
                                }
                                else if (targetinfo.LastWriteTime < sourceinfo.LastWriteTime)
                                {
                                    var tfiletarget = file.Replace(ManageSettings.GetOverwriteFolder(), TargetModPath + "_" + TimeInfo);

                                    Directory.CreateDirectory(Path.GetDirectoryName(tfiletarget));
                                    if (!File.Exists(tfiletarget))
                                        File.Move(target, tfiletarget); // move older target file
                                }
                                else
                                {
                                    target = file.Replace(ManageSettings.GetOverwriteFolder(), TargetModPath + "_" + TimeInfo);
                                }
                            }

                            Directory.CreateDirectory(Path.GetDirectoryName(target));
                            try
                            {
                                File.Move(file, target);
                            }
                            catch (IOException ex)
                            {
                                ManageLogs.Log("An error occured while file move. error:\r\n" + ex + "\r\nfile=" + file + "\r\ntarget file=" + target);
                            }
                        }
                        else if (IsSortingModPack)
                        {
                            var sortM = IsMaleUnc && Path.GetExtension(file).StartsWith(".zip", StringComparison.CurrentCultureIgnoreCase) && (Path.GetFileNameWithoutExtension(file).Contains("[Penis]") || Path.GetFileNameWithoutExtension(file).Contains("[Balls]"));
                            var sortF = IsFeMaleUnc && Path.GetExtension(file).StartsWith(".zip", StringComparison.CurrentCultureIgnoreCase) && Path.GetFileNameWithoutExtension(file).Contains("[Female]");

                            var TargetModName = modpacks[sideloadername + (sortF ? "F" : sortM ? "M" : "")];

                            //get target path for the zipmod
                            var target = ManageSettings.GetCurrentGameModsPath()
                                + Path.DirectorySeparatorChar + TargetModName //mod name
                                + Path.DirectorySeparatorChar + "mods" //mods dir
                                + Path.DirectorySeparatorChar + sideloadername // sideloader dir name
                                + file.Replace(dir, ""); // file subpath in sideloader dir

                            bool IsTargetExists = File.Exists(target);
                            bool IsSourceExists = File.Exists(file);

                            ManageStrings.CheckForLongPath(ref file);
                            ManageStrings.CheckForLongPath(ref target);

                            if (!IsSourceExists)
                            {
                                continue;
                            }
                            else if (IsTargetExists && IsSourceExists)
                            {
                                var targetinfo = new FileInfo(target);
                                var sourceinfo = new FileInfo(file);
                                if (targetinfo.Length == sourceinfo.Length && targetinfo.GetCrc32() == sourceinfo.GetCrc32())
                                {
                                    sourceinfo.Delete();
                                    continue;
                                }
                                else if (targetinfo.LastWriteTime < sourceinfo.LastWriteTime)
                                {
                                    var tfiletarget = ManageSettings.GetCurrentGameModsPath()
                                        + Path.DirectorySeparatorChar + TargetModName + "_" + TimeInfo //mod name
                                        + Path.DirectorySeparatorChar + "mods" //mods dir
                                        + Path.DirectorySeparatorChar + sideloadername // sideloader dir name
                                        + file.Replace(dir, ""); // file subpath in sideloader dir

                                    Directory.CreateDirectory(Path.GetDirectoryName(tfiletarget));
                                    if (!File.Exists(tfiletarget))
                                        File.Move(target, tfiletarget); // move older target file
                                }
                                else
                                {
                                    target = ManageSettings.GetCurrentGameModsPath()
                                        + Path.DirectorySeparatorChar + TargetModName + "_" + TimeInfo //mod name
                                        + Path.DirectorySeparatorChar + "mods" //mods dir
                                        + Path.DirectorySeparatorChar + sideloadername // sideloader dir name
                                        + file.Replace(dir, ""); // file subpath in sideloader dir
                                }
                            }


                            Directory.CreateDirectory(Path.GetDirectoryName(target));//create parent dir

                            try
                            {
                                File.Move(file, target);//move file to the marked mod
                            }
                            catch (IOException ex)
                            {
                                ManageLogs.Log("An error occured while file move. error:\r\n" + ex + "\r\nfile=" + file + "\r\ntarget file=" + target);
                            }
                        }
                    }
                }
                //catch (Exception ex)
                //{
                //    ManageLogs.Log("An error occured while zipmods sort from overwrite dir. error:\r\n" + ex);
                //}

            }

            //clean empty dirs
            ManageFilesFolders.DeleteEmptySubfolders(ManageSettings.GetOverwriteFolder(), false);

            ProgressForm.Dispose();
        }

        /// <summary>
        /// activate all mods with sideloader modpack inside mods folder
        /// </summary>
        private static void ActivateSideloaderMods(Dictionary<string, string> ZipmodsGUIDList = null)
        {
            bool getZipmodID = ZipmodsGUIDList != null;

            File.Copy(ManageSettings.CurrentMOProfileModlistPath(), ManageSettings.CurrentMOProfileModlistPath() + ".prezipmodsUpdate");

            var modlistContent = File.ReadAllLines(ManageSettings.CurrentMOProfileModlistPath());

            for (int i = 0; i < modlistContent.Length; i++)
            {
                if (modlistContent[i][0] == '+' || modlistContent[i].EndsWith("_separator", StringComparison.InvariantCulture))
                {
                    continue;
                }

                var modName = modlistContent[i].Remove(0, 1);
                var modsPath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), modName, "mods");
                if (!Directory.Exists(modsPath))
                {
                    continue;
                }

                foreach (var dir in Directory.EnumerateDirectories(modsPath))
                {
                    if (dir.ToUpperInvariant().Contains("SIDELOADER MODPACK"))
                    {
                        if (modlistContent[i][0] != '+')
                        {
                            modlistContent[i] = "+" + modName;
                        }

                        if (getZipmodID)
                        {
                            foreach (var zipmod in Directory.EnumerateFiles(dir, "*.zip*", SearchOption.AllDirectories))
                            {
                                ManageMOMods.SaveGUIDIfZipMod(zipmod, ZipmodsGUIDList);
                            }
                        }
                        else
                        {
                            break;
                        }

                    }
                }
            }

            File.WriteAllLines(ManageSettings.CurrentMOProfileModlistPath(), modlistContent);
        }

        private void PbDiscord_Click(object sender, EventArgs e)
        {
            Process.Start("https://discord.gg/rKbXzrnrMs");//Program's discord server
        }

        private void linkLabel1_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var ModUpdatesBakDir = ManageSettings.GetUpdatedModsOlderVersionsBuckupDirPath();
            if (!Directory.Exists(ModUpdatesBakDir))
            {
                Directory.CreateDirectory(ModUpdatesBakDir);
            }
            Process.Start("explorer.exe", ModUpdatesBakDir);
        }

        private void AIGirlHelperTabControl_Selected(object sender, TabControlEventArgs e)
        {
            //FoldersInit();

            if (AIGirlHelperTabControl.SelectedTab.Name == "ToolsTabPage")
            {
                //check bleeding edge txt
                if (!File.Exists(ManageSettings.KKManagerStandaloneUpdaterEXEPath()))
                {
                    cbxBleadingEdgeZipmods.Visible = false;
                    cbxBleadingEdgeZipmods.Checked = false;
                }
                else if (File.Exists(ManageSettings.ZipmodsBleedingEdgeMarkFilePath()))
                {
                    cbxBleadingEdgeZipmods.Visible = true;
                    cbxBleadingEdgeZipmods.Checked = true;
                }
                else
                {
                    //do not hide checkbox if game can have it
                    if (CurrentGame.isHaveSideloaderMods)
                    {
                        cbxBleadingEdgeZipmods.Visible = true;
                    }
                    else
                    {
                        cbxBleadingEdgeZipmods.Visible = false;
                    }

                    cbxBleadingEdgeZipmods.Checked = false;
                }
            }
            else if (AIGirlHelperTabControl.SelectedTab.Name == "LaunchTabPage")
            {
                //newformButton.Text = @"\/";

                GetEnableDisableLaunchTabButtons();
                //set bepinex log cfg
                //BepInExDisplayedLogLevelLabel.Visible = BepInExConsoleCheckBox.Checked = ManageCFG.GetCFGValueIfExist(ManageSettings.GetBepInExCfgFilePath(), "Enabled", "Logging.Console", "").ToUpperInvariant() == "TRUE";
            }
        }

        private void cbxBleadingEdgeZipmods_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as CheckBox).Checked && !File.Exists(ManageSettings.ZipmodsBleedingEdgeMarkFilePath()))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ManageSettings.ZipmodsBleedingEdgeMarkFilePath()));
                File.WriteAllText(ManageSettings.ZipmodsBleedingEdgeMarkFilePath(), string.Empty);
            }
            else if (!(sender as CheckBox).Checked && File.Exists(ManageSettings.ZipmodsBleedingEdgeMarkFilePath()))
            {
                File.Delete(ManageSettings.ZipmodsBleedingEdgeMarkFilePath());
            }
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