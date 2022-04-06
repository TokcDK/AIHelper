using AIHelper.Games;
using System.Collections.Generic;

namespace AIHelper.SharedData
{
    public class GameData
    {
        public GameData()
        {
            Game = Games[0];
        }

        /// <summary>
        /// List of valid games
        /// </summary>
        public List<GameBase> Games { get; set; } = new() { new Koikatsu(), new HoneySelect() };

        /// <summary>
        /// Current selected game
        /// </summary>
        public GameBase Game { get; set; }
    }
}
