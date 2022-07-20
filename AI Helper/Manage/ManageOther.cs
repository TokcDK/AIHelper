using AIHelper.Games;
using AIHelper.SharedData;
using GetListOfSubClasses;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIHelper.Manage
{
    static class ManageOther
    {
        internal static async void WaitIfGameIsChanging()
        {
            if (!ManageSettings.IsMoMode)
            {
                return;
            }

            await Task.Run(() => WaitIfGameIsChanging(1000)).ConfigureAwait(true);
        }

        private static void WaitIfGameIsChanging(int waittime, int maxLoops = 60)
        {
            int loopsCount = 0;
            while (ManageSettings.IsMoMode&& (ManageSettings.SetModOrganizerINISettingsForTheGame || ManageSettings.CurrentGameIsChanging) && loopsCount < maxLoops)
            {
                Thread.Sleep(waittime);
                loopsCount++;
            }
        }

        public static void CheckBoxChangeColor(CheckBox checkBox)
        {
            checkBox.ForeColor = checkBox.Checked ? Color.FromArgb(192, 255, 192) : Color.White;
        }

        public static void AutoShortcutAndRegystry()
        {
            CreateShortcuts();
            ManageRegistry.FixRegistry();
        }

        //https://bytescout.com/blog/create-shortcuts-in-c-and-vbnet.html
        public static void CreateShortcuts(bool force = false, bool auto = true)
        {
            if (!ManageSettings.AutoShortcutRegistryCheckBoxChecked && !force) return;

            //AI-Girl Helper
            string shortcutname = /*ManageSettings.GetCurrentGameFolderName() +*/ "AI " + T._("Helper");
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

            ManageModOrganizer.CheckMoUserdata();

            if (!auto) MessageBox.Show(T._("Shortcut") + " " + T._("created") + "!");
        }

        internal static void SwitchFormMinimizedNormalAll(Form[] forms, bool forceMinimize = false, bool forceMaximize = false)
        {
            foreach (var form in forms) SwitchFormMinimizedNormal(form);
        }

        internal static void SwitchFormMinimizedNormal(Form form, bool forceMinimize = false, bool forceMaximize = false)
        {
            //http://www.cyberforum.ru/windows-forms/thread31052.html
            if (form == null || form.IsDisposed)
            {
            }
            else if (forceMinimize || form.WindowState == FormWindowState.Normal)
            {
                form.WindowState = FormWindowState.Minimized;

                SharedData.GameData.MainForm.DisposeTooltips(); // tooltips is not need here
            }
            else if (forceMaximize || form.WindowState == FormWindowState.Minimized)
            {
                form.WindowState = FormWindowState.Normal;

                SharedData.GameData.MainForm.SetTooltips(); // set tooltips again
            }
        }

        //        internal static void ReportMessageForm(string title, string message)
        //        {
        ////#pragma warning disable CA2000 // Dispose objects before losing scope
        ////            Form ReportForm = new Form
        ////            {
        ////                Text = title,
        ////                //ReportForm.Size = new System.Drawing.Size(500,700);
        ////                AutoSize = true,
        ////                FormBorderStyle = FormBorderStyle.FixedDialog,
        ////                StartPosition = FormStartPosition.CenterScreen
        ////            };
        ////#pragma warning restore CA2000 // Dispose objects before losing scope
        ////            RichTextBox ReportTB = new RichTextBox
        ////            {
        ////                Size = new System.Drawing.Size(700, 900),
        ////                WordWrap = true,
        ////                Dock = DockStyle.Fill,
        ////                ReadOnly = true,
        ////                //ReportTB.BackColor = System.Drawing.Color.Gray;
        ////                Text = message,
        ////                ScrollBars = RichTextBoxScrollBars.Both
        ////            };

        ////            ReportForm.Controls.Add(ReportTB);
        ////            ReportForm.Show();
        //        }
        internal static List<GameBase> GetListOfExistsGames()
        {
            //List<Game> listOfGames = GamesList.GetGamesList();
            List<Type> listOfGames = Inherited.GetListOfInheritedTypes(typeof(GameBase));
            //var listOfGamesRead = new List<Game>(listOfGames);

            var listOfGameDirs = new List<GameBase>();

            Directory.CreateDirectory(ManageSettings.GamesBaseFolderPath);// create games dir

            // not empty txt diles in games dir where 1st line is exists dir path
            var foundPathInTxt = GetGamesFromTxt();
            // dirs in games dir
            var dirs = Directory.EnumerateDirectories(ManageSettings.GamesBaseFolderPath).Concat(foundPathInTxt);
            foreach (var entrie in dirs)
            {
                //string gameDir;
                //if (Path.GetFileName(entrie).StartsWith(ManageSettings.GetMOGameInfoFileIdentifier(), StringComparison.InvariantCultureIgnoreCase) && File.Exists(entrie))
                //{
                //    try
                //    {
                //        gameDir = Path.GetFullPath(File.ReadAllLines(entrie)[0]); // first line is game directory path
                //    }
                //    catch
                //    {
                //        // when invalid chars or other reason when getfullpath will fail
                //        continue;
                //    }
                //}
                //else
                //{
                //    gameDir = entrie;
                //}
                var gameDir = entrie;

                if (!Directory.Exists(gameDir))
                {
                    continue;
                }

                foreach (var gameType in listOfGames)
                {
                    //if (gameType == typeof(RootGame)) continue;

                    var game = (GameBase)Activator.CreateInstance(gameType);
                    if (!File.Exists(Path.Combine(gameDir, "Data", game.GameExeName + ".exe"))) continue;

                    game.GameDirInfo = new DirectoryInfo(gameDir);
                    GameData.Game = game; // temp set current game

                    var mods = Path.Combine(gameDir, "Mods");
                    Directory.CreateDirectory(mods);

                    //  check and write mod organizer dir
                    var mo = Path.Combine(gameDir, ManageSettings.AppModOrganizerDirName);
                    Directory.CreateDirectory(mo);

                    //  check and write mod organizer ini
                    var moIni = Path.Combine(mo, ManageSettings.MoIniFileName);
                    if (!File.Exists(moIni))
                    {
                        File.WriteAllText(moIni, Properties.Resources.defmoini);

                        var ini = ManageIni.GetINIFile(moIni);

                        // check mo ini game parameters exist
                        ini.SetKey("General", "gameName", game.GetGameName());
                        ini.SetKey("General", "gamePath", "@ByteArray(" + Path.Combine(game.GameDirInfo.Parent.FullName, game.GameDirName).Replace("\\", "\\\\") + ")");
                        ini.SetKey("General", "selected_profile", "Default");
                        ini.SetKey("Settings", "mod_directory", Path.Combine(gameDir, game.GameDirName, "Mods").Replace("\\", "\\\\"));
                        ini.SetKey("Settings", "download_directory", Path.Combine(gameDir, game.GameDirName, "Downloads").Replace("\\", "\\\\"));
                        ini.SetKey("Settings", "download_directory", Path.Combine(gameDir, game.GameDirName, ManageSettings.AppModOrganizerDirName, "profiles").Replace("\\", "\\\\"));
                        ini.SetKey("Settings", "download_directory", Path.Combine(gameDir, game.GameDirName, ManageSettings.AppModOrganizerDirName, "overwrite").Replace("\\", "\\\\"));
                    }

                    // check and write categories dat
                    var catDat = Path.Combine(mo, ManageSettings.MoCategoriesFileName);
                    if (!File.Exists(catDat)) File.WriteAllText(catDat, "");

                    // check and write default profile
                    var profiles = Path.Combine(mo, ManageSettings.MoProfilesDirName);
                    var defaultProfile = Path.Combine(profiles, "Default");
                    if (!Directory.Exists(defaultProfile))
                    {
                        Directory.CreateDirectory(defaultProfile);
                        File.WriteAllText(Path.Combine(defaultProfile, "modlist.txt"), "# This file was automatically generated by Mod Organizer.\r\n");
                    }

                    listOfGameDirs.Add(game);
                }

            }

            //if (Directory.Exists(GetGamesFolderPath()))
            //{
            //    foreach (var game in listOfGamesRead)
            //    {
            //        if (!game.IsValidGame())
            //        {
            //            listOfGames.Remove(game);
            //        }
            //    }
            //}
            //else
            //{
            //    listOfGames.Clear();
            //}


            // commented because obsolete
            ////if (listOfGames.Count == 0)
            //if (listOfGameDirs.Count == 0)
            //{
            //    // root game setup
            //    try
            //    {
            //        if (Directory.Exists(Path.Combine(ManageSettings.ApplicationStartupPath, "Mods"))
            //            &&
            //            Directory.Exists(Path.Combine(ManageSettings.ApplicationStartupPath, "Data"))
            //            &&
            //            !Path.Combine(ManageSettings.ApplicationStartupPath, "Data").IsNullOrEmptyDirectory(mask: "*.exe", searchForFiles: true, searchForDirs: false, recursive: false)
            //            //&&
            //            //!ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(GetMOdirPath())
            //            &&
            //            IsMoFolderValid(AppModOrganizerDirPath)
            //            //&&
            //            //Directory.Exists(Path.Combine(GetMOdirPath(), GetMoProfilesDirName())))
            //            //&&
            //            //!ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(GetMOdirPath(), GetMoProfilesDirName()))
            //            &&
            //            !ManageSymLinkExtensions.IsSymlink(MoIniFilePath)
            //            &&
            //            !ManageSymLinkExtensions.IsSymlink(MoCategoriesFilePath)
            //            )
            //        {
            //            var game = new RootGame
            //            {
            //                // the app's dir
            //                GameDirInfo = new DirectoryInfo(ManageSettings.ApplicationStartupPath)
            //            };

            //            //listOfGames.Add(new RootGame());
            //            listOfGameDirs.Add(game);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        _log.Debug("RootGame check failed. Error:" + Environment.NewLine + ex);
            //    }
            //}

            //ListOfGames = ListOfGames.Where
            //(game =>
            //    Directory.Exists(game.GetGamePath())
            //    &&
            //    !ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(game.GetGamePath(), GetAppModOrganizerDirName(), GetMoProfilesDirName()))
            //).ToList();

            return listOfGameDirs;
        }

        private static IEnumerable<string> GetGamesFromTxt()
        {
            // Directory.EnumerateFiles(GetGamesBaseFolderPath(), "*.txt").Where(t => new FileInfo(t).Length > 3 && Directory.Exists(Path.GetFullPath(File.ReadAllLines(t)[0]))).Select(t => File.ReadAllLines(t)[0])

            foreach (var txt in Directory.EnumerateFiles(ManageSettings.GamesBaseFolderPath, "*.txt"))
            {
                if (new FileInfo(txt).Length < 4) continue;

                StreamReader sr = new StreamReader(txt);
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (line.TrimStart().StartsWith(";")) continue;

                    var path = Path.GetFullPath(line);
                    if (!Directory.Exists(path)) continue;

                    yield return path;
                }
            }
        }
    }
}
