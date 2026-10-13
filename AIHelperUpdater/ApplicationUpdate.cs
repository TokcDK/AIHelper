using System;
using System.Collections.Generic;
using System.IO;

namespace AIHelperUpdater
{
    /// <summary>
    /// Replaces the files of the application with the version left in the payload folder.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Two steps, in this order and for this reason. First the files named by update-rules.txt
    /// are moved into RES\update\app\old, which is what makes the update reversible: the payload
    /// is then moved over the application with overwrite, and overwriting a file means deleting
    /// it first, so without the backup the old version would be gone before the new one is known
    /// to have arrived. Then the payload is moved in - every file of it, keeping its layout.
    /// </para>
    /// <para>
    /// A failure at any point leaves a marker file and renames the payload, so that the
    /// application does not hand over to the same failing update again on its next start. The
    /// backup is not rolled back automatically: a half-applied update is still a state the user
    /// can recover from with the backup, and an automatic rollback that itself failed would
    /// leave less to recover from, not more.
    /// </para>
    /// </remarks>
    internal sealed class ApplicationUpdate
    {
        private readonly UpdateLayout _layout;
        private readonly UpdateLog _log;

        internal ApplicationUpdate(UpdateLayout layout, UpdateLog log)
        {
            _layout = layout;
            _log = log;
        }

        /// <summary>
        /// True when the payload holds the two things an update is: the program and its RES folder.
        /// </summary>
        /// <remarks>
        /// This is the same test the application makes before it starts this tool, repeated here
        /// because the tool is the last place that can refuse. A payload that is only half
        /// unpacked would otherwise be applied as far as it goes.
        /// </remarks>
        /// <param name="applicationExecutableFileName">File name of the application's program.</param>
        internal bool IsPayloadComplete(string applicationExecutableFileName)
        {
            if (string.IsNullOrWhiteSpace(applicationExecutableFileName)) return false;

            return File.Exists(Path.Combine(_layout.PayloadDirectory, applicationExecutableFileName))
                   && Directory.Exists(Path.Combine(_layout.PayloadDirectory, UpdateLayout.ResFolderName));
        }

        /// <summary>
        /// Applies the payload.
        /// </summary>
        /// <param name="applicationExecutableFileName">File name of the application's program.</param>
        /// <returns>True when the application now holds the new version.</returns>
        internal bool Apply(string applicationExecutableFileName)
        {
            if (!IsPayloadComplete(applicationExecutableFileName))
            {
                Fail("the folder '" + _layout.PayloadDirectory + "' does not hold a complete update"
                     + " (the program and a RES folder are both required), so nothing was changed.");
                return false;
            }

            var rules = ReadRules();
            if (rules == null) return false;

            var exclusionPaths = ResolveExclusionPaths(rules);

            try
            {
                ClearFailureMarker();

                if (!MoveApplicationFilesToBackup(rules, exclusionPaths))
                {
                    Fail("the backup could not be taken.");
                    return false;
                }

                if (!MovePayloadIntoApplication(exclusionPaths))
                {
                    Fail("the payload could not be applied.");
                    return false;
                }

                RemovePayload();

                _log.Info("the update was applied; the previous version is in '" + _layout.BackupDirectory + "'.");
                return true;
            }
            catch (Exception ex)
            {
                Fail("the update failed: " + ex);
                return false;
            }
        }

        #region Rules

        /// <summary>
        /// Reads the rules, or reports why the update cannot go ahead without them.
        /// </summary>
        /// <remarks>
        /// The rules are not optional. Without them nothing would be backed up, and an update
        /// that overwrites the program with no way back is exactly what the file exists to
        /// prevent - so a missing rules file stops the update instead of quietly weakening it.
        /// </remarks>
        private IReadOnlyList<UpdateRule> ReadRules()
        {
            string error;
            var rules = new UpdateRulesFile(_layout.RulesFilePath).TryRead(out error);

            if (rules == null)
            {
                Fail(error);
                return null;
            }

            _log.Info("read " + rules.Count + " rule(s) from '" + _layout.RulesFilePath + "'.");
            return rules;
        }

