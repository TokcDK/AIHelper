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

        public override string ZipmodManifestGameName => "koikatsusunshine";

        //public override string GameDirName => base.GameDirName;
        //return GetTheGameFolderName("KoikatsuSunshine");
        public override string GameExeName => "KoikatsuSunshine";

        public override string GameDisplayingName => T._("Koikatsu Sunshine");

        public override string GameStudioExeName => "CharaStudio";

        public override string GameAbbreviation => "KKS";

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

        public override BaseGamePyFileInfo ModOrganizerBaseGamePyFile => new BaseGamePyFileInfo(nameof(Properties.Resources.game_koikatusunshine), Properties.Resources.game_koikatusunshine);
    }
}
