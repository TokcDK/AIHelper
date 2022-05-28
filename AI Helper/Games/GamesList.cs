using AIHelper.Games.Illusion;
using System.Collections.Generic;

namespace AIHelper.Games
{
    internal class GamesList
    {
        internal static List<GameBase> GetCompatibleGamePresetsList()
        {
            return new List<GameBase>()
            {
                new AiGirl(),
                new AIGirlTrial(),
                new HoneySelect(),
                new HoneySelect2(),
                new Koikatsu(),
                new KoikatsuSunshine()
            };
        }
    }
}
