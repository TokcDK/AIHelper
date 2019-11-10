using Microsoft.Win32;
using SymbolicLinkSupport;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
//using Crc32C;

namespace AI_Girl_Helper
{
    public partial class AIGirlHelper : Form
    {
        private readonly bool compressmode = false;

        //constants
        private static readonly string AppResDir = Path.Combine(Application.StartupPath, "AI Girl Helper_RES");
        private static string ModsPath = Path.Combine(Application.StartupPath, "Mods");
        private static string DownloadsPath = Path.Combine(Application.StartupPath, "Downloads");
        private static string DataPath = Path.Combine(Application.StartupPath, "Data");
        private static string MODirPath = Path.Combine(Application.StartupPath, "MO");
        private static string MOexePath = Path.Combine(MODirPath, "ModOrganizer.exe");
        private static string OverwriteFolder = Path.Combine(MODirPath, "overwrite");
        private static string OverwriteFolderLink = Path.Combine(Application.StartupPath, "MOUserData");
        private static string SetupXmlPath;
        private static bool MOmode = true;

        public AIGirlHelper()
        {
            InitializeComponent();

            //SetupXmlPath = GetSetupXmlPathForCurrentProfile();
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

            CreateShortcuts();

            MakeDummyFiles();

            button1.Text = T._("Game Ready");
            FoldersInit();
        }

