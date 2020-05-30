using AIHelper.Manage;
using SymbolicLinkSupport;
using System.IO;

namespace AI_Helper.Manage
{
    class ManageSymLinks
    {
        public static bool DeleteIfSymlink(string LinkPath, bool IsFolder = false)
        {
            if (IsFolder || Directory.Exists(LinkPath))
            {
                if (FileInfoExtensions.IsSymbolicLink(new FileInfo(LinkPath)))
                {
                    Directory.Delete(LinkPath);

                    return true;
                }
            }
            else if (File.Exists(LinkPath))
            {
                if (FileInfoExtensions.IsSymbolicLink(new FileInfo(LinkPath)))
                {
                    File.Delete(LinkPath);

                    return true;
                }
            }

            return false;
        }

        internal static bool IsSymlinkAndValid(string symlinkPath, string linkTargetPath, bool IsLinkTargetPathRelative = false)
        {
            var symlinkPathInfo = new FileInfo(symlinkPath);
            //ссылка вообще существует
            if (File.Exists(symlinkPath))
            {
                //файл является ссылкой
                if (SymbolicLinkSupport.FileInfoExtensions.IsSymbolicLink(symlinkPathInfo))
                {
                    //ссылка валидная
                    if (SymbolicLinkSupport.FileInfoExtensions.IsSymbolicLinkValid(symlinkPathInfo))
                    {
                        //целевой файл ссылки равен целевому файлу игры
                        if (SymbolicLinkSupport.FileInfoExtensions.GetSymbolicLinkTarget(symlinkPathInfo) == linkTargetPath)
                        {
                            return true;
                        };
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
        internal static void ReCreateFileLinkWhenNotValid(string symlinkPath, string targetFilePath, bool linktargetPathIsRelative = false)
        {
            if (
                //если целевой файл выбранной игры существует
                File.Exists(targetFilePath)
               )
            {
                if (!IsSymlinkAndValid(symlinkPath, targetFilePath, linktargetPathIsRelative))
                {
                    File.Delete(symlinkPath);

                    ManageSymLinks.Symlink
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

        public static bool Symlink(string objectFileDirPath, string symlinkPath, bool isRelative = false)
        {
            if (symlinkPath.Length > 0 && objectFileDirPath.Length > 0)
            {
                string parentDirPath = Path.GetDirectoryName(symlinkPath);
                if (!Directory.Exists(parentDirPath))
                {
                    Directory.CreateDirectory(parentDirPath);
                }

                //ManageMO.GetLastMOFileDirPathFromEnabledModsOfActiveMOProfile(
                string objectPath;
                if (File.Exists(objectPath = ManageMO.GetLastMOFileDirPathFromEnabledModsOfActiveMOProfile(objectFileDirPath)))
                {
                    if ((!File.Exists(symlinkPath) || (FileInfoExtensions.IsSymbolicLink(new FileInfo(symlinkPath)) && !FileInfoExtensions.IsSymbolicLinkValid(new FileInfo(symlinkPath)))))
                    {
                        if (File.Exists(symlinkPath))
                        {
                            File.Delete(symlinkPath);
                        }

                        //FileInfoExtensions.CreateSymbolicLink(new FileInfo(objectFileDirPath), symlinkPath, isRelative);//new from NuGet package
                        FileInfoExtensions.CreateSymbolicLink(new FileInfo(objectPath), symlinkPath, isRelative);//new from NuGet package
                                                                                                                 //CreateSymlink.File(file, symlink); //old

                        return true;
                    }
                }
                else if (Directory.Exists(objectPath = ManageMO.GetLastMOFileDirPathFromEnabledModsOfActiveMOProfile(objectFileDirPath, true)))
                {
                    if ((!Directory.Exists(symlinkPath) || (DirectoryInfoExtensions.IsSymbolicLink(new DirectoryInfo(symlinkPath)) && !DirectoryInfoExtensions.IsSymbolicLinkValid(new DirectoryInfo(symlinkPath)))))
                    {
                        if (Directory.Exists(symlinkPath) && ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(symlinkPath))
                        {
                            Directory.Delete(symlinkPath);
                        }

                        //DirectoryInfoExtensions.CreateSymbolicLink(new DirectoryInfo(objectFileDirPath), symlinkPath, isRelative);//new from NuGet package
                        DirectoryInfoExtensions.CreateSymbolicLink(new DirectoryInfo(objectPath), symlinkPath, isRelative);//new from NuGet package
                                                                                                                           //CreateSymlink.Folder(file, symlink); //old

                        return true;
                    }

                }
            }

            return false;
        }

        internal static bool IsSymLink(string sourceFileFolder)
        {
            if (File.Exists(sourceFileFolder))
            {
                return FileInfoExtensions.IsSymbolicLink(new FileInfo(sourceFileFolder));
            }
            else if (Directory.Exists(sourceFileFolder))
            {
                return DirectoryInfoExtensions.IsSymbolicLink(new DirectoryInfo(sourceFileFolder));
            }

            return false;
        }
    }
}
