namespace AIHelper.Install.Types.Directories.CardsFromDir
{
    /// <summary>
    /// studio scene cards
    /// </summary>
    class StudioSceneCardInstaller : CardsFromDirsInstallerBase
    {
        protected override string typeFolder => "studio";
        protected override string targetFolderName => "scene";
        public override string[] Masks => new[] { "s" };

        protected override string TargetSuffix => "Scenes";
    }
}
