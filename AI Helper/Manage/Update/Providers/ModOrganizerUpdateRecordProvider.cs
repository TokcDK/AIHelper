using System.Collections.Generic;

namespace AIHelper.Manage.Update.Providers
{
    /// <summary>
    /// Mod Organizer is not a mod: it has no folder below the mods directory and
    /// no meta.ini of its own, so its update record has always been hardcoded.
    /// <para>
    /// The target is produced even when Mod Organizer is not installed — that is
    /// how a missing installation gets offered for download.
    /// </para>
    /// </summary>
    internal sealed class ModOrganizerUpdateRecordProvider : IUpdateRecordProvider
    {
        /// <summary>Owner of the Mod Organizer repository.</summary>
        private const string Owner = "ModOrganizer2";

        /// <summary>Repository that publishes the Mod Organizer releases.</summary>
        private const string Repository = "modorganizer";

        /// <summary>
        /// Prefix of the installer file name. It keeps the source from picking up
        /// the other assets a release carries, such as the archive variants.
        /// </summary>
        private const string FileStartsWith = "Mod.Organizer-";

        /// <summary>Suffix of the installer file name.</summary>
        private const string FileEndsWith = ".exe";

        /// <summary>
        /// Mod Organizer publishes an installer rather than an archive, so the
        /// version comes from the release tag and not from the file name.
        /// </summary>
        private const bool VersionFromFile = false;

        public IEnumerable<UpdatableFolder> GetUpdatableFolders()
        {
            return new[]
            {
                new UpdatableFolder(
                    ManageSettings.AppModOrganizerDirPath,
                    new GithubUpdateRecord(Owner, Repository, FileStartsWith, FileEndsWith, VersionFromFile))
            };
        }
    }
}
