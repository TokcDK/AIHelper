//using AIHelper.SharedData;
//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Windows.Forms;

//namespace AIHelper.Manage
//{
//    class ManageGameUnPacker
//    {

//        // temp variables. Just to temporary replace
//        // need to replace when will rework game installation feature

//        internal static int GetModeValue()
//        {
//            if (File.Exists("PackMe!.txt"))
//            {
//                GameData.MainForm._compressmode = true;
//            }
//            else
//            {
//                GameData.MainForm._compressmode = false;
//            }
//            return 0;
//        }

//        internal static async void ExtractingMode()
//        {
//            GameData.MainForm.MainService.Text = T._("Extracting") + "..";
//            GameData.MainForm.MainService.Enabled = false;

//            //https://ru.stackoverflow.com/questions/222414/%d0%9a%d0%b0%d0%ba-%d0%bf%d1%80%d0%b0%d0%b2%d0%b8%d0%bb%d1%8c%d0%bd%d0%be-%d0%b2%d1%8b%d0%bf%d0%be%d0%bb%d0%bd%d0%b8%d1%82%d1%8c-%d0%bc%d0%b5%d1%82%d0%be%d0%b4-%d0%b2-%d0%be%d1%82%d0%b4%d0%b5%d0%bb%d1%8c%d0%bd%d0%be%d0%bc-%d0%bf%d0%be%d1%82%d0%be%d0%ba%d0%b5
//            await Task.Run(() => UnpackGame()).ConfigureAwait(true);
//            await Task.Run(() => UnpackMo()).ConfigureAwait(true);
//            await Task.Run(() => UnpackMods()).ConfigureAwait(true);

//            //BepinExLoadingFix();//добавлено в folderinit

//            ManageOther.CreateShortcuts();

//            ManageModOrganizer.DummyFiles();

//            GameData.MainForm.MainService.Text = T._("Game Ready");
//            GameData.MainForm.FoldersInit();
//            GameData.MainForm.MainService.Enabled = true;
//        }

//        private static void UnpackMo()
//        {
//            if (ManageSettings.IsMoMode())
//            {
//                string mo7Zip = Path.Combine(ManageSettings.GetAppResDir(), "MO.7z");
//                if (File.Exists(mo7Zip) && !File.Exists(Path.Combine(ManageSettings.GetCurrentGameModOrganizerIniPath(), "ModOrganizer.exe")))
//                {
//                    _ = GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Visible = true));
//                    _ = GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Style = ProgressBarStyle.Marquee));
//                    _ = GameData.MainForm.DataInfoLabel.Invoke((Action)(() => GameData.MainForm.DataInfoLabel.Text = T._("Extracting")));
//                    _ = GameData.MainForm.ModsInfoLabel.Invoke((Action)(() => GameData.MainForm.ModsInfoLabel.Text = T._("MO archive") + ": " + Path.GetFileNameWithoutExtension(mo7Zip)));
//                    Compressor.Decompress(mo7Zip, ManageSettings.GetCurrentGameModOrganizerIniPath());
//                    _ = GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Style = ProgressBarStyle.Blocks));
//                }
//            }
//        }

