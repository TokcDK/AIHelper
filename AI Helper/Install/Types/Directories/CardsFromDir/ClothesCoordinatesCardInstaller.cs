namespace AIHelper.Install.Types.Directories.CardsFromDir
{
    /// <summary>
    /// папка "c" с координатами
    /// </summary>
    class ClothesCoordinatesCardInstaller : CardsFromDirsInstallerBase
    {
        public override string[] Masks => new[] { "c" };

        public override string ContentDescription => "Coordinate cards (folder 'c')";

        protected override string TargetSuffix => "Coordinate";
    }
}
