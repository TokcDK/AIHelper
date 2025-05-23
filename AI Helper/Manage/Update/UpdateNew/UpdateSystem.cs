using NLog;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AIHelper.Manage.Update.UpdateNew
{
    // Represents information about an available update
    public interface IUpdateInfo
    {
        string Version { get; }
        byte[] Retrieve();
    }

    // Represents a source of updates (e.g., GitHub, local folder)
    public interface ISource
    {
        string UniqueIdentifier { get; }
        IUpdateInfo GetAvailableUpdate(string currentVersion);
    }

    // Represents an item that can be updated (e.g., Mod Organizer, a mod)
    public interface IUpdateable
    {
        string Name { get; }
        ISource Source { get; }
        string CurrentVersion { get; }
        void InstallUpdate(byte[] updateData);
    }

    // Represents a target that contains updateable items (e.g., Mod Organizer, mod list)
    public interface ITarget
    {
        string Name { get; }
        IEnumerable<IUpdateable> GetUpdateables();
    }

    // Concrete implementation of IUpdateInfo
    public class UpdateInfo : IUpdateInfo
    {
        public string Version { get; }
        private readonly Func<byte[]> _retrieveFunc;

        public UpdateInfo(string version, Func<byte[]> retrieveFunc)
        {
            Version = version ?? throw new ArgumentNullException(nameof(version));
            _retrieveFunc = retrieveFunc ?? throw new ArgumentNullException(nameof(retrieveFunc));
        }

        public byte[] Retrieve() => _retrieveFunc();
    }

// ... (другие классы без изменений)

    // Example source: GitHub repository
    public class GitHubSource : ISource
    {
        public string UniqueIdentifier { get; }
        private readonly string _author;
        private readonly string _repository;
        private readonly string _fileNamePattern;

        public GitHubSource(string id, string author, string repository, string fileNamePattern)
        {
            UniqueIdentifier = id ?? throw new ArgumentNullException(nameof(id));
            _author = author ?? throw new ArgumentNullException(nameof(author));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _fileNamePattern = fileNamePattern ?? throw new ArgumentNullException(nameof(fileNamePattern));
        }

        public IUpdateInfo GetAvailableUpdate(string currentVersion)
        {
            // Используем Octokit для получения последнего релиза и поиска нужного файла
            var latestRelease = GetLatestReleaseAsync().GetAwaiter().GetResult();
            if (latestRelease == null)
                return null;

            var asset = latestRelease.Assets.FirstOrDefault(a => a.Name != null && System.Text.RegularExpressions.Regex.IsMatch(a.Name, _fileNamePattern));
            if (asset == null)
                return null;

            string latestVersion = latestRelease.TagName ?? latestRelease.Name;
            if (string.Compare(latestVersion, currentVersion, StringComparison.Ordinal) > 0)
            {
                return new UpdateInfo(latestVersion, () =>
                {
                    // Скачиваем файл через Octokit
                    var data = DownloadAssetAsync(asset).GetAwaiter().GetResult();
                    return data;
                });
            }
            return null;
        }

        private async Task<Release> GetLatestReleaseAsync()
        {
            var client = new GitHubClient(new ProductHeaderValue("AIHelperUpdater"));
            try
            {
                var releases = await client.Repository.Release.GetAll(_author, _repository);
                return releases?.OrderByDescending(r => r.CreatedAt).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private async Task<byte[]> DownloadAssetAsync(ReleaseAsset asset)
        {
            var client = new GitHubClient(new ProductHeaderValue("AIHelperUpdater"));
            var response = await client.Connection.Get<byte[]>(new Uri(asset.BrowserDownloadUrl), new Dictionary<string, string>(), "application/octet-stream");
            return response.Body;
        }
    }

    // Example source: Local folder
    public class LocalFolderSource : ISource
    {
        public string UniqueIdentifier { get; }
        private readonly string _folderPath;
        private readonly string _fileNamePattern;

        public LocalFolderSource(string id, string folderPath, string fileNamePattern)
        {
            UniqueIdentifier = id ?? throw new ArgumentNullException(nameof(id));
            _folderPath = folderPath ?? throw new ArgumentNullException(nameof(folderPath));
            _fileNamePattern = fileNamePattern ?? throw new ArgumentNullException(nameof(fileNamePattern));
        }

        public IUpdateInfo GetAvailableUpdate(string currentVersion)
        {
            if (!System.IO.Directory.Exists(_folderPath))
                return null;

            var regex = new System.Text.RegularExpressions.Regex(_fileNamePattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var files = System.IO.Directory.GetFiles(_folderPath);
            var versionedFiles = new List<(string FilePath, string Version)>();

            foreach (var file in files)
            {
                var fileName = System.IO.Path.GetFileName(file);
                var match = regex.Match(fileName);
                if (match.Success && match.Groups.Count > 1)
                {
                    var version = match.Groups[1].Value;
                    versionedFiles.Add((file, version));
                }
            }

            if (versionedFiles.Count == 0)
                return null;

            // Найти файл с самой новой версией
            var latest = versionedFiles
                .OrderByDescending(f => f.Version, StringComparer.Ordinal)
                .FirstOrDefault();

            if (string.Compare(latest.Version, currentVersion, StringComparison.Ordinal) > 0)
            {
                return new UpdateInfo(latest.Version, () =>
                {
                    try
                    {
                        return System.IO.File.Exists(latest.FilePath)
                            ? System.IO.File.ReadAllBytes(latest.FilePath)
                            : null;
                    }
                    catch
                    {
                        return null;
                    }
                });
            }
            return null;
        }
    }

    // Updateable item: Mod Organizer
    public class ModOrganizerUpdateable : IUpdateable
    {
        static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public string Name => "Mod Organizer";
        public ISource Source { get; }
        public string CurrentVersion { get; }

        public ModOrganizerUpdateable(ISource source, string currentVersion)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            CurrentVersion = currentVersion ?? throw new ArgumentNullException(nameof(currentVersion));
        }

        public void InstallUpdate(byte[] updateData)
        {
            // Placeholder: Save updateData to temp file and run as .exe installer
            // Example: File.WriteAllBytes("temp.exe", updateData); Process.Start("temp.exe");
            // Mod Organizer dir path: ManageSettings.AppModOrganizerDirPath
            // Arguments: /dir="ManageSettings.AppModOrganizerDirPath" /noicons /nocancel /norestart /silent
            Logger.Info($"Installing update for {Name} with data length {updateData.Length}");
            try
            {
                File.WriteAllBytes("mo-installer.exe", updateData);
                Process.Start("mo-installer.exe", $"/dir=\"{ManageSettings.AppModOrganizerDirPath}\" /noicons /nocancel /norestart /silent");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to install update for Mod Organizer");
                throw;
            }
            finally
            {
                if (File.Exists("mo-installer.exe"))
                    File.Delete("mo-installer.exe");
            }
        }
    }

    // Updateable item: Individual mod
    public class ModUpdateable : IUpdateable
    {
        static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public string Name { get; }
        public ISource Source { get; }
        public string CurrentVersion { get; }
        private readonly string _modFolderPath;

        public ModUpdateable(string name, ISource source, string currentVersion, string modFolderPath)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            CurrentVersion = currentVersion ?? throw new ArgumentNullException(nameof(currentVersion));
            _modFolderPath = modFolderPath ?? throw new ArgumentNullException(nameof(modFolderPath));
        }

        public void InstallUpdate(byte[] updateData)
        {
            // Placeholder: Apply update based on rules (e.g., extract archive to _modFolderPath)
            Logger.Info($"Installing update for mod {Name} at {_modFolderPath}");
        }
    }

    // Target: Mod Organizer
    public class ModOrganizerTarget : ITarget
    {
        public string Name => "Mod Organizer";
        private readonly IUpdateable _updateable;

        public ModOrganizerTarget(IUpdateable updateable)
        {
            _updateable = updateable ?? throw new ArgumentNullException(nameof(updateable));
        }

        public IEnumerable<IUpdateable> GetUpdateables() => new[] { _updateable };
    }

    // Target: Mod list
    public class ModListTarget : ITarget
    {
        public string Name => "Mod List";
        private readonly string _modsDirPath;

        public ModListTarget(string modsDirPath)
        {
            _modsDirPath = modsDirPath ?? throw new ArgumentNullException(nameof(modsDirPath));
        }

        public IEnumerable<IUpdateable> GetUpdateables()
        {
            // Placeholder: Iterate over mod folders, read meta.ini for source data
            // Simulated example:
            var mods = new List<IUpdateable>
            {
                new ModUpdateable("Mod1", new GitHubSource("gh1", "author", "repo", "mod1-*.zip"), "1.0.0", $"{_modsDirPath}\\Mod1"),
                new ModUpdateable("Mod2", new LocalFolderSource("lf1", "C:\\Updates", "mod2-*.zip"), "1.0.0", $"{_modsDirPath}\\Mod2")
            };
            return mods;
        }
    }

    // Main executor class
    public class UpdateExecutor
    {
        public void UpdateAll(IEnumerable<ITarget> targets)
        {
            if (targets == null) throw new ArgumentNullException(nameof(targets));
            foreach (var target in targets)
            {
                foreach (var updateable in target.GetUpdateables())
                {
                    ProcessUpdate(updateable);
                }
            }
        }

        public void UpdateTarget(ITarget target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            UpdateAll(new[] { target });
        }

        public void UpdateUpdateable(IUpdateable updateable)
        {
            if (updateable == null) throw new ArgumentNullException(nameof(updateable));
            ProcessUpdate(updateable);
        }

        private void ProcessUpdate(IUpdateable updateable)
        {
            var source = updateable.Source;
            var currentVersion = updateable.CurrentVersion;
            var updateInfo = source.GetAvailableUpdate(currentVersion);
            if (updateInfo != null)
            {
                var updateData = updateInfo.Retrieve();
                updateable.InstallUpdate(updateData);
            }
        }
    }
}