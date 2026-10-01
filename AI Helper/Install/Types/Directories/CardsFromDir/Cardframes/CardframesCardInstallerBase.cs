namespace AIHelper.Install.Types.Directories.CardsFromDir.Cardframes
{
    /// <summary>
    /// Base class for the installers which are moving cardframe images of a selected
    /// content type from the folder of that type
    /// </summary>
    /// <remarks>
    /// Abstract on purpose, like its <c>CharacterCardInstallerBase</c> sibling: it declares
    /// no masks of its own, so a concrete instance of it would inherit the "any folder" mask
    /// of the base and move the images of every folder of the install dir into the UserData
    /// root instead of the folder of the type. The installers are created by reflection,
    /// which skips abstract types, so this is what keeps it out of a run.
    /// </remarks>
    abstract class CardframesCardInstallerBase : CardsFromDirsInstallerBase
    {
        protected override string TargetSuffix => "Cardframes";
    }
}
