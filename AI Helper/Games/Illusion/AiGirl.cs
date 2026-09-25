using System.Collections.Generic;

namespace AIHelper.Games.Illusion
{
    public class AiGirl : IllusionGameBase
    {
        public override string ZipmodManifestGameName => "AI Girl";

        public override string GameExeName => "AI-Syoujyo";

        public override string GameDisplayingName => T._("AI-Girl");

        public override string GameStudioExeName => "StudioNEOV2";

        public override string GameAbbreviation => "AIS";

        /// <summary>
        /// HoneySelect 2 writes the same tags, so a card with them can not be told
        /// apart from an AI Girl card.
        /// </summary>
        public override IReadOnlyList<string> CardMarkers => new[]
        {
            "【AIS_Chara】",
            "【AIS_Clothes】",
        };

        public override string BasicGamePluginName => "game_aigirl";
    }
}
