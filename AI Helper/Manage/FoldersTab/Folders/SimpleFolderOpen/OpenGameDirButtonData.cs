using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIHelper.Manage.Functions;

namespace AIHelper.Manage.FoldersTab.Folders
{
    internal class OpenGameDirButtonData : SimpleFolderOpenBase
    {
        public override string Text => T._("Game");

        public override string Description => T._("Open game dir");

        protected override string DirPath => ManageSettings.CurrentGameDataDirPath;
    }
}
