using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
//using Crc32C;

namespace AI_Girl_Helper
{
    public partial class AIGirlHelper : Form
    {
        private readonly bool compressmode = true;

        //constants
        private static readonly string AppResDir = Path.Combine(Application.StartupPath, "AI Girl Helper_RES");
        private static readonly string ModsPath = Path.Combine(Application.StartupPath, "Mods");
        private static readonly string DownloadsPath = Path.Combine(Application.StartupPath, "Downloads");
        private static readonly string DataPath = Path.Combine(Application.StartupPath, "Data");
        private static readonly string MODirPath = Path.Combine(Application.StartupPath, "MO");
        private static readonly string MOexePath = Path.Combine(MODirPath, "ModOrganizer.exe");
        private static readonly string OverwriteFolder = Path.Combine(MODirPath, "overwrite");
        private static readonly string OverwriteFolderLink = Path.Combine(Application.StartupPath, "MOUserData");
        private static string SetupXmlPath;
        private static bool MOmode = true;

        public AIGirlHelper()
        {
            InitializeComponent();

            SetLocalizationStrings();

            SetupXmlPath = GetSetupXmlPathForCurrentProfile();
        }

        private void SetLocalizationStrings()
        {

            InstallInModsButton.Text = "2MO";
            button1.Text = T._("Prepare the game");
            SettingsPage.Text = T._("Settings");
            CreateShortcutButton.Text = T._("Shortcut");
            FixRegistryButton.Text = T._("Fix registry");
            groupBox1.Text = T._("Display");
            FullScreenCheckBox.Text = T._("fullscreen");
            ShortcutsCheckBox.Text = T._("Create shortcuts after archive extraction");
            LaunchTabPage.Text = T._("Launch");
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
            button1.Text = T._("Extracting..");

            //https://ru.stackoverflow.com/questions/222414/%d0%9a%d0%b0%d0%ba-%d0%bf%d1%80%d0%b0%d0%b2%d0%b8%d0%bb%d1%8c%d0%bd%d0%be-%d0%b2%d1%8b%d0%bf%d0%be%d0%bb%d0%bd%d0%b8%d1%82%d1%8c-%d0%bc%d0%b5%d1%82%d0%be%d0%b4-%d0%b2-%d0%be%d1%82%d0%b4%d0%b5%d0%bb%d1%8c%d0%bd%d0%be%d0%bc-%d0%bf%d0%be%d1%82%d0%be%d0%ba%d0%b5 
            await Task.Run(() => UnpackGame());
            await Task.Run(() => UnpackMO());
            await Task.Run(() => UnpackMods());

            BepinExLoadingFix();

            CreateShortcuts();

            if (MOmode)
            {
                //Create dummy file and add hidden attribute
                string dummyfile = Path.Combine(Application.StartupPath, "TESV.exe");
                File.WriteAllText(dummyfile, "dummy file");
                HideFileFolder(dummyfile, true);
            }

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

        private void BepinExLoadingFix()
        {
            if (MOmode)
            {
                string linkpath = Path.Combine(ModsPath, "BepInEx5", "Bepinex", "core", "BepInEx.Preloader.dll");
                string objectpath = Path.Combine(DataPath, "Bepinex", "core", "BepInEx.Preloader.dll");
                if (!File.Exists(linkpath) && File.Exists(objectpath))
                {
                    Symlink(objectpath
                      , linkpath
                      );
                    HideFileFolder(Path.Combine(DataPath, "Bepinex"));
                }

                linkpath = Path.Combine(DataPath, "doorstop_config.ini");
                objectpath = Path.Combine(ModsPath, "BepInEx5", "doorstop_config.ini");
                if (!File.Exists(linkpath) && File.Exists(objectpath))
                {
                    Symlink(linkpath
                      , objectpath
                      );
                    HideFileFolder(linkpath, true);
                }

                linkpath = Path.Combine(DataPath, "winhttp.dll");
                objectpath = Path.Combine(ModsPath, "BepInEx5", "winhttp.dll");
                if (!File.Exists(linkpath) && File.Exists(objectpath))
                {
                    Symlink(linkpath
                          , objectpath
                          );
                    HideFileFolder(linkpath, true);
                }

                linkpath = Path.Combine(DataPath, "UserData", "MaterialEditor");
                objectpath = Path.Combine(ModsPath, "MyUserData", "UserData", "MaterialEditor");
                if (!File.Exists(linkpath) && File.Exists(objectpath))
                {
                    Symlink(linkpath
                          , objectpath
                          );
                    HideFileFolder(linkpath, true);

                }

                linkpath = Path.Combine(DataPath, "UserData", "Overlays");
                objectpath = Path.Combine(ModsPath, "MyUserData", "UserData", "Overlays");
                if (!File.Exists(linkpath) && File.Exists(objectpath))
                {
                    Symlink(linkpath
                          , objectpath
                          );
                    HideFileFolder(linkpath, true);

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

        private void Symlink(string symlink, string file)
        {
            if (File.Exists(symlink))
            {
            }
            else
            {
                if (Directory.Exists(Path.GetDirectoryName(symlink)))
                {
                }
                else
                {
                    _ = Directory.CreateDirectory(Path.GetDirectoryName(symlink));
                }
                if (File.Exists(file))
                {
                    CreateSymlink.File(file, symlink);
                }
            }
        }

        //https://bytescout.com/blog/create-shortcuts-in-c-and-vbnet.html
        private void CreateShortcuts(bool force = false)
        {
            if (ShortcutsCheckBox.Checked || force)
            {
                //AI-Girl Helper
                string shortcutname = T._("AI-Girl Helper");
                string targetpath = Path.Combine(Application.StartupPath, "AI Girl Helper.exe");
                string arguments = string.Empty;
                string workingdir = Path.GetDirectoryName(targetpath);
                string description = T._("Run ") + shortcutname;
                string iconlocation = Path.Combine(Application.StartupPath, "AI Girl Helper.exe");
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
                            label3.Invoke((Action)(() => label3.Text = T._("Extracting ") + i + "/" + files.Length));
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
                    && (File.Exists(Path.Combine(DataPath, "AI-SyoujyoTrial.exe")) || File.Exists(Path.Combine(DataPath, "AI-Syoujyo.exe"))))
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
                AutoPopDelay = 10000,
                InitialDelay = 1000,
                ReshowDelay = 500,
                UseAnimation = true,
                UseFading = true,
                // Force the ToolTip text to be displayed whether or not the form is active.
                ShowAlways = true
            };

            //Main
            THToolTip.SetToolTip(button1, T._("Unpacking mods and resources from 'Downloads' and 'AI Girl Helper_RES' folders for game when they are not installed"));
            THToolTip.SetToolTip(InstallInModsButton, T._("Automatically get required mod data, converts and moves files from 2MO folder") + (MOmode ? T._(" to MO format in Mods when possible") : T._(" to the game folder when possible")));
            THToolTip.SetToolTip(ShortcutsCheckBox, T._("When checked will create shortcut of the AIGirl Helper manager on Desktop after mods extraction"));
            THToolTip.SetToolTip(groupBox1, T._("Game Display settings"));
            THToolTip.SetToolTip(ResolutionComboBox, T._("Select preferred screen resolution"));
            THToolTip.SetToolTip(FullScreenCheckBox, T._("When checked game will be in fullscreen mode"));
            THToolTip.SetToolTip(QualityComboBox, T._("Select preferred graphics quality"));
            THToolTip.SetToolTip(CreateShortcutButton, T._("Will create shortcut in Desktop if not exist"));
            THToolTip.SetToolTip(FixRegistryButton, T._("Will set Data dir with game files as install dir in registry"));
            THToolTip.SetToolTip(GameButton, MOmode ? T._("Will execute the Game") + T._(" from Mod Organizer with attached mods") : T._("Will execute the Game"));
            THToolTip.SetToolTip(StudioButton, MOmode ? T._("Will execute Studio") + T._(" from Mod Organizer with attached mods") : T._("Will execute Studio"));
            THToolTip.SetToolTip(MOButton, T._("Will execute Mod Organizer mod manager where you can manage your mods"));
            THToolTip.SetToolTip(SettingsButton, MOmode ? T._("Will execute original game launcher") + T._(" from Mod Organizer with attached mods") : T._("Will execute original game launcher"));
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
            if (!Directory.Exists(DataPath))
            {
                Directory.CreateDirectory(DataPath);
            }
            if (MOmode && !Directory.Exists(ModsPath))
            {
                Directory.CreateDirectory(ModsPath);
                ModsInfoLabel.Text = T._("Mods dir created");
            }

            string AIGirl = "AI-Syoujyo";
            string AIGirlTrial = "AI-SyoujyoTrial";
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

                    if (NotAllModsExtracted && ModDirs.Length < Archives7z.Length)
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
                        MOButton.Visible = true;
                        SettingsButton.Visible = true;
                        GameButton.Visible = true;
                        StudioButton.Visible = true;
                        AIGirlHelperTabControl.SelectedTab = LaunchTabPage;
                    }
                }
                else
                {
                    //если нет папок модов но есть архивы в загрузках
                    if (Archives7z.Length > 0)
                    {
                        ModsInfoLabel.Text = T._("Mods Ready for extract");
                        mode = 2;
                        button1.Text = T._("Extract mods");
                    }
                }
                //если нет архивов в загрузках, но есть папки модов
                if (compressmode && Directory.Exists(DownloadsPath) && Directory.Exists(ModsPath))
                {
                    if (ModDirs.Length > 0)
                    {
                        if (Archives7z.Length == 0)
                        {
                            ModsInfoLabel.Text = "No archives in downloads";
                            button1.Text = "Pack mods";
                            mode = 0;
                        }
                    }
                }
                if (Directory.Exists(Install2MODirPath))
                {
                    if (Directory.GetFiles(Install2MODirPath, "*.*").Length > 0)
                    {
                        InstallInModsButton.Visible = true;
                        InstallInModsButton.Enabled = true;
                    }
                    string[] InstallModDirs = Directory.GetDirectories(Install2MODirPath, "*").Where(name => !name.EndsWith("Temp", StringComparison.OrdinalIgnoreCase)).ToArray();
                    if (InstallModDirs.Length > 0)
                    {
                        InstallInModsButton.Visible = true;
                        InstallInModsButton.Enabled = true;
                    }
                }
            }
            else
            {
                ModsInfoLabel.Visible = false;
            }
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
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\illusion\AI-Syoujyo\AI-SyoujyoTrial", "INSTALLDIR", Path.Combine(DataPath));
            MessageBox.Show(T._("Registry fixed! Install dir was set to Data dir."));
            //if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\illusion\AI-Syoujyo\AI-SyoujyoTrial", "INSTALLDIR", null) == null)
            //{
            //    Registry.SetValue(@"HKEY_CURRENT_USER\Software\illusion\AI-Syoujyo\AI-SyoujyoTrial", "INSTALLDIR", Path.Combine(DataPath));
            //    MessageBox.Show("Registry fixed! Installdir was set to Data dir.");
            //}
            FixRegistryButton.Enabled = true;
        }
        private void MOButton_Click(object sender, EventArgs e)
        {
            if (MOmode)
            {
                RunProgram(MOexePath, string.Empty);
            }
            else
            {
                MOButton.Enabled = false;
            }
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            if (MOmode)
            {
                RunProgram(MOexePath, "moshortcut://:InitSetting");
            }
            else
            {
                RunProgram(Path.Combine(DataPath, "InitSetting.exe"), string.Empty);
            }
        }

