using System.Collections.Generic;
using System.IO;

namespace AIHelper.Games.Illusion
{
    class RootGame : IllusionGameBase
    {
        public override void InitActions()
        {
            DetectRootGame();
            _detectedGame.InitActions();
        }

        public override bool IsRootGame { get; set; } = true;

        public void DetectRootGame()
        {
            if (_detectedGame != null && File.Exists(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "Data", _detectedGame.GetGameExeName() + ".exe")))
            {
                return;
            }

            List<GameBase> listOfGames = SharedData.GameData.Games != null ? SharedData.GameData.Games : GamesList.GetCompatibleGamePresetsList();
            foreach (var game in listOfGames)
            {
                if (File.Exists(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "Data", game.GetGameExeName() + ".exe")))
                {
                    // set game's folder name
                    game.GameDirInfo = new DirectoryInfo(Properties.Settings.Default.ApplicationStartupPath);

                    _detectedGame = game;

                    break;
                }
            }
        }

        GameBase _detectedGame;
        public override string GetGameDirName()
        {
            //DetectRootGame();
            return _detectedGame.GetGameDirName();
        }

        public override string GetGameExeName()
        {
            //DetectRootGame();
            return _detectedGame.GetGameExeName();
        }

        public override string GetGameDisplayingName()
        {
            //DetectRootGame();
            return _detectedGame.GetGameDisplayingName();
        }

        public override string GetGameStudioExeName()
        {
            //DetectRootGame();
            return _detectedGame.GetGameStudioExeName();
        }

        public override string GetGameExeNameX32()
        {
            //DetectRootGame();
            return _detectedGame.GetGameExeNameX32();
        }

        public override string GetGameStudioExeNameX32()
        {
            //DetectRootGame();
            return _detectedGame.GetGameStudioExeNameX32();
        }

        public override string[,] GetDirLinkPaths()
        {
            //DetectRootGame();
            return _detectedGame.GetDirLinkPaths();
        }

        public override string GetGameAbbreviation()
        {
            //DetectRootGame();
            return _detectedGame.GetGameAbbreviation();
        }

        public override BaseGamePyFileInfo GetBaseGamePyFile()
        {
            //DetectRootGame();
            return _detectedGame.GetBaseGamePyFile();
        }

        public override string ManifestGame => _detectedGame.ManifestGame;
    }
}
