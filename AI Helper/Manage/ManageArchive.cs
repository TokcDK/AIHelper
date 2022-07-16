using System;
using System.IO;
using System.IO.Compression;

namespace AIHelper.Manage
{
    class ManageArchive
    {
        public static void UnpackArchivesToSubfoldersWithSameName(string dirForSearch, string extension)
        {
            if (dirForSearch.Length == 0 || extension.Length == 0) return;

            if (extension.Substring(0, 1) != ".") extension = "." + extension;

            if (extension.Length == 0 || !Directory.Exists(dirForSearch)) return;

            foreach (var file in Directory.GetFiles(dirForSearch, "*" + extension, SearchOption.AllDirectories))
            {
                string targetDir = ManageFilesFoldersExtensions.GetResultTargetDirPathWithNameCheck(dirForSearch, Path.GetFileNameWithoutExtension(file));
                if (Directory.Exists(targetDir)) continue;

                try
                {
                    if (string.Equals(extension, "zip", StringComparison.InvariantCultureIgnoreCase))
                    {
                        using (ZipArchive archive = ZipFile.OpenRead(file)) archive.ExtractToDirectory(targetDir);
                    }
                    else
                    {
                        Compressor.Decompress(file, targetDir);
                    }
                    //File.Delete(file);
                    File.Move(file, file + ".Extracted" + ManageSettings.DateTimeBasedSuffix);
                }
                catch
                {
                    //if decompression failed
                }
            }
        }

        // Number of files within zip archive
        public static int GetFilesCountInZipArchive(ZipArchive inputArchive = null, string zipFilePath = "")
        {
            int count = 0;

            if (inputArchive != null)
            {
                // We count only named (i.e. that are with files) entries
                foreach (var entry in inputArchive.Entries) if (!string.IsNullOrEmpty(entry.Name)) count++;
            }
            else if (zipFilePath.Length > 0)
            {
                using (ZipArchive archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Read))
                {
                    // We count only named (i.e. that are with files) entries
                    foreach (var entry in archive.Entries) if (!string.IsNullOrEmpty(entry.Name)) count++;
                }
            }

            return count;
        }

        internal static string GetZipmodGuid(string zipmodPath)
        {
            if (string.IsNullOrWhiteSpace(zipmodPath) || !File.Exists(zipmodPath)) return string.Empty;

            var ext = Path.GetExtension(zipmodPath);
            if (!string.Equals(ext, ".zipmod", StringComparison.InvariantCultureIgnoreCase)
                && !string.Equals(ext, ".zip", StringComparison.InvariantCultureIgnoreCase))
            {
                return string.Empty;
            }

            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipmodPath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith("manifest.xml", StringComparison.OrdinalIgnoreCase))
                        {
                            return ManageXml.ReadXmlValue(entry.Open(), "manifest/guid");
                        }
                    }
                }
            }
            catch
            {
            }

            return string.Empty;
        }
    }
}
