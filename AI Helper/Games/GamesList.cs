﻿using AIHelper.Games;
using System.Collections.Generic;

namespace AIHelper.Games
{
    internal class GamesList
    {
        internal static List<Game> GetGamesList()
        {
            return new List<Game>(4)
            {
                new AiSyoujyo(),
                new AiSyoujyoTrial(),
                new HoneySelect(),
                new HoneySelect2(),
                new Koikatsu()
            };
        }
    }
}
