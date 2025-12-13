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
        private static readonly string[] _updateFilePrefixes = new[]
        {
            "",
            "v"
        };
        private readonly string[] _updateFileSuffixes = new[]
        {
            "",
            ".0"
        };
        private string SearchUpdateFilePath(string updateDownloadsDir, UpdateInfo info)
        {
            foreach (var targetDir in new[] { updateDownloadsDir, ManageSettings.Install2MoDirPath })
            {
                foreach (var prefix in _updateFilePrefixes)
                {
                    foreach (var suffix in _updateFileSuffixes)
                    {
                        var candidateUpdateFileName = $"{Info.UpdateFileStartsWith}{prefix}{Info.TargetLastVersion}{suffix}{Info.UpdateFileEndsWith}";
                        if (!File.Exists(Path.Combine(targetDir, candidateUpdateFileName)))
                        {
                            continue;
                        }

                        info.UpdateFilePath = Path.Combine(targetDir, candidateUpdateFileName);
                        _log.Debug("Found update file: {0}", info.UpdateFilePath);
                        return candidateUpdateFileName;
                    }
                }
            }

            _log.Debug("Update file not found by pattern.");
            return string.Empty;
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
        #region GetLatestGithubVersionFromReleases Private Constants

        /// <summary>
        /// Regex pattern to extract assets page info from GitHub release page.
        /// Groups: 1 = full assets page link, 2 = owner, 3 = repo, 4 = version
        /// </summary>
        private const string AssetsPageRegexPattern =
            @"src\=\""(https\:\/\/github\.com\/([^\/]+)\/([^\/]+)\/releases\/expanded_assets\/([^\""]+))\""";

        /// <summary>
        /// Regex pattern to extract version from release tag.
        /// </summary>
        private const string VersionTagRegexPattern = @"/releases/tag/([^\""]+)\""";

        private const string GitHubBaseUrl = "https://github.com";
        private const string TrueUpperCase = "TRUE";
        private const string FalseUpperCase = "FALSE";
        private const int MinValidLinkLength = 7;
        private const string HrefPrefix = "href=";

        #endregion

        private string GetLatestGithubVersionFromReleases()
        {
            try
            {
                // Extract owner, repository, and file name pattern info
                InitializeGitHubRepositoryInfo();

                // Download latest release page
                _log.Debug("Getting latest release page: " + Info.SourceLink);
                var latestReleasePage = WebClient.DownloadString(Info.SourceLink);

                // Parse assets page for version and owner/repo information
                var assetsPageFound = TryParseAndLoadAssetsPage(ref latestReleasePage);
                if (!assetsPageFound)
                {
                    // Alternative way to get version
                    TryExtractVersionFromReleaseTag(latestReleasePage);
                }

                // Search for update file link
                var updateFileMatch = SearchForUpdateFileLink(latestReleasePage);

                // Search for file locally if not found on GitHub
                if (!updateFileMatch.Success && Info.VersionFromFile)
                {
                    var localFileVersion = TryFindUpdateFileLocally();
                    if (localFileVersion != null)
                    {
                        return localFileVersion;
                    }
                }

                // Retry search for file link if author changed username
                if (!updateFileMatch.Success)
                {
                    updateFileMatch = SearchForFileLinkWithFlexibleOwner(latestReleasePage);
                }

                // Search in last 10 releases if file not found
                var searchedInMultipleReleases = false;
                if (!updateFileMatch.Success && Info.VersionFromFile)
                {
                    var searchResult = SearchInLast10Releases();
                    updateFileMatch = searchResult.Match;
                    latestReleasePage = searchResult.PageContent;
                    searchedInMultipleReleases = searchResult.Found;

                    // Check if download is needed if version is already up to date
                    if (updateFileMatch.Success && !Info.VersionFromFile)
                    {
                        if (IsCurrentVersionNewerThanRelease(updateFileMatch))
                        {
                            Info.DownloadLink = "";
                            _log.Info("Current version is newer, download is not required.");
                            return _gitLatestVersion;
                        }
                    }
                }

                // Form download link for file
                return BuildDownloadLinkAndExtractVersion(updateFileMatch, latestReleasePage, searchedInMultipleReleases);
            }
            catch (Exception ex)
            {
                // Handle errors when getting release info
                _log.Warn("Failed to check update. Error:\r\n" + ex);
                Info.LastErrorText.Append(" >" + ex.Message);
            }

            return "";
        }

        #region GetLatestGithubVersionFromReleases Helper Methods

        /// <summary>
        /// Initializes GitHub repository information from the update info configuration array.
        /// </summary>
        private void InitializeGitHubRepositoryInfo()
        {
            _gitOwner = Info.TargetFolderUpdateInfo[0];
            _gitRepository = Info.TargetFolderUpdateInfo[1];
            Info.UpdateFileStartsWith = Info.TargetFolderUpdateInfo[2];

            // Set UpdateFileEndsWith if not already set and value is not a boolean flag
            if (Info.UpdateFileEndsWith.Length == 0)
            {
                var hasAdditionalParam = Info.TargetFolderUpdateInfo.Length > 3;
                if (hasAdditionalParam)
                {
                    var paramValue = Info.TargetFolderUpdateInfo[3].ToUpperInvariant();
                    var isNotBooleanFlag = paramValue != TrueUpperCase && paramValue != FalseUpperCase;
                    Info.UpdateFileEndsWith = isNotBooleanFlag ? Info.TargetFolderUpdateInfo[3] : "";
                }
                else
                {
                    Info.UpdateFileEndsWith = "";
                }
            }

            var lastElementIndex = Info.TargetFolderUpdateInfo.Length - 1;
            Info.VersionFromFile = Info.TargetFolderUpdateInfo[lastElementIndex].ToUpperInvariant() == TrueUpperCase;
            Info.SourceLink = $"{GitHubBaseUrl}/{_gitOwner}/{_gitRepository}/releases/latest";

            _log.Debug($"Initialized GitHub info - Owner: {_gitOwner}, Repository: {_gitRepository}, " +
                       $"FileStartsWith: '{Info.UpdateFileStartsWith}', FileEndsWith: '{Info.UpdateFileEndsWith}', " +
                       $"VersionFromFile: {Info.VersionFromFile}");
        }

        /// <summary>
        /// Tries to parse assets page from release page and load its content.
        /// Also updates owner and repository names if they have changed on GitHub.
        /// </summary>
        /// <param name="releasePage">Release page content. Updated with assets page content if successful.</param>
        /// <returns>True if assets page was successfully parsed and loaded.</returns>
        private bool TryParseAndLoadAssetsPage(ref string releasePage)
        {
            // 1 = full assets page link, 2 - owner name (can be changed and be different from old), 
            // 3 - repository name (can be changed), 4 - version
            var assetPageMatch = Regex.Match(releasePage, AssetsPageRegexPattern, RegexOptions.IgnoreCase);

            if (!assetPageMatch.Success)
            {
                _log.Debug("Assets page pattern not found in release page");
                return false;
            }

            // Check and update owner and repository names if changed
            var currentOwnerName = assetPageMatch.Result("$2");
            if (currentOwnerName != _gitOwner)
            {
                Info.TargetFolderUpdateInfo[0] = _gitOwner = currentOwnerName;
                _log.Warn("Repository owner name changed to: " + _gitOwner);
            }

            var currentRepoName = assetPageMatch.Result("$3");
            if (currentRepoName != _gitRepository)
            {
                Info.TargetFolderUpdateInfo[1] = _gitRepository = currentRepoName;
                _log.Warn("Repository name changed to: " + _gitRepository);
            }

            // Set latest version and reload assets page
            _gitLatestVersion = assetPageMatch.Result("$4");
            _log.Debug("Extracted version from assets page: " + _gitLatestVersion);

            var assetsPageUrl = assetPageMatch.Result("$1");
            _log.Debug("Navigating to release assets page: " + assetsPageUrl);
            releasePage = WebClient.DownloadString(assetsPageUrl);

            return true;
        }

        /// <summary>
        /// Alternative method to extract version from release tag pattern.
        /// </summary>
        /// <param name="releasePage">The release page content.</param>
        private void TryExtractVersionFromReleaseTag(string releasePage)
        {
            var versionMatch = Regex.Match(releasePage, VersionTagRegexPattern, RegexOptions.IgnoreCase);
            if (versionMatch.Success)
            {
                _gitLatestVersion = versionMatch.Result("$1");
                _log.Debug("Extracted version from release tag: " + _gitLatestVersion);
            }
            else
            {
                _log.Debug("Could not extract version from release tag pattern");
            }
        }

        /// <summary>
        /// Searches for the update file download link in the release page.
        /// </summary>
        /// <param name="releasePage">The release page content.</param>
        /// <returns>Match result for the file link.</returns>
        private Match SearchForUpdateFileLink(string releasePage)
        {
            var linkPattern = BuildFileLinkPattern(_gitOwner, _gitRepository, _gitLatestVersion, false);
            _log.Debug($"Searching for update file: {Info.UpdateFileStartsWith}*{Info.UpdateFileEndsWith}");
            return Regex.Match(releasePage, linkPattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Searches for file link with flexible owner pattern (for cases when author changed username).
        /// </summary>
        /// <param name="releasePage">The release page content.</param>
        /// <returns>Match result for the file link.</returns>
        private Match SearchForFileLinkWithFlexibleOwner(string releasePage)
        {
            // when author changed username on git
            _log.Debug("Retrying file search with flexible owner pattern (author may have changed username)");
            var linkPattern = @"href\=\""(/[^/]+/" + _gitRepository + "/releases/download/" +
                              _gitLatestVersion + "/" + Info.UpdateFileStartsWith +
                              @"([^\""]*)" + Info.UpdateFileEndsWith + @")\""";
            return Regex.Match(releasePage, linkPattern);
        }

        /// <summary>
        /// Builds regex pattern for finding file download link.
        /// </summary>
        /// <param name="owner">Repository owner.</param>
        /// <param name="repo">Repository name.</param>
        /// <param name="version">Release version.</param>
        /// <param name="captureVersion">Whether to capture version in the pattern.</param>
        /// <returns>Regex pattern string.</returns>
        private string BuildFileLinkPattern(string owner, string repo, string version, bool captureVersion)
        {
            var versionPart = captureVersion ? "([^/]+)" : version;
            return @"href\=\""(/" + owner + "/" + repo + "/releases/download/" +
                   versionPart + "/" + Info.UpdateFileStartsWith +
                   @"([^\""]*)" + Info.UpdateFileEndsWith + @")\""";
        }

        /// <summary>
        /// Tries to find update file in local directory.
        /// </summary>
        /// <returns>Version string if file found locally, null otherwise.</returns>
        private string TryFindUpdateFileLocally()
        {
            _log.Debug($"Searching for update file locally in: {ManageSettings.Install2MoDirPath}");
            Directory.CreateDirectory(ManageSettings.Install2MoDirPath);

            var searchPattern = Info.UpdateFileStartsWith + "*" + Info.UpdateFileEndsWith;
            var localFiles = Directory.GetFiles(ManageSettings.Install2MoDirPath, searchPattern);

            _log.Debug($"Found {localFiles.Length} potential local files matching pattern: {searchPattern}");

            foreach (var file in localFiles)
            {
                var fileName = Path.GetFileName(file);
                var versionPattern = Info.UpdateFileStartsWith + "(.*)" + Info.UpdateFileEndsWith;
                var versionMatch = Regex.Match(fileName, versionPattern);

                if (!versionMatch.Success)
                {
                    continue;
                }

                var extractedVersion = versionMatch.Groups[1].Value;
                UpdateTools.CleanVersion(ref extractedVersion);
                UpdateTools.CleanVersion(ref Info.TargetCurrentVersion);

                if (extractedVersion == _gitLatestVersion || extractedVersion.IsNewerOf(Info.TargetCurrentVersion))
                {
                    Info.UpdateFilePath = file;
                    _log.Info("File found locally with version: " + extractedVersion);
                    return extractedVersion;
                }

                _log.Debug($"Local file '{fileName}' version '{extractedVersion}' is not newer than current");
            }

            _log.Debug("No suitable update file found locally");
            return null;
        }

        /// <summary>
        /// Result structure for last 10 releases search operation.
        /// </summary>
        private struct ReleasesSearchResult
        {
            public Match Match;
            public string PageContent;
            public bool Found;
        }

        /// <summary>
        /// Searches through last 10 releases for the update file.
        /// </summary>
        /// <returns>Search result containing match, page content, and success flag.</returns>
        private ReleasesSearchResult SearchInLast10Releases()
        {
            var result = new ReleasesSearchResult { Found = true };

            // search in releases assets
            Info.SourceLink = $"{GitHubBaseUrl}/{_gitOwner}/{_gitRepository}/releases";
            _log.Debug("Searching in last 10 releases: " + Info.SourceLink);

            var releasesListPage = WebClient.DownloadString(Info.SourceLink);
            var assetsList = Regex.Matches(releasesListPage, AssetsPageRegexPattern);

            _log.Debug($"Found {assetsList.Count} releases to search through");

            foreach (Match assetsMatch in assetsList)
            {
                var assetsPageUrl = assetsMatch.Result("$1");
                var releaseAssetsPage = WebClient.DownloadString(assetsPageUrl);

                var linkPattern = @"href\=\""(/" + _gitOwner + "/" + _gitRepository +
                                  "/releases/download/([^/]+)/" + Info.UpdateFileStartsWith +
                                  @"([^\""]+)" + Info.UpdateFileEndsWith + @")\""";

                var fileMatch = Regex.Match(releaseAssetsPage, linkPattern, RegexOptions.IgnoreCase);

                if (fileMatch.Success)
                {
                    _log.Debug($"Found update file in release: {assetsMatch.Result("$4")}");
                    result.Match = fileMatch;
                    result.PageContent = releaseAssetsPage;
                    return result;
                }
            }

            // Retry search for file link if author changed username
            _log.Debug("Retrying search with flexible owner pattern in releases list");
            var flexibleLinkPattern = @"href\=\""(/[^/]+/" + _gitRepository +
                                      "/releases/download/([^/]+)/" + Info.UpdateFileStartsWith +
                                      @"([^\""]+)" + Info.UpdateFileEndsWith + @")\""";

            result.Match = Regex.Match(releasesListPage, flexibleLinkPattern, RegexOptions.IgnoreCase);
            result.PageContent = releasesListPage;

            if (result.Match.Success)
            {
                _log.Debug("Found file with flexible owner pattern");
            }
            else
            {
                _log.Debug("Update file not found in any of the last 10 releases");
            }

            return result;
        }

        /// <summary>
        /// Checks if current version is newer than the version from releases.
        /// </summary>
        /// <param name="fileMatch">Match containing release version info.</param>
        /// <returns>True if current version is newer and download can be skipped.</returns>
        private bool IsCurrentVersionNewerThanRelease(Match fileMatch)
        {
            UpdateTools.CleanVersion(ref _gitLatestVersion);
            UpdateTools.CleanVersion(ref Info.TargetCurrentVersion);

            var releaseVersion = fileMatch.Result("$2");
            UpdateTools.CleanVersion(ref releaseVersion);

            // return empty when current version and release version is newer of last version from releases
            var isCurrentNewer = Info.TargetCurrentVersion.IsNewerOf(releaseVersion);
            var isLatestNewer = _gitLatestVersion.IsNewerOf(releaseVersion);

            _log.Debug($"Version comparison - Current: {Info.TargetCurrentVersion}, " +
                       $"Release: {releaseVersion}, Latest: {_gitLatestVersion}, " +
                       $"CurrentNewer: {isCurrentNewer}, LatestNewer: {isLatestNewer}");

            return isCurrentNewer && isLatestNewer;
        }

        /// <summary>
        /// Builds the final download link and extracts version information.
        /// </summary>
        /// <param name="fileMatch">Match for the file link.</param>
        /// <param name="releasePage">The release page content.</param>
        /// <param name="fromMultipleReleases">Whether the search was done across multiple releases.</param>
        /// <returns>Version string or empty if not found.</returns>
        private string BuildDownloadLinkAndExtractVersion(Match fileMatch, string releasePage, bool fromMultipleReleases)
        {
            var isValidLink = fileMatch.Success &&
                              fileMatch.Value.Length > MinValidLinkLength &&
                              fileMatch.Value.StartsWith(HrefPrefix, StringComparison.InvariantCultureIgnoreCase);

            if (isValidLink)
            {
                Info.DownloadLink = "https://" + Url + fileMatch.Result("$1");
                _log.Info("Download link for file: " + Info.DownloadLink);

                // Get version from file name if needed
                ExtractVersionFromFileName(fileMatch, releasePage, fromMultipleReleases);

                return _gitLatestVersion;
            }

            // Log if file link not found
            LogFileLinkNotFound(fileMatch);

            if (!Info.VersionFromFile && _gitLatestVersion.Length > 0)
            {
                Info.DownloadLink = "";
                _log.Debug("Returning version without download link: " + _gitLatestVersion);
                return _gitLatestVersion;
            }

            return "";
        }

        /// <summary>
        /// Extracts version from file name when VersionFromFile is enabled.
        /// </summary>
        /// <param name="fileMatch">Match for the file link.</param>
        /// <param name="releasePage">The release page content.</param>
        /// <param name="fromMultipleReleases">Whether the search was done across multiple releases.</param>
        private void ExtractVersionFromFileName(Match fileMatch, string releasePage, bool fromMultipleReleases)
        {
            if (Info.VersionFromFile)
            {
                if (!fromMultipleReleases)
                {
                    var pattern = "/releases/download/" + _gitLatestVersion + "/" +
                                  Info.UpdateFileStartsWith + "([^\"]+)" + Info.UpdateFileEndsWith + "\"";
                    var versionFromFile = Regex.Match(releasePage, pattern, RegexOptions.IgnoreCase);

                    if (versionFromFile.Success)
                    {
                        _gitLatestVersion = versionFromFile.Result("$1");
                        _log.Debug("Version from file: " + _gitLatestVersion);
                    }
                }
                else
                {
                    _gitLatestVersion = fileMatch.Result("$3");
                    _log.Debug("Version from last releases: " + _gitLatestVersion);
                }
            }
            else if (fromMultipleReleases)
            {
                _gitLatestVersion = fileMatch.Result("$2");
                _log.Debug("Version from last releases (without file): " + _gitLatestVersion);
            }
        }

        /// <summary>
        /// Logs detailed information when file link is not found.
        /// </summary>
        /// <param name="fileMatch">Match result for the file link.</param>
        private void LogFileLinkNotFound(Match fileMatch)
        {
            var linkInfo = fileMatch.Success ? ": " + fileMatch.Value : " not found";

            _log.Info("GitHub sublink to file not found or incorrect.\r\n\t" +
                      "Mod:" + Info.TargetFolderPath.Name + "\r\n\t" +
                      "link:" + Info.SourceLink + "\r\n\t" +
                      "file:" + Info.UpdateFileStartsWith + "*" + Info.UpdateFileEndsWith +
                      " =>(Link to file" + linkInfo + ")");
        }

        #endregion

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
            return WebClient.DownloadData(link);
        }
    }
}
