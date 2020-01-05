using AIHelper;
using AIHelper.Games;
using AIHelper.Manage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Helper.Games
{
    class Koikatsu : Game
    {
        public override void InitActions()
        {
            CopyMOfiles("MOKK");
        }

        private string GameFolderName = string.Empty;
        public override string GetGameFolderName()
        {
            return GameFolderName.Length > 0 ? GameFolderName : GameFolderName = SearchGameFolder();
        }

        public override string GetGameEXEName()
        {
            return "Koikatu";
        }

        public override string GetGameDisplayingName()
        {
            return T._("Koikatsu");
        }

        public override string GetGameStudioEXEName()
        {
            return "CharaStudio";
        }
    }
}
