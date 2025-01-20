using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage.ModeSwitch
{
    internal class BackupCreator2
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        internal static void CopyDirectoryWithLinks(string sourceDir, string targetDir)
        {
            sourceDir = AddLongPathPrefix(sourceDir);
            targetDir = AddLongPathPrefix(targetDir);

            Directory.CreateDirectory(targetDir);

            foreach (var entry in Directory.GetFileSystemEntries(sourceDir))
            {
                string entryName = Path.GetFileName(entry);
                string targetPath = Path.Combine(targetDir, entryName);
                string entryWithPrefix = AddLongPathPrefix(entry);
                string targetPathWithPrefix = AddLongPathPrefix(targetPath);

                try
                {
                    if (Directory.Exists(entryWithPrefix))
                    {
                        if (entryWithPrefix.IsSymlink(objectType: ObjectType.Directory))
                        {
                            string linkTarget = Path.GetFullPath(entryWithPrefix.GetSymlinkTarget(ObjectType.Directory));
                            linkTarget.CreateSymlink(targetPathWithPrefix, isRelative: false, ObjectType.Directory);
                        }
                        else
                        {
                            Directory.CreateDirectory(targetPathWithPrefix);
                            CopyDirectoryWithLinks(entryWithPrefix, targetPathWithPrefix);
                        }
                    }
                    else if (File.Exists(entryWithPrefix))
                    {
                        if (entryWithPrefix.IsSymlink(objectType: ObjectType.File))
                        {
                            string linkTarget = Path.GetFullPath(entryWithPrefix.GetSymlinkTarget(ObjectType.File));
                            targetPathWithPrefix.CreateSymlink(linkTarget, isRelative: false, ObjectType.File);
                        }
                        else
                        {
                            entryWithPrefix.CreateHardlink(targetPathWithPrefix);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error($"Mode switch. Backup. Error processing {entry}: {ex.Message}");
                }
            }
        }

        static string AddLongPathPrefix(string path)
        {
            ManageStrings.CheckForLongPath(ref path);
            return path;
        }
    }
}
