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
    /// Keeps the versions a user decided to skip in the two places this application
    /// already owns:
    /// <list type="bullet">
    /// <item>the meta.ini of the mod, when the mod has one, next to the update record
    /// the application writes there;</item>
    /// <item>a per game <c>SkipVersions.ini</c> in <c>RES\update\&lt;game&gt;</c>, which
    /// also covers Mod Organizer itself and any folder that has no meta.ini.</item>
    /// </list>
    /// <para>
    /// Both files are read and written, and the effective list is their union. That is
    /// deliberate: meta.ini travels with the mod and is where a user would look for it,
    /// while the game file survives a mod folder being replaced and is the only place a
    /// non-mod target can record a skip. Writing both means a skip is never lost because
    /// one of the two files was rebuilt.
    /// </para>
    /// <para>
    /// The engine's own JSON store is not used here for the same reason the installed
    /// version is not: a file Mod Organizer does not know about shows up in its conflict
    /// list. See <see cref="MetaIniVersionProvider"/>.
    /// </para>
    /// </summary>
    internal sealed class SkippedVersionStore : ISkippedVersionStore
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>File this application keeps the per game skip list in.</summary>
        private const string GameFileName = "SkipVersions.ini";

        /// <summary>Key the versions of one target are stored under in the game file.</summary>
        private const string GameFileVersionsKey = "versions";

        /// <summary>File Mod Organizer keeps its per mod data in.</summary>
        private const string MetaIniFileName = "meta.ini";

        /// <summary>Key the versions of a mod are stored under in its meta.ini.</summary>
        private const string MetaIniKey = "SkipVersions";

        /// <summary>
        /// Absolute path of the game level skip file, or null when the caller had no
        /// path to build it from. A null path turns the game file half into a no-op
        /// instead of throwing from a background update pass.
        /// </summary>
        private readonly string _gameFilePath;

        /// <param name="gameUpdateDirPath">
        /// Folder of the current game below <c>RES\update</c>, where the file is created.
        /// </param>
        internal SkippedVersionStore(string gameUpdateDirPath)
        {
            _gameFilePath = string.IsNullOrWhiteSpace(gameUpdateDirPath)
                ? null
                : Path.Combine(gameUpdateDirPath, GameFileName);
        }

        public Task<IReadOnlyList<string>> GetSkippedVersionsAsync(UpdateTarget target, CancellationToken ct)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            var versions = new List<string>();

            versions.AddRange(ReadFromMetaIni(target.InstallPath));
            versions.AddRange(ReadFromGameFile(target.InstallPath));

            return Task.FromResult(SkippedVersions.Normalize(versions));
        }

        public Task SetSkippedVersionsAsync(UpdateTarget target, IReadOnlyList<string> versions, CancellationToken ct)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            var normalized = SkippedVersions.Normalize(versions);

            WriteToMetaIni(target.InstallPath, normalized);
            WriteToGameFile(target.InstallPath, normalized);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Reads the skip list a mod carries in its own meta.ini. A mod without meta.ini
        /// has no per mod list and is covered by the game file alone.
        /// </summary>
        private static IReadOnlyList<string> ReadFromMetaIni(string modFolderPath)
        {
            if (string.IsNullOrWhiteSpace(modFolderPath)) return new List<string>();

            var metaIniPath = Path.Combine(modFolderPath, MetaIniFileName);
            if (!File.Exists(metaIniPath)) return new List<string>();

            try
            {
                // createIfNoIni: false — reading must not create the file it looks for.
                var ini = ManageIni.GetINIFile(metaIniPath, false);

                return ini.KeyExists(MetaIniKey, ManageSettings.AiMetaIniSectionName)
                    ? SkippedVersions.Split(ini.GetKey(ManageSettings.AiMetaIniSectionName, MetaIniKey))
                    : new List<string>();
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Update: failed to read the skipped versions of '" + modFolderPath + "'.");
                return new List<string>();
            }
        }

        /// <summary>
        /// Writes the skip list into a mod's meta.ini. A meta.ini that is not there is
        /// left alone: creating one would add a file to a mod folder that Mod Organizer
        /// did not expect, and the game file already holds the same list.
        /// </summary>
        private static void WriteToMetaIni(string modFolderPath, IReadOnlyList<string> versions)
        {
            if (string.IsNullOrWhiteSpace(modFolderPath)) return;

            var metaIniPath = Path.Combine(modFolderPath, MetaIniFileName);
            if (!File.Exists(metaIniPath)) return;

            try
            {
                var value = SkippedVersions.Join(versions);
                var ini = ManageIni.GetINIFile(metaIniPath, false);

                if (value.Length == 0)
                    ini.DeleteKey(MetaIniKey, ManageSettings.AiMetaIniSectionName);
                else
                    ini.SetKey(ManageSettings.AiMetaIniSectionName, MetaIniKey, value);
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Update: failed to write the skipped versions of '" + modFolderPath + "'.");
            }
        }

        /// <summary>
        /// Reads the entry of one target from the game file. The target path is the
        /// section name, so a folder that was renamed simply stops matching instead of
        /// leaving a stale list behind for another target.
        /// </summary>
        private IReadOnlyList<string> ReadFromGameFile(string targetPath)
        {
            if (_gameFilePath == null || string.IsNullOrWhiteSpace(targetPath)) return new List<string>();
            if (!File.Exists(_gameFilePath)) return new List<string>();

            try
            {
                var ini = ManageIni.GetINIFile(_gameFilePath, false);
                var section = GameFileSection(targetPath);

                return ini.KeyExists(GameFileVersionsKey, section)
                    ? SkippedVersions.Split(ini.GetKey(section, GameFileVersionsKey))
                    : new List<string>();
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Update: failed to read the skipped versions from '" + _gameFilePath + "'.");
                return new List<string>();
            }
        }

        /// <summary>Writes the entry of one target into the game file, creating the file on first use.</summary>
        private void WriteToGameFile(string targetPath, IReadOnlyList<string> versions)
        {
            if (_gameFilePath == null || string.IsNullOrWhiteSpace(targetPath)) return;

            try
            {
                var ini = ManageIni.GetINIFile(_gameFilePath);
                var section = GameFileSection(targetPath);
                var value = SkippedVersions.Join(versions);

                if (value.Length == 0)
                    ini.DeleteSection(section);
                else
                    ini.SetKey(section, GameFileVersionsKey, value);
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Update: failed to write the skipped versions to '" + _gameFilePath + "'.");
            }
        }

        /// <summary>
        /// Section name of a target in the game file: the target path itself. A Windows
        /// path cannot contain '[' or ']', so the section name is unambiguous, and keeping
        /// the path in the section rather than in a key avoids the comment characters a
        /// folder name is allowed to contain.
        /// </summary>
        private static string GameFileSection(string targetPath)
        {
            return targetPath.Trim();
        }
    }
}
