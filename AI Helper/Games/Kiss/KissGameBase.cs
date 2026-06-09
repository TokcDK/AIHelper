using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIHelper.Games.Kiss
{
    internal abstract class KissGameBase : GameBase
    {
        public override void InitActions()
        {
            base.InitActions();
        }

        public override bool IsHaveSideloaderMods => false;

        public override string CharacterPresetsFolderSubPath => "Preset";

        public override UserControl GameSettingsControl => new KissGameSettingsUserControl();

        public override string GetGameConfigFilePath(string parentPath)
        {
            return Path.Combine(parentPath, "config.xml");
        }

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
    }
}
