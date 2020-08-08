//using Octokit;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIHelper.Manage
{
    internal class ManageUpdates
    {
        /// <summary>
        /// GitHub mod data storing here for version checking/update time
        /// </summary>
        class ModGitData
        {
            /// <summary>
            /// Parent folder for repository
            /// </summary>
            internal string GitOwner;
            /// <summary>
            /// Repository name
            /// </summary>
            internal string GitName;
            /// <summary>
            /// First part of file name for update
            /// </summary>
            internal string GitFileNamePart;
            /// <summary>
            /// Currents installed version of mod
            /// </summary>
            internal string GitCurrentVersion;
            /// <summary>
            /// Latest GitHub version of mod
            /// </summary>
            internal string GitLatestVersion;
            /// <summary>
            /// Latest GitHub version download link for the mod file
            /// </summary>
            internal string GitLatestVersionFileDownloadLink;
            internal string GitLinkForVisit;

            public ModGitData(string[] gitinfo)
            {
                GitOwner = gitinfo[0];
                GitName = gitinfo[1];
                GitFileNamePart = gitinfo[2];
                GitCurrentVersion = gitinfo[3];
                GitLatestVersionFileDownloadLink = string.Empty;
            }
        }

        internal static class Mods
        {
            static ModGitData modGitData;
            static List<string> report;
            internal async static void CheckMods()
            {
                report = new List<string>();
                using (Form ProgressForm = new Form())
                {
                    using (ProgressBar PBar = new ProgressBar())
                    {
                        var AllModsList = ManageMO.GetModNamesListFromActiveMOProfile(false);

                        PBar.Dock = DockStyle.Bottom;
                        PBar.Maximum = AllModsList.Length;
                        ProgressForm.Controls.Add(PBar);
                        ProgressForm.StartPosition = FormStartPosition.CenterScreen;
                        ProgressForm.Size = new Size(300, 50);
                        var CheckNUpdateText = T._("Mods Check/Update");
                        ProgressForm.Text = CheckNUpdateText;
                        ProgressForm.FormBorderStyle = FormBorderStyle.FixedToolWindow;
                        ProgressForm.Show();
                        int ind = 0;
                        foreach (var mod in AllModsList)
                        {
                            PBar.Value = ind;
                            ProgressForm.Text = CheckNUpdateText + " " + mod;
                            var gitUpdateinfo = GetIMetaInfo(mod);
                            if (gitUpdateinfo != null && gitUpdateinfo.Length == 4)
                            {
                                //if (!ReportTitle)
                                //{
                                //    ReportTitle = true;
                                //    report.Add(T._("Update report") + ":" + Environment.NewLine + Environment.NewLine);
                                //}

                                modGitData = new ModGitData(gitUpdateinfo);

                                try
                                {
                                    await CheckAndUpdate(mod).ConfigureAwait(true);
                                }
                                catch (Exception ex)
                                {
                                    ManageLogs.Log("Update check error:" + Environment.NewLine + ex);
                                }
                                //if (RequiestsLimit)
                                //{
                                //    break;
                                //}
                            }
                            ind++;
                        }
                    }
                }

                ShowReport();
                wc.Dispose();
            }

            static bool IsHTMLReport = true;
            const string HTMLReportStyle = " style=\"background-color:gray;\"";
            const string HTMLBegin = "<html><body" + HTMLReportStyle + "><h1>";
            const string HTMLAfterHeader = "</h2><hr><br>";
            const string HTMLBetweenMods = "<br>";
            const string HTMLend = "<hr></body></html>";
            static string ReportModGithubPageLinkVisitText;
            private static void ShowReport()
            {
                ReportModGithubPageLinkVisitText = T._("Visit Github page");
                string ReportMessage;
                if (report != null && report.Count > 0)
                {
                    List<string> newReport = new List<string>();
                    foreach (var line in report)
                    {
                        string[] lines = line.Split(new[] { "{{visit}}" }, StringSplitOptions.None);
                        if (lines.Length == 2)
                        {
                            newReport.Add(lines[0].Replace("</p>", (IsHTMLReport ? " " + "<a href=\"" + lines[1] + "\">[" + ReportModGithubPageLinkVisitText + "]</a></p>" : string.Empty)));
                        }
                        else
                        {
                            newReport.Add(lines[0]);
                        }
                    }

                    var ReportFilePath = Path.Combine(ManageSettings.GetAppResDir(), "theme", "default", "report", ManageSettings.GetCurrentGameEXEName() + "Template.html");
                    if (File.Exists(ReportFilePath))
                    {
                        IsHTMLReport = true;
                        ReportMessage = File.ReadAllText(ReportFilePath)
                         .Replace("%BGImageLinkPath%", Path.Combine(ManageSettings.GetAppResDir(), "theme", "default", "report", ManageSettings.GetCurrentGameEXEName() + "BG.jpg").Replace(Path.DirectorySeparatorChar.ToString(),"/"))
                         .Replace("%ModsUpdateReportHeaderText%", T._("Update check report"))
                         .Replace("%SingleModUpdateReportsTextSection%", string.Join(HTMLBetweenMods, newReport));
                    }
                    else
                        ReportMessage =
                            (IsHTMLReport ? HTMLBegin : string.Empty)
                            + T._("Update check report")
                            + (IsHTMLReport ? HTMLAfterHeader : Environment.NewLine + Environment.NewLine)
                            + string.Join(IsHTMLReport ? HTMLBetweenMods : Environment.NewLine, newReport)
                            + (IsHTMLReport ? HTMLend : string.Empty);
                    //ReportMessage = string.Join(/*Environment.NewLine*/"<br>", report);
                    if (IsHTMLReport)
                    {
                        var htmlfile = Path.Combine(ManageSettings.GetCurrentGameModsUpdateDir(), "report.html");
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
                    ReportMessage = T._("No mod updates found");
                    //ReportMessage = "<html><body><h2>" + T._("Update check report") + "</h2><br><br>" + T._("No updates found") + "</body></html>";
                    MessageBox.Show(ReportMessage);
                }
                //if (RequiestsLimit)
                //{
                //    ReportMessage = ReportMessage + Environment.NewLine + Environment.NewLine + T._("Github API rate limit exceeded for your IP.") + Environment.NewLine + "https://developer.github.com/v3/#rate-limiting";
                //}
            }

            //static bool RequiestsLimit = false;
            private async static Task CheckAndUpdate(string mod)
            {
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;//включение tls12 для github

                //var GitOwner = gitinfo[0];
                //var GitName = gitinfo[1];
                //var GitFileNamePart = gitinfo[2];
                //gitinfo[3] = gitinfo[3].TrimFileVersion();
                //var currentVersion = gitinfo[3];

                modGitData.GitCurrentVersion = modGitData.GitCurrentVersion.TrimFileVersion();

                //try
                //{
                //var github = new GitHubClient(new ProductHeaderValue(GitName));
                //var latest = await github.Repository.Release.GetLatest(GitOwner, GitName).ConfigureAwait(true);

                GetLatestGithubVersion();

                modGitData.GitLatestVersion = modGitData.GitLatestVersion.TrimFileVersion();

                bool IsNewer = false;
                if (!string.IsNullOrWhiteSpace(modGitData.GitLatestVersionFileDownloadLink) && (IsNewer = IsLatestVersionNewerOfCurrent(modGitData.GitLatestVersion, modGitData.GitCurrentVersion))/*modGitData.GitLatestVersion != modGitData.GitCurrentVersion*/)
                {
                    await DownloadTheFile(mod).ConfigureAwait(true);
                }
                else
                {
                    if (IsNewer /*modGitData.GitLatestVersion != modGitData.GitCurrentVersion*/)
                    {
                        report.Add(
                            (IsHTMLReport ? "<p style=\"color=orange\">" : string.Empty)
                            + T._("Mod")
                            + " "
                            + mod
                            + " "
                            + T._("have new version but file for update not found")
                            + (IsHTMLReport ? "</p>" : string.Empty)
                            + " "
                            + (!string.IsNullOrWhiteSpace(modGitData.GitLinkForVisit) ? "{{visit}}" + modGitData.GitLinkForVisit : string.Empty));
                    }
                    ManageLogs.Log("GitHub updater> Mod " + mod + " skipped."
                        + (string.IsNullOrWhiteSpace(modGitData.GitLatestVersionFileDownloadLink) ? " Download link not found. link(" + modGitData.GitLatestVersionFileDownloadLink + ")" : string.Empty)
                        + (!IsNewer ? " Mod versions: current(" + modGitData.GitCurrentVersion + ")/latest(" + modGitData.GitLatestVersion + ")" : string.Empty));
                }
                //}
                //catch (RateLimitExceededException ex)
                //{
                //    RequiestsLimit = true;
                //    ManageLogs.Log("Github API rate limit per hour exceeded for the IP error:" + Environment.NewLine + ex);
                //}
            }

            private static bool IsLatestVersionNewerOfCurrent(string gitLatestVersion, string gitCurrentVersion)
            {
                if (string.IsNullOrWhiteSpace(gitLatestVersion))
                {
                    return false;
                }
                else if (string.IsNullOrWhiteSpace(gitCurrentVersion))
                {
                    return true;
                }

                var VersionPartsOfLatest = gitLatestVersion.Split('.', ',');
                var VersionPartsOfCurrent = gitCurrentVersion.Split('.', ',');
                int dInd = 0;
                var curCount = VersionPartsOfCurrent.Length;
                foreach (var latestDigit in VersionPartsOfLatest)
                {
                    if (curCount == dInd)//all digits was equal but current have smaller digits count
                    {
                        return true;
                    }
                    var currentDigit = VersionPartsOfCurrent[dInd];
                    var latestParsed = int.TryParse(latestDigit, out int latest);
                    var currentParsed = int.TryParse(currentDigit, out int current);
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
            static readonly WebClient wc = new WebClient();
            private static void GetLatestGithubVersion()
            {
                //using (WebClient wc = new WebClient())
                {
                    modGitData.GitLinkForVisit = "https://github.com/" + modGitData.GitOwner + "/" + modGitData.GitName + "/releases/latest";
                    var LatestReleasePage = wc.DownloadString(modGitData.GitLinkForVisit);
                    var version = Regex.Match(LatestReleasePage, "/releases/tag/[^\"]+\"");
                    modGitData.GitLatestVersion = version.Value.Remove(version.Value.Length - 1, 1).Remove(0, 14);
                    var link2file = Regex.Match(LatestReleasePage, @"href\=""/" + modGitData.GitOwner + "/" + modGitData.GitName + "/releases/download/" + modGitData.GitLatestVersion + "/" + modGitData.GitFileNamePart + "[^\"]+\"");

                    if (link2file.Value.Length > 7 && link2file.Value.StartsWith("href=", StringComparison.InvariantCultureIgnoreCase))
                    {
                        modGitData.GitLatestVersionFileDownloadLink = "https://github.com/" + link2file.Value.Remove(link2file.Value.Length - 1, 1).Remove(0, 6);
                    }
                    else
                    {
                        ManageLogs.Log("GitHub sublink to file not found or incorrect. link:" + Environment.NewLine + link2file.Value);
                    }
                }
            }

            static Form Dwnf;
            static ProgressBar Dwnpb;
            private async static Task DownloadTheFile(string mod)
            {
                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;//включение tls12 для github

                //using (WebClient wc = new WebClient())
                {
                    var fileDir = Path.Combine(ManageSettings.GetCurrentGameModsUpdateDir(), mod);
                    Directory.CreateDirectory(fileDir);

                    var fileName = Path.GetFileName(modGitData.GitLatestVersionFileDownloadLink);

                    //Octokit package Github API variant (60 connections per hour limit)
                    //var github = new GitHubClient(new ProductHeaderValue(GitName));
                    //Release latest = await github.Repository.Release.GetLatest(GitOwner, GitName).ConfigureAwait(true);
                    ////var latestAssets = await github.Repository.Release.Get(GitOwner, GitName, latest.Id).ConfigureAwait(true);
                    ////var archiveName = latestAssets.Name;
                    //foreach (var GitAsset in latest.Assets)
                    //{
                    //    if (GitAsset.Name.StartsWith(GitFileNamePart, StringComparison.InvariantCultureIgnoreCase))
                    //    {

                    //        break;
                    //    }
                    //}

                    Dwnpb = new ProgressBar
                    {
                        Dock = DockStyle.Bottom,
                        Maximum = 100
                    };
                    Dwnf = new Form
                    {
                        StartPosition = FormStartPosition.CenterScreen,
                        Size = new Size(400, 50),
                        Text = T._("Downloading") + " " + fileName,
                        FormBorderStyle = FormBorderStyle.FixedToolWindow
                    };
                    Dwnf.Controls.Add(Dwnpb);
                    Dwnf.Show();
                    wc.DownloadProgressChanged += (s, ea) =>
                    {
                        if (ea.ProgressPercentage <= Dwnpb.Maximum)
                        {
                            Dwnpb.Value = ea.ProgressPercentage;
                        }
                    };
                    var filePath = Path.Combine(fileDir, fileName);
                    wc.DownloadFileCompleted += (s, ea) =>
                    {
                        Dwnpb.Dispose();
                        Dwnf.Dispose();
                        //MessageBox.Show("Download Complete!");
                        PerformModUpdateFromArchive(filePath);
                    };

                    await wc.DownloadFileTaskAsync(modGitData.GitLatestVersionFileDownloadLink, Path.Combine(fileDir, fileName)).ConfigureAwait(true);
                }
            }

            private static void PerformModUpdateFromArchive(string filePath)
            {
                try
                {
                    var modname = Path.GetFileName(Path.GetDirectoryName(filePath));
                    var TargetModDirPath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), modname);

                    var oldModDirPath = Path.Combine(ManageSettings.GetCurrentGameModsUpdateDir(), "old", modname + modGitData.GitCurrentVersion);
                    if (Directory.Exists(oldModDirPath))
                    {
                        oldModDirPath += DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                    }

                    Directory.CreateDirectory(oldModDirPath);

                    foreach (var folder in Directory.GetDirectories(TargetModDirPath))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(folder));
                        Directory.Move(folder, Path.Combine(oldModDirPath, Path.GetFileName(folder)));
                    }
                    foreach (var file in Directory.GetFiles(TargetModDirPath))
                    {
                        string ext;
                        if ((ext = Path.GetExtension(file)) == ".ini" || ext == ".cfg" || (Path.GetFileName(Path.GetDirectoryName(file)) == TargetModDirPath && ext.IsPictureExtension()))
                        {
                            continue;
                        }
                        Directory.CreateDirectory(Path.GetDirectoryName(file));
                        Directory.Move(file, Path.Combine(oldModDirPath, Path.GetFileName(file)));
                    }

                    if (Path.GetExtension(filePath) == ".zip")
                    {
                        bool success = false;
                        using (ZipArchive archive = ZipFile.OpenRead(filePath))
                        {
                            try
                            {
                                archive.ExtractToDirectory(TargetModDirPath);
                                success = true;
                                ManageINI.WriteINIValue(Path.Combine(TargetModDirPath, "meta.ini"), "General", "version", modGitData.GitLatestVersion);

                                report.Add(
                                    (IsHTMLReport ? "<p style=\"color=lightgreen\">" : string.Empty)
                                    + T._("Mod")
                                    + " "
                                    + modGitData.GitName
                                    + " "
                                    + T._("updated to version")
                                    + " "
                                    + modGitData.GitLatestVersion
                                    + (IsHTMLReport ? "</p>" : string.Empty)
                                    + (!string.IsNullOrWhiteSpace(modGitData.GitLinkForVisit) ? "{{visit}}" + modGitData.GitLinkForVisit : string.Empty));
                            }
                            catch (Exception ex)
                            {
                                ManageLogs.Log("Error while perform mod update" + Environment.NewLine + ex);
                            }
                        }
                        if (success)
                        {
                            File.Delete(filePath);
                        }
                    }
                }
                catch { }
            }
            static bool ReportTitle = false;

            private static string[] GetIMetaInfo(string ModName)
            {
                var ModPath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), ModName);

                var metaPath = Path.Combine(ModPath, "meta.ini");
                if (File.Exists(metaPath))
                {
                    var metaNotes = ManageINI.GetINIValueIfExist(metaPath, "notes", "General");
                    //var metaComments = ManageINI.GetINIValueIfExist(metaPath, "comments", "General");

                    //updgit::bbepis,XUnity.AutoTranslator,XUnity.AutoTranslator-BepIn-5x-::
                    //var UpdateInfo = ManageHTML.GetTagInfoTextFromHTML(metaNotes, "updgit::");

                    ;
                    string UpdateInfo = Regex.Match(metaNotes, "updgit::.+,.+,.+::").Value;

                    if (!string.IsNullOrWhiteSpace(UpdateInfo))
                    {
                        UpdateInfo = UpdateInfo.Remove(UpdateInfo.Length - 2, 2).Remove(0, 8);
                        return (UpdateInfo + "," + ManageINI.GetINIValueIfExist(metaPath, "version", "General")).Split(new[] { Environment.NewLine, "\n", "\r", "," }, StringSplitOptions.RemoveEmptyEntries);
                    }
                }

                return null;
            }
        }
    }
}
