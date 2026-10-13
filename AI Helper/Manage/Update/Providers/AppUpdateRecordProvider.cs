using System.Collections.Generic;
using AIHelper.Manage.Update.SelfUpdate;
using ModUpdater.Core.Domain;

namespace AIHelper.Manage.Update.Providers
{
    /// <summary>
    /// AI Helper's own update record.
    /// <para>
    /// The application is updated in the same pass as the mods and Mod Organizer, and its
    /// record is hardcoded for the same reason Mod Organizer's is: it is not a mod, it has no
    /// meta.ini of its own to keep a record in, and there is exactly one of it.
    /// </para>
    /// </summary>
    internal sealed class AppUpdateRecordProvider : IUpdateRecordProvider
    {
        /// <summary>Owner of the repository the releases are published in.</summary>
        private const string Owner = "TokcDK";

        /// <summary>Repository the releases are published in.</summary>
        private const string Repository = "AIHelper";

        /// <summary>
        /// Prefix of the release file name.
        /// </summary>
        /// <remarks>
        /// The prefix is what makes the mask a mask: without it any file of the release would
        /// match, and the archive is not the only thing a release may carry. The separator in
        /// the name is a dot and not the space the archive is built with, because GitHub
        /// replaces the spaces of an uploaded asset name - "AI Helper 26.10.13.3.7z" is
        /// published as "AI.Helper.26.10.13.3.7z".
        /// </remarks>
        private const string FileStartsWith = "AI.Helper";

        /// <summary>Suffix of the release file name.</summary>
        private const string FileEndsWith = ".7z";

        /// <summary>
        /// The version is the release tag and not the file name. A release of this application
        /// is named "AI Helper &lt;version&gt;.7z" and published by GitHub as
        /// "AI.Helper&lt;version&gt;.7z", so the version is in both - but the tag is the one
        /// the repository is tagged with, while the file name is what a packer was told to
        /// call the archive. When they disagree, the tag is right.
        /// </summary>
        private const bool VersionFromFile = false;

        public IEnumerable<UpdatableFolder> GetUpdatableFolders()
        {
            return new[]
            {
                new UpdatableFolder(
                    // The new version is unpacked into the payload folder rather than over the
                    // application, so the folder being updated is that one. The engine installs
                    // into the install path of a target, and for this target that is the folder
                    // aihelper-updater reads from on the next start.
                    SelfUpdatePaths.PendingUpdateDirectory,
                    new GithubUpdateRecord(Owner, Repository, FileStartsWith, FileEndsWith, VersionFromFile,
                        // The payload folder is emptied before the release is unpacked into it,
                        // so that a file left by an earlier download cannot survive into the
                        // application. Nothing of this target is backed up - see
                        // ModFolderBackupService - because a copy of a release that is about to
                        // be applied is not a version anybody wants to go back to.
                        InstallKind.ExtractArchiveToCleanRoot),
                    UpdatableFolderKind.Application)
            };
        }
    }
}
