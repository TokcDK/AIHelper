using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using AIHelper.Forms.Other;
using AIHelper.Manage.Update.Sources;
using AIHelper.Manage.Update.Targets;
using AIHelper.Manage.Update.Targets.Mods;
using NLog;

namespace AIHelper.Manage.Update
{
    /// <summary>
    /// Runs one update pass: for every source and every target it resolves the
    /// latest version of each folder, downloads the new file when the folder is
    /// out of date and writes an HTML report.
    /// <para>
    /// The pass runs on the calling (UI) thread and reports progress through a
    /// small always-on-top form. Every folder is handled independently, so a
    /// single failing mod is reported and the pass continues.
    /// </para>
    /// </summary>
    class Updater
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Default for the "check each folder only once per N minutes" ini value.
        /// </summary>
        const int DefaultUpdateCheckTimeoutMinutes = 10;

        /// <summary>
        /// Minimum number of parts an update record must contain:
        /// owner, repository and the file name prefix.
        /// </summary>
        const int MinUpdateInfoParts = 3;

        /// <summary>Separators that split an update record into its parts.</summary>
        static readonly string[] UpdateInfoSeparators = { Environment.NewLine, "\n", "\r", "," };

        /// <summary>
        /// True if any plugin or MO was updated during this pass.
        /// </summary>
        internal bool UpdatedAny { get; private set; }

        /// <summary>
        /// Last check dates per source and folder path. Used to reduce server
        /// requests to one per folder per timeout window.
        /// </summary>
        Dictionary<string, Dictionary<string, DateTime>> _lastCheckDates = new Dictionary<string, Dictionary<string, DateTime>>();

        public Updater(UpdateOptionsDialogForm updateOptions = null)
        {
            UpdateOptions = updateOptions;
        }

        public UpdateOptionsDialogForm UpdateOptions { get; }

        /// <summary>
        /// Checks every target and updates the folders that are out of date.
        /// </summary>
        /// <param name="targetList">
        /// Targets to process. When null or empty the default target list is used.
        /// </param>
        internal async Task Update(List<UpdateTargetBase> targetList = null)
        {
            bool isUsingTargetList = targetList != null && targetList.Count > 0;
            UpdateInfo info = !isUsingTargetList ? new UpdateInfo(UpdateOptions) : targetList[0].Info;

            var sources = new List<UpdateSourceBase> //Sources of updates
            {
                new Github(info)
            };

            var targets = !isUsingTargetList ? new List<UpdateTargetBase> //Targets for update
            {
                new Mo(info),
                new ModsList(info),
                new ModsMeta(info)
            } : targetList;

            _lastCheckDates = GetDateTimesFromFile();

            var updateCheckTimeout = GetUpdateCheckTimeoutMinutes();

            var checkNUpdateText = T._("Update plugins");

            var progressForm = new Form
            {
                StartPosition = FormStartPosition.CenterScreen,
                Width = 400,
                Height = 50,
                Text = checkNUpdateText,
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                TopMost = true
            };

            var pBar = new ProgressBar
            {
                Dock = DockStyle.Bottom,
                Height = progressForm.Height / 2,
                Width = progressForm.Width - 2,
                Value = 0
            };

            var checkDateTimeNow = DateTime.Now;

            progressForm.Controls.Add(pBar);
            progressForm.Show();

            try
            {
                foreach (var source in sources) //enumerate sources
                {
                    if (source.IsNotWorkingNow) continue;

                    info.Source = source;

                    progressForm.Text = checkNUpdateText + ":" + source.Title;

                    var sourcePathDateCheckTime = GetSourceCheckDates(source.Title);

                    info.SourceId = source.InfoId; //set source info detect ID

                    foreach (var target in targets) //enumerate targets
                    {
                        if (source.IsNotWorkingNow) continue;

                        var tFolderInfos = target.GetUpdateInfos(); // get folders list for update, usually it is active mods

                        // skip if no targets
                        if (tFolderInfos == null || tFolderInfos.Count == 0) continue;

                        info.Target = target;

                        pBar.Maximum = tFolderInfos.Count;
                        pBar.Value = 0;

                        foreach (var tFolderInfo in tFolderInfos) //enumerate all folders with info
                        {
                            if (source.IsNotWorkingNow) continue;

                            //skip already updated
                            if (info.Excluded.Contains(tFolderInfo.Key)) continue;

                            // check only one time per timeout window
                            if (IsCheckTimeNotElapsed(sourcePathDateCheckTime, tFolderInfo.Key, checkDateTimeNow, updateCheckTimeout)) continue;

                            await UpdateFolderAsync(
                                source,
                                target,
                                info,
                                tFolderInfo,
                                sourcePathDateCheckTime,
                                checkDateTimeNow,
                                progressForm,
                                pBar,
                                checkNUpdateText).ConfigureAwait(true);
                        }
                    }
                }
            }
            finally
            {
                // release everything even when the pass was aborted by an exception
                DisposeSources(sources);

                pBar.Dispose();
                progressForm.Dispose();

                SaveCheckDateTimes();
            }

            //Show report
            ShowReport(info);
        }