        private void GameButton_Click(object sender, EventArgs e)
        {
            if (MOmode)
            {
                RunProgram(MOexePath, "moshortcut://:AI-SyoujyoTrial");
            }
            else
            {
                RunProgram(Path.Combine(DataPath, "AI-SyoujyoTrial.exe"), string.Empty);
            }
        }

        private void StudioButton_Click(object sender, EventArgs e)
        {
            if (MOmode)
            {
                RunProgram(MOexePath, "moshortcut://:AI-SyoujyoStudio");
            }
            else
            {
                RunProgram(Path.Combine(DataPath, "AI-SyoujyoStudio.exe"), string.Empty);
            }
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

        Dictionary<string, string> qualitylevels = new Dictionary<string, string>(3);
        private void QualityComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetGraphicsQuality((sender as ComboBox).SelectedIndex.ToString());
        }

        private newform LinksForm;
        private void NewformButton_Click(object sender, EventArgs e)
        {
            if (LinksForm == null || LinksForm.IsDisposed)
            {
                //show and reposition of form
                //https://stackoverflow.com/questions/31492787/how-to-set-position-second-form-depend-on-first-form
                LinksForm = new newform
                {
                    //LinksForm.Text = T._("Links");
                    StartPosition = FormStartPosition.Manual
                };
                LinksForm.Load += delegate (object s2, EventArgs e2)
                {
                    LinksForm.Location = new Point(Bounds.Location.X + (Bounds.Width / 2) - (LinksForm.Width / 2),
                        Bounds.Location.Y + /*(Bounds.Height / 2) - (f2.Height / 2) +*/ Bounds.Height);
                };
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
        private void InstallInModsButton_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(Install2MODirPath) && (Directory.GetFiles(Install2MODirPath, "*.png").Length > 0 || Directory.GetFiles(Install2MODirPath, "*.dll").Length > 0 || Directory.GetFiles(Install2MODirPath, "*.zipmod").Length > 0 || Directory.GetDirectories(Install2MODirPath, "*").Length > 0))
            {
                InstallInModsButton.Enabled = false;

                InstallCardsFrom2MO();

                InstallZipModsToMods();

                InstallZipArchivesToMods();

                InstallBepinExModsToMods();

                InstallCardsFromSubfolders();

                InstallModFilesFromSubfolders();

                MessageBox.Show(T._("All possible mods installed. Install all rest in 2MO folder manually."));
            }
        }

