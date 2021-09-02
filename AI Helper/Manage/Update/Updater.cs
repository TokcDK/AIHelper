using AIHelper.Manage.Update.Sources;
using AIHelper.Manage.Update.Targets;
using AIHelper.Manage.Update.Targets.Mods;
using System;
using System.Collections.Generic;
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

        internal async Task Update()
        {
            UpdateInfo info = new UpdateInfo();
            var sources = new List<SBase> //Sources of updates
            {
                new Github(info)
            };
            var targets = new List<Base> //Targets for update
            {
                new Mo(info),
                //new MOBaseGames(info),
                new ModsList(info),
                new ModsMeta(info)
            };

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

            progressForm.Controls.Add(pBar);
            progressForm.Show();

            foreach (var source in sources) //enumerate sources
            {
                info.Source = source;

                progressForm.Text = checkNUpdateText + ":" + source.Title;

                info.SourceId = source.InfoId; //set source info detect ID
                foreach (var target in targets) //enumerate targets
                {
                    var tFolderInfos = target.GetUpdateInfos(); // get folderslist for update, usually it is active mods
                    if (tFolderInfos == null || tFolderInfos.Count == 0) // skip if no targets
                    {
                        continue;
                    }

                    info.Target = target;

                    pBar.Maximum = tFolderInfos.Keys.Count;

                    pBar.Value = 0;

                    foreach (var tFolderInfo in tFolderInfos) //enumerate all folders with info
                    {
                        if (info.Excluded.Contains(tFolderInfo.Key)) //skip already updated
                        {
                            continue;
                        }

                        info.Reset(); // reset some infos

                        // get folder dir path
                        info.TargetFolderPath = new DirectoryInfo(Path.Combine(target.GetParentFolderPath(), tFolderInfo.Key));

                        progressForm.Text = checkNUpdateText + ":" + source.Title + ">" + info.TargetFolderPath.Name;

                        if (pBar.Value < pBar.Maximum)
                        {
                            pBar.Value += 1;
                        }

                        // Set current version
                        target.SetCurrentVersion();

                        // get info to array
                        var tInfoArray = (tFolderInfo.Value.StartsWith(source.InfoId, StringComparison.InvariantCultureIgnoreCase) ? tFolderInfo.Value.Remove(tFolderInfo.Value.Length - 2, 2).Remove(0, source.InfoId.Length + 2) : tFolderInfo.Value).Split(new[] { Environment.NewLine, "\n", "\r", "," }, StringSplitOptions.None);

                        if (tInfoArray.Length == 0) // skip if info is invalid
                        {
                            continue;
                        }
                        info.TargetFolderUpdateInfo = tInfoArray.Trim(); // get folder info
                                                                         //info.TargetCurrentVersion = tInfoArray[tInfoArray.Length - 1]; // get current version (last element of info)
                        info.TargetLastVersion = source.GetLastVersion(); // get last version

                        if (info.TargetLastVersion.Length == 0)
                        {
                            continue;
                        }

                        UpdateTools.CleanVersion(ref info.TargetLastVersion);
                        UpdateTools.CleanVersion(ref info.TargetCurrentVersion);

                        if (info.TargetLastVersion.IsNewerOf(info.TargetCurrentVersion)) //if it is last version then run update
                        {
                            bool getfileIsTrue = await source.GetFile().ConfigureAwait(true); // download latest file

                            if (getfileIsTrue && target.MakeBuckup() && target.UpdateFiles() // update folder with new files
                                )
                            {
                                UpdatedAny = true;

                                info.Excluded.Add(tFolderInfo.Key); // add path to excluded to skip it next time if will be found for other source or target

                                info.Report.Add(
                                    (_isHtmlReport ? ManageSettings.UpdateReport.HtmlBeforeModReportSuccessLine() : string.Empty)
                                        + ManageSettings.UpdateReport.HtmlModReportInLineBeforeMainMessage()
                                            //+ T._("Mod")
                                            //+ " "
                                            + ManageSettings.UpdateReport.HtmlModReportPreModnameTags()
                                                + info.TargetFolderPath.Name
                                            + ManageSettings.UpdateReport.HtmlModReportPostModnameTags()
                                            + (!info.TargetFolderPath.Name.Contains(info.TargetCurrentVersion) ?
                                                    " "
                                                    + ManageSettings.UpdateReport.HtmlModReportPreVersionTags()
                                                        + info.TargetCurrentVersion
                                                    + ManageSettings.UpdateReport.HtmlModReportPostVersionTags()
                                                : "")
                                            + " "
                                            + T._("updated to version")
                                            + " "
                                            + ManageSettings.UpdateReport.HtmlModReportPreVersionTags()
                                                + info.TargetLastVersion
                                            + ManageSettings.UpdateReport.HtmlModReportPostVersionTags()
                                        + ManageSettings.UpdateReport.HtmlModReportInLineAfterMainMessage()
                                        + (_isHtmlReport ? ManageSettings.UpdateReport.HtmlAfterModReportLine() : string.Empty)
                                            + (!string.IsNullOrWhiteSpace(info.SourceLink) ? ManageSettings.UpdateReport.InfoLinkPattern() + info.SourceLink : string.Empty)
                                    );

                            }
                            else
                            {
                                if (info.NoRemoteFile)
                                {
                                    info.Report.Add(
                                        (_isHtmlReport ? ManageSettings.UpdateReport.HtmlBeforeModReportWarningLine() : string.Empty)
                                            + ManageSettings.UpdateReport.HtmlModReportInLineBeforeMainMessage()
                                                //+ T._("Mod")
                                                //+ " "
                                                + ManageSettings.UpdateReport.HtmlModReportPreModnameTags()
                                                    + info.TargetFolderPath.Name
                                                + ManageSettings.UpdateReport.HtmlModReportPostModnameTags()
                                                + " "
                                                + T._("have new version but file for update not found")
                                            + ManageSettings.UpdateReport.HtmlModReportInLineAfterMainMessage()
                                            + (_isHtmlReport ? ManageSettings.UpdateReport.HtmlAfterModReportLine() : string.Empty)
                                                + (!string.IsNullOrWhiteSpace(info.SourceLink) ? ManageSettings.UpdateReport.InfoLinkPattern() + info.SourceLink : string.Empty)
                                        );
                                }
                                else
                                {
                                    target.RestoreBuckup();
                                    //RestoreModFromBuckup(OldModBuckupDirPath, UpdatingModDirPath);

                                    info.Report.Add(
                                        (_isHtmlReport ? ManageSettings.UpdateReport.HtmlBeforeModReportErrorLine() : string.Empty)
                                            + ManageSettings.UpdateReport.HtmlModReportInLineBeforeMainMessage()
                                                + T._("Failed to update")
                                                + " "
                                                + info.TargetFolderPath.Name
                                            + ManageSettings.UpdateReport.HtmlModReportInLineAfterMainMessage()
                                                + ErrorMessage(info.LastErrorText)
                                                + " ("
                                                + T._("Details in") + " " + Properties.Settings.Default.ApplicationProductName + ".log"
                                                + ")"
                                        + ManageSettings.UpdateReport.HtmlAfterModReportLine()
                                        );

                                    ManageLogs.Log("Failed to update" + " " + info.TargetFolderPath.Name /*+ ":" + Environment.NewLine + ex*/);
                                }
                            }
                        }
                    }
                }
            }

            pBar.Dispose();
            progressForm.Dispose();

            //save sizes
            //SaveSizes(info.UrlSizeList);

            //Show report
            ShowReport(info);
        }

        private static string ErrorMessage(StringBuilder lastErrorText)
        {
            if (lastErrorText.Length > 0)
            {
                var ret = " " + lastErrorText.ToString() + " ";
                lastErrorText.Clear();
                return ret;
            }
            return "";
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

            {

                List<string> newReport = new List<string>();
                if (info.Report != null && info.Report.Count > 0)
                {
                    foreach (var line in info.Report)
                    {
                        string[] lines = line.Split(new[] { ManageSettings.UpdateReport.InfoLinkPattern() }, StringSplitOptions.None);
                        if (lines.Length == 2 && _isHtmlReport)
                        {
                            newReport.Add(lines[0].Replace(
                                  ManageSettings.UpdateReport.HtmlAfterModReportLine()
                                , _isHtmlReport ?
                                      ManageSettings.UpdateReport.HtmlModReportPreVersionTags()
                                        + ManageSettings.UpdateReport.PreInfoLinkTitleText()
                                    + ManageSettings.UpdateReport.HtmlModReportPostVersionTags()
                                    + ManageSettings.UpdateReport.HtmlPreInfoLinkHtml()
                                        + lines[1]
                                    + ManageSettings.UpdateReport.HtmlAfterInfoLinkHtml()
                                        + ManageSettings.UpdateReport.InfoLinkText()
                                    + ManageSettings.UpdateReport.HtmlAfterInfoLinkText()
                                    + ManageSettings.UpdateReport.HtmlModReportPreVersionTags()
                                        + ManageSettings.UpdateReport.PostInfoLinkTitleText()
                                    + ManageSettings.UpdateReport.HtmlModReportPostVersionTags()
                                + ManageSettings.UpdateReport.HtmlAfterModReportLine()
                                  : string.Empty
                                ));
                        }
                        else
                        {
                            newReport.Add(lines[0]);
                        }
                    }
                }

                var noModsInfo = newReport.Count == 0;

                if (File.Exists(ManageSettings.UpdateReport.GetReportFilePath()))
                {
                    _isHtmlReport = true;
                    reportMessage = File.ReadAllText(ManageSettings.UpdateReport.GetReportFilePath())
                     .Replace(ManageSettings.UpdateReport.GetBgImageLinkPathPattern(), ManageSettings.UpdateReport.GetCurrentGameBgFilePath())
                     .Replace(ManageSettings.UpdateReport.GetModsUpdateReportHeaderTextPattern(), ManageSettings.UpdateReport.TitleText())
                     .Replace(ManageSettings.UpdateReport.GetSingleModUpdateReportsTextSectionPattern(), (noModsInfo ? ManageSettings.UpdateReport.NoModsUpdatesFoundText() : string.Join(ManageSettings.UpdateReport.HtmlBetweenModsText(), newReport)) + "<br>")
                     .Replace(ManageSettings.UpdateReport.GetModsUpdateInfoNoticePattern(), noModsInfo ? "" : ManageSettings.UpdateReport.ModsUpdateInfoNoticeText());
                }
                else
                {
                    reportMessage =
                        (_isHtmlReport ? ManageSettings.UpdateReport.HtmlBeginText() : string.Empty)
                        + ManageSettings.UpdateReport.TitleText()
                        + (_isHtmlReport ? ManageSettings.UpdateReport.HtmlAfterHeaderText() : Environment.NewLine + Environment.NewLine)
                        + (noModsInfo ? ManageSettings.UpdateReport.NoModsUpdatesFoundText() : string.Join(_isHtmlReport ? ManageSettings.UpdateReport.HtmlBetweenModsText() : Environment.NewLine, newReport))
                        + "<br>"
                        + (_isHtmlReport ? ManageSettings.UpdateReport.HtmLendText() : string.Empty);
                }
                //ReportMessage = string.Join(/*Environment.NewLine*/"<br>", report);
                //if (IsHTMLReport)
                {
                    var htmlfile = ManageSettings.UpdateReportHtmlFilePath();

                    Directory.CreateDirectory(Path.GetDirectoryName(htmlfile));// fix missing parent directory error

                    File.WriteAllText(htmlfile, reportMessage);
                    System.Diagnostics.Process.Start(htmlfile);
                }
                //else
                //{
                //    MessageBox.Show(ReportMessage);
                //}
            }
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
