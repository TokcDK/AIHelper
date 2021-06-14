using AIHelper.Manage;
using System.Collections.Generic;
using System.IO;

namespace AIHelper.Games
{
    public abstract class Game
    {
        public Game()
        {
            InitActions();
        }

        public virtual void InitActions()
        {
            GetBaseGamePyFile();
        }

        //internal string gamefolderPath { get; set; } = string.Empty;
        /// <summary>
        /// true if it is root game ie placed in same folder with game's data, mods folder
        /// </summary>
        internal virtual bool isRootGame { get; set; } = false;
        /// <summary>
        /// true if game have sideloader zipmods
        /// </summary>
        internal virtual bool isHaveSideloaderMods { get; set; } = false;

        protected string gamefolderName { get; set; } = string.Empty;
        /// <summary>
        /// search and return game folder name
        /// </summary>
        /// <returns></returns>
        public virtual string GetGameFolderName()
        {
            return SearchGameFolder();
        }

        protected string GetTheGameFolderName(string DefaultGameFolderName)
        {
            if (gamefolderName.Length > 0 || (gamefolderName = SearchGameFolder()).Length > 0)
            {
                return gamefolderName;
            }
            else
            {
                return DefaultGameFolderName;
            }
        }

        public virtual string GetGameDisplayingName()
        {
            return GetGameFolderName();
        }

        /// <summary>
        /// main game's exe name of selected game
        /// </summary>
        /// <returns></returns>
        public abstract string GetGameEXEName();

        /// <summary>
        /// prefix of selected game (kk,hs,hs2...)
        /// </summary>
        /// <returns></returns>
        public abstract string GetGamePrefix();

        /// <summary>
        /// main game's vr exe name of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string GetGameEXENameVR()
        {
            return GetGameEXEName() + "VR";
        }

        /// <summary>
        /// main game's x32 exe name of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string GetGameEXENameX32()
        {
            return string.Empty;
        }

        /// <summary>
        /// game's inisettings launcher exe name of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string GetINISettingsEXEName()
        {
            return "InitSetting";
        }

        /// <summary>
        /// game's studio exe name of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string GetGameStudioEXEName()
        {
            return string.Empty;
        }

        /// <summary>
        /// game's studio x32 exe name of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string GetGameStudioEXENameX32()
        {
            return string.Empty;
        }

        /// game's path of selected game
        public virtual string GetGamePath()
        {
            return isRootGame ?
                Properties.Settings.Default.ApplicationStartupPath
                :
                Path.Combine(ManageSettings.GetGamesFolderPath(), GetGameFolderName())
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
        /// game's 2MO folder path of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string Get2MOFolderPath()
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
            if (GetGameEXENameX32().Length > 0 && GetGameStudioEXENameX32().Length > 0)
            {
                return new string[] { "abdata", "UserData", GetGameEXEName() + "_Data", GetGameEXENameX32() + "_Data", GetGameStudioEXEName() + "_Data", GetGameStudioEXENameX32() + "_Data", "BepInEx" };
            }
            else
            {
                return new string[] { "abdata", "UserData", GetGameEXEName() + "_Data", GetGameStudioEXEName() + "_Data", "BepInEx" };
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
                        Path.Combine(ManageSettings.GetCurrentGameModsPath(), "MyUserData", "UserData", "MaterialEditor")
                        ,
                        Path.Combine(ManageSettings.GetCurrentGameDataPath(), "UserData", "MaterialEditor")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.GetCurrentGameModsPath(), "MyUserData", "UserData", "Overlays")
                        ,
                        Path.Combine(ManageSettings.GetCurrentGameDataPath(), "UserData", "Overlays")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.GetCurrentGameModsPath(), "MyUserData", "UserData", "cap")
                        ,
                        Path.Combine(ManageSettings.GetCurrentGameDataPath(), "UserData", "cap")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.GetCurrentGameModsPath(), "MyUserData", "UserData", "studioneo", "BetterSceneLoader")
                        ,
                        Path.Combine(ManageSettings.GetCurrentGameDataPath(), "UserData", "studioneo", "BetterSceneLoader")
                    }
            };
        }

        public virtual string[,] GetObjectsForMove()
        {
            return new string[,]
            {
                    {
                        Path.Combine(ManageSettings.GetCurrentGameModsPath(), "BepInEx", "BepInEx", "core", "BepInEx.Preloader.dll")
                        ,
                        Path.Combine(ManageSettings.GetCurrentGameDataPath(), "BepInEx", "core", "BepInEx.Preloader.dll")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.GetCurrentGameModsPath(), "BepInEx", "doorstop_config.ini")
                        ,
                        Path.Combine(ManageSettings.GetCurrentGameDataPath(), "doorstop_config.ini")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.GetCurrentGameModsPath(), "BepInEx", "winhttp.dll")
                        ,
                        Path.Combine(ManageSettings.GetCurrentGameDataPath(), "winhttp.dll")
                    }
            };
        }

        protected string SearchGameFolder()
        {
            try
            {
                if (Directory.Exists(ManageSettings.GetGamesFolderPath()))
                {
                    foreach (var folder in Directory.EnumerateDirectories(ManageSettings.GetGamesFolderPath()))
                    {
                        if (File.Exists(Path.Combine(folder, "Data", GetGameEXEName() + ".exe")))
                        {
                            return Path.GetFileName(folder);
                        }
                    }
                }
            }
            catch
            {
            }
            return string.Empty;
        }

        protected void CopyMOfiles(string GameMPAltDirName)
        {
            //var game = Data.CurrentGame.GetCurrentGameIndex()];
            string GameMOPath = Path.Combine(GetGamePath(), "MO");
            string GameMPAltDirNamePath = Path.Combine(GetGamePath(), GameMPAltDirName);
            if (Directory.Exists(GameMPAltDirNamePath) && !Directory.Exists(GameMOPath))
            {
                //Directory.CreateDirectory(MO);
                CopyFolder.CopyAll(Path.Combine(GameMPAltDirNamePath, "Profiles"), Path.Combine(GameMOPath, "Profiles"));
                CopyFolder.CopyAll(Path.Combine(GameMPAltDirNamePath, "Overwrite"), Path.Combine(GameMOPath, "Overwrite"));
                File.Copy(Path.Combine(GameMPAltDirNamePath, "categories.dat"), Path.Combine(GameMOPath, "categories.dat"));
                File.Copy(Path.Combine(GameMPAltDirNamePath, "ModOrganizer.ini"), Path.Combine(GameMOPath, "ModOrganizer.ini"));
            }
        }

        /// <summary>
        /// return selected game detection MO BaseGame plugin's py file or all files by default
        /// </summary>
        /// <returns></returns>
        internal virtual Dictionary<string, byte[]> GetBaseGamePyFile()
        {
            return new Dictionary<string, byte[]>
                {
                    { nameof(Properties.Resources.game_aigirl), Properties.Resources.game_aigirl },
                    { nameof(Properties.Resources.game_aigirltrial), Properties.Resources.game_aigirltrial },
                    { nameof(Properties.Resources.game_honeyselect), Properties.Resources.game_honeyselect },
                    { nameof(Properties.Resources.game_honeyselect2), Properties.Resources.game_honeyselect2},
                    { nameof(Properties.Resources.game_koikatu), Properties.Resources.game_koikatu }
                };
        }
    }
}
