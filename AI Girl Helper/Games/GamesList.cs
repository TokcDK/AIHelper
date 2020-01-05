using AIHelper.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