        private readonly string dummyfile = Path.Combine(Application.StartupPath, "TESV.exe");
        private void MakeDummyFiles()
        {
            //Create dummy file and add hidden attribute
            if (!File.Exists(dummyfile))
            {
                File.WriteAllText(dummyfile, "dummy file need to execute mod organizer");
                HideFileFolder(dummyfile, true);
            }
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

        private void BepinExLoadingFix()
        {
            if (MOmode)
            {
                string objectpath = Path.Combine(ModsPath, "BepInEx5", "Bepinex", "core", "BepInEx.Preloader.dll");
                string linkpath = Path.Combine(DataPath, "Bepinex", "core", "BepInEx.Preloader.dll");
                if (!File.Exists(linkpath) && File.Exists(objectpath))
                {
                    Symlink(linkpath
                      , objectpath
                      , true
                      );
                    HideFileFolder(Path.Combine(DataPath, "Bepinex"));
                }

                linkpath = Path.Combine(DataPath, "doorstop_config.ini");
                objectpath = Path.Combine(ModsPath, "BepInEx5", "doorstop_config.ini");
                if (!File.Exists(linkpath) && File.Exists(objectpath))
                {
                    Symlink(linkpath
                      , objectpath
                      , true
                      );
                    HideFileFolder(linkpath, true);
                }

                linkpath = Path.Combine(DataPath, "winhttp.dll");
                objectpath = Path.Combine(ModsPath, "BepInEx5", "winhttp.dll");
                if (!File.Exists(linkpath) && File.Exists(objectpath))
                {
                    Symlink(linkpath
                      , objectpath
                      , true
                      );
                    HideFileFolder(linkpath, true);
                }

                linkpath = Path.Combine(DataPath, "UserData", "MaterialEditor");
                objectpath = Path.Combine(ModsPath, "MyUserData", "UserData", "MaterialEditor");
                if (!Directory.Exists(linkpath) && Directory.Exists(objectpath))
                {
                    Symlink(linkpath
                      , objectpath
                      , true
                      );
                    HideFileFolder(linkpath);

                }

                linkpath = Path.Combine(DataPath, "UserData", "Overlays");
                objectpath = Path.Combine(ModsPath, "MyUserData", "UserData", "Overlays");
                if (!Directory.Exists(linkpath) && Directory.Exists(objectpath))
                {
                    Symlink(linkpath
                      , objectpath
                      , true
                      );
                    HideFileFolder(linkpath);

                }
            }
        }

        private void HideFileFolder(string path, bool IsFile = false)
        {
            if (IsFile)
            {
                if (File.Exists(path))
                {
                    File.SetAttributes(path, FileAttributes.Hidden);
                }
            }
            else
            {
                if (Directory.Exists(path))
                {
                    _ = new DirectoryInfo(path)
                    {
                        Attributes = FileAttributes.Hidden
                    };
                }
            }

        }

        private void Symlink(string symlink, string objectFileDir, bool isRelative = false)
        {
            if (File.Exists(symlink))
            {
            }
            else
            {
                string parentdirpath = Path.GetDirectoryName(symlink);
                if (Directory.Exists(parentdirpath))
                {
                }
                else
                {
                    Directory.CreateDirectory(parentdirpath);
                }
                if (File.Exists(objectFileDir))
                {
                    FileInfoExtensions.CreateSymbolicLink(new FileInfo(objectFileDir), symlink, isRelative);//new from NuGet package
                    //CreateSymlink.File(file, symlink); //old
                }
                else if (Directory.Exists(objectFileDir))
                {
                    DirectoryInfoExtensions.CreateSymbolicLink(new DirectoryInfo(objectFileDir), symlink, isRelative);//new from NuGet package
                    //CreateSymlink.Folder(file, symlink); //old
                }
            }
        }

        //https://bytescout.com/blog/create-shortcuts-in-c-and-vbnet.html
        private void CreateShortcuts(bool force = false, bool auto = true)
        {
            if (AutoShortcutRegistryCheckBox.Checked || force)
            {
                //AI-Girl Helper
                string shortcutname = GetCurrentGameName() + " " + T._("Helper");
                string targetpath = Application.ExecutablePath;
                string arguments = string.Empty;
                string workingdir = Path.GetDirectoryName(targetpath);
                string description = T._("Run") + " " + shortcutname;
                string iconlocation = Application.ExecutablePath;
                Shortcut.Create(shortcutname, targetpath, arguments, workingdir, description, iconlocation);

                //AI-Girl Trial
                //shortcutname = "AI-Girl Trial";
                //targetpath = Path.Combine(MOPath, "ModOrganizer.exe");
                //arguments = "\"moshortcut://:AI-SyoujyoTrial\"";
                //workingdir = Path.GetDirectoryName(targetpath);
                //description = "Run " + shortcutname + " with ModOrganizer";
                //iconlocation = Path.Combine(DataPath, "AI-SyoujyoTrial.exe");
                //Shortcut.Create(shortcutname, targetpath, arguments, workingdir, description, iconlocation);

                ////Mod Organizer
                //shortcutname = "ModOrganizer AI-Shoujo Trial";
                //targetpath = Path.Combine(MOPath, "ModOrganizer.exe");
                //arguments = string.Empty;
                //workingdir = Path.GetDirectoryName(targetpath);
                //description = shortcutname;
                //Shortcut.Create(shortcutname, targetpath, arguments, workingdir, description);

                if (!Directory.Exists(OverwriteFolderLink) && Directory.Exists(OverwriteFolder))
                {
                    CreateSymlink.Folder(OverwriteFolder, OverwriteFolderLink);
                }
                if (!auto)
                {
                    MessageBox.Show(T._("Shortcut") + " " + T._("created") + "!");
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
                    && (File.Exists(Path.Combine(DataPath, GetCurrentGameName() + ".exe"))))
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

            string categoryvalue = GetMetaParameterValue(Path.Combine(inputmoddir, "meta.ini"), "category");
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

        private string GetMetaParameterValue(string MetaFilePath, string NeededValue)
        {
            foreach (string line in File.ReadAllLines(MetaFilePath))
            {
                if (line.Length == 0)
                {
                }
                else
                {
                    if (line.StartsWith(NeededValue + "="))
                    {
                        return line.Remove(0, (NeededValue + "=").Length);
                    }
                }
            }

            return string.Empty;
        }

        private void AIGirlHelper_Load(object sender, EventArgs e)
        {
            SetLocalizationStrings();

            FoldersInit();

            //if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\illusion\AI-Syoujyo\AI-SyoujyoTrial", "INSTALLDIR", null) == null)
            //{
            //    FixRegistryButton.Visible = true;
            //}
            SetScreenSettings();

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

            ResolutionComboBox.Text = ReadXmlValue(SetupXmlPath, "Setting/Size", ResolutionComboBox.Text);
            FullScreenCheckBox.Checked = bool.Parse(ReadXmlValue(SetupXmlPath, "Setting/FullScreen", FullScreenCheckBox.Checked.ToString().ToLower()));

            QualityComboBox.SelectedIndex = int.Parse(ReadXmlValue(SetupXmlPath, "Setting/Quality", "2"));

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

            ChangeXmlValue(SetupXmlPath, "Setting/Size", Resolution);
            ChangeXmlValue(SetupXmlPath, "Setting/Width", Resolution.Replace("(16 : 9)", string.Empty).Trim().Split('x')[0].Trim());
            ChangeXmlValue(SetupXmlPath, "Setting/Height", Resolution.ToString().Replace("(16 : 9)", string.Empty).Trim().Split('x')[1].Trim());
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

            ChangeXmlValue(SetupXmlPath, "Setting/Quality", quality);
        }

        private void FoldersInit()
        {
            if (File.Exists(Path.Combine(MODirPath, "ModOrganizer.exe.GameInCommonModeNow")) || File.Exists(MOToStandartConvertationOperationsListFile))
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

            string AIGirl = GetCurrentGameName();
            string AIGirlTrial = GetCurrentGameName();
            if (File.Exists(Path.Combine(DataPath, AIGirlTrial + ".exe")))
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
                string[] ModDirs = Directory.GetDirectories(ModsPath, "*").Where(name => !name.EndsWith("_separator", StringComparison.OrdinalIgnoreCase)).ToArray();
                string[] Archives7z = Directory.GetFiles(DownloadsPath, "*.7z", SearchOption.AllDirectories);
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

                if (ModDirs.Length > 0 && File.Exists(Path.Combine(DataPath, GetCurrentGameName() + ".exe")))
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
                BepinExLoadingFix();

                //создание exe-болванки
                MakeDummyFiles();

                SetModOrganizerINISettingsForTheGame();
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
            SetupXmlPath = GetSetupXmlPathForCurrentProfile();
            ModsPath = Path.Combine(GetSelectedGameFolderPath(), "Mods");
            DownloadsPath = Path.Combine(GetSelectedGameFolderPath(), "Downloads");
            DataPath = Path.Combine(GetSelectedGameFolderPath(), "Data");
            MODirPath = Path.Combine(Application.StartupPath, "MO");
            MOexePath = Path.Combine(MODirPath, "ModOrganizer.exe");
            OverwriteFolder = GetOverwriteFolderLocation();
            OverwriteFolderLink = Path.Combine(GetSelectedGameFolderPath(), "MOUserData");
            ModOrganizerINIpath = Path.Combine(MODirPath, "ModOrganizer.ini");
            Install2MODirPath = Path.Combine(GetSelectedGameFolderPath(), "2MO");
            AIGirlHelperTabControl.SelectedTab = LaunchTabPage;

            if (AutoShortcutRegistryCheckBox.Checked)
            {
                AutoShortcutAndRegystry();
            }
        }

        private void AutoShortcutAndRegystry()
        {
            CreateShortcuts();
            FixRegistry();
        }

        private string ModOrganizerINIpath = Path.Combine(MODirPath, "ModOrganizer.ini");

        private string GetOverwriteFolderLocation()
        {
            string IniValue = GetINIValueIfExist(ModOrganizerINIpath, "overwrite_directory", "Settings");

            if (IniValue.Length > 0)
            {
                return IniValue;
            }
            else
            {
                return Path.Combine(MODirPath, "overwrite");
            }

        }

        private void GetEnableDisableLaunchButtons()
        {
            MOButton.Enabled = File.Exists(Path.Combine(MODirPath, "ModOrganizer.exe")) ? true : false;
            SettingsButton.Enabled = File.Exists(Path.Combine(DataPath, "InitSetting.exe")) ? true : false;
            GameButton.Enabled = File.Exists(Path.Combine(DataPath, GetCurrentGameName() + ".exe")) ? true : false;
            StudioButton.Enabled = File.Exists(Path.Combine(DataPath, "StudioNEOV2.exe")) ? true : false;
        }

        private void SetModOrganizerINISettingsForTheGame()
        {
            Utils.IniFile INI = new Utils.IniFile(ModOrganizerINIpath);

            //General
            string IniValue = GetSelectedGameFolderPath().Replace(@"\", @"\\");
            if (GetINIValueIfExist(ModOrganizerINIpath, "gamePath", "General") != IniValue)
            {
                INI.WriteINI("General", "gamePath", IniValue);
            }
            //customExecutables
            IniValue = GetCurrentGameName().Replace(@"\", @"\\");
            if (GetINIValueIfExist(ModOrganizerINIpath, @"1\title", "customExecutables") != IniValue)
            {
                INI.WriteINI("customExecutables", @"1\title", IniValue);
            }
            IniValue = Path.Combine(DataPath, GetCurrentGameName() + ".exe").Replace(@"\", @"\\");
            if (GetINIValueIfExist(ModOrganizerINIpath, @"1\binary", "customExecutables") != IniValue)
            {
                INI.WriteINI("customExecutables", @"1\binary", IniValue);
            }
            IniValue = DataPath.Replace(@"\", @"\\");
            if (GetINIValueIfExist(ModOrganizerINIpath, @"1\workingDirectory", "customExecutables") != IniValue)
            {
                INI.WriteINI("customExecutables", @"1\workingDirectory", IniValue);
            }
            IniValue = GetSettingsEXEPath().Replace(@"\", @"\\");
            if (GetINIValueIfExist(ModOrganizerINIpath, @"2\binary", "customExecutables") != IniValue)
            {
                INI.WriteINI("customExecutables", @"2\binary", IniValue);
            }
            IniValue = GetSettingsEXEPath().Replace(@"\", @"\\");
            if (GetINIValueIfExist(ModOrganizerINIpath, @"2\binary", "customExecutables") != IniValue)
            {
                INI.WriteINI("customExecutables", @"2\binary", IniValue);
            }
            IniValue = DataPath.Replace(@"\", @"\\");
            if (GetINIValueIfExist(ModOrganizerINIpath, @"2\workingDirectory", "customExecutables") != IniValue)
            {
                INI.WriteINI("customExecutables", @"2\workingDirectory", IniValue);
            }
            IniValue = Path.Combine(MODirPath, "explorer++", "Explorer++.exe").Replace(@"\", @"\\");
            if (GetINIValueIfExist(ModOrganizerINIpath, @"3\binary", "customExecutables") != IniValue)
            {
                INI.WriteINI("customExecutables", @"3\binary", IniValue);
            }
            IniValue = "\\\"" + DataPath.Replace(@"\", @"\\") + "\\\"";
            if (GetINIValueIfExist(ModOrganizerINIpath, @"3\arguments", "customExecutables") != IniValue)
            {
                INI.WriteINI("customExecutables", @"3\arguments", IniValue);
            }
            IniValue = Path.Combine(MODirPath, "explorer++").Replace(@"\", @"\\");
            if (GetINIValueIfExist(ModOrganizerINIpath, @"3\workingDirectory", "customExecutables") != IniValue)
            {
                INI.WriteINI("customExecutables", @"3\workingDirectory", IniValue);
            }
            IniValue = Path.Combine(GetSelectedGameFolderPath(), "TESV.exe").Replace(@"\", @"\\");
            if (GetINIValueIfExist(ModOrganizerINIpath, @"4\binary", "customExecutables") != IniValue)
            {
                INI.WriteINI("customExecutables", @"4\binary", IniValue);
            }
            IniValue = GetSelectedGameFolderPath().Replace(@"\", @"\\");
            if (GetINIValueIfExist(ModOrganizerINIpath, @"4\workingDirectory", "customExecutables") != IniValue)
            {
                INI.WriteINI("customExecutables", @"4\workingDirectory", IniValue);
            }
            //Settings
            IniValue = ModsPath.Replace(@"\", @"\\");
            if (GetINIValueIfExist(ModOrganizerINIpath, "mod_directory", "Settings") != IniValue)
            {
                INI.WriteINI("Settings", "mod_directory", IniValue);
            }
            IniValue = Path.Combine(GetSelectedGameFolderPath(), "Downloads").Replace(@"\", @"\\");
            if (GetINIValueIfExist(ModOrganizerINIpath, "download_directory", "Settings") != IniValue)
            {
                INI.WriteINI("Settings", "download_directory", IniValue);
            }

            if (CultureInfo.CurrentCulture.Name == "ru-RU")
            {
                IniValue = "ru";
                if (GetINIValueIfExist(ModOrganizerINIpath, "language", "Settings") != IniValue)
                {
                    INI.WriteINI("Settings", "language", IniValue);
                }
            }
            else
            {
                IniValue = "en";
                if (GetINIValueIfExist(ModOrganizerINIpath, "language", "Settings") != IniValue)
                {
                    INI.WriteINI("Settings", "language", IniValue);
                }
            }
        }

        private string GetSettingsEXEPath()
        {
            return Path.Combine(DataPath, "InitSetting.exe");
        }

        private string GetSelectedGameFolderPath()
        {
            return Application.StartupPath;
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBoxChangeColor(sender as CheckBox);
        }

        private void CheckBoxChangeColor(CheckBox checkBox)
        {
            if (checkBox.Checked)
            {
                checkBox.ForeColor = Color.FromArgb(192, 255, 192);
            }
            else
            {
                checkBox.ForeColor = Color.White;
            }
        }

        private void ResolutionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetScreenResolution((sender as ComboBox).SelectedItem.ToString());
        }

        private void FullScreenCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBoxChangeColor(sender as CheckBox);
            ChangeXmlValue(SetupXmlPath, "Setting/FullScreen", (sender as CheckBox).Checked.ToString().ToLower());
        }

        private void ChangeXmlValue(string xmlpath, string nodename, string value)
        {
            //https://stackoverflow.com/questions/2137957/update-value-in-xml-file
            XmlDocument xmlDoc = new XmlDocument();
            if (File.Exists(xmlpath))
            {
                xmlDoc.Load(xmlpath);
            }
            else
            {
                //если не был создан, создать с нуля
                xmlDoc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-16\"?>" + Environment.NewLine + "<Setting>" + Environment.NewLine + "  <Size>1280 x 720 (16 : 9)</Size>" + Environment.NewLine + "  <Width>1280</Width>" + Environment.NewLine + "  <Height>720</Height>" + Environment.NewLine + "  <Quality>2</Quality>" + Environment.NewLine + "  <FullScreen>false</FullScreen>" + Environment.NewLine + "  <Display>0</Display>" + Environment.NewLine + "  <Language>0</Language>" + Environment.NewLine + "</Setting>");
            }

            XmlNode node = xmlDoc.SelectSingleNode(nodename);

            if (node == null || node.InnerText == value)
            {
            }
            else
            {
                node.InnerText = value;

                xmlDoc.Save(xmlpath);
            }
        }

        private string ReadXmlValue(string xmlpath, string nodename, string defaultresult)
        {
            if (File.Exists(xmlpath))
            {
                //https://stackoverflow.com/questions/2137957/update-value-in-xml-file
                XmlDocument xmlDoc = new XmlDocument();

                xmlDoc.Load(xmlpath);

                XmlNode node = xmlDoc.SelectSingleNode(nodename);

                if (node == null || node.InnerText == defaultresult)
                {
                }
                else
                {
                    return node.InnerText;
                }
            }
            return defaultresult;
        }

        private void FixRegistryButton_Click(object sender, EventArgs e)
        {
            FixRegistryButton.Enabled = false;

            FixRegistry(false);

            FixRegistryButton.Enabled = true;
        }

        private void FixRegistry(bool auto = true)
        {
            string GameName = GetCurrentGameName();
            string RegystryPath = @"HKEY_CURRENT_USER\Software\illusion\" + GameName + @"\" + GameName;
            var InstallDirValue = Registry.GetValue(RegystryPath, "INSTALLDIR", null);
            if (InstallDirValue == null || InstallDirValue.ToString() != DataPath)
            {
                Registry.SetValue(RegystryPath, "INSTALLDIR", DataPath);
                if (!auto)
                {
                    MessageBox.Show(T._("Registry fixed! Install dir was set to Data dir."));
                }
            }
            else
            {
                if (!auto)
                {
                    MessageBox.Show(T._("Registry was already fixed"));
                }
            }
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
                RunProgram(MOexePath, "moshortcut://:InitSetting");
            }
            else
            {
                RunProgram(Path.Combine(DataPath, "InitSetting.exe"), string.Empty);
            }
            OnOffButtons();
        }

        private void GameButton_Click(object sender, EventArgs e)
        {
            OnOffButtons(false);
            if (MOmode)
            {
                RunProgram(MOexePath, "moshortcut://:" + GetCurrentGameName());
            }
            else
            {
                RunProgram(Path.Combine(DataPath, GetCurrentGameName() + ".exe"), string.Empty);
            }
            OnOffButtons();
        }

        private void OnOffButtons(bool SwitchOn = true)
        {
            AIGirlHelperTabControl.Enabled = SwitchOn;
        }

        private string GetCurrentGameName()
        {
            if (File.Exists(Path.Combine(DataPath, "AI-SyoujyoTrial.exe")))
            {
                return "AI-SyoujyoTrial";
            }
            else if (File.Exists(Path.Combine(DataPath, "AI-Syoujyo.exe")))
            {
                return "AI-Syoujyo";
            }
            return string.Empty;
        }

        private void StudioButton_Click(object sender, EventArgs e)
        {
            OnOffButtons(false);
            if (MOmode)
            {
                RunProgram(MOexePath, "moshortcut://:" + "StudioNEOV2");
            }
            else
            {
                RunProgram(Path.Combine(DataPath, "StudioNEOV2.exe"), string.Empty);
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

        string Install2MODirPath = Path.Combine(Application.StartupPath, "2MO");
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
            InstallCsScriptsForScriptLoader();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage + "..."));
            InstallZipArchivesToMods();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage));
            InstallBepinExModsToMods();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage + "."));
            InstallZipModsToMods();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage + ".."));
            InstallCardsFrom2MO();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage + "..."));
            InstallModFilesFromSubfolders();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage));
            InstallImagesFromSubfolders();

            InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = InstallMessage + "."));
            DeleteEmptySubfolders(Install2MODirPath, false);

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

        private void InstallCsScriptsForScriptLoader(string WhereFromInstallDir = "")
        {
            WhereFromInstallDir = WhereFromInstallDir.Length > 0 ? WhereFromInstallDir : Install2MODirPath;

            string[] csFiles = Directory.GetFiles(WhereFromInstallDir, "*.cs");

            if (csFiles.Length > 0)
            {
                foreach (var csFile in csFiles)
                {
                    string name = Path.GetFileNameWithoutExtension(csFile);
                    string author = string.Empty;
                    string description = string.Empty;
                    string modname = "[script]" + name;
                    string moddir = Path.GetDirectoryName(csFile).Replace(WhereFromInstallDir, Path.Combine(ModsPath, modname));

                    using (StreamReader sReader = new StreamReader(csFile))
                    {
                        string Line;
                        bool readDescriptionMode = false;
                        int i = 0;
                        while (!sReader.EndOfStream || (!readDescriptionMode && i == 10))
                        {
                            Line = sReader.ReadLine();

                            if (!readDescriptionMode /*&& Line.Length > 0 уже есть эта проверка в IsStringAContainsStringB*/ && IsStringAContainsStringB(Line, "/*"))
                            {
                                readDescriptionMode = true;
                                Line = Line.Replace("/*", string.Empty);
                                if (Line.Length > 0)
                                {
                                    description += Line + "<br>";
                                }
                            }
                            else
                            {
                                if (IsStringAContainsStringB(Line, "*/"))
                                {
                                    readDescriptionMode = false;
                                    Line = Line.Replace("*/", string.Empty);
                                }

                                description += Line + "<br>";

                                if (!readDescriptionMode)
                                {
                                    break;
                                }
                            }

                            i++;
                        }
                    }
                    string scriptsdir = Path.Combine(moddir, "scripts");
                    if (!Directory.Exists(scriptsdir))
                    {
                        Directory.CreateDirectory(scriptsdir);
                    }

                    string FileTargetPath = Path.Combine(scriptsdir, name + ".cs");
                    bool IsUpdate = false;
                    //резервная копия, если файл существовал
                    if (File.Exists(FileTargetPath))
                    {
                        IsUpdate = true;
                        if (File.GetLastWriteTime(csFile) > File.GetLastWriteTime(FileTargetPath))
                        {
                            File.Delete(FileTargetPath);
                            File.Move(csFile, FileTargetPath);
                        }
                        else
                        {
                            File.Delete(csFile);
                        }
                    }
                    else
                    {
                        File.Move(csFile, FileTargetPath);
                    }

                    string FileLastModificationTime = File.GetLastWriteTime(csFile).ToString("yyyyMMddHHmm");
                    //запись meta.ini
                    WriteMetaINI(
                        moddir
                        ,
                        IsUpdate ? string.Empty : "86,"
                        ,
                        "0." + FileLastModificationTime
                        ,
                        IsUpdate ? string.Empty : "Requires: " + "ScriptLoader"
                        ,
                        IsUpdate ? string.Empty : "<br>" + "Author" + ": " + author + "<br><br>" + (description.Length > 0 ? description : name)
                        );

                    ActivateInsertModIfPossible(modname, false, "ScriptLoader scripts_separator");

                    string[] extrafiles = Directory.GetFiles(WhereFromInstallDir, name+"*.*");
                    if (extrafiles.Length>0)
                    {
                        foreach(var extrafile in extrafiles)
                        {
                            string targetFile = extrafile.Replace(WhereFromInstallDir, moddir);

                            if (File.Exists(targetFile))
                            {
                                if (File.GetLastWriteTime(extrafile)> File.GetLastWriteTime(targetFile))
                                {
                                    File.Delete(targetFile);
                                    File.Move(extrafile, targetFile);
                                }
                                else
                                {
                                    File.Delete(extrafile);
                                }
                            }
                            else
                            {
                                File.Move(extrafile, targetFile);
                            }
                        }
                    }
                }
            }
        }

        private void InstallCardsFrom2MO(string TargetDir = "")
        {
            string ProceedDir = TargetDir.Length == 0 ? Install2MODirPath : TargetDir;
            var images = Directory.GetFiles(ProceedDir, "*.png");
            int imagesLength = images.Length;
            if (imagesLength > 0)
            {
                //bool IsCharaCard = false;
                for (int imgnum = 0; imgnum < imagesLength; imgnum++)
                {
                    string img = images[imgnum];
                    //var imgdata = Image.FromFile(img);

                    //if (imgdata.Width == 252 && imgdata.Height == 352)
                    //{
                    //    IsCharaCard = true;
                    //}

                    var targetImagePath = GetResultTargetFilePathWithNameCheck(GetUserDataSubFolder(" Chars", "f"), Path.GetFileNameWithoutExtension(img), ".png");

                    File.Move(img, targetImagePath);
                }
            }
        }

        private void InstallImagesFromSubfolders()
        {
            foreach (var dir in Directory.GetDirectories(Install2MODirPath, "*"))
            {
                if (string.Compare(Path.GetFileName(dir), "f", true) == 0)
                {
                    //папка "f" с женскими карточками внутри
                    MoveImagesToTargetContentFolder(dir, "f");
                }
                else if (string.Compare(Path.GetFileName(dir), "m", true) == 0)
                {
                    //папка "m" с мужскими карточками внутри
                    MoveImagesToTargetContentFolder(dir, "m");
                }
                else if (string.Compare(Path.GetFileName(dir), "c", true) == 0)
                {
                    //папка "c" с координатами
                    MoveImagesToTargetContentFolder(dir, "c");
                }
                else if (string.Compare(Path.GetFileName(dir), "cf", true) == 0)
                {
                    //папка "cf" с передними фреймами
                    MoveImagesToTargetContentFolder(dir, "cf");
                }
                else if (string.Compare(Path.GetFileName(dir), "cb", true) == 0)
                {
                    //папка "cb" с задними фреймами
                    MoveImagesToTargetContentFolder(dir, "cb");
                }
                else if (string.Compare(Path.GetFileName(dir), "o", true) == 0)
                {
                    //папка "cb" с оверлеями
                    MoveImagesToTargetContentFolder(dir, "o");
                }
                else if (string.Compare(Path.GetFileName(dir), "s", true) == 0)
                {
                    //папка "cb" с оверлеями
                    MoveImagesToTargetContentFolder(dir, "s");
                }
                else
                {
                    var images = Directory.GetFiles(dir, "*.png");
                    if (images.Length > 0)
                    {
                        //Просто произвольная подпапка, которая будет перемещена как новый мод
                        MoveImagesToTargetContentFolder(dir, "f", true);
                        //bool IsCharaCard = false;
                        //foreach (var img in images)
                        //{
                        //    //var imgdata = Image.FromFile(img);

                        //    //if (imgdata.Width == 252 && imgdata.Height == 352)
                        //    //{
                        //    //    IsCharaCard = true;
                        //    //}
                        //    string TargetPath = Path.Combine(GetCharactersFolder(), Path.GetFileName(img));
                        //    File.Move(img, TargetPath);
                        //}

                        string theDirName = Path.GetFileName(dir);
                        var cardsModDir = Path.Combine(ModsPath, theDirName);
                        //var cardsModDir = GetResultTargetDirPathWithNameCheck(ModsPath, Path.GetFileName(dir));

                        //Перемещение файлов в ту же папку, если она существует, вместо создания новой
                        if (Directory.Exists(cardsModDir))
                        {
                            foreach (var file in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories))
                            {
                                string fileTarget = file.Replace(Install2MODirPath, ModsPath);

                                if (File.Exists(fileTarget))
                                {
                                    fileTarget = GetResultTargetFilePathWithNameCheck(Path.GetDirectoryName(fileTarget), Path.GetFileNameWithoutExtension(fileTarget), Path.GetExtension(fileTarget));
                                }
                                File.Move(file, fileTarget);
                            }
                            DeleteEmptySubfolders(dir);
                        }
                        else
                        {
                            Directory.Move(dir, cardsModDir);
                        }

                        //запись meta.ini
                        WriteMetaINI(
                            cardsModDir
                            ,
                            "54,"
                            ,
                            string.Empty
                            ,
                            string.Empty
                            ,
                            "<br>Author: " + string.Empty + "<br><br>" + Path.GetFileNameWithoutExtension(cardsModDir) + " character cards<br><br>"
                            );

                        ActivateInsertModIfPossible(Path.GetFileName(cardsModDir), true, "UserCharacters_separator");
                    }
                }
            }
        }

