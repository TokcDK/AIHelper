using AIHelper.Manage;
using SevenZip;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;
using System.IO;
using System.Linq;

namespace AIHelper
{
    //https://stackoverflow.com/a/53896663
    //https://www.cupofdev.com/compress-files-7zip-csharp/
    class Compressor
    {
        //internal static void Compress(string sourceCodeFolder, string targetFolder)
        //{
        //    // Set source and target folders
        //    //string targetFolder = @"E:\CodeDumps";
        //    //string sourceCodeFolder = @"C:\Dev\Clients\cupofdev";
        //    Directory.CreateDirectory(targetFolder);

        //    string target7Zipfile = Path.Combine(targetFolder, Path.GetFileName(sourceCodeFolder) + ".7z");

        //    if (!File.Exists(target7Zipfile))
        //    {
        //        // Specify where 7z.dll DLL is located
        //        //var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Environment.Is64BitProcess ? "MO\\dlls" : "x86", "7z.dll");                
        //        string path = Get7ZdllPath();
        //        if (path == null)
        //        {
        //            return;//return if path was not found
        //        }
        //        SevenZipBase.SetLibraryPath(path);
        //        SevenZipCompressor sevenZipCompressor = new SevenZipCompressor
        //        {
        //            CompressionLevel = CompressionLevel.Ultra,
        //            CompressionMethod = CompressionMethod.Lzma2
        //        };

        //        // Compress the directory and save the file in a yyyyMMdd_project-files.7z format (eg. 20141024_project-files.7z
        //        sevenZipCompressor.CompressDirectory(sourceCodeFolder, target7Zipfile);
        //    }
        //}

        internal static void Decompress(string sourceFile, string targetFolder)
        {
            if (File.Exists(sourceFile))
            {
                //Specify where 7z.dll DLL is located
                string path = Get7ZdllPath();
                if (path == null)
                {
                    return;//return if path was not found
                }
                SevenZipBase.SetLibraryPath(path);

                //create dir if not exists
                if (!Directory.Exists(targetFolder))
                {
                    _ = Directory.CreateDirectory(targetFolder);
                }

                //http://qaru.site/questions/513888/how-would-i-use-sevenzipsharp-with-this-code
                //Extract archive to targetFolder
                using (SevenZipExtractor sevenZipDecompressor = new SevenZipExtractor(sourceFile))
                {
                    try
                    {
                        sevenZipDecompressor.ExtractArchive(targetFolder);
                    }
                    catch
                    {
                        //https://github.com/adamhathcock/sharpcompress/blob/master/USAGE.md
                        if (Path.GetExtension(sourceFile) == ".rar")
                        {
                            using (var archive = RarArchive.Open(sourceFile))
                            {
                                foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                                {
                                    entry.WriteToDirectory(targetFolder, new ExtractionOptions()
                                    {
                                        ExtractFullPath = true,
                                        Overwrite = true
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }

        private static string Get7ZdllPath()
        {
            string path = Path.Combine(ManageSettings.GetAppResDir(), "dlls", "x86", "7z.dll");
            if (File.Exists(path))
            {
            }
            else
            {
                path = "C:\\Program Files (x86)\\7-Zip\\7z.dll";
                if (File.Exists(path))
                {
                }
                else
                {
                    return null;
                }
            }
            return path;
        }
    }
}
