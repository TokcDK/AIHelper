﻿using AI_Girl_Helper.Games;
using AI_Girl_Helper.Manage;
using AI_Girl_Helper.Utils;
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

namespace AI_Girl_Helper
{
    public partial class AIGirlHelper : Form
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
        private static string ApplicationStartupPath { get => Properties.Settings.Default.ApplicationStartupPath; set => Properties.Settings.Default.ApplicationStartupPath = value; }
        //private static string ModOrganizerINIpath { get => Properties.Settings.Default.ModOrganizerINIpath; set => Properties.Settings.Default.ModOrganizerINIpath = value; }
        private static string Install2MODirPath { get => Properties.Settings.Default.Install2MODirPath; set => Properties.Settings.Default.Install2MODirPath = value; }
        private static bool MOmode { get => Properties.Settings.Default.MOmode; set => Properties.Settings.Default.MOmode = value; }

        Game CurrentGame;
        List<Game> ListOfGames;

        public AIGirlHelper()
        {
            InitializeComponent();

            ListOfGames = new List<Game>()
            {
                new AISyoujyo(),
                new AISyoujyoTrial(),
                new HoneySelect()
            };

            ListOfGames = ListOfGames.Where
                (game => 
                    Directory.Exists(game.GetGamePath())
                    && 
                    !FileFolderOperations.CheckDirectoryNotExistsOrEmpty_Fast(Path.Combine(game.GetGamePath(), "MO", "Profiles"))
                ).ToList();

            foreach (var game in ListOfGames)
            {
                CurrentGameComboBox.Items.Add(game.GetGameFolderName());
            }
            CurrentGame = new AISyoujyo();

            Properties.Settings.Default.CurrentGameEXEName = CurrentGame.GetGameEXEName();
            Properties.Settings.Default.CurrentGameFolderName = CurrentGame.GetGameFolderName();
            Properties.Settings.Default.StudioEXEName = CurrentGame.GetGameStudioEXEName();
            Properties.Settings.Default.INISettingsEXEName = CurrentGame.GetINISettingsEXEName();

            ApplicationStartupPath = Application.StartupPath;
            Properties.Settings.Default.CurrentGamePath = CurrentGame.GetGamePath();
            AppResDir = SettingsManage.GetAppResDir();
            ModsPath = SettingsManage.GetModsPath();
            DownloadsPath = SettingsManage.GetDownloadsPath();
            DataPath = SettingsManage.GetDataPath();
            MODirPath = SettingsManage.GetMOdirPath();
            MOexePath = SettingsManage.GetMOexePath();
            Properties.Settings.Default.ModOrganizerINIpath = SettingsManage.GetModOrganizerINIpath();
            Install2MODirPath = SettingsManage.GetInstall2MODirPath();
            OverwriteFolder = SettingsManage.GetOverwriteFolder();
            OverwriteFolderLink = SettingsManage.GetOverwriteFolderLink();
            SetupXmlPath = MOManage.GetSetupXmlPathForCurrentProfile();
            MOmode = true;

        }

