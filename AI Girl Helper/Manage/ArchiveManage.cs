using AI_Helper.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Helper.Manage
{
    class ArchiveManage
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
                    string targetDir = FilesFoldersManage.GetResultTargetFilePathWithNameCheck(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file), extension);
                    if (!Directory.Exists(targetDir))
                    {
                        try
                        {
                            Compressor.Decompress(file, targetDir);
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
    }
}