        /// <summary>
        /// Checks and, when needed, updates one folder.
        /// </summary>
        private async Task UpdateFolderAsync(
            UpdateSourceBase source,
            UpdateTargetBase target,
            UpdateInfo info,
            KeyValuePair<string, string> tFolderInfo,
            Dictionary<string, DateTime> sourcePathDateCheckTime,
            DateTime checkDateTimeNow,
            Form progressForm,
            ProgressBar pBar,
            string checkNUpdateText)
        {
            info.Reset(); // reset some infos

            // get folder dir path
            info.TargetFolderPath = BuildTargetFolderPath(target, tFolderInfo.Key);

            progressForm.Text = checkNUpdateText + ":" + source.Title + ">" + info.TargetFolderPath.Name;

            if (pBar.Value < pBar.Maximum) pBar.Value += 1;

            // Set current version
            target.SetCurrentVersion();

            // let the target constrain the file the source will look for
            target.PrepareUpdateRequest();

            // get info to array
            var tInfoArray = GetUpdateInfoArray(tFolderInfo, source.InfoId);

            // skip if info is invalid
            if (tInfoArray.Length < MinUpdateInfoParts)
            {
                _log.Warn("Update: skipped '" + tFolderInfo.Key + "'. The update record has "
                          + tInfoArray.Length + " part(s), " + MinUpdateInfoParts + " are required.");
                return;
            }

            info.TargetFolderUpdateInfo = tInfoArray.TrimEachValue(); // get folder info

            info.TargetLastVersion = source.GetLastVersion(); // get last version
            if (source.CheckIfStopWork(info))
            {
                // stop requesting the server for a while
                source.IsNotWorkingNow = true;
                return;
            }

            // remember the previous check date so it can be restored when the update fails
            var hadPreviousCheckDate = sourcePathDateCheckTime.TryGetValue(tFolderInfo.Key, out DateTime previousCheckDateTime);

            // add check date
            sourcePathDateCheckTime[tFolderInfo.Key] = checkDateTimeNow;

            if (string.IsNullOrWhiteSpace(info.TargetLastVersion)) return;

            // clean version for more correct comparison
            string cleanedTargetLastVersion = info.TargetLastVersion;
            string cleanedTargetCurrentVersion = info.TargetCurrentVersion;
            UpdateTools.CleanVersion(ref cleanedTargetLastVersion);
            UpdateTools.CleanVersion(ref cleanedTargetCurrentVersion);
            info.TargetLastVersion = cleanedTargetLastVersion;  
            info.TargetCurrentVersion = cleanedTargetCurrentVersion;
            //if it is not newer then there is nothing to do
            if (!cleanedTargetLastVersion.IsNewerOf(cleanedTargetCurrentVersion, false)) return;

            await source.PauseAsync().ConfigureAwait(true);

            bool getFileSucceeded;
            try
            {
                getFileSucceeded = await source.GetFile().ConfigureAwait(true); // download latest file
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Update: failed to get the update file of '" + info.TargetFolderPath.Name + "'.");
                info.AppendError(ex.Message);
                getFileSucceeded = false;
            }

            bool updateFilesSuccess = false;

            if (getFileSucceeded && target.MakeBackup() && (updateFilesSuccess = target.UpdateFiles())) // update folder with new files
            {
                UpdatedAny = true;

                WriteVersionStamp(info);

                info.Excluded.Add(tFolderInfo.Key); // add path to excluded to skip it next time if will be found for other source or target

                info.Report.Add(BuildSuccessReportLine(info));
            }
            else
            {
                if (!updateFilesSuccess)
                {
                    // restore the previous check date when the update failed
                    RestoreCheckDateTime(sourcePathDateCheckTime, tFolderInfo.Key, hadPreviousCheckDate, previousCheckDateTime);
                }

                if (info.NoRemoteFile)
                {
                    info.Report.Add(BuildNoRemoteFileReportLine(info));
                }
                else
                {
                    target.RestoreBackup();

                    info.Report.Add(BuildFailedReportLine(info));

                    _log.Warn("Failed to update" + " " + info.TargetFolderPath.Name);
                }
            }
        }

