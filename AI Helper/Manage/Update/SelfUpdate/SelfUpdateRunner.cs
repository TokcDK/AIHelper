using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ModUpdater.Core.Domain;
using NLog;

namespace AIHelper.Manage.Update.SelfUpdate
{
    /// <summary>
    /// The two moments AI Helper takes part in its own update: the check the update pass runs,
    /// and the hand-off the next start performs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The update is deliberately split across two sessions. A check that runs while the
    /// application is open can only get the new version as far as the disk - the files being
    /// replaced are the ones this process is running from - so it leaves the release in
    /// RES\updates\appupdate and stops there. The next start sees the folder, starts
    /// aihelper-updater and closes, and the tool does the rest while nothing of the application
    /// is in memory.
    /// </para>
    /// <para>
    /// Both halves live here because they are the same feature seen from either side, and
    /// because the second one is what the first one is for: the check is written to leave behind
    /// exactly what the hand-off looks for.
    /// </para>
    /// <para>
    /// No window is shown and no question is asked here. The check is started from the update
    /// options, which is where the user asked for it, and the hand-off happens on a start the
    /// user is not watching - the tool restarts the application when it is done, so the screen
    /// the user comes back to is the one they left.
    /// </para>
    /// </remarks>
    internal static class SelfUpdateRunner
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Checks GitHub for a newer AI Helper and, when there is one, downloads it and leaves it
        /// ready for the next start.
        /// </summary>
        /// <param name="reportStatus">
        /// Called with the phase the pass has reached, in the language of the interface, or null
        /// when there is nothing to report to.
        /// </param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>What the check came to.</returns>
        internal static async Task<SelfUpdateResult> PrepareAsync(Action<string> reportStatus, CancellationToken ct)
        {
            // The engine is assembled for this pass alone and owns the HTTP client it creates,
            // so it is disposed here - the same shape as the pass the mods are updated by.
            using (var engine = new UpdateEngineFactory())
            {
                var service = new SelfUpdateService(engine.GithubSource);

                Report(reportStatus, T._("Check AI Helper update") + "..");

                VersionInfo latest;
                try
                {
                    latest = await service.FindUpdateAsync(ct).ConfigureAwait(true);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Self update: the update check failed.");
                    return SelfUpdateResult.Failed(ex.Message);
                }

                if (latest == null) return SelfUpdateResult.UpToDate();

                Report(reportStatus, T._("Download AI Helper update") + "..");

                bool downloaded;
                try
                {
                    downloaded = await service.DownloadAsync(latest, ct).ConfigureAwait(true);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Self update: the download failed.");
                    return SelfUpdateResult.Failed(ex.Message);
                }

                if (!downloaded) return SelfUpdateResult.Failed("the new version could not be downloaded");

                // The tool is refreshed now, while this process is running and its files are
                // free, because the next start is going to run it and from then on nothing can
                // replace it. See the remarks on PendingUpdate.RefreshUpdaterTool.
                if (!PendingUpdate.RefreshUpdaterTool())
                {
                    return SelfUpdateResult.Failed("the update tool could not be updated");
                }

                return SelfUpdateResult.Ready(latest.VersionString);
            }
        }

        /// <summary>
        /// Applies an update left behind by an earlier session, if there is one.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The folder being complete is the whole of the condition: that is what a session that
        /// downloaded a release leaves behind, and what a session that was interrupted does not.
        /// A folder that is not a complete update is removed rather than skipped, because
        /// skipping it would make it be found again on every start.
        /// </para>
        /// <para>
        /// The one thing checked on top of that is that the update is not older than what is
        /// running. It cannot normally be - a release is only downloaded when it is newer than
        /// the version at the time - but the folder outlives the session that made it, and an
        /// application updated by other means in the meantime must not be taken backwards by a
        /// file it left on disk.
        /// </para>
        /// </remarks>
        /// <returns>
        /// True when the update was handed over, in which case the caller has to close this
        /// application for the tool to be able to work.
        /// </returns>
        internal static bool TryApplyPendingUpdate()
        {
            if (!PendingUpdate.IsReady)
            {
                // Only removed when there is something to remove: the folder being absent is the
                // normal case, and it is not worth a line in the log on every start.
                if (Directory.Exists(SelfUpdatePaths.PendingUpdateDirectory))
                {
                    _log.Warn("Self update: '" + SelfUpdatePaths.PendingUpdateDirectory
                              + "' does not hold a complete update and is removed.");
                    PendingUpdate.Discard();
                }

                return false;
            }

            if (!IsPendingNewerThanRunning())
            {
                _log.Warn("Self update: the waiting update is not newer than the running version '"
                          + ApplicationVersion.ReadRunning() + "'; it is removed.");
                PendingUpdate.Discard();
                return false;
            }

            // Both of these can fail, and both have already said why in the log. The application
            // is then left running with the update still on disk, which is the one state from
            // which the user can do something about it.
            if (!PendingUpdate.RefreshUpdaterTool()) return false;

            return PendingUpdateLauncher.TryHandOff();
        }

        /// <summary>
        /// Reports an update that failed while the application was closed, and clears the report.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The tool runs after the application has closed and has no window of its own, so a
        /// failure is left in a file for the next start to find - which is also what makes the
        /// message translatable and what gives it a window to be shown in. The file is read once
        /// and removed, so a failure is reported exactly once.
        /// </para>
        /// <para>
        /// The reason goes into the log and not into the message the user sees: it is an exception
        /// text or a file system error, and the message says where to find it. What the user is
        /// told is the part they can act on.
        /// </para>
        /// </remarks>
        /// <returns>True when an update failed during a previous session.</returns>
        internal static bool TakeFailureReport()
        {
            var markerPath = SelfUpdatePaths.FailureMarkerFilePath;

            try
            {
                if (!File.Exists(markerPath)) return false;

                var reason = File.ReadAllText(markerPath);
                File.Delete(markerPath);

                _log.Error("Self update: the previous update failed. " + reason);

                return true;
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Self update: the report of the previous failure at '" + markerPath
                              + "' could not be read.");
                return false;
            }
        }

        /// <summary>
        /// Whether the waiting update is newer than the version this process runs.
        /// </summary>
        /// <remarks>
        /// An update whose version cannot be read is applied rather than refused: the folder
        /// holding the program and a RES folder is the condition the update was asked for, and a
        /// version resource that is missing says nothing about whether the files are right.
        /// </remarks>
        private static bool IsPendingNewerThanRunning()
        {
            var pendingVersion = PendingUpdate.Version;

            if (string.IsNullOrWhiteSpace(pendingVersion)) return true;

            var comparer = new SelfUpdateVersionComparer();

            // Compare answers "is the second argument newer than the first", so this is
            // "the waiting version is not older than the running one".
            return comparer.Compare(ApplicationVersion.ReadRunning(), pendingVersion) >= 0;
        }

        /// <summary>
        /// Passes a phase on to whoever asked to be told about it.
        /// </summary>
        private static void Report(Action<string> reportStatus, string statusText)
        {
            if (reportStatus == null) return;

            reportStatus(statusText);
        }
    }
}
