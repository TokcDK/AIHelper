using AIHelper.Manage.Update.Sources;
using AIHelper.Manage.Update.Targets;
using AIHelper.Manage.Update.Targets.Mods;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIHelper.Manage.Update
{
    class Updater
    {
        private bool _isHtmlReport = true;

        /// <summary>
        /// true if any plugin or MO was updated
        /// </summary>
        internal bool UpdatedAny;

        /// <summary>
        /// Last check dates to reduce server requests one time per hour
        /// </summary>
        Dictionary<string, Dictionary<string, DateTime>> lastCheckDates;

        internal async Task Update()
        {
            UpdateInfo info = new UpdateInfo();
            var sources = new List<UpdateSourceBase> //Sources of updates
            {
                new Github(info)
            };
            var targets = new List<UpdateTargetBase> //Targets for update
            {
                new Mo(info),
                //new MOBaseGames(info),
                new ModsList(info),
                new ModsMeta(info)
            };

            lastCheckDates = GetDateTimesFromFile();

            // get set timeout from app ini
            var appIni = ManageIni.GetINIFile(ManageSettings.AiHelperIniPath);
            int updateCheckTimeout = 10;
            if (appIni.KeyExists(ManageSettings.UpdatesCheckTimeoutMinutesKeyName, ManageSettings.AppIniUpdateSectionName))
            {
                if (int.TryParse(appIni.GetKey(ManageSettings.AppIniUpdateSectionName, ManageSettings.UpdatesCheckTimeoutMinutesKeyName), out int result))
                {
                    updateCheckTimeout = result;
                }
            }
            else
            {
                // add value and save
                appIni.SetKey(ManageSettings.AppIniUpdateSectionName, ManageSettings.UpdatesCheckTimeoutMinutesKeyName, updateCheckTimeout.ToString());
                appIni.WriteFile();
            }

            var checkNUpdateText = T._("Update plugins");

            //using (var progressForm = new Form())
            //using (var pBar = new ProgressBar())
            //{
            //    pBar.Dock = DockStyle.Bottom;
            //    progressForm.Controls.Add(pBar);
            //    progressForm.StartPosition = FormStartPosition.CenterScreen;
            //    progressForm.Size = new Size(400, 50);
            //    progressForm.Text = checkNUpdateText;
            //    progressForm.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            //    progressForm.TopMost = true;
            //    progressForm.Show();

            //}

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

            foreach (var source in sources) //enumerate sources
            {
                if (source.IsNotWorkingNow) continue;

                info.Source = source;

                progressForm.Text = checkNUpdateText + ":" + source.Title;

                var sourcePathDateCheckTime = new Dictionary<string, DateTime>();
                if (!lastCheckDates.ContainsKey(source.Title)) lastCheckDates.Add(source.Title, sourcePathDateCheckTime);

                sourcePathDateCheckTime = lastCheckDates[source.Title];

                info.SourceId = source.InfoId; //set source info detect ID
                foreach (var target in targets) //enumerate targets
                {
                    if (source.IsNotWorkingNow) continue;

                    var tFolderInfos = target.GetUpdateInfos(); // get folderslist for update, usually it is active mods
                    
                    // skip if no targets
                    if (tFolderInfos == null || tFolderInfos.Count == 0) continue;

                    info.Target = target;

                    pBar.Maximum = tFolderInfos.Keys.Count;

                    pBar.Value = 0;

                    foreach (var tFolderInfo in tFolderInfos) //enumerate all folders with info
                    {
                        if (source.IsNotWorkingNow) continue;

                        //skip already updated
                        if (info.Excluded.Contains(tFolderInfo.Key)) continue;

                        // check only one time per hour
                        if (sourcePathDateCheckTime.ContainsKey(tFolderInfo.Key))
                        {
                            var lastCheckTimeElapsed = sourcePathDateCheckTime[tFolderInfo.Key] + TimeSpan.FromMinutes(updateCheckTimeout);
                            if (checkDateTimeNow < lastCheckTimeElapsed) continue;
                        }

                        info.Reset(); // reset some infos

                        // get folder dir path
                        info.TargetFolderPath = new DirectoryInfo(Path.Combine(target.GetParentFolderPath(), tFolderInfo.Key));

                        progressForm.Text = checkNUpdateText + ":" + source.Title + ">" + info.TargetFolderPath.Name;

                        if (pBar.Value < pBar.Maximum) pBar.Value += 1;

                        // Set current version
                        target.SetCurrentVersion();

                        // get info to array
                        var tInfoArray = (tFolderInfo.Value.StartsWith(source.InfoId, StringComparison.InvariantCultureIgnoreCase) ? tFolderInfo.Value.Remove(tFolderInfo.Value.Length - 2, 2).Remove(0, source.InfoId.Length + 2) : tFolderInfo.Value).Split(new[] { Environment.NewLine, "\n", "\r", "," }, StringSplitOptions.None);

                        // skip if info is invalid
                        if (tInfoArray.Length == 0) continue;

                        info.TargetFolderUpdateInfo = tInfoArray.Trim(); // get folder info
                                                                         //info.TargetCurrentVersion = tInfoArray[tInfoArray.Length - 1]; // get current version (last element of info)

                        info.TargetLastVersion = source.GetLastVersion(); // get last version
                        if (source.CheckIfStopWork(info))
                        {
                            // stop request server for time
                            source.IsNotWorkingNow = true;
                            break;
                        }

                        // add check date
                        var oldCheckDateTime = DateTime.MinValue;
                        if (sourcePathDateCheckTime.ContainsKey(tFolderInfo.Key))
                        {
                            oldCheckDateTime = checkDateTimeNow;
                            sourcePathDateCheckTime[tFolderInfo.Key] = checkDateTimeNow;
                        }
                        else
                        {
                            sourcePathDateCheckTime.Add(tFolderInfo.Key, checkDateTimeNow);
                        }

                        if (info.TargetLastVersion.Length == 0) continue;

                        // clean version for more correct comprasion
                        UpdateTools.CleanVersion(ref info.TargetLastVersion);
                        UpdateTools.CleanVersion(ref info.TargetCurrentVersion);

                        //if it is last version then run update
                        if (!info.TargetLastVersion.IsNewerOf(info.TargetCurrentVersion, false)) continue;

                        source.Pause();

                        bool getfileIsTrue = await source.GetFile().ConfigureAwait(true); // download latest file

                        bool updateFilesFailed = false;
                        if (getfileIsTrue && target.MakeBuckup() && (updateFilesFailed = target.UpdateFiles()) // update folder with new files
                            )
                        {
                            UpdatedAny = true;

                            if (File.Exists(info.UpdateFilePath)) File.WriteAllText(info.UpdateFilePath + ".version", info.TargetLastVersion);

                            info.Excluded.Add(tFolderInfo.Key); // add path to excluded to skip it next time if will be found for other source or target

                            info.Report.Add(
                                (_isHtmlReport ? ManageSettings.UpdateReport.HtmlBeforeModReportSuccessLine: string.Empty)
                                    + ManageSettings.UpdateReport.HtmlModReportInLineBeforeMainMessage
                                        + ManageSettings.UpdateReport.HtmlModReportPreModnameTags
                                        + info.TargetFolderPath.Name
                                        + ManageSettings.UpdateReport.HtmlModReportPostModnameTags
                                        + (!info.TargetFolderPath.Name.Contains(info.TargetCurrentVersion) ? 
                                        (
                                        " "
                                        + ManageSettings.UpdateReport.HtmlModReportPreVersionTags
                                        + info.TargetCurrentVersion
                                        + ManageSettings.UpdateReport.HtmlModReportPostVersionTags
                                        )
                                        : "")
                                        + " "
                                        + T._("updated to version")
                                        + " "
                                        + ManageSettings.UpdateReport.HtmlModReportPreVersionTags                                            + info.TargetLastVersion
                                        + ManageSettings.UpdateReport.HtmlModReportPostVersionTags                                    + ManageSettings.UpdateReport.HtmlModReportInLineAfterMainMessage                                    + (_isHtmlReport ? ManageSettings.UpdateReport.HtmlAfterModReportLine: string.Empty)
                                        + (!string.IsNullOrWhiteSpace(info.SourceLink) ? ManageSettings.UpdateReport.InfoLinkPattern+ info.SourceLink : string.Empty)
                                );

                        }
                        else
                        {
                            if (updateFilesFailed)
                            {
                                // restore check datetime if failed to update
                                if (oldCheckDateTime == DateTime.MinValue)
                                {
                                    sourcePathDateCheckTime.Remove(tFolderInfo.Key); // remove last check date
                                }
                                else
                                {
                                    sourcePathDateCheckTime[tFolderInfo.Key] = oldCheckDateTime; // restore old check date
                                }
                            }

                            if (info.NoRemoteFile)
                            {
                                info.Report.Add(
                                        (_isHtmlReport ? ManageSettings.UpdateReport.HtmlBeforeModReportWarningLine: string.Empty)
                                        + ManageSettings.UpdateReport.HtmlModReportInLineBeforeMainMessage
                                        + ManageSettings.UpdateReport.HtmlModReportPreModnameTags
                                        + info.TargetFolderPath.Name
                                        + ManageSettings.UpdateReport.HtmlModReportPostModnameTags
                                        + " "
                                        + T._("have new version but file for update not found")
                                        + ManageSettings.UpdateReport.HtmlModReportInLineAfterMainMessage                                        
                                        + (_isHtmlReport ? ManageSettings.UpdateReport.HtmlAfterModReportLine: string.Empty)
                                        + (!string.IsNullOrWhiteSpace(info.SourceLink) ? ManageSettings.UpdateReport.InfoLinkPattern+ info.SourceLink : string.Empty)
                                        );
                            }
                            else
                            {
                                target.RestoreBuckup();
                                //RestoreModFromBuckup(OldModBuckupDirPath, UpdatingModDirPath);

                                info.Report.Add(
                                        (_isHtmlReport ? ManageSettings.UpdateReport.HtmlBeforeModReportErrorLine: string.Empty)
                                        + ManageSettings.UpdateReport.HtmlModReportInLineBeforeMainMessage                                            
                                        + T._("Failed to update") + " " + info.TargetFolderPath.Name
                                        + ManageSettings.UpdateReport.HtmlModReportInLineAfterMainMessage                                            
                                        + ErrorMessage(info.LastErrorText)
                                        + " ("+ T._("Details in") + " " + Properties.Settings.Default.ApplicationProductName + ".log" + ")"
                                        + ManageSettings.UpdateReport.HtmlAfterModReportLine);

                                ManageLogs.Log("Failed to update" + " " + info.TargetFolderPath.Name /*+ ":" + Environment.NewLine + ex*/);
                            }
                        }
                    }
                }
            }

            pBar.Dispose();
            progressForm.Dispose();

            SaveCheckDateTimes();

            //save sizes
            //SaveSizes(info.UrlSizeList);

            //Show report
            ShowReport(info);
        }

        /// <summary>
        /// write update check datetimes to file
        /// </summary>
        private void SaveCheckDateTimes()
        {
            using (StreamWriter sw = new StreamWriter(ManageSettings.UpdateCheckDateTimesFilePath))
            {
                foreach (var source in lastCheckDates)
                {
                    foreach (var data in source.Value)
                    {
                        sw.WriteLine(source.Key + "|" + data.Key + "|" + data.Value.ToString("O"));
                    }
                }
            }

        }

        /// <summary>
        /// read update check datetimes from file
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, Dictionary<string, DateTime>> GetDateTimesFromFile()
        {
            var ret = new Dictionary<string, Dictionary<string, DateTime>>();
            var DateTimesFilePath = ManageSettings.UpdateCheckDateTimesFilePath;
            if (!File.Exists(DateTimesFilePath)) return ret;
            StreamReader sr = new StreamReader(DateTimesFilePath);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var data = line.Split('|');

                if (data.Length != 3) continue;

                if (!ret.ContainsKey(data[0]))
                {
                    ret.Add(data[0], new Dictionary<string, DateTime>());
                }
                if (!ret[data[0]].ContainsKey(data[1]))
                {
                    ret[data[0]].Add(data[1], DateTime.ParseExact(data[2], "O", CultureInfo.InvariantCulture));
                }
            }
            sr.Dispose();

            return ret;
        }

        private static string ErrorMessage(StringBuilder lastErrorText)
        {
            if (lastErrorText.Length == 0) return "";

            var ret = " " + lastErrorText.ToString() + " ";
            lastErrorText.Clear();
            return ret;
        }

        //private static void SaveSizes(Dictionary<string, long> urlSizeList)
        //{
        //    var sb = new StringBuilder();
        //    foreach (var pair in urlSizeList)
        //    {
        //        sb.AppendLine(pair.Key + "|" + pair.Value);
        //    }

        //    File.WriteAllText(ManageSettings.UpdateLastContentLengthInfos(), sb.ToString());
        //}

        private void ShowReport(UpdateInfo info)
        {
            string reportMessage;

            List<string> newReport = new List<string>();
            if (info.Report != null && info.Report.Count > 0)
            {
                foreach (var line in info.Report)
                {
                    string[] lines = line.Split(new[] { ManageSettings.UpdateReport.InfoLinkPattern }, StringSplitOptions.None);
                    if (lines.Length == 2 && _isHtmlReport)
                    {
                        newReport.Add(lines[0].Replace(
                              ManageSettings.UpdateReport.HtmlAfterModReportLine, _isHtmlReport ?
                                  ManageSettings.UpdateReport.HtmlModReportPreVersionTags + ManageSettings.UpdateReport.PreInfoLinkTitleText + ManageSettings.UpdateReport.HtmlModReportPostVersionTags + ManageSettings.UpdateReport.HtmlPreInfoLinkHtml + lines[1]
                                + ManageSettings.UpdateReport.HtmlAfterInfoLinkHtml + ManageSettings.UpdateReport.InfoLinkText + ManageSettings.UpdateReport.HtmlAfterInfoLinkText + ManageSettings.UpdateReport.HtmlModReportPreVersionTags + ManageSettings.UpdateReport.PostInfoLinkTitleText + ManageSettings.UpdateReport.HtmlModReportPostVersionTags + ManageSettings.UpdateReport.HtmlAfterModReportLine : string.Empty
                            ));
                    }
                    else
                    {
                        newReport.Add(lines[0]);
                    }
                }
            }

            var noModsInfo = newReport.Count == 0;

            if (File.Exists(ManageSettings.UpdateReport.ReportFilePath))
            {
                _isHtmlReport = true;
                reportMessage = File.ReadAllText(ManageSettings.UpdateReport.ReportFilePath)
                 .Replace(ManageSettings.UpdateReport.BgImageLinkPathPattern, ManageSettings.UpdateReport.CurrentGameBgFilePath)
                 .Replace(ManageSettings.UpdateReport.ModsUpdateReportHeaderTextPattern, ManageSettings.UpdateReport.TitleText)
                 .Replace(ManageSettings.UpdateReport.SingleModUpdateReportsTextSectionPattern, (noModsInfo ? ManageSettings.UpdateReport.NoModsUpdatesFoundText : string.Join(ManageSettings.UpdateReport.HtmlBetweenModsText, newReport)) + "<br>")
                 .Replace(ManageSettings.UpdateReport.ModsUpdateInfoNoticePattern, noModsInfo ? "" : ManageSettings.UpdateReport.ModsUpdateInfoNoticeText);
            }
            else
            {
                reportMessage =
                    (_isHtmlReport ? ManageSettings.UpdateReport.HtmlBeginText : string.Empty)
                    + ManageSettings.UpdateReport.TitleText + (_isHtmlReport ? ManageSettings.UpdateReport.HtmlAfterHeaderText : Environment.NewLine + Environment.NewLine)
                    + (noModsInfo ? ManageSettings.UpdateReport.NoModsUpdatesFoundText : string.Join(_isHtmlReport ? ManageSettings.UpdateReport.HtmlBetweenModsText : Environment.NewLine, newReport))
                    + "<br>"
                    + (_isHtmlReport ? ManageSettings.UpdateReport.HtmLendText : string.Empty);
            }
            //ReportMessage = string.Join(/*Environment.NewLine*/"<br>", report);
            //if (IsHTMLReport)
            {
                var htmlfile = ManageSettings.UpdateReportHtmlFilePath;

                Directory.CreateDirectory(Path.GetDirectoryName(htmlfile));// fix missing parent directory error

                File.WriteAllText(htmlfile, reportMessage);
                using (var process = new System.Diagnostics.Process())
                {
                    try
                    {
                        process.StartInfo.UseShellExecute = true;
                        process.StartInfo.FileName = htmlfile;
                        process.Start();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            //else
            //{
            //    MessageBox.Show(ReportMessage);
            //}

            //else
            //{
            //    ReportMessage = T._("No updates found");
            //    //ReportMessage = "<html><body><h2>" + T._("Update check report") + "</h2><br><br>" + T._("No updates found") + "</body></html>";
            //    MessageBox.Show(ReportMessage);
            //}
            //if (RequiestsLimit)
            //{
            //    ReportMessage = ReportMessage + Environment.NewLine + Environment.NewLine + T._("Github API rate limit exceeded for your IP.") + Environment.NewLine + "https://developer.github.com/v3/#rate-limiting";
            //}
        }
    }
}
