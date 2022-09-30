using AIHelper.Manage;
using System.IO;

namespace AIHelper.Games.Illusion
{
    public class RoomGirl : IllusionGameBase
    {
        public override void InitActions()
        {
            base.InitActions();
            //CopyMOfiles("MOHS");
        }
        public override bool IsHaveSideloaderMods => true;

        public override string ZipmodManifestGameName => "roomgirl";

        //public override string GameDirName => base.GameDirName;
        //return GetTheGameFolderName("HoneySelect2");
        public override string GameExeName => "RoomGirl";

        public override string GameStudioExeName => "StudioNEOV2";

        public override string GameDisplayingName => T._("Room Girl");

        public override string GameAbbreviation => "rg";

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

        public override string BasicGamePluginName => "game_roomgirl";
    }
}
