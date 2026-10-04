using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AIHelper.Forms.Other;
using AIHelper.Manage.ui.themes;
using AIHelper.Manage.Update.Providers;
using ModUpdater.Core.Application;
using ModUpdater.Core.Domain;
using ModUpdater.Core.Infrastructure.Sources;
using ModUpdater.UI;
using NLog;

namespace AIHelper.Manage.Update
{
    /// <summary>
    /// Runs the update pass for the plugins and for Mod Organizer.
    /// <para>
    /// The pass itself is the engine in ModUpdater.Core and the window the user
    /// works in is the one from ModUpdater.UI. What is left here is the part only
    /// this application can do: work out which folders can be updated and where the
    /// new version of each one comes from — the hardcoded Mod Organizer record, the
    /// per game updateinfo.txt and the meta.ini of each mod — and hand that to the
    /// engine.
    /// </para>
    /// <para>
    /// Every folder is checked and installed independently, so one mod that cannot
    /// be reached is reported in its own row and the rest of the pass continues.
    /// </para>
    /// </summary>
    internal sealed class Updater
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly UpdateOptionsDialogForm _updateOptions;

        /// <param name="updateOptions">
        /// Options dialog the pass was started from, or null when it was started
        /// without one.
        /// </param>
        internal Updater(UpdateOptionsDialogForm updateOptions = null)
        {
            _updateOptions = updateOptions;
        }

        /// <summary>
        /// True when files were really replaced during the pass. Callers use it to
        /// decide whether the game data has to be rebuilt afterwards.
        /// </summary>
        internal bool UpdatedAny { get; private set; }

        /// <summary>
        /// Checks Mod Organizer and every mod that carries an update record, and lets
        /// the user install what was found.
        /// </summary>
        internal Task Update()
        {
            return RunAsync(CollectFolders(includeMods: true));
        }

        /// <summary>
        /// The same pass for Mod Organizer alone.
        /// <para>
        /// Used when Mod Organizer is missing and installing it is the whole point,
        /// so the folder it will be installed into does not exist yet.
        /// </para>
        /// </summary>
        internal Task UpdateModOrganizer()
        {
            return RunAsync(CollectFolders(includeMods: false));
        }

        #region Which folders can be updated

        /// <summary>
        /// Gathers the folders that carry an update record.
        /// </summary>
        /// <param name="includeMods">
        /// False to leave the mods alone and only offer Mod Organizer itself.
        /// </param>
        private IReadOnlyList<UpdatableFolder> CollectFolders(bool includeMods)
        {
            // The dialog is the only thing that knows whether switched off mods are
            // wanted. Without it the answer is to leave them alone: a mod the user
            // turned off is a mod the user does not want touched.
            var onlyEnabledMods = _updateOptions == null
                                  || _updateOptions.CheckEnabledModsOnlyCheckBox.Checked;

            var catalog = new UpdateRecordCatalog();

            catalog.Add(new ModOrganizerUpdateRecordProvider());

            // Mods only exist inside Mod Organizer, and their records live in files
            // Mod Organizer writes, so without an installation there is nothing to
            // read and nothing to update.
            if (includeMods && ManageModOrganizer.IsInstalled)
            {
                // updateinfo.txt before meta.ini, so that a mod listed in the file is
                // taken from there and the file stays the place to override a record
                // that was written into a mod by hand.
                catalog.Add(new UpdateInfoFileProvider(onlyEnabledMods));
                catalog.Add(new MetaIniUpdateRecordProvider(onlyEnabledMods));
            }

            return catalog.Collect();
        }

        #endregion

        #region Running the pass

