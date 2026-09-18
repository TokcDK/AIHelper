namespace AIHelper.Install.Types.Directories.CardsFromDir
{
    /// <summary>
    /// studio scene cards
    /// </summary>
    class StudioSceneCardInstaller : CardsFromDirsInstallerBase
    {
        public override string[] Masks => new[] { "s" };

        protected override string TargetSuffix => "Scenes";
    }
}
