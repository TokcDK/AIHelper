using AIHelper.Games;
using AIHelper.Manage.ui.themes;
using AIHelper.SharedData;
using GetListOfSubClasses;
using INIFileMan;
using NLog;
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
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Wait while the game is changing or MO ini settings are setting
        /// </summary>
        internal static async Task WaitIfGameIsChanging()
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

        /// <summary>
        /// Shows whether a check box is on by colouring its caption.
        /// </summary>
        /// <remarks>
        /// The colours come from the theme rather than from a fixed pair, because a
        /// fixed pale green is only readable on a dark surface. The accent is used
        /// while it is on and the ordinary text colour while it is off, so the caption
        /// stays readable whichever theme is in use.
        /// </remarks>
        public static void CheckBoxChangeColor(CheckBox checkBox)
        {
            if (checkBox == null) return;

            var palette = ThemesLoader.CurrentPalette;
            if (palette == null) return;

            checkBox.ForeColor = checkBox.Checked ? palette.Accent : palette.Text;
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
            foreach (var form in forms) SwitchFormMinimizedNormal(form, forceMinimize, forceMaximize);
        }

        internal static void SwitchFormMinimizedNormal(Form form, bool forceMinimize = false, bool forceMaximize = false)
        {
            //http://www.cyberforum.ru/windows-forms/thread31052.html
            if (form == null || form.IsDisposed)
            {
                return;
            }

            if (forceMinimize || form.WindowState == FormWindowState.Normal)
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
            List<Type> listOfGames = Inherited.GetListOfInheritedTypes(typeof(GameBase));

            var listOfGameDirs = new List<GameBase>();

            Directory.CreateDirectory(ManageSettings.GamesBaseFolderPath);// create games dir

            // not empty txt diles in games dir where 1st line is exists dir path
            var foundPathInTxt = GetGamesFromTxt();

            // dirs in games dir
            var dirs = Directory.EnumerateDirectories(ManageSettings.GamesBaseFolderPath).Concat(foundPathInTxt);
            var added = new HashSet<string>();
            ManageSettings.Games = new GameData();

            foreach (var pathString in dirs)
            {
                string gameDirPath = GetGameDirPath(pathString);

                if (string.IsNullOrWhiteSpace(gameDirPath)) continue; // skip invalid path
                if (!Directory.Exists(gameDirPath)) continue; // skip not exists
                if (added.Contains(gameDirPath)) continue; // skip duples

                foreach (var gameType in listOfGames)
                {
                    if (TryAddTheGameIntoGamesList(gameType, gameDirPath, listOfGameDirs, added))
                    {
                        break;
                    }
                }
            }

            ManageSettings.Games.Games = listOfGameDirs;
            if (ManageSettings.Games.Games.Count > 0) ManageSettings.Games.Game = listOfGameDirs[0];

        }

        private static string GetGameDirPath(string pathString)
        {
            try
            {
                return Path.GetFullPath(pathString);
            }
            catch (Exception ex)
            {
                // skip when invalid chars in path
                _log.Debug("Invalid game path skipped. Path=" + pathString + "\r\nError:\r\n" + ex.Message);
                return "";
            }
        }

        private static bool TryAddTheGameIntoGamesList(Type gameType, string gameDirPath, List<GameBase> listOfGameDirs, HashSet<string> added)
        {
            var game = (GameBase)Activator.CreateInstance(gameType);

            bool exeInData = File.Exists(Path.Combine(gameDirPath, ManageSettings.DataDirName, game.GameExeName + ".exe"));
            bool exeInRoot = File.Exists(Path.Combine(gameDirPath, game.GameExeName + ".exe"));

            if (!exeInData && exeInRoot)
            {
                SortFilesForMO(gameDirPath);
            }

            if (!File.Exists(Path.Combine(gameDirPath, ManageSettings.DataDirName, game.GameExeName + ".exe"))) return false;

            game.GameDirInfo = new DirectoryInfo(gameDirPath);
            ManageSettings.Games.Game = game; // temp set current game

            var mods = Path.Combine(gameDirPath, ManageSettings.ModsDirName);
            Directory.CreateDirectory(mods);

            //  check and write mod organizer dir
            var mo = Path.Combine(gameDirPath, ManageSettings.AppModOrganizerDirName);
            Directory.CreateDirectory(mo);

            //  check and write mod organizer ini
            CheckAndWriteMOini(mo, game, gameDirPath);

            // check and write categories dat
            var catDat = Path.Combine(mo, ManageSettings.MoCategoriesFileName);
            if (!File.Exists(catDat)) File.WriteAllText(catDat, "");

            // check and write default profile
            WriteDefaultMOProfile(mo);

            listOfGameDirs.Add(game);
            added.Add(gameDirPath); // add game path to the added list to prevent duplicates

            return true;
        }

        private static void WriteDefaultMOProfile(string mo)
        {
            var profiles = Path.Combine(mo, ManageSettings.MoProfilesDirName);
            var defaultProfile = Path.Combine(profiles, "Default");
            if (!Directory.Exists(defaultProfile))
            {
                Directory.CreateDirectory(defaultProfile);
                File.WriteAllText(Path.Combine(defaultProfile, "modlist.txt"), "# This file was automatically generated by Mod Organizer.\r\n");
            }
        }

        private static void CheckAndWriteMOini(string mo, GameBase game, string gameDirPath)
        {
            var moIni = Path.Combine(mo, ManageSettings.MoIniFileName);
            if (File.Exists(moIni))
            {
                return;
            }

            File.WriteAllText(moIni, Properties.Resources.defmoini);

            var ini = ManageIni.GetINIFile(moIni);

            // check mo ini game parameters exist
            ini.SetKey("General", "gameName", game.GetGameName());
            ini.SetKey("General", "gamePath", "@ByteArray(" + Path.Combine(game.GameDirInfo.Parent.FullName, game.GameDirName).Replace("\\", "\\\\") + ")");
            ini.SetKey("General", "selected_profile", "@ByteArray(Default)");
            ini.SetKey("Settings", "mod_directory", Path.Combine(gameDirPath, ManageSettings.ModsDirName).Replace("\\", "\\\\"));
            ini.SetKey("Settings", "download_directory", Path.Combine(gameDirPath, "Downloads").Replace("\\", "\\\\"));
            ini.SetKey("Settings", "profiles_directory", Path.Combine(gameDirPath, ManageSettings.AppModOrganizerDirName, "profiles").Replace("\\", "\\\\"));
            ini.SetKey("Settings", "overwrite_directory", Path.Combine(gameDirPath, ManageSettings.AppModOrganizerDirName, "overwrite").Replace("\\", "\\\\"));
        }

        private static void SortFilesForMO(string gameDirPath)
        {
            // when all game files in root dir
            var dataDir = Path.Combine(gameDirPath, ManageSettings.DataDirName);
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

        private static IEnumerable<string> GetGamesFromTxt()
        {
            // Directory.EnumerateFiles(GetGamesBaseFolderPath(), "*.txt").Where(t => new FileInfo(t).Length > 3 && Directory.Exists(Path.GetFullPath(File.ReadAllLines(t)[0]))).Select(t => File.ReadAllLines(t)[0])

            foreach (var txt in Directory.EnumerateFiles(ManageSettings.GamesBaseFolderPath, "*.txt"))
            {
                if (new FileInfo(txt).Length < 4) continue;

                using (StreamReader sr = new StreamReader(txt))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        if (line.TrimStart().StartsWith(";")) continue;

                        var path = GetGameDirPath(line);
                        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) continue;

                        yield return path;
                    }
                }
            }
        }

        /// <summary>
        /// Create a dummy game in games folder for first run when no games found
        /// </summary>
        /// <param name="main"></param>
        /// <returns>main dummy exe path</returns>
        private static string CreateDummyGame()
        {
            try
            {
                string newGameDirName = "DummyGame";
                string newGameDirPath = Path.Combine(ManageSettings.GamesBaseFolderPath, newGameDirName);
                Directory.CreateDirectory(newGameDirPath);

                // create data dir and main dummy exe
                string dataDirPath = Path.Combine(newGameDirPath, ManageSettings.DataDirName);
                Directory.CreateDirectory(dataDirPath);
                string dummyExePath = Path.Combine(dataDirPath, "Koikatu.exe");
                File.WriteAllText(dummyExePath, "This is dummy game exe file.");
                File.WriteAllText(Path.Combine(dataDirPath, "InitSetting.exe"), "This is dummy InitSetting exe file.");
                Directory.CreateDirectory(Path.Combine(dataDirPath, "Koikatsu_data"));
                // create UserData dir
                string userDataDirPath = Path.Combine(dataDirPath, "UserData");
                Directory.CreateDirectory(userDataDirPath);
                // create setup.xml file in UserData
                File.WriteAllText(Path.Combine(userDataDirPath, "setup.xml"), ManageSettings.DefaultSetupXmlValue);

                // create mods dir
                string modsDirPath = Path.Combine(newGameDirPath, ManageSettings.ModsDirName);
                Directory.CreateDirectory(modsDirPath);
                File.WriteAllText(Path.Combine(dataDirPath, "Koikatsu.exe"), "This is dummy game exe file.");

                //create the game MO dir
                var mo = Path.Combine(newGameDirPath, ManageSettings.AppModOrganizerDirName);
                Directory.CreateDirectory(mo);
                // create dummy ModOrganizer.ini
                File.WriteAllText(Path.Combine(mo, "ModOrganizer.ini"), $"[General]\r\ngameName=Koikatsu\r\ngamePath=@ByteArray({newGameDirPath})\r\nselected_profile=@ByteArray(Default)\r\nversion=2.4.2\r\nfirst_start=false");
                // create empty categories.dat
                File.WriteAllText(Path.Combine(mo, "categories.dat"), "");
                // create default profile
                var profiles = Path.Combine(mo, "profiles");
                var defaultProfile = Path.Combine(profiles, "Default");
                Directory.CreateDirectory(defaultProfile);
                // create empty modlist.txt
                File.WriteAllText(Path.Combine(defaultProfile, "modlist.txt"), "# This file was automatically generated by Mod Organizer.\r\n");
                // create overwrite dir
                var overwrite = Path.Combine(mo, "overwrite");
                Directory.CreateDirectory(overwrite);

                return dummyExePath;
            }
            catch (Exception ex)
            {
                _log.Error("Failed to create the dummy game. Error:\r\n" + ex);
                return "";
            }
        }

        internal static bool AddNewGame(MainForm main, string newGameMainExePath = "")
        {
            if(string.IsNullOrWhiteSpace(newGameMainExePath))
            {
                var browseDialog = new OpenFileDialog();
                browseDialog.Filter = "Game exe|*.exe";
                browseDialog.Multiselect = false;
                browseDialog.Title = T._("Select exe in the game dir you want to add..");
                var result = browseDialog.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return false;
                }

                newGameMainExePath = browseDialog.FileName;
            }

            if (!File.Exists(newGameMainExePath)) return false;

            string exeName = Path.GetFileNameWithoutExtension(newGameMainExePath);

            // check if exe is exe of one of valid games
            if (!IsKnownGameExe(exeName)) return false;

            string gameTxtPath = Path.Combine(ManageSettings.GamesBaseFolderPath, $"{exeName}.txt");

            string parentDir = Path.GetDirectoryName(newGameMainExePath);
            bool isInData = string.Equals(Path.GetFileName(parentDir), ManageSettings.DataDirName, StringComparison.InvariantCultureIgnoreCase);

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
            ManageOther.SetListOfAddedGames(main);

            return true;
        }

        /// <summary>
        /// true when exe with the <paramref name="exeName"/> name is main or additional exe of one of the known games
        /// </summary>
        /// <param name="exeName"></param>
        /// <returns></returns>
        private static bool IsKnownGameExe(string exeName)
        {
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
                    return true;
                }
            }

            return false;
        }

        public static List<string> GetKnownGames(INIFile ini)
        {
            List<string> list = new List<string>();
            if (ini == null) return list;

            var games = ini.GetKey(ManageSettings.SettingsIniSectionName, ManageSettings.KnownGamesIniKeyName);

            if (!string.IsNullOrWhiteSpace(games))
            {
                var gamesList = games.Split('|');
                gamesList = gamesList.Where(p => Directory.Exists(p)).Distinct().ToArray();
                foreach (var path in gamesList) list.Add(path);
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

                if (isNoGamesFound)
                {
                    isNoGamesFound = !AddNewGame(main, ManageOther.CreateDummyGame()) && ManageSettings.KnownGames.Count == 0 && (ManageSettings.Games.Games == null || ManageSettings.Games.Games.Count == 0);
                }

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

                var games = ManageSettings.Games.Games ?? new List<GameBase>();

                // selected game folder name saved in the app ini
                string selectedGameName = ini == null
                    ? string.Empty
                    : ini.GetKey(ManageSettings.SettingsIniSectionName, ManageSettings.SelectedGameIniKeyName);

                var selectedGame = games.FirstOrDefault(g => string.Equals(g.GameDirName, selectedGameName, StringComparison.InvariantCultureIgnoreCase));
                if (selectedGame == null && games.Count > 0)
                {
                    // select the first game when saved game folder name was not found in the games list
                    selectedGame = games[0];
                }

                if (selectedGame != null) ManageSettings.Games.Game = selectedGame;

                UpdateGamesListUi(main, games);

                ini.SetKey(ManageSettings.SettingsIniSectionName, ManageSettings.KnownGamesIniKeyName, string.Join("|", ManageSettings.KnownGames));

                SetCurrentGameTitle(main);
            }
            catch (Exception ex)
            {
                _log.Debug("An error occured while SetListOfGames.path=" + ManageSettings.AiHelperIniPath + "\r\n error:\r\n" + ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// update games list controls of the <paramref name="main"/> form
        /// </summary>
        /// <param name="main"></param>
        /// <param name="games"></param>
        private static void UpdateGamesListUi(MainForm main, List<GameBase> games)
        {
            main.CurrentGameComboBox.DataSource = games;
            main.CurrentGameComboBox.DisplayMember = "GameDirName";
            main.CurrentGameComboBox.ValueMember = "GameDirName";

            // no reason to switch the game when it is the only one
            main.CurrentGameComboBox.Enabled = games.Count != 1;
        }

        /// <summary>
        /// set current game title in the <paramref name="main"/> form
        /// </summary>
        /// <param name="main"></param>
        private static void SetCurrentGameTitle(MainForm main)
        {
            var currentGame = ManageSettings.Games.Game;
            if (currentGame == null) return;

            main.CurrentGameTitleTextBox.Text = currentGame.GameDisplayingName;
            main.CurrentGameTitleTextBox.Enabled = false;
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
    }
}
