using AIHelper.Install.Cards;
using AIHelper.Manage;
using System;
using System.IO;

namespace AIHelper.Install.Types.Files.Cards
{
    /// <summary>
    /// Moves card files to the folder matching what they actually contain instead
    /// of the folder their file name suggests.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The other card installers of this folder only have the file name to go by,
    /// so a card dropped as <c>card_01.png</c> ends up among the female characters
    /// even when it is a male card, a coordinate or a studio scene. This installer
    /// reads the tag the game wrote into the file itself (see
    /// <see cref="CardFileDetector"/>) and files the card by that.
    /// </para>
    /// <para>
    /// A card which belongs to another game than the selected one is not filed
    /// among the cards of the selected game, which would never look at it. It goes
    /// into a sub folder named after the game it belongs to (see
    /// <see cref="CardGameMatcher"/>), so it stays apart and can be moved to that
    /// game.
    /// </para>
    /// <para>
    /// It is ordered behind the archive extractors, so it sees the cards which were
    /// just unpacked, and in front of every other card installer, so those only get
    /// the files whose content could not be recognized. Cards it moved are
    /// registered in <see cref="CardTargetRegistry"/>, which is what lets the later
    /// installers skip them.
    /// </para>
    /// </remarks>
    class CardTargetInstaller : CardsInstallerBase
    {
        private const string CardImageExtension = ".png";

        /// <summary>
        /// Runs behind the archive extractors (<c>50</c>) and in front of the other
        /// card installers (<see cref="FilesInstallerBase"/> is <c>500</c>, the
        /// directory installers are <c>1000</c>).
        /// </summary>
        public override int Order => 62;

        public override string[] Masks => new[] { "*" + CardImageExtension };

        protected override bool Get(FileInfo cardInfo)
        {
            CardFileInfo card;
            try
            {
                card = CardFileDetector.Detect(cardInfo.FullName);
            }
            catch (Exception ex)
            {
                _log.Warn("Failed to read card header. path:" + cardInfo.FullName + "\r\nError:" + ex);
                return false;
            }

            // not a card, or a card whose kind could not be read: leave it to the
            // installers which work by file name
            if (card == null || !card.IsDetected)
            {
                return false;
            }

            string targetFolder;
            string foreignGameId;
            if (!CardTargetFolderResolver.TryGetTargetFolder(card, out targetFolder, out foreignGameId))
            {
                return false;
            }

            // read the source path before the move: FileInfo.MoveTo repoints the
            // instance at the destination, so FullName is not the source any more
            // once the card has been moved
            var sourcePath = cardInfo.FullName;

            try
            {
                // the game folder of a card of another game does not exist yet
                Directory.CreateDirectory(targetFolder);

                var targetPath = ManageFilesFoldersExtensions.GetResultTargetFilePathWithNameCheck(
                    targetFolder, Path.GetFileNameWithoutExtension(cardInfo.Name), CardImageExtension);

                cardInfo.MoveTo(targetPath);

                CardTargetRegistry.Register(sourcePath);

                _log.Info("Card filed by content. " + card + " from:" + sourcePath + " to:" + targetPath
                    + (string.IsNullOrEmpty(foreignGameId) ? string.Empty : " (card of another game:" + foreignGameId + ")"));

                return true;
            }
            catch (Exception ex)
            {
                _log.Warn("Failed to install card. path:" + sourcePath + "\r\nError:" + ex);
                return false;
            }
        }
    }
}
