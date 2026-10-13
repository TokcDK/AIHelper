using System;
using System.Collections.Generic;
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
    /// The targets that are not mods — the Mod Organizer installation and the application
    /// itself — have no meta.ini, so their version is not read or written here. Mod
    /// Organizer's is read from its executable and passed in as the configured version of
    /// the target; the application's is answered by <see cref="ApplicationVersionProvider"/>.
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

        /// <summary>Folders of the pass that are not mods and have no meta.ini.</summary>
        private readonly IReadOnlyList<string> _nonModFolderPaths;

        /// <param name="nonModFolderPaths">
        /// Folders of the pass that are not mods, which are the ones this store leaves alone.
        /// </param>
        internal MetaIniVersionProvider(IReadOnlyCollection<string> nonModFolderPaths)
        {
            _nonModFolderPaths = nonModFolderPaths == null
                ? new List<string>()
                : new List<string>(nonModFolderPaths);
        }

        public Task<string> GetCurrentVersionAsync(UpdateTarget target, AssetSpec asset, CancellationToken ct)
        {
            if (!IsMod(target)) return Task.FromResult<string>(null);

            return Task.FromResult(ReadVersion(target.InstallPath));
        }

        public Task SetCurrentVersionAsync(UpdateTarget target, AssetSpec asset, VersionInfo version, CancellationToken ct)
        {
            if (!IsMod(target)) return Task.CompletedTask;

            WriteVersion(target.InstallPath, version != null ? version.VersionString : null);
            return Task.CompletedTask;
        }

        /// <summary>
        /// True for a target whose version is kept in its own meta.ini.
        /// </summary>
        /// <remarks>
        /// Asked by install path because that is all the engine hands a version store, and
        /// because the path is what makes a target unique.
        /// </remarks>
        private bool IsMod(UpdateTarget target)
        {
            if (target == null) return false;

            foreach (var folderPath in _nonModFolderPaths)
            {
                if (string.Equals(target.InstallPath, folderPath, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
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
