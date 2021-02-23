using AIHelper.Manage;
using System.IO;

namespace AIHelper.Games
{
    public class HoneySelect2 : Game
    {
        public override void InitActions()
        {
            //CopyMOfiles("MOHS");
        }

        public override string GetGameFolderName()
        {
            return GetTheGameFolderName("HoneySelect2");
        }

        public override string GetGameEXEName()
        {
            return "HoneySelect2";
        }

        public override string GetGameStudioEXEName()
        {
            return "StudioNEOV2";
        }

        public override string GetGameDisplayingName()
        {
            return T._("Honey Select 2");
        }

        public override string GetGamePrefix()
        {
            return "HS2";
        }

        public override string[,] GetObjectsForSymLinksPaths()
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
                        Path.Combine(ManageSettings.GetCurrentGameModsPath(), "MyUserData", "UserData", "cap")
                        ,
                        Path.Combine(ManageSettings.GetCurrentGameDataPath(), "UserData", "cap")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.GetCurrentGameModsPath(), "MyUserData", "UserData", "studioneo", "BetterSceneLoader")
                        ,
                        Path.Combine(ManageSettings.GetCurrentGameDataPath(), "UserData", "studioneo", "BetterSceneLoader")
                    }
            };
        }

        internal override System.Collections.Generic.Dictionary<string, byte[]> GetBaseGamePyFile()
        {
            return new System.Collections.Generic.Dictionary<string, byte[]>
                {
                    { nameof(Properties.Resources.game_honeyselect2), Properties.Resources.game_honeyselect2}
                };
        }
    }
}