        private void SetLocalizationStrings()
        {
            this.Text = T._("AI Girl Helper for Organized modpack");
            InstallInModsButton.Text = T._("Install from 2MO");
            //button1.Text = T._("Prepare the game");
            SettingsPage.Text = T._("Settings");
            CreateShortcutButton.Text = T._("Shortcut");
            FixRegistryButton.Text = T._("Registry");
            groupBox1.Text = T._("Display");
            FullScreenCheckBox.Text = T._("fullscreen");
            AutoShortcutRegistryCheckBox.Text = T._("Automatically create shortcut and fix registry if need");
            SettingsFoldersGroupBox.Text = T._("Folders");
            OpenGameFolderLinkLabel.Text = T._("Game");
            OpenModsFolderLinkLabel.Text = T._("Mods");
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

        int mode = 0;
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

            Other.CreateShortcuts();

            Other.MakeDummyFiles();

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
                    _ = label3.Invoke((Action)(() => label3.Text = T._("Extracting")));
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
                    _ = label3.Invoke((Action)(() => label3.Text = T._("Extracting")));
                    _ = ModsInfoLabel.Invoke((Action)(() => ModsInfoLabel.Text = T._("Game archive") + ": " + Path.GetFileNameWithoutExtension(AIGirlTrial)));
                    Compressor.Decompress(AIGirlTrial, DataPath);
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Blocks));
                }
                else if (File.Exists(AIGirl) && !File.Exists(Path.Combine(DataPath, "AI-Syoujyo.exe")))
                {
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Visible = true));
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Marquee));
                    _ = label3.Invoke((Action)(() => label3.Text = T._("Extracting")));
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
                            label3.Invoke((Action)(() => label3.Text = T._("Extracting") + " " + +i + "/" + files.Length));
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
                _ = label3.Invoke((Action)(() => label3.Text = "Compressing"));
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
                    && (File.Exists(Path.Combine(DataPath, SettingsManage.GetCurrentGameEXEName() + ".exe"))))
                {
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Visible = true));
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Marquee));
                    _ = label3.Invoke((Action)(() => label3.Text = "Compressing"));
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
                    foreach (string line in File.ReadAllLines(Path.Combine(MODirPath, "categories.dat")))
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
                        label3.Invoke((Action)(() => label3.Text = "Compressing " + i + "/" + dirs.Length));
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

                    label3.Invoke((Action)(() => label3.Text = "Compressing"));
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

            string categoryvalue = MOManage.GetMetaParameterValue(Path.Combine(inputmoddir, "meta.ini"), "category");
            if (categoryvalue.Replace("\"", string.Empty).Length == 0)
            {
            }
            else
            {

                //Subcategory from meta
                categoryvalue = categoryvalue.Replace("\"", string.Empty);//убрать кавычки
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

        private void AIGirlHelper_Load(object sender, EventArgs e)
        {
            SetLocalizationStrings();

            FoldersInit();

            //if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\illusion\AI-Syoujyo\AI-SyoujyoTrial", "INSTALLDIR", null) == null)
            //{
            //    FixRegistryButton.Visible = true;
            //}            

            SetTooltips();
        }

        private void SetTooltips()
        {
            //http://qaru.site/questions/47162/c-how-do-i-add-a-tooltip-to-a-control
            //THMainResetTableButton
            ToolTip THToolTip = new ToolTip
            {

                // Set up the delays for the ToolTip.
                AutoPopDelay = 20000,
                InitialDelay = 1000,
                ReshowDelay = 500,
                UseAnimation = true,
                UseFading = true,
                // Force the ToolTip text to be displayed whether or not the form is active.
                ShowAlways = true
            };

            //Main
            THToolTip.SetToolTip(button1, T._("Unpacking mods and resources from 'Downloads' and 'AI Girl Helper_RES' folders for game when they are not installed"));
            THToolTip.SetToolTip(InstallInModsButton, T._("Automatically get required mod data, converts and moves files from 2MO folder")
                + (MOmode ? T._(
                        " to MO format in Mods when possible"
                    ) : T._(
                        " to the game folder when possible"
                        )));
            THToolTip.SetToolTip(Install2MODirPathOpenFolderLinkLabel, T._("Open folder where you can drop/download files for autoinstallation"));
            THToolTip.SetToolTip(AutoShortcutRegistryCheckBox, T._("When checked will create shortcut for the AIGirl Helper manager on Desktop after mods extraction"));
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

            ResolutionComboBox.Text = XMLManage.ReadXmlValue(SetupXmlPath, "Setting/Size", ResolutionComboBox.Text);
            FullScreenCheckBox.Checked = bool.Parse(XMLManage.ReadXmlValue(SetupXmlPath, "Setting/FullScreen", FullScreenCheckBox.Checked.ToString().ToLower()));

            QualityComboBox.SelectedIndex = int.Parse(XMLManage.ReadXmlValue(SetupXmlPath, "Setting/Quality", "2"));
            
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

            XMLManage.ChangeXmlValue(SetupXmlPath, "Setting/Size", Resolution);
            XMLManage.ChangeXmlValue(SetupXmlPath, "Setting/Width", Resolution.Replace("(16 : 9)", string.Empty).Trim().Split('x')[0].Trim());
            XMLManage.ChangeXmlValue(SetupXmlPath, "Setting/Height", Resolution.ToString().Replace("(16 : 9)", string.Empty).Trim().Split('x')[1].Trim());
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

            XMLManage.ChangeXmlValue(SetupXmlPath, "Setting/Quality", quality);
        }

        private void FoldersInit()
        {
            if (File.Exists(SettingsManage.GetMOexePath() + ".GameInCommonModeNow") || File.Exists(SettingsManage.GetMOToStandartConvertationOperationsListFilePath()))
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

            string AIGirl = SettingsManage.GetCurrentGameEXEName();
            string AIGirlTrial = SettingsManage.GetCurrentGameEXEName();
            if (File.Exists(Path.Combine(SettingsManage.GetDataPath(), AIGirlTrial + ".exe")))
            {
                label3.Text = string.Format(T._("{0} game installed in {1}"), AIGirlTrial, "Data");
            }
            else if (File.Exists(Path.Combine(DataPath, AIGirl + ".exe")))
            {
                label3.Text = string.Format(T._("{0} game installed in {1}"), AIGirl, "Data");
            }
            else if (File.Exists(Path.Combine(AppResDir, AIGirlTrial + ".7z")))
            {
                label3.Text = string.Format(T._("{0} archive in {1}"), AIGirlTrial, "Data");
            }
            else if (File.Exists(Path.Combine(AppResDir, AIGirl + ".7z")))
            {
                label3.Text = string.Format(T._("{0} archive in {1}"), "AIGirl", "Data");
            }
            else if (Directory.Exists(DataPath))
            {
                label3.Text = string.Format(T._("{0} files not in {1}. Move {0} game files there."), AIGirl, "Data");
            }
            else
            {
                Directory.CreateDirectory(DataPath);
                label3.Text = string.Format(T._("{0} dir created. Move {1} game files there."), "Data", AIGirl);
            }

            if (MOmode)
            {
                string[] Archives7z;
                string[] ModDirs = Directory.GetDirectories(ModsPath, "*").Where(name => !name.EndsWith("_separator", StringComparison.OrdinalIgnoreCase)).ToArray();
                
                if (Directory.Exists(DownloadsPath))
                {
                    Archives7z = Directory.GetFiles(DownloadsPath, "*.7z", SearchOption.AllDirectories);
                    if (ModDirs.Length > 0 && Archives7z.Length > 0)
                    {
                        bool NotAllModsExtracted = false;
                        foreach (var Archive in Archives7z)
                        {
                            if (ModDirs.Contains(Path.Combine(ModsPath, Path.GetFileNameWithoutExtension(Archive))))
                            {
                            }
                            else
                            {
                                NotAllModsExtracted = true;
                                break;
                            }
                        }

                        if (compressmode && NotAllModsExtracted && ModDirs.Length < Archives7z.Length)
                        {
                            ModsInfoLabel.Text = T._("Not all mods in Mods dir");
                            //button1.Enabled = false;
                            mode = 2;
                            button1.Text = T._("Extract missing");
                        }
                        else
                        {
                            ModsInfoLabel.Text = T._("Found mod folders in Mods");
                            //button1.Enabled = false;
                            mode = 1;
                            button1.Text = T._("Mods Ready");
                            //MO2StandartButton.Enabled = true;
                            GetEnableDisableLaunchButtons();
                            MOCommonModeSwitchButton.Text = T._("MOToCommon");
                            AIGirlHelperTabControl.SelectedTab = LaunchTabPage;
                        }
                    }
                    else
                    {
                        //если нет папок модов но есть архивы в загрузках
                        if (Archives7z.Length > 0 && ModDirs.Length == 0)
                        {
                            ModsInfoLabel.Text = T._("Mods Ready for extract");
                            mode = 2;
                            button1.Text = T._("Extract mods");
                        }
                    }

                    //если нет архивов в загрузках, но есть папки модов
                    if (compressmode && Directory.Exists(DownloadsPath) && Directory.Exists(ModsPath))
                    {
                        if (ModDirs.Length > 0 && Archives7z.Length == 0)
                        {
                            if (Archives7z.Length == 0)
                            {
                                ModsInfoLabel.Text = "No archives in downloads";
                                button1.Text = "Pack mods";
                                mode = 0;
                            }
                        }
                    }
                }

                if (ModDirs.Length > 0 && File.Exists(Path.Combine(DataPath, SettingsManage.GetCurrentGameEXEName() + ".exe")))
                {
                    GetEnableDisableLaunchButtons();
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
                MOModsManage.BepinExLoadingFix();

                //создание exe-болванки
                Other.MakeDummyFiles();

                MOManage.SetModOrganizerINISettingsForTheGame();
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
            Properties.Settings.Default.CurrentGamePath = CurrentGame.GetGamePath();
            SetupXmlPath = MOManage.GetSetupXmlPathForCurrentProfile();
            ModsPath = SettingsManage.GetModsPath();
            DownloadsPath = SettingsManage.GetDownloadsPath();
            DataPath = SettingsManage.GetDataPath();
            MODirPath = SettingsManage.GetMOdirPath();
            MOexePath = SettingsManage.GetMOexePath();
            OverwriteFolder = SettingsManage.GetOverwriteFolder();
            OverwriteFolderLink = SettingsManage.GetOverwriteFolderLink();
            Properties.Settings.Default.ModOrganizerINIpath = SettingsManage.GetModOrganizerINIpath();
            Install2MODirPath = SettingsManage.GetInstall2MODirPath();
            AIGirlHelperTabControl.SelectedTab = LaunchTabPage;
            CurrentGameComboBox.Text = CurrentGame.GetGameFolderName();
            CurrentGameComboBox.SelectedIndex = SettingsManage.GetCurrentGameIndex();
            SetScreenSettings();
            if (AutoShortcutRegistryCheckBox.Checked)
            {
                Other.AutoShortcutAndRegystry();
            }
        }

        private void GetEnableDisableLaunchButtons()
        {
            MOButton.Enabled = File.Exists(SettingsManage.GetMOexePath());
            SettingsButton.Enabled = File.Exists(Path.Combine(DataPath, SettingsManage.GetINISettingsEXEName() + ".exe"));
            GameButton.Enabled = File.Exists(Path.Combine(DataPath, SettingsManage.GetCurrentGameEXEName() + ".exe"));
            StudioButton.Enabled = File.Exists(Path.Combine(DataPath, SettingsManage.GetStudioEXEName() + ".exe"));
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            Other.CheckBoxChangeColor(sender as CheckBox);
            Properties.Settings.Default.AutoShortcutRegistryCheckBoxChecked = AutoShortcutRegistryCheckBox.Checked;
        }

        private void ResolutionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetScreenResolution((sender as ComboBox).SelectedItem.ToString());
        }

        private void FullScreenCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Other.CheckBoxChangeColor(sender as CheckBox);
            XMLManage.ChangeXmlValue(SetupXmlPath, "Setting/FullScreen", (sender as CheckBox).Checked.ToString().ToLower());
        }

        private void FixRegistryButton_Click(object sender, EventArgs e)
        {
            FixRegistryButton.Enabled = false;

            RegistryManage.FixRegistry(false);

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
                RunProgram(MOexePath, "moshortcut://:" + SettingsManage.GetINISettingsEXEName());
            }
            else
            {
                RunProgram(Path.Combine(DataPath, SettingsManage.GetINISettingsEXEName() + ".exe"), string.Empty);
            }
            OnOffButtons();
        }

        private void GameButton_Click(object sender, EventArgs e)
        {
            OnOffButtons(false);
            if (MOmode)
            {
                RunProgram(MOexePath, "moshortcut://:" + SettingsManage.GetCurrentGameEXEName());
            }
            else
            {
                RunProgram(Path.Combine(DataPath, SettingsManage.GetCurrentGameEXEName() + ".exe"), string.Empty);
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
                RunProgram(MOexePath, "moshortcut://:" + SettingsManage.GetStudioEXEName());
            }
            else
            {
                RunProgram(Path.Combine(DataPath, SettingsManage.GetStudioEXEName() + ".exe"), string.Empty);
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

        readonly Dictionary<string, string> qualitylevels = new Dictionary<string, string>(3);
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
                LinksForm.Show();
            }
            else
            {
                newformButton.Text = @"\/";
                LinksForm.Close();
            }
        }

        private void AIGirlHelper_LocationChanged(object sender, EventArgs e)
        {
            //move second form with main form
            //https://stackoverflow.com/questions/3429445/how-to-move-two-windows-forms-together
            if (LinksForm == null || LinksForm.IsDisposed)
            {

            }
            else
            {
                LinksForm.Location = new Point(Bounds.Location.X + (Bounds.Width / 2) - (LinksForm.Width / 2),
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
            UnpackArchives(Install2MODirPath, "rar");
            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage + "."));
            UnpackArchives(Install2MODirPath, "7z");
            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage + ".."));
            MOModsManage.InstallCsScriptsForScriptLoader();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage + "..."));
            MOModsManage.InstallZipArchivesToMods();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage));
            MOModsManage.InstallBepinExModsToMods();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage + "."));
            MOModsManage.InstallZipModsToMods();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage + ".."));
            MOModsManage.InstallCardsFrom2MO();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage + "..."));
            MOModsManage.InstallModFilesFromSubfolders();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage));
            MOModsManage.InstallImagesFromSubfolders();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage + "."));
            FileFolderOperations.DeleteEmptySubfolders(Install2MODirPath, false);

            if (!Directory.Exists(Install2MODirPath))
            {
                Directory.CreateDirectory(Install2MODirPath);
            }

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = T._("Install from 2MO")));
        }

        private void UnpackArchives(string dirForSearch, string extension)
        {
            if (dirForSearch.Length > 0 && extension.Length > 0 && Directory.Exists(dirForSearch))
            {
                foreach (var file in Directory.GetFiles(dirForSearch, "*." + extension, SearchOption.AllDirectories))
                {
                    string targetDir = Path.Combine(Install2MODirPath, Path.GetFileNameWithoutExtension(file));
                    if (!Directory.Exists(targetDir))
                    {
                        try
                        {
                            Compressor.Decompress(file, targetDir);
                            //File.Delete(file);
                            File.Move(file, file + ".ExtractedAnMustBeInstalled");
                        }
                        catch
                        {
                            //if decompression failed
                        }
                    }
                }
            }
        }

        private void CreateShortcutButton_Click(object sender, EventArgs e)
        {
            Other.CreateShortcuts(true, false);
        }

        private void MO2StandartButton_Click(object sender, EventArgs e)
        {
            OnOffButtons(false);
            if (MOmode)
            {
                DialogResult result = MessageBox.Show(T._("This will move all mod files from Mods folder to Data folder to make it like common installation variant. You can restore it later back to MO mode. Continue?"), T._("Confirmation"), MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    MOmode = false;
                    MOCommonModeSwitchButton.Enabled = false;
                    LanchModeInfoLinkLabel.Enabled = false;

                    string[] EnabledModsList = MOManage.GetModsListFromActiveMOProfile();
                    int EnabledModsLength = EnabledModsList.Length;

                    if (EnabledModsList.Length == 0)
                    {
                        MOmode = true;
                        MOCommonModeSwitchButton.Enabled = true;
                        MessageBox.Show(T._("There is no enabled mods in Mod Organizer"));
                        return;
                    }

                    MOModsManage.CleanBepInExLinksFromData();

                    if (File.Exists(SettingsManage.GetDummyFilePath()))
                    {
                        File.Delete(SettingsManage.GetDummyFilePath());
                    }

                    if (!Directory.Exists(SettingsManage.GetMOmodeDataFilesBakDirPath()))
                    {
                        Directory.CreateDirectory(SettingsManage.GetMOmodeDataFilesBakDirPath());
                    }
                    StringBuilder Operations = new StringBuilder();

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
                                string FileInBakFolderWhichIsInRES = FileInDataFolder.Replace(DataPath, SettingsManage.GetMOmodeDataFilesBakDirPath());
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
                                        string FileInBakFolderWhichIsInRES = FileInDataFolder.Replace(DataPath, SettingsManage.GetMOmodeDataFilesBakDirPath());
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

                    File.WriteAllText(SettingsManage.GetMOToStandartConvertationOperationsListFilePath(), Operations.ToString());
                    string[] DataWithModsFileslist = Directory.GetFiles(DataPath, "*.*", SearchOption.AllDirectories);
                    File.WriteAllLines(SettingsManage.GetModdedDataFilesListFilePath(), DataWithModsFileslist);
                    File.WriteAllLines(SettingsManage.GetVanillaDataFilesListFilePath(), DataWithModsFileslist);

                    //Directory.Delete(ModsPath, true);
                    //Directory.Move(MODirPath, Path.Combine(AppResDir, Path.GetFileName(MODirPath)));
                    MOCommonModeSwitchButton.Text = T._("CommonToMO");
                    MOCommonModeSwitchButton.Enabled = true;
                    LanchModeInfoLinkLabel.Enabled = true;
                    File.Move(Path.Combine(MODirPath, "ModOrganizer.exe"), Path.Combine(MODirPath, "ModOrganizer.exe.GameInCommonModeNow"));
                    //обновление информации о конфигурации папок игры
                    FoldersInit();
                    MessageBox.Show(T._("All mod files now in Data folder! You can restore MO mode by same button."));
                }
            }
            else
            {
                DialogResult result = MessageBox.Show(T._("This will move all mod files back to Mods folder from Data and will switch to MO mode. Continue?"), T._("Confirmation"), MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    MOmode = true;
                    MOCommonModeSwitchButton.Enabled = false;
                    LanchModeInfoLinkLabel.Enabled = false;

                    string[] Operations = File.ReadAllLines(SettingsManage.GetMOToStandartConvertationOperationsListFilePath());
                    string[] VanillaDataFiles = File.ReadAllLines(SettingsManage.GetVanillaDataFilesListFilePath());
                    string[] ModdedDataFiles = File.ReadAllLines(SettingsManage.GetModdedDataFilesListFilePath());

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

                            bool IsForOverwriteFolder = StringEx.IsStringAContainsStringB(TargetFolderPath, OverwriteFolder);
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
                            MOManage.WriteMetaINI(
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
                            MOManage.ActivateInsertModIfPossible(NewModName, false, ModName, false);
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
                        MOManage.WriteMetaINI(
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

                        MOManage.ActivateInsertModIfPossible(addedFilesFolderName);
                    }

                    //перемещение ванильных файлов назад в дата
                    if (Directory.Exists(SettingsManage.GetMOmodeDataFilesBakDirPath()))
                    {
                        string[] FilesInMOmodeDataFilesBak = Directory.GetFiles(SettingsManage.GetMOmodeDataFilesBakDirPath(), "*.*", SearchOption.AllDirectories);
                        int FilesInMOmodeDataFilesBakLength = FilesInMOmodeDataFilesBak.Length;
                        for (int f = 0; f < FilesInMOmodeDataFilesBakLength; f++)
                        {
                            string DestFileInDataFolderPath = FilesInMOmodeDataFilesBak[f].Replace(SettingsManage.GetMOmodeDataFilesBakDirPath(), DataPath);
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
                        FileFolderOperations.DeleteEmptySubfolders(SettingsManage.GetMOmodeDataFilesBakDirPath());
                    }

                    //чистка файлов-списков
                    File.Delete(SettingsManage.GetMOToStandartConvertationOperationsListFilePath());
                    File.Delete(SettingsManage.GetVanillaDataFilesListFilePath());
                    File.Delete(SettingsManage.GetModdedDataFilesListFilePath());

                    //очистка пустых папок в Data
                    FileFolderOperations.DeleteEmptySubfolders(DataPath, false);
                    try
                    {
                        //восстановление 2х папок, что были по умолчанию сначала пустыми
                        Directory.CreateDirectory(Path.Combine(DataPath, "UserData", "audio"));
                        Directory.CreateDirectory(Path.Combine(DataPath, "UserData", "coordinate", "male"));
                    }
                    catch
                    {
                    }

                    File.Move(Path.Combine(MODirPath, "ModOrganizer.exe.GameInCommonModeNow"), Path.Combine(MODirPath, "ModOrganizer.exe"));

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
