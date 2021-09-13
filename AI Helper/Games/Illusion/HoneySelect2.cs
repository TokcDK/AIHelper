using AIHelper.Manage;
using System.IO;

namespace AIHelper.Games
{
    public class HoneySelect2 : GameBase
    {
        public override void InitActions()
        {
            base.InitActions();
            //CopyMOfiles("MOHS");
        }
        public override bool IsHaveSideloaderMods => true;

        public override string ManifestGame => "honeyselect2";

        public override string GetGameFolderName()
        {
            return base.GetGameFolderName();

            //return GetTheGameFolderName("HoneySelect2");
        }

        public override string GetGameExeName()
        {
            return "HoneySelect2";
        }

        public override string GetGameStudioExeName()
        {
            return "StudioNEOV2";
        }

        public override string GetGameDisplayingName()
        {
            return T._("Honey Select 2");
        }

        public override string GetGamePrefix()
        {
            return "HS2";
        }

        public override string[,] GetDirLinkPaths()
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
                        Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), "MyUserData", "UserData", "cap")
                        ,
                        Path.Combine(ManageSettings.GetCurrentGameDataPath(), "UserData", "cap")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), "MyUserData", "UserData", "studioneo", "BetterSceneLoader")
                        ,
                        Path.Combine(ManageSettings.GetCurrentGameDataPath(), "UserData", "studioneo", "BetterSceneLoader")
                    }
            };
        }

        public override BaseGamePyFileInfo GetBaseGamePyFile()
        {
            return new BaseGamePyFileInfo(nameof(Properties.Resources.game_honeyselect2), Properties.Resources.game_honeyselect2);
        }
    }
}
