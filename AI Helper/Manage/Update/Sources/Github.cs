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

        internal async override Task<bool> GetFile()
        {
            return await DownloadTheFile().ConfigureAwait(true);
        }

        static Form Dwnf;
        static ProgressBar Dwnpb;
        private async Task<bool> DownloadTheFile()
        {
            if (string.IsNullOrWhiteSpace(info.DownloadLink))
            {
                info.NoRemoteFile = true;
                return false;
            }

            var UpdateDownloadsDir = ManageSettings.GetModsUpdateDirDownloadsPath(); //Path.Combine(ManageSettings.GetCurrentGameModsUpdateDir(), modGitData.CurrentModName);
            Directory.CreateDirectory(UpdateDownloadsDir);

            var UpdateFileName = Path.GetFileName(info.DownloadLink);

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

            info.UpdateFilePath = Path.Combine(UpdateDownloadsDir, UpdateFileName);
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

            if (!File.Exists(info.UpdateFilePath)//not exist
                || (!info.VersionFromFile && File.Exists(info.UpdateFilePath))//when version from releases and filename is always same need to download it each time because exist in downloads is from older release
                || new FileInfo(info.UpdateFilePath).Length == 0//zero length van be when was failed previous download
                )
            {
                await DownloadFileTaskAsync(new Uri(info.DownloadLink), info.UpdateFilePath).ConfigureAwait(true);

                return File.Exists(info.UpdateFilePath) && new FileInfo(info.UpdateFilePath).Length != 0;
            }
            else
            {
                Dwnpb.Dispose();

                Dwnf.Dispose();

                if (new FileInfo(info.UpdateFilePath).Length == 0)
                {
                    File.Delete(info.UpdateFilePath);
                    return false;
                }

                //PerformModUpdateFromArchive();

                return true;
            }
        }

        ///// <summary>
        ///// when downloading file exists in install folder it will be moved in downloads dir
        ///// </summary>
        ///// <param name="PathInInstallDir"></param>
        ///// <param name="UpdateFilePath"></param>
        ///// <returns></returns>
        //private static bool FileExistsInInstallDir(string PathInInstallDir, string UpdateFilePath)
        //{
        //    if (File.Exists(PathInInstallDir) && !File.Exists(UpdateFilePath))
        //    {
        //        if(new FileInfo(PathInInstallDir).Length == 0)
        //        {
        //            File.Delete(PathInInstallDir);
        //            return false;
        //        }
        //        Directory.CreateDirectory(Path.GetDirectoryName(UpdateFilePath));
        //        File.Move(PathInInstallDir, UpdateFilePath);
        //    }
        //    return false;
        //}

        internal override string GetLastVersion()
        {
            return GetLatestGithubVersionFromReleases();
        }

        string GitOwner;
        string GitRepository;
        string GitLatestVersion;

        private string GetLatestGithubVersionFromReleases()
        {
            //using (WebClient wc = new WebClient())
            try
            {
                //if (info.GetVersionFromLink)
                //{
                //    info.SourceLink = info.TargetFolderUpdateInfo[0];
                //    var request = (HttpWebRequest)WebRequest.Create(new Uri(info.SourceLink));
                //    request.Method = "HEAD";
                //    var response = (HttpWebResponse)request.GetResponse();
                //    var data = response.LastModified;

                //    //var LastContentLength = GetLastContentLength(info.SourceLink);
                //    //if (LastContentLength != -1 && response.ContentLength == LastContentLength)
                //    //{
                //    //    return "";
                //    //}

                //    return null;
                //}

                GitOwner = info.TargetFolderUpdateInfo[0];
                GitRepository = info.TargetFolderUpdateInfo[1];
                info.UpdateFileStartsWith = info.TargetFolderUpdateInfo[2];
                if (info.UpdateFileEndsWith.Length == 0)
                    info.UpdateFileEndsWith = (info.TargetFolderUpdateInfo.Length > 3 && info.TargetFolderUpdateInfo[3].ToUpperInvariant() != "TRUE" && info.TargetFolderUpdateInfo[3].ToUpperInvariant() != "FALSE") ? info.TargetFolderUpdateInfo[3] : "";
                info.VersionFromFile = info.TargetFolderUpdateInfo[info.TargetFolderUpdateInfo.Length - 1].ToUpperInvariant() == "TRUE";
                info.SourceLink = "https://github.com/" + GitOwner + "/" + GitRepository + "/releases/latest";
                //info.SourceLink = "https://github.com/" + GitOwner + "/" + GitRepository + "/releases";

                //var request = (HttpWebRequest)WebRequest.Create(new Uri(info.SourceLink));
                //request.Method = "HEAD";
                //var response = (HttpWebResponse)request.GetResponse();
                //var LastContentLength = GetLastContentLength(info.SourceLink);
                //if (LastContentLength != -1 && response.ContentLength == LastContentLength)
                //{
                //    return "";
                //}

                var LatestReleasePage = wc.DownloadString(info.SourceLink);
                var version = Regex.Match(LatestReleasePage, @"/releases/tag/([^\""]+)\""");
                if (version.Success)
                {
                    GitLatestVersion = version.Result("$1");
                }
                //GitLatestVersion = version.Value.Remove(version.Value.Length - 1, 1).Remove(0, 14);

                var linkPattern = @"href\=\""(/" + GitOwner + "/" + GitRepository + "/releases/download/" + GitLatestVersion + "/" + info.UpdateFileStartsWith + @"([^\""]*)" + info.UpdateFileEndsWith + @")\""";
                var link2file = Regex.Match(LatestReleasePage, linkPattern);
                //var linkPattern = @"href\=\""(/" + GitOwner + "/" + GitRepository + "/releases/download/([^/]+)/" + info.UpdateFileStartsWith + @"([^\""]+)" + info.UpdateFileEndsWith + @")\""";
                //var link2file = Regex.Match(LatestReleasePage, linkPattern);
                if (!link2file.Success)//refind sublink to file
                {
                    //when author changed username on git
                    linkPattern = @"href\=\""(/[^/]+/" + GitRepository + "/releases/download/" + GitLatestVersion + "/" + info.UpdateFileStartsWith + @"([^\""]*)" + info.UpdateFileEndsWith + @")\""";
                    link2file = Regex.Match(LatestReleasePage, linkPattern);
                    //linkPattern = @"href\=\""(/[^/]+/" + GitRepository + "/releases/download/([^/]+)/" + info.UpdateFileStartsWith + @"([^\""]+)" + info.UpdateFileEndsWith + @")\""";
                    //link2file = Regex.Match(LatestReleasePage, linkPattern);
                }

                var GetFromLast10Releases = false;
                //List<GitReleasesInfo> Releases = null;
                if (!link2file.Success)
                {
                    GetFromLast10Releases = true;
                    info.SourceLink = "https://github.com/" + GitOwner + "/" + GitRepository + "/releases";
                    LatestReleasePage = wc.DownloadString(info.SourceLink);
                    linkPattern = @"href\=\""(/" + GitOwner + "/" + GitRepository + "/releases/download/([^/]+)/" + info.UpdateFileStartsWith + @"([^\""]+)" + info.UpdateFileEndsWith + @")\""";
                    link2file = Regex.Match(LatestReleasePage, linkPattern);

                    if (!link2file.Success)//refind sublink to file
                    {
                        //when author changed username on git
                        linkPattern = @"href\=\""(/[^/]+/" + GitRepository + "/releases/download/([^/]+)/" + info.UpdateFileStartsWith + @"([^\""]+)" + info.UpdateFileEndsWith + @")\""";
                        link2file = Regex.Match(LatestReleasePage, linkPattern);
                    }

                    if(link2file.Success && !info.VersionFromFile)
                    {
                        UpdateTools.CleanVersion(ref GitLatestVersion);
                        UpdateTools.CleanVersion(ref info.TargetCurrentVersion);
                        var fromReleases = link2file.Result("$2");
                        UpdateTools.CleanVersion(ref fromReleases);
                        //return empty when current version and release version is newer of last version from releases
                        if (info.TargetCurrentVersion.IsNewerOf(fromReleases) && GitLatestVersion.IsNewerOf(fromReleases))
                        {
                            info.DownloadLink = "";
                            return GitLatestVersion;
                        }
                    }

                    //Releases = GetGitLast10ReleasesInfo(GitOwner, GitRepository, info.UpdateFileStartsWith, info.UpdateFileEndsWith);
                }

                if ((link2file.Success && link2file.Value.Length > 7 && link2file.Value.StartsWith("href=", StringComparison.InvariantCultureIgnoreCase)) /*|| (Releases != null && Releases.Count > 0)*/)
                {
                    info.DownloadLink = "https://" + url + link2file.Result("$1");

                    if (info.VersionFromFile)
                    {
                        if (!GetFromLast10Releases)
                        {
                            var pattern = "/releases/download/" + GitLatestVersion + "/" + info.UpdateFileStartsWith + "([^\"]+)" + info.UpdateFileEndsWith + "\"";
                            var fromfile = Regex.Match(LatestReleasePage, pattern);
                            if (fromfile.Success)
                            {
                                GitLatestVersion = fromfile.Result("$1");
                            }
                        }
                        else
                        {
                            GitLatestVersion = link2file.Result("$3");
                        }
                        //else if (Releases != null && Releases.Count > 0)
                        //{
                        //    //first found release usually newest
                        //    info.DownloadLink = "https://" + url + "/" + Releases[0].Sublink;
                        //    GitLatestVersion = Releases[0].FileVersion;
                        //}
                        //return fromfile;
                    }
                    else if (GetFromLast10Releases)
                    {
                        GitLatestVersion = link2file.Result("$2");
                    }
                    //else
                    //{
                    //    GitLatestVersion = link2file.Result("$2");
                    //}

                    //SetContentLength(info.SourceLink, response.ContentLength);

                    return GitLatestVersion;
                }
                else
                {
                    ManageLogs.Log("GitHub sublink to file not found or incorrect.\r\n\tMod:" + info.TargetFolderPath.Name + "\r\n\tlink:" + info.SourceLink + "\r\n\tfile:" + info.UpdateFileStartsWith + "*" + info.UpdateFileEndsWith + " =>(Link to file" + (link2file.Success ? ": " + link2file.Value : " not found") + ")");

                    if (!info.VersionFromFile && GitLatestVersion.Length > 0)
                    {
                        info.DownloadLink = "";
                        return GitLatestVersion;
                    }
                }
            }
            catch (Exception ex)
            {
                ManageLogs.Log("failed to check update. error:\r\n" + ex);
                info.LastErrorText.Append(" >"+ex.Message);
            }

            return "";
        }

        /// <summary>
        /// Get all releases info
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="repository"></param>
        /// <param name="filenameStartsWith"></param>
        /// <param name="filenameEndsWith"></param>
        /// <returns></returns>
        //private List<GitReleasesInfo> GetGitLast10ReleasesInfo(string owner, string repository, string filenameStartsWith, string filenameEndsWith)
        //{
        //    //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        //    var ReleasesPage = wc.DownloadString(new Uri("https://github.com/" + owner + "/" + repository + "/releases"));

        //    var linkPattern = @"href\=\""(/" + owner + "/" + repository + "/releases/download/([^/]+)/" + filenameStartsWith + @"([^\""]+)" + filenameEndsWith + @")\""";
        //    var link2files = Regex.Matches(ReleasesPage, linkPattern);

        //    var Releases = new List<GitReleasesInfo>();
        //    if (link2files.Count > 0)
        //    {
        //        foreach (Match sublink in link2files)
        //        {
        //            Releases.Add(new GitReleasesInfo(sublink.Result("$1"), sublink.Result("$2"), sublink.Result("$3")));
        //        }
        //    }

        //    return Releases;
        //}

        //class GitReleasesInfo
        //{
        //    public string Sublink;
        //    public string ReleaseVesrion;
        //    public string FileVersion;

        //    public GitReleasesInfo(string sublink, string releaseVesrion, string fileVersion)
        //    {
        //        Sublink = sublink;
        //        ReleaseVesrion = releaseVesrion;
        //        FileVersion = fileVersion;
        //    }
        //}

        internal override byte[] DownloadFileFromTheLink(Uri link)
        {
            return wc.DownloadData(link);
        }

        //private void SetContentLength(string sourceLink, long contentLength)
        //{
        //    if (info.UrlSizeList.ContainsKey(sourceLink))
        //    {
        //        info.UrlSizeList[sourceLink] = contentLength;
        //    }
        //    else
        //    {
        //        info.UrlSizeList.Add(sourceLink, contentLength);
        //    }
        //}

        //private long GetLastContentLength(string sourceLink)
        //{
        //    if (info.UrlSizeList == null)
        //    {
        //        info.UrlSizeList = new Dictionary<string, long>();

        //        if (File.Exists(ManageSettings.UpdateLastContentLengthInfos()))
        //        {
        //            foreach (var line in File.ReadAllLines(ManageSettings.UpdateLastContentLengthInfos()))
        //            {
        //                if (line.Length == 0)
        //                {
        //                    continue;
        //                }
        //                var urldate = line.Split('|');
        //                if (urldate.Length == 2 && !info.UrlSizeList.ContainsKey(urldate[0]))
        //                {
        //                    info.UrlSizeList.Add(urldate[0], long.Parse(urldate[1], CultureInfo.InvariantCulture));
        //                }
        //            }
        //        }
        //    }

        //    if (info.UrlSizeList.ContainsKey(sourceLink))
        //    {
        //        return info.UrlSizeList[sourceLink];
        //    }

        //    return -1;
        //}

    }
}
