using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NLog;

namespace AIHelper.Manage.Update.Targets
{
    /// <summary>
    /// Base for update targets. A target knows which folders can be updated,
    /// how to read their current version and how to replace their files.
    /// <para>
    /// The update itself is destructive by design: the old content is moved
    /// into a backup directory first (<see cref="MakeBackup"/>), the new files
    /// are extracted over the folder, and the backup is restored when anything
    /// fails (<see cref="RestoreBackup"/>).
    /// </para>
    /// </summary>
    abstract class UpdateTargetBase
    {
        protected static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>File that stores the metadata of a Mod Organizer mod.</summary>
        internal const string MetaIniFileName = "meta.ini";

        /// <summary>Section of <see cref="MetaIniFileName"/> that holds the version.</summary>
        internal const string MetaIniVersionSection = "General";

        /// <summary>Key of <see cref="MetaIniFileName"/> that holds the version.</summary>
        internal const string MetaIniVersionKey = "version";

        /// <summary>Extensions that <see cref="PerformUpdate"/> knows how to apply.</summary>
        static readonly string[] SupportedUpdateExtensions = { ".ZIP", ".DLL", ".7Z", ".RAR" };

        /// <summary>
        /// Current update request state. Assigned by <see cref="Updater"/>
        /// before every folder is processed.
        /// </summary>
        internal UpdateInfo Info;

        /// <summary>
        /// Path to the backup copy of the target folder.
        /// </summary>
        internal string BackupDirPath;

        /// <summary>
        /// Files and folders that must be restored into the updated folder.
        /// </summary>
        protected readonly HashSet<string> RestoreList = new HashSet<string>();

        protected UpdateTargetBase(UpdateInfo info)
        {
            this.Info = info;
        }

        /// <summary>
        /// Gets the folders that can be updated, as
        /// "folder name or path" to "update record" pairs.
        /// </summary>
        /// <returns>The folders to process, or null when there are none.</returns>
        internal abstract Dictionary<string, string> GetUpdateInfos();

        /// <summary>
        /// Replaces the files of the current folder with the downloaded ones.
        /// </summary>
        /// <returns>True when the folder was updated.</returns>
        internal abstract bool UpdateFiles();

        /// <summary>
        /// Reads the version currently installed in the current folder.
        /// </summary>
        internal abstract void SetCurrentVersion();

        /// <summary>
        /// Lets a target adjust the request before the source resolves the
        /// latest version.
        /// <para>
        /// Called after <see cref="UpdateInfo.Reset"/> and
        /// <see cref="SetCurrentVersion"/>, and before
        /// <see cref="Sources.UpdateSourceBase.GetLastVersion"/>. Targets that
        /// need to constrain the file name the source looks for override this
        /// instead of doing it as a side effect of another method.
        /// </para>
        /// </summary>
        internal virtual void PrepareUpdateRequest()
        {
        }

        /// <summary>
        /// Folder that contains the target folders, or an empty string when the
        /// folder keys are already absolute paths.
        /// </summary>
        internal virtual string GetParentFolderPath()
        {
            return string.Empty;
        }

