using System.Collections.Generic;

namespace AIHelper.Games
{
    internal class GamesList
    {
        internal static List<Game> GetCompatibleGamePresetsList()
        {
            return new List<Game>()
            {
                new AiSyoujyo(),
                new AiSyoujyoTrial(),
                new HoneySelect(),
                new HoneySelect2(),
                new Koikatsu(),
                new KoikatsuSunshine()
            };
        }
    }
}
