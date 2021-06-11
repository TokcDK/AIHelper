using AI_Helper.Manage;
using Soft160.Data.Cryptography;
using SymbolicLinkSupport;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AIHelper.Manage
{
    internal static class ManageFilesFolders
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
        /// <summary>
        /// return true if path nul,aempty or not contains any dirs/files
        /// </summary>
        /// <param name="path"></param>
        /// <param name="Mask"></param>
        /// <param name="exclusions"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public static bool CheckDirectoryNullOrEmpty_Fast(string path, string Mask = "*", string[] exclusions = null, bool recursive = false, string nameMask = "")
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                return true; //return true if path is empty
                //throw new ArgumentNullException(path);
            }
            DirectoryInfo di;
            if ((di = new DirectoryInfo(path)).IsSymbolicLink() && !di.IsSymbolicLinkValid())
            {
                return true;
            }

            if (path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.InvariantCulture))
            {
                path += nameMask + Mask;
            }
            else
            {
                path += Path.DirectorySeparatorChar + nameMask + Mask;
            }

            var findHandle = FindFirstFile(path, out WIN32_FIND_DATA findData);

            if (findHandle != INVALID_HANDLE_VALUE)
            {
                try
                {
                    bool empty = true;
                    do
                    {
                        if (findData.cFileName != "."
                            && findData.cFileName != ".."
                            && !ManageStrings.IsStringContainsAnyExclusion(findData.cFileName, exclusions)
                            && ((recursive && IsDir(findData.dwFileAttributes) && !CheckDirectoryNullOrEmpty_Fast(path + Path.DirectorySeparatorChar + findData.cFileName + Path.DirectorySeparatorChar, Mask, exclusions, recursive))
                            || Mask.Length == 1 || findData.cFileName.EndsWith(Mask.Remove(0, 1), StringComparison.InvariantCultureIgnoreCase)
                            ))
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
            return true;

            //throw new Exception("Failed to get directory first file",
            //    Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
            //throw new DirectoryNotFoundException();
        }

        private static bool IsDir(uint dwFileAttributes)
        {
            return dwFileAttributes == 16;
        }

        /// <summary>
        /// true s ny file in dir
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="extension"></param>
        /// <param name="AllDirectories"></param>
        /// <returns></returns>
        public static bool IsAnyFileExistsInTheDir(string dirPath, string extension = ".*", bool AllDirectories = true, string nameMask = "")
        {
            if (dirPath.Length == 0)
            {
                return false;
            }

            if (extension.Length > 0 && extension[0] != '.')
            {
                extension = "." + extension;
            }

            if (!CheckDirectoryNullOrEmpty_Fast(dirPath, extension, null, AllDirectories, nameMask))
                return true;

            return false;
        }

        /// <summary>
        /// will delete empty folders
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="DeleteThisDir"></param>
        /// <param name="Exclusions"></param>
        public static void DeleteEmptySubfolders(string dirPath, bool DeleteThisDir = true, string[] Exclusions = null)
        {
            if (string.IsNullOrEmpty(dirPath) || !Directory.Exists(dirPath))
            {
                return;
            }

            DirectoryInfo dir = new DirectoryInfo(dirPath);
            try
            {
                if (dir.IsSymbolicLink() && !dir.IsSymbolicLinkValid())
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                ManageLogs.Log("dirPath=" + dirPath + Environment.NewLine + "Error:" + Environment.NewLine + ex);
                return;
            }

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
                dir.Attributes = FileAttributes.Normal;
                dir.Delete();
            }
        }

        /// <summary>
        /// will return list of empty folder paths
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="retArray"></param>
        /// <returns></returns>
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

        /// <summary>
        /// If input file\folder not exists it will create empty. Will return inputPath
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="IsFolder"></param>
        /// <returns></returns>
        internal static string GreateFileFolderIfNotExists(string inputPath, bool IsFolder = false)
        {
            if (IsFolder)
            {
                if (!Directory.Exists(inputPath))
                {
                    Directory.CreateDirectory(inputPath);
                }
            }
            else
            {
                if (!File.Exists(inputPath))
                {
                    File.WriteAllText(inputPath, string.Empty);
                }
            }

            return inputPath;

        }

        /// <summary>
        /// true if any value of string array contains string
        /// </summary>
        /// <param name="Str"></param>
        /// <param name="exclusions"></param>
        /// <returns></returns>
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

        /// <summary>
        /// wil move file but will delete id destination file is exists
        /// </summary>
        /// <param name="sourceFileName"></param>
        /// <param name="destFileName"></param>
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

        /// <summary>
        /// will change file/folder attribute to hidden
        /// </summary>
        /// <param name="path"></param>
        /// <param name="IsFile"></param>
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

        /// <summary>
        /// true if string is not empty and contains invalid symbols for file/folder path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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

        /// <summary>
        /// return new file path from source which is not exists in Folder
        /// return path will be with name like "Name (#).ext"
        /// </summary>
        /// <param name="Folder"></param>
        /// <param name="Name"></param>
        /// <param name="Extension"></param>
        /// <returns></returns>
        public static string GetResultTargetFilePathWithNameCheck(string Folder, string Name, string Extension = "")
        {
            var ResultPath = Path.Combine(Folder, Name + (Extension.Length > 0 && Extension.Substring(0, 1) != "." ? "." : string.Empty) + Extension);
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

        /// <summary>
        /// If In The Folder Only One Folder Get it Name
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Move Folder To One Level Up If It Alone And Return Moved Folder Path
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static string MoveFolderToOneLevelUpIfItAloneAndReturnMovedFolderPath(string folderPath)
        {
            string AloneFolderName = IfInTheFolderOnlyOneFolderGetItName(folderPath);
            if (AloneFolderName.Length > 0)
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

        /// <summary>
        /// Move content Of Source Folder To Target Folder And Then Clean Source
        /// </summary>
        /// <param name="SourceFolder"></param>
        /// <param name="TargetFolder"></param>
        internal static void MoveContent(string SourceFolder, string TargetFolder)
        {
            if (Directory.Exists(SourceFolder))
            {
                if (!CheckDirectoryNullOrEmpty_Fast(SourceFolder) && !ManageSymLinks.IsSymLink(SourceFolder))
                {
                    if (!Directory.Exists(TargetFolder))
                    {
                        Directory.CreateDirectory(TargetFolder);
                    }

                    foreach (string dir in Directory.EnumerateDirectories(SourceFolder))
                    {
                        var targetDir = Path.Combine(TargetFolder, Path.GetFileName(dir));
                        if (Directory.Exists(targetDir))
                        {
                            targetDir = GetNewNewWithCurrentDate(targetDir, false);
                        }

                        Directory.Move(dir, targetDir);
                    }
                    foreach (string file in Directory.EnumerateFiles(SourceFolder))
                    {
                        var sourceFile = file;
                        var targetFile = Path.Combine(TargetFolder, Path.GetFileName(sourceFile));

                        try
                        {
                            if (File.Exists(targetFile))
                            {
                                FileInfo sFile;
                                FileInfo tFile;
                                if ((sFile = new FileInfo(sourceFile)).LastWriteTime > (tFile = new FileInfo(targetFile)).LastWriteTime)
                                {
                                    File.Move(targetFile, GetNewNewWithCurrentDate(targetFile));
                                }
                                else if (sFile.LastWriteTime == tFile.LastWriteTime
                                    && sFile.Length == tFile.Length
                                    )
                                {
                                    File.Delete(targetFile);
                                }
                                else
                                {
                                    targetFile = GetNewNewWithCurrentDate(targetFile);
                                }
                            }

                            File.Move(sourceFile, targetFile);
                        }
                        catch (IOException ex)
                        {
                            ManageLogs.Log("Error in MoveContentOfSourceFolderToTargetFolderAndThenCleanSource:" + Environment.NewLine + "sourceFilePath=" + sourceFile + Environment.NewLine + "targetFilePath=" + targetFile + Environment.NewLine + ex);
                        }
                    }
                }

                DeleteEmptySubfolders(SourceFolder, true);
            }
        }

        /// <summary>
        /// return file name + current date in format "yyyy_MM_dd_HH_mm_ss"
        /// </summary>
        /// <param name="target"></param>
        /// <param name="isFile"></param>
        /// <returns></returns>
        private static string GetNewNewWithCurrentDate(string target, bool isFile = true)
        {
            if (isFile)
            {
                return Path.Combine(Path.GetDirectoryName(target)
                     + Path.GetFileNameWithoutExtension(target)
                     + "_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss", CultureInfo.InvariantCulture)
                     + Path.GetExtension(target)
                    );
            }
            else
            {
                return target + "_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss", CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Get crc32 of file
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        internal static string GetCrc32(this string FilePath)
        {
            return GetCrc32(new FileInfo(FilePath));

        }

        /// <summary>
        /// Get crc32 of file
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        internal static string GetCrc32(this FileInfo FilePath)
        {
            //https://stackoverflow.com/a/57450238
            using (var crc32 = new CRCServiceProvider())
            {
                string hash = string.Empty;
                using (var fs = FilePath.Open(FileMode.Open))
                {
                    var array = crc32.ComputeHash(fs);
                    var arrayLength = array.Length;
                    for (int i = 0; i < arrayLength; i++)
                    {
                        hash += array[i].ToString("x2", CultureInfo.InvariantCulture)/*.ToLowerInvariant()*/;
                    }
                }

                return hash;
            }

        }

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteFile(string name);

        /// <summary>
        /// must unlock locked file and delete after?
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool Unblock(string fileName)
        {
            return DeleteFile(fileName + ":Zone.Identifier");
        }

        /// <summary>
        /// true if file is picture
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        internal static bool IsPicture(this string filePath)
        {
            return IsPictureExtension(Path.GetExtension(filePath));
        }

        /// <summary>
        /// true if file is picture
        /// </summary>
        /// <param name="FileExtension"></param>
        /// <returns></returns>
        internal static bool IsPictureExtension(this string FileExtension)
        {
            return !string.IsNullOrWhiteSpace(FileExtension) && (FileExtension == ".jpg" || FileExtension == ".png" || FileExtension == ".bmp");
        }

        /// <summary>
        /// delete file even if it is read only
        /// </summary>
        /// <param name="file"></param>
        internal static void DeleteEvenIfReadOnly(this FileInfo file)
        {
            if (!file.Exists)
            {
                return;
            }

            file.Attributes = FileAttributes.Normal;
            file.Delete();
        }
    }
}
