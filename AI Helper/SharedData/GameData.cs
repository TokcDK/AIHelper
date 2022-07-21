using AIHelper.Games;
using System.Collections.Generic;

namespace AIHelper.SharedData
{
    internal class GameData
    {
        public int CurrentGameListIndex { get => Games.IndexOf(Game); internal set { Game = Games[value]; } }

        /// <summary>
        /// List of valid games
        /// </summary>
        internal List<GameBase> Games { get; set; }

        /// <summary>
        /// Current selected game
        /// </summary>
        internal GameBase Game { get; set; }
    }
}