        private void InstallCardsFrom2MO()
        {
            string targetdir = Path.Combine(ModsPath, "MyUserData");
            var images = Directory.GetFiles(Install2MODirPath, "*.png");
            if (images.Length > 0 && Directory.GetDirectories(Install2MODirPath, "*").Length == 0)
            {
                //bool IsCharaCard = false;
                foreach (var img in images)
                {
                    //var imgdata = Image.FromFile(img);

                    //if (imgdata.Width == 252 && imgdata.Height == 352)
                    //{
                    //    IsCharaCard = true;
                    //}
                    string ImgFIleName = Path.GetFileNameWithoutExtension(img);
                    string targetImagePath = string.Empty;

                    for (int i = 1; i < 100000; i++)
                    {
                        targetImagePath = Path.Combine(IllusionImagesSubFolder(targetdir), ImgFIleName + ".png");

                        if (File.Exists(targetImagePath))
                        {
                            ImgFIleName += " (" + i + ")";
                        }
                        else
                        {
                            break;
                        }
                    }

                    File.Move(img, targetImagePath);
                }
            }
        }

        private void InstallCardsFromSubfolders()
        {
            foreach (var dir in Directory.GetDirectories(Install2MODirPath, "*"))
            {
                var images = Directory.GetFiles(dir, "*.png");
                if (images.Length > 0 && Directory.GetDirectories(dir, "*").Length == 0)
                {
                    //bool IsCharaCard = false;
                    foreach (var img in images)
                    {
                        //var imgdata = Image.FromFile(img);

                        //if (imgdata.Width == 252 && imgdata.Height == 352)
                        //{
                        //    IsCharaCard = true;
                        //}
                        File.Move(img, Path.Combine(IllusionImagesSubFolder(dir), Path.GetFileName(img)));
                    }
                }

                var cardsModName = Path.GetFileName(dir);
                var cardsModDir = string.Empty;
                for (int i = 1; i < 100000; i++)
                {
                    cardsModDir = Path.Combine(ModsPath, cardsModName);

                    if (Directory.Exists(cardsModDir))
                    {
                        cardsModName += " (" + i + ")";
                    }
                    else
                    {
                        break;
                    }
                }


                Directory.Move(dir, cardsModDir);

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
                    "\"<br>Author: " + string.Empty + "<br><br>" + Path.GetFileNameWithoutExtension(cardsModDir) + "<br><br>" + " \""
                    );

                ActivateModIfPossible(Path.GetFileName(cardsModDir));
            }
        }

