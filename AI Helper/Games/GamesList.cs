using System.Collections.Generic;

namespace AIHelper.Games
{
    internal class GamesList
    {
        internal static List<GameBase> GetCompatibleGamePresetsList()
        {
            return new List<GameBase>()
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
