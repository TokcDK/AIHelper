using System.Collections.Generic;

namespace AIHelper.Games.Illusion.IllGames
{
    internal class HoneyCome : IllGameBase
    {
        public override string ZipmodManifestGameName => "honeycome";
        public override string GameExeName => "HoneyCome";

        public override string GameStudioExeName => "StudioNEOV2";

        public override string GameDisplayingName => T._("Honey Come");

        public override string GameAbbreviation => "HC";

        public override IReadOnlyList<PathPair> DirLinkPaths => CreateMyUserDataDirLinks();

        public override string BasicGamePluginName => "game_honeycome";
    }
}
