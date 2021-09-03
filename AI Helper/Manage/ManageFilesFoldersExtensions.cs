using System;
using System.IO;
using System.IO.Compression;

namespace AIHelper.Manage
{
    static class ManageFilesFoldersExtensions
    {
        /// <summary>
        /// Get case sensitive directory info of exist dir
        /// </summary>
        /// <param name="inputInfo"></param>
        /// <returns></returns>
        public static DirectoryInfo GetCaseSensitive(this DirectoryInfo inputInfo)
        {
            foreach (var info in inputInfo.Parent.GetDirectories(inputInfo.Name))
            {
                return info;
            }

            return inputInfo;
        }
        /// <summary>
        /// Get case sensitive name of the file
        /// </summary>
        /// <param name="inputInfo"></param>
        /// <returns></returns>
        public static FileInfo GetCaseSensitive(this FileInfo inputInfo)
        {
            foreach (var info in inputInfo.Directory.GetFiles(inputInfo.Name))
            {
                return info;
            }

            return inputInfo;
        }

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

        //https://code.4noobz.net/c-copy-a-folder-its-content-and-the-subfolders/
        /// <summary>
        /// Copy Directory with its all content
        /// </summary>
        /// <param name="sourceDirectory"></param>
        /// <param name="targetDirectory"></param>
        public static void CopyAll(this string sourceDirectory, string targetDirectory, bool overwriteFiles = false)
        {
            new DirectoryInfo(sourceDirectory).CopyAll(new DirectoryInfo(targetDirectory), overwriteFiles);
        }

        /// <summary>
        /// Copy Directory with its all content
        /// </summary>
        /// <param name="sourceDirectory"></param>
        /// <param name="targetDirectory"></param>
        public static void CopyAll(this DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory, bool overwriteFiles = false)
        {
            Directory.CreateDirectory(targetDirectory.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in sourceDirectory.GetFiles())
            {
                //Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                var targetPath = Path.Combine(targetDirectory.FullName, fi.Name);
                if (!overwriteFiles && File.Exists(targetPath))
                {
                    continue;
                }
                fi.CopyTo(targetPath, overwriteFiles);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in sourceDirectory.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    targetDirectory.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir, overwriteFiles);
            }
        }

        /// <summary>
        /// Copy Directory with its all content
        /// </summary>
        /// <param name="sourceDirectory"></param>
        /// <param name="targetDirectory"></param>
        public static void MoveAll(this DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory, bool overwriteFiles = false, bool cleanEmptyDirs = true)
        {
            Directory.CreateDirectory(targetDirectory.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in sourceDirectory.GetFiles())
            {
                //Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                var targetPath = Path.Combine(targetDirectory.FullName, fi.Name);
                if (File.Exists(targetPath) && !overwriteFiles)
                {
                    continue;
                }
                fi.MoveTo(targetPath, overwriteFiles);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in sourceDirectory.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    targetDirectory.CreateSubdirectory(diSourceSubDir.Name);
                MoveAll(diSourceSubDir, nextTargetSubDir, overwriteFiles);
            }

            if (cleanEmptyDirs)
            {
                ManageFilesFolders.DeleteEmptySubfolders(sourceDirectory.FullName, true);
            }
        }

        public static void MoveTo(this FileInfo fileInfo, string targetPath, bool overwrite = false)
        {
            var fi = new FileInfo(targetPath);
            if (!overwrite)
            {
                fi = new FileInfo(targetPath).GetNewTargetName();
            }

            fileInfo.CopyTo(fi.FullName, true);
        }
    }
}
