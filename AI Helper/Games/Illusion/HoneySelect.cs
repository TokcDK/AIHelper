using AIHelper.Manage;
using System.IO;

namespace AIHelper.Games
{
    public class HoneySelect : GameBase
    {
        public override string ManifestGame => "honeyselect";

        public override void InitActions()
        {
            base.InitActions();
            CopyModOrganizerUserFiles("MOHS");
        }

        public override string GetGameFolderName()
        {
            return base.GetGameFolderName();

            //return GetTheGameFolderName("HoneySelect");
        }

        public override string GetGameExeName()
        {
            return "HoneySelect_64";
        }

        public override string GetGameExeNameX32()
        {
            return "HoneySelect_32";
        }

        public override string GetGameStudioExeName()
        {
            return "StudioNEO_64";
        }

        public override string GetGameStudioExeNameX32()
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
            return new BaseGamePyFileInfo(nameof(Properties.Resources.game_honeyselect), Properties.Resources.game_honeyselect);
        }
    }
}
