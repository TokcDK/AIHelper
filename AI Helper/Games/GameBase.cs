using AIHelper.Manage;
using NLog;
using System.IO;

namespace AIHelper.Games
{
    public abstract class GameBase
    {
        protected static Logger _log = LogManager.GetCurrentClassLogger();
        public GameBase()
        {
            //InitActions();
        }

        public virtual void InitActions()
        {
        }

        //public string gamefolderPath { get; set; } = string.Empty;
        /// <summary>
        /// true if it is root game ie placed in same folder with game's data, mods folder
        /// </summary>
        public virtual bool IsRootGame { get; set; }
        /// <summary>
        /// true if game have sideloader zipmods
        /// </summary>
        public virtual bool IsHaveSideloaderMods { get => false; }

        /// <summary>
        /// name of the game's folder
        /// </summary>
        protected string GamefolderName { get; set; } = string.Empty;

        /// <summary>
        /// manifest/game name
        /// </summary>
        public abstract string ManifestGame { get; }

        /// <summary>
        /// title of current game. GameName value for MO ini GameName parameter
        /// </summary>
        public string GameName { get; internal set; }

        /// <summary>
        /// The game's registry path
        /// </summary>
        public virtual string RegistryPath { get => ""; }
        /// <summary>
        /// The game's Install dir key name
        /// </summary>
        public virtual string RegistryInstallDirKey { get => ""; }

        /// <summary>
        /// search and return game folder name
        /// </summary>
        /// <returns></returns>
        public virtual string GameDirName { get => GameDirInfo.Name; }
        //return SearchGameFolder();
        //protected string GetTheGameFolderName(string defaultGameFolderName)
        //{
        //    if (GamefolderName.Length > 0 || (GamefolderName = SearchGameFolder()).Length > 0)
        //    {
        //        return GamefolderName;
        //    }
        //    else
        //    {
        //        return defaultGameFolderName;
        //    }
        //}

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
        /// main game's vr exe name of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string GameExeNameVr { get => GameExeName + "VR"; }

        /// <summary>
        /// main game's x32 exe name of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string GameExeNameX32 { get => string.Empty; }

        /// <summary>
        /// game's inisettings launcher exe name of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string IniSettingsExeName { get => "InitSetting"; }

        /// <summary>
        /// game's studio exe name of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string GameStudioExeName { get => string.Empty; }

        /// <summary>
        /// game's studio x32 exe name of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string GameStudioExeNameX32 { get => string.Empty; }

        public DirectoryInfo GameDirInfo;

        /// game's path of selected game
        public virtual string GamePath
        {
            get => IsRootGame ? Properties.Settings.Default.ApplicationStartupPath : GameDirInfo.FullName;
        }

        /// <summary>
        /// game's Mods folder path of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string ModsPath { get => Path.Combine(GamePath, "Mods"); }

        /// <summary>
        /// game's Data folder path of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string DataPath { get => Path.Combine(GamePath, "Data"); }

        /// <summary>
        /// game's character presets folder subpath
        /// </summary>
        /// <returns></returns>
        public virtual string CharacterPresetsFolderSubPath { get => ""; }

        /// <summary>
        /// game's 2MO folder path of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string InstallFolderPath { get => Path.Combine(GamePath, "2MO"); }

        /// <summary>
        /// game's TESV.exe dummy path of selected game. for older versions of MO
        /// </summary>
        /// <returns></returns>
        public virtual string DummyFilePath { get => Path.Combine(GamePath, "TESV.exe"); }

        /// <summary>
        /// additional exe paths for selected game
        /// </summary>
        /// <returns></returns>
        public virtual string[] AdditionalExecutables => null;

        public virtual string[] GameStandartFolderNames
        {
            get
            {
                if (GameExeNameX32.Length > 0 && GameStudioExeNameX32.Length > 0)
                {
                    return new string[] { "abdata", "UserData", GameExeName + "_Data", GameExeNameX32 + "_Data", GameStudioExeName + "_Data", GameStudioExeNameX32 + "_Data", "BepInEx" };
                }
                else
                {
                    return new string[] { "abdata", "UserData", GameExeName + "_Data", GameStudioExeName + "_Data", "BepInEx" };
                }
            }
        }

