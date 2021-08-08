using AIHelper.Manage;
using System.IO;

namespace AIHelper.Install.Types.Files.Cards
{
    class PngInstaller : FilesInstallerBase
    {
        public override string Mask => "*" + ext;

        const string ext = ".png";

        protected override void Get(FileInfo pngInfo)
        {
            //var imgdata = Image.FromFile(img);

            //if (imgdata.Width == 252 && imgdata.Height == 352)
            //{
            //    IsCharaCard = true;
            //}

            var targetImagePath = ManageFilesFolders.GetResultTargetFilePathWithNameCheck(ManageModOrganizerMods.GetUserDataSubFolder(" Chars", "f"), Path.GetFileNameWithoutExtension(pngInfo.Name), ext);

            pngInfo.MoveTo(targetImagePath);
        }
    }
}
