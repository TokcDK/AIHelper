using IWshRuntimeLibrary;
using System;
using System.IO;

namespace AIHelper
{
    internal static class Shortcut
    {
        /// <summary>
        /// Creates shortcut
        /// </summary>
        /// <param name="shortcutname"></param>
        /// <param name="targetpath"></param>
        /// <param name="workingdir"></param>
        /// <param name="description"></param>
        internal static void Create(string shortcutname, string targetpath, string arguments, string workingdir, string description = "", string iconlocation = "")
        {
            var startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var shortCutLinkFilePath = Path.Combine(startupFolderPath, shortcutname + ".lnk");
            if (System.IO.File.Exists(shortCutLinkFilePath))
            {
            }
            else
            {
                var shell = new WshShell();
                var windowsApplicationShortcut = (IWshShortcut)shell.CreateShortcut(shortCutLinkFilePath);
                windowsApplicationShortcut.Description = description;
                windowsApplicationShortcut.WorkingDirectory = workingdir;
                windowsApplicationShortcut.TargetPath = targetpath;
                windowsApplicationShortcut.Arguments = arguments;
                if (string.IsNullOrEmpty(iconlocation))
                {
                }
                else
                {
                    windowsApplicationShortcut.IconLocation = iconlocation;
                }
                windowsApplicationShortcut.Save();
            }
        }
    }
}