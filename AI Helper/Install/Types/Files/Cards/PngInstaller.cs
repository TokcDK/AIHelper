using AIHelper.Manage;
using System.IO;

namespace AIHelper.Install.Types.Files.Cards
{
    class PngInstaller : FilesInstallerBase
    {
        public override string[] Masks => new[] { "*" + ext };

        const string ext = ".png";

        protected override bool Get(FileInfo pngInfo)
        {
            //var imgdata = Image.FromFile(img);

            //if (imgdata.Width == 252 && imgdata.Height == 352)
            //{
            //    IsCharaCard = true;
            //}

            try
            {
                var targetImagePath = ManageFilesFoldersExtensions.GetResultTargetFilePathWithNameCheck(ManageModOrganizerMods.GetUserDataSubFolder(" Chars", "f"), Path.GetFileNameWithoutExtension(pngInfo.Name), ext);

                pngInfo.MoveTo(targetImagePath);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
