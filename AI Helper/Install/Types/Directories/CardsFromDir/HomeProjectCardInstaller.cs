using AIHelper.Manage;
using System.IO;

namespace AIHelper.Install.Types.Directories.CardsFromDir
{
    /// <summary>
    /// ai girl home project cards
    /// </summary>
    class HomeProjectCardInstaller : CardsFromDirsInstallerBase
    {
        protected override string typeFolder => "housing";

        protected override string targetFolderName => Mask.Length == 2 ? "0" + Mask.Substring(1) : string.Empty;

        public override string[] Masks => new[] { "h", "h1", "h2", "h3", "h4" };

        protected override string TargetSuffix => "Housing";

        protected override void MoveByContentType(string contentType, string dir, string targetFolder, bool moveInThisFolder = false)
        {
            if (contentType.Length == 2)
            {
                foreach (var file in Directory.GetFiles(dir))
                {
                    File.Move(file, ManageFilesFolders.GetResultTargetFilePathWithNameCheck(targetFolder, Path.GetFileNameWithoutExtension(file), ".png"));
                }
            }
            else
            {
                foreach (var typeDir in Directory.GetDirectories(dir))
                {
                    string hSubDirName = Path.GetFileName(typeDir);
                    foreach (var file in Directory.GetFiles(typeDir))
                    {
                        File.Move(file, ManageFilesFolders.GetResultTargetFilePathWithNameCheck(Path.Combine(targetFolder, hSubDirName), Path.GetFileNameWithoutExtension(file), ".png"));
                    }
                }
            }
        }
    }
}