        #region Update helper methods

        /// <summary>
        /// Builds the folder to process from the key of an update record. The
        /// key is a plain folder name for mod targets and an absolute path for
        /// the Mod Organizer target.
        /// </summary>
        private static DirectoryInfo BuildTargetFolderPath(UpdateTargetBase target, string folderKey)
        {
            return Path.IsPathRooted(folderKey)
                ? new DirectoryInfo(folderKey)
                : new DirectoryInfo(Path.Combine(target.GetParentFolderPath(), folderKey));
        }

        /// <summary>
        /// Splits the value of an update record into its parts, removing the
        /// 'sourceId::' prefix and the trailing '::' when both are present.
        /// <para>Example: 'updgit::BepInEx,BepInEx,BepInEx_x64::' becomes
        /// { "BepInEx", "BepInEx", "BepInEx_x64" }.</para>
        /// </summary>
        private static string[] GetUpdateInfoArray(KeyValuePair<string, string> tFolderInfo, string sourceInfoId)
        {
            var value = tFolderInfo.Value ?? string.Empty;

            var hasSourcePrefix = !string.IsNullOrEmpty(sourceInfoId)
                                  && value.Length >= sourceInfoId.Length + 4
                                  && value.StartsWith(sourceInfoId, StringComparison.InvariantCultureIgnoreCase)
                                  && value.EndsWith("::", StringComparison.Ordinal);

            if (hasSourcePrefix)
            {
                value = value.Substring(sourceInfoId.Length + 2, value.Length - sourceInfoId.Length - 4);
            }

            return value.Split(UpdateInfoSeparators, StringSplitOptions.None);
        }

        /// <summary>
        /// Gets the per-folder check dates of a source, creating the entry when
        /// the source is seen for the first time.
        /// </summary>
        private Dictionary<string, DateTime> GetSourceCheckDates(string sourceTitle)
        {
            if (!_lastCheckDates.ContainsKey(sourceTitle))
            {
                _lastCheckDates.Add(sourceTitle, new Dictionary<string, DateTime>());
            }

            return _lastCheckDates[sourceTitle];
        }

        /// <summary>
        /// True when the folder was checked less than
        /// <paramref name="updateCheckTimeoutMinutes"/> ago and must be skipped.
        /// </summary>
        private static bool IsCheckTimeNotElapsed(
            Dictionary<string, DateTime> sourcePathDateCheckTime,
            string folderKey,
            DateTime checkDateTimeNow,
            int updateCheckTimeoutMinutes)
        {
            if (!sourcePathDateCheckTime.TryGetValue(folderKey, out DateTime lastCheckDateTime)) return false;

            var nextCheckTime = lastCheckDateTime + TimeSpan.FromMinutes(updateCheckTimeoutMinutes);

            return checkDateTimeNow < nextCheckTime;
        }

