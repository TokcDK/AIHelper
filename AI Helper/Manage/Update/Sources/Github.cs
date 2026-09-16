using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIHelper.Manage.Update.Sources
{
    /// <summary>
    /// Update source for GitHub releases.
    /// <para>
    /// The latest version is read from the release page: first from the
    /// "expanded assets" link (which also carries the owner, the repository and
    /// the tag), then from the release tag as a fallback. The download link is
    /// then matched against the file name prefix/suffix of the update record.
    /// </para>
    /// <para>
    /// Every value that comes from an update record is escaped before it is used
    /// in a regular expression, because prefixes such as "[Female]" would
    /// otherwise be interpreted as a character class.
    /// </para>
    /// </summary>
    class Github : UpdateSourceBase
    {
        static Github()
        {
            // GitHub no longer accepts older TLS versions
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public Github(UpdateInfo info) : base(info)
        {
        }

        /// <summary>Source url, without the scheme.</summary>
        internal override string Url
        {
            get { return "github.com"; }
        }

        /// <summary>Source identifier.</summary>
        internal override string InfoId
        {
            get { return "updgit"; }
        }

        /// <summary>Source title.</summary>
        internal override string Title
        {
            get { return "Github"; }
        }

        /// <summary>
        /// Downloads the update file of the current target. Never throws: a
        /// failure is logged, recorded and reported through the return value so
        /// one broken mod cannot abort the whole update pass.
        /// </summary>
        /// <returns>True when a usable file is available on disk.</returns>
        internal override async Task<bool> GetFile()
        {
            _log.Info("Update: requested a file download for: " + GetTargetName());

            try
            {
                var result = await DownloadTheFile().ConfigureAwait(true);
                _log.Info("Update: file download result: " + result);
                return result;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Update: error while downloading the file of: " + GetTargetName());
                Info.AppendError(ex.Message);
                DisposeDownloadUi();
                return false;
            }
        }

        protected override void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            base.DownloadProgressChanged(sender, e);

            // raised on a thread pool thread, so the UI update must be marshalled
            SetDownloadProgress(e.ProgressPercentage);

            _log.Debug("Update: download progress " + e.ProgressPercentage + "% for " + Info.UpdateFilePath);
        }

        protected override void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            base.DownloadFileCompleted(sender, e);

            // release the progress window and the progress bar
            DisposeDownloadUi();

            if (e.Error != null)
            {
                _log.Error(e.Error, "Update: error on file download completion: " + Info.UpdateFilePath);
            }
            else if (e.Cancelled)
            {
                _log.Warn("Update: file download cancelled: " + Info.UpdateFilePath);
            }
            else
            {
                _log.Info("Update: file download completed: " + Info.UpdateFilePath);
            }
        }

        #region Download UI

        // the updater shows one download at a time, so the controls are static
        private static Form _downloadForm;
        private static ProgressBar _downloadProgressBar;

        private static void CreateAndShowDownloadUi(string fileName)
        {
            _downloadProgressBar = new ProgressBar
            {
                Dock = DockStyle.Bottom,
                Maximum = 100
            };

            _downloadForm = new Form
            {
                StartPosition = FormStartPosition.CenterScreen,
                TopMost = true,
                Size = new Size(400, 50),
                Text = T._("Downloading") + ": " + fileName,
                FormBorderStyle = FormBorderStyle.FixedToolWindow
            };

            _downloadForm.Controls.Add(_downloadProgressBar);
            _downloadForm.Show();

            _log.Debug("Update: download UI shown. Title: " + _downloadForm.Text);
        }

        private static void DisposeDownloadUi()
        {
            var progressBar = _downloadProgressBar;
            _downloadProgressBar = null;

            if (progressBar != null)
            {
                try
                {
                    progressBar.Dispose();
                }
                catch (Exception ex)
                {
                    _log.Debug("Update: failed to dispose the download progress bar. error:" + Environment.NewLine + ex);
                }
            }

            var form = _downloadForm;
            _downloadForm = null;

            if (form != null)
            {
                try
                {
                    form.Dispose();
                }
                catch (Exception ex)
                {
                    _log.Debug("Update: failed to dispose the download window. error:" + Environment.NewLine + ex);
                }
            }
        }

        /// <summary>
        /// Sets the progress bar value, marshalling to the UI thread when the
        /// call comes from the WebClient callback thread.
        /// </summary>
        private static void SetDownloadProgress(int percentage)
        {
            var progressBar = _downloadProgressBar;
            if (progressBar == null || progressBar.IsDisposed) return;

            if (progressBar.Minimum > percentage) percentage = progressBar.Minimum;
            if (progressBar.Maximum < percentage) percentage = progressBar.Maximum;

            if (!progressBar.InvokeRequired)
            {
                progressBar.Value = percentage;
                return;
            }

            try
            {
                progressBar.BeginInvoke((Action)(() =>
                {
                    var bar = _downloadProgressBar;
                    if (bar == null || bar.IsDisposed) return;

                    bar.Value = percentage;
                }));
            }
            catch (Exception ex)
            {
                _log.Debug("Update: failed to marshal the download progress. error:" + Environment.NewLine + ex);
            }
        }

        #endregion

        /// <summary>
        /// Resolves the update file name, decides whether it must be downloaded
        /// and downloads it when needed.
        /// </summary>
        /// <returns>True when a usable file is available on disk.</returns>
        private async Task<bool> DownloadTheFile()
        {
            if (_downloadForm != null)
            {
                _log.Warn("Update: a download UI is already active. Aborting the new download request.");
                return false; // prevent multiple download UIs
            }

            var updateDownloadsDir = ManageSettings.ModsUpdateDirDownloadsPath;
            Directory.CreateDirectory(updateDownloadsDir);
            _log.Debug("Update: ensured the update downloads directory exists: " + updateDownloadsDir);

            var updateFileName = ResolveUpdateFileName(updateDownloadsDir);

            CreateAndShowDownloadUi(updateFileName);

            try
            {
                var shouldDownload = ShouldDownload(updateFileName, Info.UpdateFilePath, out var downloadReason);

                _log.Debug("Update: download decision: shouldDownload=" + shouldDownload
                           + ", reason=" + downloadReason
                           + ", updateFilePath=" + Info.UpdateFilePath);

                if (!shouldDownload)
                {
                    // the file is already downloaded, only drop it when it is empty
                    if (UpdateIo.GetFileLengthOrZero(Info.UpdateFilePath) == 0)
                    {
                        _log.Warn("Update: a zero length update file was found and will be deleted: " + Info.UpdateFilePath);
                        UpdateIo.TryDeleteFile(Info.UpdateFilePath);
                        return false;
                    }

                    _log.Info("Update: the file already exists and does not require downloading: " + Info.UpdateFilePath);
                    return true;
                }

                if (string.IsNullOrWhiteSpace(Info.DownloadLink))
                {
                    Info.NoRemoteFile = true;
                    _log.Warn("Update: the download link is empty. The file will not be downloaded.");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(Info.UpdateFilePath))
                {
                    // never hand a directory path to WebClient
                    Info.NoRemoteFile = true;
                    _log.Warn("Update: the local update file path is empty. The file will not be downloaded.");
                    return false;
                }

                _log.Info("Update: starting the file download: " + Info.DownloadLink);

                var completed = await DownloadFileTaskAsync(new Uri(Info.DownloadLink), Info.UpdateFilePath).ConfigureAwait(true);

                if (completed)
                {
                    _log.Info("Update: file successfully downloaded: " + Info.UpdateFilePath);
                }
                else
                {
                    _log.Warn("Update: the file download did not complete or the file is corrupted: " + Info.UpdateFilePath);
                }

                return completed;
            }
            finally
            {
                // the window must be released on every path, including a failed download
                DisposeDownloadUi();
            }
        }

        /// <summary>
        /// Resolves the name of the update file and the local path it is stored
        /// at, taking a manually downloaded file into account.
        /// </summary>
        /// <param name="updateDownloadsDir">Directory the file is downloaded to.</param>
        /// <returns>The file name, or an empty string when it cannot be resolved.</returns>
        private string ResolveUpdateFileName(string updateDownloadsDir)
        {
            var updateFileName = !string.IsNullOrWhiteSpace(Info.UpdateFilePath)
                ? Path.GetFileName(Info.UpdateFilePath)
                : Path.GetFileName(Info.DownloadLink);

            if (string.IsNullOrWhiteSpace(updateFileName))
            {
                // look for a file that was downloaded manually earlier
                updateFileName = FindExistingUpdateFile(Info);
                _log.Debug("Update: searched for an existing update file, found: '" + updateFileName + "'");
            }
            else
            {
                Info.UpdateFilePath = Path.Combine(updateDownloadsDir, updateFileName);
                _log.Debug("Update: resolved the update file path: " + Info.UpdateFilePath);
            }

            // check the alternative update file path
            var altUpdateFilePath = Path.Combine(ManageSettings.Install2MoDirPath, updateFileName);

            if (Info.VersionFromFile
                && !File.Exists(Info.UpdateFilePath)
                && File.Exists(altUpdateFilePath))
            {
                Info.UpdateFilePath = altUpdateFilePath;
                _log.Info("Update: using the alternative update file path: " + altUpdateFilePath);
            }

            return updateFileName;
        }

        /// <summary>
        /// Decides whether the update file must be downloaded. The checks are
        /// ordered from the cheapest to the most expensive and stop at the first
        /// reason to download.
        /// </summary>
        /// <param name="fileName">Resolved file name.</param>
        /// <param name="updateFilePath">Local path of the file.</param>
        /// <param name="reason">Reason the decision was taken, for logging.</param>
        /// <returns>True when the file must be downloaded.</returns>
        private bool ShouldDownload(string fileName, string updateFilePath, out string reason)
        {
            if (!File.Exists(updateFilePath))
            {
                reason = "file does not exist";
                return true;
            }

            if (!Info.VersionFromFile && !fileName.Contains(Info.TargetLastVersion))
            {
                reason = "existing filename does not contain target version: " + Info.TargetLastVersion;
                return true;
            }

            // the file may be removed by another process between the checks
            if (UpdateIo.GetFileLengthOrZero(updateFilePath) == 0)
            {
                reason = "existing file has zero length";
                return true;
            }

            reason = "existing file is valid";
            return false;
        }

        /// <summary>
        /// Searches the download directories for a file matching the update
        /// record, so a manually downloaded archive is reused.
        /// </summary>
        /// <param name="info">Current update state.</param>
        /// <returns>The file name, or an empty string when nothing was found.</returns>
        private string FindExistingUpdateFile(UpdateInfo info)
        {
            foreach (var targetDir in new[] { ManageSettings.ModsUpdateDirDownloadsPath, ManageSettings.Install2MoDirPath })
            {
                foreach (var prefix in _updateFilePrefixes)
                {
                    foreach (var suffix in _updateFileSuffixes)
                    {
                        var candidateUpdateFileName =
                            info.UpdateFileStartsWith + prefix + info.TargetLastVersion + suffix + info.UpdateFileEndsWith;

                        if (!File.Exists(Path.Combine(targetDir, candidateUpdateFileName))) continue;

                        info.UpdateFilePath = Path.Combine(targetDir, candidateUpdateFileName);
                        _log.Debug("Update: found the update file: " + info.UpdateFilePath);
                        return candidateUpdateFileName;
                    }
                }
            }

            _log.Debug("Update: no update file found by pattern.");
            return string.Empty;
        }

        /// <summary>
        /// Name prefixes tried when searching for an already downloaded file.
        /// </summary>
        static readonly string[] _updateFilePrefixes =
        {
            "",
            "v"
        };

        /// <summary>
        /// Name suffixes tried when searching for an already downloaded file.
        /// </summary>
        static readonly string[] _updateFileSuffixes =
        {
            "",
            ".0"
        };

        /// <summary>
        /// Gets the latest version of the current target from GitHub releases.
        /// </summary>
        /// <returns>Version string, or empty when it could not be resolved.</returns>
        internal override string GetLastVersion()
        {
            _log.Info("Update: requested the latest version for: " + GetTargetName());

            var version = GetLatestGithubVersionFromReleases();

            _log.Info("Update: latest version: " + version);
            return version;
        }

        #region Github info

        private string _gitOwner;
        private string _gitRepository;
        private string _gitLatestVersion;

        /// <summary>
        /// Regex pattern to extract the assets page info from a GitHub release page.
        /// Groups: 1 = full assets page link, 2 = owner, 3 = repo, 4 = version.
        /// </summary>
        private const string AssetsPageRegexPattern =
            @"src\=\""(https\:\/\/github\.com\/([^\/]+)\/([^\/]+)\/releases\/expanded_assets\/([^\""]+))\""";

        /// <summary>Regex pattern to extract the version from a release tag.</summary>
        private const string VersionTagRegexPattern = @"/releases/tag/([^\""]+)\""";

        private const string GitHubBaseUrl = "https://github.com";
        private const string TrueUpperCase = "TRUE";
        private const string FalseUpperCase = "FALSE";
        private const string HrefPrefix = "href=";

        /// <summary>
        /// Smallest accepted download link match: 'href=' + two quotes + at
        /// least one character, so an empty 'href=""' is rejected.
        /// </summary>
        private const int MinValidLinkLength = 7;

        /// <summary>How many releases the fallback search walks through.</summary>
        private const int MaxReleasesToSearch = 10;

        /// <summary>Upper bound of the random pause between GitHub requests.</summary>
        private const int MaxPauseMilliseconds = 500;

        private static readonly Random _pauseRandom = new Random();
        private static readonly object _pauseRandomLock = new object();

        #endregion

        /// <summary>
        /// Resolves the latest version and the download link from the release
        /// pages of the current target.
        /// </summary>
        /// <returns>Version string, or empty when it could not be resolved.</returns>
        private string GetLatestGithubVersionFromReleases()
        {
            try
            {
                // extract owner, repository and file name pattern info
                InitializeGitHubRepositoryInfo();

                _log.Debug("Update: getting the latest release page: " + Info.SourceLink);
                var releasePage = WebClient.DownloadString(Info.SourceLink);

                // parse the assets page for the version and the owner/repository info
                var assetsPageFound = TryParseAndLoadAssetsPage(
                    ref releasePage,
                    out string currentOwnerName,
                    out string currentRepositoryName);

                if (assetsPageFound)
                {
                    ApplyRenamedRepository(currentOwnerName, currentRepositoryName);
                }
                else
                {
                    // alternative way to get the version
                    TryExtractVersionFromReleaseTag(releasePage);
                }

                // search for the update file link
                var updateFileMatch = SearchForUpdateFileLink(releasePage);

                // search for the file locally when it is not on GitHub
                if (!updateFileMatch.Success && Info.VersionFromFile)
                {
                    var localFileVersion = TryFindUpdateFileLocally();
                    if (localFileVersion != null) return localFileVersion;
                }

                // retry the search for the file link if the author changed username
                if (!updateFileMatch.Success)
                {
                    updateFileMatch = SearchForFileLinkWithFlexibleOwner(releasePage);
                }

                // search in the last releases when the file is still not found
                var searchedInMultipleReleases = false;
                if (!updateFileMatch.Success && Info.VersionFromFile)
                {
                    var searchResult = SearchInLast10Releases();
                    updateFileMatch = searchResult.Match;
                    releasePage = searchResult.PageContent;
                    searchedInMultipleReleases = searchResult.Found;

                    // nothing to download when the installed version is already newer
                    if (updateFileMatch.Success && IsCurrentVersionNewerThanRelease(updateFileMatch))
                    {
                        Info.DownloadLink = "";
                        _log.Info("Update: the current version is newer, the download is not required.");
                        return _gitLatestVersion;
                    }
                }

                return BuildDownloadLinkAndExtractVersion(updateFileMatch, releasePage, searchedInMultipleReleases);
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Update: failed to check for an update.");
                Info.AppendError(ex.Message);
            }

            return "";
        }

        #region GetLatestGithubVersionFromReleases helper methods

        /// <summary>
        /// Initializes the GitHub repository information from the update record
        /// of the current folder.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the update record does not describe a GitHub repository.
        /// </exception>
        private void InitializeGitHubRepositoryInfo()
        {
            var updateRecord = Info.TargetFolderUpdateInfo;

            if (updateRecord == null || updateRecord.Length < 3)
            {
                _log.Error("Update: invalid TargetFolderUpdateInfo configuration.");
                throw new InvalidOperationException("Invalid GitHub repository configuration");
            }

            _gitOwner = updateRecord[0];
            _gitRepository = updateRecord[1];
            Info.UpdateFileStartsWith = updateRecord[2];

            // set UpdateFileEndsWith if not already set and the value is not a boolean flag
            if (Info.UpdateFileEndsWith.Length == 0)
            {
                var hasAdditionalParam = updateRecord.Length > 3;

                if (hasAdditionalParam)
                {
                    var paramValue = (updateRecord[3] ?? string.Empty).ToUpperInvariant();
                    var isNotBooleanFlag = paramValue != TrueUpperCase && paramValue != FalseUpperCase;
                    Info.UpdateFileEndsWith = isNotBooleanFlag ? updateRecord[3] : "";
                }
                else
                {
                    Info.UpdateFileEndsWith = "";
                }
            }

            var lastElementIndex = updateRecord.Length - 1;
            Info.VersionFromFile = (updateRecord[lastElementIndex] ?? string.Empty).ToUpperInvariant() == TrueUpperCase;
            Info.SourceLink = GitHubBaseUrl + "/" + _gitOwner + "/" + _gitRepository + "/releases/latest";

            _log.Debug("Update: initialized GitHub info - Owner: " + _gitOwner
                       + ", Repository: " + _gitRepository
                       + ", FileStartsWith: '" + Info.UpdateFileStartsWith
                       + "', FileEndsWith: '" + Info.UpdateFileEndsWith
                       + "', VersionFromFile: " + Info.VersionFromFile);
        }

        /// <summary>
        /// Keeps the update record of the current folder in sync when the
        /// repository was renamed on GitHub.
        /// </summary>
        /// <param name="currentOwnerName">Owner name read from the release page.</param>
        /// <param name="currentRepositoryName">Repository name read from the release page.</param>
        private void ApplyRenamedRepository(string currentOwnerName, string currentRepositoryName)
        {
            if (!string.IsNullOrWhiteSpace(currentOwnerName) && currentOwnerName != _gitOwner)
            {
                _gitOwner = currentOwnerName;
                ApplyUpdateRecordChange(0, currentOwnerName);
                _log.Warn("Update: the repository owner name changed to: " + _gitOwner);
            }

            if (!string.IsNullOrWhiteSpace(currentRepositoryName) && currentRepositoryName != _gitRepository)
            {
                _gitRepository = currentRepositoryName;
                ApplyUpdateRecordChange(1, currentRepositoryName);
                _log.Warn("Update: the repository name changed to: " + _gitRepository);
            }
        }

        private void ApplyUpdateRecordChange(int index, string newValue)
        {
            var updateRecord = Info.TargetFolderUpdateInfo;

            if (updateRecord == null || index < 0 || index >= updateRecord.Length) return;

            updateRecord[index] = newValue;
        }

        /// <summary>
        /// Tries to parse the assets page out of the release page and loads its
        /// content. Also reports the owner and repository names found on the
        /// page, which may differ from the configured ones.
        /// </summary>
        /// <param name="releasePage">Release page content, replaced with the assets page content on success.</param>
        /// <param name="ownerName">Owner name read from the assets link.</param>
        /// <param name="repositoryName">Repository name read from the assets link.</param>
        /// <returns>True when the assets page was parsed and loaded.</returns>
        private bool TryParseAndLoadAssetsPage(ref string releasePage, out string ownerName, out string repositoryName)
        {
            ownerName = _gitOwner;
            repositoryName = _gitRepository;

            // 1 = full assets page link, 2 = owner name (can be changed and be different from the old),
            // 3 = repository name (can be changed), 4 = version
            var assetPageMatch = Regex.Match(releasePage, AssetsPageRegexPattern, RegexOptions.IgnoreCase);

            if (!assetPageMatch.Success)
            {
                _log.Debug("Update: the assets page pattern was not found in the release page.");
                return false;
            }

            ownerName = assetPageMatch.Result("$2");
            repositoryName = assetPageMatch.Result("$3");

            _gitLatestVersion = assetPageMatch.Result("$4");
            _log.Debug("Update: extracted the version from the assets page: " + _gitLatestVersion);

            var assetsPageUrl = assetPageMatch.Result("$1");
            _log.Debug("Update: navigating to the release assets page: " + assetsPageUrl);
            releasePage = WebClient.DownloadString(assetsPageUrl);

            return true;
        }

        /// <summary>
        /// Alternative way to extract the version from the release tag pattern.
        /// </summary>
        /// <param name="releasePage">Release page content.</param>
        private void TryExtractVersionFromReleaseTag(string releasePage)
        {
            var versionMatch = Regex.Match(releasePage, VersionTagRegexPattern, RegexOptions.IgnoreCase);

            if (versionMatch.Success)
            {
                _gitLatestVersion = versionMatch.Result("$1");
                _log.Debug("Update: extracted the version from the release tag: " + _gitLatestVersion);
            }
            else
            {
                _log.Debug("Update: could not extract the version from the release tag pattern.");
            }
        }

        /// <summary>
        /// Searches the release page for the download link of the update file.
        /// </summary>
        /// <param name="releasePage">Release page content.</param>
        /// <returns>Match for the file link.</returns>
        private Match SearchForUpdateFileLink(string releasePage)
        {
            var linkPattern = BuildFileLinkPattern(_gitOwner, _gitRepository, _gitLatestVersion);

            _log.Debug("Update: searching for the update file: " + Info.UpdateFileStartsWith + "*" + Info.UpdateFileEndsWith);
            return Regex.Match(releasePage, linkPattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Searches the release page for the file link with a flexible owner
        /// pattern, for the case when the author changed their username.
        /// </summary>
        /// <param name="releasePage">Release page content.</param>
        /// <returns>Match for the file link.</returns>
        private Match SearchForFileLinkWithFlexibleOwner(string releasePage)
        {
            _log.Debug("Update: retrying the file search with the flexible owner pattern (the author may have changed username).");

            var linkPattern = @"href\=\""(/[^/]+/" + Regex.Escape(_gitRepository)
                              + "/releases/download/" + Regex.Escape(_gitLatestVersion) + "/"
                              + Regex.Escape(Info.UpdateFileStartsWith) + @"([^\""]*)"
                              + Regex.Escape(Info.UpdateFileEndsWith) + @")\""";

            return Regex.Match(releasePage, linkPattern);
        }

        /// <summary>
        /// Builds the regex pattern that finds the download link of the update
        /// file. Every value that comes from the update record or from the page
        /// is escaped, because a prefix such as "[Female]" would otherwise be
        /// treated as a character class.
        /// </summary>
        /// <param name="owner">Repository owner.</param>
        /// <param name="repository">Repository name.</param>
        /// <param name="version">Release version.</param>
        /// <returns>Regex pattern string.</returns>
        private string BuildFileLinkPattern(string owner, string repository, string version)
        {
            return @"href\=\""(/" + Regex.Escape(owner) + "/" + Regex.Escape(repository)
                   + "/releases/download/" + Regex.Escape(version) + "/"
                   + Regex.Escape(Info.UpdateFileStartsWith) + @"([^\""]*)"
                   + Regex.Escape(Info.UpdateFileEndsWith) + @")\""";
        }

        /// <summary>
        /// Tries to find the update file in the local "install to MO" directory.
        /// </summary>
        /// <returns>Version string when a usable file was found, null otherwise.</returns>
        private string TryFindUpdateFileLocally()
        {
            _log.Debug("Update: searching for the update file locally in: " + ManageSettings.Install2MoDirPath);
            Directory.CreateDirectory(ManageSettings.Install2MoDirPath);

            var searchPattern = Info.UpdateFileStartsWith + "*" + Info.UpdateFileEndsWith;
            var localFiles = Directory.GetFiles(ManageSettings.Install2MoDirPath, searchPattern);

            _log.Debug("Update: found " + localFiles.Length + " potential local files matching the pattern: " + searchPattern);

            // clean into a local, the shared state must not be modified from here
            var installedVersion = UpdateTools.CleanVersion(Info.TargetCurrentVersion);

            foreach (var file in localFiles)
            {
                var fileName = Path.GetFileName(file);

                var versionPattern = Regex.Escape(Info.UpdateFileStartsWith) + "(.*)" + Regex.Escape(Info.UpdateFileEndsWith);
                var versionMatch = Regex.Match(fileName, versionPattern);

                if (!versionMatch.Success) continue;

                var extractedVersion = UpdateTools.CleanVersion(versionMatch.Groups[1].Value);

                if (extractedVersion == _gitLatestVersion || extractedVersion.IsNewerOf(installedVersion))
                {
                    Info.UpdateFilePath = file;
                    _log.Info("Update: file found locally with version: " + extractedVersion);
                    return extractedVersion;
                }

                _log.Debug("Update: the local file '" + fileName + "' version '" + extractedVersion
                           + "' is not newer than the current one.");
            }

            _log.Debug("Update: no suitable update file found locally.");
            return null;
        }

        /// <summary>
        /// Result of the fallback search through the last releases.
        /// </summary>
        private readonly struct ReleasesSearchResult
        {
            internal ReleasesSearchResult(Match match, string pageContent, bool found)
            {
                Match = match;
                PageContent = pageContent;
                Found = found;
            }

            /// <summary>Match for the file link.</summary>
            internal Match Match { get; }

            /// <summary>Page the match was found on.</summary>
            internal string PageContent { get; }

            /// <summary>True when the file link was actually found.</summary>
            internal bool Found { get; }
        }

        /// <summary>
        /// Searches the assets pages of the last releases for the update file.
        /// </summary>
        /// <returns>The match, the page it was found on and whether it was found.</returns>
        private ReleasesSearchResult SearchInLast10Releases()
        {
            var releasesListUrl = GitHubBaseUrl + "/" + _gitOwner + "/" + _gitRepository + "/releases";
            _log.Debug("Update: searching the last " + MaxReleasesToSearch + " releases: " + releasesListUrl);

            var releasesListPage = WebClient.DownloadString(releasesListUrl);
            var assetsList = Regex.Matches(releasesListPage, AssetsPageRegexPattern);

            _log.Debug("Update: found " + assetsList.Count + " release assets pages to search through.");

            var linkPattern = @"href\=\""(/" + Regex.Escape(_gitOwner) + "/" + Regex.Escape(_gitRepository)
                              + "/releases/download/([^/]+)/" + Regex.Escape(Info.UpdateFileStartsWith)
                              + @"([^\""]+)" + Regex.Escape(Info.UpdateFileEndsWith) + @")\""";

            var searchedCount = 0;

            foreach (Match assetsMatch in assetsList)
            {
                // the page can contain more links than the wanted number of releases
                if (searchedCount >= MaxReleasesToSearch) break;
                searchedCount++;

                var assetsPageUrl = assetsMatch.Result("$1");
                var releaseAssetsPage = WebClient.DownloadString(assetsPageUrl);

                var fileMatch = Regex.Match(releaseAssetsPage, linkPattern, RegexOptions.IgnoreCase);

                if (!fileMatch.Success) continue;

                _log.Debug("Update: found the update file in the release: " + assetsMatch.Result("$4"));
                return new ReleasesSearchResult(fileMatch, releaseAssetsPage, true);
            }

            // retry the search with a flexible owner pattern, the author may have changed username
            _log.Debug("Update: retrying the search with the flexible owner pattern in the releases list.");

            var flexibleLinkPattern = @"href\=\""(/[^/]+/" + Regex.Escape(_gitRepository)
                                      + "/releases/download/([^/]+)/" + Regex.Escape(Info.UpdateFileStartsWith)
                                      + @"([^\""]+)" + Regex.Escape(Info.UpdateFileEndsWith) + @")\""";

            var flexibleMatch = Regex.Match(releasesListPage, flexibleLinkPattern, RegexOptions.IgnoreCase);

            if (flexibleMatch.Success)
            {
                _log.Debug("Update: found the file with the flexible owner pattern.");
            }
            else
            {
                _log.Debug("Update: the update file was not found in any of the last " + MaxReleasesToSearch + " releases.");
            }

            return new ReleasesSearchResult(flexibleMatch, releasesListPage, flexibleMatch.Success);
        }

        /// <summary>
        /// Checks whether the installed version is already newer than the
        /// version found in the last releases.
        /// </summary>
        /// <param name="fileMatch">Match containing the release version.</param>
        /// <returns>True when the download can be skipped.</returns>
        private bool IsCurrentVersionNewerThanRelease(Match fileMatch)
        {
            // clean into locals, the shared state must not be modified from here
            var latestVersion = UpdateTools.CleanVersion(_gitLatestVersion);
            var currentVersion = UpdateTools.CleanVersion(Info.TargetCurrentVersion);
            var releaseVersion = UpdateTools.CleanVersion(fileMatch.Result("$2"));

            // return empty when the current version and the release version are newer than the last version from releases
            var isCurrentNewer = currentVersion.IsNewerOf(releaseVersion);
            var isLatestNewer = latestVersion.IsNewerOf(releaseVersion);

            _log.Debug("Update: version comparison - Current: " + currentVersion
                       + ", Release: " + releaseVersion
                       + ", Latest: " + latestVersion
                       + ", CurrentNewer: " + isCurrentNewer
                       + ", LatestNewer: " + isLatestNewer);

            return isCurrentNewer && isLatestNewer;
        }

        /// <summary>
        /// Builds the final download link and extracts the version information.
        /// </summary>
        /// <param name="fileMatch">Match for the file link.</param>
        /// <param name="releasePage">Release page content.</param>
        /// <param name="fromMultipleReleases">Whether the search walked through several releases.</param>
        /// <returns>Version string, or empty when it could not be resolved.</returns>
        private string BuildDownloadLinkAndExtractVersion(Match fileMatch, string releasePage, bool fromMultipleReleases)
        {
            var isValidLink = fileMatch.Success
                              && fileMatch.Value.Length > MinValidLinkLength
                              && fileMatch.Value.StartsWith(HrefPrefix, StringComparison.InvariantCultureIgnoreCase);

            if (isValidLink)
            {
                Info.DownloadLink = "https://" + Url + fileMatch.Result("$1");
                _log.Info("Update: download link for the file: " + Info.DownloadLink);

                // get the version from the file name if needed
                ExtractVersionFromFileName(fileMatch, releasePage, fromMultipleReleases);

                return _gitLatestVersion;
            }

            LogFileLinkNotFound(fileMatch);

            if (!Info.VersionFromFile && _gitLatestVersion.Length > 0)
            {
                Info.DownloadLink = "";
                _log.Debug("Update: returning the version without a download link: " + _gitLatestVersion);
                return _gitLatestVersion;
            }

            return "";
        }

        /// <summary>
        /// Extracts the version from the file name when the update record asks
        /// for it.
        /// </summary>
        /// <param name="fileMatch">Match for the file link.</param>
        /// <param name="releasePage">Release page content.</param>
        /// <param name="fromMultipleReleases">Whether the search walked through several releases.</param>
        private void ExtractVersionFromFileName(Match fileMatch, string releasePage, bool fromMultipleReleases)
        {
            if (Info.VersionFromFile)
            {
                if (!fromMultipleReleases)
                {
                    var pattern = "/releases/download/" + Regex.Escape(_gitLatestVersion) + "/"
                                  + Regex.Escape(Info.UpdateFileStartsWith) + "([^\"]+)"
                                  + Regex.Escape(Info.UpdateFileEndsWith) + "\"";

                    var versionFromFile = Regex.Match(releasePage, pattern, RegexOptions.IgnoreCase);

                    if (versionFromFile.Success)
                    {
                        _gitLatestVersion = versionFromFile.Result("$1");
                        _log.Debug("Update: version from the file: " + _gitLatestVersion);
                    }
                }
                else
                {
                    _gitLatestVersion = fileMatch.Result("$3");
                    _log.Debug("Update: version from the last releases: " + _gitLatestVersion);
                }
            }
            else if (fromMultipleReleases)
            {
                _gitLatestVersion = fileMatch.Result("$2");
                _log.Debug("Update: version from the last releases (without file): " + _gitLatestVersion);
            }
        }

        /// <summary>
        /// Logs the details of a file link that was not found.
        /// </summary>
        /// <param name="fileMatch">Match for the file link.</param>
        private void LogFileLinkNotFound(Match fileMatch)
        {
            var linkInfo = fileMatch.Success ? ": " + fileMatch.Value : " not found";

            _log.Info("GitHub sublink to file not found or incorrect.\r\n\t"
                      + "Mod:" + GetTargetName() + "\r\n\t"
                      + "link:" + Info.SourceLink + "\r\n\t"
                      + "file:" + Info.UpdateFileStartsWith + "*" + Info.UpdateFileEndsWith
                      + " =>(Link to file" + linkInfo + ")");
        }

        #endregion

        /// <summary>
        /// True when the source hit a GitHub rate limit and must be skipped for
        /// the rest of the session.
        /// </summary>
        /// <param name="info">Current update state.</param>
        internal override bool CheckIfStopWork(UpdateInfo info)
        {
            var lastError = info.LastError;

            var shouldStop = lastError.IndexOf("(429)", StringComparison.OrdinalIgnoreCase) >= 0
                             || lastError.IndexOf("too many requests", StringComparison.OrdinalIgnoreCase) >= 0;

            if (shouldStop)
            {
                _log.Warn("Source " + Title + " will be skipped for a while because it stopped approving requests. Error:\r\n" + lastError);
            }

            return shouldStop;
        }

        /// <summary>
        /// Waits a random short time before the next request so the updater does
        /// not look like a scraper. Awaitable, so the UI thread is not blocked.
        /// </summary>
        internal override async Task PauseAsync()
        {
            var milliseconds = NextPauseMilliseconds();

            _log.Debug("Update: pause between requests to Github: " + milliseconds + " ms");

            await Task.Delay(milliseconds).ConfigureAwait(true);
        }

        /// <summary>
        /// Reads a file into memory.
        /// </summary>
        /// <param name="link">File to read.</param>
        /// <returns>Payload bytes, or an empty array on failure.</returns>
        internal override byte[] DownloadFileFromTheLink(Uri link)
        {
            _log.Info("Update: downloading a file from the link: " + link);

            try
            {
                return WebClient.DownloadData(link);
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Update: failed to download from the link: " + link);
                return Array.Empty<byte>();
            }
        }

        private static int NextPauseMilliseconds()
        {
            // Random is not thread-safe, and a new instance would repeat the seed
            lock (_pauseRandomLock)
            {
                return _pauseRandom.Next(0, MaxPauseMilliseconds);
            }
        }

        private string GetTargetName()
        {
            return Info == null || Info.TargetFolderPath == null ? "unknown" : Info.TargetFolderPath.Name;
        }
    }
}
