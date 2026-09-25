using System.Collections.Generic;

namespace AIHelper.Games.Illusion
{
    public class AIGirlTrial : IllusionGameBase
    {
        public override string ZipmodManifestGameName => "AI Girl";

        public override string GameExeName => "AI-SyoujyoTrial";

        public override string GameDisplayingName => T._("AI-Girl") + "Trial";

        public override string GameAbbreviation => "AIS";

        /// <summary>
        /// The trial build writes the tags of the released game.
        /// </summary>
        public override IReadOnlyList<string> CardMarkers => new[]
        {
            "【AIS_Chara】",
            "【AIS_Clothes】",
        };

        public override string BasicGamePluginName => "game_aigirltrial";
    }
}
