using AIHelper.Manage;
using System.IO;

namespace AIHelper.Install.Types.Directories.CardsFromDir
{
    /// <summary>
    /// overlays mo images
    /// </summary>
    class OverlayImageInstaller : CardsFromDirsInstallerBase
    {
        protected override string targetFolderName => "Overlays";

        public override string[] Masks => new[] { "o" };

        protected override string TargetSuffix => "Overlays";

        protected override bool MoveByContentType(string contentType, string dir, string targetFolder, bool moveInThisFolder = false)
        {
            var ret = false;
            ManageArchive.UnpackArchivesToSubfoldersWithSameName(dir, ".zip");
            foreach (var oSubDir in Directory.GetDirectories(dir, "*"))
            {
                ret = true;

                string newTarget = ManageFilesFoldersExtensions.MoveFolderToOneLevelUpIfItAloneAndReturnMovedFolderPath(oSubDir);
                string targetDirName = Path.GetFileName(newTarget);
                var resultTargetPath = ManageFilesFoldersExtensions.GetResultTargetDirPathWithNameCheck(targetFolder, targetDirName);

                Directory.Move(newTarget, resultTargetPath);
            }

            return ret;
        }
    }
}
