using AIHelper.Games;
using System.Collections.Generic;
using System.IO;

namespace AIHelper.Games
{
    class RootGame : Game
    {
        public override void InitActions()
        {
            DetectRootGame();
            DetectedGame.InitActions();
        }

        public override bool isRootGame { get; set; } = true;

        public void DetectRootGame()
        {
            if (DetectedGame != null && File.Exists(Path.Combine(AIHelper.Properties.Settings.Default.ApplicationStartupPath, "Data", DetectedGame.GetGameEXEName() + ".exe")))
            {
                return;
            }

            List<Game> ListOfGames = GamesList.GetGamesList();
            foreach (var game in ListOfGames)
            {
                if (File.Exists(Path.Combine(AIHelper.Properties.Settings.Default.ApplicationStartupPath, "Data", game.GetGameEXEName() + ".exe")))
                {
                    DetectedGame = game;
                    break;
                }
            }
        }

        Game DetectedGame = null;
        public override string GetGameFolderName()
        {
            DetectRootGame();
            return "RootGame";
        }

        public override string GetGameEXEName()
        {
            DetectRootGame();
            return DetectedGame.GetGameEXEName();
        }

        public override string GetGameDisplayingName()
        {
            DetectRootGame();
            return DetectedGame.GetGameDisplayingName();
        }

        public override string GetGameStudioEXEName()
        {
            DetectRootGame();
            return DetectedGame.GetGameStudioEXEName();
        }

        public override string GetGameEXENameX32()
        {
            DetectRootGame();
            return DetectedGame.GetGameEXENameX32();
        }

        public override string GetGameStudioEXENameX32()
        {
            DetectRootGame();
            return DetectedGame.GetGameStudioEXENameX32();
        }

        public override string[,] GetDirLinkPaths()
        {
            DetectRootGame();
            return DetectedGame.GetDirLinkPaths();
        }

        public override string GetGamePrefix()
        {
            DetectRootGame();
            return DetectedGame.GetGamePrefix();
        }

        public override Dictionary<string, byte[]> GetBaseGamePyFile()
        {
            DetectRootGame();
            return DetectedGame.GetBaseGamePyFile();
        }
    }
}
