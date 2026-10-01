using AIHelper.Manage;
using System.IO;

namespace AIHelper.Install.Types.Directories.CardsFromDir
{
    /// <summary>
    /// overlays mo images
    /// </summary>
    class OverlayImageInstaller : CardsFromDirsInstallerBase
    {
        public override string[] Masks => new[] { "o" };

        public override string ContentDescription => "Overlay images (folder 'o')";

        protected override string TargetSuffix => "Overlays";

        protected override bool MoveByContentType(string contentType, string dir, string targetFolder, bool moveInThisFolder = false)
        {
            var anyMoved = false;

            ManageArchive.UnpackArchivesToSubfoldersWithSameName(dir, ".zip");

            foreach (var oSubDir in Directory.GetDirectories(dir, "*"))
            {
                string newTarget = ManageFilesFoldersExtensions.MoveFolderToOneLevelUpIfItAloneAndReturnMovedFolderPath(oSubDir);
                string targetDirName = Path.GetFileName(newTarget);
                var resultTargetPath = ManageFilesFoldersExtensions.GetResultTargetDirPathWithNameCheck(targetFolder, targetDirName);

                Directory.Move(newTarget, resultTargetPath);

                anyMoved = true;
            }

            return anyMoved;
        }
    }
}
