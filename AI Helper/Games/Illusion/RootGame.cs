﻿using AIHelper.Games;
using System.Collections.Generic;
using System.IO;

namespace AIHelper.Games
{
    class RootGame : Game
    {
        public override void InitActions()
        {
            DetectRootGame();
            _detectedGame.InitActions();
        }

        public override bool IsRootGame { get; set; } = true;

        public void DetectRootGame()
        {
            if (_detectedGame != null && File.Exists(Path.Combine(AIHelper.Properties.Settings.Default.ApplicationStartupPath, "Data", _detectedGame.GetGameExeName() + ".exe")))
            {
                return;
            }

            List<Game> listOfGames = GamesList.GetGamesList();
            foreach (var game in listOfGames)
            {
                if (File.Exists(Path.Combine(AIHelper.Properties.Settings.Default.ApplicationStartupPath, "Data", game.GetGameExeName() + ".exe")))
                {
                    _detectedGame = game;
                    break;
                }
            }
        }

        Game _detectedGame;
        public override string GetGameFolderName()
        {
            DetectRootGame();
            return "RootGame";
        }

        public override string GetGameExeName()
        {
            DetectRootGame();
            return _detectedGame.GetGameExeName();
        }

        public override string GetGameDisplayingName()
        {
            DetectRootGame();
            return _detectedGame.GetGameDisplayingName();
        }

        public override string GetGameStudioExeName()
        {
            DetectRootGame();
            return _detectedGame.GetGameStudioExeName();
        }

        public override string GetGameExeNameX32()
        {
            DetectRootGame();
            return _detectedGame.GetGameExeNameX32();
        }

        public override string GetGameStudioExeNameX32()
        {
            DetectRootGame();
            return _detectedGame.GetGameStudioExeNameX32();
        }

        public override string[,] GetDirLinkPaths()
        {
            DetectRootGame();
            return _detectedGame.GetDirLinkPaths();
        }

        public override string GetGamePrefix()
        {
            DetectRootGame();
            return _detectedGame.GetGamePrefix();
        }

        public override Dictionary<string, byte[]> GetBaseGamePyFile()
        {
            DetectRootGame();
            return _detectedGame.GetBaseGamePyFile();
        }

        public override string ManifestGame => _detectedGame.ManifestGame;
    }
}
