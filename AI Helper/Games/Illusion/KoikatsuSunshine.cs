using AIHelper.Manage;
using System.IO;

namespace AIHelper.Games.Illusion
{
    public class KoikatsuSunshine : IllusionGameBase
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

        public override string GetGameAbbreviation()
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
                        Path.Combine(ManageSettings.CurrentGameMoOverwritePath, "UserData", "MaterialEditor")
                        ,
                        Path.Combine(ManageSettings.CurrentGameDataDirPath, "UserData", "MaterialEditor")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.CurrentGameMoOverwritePath, "UserData", "Overlays")
                        ,
                        Path.Combine(ManageSettings.CurrentGameDataDirPath, "UserData", "Overlays")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.CurrentGameMoOverwritePath, "UserData", "cap")
                        ,
                        Path.Combine(ManageSettings.CurrentGameDataDirPath, "UserData", "cap")
                    }
            };
        }

        public override BaseGamePyFileInfo GetBaseGamePyFile()
        {
            return new BaseGamePyFileInfo(nameof(Properties.Resources.game_koikatusunshine), Properties.Resources.game_koikatusunshine);
        }
    }
}
