﻿using SymbolicLinkSupport;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AI_Helper.Manage
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

        public static void Symlink(string objectFileDirPath, string symlinkPath, bool isRelative = false)
        {
            if (symlinkPath.Length > 0 && objectFileDirPath.Length > 0)
            {
                string parentDirPath = Path.GetDirectoryName(symlinkPath);
                if (Directory.Exists(parentDirPath))
                {
                }
                else
                {
                    Directory.CreateDirectory(parentDirPath);
                }

                if (File.Exists(objectFileDirPath) && !File.Exists(symlinkPath))
                {
                    FileInfoExtensions.CreateSymbolicLink(new FileInfo(objectFileDirPath), symlinkPath, isRelative);//new from NuGet package
                    //CreateSymlink.File(file, symlink); //old
                }
                else if (Directory.Exists(objectFileDirPath) && !Directory.Exists(symlinkPath))
                {
                    DirectoryInfoExtensions.CreateSymbolicLink(new DirectoryInfo(objectFileDirPath), symlinkPath, isRelative);//new from NuGet package
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

        public static string IfInTheFolderOnlyOneFolderGetItName(string folderPath)
        {
            string AloneFolderName = string.Empty;
            int cnt = 1;
            foreach (var file in Directory.GetFiles(folderPath))
            {
                cnt--;
                if (cnt == 0)
                {
                    return string.Empty;
                }
            }
            cnt = 2;
            foreach (var dir in Directory.GetDirectories(folderPath))
            {
                cnt--;
                if (cnt == 0)
                {
                    return string.Empty;
                }
                AloneFolderName = Path.GetFileName(dir);
            }

            return AloneFolderName;
        }

        public static string MoveFolderToOneLevelUpIfItAloneAndReturnMovedFolderPath(string folderPath)
        {
            string AloneFolderName = IfInTheFolderOnlyOneFolderGetItName(folderPath);
            if (AloneFolderName.Length>0)
            {
                if (!ManageStrings.IsStringAContainsAnyStringFromStringArray(AloneFolderName, ManageSettings.GetListOfExistsGames()[ManageSettings.GetCurrentGameIndex()].GetGameStandartFolderNames(), true))
                {
                    string folderPathName = Path.GetFileName(folderPath);
                    string folderParentDirPath = Path.GetDirectoryName(folderPath);
                    string oDirSubdir = Path.Combine(folderPath, AloneFolderName);
                    string newDirPath;
                    if (AloneFolderName == folderPathName)
                    {
                        newDirPath = GetResultTargetDirPathWithNameCheck(folderParentDirPath, AloneFolderName);
                        Directory.Move(oDirSubdir, newDirPath);
                        Directory.Delete(folderPath);
                        Directory.Move(newDirPath, folderPath);
                    }
                    else
                    {
                        newDirPath = Path.Combine(folderParentDirPath, AloneFolderName);
                        Directory.Move(oDirSubdir, newDirPath);
                        Directory.Delete(folderPath);
                        if (folderPathName.Length > AloneFolderName.Length)//если изначальное имя было длиннее, то переименовать имя субпапки на него
                        {
                            Directory.Move(newDirPath, folderPath);
                        }
                        else
                        {
                            return MoveFolderToOneLevelUpIfItAloneAndReturnMovedFolderPath(newDirPath);
                        }
                    }

                    //возвращение через эту же функцию, если вложенных одиночных подпапок было более одной
                    return MoveFolderToOneLevelUpIfItAloneAndReturnMovedFolderPath(folderPath);
                }
            }
            return folderPath;
        }
    }
}