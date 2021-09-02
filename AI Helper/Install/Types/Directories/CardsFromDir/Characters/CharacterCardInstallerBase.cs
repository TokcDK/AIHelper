namespace AIHelper.Install.Types.Directories.CardsFromDir.Characters
{
    abstract class CharacterCardInstallerBase : CardsFromDirsInstallerBase
    {
        protected override string typeFolder => "chara";
        protected override string TargetSuffix => "Chars";
    }
}
