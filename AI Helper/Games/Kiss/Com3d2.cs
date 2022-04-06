namespace AIHelper.Games
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

        public override string ManifestGame => "com3d2";

        public override bool IsHaveSideloaderMods => false;

        public override string GetGameDirName()
        {
            return base.GetGameDirName();

            //return GetTheGameFolderName("Koikatsu");
        }

        public override string GetGameExeName()
        {
            return "COM3D2x64";
        }

        public override string GetGameDisplayingName()
        {
            return T._("Custom Order Maid 3D2");
        }

        public override string GetGameStudioExeName()
        {
            return "CharaStudio";
        }

        public override string GetGameAbbreviation()
        {
            return "COM3D2";
        }

        public override string GetCharacterPresetsFolderSubPath()
        {
            return "Preset";
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
        }

        public override BaseGamePyFileInfo GetBaseGamePyFile()
        {
            return new BaseGamePyFileInfo(nameof(AIHelper.Properties.Resources.game_com3d2), Properties.Resources.game_com3d2);
        }
    }
}
