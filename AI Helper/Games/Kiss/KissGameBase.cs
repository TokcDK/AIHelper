using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace AIHelper.Games.Kiss
{
    internal abstract class KissGameBase : GameBase
    {
        public override bool IsHaveSideloaderMods => false;

        public override string CharacterPresetsFolderSubPath => "Preset";

        public override UserControl GameSettingsControl => new KissGameSettingsUserControl();

        public override string GetGameConfigFilePath(string parentPath)
        {
            return Path.Combine(parentPath, "config.xml");
        }

        /// <summary>
        /// The Kiss games keep no user data inside the Mod Organizer folder, so
        /// there is nothing to link into the game folder.
        /// </summary>
        public override IReadOnlyList<PathPair> DirLinkPaths => Array.Empty<PathPair>();
    }
}
