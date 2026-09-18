using NLog;
using System;
using System.IO;
using System.IO.Compression;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;

namespace AIHelper.Manage
{
    class ManageArchive
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private const string ZipExtension = ".zip";

        public static void UnpackArchivesToSubfoldersWithSameName(string dirForSearch, string extension)
        {
            if (string.IsNullOrWhiteSpace(dirForSearch) || string.IsNullOrWhiteSpace(extension)) return;

            if (extension[0] != '.') extension = "." + extension;

            if (!Directory.Exists(dirForSearch)) return;

            foreach (var file in Directory.GetFiles(dirForSearch, "*" + extension, SearchOption.AllDirectories))
            {
                string targetDir = ManageFilesFoldersExtensions.GetResultTargetDirPathWithNameCheck(dirForSearch, Path.GetFileNameWithoutExtension(file));
                if (Directory.Exists(targetDir)) continue;

                try
                {
                    if (string.Equals(extension, ZipExtension, StringComparison.InvariantCultureIgnoreCase))
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
                catch (Exception ex)
                {
                    //if decompression failed
                    _log.Warn("Failed to unpack the archive " + file + ". Error:\r\n" + ex.Message);
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
            else if (!string.IsNullOrWhiteSpace(zipFilePath))
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
                && !string.Equals(ext, ZipExtension, StringComparison.InvariantCultureIgnoreCase))
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
            catch (Exception ex)
            {
                _log.Warn("Failed to read zipmod guid from " + zipmodPath + ". Error:\r\n" + ex.Message);
            }

            return string.Empty;
        }

        internal static void Extract7zipTo(string archiveFilePath, string dirToExtract)
        {
            if (!SevenZipArchive.IsSevenZipFile(archiveFilePath)) return;

            using (var archive = SevenZipArchive.OpenArchive(archiveFilePath))
            {
                foreach (var entry in archive.Entries)
                {
                    entry.WriteToDirectory(dirToExtract, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }
        }

        internal static void ExtractRarTo(string archiveFilePath, string dirToExtract)
        {
            if (!RarArchive.IsRarFile(archiveFilePath)) return;

            using (var archive = RarArchive.OpenArchive(archiveFilePath))
            {
                foreach (var entry in archive.Entries)
                {
                    entry.WriteToDirectory(dirToExtract, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }
        }
    }
}
