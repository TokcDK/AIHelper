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
                if (new DirectoryInfo(LinkPath).IsSymbolicLink())
                {
                    Directory.Delete(LinkPath);

                    return true;
                }
            }
            else if (File.Exists(LinkPath))
            {
                if (new FileInfo(LinkPath).IsSymbolicLink())
                {
                    //https://stackoverflow.com/a/6375373
                    //ManageFilesFolders.Unblock(LinkPath);

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
                if (symlinkPathInfo.IsSymbolicLink())
                {
                    //ссылка валидная
                    if (symlinkPathInfo.IsSymbolicLinkValid())
                    {
                        //целевой файл ссылки равен целевому файлу игры
                        if (symlinkPathInfo.GetSymbolicLinkTarget() == linkTargetPath)
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
                if (targetFilePath != symlinkPath && !IsSymlinkAndValid(symlinkPath, targetFilePath, linktargetPathIsRelative))
                {
                    if(File.Exists(symlinkPath))
                    {
                        File.Delete(symlinkPath);
                    }

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

                        CheckParentDirForSymLink(symlinkPath);

                        new FileInfo(objectPath).CreateSymbolicLink(symlinkPath, isRelative);//new from NuGet package

                        return true;
                    }
                }
                else if (Directory.Exists(objectPath = ManageMO.GetLastMOFileDirPathFromEnabledModsOfActiveMOProfile(objectFileDirPath, true)))
                {
                    DirectoryInfo di;
                    if ((objectPath != symlinkPath && !Directory.Exists(symlinkPath)) || ((di = new DirectoryInfo(symlinkPath)).IsSymbolicLink() && !di.IsSymbolicLinkValid()))
                    {
                        if (Directory.Exists(symlinkPath) && ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(symlinkPath))
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

        private static void CheckParentDirForSymLink(string symlinkPath)
        {
            var pdir = new DirectoryInfo(Path.GetDirectoryName(symlinkPath));
            if (pdir.IsSymbolicLink() && !pdir.IsSymbolicLinkValid())
            {
                pdir.Delete();
            }
            pdir.Create();
        }

        internal static bool IsSymLink(string sourceFileFolder)
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
    }
}
