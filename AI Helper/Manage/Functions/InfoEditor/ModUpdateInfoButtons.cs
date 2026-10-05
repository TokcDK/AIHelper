namespace AIHelper.Manage.Functions.InfoEditor
{
    /// <summary>
    /// A button of one mod info block: what it says, what it explains and where it
    /// takes the user.
    /// </summary>
    /// <remarks>
    /// The target is asked for when the button is pressed rather than kept in the
    /// button, so that a block which was built before its data was read shows what it
    /// ends up with, and so that a button which follows a tab follows the tab the user
    /// is actually looking at.
    /// </remarks>
    internal interface IModUpdateInfoButton
    {
        string Text { get; }

        string Description { get; }

        /// <param name="infoData">The mod the block belongs to.</param>
        /// <param name="selectedSource">
        /// Data of the source whose tab is selected, or null when the block has no tab.
        /// </param>
        string GetProcessStartTarget(UpdateInfoData infoData, UpdateSourceData selectedSource);
    }

    /// <summary>
    /// Opens the page of the newest release of the mod, at the source whose tab is shown.
    /// </summary>
    internal sealed class OpenWebPage : IModUpdateInfoButton
    {
        public string Text => T._("Web");

        public string Description => T._("Open web page of the mod");

        public string GetProcessStartTarget(UpdateInfoData infoData, UpdateSourceData selectedSource)
        {
            // A page belongs to a host, and a mod may be listed at more than one, so the
            // button opens the page of the source the user is looking at rather than one
            // of them chosen here.
            return selectedSource == null ? null : selectedSource.Source.GetWebUrl(selectedSource);
        }
    }

    /// <summary>
    /// Opens the folder of the mod.
    /// </summary>
    internal sealed class OpenModDir : IModUpdateInfoButton
    {
        public string Text => T._("Dir");

        public string Description => T._("Open directory of the mod");

        public string GetProcessStartTarget(UpdateInfoData infoData, UpdateSourceData selectedSource)
        {
            // The folder belongs to the mod rather than to a source, so it is read from the
            // mod and not from the data of whichever tab happens to be selected.
            var ini = infoData.Mod.MetaIni;

            return ini == null ? null : ini.INIPath.DirectoryName;
        }
    }
}
