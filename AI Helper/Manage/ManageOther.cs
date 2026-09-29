using AIHelper.Games;
using AIHelper.Manage.ui.themes;
using AIHelper.SharedData;
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
        /// <summary>
        /// Builds the list of the games which are in the games base folder.
        /// </summary>
        /// <remarks>
        /// A game is a folder which holds the main exe of one of the game presets in its
        /// data folder, or which is named by a list file in the games base folder. The
        /// folder structure of every game which is found is completed while the list is
        /// built, so a build the user copied in can be managed right away.
        /// </remarks>
        internal static void GetListOfExistsGames()
        {
            Directory.CreateDirectory(ManageSettings.GamesBaseFolderPath); // create games dir

            // folders in the games dir and folders named by the list files in it.
            // Materialised on purpose: the folders of a game are created while the list is
            // built, and a folder tree must not be walked while it is changed.
            var gameDirPaths = Directory.EnumerateDirectories(ManageSettings.GamesBaseFolderPath)
                .Concat(GameFolderRegistry.ReadGameFolderPaths())
                .ToArray();

            // the game name of a Mod Organizer ini is read from the basic game plugin of
            // the current game, so a game has to be the current one while its folder is
            // completed. The list is filled in place and is only read by the caller once
            // the whole scan is done.
            var gameData = new GameData();
            ManageSettings.Games = gameData;

            var games = new List<GameBase>();
            var addedGameDirPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var pathString in gameDirPaths)
            {
                string gameDirPath = GameFolderRegistry.GetGameDirPath(pathString);

                if (gameDirPath == null) continue; // skip invalid path
                if (!Directory.Exists(gameDirPath)) continue; // skip not exists
                if (!addedGameDirPaths.Add(gameDirPath)) continue; // skip duples

                var game = FindGame(gameDirPath);
                if (game == null) continue;

                games.Add(game);
            }

            gameData.Games = games;
            if (games.Count > 0) gameData.Game = games[0];
        }

        /// <summary>
        /// Completes the folder structure of a game folder and returns the game it holds.
        /// </summary>
        /// <param name="gameDirPath">folder which is expected to hold a game</param>
        /// <returns>the game, or null when the folder does not hold a known game</returns>
        private static GameBase FindGame(string gameDirPath)
        {
            // the presets are tried in a fixed order: a folder which holds the exes of two
            // games has to be claimed by the same game on every run
            foreach (var gameType in GamePresets.Types)
            {
                GameBase game;
                try
                {
                    game = (GameBase)Activator.CreateInstance(gameType);
                }
                catch (Exception ex)
                {
                    // a preset which cannot be created must not cost the games of the others
                    _log.Warn("Failed to create the game preset " + gameType.Name + ". Error:\r\n" + ex);
                    continue;
                }

                // the files of a build which is not sorted yet are in the game folder
                // itself rather than in its data folder
                if (!GameFolderLayout.HasDataExe(game, gameDirPath) && GameFolderLayout.HasRootExe(game, gameDirPath))
                {
                    GameFolderLayout.MoveGameFilesIntoDataFolder(gameDirPath);
                }

                if (!GameFolderLayout.HasDataExe(game, gameDirPath)) continue;

                game.GameDirInfo = new DirectoryInfo(gameDirPath);
                ManageSettings.Games.Game = game; // temp set current game

                try
                {
                    GameFolderLayout.EnsureCreated(gameDirPath, game.GetGameName(), Properties.Resources.defmoini);
                }
                catch (Exception ex)
                {
                    // a folder which cannot be completed is a game all the same: the user
                    // can see it, and the app completes it on the next start
                    _log.Error("Failed to complete the folder of the game " + gameDirPath + ". Error:\r\n" + ex);
                }

                return game;
            }

            return null;
        }

        /// <summary>
        /// Folder and exe name of the placeholder game which is created when no game was
        /// found at all.
        /// </summary>
        /// <remarks>
        /// The exe is the main exe of the Koikatsu preset, because a folder is only
        /// accepted as a game when its data folder holds the main exe of a preset.
        /// </remarks>
        private const string DummyGameDirName = "DummyGame";
        private const string DummyGameExeName = "Koikatu";
        private const string DummyGameExeContent = "This is dummy game exe file.";
        private const string DummyGameInitSettingExeContent = "This is dummy InitSetting exe file.";
        private const string DummyGameDataDirName = DummyGameExeName + "_Data";

        /// <summary>
        /// Creates a dummy game in the games folder, used on the first run when no game
        /// was found, so the user has something to start from.
        /// </summary>
        /// <returns>true when the dummy game was created</returns>
        private static bool CreateDummyGame()
        {
            try
            {
                string dummyGameDirPath = Path.Combine(ManageSettings.GamesBaseFolderPath, DummyGameDirName);
                Directory.CreateDirectory(dummyGameDirPath);

                // the game itself: a folder is only accepted as a game when its data
                // folder holds the main exe of one of the presets
                string dataDirPath = Path.Combine(dummyGameDirPath, ManageSettings.DataDirName);
                Directory.CreateDirectory(dataDirPath);
                File.WriteAllText(Path.Combine(dataDirPath, DummyGameExeName + ".exe"), DummyGameExeContent);
                File.WriteAllText(Path.Combine(dataDirPath, ManageSettings.DefaultInitSettingExeName + ".exe"), DummyGameInitSettingExeContent);
                Directory.CreateDirectory(Path.Combine(dataDirPath, DummyGameDataDirName));

                // create UserData dir and setup.xml file in it
                string userDataDirPath = Path.Combine(dataDirPath, "UserData");
                Directory.CreateDirectory(userDataDirPath);
                File.WriteAllText(Path.Combine(userDataDirPath, "setup.xml"), ManageSettings.DefaultSetupXmlValue);

                // the folders and the Mod Organizer data of the game, exactly the way a
                // game the user added gets them
                GameFolderLayout.EnsureCreated(dummyGameDirPath, DummyGameExeName, Properties.Resources.defmoini);

                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Failed to create the dummy game. Error:\r\n" + ex);
                return false;
            }
        }

        /// <summary>
        /// Asks the user for the exe of a game and adds the game it belongs to to the list
        /// of the managed games.
        /// </summary>
        /// <param name="main">form which shows the list of games</param>
        /// <returns>true when a game was added</returns>
        internal static bool AddNewGame(MainForm main)
        {
            string gameExePath = AskForGameExePath();
            if (string.IsNullOrEmpty(gameExePath)) return false; // the user cancelled

            if (!TryAddGameFolder(gameExePath, out string gameDirPath))
            {
                MessageBox.Show(T._("This exe does not belong to a supported game")
                    + ":" + Environment.NewLine + gameExePath);
                return false;
            }

            // rebuild the list from the folders, so the game which was added can be
            // selected and managed right away
            SetListOfAddedGames(main);
            SelectGameInUi(main, gameDirPath);

            return true;
        }

        /// <summary>
        /// Asks the user for the exe of a game.
        /// </summary>
        /// <returns>the exe path, or an empty string when the user cancelled</returns>
        private static string AskForGameExePath()
        {
            using (var browseDialog = new OpenFileDialog())
            {
                browseDialog.Filter = "Game exe|*.exe";
                browseDialog.Multiselect = false;
                browseDialog.Title = T._("Select exe in the game dir you want to add..");

                return browseDialog.ShowDialog() == DialogResult.OK
                    ? browseDialog.FileName
                    : string.Empty;
            }
        }

        /// <summary>
        /// Adds the game the given exe belongs to to the list of the game folders the app
        /// manages.
        /// </summary>
        /// <param name="gameExePath">exe which was picked</param>
        /// <param name="gameDirPath">folder of the game, once it is registered</param>
        /// <returns>true when the game folder is registered</returns>
        /// <remarks>
        /// Nothing is shown to the user here: the caller decides how to report an exe which
        /// could not be added.
        /// </remarks>
        internal static bool TryAddGameFolder(string gameExePath, out string gameDirPath)
        {
            gameDirPath = null;

            if (string.IsNullOrWhiteSpace(gameExePath) || !File.Exists(gameExePath)) return false;

            string exeName = Path.GetFileNameWithoutExtension(gameExePath);

            // only the exe of one of the known games can be added, because a game is found
            // by its main exe while the list of games is built
            if (!GamePresets.TryFindByExeName(exeName, out _)) return false;

            string gameDirPathToAdd = GetGameDirPathOfExe(gameExePath);
            if (gameDirPathToAdd == null) return false;

            if (!GameFolderRegistry.AddGameFolder(gameDirPathToAdd, exeName)) return false;

            gameDirPath = gameDirPathToAdd;
            return true;
        }

        /// <summary>
        /// Folder of the game an exe belongs to: the folder of the exe, or the folder above
        /// it when the exe is in the data folder of the game.
        /// </summary>
        /// <param name="gameExePath">exe of a game</param>
        /// <returns>the game folder, or null when the path cannot be used</returns>
        private static string GetGameDirPathOfExe(string gameExePath)
        {
            string exeDirPath = Path.GetDirectoryName(gameExePath);
            if (string.IsNullOrEmpty(exeDirPath)) return null;

            bool isInDataDir = string.Equals(
                Path.GetFileName(exeDirPath),
                ManageSettings.DataDirName,
                StringComparison.OrdinalIgnoreCase);

            string gameDirPath = isInDataDir ? Path.GetDirectoryName(exeDirPath) : exeDirPath;

            return GameFolderRegistry.GetGameDirPath(gameDirPath);
        }

        /// <summary>
        /// Makes the game of the given folder the current one of the
        /// <paramref name="main"/> form.
        /// </summary>
        /// <param name="main">form which shows the list of games</param>
        /// <param name="gameDirPath">folder of the game to select</param>
        /// <remarks>
        /// The form switches the game when the selection of the combo box changes, so the
        /// current game is not set here - selecting the game is enough, and it is the same
        /// path a game the user picks in the list takes.
        /// </remarks>
        private static void SelectGameInUi(MainForm main, string gameDirPath)
        {
            var games = ManageSettings.Games.Games;
            if (games == null) return;

            int index = games.FindIndex(game => game.GameDirInfo != null
                && string.Equals(game.GameDirInfo.FullName, gameDirPath, StringComparison.OrdinalIgnoreCase));

            // nothing to do when the game is the selected one already
            if (index < 0 || index == main.CurrentGameComboBox.SelectedIndex) return;

            main.CurrentGameComboBox.SelectedIndex = index;
        }

        /// <summary>
        /// The game folders which are saved in the app ini.
        /// </summary>
        /// <param name="ini">app ini</param>
        /// <returns>the folders which exist, never null</returns>
        public static List<string> GetKnownGames(INIFile ini)
        {
            var knownGames = new List<string>();
            if (ini == null) return knownGames;

            var games = ini.GetKey(ManageSettings.SettingsIniSectionName, ManageSettings.KnownGamesIniKeyName);
            if (string.IsNullOrWhiteSpace(games)) return knownGames;

            foreach (var gamePath in games.Split('|'))
            {
                if (string.IsNullOrWhiteSpace(gamePath)) continue;
                if (!Directory.Exists(gamePath)) continue;
                if (ContainsGamePath(knownGames, gamePath)) continue;

                knownGames.Add(gamePath);
            }

            return knownGames;
        }

        /// <summary>
        /// Rebuilds the list of the games from the games base folder and shows it in the
        /// <paramref name="main"/> form.
        /// </summary>
        /// <param name="main">form which shows the list of games</param>
        /// <returns>false when no game was found: the app cannot be used without one</returns>
        public static bool SetListOfAddedGames(MainForm main)
        {
            try
            {
                var ini = ManageIni.GetINIFile(ManageSettings.AiHelperIniPath);
                ManageSettings.KnownGames = GetKnownGames(ini);

                GetListOfExistsGames();

                if (!HasAnyGame())
                {
                    // first run: create a game the user can start from and look again
                    CreateDummyGame();
                    GetListOfExistsGames();
                }

                if (!HasAnyGame())
                {
                    ShowGamesNotFoundMessage();
                    //Application.Exit();
                    return false;
                }

                ReportNewGames();

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

                if (ini != null)
                {
                    ini.SetKey(ManageSettings.SettingsIniSectionName, ManageSettings.KnownGamesIniKeyName, string.Join("|", ManageSettings.KnownGames));
                }

                SetCurrentGameTitle(main);
            }
            catch (Exception ex)
            {
                _log.Error("An error occured while the list of games was built. ini=" + ManageSettings.AiHelperIniPath + "\r\n error:\r\n" + ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// true when a game was found, either in a folder or in the app ini.
        /// </summary>
        private static bool HasAnyGame()
        {
            if (ManageSettings.KnownGames != null && ManageSettings.KnownGames.Count > 0) return true;

            return ManageSettings.Games != null
                && ManageSettings.Games.Games != null
                && ManageSettings.Games.Games.Count > 0;
        }

        /// <summary>
        /// Adds the games which are not known yet to the known games list and tells the
        /// user about them.
        /// </summary>
        private static void ReportNewGames()
        {
            var newGamesReport = new List<string>();

            foreach (var game in ManageSettings.Games.Games)
            {
                var gamePath = game.GamePath;
                if (ContainsGamePath(ManageSettings.KnownGames, gamePath)) continue;

                ManageSettings.KnownGames.Add(gamePath);
                newGamesReport.Add("\n" + game.GameDisplayingName + ": " + gamePath);
            }

            if (newGamesReport.Count == 0) return;

            newGamesReport = newGamesReport.OrderBy(p => p).ToList();

            MessageBox.Show(T._("New games:") +
                "\n"
                + Environment.NewLine + "----------------"
                + string.Join("", newGamesReport)
                + Environment.NewLine + "----------------"
            );
        }

        /// <summary>
        /// true when the list holds the path of a game.
        /// </summary>
        /// <remarks>
        /// A game folder is a folder of this machine, so the case of the path is not part
        /// of its identity.
        /// </remarks>
        private static bool ContainsGamePath(IEnumerable<string> gamePaths, string gamePath)
        {
            if (gamePaths == null || string.IsNullOrWhiteSpace(gamePath)) return false;

            foreach (var knownGamePath in gamePaths)
            {
                if (string.Equals(knownGamePath, gamePath, StringComparison.OrdinalIgnoreCase)) return true;
            }

            return false;
        }

        /// <summary>
        /// Tells the user that no game was found and what a game folder has to contain.
        /// </summary>
        private static void ShowGamesNotFoundMessage()
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
        }

        /// <summary>
        /// update games list controls of the <paramref name="main"/> form
        /// </summary>
        /// <param name="main"></param>
        /// <param name="games"></param>
        private static void UpdateGamesListUi(MainForm main, List<GameBase> games)
        {
            // the members are set before the data source: a source which is set first is
            // bound once with the members of the previous list
            main.CurrentGameComboBox.DisplayMember = "GameDirName";
            main.CurrentGameComboBox.ValueMember = "GameDirName";
            main.CurrentGameComboBox.DataSource = games;

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

            // the box only shows the name of the current game, so it is not editable
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
