using System.IO;
using NLog;

namespace AIHelper.Manage.Update.SelfUpdate
{
    /// <summary>
    /// The update that was downloaded and is waiting to be applied: the folder
    /// RES\update\appupdate and what can be done with it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This folder is the whole message between the two halves of the self update. One session
    /// downloads into it - the update pass does, while the application is open - and the next
    /// start finds it and hands over to aihelper-updater. The presence of a complete update is
    /// therefore the thing that is asked about, and not a flag somewhere else that could
    /// disagree with what is on disk.
    /// </para>
    /// <para>
    /// The folder holds a release unpacked as it was packed: the program and a RES folder, the
    /// two things an update is made of. A folder that is only half there - a download that was
    /// stopped, an unpack that failed - is not an update, and the test is the same one
    /// aihelper-updater makes before it moves anything.
    /// </para>
    /// </remarks>
    internal static class PendingUpdate
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// True when the folder holds a complete update: the program and its RES folder.
        /// </summary>
        internal static bool IsReady
        {
            get
            {
                return File.Exists(SelfUpdatePaths.PendingApplicationExecutablePath)
                       && Directory.Exists(SelfUpdatePaths.PendingResDirectory);
            }
        }

        /// <summary>
        /// Version the waiting update carries, or null when it cannot be read.
        /// </summary>
        /// <remarks>
        /// Read from the program inside the folder rather than remembered from the download.
        /// The folder is what survives the session - a file left on disk - so it is also the
        /// only thing that can say which version it is, and it says it in the same place the
        /// installed version is written.
        /// </remarks>
        internal static string Version
        {
            get { return ApplicationVersion.ReadFromFile(SelfUpdatePaths.PendingApplicationExecutablePath); }
        }

        /// <summary>
        /// Replaces the deployed copy of aihelper-updater with the one the update ships.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This has to happen while the application runs and before the tool is started, and it
        /// is the reason the tool is shipped inside the release at all. The tool is the program
        /// that moves the new version over the old one, so the version that moves the files has
        /// to be the new one - and once it is running, its own files are locked and nothing can
        /// replace them any more.
        /// </para>
        /// <para>
        /// A release that carries no tool is therefore not an update this program can apply, and
        /// the hand-off is refused rather than started with whatever copy happens to be deployed:
        /// the deployed one belongs to the version being replaced, and letting it move the new
        /// files would be running the old program over the new one. The payload is what decides,
        /// so the check belongs here and not at the call site.
        /// </para>
        /// </remarks>
        /// <returns>True when the deployed tool was refreshed from the update.</returns>
        internal static bool RefreshUpdaterTool()
        {
            var sourceDirectoryPath = SelfUpdatePaths.PendingUpdaterToolDirectory;

            if (!Directory.Exists(sourceDirectoryPath))
            {
                _log.Error("Self update: the update ships no " + SelfUpdatePaths.UpdaterToolFolderName
                           + " in '" + sourceDirectoryPath + "', so it cannot be applied.");
                return false;
            }

            if (!UpdateIo.TryCopyDirectory(sourceDirectoryPath, SelfUpdatePaths.UpdaterToolDirectory))
            {
                _log.Error("Self update: the installed " + SelfUpdatePaths.UpdaterToolFolderName
                           + " could not be refreshed from the update.");
                return false;
            }

            _log.Info("Self update: the installed " + SelfUpdatePaths.UpdaterToolFolderName
                      + " was refreshed from the update.");
            return true;
        }

        /// <summary>
        /// Removes the waiting update.
        /// </summary>
        /// <remarks>
        /// Used for a folder that turned out not to be an update and for one that is not newer
        /// than what is installed. Both would otherwise be found again on every start.
        /// </remarks>
        /// <returns>True when the folder is gone.</returns>
        internal static bool Discard()
        {
            if (!Directory.Exists(SelfUpdatePaths.PendingUpdateDirectory)) return true;

            var removed = UpdateIo.TryDeleteDirectory(SelfUpdatePaths.PendingUpdateDirectory, true);

            if (removed)
            {
                _log.Info("Self update: '" + SelfUpdatePaths.PendingUpdateDirectory + "' was removed.");
            }
            else
            {
                _log.Warn("Self update: '" + SelfUpdatePaths.PendingUpdateDirectory + "' could not be removed.");
            }

            return removed;
        }
    }
}
