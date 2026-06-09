using AIHelper.SharedData;
using CheckForEmptyDir;
using NLog;
using Soft160.Data.Cryptography;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage
{
    static class ManageFilesFoldersExtensions
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// if directory exist in the <paramref name="path"/>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsDirectory(this string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>
        /// Move <paramref name="sourcePath"/> to <paramref name="targetPath"/>.
        /// If <paramref name="IsSymlink"/> is true or <paramref name="sourcePath"/> is symlink then will recreate symlink in target path
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        /// <param name="IsSymlink"></param>
        /// <returns></returns>
        public static void MoveTo(this string sourcePath, string targetPath, bool IsSymlink = false, ObjectType objectType = ObjectType.File)
        {
            if (IsSymlink || sourcePath.IsSymlink(objectType))
            {
                if (sourcePath.IsValidSymlink(objectType: objectType))
                {
                    var symlinkTarget = Path.GetFullPath(sourcePath.GetSymlinkTarget());

                    symlinkTarget.CreateSymlink(targetPath, isRelative: true, objectType: objectType);
                }

                if (objectType == ObjectType.File)
                {
                    File.Delete(sourcePath);
                }
                else
                {
                    Directory.Delete(sourcePath);
                }
            }
            else
            {

                if (objectType == ObjectType.File)
                {
                    File.Move(sourcePath, targetPath);//перенос файла из папки Overwrite в Data
                }
                else
                {
                    Directory.Move(sourcePath, targetPath);
                }
            }
        }

        /// <summary>
        /// true when any file in dir
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="Mask"></param>
        /// <param name="allDirectories"></param>
        /// <returns></returns>
        public static bool IsAnyFileExistsInTheDir(this string dirPath, string Mask = "*", bool allDirectories = true)
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
        public static bool IsAnySubDirExistsInTheDir(this string dirPath, string Mask = "*")
        {
            return !dirPath.IsNullOrEmptyDirectory(mask: Mask, searchForDirs: true, searchForFiles: false, recursive: false);
        }

        /// <summary>
        /// check if dir is empty. No any dirs or files inside.
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="Mask"></param>
        /// <param name="allDirectories"></param>
        /// <returns></returns>
        public static bool IsEmptyDir(this string dirPath)
        {
            return dirPath.IsNullOrEmptyDirectory(mask: "*", searchForDirs: true, searchForFiles: true, recursive: false);
        }
        /// <summary>
        /// check if dir is empty. No any dirs or files inside.
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="Mask"></param>
        /// <param name="allDirectories"></param>
        /// <returns></returns>
        public static bool IsEmpty(this DirectoryInfo dirPath)
        {
            return dirPath.FullName.IsEmptyDir();
        }

        /// <summary>
        /// will delete empty folders
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="deleteThisDir"></param>
        /// <param name="exclusions"></param>
        public static void DeleteEmptySubfolders(string dirPath, bool deleteThisDir, string[] exclusions)
        {
            DeleteEmptySubfolders(dirPath, deleteThisDir, exclusions.ToHashSet(), false);
        }

        /// <summary>
        /// will delete empty folders
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="deleteThisDir"></param>
        /// <param name="exclusions"></param>
        public static void DeleteEmptySubfolders(string dirPath, bool deleteThisDir = true, HashSet<string> exclusions = null, bool notUseExclusions = true)
        {
            if (string.IsNullOrEmpty(dirPath) || !Directory.Exists(dirPath)) return;

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
                _log.Error("dirPath=" + dirPath + Environment.NewLine + "Error:" + Environment.NewLine + ex);
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
                    DeleteEmptySubfolders(subfolders[d], !IsInExclusionsList(subfolders[d], exclusions, notUseExclusions), exclusions, notUseExclusions);
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
        /// get list of empty folder paths from <paramref name="dirPath"/> in <paramref name="pathsList"/>
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="pathsList"></param>
        /// <returns></returns>
        public static void GetEmptySubfoldersPaths(string dirPath, StringBuilder pathsList)
        {
            if (!Directory.Exists(dirPath)) return;

            if (pathsList == null) pathsList = new StringBuilder();

            string[] subfolders = Directory.GetDirectories(dirPath, "*");
            int subfoldersLength = subfolders.Length;
            if (subfoldersLength > 0)
            {
                for (int d = 0; d < subfoldersLength; d++)
                {
                    GetEmptySubfoldersPaths(subfolders[d], pathsList);
                }
            }

            if (dirPath.IsNullOrEmptyDirectory()) pathsList.AppendLine(dirPath);///*Directory.GetDirectories(dataPath, "*").Length == 0 && Directory.GetFiles(dataPath, "*.*").Length == 0*/)
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
                if (!Directory.Exists(inputPath)) Directory.CreateDirectory(inputPath);
            }
            else
            {
                if (!File.Exists(inputPath)) File.WriteAllText(inputPath, string.Empty);
            }

            return inputPath;

        }

        /// <summary>
        /// true if any value of string array contains string
        /// </summary>
        /// <param name="str"></param>
        /// <param name="exclusions"></param>
        /// <returns></returns>
        public static bool IsInExclusionsList(string str, HashSet<string> exclusions, bool notUseExclusions)
        {
            return notUseExclusions || exclusions == null || string.IsNullOrEmpty(str) ? false : exclusions.Contains(str);
        }

        /// <summary>
        /// wil move file but will delete id destination file is exists
        /// </summary>
        /// <param name="sourceFileName"></param>
        /// <param name="destFileName"></param>
        public static void MoveFileWithReplace(string sourceFileName, string destFileName)
        {

            //first, delete target file if exists, as File.Move() does not support overwrite
            if (File.Exists(destFileName)) File.Delete(destFileName);

            string destFolder = Path.GetDirectoryName(destFileName);
            if (!Directory.Exists(destFolder)) Directory.CreateDirectory(destFolder);
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
                if (File.Exists(path)) File.SetAttributes(path, FileAttributes.Hidden);
            }
            else
            {
                if (!Directory.Exists(path)) return;
                _ = new DirectoryInfo(path) { Attributes = FileAttributes.Hidden };
            }

        }

        /// <summary>
        /// true if <paramref name="path"/> is not empty and contains invalid symbols for file/folder path
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
                    if (ManageStrings.IsStringAContainsStringB(name, zipname)) return path;
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
                if (cnt == 0) return string.Empty;
            }
            cnt = 2;
            foreach (var dir in Directory.GetDirectories(folderPath))
            {
                cnt--;
                if (cnt == 0) return string.Empty;
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
            if (aloneFolderName.Length == 0) return folderPath;
            if (ManageStrings.IsStringAContainsAnyStringFromStringArray(aloneFolderName, ManageSettings.Games.Game.GameStandartFolderNames, true))
            {
                return folderPath;
            }

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

        /// <summary>
        /// Move content Of Source Folder To Target Folder And Then Clean Source.
        /// There is MoveAll extension method for DirectoryInfo with same function and it added to symlink creation
        /// </summary>
        /// <param name="sourceFolder"></param>
        /// <param name="targetFolder"></param>
        internal static void MoveContent(string sourceFolder, string targetFolder)
        {
            if (!Directory.Exists(sourceFolder)) return;
            if (sourceFolder.IsNullOrEmptyDirectory()) return;
            if (sourceFolder.IsSymlink(ObjectType.Directory)) return;
            if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);

            foreach (string file in Directory.EnumerateFiles(sourceFolder))
            {
                var sourceFile = file;
                var targetFile = Path.Combine(targetFolder, Path.GetFileName(sourceFile));

                try
                {
                    if (!File.Exists(targetFile))
                    {
                        continue;
                    }

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

                    File.Move(sourceFile, targetFile);
                }
                catch (IOException ex)
                {
                    _log.Error("Error in MoveContentOfSourceFolderToTargetFolderAndThenCleanSource:" + Environment.NewLine + "sourceFilePath=" + sourceFile + Environment.NewLine + "targetFilePath=" + targetFile + Environment.NewLine + ex);
                }
            }

            foreach (string dir in Directory.EnumerateDirectories(sourceFolder))
            {
                var targetDir = Path.Combine(targetFolder, Path.GetFileName(dir));
                if (Directory.Exists(targetDir)) targetDir = GetNewNewWithCurrentDate(targetDir, false);

                Directory.Move(dir, targetDir);
            }

            DeleteEmptySubfolders(sourceFolder, true);
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
                    for (int i = 0; i < arrayLength; i++) hash += array[i].ToString("x2", CultureInfo.InvariantCulture)/*.ToLowerInvariant()*/;
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
            fileExtension = fileExtension.ToLowerInvariant();
            return !string.IsNullOrWhiteSpace(fileExtension) && (fileExtension == ".jpg" || fileExtension == ".png" || fileExtension == ".bmp");
        }

        /// <summary>
        /// delete file even if it is read only
        /// </summary>
        /// <param name="file"></param>
        internal static void DeleteReadOnly(this FileInfo file)
        {
            if (!file.Exists) return;

            file.Attributes = FileAttributes.Normal;
            file.Delete();
        }

        /// <summary>
        /// Get case sensitive directory info of exist dir
        /// </summary>
        /// <param name="inputInfo"></param>
        /// <returns></returns>
        public static DirectoryInfo GetCaseSensitive(this DirectoryInfo inputInfo)
        {
            foreach (var info in inputInfo.Parent.GetDirectories(inputInfo.Name)) return info;

            return inputInfo;
        }
        /// <summary>
        /// Get case sensitive name of the file
        /// </summary>
        /// <param name="inputInfo"></param>
        /// <returns></returns>
        public static FileInfo GetCaseSensitive(this FileInfo inputInfo)
        {
            foreach (var info in inputInfo.Directory.GetFiles(inputInfo.Name)) return info;

            return inputInfo;
        }

        /// <summary>
        /// Delete directory with subfiles and dirs even if it is readonly
        /// </summary>
        /// <param name="targetDir"></param>
        internal static void DeleteRecursive(this DirectoryInfo targetDir)
        {
            targetDir.Attributes = FileAttributes.Normal;

            foreach (var file in targetDir.GetFiles())
            {
                file.Attributes = FileAttributes.Normal;
                file.Delete();
            }

            foreach (var dir in targetDir.GetDirectories()) dir.DeleteRecursive();

            targetDir.Delete(false);
        }

        /// <summary>
        /// removes invalid chars from string <paramref name="path"/> which can be used in file\dir path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string RemoveInvalidPathChars(this string path)
        {
            foreach (var c in Path.GetInvalidPathChars())
            {
                int index;
                while ((index = path.IndexOf(c)) != -1) path = path.Remove(index, 1);
            }

            return path;
        }

        /// <summary>
        /// Opens <paramref name="zipfile"/> and find <paramref name="fileName"/> there and then returns stream of the entry
        /// </summary>
        /// <param name="zipfile">path ro exist zipFile</param>
        /// <param name="fileName">file name with extension</param>
        /// <returns>zipfile entry stream</returns>
        public static Stream GetZipEntryStream(this ZipArchive zipfile, string fileName)
        {
            if (zipfile == null || string.IsNullOrWhiteSpace(fileName)) return null;

            Stream ret = null;

            try
            {
                foreach (ZipArchiveEntry entry in zipfile.Entries)
                {
                    if (entry.FullName.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        ret = entry.Open();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("GetZipEntryStream. error:\r\n" + ex);
            }

            return ret;
        }

        //https://code.4noobz.net/c-copy-a-folder-its-content-and-the-subfolders/
        /// <summary>
        /// Copy Directory with its all content
        /// </summary>
        /// <param name="sourceDirectory"></param>
        /// <param name="targetDirectory"></param>
        public static void CopyAll(this string sourceDirectory, string targetDirectory, bool overwriteFiles = false)
        {
            new DirectoryInfo(sourceDirectory).CopyAll(new DirectoryInfo(targetDirectory), overwriteFiles);
        }

        /// <summary>
        /// Copy Directory with its all content
        /// </summary>
        /// <param name="sourceDirectory"></param>
        /// <param name="targetDirectory"></param>
        public static void CopyAll(this DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory, bool overwriteFiles = false, HashSet<string> exclusions = null)
        {
            Directory.CreateDirectory(targetDirectory.FullName);

            bool useExclusions = exclusions != null && exclusions.Count > 0;

            // Copy each file into the new directory.
            foreach (FileInfo fi in sourceDirectory.GetFiles())
            {
                //Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                var targetPath = Path.Combine(targetDirectory.FullName, fi.Name);
                if (!overwriteFiles && File.Exists(targetPath)) continue;

                if (useExclusions && fi.FullName.ContainsAnyFrom(exclusions)) continue;

                fi.CopyTo(targetPath, overwriteFiles);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in sourceDirectory.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    targetDirectory.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir, overwriteFiles, exclusions);
            }
        }

        /// <summary>
        /// Copy Directory with its all content
        /// </summary>
        /// <param name="sourceDirectory"></param>
        /// <param name="targetDirectory"></param>
        public static void MoveAll(this DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory, bool overwriteFiles = false, bool cleanEmptyDirs = true)
        {
            Directory.CreateDirectory(targetDirectory.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in sourceDirectory.GetFiles())
            {
                //Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                var targetPath = Path.Combine(targetDirectory.FullName, fi.Name);
                if (File.Exists(targetPath) && !overwriteFiles) continue;

                fi.MoveToWithBackup(targetPath, overwrite: overwriteFiles);
            }

            // move each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in sourceDirectory.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    targetDirectory.CreateSubdirectory(diSourceSubDir.Name);
                MoveAll(diSourceSubDir, nextTargetSubDir, overwriteFiles);
            }

            if (cleanEmptyDirs) DeleteEmptySubfolders(sourceDirectory.FullName, true);
        }

        /// <summary>
        /// Move file to target path. If file with same name exist in target path it will be renamed with current date and time in format "yyyy_MM_dd_HH_mm_ss" and then moved to target path. If <paramref name="overwrite"/> is true, then existing file will be renamed with ".moveBak" extension and after moving source file to target path, if ".moveBak" file exist, it will be deleted if target file exist or restored if target file not exist. So in this case no files will be lost.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="targetPath"></param>
        /// <param name="overwrite"></param>
        public static void MoveToWithBackup(this FileInfo fileInfo, string targetPath, bool overwrite = false)
        {
            if (File.Exists(targetPath))
            {
                if (overwrite)
                {
                    File.Move(targetPath, targetPath + ".moveBak"); // create temp backup

                    try
                    {
                        File.Move(fileInfo.FullName, targetPath);

                        if (File.Exists(targetPath + ".moveBak"))
                        {
                            if (File.Exists(targetPath))
                            {
                                File.Delete(targetPath + ".moveBak"); // remove tem bak when file succefully moved
                            }
                            else
                            {
                                File.Move(targetPath + ".moveBak", targetPath); // else restore from bak
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Failed to move file " + targetPath + " / error:\r\n" + ex);
                    }

                    return;
                }
                else
                {
                    fileInfo = new FileInfo(targetPath).GetNewTargetName();
                }
            }

            File.Move(fileInfo.FullName, targetPath);
        }

        /// <summary>
        /// get paths to all symlinks in <paramref name="dirPath"/>
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="dirLinksList"></param>
        internal static void GetFolderSymlinks(string dirPath, List<string> dirLinksList)
        {
            Parallel.ForEach(Directory.EnumerateDirectories(dirPath), dir =>
            {
                if (dir.IsSymlink())
                {
                    dirLinksList.Add(dir);
                }
                else
                {
                    GetFolderSymlinks(dir, dirLinksList);
                }
            });
        }

        /// <summary>
        /// If input <paramref name="path"/> for file or folder is exists
        /// </summary>
        /// <param name="IsDir">determines if <paramref name="path"/> is directory else file</param>
        /// <param name="path">input path</param>
        /// <returns></returns>
        internal static bool Exists(this string path, bool IsDir = true)
        {
            return IsDir ? Directory.Exists(path) : File.Exists(path);
        }


        /// <summary>
        /// Normalizes separators and removes trailing slashes for directory-root comparisons.
        /// </summary>
        internal static string NormalizeDirectoryPath(string path)
        {
            path = NormalizePath(path);
            return string.IsNullOrEmpty(path)
                ? path
                : path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        /// <summary>
        /// Normalizes path separators to the platform directory separator.
        /// </summary>
        internal static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            return Path.AltDirectorySeparatorChar == Path.DirectorySeparatorChar
                ? path
                : path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Compares two files for equality by checking their sizes and the first 1KB of their contents. This is a quick check and may not guarantee that the files are identical, but it can be useful for a fast comparison.
        /// </summary>
        /// <param name="sourcePyInfo"></param>
        /// <param name="targetPyInfo"></param>
        /// <returns></returns>
        internal static bool FilesAreEqual(this FileInfo sourcePyInfo, FileInfo targetPyInfo)
        {
            const int BYTES_TO_CHECK = 1024; // Check the first 1KB for a quick comparison
            if (sourcePyInfo.Length != targetPyInfo.Length)
                return false;
            using (FileStream fs1 = sourcePyInfo.OpenRead())
            using (FileStream fs2 = targetPyInfo.OpenRead())
            {
                byte[] buffer1 = new byte[BYTES_TO_CHECK];
                byte[] buffer2 = new byte[BYTES_TO_CHECK];
                int bytesRead1 = fs1.Read(buffer1, 0, BYTES_TO_CHECK);
                int bytesRead2 = fs2.Read(buffer2, 0, BYTES_TO_CHECK);
                if (bytesRead1 != bytesRead2)
                    return false;
                for (int i = 0; i < bytesRead1; i++)
                {
                    if (buffer1[i] != buffer2[i])
                        return false;
                }
            }
            return true;
        }

        internal static bool StringFilesEqual(FileInfo sourcePyInfo, FileInfo targetPyInfo)
        {
            try
            {
                return sourcePyInfo.FullName == targetPyInfo.FullName || (sourcePyInfo.Exists && targetPyInfo.Exists && sourcePyInfo.Length == targetPyInfo.Length && sourcePyInfo.GetHashCode() == targetPyInfo.GetHashCode());
            }
            catch (Exception ex)
            {
                _log.Error($"Error in {nameof(StringFilesEqual)}:" + Environment.NewLine + "sourceFilePath=" + sourcePyInfo.FullName + Environment.NewLine + "targetFilePath=" + targetPyInfo.FullName + Environment.NewLine + ex);
                return false;
            }
        }
    }
}
