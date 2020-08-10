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

        public override string GetGameFolderName()
        {
            return GetTheGameFolderName("HoneySelect");
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

        public override string GetGameStudioEXENameX32()
        {
            return "StudioNEO_32";
        }

        public override string GetGameDisplayingName()
        {
            return T._("Honey Select");
        }

        public override string GetGamePrefix()
        {
            return "HS";
        }

        public override string[,] GetObjectsForSymLinksPaths()
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
    }
}
