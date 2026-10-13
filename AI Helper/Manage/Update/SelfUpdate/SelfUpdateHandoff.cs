using System;
using System.IO;
using AIHelper.Manage.Update.Versioning;
using NLog;

namespace AIHelper.Manage.Update.SelfUpdate
{
    /// <summary>
    /// The application's own update, as seen from a start: hand a waiting update over to
    /// aihelper-updater, and report a failure the tool left behind.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The update is deliberately split across two sessions, and this is the second one. The
    /// new version is checked for and downloaded by the update pass, while the application is
    /// open - the pass is <see cref="Updater"/>, and the application is one of the rows in its
    /// window like any mod. That pass can only get the release as far as the disk, because the
    /// files being replaced are the ones this process is running from. The next start finds
    /// the folder and hands over, and the tool does the rest while nothing of the application
    /// is in memory.
    /// </para>
    /// <para>
    /// No window is shown and no question is asked here. The check was started from the update
    /// options, which is where the user asked for it, and the hand-off happens on a start the
    /// user is not watching - the tool restarts the application when it is done, so the screen
    /// the user comes back to is the one they left.
    /// </para>
    /// </remarks>
    internal static class SelfUpdateHandoff
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

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
        /// The one thing checked on top of that is that the update is newer than what is
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

            // The tool is replaced before it is started and not after: it is the program that
            // moves the new version over the old one, so the copy that moves the files has to
            // be the new one, and once it is running its own files are locked.
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

            var comparer = new UpdateToolsVersionComparer();

            // Compare answers "is the second argument newer than the first", so this asks the
            // question the method is named after: the waiting version against the running one.
            // A version the comparer cannot tell apart from the running one counts as not newer,
            // which is the right answer for it - there would be nothing to apply.
            return comparer.Compare(ApplicationVersion.ReadRunning(), pendingVersion) > 0;
        }
    }
}