        /// <summary>
        /// Resolves a rule to an absolute path inside the application folder.
        /// </summary>
        /// <remarks>
        /// A rule that is rooted, or that walks out of the application folder with "..", is
        /// refused: the rules file is a file a user may edit, and a rule that names something
        /// outside the application would move files that have nothing to do with the update.
        /// </remarks>
        /// <param name="relativePath">Path as the rules file spells it.</param>
        /// <param name="error">Why the path was refused, or null.</param>
        /// <returns>The absolute path, or null.</returns>
        private string ResolveRulePath(string relativePath, out string error)
        {
            error = null;

            if (Path.IsPathRooted(relativePath))
            {
                error = "'" + relativePath + "' is an absolute path; rules are relative to the application folder.";
                return null;
            }

            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(Path.Combine(_layout.ApplicationDirectory, relativePath));
            }
            catch (Exception ex)
            {
                error = "'" + relativePath + "' is not a usable path: " + ex.Message;
                return null;
            }

            if (!_layout.IsInsideApplication(fullPath))
            {
                error = "'" + relativePath + "' points outside the application folder.";
                return null;
            }

            return fullPath;
        }

        /// <summary>
        /// Resolves the exclusion rules to absolute paths, once per update.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Resolved up front because the question they answer is asked about every entry of every
        /// folder the update walks, and a path that has to be turned into an absolute one on each
        /// of those is a path rebuilt a few thousand times for one answer.
        /// </para>
        /// <para>
        /// An exclusion that cannot be resolved is dropped rather than fatal. It names something
        /// that would not be moved anyway - a path outside the application, or one that is not a
        /// path - and refusing the whole update over it would be refusing it over a line that
        /// protects nothing.
        /// </para>
        /// </remarks>
        private IReadOnlyList<string> ResolveExclusionPaths(IReadOnlyList<UpdateRule> rules)
        {
            var exclusionPaths = new List<string>();

            foreach (var rule in rules)
            {
                if (!rule.IsExclusion) continue;

                string error;
                var excludedPath = ResolveRulePath(rule.RelativePath, out error);

                if (excludedPath == null)
                {
                    _log.Warn("exclusion skipped: " + error);
                    continue;
                }

                exclusionPaths.Add(excludedPath);
            }

            if (exclusionPaths.Count > 0)
            {
                _log.Info("excluding " + exclusionPaths.Count + " path(s) from the update.");
            }

            return exclusionPaths;
        }

        /// <summary>
        /// True when an exclusion covers the given path.
        /// </summary>
        /// <remarks>
        /// An exclusion names a path, and everything inside it is excluded with it: the rule that
        /// keeps the user's own themes is written once, for the folder, and not once per theme.
        /// </remarks>
        private static bool IsExcluded(string absolutePath, IReadOnlyList<string> exclusionPaths)
        {
            foreach (var excludedPath in exclusionPaths)
            {
                if (UpdateLayout.IsUnderPath(absolutePath, excludedPath)) return true;
            }

            return false;
        }

        #endregion

        #region Backup

