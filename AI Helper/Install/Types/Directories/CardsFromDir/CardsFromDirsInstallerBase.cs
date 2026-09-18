using AIHelper.Manage;
using System.IO;

namespace AIHelper.Install.Types.Directories.CardsFromDir
{
    /// <summary>
    /// Base class for installers which are moving card images from the install dir subfolders
    /// (like 'f', 'm', 'c', 'cf', 'cb', 'h', 'o', 's') to the corresponding UserData folders
    /// </summary>
    abstract class CardsFromDirsInstallerBase : DirectoriesInstallerBase
    {
        /// <summary>
        /// Extension of the processed card images
        /// </summary>
        private const string CardImageExtension = ".png";

        /// <summary>
        /// Suffix for target folder name
        /// </summary>
        protected abstract string TargetSuffix { get; }

        protected override bool Get(DirectoryInfo dir)
        {
            var anyMoved = false;

            foreach (var mask in Masks)
            {
                if (MoveImagesToTargetContentFolder(dir.FullName, mask))
                {
                    anyMoved = true;
                }
            }

            return anyMoved;
        }

        /// <summary>
        /// Extra actions which must be executed for the selected content type
        /// </summary>
        /// <param name="contentType">Mask of the content type which is processed now</param>
        /// <param name="dir">Source directory of the content</param>
        /// <param name="targetFolder">Target folder of the content type</param>
        /// <param name="moveInThisFolder">Move content in the source dir instead of the UserData folder</param>
        /// <returns>True when something was moved</returns>
        protected virtual bool MoveByContentType(string contentType, string dir, string targetFolder, bool moveInThisFolder = false)
        {
            return false;
        }

        /// <summary>
        /// Moves images of the selected content type from <paramref name="dir"/> to the corresponding UserData folder
        /// </summary>
        /// <param name="dir">Source directory of the content</param>
        /// <param name="contentType">Mask of the content type which is processed now</param>
        /// <param name="moveInThisFolder">Move content in the source dir instead of the UserData folder</param>
        /// <returns>True when something was moved</returns>
        protected bool MoveImagesToTargetContentFolder(string dir, string contentType, bool moveInThisFolder = false)
        {
            var anyMoved = false;

            string targetFolder = ManageModOrganizer.GetUserDataSubFolder(moveInThisFolder ? dir : " " + TargetSuffix, contentType);

            //Для всех, сброс png из корневой папки в целевую
            foreach (var imagePath in Directory.GetFiles(dir, "*" + CardImageExtension))
            {
                var targetImagePath = ManageFilesFoldersExtensions.GetResultTargetFilePathWithNameCheck(
                    targetFolder, Path.GetFileNameWithoutExtension(imagePath), CardImageExtension);

                File.Move(imagePath, targetImagePath);

                anyMoved = true;
            }

            if (MoveByContentType(contentType, dir, targetFolder, moveInThisFolder))
            {
                anyMoved = true;
            }

            ManageFilesFoldersExtensions.DeleteEmptySubfolders(dir);

            return anyMoved;
        }
    }
}
