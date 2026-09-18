using AIHelper.Manage;
using System;
using System.IO;
using System.IO.Compression;

namespace AIHelper.Install.Types.Files.Archive.Extractors
{
    /// <summary>
    /// Extracts archives to the subfolders of the install dir
    /// </summary>
    abstract class ArchiveExtractorBase : ArchiveInstallerBase
    {
        public override int Order => base.Order / 2;

        protected override bool Get(FileInfo fileInfo)
        {
            string targetDir = ManageFilesFoldersExtensions.GetResultTargetDirPathWithNameCheck(ManageSettings.Install2MoDirPath, Path.GetFileNameWithoutExtension(fileInfo.Name));

            try
            {
                if (fileInfo.Extension.EndsWith("zip", StringComparison.InvariantCultureIgnoreCase))
                {
                    using (ZipArchive archive = ZipFile.OpenRead(fileInfo.FullName))
                    {
                        archive.ExtractToDirectory(targetDir);
                    }
                }
                else
                {
                    Compressor.Decompress(fileInfo.FullName, targetDir);
                }
                //File.Delete(file);
                fileInfo.MoveTo(fileInfo.FullName + ".Extracted" + ManageSettings.DateTimeBasedSuffix);
            }
            catch (Exception ex)
            {
                _log.Warn("Failed to extract archive. path:" + fileInfo.FullName + "\r\nError:" + ex);
                //if decompression failed
                return false;
            }

            return true;
        }
    }
}
