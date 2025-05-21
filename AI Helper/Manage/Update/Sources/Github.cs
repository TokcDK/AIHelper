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
        // Constructor: initialize Github update source
        public Github(UpdateInfo info) : base(info)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; // enable TLS 1.2 for github
            _log.Info("Initialized Github update source for: " + (info?.TargetFolderPath?.Name ?? "unknown"));
        }

        // Source URL
        internal override string Url { get => "github.com"; }

        // Source identifier
        internal override string InfoId => "updgit";

        // Source title
        internal override string Title => "Github";

        // Main method to download file
        internal async override Task<bool> GetFile()
        {
            _log.Info("Requested file download for: " + (Info?.TargetFolderPath?.Name ?? "unknown"));
            try
            {
                // Start file download
                var result = await DownloadTheFile().ConfigureAwait(true);
                _log.Info("File download result: " + result);
                return result;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error while downloading file for: " + (Info?.TargetFolderPath?.Name ?? "unknown"));
                throw;
            }
        }

        // Download progress event handler
        protected override void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            base.DownloadProgressChanged(sender, e);

            // Update progress bar
            if (e.ProgressPercentage <= _dwnpb.Maximum)
            {
                _dwnpb.Value = e.ProgressPercentage;
            }
            _log.Debug($"Download progress: {e.ProgressPercentage}% for {Info?.UpdateFilePath}");
        }

        // Download completed event handler
        protected override void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            base.DownloadFileCompleted(sender, e);

            // Release progress bar and form resources
            _dwnpb?.Dispose();
            _dwnf?.Dispose();

            // Log download result
            if (e.Error != null)
            {
                _log.Error(e.Error, "Error on file download completion: " + (Info?.UpdateFilePath ?? "unknown"));
            }
            else if (e.Cancelled)
            {
                _log.Warn("File download cancelled: " + (Info?.UpdateFilePath ?? "unknown"));
            }
            else
            {
                _log.Info("File download completed: " + (Info?.UpdateFilePath ?? "unknown"));
            }
        }

        // Static variables for progress form
        static Form _dwnf;
        static ProgressBar _dwnpb;

        // Download file with progress UI
        private async Task<bool> DownloadTheFile()
        {
            // Get update downloads directory
            var updateDownloadsDir = ManageSettings.ModsUpdateDirDownloadsPath;
            Directory.CreateDirectory(updateDownloadsDir);

            // Determine update file name
            var updateFileName = Info.UpdateFilePath.Length > 0 ? Path.GetFileName(Info.UpdateFilePath) : Path.GetFileName(Info.DownloadLink);

            if (string.IsNullOrWhiteSpace(updateFileName))
            {
                // Search for update file name by pattern
                updateFileName = SearchUpdateFilePath(updateDownloadsDir, Info);
                _log.Debug("Searching for update file name: " + updateFileName);
            }
            else
            {
                Info.UpdateFilePath = Path.Combine(updateDownloadsDir, updateFileName);
            }

            // Check alternative update file path
            string altUpdateFilePath = Path.Combine(ManageSettings.Install2MoDirPath, updateFileName);
            if (Info.VersionFromFile && !File.Exists(Info.UpdateFilePath) && File.Exists(altUpdateFilePath))
            {
                Info.UpdateFilePath = altUpdateFilePath;
                _log.Info("Using alternative update file path: " + altUpdateFilePath);
            }

            // Create and show progress form
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

            // Check if file needs to be downloaded
            if (!File.Exists(Info.UpdateFilePath)
                || (!Info.VersionFromFile && File.Exists(Info.UpdateFilePath) && !updateFileName.Contains(Info.TargetLastVersion))
                || new FileInfo(Info.UpdateFilePath).Length == 0
                )
            {
                // If download link is empty
                if (string.IsNullOrWhiteSpace(Info.DownloadLink))
                {
                    Info.NoRemoteFile = true;
                    _log.Warn("Download link is empty. File will not be downloaded.");
                    _dwnpb.Dispose();
                    _dwnf.Dispose();
                    return false;
                }

                // Start file download
                _log.Info("Starting file download: " + Info.DownloadLink);
                await DownloadFileTaskAsync(new Uri(Info.DownloadLink), Info.UpdateFilePath).ConfigureAwait(true);

                // Check if download completed successfully
                var completed = IsCompletedDownload && File.Exists(Info.UpdateFilePath) && new FileInfo(Info.UpdateFilePath).Length != 0;
                if (!completed)
                {
                    _log.Warn("File download not completed or file is corrupted: " + Info.UpdateFilePath);
                }
                else
                {
                    _log.Info("File successfully downloaded: " + Info.UpdateFilePath);
                }
                return completed;
            }
            else
            {
                // If file already exists
                _dwnpb.Dispose();
                _dwnf.Dispose();

                if (new FileInfo(Info.UpdateFilePath).Length == 0)
                {
                    _log.Warn("File with zero length found and will be deleted: " + Info.UpdateFilePath);
                    File.Delete(Info.UpdateFilePath);
                    return false;
                }

                _log.Info("File already exists and does not require downloading: " + Info.UpdateFilePath);
                return true;
            }
        }

        // Search for update file by pattern
        private string SearchUpdateFilePath(string updateDownloadsDir, UpdateInfo info)
        {
            foreach (var targetDir in new[] { updateDownloadsDir, ManageSettings.Install2MoDirPath })
            {
                foreach (var prefix in new[] { "", "v" })
                {
                    foreach (var ending in new[] { "", ".0" })
                    {
                        var possibleName = Info.UpdateFileStartsWith + prefix + Info.TargetLastVersion + ending + Info.UpdateFileEndsWith;
                        if (!File.Exists(Path.Combine(targetDir, possibleName))) continue;

                        info.UpdateFilePath = Path.Combine(targetDir, possibleName);
                        _log.Debug("Found update file: " + info.UpdateFilePath);
                        return possibleName;
                    }
                }
            }

            _log.Debug("Update file not found by pattern.");
            return "";
        }

        // Get latest version from Github
        internal override string GetLastVersion()
        {
            _log.Info("Requested latest version for: " + (Info?.TargetFolderPath?.Name ?? "unknown"));
            var version = GetLatestGithubVersionFromReleases();
            _log.Info("Latest version: " + version);
            return version;
        }

        // Internal variables for Github info
        string _gitOwner;
        string _gitRepository;
        string _gitLatestVersion;

        // Get latest version and file link from Github releases
        private string GetLatestGithubVersionFromReleases()
        {
            try
            {
                // Extract owner, repository, and file name pattern info
                _gitOwner = Info.TargetFolderUpdateInfo[0];
                _gitRepository = Info.TargetFolderUpdateInfo[1];
                Info.UpdateFileStartsWith = Info.TargetFolderUpdateInfo[2];
                if (Info.UpdateFileEndsWith.Length == 0)
                    Info.UpdateFileEndsWith = (Info.TargetFolderUpdateInfo.Length > 3 && Info.TargetFolderUpdateInfo[3].ToUpperInvariant() != "TRUE" && Info.TargetFolderUpdateInfo[3].ToUpperInvariant() != "FALSE") ? Info.TargetFolderUpdateInfo[3] : "";
                Info.VersionFromFile = Info.TargetFolderUpdateInfo[Info.TargetFolderUpdateInfo.Length - 1].ToUpperInvariant() == "TRUE";
                Info.SourceLink = $"https://github.com/{_gitOwner}/{_gitRepository}/releases/latest";

                // Download latest release page
                _log.Debug("Getting latest release page: " + Info.SourceLink);
                var latestReleasePage = WC.DownloadString(Info.SourceLink);
                string assetsPagePattern = @"src\=\""(https\:\/\/github\.com\/" + @"([^\/]+)" + @"\/" + @"([^\/]+)" + @"\/releases\/expanded_assets\/([^\""]+))\"""; // 1 = full assets page link, 2 - owner, 3 - repo, 4 - version
                // 1 = full assets page link, 2 - owner name (can be changed and be different from old), 3 - repository name (can be changed), 4 - version
                var assetPageMatch = Regex.Match(latestReleasePage, assetsPagePattern, RegexOptions.IgnoreCase);
                if (assetPageMatch.Success)
                {
                    // Check and update owner and repository names if changed
                    var currentOwnerName = assetPageMatch.Result("$2");
                    if (currentOwnerName != _gitOwner)
                    {
                        Info.TargetFolderUpdateInfo[0] = _gitOwner = currentOwnerName;
                        _log.Warn("Repository owner name changed to: " + _gitOwner);
                    }
                    var currentRepName = assetPageMatch.Result("$3");
                    if (currentRepName != _gitRepository)
                    {
                        Info.TargetFolderUpdateInfo[1] = _gitRepository = currentRepName;
                        _log.Warn("Repository name changed to: " + _gitRepository);
                    }

                    // Set latest version and reload assets page
                    if (assetPageMatch.Success) _gitLatestVersion = assetPageMatch.Result("$4");
                    _log.Debug("Navigating to release assets page: " + assetPageMatch.Result("$1"));
                    latestReleasePage = WC.DownloadString(assetPageMatch.Result("$1"));
                }
                else
                {
                    // Alternative way to get version
                    var version = Regex.Match(latestReleasePage, @"/releases/tag/([^\""]+)\""", RegexOptions.IgnoreCase);
                    if (version.Success) _gitLatestVersion = version.Result("$1");
                }

                // Search for update file link
                var linkPattern = @"href\=\""(/" + _gitOwner + "/" + _gitRepository + "/releases/download/" + _gitLatestVersion + "/" + Info.UpdateFileStartsWith + "([^\"]*)" + Info.UpdateFileEndsWith + ")\"";
                var link2File = Regex.Match(latestReleasePage, linkPattern, RegexOptions.IgnoreCase);
                if (!link2File.Success && Info.VersionFromFile)
                {
                    // Search for file locally if not found on Github
                    Directory.CreateDirectory(ManageSettings.Install2MoDirPath);

                    foreach (var file in Directory.GetFiles(ManageSettings.Install2MoDirPath, Info.UpdateFileStartsWith + "*" + Info.UpdateFileEndsWith))
                    {
                        var ver = Regex.Match(Path.GetFileName(file), Info.UpdateFileStartsWith + "(.*)" + Info.UpdateFileEndsWith).Groups[1].Value;
                        UpdateTools.CleanVersion(ref ver);
                        UpdateTools.CleanVersion(ref Info.TargetCurrentVersion);
                        if (ver == _gitLatestVersion || ver.IsNewerOf(Info.TargetCurrentVersion))
                        {
                            Info.UpdateFilePath = file;
                            _log.Info("File found locally with version: " + ver);
                            return ver;
                        }
                    }
                }

                // Retry search for file link if author changed username
                if (!link2File.Success)
                {
                    //when author changed username on git
                    linkPattern = @"href\=\""(/[^/]+/" + _gitRepository + "/releases/download/" + _gitLatestVersion + "/" + Info.UpdateFileStartsWith + @"([^\""]*)" + Info.UpdateFileEndsWith + @")\""";
                    link2File = Regex.Match(latestReleasePage, linkPattern);
                }

                // Search in last 10 releases if file not found
                var getFromLast10Releases = false;
                if (!link2File.Success && Info.VersionFromFile)
                {
                    // search in releases assets
                    getFromLast10Releases = true;
                    Info.SourceLink = "https://github.com/" + _gitOwner + "/" + _gitRepository + "/releases";
                    _log.Debug("Searching in last 10 releases: " + Info.SourceLink);
                    latestReleasePage = WC.DownloadString(Info.SourceLink);
                    MatchCollection assetsList = Regex.Matches(latestReleasePage, assetsPagePattern);
                    foreach (Match assetsMatch in assetsList)
                    {
                        string theReleaseAssetsPage = WC.DownloadString(assetsMatch.Result("$1"));

                        linkPattern = @"href\=\""(/" + _gitOwner + "/" + _gitRepository + "/releases/download/([^/]+)/" + Info.UpdateFileStartsWith + @"([^\""]+)" + Info.UpdateFileEndsWith + @")\""";
                        link2File = Regex.Match(theReleaseAssetsPage, linkPattern, RegexOptions.IgnoreCase);

                        if (!link2File.Success) continue;

                        latestReleasePage = theReleaseAssetsPage;
                        break;
                    }

                    // Retry search for file link if author changed username
                    if (!link2File.Success)
                    {
                        //when author changed username on git
                        linkPattern = @"href\=\""(/[^/]+/" + _gitRepository + "/releases/download/([^/]+)/" + Info.UpdateFileStartsWith + @"([^\""]+)" + Info.UpdateFileEndsWith + @")\""";
                        link2File = Regex.Match(latestReleasePage, linkPattern, RegexOptions.IgnoreCase);
                    }

                    // Check if download is needed if version is already up to date
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
                            _log.Info("Current version is newer, download is not required.");
                            return _gitLatestVersion;
                        }
                    }
                }

                // Form download link for file
                if ((link2File.Success && link2File.Value.Length > 7 && link2File.Value.StartsWith("href=", StringComparison.InvariantCultureIgnoreCase)))
                {
                    Info.DownloadLink = "https://" + Url + link2File.Result("$1");
                    _log.Info("Download link for file: " + Info.DownloadLink);

                    // Get version from file name if needed
                    if (Info.VersionFromFile)
                    {
                        if (!getFromLast10Releases)
                        {
                            var pattern = "/releases/download/" + _gitLatestVersion + "/" + Info.UpdateFileStartsWith + "([^\"]+)" + Info.UpdateFileEndsWith + "\"";
                            var fromfile = Regex.Match(latestReleasePage, pattern, RegexOptions.IgnoreCase);
                            if (fromfile.Success)
                            {
                                _gitLatestVersion = fromfile.Result("$1");
                                _log.Debug("Version from file: " + _gitLatestVersion);
                            }
                        }
                        else
                        {
                            _gitLatestVersion = link2File.Result("$3");
                            _log.Debug("Version from last releases: " + _gitLatestVersion);
                        }
                    }
                    else if (getFromLast10Releases)
                    {
                        _gitLatestVersion = link2File.Result("$2");
                        _log.Debug("Version from last releases (without file): " + _gitLatestVersion);
                    }

                    return _gitLatestVersion;
                }
                else
                {
                    // Log if file link not found
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
                // Handle errors when getting release info
                _log.Warn("Failed to check update. Error:\r\n" + ex);
                Info.LastErrorText.Append(" >" + ex.Message);
            }

            return "";
        }

        // Check if source should stop working (e.g. due to rate limiting)
        internal override bool CheckIfStopWork(UpdateInfo info)
        {
            var s = info.LastErrorText.ToString();
            var b = s.Contains("(429) too many requests");
            if (b) _log.Warn("Source " + this.Title + " will be skipped for a while because stop approve requests. Error:\r\n" + s);

            return b;
        }

        // Pause between Github requests
        internal override void Pause()
        {
            int ms = (int)(new Random().NextDouble() * 500);
            _log.Debug("Pause between requests to Github: " + ms + " ms");
            Thread.Sleep(ms);
        }

        // Download file from link
        internal override byte[] DownloadFileFromTheLink(Uri link)
        {
            _log.Info("Downloading file from link: " + link);
            return WC.DownloadData(link);
        }
    }
}
