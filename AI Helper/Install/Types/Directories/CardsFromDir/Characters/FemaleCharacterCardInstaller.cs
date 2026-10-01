namespace AIHelper.Install.Types.Directories.CardsFromDir.Characters
{
    /// <summary>
    /// папка с женскими карточками внутри
    /// </summary>
    class FemaleCharacterCardInstaller : CharacterCardInstallerBase
    {
        public override string[] Masks => new[] { "f" };

        public override string ContentDescription => "Female character cards (folder 'f')";

        protected override bool MoveByContentType(string contentType, string fDir, string targetFolder, bool moveInThisFolder = false)
        {
            // dir 'f' can contain the 'm' subdir with male cards, so they must be installed by the male cards installer
            return new MaleCharacterCardInstaller().InstallFrom(fDir);
        }
    }
}
