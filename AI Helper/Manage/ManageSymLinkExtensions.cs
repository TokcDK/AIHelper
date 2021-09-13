﻿using CheckForEmptyDir;
using SymbolicLinkSupport;
using System.IO;

namespace AIHelper.Manage
{
    /// <summary>
    /// reference symbolic links using here
    /// </summary>
    static class ManageSymLinkExtensions
    {
        public static bool DeleteIfSymlink(this string linkPath, bool isFolder = false)
        {
            if (isFolder || Directory.Exists(linkPath))
            {
                if (new DirectoryInfo(linkPath).IsSymlink())
                {
                    Directory.Delete(linkPath);

                    return true;
                }
            }
            else if (File.Exists(linkPath))
            {
                if (new FileInfo(linkPath).IsSymlink())
                {
                    //https://stackoverflow.com/a/6375373
                    //ManageFilesFolders.Unblock(LinkPath);

                    File.Delete(linkPath);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// true if is symbolic link and target is exists.
        /// when linkTargetPath not null also will check if symlink target equal to linkTargetPath
        /// </summary>
        /// <param name="symlinkPath"></param>
        /// <param name="linkTargetPath"></param>
        /// <returns></returns>
        internal static bool IsValidSymlink(this string symlinkPath, string linkTargetPath = null, ObjectType objectType = ObjectType.NotDefined)
        {
            //ссылка вообще существует
            if (objectType == ObjectType.File || (objectType == ObjectType.NotDefined && File.Exists(symlinkPath)))
            {
                var i = new FileInfo(symlinkPath);
                return i.IsSymlink() && i.IsValidSymlink(linkTargetPath);
            }
            else if (objectType == ObjectType.Directory || (objectType == ObjectType.NotDefined && Directory.Exists(symlinkPath)))
            {
                var i = new DirectoryInfo(symlinkPath);
                return i.IsSymlink() && i.IsValidSymlink(linkTargetPath);
            }

            return false;
        }

        /// <summary>
        /// true when object is symbolic link and simbolic link target is exists
        /// </summary>
        /// <param name="symlinkPath"></param>
        /// <returns></returns>
        internal static bool IsValidSymlink(this FileInfo symlinkPath, string linkTargetPath = null)
        {
            try
            {
                //ссылка вообще существует
                if (!symlinkPath.IsSymbolicLinkValid())
                {
                    return false;
                };

                if (string.IsNullOrWhiteSpace(linkTargetPath))
                {
                    // true because already was same check
                    return true;// File.Exists(Path.GetFullPath(symlinkPath.GetSymlinkTarget()));
                }
                else
                {
                    return symlinkPath.IsSymlinkTargetEquals(linkTargetPath);
                }
            }
            catch (IOException)
            {
                return false;
            }
        }

        /// <summary>
        /// true when object is symbolic link and simbolic link target is exists
        /// </summary>
        /// <param name="symlinkPath"></param>
        /// <returns></returns>
        internal static bool IsValidSymlink(this DirectoryInfo symlinkPath, string linkTargetPath = null)
        {
            try
            {
                //ссылка вообще существует
                if (!symlinkPath.IsSymbolicLinkValid())
                {
                    return false;
                };

                if (string.IsNullOrWhiteSpace(linkTargetPath))
                {
                    // true because already was same check
                    return true; // Directory.Exists(Path.GetFullPath(symlinkPath.GetSymlinkTarget()));
                }
                else
                {
                    return symlinkPath.IsSymlinkTargetEquals(linkTargetPath);
                }
            }
            catch (IOException)
            {
                return false;
            }
        }

        /// <summary>
        /// true when object is symbolic link
        /// </summary>
        /// <param name="objectPath"></param>
        /// <returns></returns>
        internal static bool IsSymlink(this DirectoryInfo objectPath)
        {
            try
            {
                return objectPath.IsSymbolicLink();
            }
            catch (IOException)
            {
                return false;
            }
        }

        /// <summary>
        /// true when object is symbolic link
        /// </summary>
        /// <param name="objectPath"></param>
        /// <returns></returns>
        internal static bool IsSymlink(this FileInfo objectPath)
        {
            try
            {
                return objectPath.IsSymbolicLink();
            }
            catch (IOException)
            {
                return false;
            }
        }

        /// <summary>
        /// true when object is symbolic link
        /// </summary>
        /// <returns></returns>
        internal static bool IsSymlink(this string objectPath, ObjectType objectType = ObjectType.NotDefined)
        {
            if (objectType == ObjectType.File || (objectType == ObjectType.NotDefined && File.Exists(objectPath)))
            {
                return new FileInfo(objectPath).IsSymlink();
            }
            else if (objectType == ObjectType.Directory || (objectType == ObjectType.NotDefined && Directory.Exists(objectPath)))
            {
                return new DirectoryInfo(objectPath).IsSymlink();
            }

            return false;
        }

        /// <summary>
        /// true when object is symbolic link
        /// </summary>
        /// <returns></returns>
        internal static string GetSymlinkTarget(this string symlinkPath, ObjectType objectType = ObjectType.NotDefined)
        {
            if (objectType == ObjectType.File || (objectType == ObjectType.NotDefined && File.Exists(symlinkPath)))
            {
                return new FileInfo(symlinkPath).GetSymlinkTarget();
            }
            else if (objectType == ObjectType.Directory || (objectType == ObjectType.NotDefined && Directory.Exists(symlinkPath)))
            {
                return new DirectoryInfo(symlinkPath).GetSymlinkTarget();
            }

            return "";
        }

        /// <summary>
        /// get target of the symlink
        /// </summary>
        /// <param name="symlinkPath"></param>
        /// <returns></returns>
        internal static string GetSymlinkTarget(this DirectoryInfo symlinkPath, string linkTargetPath = null)
        {
            return symlinkPath.GetSymbolicLinkTarget();
        }

        /// <summary>
        /// get target of the symlink
        /// </summary>
        /// <param name="symlinkPath"></param>
        /// <returns></returns>
        internal static string GetSymlinkTarget(this FileInfo symlinkPath, string linkTargetPath = null)
        {
            return symlinkPath.GetSymbolicLinkTarget();
        }

        /// <summary>
        /// Will recreate Symlink for target file if it is not a link or not valid link
        /// </summary>
        /// <param name="symlinkPath"></param>
        /// <param name="targetFilePath"></param>
        /// <param name="linktargetPathIsRelative"></param>
        internal static void ReCreateFileLinkWhenNotValid(this string symlinkPath, string targetFilePath, bool linktargetPathIsRelative = false)
        {
            if (
                //если целевой файл выбранной игры существует
                File.Exists(targetFilePath)
               )
            {
                if (targetFilePath != symlinkPath && !IsValidSymlink(symlinkPath, targetFilePath))
                {
                    if (File.Exists(symlinkPath))
                    {
                        File.Delete(symlinkPath);
                    }

                    ManageSymLinkExtensions.CreateSymlink
                      (
                       targetFilePath
                       ,
                       symlinkPath
                       ,
                       linktargetPathIsRelative
                      );
                }

            }

        }

        /// <summary>
        /// Create symlink for file or folder if object is exists (will create empty folder if not).
        /// Will create link if link target is invalid or link path not exists
        /// </summary>
        /// <param name="objectDirPath"></param>
        /// <param name="symlinkPath"></param>
        /// <param name="isRelative"></param>
        /// <returns></returns>
        public static bool CreateSymlink(this DirectoryInfo objectDirPath, string symlinkPath, bool isRelative = false)
        {
            return objectDirPath.FullName.CreateSymlink(symlinkPath, isRelative, ObjectType.Directory);
        }

        /// <summary>
        /// Create symlink for file or folder if object is exists (will create empty folder if not).
        /// Will create link if link target is invalid or link path not exists
        /// </summary>
        /// <param name="objectFilePath"></param>
        /// <param name="symlinkPath"></param>
        /// <param name="isRelative"></param>
        /// <returns></returns>
        public static bool CreateSymlink(this FileInfo objectFilePath, string symlinkPath, bool isRelative = false)
        {
            return objectFilePath.FullName.CreateSymlink(symlinkPath, isRelative, ObjectType.File);
        }

        /// <summary>
        /// Create symlink for file or folder if object is exists (will create empty folder if not).
        /// Will create link if link target is invalid or link path not exists
        /// </summary>
        /// <param name="objectFileDirPath"></param>
        /// <param name="symlinkPath"></param>
        /// <param name="isRelative"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public static bool CreateSymlink(this string objectFileDirPath, string symlinkPath, bool isRelative = false, ObjectType objectType = ObjectType.NotDefined)
        {
            // skip if empty or equal values
            if (string.IsNullOrWhiteSpace(symlinkPath) 
                || string.IsNullOrWhiteSpace(objectFileDirPath) 
                || string.Equals(objectFileDirPath, symlinkPath, System.StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (objectType == ObjectType.Directory && !Directory.Exists(objectFileDirPath))
            {
                return false;
            }
            else if (objectType == ObjectType.File && !File.Exists(objectFileDirPath))
            {
                return false;
            }

            //ManageMO.GetLastMOFileDirPathFromEnabledModsOfActiveMOProfile(
            string objectPath = null;
            if ((objectType == ObjectType.NotDefined && File.Exists(objectPath = ManageModOrganizer.GetLastPath(objectFileDirPath))) || objectType == ObjectType.File)
            {
                if (objectType == ObjectType.File)
                {
                    // if objectType is set to file or dir objectPath will no set
                    if(objectPath == null)
                    {
                        objectPath = ManageModOrganizer.GetLastPath(objectFileDirPath);
                    }
                }

                if (!File.Exists(objectPath))
                {
                    return false; // return if file is not exists
                }

                var fi = new FileInfo(symlinkPath);
                bool IsInvalidSymlink = false;
                if ((objectPath != symlinkPath && !File.Exists(symlinkPath))
                    || (IsInvalidSymlink =
                            (fi.IsSymlink()
                            &&
                            !fi.IsValidSymlink())
                        )
                   )
                {
                    if (IsInvalidSymlink && File.Exists(symlinkPath))
                    {
                        File.Delete(symlinkPath);
                    }

                    CheckParentDirForSymLink(symlinkPath); // prevent error of missing dir

                    // create parent dir
                    if (fi.Directory.Root != fi.Directory)
                    {
                        fi.Directory.Create();
                    }

                    var objectInfo = new FileInfo(objectPath);
                    if (isRelative)
                    {
                        isRelative = objectInfo.Directory.Root == new FileInfo(symlinkPath).Directory.Root;
                    }

                    try
                    {
                        objectInfo.CreateSymbolicLink(symlinkPath, isRelative);//new from NuGet package
                    }
                    catch (IOException ex)
                    {
                        ManageLogs.Log("An error accured while tried to create symlink. creation skipped. error:" + ex.Message);
                    }

                    return true;
                }
            }
            else if ((objectType == ObjectType.NotDefined && Directory.Exists(objectPath = ManageModOrganizer.GetLastPath(objectFileDirPath, isDir: true)))
                || objectType == ObjectType.Directory)
            {
                if (objectType == ObjectType.Directory)
                {
                    // if objectType is set to file or dir objectPath will no set
                    if(objectPath == null)
                    {
                        objectPath = ManageModOrganizer.GetLastPath(objectFileDirPath, isDir: true);
                    }

                    Directory.CreateDirectory(objectPath); // create dir if setlastpath returned input dir path and the dir is not exists
                }

                // NOTE
                // here is exist problem when for example from disc D: was created symlink for dir "Data" on disc C:
                // and inside Data dir was another symlink with relative path. And even if relative path is working
                // and Directory.Exists will be true the link can be not able to be opened with explorer or will throw
                // exception for methods like Directory.GetFiles
                var di = new DirectoryInfo(symlinkPath);
                bool IsInvalidSymlink = false;
                if ((objectPath != symlinkPath && !Directory.Exists(symlinkPath)) || (IsInvalidSymlink = (di.IsSymlink() && !di.IsValidSymlink())))
                {
                    var objectInfo = new DirectoryInfo(objectPath);

                    if (Directory.Exists(symlinkPath))
                    {
                        if (IsInvalidSymlink || symlinkPath.IsNullOrEmptyDirectory())
                        {
                            Directory.Delete(symlinkPath);
                        }
                        else
                        {
                            di.MoveAll(objectInfo);
                        }
                    }

                    CheckParentDirForSymLink(symlinkPath);

                    // create parent dir
                    if (di.Root != di.Parent)
                    {
                        di.Parent.Create();
                    }
                    if (isRelative)
                    {
                        isRelative = objectInfo.Root == new DirectoryInfo(symlinkPath).Root;
                    }

                    try
                    {
                        objectInfo.CreateSymbolicLink(symlinkPath, isRelative);//new from NuGet package
                                                                                                  //CreateSymlink.Folder(file, symlink); //old
                    }
                    catch (IOException ex) { ManageLogs.Log("An error accured while tried to create symlink. creation skipped. error:" + ex.Message); }

                    return true;
                }

            }

            return false;
        }

        /// <summary>
        /// create parent dirs if not exist and remove invalid symlinks
        /// </summary>
        /// <param name="symlinkPath"></param>
        private static void CheckParentDirForSymLink(this string symlinkPath)
        {
            var pdir = new DirectoryInfo(Path.GetDirectoryName(symlinkPath));
            if (pdir.IsSymlink() && !pdir.IsValidSymlink())
            {
                pdir.Delete();
            }
            pdir.Create();
        }

        /// <summary>
        /// true when object path is valid symbolic link and it's target path is equal required target path
        /// </summary>
        /// <param name="symlink"></param>
        /// <param name="requiredTargetPath"></param>
        /// <returns></returns>
        internal static bool IsValidSymlinkTargetEquals(this DirectoryInfo symlink, string requiredTargetPath)
        {
            return symlink.Exists && symlink.IsSymlink() && symlink.IsValidSymlink() && symlink.GetSymlinkTarget() == requiredTargetPath;
        }

        /// <summary>
        /// true when object path is valid symbolic link and it's target path is equal required target path
        /// </summary>
        /// <param name="symlink"></param>
        /// <param name="requiredTargetPath"></param>
        /// <returns></returns>
        internal static bool IsSymlinkTargetEquals(this FileInfo symlink, string requiredTargetPath)
        {
            return symlink.Exists && symlink.IsSymlink() && symlink.GetSymlinkTarget() == requiredTargetPath;
        }

        /// <summary>
        /// true when object path is valid symbolic link and it's target path is equal required target path
        /// </summary>
        /// <param name="symlink"></param>
        /// <param name="requiredTargetPath"></param>
        /// <returns></returns>
        internal static bool IsSymlinkTargetEquals(this DirectoryInfo symlink, string requiredTargetPath)
        {
            string targetPath;
            return symlink.Exists && symlink.IsSymlink() && ((targetPath = symlink.GetSymlinkTarget()) == requiredTargetPath || Path.GetFullPath(targetPath) == Path.GetFullPath(requiredTargetPath));
        }
    }
}
