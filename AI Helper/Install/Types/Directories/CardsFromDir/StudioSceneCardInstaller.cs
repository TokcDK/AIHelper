namespace AIHelper.Install.Types.Directories.CardsFromDir
{
    /// <summary>
    /// studio scene cards
    /// </summary>
    class StudioSceneCardInstaller : CardsFromDirsInstallerBase
    {
        public override string[] Masks => new[] { "s" };

        public override string ContentDescription => T._("Studio scene cards (folder 's')");

        protected override string TargetSuffix => "Scenes";
    }
}
