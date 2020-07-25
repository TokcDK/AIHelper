using AIHelper.Manage;
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

        }

        protected string gamefolderName { get; set; } = string.Empty;

        //internal string gamefolderPath { get; set; } = string.Empty;
        internal virtual bool isRootGame { get; set; } = false;

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

        public abstract string GetGameEXEName();

        public virtual string GetGameEXENameVR()
        {
            return GetGameEXEName() + "VR";
        }

        public virtual string GetGameEXENameX32()
        {
            return string.Empty;
        }

        public virtual string GetINISettingsEXEName()
        {
            return "InitSetting";
        }

        public virtual string GetGameStudioEXEName()
        {
            return string.Empty;
        }

        public virtual string GetGameStudioEXENameX32()
        {
            return string.Empty;
        }

        public virtual string GetGamePath()
        {
            return isRootGame ?
                Properties.Settings.Default.ApplicationStartupPath
                :
                Path.Combine(ManageSettings.GetGamesFolderPath(), GetGameFolderName())
                ;
        }

        public virtual string GetModsPath()
        {
            return Path.Combine(GetGamePath(), "Mods");
        }

        public virtual string GetDataPath()
        {
            return Path.Combine(GetGamePath(), "Data");
        }

        public virtual string Get2MOFolderPath()
        {
            return Path.Combine(GetGamePath(), "2MO");
        }

        public virtual string GetDummyFile()
        {
            return Path.Combine(GetGamePath(), "TESV.exe");
        }

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

        public virtual string[,] GetObjectsForSymLinksPaths()
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
            //var game = ManageSettings.GetListOfExistsGames()[ManageSettings.GetCurrentGameIndex()];
            string GameMOPath = Path.Combine(GetGamePath(), "MO");
            string GameMPAltDirNamePath = Path.Combine(GetGamePath(), GameMPAltDirName);
            if (Directory.Exists(GameMPAltDirNamePath) && !Directory.Exists(GameMOPath))
            {
                //Directory.CreateDirectory(MO);
                CopyFolder.Copy(Path.Combine(GameMPAltDirNamePath, "Profiles"), Path.Combine(GameMOPath, "Profiles"));
                CopyFolder.Copy(Path.Combine(GameMPAltDirNamePath, "Overwrite"), Path.Combine(GameMOPath, "Overwrite"));
                File.Copy(Path.Combine(GameMPAltDirNamePath, "categories.dat"), Path.Combine(GameMOPath, "categories.dat"));
                File.Copy(Path.Combine(GameMPAltDirNamePath, "ModOrganizer.ini"), Path.Combine(GameMOPath, "ModOrganizer.ini"));
            }
        }
    }
}
