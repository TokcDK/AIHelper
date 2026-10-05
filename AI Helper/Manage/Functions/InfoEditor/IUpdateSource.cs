using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AIHelper.Data.Modlist;

namespace AIHelper.Manage.Functions.InfoEditor
{
    /// <summary>
    /// A place mods are updated from.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The editor knows no host of its own: it asks every source whether it holds data for a
    /// mod, gives each source which does a tab in that mod's block, and lets the source build
    /// both the control which edits the data and the control which adds a new mod of that
    /// kind. Adding a host is therefore an implementation plus one line in
    /// <see cref="UpdateSourceCatalog"/> rather than a change to the editor.
    /// </para>
    /// <para>
    /// What a source deliberately does not own is the mods folder. It downloads and installs
    /// into a folder it is handed, and the editor decides which folder that is, whether the
    /// name may be used at all, and what is written into the mod afterwards.
    /// </para>
    /// </remarks>
    internal interface IUpdateSource
    {
        /// <summary>Caption of the tabs which belong to this source.</summary>
        string Name { get; }

        /// <summary>
        /// Reads the data this source holds for a mod out of the mod's own files.
        /// </summary>
        /// <param name="mod">Mod to read.</param>
        /// <param name="data">Data that was read, or null when there is none.</param>
        /// <returns>True when the mod carries data of this source.</returns>
        bool TryReadData(ModData mod, out UpdateSourceData data);

        /// <summary>
        /// Builds the control which shows and edits this source's data of one mod.
        /// </summary>
        /// <param name="data">Data the control edits.</param>
        /// <param name="toolTip">Tooltip the control explains its fields with.</param>
        UpdateSourceInfoControl CreateInfoControl(UpdateSourceData data, ToolTip toolTip);

        /// <summary>
        /// Builds the control which adds a new mod of this source.
        /// </summary>
        /// <param name="log">
        /// Called with a line for the editor's log box, so that a source can report what it
        /// is doing without knowing where that box is.
        /// </param>
        UpdateSourceAddControl CreateAddControl(Action<string> log);

        /// <summary>
        /// The url of the page a mod of this source is looked at, or null when there is none.
        /// </summary>
        /// <param name="data">Data of the mod whose page is wanted.</param>
        string GetWebUrl(UpdateSourceData data);

        /// <summary>
        /// Downloads the release the data points at and installs it into a folder.
        /// </summary>
        /// <remarks>
        /// The lookup and the download are the update engine's; what a source owns here is the
        /// ordering this application needs — resolve the version first, then put the files in
        /// place — so that a failed download never leaves a mod claiming to be updatable.
        /// </remarks>
        /// <param name="data">Data which says what to download.</param>
        /// <param name="targetFolderPath">Folder to install into.</param>
        /// <param name="log">Called with a line for the editor's log box.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True when the folder holds the installed release.</returns>
        Task<bool> DownloadAndInstallAsync(UpdateSourceData data, string targetFolderPath,
            Action<string> log, CancellationToken cancellationToken);
    }

    /// <summary>
    /// The sources the editor offers, in the order their tabs are shown.
    /// </summary>
    internal static class UpdateSourceCatalog
    {
        private static readonly IUpdateSource[] Registered =
        {
            GithubUpdateSource.Instance,
        };

        internal static IReadOnlyList<IUpdateSource> All
        {
            get { return Registered; }
        }
    }
}
