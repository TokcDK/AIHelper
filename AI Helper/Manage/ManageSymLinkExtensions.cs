using CheckForEmptyDir;
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
        internal static bool IsValidSymlink(this string symlinkPath, string linkTargetPath = null)
        {
            //ссылка вообще существует
            if (File.Exists(symlinkPath))
            {
                return new FileInfo(symlinkPath).IsValidSymlink(linkTargetPath);
            }
            else if (Directory.Exists(symlinkPath))
            {
                return new DirectoryInfo(symlinkPath).IsValidSymlink(linkTargetPath);
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
            //ссылка вообще существует
            if (!symlinkPath.Exists || !symlinkPath.IsSymlink() || !symlinkPath.IsSymbolicLinkValid())
            {
                return false;
            };

            if (linkTargetPath == null)
            {
                return File.Exists(symlinkPath.GetSymlinkTarget());
            }
            else
            {
                return symlinkPath.IsSymlinkTargetEquals(linkTargetPath);
            }
        }

        /// <summary>
        /// true when object is symbolic link and simbolic link target is exists
        /// </summary>
        /// <param name="symlinkPath"></param>
        /// <returns></returns>
        internal static bool IsValidSymlink(this DirectoryInfo symlinkPath, string linkTargetPath = null)
        {
            //ссылка вообще существует
            if (!symlinkPath.Exists || !symlinkPath.IsSymlink() || !symlinkPath.IsSymbolicLinkValid())
            {
                return false;
            };

            if (linkTargetPath == null)
            {
                return Directory.Exists(symlinkPath.GetSymlinkTarget());
            }
            else
            {
                return symlinkPath.IsSymlinkTargetEquals(linkTargetPath);
            }
        }

        /// <summary>
        /// true when object is symbolic link
        /// </summary>
        /// <param name="symlinkPath"></param>
        /// <returns></returns>
        internal static bool IsSymlink(this DirectoryInfo symlinkPath, string linkTargetPath = null)
        {
            try
            {
                return symlinkPath.IsSymbolicLink();
            }
            catch (IOException)
            {
                return false;
            }
        }

        /// <summary>
        /// true when object is symbolic link
        /// </summary>
        /// <param name="symlinkPath"></param>
        /// <returns></returns>
        internal static bool IsSymlink(this FileInfo symlinkPath, string linkTargetPath = null)
        {
            try
            {
                return symlinkPath.IsSymbolicLink();
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
        internal static bool IsSymLink(this string sourceFileFolder)
        {
            if (File.Exists(sourceFileFolder))
            {
                return new FileInfo(sourceFileFolder).IsSymlink();
            }
            else if (Directory.Exists(sourceFileFolder))
            {
                return new DirectoryInfo(sourceFileFolder).IsSymlink();
            }

            return false;
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
            return objectDirPath.FullName.CreateSymlink(symlinkPath, isRelative, ObjectType.Dir);
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
        /// <param name="oType"></param>
        /// <returns></returns>
        public static bool CreateSymlink(this string objectFileDirPath, string symlinkPath, bool isRelative = false, ObjectType oType = ObjectType.NotDefined)
        {
            if (string.IsNullOrWhiteSpace(symlinkPath) || string.IsNullOrWhiteSpace(objectFileDirPath))
            {
                return false;
            }

            if (oType == ObjectType.Dir && !Directory.Exists(objectFileDirPath))
            {
                return false;
            }
            else if (oType == ObjectType.File && !File.Exists(objectFileDirPath))
            {
                return false;
            }

            //ManageMO.GetLastMOFileDirPathFromEnabledModsOfActiveMOProfile(
            string objectPath = null;
            if ((oType == ObjectType.NotDefined && File.Exists(objectPath = ManageModOrganizer.GetLastMoFileDirPathFromEnabledModsOfActiveMoProfile(objectFileDirPath))) || oType == ObjectType.File)
            {
                if (oType != ObjectType.NotDefined && !File.Exists(objectFileDirPath))
                {
                    return false;
                }

                var fi = new FileInfo(symlinkPath);
                if ((objectPath != symlinkPath && !File.Exists(symlinkPath))
                    || (
                            fi.IsSymlink()
                            &&
                            !fi.IsValidSymlink()
                        )
                   )
                {
                    if (File.Exists(symlinkPath))
                    {
                        File.Delete(symlinkPath);
                    }

                    CheckParentDirForSymLink(symlinkPath); // prevent error of missing dir

                    // create parent dir
                    if (fi.Directory.Root != fi.Directory)
                    {
                        fi.Directory.Create();
                    }

                    try
                    {
                        new FileInfo(objectPath).CreateSymbolicLink(symlinkPath, isRelative);//new from NuGet package
                    }
                    catch (IOException ex) { ManageLogs.Log("An error accured while tried to create symlink. creation skipped. error:" + ex.Message); }

                    return true;
                }
            }
            else if ((oType == ObjectType.NotDefined && Directory.Exists(objectPath = ManageModOrganizer.GetLastMoFileDirPathFromEnabledModsOfActiveMoProfile(objectFileDirPath, true))) || oType == ObjectType.Dir)
            {
                if (oType != ObjectType.NotDefined && !File.Exists(objectFileDirPath))
                {
                    if (Directory.Exists(objectPath = ManageModOrganizer.GetLastMoFileDirPathFromEnabledModsOfActiveMoProfile(objectFileDirPath, true)))
                    {
                    }
                    else
                    {
                        Directory.CreateDirectory(objectFileDirPath);
                    }
                }


                // NOTE
                // here is exist problem when for example from disc D: was created symlink for dir "Data" on disc C:
                // and inside Data dir was another symlink with relative path. And even if relative path is working
                // and Directory.Exists will be true the link can be not able to be opened with explorer or will throw
                // exception for methods like Directory.GetFiles
                var di = new DirectoryInfo(symlinkPath);
                bool b1 = (objectPath != symlinkPath && !Directory.Exists(symlinkPath));
                bool issm = di.IsSymlink();
                bool isvsm = di.IsValidSymlink();

                bool b2 = (issm && !isvsm);
                if (b1 || b2)
                {
                    if (Directory.Exists(symlinkPath) && symlinkPath.IsNullOrEmptyDirectory())
                    {
                        Directory.Delete(symlinkPath);
                    }

                    CheckParentDirForSymLink(symlinkPath);

                    // create parent dir
                    if (di.Root != di.Parent)
                    {
                        di.Parent.Create();
                    }

                    try
                    {
                        new DirectoryInfo(objectPath).CreateSymbolicLink(symlinkPath, isRelative);//new from NuGet package
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
            return symlink.IsValidSymlink() && symlink.GetSymlinkTarget() == requiredTargetPath;
        }

        /// <summary>
        /// true when object path is valid symbolic link and it's target path is equal required target path
        /// </summary>
        /// <param name="symlink"></param>
        /// <param name="requiredTargetPath"></param>
        /// <returns></returns>
        internal static bool IsSymlinkTargetEquals(this FileInfo symlink, string requiredTargetPath)
        {
            return symlink.IsSymlink() && symlink.GetSymlinkTarget() == requiredTargetPath;
        }

        /// <summary>
        /// true when object path is valid symbolic link and it's target path is equal required target path
        /// </summary>
        /// <param name="symlink"></param>
        /// <param name="requiredTargetPath"></param>
        /// <returns></returns>
        internal static bool IsSymlinkTargetEquals(this DirectoryInfo symlink, string requiredTargetPath)
        {
            return symlink.IsSymlink() && symlink.GetSymlinkTarget() == requiredTargetPath;
        }
    }

    public enum ObjectType
    {
        NotDefined = 0,
        Dir = 1,
        File = 2
    }
}
