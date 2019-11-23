using System;
using System.IO;
using System.Reflection;
using SevenZip;

namespace AI_Helper
{
    //https://stackoverflow.com/a/53896663
    //https://www.cupofdev.com/compress-files-7zip-csharp/
    class Compressor
    {
        internal static void Compress(string sourceCodeFolder, string targetFolder)
        {
            // Set source and target folders
            //string targetFolder = @"E:\CodeDumps";
            //string sourceCodeFolder = @"C:\Dev\Clients\cupofdev";
            if (Directory.Exists(targetFolder))
            {
                string target7zipfile = Path.Combine(targetFolder, Path.GetFileName(sourceCodeFolder) + ".7z");

                if (!File.Exists(target7zipfile))
                {
                    // Specify where 7z.dll DLL is located
                    //var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Environment.Is64BitProcess ? "MO\\dlls" : "x86", "7z.dll");                
                    string path = Get7zdllPath();
                    if (path == null)
                    {
                        return;//return if path was not found
                    }
                    SevenZipBase.SetLibraryPath(path);
                    SevenZipCompressor sevenZipCompressor = new SevenZipCompressor
                    {
                        CompressionLevel = CompressionLevel.Ultra,
                        CompressionMethod = CompressionMethod.Lzma2
                    };

                    // Compress the directory and save the file in a yyyyMMdd_project-files.7z format (eg. 20141024_project-files.7z
                    sevenZipCompressor.CompressDirectory(sourceCodeFolder, target7zipfile);
                }
            }
        }

        internal static void Decompress(string sourceFile, string targetFolder)
        {
            if (File.Exists(sourceFile))
            {
                //Specify where 7z.dll DLL is located
                string path = Get7zdllPath();
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
                    sevenZipDecompressor.ExtractArchive(targetFolder);
                }
            }
        }

        private static string Get7zdllPath()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "AI Girl Helper_RES", "dlls", "7z.dll");
            if (File.Exists(path))
            {
            }
            else
            {
                path = Path.Combine("C:", "Program Files", "7-Zip", "7z.dll");
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
