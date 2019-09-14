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

namespace AI_Girl_Helper
{
    public partial class AIGirlHelper : Form
    {
        //constants
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
            button1.Text = "Extracting..";

            //https://ru.stackoverflow.com/questions/222414/%d0%9a%d0%b0%d0%ba-%d0%bf%d1%80%d0%b0%d0%b2%d0%b8%d0%bb%d1%8c%d0%bd%d0%be-%d0%b2%d1%8b%d0%bf%d0%be%d0%bb%d0%bd%d0%b8%d1%82%d1%8c-%d0%bc%d0%b5%d1%82%d0%be%d0%b4-%d0%b2-%d0%be%d1%82%d0%b4%d0%b5%d0%bb%d1%8c%d0%bd%d0%be%d0%bc-%d0%bf%d0%be%d1%82%d0%be%d0%ba%d0%b5 
            await Task.Run(() => UnpackGame());
            await Task.Run(() => UnpackMods());

            BepinExLoadingFix();

            CreateShortcuts();

            button1.Text = "Game Ready";
            FoldersInit();
        }

        private void UnpackGame()
        {
            if (Directory.Exists(DataPath))
            {
                string AIGirlTrial = Path.Combine(DataPath, "AIGirlTrial.7z");
                string AIGirl = Path.Combine(DataPath, "AIGirl.7z");
                if (File.Exists(AIGirlTrial) && !File.Exists(Path.Combine(DataPath, "AI-SyoujyoTrial.exe")))
                {
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Visible = true));
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Marquee));
                    _ = label3.Invoke((Action)(() => label3.Text = "Extracting"));
                    _ = label4.Invoke((Action)(() => label4.Text = "Game archive: " + Path.GetFileNameWithoutExtension(AIGirlTrial)));
                    Compressor.Decompress(AIGirlTrial, DataPath);
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Blocks));
                }
                else if (File.Exists(AIGirl) && !File.Exists(Path.Combine(DataPath, "AI-Syoujyo.exe")))
                {
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Visible = true));
                    _ = progressBar1.Invoke((Action)(() => progressBar1.Style = ProgressBarStyle.Marquee));
                    _ = label3.Invoke((Action)(() => label3.Text = "Extracting"));
                    _ = label4.Invoke((Action)(() => label4.Text = "Game archive: " + Path.GetFileNameWithoutExtension(AIGirl)));
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
                    DirectoryInfo dirinfo = new DirectoryInfo(path);
                    dirinfo.Attributes = FileAttributes.Hidden;
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
                string shortcutname = "AI-Girl Helper";
                string targetpath = Path.Combine(Application.StartupPath, "AI Girl Helper.exe");
                string arguments = string.Empty;
                string workingdir = Path.GetDirectoryName(targetpath);
                string description = "Run " + shortcutname;
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
                        label3.Invoke((Action)(() => label3.Text = "Extracting " + i + "/" + files.Length));
                        label4.Invoke((Action)(() => label4.Text = "Mod: " + filename));
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
        }

        private async void CompressingMode()
        {
            button1.Text = "Compressing..";

            //https://ru.stackoverflow.com/questions/222414/%d0%9a%d0%b0%d0%ba-%d0%bf%d1%80%d0%b0%d0%b2%d0%b8%d0%bb%d1%8c%d0%bd%d0%be-%d0%b2%d1%8b%d0%bf%d0%be%d0%bb%d0%bd%d0%b8%d1%82%d1%8c-%d0%bc%d0%b5%d1%82%d0%be%d0%b4-%d0%b2-%d0%be%d1%82%d0%b4%d0%b5%d0%bb%d1%8c%d0%bd%d0%be%d0%bc-%d0%bf%d0%be%d1%82%d0%be%d0%ba%d0%b5 
            await Task.Run(() => PackMods());

            ////http://www.sql.ru/forum/1149655/kak-peredat-parametr-s-metodom-delegatom
            //Thread open = new Thread(new ParameterizedThreadStart((obj) => PackMods()));
            //open.Start();

            button1.Text = "Prepare the game";
            FoldersInit();
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

        private void FoldersInit()
        {
            if (File.Exists(Path.Combine(DataPath, "AI-SyoujyoTrial.exe")))
            {
                label3.Text = "AI-SyoujyoTrial game installed in Data";
            }
            else if (File.Exists(Path.Combine(DataPath, "AI-Syoujyo.exe")))
            {
                label3.Text = "AI-Syoujyo game installed in Data";
            }
            else if (File.Exists(Path.Combine(DataPath, "AIGirlTrial.7z")))
            {
                label3.Text = "AIGirlTrial archive in Data";
            }
            else if (File.Exists(Path.Combine(DataPath, "AIGirl.7z")))
            {
                label3.Text = "AIGirl archive in Data";
            }
            else if (Directory.Exists(DataPath))
            {
                label3.Text = "AIGirl files not detected in Data. Move AIGirl game files there.";
            }
            else
            {
                Directory.CreateDirectory(DataPath);
                label3.Text = "Data dir created. Move AIGirl game files there.";
            }

            string[] ModDirs;
            if (!Directory.Exists(ModsPath))
            {
                Directory.CreateDirectory(ModsPath);
                label4.Text = "Mods dir created";
            }
            else if ( (ModDirs = Directory.GetDirectories(ModsPath, "*").Where(name => !name.EndsWith("_separator", StringComparison.OrdinalIgnoreCase)).ToArray()).Length > 0 )
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
                    label4.Text = "Not all mods extracted to mods";
                    //button1.Enabled = false;
                    mode = 2;
                    button1.Text = "Extract missing";
                }
                else
                {
                    label4.Text = "Found mod folders in Mods";
                    //button1.Enabled = false;
                    mode = 1;
                    button1.Text = "Mods Ready";
                    MOButton.Visible = true;
                    SettingsButton.Visible = true;
                    GameButton.Visible = true;
                    StudioButton.Visible = true;
                    TabControl1.SelectedTab = LaunchTabPage;
                }
            }
            else
            {
                label4.Text = "There no any mod folders in Mods";
                mode = 2;
                button1.Text = "Extract mods";
            }

            //если нет папок модов но есть архивы в загрузках
            if (Directory.Exists(DownloadsPath))
            {
                if (Directory.GetFiles(DownloadsPath, "*.7z", SearchOption.AllDirectories).Length > 0)
                {
                    if (Directory.GetDirectories(ModsPath, "*").Where(name => !name.EndsWith("_separator", StringComparison.OrdinalIgnoreCase)).ToArray().Length == 0)
                    {
                        label4.Text = "Mods Ready for extract";
                        mode = 2;
                        button1.Text = "Extract mods";
                    }
                }
            }

            //если нет архивов в загрузках, но есть папки модов
            if (Directory.Exists(DownloadsPath) && Directory.Exists(ModsPath))
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
            if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\illusion\AI-Syoujyo\AI-SyoujyoTrial", "INSTALLDIR", null) == null)
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\illusion\AI-Syoujyo\AI-SyoujyoTrial", "INSTALLDIR", Path.Combine(DataPath));
                MessageBox.Show("Registry fixed! Installdir was set to Data dir.");
            }
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
