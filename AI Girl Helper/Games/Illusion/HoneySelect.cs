using AIHelper.Manage;
using System.IO;

namespace AIHelper.Games
{
    public class HoneySelect : Game
    {
        public override void InitActions()
        {
            CopyMOfiles("MOHS");
        }

        private string GameFolderName = string.Empty;

        public override string GetGameFolderName()
        {
            return GameFolderName.Length > 0 ? GameFolderName : GameFolderName = SearchGameFolder();
        }

        public override string GetGameEXEName()
        {
            return "HoneySelect_64";
        }

        public override string GetGameEXENameX32()
        {
            return "HoneySelect_32";
        }

        public override string GetGameStudioEXEName()
        {
            return "StudioNEO_64";
        }

        public override string GetGameDisplayingName()
        {
            return T._("Honey Select");
        }

        public override string[,] GetObjectsForSymLinksPaths()
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
                        Path.Combine(ManageSettings.GetModsPath(), "MyUserData", "UserData", "cap")
                        ,
                        Path.Combine(ManageSettings.GetDataPath(), "UserData", "cap")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.GetModsPath(), "MyUserData", "UserData", "studioneo", "BetterSceneLoader")
                        ,
                        Path.Combine(ManageSettings.GetDataPath(), "UserData", "studioneo", "BetterSceneLoader")
                    }
            };
        }
    }
}
