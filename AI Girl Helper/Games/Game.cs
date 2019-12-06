using AI_Helper.Utils;
using System.IO;

namespace AI_Helper.Games
{
    public abstract class Game
    {
        public string GamesLocationFolderName = "Games";

        public abstract string GetGameFolderName();

        public abstract string GetGameEXEName();

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
            return Path.Combine(Properties.Settings.Default.ApplicationStartupPath, GamesLocationFolderName, GetGameFolderName());
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

        public virtual string[,] GetObjectsForSymLinksPaths()
        {
            return new string[,] 
            {
                    {
                    Path.Combine(ManageSettings.GetModsPath(), "BepInEx", "Bepinex", "core", "BepInEx.Preloader.dll")
                        ,
                        Path.Combine(ManageSettings.GetDataPath(), "Bepinex", "core", "BepInEx.Preloader.dll")
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
            };
        }
    }
}
