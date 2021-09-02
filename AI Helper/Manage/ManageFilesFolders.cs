using AIHelper.SharedData;
using CheckForEmptyDir;
using Soft160.Data.Cryptography;

using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AIHelper.Manage
{
    internal static class ManageFilesFolders
    {
        /// <summary>
        /// object (file or folder or any)
        /// </summary>
        public enum ObjectType
        {
            Any = 0,
            File = 1,
            Directory = 2
        }

        /// <summary>
        /// true s ny file in dir
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="Mask"></param>
        /// <param name="allDirectories"></param>
        /// <returns></returns>
        public static bool IsAnyFileExistsInTheDir(string dirPath, string Mask = "*", bool allDirectories = true)
        {
            return !dirPath.IsNullOrEmptyDirectory(mask: Mask, searchForFiles: true, searchForDirs: false, recursive: allDirectories);
        }

        /// <summary>
        /// true s ny file in dir
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="Mask"></param>
        /// <param name="allDirectories"></param>
        /// <returns></returns>
        public static bool IsAnySubDirExistsInTheDir(string dirPath, string Mask = "*")
        {
            return !dirPath.IsNullOrEmptyDirectory(mask: Mask, searchForDirs: true, searchForFiles: false, recursive: false);
        }

        /// <summary>
        /// will delete empty folders
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="deleteThisDir"></param>
        /// <param name="exclusions"></param>
        public static void DeleteEmptySubfolders(string dirPath, bool deleteThisDir = true, string[] exclusions = null)
        {
            if (string.IsNullOrEmpty(dirPath) || !Directory.Exists(dirPath))
            {
                return;
            }

            DirectoryInfo dir = new DirectoryInfo(dirPath);

            try
            {
                if (dir.IsSymlink())
                {
                    if (!dir.IsValidSymlink())
                    {
                        dir.Delete(); // remove invalid symlink
                        return;
                    }
                    else
                    {
                        dirPath = Path.GetFullPath(dir.GetSymlinkTarget());
                    }
                }
            }
            catch (Exception ex)
            {
                ManageLogs.Log("dirPath=" + dirPath + Environment.NewLine + "Error:" + Environment.NewLine + ex);
                return;
            }

            // NOTE
            // here is exist problem when for example from disc D: was created symlink for dir "Data" on disc C:
            // and inside Data dir was another symlink with relative path. And even if relative path is working
            // and Directory.Exists will be true the link can be not able to be opened with explorer or will throw
            // exception for methods like Directory.GetDirectories
            string[] subfolders;
            try
            {
                subfolders = Directory.GetDirectories(dirPath, "*");
                int subfoldersLength = subfolders.Length;
                for (int d = 0; d < subfoldersLength; d++)
                {
                    DeleteEmptySubfolders(subfolders[d], !TrueIfStringInExclusionsList(subfolders[d], exclusions), exclusions);
                }
            }
            catch
            {

            }

            if (deleteThisDir && dir.IsNullOrEmptyDirectory(recursive: false)/*Directory.GetDirectories(dataPath, "*").Length == 0 && Directory.GetFiles(dataPath, "*.*").Length == 0*/)
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

            if (dirPath.IsNullOrEmptyDirectory()/*Directory.GetDirectories(dataPath, "*").Length == 0 && Directory.GetFiles(dataPath, "*.*").Length == 0*/)
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
        /// <param name="isFolder"></param>
        /// <returns></returns>
        internal static string GreateFileFolderIfNotExists(string inputPath, bool isFolder = false)
        {
            if (isFolder)
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
        /// <param name="str"></param>
        /// <param name="exclusions"></param>
        /// <returns></returns>
        public static bool TrueIfStringInExclusionsList(string str, string[] exclusions)
        {
            if (exclusions == null || str.Length == 0)
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
                    if (ManageStrings.IsStringAContainsStringB(str, exclusions[i]))
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
        /// <param name="isFile"></param>
        public static void HideFileFolder(string path, bool isFile = false)
        {
            if (isFile)
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
        /// <param name="parentFolderPath"></param>
        /// <param name="targetFolderName"></param>
        /// <returns></returns>
        public static string GetResultTargetDirPathWithNameCheck(string parentFolderPath, string targetFolderName)
        {
            string resultTargetDirPath = Path.Combine(parentFolderPath, targetFolderName);
            int i = 0;
            while (Directory.Exists(resultTargetDirPath))
            {
                i++;
                resultTargetDirPath = Path.Combine(parentFolderPath, targetFolderName + " (" + i + ")");
            }
            return resultTargetDirPath;
        }

        /// <summary>
        /// return new file path from source which is not exists in Folder
        /// return path will be with name like "Name (#).ext"
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="name"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GetResultTargetFilePathWithNameCheck(string folder, string name, string extension = "")
        {
            var resultPath = Path.Combine(folder, name + (extension.Length > 0 && extension.Substring(0, 1) != "." ? "." : string.Empty) + extension);
            int i = 0;
            while (File.Exists(resultPath))
            {
                i++;
                resultPath = Path.Combine(folder, name + " (" + i + ")" + extension);
            }
            return resultPath;
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
            string aloneFolderName = string.Empty;
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
                aloneFolderName = Path.GetFileName(dir);
            }

            return aloneFolderName;
        }

        /// <summary>
        /// Move Folder To One Level Up If It Alone And Return Moved Folder Path
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static string MoveFolderToOneLevelUpIfItAloneAndReturnMovedFolderPath(string folderPath)
        {
            string aloneFolderName = IfInTheFolderOnlyOneFolderGetItName(folderPath);
            if (aloneFolderName.Length > 0)
            {
                if (!ManageStrings.IsStringAContainsAnyStringFromStringArray(aloneFolderName, GameData.CurrentGame.GetGameStandartFolderNames(), true))
                {
                    string folderPathName = Path.GetFileName(folderPath);
                    string folderParentDirPath = Path.GetDirectoryName(folderPath);
                    string oDirSubdir = Path.Combine(folderPath, aloneFolderName);
                    string newDirPath;
                    if (aloneFolderName == folderPathName)
                    {
                        newDirPath = GetResultTargetDirPathWithNameCheck(folderParentDirPath, aloneFolderName);
                        Directory.Move(oDirSubdir, newDirPath);
                        Directory.Delete(folderPath);
                        Directory.Move(newDirPath, folderPath);
                    }
                    else
                    {
                        newDirPath = Path.Combine(folderParentDirPath, aloneFolderName);
                        Directory.Move(oDirSubdir, newDirPath);
                        Directory.Delete(folderPath);
                        if (folderPathName.Length > aloneFolderName.Length)//если изначальное имя было длиннее, то переименовать имя субпапки на него
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
        /// <param name="sourceFolder"></param>
        /// <param name="targetFolder"></param>
        internal static void MoveContent(string sourceFolder, string targetFolder)
        {
            if (Directory.Exists(sourceFolder))
            {
                if (!sourceFolder.IsNullOrEmptyDirectory() && !ManageSymLinkExtensions.IsSymLink(sourceFolder))
                {
                    if (!Directory.Exists(targetFolder))
                    {
                        Directory.CreateDirectory(targetFolder);
                    }

                    foreach (string dir in Directory.EnumerateDirectories(sourceFolder))
                    {
                        var targetDir = Path.Combine(targetFolder, Path.GetFileName(dir));
                        if (Directory.Exists(targetDir))
                        {
                            targetDir = GetNewNewWithCurrentDate(targetDir, false);
                        }

                        Directory.Move(dir, targetDir);
                    }
                    foreach (string file in Directory.EnumerateFiles(sourceFolder))
                    {
                        var sourceFile = file;
                        var targetFile = Path.Combine(targetFolder, Path.GetFileName(sourceFile));

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

                DeleteEmptySubfolders(sourceFolder, true);
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
        /// <param name="filePath"></param>
        /// <returns></returns>
        internal static string GetCrc32(this string filePath)
        {
            return GetCrc32(new FileInfo(filePath));

        }

        /// <summary>
        /// Get crc32 of file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        internal static string GetCrc32(this FileInfo filePath)
        {
            //https://stackoverflow.com/a/57450238
            using (var crc32 = new CRCServiceProvider())
            {
                string hash = string.Empty;
                using (var fs = filePath.Open(FileMode.Open))
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
        /// <param name="fileExtension"></param>
        /// <returns></returns>
        internal static bool IsPictureExtension(this string fileExtension)
        {
            return !string.IsNullOrWhiteSpace(fileExtension) && (fileExtension == ".jpg" || fileExtension == ".png" || fileExtension == ".bmp");
        }

        /// <summary>
        /// delete file even if it is read only
        /// </summary>
        /// <param name="file"></param>
        internal static void DeleteReadOnly(this FileInfo file)
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
