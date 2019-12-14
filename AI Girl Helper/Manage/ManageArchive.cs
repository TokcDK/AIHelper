using AIHelper.Manage;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage
{
    class ManageArchive
    {
        public static void UnpackArchivesToSubfoldersWithSameName(string dirForSearch, string extension)
        {
            if (dirForSearch.Length == 0 || extension.Length == 0)
            {
                return;
            }

            if (extension.Substring(0, 1) != ".")
            {
                extension = "." + extension;
            }

            if (dirForSearch.Length > 0 && extension.Length > 0 && Directory.Exists(dirForSearch))
            {
                foreach (var file in Directory.GetFiles(dirForSearch, "*" + extension, SearchOption.AllDirectories))
                {
                    string targetDir = ManageFilesFolders.GetResultTargetDirPathWithNameCheck(dirForSearch, Path.GetFileNameWithoutExtension(file));
                    if (!Directory.Exists(targetDir))
                    {
                        try
                        {
                            if (extension.EndsWith("zip"))
                            {
                                using (ZipArchive archive = ZipFile.OpenRead(file))
                                {
                                    archive.ExtractToDirectory(targetDir);
                                }
                            }
                            else
                            {
                                Compressor.Decompress(file, targetDir);
                            }
                            //File.Delete(file);
                            File.Move(file, file + ".ExtractedAndShouldHaveBeenInstalled");
                        }
                        catch
                        {
                            //if decompression failed
                        }
                    }
                }
            }
        }
        
        // Number of files within zip archive
        public static int GetFilesCountInZipArchive(ZipArchive InputArchive=null, string zipFilePath="")
        {
            int count = 0;

            if (InputArchive != null)
            {
                // We count only named (i.e. that are with files) entries
                foreach (var entry in InputArchive.Entries)
                {
                    if (!string.IsNullOrEmpty(entry.Name))
                    {
                        count++;
                    }
                }
            }
            else if (zipFilePath.Length>0)
            {
                using (ZipArchive archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Read))
                {
                    // We count only named (i.e. that are with files) entries
                    foreach (var entry in archive.Entries)
                    {
                        if (!string.IsNullOrEmpty(entry.Name))
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }
    }
}
