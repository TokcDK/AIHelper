using System;
using System.Collections.Generic;

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
        // Placeholder: Use GitHub API to check latest release
        // Compare version from tag/file name with currentVersion
        // Example logic (to be implemented with real API calls):
        string latestVersion = "1.2.3"; // Simulated latest version from GitHub
        if (string.Compare(latestVersion, currentVersion, StringComparison.Ordinal) > 0)
        {
            return new UpdateInfo(latestVersion, () =>
            {
                // Simulated download from GitHub release
                return new byte[] { /* file data */ };
            });
        }
        return null;
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
        // Placeholder: Check files in _folderPath matching _fileNamePattern
        // Determine latest version from file name or metadata
        string latestVersion = "1.0.1"; // Simulated latest version
        if (string.Compare(latestVersion, currentVersion, StringComparison.Ordinal) > 0)
        {
            return new UpdateInfo(latestVersion, () =>
            {
                // Simulated file read from local folder
                return new byte[] { /* file data */ };
            });
        }
        return null;
    }
}

// Updateable item: Mod Organizer
public class ModOrganizerUpdateable : IUpdateable
{
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
        Console.WriteLine($"Installing update for {Name} with data length {updateData.Length}");
    }
}

// Updateable item: Individual mod
public class ModUpdateable : IUpdateable
{
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
        Console.WriteLine($"Installing update for mod {Name} at {_modFolderPath}");
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
    private readonly string _modOrganizerPath;

    public ModListTarget(string modOrganizerPath)
    {
        _modOrganizerPath = modOrganizerPath ?? throw new ArgumentNullException(nameof(modOrganizerPath));
    }

    public IEnumerable<IUpdateable> GetUpdateables()
    {
        // Placeholder: Iterate over mod folders, read meta.ini for source data
        // Simulated example:
        var mods = new List<IUpdateable>
        {
            new ModUpdateable("Mod1", new GitHubSource("gh1", "author", "repo", "mod1-*.zip"), "1.0.0", $"{_modOrganizerPath}\\mods\\Mod1"),
            new ModUpdateable("Mod2", new LocalFolderSource("lf1", "C:\\Updates", "mod2-*.zip"), "1.0.0", $"{_modOrganizerPath}\\mods\\Mod2")
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