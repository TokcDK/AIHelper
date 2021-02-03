using AIHelper.Manage.Update.Sources;
using AIHelper.Manage.Update.Targets;
using AIHelper.Manage.Update.Targets.Mods;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        internal bool GetVersionFromLink;

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
                            info.TargetFolderUpdateInfo = tInfoArray; // get folder info
                                                                      //info.TargetCurrentVersion = tInfoArray[tInfoArray.Length - 1]; // get current version (last element of info)
                            info.TargetLastVersion = source.GetLastVersion(); // get last version

                            if (info.TargetLastVersion.Length == 0)
                            {
                                continue;
                            }

                            CleanVersion(ref info.TargetLastVersion);
                            CleanVersion(ref info.TargetCurrentVersion);

                            if (IsLatestVersionNewerOfCurrent(info.TargetLastVersion, info.TargetCurrentVersion)) //if it is last version then run update
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
                                            + T._("Details in") + " " + Application.ProductName + ".log"
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

            //Show report
            ShowReport(info);
        }

        /// <summary>
        /// Clean version from prefixes. change v13 to 13 and kind of
        /// </summary>
        /// <param name="version"></param>
        private static void CleanVersion(ref string version)
        {
            if (version.Length == 0)
                return;

            version = version.TrimFileVersion();

            //foreach (var prefix in new[] { "VERSION", "VER", "V" })
            //{
            //    if (version.ToUpperInvariant().StartsWith(prefix, StringComparison.InvariantCulture))
            //    {
            //        version = version.Remove(0, prefix.Length);
            //        break;
            //    }
            //}
        }

        private static bool IsLatestVersionNewerOfCurrent(string LatestVersion, string CurrentVersion)
        {
            if (string.IsNullOrWhiteSpace(LatestVersion))
            {
                return false;
            }
            else if (string.IsNullOrWhiteSpace(CurrentVersion))
            {
                return true;
            }

            var VersionPartsOfLatest = LatestVersion.TrimEnd('0', ',', '.').Split('.', ',');
            var VersionPartsOfCurrent = CurrentVersion.TrimEnd('0', ',', '.').Split('.', ',');
            int dInd = 0;
            var curCount = VersionPartsOfCurrent.Length;
            foreach (var DigitL in VersionPartsOfLatest)
            {
                if (curCount == dInd)//all digits was equal but current have smaller digits count
                {
                    return true;
                }
                var DigitC = VersionPartsOfCurrent[dInd];
                var latestParsed = int.TryParse(DigitL, out int latest);
                var currentParsed = int.TryParse(DigitC, out int current);
                if (latestParsed && currentParsed)
                {
                    if (latest > current)
                    {
                        return true;
                    }
                }
                dInd++;
            }
            return false;
        }

        const string HTMLReportStyle = " style=\"background-color:gray;\"";
        const string HTMLBegin = "<html><body" + HTMLReportStyle + "><h1>";
        const string HTMLAfterHeader = "</h2><hr><br>";
        const string HTMLBetweenMods = "";//<p> already make new line //"<br>";
        const string HTMLend = "<hr></body></html>";
        static string ReportModSourcePageLinkVisitText;
        private void ShowReport(updateInfo info)
        {
            ReportModSourcePageLinkVisitText = "[" + T._("Information") + "]";
            string ReportMessage;
            if (info.report != null && info.report.Count > 0)
            {
                List<string> newReport = new List<string>();
                foreach (var line in info.report)
                {
                    string[] lines = line.Split(new[] { "{{visit}}" }, StringSplitOptions.None);
                    if (lines.Length == 2 && IsHTMLReport)
                    {
                        newReport.Add(lines[0].Replace("</p>", (IsHTMLReport ? " / " + "<a target=\"_blank\" rel=\"noopener noreferrer\" href=\"" + lines[1] + "\">" + ReportModSourcePageLinkVisitText + "</a></p>" : string.Empty)));
                    }
                    else
                    {
                        newReport.Add(lines[0]);
                    }
                }

                var reportTitle = T._("Update report");
                var ReportFilePath = Path.Combine(ManageSettings.GetAppResDir(), "theme", "default", "report", "ReportTemplate.html");
                if (File.Exists(ReportFilePath))
                {
                    IsHTMLReport = true;
                    ReportMessage = File.ReadAllText(ReportFilePath)
                     .Replace("%BGImageLinkPath%", Path.Combine(ManageSettings.GetAppResDir(), "theme", "default", "report", ManageSettings.GetCurrentGameEXEName() + "ReportBG.jpg").Replace(Path.DirectorySeparatorChar.ToString(), "/"))
                     .Replace("%ModsUpdateReportHeaderText%", reportTitle)
                     .Replace("%SingleModUpdateReportsTextSection%", string.Join(HTMLBetweenMods, newReport))
                     .Replace("%ModsUpdateInfoNotice%", T._("If you click on <b style=\"font-size:20px;color:blue;\">[Info]</b> link you can view update important update info include requuirements and incompatibilities for latest update."));
                }
                else
                {
                    ReportMessage =
                        (IsHTMLReport ? HTMLBegin : string.Empty)
                        + reportTitle
                        + (IsHTMLReport ? HTMLAfterHeader : Environment.NewLine + Environment.NewLine)
                        + string.Join(IsHTMLReport ? HTMLBetweenMods : Environment.NewLine, newReport)
                        + (IsHTMLReport ? HTMLend : string.Empty);
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