        public virtual string[,] DirLinkPaths => new string[,]
            {
                    //{
                    //    Path.Combine(ManageSettings.GetCurrentGameModsPath(), "BepInEx", "BepInEx", "core", "BepInEx.Preloader.dll")
                    //    ,
                    //    Path.Combine(ManageSettings.GetCurrentGameDataPath(), "BepInEx", "core", "BepInEx.Preloader.dll")
                    //}
                    //,
                    //{
                    //    Path.Combine(ManageSettings.GetCurrentGameModsPath(), "BepInEx", "doorstop_config.ini")
                    //    ,
                    //    Path.Combine(ManageSettings.GetCurrentGameDataPath(), "doorstop_config.ini")
                    //}
                    //,
                    //{
                    //    Path.Combine(ManageSettings.GetCurrentGameModsPath(), "BepInEx", "winhttp.dll")
                    //    ,
                    //    Path.Combine(ManageSettings.GetCurrentGameDataPath(), "winhttp.dll")
                    //}
                    //,
                    {
                        Path.Combine(ManageSettings.CurrentGameModsDirPath, "MyUserData", "UserData", "MaterialEditor")
                        ,
                        Path.Combine(ManageSettings.CurrentGameDataDirPath, "UserData", "MaterialEditor")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.CurrentGameModsDirPath, "MyUserData", "UserData", "Overlays")
                        ,
                        Path.Combine(ManageSettings.CurrentGameDataDirPath, "UserData", "Overlays")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.CurrentGameModsDirPath, "MyUserData", "UserData", "cap")
                        ,
                        Path.Combine(ManageSettings.CurrentGameDataDirPath, "UserData", "cap")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.CurrentGameModsDirPath, "MyUserData", "UserData", "studioneo", "BetterSceneLoader")
                        ,
                        Path.Combine(ManageSettings.CurrentGameDataDirPath, "UserData", "studioneo", "BetterSceneLoader")
                    }
            };

        public virtual string[,] ObjectsForMove => new string[,]
            {
                    {
                        Path.Combine(ManageSettings.CurrentGameModsDirPath, "BepInEx", "BepInEx", "core", "BepInEx.Preloader.dll")
                        ,
                        Path.Combine(ManageSettings.CurrentGameDataDirPath, "BepInEx", "core", "BepInEx.Preloader.dll")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.CurrentGameModsDirPath, "BepInEx", "doorstop_config.ini")
                        ,
                        Path.Combine(ManageSettings.CurrentGameDataDirPath, "doorstop_config.ini")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.CurrentGameModsDirPath, "BepInEx", "winhttp.dll")
                        ,
                        Path.Combine(ManageSettings.CurrentGameDataDirPath, "winhttp.dll")
                    }
            };

        internal string GetGameName()
        {
            return !string.IsNullOrWhiteSpace(GameName) ? GameName : GameName = ManageModOrganizer.GetMoBasicGamePluginGameName();
        }

        //protected string SearchGameFolder()
        //{
        //    try
        //    {
        //        if (Directory.Exists(ManageSettings.GetCurrentGameParentDirPath()))
        //        {
        //            foreach (var folder in Directory.EnumerateDirectories(ManageSettings.GetCurrentGameParentDirPath()))
        //            {
        //                if (File.Exists(Path.Combine(folder, "Data", GetGameExeName() + ".exe")))
        //                {
        //                    return Path.GetFileName(folder);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _log.Debug("An error occured while SearchGameFolder. error:\r\n" + ex);
        //    }
        //    return string.Empty;
        //}

        /// <summary>
        /// return selected game detection MO BaseGame plugin's py file or all files by default
        /// </summary>
        /// <returns></returns>
        public abstract BaseGamePyFileInfo BaseGamePyFile { get; }
    }
}
