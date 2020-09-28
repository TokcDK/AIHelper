using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AIHelper.Manage.ManageUpdates;

namespace AIHelper.Manage.Update.UpdateMods
{
    class UpdateFromGithub : UpdateModsBase
    {
        public UpdateFromGithub(UpdateData updateData) : base(updateData)
        {
        }

        protected WebClient wc = new WebClient();
        internal override bool Check()
        {
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

                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;//включение tls12 для github

                    foreach (var mod in AllModsList)
                    {
                        PBar.Value = ind;
                        ProgressForm.Text = CheckNUpdateText + " " + mod;
                        var gitUpdateinfo = GetIMetaInfo(mod);
                        if (gitUpdateinfo != null && gitUpdateinfo.Length == 4)
                        {
                            updateData = new UpdateData(gitUpdateinfo)
                            {
                                CurrentModName = mod,
                                IsMod = true
                            };

                            updateData.GitCurrentVersion = updateData.GitCurrentVersion.TrimFileVersion();

                            GetLatestGithubVersion();

                            updateData.GitLatestVersion = updateData.GitLatestVersion.TrimFileVersion();

                            bool IsNewer = false;
                            if (!string.IsNullOrWhiteSpace(updateData.GitLatestVersionFileDownloadLink) && (IsNewer = IsLatestVersionNewerOfCurrent(updateData.GitLatestVersion, updateData.GitCurrentVersion)))
                            {
                                return true;
                            }
                            else
                            {
                                if (IsNewer)
                                {
                                    updateData.report.Add(
                                        (updateData.IsHTMLReport ? "<p style=\"color:orange\">" : string.Empty)
                                        + T._("Mod")
                                        + " "
                                        + updateData.CurrentModName
                                        + " "
                                        + T._("have new version but file for update not found")
                                        + (updateData.IsHTMLReport ? "</p>" : string.Empty)
                                        + " "
                                        + (!string.IsNullOrWhiteSpace(updateData.GitLinkForVisit) ? "{{visit}}" + updateData.GitLinkForVisit : string.Empty));
                                }
                                ManageLogs.Log("GitHub updater> Mod " + updateData.CurrentModName + " skipped."
                                    + (string.IsNullOrWhiteSpace(updateData.GitLatestVersionFileDownloadLink) ? " Download link not found. link(" + updateData.GitLatestVersionFileDownloadLink + ")" : string.Empty)
                                    + (!IsNewer ? " Mod versions: current(" + updateData.GitCurrentVersion + ")/latest(" + updateData.GitLatestVersion + ")" : string.Empty));
                            }
                        }
                        ind++;
                    }
                }
            }

            return false;
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

        private void GetLatestGithubVersion()
        {
            //using (WebClient wc = new WebClient())
            {
                updateData.GitLinkForVisit = "https://github.com/" + updateData.GitOwner + "/" + updateData.GitName + "/releases/latest";
                var LatestReleasePage = wc.DownloadString(updateData.GitLinkForVisit);
                var version = Regex.Match(LatestReleasePage, "/releases/tag/[^\"]+\"");
                updateData.GitLatestVersion = version.Value.Remove(version.Value.Length - 1, 1).Remove(0, 14);
                var link2file = Regex.Match(LatestReleasePage, @"href\=""/" + updateData.GitOwner + "/" + updateData.GitName + "/releases/download/" + updateData.GitLatestVersion + "/" + updateData.GitFileNamePart + "[^\"]+\"");

                if (link2file.Value.Length > 7 && link2file.Value.StartsWith("href=", StringComparison.InvariantCultureIgnoreCase))
                {
                    updateData.GitLatestVersionFileDownloadLink = "https://github.com/" + link2file.Value.Remove(link2file.Value.Length - 1, 1).Remove(0, 6);
                }
                else
                {
                    ManageLogs.Log("GitHub sublink to file not found or incorrect. link:" + Environment.NewLine + link2file.Value);
                }
            }
        }

