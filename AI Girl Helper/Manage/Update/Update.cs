using AIHelper.Manage.Update.Sources;
using AIHelper.Manage.Update.Targets;
using AIHelper.Manage.Update.Targets.Mods;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using AIHelper.Manage.Update;

namespace AIHelper.Manage.Update
{
    internal class updateInfo
    {
        internal string SourceID;
        internal string TargetCurrentVersion;
        internal string SourceLink;
        internal string TargetLastVersion;
        internal HashSet<string> Excluded;
        internal DirectoryInfo TargetFolderPath;
        internal string[] TargetFolderUpdateInfo;
        /// <summary>
        /// report content about update
        /// </summary>
        internal List<string> report;
        internal bool NoRemoteFile;
        /// <summary>
        /// path to update file
        /// </summary>
        internal string UpdateFilePath;
        /// <summary>
        /// string with wich update file name starts
        /// </summary>
        internal string UpdateFileStartsWith;
        /// <summary>
        /// string with wich update file name ends
        /// </summary>
        internal string UpdateFileEndsWith;
        /// <summary>
        /// buckup dir path for old versions of mods
        /// </summary>
        internal string BuckupDirPath;
        /// <summary>
        /// selected source
        /// </summary>
        internal SBase source;
        /// <summary>
        /// selected target
        /// </summary>
        internal TBase target;
        internal bool VersionFromFile;
        //internal bool GetVersionFromLink;
        //internal Dictionary<string, long> UrlSizeList;
        /// <summary>
        /// file download link
        /// </summary>
        public string DownloadLink { get; internal set; }

        public updateInfo()
        {
            Excluded = new HashSet<string>();
            report = new List<string>();
            reset();
        }

        /// <summary>
        /// vars need to be reset for each folder
        /// </summary>
        internal void reset()
        {
            TargetCurrentVersion = "";
            SourceLink = "";
            TargetLastVersion = "";
            TargetFolderPath = null;
            TargetFolderUpdateInfo = null;
            NoRemoteFile = false;
            UpdateFilePath = "";
            UpdateFileStartsWith = "";
            UpdateFileEndsWith = "";
            DownloadLink = "";
        }

    }

    class Update
    {
        private bool IsHTMLReport = true;

