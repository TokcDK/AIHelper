namespace AIHelper.Install.Types.Directories.CardsFromDir.Cardframes
{
    /// <summary>
    /// character background image card
    /// </summary>
    class CharacterBackgroundCardInstaller : CardframesCardInstallerBase
    {
        protected override string targetFolderName => "Back";

        public override string[] Masks => new[] { "cb" };
    }
}
