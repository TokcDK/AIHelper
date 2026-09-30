using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ModUpdater.Core.Infrastructure.Install;
using NLog;

namespace AIHelper.Manage.Update.Installers
{
    /// <summary>
    /// Installs Mod Organizer, which is the one target updated by running a
    /// published installer instead of extracting an archive.
    /// <para>
    /// The engine already knows how to run an installer and wait for it. What it
    /// cannot know is which files inside the Mod Organizer installation are not
    /// Mod Organizer's to replace: the python plugins this application adds for the
    /// supported games, and the localisation and configuration files the user is
    /// meant to keep. An installer ships none of them and replaces the folders they
    /// live in, so they are kept aside beforehand and put back afterwards.
    /// </para>
    /// <para>
    /// The target is recognised by its install path rather than by the install kind,
    /// because that is the only thing that tells this target apart from any future
    /// target that also runs an installer.
    /// </para>
    /// </summary>
    internal sealed class ModOrganizerInstaller : IInstaller
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Files of the installation that must survive an update, relative to the
        /// Mod Organizer folder. Mod Organizer does not ship any of them: the ini
        /// file and the category list are created on first run, and the explorer
        /// localisation is added by the repack this application is built around.
        /// </summary>
        private static readonly string[] FilesToKeep =
        {
            "ModOrganizer.ini",
            "categories.dat",
            "explorer++/Explorer++RU.dll",
            "explorer++/config.xml",
        };

        private readonly IInstaller _inner;
        private readonly string _modOrganizerInstallPath;

        internal ModOrganizerInstaller(IInstaller inner, string modOrganizerInstallPath)
        {
            _inner = inner;
            _modOrganizerInstallPath = modOrganizerInstallPath ?? string.Empty;
        }

        public bool CanInstall(InstallContext context)
        {
            return IsModOrganizer(context) && _inner != null && _inner.CanInstall(context);
        }

        public async Task<InstallResult> InstallAsync(InstallContext context, CancellationToken ct)
        {
            var keptFiles = ReadFilesToKeep();

            ManageModOrganizer.PreserveModOrganizerBasicGamePluginFiles();

            try
            {
                var result = await _inner.InstallAsync(context, ct).ConfigureAwait(false);

                // Only after a successful install: the installer put its own version
                // of everything in place, and whatever it did not provide is what has
                // to come back.
                if (result.Success) RestoreMissingFiles(keptFiles);

                return result;
            }
            finally
            {
                // Runs on failure too. The python plugins are put back whenever they
                // are missing, and an installer that failed halfway may well have
                // removed them; it also clears the folder they were kept in, which
                // would otherwise be left behind.
                ManageModOrganizer.RestoreMissingModOrganizerBasicGamePluginFiles();
            }
        }

        /// <summary>
        /// True for the Mod Organizer target. The engine hands the installer the
        /// target's install path and nothing else that identifies it.
        /// </summary>
        private bool IsModOrganizer(InstallContext context)
        {
            if (context == null || _modOrganizerInstallPath.Length == 0) return false;

            return string.Equals(
                context.TargetInstallPath, _modOrganizerInstallPath, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Reads the files that must survive, keyed by their relative path. A file
        /// that is not there is simply not kept, so a partially installed folder
        /// does not produce a restore attempt for something that never existed.
        /// </summary>
        private Dictionary<string, string> ReadFilesToKeep()
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var relativePath in FilesToKeep)
            {
                var filePath = Path.Combine(_modOrganizerInstallPath, relativePath);

                try
                {
                    if (File.Exists(filePath)) result[relativePath] = File.ReadAllText(filePath);
                }
                catch (Exception ex)
                {
                    _log.Warn(ex, "Update: failed to read '" + filePath + "' before updating Mod Organizer.");
                }
            }

            return result;
        }

        /// <summary>
        /// Writes back the kept files the installer did not provide itself.
        /// </summary>
        private void RestoreMissingFiles(Dictionary<string, string> keptFiles)
        {
            foreach (var pair in keptFiles)
            {
                var filePath = Path.Combine(_modOrganizerInstallPath, pair.Key);

                try
                {
                    if (File.Exists(filePath)) continue;

                    var directoryPath = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(directoryPath)) Directory.CreateDirectory(directoryPath);

                    File.WriteAllText(filePath, pair.Value);

                    _log.Info("Update: restored '" + filePath + "' after updating Mod Organizer.");
                }
                catch (Exception ex)
                {
                    _log.Warn(ex, "Update: failed to restore '" + filePath + "' after updating Mod Organizer.");
                }
            }
        }
    }
}