        /// <summary>
        /// Moves the files the rules name into the backup folder.
        /// </summary>
        /// <param name="rules">The rules, already read.</param>
        /// <param name="exclusionPaths">Absolute paths the update must not touch.</param>
        /// <returns>True when the backup is complete.</returns>
        private bool MoveApplicationFilesToBackup(
            IReadOnlyList<UpdateRule> rules, IReadOnlyList<string> exclusionPaths)
        {
            _log.Info("taking a backup of the current version into '" + _layout.BackupDirectory + "'.");

            // A backup left by an earlier update is replaced rather than added to, so that the
            // folder always holds the one version the current files were taken from. Failing to
            // clear it is not fatal: the moves below overwrite what they find either way.
            if (!FileTreeMover.TryRemoveDirectory(_layout.BackupDirectory))
            {
                _log.Warn("the previous backup in '" + _layout.BackupDirectory + "' could not be removed.");
            }

            Directory.CreateDirectory(_layout.BackupDirectory);

            foreach (var rule in rules)
            {
                if (rule.IsExclusion) continue;

                string error;
                var sourcePath = ResolveRulePath(rule.RelativePath, out error);
                if (sourcePath == null)
                {
                    _log.Warn("rule skipped: " + error);
                    continue;
                }

                if (_layout.IsReserved(sourcePath))
                {
                    _log.Info("rule skipped, '" + sourcePath + "' belongs to the update itself.");
                    continue;
                }

                if (IsExcluded(sourcePath, exclusionPaths))
                {
                    _log.Info("rule skipped, '" + sourcePath + "' is excluded by another rule.");
                    continue;
                }

                if (!File.Exists(sourcePath) && !Directory.Exists(sourcePath))
                {
                    _log.Info("nothing to back up at '" + sourcePath + "'.");
                    continue;
                }

                var destinationPath = Path.Combine(_layout.BackupDirectory, rule.RelativePath);

                if (Directory.Exists(sourcePath))
                {
                    var movedFiles = FileTreeMover.MoveDirectory(
                        sourcePath,
                        destinationPath,
                        (source, destination) => SkipWhileBackingUp(source, exclusionPaths),
                        _log.Info);

                    _log.Info("backed up " + movedFiles + " file(s) from '" + sourcePath + "'.");
                }
                else
                {
                    FileTreeMover.MoveFileOverwrite(sourcePath, destinationPath);
                    _log.Info("backed up '" + sourcePath + "'.");
                }
            }

            return true;
        }

        /// <summary>
        /// Skip rule used while the backup is taken: what may not leave the application.
        /// </summary>
        /// <remarks>
        /// Asked about every entry of every folder the backup walks, and not only about the paths
        /// the rules name. An exclusion almost always names something *inside* a rule - the user's
        /// own themes live inside RES, which is a rule - so a check made only at the top of a rule
        /// would never see it, and the folder it protects would be carried into the backup and
        /// lost from the application.
        /// </remarks>
        private bool SkipWhileBackingUp(string sourcePath, IReadOnlyList<string> exclusionPaths)
        {
            return _layout.IsReserved(sourcePath) || IsExcluded(sourcePath, exclusionPaths);
        }

        #endregion

        #region Applying

        /// <summary>
        /// Moves every entry of the payload into the application folder.
        /// </summary>
        /// <param name="exclusionPaths">Absolute paths the update must not touch.</param>
        /// <returns>True when the payload was moved in.</returns>
        private bool MovePayloadIntoApplication(IReadOnlyList<string> exclusionPaths)
        {
            _log.Info("applying the payload from '" + _layout.PayloadDirectory + "'.");

            var movedFileCount = 0;

            // To a list first: the entries are moved out of the payload folder as the loop runs.
            var entries = Directory.GetFileSystemEntries(_layout.PayloadDirectory);

            foreach (var entry in entries)
            {
                var destination = Path.Combine(_layout.ApplicationDirectory, Path.GetFileName(entry));

                if (Directory.Exists(entry))
                {
                    movedFileCount += FileTreeMover.MoveDirectory(
                        entry,
                        destination,
                        (source, target) => SkipWhileApplying(target, exclusionPaths),
                        _log.Info);
                }
                else if (SkipWhileApplying(destination, exclusionPaths))
                {
                    _log.Info("left in place: " + destination);
                }
                else
                {
                    FileTreeMover.MoveFileOverwrite(entry, destination);
                    movedFileCount++;
                }
            }

            _log.Info("applied " + movedFileCount + " file(s).");
            return true;
        }

