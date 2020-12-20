using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIHelper.Manage.Update.Sources
{
    class Github : SBase
    {
        public Github(updateInfo info) : base(info)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;//включение tls12 для github
        }

        internal override string url { get => "github.com"; }

        internal override string infoID => "updgit";

        internal override string title => "Github";

        internal override Task<bool> GetFile()
        {
            return DownloadTheFile();
        }

        static Form Dwnf;
        static ProgressBar Dwnpb;
        private async Task<bool> DownloadTheFile()
        {
            if (string.IsNullOrWhiteSpace(GitLatestVersionFileDownloadLink))
            {
                info.NoRemoteFile = true;
                return false;
            }

            var UpdateDir = ManageSettings.GetModsUpdateDir(); //Path.Combine(ManageSettings.GetCurrentGameModsUpdateDir(), modGitData.CurrentModName);
            Directory.CreateDirectory(UpdateDir);

            var UpdateFileName = Path.GetFileName(GitLatestVersionFileDownloadLink);

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

            info.UpdateFilePath = Path.Combine(UpdateDir, UpdateFileName);
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
                //PerformModUpdateFromArchive();
            };

            if (!File.Exists(info.UpdateFilePath))
            {
                await wc.DownloadFileTaskAsync(GitLatestVersionFileDownloadLink, info.UpdateFilePath).ConfigureAwait(true);
                return File.Exists(info.UpdateFilePath);
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

                if (new FileInfo(info.UpdateFilePath).Length == 0)
                {
                    File.Delete(info.UpdateFilePath);
                    return false;
                }

                //PerformModUpdateFromArchive();

                return true;
            }
        }

        internal override string GetLastVersion()
        {
            return GetLatestGithubVersionFromReleases();
        }

        internal string GitOwner;
        internal string GitRepository;
        internal string GitLatestVersion;
        //internal string GitLatestVersionOfFile;
        internal string GitLatestVersionFileDownloadLink;

        private string GetLatestGithubVersionFromReleases()
        {
            //using (WebClient wc = new WebClient())
            try
            {
                GitOwner = info.TargetFolderUpdateInfo[0];
                GitRepository = info.TargetFolderUpdateInfo[1];
                info.UpdateFileStartsWith = info.TargetFolderUpdateInfo[2];
                if (info.UpdateFileEndsWith.Length == 0)
                    info.UpdateFileEndsWith = (info.TargetFolderUpdateInfo.Length > 3 && info.TargetFolderUpdateInfo[3].ToUpperInvariant() != "TRUE" && info.TargetFolderUpdateInfo[3].ToUpperInvariant() != "FALSE") ? info.TargetFolderUpdateInfo[3] : "";
                info.VersionFromFile = info.TargetFolderUpdateInfo[info.TargetFolderUpdateInfo.Length - 1].ToUpperInvariant() == "TRUE";
                info.SourceLink = "https://github.com/" + GitOwner + "/" + GitRepository + "/releases/latest";
                var LatestReleasePage = wc.DownloadString(info.SourceLink);
                var version = Regex.Match(LatestReleasePage, "/releases/tag/[^\"]+\"");
                GitLatestVersion = version.Value.Remove(version.Value.Length - 1, 1).Remove(0, 14);

                var linkPattern = @"href\=""/" + GitOwner + "/" + GitRepository + "/releases/download/" + GitLatestVersion + "/" + info.UpdateFileStartsWith + "[^\"]+" + info.UpdateFileEndsWith + "\"";
                var link2file = Regex.Match(LatestReleasePage, linkPattern);

                if (link2file.Value.Length > 7 && link2file.Value.StartsWith("href=", StringComparison.InvariantCultureIgnoreCase))
                {
                    GitLatestVersionFileDownloadLink = "https://" + url + "/" + link2file.Value.Remove(link2file.Value.Length - 1, 1).Remove(0, 6);

                    if (info.VersionFromFile)
                    {
                        var pattern = "/releases/download/" + GitLatestVersion + "/" + info.UpdateFileStartsWith + "([^\"]+)" + (info.UpdateFileEndsWith.Length > 0 ? info.UpdateFileEndsWith : @"") + "\"";
                        var fromfile = Regex.Match(LatestReleasePage, pattern).Result("$1");
                        return fromfile;
                    }

                    return GitLatestVersion;
                }
                else
                {
                    ManageLogs.Log("GitHub sublink to file not found or incorrect. link:" + Environment.NewLine + link2file.Value+" / owner:"+ GitOwner+" / repository:"+ GitRepository);
                }
            }
            catch (Exception ex)
            {
                ManageLogs.Log("failed to check update. error:\r\n"+ex);
            }

            return "";
        }
    }
}
