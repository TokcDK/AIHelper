namespace AIHelper.Games.Kiss
{
    class Com3d2 : GameBase
    {
        public override void InitActions()
        {
            base.InitActions();
        }

        //english is 'HKEY_CURRENT_USER\Software\KISS\CUSTOM ORDER MAID3D 2'
        public override string RegistryPath => @"HKEY_CURRENT_USER\Software\KISS\カスタムオーダーメイド3D2";
        public override string RegistryInstallDirKey => "InstallPath";

        public override string ZipmodManifestGameName => "com3d2";

        public override bool IsHaveSideloaderMods => false;

        //public override string GameDirName => base.GameDirName;
        //return GetTheGameFolderName("Koikatsu");
        public override string GameExeName => "COM3D2x64";

        public override string GameDisplayingName => T._("Custom Order Maid 3D2");

        public override string GameStudioExeName => "CharaStudio";

        public override string GameAbbreviation => "COM3D2";

        public override string CharacterPresetsFolderSubPath => "Preset";

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
                    //{
                    //    Path.Combine(ManageSettings.GetCurrentGameMoOverwritePath(), "UserData", "MaterialEditor")
                    //    ,
                    //    Path.Combine(ManageSettings.GetCurrentGameDataPath(), "UserData", "MaterialEditor")
                    //}
                    //,
                    //{
                    //    Path.Combine(ManageSettings.GetCurrentGameMoOverwritePath(), "UserData", "Overlays")
                    //    ,
                    //    Path.Combine(ManageSettings.GetCurrentGameDataPath(), "UserData", "Overlays")
                    //}
                    //,
                    //{
                    //    Path.Combine(ManageSettings.GetCurrentGameMoOverwritePath(), "UserData", "cap")
                    //    ,
                    //    Path.Combine(ManageSettings.GetCurrentGameDataPath(), "UserData", "cap")
                    //}
            };

        public override BaseGamePyFileInfo ModOrganizerBaseGamePyFile => new BaseGamePyFileInfo(nameof(Properties.Resources.game_com3d2), Properties.Resources.game_com3d2);
    }
}
