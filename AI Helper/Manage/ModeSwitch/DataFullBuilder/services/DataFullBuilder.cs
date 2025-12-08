using AIHelper.Manage.ModeSwitch.DataFullBuilder.interfaces;
using AIHelper.Manage.ModeSwitch.DataFullBuilder.Interfaces;
using AIHelper.Manage.ModeSwitch.DataFullBuilder.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AIHelper.Manage.ModeSwitch.DataFullBuilder.Services
{
    public sealed class DataFullBuilder : IDataFullBuilder
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly string MetaFolderName = ManageSettings.CommonModeMetaDirName;
        private const string ErrorLogFileName = "build-errors.log";
        private readonly string OverwriteFolderName = ManageSettings.MoOverwriteDirName;
        private readonly string DataFolderName = ManageSettings.DataDirName;

        private readonly IFileSystemHelper _fileSystemHelper;

        private int _hardLinksCreated;
        private int _symbolicLinksCreated;

        public DataFullBuilder(
            IFileSystemHelper fileSystemHelper)
        {
            _fileSystemHelper = fileSystemHelper 
                ?? throw new ArgumentNullException(nameof(fileSystemHelper));
        }

        public BuildResult Build(BuildConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _log.Info("Starting build process...");
            var stopwatch = Stopwatch.StartNew();
            var result = new BuildResult();

            // Setup paths
            var outputPath = _fileSystemHelper.NormalizePath(configuration.OutputFolderPath);
            var metaFolderPath = Path.Combine(outputPath, MetaFolderName);
            var errorLogPath = Path.Combine(metaFolderPath, ErrorLogFileName);

            result.ErrorLogPath = errorLogPath;

            // Create output structure
            _fileSystemHelper.CreateDirectory(outputPath);
            _fileSystemHelper.CreateDirectory(metaFolderPath);

            _log.Info("Output structure created. Setting up error logger...");
            using (var errorLogger = new ErrorLogger(errorLogPath))
            {
                try
                {
                    // Get source folders in priority order
                    _log.Info("Retrieving source folders in priority order...");
                    var sourceFolders = GetSourceFoldersInPriorityOrder(configuration, errorLogger);

                    // Build virtual file system
                    _log.Info("Building virtual file system...");
                    var vfsBuilder = new VirtualFileSystemBuilder(_fileSystemHelper, errorLogger);
                    var vfs = vfsBuilder.Build(sourceFolders, configuration.MaxDegreeOfParallelism);

                    result.TotalFiles = vfs.Files.Count;
                    result.TotalDirectories = vfs.Directories.Count;

                    // Create directory structure
                    _log.Info("Creating directory structure...");
                    CreateDirectoryStructure(outputPath, vfs.Directories.Keys, errorLogger);

                    // Create links for files
                    _log.Info("Creating file links...");
                    CreateFileLinks(outputPath, vfs.Files.Values, configuration, errorLogger);

                    result.HardLinksCreated = _hardLinksCreated;
                    result.SymbolicLinksCreated = _symbolicLinksCreated;
                    result.Success = true;
                    _log.Info("Build completed successfully.");
                }
                catch (Exception ex)
                {
                    _log.Info("Build failed with exception.");
                    errorLogger.LogError(outputPath, "Build", ex);
                    result.Success = false;
                }

                errorLogger.Flush();
                result.ErrorCount = ((ErrorLogger)errorLogger).ErrorCount;
            }

            stopwatch.Stop();
            result.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            _log.Info($"Build process finished. Total time: {result.ElapsedMilliseconds} ms.");

            return result;
        }

        private IReadOnlyList<SourceFolder> GetSourceFoldersInPriorityOrder(
            BuildConfiguration configuration,
            IErrorLogger errorLogger)
        {
            var sources = new List<SourceFolder>();
            int priority = 0;

            // 1. Data folder (lowest priority)
            var dataPath = _fileSystemHelper.NormalizePath(configuration.DataFolderPath);
            if (_fileSystemHelper.DirectoryExists(dataPath))
            {
                sources.Add(new SourceFolder(dataPath, priority++, DataFolderName));
            }
            else
            {
                errorLogger.LogError(dataPath, "SourceValidation", 
                    "Data folder does not exist");
            }

            // 2. Mods from loadorder.txt (ascending priority)
            var modOrderProvider = new ModOrderProvider(
                configuration.LoadOrderFilePath,
                configuration.ModsFolderPath,
                _fileSystemHelper);

            var modFolders = modOrderProvider.GetOrderedModFolders();
            foreach (var modName in modFolders)
            {
                var modPath = Path.Combine(
                    _fileSystemHelper.NormalizePath(configuration.ModsFolderPath), 
                    modName);
                sources.Add(new SourceFolder(modPath, priority++, modName));
            }

            // 3. Overwrite folder (highest priority)
            var overwritePath = _fileSystemHelper.NormalizePath(configuration.OverwriteFolderPath);
            if (_fileSystemHelper.DirectoryExists(overwritePath))
            {
                sources.Add(new SourceFolder(overwritePath, priority++, OverwriteFolderName));
            }

            return sources;
        }

        private void CreateDirectoryStructure(
            string outputPath,
            IEnumerable<string> directories,
            IErrorLogger errorLogger)
        {
            // Sort directories to ensure parent directories are created first
            var sortedDirs = directories
                .OrderBy(d => d.Count(c => c == Path.DirectorySeparatorChar || 
                                           c == Path.AltDirectorySeparatorChar))
                .ThenBy(d => d, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var dir in sortedDirs)
            {
                var targetPath = Path.Combine(outputPath, dir);
                
                try
                {
                    if (!_fileSystemHelper.DirectoryExists(targetPath))
                    {
                        _fileSystemHelper.CreateDirectory(targetPath);
                    }
                }
                catch (Exception ex)
                {
                    errorLogger.LogError(dir, "CreateDirectory", ex);
                }
            }
        }

        private void CreateFileLinks(
            string outputPath,
            IEnumerable<FileEntry> files,
            BuildConfiguration configuration,
            IErrorLogger errorLogger)
        {
            var outputDriveRoot = _fileSystemHelper.GetDriveRoot(outputPath);
            
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = configuration.MaxDegreeOfParallelism > 0
                    ? configuration.MaxDegreeOfParallelism
                    : Environment.ProcessorCount
            };

            Parallel.ForEach(files, parallelOptions, file =>
            {
                var targetPath = Path.Combine(outputPath, file.RelativePath);
                
                try
                {
                    // Ensure parent directory exists
                    var parentDir = Path.GetDirectoryName(targetPath);
                    if (!string.IsNullOrEmpty(parentDir) && 
                        !_fileSystemHelper.DirectoryExists(parentDir))
                    {
                        _fileSystemHelper.CreateDirectory(parentDir);
                    }

                    // Determine link type based on drive
                    var sourceDriveRoot = _fileSystemHelper.GetDriveRoot(file.SourceAbsolutePath);
                    var sameDrive = string.Equals(
                        outputDriveRoot, 
                        sourceDriveRoot, 
                        StringComparison.OrdinalIgnoreCase);

                    bool success;
                    if (sameDrive)
                    {
                        success = _fileSystemHelper.CreateHardLink(
                            _fileSystemHelper.GetLongPath(targetPath),
                            _fileSystemHelper.GetLongPath(file.SourceAbsolutePath));
                        
                        if (success)
                        {
                            Interlocked.Increment(ref _hardLinksCreated);
                        }
                    }
                    else
                    {
                        success = _fileSystemHelper.CreateSymbolicLink(
                            _fileSystemHelper.GetLongPath(targetPath),
                            _fileSystemHelper.GetLongPath(file.SourceAbsolutePath),
                            isDirectory: false);
                        
                        if (success)
                        {
                            Interlocked.Increment(ref _symbolicLinksCreated);
                        }
                    }

                    if (!success)
                    {
                        errorLogger.LogError(file.RelativePath, 
                            sameDrive ? "CreateHardLink" : "CreateSymbolicLink",
                            "Link creation failed");
                    }
                }
                catch (Exception ex)
                {
                    errorLogger.LogError(file.RelativePath, "CreateLink", ex);
                }
            });
        }
    }
}