        private void MoveImagesToTargetContentFolder(string dir, string ContentType, bool MoveInThisFolder = false)
        {
            if (Directory.Exists(dir))
            {
                string TargetFolder = string.Empty;
                string Extension = ".png";
                if (ContentType == "f")
                {
                    //TargetFolder = GetCharactersFolder();
                    TargetFolder = GetUserDataSubFolder(MoveInThisFolder ? dir : " Chars", ContentType);
                }
                else if (ContentType == "m")
                {
                    //TargetFolder = GetCharactersFolder(true);
                    TargetFolder = GetUserDataSubFolder(MoveInThisFolder ? dir : " Chars", ContentType);
                }
                else if (ContentType == "c")
                {
                    //TargetFolder = GetCoordinateFolder();
                    TargetFolder = GetUserDataSubFolder(MoveInThisFolder ? dir : " Coordinate", ContentType);
                }
                else if (ContentType == "cf")
                {
                    //TargetFolder = GetCardFrameFolder();
                    TargetFolder = GetUserDataSubFolder(MoveInThisFolder ? dir : " Cardframes", ContentType);
                }
                else if (ContentType == "cb")
                {
                    //TargetFolder = GetCardFrameFolder(false);
                    TargetFolder = GetUserDataSubFolder(MoveInThisFolder ? dir : " Cardframes", ContentType);
                }
                else if (ContentType == "o")
                {
                    //TargetFolder = GetOverlaysFolder();
                    TargetFolder = GetUserDataSubFolder(MoveInThisFolder ? dir : " Overlays", ContentType);
                }
                else if (ContentType == "s")
                {
                    //TargetFolder = GetOverlaysFolder();
                    TargetFolder = GetUserDataSubFolder(MoveInThisFolder ? dir : " Scenes", ContentType);
                }
                var Targets = Directory.GetFiles(dir, "*" + Extension);
                if (Targets.Length > 0)
                {
                    foreach (var target in Targets)
                    {
                        var CardframeTargetFolder = GetResultTargetFilePathWithNameCheck(TargetFolder, Path.GetFileNameWithoutExtension(target), Extension);

                        File.Move(target, CardframeTargetFolder);
                    }
                }

                if (ContentType == "o")
                {
                    Targets = Directory.GetDirectories(dir, "*");
                    if (Targets.Length > 0)
                    {
                        foreach (var target in Targets)
                        {
                            var ResultTargetPath = GetResultTargetDirPathWithNameCheck(Path.GetDirectoryName(TargetFolder), Path.GetFileName(target));

                            Directory.Move(target, ResultTargetPath);
                        }
                    }
                }
                else if (ContentType == "f")
                {
                    string MaleDir = Path.Combine(dir, "m");
                    if (Directory.Exists(MaleDir))
                    {
                        Targets = Directory.GetFiles(MaleDir, "*.png");
                        foreach (var target in Targets)
                        {
                            string name = Path.GetFileName(target);
                            File.Move(target, Path.Combine(dir, name));
                        }
                        Directory.Move(MaleDir, MaleDir + "_");
                        MoveImagesToTargetContentFolder(dir, "m", MoveInThisFolder);
                    }
                }

                DeleteEmptySubfolders(dir);
            }
        }