        /// <summary>
        /// Puts the previous check date back (or removes the entry) so a failed
        /// folder is retried on the next pass instead of being skipped.
        /// </summary>
        private static void RestoreCheckDateTime(
            Dictionary<string, DateTime> sourcePathDateCheckTime,
            string folderKey,
            bool hadPreviousCheckDate,
            DateTime previousCheckDateTime)
        {
            if (hadPreviousCheckDate)
            {
                sourcePathDateCheckTime[folderKey] = previousCheckDateTime;
            }
            else
            {
                sourcePathDateCheckTime.Remove(folderKey);
            }
        }

        /// <summary>
        /// Writes the version of the downloaded file next to it so the same
        /// archive is not downloaded again.
        /// </summary>
        private void WriteVersionStamp(UpdateInfo info)
        {
            if (!File.Exists(info.UpdateFilePath)) return;

            try
            {
                File.WriteAllText(info.UpdateFilePath + ".version", info.TargetLastVersion);
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Update: failed to write the version stamp of '" + info.UpdateFilePath + "'.");
            }
        }

        /// <summary>
        /// Disposes every source, ignoring failures.
        /// </summary>
        private static void DisposeSources(List<UpdateSourceBase> sources)
        {
            foreach (var source in sources)
            {
                try
                {
                    source.Dispose();
                }
                catch (Exception ex)
                {
                    _log.Debug("Update: failed to dispose an update source. error:" + Environment.NewLine + ex);
                }
            }
        }

        /// <summary>
        /// Reads the "check each folder once per N minutes" value from the app
        /// ini, creating it with the default when it is missing.
        /// </summary>
        private static int GetUpdateCheckTimeoutMinutes()
        {
            var appIni = ManageIni.GetINIFile(ManageSettings.AiHelperIniPath);

            if (appIni.KeyExists(ManageSettings.UpdatesCheckTimeoutMinutesKeyName, ManageSettings.AppIniUpdateSectionName))
            {
                if (int.TryParse(
                        appIni.GetKey(ManageSettings.AppIniUpdateSectionName, ManageSettings.UpdatesCheckTimeoutMinutesKeyName),
                        out int result))
                {
                    return result;
                }
            }
            else
            {
                // add value and save
                appIni.SetKey(
                    ManageSettings.AppIniUpdateSectionName,
                    ManageSettings.UpdatesCheckTimeoutMinutesKeyName,
                    DefaultUpdateCheckTimeoutMinutes.ToString(CultureInfo.InvariantCulture));
                appIni.WriteFile();
            }

            return DefaultUpdateCheckTimeoutMinutes;
        }

        #endregion

        #region Report line builders

        /// <summary>
        /// The optional "visit the release page" suffix of a report line.
        /// </summary>
        private static string BuildInfoLinkSuffix(UpdateInfo info)
        {
            return !string.IsNullOrWhiteSpace(info.SourceLink)
                ? ManageSettings.UpdateReport.InfoLinkPattern + info.SourceLink
                : string.Empty;
        }

        /// <summary>Report line for a folder that was updated.</summary>
        private static string BuildSuccessReportLine(UpdateInfo info)
        {
            // the current version is only worth showing when it is not already part of the name
            var currentVersionIsInName = info.TargetFolderPath.Name.Contains(info.TargetCurrentVersion);

            return ManageSettings.UpdateReport.HtmlBeforeModReportSuccessLine
                + ManageSettings.UpdateReport.HtmlModReportInLineBeforeMainMessage
                + ManageSettings.UpdateReport.HtmlModReportPreModnameTags
                + info.TargetFolderPath.Name
                + ManageSettings.UpdateReport.HtmlModReportPostModnameTags
                + (currentVersionIsInName
                    ? ""
                    : " "
                        + ManageSettings.UpdateReport.HtmlModReportPreVersionTags
                        + info.TargetCurrentVersion
                        + ManageSettings.UpdateReport.HtmlModReportPostVersionTags)
                + " "
                + T._("updated to version")
                + " "
                + ManageSettings.UpdateReport.HtmlModReportPreVersionTags
                + info.TargetLastVersion
                + ManageSettings.UpdateReport.HtmlModReportPostVersionTags
                + ManageSettings.UpdateReport.HtmlModReportInLineAfterMainMessage
                + ManageSettings.UpdateReport.HtmlAfterModReportLine
                + BuildInfoLinkSuffix(info);
        }

