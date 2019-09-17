using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        private static readonly string SetupXmlPath = Path.Combine(OverwriteFolderLink, "UserData", "setup.xml");

        public AIGirlHelper()
        {
            InitializeComponent();

            SetLocalizationStrings();
        }

        private void SetLocalizationStrings()
        {

            button1.Text = T._("Prepare the game");
            SettingsPage.Text = T._("Settings");
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
                    TabControl1.SelectedTab = LaunchTabPage;
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

            //Create dummy file and add hidden attribute
            string dummyfile = Path.Combine(Application.StartupPath, "TESV.exe");
            File.WriteAllText(dummyfile, "dummy file");
            HideFileFolder(dummyfile, true);

            button1.Text = T._("Game Ready");
            FoldersInit();
        }

        private void UnpackMO()
        {
            string MO7zip = Path.Combine(AppResDir, "MO.7z");
            if (File.Exists(MO7zip) && !File.Exists(Path.Combine(MODirPath, "ModOrganizer.exe")))
            {
                _ = progressBar1.Invoke((Action)(() => progressBar1.Visible = true));
                _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Marquee));
                _ = label3.Invoke((Action)(() => label3.Text = T._("Extracting")));
                _ = label4.Invoke((Action)(() => label4.Text = T._("MO archive: ") + Path.GetFileNameWithoutExtension(MO7zip)));
                Compressor.Decompress(MO7zip, MODirPath);
                _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Blocks));
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
                    _ = label4.Invoke((Action)(() => label4.Text = T._("Game archive: ") + Path.GetFileNameWithoutExtension(AIGirlTrial)));
                    Compressor.Decompress(AIGirlTrial, DataPath);
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Blocks));
                }
                else if (File.Exists(AIGirl) && !File.Exists(Path.Combine(DataPath, "AI-Syoujyo.exe")))
                {
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Visible = true));
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Marquee));
                    _ = label3.Invoke((Action)(() => label3.Text = T._("Extracting")));
                    _ = label4.Invoke((Action)(() => label4.Text = T._("Game archive: ") + Path.GetFileNameWithoutExtension(AIGirl)));
                    Compressor.Decompress(AIGirl, DataPath);
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Blocks));
                }
            }
        }

        private void BepinExLoadingFix()
        {
            Symlink(Path.Combine(DataPath, "Bepinex", "core", "BepInEx.Preloader.dll")
                  , Path.Combine(ModsPath, "BepInEx5", "Bepinex", "core", "BepInEx.Preloader.dll")
                  );
            HideFileFolder(Path.Combine(DataPath, "Bepinex"));

            Symlink(Path.Combine(DataPath, "doorstop_config.ini")
                  , Path.Combine(ModsPath, "BepInEx5", "doorstop_config.ini")
                  );
            HideFileFolder(Path.Combine(DataPath, "doorstop_config.ini"),true);

            Symlink(Path.Combine(DataPath, "version.dll")
                  , Path.Combine(ModsPath, "BepInEx5", "version.dll")
                  );
            HideFileFolder(Path.Combine(DataPath, "version.dll"), true);
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
        private void CreateShortcuts()
        {
            if (ShortcutsCheckBox.Checked)
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
                            label4.Invoke((Action)(() => label4.Text = T._("Mod: ") + filename));
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
                TabControl1.SelectedTab = LaunchTabPage;
            }
        }

        private void PackMO()
        {
            if (Directory.Exists(MODirPath) && Directory.Exists(AppResDir))
            {
                _ = progressBar1.Invoke((Action)(() => progressBar1.Visible = true));
                _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Marquee));
                _ = label3.Invoke((Action)(() => label3.Text = "Compressing"));
                _ = label4.Invoke((Action)(() => label4.Text = "MO archive.."));
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
                    _ = label4.Invoke((Action)(() => label4.Text = "Game archive: " + Path.GetFileNameWithoutExtension(AIGirlTrial)));
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
                    foreach(string line in File.ReadAllLines(Path.Combine(MODirPath, "categories.dat")))
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
                        label4.Invoke((Action)(() => label4.Text = "Folder: " + Path.GetFileNameWithoutExtension(dir)));
                        
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
                    label4.Invoke((Action)(() => label4.Text = "Folder: " + Path.GetFileNameWithoutExtension(tempdir)));

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
                int categiryindex = int.Parse(categoryvalue)-1;//В List индекс идет от нуля
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
                if (int.Parse(screenWidth) > width[width.Length-1])
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
            if (!Directory.Exists(ModsPath))
            {
                Directory.CreateDirectory(ModsPath);
                label4.Text = T._("Mods dir created");
            }

            if (File.Exists(Path.Combine(DataPath, "AI-SyoujyoTrial.exe")))
            {
                label3.Text = T._("AI-SyoujyoTrial game installed in Data");
            }
            else if (File.Exists(Path.Combine(DataPath, "AI-Syoujyo.exe")))
            {
                label3.Text = T._("AI-Syoujyo game installed in Data");
            }
            else if (File.Exists(Path.Combine(AppResDir, "AIGirlTrial.7z")))
            {
                label3.Text = T._("AIGirlTrial archive in Data");
            }
            else if (File.Exists(Path.Combine(AppResDir, "AIGirl.7z")))
            {
                label3.Text = T._("AIGirl archive in Data");
            }
            else if (Directory.Exists(DataPath))
            {
                label3.Text = T._("AIGirl files not in Data. Move AIGirl game files there.");
            }
            else
            {
                Directory.CreateDirectory(DataPath);
                label3.Text = T._("Data dir created. Move AIGirl game files there.");
            }

            string[] ModDirs;
            if ( (ModDirs = Directory.GetDirectories(ModsPath, "*").Where(name => !name.EndsWith("_separator", StringComparison.OrdinalIgnoreCase)).ToArray()).Length > 0 )
            {
                bool NotAllModsExtracted = false;
                foreach (var file in Directory.GetFiles(DownloadsPath, "*.7z", SearchOption.AllDirectories))
                {
                    if (ModDirs.Contains(Path.Combine(ModsPath, Path.GetFileNameWithoutExtension(file))))
                    {
                    }
                    else
                    {
                        NotAllModsExtracted = true;
                        break;
                    }
                }

                if (NotAllModsExtracted)
                {
                    label4.Text = T._("Not all mods in Mods dir");
                    //button1.Enabled = false;
                    mode = 2;
                    button1.Text = T._("Extract missing");
                }
                else
                {
                    label4.Text = T._("Found mod folders in Mods");
                    //button1.Enabled = false;
                    mode = 1;
                    button1.Text = T._("Mods Ready");
                    MOButton.Visible = true;
                    SettingsButton.Visible = true;
                    GameButton.Visible = true;
                    StudioButton.Visible = true;
                    TabControl1.SelectedTab = LaunchTabPage;
                }
            }
            else
            {
                label4.Text = T._("There no any mod folders in Mods");
                mode = 2;
                button1.Text = T._("Extract mods");
            }

            //если нет папок модов но есть архивы в загрузках
            if (Directory.Exists(DownloadsPath))
            {
                if (Directory.GetFiles(DownloadsPath, "*.7z", SearchOption.AllDirectories).Length > 0)
                {
                    if (Directory.GetDirectories(ModsPath, "*").Where(name => !name.EndsWith("_separator", StringComparison.OrdinalIgnoreCase)).ToArray().Length == 0)
                    {
                        label4.Text = T._("Mods Ready for extract");
                        mode = 2;
                        button1.Text = T._("Extract mods");
                    }
                }
            }

            //если нет архивов в загрузках, но есть папки модов
            if (compressmode && Directory.Exists(DownloadsPath) && Directory.Exists(ModsPath))
            {
                if (Directory.GetDirectories(ModsPath, "*").Where(name => !name.EndsWith("_separator", StringComparison.OrdinalIgnoreCase)).ToArray().Length > 0)
                {
                    if (Directory.GetFiles(DownloadsPath, "*.7z", SearchOption.AllDirectories).Length == 0)
                    {
                        label4.Text = "No archives in downloads";
                        button1.Text = "Pack mods";
                        mode = 0;
                    }
                }
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
            RunProgram(MOexePath, string.Empty);
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            RunProgram(MOexePath, "moshortcut://:InitSetting");
        }

        private void GameButton_Click(object sender, EventArgs e)
        {
            RunProgram(MOexePath, "moshortcut://:AI-SyoujyoTrial");
        }

        private void StudioButton_Click(object sender, EventArgs e)
        {
            RunProgram(MOexePath, "moshortcut://:AI-SyoujyoStudio");
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

                    _ = Program.Start();
                    Program.WaitForExit();

                    // Показать
                    WindowState = FormWindowState.Normal;
                }
            }
        }

        Dictionary<string, string> qualitylevels = new Dictionary<string, string>(3);
        private void QualityComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetGraphicsQuality((sender as ComboBox).SelectedIndex.ToString());
        }

        newform f2;
        private void Newform_Click(object sender, EventArgs e)
        {
            if (f2 == null || f2.IsDisposed)
            {
                //show and reposition of form
                //https://stackoverflow.com/questions/31492787/how-to-set-position-second-form-depend-on-first-form
                f2 = new newform();
                f2.StartPosition = FormStartPosition.Manual;
                f2.Load += delegate (object s2, EventArgs e2)
                {
                    f2.Location = new Point(Bounds.Location.X + (Bounds.Width / 2) - (f2.Width / 2),
                        Bounds.Location.Y + /*(Bounds.Height / 2) - (f2.Height / 2) +*/ Bounds.Height);
                };
                f2.Show();
            }
            else
            {
                f2.Close();
            }
        }

        private void AIGirlHelper_LocationChanged(object sender, EventArgs e)
        {
            //move second form with main form
            //https://stackoverflow.com/questions/3429445/how-to-move-two-windows-forms-together
            if (f2 != null)
            {
                f2.Location = new Point(Bounds.Location.X + (Bounds.Width / 2) - (f2.Width / 2),
                    Bounds.Location.Y + /*(Bounds.Height / 2) - (f2.Height / 2) +*/ Bounds.Height);
            }
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
