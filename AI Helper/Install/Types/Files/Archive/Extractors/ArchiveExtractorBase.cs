using AIHelper.Manage;
using System;
using System.IO;
using System.IO.Compression;

namespace AIHelper.Install.Types.Files.Archive.Extractors
{
    abstract class ArchiveExtractorBase : ArchiveInstallerBase
    {
        public override int Order => base.Order / 2;

        protected override bool Get(FileInfo fileInfo)
        {
            string targetDir = ManageFilesFoldersExtensions.GetResultTargetDirPathWithNameCheck(ManageSettings.GetInstall2MoDirPath(), Path.GetFileNameWithoutExtension(fileInfo.Name));
            if (Directory.Exists(targetDir))
            {
                return false;
            }

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
                fileInfo.MoveTo(fileInfo.FullName + ".ExtractedAndShouldHaveBeenInstalled");
            }
            catch
            {
                return false;
                //if decompression failed
            }

            return true;
        }
    }
}
