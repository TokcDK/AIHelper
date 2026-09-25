using AIHelper.Manage;
using System.Collections.Generic;
using System.IO;

namespace AIHelper.Games.Illusion
{
    public class KoikatsuSunshine : IllusionGameBase
    {
        // ManageModOrganizer.CopyModOrganizerUserFiles("MOKKS") is not called for
        // this game yet, so InitActions() is left as it is.

        public override string ZipmodManifestGameName => "koikatsusunshine";

        public override string GameExeName => "KoikatsuSunshine";
        public override string GameExeNameVr => "KoikatsuSunshine_VR";

        public override string GameDisplayingName => T._("Koikatsu Sunshine");

        public override string GameStudioExeName => "CharaStudio";

        public override string GameAbbreviation => "KKS";

        public override IReadOnlyList<PathPair> DirLinkPaths => new[]
        {
            DirLink(
                Path.Combine(ManageSettings.CurrentGameMoOverwritePath, UserDataDirName, "MaterialEditor"),
                Path.Combine(ManageSettings.CurrentGameDataDirPath, UserDataDirName, "MaterialEditor")),
            DirLink(
                Path.Combine(ManageSettings.CurrentGameMoOverwritePath, UserDataDirName, "Overlays"),
                Path.Combine(ManageSettings.CurrentGameDataDirPath, UserDataDirName, "Overlays")),
            DirLink(
                Path.Combine(ManageSettings.CurrentGameMoOverwritePath, UserDataDirName, "cap"),
                Path.Combine(ManageSettings.CurrentGameDataDirPath, UserDataDirName, "cap")),
        };

        public override string BasicGamePluginName => "game_koikatusunshine";
    }
}
