using System.IO;

namespace AIHelper.Manage
{
    static class ManageFilesFoldersExtensions
    {
        /// <summary>
        /// Delete directory with subfiles and dirs even if it is readonly
        /// </summary>
        /// <param name="targetDir"></param>
        internal static void DeleteRecursive(this DirectoryInfo targetDir)
        {
            targetDir.Attributes = FileAttributes.Normal;

            foreach (var file in targetDir.EnumerateFiles())
            {
                file.Attributes = FileAttributes.Normal;
                file.Delete();
            }

            foreach (var dir in targetDir.EnumerateDirectories())
            {
                dir.DeleteRecursive();
            }

            targetDir.Delete(false);
        }
    }
}
