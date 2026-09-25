namespace AIHelper.Games.Illusion.IllGames
{
    /// <summary>
    /// Marker base for the Illusion titles that run on the newer engine
    /// generation. Kept abstract on purpose: the game list is built from every
    /// concrete <see cref="GameBase"/> subclass found by reflection, so an
    /// abstract intermediate class is never offered to the user.
    /// </summary>
    internal abstract class IllGameBase : IllusionGameBase
    {
    }
}