        /// <summary>Report line for a folder that has a new version but no file.</summary>
        private static string BuildNoRemoteFileReportLine(UpdateInfo info)
        {
            return ManageSettings.UpdateReport.HtmlBeforeModReportWarningLine
                + ManageSettings.UpdateReport.HtmlModReportInLineBeforeMainMessage
                + ManageSettings.UpdateReport.HtmlModReportPreModnameTags
                + info.TargetFolderPath.Name
                + ManageSettings.UpdateReport.HtmlModReportPostModnameTags
                + " "
                + T._("have new version but file for update not found")
                + ManageSettings.UpdateReport.HtmlModReportInLineAfterMainMessage
                + ManageSettings.UpdateReport.HtmlAfterModReportLine
                + BuildInfoLinkSuffix(info);
        }

        /// <summary>Report line for a folder that could not be updated.</summary>
        private static string BuildFailedReportLine(UpdateInfo info)
        {
            return ManageSettings.UpdateReport.HtmlBeforeModReportErrorLine
                + ManageSettings.UpdateReport.HtmlModReportInLineBeforeMainMessage
                + T._("Failed to update") + " " + info.TargetFolderPath.Name
                + ManageSettings.UpdateReport.HtmlModReportInLineAfterMainMessage
                + info.TakeAndClearError()
                + " (" + T._("Details in") + " " + ManageSettings.ApplicationProductName + ".log" + ")"
                + ManageSettings.UpdateReport.HtmlAfterModReportLine;
        }

        #endregion

