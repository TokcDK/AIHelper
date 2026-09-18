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
    public abstract class SimpleFolderOpenBase : IFolderTabButtonData
    {
        public abstract string Text { get; }

        public abstract string Description { get; }

        public void OnClick(object o, EventArgs e) { OpenDir(); }

        protected abstract string DirPath { get; }

        /// <summary>
        /// for external use. open dir using selected data
        /// </summary>
        public void OpenDir()
        {
            //guard the directory that is actually opened: DirPath is overridden per button
            //(Mods, Overwrite, MO dir, 2MO dir, user files) and has nothing to do with the Data dir
            if (!Directory.Exists(DirPath)) return;

            Process.Start("explorer.exe", DirPath);
        }
    }
}
