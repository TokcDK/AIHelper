using AIHelper.Manage;
using System.IO;

namespace AIHelper.Install.Cards
{
    /// <summary>
    /// Maps a detected card to the UserData folder the card belongs to.
    /// </summary>
    /// <remarks>
    /// This is the only part of the detection which knows about the application:
    /// <see cref="CardFileDetector"/> stays free of it, and the folder names and
    /// the type codes are reused from the installers which already move cards by
    /// their file name, so both routes land in the same place.
    /// </remarks>
    internal static class CardTargetFolderResolver
    {
        /// <summary>
        /// Returns the folder a detected card must be moved to.
        /// </summary>
        /// <param name="card">Detected card</param>
        /// <param name="targetFolder">Folder the card belongs to</param>
        /// <param name="foreignGameId">
        /// Id of the game the card belongs to when that is not the selected game,
        /// otherwise null
        /// </param>
        /// <returns>False when the kind of the card has no known target folder</returns>
        internal static bool TryGetTargetFolder(CardFileInfo card, out string targetFolder, out string foreignGameId)
        {
            targetFolder = null;
            foreignGameId = null;

            if (card == null) return false;

            string typeFolder;
            if (!TryGetTypeFolder(card.Kind, out typeFolder)) return false;

            // A card of another game must not land among the cards of the selected
            // game, which would never look at it. It goes into a folder named after
            // the game it actually belongs to, so it stays apart and can be moved to
            // that game later.
            if (CardGameMatcher.TryGetForeignGameId(card.Marker, out foreignGameId))
            {
                targetFolder = Path.Combine(typeFolder, foreignGameId);
                return true;
            }

            targetFolder = typeFolder;
            return true;
        }

        /// <summary>
        /// Returns the folder of the selected game a card kind belongs to.
        /// </summary>
        /// <param name="kind">Detected content kind of the card</param>
        /// <param name="targetFolder">Folder the card belongs to</param>
        /// <returns>False when the kind has no known target folder</returns>
        private static bool TryGetTypeFolder(CardContentKind kind, out string targetFolder)
        {
            switch (kind)
            {
                case CardContentKind.FemaleCharacter:
                    targetFolder = ManageModOrganizer.GetUserDataSubFolder(" Chars", "f");
                    return true;

                case CardContentKind.MaleCharacter:
                    targetFolder = ManageModOrganizer.GetUserDataSubFolder(" Chars", "m");
                    return true;

                case CardContentKind.Coordinate:
                    targetFolder = ManageModOrganizer.GetUserDataSubFolder(" Coordinate", "c");
                    return true;

                case CardContentKind.StudioScene:
                    targetFolder = ManageModOrganizer.GetUserDataSubFolder(" Scenes", "s");
                    return true;

                default:
                    targetFolder = null;
                    return false;
            }
        }
    }
}
