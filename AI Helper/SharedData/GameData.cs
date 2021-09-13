using AIHelper.Games;
using System.Collections.Generic;

namespace AIHelper.SharedData
{
    internal static class GameData
    {
        /// <summary>
        /// List of valid games
        /// </summary>
        internal static List<GameBase> ListOfGames;

        /// <summary>
        /// Current selected game
        /// </summary>
        internal static GameBase CurrentGame;

        /// <summary>
        /// reference to the main form
        /// </summary>
        public static MainForm MainForm { get; internal set; }
    }
}
