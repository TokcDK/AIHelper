using System;
using System.Diagnostics;
using System.IO;

namespace AIHelper.Manage.FoldersTab.Folders
{
    /// <summary>
    /// Base for the buttons of the folders tab which do nothing but open one folder.
    /// </summary>
    /// <remarks>
    /// The button's <see cref="DirPath"/> is both what it announces and what it opens, so
    /// the guard and the explorer call below stay in step with the description a subclass
    /// writes.
    /// </remarks>
    internal abstract class SimpleFolderOpenBase : IFolderTabButtonData
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
