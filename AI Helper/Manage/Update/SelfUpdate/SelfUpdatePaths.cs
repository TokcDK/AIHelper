using System.IO;
using System.Windows.Forms;

namespace AIHelper.Manage.Update.SelfUpdate
{
    /// <summary>
    /// The folders the application's own update works with, and the file names in them.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The layout mirrors the one aihelper-updater works with, and the two have to agree - the
    /// application writes into the folders the tool reads from, and the tool is started with a
    /// path rather than with this class. The names are therefore constants on both sides, and
    /// the shape of the exchange is one folder:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// RES\update\appupdate - a downloaded and unpacked release. Its presence is the whole
    /// message "an update is ready": the application looks for the program and a RES folder in
    /// it and hands over to the tool, which moves it over the application.
    /// </item>
    /// <item>
    /// RES\tools\aihelper-updater - the tool itself. The application refreshes it from the
    /// payload *before* starting it, because once it runs its own files are locked.
    /// </item>
    /// </list>
    /// <para>
    /// The payload is unpacked into RES\update rather than into a folder of its own because
    /// that is where this application already keeps everything an update works with: the per
    /// game data, the update check dates, the downloads. It is also the folder aihelper-updater
    /// reserves for its own backup and its log, so the payload, the backup and the log all
    /// survive the update they are part of - the update replaces RES, and anything of it kept
    /// inside the replaced part would be lost by it.
    /// </para>
    /// </remarks>
    internal static class SelfUpdatePaths
    {
        /// <summary>Folder of the tool inside RES\tools.</summary>
        internal const string UpdaterToolFolderName = "aihelper-updater";

        /// <summary>File name of the tool.</summary>
        internal const string UpdaterExecutableName = "aihelper-updater.exe";

        /// <summary>Folder a downloaded release is unpacked into, inside RES\update.</summary>
        internal const string PendingUpdateFolderName = "appupdate";

        /// <summary>Folder name of the application's RES, which is what a release carries.</summary>
        internal const string ResFolderName = "RES";

        /// <summary>
        /// File aihelper-updater leaves behind when it could not apply an update.
        /// </summary>
        /// <remarks>
        /// The tool runs after this application has closed and has no window to say anything in,
        /// so a failure is reported by the application on its next start instead - which is also
        /// what makes the message translatable.
        /// </remarks>
        internal const string FailureMarkerFileName = "update-failed.txt";

        /// <summary>Folder of the application's own tools.</summary>
        internal static string ToolsDirectory
        {
            get { return ManageSettings.AppResToolsDirPath; }
        }

        /// <summary>Folder a downloaded release is unpacked into.</summary>
        internal static string PendingUpdateDirectory
        {
            get { return Path.Combine(ManageSettings.ModsUpdateDirPath, PendingUpdateFolderName); }
        }

        /// <summary>File name of the running application, which is what a release replaces.</summary>
        internal static string ApplicationExecutableFileName
        {
            get { return Path.GetFileName(Application.ExecutablePath); }
        }

        /// <summary>
        /// The program inside the pending update, which is what makes the folder an update.
        /// </summary>
        internal static string PendingApplicationExecutablePath
        {
            get { return Path.Combine(PendingUpdateDirectory, ApplicationExecutableFileName); }
        }

        /// <summary>RES folder inside the pending update.</summary>
        internal static string PendingResDirectory
        {
            get { return Path.Combine(PendingUpdateDirectory, ResFolderName); }
        }

        /// <summary>Folder of the tool inside the payload, which is refreshed before it runs.</summary>
        internal static string PendingUpdaterToolDirectory
        {
            get { return Path.Combine(PendingResDirectory, "tools", UpdaterToolFolderName); }
        }

        /// <summary>Folder the tool is deployed in.</summary>
        internal static string UpdaterToolDirectory
        {
            get { return Path.Combine(ToolsDirectory, UpdaterToolFolderName); }
        }

        /// <summary>Where the tool is deployed.</summary>
        internal static string UpdaterExecutablePath
        {
            get { return Path.Combine(UpdaterToolDirectory, UpdaterExecutableName); }
        }

        /// <summary>Where a failed update is reported from on the next start.</summary>
        internal static string FailureMarkerFilePath
        {
            get { return Path.Combine(ManageSettings.ModsUpdateDirPath, FailureMarkerFileName); }
        }
    }
}
