using AIHelper.Install.Cards;
using AIHelper.Manage;
using System;
using System.IO;

namespace AIHelper.Install.Types.Files.Cards
{
    /// <summary>
    /// Installs png images as character cards
    /// </summary>
    /// <remarks>
    /// This installer has only the file name to go by, so it treats every png as a
    /// female character card. <see cref="CardTargetInstaller"/> runs first and takes
    /// the cards whose content it could recognize, which leaves only the unknown
    /// images here.
    /// </remarks>
    class PngInstaller : CardsInstallerBase
    {
        private const string ImageExtension = ".png";

        public override string[] Masks => new[] { "*" + ImageExtension };

        public override string ContentDescription => "Card images (*.png) which content could not be recognized";

        protected override bool Get(FileInfo pngInfo)
        {
            //var imgdata = Image.FromFile(img);

            //if (imgdata.Width == 252 && imgdata.Height == 352)
            //{
            //    IsCharaCard = true;
            //}

            if (CardTargetRegistry.IsClaimed(pngInfo.FullName))
            {
                return false;
            }

            try
            {
                var targetImagePath = ManageFilesFoldersExtensions.GetResultTargetFilePathWithNameCheck(
                    ManageModOrganizer.GetUserDataSubFolder(" Chars", "f"), Path.GetFileNameWithoutExtension(pngInfo.Name), ImageExtension);

                pngInfo.MoveTo(targetImagePath);
            }
            catch (Exception ex)
            {
                _log.Warn("Failed to install png. path:" + pngInfo.FullName + "\r\nError:" + ex);
                return false;
            }

            return true;
        }
    }
}