//        private static void UnpackGame()
//        {
//            if (Directory.Exists(ManageSettings.GetCurrentGameDataPath()))
//            {
//                string aiGirlTrial = Path.Combine(ManageSettings.GetAppResDir(), "AIGirlTrial.7z");
//                string aiGirl = Path.Combine(ManageSettings.GetAppResDir(), "AIGirl.7z");
//                if (File.Exists(aiGirlTrial) && !File.Exists(Path.Combine(ManageSettings.GetCurrentGameDataPath(), "AI-SyoujyoTrial.exe")))
//                {
//                    _ = GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Visible = true));
//                    _ = GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Style = ProgressBarStyle.Marquee));
//                    _ = GameData.MainForm.DataInfoLabel.Invoke((Action)(() => GameData.MainForm.DataInfoLabel.Text = T._("Extracting")));
//                    _ = GameData.MainForm.ModsInfoLabel.Invoke((Action)(() => GameData.MainForm.ModsInfoLabel.Text = T._("Game archive") + ": " + Path.GetFileNameWithoutExtension(aiGirlTrial)));
//                    Compressor.Decompress(aiGirlTrial, ManageSettings.GetCurrentGameDataPath());
//                    _ = GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Style = ProgressBarStyle.Blocks));
//                }
//                else if (File.Exists(aiGirl) && !File.Exists(Path.Combine(ManageSettings.GetCurrentGameDataPath(), "AI-Syoujyo.exe")))
//                {
//                    _ = GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Visible = true));
//                    _ = GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Style = ProgressBarStyle.Marquee));
//                    _ = GameData.MainForm.DataInfoLabel.Invoke((Action)(() => GameData.MainForm.DataInfoLabel.Text = T._("Extracting")));
//                    _ = GameData.MainForm.ModsInfoLabel.Invoke((Action)(() => GameData.MainForm.ModsInfoLabel.Text = T._("Game archive") + ": " + Path.GetFileNameWithoutExtension(aiGirl)));
//                    Compressor.Decompress(aiGirl, ManageSettings.GetCurrentGameDataPath());
//                    _ = GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Style = ProgressBarStyle.Blocks));
//                }
//            }
//        }

//        private static void UnpackMods()
//        {
//            if (Directory.Exists(ManageSettings.GetCurrentGameModsDirPath()) && Directory.Exists(ManageSettings.GetDownloadsPath()))
//            {
//                if (Directory.Exists(ManageSettings.GetDownloadsPath()))
//                {
//                    string[] modDirs = Directory.GetDirectories(ManageSettings.GetCurrentGameModsDirPath(), "*").Where(name => !name.EndsWith("_separator", StringComparison.OrdinalIgnoreCase)).ToArray();
//                    string[] files = Directory.GetFiles(ManageSettings.GetDownloadsPath(), "*.7z", SearchOption.AllDirectories).Where(name => !modDirs.Contains(Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), Path.GetFileNameWithoutExtension(name)))).ToArray();
//                    if (files.Length == 0)
//                    {
//                    }
//                    else
//                    {
//                        int i = 1;
//                        GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Visible = true));
//                        GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Maximum = files.Length));
//                        GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Value = i));
//                        foreach (string file in files)
//                        {
//                            string filename = Path.GetFileNameWithoutExtension(file);
//                            GameData.MainForm.DataInfoLabel.Invoke((Action)(() => GameData.MainForm.DataInfoLabel.Text = T._("Extracting") + " " + +i + "/" + files.Length));
//                            GameData.MainForm.ModsInfoLabel.Invoke((Action)(() => GameData.MainForm.ModsInfoLabel.Text = T._("Mod") + ": " + filename));
//                            string moddirpath = Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), filename);
//                            if (!Directory.Exists(moddirpath))
//                            {
//                                Compressor.Decompress(file, moddirpath);
//                            }
//                            GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Value = i));
//                            i++;
//                        }
//                        GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Visible = false));
//                    }
//                }

//                //Unpack separators
//                string separators = Path.Combine(ManageSettings.GetAppResDir(), "MOModsSeparators.7z");
//                if (File.Exists(separators))
//                {
//                    Compressor.Decompress(separators, ManageSettings.GetCurrentGameModsDirPath());
//                }
//            }
//        }

//        internal static async void CompressingMode()
//        {
//            if (GameData.MainForm._compressmode)
//            {
//                GameData.MainForm.MainService.Enabled = false;
//                GameData.MainForm.MainService.Text = "Compressing..";

//                //https://ru.stackoverflow.com/questions/222414/%d0%9a%d0%b0%d0%ba-%d0%bf%d1%80%d0%b0%d0%b2%d0%b8%d0%bb%d1%8c%d0%bd%d0%be-%d0%b2%d1%8b%d0%bf%d0%be%d0%bb%d0%bd%d0%b8%d1%82%d1%8c-%d0%bc%d0%b5%d1%82%d0%be%d0%b4-%d0%b2-%d0%be%d1%82%d0%b4%d0%b5%d0%bb%d1%8c%d0%bd%d0%be%d0%bc-%d0%bf%d0%be%d1%82%d0%be%d0%ba%d0%b5
//                //await Task.Run(() => PackGame());
//                //await Task.Run(() => PackMO());
//                await Task.Run(() => PackMods()).ConfigureAwait(true);
//                await Task.Run(() => PackSeparators()).ConfigureAwait(true);

