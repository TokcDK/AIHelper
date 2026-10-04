using System;
using System.Collections.Generic;
using ModUpdater.Core.Domain;

namespace AIHelper.Manage.Update
{
    /// <summary>
    /// GitHub coordinates of one updatable folder, in the shape this application
    /// has always kept them in meta.ini and updateinfo.txt:
    /// <c>owner,repository,fileStartsWith,fileEndsWith,versionFromFile</c>.
    /// <para>
    /// The record is the application's own format, so it lives here and not in the
    /// update engine: the engine only understands a repository plus a file mask,
    /// and translating one into the other is the adapter's job.
    /// </para>
    /// </summary>
    internal sealed class GithubUpdateRecord
    {
        /// <summary>Owner of the GitHub repository.</summary>
        internal string Owner { get; }

        /// <summary>Repository that publishes the releases.</summary>
        internal string Repository { get; }

        /// <summary>Prefix every candidate release file name starts with.</summary>
        internal string FileStartsWith { get; }

        /// <summary>
        /// Suffix the candidate release file name ends with. Empty when the record
        /// does not constrain the name at all.
        /// </summary>
        internal string FileEndsWith { get; }

        /// <summary>
        /// True when the version has to be read out of the file name instead of
        /// being taken from the release tag.
        /// </summary>
        internal bool VersionFromFile { get; }

        /// <summary>
        /// How the release file of this record is installed.
        /// <para>
        /// A record read from a mod's own file describes a mod archive, so the default
        /// is the clean extraction a mod folder needs: the new version must not be
        /// unpacked over the files of the old one. Mod Organizer is not a mod and its
        /// record is hardcoded, so it asks for <see cref="InstallKind.RunInstaller"/>
        /// instead — and that request is the only thing keeping the archive installer
        /// away from it, because the engine's resolver takes the first installer that
        /// accepts the asset.
        /// </para>
        /// </summary>
        internal InstallKind InstallKind { get; }

        /// <summary>
        /// Command line for <see cref="InstallKind.RunInstaller"/>. The engine replaces
        /// the <c>{installPath}</c> token with the target's install path. Unused by the
        /// other install kinds.
        /// </summary>
        internal string InstallArguments { get; }

        /// <param name="installKind">
        /// How to install the file. The default is what a record read from a mod's
        /// meta.ini or updateinfo.txt describes.
        /// </param>
        /// <param name="installArguments">
        /// Command line for <see cref="InstallKind.RunInstaller"/>, or null.
        /// </param>
        internal GithubUpdateRecord(string owner, string repository, string fileStartsWith,
            string fileEndsWith, bool versionFromFile,
            InstallKind installKind = InstallKind.ExtractArchiveToCleanRoot,
            string installArguments = null)
        {
            Owner = (owner ?? string.Empty).Trim();
            Repository = (repository ?? string.Empty).Trim();
            FileStartsWith = (fileStartsWith ?? string.Empty).Trim();
            FileEndsWith = (fileEndsWith ?? string.Empty).Trim();
            VersionFromFile = versionFromFile;
            InstallKind = installKind;
            InstallArguments = installArguments;
        }

        /// <summary>
        /// True when the record says enough to look a release up.
        /// <para>
        /// The file name prefix is part of the requirement, not a nicety: without
        /// it the mask would match any file of the release and the source archive
        /// would be picked up as readily as the plugin.
        /// </para>
        /// </summary>
        internal bool IsValid
        {
            get { return Owner.Length > 0 && Repository.Length > 0 && FileStartsWith.Length > 0; }
        }

        /// <summary>
        /// File name mask for the engine, where '*' is the dynamic (version) part.
        /// <para>Example: ("BepInEx", "_x64.zip") becomes "BepInEx*_x64.zip".</para>
        /// </summary>
        internal string ToFileNameMask()
        {
            return FileStartsWith + "*" + FileEndsWith;
        }

        /// <summary>
        /// Describes the record to the engine: where to look, how the version is
        /// spelled and how to install what is found.
        /// </summary>
        /// <param name="currentVersion">
        /// Version already installed, or null/empty when it is unknown. An unknown
        /// version means "any release found is newer", which is what the update
        /// check has always done.
        /// </param>
        internal GithubSourceConfig ToSourceConfig(string currentVersion)
        {
            return new GithubSourceConfig
            {
                Owner = Owner,
                Repository = Repository,
                VersionCheckMode = VersionFromFile ? VersionCheckMode.FileName : VersionCheckMode.Tag,

                // The release list is walked rather than "latest" trusted: the newest
                // release of a repository regularly carries no file matching the
                // record, while an older one does. The engine stops at the first
                // match walking from the newest release down, which is exactly the
                // fallback the hand written source used to perform.
                ScanAllReleases = true,

                CurrentVersion = string.IsNullOrWhiteSpace(currentVersion)
                    ? UpdateSourceConfig.DefaultCurrentVersion
                    : currentVersion,

                Assets = new List<AssetSpec>
                {
                    new AssetSpec
                    {
                        FileNameMask = ToFileNameMask(),

                        // How the file is installed is part of what the record says
                        // about itself: a mod archive into an emptied folder, the Mod
                        // Organizer installer by running it.
                        InstallKind = this.InstallKind,
                        InstallArguments = InstallArguments
                    }
                }
            };
        }

        /// <summary>
        /// True when both records point at the same release file. Used to refuse a
        /// second mod that would be fed by the release an existing mod already uses.
        /// </summary>
        internal bool PointsAtSameFileAs(GithubUpdateRecord other)
        {
            if (other == null) return false;

            return string.Equals(Owner, other.Owner, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Repository, other.Repository, StringComparison.OrdinalIgnoreCase)
                && string.Equals(FileStartsWith, other.FileStartsWith, StringComparison.OrdinalIgnoreCase)
                && string.Equals(FileEndsWith, other.FileEndsWith, StringComparison.OrdinalIgnoreCase);
        }
    }
}
