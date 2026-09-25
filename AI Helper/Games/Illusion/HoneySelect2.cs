using System.Collections.Generic;

namespace AIHelper.Games.Illusion
{
    public class HoneySelect2 : IllusionGameBase
    {
        // ManageModOrganizer.CopyModOrganizerUserFiles("MOHS") is not called for
        // this game yet, so InitActions() is left as it is.

        public override string ZipmodManifestGameName => "honeyselect2";

        public override string GameExeName => "HoneySelect2";

        public override string GameStudioExeName => "StudioNEOV2";

        public override string GameDisplayingName => T._("Honey Select 2");

        public override string GameAbbreviation => "HS2";

        public override IReadOnlyList<PathPair> DirLinkPaths => CreateMyUserDataDirLinks();

        public override string BasicGamePluginName => "game_honeyselect2";
    }
}
