using AIHelper.Forms.Other;
using AIHelper.Manage.Report;
using AIHelper.Manage.Update.Sources;
using AIHelper.Manage.Update.Targets;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AIHelper.Manage.Update
{
    /// <summary>
    /// Mutable state of a single update check. One instance is created per
    /// <see cref="Updater.Update"/> run and reused for every folder, so
    /// <see cref="Reset"/> must be called before each folder to clear the
    /// per-folder values.
    /// <para>
    /// <see cref="Excluded"/>, <see cref="Report"/> and <see cref="UpdateOptions"/>
    /// are per-run and are deliberately not touched by <see cref="Reset"/>.
    /// </para>
    /// </summary>
    internal class UpdateInfo
    {
        private readonly StringBuilder _lastErrorText = new StringBuilder();

        /// <summary>Id of the source that produced this record, for example "updgit".</summary>
        internal string SourceId { get; set; }

        /// <summary>Version currently installed in the target folder.</summary>
        internal string TargetCurrentVersion { get; set; }

        /// <summary>Web page of the release, used for the report link.</summary>
        internal string SourceLink { get; set; }

        /// <summary>Latest version offered by the source.</summary>
        internal string TargetLastVersion { get; set; }

        /// <summary>
        /// Folder keys already updated in this run, so another source or target
        /// does not process them again.
        /// </summary>
        internal HashSet<string> Excluded { get; } = new HashSet<string>();

        /// <summary>Folder being processed.</summary>
        internal DirectoryInfo TargetFolderPath { get; set; }

        /// <summary>
        /// Parsed update record of the folder, for example
        /// { owner, repository, fileStartsWith, fileEndsWith, versionFromFile }.
        /// </summary>
        internal string[] TargetFolderUpdateInfo { get; set; }

        /// <summary>
        /// Entries collected during the run. They carry the facts only, the
        /// report markup is built from them once the pass is over.
        /// </summary>
        internal List<ReportEntry> Report { get; } = new List<ReportEntry>();

        /// <summary>True when the source has a newer version but no downloadable file.</summary>
        internal bool NoRemoteFile { get; set; }

        /// <summary>Path to the downloaded update file.</summary>
        internal string UpdateFilePath { get; set; }

        /// <summary>String the update file name starts with.</summary>
        internal string UpdateFileStartsWith { get; set; }

        /// <summary>String the update file name ends with.</summary>
        internal string UpdateFileEndsWith { get; set; }

        /// <summary>Backup directory of the previous version of the folder.</summary>
        internal string BackupDirPath { get; set; }

        /// <summary>Source selected for the current run.</summary>
        internal UpdateSourceBase Source { get; set; }

        /// <summary>Target selected for the current folder.</summary>
        internal UpdateTargetBase Target { get; set; }

        /// <summary>True when the version must be read from the file name.</summary>
        internal bool VersionFromFile { get; set; }

        /// <summary>Download link of the update file.</summary>
        public string DownloadLink { get; internal set; }

        /// <summary>Options dialog the run was started from, may be null.</summary>
        public UpdateOptionsDialogForm UpdateOptions { get; }

        public UpdateInfo()
        {
            Reset();
        }

        /// <summary>
        /// Creates the state for a run started from the given options dialog.
        /// </summary>
        /// <param name="updateOptions">Options dialog, or null for a headless run.</param>
        public UpdateInfo(UpdateOptionsDialogForm updateOptions) : this()
        {
            UpdateOptions = updateOptions;
        }

        /// <summary>
        /// Clears every value that is specific to one folder. Must be called
        /// before a folder is processed.
        /// </summary>
        internal void Reset()
        {
            TargetCurrentVersion = string.Empty;
            SourceLink = string.Empty;
            TargetLastVersion = string.Empty;
            TargetFolderPath = null;
            TargetFolderUpdateInfo = null;
            NoRemoteFile = false;
            UpdateFilePath = string.Empty;
            UpdateFileStartsWith = string.Empty;
            UpdateFileEndsWith = string.Empty;
            BackupDirPath = string.Empty;
            DownloadLink = string.Empty;
            _lastErrorText.Clear();
        }

        /// <summary>
        /// Appends an error message collected while checking or updating the
        /// current folder.
        /// </summary>
        /// <param name="message">Message to append. Empty messages are ignored.</param>
        internal void AppendError(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            _lastErrorText.AppendLine(" >" + message);
        }

        /// <summary>
        /// Returns the collected error text and clears it, so the same text is
        /// never reported twice.
        /// </summary>
        /// <returns>Error text surrounded by spaces, or an empty string.</returns>
        internal string TakeAndClearError()
        {
            if (_lastErrorText.Length == 0) return "";

            var result = " " + _lastErrorText + " ";
            _lastErrorText.Clear();
            return result;
        }

        /// <summary>
        /// Collected error text without clearing it.
        /// </summary>
        internal string LastError
        {
            get { return _lastErrorText.ToString(); }
        }
    }
}
