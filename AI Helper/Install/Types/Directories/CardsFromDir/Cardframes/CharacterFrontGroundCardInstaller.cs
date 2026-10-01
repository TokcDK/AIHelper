namespace AIHelper.Install.Types.Directories.CardsFromDir.Cardframes
{
    /// <summary>
    /// character frontground image card
    /// </summary>
    class CharacterFrontgroundCardInstaller : CardframesCardInstallerBase
    {
        public override string[] Masks => new[] { "cf" };

        public override string ContentDescription => "Character foreground cards (folder 'cf')";
    }
}