//                ////http://www.sql.ru/forum/1149655/kak-peredat-parametr-s-metodom-delegatom
//                //Thread open = new Thread(new ParameterizedThreadStart((obj) => PackMods()));
//                //open.Start();

//                GameData.MainForm.MainService.Text = T._("Prepare the game");
//                GameData.MainForm.FoldersInit();
//                GameData.MainForm.MainService.Enabled = true;
//            }
//            else
//            {
//                GameData.MainForm.AIGirlHelperTabControl.SelectedTab = GameData.MainForm.LaunchTabPage;
//            }
//        }

//        //private void PackMo()
//        //{
//        //    if (Directory.Exists(ManageSettings.GetCurrentGameModOrganizerIniPath()) && Directory.Exists(ManageSettings.GetAppResDir()))
//        //    {
//        //        _ = GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Visible = true));
//        //        _ = GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Style = ProgressBarStyle.Marquee));
//        //        _ = GameData.MainForm.DataInfoLabel.Invoke((Action)(() => GameData.MainForm.DataInfoLabel.Text = "Compressing"));
//        //        _ = GameData.MainForm.ModsInfoLabel.Invoke((Action)(() => GameData.MainForm.ModsInfoLabel.Text = "MO archive.."));
//        //        Compressor.Compress(ManageSettings.GetCurrentGameModOrganizerIniPath(), ManageSettings.GetAppResDir());
//        //        _ = GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Style = ProgressBarStyle.Blocks));
//        //    }
//        //}

//        //private void PackGame()
//        //{
//        //    if (Directory.Exists(ManageSettings.GetCurrentGameDataPath()) && Directory.Exists(ManageSettings.GetAppResDir()))
//        //    {
//        //        string aiGirlTrial = Path.Combine(ManageSettings.GetAppResDir(), "AIGirlTrial.7z");
//        //        string aiGirl = Path.Combine(ManageSettings.GetAppResDir(), "AIGirl.7z");
//        //        if (!File.Exists(aiGirlTrial)
//        //            && (File.Exists(Path.Combine(ManageSettings.GetCurrentGameDataPath(), ManageSettings.GetCurrentGameExeName() + ".exe"))))
//        //        {
//        //            _ = GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Visible = true));
//        //            _ = GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Style = ProgressBarStyle.Marquee));
//        //            _ = GameData.MainForm.DataInfoLabel.Invoke((Action)(() => GameData.MainForm.DataInfoLabel.Text = "Compressing"));
//        //            _ = GameData.MainForm.ModsInfoLabel.Invoke((Action)(() => GameData.MainForm.ModsInfoLabel.Text = "Game archive: " + Path.GetFileNameWithoutExtension(aiGirlTrial)));
//        //            Compressor.Compress(ManageSettings.GetCurrentGameDataPath(), ManageSettings.GetAppResDir());
//        //            _ = GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Style = ProgressBarStyle.Blocks));
//        //        }
//        //    }
//        //}

//        private static void PackMods()
//        {
//            if (!Directory.Exists(ManageSettings.GetCurrentGameModsDirPath())) return;
//            string[] dirs = Directory.GetDirectories(ManageSettings.GetCurrentGameModsDirPath(), "*").Where(name => !name.EndsWith("_separator", StringComparison.OrdinalIgnoreCase)).ToArray();//с игнором сепараторов
//            if (dirs.Length == 0)
//            {
//            }
//            else
//            {
//                //Read categories.dat
//                List<CategoriesList> categories = new List<CategoriesList>();
//                foreach (string line in File.ReadAllLines(ManageSettings.GetMOcategoriesPath()))
//                {
//                    if (line.Length == 0)
//                    {
//                    }
//                    else
//                    {
//                        string[] linevalues = line.Split('|');
//                        categories.Add(new CategoriesList(linevalues[0], linevalues[1], linevalues[2], linevalues[3]));
//                    }
//                }

