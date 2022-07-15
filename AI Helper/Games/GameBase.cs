using AIHelper.Manage;
using NLog;
using System;
using System.Collections.Generic;
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
        public virtual string GetGameDirName()
        {
            return GameDirInfo.Name;
            //return SearchGameFolder();
        }

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

        public virtual string GetGameDisplayingName()
        {
            return GetGameDirName();
        }

        /// <summary>
        /// main game's exe name of selected game
        /// </summary>
        /// <returns></returns>
        public abstract string GetGameExeName();

        /// <summary>
        /// prefix of selected game (kk,hs,hs2,ai,kks)
        /// </summary>
        /// <returns></returns>
        public abstract string GetGameAbbreviation();

        /// <summary>
        /// main game's vr exe name of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string GetGameExeNameVr()
        {
            return GetGameExeName() + "VR";
        }

        /// <summary>
        /// main game's x32 exe name of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string GetGameExeNameX32()
        {
            return string.Empty;
        }

        /// <summary>
        /// game's inisettings launcher exe name of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string GetIniSettingsExeName()
        {
            return "InitSetting";
        }

        /// <summary>
        /// game's studio exe name of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string GetGameStudioExeName()
        {
            return string.Empty;
        }

        /// <summary>
        /// game's studio x32 exe name of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string GetGameStudioExeNameX32()
        {
            return string.Empty;
        }

        public DirectoryInfo GameDirInfo;

        /// game's path of selected game
        public virtual string GetGamePath()
        {
            return IsRootGame ?
                Properties.Settings.Default.ApplicationStartupPath
                :
                GameDirInfo.FullName
                ;
        }

        /// <summary>
        /// game's Mods folder path of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string GetModsPath()
        {
            return Path.Combine(GetGamePath(), "Mods");
        }

        /// <summary>
        /// game's Data folder path of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string GetDataPath()
        {
            return Path.Combine(GetGamePath(), "Data");
        }

        /// <summary>
        /// game's character presets folder subpath
        /// </summary>
        /// <returns></returns>
        public virtual string GetCharacterPresetsFolderSubPath()
        {
            return "";
        }

        /// <summary>
        /// game's 2MO folder path of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string Get2MoFolderPath()
        {
            return Path.Combine(GetGamePath(), "2MO");
        }

        /// <summary>
        /// game's TESV.exe dummy path of selected game. for older versions of MO
        /// </summary>
        /// <returns></returns>
        public virtual string GetDummyFile()
        {
            return Path.Combine(GetGamePath(), "TESV.exe");
        }

        /// <summary>
        /// additional exe paths for selected game
        /// </summary>
        /// <returns></returns>
        public virtual string[] GetAdditionalExecutables()
        {
            return null;
        }

        public virtual string[] GetGameStandartFolderNames()
        {
            if (GetGameExeNameX32().Length > 0 && GetGameStudioExeNameX32().Length > 0)
            {
                return new string[] { "abdata", "UserData", GetGameExeName() + "_Data", GetGameExeNameX32() + "_Data", GetGameStudioExeName() + "_Data", GetGameStudioExeNameX32() + "_Data", "BepInEx" };
            }
            else
            {
                return new string[] { "abdata", "UserData", GetGameExeName() + "_Data", GetGameStudioExeName() + "_Data", "BepInEx" };
            }
        }

        public virtual string[,] GetDirLinkPaths()
        {
            return new string[,]
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
        }

        public virtual string[,] GetObjectsForMove()
        {
            return new string[,]
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
        }

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


        protected void CopyModOrganizerUserFiles(string moDirAltName)
        {
            //var game = Data.CurrentGame.GetCurrentGameIndex()];
            string gameMoDirPath = Path.Combine(GetGamePath(), "MO");
            string gameMoDirPathAlt = Path.Combine(GetGamePath(), moDirAltName);

            // dirs and files required for work
            var subPaths = new Dictionary<string, ObjectType>
            {
                { "Profiles", ObjectType.Directory },
                { "Overwrite", ObjectType.Directory },
                { "categories.dat", ObjectType.File },
                { "ModOrganizer.ini", ObjectType.File }
            };

            foreach (var subPath in subPaths)
            {
                try
                {
                    var altpath = Path.GetFullPath(Path.Combine(gameMoDirPathAlt, subPath.Key));
                    var workpath = Path.GetFullPath(Path.Combine(gameMoDirPath, subPath.Key));
                    if (subPath.Value == ObjectType.Directory && Directory.Exists(altpath) && (!Directory.Exists(workpath) || !ManageFilesFoldersExtensions.IsAnyFileExistsInTheDir(workpath, allDirectories: true)))
                    {
                        ManageFilesFoldersExtensions.CopyAll(altpath, workpath);
                    }
                    else if (subPath.Value == ObjectType.File && File.Exists(altpath) && !File.Exists(workpath))
                    {
                        File.Copy(altpath, workpath);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error("An error occured while MO files coping. error:\r\n" + ex);
                }
            }
        }

        /// <summary>
        /// return selected game detection MO BaseGame plugin's py file or all files by default
        /// </summary>
        /// <returns></returns>
        public abstract BaseGamePyFileInfo GetBaseGamePyFile();
    }

    public class BaseGamePyFileInfo
    {
        public string Name;
        public byte[] Value;

        public BaseGamePyFileInfo(string filename, byte[] value)
        {
            Name = filename;
            Value = value;
        }
    }
}
