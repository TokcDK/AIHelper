namespace AIHelper.Install.Types.Directories.CardsFromDir
{
    /// <summary>
    /// папка "c" с координатами
    /// </summary>
    class ClothesCoordinatesCardInstaller : CardsFromDirsInstallerBase
    {
        protected override string targetFolderName => "coordinate";

        public override string[] Masks => new[] { "c" };

        protected override string TargetSuffix => "Coordinate";
    }
}