        internal async override void Update()
        {
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

                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;//включение tls12 для github

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

                            updateData = new UpdateData(gitUpdateinfo)
                            {
                                CurrentModName = mod,
                                IsMod = true
                            };

                            try
                            {
                                await CheckAndUpdate().ConfigureAwait(true);
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
        }

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

        //static bool RequiestsLimit = false;
        private async Task CheckAndUpdate()
        {
            //var GitOwner = gitinfo[0];
            //var GitName = gitinfo[1];
            //var GitFileNamePart = gitinfo[2];
            //gitinfo[3] = gitinfo[3].TrimFileVersion();
            //var currentVersion = gitinfo[3];

            updateData.GitCurrentVersion = updateData.GitCurrentVersion.TrimFileVersion();

            //try
            //{
            //var github = new GitHubClient(new ProductHeaderValue(GitName));
            //var latest = await github.Repository.Release.GetLatest(GitOwner, GitName).ConfigureAwait(true);

            GetLatestGithubVersion();

            updateData.GitLatestVersion = updateData.GitLatestVersion.TrimFileVersion();

            bool IsNewer = false;
            if (!string.IsNullOrWhiteSpace(updateData.GitLatestVersionFileDownloadLink) && (IsNewer = IsLatestVersionNewerOfCurrent(updateData.GitLatestVersion, updateData.GitCurrentVersion))/*modGitData.GitLatestVersion != modGitData.GitCurrentVersion*/)
            {
                await DownloadTheFile().ConfigureAwait(true);
            }
            else
            {
                if (IsNewer /*modGitData.GitLatestVersion != modGitData.GitCurrentVersion*/)
                {
                    updateData.report.Add(
                        (updateData.IsHTMLReport ? "<p style=\"color:orange\">" : string.Empty)
                        + T._("Mod")
                        + " "
                        + updateData.CurrentModName
                        + " "
                        + T._("have new version but file for update not found")
                        + (updateData.IsHTMLReport ? "</p>" : string.Empty)
                        + " "
                        + (!string.IsNullOrWhiteSpace(updateData.GitLinkForVisit) ? "{{visit}}" + updateData.GitLinkForVisit : string.Empty));
                }
                ManageLogs.Log("GitHub updater> Mod " + updateData.CurrentModName + " skipped."
                    + (string.IsNullOrWhiteSpace(updateData.GitLatestVersionFileDownloadLink) ? " Download link not found. link(" + updateData.GitLatestVersionFileDownloadLink + ")" : string.Empty)
                    + (!IsNewer ? " Mod versions: current(" + updateData.GitCurrentVersion + ")/latest(" + updateData.GitLatestVersion + ")" : string.Empty));
            }
            //}
            //catch (RateLimitExceededException ex)
            //{
            //    RequiestsLimit = true;
            //    ManageLogs.Log("Github API rate limit per hour exceeded for the IP error:" + Environment.NewLine + ex);
            //}
        }

        static Form Dwnf;
        static ProgressBar Dwnpb;
        private async Task DownloadTheFile()
        {
            //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;//включение tls12 для github

            //using (WebClient wc = new WebClient())
            {
                var UpdateDir = ManageSettings.GetCurrentGameModsUpdateDir(); //Path.Combine(ManageSettings.GetCurrentGameModsUpdateDir(), modGitData.CurrentModName);
                Directory.CreateDirectory(UpdateDir);

                var UpdateFileName = Path.GetFileName(updateData.GitLatestVersionFileDownloadLink);

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
                    Text = T._("Downloading") + ": " + UpdateFileName,
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
                updateData.UpdateFilePath = Path.Combine(UpdateDir, UpdateFileName);
                wc.DownloadFileCompleted += (s, ea) =>
                {
                    if (Dwnpb != null)
                    {
                        Dwnpb.Dispose();
                    }

                    if (Dwnf != null)
                    {
                        Dwnf.Dispose();
                    }
                    //MessageBox.Show("Download Complete!");
                    PerformModUpdateFromArchive();
                };

                if (!File.Exists(updateData.UpdateFilePath))
                {
                    await wc.DownloadFileTaskAsync(updateData.GitLatestVersionFileDownloadLink, updateData.UpdateFilePath).ConfigureAwait(true);
                }
                else
                {
                    if (Dwnpb != null)
                    {
                        Dwnpb.Dispose();
                    }

                    if (Dwnf != null)
                    {
                        Dwnf.Dispose();
                    }

                    PerformModUpdateFromArchive();
                }
            }
        }

        private  void PerformModUpdateFromArchive()
        {
            if (!Path.GetFileNameWithoutExtension(updateData.UpdateFilePath).StartsWith(updateData.GitFileNamePart, StringComparison.InvariantCultureIgnoreCase)
                || !IsLatestVersionNewerOfCurrent(updateData.GitLatestVersion, updateData.GitCurrentVersion)
                )
            {
                return;
            }

            if (!updateData.IsMod)
            {
                return;
            }

            //var modname = modGitData.CurrentModName; //Path.GetFileName(Path.GetDirectoryName(filePath));
            var UpdatingModDirPath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), updateData.CurrentModName);

