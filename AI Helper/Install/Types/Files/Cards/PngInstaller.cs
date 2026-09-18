using AIHelper.Manage;
using System;
using System.IO;

namespace AIHelper.Install.Types.Files.Cards
{
    /// <summary>
    /// Installs png images as character cards
    /// </summary>
    class PngInstaller : CardsInstallerBase
    {
        private const string ImageExtension = ".png";

        public override string[] Masks => new[] { "*" + ImageExtension };

        protected override bool Get(FileInfo pngInfo)
        {
            //var imgdata = Image.FromFile(img);

            //if (imgdata.Width == 252 && imgdata.Height == 352)
            //{
            //    IsCharaCard = true;
            //}

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
