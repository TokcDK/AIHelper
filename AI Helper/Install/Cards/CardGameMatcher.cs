using AIHelper.Games;
using AIHelper.Manage;
using GetListOfSubClasses;
using NLog;
using System;
using System.Collections.Generic;

namespace AIHelper.Install.Cards
{
    /// <summary>
    /// Tells which game a card belongs to, using the tag the card carries behind
    /// its preview image.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The tags are declared by the games themselves
    /// (<see cref="GameBase.CardMarkers"/>), so a new game only has to fill in that
    /// member and is understood here, instead of a second table which has to be
    /// kept in sync by hand.
    /// </para>
    /// <para>
    /// The games come from the reflection list rather than from the games the user
    /// has installed, because a card of a game which is not installed still has to
    /// be recognized.
    /// </para>
    /// <para>
    /// Several games share one tag (AI Girl and HoneySelect 2 both write
    /// <c>【AIS_Chara】</c>), and such a card can not be attributed to a single
    /// game. The ids are kept in a stable order and an installed game wins, so the
    /// result does not change between runs.
    /// </para>
    /// </remarks>
    internal static class CardGameMatcher
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private static readonly object SyncRoot = new object();

        /// <summary>
        /// Tag to the ids of the games which write it, or null until it was built.
        /// </summary>
        private static Dictionary<string, List<string>> _gameIdsByMarker;

        /// <summary>
        /// Returns the id of the game a card with the passed tag belongs to, when
        /// that is not the selected game.
        /// </summary>
        /// <param name="marker">Tag the card carries, or null when it has none</param>
        /// <param name="foreignGameId">Id of the game the card belongs to</param>
        /// <returns>
        /// False when the card is of the selected game, or of a game which is not
        /// known here.
        /// </returns>
        internal static bool TryGetForeignGameId(string marker, out string foreignGameId)
        {
            foreignGameId = null;

            if (string.IsNullOrEmpty(marker)) return false;

            List<string> gameIds;
            if (!GetGameIdsByMarker().TryGetValue(marker, out gameIds) || gameIds.Count == 0) return false;

            var currentGameId = GetCurrentGameId();
            if (!string.IsNullOrEmpty(currentGameId) && ContainsIgnoreCase(gameIds, currentGameId)) return false;

            foreignGameId = PreferInstalled(gameIds);

            return !string.IsNullOrEmpty(foreignGameId);
        }

        /// <summary>
        /// Id of the game the user selected, or null while no game is selected.
        /// </summary>
        private static string GetCurrentGameId()
        {
            var games = ManageSettings.Games;
            var currentGame = games == null ? null : games.Game;

            return currentGame?.GameAbbreviation;
        }

        /// <summary>
        /// Picks an installed game when the tag is shared by several of them, so a
        /// shared card goes to a game the user actually has.
        /// </summary>
        private static string PreferInstalled(List<string> gameIds)
        {
            var games = ManageSettings.Games == null ? null : ManageSettings.Games.Games;

            if (games != null)
            {
                for (var i = 0; i < gameIds.Count; i++)
                {
                    for (var j = 0; j < games.Count; j++)
                    {
                        if (string.Equals(games[j].GameAbbreviation, gameIds[i], StringComparison.OrdinalIgnoreCase))
                        {
                            return gameIds[i];
                        }
                    }
                }
            }

            return gameIds[0];
        }

        /// <summary>
        /// Tag to game ids map, built on first use.
        /// </summary>
        private static Dictionary<string, List<string>> GetGameIdsByMarker()
        {
            if (_gameIdsByMarker != null) return _gameIdsByMarker;

            lock (SyncRoot)
            {
                if (_gameIdsByMarker == null)
                {
                    _gameIdsByMarker = BuildGameIdsByMarker();
                }
            }

            return _gameIdsByMarker;
        }

        /// <summary>
        /// Reads the tags of every known game preset.
        /// </summary>
        private static Dictionary<string, List<string>> BuildGameIdsByMarker()
        {
            var gameIdsByMarker = new Dictionary<string, List<string>>(StringComparer.Ordinal);

            List<GameBase> games;
            try
            {
                games = Inherited.GetListOfinheritedSubClasses<GameBase>();
            }
            catch (Exception ex)
            {
                _log.Warn("Failed to list the game presets. Cards of another game can not be told apart.\r\nError:" + ex);
                return gameIdsByMarker;
            }

            // one broken preset must not cost the tags of all the other ones
            for (var i = 0; i < games.Count; i++)
            {
                try
                {
                    AddMarkersOf(games[i], gameIdsByMarker);
                }
                catch (Exception ex)
                {
                    _log.Warn("Failed to read the card tags of a game preset. Preset:" + games[i].GetType().FullName + "\r\nError:" + ex);
                }
            }

            // a stable order keeps the chosen game the same between runs, which the
            // reflection order of the presets is not
            foreach (var gameIds in gameIdsByMarker.Values)
            {
                gameIds.Sort(StringComparer.OrdinalIgnoreCase);
            }

            return gameIdsByMarker;
        }

        /// <summary>
        /// Adds the tags of one game to the map.
        /// </summary>
        private static void AddMarkersOf(GameBase game, Dictionary<string, List<string>> gameIdsByMarker)
        {
            var gameId = game.GameAbbreviation;
            if (string.IsNullOrWhiteSpace(gameId)) return;

            foreach (var marker in game.CardMarkers)
            {
                if (string.IsNullOrEmpty(marker)) continue;

                List<string> gameIds;
                if (!gameIdsByMarker.TryGetValue(marker, out gameIds))
                {
                    gameIds = new List<string>();
                    gameIdsByMarker[marker] = gameIds;
                }

                // AI Girl and its trial build share one id, and the same tag
                if (!ContainsIgnoreCase(gameIds, gameId)) gameIds.Add(gameId);
            }
        }

        private static bool ContainsIgnoreCase(List<string> values, string value)
        {
            for (var i = 0; i < values.Count; i++)
            {
                if (string.Equals(values[i], value, StringComparison.OrdinalIgnoreCase)) return true;
            }

            return false;
        }
    }
}
