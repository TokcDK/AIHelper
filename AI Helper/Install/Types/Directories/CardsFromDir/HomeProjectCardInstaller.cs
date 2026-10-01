using AIHelper.Manage;
using System.IO;

namespace AIHelper.Install.Types.Directories.CardsFromDir
{
    /// <summary>
    /// ai girl home project cards
    /// </summary>
    class HomeProjectCardInstaller : CardsFromDirsInstallerBase
    {
        public override string[] Masks => new[] { "h", "h1", "h2", "h3", "h4" };

        public override string ContentDescription => "Home project cards (folders 'h', 'h1'..'h4')";

        protected override string TargetSuffix => "Housing";

        protected override bool MoveByContentType(string contentType, string dir, string targetFolder, bool moveInThisFolder = false)
        {
            var anyMoved = false;

            if (contentType.Length == 2)
            {
                foreach (var file in Directory.GetFiles(dir))
                {
                    File.Move(file, ManageFilesFoldersExtensions.GetResultTargetFilePathWithNameCheck(
                        targetFolder, Path.GetFileNameWithoutExtension(file), CardsImageExtension));

                    anyMoved = true;
                }
            }
            else
            {
                foreach (var typeDir in Directory.GetDirectories(dir))
                {
                    string hSubDirName = Path.GetFileName(typeDir);
                    string targetSubDir = Path.Combine(targetFolder, hSubDirName);
                    if (!Directory.Exists(targetSubDir))
                    {
                        Directory.CreateDirectory(targetSubDir);
                    }

                    foreach (var file in Directory.GetFiles(typeDir))
                    {
                        File.Move(file, ManageFilesFoldersExtensions.GetResultTargetFilePathWithNameCheck(
                            targetSubDir, Path.GetFileNameWithoutExtension(file), CardsImageExtension));

                        anyMoved = true;
                    }
                }
            }

            return anyMoved;
        }

        private const string CardsImageExtension = ".png";
    }
}
