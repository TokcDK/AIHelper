using System.IO;
using AIHelper.Manage;

namespace AIHelper.Games.Illusion.IllGames
{
    internal class HoneyCome : IllGameBase
    {
        public override void InitActions()
        {
            base.InitActions();
        }

        public override string ZipmodManifestGameName => "honeycome";
        public override string GameExeName => "HoneyCome";

        public override string GameStudioExeName => "StudioNEOV2";

        public override string GameDisplayingName => T._("Honey Come");

        public override string GameAbbreviation => "HC";

        public override string[,] DirLinkPaths => new string[,]
            {
                    {
                        Path.Combine(ManageSettings.CurrentGameModsDirPath, "MyUserData", "UserData", "cap")
                        ,
                        Path.Combine(ManageSettings.CurrentGameDataDirPath, "UserData", "cap")
                    }
                    ,
                    {
                        Path.Combine(ManageSettings.CurrentGameModsDirPath, "MyUserData", "UserData", "studioneo", "BetterSceneLoader")
                        ,
                        Path.Combine(ManageSettings.CurrentGameDataDirPath, "UserData", "studioneo", "BetterSceneLoader")
                    }
            };

        public override string BasicGamePluginName => "game_honeycome";
    }
}
