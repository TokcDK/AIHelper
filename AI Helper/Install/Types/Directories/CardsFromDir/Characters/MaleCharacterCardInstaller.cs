namespace AIHelper.Install.Types.Directories.CardsFromDir.Characters
{
    /// <summary>
    /// папка с мужскими карточками внутри
    /// </summary>
    class MaleCharacterCardInstaller : CharacterCardInstallerBase
    {
        protected override string targetFolderName => "male";
        public override string[] Masks => new[] { "m" };
    }
}
