using AIHelper.Manage;
using System.IO;

namespace AIHelper.Games.Illusion
{
    public class HoneySelect : IllusionGameBase
    {
        public override string ZipmodManifestGameName => "honeyselect";

        public override void InitActions()
        {
            base.InitActions();
            ManageModOrganizer.CopyModOrganizerUserFiles("MOHS");
        }

        //public override string GameDirName => base.GameDirName;
        //return GetTheGameFolderName("HoneySelect");
        public override string GameExeName => "HoneySelect_64";

        public override string GameExeNameX32 => "HoneySelect_32";

        public override string GameStudioExeName => "StudioNEO_64";

        public override string GameStudioExeNameX32 => "StudioNEO_32";

        public override string GameDisplayingName => T._("Honey Select");

        public override string GameAbbreviation => "HS";

        public override string[,] DirLinkPaths => new string[,]
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

        public override string BasicGamePluginName => "game_honeyselect";
    }
}