//                int i = 1;
//                GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Visible = true));
//                GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Maximum = dirs.Length));
//                GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Value = i));
//                foreach (string dir in dirs)
//                {
//                    GameData.MainForm.DataInfoLabel.Invoke((Action)(() => GameData.MainForm.DataInfoLabel.Text = "Compressing " + i + "/" + dirs.Length));
//                    GameData.MainForm.ModsInfoLabel.Invoke((Action)(() => GameData.MainForm.ModsInfoLabel.Text = "Folder: " + Path.GetFileNameWithoutExtension(dir)));

//                    Compressor.Compress(dir, GetResultTargetName(categories, dir));

//                    GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Value = i));
//                    i++;
//                }
//                GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Visible = false));
//            }
//        }

//        /// <summary>
//        /// Get result subfolder in Downloads dir as set for mod in categories.dat
//        /// </summary>
//        /// <param name="categories"></param>
//        /// <param name="inputmoddir"></param>
//        /// <returns></returns>
//        private static string GetResultTargetName(List<CategoriesList> categories, string inputmoddir)
//        {
//            string targetdir = ManageSettings.GetDownloadsPath();
//            targetdir = ManageSettings.GetCurrentGameInstallDirPath();

//            //Sideloader mods
//            if (Directory.Exists(Path.Combine(inputmoddir, "mods")))
//            {
//                targetdir = Path.Combine(targetdir, "Sideloader");
//            }

//            string categoryvalue = ManageModOrganizer.GetMetaParameterValue(Path.Combine(inputmoddir, "meta.ini"), "category").Replace("\"", string.Empty);
//            if (categoryvalue.Length == 0)
//            {
//            }
//            else
//            {
//                //Subcategory from meta
//                categoryvalue = categoryvalue.Split(',')[0];//взять только первое значение
//                int categiryindex = int.Parse(categoryvalue, CultureInfo.InvariantCulture) - 1;//В List индекс идет от нуля
//                if (categiryindex > 0)
//                {
//                    if (categiryindex < categories.Count)//на всякий, защита от ошибки выхода за диапазон
//                    {
//                        //Проверить родительскую категорию
//                        int parentIDindex = int.Parse(categories[categiryindex].ParentId, CultureInfo.InvariantCulture) - 1;//В List индекс идет от нуля
//                        if (parentIDindex > 0 && parentIDindex < categories.Count)
//                        {
//                            targetdir = Path.Combine(targetdir, categories[parentIDindex].Name);
//                        }

//                        targetdir = Path.Combine(targetdir, categories[categiryindex].Name);

//                        Directory.CreateDirectory(targetdir);
//                    }
//                }
//            }

//            return targetdir;
//        }

//        private static void PackSeparators()
//        {
//            if (!Directory.Exists(ManageSettings.GetCurrentGameModsDirPath()))
//            {
//                return;
//            }

//            string[] dirs = Directory.GetDirectories(ManageSettings.GetCurrentGameModsDirPath(), "*").Where(name => name.EndsWith("_separator", StringComparison.OrdinalIgnoreCase)).ToArray();//с игнором сепараторов
//            if (dirs.Length == 0)
//            {
//            }
//            else
//            {
//                //GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Visible = true));
//                //GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Maximum = dirs.Length));
//                //GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Value = 0));
//                string tempdir = Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), "MOModsSeparators");

//                GameData.MainForm.DataInfoLabel.Invoke((Action)(() => GameData.MainForm.DataInfoLabel.Text = "Compressing"));
//                GameData.MainForm.ModsInfoLabel.Invoke((Action)(() => GameData.MainForm.ModsInfoLabel.Text = "Folder: " + Path.GetFileNameWithoutExtension(tempdir)));

//                Directory.CreateDirectory(tempdir);
//                foreach (string dir in dirs)
//                {
//                    CopyFolder.CopyAll(dir, Path.Combine(tempdir, Path.GetFileName(dir)));
//                }

//                Compressor.Compress(tempdir, ManageSettings.GetAppResDir());
//                Directory.Delete(tempdir, true);

//                //GameData.MainForm.progressBar1.Invoke((Action)(() => GameData.MainForm.progressBar1.Visible = false));
//            }
//        }
//    }
//}