        /// <summary>
        /// Copies the current folder into <see cref="BackupDirPath"/> so the
        /// update can be undone.
        /// </summary>
        /// <returns>True when a usable backup exists.</returns>
        internal virtual bool MakeBackup()
        {
            try
            {
                BackupDirPath = Path.Combine(
                    ManageSettings.UpdatedModsOlderVersionsBuckupDirPath,
                    Info.TargetFolderPath.Name + "_" + Info.TargetCurrentVersion);

                if (Directory.Exists(BackupDirPath))
                {
                    BackupDirPath += ManageSettings.DateTimeBasedSuffix;
                }

                Info.BackupDirPath = BackupDirPath;

                //set RestoreList
                RestoreList.Clear();
                foreach (var path in RestorePathsList())
                {
                    if (string.IsNullOrWhiteSpace(path)) continue;

                    RestoreList.Add(Path.GetFullPath(Path.Combine(Info.BackupDirPath, path)));
                }

                foreach (var path in RestorePathsListExtra())
                {
                    if (string.IsNullOrWhiteSpace(path)) continue;

                    var dirinfo = new DirectoryInfo(path);
                    if (!dirinfo.Exists
                        || !dirinfo.IsSymlink()
                        || !dirinfo.IsValidSymlink()) continue;

                    var modPath = ManageModOrganizer.GetPathInMods(dirinfo.GetSymlinkTarget());

                    //GetPathInMods returns a full path like "<Mods>\<mod name>", so it must be compared
                    //with FullName. Comparing it against Path.GetFileName(...) was always unequal, which
                    //silently kept every extra restore path out of RestoreList.
                    if (!string.Equals(modPath, Info.TargetFolderPath.FullName, StringComparison.OrdinalIgnoreCase)) continue;

                    // if parsing mod path is same then add for bak
                    RestoreList.Add(path);
                }

                MakeBackup(BackupDirPath, Info.TargetFolderPath.FullName);

                if (!Directory.Exists(BackupDirPath)) return false;

                // a folder with content must have produced a backup with content,
                // otherwise the mod would be overwritten without a way back
                if (Info.TargetFolderPath.FullName.IsAnyFileExistsInTheDir()
                    && !BackupDirPath.IsAnyFileExistsInTheDir())
                {
                    _log.Warn("Update: backup of '" + Info.TargetFolderPath.Name + "' is empty while the folder is not. Update skipped.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "An error occured while backup creation.");
                return false;
            }
        }

        /// <summary>
        /// Puts the content of <see cref="BackupDirPath"/> back into the target
        /// folder, undoing a failed update.
        /// </summary>
        internal virtual void RestoreBackup()
        {
            RestoreModFromBackup(BackupDirPath, Info.TargetFolderPath.FullName);
        }

        /// <summary>
        /// Extracts or installs the downloaded file into the current folder.
        /// </summary>
        /// <returns>True when the folder was updated.</returns>
        protected bool PerformUpdate()
        {
            if (!IsUpdateFileForThisTarget()) return false;

            // Move file from 2mo to downloads when it was manually downloaded in 2mo
            if (UpdateIo.IsUnderPath(Info.UpdateFilePath, ManageSettings.Install2MoDirPath))
            {
                Directory.CreateDirectory(ManageSettings.ModsUpdateDirDownloadsPath);

                // keep the sub path the file had under the 2mo directory
                var newFilePathInDownloads = UpdateIo.RemapPath(
                    Info.UpdateFilePath,
                    ManageSettings.Install2MoDirPath,
                    ManageSettings.ModsUpdateDirDownloadsPath);

                UpdateIo.EnsureParentDirectory(newFilePathInDownloads);
                UpdateIo.TryDeleteFile(newFilePathInDownloads);

                // move file to new path
                File.Move(Info.UpdateFilePath, newFilePathInDownloads);

                // set changed path to new
                Info.UpdateFilePath = newFilePathInDownloads;
            }

            var updatingModDirPath = Info.TargetFolderPath.FullName;
            Directory.CreateDirectory(updatingModDirPath);

            bool success = false;

            try
            {
                if (File.Exists(Info.UpdateFilePath))
                {
                    success = ApplyUpdateFile(Info.UpdateFilePath, updatingModDirPath);
                }

                if (success)
                {
                    RestoreSomeFiles(Info.BackupDirPath, updatingModDirPath);

                    ManageIni.WriteIniValue(
                        Path.Combine(updatingModDirPath, MetaIniFileName),
                        MetaIniVersionSection,
                        MetaIniVersionKey,
                        Info.TargetLastVersion);
                }
                else
                {
                    if (!updatingModDirPath.IsAnyFileExistsInTheDir()) UpdateIo.TryDeleteDirectory(updatingModDirPath);

                    return false;
                }
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Failed to update mod" + " " + Info.TargetFolderPath.Name);

                if (!updatingModDirPath.IsAnyFileExistsInTheDir()) UpdateIo.TryDeleteDirectory(updatingModDirPath);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Applies the downloaded archive or dll to the folder.
        /// </summary>
        /// <param name="updateFilePath">Downloaded file.</param>
        /// <param name="updatingModDirPath">Folder to update.</param>
        /// <returns>True when the file was applied.</returns>
        private bool ApplyUpdateFile(string updateFilePath, string updatingModDirPath)
        {
            string ext = Path.GetExtension(updateFilePath).ToUpperInvariant();

            if (ext == ".ZIP")
            {
                using (ZipArchive archive = ZipFile.OpenRead(updateFilePath))
                {
                    archive.ExtractToDirectory(updatingModDirPath);
                }
            }
            else if (ext == ".DLL")
            {
                var fullDllFileName = Path.GetFileName(updateFilePath);

                var targetDir = GetDllTargetDir(fullDllFileName, updatingModDirPath);

                if (string.IsNullOrWhiteSpace(targetDir))
                {
                    // default BepInEx plugins dir
                    targetDir = Path.Combine(updatingModDirPath, "BepInEx", "plugins");
                }

                Directory.CreateDirectory(targetDir);
                File.Move(updateFilePath, Path.Combine(targetDir, fullDllFileName));
            }
            else if (ext == ".7Z")
            {
                ManageArchive.Extract7zipTo(updateFilePath, updatingModDirPath);
            }
            else if (ext == ".RAR")
            {
                ManageArchive.ExtractRarTo(updateFilePath, updatingModDirPath);
            }
            else
            {
                // never report success for a file that was not applied, otherwise
                // the mod is marked as up to date and will not be retried
                _log.Warn("Update: unsupported update file extension '" + ext + "' for '"
                          + Info.TargetFolderPath.Name + "'. Supported: "
                          + string.Join(", ", SupportedUpdateExtensions) + ".");
                return false;
            }

            return true;
        }

        /// <summary>
        /// True when the downloaded file matches the prefix of the update
        /// record. The prefix can be a plain name ("BepInEx") or a name with an
        /// extension ("Mod.Organizer-"), so the file name is compared both with
        /// and without its extension.
        /// </summary>
        private bool IsUpdateFileForThisTarget()
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(Info.UpdateFilePath);
            var prefixWithoutExtension = Path.GetFileNameWithoutExtension(Info.UpdateFileStartsWith);

            return fileNameWithoutExtension.StartsWith(
                       Info.UpdateFileStartsWith,
                       StringComparison.InvariantCultureIgnoreCase)
                   || fileNameWithoutExtension.ToUpperInvariant().StartsWith(
                       prefixWithoutExtension.ToUpperInvariant(),
                       StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Finds the folder a dll lived in before the update, so an updated dll
        /// lands in the same sub folder.
        /// </summary>
        /// <param name="fullDllFileName">File name of the dll.</param>
        /// <param name="updatingModDirPath">Folder being updated.</param>
        /// <returns>Target folder, or an empty string when the dll is new.</returns>
        private string GetDllTargetDir(string fullDllFileName, string updatingModDirPath)
        {
            if (string.IsNullOrWhiteSpace(Info.BackupDirPath)) return string.Empty;
            if (!Directory.Exists(Info.BackupDirPath)) return string.Empty;

            //search dll with same name and get subpath from this dll
            foreach (var dll in Directory.EnumerateFiles(Info.BackupDirPath, "*.dll", SearchOption.AllDirectories))
            {
                if (Path.GetFileName(dll) != fullDllFileName) continue;

                var dllDirPath = Path.GetDirectoryName(dll);
                if (string.IsNullOrWhiteSpace(dllDirPath)) return string.Empty;

                return UpdateIo.RemapPath(dllDirPath, Info.BackupDirPath, updatingModDirPath);
            }

            return string.Empty;
        }

        /// <summary>
        /// Files and folders that must survive an update, relative to the mod
        /// folder, for example ".\meta.ini".
        /// </summary>
        internal virtual string[] RestorePathsList()
        {
            return new string[0];
        }

        /// <summary>
        /// Folders from the game's list of symlink targets that must be checked
        /// for links pointing into the updated mod.
        /// </summary>
        internal virtual string[] RestorePathsListExtra()
        {
            return new string[0];
        }

        /// <summary>
        /// Copies the files listed in <see cref="RestoreList"/> plus user
        /// configuration back into the updated folder.
        /// </summary>
        /// <param name="oldModBackupDirPath">Backup of the previous version.</param>
        /// <param name="updatingModDirPath">Folder being updated.</param>
        protected void RestoreSomeFiles(string oldModBackupDirPath, string updatingModDirPath)
        {
            if (string.IsNullOrWhiteSpace(oldModBackupDirPath)
                || string.IsNullOrWhiteSpace(updatingModDirPath)) return;

            if (!Directory.Exists(oldModBackupDirPath)
                || !Directory.Exists(updatingModDirPath)) return;

            // restore some dirs
            foreach (var dir in new DirectoryInfo(oldModBackupDirPath).EnumerateDirectories("*", SearchOption.AllDirectories))
            {
                try
                {
                    if (Directory.Exists(Path.Combine(updatingModDirPath, dir.Name))
                        || !RestoreList.Contains(dir.FullName)) continue;

                    var targetPath = dir.FullName.Replace(oldModBackupDirPath, updatingModDirPath);
                    UpdateIo.EnsureParentDirectory(targetPath);

                    if (!Directory.Exists(dir.FullName)) continue;

                    dir.FullName.CopyAll(targetPath);
                }
                catch (Exception ex)
                {
                    _log.Debug("Update: failed to restore a directory. error:" + Environment.NewLine + ex);
                }
            }

            // restore some files
            foreach (var file in new DirectoryInfo(oldModBackupDirPath).EnumerateFiles("*", SearchOption.AllDirectories))
            {
                try
                {
                    if (!ShouldRestoreFile(file, oldModBackupDirPath, updatingModDirPath)) continue;

                    var targetFilePath = file.FullName.Replace(oldModBackupDirPath, updatingModDirPath);

                    UpdateIo.EnsureParentDirectory(targetFilePath);

                    if (!File.Exists(file.FullName)
                        || File.Exists(targetFilePath)) continue;

                    File.Copy(file.FullName, targetFilePath);
                }
                catch (Exception ex)
                {
                    _log.Debug("Update: failed to restore a file. error:" + Environment.NewLine + ex);
                }
            }
        }

        /// <summary>
        /// True when a backed up file must be copied into the updated folder.
        /// A file is restored when it is a config file, a picture that lived in
        /// the root of the mod, or one of the explicitly listed restore paths —
        /// and only when the update did not bring its own copy.
        /// </summary>
        /// <param name="file">File from the backup.</param>
        /// <param name="oldModBackupDirPath">Backup of the previous version.</param>
        /// <param name="updatingModDirPath">Folder being updated.</param>
        private bool ShouldRestoreFile(FileInfo file, string oldModBackupDirPath, string updatingModDirPath)
        {
            //the check must use the file's own target path. Comparing only file.Name against the root of
            //updatingModDirPath treated any same named file in the root as a collision and skipped restoring
            //a nested file whose real target path was free.
            var targetFilePath = file.FullName.Replace(oldModBackupDirPath, updatingModDirPath);
            if (File.Exists(targetFilePath)) return false;

            var isConfigFile = file.Extension == ".ini" || file.Extension == ".cfg";

            var isRootPicture = file.DirectoryName == oldModBackupDirPath && file.Extension.IsPictureExtension();

            return isConfigFile
                   || isRootPicture
                   || RestoreList.Contains(file.FullName);
        }

        /// <summary>
        /// Puts the previous version of the folder back, replacing whatever the
        /// failed update left behind.
        /// </summary>
        /// <param name="oldModBackupDirPath">Backup of the previous version.</param>
        /// <param name="updatingModDirPath">Folder that was being updated.</param>
        private static void RestoreModFromBackup(string oldModBackupDirPath, string updatingModDirPath)
        {
            if (string.IsNullOrWhiteSpace(oldModBackupDirPath)
                || string.IsNullOrWhiteSpace(updatingModDirPath)) return;

            //same guard as RestoreSomeFiles: MakeBackup can report failure before it ever created the backup
            //directory, and Directory.GetDirectories below would then throw DirectoryNotFoundException and
            //abort the whole update pass instead of just failing this one folder
            if (!Directory.Exists(oldModBackupDirPath)
                || !Directory.Exists(updatingModDirPath)) return;

            // restore old dirs
            foreach (var folder in Directory.GetDirectories(oldModBackupDirPath))
            {
                try
                {
                    var targetPath = folder.Replace(oldModBackupDirPath, updatingModDirPath);

                    if (Directory.Exists(targetPath))
                    {
                        _log.Info("Restore: replacing '" + targetPath + "' with the backed up version.");
                        UpdateIo.TryDeleteDirectory(targetPath, true);
                    }

                    UpdateIo.EnsureParentDirectory(targetPath);
                    if (Directory.Exists(folder)) Directory.Move(folder, targetPath);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Restore: failed to move dir to mod dir.");
                }
            }

            // restore old files
            foreach (var file in Directory.GetFiles(oldModBackupDirPath))
            {
                try
                {
                    var targetPath = file.Replace(oldModBackupDirPath, updatingModDirPath);

                    if (File.Exists(targetPath)) UpdateIo.TryDeleteFile(targetPath);

                    UpdateIo.EnsureParentDirectory(targetPath);
                    if (File.Exists(file)) File.Move(file, targetPath);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Restore: failed to move file to mod dir.");
                }
            }

            ManageFilesFoldersExtensions.DeleteEmptySubfolders(oldModBackupDirPath);
        }

        /// <summary>
        /// Moves the content of the folder into the backup directory. Folders
        /// and files that must be restored later are copied instead of moved.
        /// </summary>
        /// <param name="oldModBackupDirPath">Backup directory to fill.</param>
        /// <param name="updatingModDirPath">Folder being updated.</param>
        private void MakeBackup(string oldModBackupDirPath, string updatingModDirPath)
        {
            Directory.CreateDirectory(oldModBackupDirPath);

            //backup old dirs
            //NOTE: materialized, because the loop below moves the entries out of the very directory
            //being enumerated. Enumerating while moving makes the walk skip entries, which would leave
            //files out of the backup that the rollback depends on.
            foreach (var folderPath in new DirectoryInfo(updatingModDirPath).EnumerateDirectories().ToArray())
            {
                try
                {
                    bool isSynlinkTarget = false;
                    var folder = folderPath;
                    if (folder.IsSymlink())
                    {
                        if (!folder.IsValidSymlink()) continue;

                        isSynlinkTarget = true;
                        folder = new DirectoryInfo(folder.GetSymlinkTarget());
                    }

                    //The backup path must come from the entry's own path. Using folder.FullName here would
                    //use the resolved symlink target, which lives outside updatingModDirPath: the Replace
                    //would not match and the copy would target its own source.
                    var backupPath = new DirectoryInfo(folderPath.FullName.Replace(updatingModDirPath, oldModBackupDirPath));
                    UpdateIo.EnsureParentDirectory(backupPath.FullName);

                    if (!folder.Exists) continue;

                    if (isSynlinkTarget)
                    {
                        folder.CopyAll(backupPath);
                    }
                    else
                    {
                        _log.Debug("Update: moving '" + folder.FullName + "' to backup '" + backupPath.FullName + "'.");
                        folder.MoveTo(backupPath.FullName);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Update: failed to move dir to bak dir.");
                }
            }

            //backup old files
            foreach (var filePath in new DirectoryInfo(updatingModDirPath).EnumerateFiles().ToArray())
            {
                try
                {
                    bool isSynlinkTarget = false;
                    var file = filePath;
                    if (file.IsSymlink())
                    {
                        if (!file.IsValidSymlink()) continue;

                        isSynlinkTarget = true;
                        file = new FileInfo(filePath.GetSymlinkTarget());
                    }

                    //same as for directories: the backup path belongs to the entry, not to the link target
                    var backupPath = filePath.FullName.Replace(updatingModDirPath, oldModBackupDirPath);
                    UpdateIo.EnsureParentDirectory(backupPath);

                    if (RestoreList.Contains(file.FullName) || isSynlinkTarget)
                    {
                        file.CopyTo(backupPath);
                    }
                    else
                    {
                        _log.Debug("Update: moving '" + file.FullName + "' to backup '" + backupPath + "'.");
                        file.MoveTo(backupPath);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Update: failed to move file to bak dir.");
                }
            }
        }
    }
}
