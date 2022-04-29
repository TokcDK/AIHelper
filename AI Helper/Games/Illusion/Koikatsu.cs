using AIHelper.Manage;
using System.IO;

namespace AIHelper.Games.Illusion
{
    public class Koikatsu : IllusionGameBase
    {
        public override void InitActions()
        {
            base.InitActions();
            CopyModOrganizerUserFiles("MOKK");
        }
        public override bool IsHaveSideloaderMods => true;

        public override string ManifestGame => "koikatsu";

        public override string GetGameDirName()
        {
            return base.GetGameDirName();

            //return GetTheGameFolderName("Koikatsu");
        }

        public override string GetGameExeName()
        {
            return "Koikatu";
        }

        public override string GetGameDisplayingName()
        {
            return T._("Koikatsu");
        }

        public override string GetGameStudioExeName()
        {
            return "CharaStudio";
        }

        public override string GetGameAbbreviation()
        {
            return "KK";
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
            return new BaseGamePyFileInfo(nameof(Properties.Resources.game_koikatu), Properties.Resources.game_koikatu);
        }
    }
}
