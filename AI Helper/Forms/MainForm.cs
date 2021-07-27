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
    public partial class MainForm : Form
    {
        private bool _compressmode;

        //constants
        private static string AppResDir { get => Properties.Settings.Default.AppResDir; set => Properties.Settings.Default.AppResDir = value; }

        private static string ModsPath { get => Properties.Settings.Default.ModsPath; set => Properties.Settings.Default.ModsPath = value; }
        private static string DownloadsPath { get => Properties.Settings.Default.DownloadsPath; set => Properties.Settings.Default.DownloadsPath = value; }
        private static string DataPath { get => ManageSettings.GetCurrentGameDataPath(); /*set => Properties.Settings.Default.DataPath = value;*/ }
        private static string MoDirPath { get => Properties.Settings.Default.MODirPath; set => Properties.Settings.Default.MODirPath = value; }
        private static string MOexePath { get => Properties.Settings.Default.MOexePath; set => Properties.Settings.Default.MOexePath = value; }
        private static string OverwriteFolder { get => Properties.Settings.Default.OverwriteFolder; set => Properties.Settings.Default.OverwriteFolder = value; }
        private static string OverwriteFolderLink { get => Properties.Settings.Default.OverwriteFolderLink; set => Properties.Settings.Default.OverwriteFolderLink = value; }
        private static string SetupXmlPath { get => Properties.Settings.Default.SetupXmlPath; set => Properties.Settings.Default.SetupXmlPath = value; }
        private static string ApplicationStartupPath { /*get => Properties.Settings.Default.ApplicationStartupPath; */set => Properties.Settings.Default.ApplicationStartupPath = value; }

        //private static string ModOrganizerINIpath { get => Properties.Settings.Default.ModOrganizerINIpath; set => Properties.Settings.Default.ModOrganizerINIpath = value; }
        private static string Install2MoDirPath { get => Properties.Settings.Default.Install2MODirPath; set => Properties.Settings.Default.Install2MODirPath = value; }

        private static bool MOmode { get => ManageSettings.IsMoMode(); set => Properties.Settings.Default.MOmode = value; }

        private static Game CurrentGame { get => GameData.CurrentGame; set => GameData.CurrentGame = value; }
        private static List<Game> ListOfGames { get => GameData.ListOfGames; set => GameData.ListOfGames = value; }

        public MainForm()
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

            CheckMoAndEndInit();
        }

        private async void CheckMoAndEndInit()
        {
            //MO data parse
            Properties.Settings.Default.MOIsNew = ManageModOrganizer.IsMo23OrNever();

            if (!File.Exists(ManageSettings.GetMOexePath()))
            {
                await new Updater().Update().ConfigureAwait(true);
            }

            ManageModOrganizer.RedefineGameMoData();

            ManageModOrganizer.CleanMoFolder();
            //
            ManageModOrganizer.CheckBaseGamesPy();

            CleanLog();

            VariablesInit();
            SetMoMode(false);
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
                catch (Exception ex)
                {
                    ManageLogs.Log("An error occured whil tried to CleanLog. error:" + ex);
                }
            }
        }

        private static void VariablesInit()
        {
            Properties.Settings.Default.CurrentGamePath = CurrentGame.GetGamePath();
            //SettingsManage.SettingsINIT();
            AppResDir = ManageSettings.GetAppResDir();
            ModsPath = ManageSettings.GetCurrentGameModsPath();
            DownloadsPath = ManageSettings.GetDownloadsPath();
            //DataPath = ManageSettings.GetDataPath();
            MoDirPath = ManageSettings.GetMOdirPath();
            MOexePath = ManageSettings.GetMOexePath();
            Properties.Settings.Default.ModOrganizerINIpath = ManageSettings.GetModOrganizerInIpath();
            Install2MoDirPath = ManageSettings.GetInstall2MoDirPath();
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
                        new INIFile(ManageSettings.GetAiHelperIniPath()).ReadINI("Settings", "selected_game")
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
                ManageLogs.Log("An error occured while SetListOfGames. error:\r\n" + ex);
                return false;
            }


            return true;
        }

        private void SetSelectedGameIndexAndBasicVariables(int index = 0)
        {
            Properties.Settings.Default.CurrentGameListIndex = index;
            CurrentGame = ListOfGames[Properties.Settings.Default.CurrentGameListIndex];
            CurrentGameComboBox.SelectedIndex = index;

            Properties.Settings.Default.CurrentGameEXEName = CurrentGame.GetGameExeName();
            Properties.Settings.Default.CurrentGameFolderName = CurrentGame.GetGameFolderName();
            Properties.Settings.Default.StudioEXEName = CurrentGame.GetGameStudioExeName();
            Properties.Settings.Default.INISettingsEXEName = CurrentGame.GetIniSettingsExeName();
            Properties.Settings.Default.CurrentGamePath = CurrentGame.GetGamePath();
            //Properties.Settings.Default.DataPath = Path.Combine(CurrentGame.GetGamePath(), "Data");
            Properties.Settings.Default.ModsPath = Path.Combine(CurrentGame.GetGamePath(), "Mods");

            //set checkbox
            Properties.Settings.Default.AutoShortcutRegistryCheckBoxChecked = bool.Parse(ManageIni.GetIniValueIfExist(ManageSettings.GetAiHelperIniPath(), "autoCreateShortcutAndFixRegystry", "Settings", "False"));
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

        private int _mode;

        private void MainService_Click(object sender, EventArgs e)
        {
            //MainService.Enabled = false;

            _mode = GetModeValue();

            switch (_mode)
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
                _compressmode = true;
            }
            else
            {
                _compressmode = false;
            }
            return 0;
        }

        private async void ExtractingMode()
        {
            MainService.Text = T._("Extracting") + "..";
            MainService.Enabled = false;

            //https://ru.stackoverflow.com/questions/222414/%d0%9a%d0%b0%d0%ba-%d0%bf%d1%80%d0%b0%d0%b2%d0%b8%d0%bb%d1%8c%d0%bd%d0%be-%d0%b2%d1%8b%d0%bf%d0%be%d0%bb%d0%bd%d0%b8%d1%82%d1%8c-%d0%bc%d0%b5%d1%82%d0%be%d0%b4-%d0%b2-%d0%be%d1%82%d0%b4%d0%b5%d0%bb%d1%8c%d0%bd%d0%be%d0%bc-%d0%bf%d0%be%d1%82%d0%be%d0%ba%d0%b5
            await Task.Run(() => UnpackGame()).ConfigureAwait(true);
            await Task.Run(() => UnpackMo()).ConfigureAwait(true);
            await Task.Run(() => UnpackMods()).ConfigureAwait(true);

            //BepinExLoadingFix();//добавлено в folderinit

            ManageOther.CreateShortcuts();

            ManageModOrganizer.DummyFiles();

            MainService.Text = T._("Game Ready");
            FoldersInit();
            MainService.Enabled = true;
        }

        private void UnpackMo()
        {
            if (MOmode)
            {
                string mo7Zip = Path.Combine(AppResDir, "MO.7z");
                if (File.Exists(mo7Zip) && !File.Exists(Path.Combine(MoDirPath, "ModOrganizer.exe")))
                {
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Visible = true));
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Marquee));
                    _ = DataInfoLabel.Invoke((Action)(() => DataInfoLabel.Text = T._("Extracting")));
                    _ = ModsInfoLabel.Invoke((Action)(() => ModsInfoLabel.Text = T._("MO archive") + ": " + Path.GetFileNameWithoutExtension(mo7Zip)));
                    Compressor.Decompress(mo7Zip, MoDirPath);
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Blocks));
                }
            }
        }

        private void UnpackGame()
        {
            if (Directory.Exists(DataPath))
            {
                string aiGirlTrial = Path.Combine(AppResDir, "AIGirlTrial.7z");
                string aiGirl = Path.Combine(AppResDir, "AIGirl.7z");
                if (File.Exists(aiGirlTrial) && !File.Exists(Path.Combine(DataPath, "AI-SyoujyoTrial.exe")))
                {
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Visible = true));
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Marquee));
                    _ = DataInfoLabel.Invoke((Action)(() => DataInfoLabel.Text = T._("Extracting")));
                    _ = ModsInfoLabel.Invoke((Action)(() => ModsInfoLabel.Text = T._("Game archive") + ": " + Path.GetFileNameWithoutExtension(aiGirlTrial)));
                    Compressor.Decompress(aiGirlTrial, DataPath);
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Blocks));
                }
                else if (File.Exists(aiGirl) && !File.Exists(Path.Combine(DataPath, "AI-Syoujyo.exe")))
                {
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Visible = true));
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Marquee));
                    _ = DataInfoLabel.Invoke((Action)(() => DataInfoLabel.Text = T._("Extracting")));
                    _ = ModsInfoLabel.Invoke((Action)(() => ModsInfoLabel.Text = T._("Game archive") + ": " + Path.GetFileNameWithoutExtension(aiGirl)));
                    Compressor.Decompress(aiGirl, DataPath);
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
                    string[] modDirs = Directory.GetDirectories(ModsPath, "*").Where(name => !name.EndsWith("_separator", StringComparison.OrdinalIgnoreCase)).ToArray();
                    string[] files = Directory.GetFiles(DownloadsPath, "*.7z", SearchOption.AllDirectories).Where(name => !modDirs.Contains(Path.Combine(ModsPath, Path.GetFileNameWithoutExtension(name)))).ToArray();
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
            if (_compressmode)
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

        private void PackMo()
        {
            if (Directory.Exists(MoDirPath) && Directory.Exists(AppResDir))
            {
                _ = progressBar1.Invoke((Action)(() => progressBar1.Visible = true));
                _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Marquee));
                _ = DataInfoLabel.Invoke((Action)(() => DataInfoLabel.Text = "Compressing"));
                _ = ModsInfoLabel.Invoke((Action)(() => ModsInfoLabel.Text = "MO archive.."));
                Compressor.Compress(MoDirPath, AppResDir);
                _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Blocks));
            }
        }

        private void PackGame()
        {
            if (Directory.Exists(DataPath) && Directory.Exists(AppResDir))
            {
                string aiGirlTrial = Path.Combine(AppResDir, "AIGirlTrial.7z");
                string aiGirl = Path.Combine(AppResDir, "AIGirl.7z");
                if (!File.Exists(aiGirlTrial)
                    && (File.Exists(Path.Combine(DataPath, ManageSettings.GetCurrentGameExeName() + ".exe"))))
                {
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Visible = true));
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Marquee));
                    _ = DataInfoLabel.Invoke((Action)(() => DataInfoLabel.Text = "Compressing"));
                    _ = ModsInfoLabel.Invoke((Action)(() => ModsInfoLabel.Text = "Game archive: " + Path.GetFileNameWithoutExtension(aiGirlTrial)));
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

            string categoryvalue = ManageModOrganizer.GetMetaParameterValue(Path.Combine(inputmoddir, "meta.ini"), "category").Replace("\"", string.Empty);
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
                        int parentIDindex = int.Parse(categories[categiryindex].ParentId, CultureInfo.InvariantCulture) - 1;//В List индекс идет от нуля
                        if (parentIDindex > 0 && parentIDindex < categories.Count)
                        {
                            targetdir = Path.Combine(targetdir, categories[parentIDindex].Name);
                        }

                        targetdir = Path.Combine(targetdir, categories[categiryindex].Name);

                        Directory.CreateDirectory(targetdir);
                    }
                }
            }

            return targetdir;
        }

        ToolTip _thToolTip;
        private void SetTooltips()
        {
            try
            {
                if (_thToolTip != null)
                {
                    _thToolTip.RemoveAll();
                }
            }
            catch (Exception ex)
            {
                ManageLogs.Log("An error occured while SetTooltips. error:\r\n" + ex);
            }

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

            _thToolTip.SetToolTip(ProgramNameLabelPart2, Properties.Settings.Default.ApplicationProductName + " - " + T._("Illusion games manager.\n\n"
                    + "Move mouse over wished button or text to see info about it"
                    )
                );
            _thToolTip.SetToolTip(SelectedGameLabel, T._("Selected game title"));

            //Launch
            _thToolTip.SetToolTip(VRGameCheckBox, T._("Check to run VR exe instead on standart"));

            //Main
            //THToolTip.SetToolTip(button1, T._("Unpacking mods and resources from 'Downloads' and 'RES' folders for game when they are not installed"));
            _thToolTip.SetToolTip(InstallInModsButton, T._("Install mods and userdata, placed in") + " " + ManageSettings.ModsInstallDirName()
                + (MOmode ? T._(
                        " to MO format in Mods when possible"
                    ) : T._(
                        " to the game folder when possible"
                        )));
            _thToolTip.SetToolTip(ToolsFixModListButton, T._("Fix problems in current enabled mods list"));
            _thToolTip.SetToolTip(btnUpdateMods,
                T._("Update Mod Organizer and enabled mods") + "\n" +
                T._("Mod Organizer already have hardcoded info") + "\n" +
                T._("Mods will be updated if there exist info in meta.ini notes or in updateInfo.txt") + "\n" +
                T._("After plugins update check will be executed KKManager StandaloneUpdater for Sideloader modpack updates check for games where it is possible")
                );
            _thToolTip.SetToolTip(llOpenOldPluginsBuckupFolder,
                T._("Open older plugins buckup folder")
                );
            _thToolTip.SetToolTip(cbxBleadingEdgeZipmods,
                T._("Check also Bleeding Edge SIdeloader Modpack in KKManager") + "\n" +
                T._("Bleeding Edge SIdeloader modpack contains test versions of zipmods which is still not added in main modpacks")
                );
            _thToolTip.SetToolTip(Install2MODirPathOpenFolderLinkLabel, T._("Open folder where you can drop/download files for autoinstallation"));
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

            //Open Folders
            _thToolTip.SetToolTip(OpenGameFolderLinkLabel, T._("Open Data folder of selected game"));
            _thToolTip.SetToolTip(OpenModsFolderLinkLabel, T._("Open Mods folder of selected game"));
            _thToolTip.SetToolTip(OpenMOFolderLinkLabel, T._("Open Mod Organizer folder"));
            _thToolTip.SetToolTip(OpenMOOverwriteFolderLinkLabel, T._("Open Overwrite folder of Mod Organizer with possible new generated files for selected game\n\nFiles here have highest priority and will be loaded over any enabled mod files"));
            _thToolTip.SetToolTip(OpenMyUserDataFolderLinkLabel, T._("Open MyUserData folder in Mods if exist\n\nHere placed usual User files of Organized ModPack for selected game"));

            _thToolTip.SetToolTip(LaunchLinksLinkLabel, T._("Open list of links for game resources"));
            _thToolTip.SetToolTip(pbDiscord, T._("Discord page. Info, links, support."));
            _thToolTip.SetToolTip(ExtraSettingsLinkLabel, T._("Open extra setting window for plugins and etc"));

            _thToolTip.SetToolTip(OpenLogLinkLabel, T._("Open BepinEx log if found"));
            _thToolTip.SetToolTip(BepInExDisplayedLogLevelLabel, T._("Click here to select log level\n" +
                "Only displays the specified log level and above in the console output"));


            _thToolTip.SetToolTip(CurrentGameComboBox, T._("List of found games. Current") + ": " + GameData.CurrentGame.GetGameDisplayingName());

            var toMo = ManageSettings.ModsInstallDirName();
            _thToolTip.SetToolTip(Open2MOLinkLabel,
                T._($"Open {toMo} folder fo selected game" +
                "\n\nHere can be placed mod files which you want to install for selected game in approriate subfolders in mods" +
                "\nand then can be installed all by one click on") + " " + InstallInModsButton.Text + " " + T._("button") +
                "\n" + T._("which can be found in") + " " + ToolsTabPage.Text + " " + T._("tab page") +
                "\n\n" + T._("Helper recognize") + ":"
                + "\n " + T._($".dll files of BepinEx plugins in \"{toMo}\" folder")
                + "\n " + T._($"Sideloader mod archives in \"{toMo}\" folder")
                + "\n " + T._($"Female character cards in \"{toMo}\" folder")
                + "\n " + T._("Female character cards in \"f\" subfolder")
                + "\n " + T._("Male character cards in \"m\" subfolder")
                + "\n " + T._("Coordinate clothes set cards in \"c\" subfolder")
                + "\n " + T._("Studio scene cards in \"s\" subfolder")
                + "\n " + T._("Cardframe Front cards in \"cf\" subfolder")
                + "\n " + T._("Cardframe Back cards in \"cf\" subfolder")
                + "\n " + T._($"Script loader scripts in \"{toMo}\" folder")
                + "\n " + T._("Housing plan cards in \"h\\01\", \"h\\02\", \"h\\03\" subfolders")
                + "\n " + T._("Overlays cards in \"o\" subfolder")
                + "\n " + T._("folders with overlays cards in \"o\" subfolder")
                + "\n " + T._($"Subfolders with modfiles in \"{toMo}\" folder")
                + "\n " + T._($"Zip archives with mod files in \"{toMo}\" folder")
                + "\n " + T._($"Zip archives with mod files in \"{toMo}\" folder")
                + "\n\n" + T._($"Any Rar and 7z archives in \"{toMo}\" folder will be extracted" +
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
        private void FoldersInit()
        {
            EnableDisableSomeTools();

            SetMoMode();

            if (!Directory.Exists(DataPath))
            {
                Directory.CreateDirectory(DataPath);
            }
            if (MOmode && !Directory.Exists(ModsPath))
            {
                Directory.CreateDirectory(ModsPath);
                ModsInfoLabel.Text = T._("Mods dir created");
            }

            if (File.Exists(Path.Combine(DataPath, ManageSettings.GetCurrentGameExeName() + ".exe")))
            {
                DataInfoLabel.Text = string.Format(CultureInfo.InvariantCulture, T._("{0} game installed in {1}"), ManageSettings.GetCurrentGameDisplayingName(), "Data");
            }
            else if (File.Exists(Path.Combine(AppResDir, ManageSettings.GetCurrentGameExeName() + ".7z")))
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
                ManageModOrganizer.RestoreModlist();

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

                ManageModOrganizer.MoiniFixes();

                //try start in another thread for perfomance purposes
                new Thread(new ParameterizedThreadStart((obj) => RunSlowActions())).Start();

                SetupXmlPath = ManageModOrganizer.GetSetupXmlPathForCurrentProfile();

                ManageModOrganizerMods.SetMoModsVariables();
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
            VariablesInit();

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

        private void SetMoMode(bool setText = true)
        {
            if (File.Exists(ManageSettings.GetMoToStandartConvertationOperationsListFilePath()))
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

        bool _isDebug;
        bool _isBetaTest;
        private void EnableDisableSomeTools()
        {
            _isDebug = Path.GetFileName(Properties.Settings.Default.ApplicationStartupPath) == "Debug" && File.Exists(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "IsDevDebugMode.txt"));
            _isBetaTest = File.Exists(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "IsBetaTest.txt"));

            //Debug

            //Beta
            btnUpdateMods.Visible = true;// IsDebug || IsBetaTest;
        }

        private static void RunSlowActions()
        {
            //создание ссылок на файлы bepinex, НА ЭТО ТРАТИТСЯ МНОГО ВРЕМЕНИ
            ManageModOrganizerMods.MousfsLoadingFix();
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
            JPLauncherRunLinkLabel.Enabled = File.Exists(Path.Combine(DataPath, ManageSettings.GetIniSettingsExeName() + ".exe"));
            GameButton.Enabled = File.Exists(Path.Combine(DataPath, ManageSettings.GetCurrentGameExeName() + ".exe"));
            StudioButton.Enabled = File.Exists(Path.Combine(DataPath, ManageSettings.GetStudioExeName() + ".exe"));

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
            new INIFile(ManageSettings.GetAiHelperIniPath()).WriteINI("Settings", "autoCreateShortcutAndFixRegystry", Properties.Settings.Default.AutoShortcutRegistryCheckBoxChecked.ToString(CultureInfo.InvariantCulture));
        }

        private void ResolutionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetScreenResolution((sender as ComboBox).SelectedItem.ToString());
        }

        private void FullScreenCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ManageOther.CheckBoxChangeColor(sender as CheckBox);
            ManageXml.ChangeSetupXmlValue(SetupXmlPath, "Setting/FullScreen", (sender as CheckBox).Checked.ToString().ToLower());
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
            if (VRGameCheckBox.Visible && VRGameCheckBox.Checked && ManageSettings.GetCurrentGameIsHaveVr())
            {
                vr = "VR";
            }
            if (MOmode)
            {
                var getCurrentGameExemoProfileName = ManageSettings.GetCurrentGameExemoProfileName();
                RunProgram(MOexePath, "moshortcut://:" + getCurrentGameExemoProfileName + vr);
            }
            else
            {
                RunProgram(Path.Combine(DataPath, ManageSettings.GetCurrentGameExeName() + vr + ".exe"), string.Empty);
            }
            OnOffButtons();
        }

        private void OnOffButtons(bool switchOn = true)
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
                RunProgram(MOexePath, "moshortcut://:" + studio);
            }
            else
            {
                RunProgram(Path.Combine(DataPath, ManageSettings.GetStudioExeName() + ".exe"), string.Empty);
            }
            OnOffButtons();
        }

        private void RunProgram(string programPath, string arguments = "")
        {
            if (File.Exists(programPath))
            {
                GC.Collect();//reduce memory usage before run a program

                //fix mo profile name missing quotes when profile name with spaces
                if (!string.IsNullOrWhiteSpace(arguments) && arguments.Contains("moshortcut://:") && !arguments.Contains("moshortcut://:\""))
                {
                    arguments = arguments.Replace("moshortcut://:", "moshortcut://:\"") + "\"";
                }

                if (Path.GetFileNameWithoutExtension(programPath) == "ModOrganizer" && arguments.Length > 0)
                {
                    Task.Run(() => RunFreezedMoKiller(arguments.Replace("moshortcut://:", string.Empty))).ConfigureAwait(false);
                }

                using (Process program = new Process())
                {
                    //MessageBox.Show("outdir=" + outdir);
                    program.StartInfo.FileName = programPath;
                    if (arguments.Length > 0)
                    {
                        program.StartInfo.Arguments = arguments;
                    }

                    if (!ManageSettings.IsMoMode() || string.IsNullOrWhiteSpace(program.StartInfo.Arguments))
                    {
                        program.StartInfo.WorkingDirectory = Path.GetDirectoryName(programPath);
                    }

                    //http://www.cyberforum.ru/windows-forms/thread31052.html
                    // свернуть
                    ManageOther.SwitchFormMinimizedNormalAll(new Form[3] { this, _linksForm, _extraSettingsForm });
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

                    _ = program.Start();
                    program.WaitForExit();

                    // Показать
                    ManageOther.SwitchFormMinimizedNormalAll(new Form[3] { this, _linksForm, _extraSettingsForm });
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
        private static void RunFreezedMoKiller(string processName)
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

        private readonly Dictionary<string, string> _qualitylevels = new Dictionary<string, string>(3);

        private void QualityComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetGraphicsQuality((sender as ComboBox).SelectedIndex.ToString(CultureInfo.InvariantCulture));
        }

        private LinksForm _linksForm;

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

        readonly string _toMo = ManageSettings.ModsInstallDirName();
        private async void InstallInModsButton_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(Install2MoDirPath) && (Directory.GetFiles(Install2MoDirPath, "*.rar").Length > 0 || Directory.GetFiles(Install2MoDirPath, "*.7z").Length > 0 || Directory.GetFiles(Install2MoDirPath, "*.png").Length > 0 || Directory.GetFiles(Install2MoDirPath, "*.cs").Length > 0 || Directory.GetFiles(Install2MoDirPath, "*.dll").Length > 0 || Directory.GetFiles(Install2MoDirPath, "*.zipmod").Length > 0 || Directory.GetFiles(Install2MoDirPath, "*.zip").Length > 0 || Directory.GetDirectories(Install2MoDirPath, "*").Length > 0))
            {
                OnOffButtons(false);

                //impossible to correctly update mods in common mode
                if (!ManageSettings.IsMoMode())
                {
                    DialogResult result = MessageBox.Show(T._("Attention") + "\n\n" + T._("Impossible to correctly install/update mods\n\n in standart mode because files was moved in Data.") + "\n\n" + T._("Switch to MO mode?"), T._("Confirmation"), MessageBoxButtons.OKCancel);
                    if (result == DialogResult.OK)
                    {
                        SwitchBetweenMoAndStandartModes();
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

                MessageBox.Show(T._($"All possible mods installed. Install all rest in {_toMo} folder manually."));
            }
            else
            {
                MessageBox.Show(T._($"No compatible for installation formats found in {_toMo} folder.\nFormats: zip, zipmod, png, png in subfolder, unpacked mod in subfolder"));
            }
            Process.Start("explorer.exe", Install2MoDirPath);
        }

        private void InstallModFilesAndCleanEmptyFolder()
        {
            string installMessage = T._("Installing");
            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage));
            ManageArchive.UnpackArchivesToSubfoldersWithSameName(Install2MoDirPath, ".rar");
            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage + "."));
            ManageArchive.UnpackArchivesToSubfoldersWithSameName(Install2MoDirPath, ".7z");
            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage + ".."));
            ManageModOrganizerMods.InstallCsScriptsForScriptLoader();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage + "..."));
            ManageModOrganizerMods.InstallZipArchivesToMods();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage));
            ManageModOrganizerMods.InstallBepinExModsToMods();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage + "."));
            ManageModOrganizerMods.InstallZipModsToMods();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage + ".."));
            ManageModOrganizerMods.InstallCardsFrom2Mo();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage + "..."));
            ManageModOrganizerMods.InstallModFilesFromSubfolders();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage));
            ManageModOrganizerMods.InstallImagesFromSubfolders();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage + "."));
            ManageFilesFolders.DeleteEmptySubfolders(Install2MoDirPath, false);

            if (!Directory.Exists(Install2MoDirPath))
            {
                Directory.CreateDirectory(Install2MoDirPath);
            }

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = T._("Install from") + " " + ManageSettings.ModsInstallDirName()));
        }

        private void CreateShortcutButton_Click(object sender, EventArgs e)
        {
            ManageOther.CreateShortcuts(true, false);
        }

        private void MO2StandartButton_Click(object sender, EventArgs e)
        {
            SwitchBetweenMoAndStandartModes();
        }

        private async void SwitchBetweenMoAndStandartModes()
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
                    using (ProgressBar mo2CommonProgressBar = new ProgressBar())
                    {
                        mo2CommonProgressBar.Style = ProgressBarStyle.Marquee;
                        mo2CommonProgressBar.MarqueeAnimationSpeed = 50;
                        mo2CommonProgressBar.Dock = DockStyle.Bottom;
                        mo2CommonProgressBar.Height = 10;

                        this.Controls.Add(mo2CommonProgressBar);

                        await Task.Run(() => SwitchToCommonMode()).ConfigureAwait(true);

                        this.Controls.Remove(mo2CommonProgressBar);

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

                    using (ProgressBar mo2CommonProgressBar = new ProgressBar())
                    {
                        mo2CommonProgressBar.Style = ProgressBarStyle.Marquee;
                        mo2CommonProgressBar.MarqueeAnimationSpeed = 50;
                        mo2CommonProgressBar.Dock = DockStyle.Bottom;
                        mo2CommonProgressBar.Height = 10;

                        this.Controls.Add(mo2CommonProgressBar);

                        await Task.Run(() => SwitchBackToMoMode()).ConfigureAwait(true);

                        this.Controls.Remove(mo2CommonProgressBar);
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

        private static void SwitchBackToMoMode()
        {
            StringBuilder operationsMade = new StringBuilder();
            string[] moToStandartConvertationOperationsList = null;
            try
            {
                moToStandartConvertationOperationsList = File.ReadAllLines(ManageSettings.GetMoToStandartConvertationOperationsListFilePath());
                ReplaceVarsToPaths(ref moToStandartConvertationOperationsList);
                var operationsSplitString = new string[] { "|MovedTo|" };
                var vanillaDataFilesList = File.ReadAllLines(ManageSettings.GetVanillaDataFilesListFilePath());
                ReplaceVarsToPaths(ref vanillaDataFilesList);
                var moddedDataFilesList = File.ReadAllLines(ManageSettings.GetModdedDataFilesListFilePath());
                ReplaceVarsToPaths(ref moddedDataFilesList);
                Dictionary<string, string> zipmodsGuidList = new Dictionary<string, string>();
                bool zipmodsGuidListNotEmpty = false;
                if (File.Exists(ManageSettings.GetZipmodsGuidListFilePath()))
                {
                    using (var sr = new StreamReader(ManageSettings.GetZipmodsGuidListFilePath()))
                    {
                        while (!sr.EndOfStream)
                        {
                            var line = sr.ReadLine();

                            if (!string.IsNullOrWhiteSpace(line) && line.Contains("{{ZIPMOD}}"))
                            {
                                var guidPathPair = ReplaceVarsToPaths(line).Split(new[] { "{{ZIPMOD}}" }, StringSplitOptions.None);
                                zipmodsGuidList.Add(guidPathPair[0], guidPathPair[1]);
                            }
                        }
                    }
                    zipmodsGuidListNotEmpty = zipmodsGuidList.Count > 0;
                }

                //remove normal mode identifier
                SwitchNormalModeIdentifier(false);

                StringBuilder filesWhichAlreadyHaveSameDestFileInMods = new StringBuilder();
                bool filesWhichAlreadyHaveSameDestFileInModsIsNotEmpty = false;

                //Перемещение файлов модов по списку
                int operationsLength = moToStandartConvertationOperationsList.Length;
                for (int o = 0; o < operationsLength; o++)
                {
                    if (string.IsNullOrWhiteSpace(moToStandartConvertationOperationsList[o]))
                    {
                        continue;
                    }

                    string[] movePaths = moToStandartConvertationOperationsList[o].Split(operationsSplitString, StringSplitOptions.None);

                    var filePathInModsExists = File.Exists(movePaths[0]);
                    var filePathInDataExists = File.Exists(movePaths[1]);

                    if (!filePathInDataExists)
                    {
                        continue;
                    }

                    if (!filePathInModsExists)
                    {
                        string modsubfolder = Path.GetDirectoryName(movePaths[0]);
                        if (!Directory.Exists(modsubfolder))
                        {
                            Directory.CreateDirectory(modsubfolder);
                        }

                        try//ignore move file error if file will be locked and write in log about this
                        {
                            File.Move(movePaths[1], movePaths[0]);

                            //запись выполненной операции для удаления из общего списка в случае ошибки при переключении из обычного режима
                            operationsMade.AppendLine(moToStandartConvertationOperationsList[o]);

                            try
                            {
                                //Move bonemod file both with original
                                if (File.Exists(movePaths[1] + ".bonemod.txt"))
                                {
                                    File.Move(movePaths[1] + ".bonemod.txt", movePaths[0] + ".bonemod.txt");
                                }
                                //запись выполненной операции для удаления из общего списка в случае ошибки при переключении из обычного режима
                                operationsMade.AppendLine(movePaths[1] + ".bonemod.txt" + "|MovedTo|" + movePaths[0] + ".bonemod.txt");
                            }
                            catch (Exception ex)
                            {
                                ManageLogs.Log("An error occured while file moving."+ "MovePaths[0]=" + movePaths[0]+ ";MovePaths[1]="+ movePaths[0] + ".error:\r\n" + ex);
                            }
                        }
                        catch (Exception ex)
                        {
                            ManageLogs.Log("Failed to move file: '" + Environment.NewLine + movePaths[1] + "' " + Environment.NewLine + "Error:" + Environment.NewLine + ex);
                        }
                    }
                    else
                    {
                        //если в Mods на месте планируемого для перемещения назад в Mods файла появился новый файл, то записать информацию о нем в новый мод, чтобы перенести его в новый мод
                        filesWhichAlreadyHaveSameDestFileInMods.AppendLine(movePaths[1] + "|MovedTo|" + movePaths[0]);
                        filesWhichAlreadyHaveSameDestFileInModsIsNotEmpty = true;
                    }
                }

                //string destFolderForNewFiles = Path.Combine(ModsPath, "NewAddedFiles");

                //получение даты и времени для дальнейшего использования
                string dateTimeInFormat = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

                if (filesWhichAlreadyHaveSameDestFileInModsIsNotEmpty)
                {
                    foreach (string fromToPathsLine in filesWhichAlreadyHaveSameDestFileInMods.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (string.IsNullOrWhiteSpace(fromToPathsLine))
                        {
                            continue;
                        }

                        string[] fromToPaths = fromToPathsLine.Split(new string[] { "|MovedTo|" }, StringSplitOptions.None);

                        string targetFolderPath = Path.GetDirectoryName(fromToPaths[1]);

                        bool isForOverwriteFolder = ManageStrings.IsStringAContainsStringB(targetFolderPath, OverwriteFolder);
                        //поиск имени мода с учетом обработки файлов папки Overwrite
                        string modName = targetFolderPath;
                        if (isForOverwriteFolder)
                        {
                            modName = Path.GetFileName(OverwriteFolder);
                        }
                        else
                        {
                            while (Path.GetDirectoryName(modName) != ModsPath)
                            {
                                modName = Path.GetDirectoryName(modName);
                            }
                            modName = Path.GetFileName(modName);
                        }

                        //Новое имя для новой целевой папки мода
                        string originalModPath = isForOverwriteFolder ? OverwriteFolder : Path.Combine(ModsPath, modName);
                        string newModName = modName + "_" + dateTimeInFormat;
                        string newModPath = Path.Combine(ModsPath, newModName);
                        targetFolderPath = targetFolderPath.Replace(originalModPath, newModPath);

                        string targetFileName = Path.GetFileNameWithoutExtension(fromToPaths[1]);
                        string targetFileExtension = Path.GetExtension(fromToPaths[1]);
                        string targetPath = Path.Combine(targetFolderPath, targetFileName + targetFileExtension);

                        //создать подпапку для файла
                        if (!Directory.Exists(targetFolderPath))
                        {
                            Directory.CreateDirectory(targetFolderPath);
                        }

                        //переместить файл в новую для него папку
                        File.Move(fromToPaths[0], targetPath);

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
                        File.WriteAllText(Path.Combine(newModPath, "NOTE!.txt"), note);

                        //запись meta.ini с замечанием
                        ManageModOrganizer.WriteMetaIni(
                            newModPath
                            ,
                            string.Empty
                            ,
                            "0." + dateTimeInFormat
                            ,
                            string.Empty
                            ,
                            note.Replace("\n", "<br>")
                            );
                        ManageModOrganizer.ActivateDeactivateInsertMod(newModName, false, modName, false);
                    }
                }

                //Перемещение новых файлов
                //
                //добавление всех файлов из дата, которых нет в списке файлов модов и игры в дата, что был создан сразу после перехода в обычный режим
                string[] addedFiles = Directory.GetFiles(DataPath, "*.*", SearchOption.AllDirectories).Where(line => !moddedDataFilesList.Contains(line)).ToArray();
                //задание имени целевой папки для новых модов
                string addedFilesFolderName = "[added]UseFiles_" + dateTimeInFormat;
                string destFolderPath = Path.Combine(ModsPath, addedFilesFolderName);

                int addedFilesLength = addedFiles.Length;
                for (int f = 0; f < addedFilesLength; f++)
                {
                    string destFileName = null;
                    try
                    {
                        //если zipmod guid присутствует в сохраненных, переместить его на место удаленного
                        string ext;
                        string guid;
                        if (zipmodsGuidListNotEmpty
                            && addedFiles[f].ToUpperInvariant().Contains("SIDELOADER MODPACK")
                            && ((ext = Path.GetExtension(addedFiles[f]).ToUpperInvariant()) == ".ZIPMOD" || ext == ".ZIP")
                            && !string.IsNullOrWhiteSpace(guid = ManageArchive.GetZipmodGuid(addedFiles[f]))
                            && zipmodsGuidList.ContainsKey(guid)
                            )
                        {
                            if (zipmodsGuidList[guid].Contains("%"))//temp check
                            {
                                ManageLogs.Log("zipmod contains %VAR%:" + zipmodsGuidList[guid]);
                            }

                            var zipmod = ReplaceVarsToPaths(zipmodsGuidList[guid]);

                            if (Path.GetFileName(addedFiles[f]) == Path.GetFileName(zipmod))//when zipmod has same name but moved
                            {
                                var targetfolder = zipmod.IsInOverwriteFolder() ?
                                    ManageSettings.GetOverwriteFolder() : ManageSettings.GetCurrentGameModsPath();
                                destFileName = addedFiles[f].Replace(ManageSettings.GetCurrentGameDataPath(), targetfolder
                                    );
                            }
                            else//when mod was renamed
                            {
                                if (zipmod.IsInOverwriteFolder())//zipmod in overwrite
                                {
                                    var newFilePath = addedFiles[f].Replace(ManageSettings.GetCurrentGameDataPath(), ManageSettings.GetOverwriteFolder());
                                    if (Directory.Exists(Path.GetDirectoryName(newFilePath)) && newFilePath != addedFiles[f])
                                    {
                                        destFileName = newFilePath;
                                    }
                                }
                                else//zipmod in Mods
                                {
                                    var modPath = ManageModOrganizerMods.GetMoModPathInMods(zipmod);
                                    if (Path.GetFileName(modPath).ToUpperInvariant() != "MODS" && Directory.Exists(modPath))
                                    {
                                        destFileName = addedFiles[f].Replace(ManageSettings.GetCurrentGameDataPath(), modPath);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ManageLogs.Log("Error occured while to MO mode switch:" + Environment.NewLine + ex);
                    }

                    if (string.IsNullOrEmpty(destFileName))
                    {
                        destFileName = addedFiles[f].Replace(DataPath, destFolderPath);
                    }
                    Directory.CreateDirectory(Path.GetDirectoryName(destFileName));
                    File.Move(addedFiles[f], destFileName);
                }

                //подключить новый мод, если он существует
                if (Directory.Exists(destFolderPath))
                {
                    //запись meta.ini
                    ManageModOrganizer.WriteMetaIni(
                        destFolderPath
                        ,
                        "53,"
                        ,
                        dateTimeInFormat
                        ,
                        T._("Sort files if need")
                        ,
                        T._("<br>This files was added in Common mode<br>and moved as mod after convertation in MO mode.<br>Date: ") + dateTimeInFormat
                        );

                    ManageModOrganizer.ActivateDeactivateInsertMod(addedFilesFolderName);
                }

                //перемещение ванильных файлов назад в дата
                MoveVanillaFIlesBackToData();

                //очистка пустых папок в Data
                if (File.Exists(ManageSettings.GetVanillaDataEmptyFoldersListFilePath()))
                {
                    //удалить все, за исключением добавленных ранее путей до пустых папок
                    string[] vanillaDataEmptyFoldersList = File.ReadAllLines(ManageSettings.GetVanillaDataEmptyFoldersListFilePath());
                    ReplaceVarsToPaths(ref vanillaDataEmptyFoldersList);
                    ManageFilesFolders.DeleteEmptySubfolders(DataPath, false, vanillaDataEmptyFoldersList);
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
                File.Delete(ManageSettings.GetMoToStandartConvertationOperationsListFilePath());
                File.Delete(ManageSettings.GetVanillaDataFilesListFilePath());
                File.Delete(ManageSettings.GetVanillaDataEmptyFoldersListFilePath());
                File.Delete(ManageSettings.GetModdedDataFilesListFilePath());
                if (File.Exists(ManageSettings.GetZipmodsGuidListFilePath()))
                {
                    File.Delete(ManageSettings.GetZipmodsGuidListFilePath());
                }

                MOmode = true;

                MessageBox.Show(T._("Mod Organizer mode restored! All mod files moved back to Mods folder. If in Data folder was added new files they also moved in Mods folder as new mod, check and sort it if need"));

            }
            catch (Exception ex)
            {
                //обновление списка операций с файлами, для удаления уже выполненных и записи обновленного списка
                if (operationsMade.ToString().Length > 0 && moToStandartConvertationOperationsList != null && moToStandartConvertationOperationsList.Length > 0)
                {
                    foreach (string operationsMadeLine in operationsMade.ToString().SplitToLines())
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(operationsMadeLine))
                            {
                                continue;
                            }

                            moToStandartConvertationOperationsList = moToStandartConvertationOperationsList.Where(operationsLine => operationsLine != operationsMadeLine).ToArray();
                        }
                        catch
                        {
                        }
                    }

                    File.WriteAllLines(ManageSettings.GetMoToStandartConvertationOperationsListFilePath(), moToStandartConvertationOperationsList);
                }

                //recreate normal mode identifier if failed
                SwitchNormalModeIdentifier();

                MessageBox.Show("Failed to switch in MO mode. Error:" + Environment.NewLine + ex);
            }
        }

        /// <summary>
        /// normal mode identifier switcher
        /// </summary>
        /// <param name="create">true=Create/false=Delete</param>
        private static void SwitchNormalModeIdentifier(bool create = true)
        {
            if (create)
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
            var enabledModsList = ManageModOrganizer.GetModNamesListFromActiveMoProfile();

            if (enabledModsList.Length == 0)
            {
                MessageBox.Show(T._("There is no enabled mods in Mod Organizer"));
                return;
            }

            //список выполненных операций с файлами.
            var moToStandartConvertationOperationsList = new StringBuilder();
            //список пустых папок в data до ереноса файлов модов
            StringBuilder vanillaDataEmptyFoldersList = new StringBuilder();
            //список файлов в data без модов
            string[] vanillaDataFilesList;
            //список guid zipmod-ов
            Dictionary<string, string> zipmodsGuidList = new Dictionary<string, string>();
            List<string> longPaths = new List<string>();


            var enabledModsLength = enabledModsList.Length;

            var debufStr = "";
            try
            {
                ManageModOrganizerMods.CleanBepInExLinksFromData();

                if (!ManageSettings.MoIsNew)
                {
                    if (File.Exists(ManageSettings.GetDummyFilePath()) && /*Удалил TESV.exe, который был лаунчером, а не болванкой*/new FileInfo(ManageSettings.GetDummyFilePath()).Length < 10000)
                    {
                        File.Delete(ManageSettings.GetDummyFilePath());
                    }
                }

                debufStr = ManageSettings.GetMOmodeDataFilesBakDirPath();
                Directory.CreateDirectory(ManageSettings.GetMOmodeDataFilesBakDirPath());
                moToStandartConvertationOperationsList = new StringBuilder();

                vanillaDataEmptyFoldersList = ManageFilesFolders.GetEmptySubfoldersPaths(ManageSettings.GetCurrentGameDataPath(), new StringBuilder());

                //получение всех файлов из Data
                vanillaDataFilesList = Directory.GetFiles(DataPath, "*.*", SearchOption.AllDirectories);


                //получение всех файлов из папки Overwrite и их обработка
                var filesInOverwrite = Directory.GetFiles(OverwriteFolder, "*.*", SearchOption.AllDirectories);
                if (filesInOverwrite.Length > 0)
                {
                    //if (Path.GetFileName(FilesInOverwrite[0]).Contains("Overwrite"))
                    //{
                    //    OverwriteFolder = OverwriteFolder.Replace("overwrite", "Overwrite");
                    //}

                    string fileInDataFolder;
                    var filesInOverwriteLength = filesInOverwrite.Length;

                    using (var frmProgress = new Form())
                    {
                        frmProgress.Text = T._("Move files from Overwrite folder");
                        frmProgress.Size = new Size(200, 50);
                        frmProgress.StartPosition = FormStartPosition.CenterScreen;
                        frmProgress.FormBorderStyle = FormBorderStyle.FixedToolWindow;
                        using (var pbProgress = new ProgressBar())
                        {
                            pbProgress.Maximum = filesInOverwriteLength;
                            pbProgress.Dock = DockStyle.Bottom;
                            frmProgress.Controls.Add(pbProgress);
                            frmProgress.Show();
                            for (int n = 0; n < filesInOverwriteLength; n++)
                            {
                                if (n < pbProgress.Maximum)
                                {
                                    pbProgress.Value = n;
                                }

                                var fileInOverwrite = filesInOverwrite[n];

                                if (ManageStrings.CheckForLongPath(ref fileInOverwrite))
                                {
                                    longPaths.Add(fileInOverwrite.Remove(0, 4));
                                }
                                //if (FileInOverwrite.Length > 259)
                                //{
                                //    if (OfferToSkipTheFileConfirmed(FileInOverwrite))
                                //    {
                                //        continue;
                                //    }
                                //    //    FileInOverwrite = @"\\?\" + FileInOverwrite;
                                //}

                                fileInDataFolder = fileInOverwrite.Replace(OverwriteFolder, DataPath);
                                if (ManageStrings.CheckForLongPath(ref fileInDataFolder))
                                {
                                    longPaths.Add(fileInDataFolder.Remove(0, 4));
                                }
                                //if (FileInDataFolder.Length > 259)
                                //{
                                //    FileInDataFolder = @"\\?\" + FileInDataFolder;
                                //}
                                if (File.Exists(fileInDataFolder))
                                {
                                    var fileInBakFolderWhichIsInRes = fileInDataFolder.Replace(DataPath, ManageSettings.GetMOmodeDataFilesBakDirPath());
                                    //if (FileInBakFolderWhichIsInRES.Length > 259)
                                    //{
                                    //    FileInBakFolderWhichIsInRES = @"\\?\" + FileInBakFolderWhichIsInRES;
                                    //}
                                    if (!File.Exists(fileInBakFolderWhichIsInRes) && vanillaDataFilesList.Contains(fileInDataFolder))
                                    {

                                        var bakfolder = Path.GetDirectoryName(fileInBakFolderWhichIsInRes);
                                        try
                                        {
                                            if (_isDebug)
                                                debufStr = bakfolder + ":bakfolder,l2043";
                                            Directory.CreateDirectory(bakfolder);

                                            File.Move(fileInDataFolder, fileInBakFolderWhichIsInRes);//перенос файла из Data в Bak, если там не было

                                            ManageModOrganizerMods.SaveGuidIfZipMod(fileInOverwrite, zipmodsGuidList);

                                            File.Move(fileInOverwrite, fileInDataFolder);//перенос файла из папки Overwrite в Data
                                            moToStandartConvertationOperationsList.AppendLine(fileInOverwrite + "|MovedTo|" + fileInDataFolder);//запись об операции будет пропущена, если будет какая-то ошибка

                                        }
                                        catch (Exception ex)
                                        {
                                            //когда файла в дата нет, файл в бак есть и есть файл в папке Overwrite - вернуть файл из bak назад
                                            if (!File.Exists(fileInDataFolder) && File.Exists(fileInBakFolderWhichIsInRes) && File.Exists(fileInOverwrite))
                                            {
                                                File.Move(fileInBakFolderWhichIsInRes, fileInDataFolder);
                                            }

                                            ManageLogs.Log("Error occured while to common mode switch:" + Environment.NewLine + ex + "\r\nparent dir=" + bakfolder + "\r\nData path=" + fileInDataFolder + "\r\nOverwrite path=" + fileInOverwrite);
                                        }
                                    }
                                }
                                else
                                {
                                    var destFolder = Path.GetDirectoryName(fileInDataFolder);
                                    try
                                    {
                                        if (_isDebug)
                                            debufStr = destFolder + ":destFolder,l2068";
                                        Directory.CreateDirectory(destFolder);

                                        ManageModOrganizerMods.SaveGuidIfZipMod(fileInOverwrite, zipmodsGuidList);

                                        File.Move(fileInOverwrite, fileInDataFolder);//перенос файла из папки мода в Data
                                        moToStandartConvertationOperationsList.AppendLine(fileInOverwrite + "|MovedTo|" + fileInDataFolder);//запись об операции будет пропущена, если будет какая-то ошибка
                                    }
                                    catch (Exception ex)
                                    {
                                        ManageLogs.Log("Error occured while to common mode switch:" + Environment.NewLine + ex + "\r\npath=" + destFolder + "\r\nData path=" + fileInDataFolder + "\r\nOverwrite path=" + fileInOverwrite);
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
                        pbProgress.Maximum = enabledModsLength;
                        pbProgress.Dock = DockStyle.Bottom;
                        frmProgress.Controls.Add(pbProgress);
                        frmProgress.Show();
                        for (int n = 0; n < enabledModsLength; n++)
                        {
                            if (n < pbProgress.Maximum)
                            {
                                pbProgress.Value = n;
                            }

                            var modFolder = Path.Combine(ModsPath, enabledModsList[n]);
                            if (modFolder.Length > 0 && Directory.Exists(modFolder))
                            {
                                var modFiles = Directory.GetFiles(modFolder, "*.*", SearchOption.AllDirectories);
                                if (modFiles.Length > 0)
                                {
                                    var modFilesLength = modFiles.Length;
                                    string fileInDataFolder;

                                    var metaskipped = false;
                                    for (int f = 0; f < modFilesLength; f++)
                                    {
                                        //"\\?\" - prefix to ignore 260 path cars limit

                                        var fileOfMod = modFiles[f];
                                        if (ManageStrings.CheckForLongPath(ref fileOfMod))
                                        {
                                            longPaths.Add(fileOfMod.Remove(0, 4));
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
                                            var fileExtension = Path.GetExtension(fileOfMod);
                                            if (fileExtension == ".txt" || fileExtension.IsPictureExtension())
                                            {
                                                if (Path.GetFileName(fileOfMod.Replace(Path.DirectorySeparatorChar + Path.GetFileName(fileOfMod), string.Empty)) == enabledModsList[n])
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
                                        else if (Path.GetFileName(fileOfMod) == "meta.ini" /*ModFile.Length >= 8 && string.Compare(ModFile.Substring(ModFile.Length - 8, 8), "meta.ini", true, CultureInfo.InvariantCulture) == 0*/)
                                        {
                                            metaskipped = true;//для ускорения проверки, когда meta будет найден, будет делать быструю проверку bool переменной
                                            continue;
                                        }

                                        //MOCommonModeSwitchButton.Text = "..." + EnabledModsLength + "/" + N + ": " + f + "/" + ModFilesLength;
                                        fileInDataFolder = fileOfMod.Replace(modFolder, DataPath);
                                        if (ManageStrings.CheckForLongPath(ref fileInDataFolder))
                                        {
                                            longPaths.Add(fileInDataFolder.Remove(0, 4));
                                        }
                                        //if (FileInDataFolder.Length > 259)
                                        //{
                                        //    FileInDataFolder = @"\\?\" + FileInDataFolder;
                                        //}

                                        if (File.Exists(fileInDataFolder))
                                        {
                                            var fileInBakFolderWhichIsInRes = fileInDataFolder.Replace(DataPath, ManageSettings.GetMOmodeDataFilesBakDirPath());
                                            //if (FileInBakFolderWhichIsInRES.Length > 259)
                                            //{
                                            //    FileInBakFolderWhichIsInRES = @"\\?\" + FileInBakFolderWhichIsInRES;
                                            //}
                                            if (!File.Exists(fileInBakFolderWhichIsInRes) && vanillaDataFilesList.Contains(fileInDataFolder))
                                            {
                                                var bakfolder = Path.GetDirectoryName(fileInBakFolderWhichIsInRes);
                                                try
                                                {
                                                    if (_isDebug)
                                                        debufStr = bakfolder + ":bakfolder,l2183";
                                                    Directory.CreateDirectory(bakfolder);

                                                    File.Move(fileInDataFolder, fileInBakFolderWhichIsInRes);//перенос файла из Data в Bak, если там не было

                                                    ManageModOrganizerMods.SaveGuidIfZipMod(fileOfMod, zipmodsGuidList);

                                                    File.Move(fileOfMod, fileInDataFolder);//перенос файла из папки мода в Data
                                                    moToStandartConvertationOperationsList.AppendLine(fileOfMod + "|MovedTo|" + fileInDataFolder);//запись об операции будет пропущена, если будет какая-то ошибка
                                                }
                                                catch (Exception ex)
                                                {
                                                    //когда файла в дата нет, файл в бак есть и есть файл в папке мода - вернуть файл из bak назад
                                                    if (!File.Exists(fileInDataFolder) && File.Exists(fileInBakFolderWhichIsInRes) && File.Exists(fileOfMod))
                                                    {
                                                        File.Move(fileInBakFolderWhichIsInRes, fileInDataFolder);
                                                    }

                                                    ManageLogs.Log("Error occured while to common mode switch:" + Environment.NewLine + ex + "\r\npath=" + bakfolder + "\r\nData path=" + fileInDataFolder + "\r\nMods path=" + fileOfMod);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var destFolder = Path.GetDirectoryName(fileInDataFolder);
                                            try
                                            {
                                                if (_isDebug)
                                                    debufStr = destFolder + ":destFolder,l2208";
                                                Directory.CreateDirectory(destFolder);

                                                ManageModOrganizerMods.SaveGuidIfZipMod(fileOfMod, zipmodsGuidList);

                                                File.Move(fileOfMod, fileInDataFolder);//перенос файла из папки мода в Data
                                                moToStandartConvertationOperationsList.AppendLine(fileOfMod + "|MovedTo|" + fileInDataFolder);//запись об операции будет пропущена, если будет какая-то ошибка
                                            }
                                            catch (Exception ex)
                                            {
                                                ManageLogs.Log("Error occured while to common mode switch:" + Environment.NewLine + ex + "\r\npath=" + destFolder + "\r\nData path=" + fileInDataFolder + "\r\nMods path=" + fileOfMod);
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

                ReplacePathsToVars(ref moToStandartConvertationOperationsList);
                File.WriteAllText(ManageSettings.GetMoToStandartConvertationOperationsListFilePath(), moToStandartConvertationOperationsList.ToString());
                moToStandartConvertationOperationsList.Clear();

                var dataWithModsFileslist = Directory.GetFiles(DataPath, "*.*", SearchOption.AllDirectories);
                ReplacePathsToVars(ref dataWithModsFileslist);
                File.WriteAllLines(ManageSettings.GetModdedDataFilesListFilePath(), dataWithModsFileslist);

                ReplacePathsToVars(ref vanillaDataFilesList);
                File.WriteAllLines(ManageSettings.GetVanillaDataFilesListFilePath(), vanillaDataFilesList);

                if (zipmodsGuidList.Count > 0)
                {
                    //using (var file = new StreamWriter(ManageSettings.GetZipmodsGUIDListFilePath()))
                    //{
                    //    foreach (var entry in ZipmodsGUIDList)
                    //    {
                    //        file.WriteLine("{0}{{ZIPMOD}}{1}", entry.Key, entry.Value);
                    //    }
                    //}
                    File.WriteAllLines(ManageSettings.GetZipmodsGuidListFilePath(),
                        zipmodsGuidList.Select(x => x.Key + "{{ZIPMOD}}" + x.Value).ToArray());
                }
                dataWithModsFileslist = null;

                //create normal mode identifier
                SwitchNormalModeIdentifier();


                //записать пути до пустых папок, чтобы при восстановлении восстановить и их
                if (vanillaDataEmptyFoldersList.ToString().Length > 0)
                {
                    ReplacePathsToVars(ref vanillaDataEmptyFoldersList);
                    File.WriteAllText(ManageSettings.GetVanillaDataEmptyFoldersListFilePath(), vanillaDataEmptyFoldersList.ToString());
                }

                MOmode = false;

                MessageBox.Show(T._("All mod files now in Data folder! You can restore MO mode by same button."));
            }
            catch (Exception ex)
            {
                //восстановление файлов в первоначальные папки
                RestoreMovedFilesLocation(moToStandartConvertationOperationsList);

                //clean empty folders except whose was already in Data
                ManageFilesFolders.DeleteEmptySubfolders(DataPath, false, vanillaDataEmptyFoldersList.ToString().SplitToLines().ToArray());

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
            .Replace(ManageSettings.VarCurrentGameMoOverwritePath(), ManageSettings.GetCurrentGameMoOverwritePath());
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
                .Replace(ManageSettings.GetCurrentGameMoOverwritePath(), ManageSettings.VarCurrentGameMoOverwritePath());
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
                .Replace(ManageSettings.VarCurrentGameMoOverwritePath(), ManageSettings.GetCurrentGameMoOverwritePath());
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
                .Replace(ManageSettings.GetCurrentGameMoOverwritePath(), ManageSettings.VarCurrentGameMoOverwritePath());
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
                .Replace(ManageSettings.VarCurrentGameMoOverwritePath(), ManageSettings.GetCurrentGameMoOverwritePath());
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

        private static void RestoreMovedFilesLocation(StringBuilder operations)
        {
            if (operations.Length > 0)
            {
                foreach (string record in operations.ToString().SplitToLines())
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(record))
                        {
                            continue;
                        }

                        var movePaths = record.Split(new string[] { "|MovedTo|" }, StringSplitOptions.None);

                        if (movePaths.Length != 2)
                        {
                            continue;
                        }

                        var filePathInMods = movePaths[0];
                        var filePathInData = movePaths[1];

                        if (File.Exists(filePathInData))
                        {
                            if (!File.Exists(filePathInMods))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(filePathInMods));

                                File.Move(filePathInData, filePathInMods);
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
            var mOmodeDataFilesBakDirPath = ManageSettings.GetMOmodeDataFilesBakDirPath();
            if (Directory.Exists(mOmodeDataFilesBakDirPath))
            {
                var filesInMOmodeDataFilesBak = Directory.GetFiles(mOmodeDataFilesBakDirPath, "*.*", SearchOption.AllDirectories);
                int filesInMOmodeDataFilesBakLength = filesInMOmodeDataFilesBak.Length;
                for (int f = 0; f < filesInMOmodeDataFilesBakLength; f++)
                {
                    if (string.IsNullOrWhiteSpace(filesInMOmodeDataFilesBak[f]))
                    {
                        continue;
                    }

                    var destFileInDataFolderPath = filesInMOmodeDataFilesBak[f].Replace(mOmodeDataFilesBakDirPath, DataPath);
                    if (!File.Exists(destFileInDataFolderPath))
                    {
                        var destFileInDataFolderPathFolder = Path.GetDirectoryName(destFileInDataFolderPath);
                        if (!Directory.Exists(destFileInDataFolderPathFolder))
                        {
                            Directory.CreateDirectory(destFileInDataFolderPathFolder);
                        }
                        File.Move(filesInMOmodeDataFilesBak[f], destFileInDataFolderPath);
                    }
                }

                //удаление папки, где хранились резервные копии ванильных файлов
                ManageFilesFolders.DeleteEmptySubfolders(mOmodeDataFilesBakDirPath);
            }
        }

        private void OpenGameFolderLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Directory.Exists(DataPath))
                Process.Start("explorer.exe", DataPath);
        }

        private void OpenMOFolderLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Directory.Exists(MoDirPath))
                Process.Start("explorer.exe", MoDirPath);
        }

        private void OpenModsFolderLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Directory.Exists(ModsPath))
                Process.Start("explorer.exe", ModsPath);
        }

        private void Install2MODirPathOpenFolderLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!Directory.Exists(Install2MoDirPath))
            {
                Directory.CreateDirectory(Install2MoDirPath);
            }
            Process.Start("explorer.exe", Install2MoDirPath);
        }

        private void OpenMyUserDataFolderLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string userFilesFolder = Path.Combine(ModsPath, "MyUserData");
            if (!Directory.Exists(userFilesFolder))
            {
                userFilesFolder = Path.Combine(ModsPath, "MyUserFiles");
            }
            if (!Directory.Exists(userFilesFolder))
            {
                userFilesFolder = ManageSettings.GetOverwriteFolder();
            }
            if (Directory.Exists(userFilesFolder))
            {
                Process.Start("explorer.exe", userFilesFolder);
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
            ManageModOrganizerMods.OpenBepinexLog();
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

                new INIFile(ManageSettings.GetAiHelperIniPath()).WriteINI("Settings", "selected_game", ManageSettings.GetCurrentGameFolderName());

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
            ManageModOrganizer.RedefineGameMoData();
            Properties.Settings.Default.BepinExCfgPath = string.Empty;
            Properties.Settings.Default.MOSelectedProfileDirName = string.Empty;

            ManageModOrganizer.CheckBaseGamesPy();

            CurrentGame.InitActions();
            CurrentGameTitleTextBox.Text = CurrentGame.GetGameDisplayingName();
            Properties.Settings.Default.CurrentGameDisplayingName = CurrentGame.GetGameDisplayingName();
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

        ExtraSettingsForm _extraSettingsForm;
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
                RunProgram(MOexePath, "moshortcut://:" + ManageSettings.GetIniSettingsExeName());
            }
            else
            {
                RunProgram(Path.Combine(DataPath, ManageSettings.GetIniSettingsExeName() + ".exe"), string.Empty);
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
            if (!ManageSettings.IsMoMode())
            {
                DialogResult result = MessageBox.Show(T._("Attention") + "\n\n" + T._("Correct modlist fixes possible only in MO mode") + "\n\n" + T._("Switch to MO mode?"), T._("Confirmation"), MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    SwitchBetweenMoAndStandartModes();
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
                await new Updater().Update().ConfigureAwait(true);
            }

            //run zipmod's check if updater found and only for KK, AI, HS2
            if (CurrentGame.IsHaveSideloaderMods && File.Exists(ManageSettings.KkManagerStandaloneUpdaterExePath()))
            {
                if (MOmode)
                {
                    ManageModOrganizer.CleanMoFolder();
                    //
                    ManageModOrganizer.CheckBaseGamesPy();

                    ManageModOrganizer.RedefineGameMoData();

                    //add updater as new exe in mo list if not exists
                    //if (!ManageMO.IsMOcustomExecutableTitleByExeNameExists("StandaloneUpdater"))
                    {
                        var kkManagerStandaloneUpdater = new ManageModOrganizer.CustomExecutables.CustomExecutable();
                        kkManagerStandaloneUpdater.Attribute["title"] = "KKManagerStandaloneUpdater";
                        kkManagerStandaloneUpdater.Attribute["binary"] = ManageSettings.KkManagerStandaloneUpdaterExePath();
                        kkManagerStandaloneUpdater.Attribute["workingDirectory"] = ManageSettings.GetCurrentGameDataPath();
                        ManageModOrganizer.InsertCustomExecutable(kkManagerStandaloneUpdater);
                    }

                    var zipmodsGuidList = new Dictionary<string, string>();

                    //activate all mods with Sideloader modpack inside
                    ActivateSideloaderMods(zipmodsGuidList);

                    RunProgram(MOexePath, "moshortcut://:" + ManageModOrganizer.GetMOcustomExecutableTitleByExeName("StandaloneUpdater"));

                    //restore modlist
                    ManageModOrganizer.RestoreModlist();

                    //restore zipmods to source mods
                    MoveZipModsFromOverwriteToSourceMod(zipmodsGuidList);
                }
                else
                {
                    //run updater normal
                    RunProgram(ManageSettings.KkManagerStandaloneUpdaterExePath(), "\"" + ManageSettings.GetCurrentGameDataPath() + "\"");
                }
            }

            FoldersInit();

            AIGirlHelperTabControl.Enabled = true;
        }

        private static void MoveZipModsFromOverwriteToSourceMod(Dictionary<string, string> zipmodsGuidList)
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

            var progressForm = new Form
            {
                StartPosition = FormStartPosition.CenterScreen,
                Size = new Size(400, 50),
                Text = T._("Sideloader dirs sorting") + "..",
                FormBorderStyle = FormBorderStyle.FixedToolWindow
            };
            var pBar = new ProgressBar
            {
                Dock = DockStyle.Bottom
            };

            progressForm.Controls.Add(pBar);
            progressForm.Show();

            var modpacks = ManageModOrganizerMods.GetSideloaderModpackTargetDirs();

            pBar.Maximum = dirs.Count();
            pBar.Value = 0;

            var timeInfo = DateTime.Now.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture);

            foreach (var dir in dirs)
            {
                //try
                {
                    if (pBar.Value < pBar.Maximum)
                    {
                        pBar.Value++;
                    }

                    //if (!dir.ToUpperInvariant().Contains("SIDELOADER MODPACK"))
                    //{
                    //    continue;
                    //}

                    //set sideloader dir name
                    var sideloadername = Path.GetFileName(dir);

                    progressForm.Text = T._("Sorting") + ":" + sideloadername;

                    var isUnc = ManageModOrganizerMods.IsUncensorSelector(sideloadername);
                    var isMaleUnc = isUnc && modpacks.ContainsKey(sideloadername + "M");
                    var isFeMaleUnc = isUnc && modpacks.ContainsKey(sideloadername + "F");
                    var isSortingModPack = modpacks.ContainsKey(sideloadername) || isMaleUnc || isFeMaleUnc;
                    foreach (var f in Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories))
                    {
                        var file = f;

                        // Check if TargetIsInSideloader by guid
                        var guid = ManageArchive.GetZipmodGuid(file);
                        bool isguid = guid.Length > 0 && zipmodsGuidList.ContainsKey(guid);
                        string targetModPath = isguid ? ManageModOrganizerMods.GetMoModPathInMods(zipmodsGuidList[guid]) : "";
                        var pathElements = !string.IsNullOrWhiteSpace(targetModPath) ? file.Replace(targetModPath, "").Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries) : null;
                        var targetzipModDirName = pathElements != null && pathElements.Length > 1 ? pathElements[1] : ""; // %modpath%\mods\%sideloadermodpackdir%
                        var targetIsInSideloader = targetzipModDirName.ToUpperInvariant().Contains("SIDELOADER MODPACK"); // dir in mods is sideloader

                        if (isguid && !targetIsInSideloader/*do not touch sideloader files and let them be updated properly*/)//move by guid
                        {
                            var target = file.Replace(ManageSettings.GetOverwriteFolder(), targetModPath);


                            bool isTargetExists = File.Exists(target);
                            bool isSourceExists = File.Exists(file);

                            ManageStrings.CheckForLongPath(ref file);
                            ManageStrings.CheckForLongPath(ref target);

                            if (!isSourceExists)
                            {
                                continue;
                            }
                            else if (isTargetExists)
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
                                    var tfiletarget = file.Replace(ManageSettings.GetOverwriteFolder(), targetModPath + "_" + timeInfo);

                                    Directory.CreateDirectory(Path.GetDirectoryName(tfiletarget));
                                    if (!File.Exists(tfiletarget))
                                        File.Move(target, tfiletarget); // move older target file
                                }
                                else
                                {
                                    target = file.Replace(ManageSettings.GetOverwriteFolder(), targetModPath + "_" + timeInfo);
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
                        else if (isSortingModPack)
                        {
                            var sortM = isMaleUnc && Path.GetExtension(file).StartsWith(".zip", StringComparison.CurrentCultureIgnoreCase) && (Path.GetFileNameWithoutExtension(file).Contains("[Penis]") || Path.GetFileNameWithoutExtension(file).Contains("[Balls]"));
                            var sortF = isFeMaleUnc && Path.GetExtension(file).StartsWith(".zip", StringComparison.CurrentCultureIgnoreCase) && Path.GetFileNameWithoutExtension(file).Contains("[Female]");

                            var targetModName = modpacks[sideloadername + (sortF ? "F" : sortM ? "M" : "")];

                            //get target path for the zipmod
                            var target = ManageSettings.GetCurrentGameModsPath()
                                + Path.DirectorySeparatorChar + targetModName //mod name
                                + Path.DirectorySeparatorChar + "mods" //mods dir
                                + Path.DirectorySeparatorChar + sideloadername // sideloader dir name
                                + file.Replace(dir, ""); // file subpath in sideloader dir

                            bool isTargetExists = File.Exists(target);
                            bool isSourceExists = File.Exists(file);

                            ManageStrings.CheckForLongPath(ref file);
                            ManageStrings.CheckForLongPath(ref target);

                            if (!isSourceExists)
                            {
                                continue;
                            }
                            else if (isTargetExists)
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
                                        + Path.DirectorySeparatorChar + targetModName + "_" + timeInfo //mod name
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
                                        + Path.DirectorySeparatorChar + targetModName + "_" + timeInfo //mod name
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

            progressForm.Dispose();
        }

        /// <summary>
        /// activate all mods with sideloader modpack inside mods folder
        /// </summary>
        private static void ActivateSideloaderMods(Dictionary<string, string> zipmodsGuidList = null)
        {
            bool getZipmodId = zipmodsGuidList != null;

            File.Copy(ManageSettings.GetCurrentMoProfileModlistPath(), ManageSettings.GetCurrentMoProfileModlistPath() + ".prezipmodsUpdate");

            var modlistContent = File.ReadAllLines(ManageSettings.GetCurrentMoProfileModlistPath());

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

                        if (getZipmodId)
                        {
                            foreach (var zipmod in Directory.EnumerateFiles(dir, "*.zip*", SearchOption.AllDirectories))
                            {
                                ManageModOrganizerMods.SaveGuidIfZipMod(zipmod, zipmodsGuidList);
                            }
                        }
                        else
                        {
                            break;
                        }

                    }
                }
            }

            File.WriteAllLines(ManageSettings.GetCurrentMoProfileModlistPath(), modlistContent);
        }

        private void PbDiscord_Click(object sender, EventArgs e)
        {
            Process.Start("https://discord.gg/rKbXzrnrMs");//Program's discord server
        }

        private void linkLabel1_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
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
                //check bleeding edge txt
                if (!File.Exists(ManageSettings.KkManagerStandaloneUpdaterExePath()))
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
                    if (CurrentGame.IsHaveSideloaderMods)
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