        private string GetOverlaysFolder()
        {
            string CoordinateFolder = Path.Combine(ModsPath, "OrganizedModPack Downloaded Overlays");
            if (Directory.Exists(CoordinateFolder))
            {
                CoordinateFolder = Path.Combine(ModsPath, "OrganizedModPack Downloaded Overlays", "UserData", "Overlays");
                if (!Directory.Exists(CoordinateFolder))
                {
                    Directory.CreateDirectory(CoordinateFolder);
                }
                return CoordinateFolder;
            }
            if (Directory.Exists(Path.Combine(ModsPath, "MyUserData")))
            {
                CoordinateFolder = Path.Combine(ModsPath, "MyUserData", "UserData", "Overlays");
                if (!Directory.Exists(CoordinateFolder))
                {
                    Directory.CreateDirectory(CoordinateFolder);
                }
                return CoordinateFolder;
            }
            CoordinateFolder = Path.Combine(OverwriteFolder, "MyUserData", "UserData", "Overlays");
            if (!Directory.Exists(CoordinateFolder))
            {
                Directory.CreateDirectory(CoordinateFolder);
            }
            return CoordinateFolder;
        }

        private string GetResultTargetFilePathWithNameCheck(string Folder, string Name, string Extension)
        {
            var ResultPath = Path.Combine(Folder, Name + Extension);
            int i = 0;
            while (File.Exists(ResultPath))
            {
                i++;
                ResultPath = Path.Combine(Folder, Name + " (" + i + ")" + Extension);
            }
            return ResultPath;
        }

        private string GetCardFrameFolder(bool IsFront = true)
        {
            var imagesSubdir = Path.Combine(ModsPath, "OrganizedModPack Downloaded CardFrames");
            var FrontBack = IsFront ? "Front" : "Back";
            if (Directory.Exists(imagesSubdir))
            {
                imagesSubdir = Path.Combine(ModsPath, "OrganizedModPack Downloaded Cardframes", "UserData", "cardframe", FrontBack);
                if (!Directory.Exists(imagesSubdir))
                {
                    Directory.CreateDirectory(imagesSubdir);
                }
                return imagesSubdir;
            }
            imagesSubdir = Path.Combine(ModsPath, "MyUserData");
            if (Directory.Exists(imagesSubdir))
            {
                imagesSubdir = Path.Combine(imagesSubdir, "UserData", "cardframe", FrontBack);
                if (!Directory.Exists(imagesSubdir))
                {
                    Directory.CreateDirectory(imagesSubdir);
                }
                return imagesSubdir;
            }

            imagesSubdir = Path.Combine(OverwriteFolder, "UserData", "cardframe", FrontBack);
            if (!Directory.Exists(imagesSubdir))
            {
                Directory.CreateDirectory(imagesSubdir);
            }
            return imagesSubdir;
        }

        private string GetCoordinateFolder()
        {
            string CoordinateFolder = Path.Combine(ModsPath, "OrganizedModPack Downloaded Coordinate");
            if (Directory.Exists(CoordinateFolder))
            {
                CoordinateFolder = Path.Combine(ModsPath, "OrganizedModPack Downloaded Coordinate", "UserData", "coordinate");
                if (!Directory.Exists(CoordinateFolder))
                {
                    Directory.CreateDirectory(CoordinateFolder);
                }
                return CoordinateFolder;
            }
            if (Directory.Exists(Path.Combine(ModsPath, "MyUserData")))
            {
                CoordinateFolder = Path.Combine(ModsPath, "MyUserData", "UserData", "coordinate");
                if (!Directory.Exists(CoordinateFolder))
                {
                    Directory.CreateDirectory(CoordinateFolder);
                }
                return CoordinateFolder;
            }
            CoordinateFolder = Path.Combine(OverwriteFolder, "MyUserData", "UserData", "coordinate");
            if (!Directory.Exists(CoordinateFolder))
            {
                Directory.CreateDirectory(CoordinateFolder);
            }
            return CoordinateFolder;
        }

        private string GetCharactersFolder(bool IsMaleCards = false)
        {
            string[] TargetFolders = new string[3]
            {
                Path.Combine(ModsPath, "OrganizedModPack Downloaded Chars"),
                Path.Combine(ModsPath, "MyUserData"),
                OverwriteFolder
            };

            var FeMaleFolder = IsMaleCards ? "male" : "female";
            int TargetFoldersLength = TargetFolders.Length;
            for (int i = 0; i < TargetFoldersLength; i++)
            {
                string Folder = TargetFolders[i];
                if (Directory.Exists(Folder))
                {
                    var Subdir = Path.Combine(Folder, "UserData", "chara", FeMaleFolder);
                    if (!Directory.Exists(Subdir))
                    {
                        Directory.CreateDirectory(Subdir);
                    }
                    return Subdir;
                }
            }
            return OverwriteFolder;
        }

        private string GetUserDataSubFolder(string FirstCandidateFolder, string Type)
        {
            string[] TargetFolders = new string[3]
            {
                FirstCandidateFolder.Substring(0,1)== " " ? Path.Combine(ModsPath, "OrganizedModPack Downloaded"+FirstCandidateFolder) : FirstCandidateFolder,
                Path.Combine(ModsPath, "MyUserData"),
                OverwriteFolder
            };

            string TypeFolder = string.Empty;
            string TargetFolderName = string.Empty;
            if (Type == "f")
            {
                TypeFolder = "chara";
                TargetFolderName = "female";
            }
            else if (Type == "m")
            {
                TypeFolder = "chara";
                TargetFolderName = "male";
            }
            else if (Type == "c")
            {
                TypeFolder = "coordinate";
            }
            else if (Type == "cf")
            {
                TypeFolder = "cardframe";
                TargetFolderName = "Front";
            }
            else if (Type == "cb")
            {
                TypeFolder = "cardframe";
                TargetFolderName = "Back";
            }
            else if (Type == "o")
            {
                TypeFolder = "Overlays";
            }
            else if (Type == "s")
            {
                TypeFolder = "studio";
                TargetFolderName = "scene";
            }

            int TargetFoldersLength = TargetFolders.Length;
            for (int i = 0; i < TargetFoldersLength; i++)
            {
                string Folder = TargetFolders[i];
                if (Directory.Exists(Folder))
                {
                    var TargetResultDirPath = Path.Combine(Folder, "UserData", TypeFolder, TargetFolderName);
                    if (!Directory.Exists(TargetResultDirPath))
                    {
                        Directory.CreateDirectory(TargetResultDirPath);
                    }
                    return TargetResultDirPath;
                }
            }

            return Path.Combine(OverwriteFolder, "UserData", TypeFolder, TargetFolderName);
        }


