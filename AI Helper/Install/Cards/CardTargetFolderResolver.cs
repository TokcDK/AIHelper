using AIHelper.Manage;

namespace AIHelper.Install.Cards
{
    /// <summary>
    /// Maps a detected card content kind to the UserData folder the card belongs
    /// to.
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
        /// Returns the folder the card of the passed kind must be moved to.
        /// </summary>
        /// <param name="kind">Detected content kind of the card</param>
        /// <param name="targetFolder">Folder the card belongs to</param>
        /// <returns>False when the kind has no known target folder</returns>
        internal static bool TryGetTargetFolder(CardContentKind kind, out string targetFolder)
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
