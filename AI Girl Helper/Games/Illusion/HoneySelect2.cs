using AIHelper.Manage;
using System.IO;

namespace AIHelper.Games
{
    public class HoneySelect2 : Game
    {
        public override void InitActions()
        {
            //CopyMOfiles("MOHS");
        }

        public override string GetGameFolderName()
        {
            return GetTheGameFolderName("HoneySelect2");
        }

        public override string GetGameEXEName()
        {
            return "HoneySelect2";
        }

        public override string GetGameStudioEXEName()
        {
            return "StudioNEOV2";
        }

        public override string GetGameDisplayingName()
        {
            return T._("Honey Select 2");
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
