using AIHelper.Manage;
using System.IO;

namespace AIHelper.Games.Illusion
{
    public class HoneySelect2 : IllusionGameBase
    {
        public override void InitActions()
        {
            base.InitActions();
            //CopyMOfiles("MOHS");
        }
        public override bool IsHaveSideloaderMods => true;

        public override string ZipmodManifestGameName => "honeyselect2";

        //public override string GameDirName => base.GameDirName;
        //return GetTheGameFolderName("HoneySelect2");
        public override string GameExeName => "HoneySelect2";

        public override string GameStudioExeName => "StudioNEOV2";

        public override string GameDisplayingName => T._("Honey Select 2");

        public override string GameAbbreviation => "HS2";

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

        public override BaseGamePyFileInfo ModOrganizerBaseGamePyFile => new BaseGamePyFileInfo(nameof(Properties.Resources.game_honeyselect2), Properties.Resources.game_honeyselect2);
    }
}
