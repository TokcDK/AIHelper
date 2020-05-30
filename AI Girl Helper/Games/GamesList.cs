using AIHelper.Games;
using System.Collections.Generic;

namespace AI_Helper.Games
{
    internal class GamesList
    {
        internal static List<Game> GetGamesList()
        {
            return new List<Game>(4)
            {
                new AISyoujyo(),
                new AISyoujyoTrial(),
                new HoneySelect(),
                new Koikatsu()
            };
        }
    }
}
