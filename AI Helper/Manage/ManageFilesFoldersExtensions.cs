using System;
using System.IO;
using System.IO.Compression;

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

            foreach (var file in targetDir.GetFiles())
            {
                file.Attributes = FileAttributes.Normal;
                file.Delete();
            }

            foreach (var dir in targetDir.GetDirectories())
            {
                dir.DeleteRecursive();
            }

            targetDir.Delete(false);
        }

        /// <summary>
        /// removes invalid chars from string <paramref name="path"/> which can be used in file\dir path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string RemoveInvalidPathChars(this string path)
        {
            foreach (var c in Path.GetInvalidPathChars())
            {
                int index;
                while ((index = path.IndexOf(c)) != -1)
                {
                    path = path.Remove(index, 1);
                }
            }

            return path;
        }

        /// <summary>
        /// Opens <paramref name="zipfile"/> and find <paramref name="fileName"/> there and then returns stream of the entry
        /// </summary>
        /// <param name="zipfile">path ro exist zipFile</param>
        /// <param name="fileName">file name with extension</param>
        /// <returns>zipfile entry stream</returns>
        public static Stream GetZipEntryStream(this ZipArchive zipfile, string fileName)
        {
            if (zipfile == null || string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            Stream ret = null;

            try
            {
                foreach (ZipArchiveEntry entry in zipfile.Entries)
                {
                    if (entry.FullName.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        ret = entry.Open();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ManageLogs.Error("GetZipEntryStream. error:\r\n" + ex);
            }

            return ret;
        }
    }
}