            var OldModBuckupDirPath = Path.Combine(ManageSettings.GetCurrentGameModsUpdateDir(), "old", updateData.CurrentModName + "_" + updateData.GitCurrentVersion);
            if (Directory.Exists(OldModBuckupDirPath))
            {
                OldModBuckupDirPath += "_" + DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            }

            Directory.CreateDirectory(OldModBuckupDirPath);

            bool success = false;

            try
            {
                MakeBuckup(OldModBuckupDirPath, UpdatingModDirPath);

                int code = 0;
                string ext;
                if (!File.Exists(updateData.UpdateFilePath))
                {
                    code = -1;
                }
                else if ((ext = Path.GetExtension(updateData.UpdateFilePath).ToUpperInvariant()) == ".ZIP")
                {
                    code = 1;
                }
                else if (ext == ".DLL")
                {
                    code = 2;
                }

                if (code > 0)
                {
                    if (code == 1)
                    {
                        using (ZipArchive archive = ZipFile.OpenRead(updateData.UpdateFilePath))
                        {
                            archive.ExtractToDirectory(UpdatingModDirPath);
                        }
                    }
                    else if (code == 2)
                    {
                        var FullDllFileName = Path.GetFileName(updateData.UpdateFilePath);
                        if (updateData.UpdateFilenameSubPathData.ContainsKey(FullDllFileName))
                        {
                            var targetDir = UpdatingModDirPath + Path.DirectorySeparatorChar + updateData.UpdateFilenameSubPathData[FullDllFileName];
                            Directory.CreateDirectory(targetDir);
                            var targetFilePath = Path.Combine(targetDir, FullDllFileName);
                            File.Move(updateData.UpdateFilePath, targetFilePath);
                        }
                        else
                        {
                            foreach (var dll in Directory.EnumerateFiles(OldModBuckupDirPath, "*.dll", SearchOption.AllDirectories))
                            {
                                if (Path.GetFileName(dll) == FullDllFileName)
                                {
                                    var targetDir = Path.GetDirectoryName(dll).Replace(OldModBuckupDirPath, UpdatingModDirPath);
                                    Directory.CreateDirectory(targetDir);
                                    var targetFilePath = Path.Combine(targetDir, FullDllFileName);
                                    File.Move(updateData.UpdateFilePath, targetFilePath);
                                    break;
                                }
                            }
                        }
                    }
                    success = true;
                }
                if (success)
                {
                    //File.Delete(modGitData.UpdateFilePath);

                    RestoreSomeFiles(OldModBuckupDirPath, UpdatingModDirPath);

                    updateData.GitCurrentVersion = updateData.GitLatestVersion;

                    ManageINI.WriteINIValue(Path.Combine(UpdatingModDirPath, "meta.ini"), "General", "version", updateData.GitLatestVersion);

                    updateData.report.Add(
                        (updateData.IsHTMLReport ? "<p style=\"color:lightgreen\">" : string.Empty)
                        + T._("Mod")
                        + " "
                        + updateData.GitName
                        + " "
                        + T._("updated to version")
                        + " "
                        + updateData.GitLatestVersion
                        + (updateData.IsHTMLReport ? "</p>" : string.Empty)
                        + (!string.IsNullOrWhiteSpace(updateData.GitLinkForVisit) ? "{{visit}}" + updateData.GitLinkForVisit : string.Empty));
                }
                else
                {
                    RestoreModFromBuckup(OldModBuckupDirPath, UpdatingModDirPath);

                    updateData.report.Add(
                        (updateData.IsHTMLReport ? "<p style=\"color:orange\">" : string.Empty)
                        + T._("Failed to update mod")
                        + " "
                        + updateData.CurrentModName
                        + " "
                        + (code == 1 ? " " + T._("Update file not found") : string.Empty)
                        + (code == 2 ? " " + T._("Update file not a zip") : string.Empty)
                        + "</p>"
                        );
                }
            }
            catch (Exception ex)
            {
                RestoreModFromBuckup(OldModBuckupDirPath, UpdatingModDirPath);

                updateData.report.Add(
                    (updateData.IsHTMLReport ? "<p style=\"color:red\">" : string.Empty)
                    + T._("Failed to update mod")
                    + " "
                    + updateData.CurrentModName
                    + " (" + ex.Message + ") "
                    + T._("Details in") + " " + Application.ProductName + ".log"
                    + "</p>"
                    );

                ManageLogs.Log("Failed to update mod" + " " + updateData.CurrentModName + ":" + Environment.NewLine + ex);
            }
        }

        private static void RestoreSomeFiles(string OldModBuckupDirPath, string UpdatingModDirPath)
        {
            //restore some files files
            foreach (var file in Directory.GetFiles(OldModBuckupDirPath))
            {
                try
                {
                    string ext;
                    if (File.Exists(Path.Combine(UpdatingModDirPath, Path.GetFileName(file))) ||
                        (
                            (ext = Path.GetExtension(file)) != ".ini"
                            && ext != ".cfg"
                            && !ext.IsPictureExtension()
                            )
                        )
                    {
                        continue;
                    }
                    var filePath = file.Replace(OldModBuckupDirPath, UpdatingModDirPath);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    if (File.Exists(file))
                    {
                        File.Move(file, filePath);
                    }
                }
                catch// (Exception ex)
                {
                    //ManageLogs.Log("Update: Failed to move file to bak dir. error:" + Environment.NewLine + ex);
                }
            }
        }

        private static void RestoreModFromBuckup(string OldModBuckupDirPath, string UpdatingModDirPath)
        {
            //restore old dirs
            foreach (var folder in Directory.GetDirectories(OldModBuckupDirPath))
            {
                try
                {
                    var targetPath = folder.Replace(OldModBuckupDirPath, UpdatingModDirPath);
                    if (Directory.Exists(targetPath))
                    {
                        Directory.Delete(targetPath, true);
                    }
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                    if (Directory.Exists(folder))
                    {
                        Directory.Move(folder, targetPath);
                    }
                }
                catch (Exception ex)
                {
                    ManageLogs.Log("Restore: Failed to move dir to mod dir. error:" + Environment.NewLine + ex);
                }
            }
            //restore old files
            foreach (var file in Directory.GetFiles(OldModBuckupDirPath))
            {
                try
                {
                    var targetPath = file.Replace(OldModBuckupDirPath, UpdatingModDirPath);
                    if (File.Exists(targetPath))
                    {
                        File.Delete(targetPath);
                    }
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                    if (File.Exists(file))
                    {
                        File.Move(file, targetPath);
                    }
                }
                catch (Exception ex)
                {
                    ManageLogs.Log("Restore: Failed to move file to mod dir. error:" + Environment.NewLine + ex);
                }
            }

            ManageFilesFolders.DeleteEmptySubfolders(OldModBuckupDirPath);
        }

        private static void MakeBuckup(string OldModBuckupDirPath, string UpdatingModDirPath)
        {

            //buckup old dirs
            foreach (var folder in Directory.GetDirectories(UpdatingModDirPath))
            {
                try
                {
                    var backupPath = folder.Replace(UpdatingModDirPath, OldModBuckupDirPath);
                    Directory.CreateDirectory(Path.GetDirectoryName(backupPath));
                    if (Directory.Exists(folder))
                    {
                        Directory.Move(folder, backupPath);
                    }
                }
                catch (Exception ex)
                {
                    ManageLogs.Log("Update: Filed to move dir to bak dir. error:" + Environment.NewLine + ex);
                }
            }
            //buckup old files
            foreach (var file in Directory.GetFiles(UpdatingModDirPath))
            {
                try
                {
                    if (Path.GetFileName(file) == "meta.ini")
                    {
                        continue;
                    }
                    //string ext;
                    //if ((ext = Path.GetExtension(file)) == ".ini" || ext == ".cfg" || (Path.GetFileName(Path.GetDirectoryName(file)) == UpdatingModDirPath && ext.IsPictureExtension()))
                    //{
                    //    continue;
                    //}
                    var backupPath = file.Replace(UpdatingModDirPath, OldModBuckupDirPath);
                    Directory.CreateDirectory(Path.GetDirectoryName(backupPath));
                    File.Move(file, backupPath);
                }
                catch (Exception ex)
                {
                    ManageLogs.Log("Update: Failed to move file to bak dir. error:" + Environment.NewLine + ex);
                }
            }
        }
    }
}
