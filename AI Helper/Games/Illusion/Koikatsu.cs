using AIHelper.Manage;
using System.IO;

namespace AIHelper.Games.Illusion
{
    public class Koikatsu : IllusionGameBase
    {
        public override void InitActions()
        {
            base.InitActions();
            ManageModOrganizer.CopyModOrganizerUserFiles("MOKK");
        }
        public override bool IsHaveSideloaderMods => true;

        public override string ZipmodManifestGameName => "koikatsu";

        //public override string GameDirName => base.GameDirName;
        //return GetTheGameFolderName("Koikatsu");
        public override string GameExeName => "Koikatu";

        public override string GameDisplayingName => T._("Koikatsu");

        public override string GameStudioExeName => "CharaStudio";

        public override string GameAbbreviation => "KK";

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

        public override string BasicGamePluginName => "game_koikatu";
    }
}
