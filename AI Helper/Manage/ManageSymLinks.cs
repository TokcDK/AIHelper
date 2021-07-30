using CheckForEmptyDir;
using SymbolicLinkSupport;
using System.IO;

namespace AIHelper.Manage
{
    static class ManageSymLinkExtensions
    {
        public static bool DeleteIfSymlink(this string linkPath, bool isFolder = false)
        {
            if (isFolder || Directory.Exists(linkPath))
            {
                if (new DirectoryInfo(linkPath).IsSymbolicLink())
                {
                    Directory.Delete(linkPath);

                    return true;
                }
            }
            else if (File.Exists(linkPath))
            {
                if (new FileInfo(linkPath).IsSymbolicLink())
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
            if (symlinkPath.Exists)
            {
                //файл является ссылкой
                if (symlinkPath.IsSymbolicLink())
                {
                    //ссылка валидная
                    if (symlinkPath.IsSymbolicLinkValid())
                    {
                        if (linkTargetPath == null)
                        {
                            return true;
                        }
                        else if (symlinkPath.IsSymlinkTargetEquals(linkTargetPath))
                        {
                            return true;
                        }
                    };
                };
            };

            return false;
        }

        /// <summary>
        /// true when object is symbolic link and simbolic link target is exists
        /// </summary>
        /// <param name="symlinkPath"></param>
        /// <returns></returns>
        internal static bool IsValidSymlink(this DirectoryInfo symlinkPath, string linkTargetPath = null)
        {
            //ссылка вообще существует
            if (symlinkPath.Exists)
            {
                //файл является ссылкой
                if (symlinkPath.IsSymbolicLink())
                {
                    //ссылка валидная
                    if (symlinkPath.IsSymbolicLinkValid())
                    {
                        if (linkTargetPath == null)
                        {
                            return true;
                        }
                        else if (symlinkPath.IsSymlinkTargetEquals(linkTargetPath))
                        {
                            return true;
                        }
                    };
                };
            };

            return false;
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
            if (symlinkPath.Length > 0 && objectFileDirPath.Length > 0)
            {
                string parentDirPath = Path.GetDirectoryName(symlinkPath);
                Directory.CreateDirectory(parentDirPath);

                //ManageMO.GetLastMOFileDirPathFromEnabledModsOfActiveMOProfile(
                string objectPath = null;
                if ((oType == ObjectType.NotDefined && File.Exists(objectPath = ManageModOrganizer.GetLastMoFileDirPathFromEnabledModsOfActiveMoProfile(objectFileDirPath))) || oType == ObjectType.File)
                {
                    if (oType != ObjectType.NotDefined && !File.Exists(objectFileDirPath))
                    {
                        return false;
                    }

                    FileInfo fi;
                    if ((objectPath != symlinkPath && !File.Exists(symlinkPath))
                        || (
                                (fi = new FileInfo(symlinkPath)).IsSymbolicLink()
                                &&
                                !fi.IsSymbolicLinkValid()
                            )
                       )
                    {
                        if (File.Exists(symlinkPath))
                        {
                            File.Delete(symlinkPath);
                        }

                        CheckParentDirForSymLink(symlinkPath); // prevent error of missing dir

                        new FileInfo(objectPath).CreateSymbolicLink(symlinkPath, isRelative);//new from NuGet package

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

                    DirectoryInfo di;
                    if ((objectPath != symlinkPath && !Directory.Exists(symlinkPath)) || ((di = new DirectoryInfo(symlinkPath)).IsSymbolicLink() && !di.IsSymbolicLinkValid()))
                    {
                        if (Directory.Exists(symlinkPath) && symlinkPath.IsNullOrEmptyDirectory())
                        {
                            Directory.Delete(symlinkPath);
                        }

                        CheckParentDirForSymLink(symlinkPath);

                        new DirectoryInfo(objectPath).CreateSymbolicLink(symlinkPath, isRelative);//new from NuGet package
                                                                                                  //CreateSymlink.Folder(file, symlink); //old

                        return true;
                    }

                }
            }

            return false;
        }

        private static void CheckParentDirForSymLink(this string symlinkPath)
        {
            var pdir = new DirectoryInfo(Path.GetDirectoryName(symlinkPath));
            if (pdir.IsSymbolicLink() && !pdir.IsSymbolicLinkValid())
            {
                pdir.Delete();
            }
            pdir.Create();
        }

        internal static bool IsSymLink(this string sourceFileFolder)
        {
            if (File.Exists(sourceFileFolder))
            {
                return new FileInfo(sourceFileFolder).IsSymbolicLink();
            }
            else if (Directory.Exists(sourceFileFolder))
            {
                return new DirectoryInfo(sourceFileFolder).IsSymbolicLink();
            }

            return false;
        }

        /// <summary>
        /// true when object path is valid symbolic link and it's target path is equal required target path
        /// </summary>
        /// <param name="symlink"></param>
        /// <param name="requiredTargetPath"></param>
        /// <returns></returns>
        internal static bool IsValidSymlinkTargetEquals(this DirectoryInfo symlink, string requiredTargetPath)
        {
            return symlink.IsValidSymlink() && symlink.GetSymbolicLinkTarget() == requiredTargetPath;
        }

        /// <summary>
        /// true when object path is valid symbolic link and it's target path is equal required target path
        /// </summary>
        /// <param name="symlink"></param>
        /// <param name="requiredTargetPath"></param>
        /// <returns></returns>
        internal static bool IsSymlinkTargetEquals(this FileInfo symlink, string requiredTargetPath)
        {
            return symlink.IsValidSymlink() && symlink.GetSymbolicLinkTarget() == requiredTargetPath;
        }

        /// <summary>
        /// true when object path is valid symbolic link and it's target path is equal required target path
        /// </summary>
        /// <param name="symlink"></param>
        /// <param name="requiredTargetPath"></param>
        /// <returns></returns>
        internal static bool IsSymlinkTargetEquals(this DirectoryInfo symlink, string requiredTargetPath)
        {
            return symlink.IsValidSymlink() && symlink.GetSymbolicLinkTarget() == requiredTargetPath;
        }
    }

    public enum ObjectType
    {
        NotDefined = 0,
        Dir = 1,
        File = 2
    }
}
