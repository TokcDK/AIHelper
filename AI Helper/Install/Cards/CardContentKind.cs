namespace AIHelper.Install.Cards
{
    /// <summary>
    /// Kind of user data a card file contains.
    /// </summary>
    /// <remarks>
    /// The names follow the content, not the game: several games share one card
    /// format (AI Girl and HoneySelect 2 both write <c>【AIS_Chara】</c> cards), so
    /// the format can tell what a card is but not always which game made it. The
    /// target folder only needs the kind.
    /// </remarks>
    internal enum CardContentKind
    {
        /// <summary>
        /// Not a card, a card of an unknown game, or a card whose kind could not
        /// be read. Such a file must be left for the other installers.
        /// </summary>
        Unknown = 0,

        /// <summary>Character card of a female character.</summary>
        FemaleCharacter,

        /// <summary>Character card of a male character.</summary>
        MaleCharacter,

        /// <summary>Clothes / coordinate card.</summary>
        Coordinate,

        /// <summary>Studio scene.</summary>
        StudioScene,
    }
}