        /// <summary>
        /// Writes the update check datetimes to the cache file.
        /// </summary>
        private void SaveCheckDateTimes()
        {
            var dateTimesFilePath = ManageSettings.UpdateCheckDateTimesFilePath;

            try
            {
                // create the parent directory first, otherwise the writer throws on a fresh install
                var dateTimesDirPath = Path.GetDirectoryName(dateTimesFilePath);
                if (!string.IsNullOrWhiteSpace(dateTimesDirPath))
                {
                    Directory.CreateDirectory(dateTimesDirPath);
                }

                using (var sw = new StreamWriter(dateTimesFilePath))
                {
                    foreach (var source in _lastCheckDates)
                    {
                        foreach (var data in source.Value)
                        {
                            sw.WriteLine(source.Key + "|" + data.Key + "|" + data.Value.ToString("O", CultureInfo.InvariantCulture));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Update: failed to save the check datetimes to '" + dateTimesFilePath + "'.");
            }
        }

        /// <summary>
        /// Reads the update check datetimes from the cache file. Unreadable
        /// lines are skipped instead of aborting the pass.
        /// </summary>
        private static Dictionary<string, Dictionary<string, DateTime>> GetDateTimesFromFile()
        {
            var result = new Dictionary<string, Dictionary<string, DateTime>>();
            var dateTimesFilePath = ManageSettings.UpdateCheckDateTimesFilePath;

            if (!File.Exists(dateTimesFilePath)) return result;

            try
            {
                using (var sr = new StreamReader(dateTimesFilePath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var data = line.Split('|');

                        if (data.Length != 3) continue;

                        if (!DateTime.TryParseExact(
                                data[2],
                                "O",
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None,
                                out DateTime checkDateTime))
                        {
                            _log.Warn("Update: skipped an unreadable check datetime '" + data[2] + "' in '" + dateTimesFilePath + "'.");
                            continue;
                        }

                        if (!result.ContainsKey(data[0]))
                        {
                            result.Add(data[0], new Dictionary<string, DateTime>());
                        }

                        if (!result[data[0]].ContainsKey(data[1]))
                        {
                            result[data[0]].Add(data[1], checkDateTime);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Update: failed to read the check datetimes from '" + dateTimesFilePath + "'.");
            }

            return result;
        }

        /// <summary>
        /// Builds the report HTML from the collected lines and opens it in the
        /// default browser.
        /// </summary>
        private void ShowReport(UpdateInfo info)
        {
            var newReport = new List<string>();

            foreach (var line in info.Report)
            {
                var infoLinkIndex = line.IndexOf(ManageSettings.UpdateReport.InfoLinkPattern, StringComparison.Ordinal);

                if (infoLinkIndex < 0)
                {
                    newReport.Add(line);
                    continue;
                }

                var reportLine = line.Substring(0, infoLinkIndex);
                var infoLink = line.Substring(infoLinkIndex + ManageSettings.UpdateReport.InfoLinkPattern.Length);

                newReport.Add(reportLine.Replace(
                    ManageSettings.UpdateReport.HtmlAfterModReportLine,
                    ManageSettings.UpdateReport.HtmlModReportPreVersionTags
                        + ManageSettings.UpdateReport.PreInfoLinkTitleText
                        + ManageSettings.UpdateReport.HtmlModReportPostVersionTags
                        + ManageSettings.UpdateReport.HtmlPreInfoLinkHtml
                        + infoLink
                        + ManageSettings.UpdateReport.HtmlAfterInfoLinkHtml
                        + ManageSettings.UpdateReport.InfoLinkText
                        + ManageSettings.UpdateReport.HtmlAfterInfoLinkText
                        + ManageSettings.UpdateReport.HtmlModReportPreVersionTags
                        + ManageSettings.UpdateReport.PostInfoLinkTitleText
                        + ManageSettings.UpdateReport.HtmlModReportPostVersionTags
                        + ManageSettings.UpdateReport.HtmlAfterModReportLine));
            }

            var noModsInfo = newReport.Count == 0;

            string reportMessage = BuildReportMessage(newReport, noModsInfo);

            var htmlfile = ManageSettings.UpdateReportHtmlFilePath;

            try
            {
                var htmlDirPath = Path.GetDirectoryName(htmlfile);
                if (!string.IsNullOrWhiteSpace(htmlDirPath))
                {
                    Directory.CreateDirectory(htmlDirPath); // fix missing parent directory error
                }

                File.WriteAllText(htmlfile, reportMessage);
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Update: failed to write the update report to '" + htmlfile + "'.");
                return;
            }

            using (var process = new System.Diagnostics.Process())
            {
                try
                {
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = htmlfile;
                    process.Start();
                }
                catch (Exception ex)
                {
                    _log.Warn(ex, "Update: failed to open the update report '" + htmlfile + "'.");
                }
            }
        }

        /// <summary>
        /// Fills the report template when it is available, and falls back to a
        /// self-contained HTML document when it is not.
        /// </summary>
        private static string BuildReportMessage(List<string> newReport, bool noModsInfo)
        {
            var modsReport = noModsInfo
                ? ManageSettings.UpdateReport.NoModsUpdatesFoundText
                : string.Join(ManageSettings.UpdateReport.HtmlBetweenModsText, newReport);

            var reportTemplatePath = ManageSettings.UpdateReport.ReportFilePath;

            if (File.Exists(reportTemplatePath))
            {
                try
                {
                    return File.ReadAllText(reportTemplatePath)
                        .Replace(ManageSettings.UpdateReport.BgImageLinkPathPattern, ManageSettings.UpdateReport.CurrentGameBgFilePath)
                        .Replace(ManageSettings.UpdateReport.ModsUpdateReportHeaderTextPattern, ManageSettings.UpdateReport.TitleText)
                        .Replace(ManageSettings.UpdateReport.SingleModUpdateReportsTextSectionPattern, modsReport + "<br>")
                        .Replace(ManageSettings.UpdateReport.ModsUpdateInfoNoticePattern, noModsInfo ? "" : ManageSettings.UpdateReport.ModsUpdateInfoNoticeText);
                }
                catch (Exception ex)
                {
                    _log.Warn(ex, "Update: failed to read the report template '" + reportTemplatePath + "'.");
                }
            }

            return ManageSettings.UpdateReport.HtmlBeginText
                + ManageSettings.UpdateReport.TitleText
                + ManageSettings.UpdateReport.HtmlAfterHeaderText
                + modsReport
                + "<br>"
                + ManageSettings.UpdateReport.HtmLendText;
        }
    }
}
