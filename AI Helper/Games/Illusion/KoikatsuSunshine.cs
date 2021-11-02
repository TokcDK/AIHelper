using AIHelper.Manage;
using System.IO;

namespace AIHelper.Games
{
    class KoikatsuSunshine : GameBase
    {
        public override void InitActions()
        {
            base.InitActions();
            //CopyModOrganizerUserFiles("MOKKS");
        }
        public override bool IsHaveSideloaderMods => true;

        public override string ManifestGame => "koikatsusunshine";

        public override string GetGameDirName()
        {
            return base.GetGameDirName();

            //return GetTheGameFolderName("KoikatsuSunshine");
        }

        public override string GetGameExeName()
        {
            return "KoikatsuSunshine";
        }

        public override string GetGameDisplayingName()
        {
            return T._("Koikatsu Sunshine");
        }

        public override string GetGameStudioExeName()
        {
            return "CharaStudio";
        }

        public override string GetGamePrefix()
        {
            return "KKS";
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
                        Path.Combine(ManageSettings.GetCurrentGameMoOverwritePath(), "UserData", "MaterialEditor")
                        ,
                        Path.Combine(ManageSettings.GetCurrentGameDataDirPath(), "UserData", "MaterialEditor")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.GetCurrentGameMoOverwritePath(), "UserData", "Overlays")
                        ,
                        Path.Combine(ManageSettings.GetCurrentGameDataDirPath(), "UserData", "Overlays")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.GetCurrentGameMoOverwritePath(), "UserData", "cap")
                        ,
                        Path.Combine(ManageSettings.GetCurrentGameDataDirPath(), "UserData", "cap")
                    }
            };
        }

        public override BaseGamePyFileInfo GetBaseGamePyFile()
        {
            return new BaseGamePyFileInfo(nameof(Properties.Resources.game_koikatusunshine), Properties.Resources.game_koikatusunshine);
        }
    }
}
