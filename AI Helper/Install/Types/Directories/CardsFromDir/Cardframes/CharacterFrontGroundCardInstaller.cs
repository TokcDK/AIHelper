namespace AIHelper.Install.Types.Directories.CardsFromDir.Cardframes
{
    /// <summary>
    /// character frontground image card
    /// </summary>
    class CharacterFrontgroundCardInstaller : CardframesCardInstallerBase
    {
        protected override string targetFolderName => "Front";

        public override string[] Masks => new[] { "cf" };
    }
}