        /// <summary>
        /// Runs the pass and waits for the window to be closed.
        /// <para>
        /// Deliberately synchronous: the window is shown modally, so the caller is
        /// held until the user is done with it, and everything the pass does happens
        /// on the nested message loop the window is running. The returned task exists
        /// so that the callers, which are already asynchronous, can await it like
        /// before.
        /// </para>
        /// </summary>
        private Task RunAsync(IReadOnlyList<UpdatableFolder> folders)
        {
            if (folders.Count == 0)
            {
                _log.Info("Update: no folder carries an update record, there is nothing to check.");
                return Task.CompletedTask;
            }

            using (var engine = new UpdateEngineFactory())
            {
                var manager = engine.CreateManager(BuildTargets(folders, engine.GithubSource));

                // UpdatedAny has to mean "files were really replaced", so it is driven
                // by the event the engine raises only after a successful install, and
                // not by the one it raises for a target it merely finished with.
                manager.TargetUpdated += OnTargetUpdated;

                using (var updaterForm = new UpdaterForm(
                    manager,
                    // Windows the window opens for itself (the skipped version editor) are
                    // themed through the same call, so a dialog created by the engine's UI
                    // layer does not come up with the system colours in a dark theme.
                    control => ThemesLoader.SetTheme(ManageSettings.CurrentTheme, control)))
                {
                    ThemesLoader.SetTheme(ManageSettings.CurrentTheme, updaterForm);

                    // The window is the point of the pass: it lists every folder with
                    // the source it is checked against, and installs what the user
                    // picks. The check is started as soon as the window can pump
                    // messages, so the list is filled without a click.
                    updaterForm.Shown += StartCheckOnShown(manager);

                    updaterForm.ShowDialog();
                }
            }

            return Task.CompletedTask;
        }

        private static EventHandler StartCheckOnShown(IUpdateManager manager)
        {
            return async (sender, args) =>
            {
                try
                {
                    await manager.CheckForUpdatesAsync(CancellationToken.None).ConfigureAwait(true);
                }
                catch (Exception ex)
                {
                    _log.Warn(ex, "Update: the update check failed.");
                }
            };
        }

        private void OnTargetUpdated(object sender, UpdateCandidate candidate)
        {
            UpdatedAny = true;
        }

        #endregion

        #region Targets

        /// <summary>
        /// Describes the folders to the engine.
        /// </summary>
        private static List<UpdateTarget> BuildTargets(
            IReadOnlyList<UpdatableFolder> folders, IUpdateSource source)
        {
            var targets = new List<UpdateTarget>(folders.Count);

            foreach (var folder in folders)
            {
                var isModOrganizer = IsModOrganizerFolder(folder.FolderPath);

                // Mod Organizer is read from its own executable rather than from a
                // record, because that is the only place its version is written.
                var currentVersion = isModOrganizer ? ReadModOrganizerVersion() : null;

                var binding = new SourceBinding(source, folder.Record.ToSourceConfig(currentVersion));

                targets.Add(new UpdateTarget(
                    // The folder path doubles as the id: the engine keys its per
                    // target bookkeeping by it, and the path is what makes a folder
                    // unique — two mods can carry the same name in different games.
                    folder.FolderPath,
                    isModOrganizer ? ModOrganizerDisplayName : Path.GetFileName(folder.FolderPath),
                    folder.FolderPath,
                    new[] { binding },
                    // Mods live inside the Mod Organizer folder, so Mod Organizer is
                    // installed before them and not after.
                    isModOrganizer ? null : ModOrganizerTargetId));
            }

            return targets;
        }

        /// <summary>Name Mod Organizer is shown under in the window.</summary>
        private const string ModOrganizerDisplayName = "Mod Organizer";

        /// <summary>Id of the Mod Organizer target, which is its folder path.</summary>
        private static string ModOrganizerTargetId
        {
            get { return ManageSettings.AppModOrganizerDirPath; }
        }

        private static bool IsModOrganizerFolder(string folderPath)
        {
            return string.Equals(
                folderPath, ManageSettings.AppModOrganizerDirPath, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Version of the installed Mod Organizer, read from its executable. The
        /// engine compares it with the release tag, so it has to be the real version
        /// and not something out of a config file.
        /// </summary>
        /// <returns>The version, or null when Mod Organizer is not installed.</returns>
        private static string ReadModOrganizerVersion()
        {
            var modOrganizerExePath = ManageSettings.AppMOexePath;

            try
            {
                if (!File.Exists(modOrganizerExePath)) return null;

                var versionInfo = FileVersionInfo.GetVersionInfo(modOrganizerExePath);

                // ProductVersion is null when the resource is missing, and FileVersion
                // is not filled in by every build either.
                return !string.IsNullOrWhiteSpace(versionInfo.ProductVersion)
                    ? versionInfo.ProductVersion
                    : versionInfo.FileVersion;
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Update: failed to read the version of '" + modOrganizerExePath + "'.");
                return null;
            }
        }

        #endregion
    }
}
