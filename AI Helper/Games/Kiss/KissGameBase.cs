using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIHelper.Games.Kiss
{
    internal abstract class KissGameBase : GameBase
    {
        public override void InitActions()
        {
            base.InitActions();
        }

        public override bool IsHaveSideloaderMods => false;

        public override string CharacterPresetsFolderSubPath => "Preset";

        public override UserControl GameSettingsControl => new KissGameSettingsUserControl();

        public override string GetGameConfigFilePath(string parentPath)
        {
            return Path.Combine(parentPath, "config.xml");
        }

        public override string[,] DirLinkPaths => new string[,]
            {
            };
    }
}
