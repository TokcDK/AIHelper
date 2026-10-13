using System;
using System.Threading;
using System.Threading.Tasks;
using AIHelper.Manage.Update.Sources;
using ModUpdater.Core.Domain;
using ModUpdater.Core.Infrastructure.Sources;
using ModUpdater.Core.Infrastructure.Versioning;
using NLog;

namespace AIHelper.Manage.Update.SelfUpdate
{
    /// <summary>
    /// The application's own update, as far as it can be done while the application runs:
    /// find the newest release of AI Helper on GitHub and leave it unpacked in
    /// RES\updates\appupdate.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The lookup and the download are the update engine's, the same one the mods are updated
    /// through - the source, the file name mask, the version comparer and the archive
    /// extractor all come from there. What is added here is only what makes the release being
    /// looked for the application's own: which repository it is, which file of a release it is,
    /// which version is already installed, and where the result goes.
    /// </para>
    /// <para>
    /// The version is the release tag and not the file name. A release of this application is
    /// named "AI Helper &lt;version&gt;.7z" and published by GitHub as "AI.Helper&lt;version&gt;.7z",
    /// so the version is in both - but the tag is the one the repository is tagged with, while
    /// the file name is what a packer was told to call the archive. When they disagree, the tag
    /// is right.
    /// </para>
    /// <para>
    /// Nothing here touches the running program. Applying the update needs the application to
    /// be closed, and the program that does it is aihelper-updater - see
    /// <see cref="PendingUpdateLauncher"/>. Everything up to the download is therefore the
    /// whole of what this class does.
    /// </para>
    /// </remarks>
    internal sealed class SelfUpdateService
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// What to look for on GitHub: the repository, and the release file of the application.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The version is taken from the tag (<c>versionFromFile: false</c>), which is also what
        /// makes the file name mask a mask: <c>AI.Helper*.7z</c> matches the archive of every
        /// release and the version comes from the release itself.
        /// </para>
        /// <para>
        /// The install kind is the clean extraction, because the folder is unpacked into is
        /// replaced rather than added to: a file of an earlier download that the new release
        /// does not have must not survive into the application. The engine's extractor is not
        /// used to install anything here - the folder it fills is the payload, and the program
        /// that moves the payload into the application is a separate one.
        /// </para>
        /// </remarks>
        private static readonly GithubUpdateRecord Release = new GithubUpdateRecord(
            SelfUpdatePaths.RepositoryOwner,
            SelfUpdatePaths.RepositoryName,
            SelfUpdatePaths.ReleaseFileStartsWith,
            SelfUpdatePaths.ReleaseFileEndsWith,
            versionFromFile: false,
            installKind: InstallKind.ExtractArchiveToCleanRoot);

        private readonly GithubUpdateProbe _probe;

        private readonly IVersionComparer _comparer = new SelfUpdateVersionComparer();

        /// <param name="source">Source the releases are read from.</param>
        internal SelfUpdateService(IUpdateSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            _probe = new GithubUpdateProbe(source);
        }

        /// <summary>
        /// Looks for a release newer than the installed version.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The engine returns the newest release whose files match, and the comparison against
        /// what is installed is made here rather than passed to the source: the source of this
        /// application answers "the newest release", and "is that newer than mine" is a question
        /// about the application, answered by the rule in
        /// <see cref="SelfUpdateVersionComparer"/>.
        /// </para>
        /// <para>
        /// The version asked about is the one the next start will be running, which is the waiting
        /// update when there is one - see <see cref="ApplicationVersion.ReadInstalled"/>. Without
        /// that, a session that had already downloaded a release would offer the same release
        /// again.
        /// </para>
        /// </remarks>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The release to install, or null when there is nothing newer.</returns>
        internal async Task<VersionInfo> FindUpdateAsync(CancellationToken ct)
        {
            var installedVersion = ApplicationVersion.ReadInstalled();

            var latest = await _probe.FindLatestAsync(Release, ct).ConfigureAwait(false);

            if (latest == null)
            {
                _log.Info("Self update: no release matching '" + Release.ToFileNameMask()
                          + "' was found in " + SelfUpdatePaths.RepositoryOwner + "/"
                          + SelfUpdatePaths.RepositoryName + ".");
                return null;
            }

            if (_comparer.Compare(installedVersion, latest.VersionString) <= 0)
            {
                _log.Info("Self update: AI Helper is up to date. installed '" + installedVersion
                          + "', newest release '" + latest.VersionString + "'.");
                return null;
            }

            _log.Info("Self update: '" + latest.VersionString + "' is newer than the installed '"
                      + installedVersion + "'.");

            return latest;
        }

        /// <summary>
        /// Downloads a release and unpacks it into the waiting update folder.
        /// </summary>
        /// <remarks>
        /// The download is unpacked into a folder of its own first and only then extracted, so
        /// that a download which failed cannot leave a half-written archive among the files of
        /// the application. The folder the release is unpacked into is emptied before the
        /// extraction, which is what the install kind above asks the engine's extractor for.
        /// </remarks>
        /// <param name="version">Release to download.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>True when a complete update is waiting in the payload folder.</returns>
        internal async Task<bool> DownloadAsync(VersionInfo version, CancellationToken ct)
        {
            if (version == null) return false;

            _log.Info("Self update: downloading '" + version.FileName + "' into '"
                      + SelfUpdatePaths.PendingUpdateDirectory + "'.");

            var installed = await _probe
                .DownloadAndInstallAsync(version, SelfUpdatePaths.PendingUpdateDirectory, ct)
                .ConfigureAwait(false);

            if (!installed)
            {
                _log.Error("Self update: '" + version.FileName + "' could not be downloaded and unpacked.");
                return false;
            }

            // The extraction reporting success is not the same as the folder holding an update:
            // an archive whose layout is not the one expected - a release packed one folder too
            // deep, an archive that was not the release at all - unpacks perfectly well and is
            // still not something the tool may be handed.
            if (!PendingUpdate.IsReady)
            {
                _log.Error("Self update: '" + version.FileName + "' was unpacked into '"
                           + SelfUpdatePaths.PendingUpdateDirectory + "' but the folder does not hold "
                           + "the program and a RES folder, so it is not an update.");
                return false;
            }

            _log.Info("Self update: '" + version.VersionString + "' is waiting to be applied on the next start.");
            return true;
        }
    }
}
