namespace AIHelper.Install.Types.Directories.CardsFromDir.Characters
{
    /// <summary>
    /// папка с женскими карточками внутри
    /// </summary>
    class FemaleCharacterCardInstaller : CharacterCardInstallerBase
    {
        protected override string targetFolderName => "female";

        public override string[] Masks => new[] { "f" };

        protected override bool MoveByContentType(string contentType, string fDir, string targetFolder, bool moveInThisFolder = false)
        {
            // dont understand why but this was in old code. it looks like move cards from male 'm' subfolder which is in 'f' dir
            return new MaleCharacterCardInstaller().InstallFrom(fDir);
        }
    }
}
