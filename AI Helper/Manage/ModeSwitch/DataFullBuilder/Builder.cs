using AIHelper.Manage.ModeSwitch.DataFullBuilder.interfaces;
using AIHelper.Manage.ModeSwitch.DataFullBuilder.Interfaces;
using AIHelper.Manage.ModeSwitch.DataFullBuilder.Models;
using AIHelper.Manage.ModeSwitch.DataFullBuilder.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage.ModeSwitch.DataFullBuilder
{
    internal class Builder
    {
        protected static Logger _log = LogManager.GetCurrentClassLogger();

        internal static void Build(string[] args)
        {
            try
            {
                // Parse arguments or use defaults
                var basePath = args.Length > 0 ? args[0] : Environment.CurrentDirectory;

                var configuration = new BuildConfiguration
                {
                    DataFolderPath = System.IO.Path.Combine(basePath, "Data"),
                    ModsFolderPath = System.IO.Path.Combine(basePath, "Mods"),
                    OverwriteFolderPath = System.IO.Path.Combine(basePath, "Overwrite"),
                    LoadOrderFilePath = System.IO.Path.Combine(basePath, "modlist.txt"),
                    OutputFolderPath = System.IO.Path.Combine(basePath, "Data-full"),
                    MaxDegreeOfParallelism = -1 // Use all available cores
                };

                _log.Info("DataFullBuilder - Starting build process...");
                _log.Info($"  Data folder: {configuration.DataFolderPath}");
                _log.Info($"  Mods folder: {configuration.ModsFolderPath}");
                _log.Info($"  Overwrite folder: {configuration.OverwriteFolderPath}");
                _log.Info($"  Output folder: {configuration.OutputFolderPath}");
                _log.Info("");

                var fileSystemHelper = new FileSystemHelper();
                var builder = new Services.DataFullBuilder(fileSystemHelper);

                var result = builder.Build(configuration);

                _log.Info("Build completed!");
                _log.Info($"  Success: {result.Success}");
                _log.Info($"  Total files: {result.TotalFiles}");
                _log.Info($"  Total directories: {result.TotalDirectories}");
                _log.Info($"  Hard links created: {result.HardLinksCreated}");
                _log.Info($"  Symbolic links created: {result.SymbolicLinksCreated}");
                _log.Info($"  Errors: {result.ErrorCount}");
                _log.Info($"  Time elapsed: {result.ElapsedMilliseconds}ms");

                if (result.ErrorCount > 0)
                {
                    _log.Warn($"  Error log: {result.ErrorLogPath}");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Fatal error: {ex.Message}");
                _log.Error(ex.StackTrace);
            }
        }
    }
}
