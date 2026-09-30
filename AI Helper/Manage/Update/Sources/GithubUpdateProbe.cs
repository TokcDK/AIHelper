using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ModUpdater.Core.Domain;
using ModUpdater.Core.Infrastructure.Install;
using ModUpdater.Core.Infrastructure.Sources;
using NLog;

namespace AIHelper.Manage.Update.Sources
{
    /// <summary>
    /// Looks a release up for the mod info editor.
    /// <para>
    /// The editor needs the version of a mod that is not installed yet, which the
    /// update pass never does: a pass starts from folders that exist. The lookup
    /// itself is the engine's, so what is left here is turning a record into the
    /// question and putting the answer somewhere.
    /// </para>
    /// </summary>
    internal sealed class GithubUpdateProbe
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly IUpdateSource _source;

        internal GithubUpdateProbe(IUpdateSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            _source = source;
        }

        /// <summary>
        /// Finds the newest release file that matches the record.
        /// </summary>
        /// <param name="record">Where to look and what to look for.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The release file, or null when the repository has no match.</returns>
        internal async Task<VersionInfo> FindLatestAsync(GithubUpdateRecord record, CancellationToken ct)
        {
            if (record == null || !record.IsValid) return null;

            // The version already installed is not part of the question: the caller
            // decides whether what comes back is newer than what it has.
            var versions = await _source
                .GetLatestVersionsAsync(record.ToSourceConfig(null), ct)
                .ConfigureAwait(false);

            return versions.Count > 0 ? versions[0] : null;
        }

        /// <summary>
        /// Downloads a release file and extracts it into a folder, which is how the
        /// editor adds a mod it has just looked up.
        /// </summary>
        /// <param name="version">Release file to install.</param>
        /// <param name="targetFolderPath">Folder to extract into.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>True when the folder holds the extracted release.</returns>
        internal async Task<bool> DownloadAndInstallAsync(
            VersionInfo version, string targetFolderPath, CancellationToken ct)
        {
            if (version == null || string.IsNullOrWhiteSpace(targetFolderPath)) return false;

            // The download is unpacked in a folder of its own, so that the file and
            // whatever the extractor leaves behind can both be removed in one step.
            var tempDirectoryPath = Path.Combine(
                Path.GetTempPath(), "AIHelperUpdate", Guid.NewGuid().ToString("N"));

            try
            {
                Directory.CreateDirectory(tempDirectoryPath);

                var tempFilePath = Path.Combine(tempDirectoryPath, Path.GetFileName(version.FileName));

                var download = await _source
                    .DownloadAsync(version, tempFilePath, null, ct)
                    .ConfigureAwait(false);

                if (!download.Success)
                {
                    _log.Warn("Update: failed to download '" + version.FileName + "'. " + download.ErrorMessage);
                    return false;
                }

                Directory.CreateDirectory(targetFolderPath);

                var context = new InstallContext
                {
                    DownloadedFilePath = download.FilePath,
                    TargetInstallPath = targetFolderPath,
                    Asset = version.MatchedAsset,
                    Version = version
                };

                var result = await new ArchiveInstaller().InstallAsync(context, ct).ConfigureAwait(false);

                if (!result.Success)
                {
                    _log.Warn("Update: failed to extract '" + version.FileName + "'. " + result.ErrorMessage);
                }

                return result.Success;
            }
            finally
            {
                TryDeleteDirectory(tempDirectoryPath);
            }
        }

        private static void TryDeleteDirectory(string directoryPath)
        {
            try
            {
                if (Directory.Exists(directoryPath)) Directory.Delete(directoryPath, true);
            }
            catch (Exception ex)
            {
                _log.Debug("Update: failed to delete the temporary folder '" + directoryPath
                           + "'. error:" + Environment.NewLine + ex);
            }
        }
    }
}
