using AIHelper.Manage;
using System.Collections.Generic;
using System.IO;

namespace AIHelper.Games.Illusion
{
    public class Koikatsu : IllusionGameBase
    {
        public override void InitActions()
        {
            base.InitActions();
            ManageModOrganizer.CopyModOrganizerUserFiles("MOKK");
        }

        public override string ZipmodManifestGameName => "koikatu";

        public override string GameExeName => "Koikatu";

        public override string GameDisplayingName => T._("Koikatsu");

        public override string GameStudioExeName => "CharaStudio";

        public override string GameAbbreviation => "KK";

        /// <summary>
        /// The Steam build of the game and its special patch write their own tags,
        /// and they are Koikatsu cards as well.
        /// </summary>
        public override IReadOnlyList<string> CardMarkers => new[]
        {
            "【KoiKatuChara】",
            "【KoiKatuCharaS】",
            "【KoiKatuCharaSP】",
            "【KoiKatuClothes】",
        };

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

        public override string BasicGamePluginName => "game_koikatu";
    }
}
