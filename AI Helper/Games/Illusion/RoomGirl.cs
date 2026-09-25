using System.Collections.Generic;

namespace AIHelper.Games.Illusion
{
    public class RoomGirl : IllusionGameBase
    {
        // ManageModOrganizer.CopyModOrganizerUserFiles("MOHS") is not called for
        // this game yet, so InitActions() is left as it is.

        public override string ZipmodManifestGameName => "roomgirl";

        public override string GameExeName => "RoomGirl";

        public override string GameStudioExeName => "StudioNEOV2";

        public override string GameDisplayingName => T._("Room Girl");

        public override string GameAbbreviation => "RG";

        public override IReadOnlyList<string> CardMarkers => new[]
        {
            "【RG_Chara】",
        };

        public override IReadOnlyList<PathPair> DirLinkPaths => CreateMyUserDataDirLinks();

        public override string BasicGamePluginName => "game_roomgirl";
    }
}
