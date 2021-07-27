using System.IO;

namespace AIHelper
{
    //https://code.4noobz.net/c-copy-a-folder-its-content-and-the-subfolders/
    internal static class CopyFolder
    {
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
                fi.CopyTo(Path.Combine(targetDirectory.FullName, fi.Name), overwriteFiles);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in sourceDirectory.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    targetDirectory.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir, overwriteFiles);
            }
        }
    }
}