using SymbolicLinkSupport;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace AI_Girl_Helper.Utils
{
    class FileFolderOperations
    {
        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct WIN32_FIND_DATA
        {
            public uint dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll")]
        private static extern bool FindClose(IntPtr hFindFile);

        //https://stackoverflow.com/a/757925
        //быстро проверить, пуста ли папка
        public static bool CheckDirectoryEmpty_Fast(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(path);
            }

            if (Directory.Exists(path))
            {
                if (path.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    path += "*";
                }
                else
                {
                    path += Path.DirectorySeparatorChar + "*";
                }

                var findHandle = FindFirstFile(path, out WIN32_FIND_DATA findData);

                if (findHandle != INVALID_HANDLE_VALUE)
                {
                    try
                    {
                        bool empty = true;
                        do
                        {
                            if (findData.cFileName != "." && findData.cFileName != "..")
                            {
                                empty = false;
                            }
                        } while (empty && FindNextFile(findHandle, out findData));

                        return empty;
                    }
                    finally
                    {
                        FindClose(findHandle);
                    }
                }

                throw new Exception("Failed to get directory first file",
                    Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
            }
            throw new DirectoryNotFoundException();
        }

        public static string GetCategoriesForTheFolder(string moddir, string category)
        {
            string Category = category;

            string[,] Categories =
            {
                { Path.Combine(moddir, "BepInEx", "Plugins"), "51", "dll" } //Plug-ins
                ,
                { Path.Combine(moddir, "UserData"), "53", "*" } //UserFiles
                ,
                { Path.Combine(moddir, "UserData", "chara"), "54", "png" } //Characters
                ,
                { Path.Combine(moddir, "UserData", "studio", "scene"), "55", "png"} //Studio scenes
                ,
                { Path.Combine(moddir, "Mods"), "60", "zip" } //Sideloader
                ,
                { Path.Combine(moddir, "scripts"), "86", "cs"} //ScriptLoader scripts
                ,
                { Path.Combine(moddir, "UserData", "coordinate"), "87", "png"} //Coordinate
                ,
                { Path.Combine(moddir, "UserData", "Overlays"), "88", "png"} //Overlay
                ,
                { Path.Combine(moddir, "UserData", "housing"), "89", "png"} //Housing
                ,
                { Path.Combine(moddir, "UserData", "housing"), "90", "png"} //Cardframe
            };

            int CategoriesLength = Categories.Length/3;
            for (int i = 0; i < CategoriesLength; i++)
            {
                string dir = Categories[i, 0];
                string categorieNum = Categories[i, 1];
                string extension = Categories[i, 2];
                if (
                    (
                        (category.Length > 0
                        && !category.Contains("," + categorieNum)
                        && !category.Contains(categorieNum + ",")
                        )
                     || category.Length == 0
                    )
                    && Directory.Exists(dir)
                    && !CheckDirectoryEmpty_Fast(dir)
                    && IsAnyFileExistsInTheDir(dir, extension)
                   )
                {
                    if (Category.Length > 0)
                    {
                        if (Category.Substring(Category.Length - 1, 1) == ",")
                        {
                            Category += categorieNum;
                        }
                        else
                        {
                            Category += "," + categorieNum;
                        }
                    }
                    else
                    {
                        Category = categorieNum + ",";
                    }
                }
            }

            return Category;
        }

        public static bool IsAnyFileExistsInTheDir(string dirPath, string extension)
        {
            foreach (var file in (Directory.GetFiles(dirPath, "*." + extension, SearchOption.AllDirectories)))
            {
                return true;
            }
            return false;
        }

        public static void DeleteEmptySubfolders(string dataPath, bool DeleteThisDir = true, string[] Exclusions = null)
        {
            string[] subfolders = Directory.GetDirectories(dataPath, "*");
            int subfoldersLength = subfolders.Length;
            if (subfoldersLength > 0)
            {
                for (int d = 0; d < subfoldersLength; d++)
                {
                    DeleteEmptySubfolders(subfolders[d], !TrueIfStringInExclusionsList(subfolders[d], Exclusions), Exclusions);
                }
            }

            if (DeleteThisDir && Directory.GetDirectories(dataPath, "*").Length == 0 && Directory.GetFiles(dataPath, "*.*").Length == 0)
            {
                Directory.Delete(dataPath);
            }
        }

        public static bool TrueIfStringInExclusionsList(string Str, string[] exclusions)
        {
            if (exclusions == null || Str.Length == 0)
            {
                return false;
            }
            else
            {
                int exclusionsLength = exclusions.Length;
                for (int i = 0; i < exclusionsLength; i++)
                {
                    if (string.IsNullOrWhiteSpace(exclusions[i]))
                    {
                        continue;
                    }
                    if (StringEx.IsStringAContainsStringB(Str, exclusions[i]))
                    {
                        return true;
                    }
                }

            }
            return false;
        }

        public static void MoveFileWithReplace(string sourceFileName, string destFileName)
        {

            //first, delete target file if exists, as File.Move() does not support overwrite
            if (File.Exists(destFileName))
            {
                File.Delete(destFileName);
            }

            string destFolder = Path.GetDirectoryName(destFileName);
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }
            File.Move(sourceFileName, destFileName);

        }

        public static void HideFileFolder(string path, bool IsFile = false)
        {
            if (IsFile)
            {
                if (File.Exists(path))
                {
                    File.SetAttributes(path, FileAttributes.Hidden);
                }
            }
            else
            {
                if (Directory.Exists(path))
                {
                    _ = new DirectoryInfo(path)
                    {
                        Attributes = FileAttributes.Hidden
                    };
                }
            }

        }

        public static void Symlink(string objectFileDir, string symlink, bool isRelative = false)
        {
            if (File.Exists(symlink))
            {
            }
            else
            {
                string parentdirpath = Path.GetDirectoryName(symlink);
                if (Directory.Exists(parentdirpath))
                {
                }
                else
                {
                    Directory.CreateDirectory(parentdirpath);
                }
                if (File.Exists(objectFileDir))
                {
                    FileInfoExtensions.CreateSymbolicLink(new FileInfo(objectFileDir), symlink, isRelative);//new from NuGet package
                    //CreateSymlink.File(file, symlink); //old
                }
                else if (Directory.Exists(objectFileDir))
                {
                    DirectoryInfoExtensions.CreateSymbolicLink(new DirectoryInfo(objectFileDir), symlink, isRelative);//new from NuGet package
                    //CreateSymlink.Folder(file, symlink); //old
                }
            }
        }

        public static void DeleteIfSymlink(string LinkPath, bool IsFolder = false)
        {
            if (IsFolder)
            {
                if (Directory.Exists(LinkPath))
                {
                    if (FileInfoExtensions.IsSymbolicLink(new FileInfo(LinkPath)))
                    {
                        Directory.Delete(LinkPath, true);
                    }
                }
            }
            else
            {
                if (File.Exists(LinkPath))
                {
                    if (FileInfoExtensions.IsSymbolicLink(new FileInfo(LinkPath)))
                    {
                        File.Delete(LinkPath);
                    }
                }
            }
        }

        public static bool ContainsAnyInvalidCharacters(string path)
        {
            return (path.Length > 0 && path.IndexOfAny(Path.GetInvalidPathChars()) >= 0);
        }
    }
}