        private void InstallZipArchivesToMods()
        {
            foreach (var zipfile in Directory.GetFiles(Install2MODirPath, "*.zip"))
            {
                //следующий, если не существует
                if (!File.Exists(zipfile))
                {
                    continue;
                }

                bool FoundZipMod = false;
                bool FoundStandardModInZip = false;
                bool FoundModsDir = false;
                bool FoundcsFiles = false;

                string author = string.Empty;
                string category = string.Empty;
                string version = string.Empty;
                string comment = string.Empty;
                string description = string.Empty;
                string UpdateModNameFromMeta = string.Empty;
                bool FoundUpdateName = false;
                string ModFolderForUpdate = string.Empty;
                string ZipName = Path.GetFileNameWithoutExtension(zipfile);
                string targetFileAny = string.Empty;

                using (ZipArchive archive = ZipFile.OpenRead(zipfile))
                {
                    int archiveEntriesCount = archive.Entries.Count;
                    for (int entrieNum = 0; entrieNum < archiveEntriesCount; entrieNum++)
                    {
                        string entryName = archive.Entries[entrieNum].Name;
                        string entryFullName = archive.Entries[entrieNum].FullName;

                        int entryFullNameLength = entryFullName.Length;
                        if (!FoundZipMod && entryFullNameLength >= 12 && string.Compare(entryFullName.Substring(entryFullNameLength - 12, 12), "manifest.xml", true) == 0) //entryFullName=="manifest.xml"
                        {
                            FoundZipMod = true;
                            break;
                        }

                        if (!FoundStandardModInZip)
                        {
                            if (
                                   (entryFullNameLength >= 7 && (string.Compare(entryFullName.Substring(entryFullNameLength - 7, 6), "abdata", true) == 0 || string.Compare(entryFullName.Substring(0, 6), "abdata", true) == 0)) //entryFullName=="abdata/"
                                || (entryFullNameLength >= 6 && (string.Compare(entryFullName.Substring(entryFullNameLength - 6, 5), "_data", true) == 0 || string.Compare(entryFullName.Substring(0, 5), "_data", true) == 0)) //entryFullName=="_data/"
                                || (entryFullNameLength >= 8 && (string.Compare(entryFullName.Substring(entryFullNameLength - 8, 7), "bepinex", true) == 0 || string.Compare(entryFullName.Substring(0, 7), "bepinex", true) == 0)) //entryFullName=="bepinex/"
                                || (entryFullNameLength >= 9 && (string.Compare(entryFullName.Substring(entryFullNameLength - 9, 8), "userdata", true) == 0 || string.Compare(entryFullName.Substring(0, 8), "userdata", true) == 0)) //entryFullName=="userdata/"
                               )
                            {
                                FoundStandardModInZip = true;
                            }
                        }

                        //когда найдена папка mods, если найден zipmod
                        if (FoundModsDir && !FoundStandardModInZip)
                        {
                            if (entryFullNameLength >= 7 && string.Compare(entryFullName.Substring(entryFullNameLength - 7, 7), ".zipmod", true) == 0)//entryFullName==".zipmod"
                            {
                                archive.Entries[entrieNum].ExtractToFile(Path.Combine(Install2MODirPath, entryName));
                                break;
                            }
                            else if (entryFullNameLength >= 4 && string.Compare(entryFullName.Substring(entryFullNameLength - 4, 4), ".zip", true) == 0)
                            {
                                archive.Entries[entrieNum].ExtractToFile(Path.Combine(Install2MODirPath, entryName + "mod"));
                                break;
                            }
                        }

                        //если найдена папка mods
                        if (!FoundModsDir && entryFullNameLength >= 5 && (string.Compare(entryFullName.Substring(entryFullNameLength - 5, 4), "mods", true) == 0 || string.Compare(entryFullName.Substring(0, 4), "mods", true) == 0))
                        {
                            FoundModsDir = true;
                        }

                        //если найден cs
                        if (!FoundcsFiles && entryFullNameLength >= 3 && string.Compare(entryFullName.Substring(entryFullNameLength - 3, 3), ".cs", true) == 0)
                        {
                            FoundStandardModInZip = false;
                            FoundcsFiles = true;
                            break;
                        }

                        //получение информации о моде из dll
                        if (entryFullNameLength >= 4 && string.Compare(entryFullName.Substring(entryFullNameLength - 4, 4), ".dll", true) == 0)
                        {
                            if (description.Length == 0 && version.Length == 0 && author.Length == 0)
                            {
                                string temp = Path.Combine(Install2MODirPath, "temp");
                                string entryPath = Path.Combine(temp, entryFullName);
                                string entryDir = Path.GetDirectoryName(entryPath);
                                if (!Directory.Exists(entryDir))
                                {
                                    Directory.CreateDirectory(entryDir);
                                }

                                archive.Entries[entrieNum].ExtractToFile(entryPath);

                                if (File.Exists(entryPath))
                                {
                                    FileVersionInfo dllInfo = FileVersionInfo.GetVersionInfo(entryPath);
                                    description = dllInfo.FileDescription;
                                    version = dllInfo.FileVersion;
                                    //string version = dllInfo.ProductVersion;
                                    string copyright = dllInfo.LegalCopyright;

                                    if (copyright.Length >= 4)
                                    {
                                        //"Copyright © AuthorName 2019"
                                        author = copyright.Remove(copyright.Length - 4, 4).Replace("Copyright © ", string.Empty).Trim();
                                    }

                                    string[] ModsList = GetModsListFromActiveMOProfile(false);

                                    foreach (var ModFolder in ModsList)
                                    {
                                        ModFolderForUpdate = Path.Combine(ModsPath, ModFolder);
                                        string targetfile = Path.Combine(ModFolderForUpdate, entryFullName);
                                        targetFileAny = GetTheDllFromSubfolders(ModFolderForUpdate, entryName.Remove(entryName.Length - 4, 4), "dll");
                                        if (File.Exists(targetfile) || (targetFileAny.Length > 0 && File.Exists(targetFileAny)))
                                        {
                                            if (targetFileAny.Length > 0)
                                            {
                                                targetfile = targetFileAny;
                                            }

                                            UpdateModNameFromMeta = GetINIValueIfExist(Path.Combine(ModFolderForUpdate, "meta.ini"), "notes", "General");
                                            if (UpdateModNameFromMeta.Length > 0)
                                            {
                                                int upIndex = UpdateModNameFromMeta.IndexOf("ompupname:");
                                                if (upIndex > -1)
                                                {
                                                    //get update name
                                                    UpdateModNameFromMeta = UpdateModNameFromMeta.Substring(upIndex).Split(':')[1];
                                                    if (UpdateModNameFromMeta.Length > 0 && ZipName.Length >= UpdateModNameFromMeta.Length && IsStringAContainsStringB(ZipName, UpdateModNameFromMeta))
                                                    {
                                                        FoundUpdateName = true;
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        UpdateModNameFromMeta = string.Empty;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    //File.Delete(entryPath);
                                    Directory.Delete(temp, true);
                                }
                            }
                        }
                    }
                }

                if (FoundZipMod)
                {
                    //если файл имеет расширение zip. Надо, т.к. здесь может быть файл zipmod
                    if (zipfile.Length >= 4 && string.Compare(zipfile.Substring(zipfile.Length - 4, 4), ".zip", true) == 0)
                    {
                        File.Move(zipfile, zipfile + "mod");
                    }
                    InstallZipModsToMods();//будет после установлено соответствующей функцией
                }
                else if (FoundStandardModInZip)
                {
                    string TargetModDirPath;
                    if (FoundUpdateName)
                    {
                        TargetModDirPath = Path.Combine(Install2MODirPath, ZipName + "_temp");
                    }
                    else
                    {
                        TargetModDirPath = Path.Combine(ModsPath, ZipName);
                    }

                    Compressor.Decompress(zipfile, TargetModDirPath);

                    if (FoundUpdateName)
                    {
                        string[] modfiles = Directory.GetFiles(TargetModDirPath, "*.*", SearchOption.AllDirectories);
                        foreach (var file in modfiles)
                        {
                            //ModFolderForUpdate
                            string TargetFIle = file.Replace(TargetModDirPath, ModFolderForUpdate);
                            string TargetFileDir = Path.GetDirectoryName(TargetFIle);
                            bool targetfileIsNewerOrSame = false;
                            if (File.Exists(TargetFIle))
                            {
                                if (File.GetLastWriteTime(file) > File.GetLastWriteTime(TargetFIle))
                                {
                                    File.Delete(TargetFIle);
                                }
                                else
                                {
                                    targetfileIsNewerOrSame = true;
                                }
                            }
                            else
                            {
                                if (
                                TargetFIle.Length >= 4 && TargetFIle.Substring(TargetFIle.Length - 4, 4) == ".dll"
                                && Path.GetFileNameWithoutExtension(targetFileAny) == Path.GetFileNameWithoutExtension(file)
                                && File.Exists(targetFileAny)
                                   )
                                {
                                    if (File.GetLastWriteTime(file) > File.GetLastWriteTime(targetFileAny))
                                    {
                                        File.Delete(targetFileAny);
                                    }
                                    else
                                    {
                                        targetfileIsNewerOrSame = true;
                                    }
                                }
                            }
                            if (!Directory.Exists(TargetFileDir))
                            {
                                Directory.CreateDirectory(TargetFileDir);
                            }
                            if (targetfileIsNewerOrSame)
                            {
                                File.Delete(file);
                            }
                            else
                            {
                                File.Move(file, TargetFIle);
                            }
                        }
                        Directory.Delete(TargetModDirPath, true);
                        //присваивание папки на целевую, после переноса, для дальнейшей работы с ней
                        TargetModDirPath = ModFolderForUpdate;
                    }

                    File.Move(zipfile, zipfile + (FoundUpdateName ? ".InstalledUpdatedMod" : ".InstalledExtractedToMods"));

                    if (version.Length == 0)
                    {
                        version = Regex.Match(ZipName, @"\d+(\.\d+)*").Value;
                    }
                    if (!FoundUpdateName && author.Length == 0)
                    {
                        author = ZipName.StartsWith("[AI][") || (ZipName.StartsWith("[") && !ZipName.StartsWith("[AI]")) ? ZipName.Substring(ZipName.IndexOf("[") + 1, ZipName.IndexOf("]") - 1) : string.Empty;
                    }

                    //запись meta.ini
                    WriteMetaINI(
                        TargetModDirPath
                        ,
                        FoundUpdateName ? string.Empty : category
                        ,
                        version
                        ,
                        FoundUpdateName ? string.Empty : comment
                        ,
                        FoundUpdateName ? string.Empty : "<br>Author: " + author + "<br><br>" + (description.Length > 0 ? description : Path.GetFileNameWithoutExtension(zipfile)) + "<br><br>"
                        );

                    ActivateInsertModIfPossible(Path.GetFileName(TargetModDirPath));
                }
                else if (FoundModsDir)
                {
                    File.Move(zipfile, zipfile + ".InstalledExtractedZipmod");
                }
                else if (FoundcsFiles)
                {
                    //extract to handle as subdir
                    string extractpath = Path.Combine(Install2MODirPath, Path.GetFileNameWithoutExtension(zipfile));
                    Compressor.Decompress(zipfile, extractpath);
                    File.Move(zipfile, zipfile + ".InstalledExtractedAsSubfolder");
                }
            }
        }

        private string GetTheDllFromSubfolders(string Dir, string FileName, string Extension)
        {
            if (Directory.Exists(Dir))
            {
                foreach (var file in Directory.GetFiles(Dir, "*." + Extension, SearchOption.AllDirectories))
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    if (string.Compare(name, FileName, true) == 0)
                    {
                        return file;
                    }
                }
            }
            return string.Empty;
        }

        private void InstallModFilesFromSubfolders()
        {
            foreach (var dirIn2mo in Directory.GetDirectories(Install2MODirPath, "*"))
            {
                if (dirIn2mo.Length >= 4 && string.Compare(dirIn2mo.Substring(dirIn2mo.Length - 4, 4), "Temp", true) == 0)//path ends with 'Temp'
                {
                    continue;
                }

                string dir = dirIn2mo;
                string name = Path.GetFileName(dir);
                string category = string.Empty;
                string version = string.Empty;
                string author = string.Empty;
                string comment = string.Empty;
                string description = string.Empty;
                string moddir = string.Empty;

                bool AnyModFound = false;

                string[] subDirs = Directory.GetDirectories(dir, "*");

                //when was extracted archive where is one folder with same name and this folder contains game files
                if (subDirs.Length == 1 && Path.GetFileName(subDirs[0]) == name)
                {
                    //re-set dir to this one subdir and get dirs from there
                    dir = subDirs[0];
                    subDirs = Directory.GetDirectories(dir, "*");
                }
                int subDirsLength = subDirs.Length;
                for (int i = 0; i < subDirsLength; i++)
                {
                    string subdir = subDirs[i];
                    string subdirname = Path.GetFileName(subdir);

                    if (
                           string.Compare(subdirname, "abdata", true) == 0
                        || string.Compare(subdirname, "userdata", true) == 0
                        || string.Compare(subdirname, "ai-syoujyotrial_data", true) == 0
                        || string.Compare(subdirname, "ai-syoujyo_data", true) == 0
                        || string.Compare(subdirname, "StudioNEOV2_Data", true) == 0
                        || string.Compare(subdirname, "manual_s", true) == 0
                        || string.Compare(subdirname, "manual", true) == 0
                        || string.Compare(subdirname, "MonoBleedingEdge", true) == 0
                        || string.Compare(subdirname, "DefaultData", true) == 0
                        || string.Compare(subdirname, "bepinex", true) == 0
                        || string.Compare(subdirname, "scripts", true) == 0
                        || string.Compare(subdirname, "mods", true) == 0
                        )
                    {
                        //CopyFolder.Copy(dir, Path.Combine(ModsPath, dir));
                        //Directory.Move(dir, "[installed]" + dir);

                        var TargetModDIr = Path.Combine(ModsPath, Path.GetFileName(dir));
                        //var TargetModDIr = GetResultTargetDirPathWithNameCheck(ModsPath, Path.GetFileName(dir));

                        if (Directory.Exists(TargetModDIr))
                        {
                            foreach (var file in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories))
                            {
                                string fileTarget = file.Replace(dir, TargetModDIr);
                                if (File.Exists(fileTarget))
                                {
                                    if (File.GetLastWriteTime(file) > File.GetLastWriteTime(fileTarget))
                                    {
                                        File.Delete(fileTarget);
                                        File.Move(file, fileTarget);
                                    }
                                }
                                else
                                {
                                    File.Move(file, fileTarget);
                                }
                            }
                            Directory.Delete(dir, true);
                        }
                        else
                        {
                            Directory.Move(dir, TargetModDIr);
                        }

                        moddir = TargetModDIr;
                        AnyModFound = true;
                        version = Regex.Match(name, @"\d+(\.\d+)*").Value;
                        author = name.StartsWith("[AI][") || (name.StartsWith("[") && !name.StartsWith("[AI]")) ? name.Substring(name.IndexOf("[") + 1, name.IndexOf("]") - 1) : string.Empty;
                        description = name;
                        break;
                    }
                }

                if (!AnyModFound)
                {
                    moddir = dir.Replace(Install2MODirPath, ModsPath);
                    string targetfilepath = "readme.txt";
                    foreach (var file in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories))
                    {
                        if (string.Compare(Path.GetExtension(file), ".unity3d", true) == 0)//if extension == .unity3d
                        {
                            //string[] datafiles = Directory.GetFiles(dir, Path.GetFileName(file), SearchOption.AllDirectories);

                            DirectoryInfo dirinfo = new DirectoryInfo(DataPath);

                            var datafiles = dirinfo.GetFiles(Path.GetFileName(file), SearchOption.AllDirectories);

                            if (datafiles.Length > 0)
                            {
                                string selectedfile = datafiles[0].FullName;
                                if (datafiles.Length > 1)
                                {
                                    long size = 0;
                                    for (int f = 0; f < datafiles.Length; f++)
                                    {
                                        if (datafiles[f].Length > size)
                                        {
                                            size = datafiles[f].Length;
                                            selectedfile = datafiles[f].FullName;
                                        }
                                    }
                                }

                                targetfilepath = selectedfile.Replace(DataPath, moddir);

                                Directory.CreateDirectory(Path.GetDirectoryName(targetfilepath));
                                File.Move(file, targetfilepath);
                            }
                            AnyModFound = true;
                        }
                        else if (string.Compare(Path.GetExtension(file), ".cs", true) == 0)//if extension == .cs
                        {
                            string targetsubdirpath = Path.Combine(moddir, "scripts");
                            if (!Directory.Exists(targetsubdirpath))
                            {
                                Directory.CreateDirectory(targetsubdirpath);
                            }

                            File.Move(file, Path.Combine(targetsubdirpath, Path.GetFileName(file)));
                            if (comment.Length == 0 || !IsStringAContainsStringB(comment, "Requires: ScriptLoader"))
                            {
                                comment += " Requires: ScriptLoader";
                            }
                            if (category.Length == 0 || !IsStringAContainsStringB(comment, "86"))
                            {
                                if (category.Length == 0 || category == "-1,")
                                {
                                    category = "86,";
                                }
                                else
                                {
                                    category += category.EndsWith(",") ? "86" : ",86";
                                }
                            }

                            AnyModFound = true;
                        }
                    }

                    if (AnyModFound)
                    {
                        string[] txts = Directory.GetFiles(dir, "*.txt");
                        string infofile = string.Empty;
                        if (txts.Length > 0)
                        {
                            foreach (string txt in txts)
                            {
                                string txtFileName = Path.GetFileName(txt);

                                if (
                                        string.Compare(txt, "readme.txt", true) == 0
                                    || string.Compare(txt, "description.txt", true) == 0
                                    || string.Compare(txt, Path.GetFileName(dir) + ".txt", true) == 0
                                    || string.Compare(txt, Path.GetFileNameWithoutExtension(targetfilepath) + ".txt", true) == 0
                                    )
                                {
                                    infofile = txt;
                                }
                            }

                            if (File.Exists(Path.Combine(dir, Path.GetFileName(dir) + ".txt")))
                            {
                                infofile = Path.Combine(dir, Path.GetFileName(dir) + ".txt");
                            }
                            else if (File.Exists(Path.Combine(dir, "readme.txt")))
                            {
                                infofile = Path.Combine(dir, "readme.txt");
                            }
                            else if (File.Exists(Path.Combine(dir, "description.txt")))
                            {
                                infofile = Path.Combine(dir, "description.txt");
                            }
                            else if (File.Exists(Path.Combine(dir, Path.GetFileNameWithoutExtension(targetfilepath) + ".txt")))
                            {
                                infofile = Path.Combine(dir, Path.GetFileNameWithoutExtension(targetfilepath) + ".txt");
                            }
                        }

                        bool d = false;
                        if (infofile.Length > 0)
                        {
                            string[] filecontent = File.ReadAllLines(infofile);
                            for (int l = 0; l < filecontent.Length; l++)
                            {
                                if (d)
                                {
                                    description += filecontent[l] + "<br>";
                                }
                                else if (filecontent[l].StartsWith("name:"))
                                {
                                    string s = filecontent[l].Replace("name:", string.Empty);
                                    if (s.Length > 1)
                                    {
                                        name = s;
                                    }
                                }
                                else if (filecontent[l].StartsWith("author:"))
                                {
                                    string s = filecontent[l].Replace("author:", string.Empty);
                                    if (s.Length > 1)
                                    {
                                        author = s;
                                    }
                                }
                                else if (filecontent[l].StartsWith("version:"))
                                {
                                    string s = filecontent[l].Replace("version:", string.Empty);
                                    if (s.Length > 1)
                                    {
                                        version = s;
                                    }
                                }
                                else if (filecontent[l].StartsWith("description:"))
                                {
                                    description += filecontent[l].Replace("description:", string.Empty) + "<br>";
                                    d = true;
                                }
                            }
                            if (File.Exists(infofile))
                            {
                                File.Move(infofile, Path.Combine(moddir, Path.GetFileName(infofile)));
                            }
                        }
                    }
                }

                if (AnyModFound)
                {
                    string[] dlls = Directory.GetFiles(moddir, "*.dll", SearchOption.AllDirectories);
                    if (author.Length == 0 && dlls.Length > 0)
                    {
                        foreach (string dll in dlls)
                        {
                            FileVersionInfo dllInfo = FileVersionInfo.GetVersionInfo(dll);

                            if (description.Length == 0)
                            {
                                description = dllInfo.FileDescription;
                            }
                            if (version.Length == 0)
                            {
                                version = dllInfo.FileVersion;
                            }
                            if (version.Length == 0)
                            {
                                version = dllInfo.FileVersion;
                            }
                            if (author.Length == 0)
                            {
                                author = dllInfo.LegalCopyright;
                                //"Copyright © AuthorName 2019"
                                author = author.Length >= 4 ? author.Remove(author.Length - 4, 4).Replace("Copyright © ", string.Empty).Trim() : author;
                            }
                        }
                    }

                    if (version.Length == 0)
                    {
                        var r = Regex.Match(Path.GetFileName(moddir), @"(\d+(\.\d+)*)");
                        if (r.Value.Length > 0)
                        {
                            version = r.Value;
                        }
                    }

                    //запись meta.ini
                    WriteMetaINI(
                        moddir
                        ,
                        category
                        ,
                        version
                        ,
                        comment
                        ,
                        "<br>Author: " + author + "<br><br>" + description
                        );
                    //Utils.IniFile INI = new Utils.IniFile(Path.Combine(dllmoddirpath, "meta.ini"));
                    //INI.WriteINI("General", "category", "\"51,\"");
                    //INI.WriteINI("General", "version", version);
                    //INI.WriteINI("General", "gameName", "Skyrim");
                    //INI.WriteINI("General", "comments", "Requires: BepinEx");
                    //INI.WriteINI("General", "notes", "\"<br>Author: " + author + "<br><br>" + description + "<br><br>" + copyright + " \"");
                    //INI.WriteINI("General", "validated", "true");

                    ActivateInsertModIfPossible(Path.GetFileName(moddir));
                }
            }
        }

        /// <summary>
        /// Проверки существования целевой папки и модификация имени на уникальное
        /// </summary>
        /// <param name="ParentFolder"></param>
        /// <param name="TargetFolder"></param>
        /// <returns></returns>
        private string GetResultTargetDirPathWithNameCheck(string ParentFolder, string TargetFolder)
        {
            string ResultTargetDirPath = Path.Combine(ParentFolder, TargetFolder);
            int i = 0;
            while (Directory.Exists(ResultTargetDirPath))
            {
                i++;
                ResultTargetDirPath = Path.Combine(ParentFolder, TargetFolder + " (" + i + ")");
            }
            return ResultTargetDirPath;
        }

        private void InstallBepinExModsToMods()
        {
            foreach (var dllfile in Directory.GetFiles(Install2MODirPath, "*.dll"))
            {
                FileVersionInfo dllInfo = FileVersionInfo.GetVersionInfo(dllfile);
                string name = dllInfo.ProductName;
                string description = dllInfo.FileDescription;
                string version = dllInfo.FileVersion;
                //string version = dllInfo.ProductVersion;
                string copyright = dllInfo.LegalCopyright;

                if (name.Length == 0)
                {
                    name = Path.GetFileNameWithoutExtension(dllfile);
                }

                string author = string.Empty;
                if (copyright.Length >= 4)
                {
                    //"Copyright © AuthorName 2019"
                    author = copyright.Remove(copyright.Length - 4, 4).Replace("Copyright © ", string.Empty).Trim();
                }

                //добавление имени автора в начало имени папки
                if ((!string.IsNullOrEmpty(name) && name.Substring(0, 1) == "[" && !name.StartsWith("[AI]")) || (name.Length >= 5 && name.Substring(0, 5) == "[AI][") || IsStringAContainsStringB(name, author))
                {
                }
                else if (author.Length > 0)
                {
                    //проверка на любые невалидные для имени папки символы
                    if (ContainsAnyInvalidCharacters(author))
                    {
                    }
                    else
                    {
                        name = "[" + author + "]" + name;
                    }
                }

                string dllName = Path.GetFileName(dllfile);
                string dllTargetModDirPath = Path.Combine(ModsPath, name);
                string dllTargetModPluginsSubdirPath = Path.Combine(dllTargetModDirPath, "BepInEx", "Plugins");
                string dllTargetPath = Path.Combine(dllTargetModPluginsSubdirPath, dllName);
                bool IsUpdate = false;
                string modNameWithAuthor = string.Empty;
                string newModDirPath = string.Empty;

                if (Directory.Exists(dllTargetModDirPath))
                {
                    if (File.Exists(dllTargetPath) && File.GetLastWriteTime(dllfile) > File.GetLastWriteTime(dllTargetPath))
                    {
                        //обновление существующей dll на более новую
                        IsUpdate = true;
                        File.Delete(dllTargetPath);
                    }
                    else
                    {
                        //Проверки существования целевой папки и модификация имени на более уникальное
                        dllTargetModDirPath = GetResultTargetDirPathWithNameCheck(ModsPath, name);
                    }
                }
                else
                {
                    //найти имя мода из списка модов
                    modNameWithAuthor = GetModFromModListContainsTheName(name, false);
                    if (modNameWithAuthor.Length == 0)
                    {
                        //если пусто, поискать также имя по имени дллки
                        modNameWithAuthor = GetModFromModListContainsTheName(dllName.Remove(dllName.Length - 4, 4), false);
                    }
                    if (modNameWithAuthor.Length > 0)
                    {
                        newModDirPath = Path.Combine(ModsPath, modNameWithAuthor);
                        if (Directory.Exists(newModDirPath))
                        {
                            dllTargetModDirPath = newModDirPath;
                            dllTargetModPluginsSubdirPath = Path.Combine(dllTargetModDirPath, "BepInEx", "Plugins");
                            dllTargetPath = Path.Combine(dllTargetModPluginsSubdirPath, dllName);
                            if (File.Exists(dllTargetPath) && File.GetLastWriteTime(dllfile) > File.GetLastWriteTime(dllTargetPath))
                            {
                                //обновление существующей dll на более новую, найдена папка существующего мода, с измененным именем
                                IsUpdate = true;
                                File.Delete(dllTargetPath);
                            }
                        }
                    }
                }

                //перемещение zipmod-а в свою подпапку в Mods
                Directory.CreateDirectory(dllTargetModPluginsSubdirPath);
                File.Move(dllfile, dllTargetPath);

                string readme = Path.Combine(Path.GetDirectoryName(dllfile), Path.GetFileNameWithoutExtension(dllfile) + " Readme.txt");
                if (File.Exists(readme))
                {
                    File.Move(readme, Path.Combine(dllTargetModDirPath, Path.GetFileName(readme)));
                }


                //запись meta.ini
                WriteMetaINI(
                    dllTargetModDirPath
                    ,
                    IsUpdate ? string.Empty : "51,"
                    ,
                    version
                    ,
                    IsUpdate ? string.Empty : "Requires: BepinEx"
                    ,
                    IsUpdate ? string.Empty : "<br>Author: " + author + "<br><br>" + description + "<br><br>" + copyright
                    );
                //Utils.IniFile INI = new Utils.IniFile(Path.Combine(dllmoddirpath, "meta.ini"));
                //INI.WriteINI("General", "category", "\"51,\"");
                //INI.WriteINI("General", "version", version);
                //INI.WriteINI("General", "gameName", "Skyrim");
                //INI.WriteINI("General", "comments", "Requires: BepinEx");
                //INI.WriteINI("General", "notes", "\"<br>Author: " + author + "<br><br>" + description + "<br><br>" + copyright + " \"");
                //INI.WriteINI("General", "validated", "true");

                ActivateInsertModIfPossible(Path.GetFileName(dllTargetModDirPath));
            }
        }

        private string GetModFromModListContainsTheName(string name, bool OnlyFromEnabledMods = true)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                string[] modList = GetModsListFromActiveMOProfile(OnlyFromEnabledMods);
                int nameLength = name.Length;
                int modListLength = modList.Length;
                for (int modlineNumber = 0; modlineNumber < modListLength; modlineNumber++)
                {
                    string modname = modList[modlineNumber];
                    if (modname.Length >= nameLength && IsStringAContainsStringB(modname, name))
                    {
                        return modname;
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Check if string A contains string B (if length of A > length of A with replaced B by "")<br></br><br></br>
        /// Speed check tests: https://cc.davelozinski.com/c-sharp/fastest-way-to-check-if-a-string-occurs-within-a-string
        /// </summary>
        /// <param name="StringAWhereSearch"></param>
        /// <param name="StringBToSearch"></param>
        /// <returns></returns>
        private bool IsStringAContainsStringB(string StringAWhereSearch, string StringBToSearch)
        {
            int StringAInWhichSearchLength = StringAWhereSearch.Length;
            if (StringAInWhichSearchLength > 0 && StringBToSearch.Length > 0)//safe check for empty values
            {
                //if string A contains string B then string A with replaced stringB by empty will be
                return StringAInWhichSearchLength > StringAWhereSearch.Replace(StringBToSearch, string.Empty).Length;
            }
            return false;

        }

        /// <summary>
        /// Writes required parameters in meta.ini
        /// </summary>
        /// <param name="moddir"></param>
        /// <param name="category"></param>
        /// <param name="version"></param>
        /// <param name="comments"></param>
        /// <param name="notes"></param>
        private void WriteMetaINI(string moddir, string category = "", string version = "", string comments = "", string notes = "")
        {
            if (Directory.Exists(moddir))
            {
                string metaPath = Path.Combine(moddir, "meta.ini");
                Utils.IniFile INI = new Utils.IniFile(metaPath);

                bool IsKeyExists = INI.KeyExists("category", "General");
                if (!IsKeyExists || (IsKeyExists && category.Length > 0 && INI.ReadINI("General", "category").Replace("\"", string.Empty).Length == 0))
                {
                    INI.WriteINI("General", "category", "\"" + category + "\"");
                }

                if (version.Length > 0)
                {
                    INI.WriteINI("General", "version", version);
                }

                INI.WriteINI("General", "gameName", GETMOCurrentGame());

                if (comments.Length > 0)
                {
                    INI.WriteINI("General", "comments", comments);
                }

                if (notes.Length > 0)
                {
                    INI.WriteINI("General", "notes", "\"" + notes + "\"");
                }

                INI.WriteINI("General", "validated", "true");
            }
        }

        private string GETMOCurrentGame()
        {
            return "Skyrim";
        }

        private void ActivateInsertModIfPossible(string modname, bool Activate = true, string modAfterWhichInsert = "", bool PlaceAfter = true)
        {
            if (modname.Length > 0)
            {
                string currentMOprofile = GetINIValueIfExist(Path.Combine(MODirPath, "ModOrganizer.ini"), "selected_profile", "General");

                if (currentMOprofile.Length == 0)
                {
                }
                else
                {
                    string profilemodlistpath = Path.Combine(MODirPath, "profiles", currentMOprofile, "modlist.txt");

                    InsertLineInFile(profilemodlistpath, (Activate ? "+" : "-") + modname, 1, modAfterWhichInsert, PlaceAfter);
                }
            }
        }

        /// <summary>
        /// Gets setup.xml path from latest enabled mod like must be in Mod Organizer
        /// </summary>
        /// <returns></returns>
        private string GetSetupXmlPathForCurrentProfile()
        {
            if (MOmode)
            {
                string currentMOprofile = GetINIValueIfExist(Path.Combine(MODirPath, "ModOrganizer.ini"), "selected_profile", "General");

                if (currentMOprofile.Length == 0)
                {
                }
                else
                {
                    string profilemodlistpath = Path.Combine(MODirPath, "profiles", currentMOprofile, "modlist.txt");

                    if (File.Exists(profilemodlistpath))
                    {
                        string[] lines = File.ReadAllLines(profilemodlistpath);

                        int linescount = lines.Length;
                        for (int i = 1; i < linescount; i++) // 1- означает пропуск нулевой строки, где комментарий
                        {
                            if (lines[i].StartsWith("+"))
                            {
                                string SetupXmlPath = Path.Combine(ModsPath, lines[i].Remove(0, 1), "UserData", "setup.xml");
                                if (File.Exists(SetupXmlPath))
                                {
                                    return SetupXmlPath;
                                }
                            }
                        }
                    }
                }

                return Path.Combine(OverwriteFolderLink, "UserData", "setup.xml");
            }
            else
            {
                return Path.Combine(DataPath, "UserData", "setup.xml");
            }
        }

        private string GetINIValueIfExist(string INIPath, string Key, string Section)
        {
            Utils.IniFile INI = new Utils.IniFile(INIPath);
            if (INI.KeyExists(Key, Section))
            {
                return INI.ReadINI(Section, Key);
            }
            return string.Empty;
        }

        //https://social.msdn.microsoft.com/Forums/vstudio/en-US/8f713e50-0789-4bf6-865f-c87cdebd0b4f/insert-line-to-text-file-using-streamwriter-using-csharp?forum=csharpgeneral
        /// <summary>
        /// Inserts line in file in set position
        /// </summary>
        /// <param name="path"></param>
        /// <param name="line"></param>
        /// <param name="Position"></param>
        public static void InsertLineInFile(string path, string Line, int Position = 1, string InsertWithThisMod = "", bool PlaceAfter = true)
        {
            if (path.Length > 0 && File.Exists(path) && Line.Length > 0)
            {
                string[] FileLines = File.ReadAllLines(path);
                if (!FileLines.Contains(Line))
                {
                    int FileLinesLength = FileLines.Length;
                    bool InsertWithMod = InsertWithThisMod.Length > 0;
                    Position = InsertWithMod ? FileLinesLength : Position;
                    using (StreamWriter writer = new StreamWriter(path))
                    {
                        for (int LineNumber = 0; LineNumber < Position; LineNumber++)
                        {
                            if (InsertWithMod && FileLines[LineNumber].Length > 0 && string.Compare(FileLines[LineNumber].Remove(0, 1), InsertWithThisMod, true) == 0)
                            {
                                if (PlaceAfter)
                                {
                                    Position = LineNumber;
                                }
                                else
                                {
                                    writer.WriteLine(FileLines[LineNumber]);
                                    Position = LineNumber + 1;
                                }
                                break;

                            }
                            else
                            {
                                writer.WriteLine(FileLines[LineNumber]);
                            }
                        }

                        writer.WriteLine(Line);

                        if (Position < FileLinesLength)
                        {
                            for (int LineNumber = Position; LineNumber < FileLinesLength; LineNumber++)
                            {
                                writer.WriteLine(FileLines[LineNumber]);
                            }
                        }
                    }
                }
            }
        }

        private void InstallZipModsToMods()
        {
            string TempDir = Path.Combine(Install2MODirPath, "Temp");
            foreach (var zipfile in Directory.GetFiles(Install2MODirPath, "*.zipmod"))
            {
                string guid = string.Empty;
                string name = string.Empty;
                string version = string.Empty;
                string author = string.Empty;
                string description = string.Empty;
                string website = string.Empty;
                string game = string.Empty;

                bool IsManifestFound = false;
                using (ZipArchive archive = ZipFile.OpenRead(zipfile))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith("manifest.xml", StringComparison.OrdinalIgnoreCase))
                        {
                            if (Directory.Exists(TempDir))
                            {
                            }
                            else
                            {
                                Directory.CreateDirectory(TempDir);
                            }

                            string xmlpath = Path.Combine(Install2MODirPath, "Temp", entry.FullName);
                            entry.ExtractToFile(xmlpath);

                            IsManifestFound = true;

                            guid = ReadXmlValue(xmlpath, "manifest/name", string.Empty);
                            name = ReadXmlValue(xmlpath, "manifest/name", string.Empty);
                            version = ReadXmlValue(xmlpath, "manifest/version", "0");
                            author = ReadXmlValue(xmlpath, "manifest/author", "Unknown author");
                            description = ReadXmlValue(xmlpath, "manifest/description", "Unknown description");
                            website = ReadXmlValue(xmlpath, "manifest/website", string.Empty);
                            game = ReadXmlValue(xmlpath, "manifest/game", "AI Girl"); //установил умолчание как "AI Girl"
                            File.Delete(xmlpath);
                            break;
                        }
                    }
                }

                string zipArchiveName = Path.GetFileNameWithoutExtension(zipfile);
                if (IsManifestFound && game == "AI Girl")
                {
                    if (name.Length == 0)
                    {
                        name = Path.GetFileNameWithoutExtension(zipfile);
                    }

                    //добавление имени автора в начало имени папки
                    if (name.StartsWith("[AI][") || (name.StartsWith("[") && !name.StartsWith("[AI]")) || IsStringAContainsStringB(name, author))
                    {
                    }
                    else if (author.Length > 0)
                    {
                        //проверка на любые невалидные для имени папки символы
                        if (ContainsAnyInvalidCharacters(author))
                        {
                        }
                        else
                        {
                            name = "[" + author + "]" + name;
                        }
                    }

                    //string zipmoddirpath = GetResultTargetDirPathWithNameCheck(ModsPath, name);
                    string zipmoddirpath = Path.Combine(ModsPath, name);
                    string zipmoddirmodspath = Path.Combine(zipmoddirpath, "mods");
                    string targetZipFile = Path.Combine(zipmoddirmodspath, zipArchiveName + ".zipmod");
                    bool update = false;
                    string anyfname = string.Empty;
                    if (Directory.Exists(zipmoddirpath))
                    {
                        if (File.Exists(targetZipFile) || (anyfname = IsAnyFileWithSameExtensionContainsNameOfTheFile(zipmoddirmodspath, zipArchiveName, "*.zip")).Length > 0)
                        {
                            if (File.GetLastWriteTime(zipfile) > File.GetLastWriteTime(targetZipFile))
                            {
                                update = true;
                                File.Delete(anyfname.Length > 0 ? anyfname : targetZipFile);
                            }
                        }
                    }
                    else
                    {
                        zipmoddirpath = GetResultTargetDirPathWithNameCheck(ModsPath, name);
                        zipmoddirmodspath = Path.Combine(zipmoddirpath, "mods");
                        targetZipFile = Path.Combine(zipmoddirmodspath, zipArchiveName + ".zipmod");
                    }

                    //перемещение zipmod-а в свою подпапку в Mods
                    Directory.CreateDirectory(zipmoddirmodspath);
                    File.Move(zipfile, targetZipFile);

                    //Перемещение файлов мода, начинающихся с того же имени в папку этого мода
                    string[] PossibleFilesOfTheMod = Directory.GetFiles(Install2MODirPath, "*.*").Where(file => Path.GetFileName(file).Trim().StartsWith(zipArchiveName)).ToArray();
                    int PossibleFilesOfTheModLength = PossibleFilesOfTheMod.Length;
                    if (PossibleFilesOfTheModLength > 0)
                    {
                        for (int n = 0; n < PossibleFilesOfTheModLength; n++)
                        {
                            if (File.Exists(PossibleFilesOfTheMod[n]))
                            {
                                File.Move(PossibleFilesOfTheMod[n], Path.Combine(zipmoddirpath, Path.GetFileName(PossibleFilesOfTheMod[n]).Replace(zipArchiveName, Path.GetFileNameWithoutExtension(zipmoddirpath))));
                            }
                        }
                    }

                    //запись meta.ini
                    WriteMetaINI(
                        zipmoddirpath
                        ,
                        string.Empty
                        ,
                        version
                        ,
                        update ? string.Empty : "Requires: Sideloader plugin"
                        ,
                        update ? string.Empty : "<br>Author: " + author + "<br><br>" + description + "<br><br>" + website
                        );
                    //Utils.IniFile INI = new Utils.IniFile(Path.Combine(zipmoddirpath, "meta.ini"));
                    //INI.WriteINI("General", "category", string.Empty);
                    //INI.WriteINI("General", "version", version);
                    //INI.WriteINI("General", "gameName", "Skyrim");
                    //INI.WriteINI("General", "comments", "Requires: Sideloader plugin");
                    //INI.WriteINI("General", "notes", "\"<br>Author: " + author + "<br><br>" + description + "<br><br>" + website + " \"");
                    //INI.WriteINI("General", "validated", "true");

                    ActivateInsertModIfPossible(Path.GetFileName(zipmoddirpath));
                }
            }
        }

        private string IsAnyFileWithSameExtensionContainsNameOfTheFile(string zipmoddirmodspath, string zipname, string extension)
        {
            if (Directory.Exists(zipmoddirmodspath))
            {
                foreach (var path in Directory.GetFiles(zipmoddirmodspath, extension))
                {
                    string name = Path.GetFileNameWithoutExtension(path);
                    if (IsStringAContainsStringB(name, zipname))
                    {
                        return path;
                    }
                }
            }
            return string.Empty;
        }

        private bool ContainsAnyInvalidCharacters(string path)
        {
            return (path.Length > 0 && path.IndexOfAny(Path.GetInvalidPathChars()) >= 0);
        }

        private void CreateShortcutButton_Click(object sender, EventArgs e)
        {
            CreateShortcuts(true, false);
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

                    string[] EnabledModsList = GetModsListFromActiveMOProfile();
                    int EnabledModsLength = EnabledModsList.Length;

                    if (EnabledModsList.Length == 0)
                    {
                        MOmode = true;
                        MOCommonModeSwitchButton.Enabled = true;
                        MessageBox.Show(T._("There is no enabled mods in Mod Organizer"));
                        return;
                    }

                    CleanBepInExLinksFromData();

                    if (File.Exists(dummyfile))
                    {
                        File.Delete(dummyfile);
                    }

                    if (!Directory.Exists(MOmodeDataFilesBak))
                    {
                        Directory.CreateDirectory(MOmodeDataFilesBak);
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
                                string FileInBakFolderWhichIsInRES = FileInDataFolder.Replace(DataPath, MOmodeDataFilesBak);
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
                                        string FileInBakFolderWhichIsInRES = FileInDataFolder.Replace(DataPath, MOmodeDataFilesBak);
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

                    File.WriteAllText(MOToStandartConvertationOperationsListFile, Operations.ToString());
                    string[] DataWithModsFileslist = Directory.GetFiles(DataPath, "*.*", SearchOption.AllDirectories);
                    File.WriteAllLines(ModdedDataFilesListFile, DataWithModsFileslist);
                    File.WriteAllLines(VanillaDataFilesListFile, DataWithModsFileslist);

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

                    string[] Operations = File.ReadAllLines(MOToStandartConvertationOperationsListFile);
                    string[] VanillaDataFiles = File.ReadAllLines(VanillaDataFilesListFile);
                    string[] ModdedDataFiles = File.ReadAllLines(ModdedDataFilesListFile);

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

                            bool IsForOverwriteFolder = IsStringAContainsStringB(TargetFolderPath, OverwriteFolder);
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
                            WriteMetaINI(
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
                            ActivateInsertModIfPossible(NewModName, false, ModName, false);
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
                        WriteMetaINI(
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

                        ActivateInsertModIfPossible(addedFilesFolderName);
                    }

                    //перемещение ванильных файлов назад в дата
                    if (Directory.Exists(MOmodeDataFilesBak))
                    {
                        string[] FilesInMOmodeDataFilesBak = Directory.GetFiles(MOmodeDataFilesBak, "*.*", SearchOption.AllDirectories);
                        int FilesInMOmodeDataFilesBakLength = FilesInMOmodeDataFilesBak.Length;
                        for (int f = 0; f < FilesInMOmodeDataFilesBakLength; f++)
                        {
                            string DestFileInDataFolderPath = FilesInMOmodeDataFilesBak[f].Replace(MOmodeDataFilesBak, DataPath);
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
                        DeleteEmptySubfolders(MOmodeDataFilesBak);
                    }

                    //чистка файлов-списков
                    File.Delete(MOToStandartConvertationOperationsListFile);
                    File.Delete(VanillaDataFilesListFile);
                    File.Delete(ModdedDataFilesListFile);

                    //очистка пустых папок в Data
                    DeleteEmptySubfolders(DataPath, false);
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
        string MOmodeDataFilesBak = Path.Combine(AppResDir, "MOmodeDataFilesBak");
        string ModdedDataFilesListFile = Path.Combine(AppResDir, "ModdedDataFilesList.txt");
        string VanillaDataFilesListFile = Path.Combine(AppResDir, "VanillaDataFilesList.txt");
        string MOToStandartConvertationOperationsListFile = Path.Combine(AppResDir, "MOToStandartConvertationOperationsList.txt");

        private void DeleteEmptySubfolders(string dataPath, bool DeleteThisDir = true, string[] Exclusions = null)
        {
            string[] subfolders = Directory.GetDirectories(dataPath, "*");
            int subfoldersLength = subfolders.Length;
            if (subfoldersLength > 0)
            {
                for (int d = 0; d < subfoldersLength; d++)
                {
                    DeleteEmptySubfolders(subfolders[d], !TrueIfStringInExclusionsList(subfolders[d], Exclusions), Exclusions);
                }
            }

            if (DeleteThisDir && Directory.GetDirectories(dataPath, "*").Length == 0 && Directory.GetFiles(dataPath, "*.*").Length == 0)
            {
                Directory.Delete(dataPath);
            }
        }

        private bool TrueIfStringInExclusionsList(string Str, string[] exclusions)
        {
            if (exclusions == null || Str.Length == 0)
            {
                return false;
            }
            else
            {
                int exclusionsLength = exclusions.Length;
                for (int i = 0; i < exclusionsLength; i++)
                {
                    if (string.IsNullOrWhiteSpace(exclusions[i]))
                    {
                        continue;
                    }
                    if (IsStringAContainsStringB(Str, exclusions[i]))
                    {
                        return true;
                    }
                }

            }
            return false;
        }

        public static void MoveWithReplace(string sourceFileName, string destFileName)
        {

            //first, delete target file if exists, as File.Move() does not support overwrite
            if (File.Exists(destFileName))
            {
                File.Delete(destFileName);
            }

            string destFolder = Path.GetDirectoryName(destFileName);
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }
            File.Move(sourceFileName, destFileName);

        }

        private void CleanBepInExLinksFromData()
        {
            //удаление файлов BepinEx
            DeleteIfSymlink(Path.Combine(DataPath, "doorstop_config.ini"));
            DeleteIfSymlink(Path.Combine(DataPath, "winhttp.dll"));
            string BepInExDir = Path.Combine(DataPath, "BepInEx");
            if (Directory.Exists(BepInExDir))
            {
                Directory.Delete(BepInExDir, true);
            }

            //удаление ссылок на папки плагинов BepinEx
            DeleteIfSymlink(Path.Combine(DataPath, "UserData", "MaterialEditor"), true);
            DeleteIfSymlink(Path.Combine(DataPath, "UserData", "Overlays"), true);
        }

        private void DeleteIfSymlink(string LinkPath, bool IsFolder = false)
        {
            if (IsFolder)
            {
                if (Directory.Exists(LinkPath))
                {
                    if (FileInfoExtensions.IsSymbolicLink(new FileInfo(LinkPath)))
                    {
                        Directory.Delete(LinkPath, true);
                    }
                }
            }
            else
            {
                if (File.Exists(LinkPath))
                {
                    if (FileInfoExtensions.IsSymbolicLink(new FileInfo(LinkPath)))
                    {
                        File.Delete(LinkPath);
                    }
                }
            }
        }

        private string[] GetModsListFromActiveMOProfile(bool OnlyEnabled = true)
        {
            string currentMOprofile = GetINIValueIfExist(Path.Combine(MODirPath, "ModOrganizer.ini"), "selected_profile", "General");

            if (currentMOprofile.Length > 0)
            {
                string profilemodlistpath = Path.Combine(MODirPath, "profiles", currentMOprofile, "modlist.txt");

                if (File.Exists(profilemodlistpath))
                {
                    string[] lines;
                    if (OnlyEnabled)
                    {
                        //все строки с + в начале
                        lines = File.ReadAllLines(profilemodlistpath).Where(line => line.Length > 0 && string.CompareOrdinal(line.Substring(0, 1), "#") != 0 && string.CompareOrdinal(line.Substring(0, 1), "+") == 0).ToArray();
                    }
                    else
                    {
                        //все строки кроме сепараторов
                        lines = File.ReadAllLines(profilemodlistpath).Where(line => line.Length > 0 && string.CompareOrdinal(line.Substring(0, 1), "#") != 0 && (line.Length < 10 || string.CompareOrdinal(line.Substring(line.Length - 10, 10), "_separator") != 0)).ToArray();
                    }
                    //Array.Reverse(lines); //убрал, т.к. дулаю архив с резервными копиями
                    int linesLength = lines.Length;
                    for (int l = 0; l < linesLength; l++)
                    {
                        //remove +-
                        lines[l] = lines[l].Remove(0, 1);
                    }

                    return lines;
                }
            }

            return null;
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
