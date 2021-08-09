using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Install.Types.Directories.CardsFromDir.Characters
{
    abstract class CharacterCardInstallerBase:CardsFromDirsInstallerBase
    {
        protected override string typeFolder => "chara";
        protected override string TargetSuffix => "Chars";
    }
}
