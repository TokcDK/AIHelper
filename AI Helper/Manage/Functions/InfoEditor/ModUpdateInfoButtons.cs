using AIHelper.Manage.Update;

namespace AIHelper.Manage.Functions.InfoEditor
{
    /// <summary>
    /// A button of one mod info block: what it says, what it explains and where it
    /// takes the user.
    /// </summary>
    /// <remarks>
    /// The target is asked for when the button is pressed rather than kept in the
    /// button, so that a block which was built before its coordinates were read shows
    /// the coordinates it ends up with.
    /// </remarks>
    internal interface IModUpdateInfoButton
    {
        string Text { get; }

        string Description { get; }

        string GetProcessStartTarget(UpdateInfoData infoData);
    }

    /// <summary>
    /// Opens the page of the newest release of the mod.
    /// </summary>
    internal sealed class OpenWebPage : IModUpdateInfoButton
    {
        public string Text => T._("Web");

        public string Description => T._("Open web page of the mod");

        public string GetProcessStartTarget(UpdateInfoData infoData)
        {
            return GithubUrl.BuildRepositoryUrl(
                infoData.GitInfo.Site,
                infoData.GitInfo.Owner,
                infoData.GitInfo.Repository) + "/releases/latest";
        }
    }

    /// <summary>
    /// Opens the folder of the mod.
    /// </summary>
    internal sealed class OpenModDir : IModUpdateInfoButton
    {
        public string Text => T._("Dir");

        public string Description => T._("Open directory of the mod");

        public string GetProcessStartTarget(UpdateInfoData infoData)
        {
            // A mod the editor could not read meta.ini for has no folder to open.
            var ini = infoData.GitInfo.INI;

            return ini == null ? null : ini.INIPath.DirectoryName;
        }
    }
}
