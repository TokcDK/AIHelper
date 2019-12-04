using SymbolicLinkSupport;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AI_Helper.Utils
{
    class ManageFilesFolders
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
        public static bool CheckDirectoryNullOrEmpty_Fast(string path, string Mask = "*", string[] exclusions = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                return true; //return true if path is empty
                //throw new ArgumentNullException(path);
            }

            if (Directory.Exists(path))
            {
                if (path.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    path += Mask;
                }
                else
                {
                    path += Path.DirectorySeparatorChar + Mask;
                }

                var findHandle = FindFirstFile(path, out WIN32_FIND_DATA findData);

                if (findHandle != INVALID_HANDLE_VALUE)
                {
                    try
                    {
                        bool empty = true;
                        do
                        {
                            if (findData.cFileName != "." && findData.cFileName != ".." && !ManageStrings.IsStringContainsAnyExclusion(findData.cFileName, exclusions))
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
            else
            {
                return true;//return true if not exists
            }
            //throw new DirectoryNotFoundException();
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

            int CategoriesLength = Categories.Length / 3;
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
                    && !CheckDirectoryNullOrEmpty_Fast(dir)
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

        public static void DeleteEmptySubfolders(string dirPath, bool DeleteThisDir = true, string[] Exclusions = null)
        {
            if (Directory.Exists(dirPath))
            {
                string[] subfolders = Directory.GetDirectories(dirPath, "*");
                int subfoldersLength = subfolders.Length;
                if (subfoldersLength > 0)
                {
                    for (int d = 0; d < subfoldersLength; d++)
                    {
                        DeleteEmptySubfolders(subfolders[d], !TrueIfStringInExclusionsList(subfolders[d], Exclusions), Exclusions);
                    }
                }

                if (DeleteThisDir && CheckDirectoryNullOrEmpty_Fast(dirPath)/*Directory.GetDirectories(dataPath, "*").Length == 0 && Directory.GetFiles(dataPath, "*.*").Length == 0*/)
                {
                    Directory.Delete(dirPath);
                }
            }
        }

        public static StringBuilder GetEmptySubfoldersPaths(string dirPath, StringBuilder retArray)
        {
            string[] subfolders = Directory.GetDirectories(dirPath, "*");
            int subfoldersLength = subfolders.Length;
            if (subfoldersLength > 0)
            {
                for (int d = 0; d < subfoldersLength; d++)
                {
                    GetEmptySubfoldersPaths(subfolders[d], retArray);
                }
            }

            if (CheckDirectoryNullOrEmpty_Fast(dirPath)/*Directory.GetDirectories(dataPath, "*").Length == 0 && Directory.GetFiles(dataPath, "*.*").Length == 0*/)
            {
                return retArray.AppendLine(dirPath);
            }
            else
            {
                return retArray;
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
                    if (ManageStrings.IsStringAContainsStringB(Str, exclusions[i]))
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

        /// <summary>
        /// Проверки существования целевой папки и модификация имени на уникальное
        /// </summary>
        /// <param name="ParentFolderPath"></param>
        /// <param name="TargetFolderName"></param>
        /// <returns></returns>
        public static string GetResultTargetDirPathWithNameCheck(string ParentFolderPath, string TargetFolderName)
        {
            string ResultTargetDirPath = Path.Combine(ParentFolderPath, TargetFolderName);
            int i = 0;
            while (Directory.Exists(ResultTargetDirPath))
            {
                i++;
                ResultTargetDirPath = Path.Combine(ParentFolderPath, TargetFolderName + " (" + i + ")");
            }
            return ResultTargetDirPath;
        }

        public static string GetResultTargetFilePathWithNameCheck(string Folder, string Name, string Extension = ".*")
        {
            var ResultPath = Path.Combine(Folder, Name + (Extension.Substring(0, 1) != "." ? "." : string.Empty) + Extension);
            int i = 0;
            while (File.Exists(ResultPath))
            {
                i++;
                ResultPath = Path.Combine(Folder, Name + " (" + i + ")" + Extension);
            }
            return ResultPath;
        }

        public static string IsAnyFileWithSameExtensionContainsNameOfTheFile(string zipmoddirmodspath, string zipname, string extension)
        {
            if (Directory.Exists(zipmoddirmodspath))
            {
                foreach (var path in Directory.GetFiles(zipmoddirmodspath, extension))
                {
                    string name = Path.GetFileNameWithoutExtension(path);
                    if (ManageStrings.IsStringAContainsStringB(name, zipname))
                    {
                        return path;
                    }
                }
            }
            return string.Empty;
        }

        public static bool IsInTheFolderOnlyOneFolder(string folderPath)
        {
            int cnt = 1;
            foreach (var file in Directory.GetFiles(folderPath))
            {
                cnt--;
                if (cnt == 0)
                {
                    return false;
                }
            }
            cnt = 2;
            foreach (var dir in Directory.GetDirectories(folderPath))
            {
                cnt--;
                if (cnt == 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static string MoveFolderToOneLevelUpIfItAloneAndReturnMovedFolderPath(string folderPath)
        {
            if (IsInTheFolderOnlyOneFolder(folderPath))
            {
                foreach (var oDirSubdir in Directory.GetDirectories(folderPath))
                {
                    string oDirSubdirName = Path.GetFileName(oDirSubdir);
                    string folderParentDirPath = Path.GetDirectoryName(folderPath);
                    if (oDirSubdirName == Path.GetFileName(folderPath))
                    {
                        string newSubDirPath = GetResultTargetDirPathWithNameCheck(folderParentDirPath, oDirSubdirName);
                        Directory.Move(oDirSubdir, newSubDirPath);
                        Directory.Delete(folderPath);
                        Directory.Move(newSubDirPath, folderPath);
                    }
                    else
                    {
                        string newDirPath = Path.Combine(folderParentDirPath, oDirSubdirName);
                        Directory.Move(oDirSubdir, newDirPath);
                        Directory.Delete(folderPath);
                        return newDirPath;
                    }
                }
            }

            return folderPath;
        }
    }
}
