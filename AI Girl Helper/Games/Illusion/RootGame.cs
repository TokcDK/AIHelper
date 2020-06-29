using AIHelper.Games;
using System.Collections.Generic;
using System.IO;

namespace AI_Helper.Games
{
    class RootGame : Game
    {
        public override void InitActions()
        {
            DetectRootGame();
        }

        public void DetectRootGame()
        {
            if (DetectedGame == null)
            {
                List<Game> ListOfGames = GamesList.GetGamesList();
                foreach (var game in ListOfGames)
                {
                    if (File.Exists(Path.Combine(AIHelper.Properties.Settings.Default.ApplicationStartupPath, "Data", game.GetGameEXEName())))
                    {
                        DetectedGame = game;
                        break;
                    }
                }
            }
        }

        Game DetectedGame = null;
        public override string GetGameFolderName()
        {
            DetectRootGame();
            return string.Empty;
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

        public override string[,] GetObjectsForSymLinksPaths()
        {
            DetectRootGame();
            return DetectedGame.GetObjectsForSymLinksPaths();
        }
    }
}
