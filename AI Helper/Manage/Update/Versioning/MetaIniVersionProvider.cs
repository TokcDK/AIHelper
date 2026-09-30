using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ModUpdater.Core.Domain;
using ModUpdater.Core.Infrastructure.Versioning;
using NLog;

namespace AIHelper.Manage.Update.Versioning
{
    /// <summary>
    /// Keeps the installed version of a mod in its own meta.ini.
    /// <para>
    /// The engine ships a provider that writes a JSON manifest into the target
    /// folder. That is wrong for a mod folder: the manifest is a file Mod Organizer
    /// does not know about, so as soon as one mod overwrites another the manifest
    /// shows up in the conflict list the user has to work through. meta.ini is
    /// already there, is already excluded from conflict checking, and is where Mod
    /// Organizer itself keeps the version of a mod.
    /// </para>
    /// <para>
    /// The Mod Organizer installation is not a mod and has no meta.ini, so its
    /// version is not read or written here: it is read from the executable and
    /// passed in as the configured version of the target.
    /// </para>
    /// </summary>
    internal sealed class MetaIniVersionProvider : IInstalledVersionProvider
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>File Mod Organizer keeps its per mod data in.</summary>
        private const string MetaIniFileName = "meta.ini";

        /// <summary>Section of meta.ini that holds the version.</summary>
        private const string VersionSection = "General";

        /// <summary>Key of meta.ini that holds the version.</summary>
        private const string VersionKey = "version";

        private readonly string _modOrganizerInstallPath;

        /// <param name="modOrganizerInstallPath">
        /// Folder of the Mod Organizer installation, used to recognise the one
        /// target that must not be treated as a mod.
        /// </param>
        internal MetaIniVersionProvider(string modOrganizerInstallPath)
        {
            _modOrganizerInstallPath = modOrganizerInstallPath ?? string.Empty;
        }

        public Task<string> GetCurrentVersionAsync(UpdateTarget target, AssetSpec asset, CancellationToken ct)
        {
            if (target == null || IsModOrganizerTarget(target)) return Task.FromResult<string>(null);

            return Task.FromResult(ReadVersion(target.InstallPath));
        }

        public Task SetCurrentVersionAsync(UpdateTarget target, AssetSpec asset, VersionInfo version, CancellationToken ct)
        {
            if (target == null || IsModOrganizerTarget(target)) return Task.CompletedTask;

            WriteVersion(target.InstallPath, version != null ? version.VersionString : null);
            return Task.CompletedTask;
        }

        /// <summary>
        /// True for the Mod Organizer installation, which has no meta.ini.
        /// </summary>
        private bool IsModOrganizerTarget(UpdateTarget target)
        {
            if (_modOrganizerInstallPath.Length == 0) return false;

            return string.Equals(
                target.InstallPath, _modOrganizerInstallPath, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Reads the installed version of a mod, or null when it has none.
        /// A missing version means "unknown", and an unknown version is older than
        /// anything found online, so the mod is offered for update — which is what
        /// an update check should do with a mod it cannot date.
        /// </summary>
        private static string ReadVersion(string modFolderPath)
        {
            if (string.IsNullOrWhiteSpace(modFolderPath)) return null;

            var metaIniPath = Path.Combine(modFolderPath, MetaIniFileName);
            if (!File.Exists(metaIniPath)) return null;

            try
            {
                var version = ManageIni.GetIniValueIfExist(metaIniPath, VersionKey, VersionSection);

                return string.IsNullOrWhiteSpace(version) ? null : version;
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Update: failed to read the version of '" + modFolderPath + "'.");
                return null;
            }
        }

        /// <summary>
        /// Writes the installed version of a mod into its meta.ini, leaving the
        /// other keys — the update record among them — untouched.
        /// </summary>
        private static void WriteVersion(string modFolderPath, string version)
        {
            if (string.IsNullOrWhiteSpace(modFolderPath) || string.IsNullOrWhiteSpace(version)) return;

            try
            {
                Directory.CreateDirectory(modFolderPath);

                var metaIniPath = Path.Combine(modFolderPath, MetaIniFileName);

                // Created through the ini helper rather than by the writer below:
                // writing to a missing file makes that helper rebuild the game data
                // first, which is a heavy side effect to trigger from here.
                if (!File.Exists(metaIniPath))
                {
                    ManageIni.GetINIFile(metaIniPath);
                }

                ManageIni.WriteIniValue(metaIniPath, VersionSection, VersionKey, version);
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Update: failed to write the version of '" + modFolderPath + "'.");
            }
        }
    }
}
