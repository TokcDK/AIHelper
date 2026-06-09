using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AIHelper.Manage.ModeSwitch.DataFullBuilder.Interfaces;
using AIHelper.Manage.ModeSwitch.DataFullBuilder.Models;

namespace AIHelper.Manage.ModeSwitch.DataFullBuilder.Services
{
    public sealed class VirtualFileSystemBuilder
    {
        private readonly IFileSystemHelper _fileSystemHelper;
        private readonly IErrorLogger _errorLogger;

        public VirtualFileSystemBuilder(
            IFileSystemHelper fileSystemHelper,
            IErrorLogger errorLogger)
        {
            _fileSystemHelper = fileSystemHelper 
                ?? throw new ArgumentNullException(nameof(fileSystemHelper));
            _errorLogger = errorLogger 
                ?? throw new ArgumentNullException(nameof(errorLogger));
        }

        public VirtualFileSystem Build(
            IReadOnlyList<SourceFolder> sourceFolders, 
            int maxDegreeOfParallelism)
        {
            var files = new ConcurrentDictionary<string, FileEntry>(
                StringComparer.OrdinalIgnoreCase);
            var directories = new ConcurrentDictionary<string, int>(
                StringComparer.OrdinalIgnoreCase);

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism > 0 
                    ? maxDegreeOfParallelism 
                    : Environment.ProcessorCount
            };

            // Process sources sequentially by priority, but files in parallel
            foreach (var source in sourceFolders)
            {
                if (!_fileSystemHelper.DirectoryExists(source.Path))
                {
                    _errorLogger.LogError(source.Path, "SourceValidation", 
                        "Source folder does not exist");
                    continue;
                }

                ProcessSource(source, files, directories, parallelOptions);
            }

            return new VirtualFileSystem(files, directories);
        }

        private void ProcessSource(
            SourceFolder source,
            ConcurrentDictionary<string, FileEntry> files,
            ConcurrentDictionary<string, int> directories,
            ParallelOptions parallelOptions)
        {
            var basePath = _fileSystemHelper.NormalizePath(source.Path);

            // Collect directories
            var sourceDirs = new List<string>();
            try
            {
                foreach (var dir in _fileSystemHelper.EnumerateDirectories(basePath, true))
                {
                    try
                    {
                        // Skip invalid symlinks
                        if (_fileSystemHelper.IsSymbolicLink(dir) && 
                            !_fileSystemHelper.IsValidSymbolicLink(dir))
                        {
                            _errorLogger.LogError(dir, "DirectoryScan", 
                                "Invalid symbolic link - skipped");
                            continue;
                        }

                        var relativePath = _fileSystemHelper.GetRelativePath(basePath, dir);
                        sourceDirs.Add(relativePath);
                    }
                    catch (Exception ex)
                    {
                        _errorLogger.LogError(dir, "DirectoryScan", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _errorLogger.LogError(basePath, "DirectoryEnumeration", ex);
            }

            // Add directories (higher priority overwrites)
            foreach (var dir in sourceDirs)
            {
                directories.AddOrUpdate(dir, source.Priority, 
                    (_, existing) => Math.Max(existing, source.Priority));
            }

            // Collect and process files in parallel
            var sourceFiles = new List<string>();
            try
            {
                foreach (var file in _fileSystemHelper.EnumerateFiles(basePath, true))
                {
                    sourceFiles.Add(file);
                }
            }
            catch (Exception ex)
            {
                _errorLogger.LogError(basePath, "FileEnumeration", ex);
                return;
            }

            Parallel.ForEach(sourceFiles, parallelOptions, filePath =>
            {
                try
                {
                    // Skip invalid symlinks
                    if (_fileSystemHelper.IsSymbolicLink(filePath) && 
                        !_fileSystemHelper.IsValidSymbolicLink(filePath))
                    {
                        _errorLogger.LogError(filePath, "FileScan", 
                            "Invalid symbolic link - skipped");
                        return;
                    }

                    // Check if file is readable
                    if (!_fileSystemHelper.CanReadFile(filePath))
                    {
                        _errorLogger.LogError(filePath, "FileScan", 
                            "File is not readable - skipped");
                        return;
                    }

                    var relativePath = _fileSystemHelper.GetRelativePath(basePath, filePath);
                    var entry = new FileEntry(relativePath, filePath, source.Priority);

                    files.AddOrUpdate(relativePath, entry, (_, existing) =>
                    {
                        // Keep higher priority
                        return entry.Priority >= existing.Priority ? entry : existing;
                    });
                }
                catch (Exception ex)
                {
                    _errorLogger.LogError(filePath, "FileProcess", ex);
                }
            });
        }
    }

    public sealed class VirtualFileSystem
    {
        public IReadOnlyDictionary<string, FileEntry> Files { get; }
        public IReadOnlyDictionary<string, int> Directories { get; }

        public VirtualFileSystem(
            ConcurrentDictionary<string, FileEntry> files,
            ConcurrentDictionary<string, int> directories)
        {
            Files = files;
            Directories = directories;
        }
    }
}
