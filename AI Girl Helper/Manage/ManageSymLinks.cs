using AIHelper.Manage;
using System.IO;

namespace AI_Helper.Manage
{
    class ManageSymLinks
    {
        internal static bool IsSymlinkAndValid(string symlinkPath, string linkTargetPath, bool IsLinkTargetPathRelative=false)
        {
            var symlinkPathInfo = new FileInfo(symlinkPath);
            if (
                //ссылка вообще существует
                File.Exists(symlinkPath)
                &&
                //файл является ссылкой
                SymbolicLinkSupport.FileInfoExtensions.IsSymbolicLink(symlinkPathInfo)
                &&
                //ссылка валидная
                SymbolicLinkSupport.FileInfoExtensions.IsSymbolicLinkValid(symlinkPathInfo)
                &&
                //целевой файл ссылки равен целевому файлу игры
                (
                (!IsLinkTargetPathRelative && SymbolicLinkSupport.FileInfoExtensions.GetSymbolicLinkTarget(symlinkPathInfo) != linkTargetPath)
                ||
                (IsLinkTargetPathRelative && SymbolicLinkSupport.FileInfoExtensions.GetSymbolicLinkTarget(symlinkPathInfo) != linkTargetPath.Replace(Path.GetDirectoryName(symlinkPath), ".."))
                ))
            {
                return true;
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

                    ManageFilesFolders.Symlink
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
    }
}