        internal async Task update()
        {
            updateInfo info = new updateInfo();
            var sources = new List<SBase> //Sources of updates
            {
                new Github(info)
            };
            var targets = new List<TBase> //Targets for update
            {
                new MO(info),
                //new MOBaseGames(info),
                new ModsList(info),
                new ModsMeta(info)
            };

            using (var ProgressForm = new Form())
            using (var PBar = new ProgressBar())
            {
                PBar.Dock = DockStyle.Bottom;
                ProgressForm.Controls.Add(PBar);
                ProgressForm.StartPosition = FormStartPosition.CenterScreen;
                ProgressForm.Size = new Size(400, 50);
                var CheckNUpdateText = T._("Checking");
                ProgressForm.Text = CheckNUpdateText;
                ProgressForm.FormBorderStyle = FormBorderStyle.FixedToolWindow;
                ProgressForm.Show();

                foreach (var source in sources) //enumerate sources
                {
                    info.source = source;

                    ProgressForm.Text = CheckNUpdateText + ":" + source.title;

                    info.SourceID = source.infoID; //set source info detect ID
                    foreach (var target in targets) //enumerate targets
                    {
                        var tFolderInfos = target.GetUpdateInfos(); // get folderslist for update, usually it is active mods
                        if (tFolderInfos == null || tFolderInfos.Count == 0) // skip if no targets
                        {
                            continue;
                        }

                        info.target = target;

                        PBar.Maximum = tFolderInfos.Keys.Count;

                        PBar.Value = 0;

                        foreach (var tFolderInfo in tFolderInfos) //enumerate all folders with info
                        {
                            if (info.Excluded.Contains(tFolderInfo.Key)) //skip already updated
                            {
                                continue;
                            }

                            info.reset(); // reset some infos

                            // get folder dir path
                            info.TargetFolderPath = new DirectoryInfo(Path.Combine(target.GetParentFolderPath(), tFolderInfo.Key));

                            ProgressForm.Text = CheckNUpdateText + ":" + source.title + ">" + info.TargetFolderPath.Name;

                            if (PBar.Value < PBar.Maximum)
                            {
                                PBar.Value += 1;
                            }

                            // Set current version
                            target.SetCurrentVersion();

                            // get info to array
                            var tInfoArray = (tFolderInfo.Value.StartsWith(source.infoID, StringComparison.InvariantCultureIgnoreCase) ? tFolderInfo.Value.Remove(tFolderInfo.Value.Length - 2, 2).Remove(0, source.infoID.Length + 2) : tFolderInfo.Value).Split(new[] { Environment.NewLine, "\n", "\r", "," }, StringSplitOptions.None);

                            if (tInfoArray == null || tInfoArray.Length == 0) // skip if info is invalid
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
                                    info.Excluded.Add(tFolderInfo.Key); // add path to excluded to skip it next time if will be found for other source or target

                                    info.report.Add(
                                        (IsHTMLReport ? "<p style=\"color:lightgreen\">" : string.Empty)
                                        + T._("Mod")
                                        + " "
                                        + info.TargetFolderPath.Name
                                        + (!info.TargetFolderPath.Name.Contains(info.TargetCurrentVersion) ? " " + info.TargetCurrentVersion : "")
                                        + " "
                                        + T._("updated to version")
                                        + " "
                                        + info.TargetLastVersion
                                        + " "
                                        + (IsHTMLReport ? "</p>" : string.Empty)
                                        + (!string.IsNullOrWhiteSpace(info.SourceLink) ? "{{visit}}" + info.SourceLink : string.Empty));

                                }
                                else
                                {
                                    if (info.NoRemoteFile)
                                    {
                                        info.report.Add(
                                            (IsHTMLReport ? "<p style=\"color:orange\">" : string.Empty)
                                            + T._("Mod")
                                            + " "
                                            + info.TargetFolderPath.Name
                                            + " "
                                            + T._("have new version but file for update not found")
                                            + (IsHTMLReport ? "</p>" : string.Empty)
                                            + " "
                                            + (!string.IsNullOrWhiteSpace(info.SourceLink) ? "{{visit}}" + info.SourceLink : string.Empty));
                                    }
                                    else
                                    {
                                        target.RestoreBuckup();
                                        //RestoreModFromBuckup(OldModBuckupDirPath, UpdatingModDirPath);

                                        info.report.Add(
                                            (IsHTMLReport ? "<p style=\"color:red\">" : string.Empty)
                                            + T._("Failed to update mod")
                                            + " "
                                            + info.TargetFolderPath.Name
                                            //+ " (" + ex.Message + ") "
                                            +" ("
                                            + T._("Details in") + " " + Application.ProductName + ".log"
                                            +")"
                                            + "</p>"
                                            );

                                        ManageLogs.Log("Failed to update mod" + " " + info.TargetFolderPath.Name /*+ ":" + Environment.NewLine + ex*/);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //save sizes
            //SaveSizes(info.UrlSizeList);

            //Show report
            ShowReport(info);
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

        private void ShowReport(updateInfo info)
        {
            string ReportMessage;
            if (info.report != null && info.report.Count > 0)
            {
                List<string> newReport = new List<string>();
                foreach (var line in info.report)
                {
                    string[] lines = line.Split(new[] { "{{visit}}" }, StringSplitOptions.None);
                    if (lines.Length == 2 && IsHTMLReport)
                    {
                        newReport.Add(lines[0].Replace("</p>", (IsHTMLReport ? " / " + "<a target=\"_blank\" rel=\"noopener noreferrer\" href=\"" + lines[1] + "\">" + ManageSettings.UpdateReport.ReportModSourcePageLinkVisitText() + "</a></p>" : string.Empty)));
                    }
                    else
                    {
                        newReport.Add(lines[0]);
                    }
                }

                if (File.Exists(ManageSettings.UpdateReport.GetReportFilePath()))
                {
                    IsHTMLReport = true;
                    ReportMessage = File.ReadAllText(ManageSettings.UpdateReport.GetReportFilePath())
                     .Replace(ManageSettings.UpdateReport.GetBGImageLinkPathPattern(), ManageSettings.UpdateReport.GetCurrentGameBGFilePath())
                     .Replace(ManageSettings.UpdateReport.GetModsUpdateReportHeaderTextPattern(), ManageSettings.UpdateReport.TitleText())
                     .Replace(ManageSettings.UpdateReport.GetSingleModUpdateReportsTextSectionPattern(), string.Join(ManageSettings.UpdateReport.HTMLBetweenModsText(), newReport))
                     .Replace(ManageSettings.UpdateReport.GetModsUpdateInfoNoticePattern(), ManageSettings.UpdateReport.ModsUpdateInfoNoticeText());
                }
                else
                {
                    ReportMessage =
                        (IsHTMLReport ? ManageSettings.UpdateReport.HTMLBeginText() : string.Empty)
                        + ManageSettings.UpdateReport.TitleText()
                        + (IsHTMLReport ? ManageSettings.UpdateReport.HTMLAfterHeaderText() : Environment.NewLine + Environment.NewLine)
                        + string.Join(IsHTMLReport ? ManageSettings.UpdateReport.HTMLBetweenModsText() : Environment.NewLine, newReport)
                        + (IsHTMLReport ? ManageSettings.UpdateReport.HTMLendText() : string.Empty);
                }
                //ReportMessage = string.Join(/*Environment.NewLine*/"<br>", report);
                if (IsHTMLReport)
                {
                    var htmlfile = ManageSettings.UpdateReportHTMLFilePath();
                    File.WriteAllText(htmlfile, ReportMessage);
                    System.Diagnostics.Process.Start(htmlfile);
                }
                //else
                //{
                //    MessageBox.Show(ReportMessage);
                //}
            }
            else
            {
                ReportMessage = T._("No updates found");
                //ReportMessage = "<html><body><h2>" + T._("Update check report") + "</h2><br><br>" + T._("No updates found") + "</body></html>";
                MessageBox.Show(ReportMessage);
            }
            //if (RequiestsLimit)
            //{
            //    ReportMessage = ReportMessage + Environment.NewLine + Environment.NewLine + T._("Github API rate limit exceeded for your IP.") + Environment.NewLine + "https://developer.github.com/v3/#rate-limiting";
            //}
        }
    }
}
