using AIHelper.Manage;
using System.Collections.Generic;

namespace AIHelper.Games.Illusion
{
    public class HoneySelect : IllusionGameBase
    {
        public override string ZipmodManifestGameName => "honeyselect";

        public override void InitActions()
        {
            base.InitActions();
            ManageModOrganizer.CopyModOrganizerUserFiles("MOHS");
        }

        public override string GameExeName => "HoneySelect_64";

        public override string GameExeNameX32 => "HoneySelect_32";

        public override string GameStudioExeName => "StudioNEO_64";

        public override string GameStudioExeNameX32 => "StudioNEO_32";

        public override string GameDisplayingName => T._("Honey Select");

        public override string GameAbbreviation => "HS";

        /// <summary>
        /// The tags of this game name the sex of the character already.
        /// </summary>
        public override IReadOnlyList<string> CardMarkers => new[]
        {
            "【HoneySelectCharaFemale】",
            "【HoneySelectCharaMale】",
            "【HoneySelectClothesFemale】",
            "【HoneySelectClothesMale】",
        };

        public override bool IsHaveSideloaderMods => false;

        public override IReadOnlyList<PathPair> DirLinkPaths => CreateMyUserDataDirLinks();

        public override string BasicGamePluginName => "game_honeyselect";
    }
}
