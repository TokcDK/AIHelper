using AIHelper;
using AIHelper.Games;
using AIHelper.Manage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Helper.Games
{
    class Koikatsu : Game
    {
        public override void InitActions()
        {
            CopyMOfiles("MOKK");
        }

        private string GameFolderName = string.Empty;
        public override string GetGameFolderName()
        {
            return GameFolderName.Length > 0 ? GameFolderName : (GameFolderName = SearchGameFolder()).Length>0? GameFolderName:"Koikatsu";
        }

        public override string GetGameEXEName()
        {
            return "Koikatu";
        }

        public override string GetGameDisplayingName()
        {
            return T._("Koikatsu");
        }

        public override string GetGameStudioEXEName()
        {
            return "CharaStudio";
        }

        public override string[,] GetObjectsForSymLinksPaths()
        {
            return new string[,]
            {
                    {
                        Path.Combine(ManageSettings.GetModsPath(), "BepInEx", "BepInEx", "core", "BepInEx.Preloader.dll")
                        ,
                        Path.Combine(ManageSettings.GetDataPath(), "BepInEx", "core", "BepInEx.Preloader.dll")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.GetModsPath(), "BepInEx", "doorstop_config.ini")
                        ,
                        Path.Combine(ManageSettings.GetDataPath(), "doorstop_config.ini")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.GetModsPath(), "BepInEx", "winhttp.dll")
                        ,
                        Path.Combine(ManageSettings.GetDataPath(), "winhttp.dll")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.GetCurrentGameMOOverwritePath(), "UserData", "MaterialEditor")
                        ,
                        Path.Combine(ManageSettings.GetDataPath(), "UserData", "MaterialEditor")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.GetCurrentGameMOOverwritePath(), "UserData", "Overlays")
                        ,
                        Path.Combine(ManageSettings.GetDataPath(), "UserData", "Overlays")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.GetCurrentGameMOOverwritePath(), "UserData", "cap")
                        ,
                        Path.Combine(ManageSettings.GetDataPath(), "UserData", "cap")
                    }
            };
        }
    }
}
