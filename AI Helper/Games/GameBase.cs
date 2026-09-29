using AIHelper.Manage;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AIHelper.Games
{
    /// <summary>
    /// Base preset of one supported game. Every non abstract subclass found by
    /// reflection is offered to the user, so a new game only has to add a class
    /// and fill in the members it actually differs in.
    /// </summary>
    /// <remarks>
    /// <see cref="Manage.ManageOther.GetListOfExistsGames"/> builds the game list
    /// from <c>Inherited.GetListOfInheritedTypes(typeof(GameBase))</c>, which
    /// instantiates every concrete subclass through its parameterless
    /// constructor. Do not declare a constructor with parameters here, and keep
    /// intermediate classes abstract.
    /// </remarks>
    public abstract class GameBase
    {
        protected static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Folder the mod loader reads its plugins from.
        /// </summary>
        protected const string BepInExDirName = "BepInEx";

        /// <summary>
        /// Standard data folder of the older Illusion games.
        /// </summary>
        protected const string AbdataDirName = "abdata";

        /// <summary>
        /// Folder the game stores its user data in.
        /// </summary>
        protected const string UserDataDirName = "UserData";

        /// <summary>
        /// Folder Mod Organizer uses as the source of the linked user data.
        /// </summary>
        protected const string MyUserDataDirName = "MyUserData";

        /// <summary>
        /// Suffix Unity adds to the name of the data folder of an executable.
        /// </summary>
        protected const string DataDirSuffix = "_Data";

        public abstract string BasicGamePluginName { get; }

        /// <summary>
        /// manifest/game name
        /// </summary>
        public abstract string ZipmodManifestGameName { get; }

        public virtual void InitActions() { }

        /// <summary>
        /// true if game have sideloader zipmods
        /// </summary>
        public virtual bool IsHaveSideloaderMods { get; }

        /// <summary>
        /// name of the game's folder
        /// </summary>
        protected string GamefolderName { get; set; }

        /// <summary>
        /// title of current game. GameName value for MO ini GameName parameter
        /// </summary>
        public string GameName { get; internal set; }

        /// <summary>
        /// The game's registry path
        /// </summary>
        public virtual string RegistryPath { get; }
        /// <summary>
        /// The game's Install dir key name
        /// </summary>
        public virtual string RegistryInstallDirKey { get; }

        /// <summary>
        /// The game's install folder. Set by <see cref="Manage.ManageOther"/>
        /// while the game list is built, so it is never null for a game the user
        /// can select.
        /// </summary>
        public DirectoryInfo GameDirInfo { get; set; }

        /// <summary>
        /// search and return game folder name
        /// </summary>
        /// <returns>the folder name, or an empty string while the game folder is not known yet</returns>
        public virtual string GameDirName { get => GameDirInfo?.Name ?? string.Empty; }

        public virtual string GameDisplayingName { get => GameDirName; }

        /// <summary>
        /// main game's exe name of selected game
        /// </summary>
        /// <returns></returns>
        public abstract string GameExeName { get; }

        /// <summary>
        /// prefix of selected game (kk,hs,hs2,ai,kks)
        /// </summary>
        /// <returns></returns>
        public abstract string GameAbbreviation { get; }

        /// <summary>
        /// tags the game writes behind the preview image of its cards, used to tell
        /// which game a card belongs to
        /// </summary>
        /// <remarks>
        /// A tag can be shared by several games (AI Girl and HoneySelect 2 both write
        /// 【AIS_Chara】), so a tag names a set of games rather than a single one.
        /// </remarks>
        /// <returns>the tags, never null</returns>
        public virtual IReadOnlyList<string> CardMarkers => Array.Empty<string>();

        /// <summary>
        /// main game's vr exe name of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string GameExeNameVr { get => GameExeName + ManageSettings.GameVRexeSuffixName; }

        /// <summary>
        /// main game's x32 exe name of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string GameExeNameX32 { get => string.Empty; }

        /// <summary>
        /// game's inisettings launcher exe name of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string IniSettingsExeName { get => ManageSettings.DefaultInitSettingExeName; }

        /// <summary>
        /// game's studio exe name of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string GameStudioExeName { get; }

        /// <summary>
        /// game's studio x32 exe name of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string GameStudioExeNameX32 { get; }

        /// <summary>
        /// game's path of selected game
        /// </summary>
        public virtual string GamePath { get => GameDirInfo.FullName; set { GameDirInfo = new DirectoryInfo(value); } }

        /// <summary>
        /// game's Mods folder path of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string ModsPath { get => Path.Combine(GamePath, ManageSettings.CurrentGameModsDirName); }

        /// <summary>
        /// game's Data folder path of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string DataPath { get => Path.Combine(GamePath, ManageSettings.CurrentGameDataDirName); }

        /// <summary>
        /// game's character presets folder subpath
        /// </summary>
        /// <returns></returns>
        public virtual string CharacterPresetsFolderSubPath { get; }

        /// <summary>
        /// game's 2MO folder path of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string InstallFolderPath { get => Path.Combine(GamePath, ManageSettings.CurrentGameInstallDirName); }

        /// <summary>
        /// game's TESV.exe dummy path of selected game. for older versions of MO
        /// </summary>
        /// <returns></returns>
        public virtual string DummyFilePath { get => Path.Combine(GamePath, ManageSettings.DummyFileName); }

        /// <summary>
        /// additional exe paths for selected game
        /// </summary>
        /// <returns>the extra executables, never null</returns>
        public virtual IReadOnlyList<string> AdditionalExecutables => Array.Empty<string>();

        /// <summary>
        /// Every exe name the game can be started with: the main exe, its x32 and vr
        /// builds, the studio, the settings launcher and the additional executables.
        /// </summary>
        /// <remarks>
        /// A game is recognized in a folder by its main exe only, but any of these names
        /// is accepted when the user picks the exe to add, because the folder of the game
        /// has to be found whichever build the user starts. The names are declared here
        /// rather than in the code which looks for a game, so a preset which adds an exe
        /// does not have to be taught to that code as well.
        /// </remarks>
        /// <returns>the names without the extension, without empty ones, never null</returns>
        public virtual IReadOnlyList<string> KnownExeNames
        {
            get
            {
                var exeNames = new List<string>
                {
                    GameExeName,
                    GameExeNameX32,
                    GameExeNameVr,
                    GameStudioExeName,
                    GameStudioExeNameX32,
                    IniSettingsExeName
                };

                foreach (var additionalExePath in AdditionalExecutables)
                {
                    exeNames.Add(Path.GetFileNameWithoutExtension(additionalExePath));
                }

                return exeNames.Where(exeName => !string.IsNullOrWhiteSpace(exeName)).ToArray();
            }
        }

        /// <summary>
        /// true when <paramref name="exeName"/> is one of <see cref="KnownExeNames"/>.
        /// </summary>
        /// <param name="exeName">exe file name, with or without the extension</param>
        /// <returns>true when the exe belongs to the game</returns>
        /// <remarks>
        /// The comparison ignores the case: an exe is a file of this machine, and the
        /// user may pick it in any case the file system reports it in.
        /// </remarks>
        public bool IsKnownExeName(string exeName)
        {
            if (string.IsNullOrWhiteSpace(exeName))
            {
                return false;
            }

            string name = Path.GetFileNameWithoutExtension(exeName);

            foreach (var knownExeName in KnownExeNames)
            {
                if (string.Equals(knownExeName, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Folder names the game itself owns. Content inside them must not be
        /// moved one level up while an archive is unpacked.
        /// </summary>
        public virtual IReadOnlyList<string> GameStandardFolderNames
        {
            get
            {
                var folderNames = new List<string>
                {
                    AbdataDirName,
                    UserDataDirName,
                    GameExeName + DataDirSuffix,
                    GameStudioExeName + DataDirSuffix,
                    BepInExDirName
                };

                // a game may have an x32 build of one executable without the
                // other one, so each name is checked on its own
                if (!string.IsNullOrEmpty(GameExeNameX32)) folderNames.Add(GameExeNameX32 + DataDirSuffix);
                if (!string.IsNullOrEmpty(GameStudioExeNameX32)) folderNames.Add(GameStudioExeNameX32 + DataDirSuffix);

                return folderNames;
            }
        }

        /// <summary>
        /// Directory link pairs of the game. <see cref="PathPair.Source"/> is the
        /// real folder, <see cref="PathPair.Target"/> is where the link is created.
        /// </summary>
        public virtual IReadOnlyList<PathPair> DirLinkPaths => new[]
        {
            DirLink(
                Path.Combine(ManageSettings.CurrentGameModsDirPath, MyUserDataDirName, UserDataDirName, "MaterialEditor"),
                Path.Combine(ManageSettings.CurrentGameDataDirPath, UserDataDirName, "MaterialEditor")),
            DirLink(
                Path.Combine(ManageSettings.CurrentGameModsDirPath, MyUserDataDirName, UserDataDirName, "Overlays"),
                Path.Combine(ManageSettings.CurrentGameDataDirPath, UserDataDirName, "Overlays")),
            DirLink(
                Path.Combine(ManageSettings.CurrentGameModsDirPath, MyUserDataDirName, UserDataDirName, "cap"),
                Path.Combine(ManageSettings.CurrentGameDataDirPath, UserDataDirName, "cap")),
            DirLink(
                Path.Combine(ManageSettings.CurrentGameModsDirPath, MyUserDataDirName, UserDataDirName, "studioneo", "BetterSceneLoader"),
                Path.Combine(ManageSettings.CurrentGameDataDirPath, UserDataDirName, "studioneo", "BetterSceneLoader")),
        };

        /// <summary>
        /// File pairs that have to be moved out of the mod folder into the game
        /// folder so the mod loader can start.
        /// </summary>
        public virtual IReadOnlyList<PathPair> ObjectsForMove => new[]
        {
            FileCopy(
                Path.Combine(ManageSettings.CurrentGameModsDirPath, BepInExDirName, BepInExDirName, "core", "BepInEx.Preloader.Core.dll"),
                Path.Combine(ManageSettings.CurrentGameDataDirPath, BepInExDirName, "core", "BepInEx.Preloader.Core.dll")),
            FileCopy(
                Path.Combine(ManageSettings.CurrentGameModsDirPath, BepInExDirName, BepInExDirName, "core", "BepInEx.Preloader.dll"),
                Path.Combine(ManageSettings.CurrentGameDataDirPath, BepInExDirName, "core", "BepInEx.Preloader.dll")),
            FileCopy(
                Path.Combine(ManageSettings.CurrentGameModsDirPath, BepInExDirName, "doorstop_config.ini"),
                Path.Combine(ManageSettings.CurrentGameDataDirPath, "doorstop_config.ini")),
            FileCopy(
                Path.Combine(ManageSettings.CurrentGameModsDirPath, BepInExDirName, "winhttp.dll"),
                Path.Combine(ManageSettings.CurrentGameDataDirPath, "winhttp.dll")),
        };

        /// <summary>
        /// Settings tab of the game, or null when the game has no editable settings.
        /// </summary>
        /// <remarks>
        /// A new control is built on every read, so the caller owns it and has to
        /// dispose the instance it replaced.
        /// </remarks>
        public virtual UserControl GameSettingsControl { get => null; }

        public virtual string GetGameConfigFilePath(string parentPath) => string.Empty;

        /// <summary>
        /// A directory link pair: <paramref name="objectPath"/> is the real folder,
        /// <paramref name="linkPath"/> is where the link is created.
        /// </summary>
        protected static PathPair DirLink(string objectPath, string linkPath) => new PathPair(objectPath, linkPath);

        /// <summary>
        /// A file pair: <paramref name="sourcePath"/> is copied to <paramref name="targetPath"/>.
        /// </summary>
        protected static PathPair FileCopy(string sourcePath, string targetPath) => new PathPair(sourcePath, targetPath);

        /// <summary>
        /// GameName as it is written into the Mod Organizer ini, looked up in the
        /// basic game plugin when it was not set yet.
        /// </summary>
        internal string GetGameName()
        {
            if (string.IsNullOrWhiteSpace(GameName))
            {
                GameName = ManageModOrganizer.GetMoBasicGamePluginGameName();
            }

            return GameName;
        }
    }
}
