﻿using AIHelper;
using AIHelper.Games;
using AIHelper.Manage;
using System.IO;

namespace AIHelper.Games
{
    class Koikatsu : Game
    {
        public override void InitActions()
        {
            base.InitActions();
            CopyModOrganizerUserFiles("MOKK");
        }
        public override bool IsHaveSideloaderMods { get; set; } = true;

        public override string GetGameFolderName()
        {
            return GetTheGameFolderName("Koikatsu");
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

        public override string GetGamePrefix()
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
                        Path.Combine(ManageSettings.GetCurrentGameDataPath(), "UserData", "MaterialEditor")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.GetCurrentGameMoOverwritePath(), "UserData", "Overlays")
                        ,
                        Path.Combine(ManageSettings.GetCurrentGameDataPath(), "UserData", "Overlays")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.GetCurrentGameMoOverwritePath(), "UserData", "cap")
                        ,
                        Path.Combine(ManageSettings.GetCurrentGameDataPath(), "UserData", "cap")
                    }
            };
        }

        public override System.Collections.Generic.Dictionary<string, byte[]> GetBaseGamePyFile()
        {
            return new System.Collections.Generic.Dictionary<string, byte[]>
                {
                    { nameof(AIHelper.Properties.Resources.game_koikatu), AIHelper.Properties.Resources.game_koikatu }
                };
        }
    }
}
