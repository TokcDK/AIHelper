using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AIHelper.Games;
using AIHelper.SharedData;
using GetListOfSubClasses;
using INIFileMan;
using NLog;
using MAB.DotIgnore;
using AIHelper.Forms.Other;

namespace AIHelper.Manage
{
    static class ManageOther
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();
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
            while (ManageSettings.IsMoMode && (ManageSettings.SetModOrganizerINISettingsForTheGame || ManageSettings.CurrentGameIsChanging) && loopsCount < maxLoops)
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

                ManageSettings.MainForm.DisposeTooltips(); // tooltips is not need here
            }
            else if (forceMaximize || form.WindowState == FormWindowState.Minimized)
            {
                form.WindowState = FormWindowState.Normal;

                ManageSettings.MainForm.SetTooltips(); // set tooltips again
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
        internal static void GetListOfExistsGames()
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
            var added = new HashSet<string>();
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
                string gameDirPath = "";
                try
                {
                    gameDirPath = Path.GetFullPath(entrie);
                }
                catch { continue; } // skip when invalid chars in path

                if (!Directory.Exists(gameDirPath)) continue; // skip not exists
                if (added.Contains(gameDirPath)) continue; // skip duples

                ManageSettings.Games = new GameData();
                foreach (var gameType in listOfGames)
                {
                    //if (gameType == typeof(RootGame)) continue;

                    var game = (GameBase)Activator.CreateInstance(gameType);

                    bool exeInData = File.Exists(Path.Combine(gameDirPath, "Data", game.GameExeName + ".exe"));
                    bool exeInRoot = File.Exists(Path.Combine(gameDirPath, game.GameExeName + ".exe"));

                    if (!exeInData && exeInRoot)
                    {
                        // when all game files in root dir

                        var dataDir = Path.Combine(gameDirPath, "Data");
                        Directory.CreateDirectory(dataDir);

                        // move dirs except data into data
                        foreach (var dir in Directory.GetDirectories(gameDirPath))
                        {
                            if (string.Equals(dir, dataDir)) continue;

                            Directory.Move(dir, Path.Combine(dataDir, Path.GetFileName(dir)));
                        }
                        // move files into data
                        foreach (var file in Directory.GetFiles(gameDirPath))
                        {
                            File.Move(file, Path.Combine(dataDir, Path.GetFileName(file)));
                        }
                    }

                    if (!File.Exists(Path.Combine(gameDirPath, "Data", game.GameExeName + ".exe"))) continue;

                    game.GameDirInfo = new DirectoryInfo(gameDirPath);
                    ManageSettings.Games.Game = game; // temp set current game

                    var mods = Path.Combine(gameDirPath, "Mods");
                    Directory.CreateDirectory(mods);

                    //  check and write mod organizer dir
                    var mo = Path.Combine(gameDirPath, ManageSettings.AppModOrganizerDirName);
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
                        ini.SetKey("General", "selected_profile", "@ByteArray(Default)");
                        ini.SetKey("Settings", "mod_directory", Path.Combine(gameDirPath, game.GameDirName, "Mods").Replace("\\", "\\\\"));
                        ini.SetKey("Settings", "download_directory", Path.Combine(gameDirPath, game.GameDirName, "Downloads").Replace("\\", "\\\\"));
                        ini.SetKey("Settings", "download_directory", Path.Combine(gameDirPath, game.GameDirName, ManageSettings.AppModOrganizerDirName, "profiles").Replace("\\", "\\\\"));
                        ini.SetKey("Settings", "download_directory", Path.Combine(gameDirPath, game.GameDirName, ManageSettings.AppModOrganizerDirName, "overwrite").Replace("\\", "\\\\"));
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
                    added.Add(gameDirPath); // add game path to the added list to prevent duplicates
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

            ManageSettings.Games.Games = listOfGameDirs;
            if (ManageSettings.Games.Games.Count > 0) ManageSettings.Games.Game = listOfGameDirs[0];

            //return listOfGameDirs;
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

        internal static bool AddNewGame(MainForm main)
        {
            var browseDialog = new OpenFileDialog();
            browseDialog.Filter = "Game exe|*.exe";
            browseDialog.Multiselect = false;
            browseDialog.Title = T._("Select exe in the game dir you want to add..");
            var result = browseDialog.ShowDialog();
            if (result != DialogResult.OK) return false;

            var filePath = browseDialog.FileName;
            if (!File.Exists(filePath)) return false;

            string exeName = Path.GetFileNameWithoutExtension(filePath);

            // check if exe is exe of one of valid games
            bool invalid = true;
            foreach (var gameType in Inherited.GetListOfInheritedTypes(typeof(GameBase)))
            {
                var game = (GameBase)Activator.CreateInstance(gameType);
                if (game.GameExeName == exeName
                    || game.GameExeNameX32 == exeName
                    || game.GameExeNameVr == exeName
                    || game.GameStudioExeName == exeName
                    || game.GameStudioExeNameX32 == exeName
                    || game.IniSettingsExeName == exeName
                    )
                {
                    invalid = false;
                    break;
                }
            }

            if (invalid) return false;

            string gameTxtPath = Path.Combine(ManageSettings.GamesBaseFolderPath, $"{exeName}.txt");

            string parentDir = Path.GetDirectoryName(filePath);
            bool isInData = string.Equals(Path.GetFileName(parentDir), "data", StringComparison.InvariantCultureIgnoreCase);

            string gamePathToAdd = isInData ? Path.GetDirectoryName(parentDir) : parentDir;

            // add game path to txt with selected exe name
            if (File.Exists(gameTxtPath))
            {
                bool lineAlreadyAdded = false;
                var lines = File.ReadAllLines(gameTxtPath);
                foreach (var line in lines)
                {
                    if (string.Equals(gamePathToAdd, line, StringComparison.InvariantCultureIgnoreCase))
                    {
                        lineAlreadyAdded = true;
                        break;
                    }
                }

                // write new path in the file if missing
                if (!lineAlreadyAdded) File.WriteAllLines(gameTxtPath, lines.Concat(new[] { gamePathToAdd }).ToArray());
            }
            else File.WriteAllText(gameTxtPath, $"{gamePathToAdd}\r\n");

            // update games list
            SetListOfAddedGames(main);

            return true;
        }

        public static List<string> GetKnownGames(INIFile ini)
        {
            List<string> list = new List<string>();
            if (ini == null || ini.Configuration == null) return list;

            var games = ini.GetKey(ManageSettings.SettingsIniSectionName, ManageSettings.KnownGamesIniKeyName);

            if (!string.IsNullOrWhiteSpace(games))
            {
                var gamesList = games.Split('|');
                if (gamesList != null && gamesList.Length > 0)
                {
                    gamesList = gamesList.Where(p => Directory.Exists(p)).Distinct().ToArray();
                    foreach (var path in gamesList) list.Add(path);
                }
            }

            return list;
        }

        public static bool SetListOfAddedGames(MainForm main)
        {
            try
            {
                var ini = ManageIni.GetINIFile(ManageSettings.AiHelperIniPath);
                ManageSettings.KnownGames = GetKnownGames(ini);

                ManageOther.GetListOfExistsGames();

                bool isNoGamesFound = ManageSettings.KnownGames.Count == 0 && (ManageSettings.Games.Games == null || ManageSettings.Games.Games.Count == 0);

                if (isNoGamesFound) isNoGamesFound = !AddNewGame(main) && ManageSettings.KnownGames.Count == 0 && (ManageSettings.Games.Games == null || ManageSettings.Games.Games.Count == 0);

                if (isNoGamesFound)
                {
                    MessageBox.Show(T._("Games not found") + "."
                        + Environment.NewLine + T._("Need atleast one game in subfolder in Games folder") + "."
                        + Environment.NewLine + "----------------"
                        + Environment.NewLine + T._("List of games") + ":"
                        + Environment.NewLine + ManageSettings.FolderNamesOfFoundGame
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
                else
                {
                    List<string> newGamesReport = new List<string>();
                    foreach (var game in ManageSettings.Games.Games)
                    {
                        var gamePath = game.GamePath;
                        if (ManageSettings.KnownGames.Contains(gamePath)) continue;

                        ManageSettings.KnownGames.Add(gamePath);
                        newGamesReport.Add("\n" + game.GameDisplayingName + ": " + gamePath);
                    }

                    newGamesReport = newGamesReport.OrderBy(p => p).ToList();

                    if (newGamesReport.Count > 0)
                    {
                        MessageBox.Show(T._("New games:") +
                            "\n"
                            + Environment.NewLine + "----------------"
                            + string.Join("", newGamesReport)
                            + Environment.NewLine + "----------------"
                        );
                    }
                }

                string selected_game;
                if (ini.Configuration == null)
                {
                    selected_game = "";
                }
                else
                {
                    selected_game = ini.GetKey(ManageSettings.SettingsIniSectionName, ManageSettings.SelectedGameIniKeyName);
                    if (string.IsNullOrWhiteSpace(selected_game))
                    {
                        var game = ManageSettings.Games.Games[0];
                        selected_game = game.GameDirName;
                    }
                }

                ManageSettings.Games.Game = ManageSettings.Games.Games.First(g => g.GameDirName == selected_game);
                var bindingSource1 = new BindingSource();
                bindingSource1.DataSource = ManageSettings.Games.Games;
                main.CurrentGameComboBox.DataSource = bindingSource1.DataSource;
                main.CurrentGameComboBox.DisplayMember = "GameDirName";
                main.CurrentGameComboBox.ValueMember = "GameDirName";

                //foreach (var game in ManageSettings.Games.Games)
                //{
                //    CurrentGameComboBox.Items.Add(game.GameDirName);
                //}
                if (ManageSettings.Games.Games.Count == 1) main.CurrentGameComboBox.Enabled = false;

                //SetSelectedGameIndexAndBasicVariables(ManageSettings.GetCurrentGameIndexByFolderName(
                //        ManageSettings.Games.Games
                //        ,
                //        selected_game
                //        ));

                ini.SetKey(ManageSettings.SettingsIniSectionName, ManageSettings.KnownGamesIniKeyName, string.Join("|", ManageSettings.KnownGames));

                try
                {
                    main.CurrentGameTitleTextBox.Text = ManageSettings.Games.Game.GameDisplayingName;
                    main.CurrentGameTitleTextBox.Enabled = false;
                }
                catch (Exception ex) { _log.Info("Error while game title setup. Set more specific exception there. Error:\r\n" + ex); }
            }
            catch (Exception ex)
            {
                _log.Debug("An error occured while SetListOfGames.path=" + ManageSettings.AiHelperIniPath + "\r\n error:\r\n" + ex);
                return false;
            }

            return true;
        }

        public static void SetSelectedGameIndexAndBasicVariables(MainForm main)
        {
            //ManageSettings.Games.CurrentGameListIndex = index;
            //ManageSettings.Games.Game = ManageSettings.Games.Games[ManageSettings.Games.CurrentGameListIndex];
            //CurrentGameComboBox.SelectedIndex = index;

            //set checkbox
            ManageSettings.AutoShortcutRegistryCheckBoxChecked = bool.Parse(ManageIni.GetIniValueIfExist(ManageSettings.AiHelperIniPath, "autoCreateShortcutAndFixRegystry", "Settings", "False"));
            main.AutoShortcutRegistryCheckBox.Checked = ManageSettings.AutoShortcutRegistryCheckBoxChecked;
        }

        /// <summary>
        /// clean Data dir of current selected game from not native files
        /// </summary>
        internal static void CleanCurrentGameDataDir()
        {
            if (!ManageSettings.IsMoMode) return; // dont touch in common mode

            var options = new CleanOptionsDialogForm
            {
                Location = new Point(ManageSettings.MainForm.Location.X, ManageSettings.MainForm.Location.Y),
                StartPosition = FormStartPosition.Manual
            };

            DialogResult result = options.ShowDialog();
            if (result != DialogResult.OK) return;

            // set vars
            bool moveIntoNewMod = options.cbxMoveToNewMod.Checked;
            var cleanDataDirInfoPath = ManageSettings.CurrentGameCleanFunctionDirPath;
            var dataDipPath = ManageSettings.CurrentGameDataDirPath;
            int dataDipPathLength = dataDipPath.Length;
            var currentGameAbbr = ManageSettings.CurrentGame.GameAbbreviation;
            var blackListPath = ManageSettings.CurrentGameCleanFunctionBlackListFilePath;
            var whiteListPath = ManageSettings.CurrentGameCleanFunctionWhiteListFilePath;
            var hardcodedWhiteList = ManageSettings.CurrentGameCleanFunctionHardcodedWhiteList;
            var dir2move = Path.Combine(ManageSettings.CurrentGameDirPath, "testbak");
            Directory.CreateDirectory(dir2move);

            options.Dispose();

            // fill lists
            //var blackList = new HashSet<string>();
            //var whiteList = new HashSet<string>();
            var blackList = File.Exists(blackListPath) ? new IgnoreList(blackListPath) : null;
            var whiteList = File.Exists(whiteListPath) ? new IgnoreList(whiteListPath) : null;

            int failedCount = 0;
            int movedDirs = 0;
            int movedFiles = 0;

            // whiteList
            // move dirs first
            foreach (var item in new DirectoryInfo(dataDipPath).EnumerateDirectories("*", SearchOption.AllDirectories).Where(d => !whiteList.IsIgnored(d) && d.Exists))
            {
                try
                {
                    var targetPath = $"{dir2move}{item.FullName.Substring(dataDipPathLength)}";
                    item.MoveTo(targetPath);
                    movedDirs++;
                }
                catch (IOException ex)
                {
                    failedCount++;
                    _log.Error($"{nameof(CleanCurrentGameDataDir)}: Failed to remove dir '{item.FullName}'. Error:\r\n{ex}");
                }
            }
            // move files
            foreach (var item in new DirectoryInfo(dataDipPath).EnumerateFiles("*.*", SearchOption.AllDirectories).Where(f => !whiteList.IsIgnored(f) && f.Exists))
            {
                try
                {
                    var targetPath = $"{dir2move}{item.FullName.Substring(dataDipPathLength)}";
                    item.MoveTo(targetPath);
                    movedFiles++;
                }
                catch (IOException ex)
                {
                    failedCount++;
                    _log.Error($"{nameof(CleanCurrentGameDataDir)}: Failed to remove file '{item.FullName}'. Error:\r\n{ex}");
                }
            }

            // blacklist
            // move dirs first
            foreach (var item in new DirectoryInfo(dataDipPath).EnumerateDirectories("*", SearchOption.AllDirectories).Where(d => blackList.IsIgnored(d) && d.Exists))
            {
                try
                {
                    var targetPath = $"{dir2move}{item.FullName.Substring(dataDipPathLength)}";
                    item.MoveTo(targetPath);
                    movedDirs++;
                }
                catch (IOException ex)
                {
                    failedCount++;
                    _log.Error($"{nameof(CleanCurrentGameDataDir)}: Failed to remove dir '{item.FullName}'. Error:\r\n{ex}");
                }
            }
            // move files
            foreach (var item in new DirectoryInfo(dataDipPath).EnumerateFiles("*.*", SearchOption.AllDirectories).Where(f => blackList.IsIgnored(f) && f.Exists))
            {
                try
                {
                    var targetPath = $"{dir2move}{item.FullName.Substring(dataDipPathLength)}";
                    item.MoveTo(targetPath);
                    movedFiles++;
                }
                catch (IOException ex)
                {
                    failedCount++;
                    _log.Error($"{nameof(CleanCurrentGameDataDir)}: Failed to remove file '{item.FullName}'. Error:\r\n{ex}");
                }
            }

            MessageBox.Show($"Cleaning finished! Moved {movedDirs} dirs and {movedFiles} files. Failed to move {failedCount} items.");

            // fill from hardcoded whitelist
            //foreach (var str in hardcodedWhiteList)
            //{
            //    var subpath = str;
            //    int lastCharIndex = subpath.Length - 1;
            //    bool isDir = subpath[lastCharIndex] == '\\';
            //    if (isDir) subpath = subpath.Remove(lastCharIndex);
            //    bool isTopDir = subpath.Remove(lastCharIndex).IndexOf('\\') == -1;
            //    var searchOption = isTopDir ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;

                //    //var path = $"{dataDipPath}\\{subpath}";

                //    var paths = isDir ? Directory.EnumerateDirectories(dataDipPath, subpath, searchOption) : Directory.EnumerateFiles(dataDipPath, subpath, searchOption);

                //    foreach (var entrie in paths)
                //    {
                //        // skip missing paths
                //        if (isDir)
                //        {
                //            if (!Directory.Exists(entrie)) continue;
                //        }
                //        else if (!File.Exists(entrie)) continue;

                //        if (whiteList.Contains(entrie)) continue;
                //        whiteList.Add(entrie);
                //    }
                //}

        }
    }
}
