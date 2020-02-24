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

        public virtual string GetGameFolderName()
        {
            return SearchGameFolder();
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
            return Path.Combine(ManageSettings.GetGamesFolderPath(), GetGameFolderName());
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
                    {
                        Path.Combine(ManageSettings.GetModsPath(), "BepInEx", "BepInEx", "core", "BepInEx.Preloader.dll")
                        ,
                        Path.Combine(ManageSettings.GetDataPath(), "BepInEx", "core", "BepInEx.Preloader.dll")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.GetModsPath(), "BepInEx", "doorstop_config.ini")
                        ,
                        Path.Combine(ManageSettings.GetDataPath(), "doorstop_config.ini")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.GetModsPath(), "BepInEx", "winhttp.dll")
                        ,
                        Path.Combine(ManageSettings.GetDataPath(), "winhttp.dll")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.GetModsPath(), "MyUserData", "UserData", "MaterialEditor")
                        ,
                        Path.Combine(ManageSettings.GetDataPath(), "UserData", "MaterialEditor")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.GetModsPath(), "MyUserData", "UserData", "Overlays")
                        ,
                        Path.Combine(ManageSettings.GetDataPath(), "UserData", "Overlays")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.GetModsPath(), "MyUserData", "UserData", "cap")
                        ,
                        Path.Combine(ManageSettings.GetDataPath(), "UserData", "cap")
                    }
            };
        }

        protected string SearchGameFolder()
        {
            foreach (var folder in Directory.EnumerateDirectories(ManageSettings.GetGamesFolderPath()))
            {
                if (File.Exists(Path.Combine(folder, "Data", GetGameEXEName() + ".exe")))
                {
                    return Path.GetFileName(folder);
                }
            }
            return "None";
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