        /// <summary>
        /// Skip rule used while the payload is applied: what may not be written to.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is what keeps the running tool alive. The payload ships a copy of this program in
        /// RES\tools\aihelper-updater, and those files are the ones this process is executing
        /// from - writing over them would fail at best. The application has already refreshed
        /// that copy from this very payload before starting the tool, so nothing is lost by
        /// skipping it here.
        /// </para>
        /// <para>
        /// An excluded path is skipped here as well as while the backup is taken, and that is the
        /// half that is easy to leave out. The release ships the folder the user's own themes live
        /// in, so an update that only refused to back them up would still write the shipped ones
        /// over them; "left in place" has to mean both.
        /// </para>
        /// </remarks>
        private bool SkipWhileApplying(string destinationPath, IReadOnlyList<string> exclusionPaths)
        {
            return _layout.IsReserved(destinationPath) || IsExcluded(destinationPath, exclusionPaths);
        }

        /// <summary>
        /// Removes the payload once it has been applied.
        /// </summary>
        /// <remarks>
        /// The application decides whether an update is waiting by looking at this folder, so
        /// emptying it is what stops the next start from handing over to an update that has
        /// already happened.
        /// </remarks>
        private void RemovePayload()
        {
            if (FileTreeMover.TryRemoveDirectory(_layout.PayloadDirectory)) return;

            _log.Warn("the applied payload in '" + _layout.PayloadDirectory + "' could not be removed.");
        }

        #endregion

        #region Failure

        /// <summary>
        /// Reports a failure and takes the payload out of the way.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Moving the payload aside is not optional and not a detail of the failure. This program
        /// starts the application again when it is done whatever happened, and the application
        /// decides that an update is waiting by looking at this very folder - so a payload left in
        /// place after a failure would be handed back to this program on the next start, which
        /// would fail the same way and start the application again, for as long as the user puts
        /// up with it. Renaming the folder is what breaks that loop while leaving the update there
        /// to be looked at.
        /// </para>
        /// <para>
        /// The failure that is easiest to get wrong is the incomplete payload: refusing to apply
        /// it looks like "nothing was touched, so nothing has to be moved aside", and it is
        /// exactly the case that would be found again.
        /// </para>
        /// </remarks>
        private void Fail(string reason)
        {
            _log.Error(reason);
            WriteFailureMarker(reason);
            MovePayloadAside();
        }

        /// <summary>
        /// Renames the payload so that the next start does not try the same update again.
        /// </summary>
        private void MovePayloadAside()
        {
            try
            {
                FileTreeMover.TryRemoveDirectory(_layout.FailedPayloadDirectory);

                if (!Directory.Exists(_layout.PayloadDirectory)) return;

                Directory.Move(_layout.PayloadDirectory, _layout.FailedPayloadDirectory);
                _log.Warn("the payload was moved to '" + _layout.FailedPayloadDirectory
                          + "' so that the update is not attempted again.");
            }
            catch (Exception ex)
            {
                _log.Error("the payload could not be moved aside: " + ex.Message);
            }
        }

        /// <summary>
        /// Leaves the reason for the application to read on its next start.
        /// </summary>
        private void WriteFailureMarker(string reason)
        {
            try
            {
                var directory = Path.GetDirectoryName(_layout.FailureMarkerFilePath);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

                File.WriteAllText(_layout.FailureMarkerFilePath, reason ?? string.Empty);
            }
            catch (Exception ex)
            {
                _log.Error("the failure marker could not be written: " + ex.Message);
            }
        }

        /// <summary>
        /// Removes a marker left by an earlier run, so that a successful update is not reported
        /// as a failure.
        /// </summary>
        private void ClearFailureMarker()
        {
            try
            {
                if (File.Exists(_layout.FailureMarkerFilePath)) File.Delete(_layout.FailureMarkerFilePath);
            }
            catch (Exception ex)
            {
                _log.Warn("a marker from an earlier run could not be removed: " + ex.Message);
            }
        }

        #endregion
    }
}
