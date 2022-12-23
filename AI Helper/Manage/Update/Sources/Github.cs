using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIHelper.Manage.Update.Sources
{
    class Github : UpdateSourceBase
    {
        public Github(UpdateInfo info) : base(info)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;//включение tls12 для github
        }

        internal override string Url { get => "github.com"; }

        internal override string InfoId => "updgit";

        internal override string Title => "Github";

        internal async override Task<bool> GetFile()
        {
            return await DownloadTheFile().ConfigureAwait(true);
        }

        protected override void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            base.DownloadProgressChanged(sender, e);

            if (e.ProgressPercentage <= _dwnpb.Maximum)
            {
                _dwnpb.Value = e.ProgressPercentage;
            }
        }

        protected override void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            base.DownloadFileCompleted(sender, e);

            if (_dwnpb != null)
            {
                _dwnpb.Dispose();
            }

            if (_dwnf != null)
            {
                _dwnf.Dispose();
            }
        }

        static Form _dwnf;
        static ProgressBar _dwnpb;
        private async Task<bool> DownloadTheFile()
        {
            var updateDownloadsDir = ManageSettings.ModsUpdateDirDownloadsPath; //Path.Combine(ManageSettings.GetCurrentGameModsUpdateDir(), modGitData.CurrentModName);
            Directory.CreateDirectory(updateDownloadsDir);

            var updateFileName = Info.UpdateFilePath.Length > 0 ? Path.GetFileName(Info.UpdateFilePath) : Path.GetFileName(Info.DownloadLink);

            if (string.IsNullOrWhiteSpace(updateFileName))
            {
                bool found = false;
                foreach (var targetDir in new[] { updateDownloadsDir, ManageSettings.Install2MoDirPath })
                {
                    foreach (var prefix in new[] { "", "v" })
                    {
                        foreach (var ending in new[] { "", ".0" })
                        {
                            var possibleName = Info.UpdateFileStartsWith + prefix + Info.TargetLastVersion + ending + Info.UpdateFileEndsWith;
                            if (File.Exists(Path.Combine(targetDir, possibleName)))
                            {
                                found = true;
                                updateFileName = possibleName;
                                Info.UpdateFilePath = Path.Combine(targetDir, possibleName);
                                break;
                            }
                        }

                        if (found) break;
                    }

                    if (found) break;
                }
            }
            else
            {
                Info.UpdateFilePath = Path.Combine(updateDownloadsDir, updateFileName);
            }

            string altUpdateFilePath = Path.Combine(ManageSettings.Install2MoDirPath, updateFileName);
            if (Info.VersionFromFile && !File.Exists(Info.UpdateFilePath) && File.Exists(altUpdateFilePath))
            {
                Info.UpdateFilePath = altUpdateFilePath;
            }

            _dwnpb = new ProgressBar
            {
                Dock = DockStyle.Bottom,
                Maximum = 100
            };
            _dwnf = new Form
            {
                StartPosition = FormStartPosition.CenterScreen,
                TopMost = true,
                Size = new Size(400, 50),
                Text = T._("Downloading") + ": " + updateFileName,
                FormBorderStyle = FormBorderStyle.FixedToolWindow
            };
            _dwnf.Controls.Add(_dwnpb);
            _dwnf.Show();

            if (!File.Exists(Info.UpdateFilePath)//not exist
                || (!Info.VersionFromFile && File.Exists(Info.UpdateFilePath) && !updateFileName.Contains(Info.TargetLastVersion))//when version from releases and filename is always same need to download it each time because exist in downloads is from older release
                || new FileInfo(Info.UpdateFilePath).Length == 0//zero length van be when was failed previous download
                )
            {
                if (string.IsNullOrWhiteSpace(Info.DownloadLink))
                {
                    Info.NoRemoteFile = true;
                    _dwnpb.Dispose();
                    _dwnf.Dispose();
                    return false;
                }

                await DownloadFileTaskAsync(new Uri(Info.DownloadLink), Info.UpdateFilePath).ConfigureAwait(true);

                return IsCompletedDownload && File.Exists(Info.UpdateFilePath) && new FileInfo(Info.UpdateFilePath).Length != 0;
            }
            else
            {
                _dwnpb.Dispose();

                _dwnf.Dispose();

                if (new FileInfo(Info.UpdateFilePath).Length == 0)
                {
                    File.Delete(Info.UpdateFilePath);
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

        string _gitOwner;
        string _gitRepository;
        string _gitLatestVersion;

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

                _gitOwner = Info.TargetFolderUpdateInfo[0];
                _gitRepository = Info.TargetFolderUpdateInfo[1];
                Info.UpdateFileStartsWith = Info.TargetFolderUpdateInfo[2];
                if (Info.UpdateFileEndsWith.Length == 0)
                    Info.UpdateFileEndsWith = (Info.TargetFolderUpdateInfo.Length > 3 && Info.TargetFolderUpdateInfo[3].ToUpperInvariant() != "TRUE" && Info.TargetFolderUpdateInfo[3].ToUpperInvariant() != "FALSE") ? Info.TargetFolderUpdateInfo[3] : "";
                Info.VersionFromFile = Info.TargetFolderUpdateInfo[Info.TargetFolderUpdateInfo.Length - 1].ToUpperInvariant() == "TRUE";
                Info.SourceLink = $"https://github.com/{_gitOwner}/{_gitRepository}/releases/latest";
                //info.SourceLink = "https://github.com/" + GitOwner + "/" + GitRepository + "/releases";

                //var request = (HttpWebRequest)WebRequest.Create(new Uri(info.SourceLink));
                //request.Method = "HEAD";
                //var response = (HttpWebResponse)request.GetResponse();
                //var LastContentLength = GetLastContentLength(info.SourceLink);
                //if (LastContentLength != -1 && response.ContentLength == LastContentLength)
                //{
                //    return "";
                //}

                var latestReleasePage = WC.DownloadString(Info.SourceLink);
                string assetsPagePattern = @"src\=\""(https\:\/\/github\.com\/" + @"([^\/]+)" + @"\/" + @"([^\/]+)" + @"\/releases\/expanded_assets\/([^\""]+))\""";
                // 1 = full assets page link, 2 - owner name (can be changed and be different from old), 3 - repository name (can be changed), 4 - version
                var assetPageMatch = Regex.Match(latestReleasePage, assetsPagePattern);
                if (assetPageMatch.Success)
                {
                    // check owner and repository names because they can be changed
                    var currentOwnerName = assetPageMatch.Result("$2");
                    if (currentOwnerName != _gitOwner)
                    {
                        Info.TargetFolderUpdateInfo[0] = _gitOwner = currentOwnerName;
                    }
                    var currentRepName = assetPageMatch.Result("$3");
                    if (currentRepName != _gitRepository)
                    {
                        Info.TargetFolderUpdateInfo[1] = _gitRepository = currentRepName;
                    }

                    // set latest version and redownload files page from assets page
                    if (assetPageMatch.Success) _gitLatestVersion = assetPageMatch.Result("$4");
                    latestReleasePage = WC.DownloadString(assetPageMatch.Result("$1")); // redownload assets page as release page
                }
                else
                {
                    var version = Regex.Match(latestReleasePage, @"/releases/tag/([^\""]+)\""");
                    if (version.Success) _gitLatestVersion = version.Result("$1");
                    //GitLatestVersion = version.Value.Remove(version.Value.Length - 1, 1).Remove(0, 14);
                }

                var linkPattern = @"href\=\""(/" + _gitOwner + "/" + _gitRepository + "/releases/download/" + _gitLatestVersion + "/" + Info.UpdateFileStartsWith + "([^\"]*)" + Info.UpdateFileEndsWith + ")\"";
                var link2File = Regex.Match(latestReleasePage, linkPattern);
                //var linkPattern = @"href\=\""(/" + GitOwner + "/" + GitRepository + "/releases/download/([^/]+)/" + info.UpdateFileStartsWith + @"([^\""]+)" + info.UpdateFileEndsWith + @")\""";
                //var link2file = Regex.Match(LatestReleasePage, linkPattern);
                if (!link2File.Success && Info.VersionFromFile)
                {
                    Directory.CreateDirectory(ManageSettings.Install2MoDirPath);

                    foreach (var file in Directory.GetFiles(ManageSettings.Install2MoDirPath, Info.UpdateFileStartsWith + "*" + Info.UpdateFileEndsWith))
                    {
                        var ver = Regex.Match(Path.GetFileName(file), Info.UpdateFileStartsWith + "(.*)" + Info.UpdateFileEndsWith).Groups[1].Value;
                        UpdateTools.CleanVersion(ref ver);
                        UpdateTools.CleanVersion(ref Info.TargetCurrentVersion);
                        if (ver == _gitLatestVersion || ver.IsNewerOf(Info.TargetCurrentVersion))
                        {
                            Info.UpdateFilePath = file;
                            return ver;
                        }
                    }
                }

                if (!link2File.Success)//refind sublink to file
                {
                    //when author changed username on git
                    linkPattern = @"href\=\""(/[^/]+/" + _gitRepository + "/releases/download/" + _gitLatestVersion + "/" + Info.UpdateFileStartsWith + @"([^\""]*)" + Info.UpdateFileEndsWith + @")\""";
                    link2File = Regex.Match(latestReleasePage, linkPattern);
                    //linkPattern = @"href\=\""(/[^/]+/" + GitRepository + "/releases/download/([^/]+)/" + info.UpdateFileStartsWith + @"([^\""]+)" + info.UpdateFileEndsWith + @")\""";
                    //link2file = Regex.Match(LatestReleasePage, linkPattern);
                }

                var getFromLast10Releases = false;
                //List<GitReleasesInfo> Releases = null;
                if (!link2File.Success && Info.VersionFromFile)
                {
                    // search in releases assets
                    getFromLast10Releases = true;
                    Info.SourceLink = "https://github.com/" + _gitOwner + "/" + _gitRepository + "/releases";
                    latestReleasePage = WC.DownloadString(Info.SourceLink);
                    MatchCollection assetsList = Regex.Matches(latestReleasePage, assetsPagePattern);
                    foreach (Match assetsMatch in assetsList)
                    {
                        string theReleaseAssetsPage = WC.DownloadString(assetsMatch.Result("$1")); // download page for the release assets

                        linkPattern = @"href\=\""(/" + _gitOwner + "/" + _gitRepository + "/releases/download/([^/]+)/" + Info.UpdateFileStartsWith + @"([^\""]+)" + Info.UpdateFileEndsWith + @")\""";
                        link2File = Regex.Match(theReleaseAssetsPage, linkPattern);

                        if (!link2File.Success) continue;

                        latestReleasePage = theReleaseAssetsPage;
                        break;
                    }

                    if (!link2File.Success)//refind sublink to file
                    {
                        //when author changed username on git
                        linkPattern = @"href\=\""(/[^/]+/" + _gitRepository + "/releases/download/([^/]+)/" + Info.UpdateFileStartsWith + @"([^\""]+)" + Info.UpdateFileEndsWith + @")\""";
                        link2File = Regex.Match(latestReleasePage, linkPattern);
                    }

                    if (link2File.Success && !Info.VersionFromFile)
                    {
                        UpdateTools.CleanVersion(ref _gitLatestVersion);
                        UpdateTools.CleanVersion(ref Info.TargetCurrentVersion);
                        var fromReleases = link2File.Result("$2");
                        UpdateTools.CleanVersion(ref fromReleases);
                        //return empty when current version and release version is newer of last version from releases
                        if (Info.TargetCurrentVersion.IsNewerOf(fromReleases) && _gitLatestVersion.IsNewerOf(fromReleases))
                        {
                            Info.DownloadLink = "";
                            return _gitLatestVersion;
                        }
                    }

                    //Releases = GetGitLast10ReleasesInfo(GitOwner, GitRepository, info.UpdateFileStartsWith, info.UpdateFileEndsWith);
                }

                if ((link2File.Success && link2File.Value.Length > 7 && link2File.Value.StartsWith("href=", StringComparison.InvariantCultureIgnoreCase)) /*|| (Releases != null && Releases.Count > 0)*/)
                {
                    Info.DownloadLink = "https://" + Url + link2File.Result("$1");

                    if (Info.VersionFromFile)
                    {
                        if (!getFromLast10Releases)
                        {
                            var pattern = "/releases/download/" + _gitLatestVersion + "/" + Info.UpdateFileStartsWith + "([^\"]+)" + Info.UpdateFileEndsWith + "\"";
                            var fromfile = Regex.Match(latestReleasePage, pattern);
                            if (fromfile.Success)
                            {
                                _gitLatestVersion = fromfile.Result("$1");
                            }
                        }
                        else
                        {
                            _gitLatestVersion = link2File.Result("$3");
                        }
                        //else if (Releases != null && Releases.Count > 0)
                        //{
                        //    //first found release usually newest
                        //    info.DownloadLink = "https://" + url + "/" + Releases[0].Sublink;
                        //    GitLatestVersion = Releases[0].FileVersion;
                        //}
                        //return fromfile;
                    }
                    else if (getFromLast10Releases)
                    {
                        _gitLatestVersion = link2File.Result("$2");
                    }
                    //else
                    //{
                    //    GitLatestVersion = link2file.Result("$2");
                    //}

                    //SetContentLength(info.SourceLink, response.ContentLength);

                    return _gitLatestVersion;
                }
                else
                {
                    _log.Info("GitHub sublink to file not found or incorrect.\r\n\tMod:" + Info.TargetFolderPath.Name + "\r\n\tlink:" + Info.SourceLink + "\r\n\tfile:" + Info.UpdateFileStartsWith + "*" + Info.UpdateFileEndsWith + " =>(Link to file" + (link2File.Success ? ": " + link2File.Value : " not found") + ")");

                    if (!Info.VersionFromFile && _gitLatestVersion.Length > 0)
                    {
                        Info.DownloadLink = "";
                        return _gitLatestVersion;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warn("failed to check update. error:\r\n" + ex);
                Info.LastErrorText.Append(" >" + ex.Message);
            }

            return "";
        }

        internal override bool CheckIfStopWork(UpdateInfo info)
        {
            var s = info.LastErrorText.ToString();
            var b = s.Contains("(429) too many requests");
            if (b) _log.Warn("Source " + this.Title + " will be skipped for a while because stop approve requests. Error:\r\n" + s);

            return b;
        }

        internal override void Pause()
        {
            Thread.Sleep((int)(new Random().NextDouble() * 500));
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
            return WC.DownloadData(link);
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