        private string IllusionImagesSubFolder(string dir)
        {
            var imagesSubdir = Path.Combine(dir, "UserData", "chara", "female");
            if (!Directory.Exists(imagesSubdir))
            {
                Directory.CreateDirectory(imagesSubdir);
            }
            return imagesSubdir;
        }

        private void InstallZipArchivesToMods()
        {
            foreach (var zipfile in Directory.GetFiles(Install2MODirPath, "*.zip"))
            {
                bool FoundZipMod = false;
                bool FoundStandardModInZip = false;
                using (ZipArchive archive = ZipFile.OpenRead(zipfile))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith("manifest.xml", StringComparison.OrdinalIgnoreCase))
                        {
                            FoundZipMod = true;
                            break;
                        }
                        if (entry.FullName.EndsWith("abdata/", StringComparison.OrdinalIgnoreCase)
                            || entry.FullName.EndsWith("_data/", StringComparison.OrdinalIgnoreCase)
                            || entry.FullName.EndsWith("bepinex/", StringComparison.OrdinalIgnoreCase)
                            || entry.FullName.EndsWith("userdata/", StringComparison.OrdinalIgnoreCase)
                            )
                        {
                            FoundStandardModInZip = true;
                            break;
                        }
                    }
                }

                if (FoundZipMod)
                {
                    File.Move(zipfile, zipfile + "mod");
                    InstallZipModsToMods();
                }
                else if (FoundStandardModInZip)
                {
                    string zipmoddirpath = Path.Combine(ModsPath, Path.GetFileNameWithoutExtension(zipfile));
                    Compressor.Decompress(zipfile, zipmoddirpath);
                    File.Move(zipfile, zipfile + ".Installed");

                    //запись meta.ini
                    WriteMetaINI(
                        zipmoddirpath
                        ,
                        string.Empty
                        ,
                        "0.0.1"
                        ,
                        string.Empty
                        ,
                        "\"<br>Author: " + string.Empty + "<br><br>" + Path.GetFileNameWithoutExtension(zipfile) + "<br><br>" + " \""
                        );

                    ActivateModIfPossible(Path.GetFileName(zipmoddirpath));
                }
            }
        }

        private void InstallModFilesFromSubfolders()
        {
            foreach (var dir in Directory.GetDirectories(Install2MODirPath, "*"))
            {
                if (dir.EndsWith("\\Temp"))
                {
                    continue;
                }
                bool b = false;
                foreach (var subdir in Directory.GetDirectories(dir, "*"))
                {
                    string subdirname = Path.GetFileName(subdir);
                    if (
                           subdirname == "abdata"
                        || subdirname == "UserData"
                        || subdirname == "AI-SyoujyoTrial_Data"
                        || subdirname == "AI-Syoujyo_Data"
                        )
                    {
                        CopyFolder.Copy(dir, Path.Combine(ModsPath, subdirname));
                        Directory.Move(dir, Path.Combine(Install2MODirPath, "dir_copied"));
                        b = true;
                        break;
                    }
                }

                if (!b)
                {

                    string moddir = dir.Replace(Install2MODirPath, ModsPath);
                    string targetfilepath = "readme.txt";
                    foreach (var file in Directory.GetFiles(dir, "*.*"))
                    {
                        if (Path.GetExtension(file) == ".unity3d")
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

                        }
                    }

                    string infofile = string.Empty;
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

                    string name = Path.GetFileName(dir);
                    string version = string.Empty;
                    string author = string.Empty;
                    string description = string.Empty;
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
                        File.Move(infofile, Path.Combine(moddir, Path.GetFileName(infofile)));
                    }

                    //запись meta.ini
                    WriteMetaINI(
                        moddir
                        ,
                        ""
                        ,
                        version
                        ,
                        string.Empty
                        ,
                        "\"<br>Author: " + author + "<br><br>" + description + " \""
                        );
                    //Utils.IniFile INI = new Utils.IniFile(Path.Combine(dllmoddirpath, "meta.ini"));
                    //INI.WriteINI("General", "category", "\"51,\"");
                    //INI.WriteINI("General", "version", version);
                    //INI.WriteINI("General", "gameName", "Skyrim");
                    //INI.WriteINI("General", "comments", "Requires: BepinEx");
                    //INI.WriteINI("General", "notes", "\"<br>Author: " + author + "<br><br>" + description + "<br><br>" + copyright + " \"");
                    //INI.WriteINI("General", "validated", "true");

                    ActivateModIfPossible(Path.GetFileName(moddir));

                }

            }
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

                //"Copyright © AuthorName 2019"
                string author = copyright.Remove(copyright.Length - 4, 4).Replace("Copyright © ", string.Empty).Trim();

                //добавление имени автора в начало имени папки
                if (name.StartsWith("[") || name.Contains(author))
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

                string dllmoddirpath = Path.Combine(ModsPath, name);

                //Проверки существования целевой папки и модификация имени на более уникальное
                if (Directory.Exists(dllmoddirpath))
                {
                    dllmoddirpath = Path.Combine(ModsPath, name + "(" + DateTime.Now.ToString().Replace(":", string.Empty) + ")");
                }

                //перемещение zipmod-а в свою подпапку в Mods
                string dllmoddirmodspath = Path.Combine(dllmoddirpath, "BepInEx", "Plugins");
                Directory.CreateDirectory(dllmoddirmodspath);
                File.Move(dllfile, Path.Combine(dllmoddirmodspath, Path.GetFileName(dllfile)));

                string readme = Path.Combine(Path.GetDirectoryName(dllfile), Path.GetFileNameWithoutExtension(dllfile) + " Readme.txt");
                if (File.Exists(readme))
                {
                    File.Move(readme, Path.Combine(dllmoddirpath, Path.GetFileName(readme)));
                }


                //запись meta.ini
                WriteMetaINI(
                    dllmoddirpath
                    ,
                    "\"51,\""
                    ,
                    version
                    ,
                    "Requires: BepinEx"
                    ,
                    "\"<br>Author: " + author + "<br><br>" + description + "<br><br>" + copyright + " \""
                    );
                //Utils.IniFile INI = new Utils.IniFile(Path.Combine(dllmoddirpath, "meta.ini"));
                //INI.WriteINI("General", "category", "\"51,\"");
                //INI.WriteINI("General", "version", version);
                //INI.WriteINI("General", "gameName", "Skyrim");
                //INI.WriteINI("General", "comments", "Requires: BepinEx");
                //INI.WriteINI("General", "notes", "\"<br>Author: " + author + "<br><br>" + description + "<br><br>" + copyright + " \"");
                //INI.WriteINI("General", "validated", "true");

                ActivateModIfPossible(Path.GetFileName(dllmoddirpath));
            }
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
            if (!File.Exists(Path.Combine(moddir, "meta.ini")))
            {
                Utils.IniFile INI = new Utils.IniFile(Path.Combine(moddir, "meta.ini"));
                INI.WriteINI("General", "category", category);
                INI.WriteINI("General", "version", version);
                INI.WriteINI("General", "gameName", "Skyrim");
                INI.WriteINI("General", "comments", comments);
                INI.WriteINI("General", "notes", notes);
                INI.WriteINI("General", "validated", "true");
            }
        }

        private void ActivateModIfPossible(string modname)
        {
            if (modname.Length > 0)
            {
                Utils.IniFile INI = new Utils.IniFile(Path.Combine(MODirPath, "ModOrganizer.ini"));
                if (INI.KeyExists("selected_profile", "General"))
                {
                    string currentMOprofile = INI.ReadINI("General", "selected_profile");

                    if (currentMOprofile.Length == 0)
                    {
                    }
                    else
                    {
                        string profilemodlistpath = Path.Combine(MODirPath, "profiles", currentMOprofile, "modlist.txt");

                        InsertLineInFile(profilemodlistpath, "+" + modname);
                    }
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
                Utils.IniFile INI = new Utils.IniFile(Path.Combine(MODirPath, "ModOrganizer.ini"));
                if (INI.KeyExists("selected_profile", "General"))
                {
                    string currentMOprofile = INI.ReadINI("General", "selected_profile");

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
                }

                return Path.Combine(OverwriteFolderLink, "UserData", "setup.xml");
            }
            else
            {
                return Path.Combine(DataPath, "UserData", "setup.xml");
            }
        }

        //https://social.msdn.microsoft.com/Forums/vstudio/en-US/8f713e50-0789-4bf6-865f-c87cdebd0b4f/insert-line-to-text-file-using-streamwriter-using-csharp?forum=csharpgeneral
        /// <summary>
        /// Inserts line in file in set position
        /// </summary>
        /// <param name="path"></param>
        /// <param name="line"></param>
        /// <param name="position"></param>
        public static void InsertLineInFile(string path, string line, int position = 1)
        {
            if (File.Exists(path) && line.Length > 0)
            {
                string[] lines = File.ReadAllLines(path);
                if (lines.Contains(line))
                {
                }
                else
                {
                    using (StreamWriter writer = new StreamWriter(path))
                    {
                        for (int i = 0; i < position; i++)
                        {
                            writer.WriteLine(lines[i]);
                        }

                        writer.WriteLine(line);

                        for (int i = position; i < lines.Length; i++)
                        {
                            writer.WriteLine(lines[i]);
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
                            game = ReadXmlValue(xmlpath, "manifest/game", string.Empty);
                            File.Delete(xmlpath);
                            break;
                        }
                    }
                }

                if (IsManifestFound && game == "AI Girl")
                {
                    if (name.Length == 0)
                    {
                        name = Path.GetFileNameWithoutExtension(zipfile);
                    }

                    //добавление имени автора в начало имени папки
                    if (name.StartsWith("[") || name.Contains(author))
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

                    string zipmoddirpath = Path.Combine(ModsPath, name);

                    //Проверки существования целевой папки и модификация имени на более уникальное
                    if (Directory.Exists(zipmoddirpath))
                    {
                        zipmoddirpath = Path.Combine(ModsPath, guid);
                        if (Directory.Exists(zipmoddirpath))
                        {
                            zipmoddirpath = Path.Combine(ModsPath, name + "(" + guid + ")");
                        }
                    }

                    //перемещение zipmod-а в свою подпапку в Mods
                    string zipmoddirmodspath = Path.Combine(zipmoddirpath, "mods");
                    Directory.CreateDirectory(zipmoddirmodspath);
                    File.Move(zipfile, Path.Combine(zipmoddirmodspath, Path.GetFileName(zipfile)));

                    //запись meta.ini
                    WriteMetaINI(
                        zipmoddirpath
                        ,
                        string.Empty
                        ,
                        version
                        ,
                        "Requires: Sideloader plugin"
                        ,
                        "\"<br>Author: " + author + "<br><br>" + description + "<br><br>" + website + " \""
                        );
                    //Utils.IniFile INI = new Utils.IniFile(Path.Combine(zipmoddirpath, "meta.ini"));
                    //INI.WriteINI("General", "category", string.Empty);
                    //INI.WriteINI("General", "version", version);
                    //INI.WriteINI("General", "gameName", "Skyrim");
                    //INI.WriteINI("General", "comments", "Requires: Sideloader plugin");
                    //INI.WriteINI("General", "notes", "\"<br>Author: " + author + "<br><br>" + description + "<br><br>" + website + " \"");
                    //INI.WriteINI("General", "validated", "true");

                    ActivateModIfPossible(Path.GetFileName(zipmoddirpath));
                }
            }


            if (Directory.Exists(TempDir))
            {
                Directory.Delete(Path.Combine(Install2MODirPath, "Temp"));
            }
        }

        private bool ContainsAnyInvalidCharacters(string path)
        {
            return (path.Length > 0 && path.IndexOfAny(Path.GetInvalidPathChars()) >= 0);
        }

        private void CreateShortcutButton_Click(object sender, EventArgs e)
        {
            CreateShortcuts(true);
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
