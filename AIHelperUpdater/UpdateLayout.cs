using System;
using System.IO;

namespace AIHelperUpdater
{
    /// <summary>
    /// Where everything an update works with lives, worked out from two folders.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The layout is derived once, here, and not spelled out again at each use. Two of the paths
    /// are load-bearing and easy to get wrong:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// the payload is *inside* the application folder (RES\updates\appupdate), so moving the
    /// application's own RES folder has to leave the payload, the backup and this program's own
    /// folder where they are - see <see cref="IsReserved"/>. A backup that swallowed its own
    /// source or destination would take the update down with it.
    /// </item>
    /// <item>
    /// the log and the backup both live under RES\update, which is the folder the application
    /// already keeps per-game update data in. That is deliberate: the update replaces RES, and
    /// anything of the update kept inside the replaced part would be lost by the update.
    /// </item>
    /// </list>
    /// </remarks>
    internal sealed class UpdateLayout
    {
        internal const string ResFolderName = "RES";
        internal const string UpdatesFolderName = "updates";
        internal const string UpdateFolderName = "update";
        internal const string ToolsFolderName = "tools";
        internal const string PayloadFolderName = "appupdate";
        internal const string UpdaterToolFolderName = "aihelper-updater";
        internal const string RulesFileName = "update-rules.txt";
        internal const string LogFileName = "aihelper-updater.log";
        internal const string FailureMarkerFileName = "update-failed.txt";

        /// <summary>
        /// Suffix of the folder the payload is renamed to when the update could not be applied.
        /// </summary>
        /// <remarks>
        /// A payload that stays in place after a failure would be applied again on the next
        /// start, and a start that keeps handing over to an update which keeps failing is a
        /// loop with no way out. Renaming the folder is what takes it out of the way while
        /// leaving it there to be looked at.
        /// </remarks>
        internal const string FailedPayloadSuffix = ".failed";

        private readonly string[] _reservedPaths;

        /// <param name="applicationDirectory">Folder the application runs from.</param>
        /// <param name="toolDirectory">Folder this program runs from.</param>
        internal UpdateLayout(string applicationDirectory, string toolDirectory)
        {
            ApplicationDirectory = Normalize(applicationDirectory);
            ToolDirectory = Normalize(toolDirectory);

            ResDirectory = Path.Combine(ApplicationDirectory, ResFolderName);
            PayloadDirectory = Path.Combine(ResDirectory, UpdatesFolderName, PayloadFolderName);
            FailedPayloadDirectory = PayloadDirectory + FailedPayloadSuffix;
            BackupDirectory = Path.Combine(ResDirectory, UpdateFolderName, "app", "old");
            RulesFilePath = Path.Combine(ToolDirectory, RulesFileName);
            LogFilePath = Path.Combine(ResDirectory, UpdateFolderName, LogFileName);
            FailureMarkerFilePath = Path.Combine(ResDirectory, UpdateFolderName, FailureMarkerFileName);

            // The three folders the update may never move, in either direction: the payload it
            // reads from, the workspace it writes its backup and its log into, and the folder
            // this program is running from - whose files are locked by this very process.
            _reservedPaths = new[]
            {
                Path.Combine(ResDirectory, UpdatesFolderName),
                Path.Combine(ResDirectory, UpdateFolderName),
                Path.Combine(ResDirectory, ToolsFolderName, UpdaterToolFolderName)
            };
        }

        /// <summary>Folder the application runs from.</summary>
        internal string ApplicationDirectory { get; }

        /// <summary>Folder this program runs from, which is where its rules file is.</summary>
        internal string ToolDirectory { get; }

        /// <summary>RES folder of the application.</summary>
        internal string ResDirectory { get; }

        /// <summary>Folder the downloaded and unpacked new version was left in.</summary>
        internal string PayloadDirectory { get; }

        /// <summary>Where the payload is moved when the update could not be applied.</summary>
        internal string FailedPayloadDirectory { get; }

        /// <summary>Folder the files named by the rules are moved into.</summary>
        internal string BackupDirectory { get; }

        /// <summary>Rules file, read from the folder this program runs from.</summary>
        internal string RulesFilePath { get; }

        /// <summary>Log of the run.</summary>
        internal string LogFilePath { get; }

        /// <summary>
        /// File left behind when the update failed, and read by the application on its next start.
        /// </summary>
        /// <remarks>
        /// The tool cannot tell the user anything itself - it is a windowless program started
        /// after the application closed - so the one thing the user has to be told is handed over
        /// in a file instead of being shown here. It also carries the reason, which is what the
        /// application puts in its own log.
        /// </remarks>
        internal string FailureMarkerFilePath { get; }

        /// <summary>
        /// True for the folders an update must leave alone.
        /// </summary>
        /// <remarks>
        /// Asked about a path on either side of a move, because what matters differs: while the
        /// backup is being taken it is the application's own folders that must not be moved, and
        /// while the payload is being applied it is the destinations that must not be written to.
        /// Both are these three.
        /// </remarks>
        /// <param name="absolutePath">Path to test.</param>
        internal bool IsReserved(string absolutePath)
        {
            foreach (var reservedPath in _reservedPaths)
            {
                if (IsUnderPath(absolutePath, reservedPath)) return true;
            }

            return false;
        }

        /// <summary>True when <paramref name="absolutePath"/> is the application folder or below it.</summary>
        internal bool IsInsideApplication(string absolutePath)
        {
            return IsUnderPath(absolutePath, ApplicationDirectory);
        }

        /// <summary>
        /// Case-insensitive, separator-aware "is inside" test.
        /// </summary>
        /// <remarks>
        /// A plain prefix comparison is not enough on Windows: "C:\App\RESX" starts with
        /// "C:\App\RES" and is not inside it. The separator has to be part of the comparison, and
        /// the comparison has to ignore case, which is how this file system matches paths.
        /// </remarks>
        /// <param name="path">Path to test.</param>
        /// <param name="rootPath">Candidate parent folder.</param>
        internal static bool IsUnderPath(string path, string rootPath)
        {
            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(rootPath)) return false;

            var normalizedPath = Normalize(path);
            var normalizedRoot = Normalize(rootPath);

            if (string.Equals(normalizedPath, normalizedRoot, StringComparison.OrdinalIgnoreCase)) return true;

            return normalizedPath.StartsWith(
                normalizedRoot + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Full path with the separators made uniform and any trailing one removed, so that two
        /// spellings of the same folder compare equal.
        /// </summary>
        private static string Normalize(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;

            return path
                .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
                .TrimEnd(Path.DirectorySeparatorChar);
        }
    }
}
