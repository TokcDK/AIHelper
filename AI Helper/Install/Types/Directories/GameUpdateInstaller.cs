using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AIHelper.Data.Modlist;
using AIHelper.Manage;
using AIHelper.Manage.Update;

namespace AIHelper.Install.Types.Directories
{
    /// <summary>
    /// Installs the game update which is created by the update maker
    /// </summary>
    class GameUpdateInstaller : DirectoriesInstallerBase
    {
        /// <summary>
        /// Separators of the sub paths lists in the update ini
        /// </summary>
        private static readonly string[] SubPathSeparators = { ", ", "," };

        public override int Order => base.Order / 5;

        public override string ContentDescription => "Game update made by the update maker (a folder with its update info file)";

        private static string GameUpdateInfoFileName => ManageSettings.GameUpdateInstallerIniFileName;

        private DirectoryInfo _dir;
        private GameUpdateInfo _updateInfo;
        private bool _isRoot;
        private string _dateTimeSuffix;
        private string _backupsDir;
        private string _bakDir;
        private HashSet<string> _skipExistDirs;
        private HashSet<string> _skipExistFiles;
        private bool _crcFiles;
        private bool _sizeFiles;
        private bool _writeTimeFiles;

        protected override bool Get(DirectoryInfo dir)
        {
            _dir = dir;
            var updateInfoIniFilePath = Path.Combine(dir.FullName, GameUpdateInfoFileName);
            if (!File.Exists(updateInfoIniFilePath))
            {
                // not update
                return false;
            }

            _updateInfo = new GameUpdateInfo(updateInfoIniFilePath);

            if (string.IsNullOrWhiteSpace(_updateInfo.GameFolderName))
            {
                // empty game folder name
                return false;
            }

            if (ManageSettings.CurrentGameDirName != _updateInfo.GameFolderName)
            {
                // incorrect game
                MessageBox.Show(_updateInfo.GameFolderName + ": " + T._("Incorrect current game"));
                return false;
            }

            _crcFiles = _updateInfo.CRCFiles == "true";
            _sizeFiles = _updateInfo.SizeFiles == "true";
            _writeTimeFiles = _updateInfo.WriteTimeFiles == "true";

            _isRoot = _updateInfo.IsRoot == "true";

            _dateTimeSuffix = ManageSettings.DateTimeBasedSuffix;

            SetSkipLists();

            _backupsDir = Path.Combine(ManageSettings.CurrentGameDirPath, "Buckups");
            _bakDir = Path.Combine(_backupsDir, "Backup");
            if (Directory.Exists(_bakDir))
            {
                if (!_bakDir.IsEmptyDir())
                {
                    Directory.Move(_bakDir, _bakDir + "_old" + _dateTimeSuffix);
                }
            }
            Directory.CreateDirectory(_bakDir);

            // proceed update actions
            var updated = ProceedUpdateActions();

            // clean update info and folder
            //File.Delete(Path.Combine(dir.FullName, GameUpdateInfoFileName));
            //ManageFilesFolders.DeleteEmptySubfolders(dirPath: dir.FullName, deleteThisDir: true);
            // move dir to bak instead of delete
            dir.MoveTo(Path.Combine(_bakDir, dir.Name) + _dateTimeSuffix);

            if (updated)
            {
                MessageBox.Show(_updateInfo.GameFolderName + ": " + T._("Update applied.\n\nWill be opened Backup folder.\nCheck content and remove files and dirs which not need for you animore."));
                Process.Start(_bakDir);
            }
            else
            {
                MessageBox.Show(_updateInfo.GameFolderName + ": " + T._("Update not required."));
            }

            return updated;
        }

        /// <summary>
        /// Proceeds all update actions in the order which is required by the update
        /// </summary>
        /// <returns>True when at least one action changed something</returns>
        private bool ProceedUpdateActions()
        {
            var updated = false;

            foreach (var updateAction in GetUpdateActions())
            {
                if (updateAction())
                {
                    updated = true;
                }
            }

            return updated;
        }

        /// <summary>
        /// Update actions in the order of their execution
        /// </summary>
        private IEnumerable<Func<bool>> GetUpdateActions()
        {
            yield return ProceedRenameDirs;
            yield return ProceedRenameFiles;
            yield return ProceedRemoveDirs;
            yield return ProceedRemoveFiles;
            yield return ProceedUpdateMods;
            yield return ProceedUpdateData;
            yield return ProceedUpdateMO;
            yield return ProceedCreateDirs;
            yield return ProceedCreateFiles;
            yield return ProceedRemoveUnusedSeparators;
            yield return ProceedFixes;
        }

        /// <summary>
        /// Renames files of the game by the 'subPath:newName' records of the update ini
        /// </summary>
        private bool ProceedRenameFiles()
        {
            return ProceedRenameEntries(_updateInfo.RenameFiles, isDir: false);
        }

        /// <summary>
        /// Renames dirs of the game by the 'subPath:newName' records of the update ini
        /// </summary>
        private bool ProceedRenameDirs()
        {
            return ProceedRenameEntries(_updateInfo.RenameDirs, isDir: true);
        }

        /// <summary>
        /// Renames game entries by the 'subPath:newName' records of the update ini.
        /// Entry is moved to the backup dir when the new name is already used.
        /// </summary>
        /// <param name="value">List of the 'subPath:newName' records</param>
        /// <param name="isDir">True when dirs must be renamed else files</param>
        /// <returns>True when at least one entry must be renamed</returns>
        private bool ProceedRenameEntries(string value, bool isDir)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var anyRenamed = false;

            foreach (var record in SplitSubPaths(value))
            {
                try
                {
                    var renameData = record.Split(':');
                    if (renameData.Length != 2)
                    {
                        continue;
                    }

                    var subPath = renameData[0];
                    var newName = renameData[1];

                    var targetPath = ManageSettings.CurrentGameDirPath + Path.DirectorySeparatorChar + subPath;

                    if (!(isDir ? Directory.Exists(targetPath) : File.Exists(targetPath)))
                    {
                        continue;
                    }

                    anyRenamed = true;

                    var newPath = Path.Combine(Path.GetDirectoryName(targetPath), newName);
                    if (isDir ? Directory.Exists(newPath) : File.Exists(newPath))
                    {
                        // move to bak if exists new path
                        MoveEntryToBak(targetPath, subPath, isDir);
                    }
                    else
                    {
                        // rename entry to new name
                        MoveEntry(targetPath, newPath, isDir);
                    }
                }
                catch (Exception ex)
                {
                    _log.Warn("Failed to rename " + (isDir ? "dir" : "file") + ". Record:" + record + "\r\nError:" + ex);
                }
            }

            return anyRenamed;
        }

        /// <summary>
        /// Moves the game entry to the backup dir
        /// </summary>
        /// <param name="targetPath">Path of the entry which must be moved to the backup dir</param>
        /// <param name="subPath">Sub path of the entry relative to the game dir</param>
        /// <param name="isDir">True when dir must be moved else file</param>
        private void MoveEntryToBak(string targetPath, string subPath, bool isDir)
        {
            MoveEntry(targetPath, _bakDir + Path.DirectorySeparatorChar + subPath, isDir);
        }

        /// <summary>
        /// Moves the file or dir to the selected path
        /// </summary>
        /// <param name="sourcePath">Source path of the entry</param>
        /// <param name="targetPath">Target path of the entry</param>
        /// <param name="isDir">True when dir must be moved else file</param>
        private static void MoveEntry(string sourcePath, string targetPath, bool isDir)
        {
            if (isDir)
            {
                Directory.Move(sourcePath, targetPath);
            }
            else
            {
                File.Move(sourcePath, targetPath);
            }
        }

        private bool ProceedFixes()
        {
            return _updateInfo.ApplyFixes == "true" && MetaIniNotesTweaks();
        }

        private static bool MetaIniNotesTweaks()
        {
            Dictionary<string, string> modDirNamesValues = new Dictionary<string, string>()
            {
                { ManageSettings.KKManagerFilesModName, ManageSettings.KKManagerFilesNotes},
                { "GameUserData", "<!DOCTYPE HTML PUBLIC \\\"-//W3C//DTD HTML 4.0//EN\\\" \\\"http://www.w3.org/TR/REC-html40/strict.dtd\\\">\\n<html><head><meta name=\\\"qrichtext\\\" content=\\\"1\\\" /><style type=\\\"text/css\\\">\\np, li { white-space: pre-wrap; }\\n</style></head><body style=\\\" font-family:'MS Shell Dlg 2'; font-size:8.25pt; font-weight:400; font-style:normal;\\\">\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">New files created by the game will be stored here. Saves, plugins configs and other.</p>\\n<p style=\\\"-qt-paragraph-type:empty; margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\"><br /></p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">\\x41d\\x43e\\x432\\x44b\\x435 \\x444\\x430\\x439\\x43b\\x44b \\x441\\x43e\\x437\\x434\\x430\\x43d\\x43d\\x44b\\x435 \\x438\\x433\\x440\\x43e\\x439 \\x431\\x443\\x434\\x443\\x442 \\x434\\x43e\\x431\\x430\\x432\\x43b\\x44f\\x442\\x44c\\x441\\x44f \\x441\\x44e\\x434\\x430. \\x421\\x43e\\x445\\x440\\x430\\x43d\\x435\\x43d\\x438\\x44f, \\x43d\\x430\\x441\\x442\\x440\\x43e\\x439\\x43a\\x438 \\x43f\\x43b\\x430\\x433\\x438\\x43d\\x43e\\x432 \\x438 \\x434\\x440.</p></body></html>"},
                { "Sideloader Modpack - Exclusive KKS", "<!DOCTYPE HTML PUBLIC \\\"-//W3C//DTD HTML 4.0//EN\\\" \\\"http://www.w3.org/TR/REC-html40/strict.dtd\\\">\\n<html><head><meta name=\\\"qrichtext\\\" content=\\\"1\\\" /><style type=\\\"text/css\\\">\\np, li { white-space: pre-wrap; }\\n</style></head><body style=\\\" font-family:'MS Shell Dlg 2'; font-size:8.25pt; font-weight:400; font-style:normal;\\\">\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">EN</p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">Koikatsu Sunshine exclusive mods.</p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">Warning! More mods more memory consumption.</p>\\n<p style=\\\"-qt-paragraph-type:empty; margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\"><br /></p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">required: Sideloader</p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">------------------------------------------------------------</p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">RU</p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">\\x42d\\x43a\\x441\\x43a\\x43b\\x44e\\x437\\x438\\x432\\x43d\\x44b\\x435 \\x43c\\x43e\\x434\\x44b \\x434\\x43b\\x44f Koikatsu Sunshine.</p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">\\x412\\x43d\\x438\\x43c\\x430\\x43d\\x438\\x435! \\x427\\x435\\x43c \\x431\\x43e\\x43b\\x44c\\x448\\x435 \\x43c\\x43e\\x434\\x43e\\x432, \\x442\\x435\\x43c \\x431\\x43e\\x43b\\x44c\\x448\\x435 \\x43f\\x43e\\x442\\x440\\x435\\x431\\x43b\\x435\\x43d\\x438\\x44f \\x43f\\x430\\x43c\\x44f\\x442\\x438.</p>\\n<p style=\\\"-qt-paragraph-type:empty; margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\"><br /></p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">\\x442\\x440\\x435\\x431\\x443\\x435\\x442\\x441\\x44f: Sideloader</p></body></html>"},
                { "Sideloader Modpack - KK_UncensorSelector", "<!DOCTYPE HTML PUBLIC \\\"-//W3C//DTD HTML 4.0//EN\\\" \\\"http://www.w3.org/TR/REC-html40/strict.dtd\\\">\\n<html><head><meta name=\\\"qrichtext\\\" content=\\\"1\\\" /><style type=\\\"text/css\\\">\\np, li { white-space: pre-wrap; }\\n</style></head><body style=\\\" font-family:'MS Shell Dlg 2'; font-size:8.25pt; font-weight:400; font-style:normal;\\\">\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">EN</p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">Koikatsu Sunshine mods for Uncensor Selector.</p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">Warning! More mods more memory consumption.</p>\\n<p style=\\\"-qt-paragraph-type:empty; margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\"><br /></p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">required: Sideloader and Uncensor Selector</p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">------------------------------------------------------------</p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">RU</p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">\\x42d\\x43a\\x441\\x43a\\x43b\\x44e\\x437\\x438\\x432\\x43d\\x44b\\x435 \\x43c\\x43e\\x434\\x44b \\x434\\x43b\\x44f Koikatsu Sunshine \\x434\\x43b\\x44f Uncensor Selector.</p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">\\x412\\x43d\\x438\\x43c\\x430\\x43d\\x438\\x435! \\x427\\x435\\x43c \\x431\\x43e\\x43b\\x44c\\x448\\x435 \\x43c\\x43e\\x434\\x43e\\x432, \\x442\\x435\\x43c \\x431\\x43e\\x43b\\x44c\\x448\\x435 \\x43f\\x43e\\x442\\x440\\x435\\x431\\x43b\\x435\\x43d\\x438\\x44f \\x43f\\x430\\x43c\\x44f\\x442\\x438.</p>\\n<p style=\\\"-qt-paragraph-type:empty; margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\"><br /></p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">\\x442\\x440\\x435\\x431\\x443\\x435\\x442\\x441\\x44f: Sideloader \\x438 Uncensor Selector</p></body></html>"},
            };

            var ret = false;

            foreach (var modDirNameValue in modDirNamesValues)
            {
                try
                {
                    var targetDir = new DirectoryInfo(Path.Combine(ManageSettings.CurrentGameModsDirPath, modDirNameValue.Key));
                    if (!targetDir.Exists) continue;

                    var metainiPath = Path.Combine(targetDir.FullName, "meta.ini");

                    var ini = ManageIni.GetINIFile(metainiPath);

                    var currentValue = ini.GetKey("General", "notes");
                    if (currentValue != "\"" + modDirNameValue.Value + "\"")
                    {
                        ini.SetKey("General", "notes", "\"" + modDirNameValue.Value + "\"");
                        ini.WriteFile();
                        ret = true;
                    }
                }
                catch (Exception ex)
                {
                    _log.Warn("Failed to apply notes default value. Mod name:" + modDirNameValue.Key + "\r\nError:" + ex);
                }
            }

            return ret;
        }

        private bool ProceedCreateFiles()
        {
            return !string.IsNullOrWhiteSpace(_updateInfo.CreateFiles) && ProceedCreateRemove(files: true, create: true);
        }

        private bool ProceedCreateDirs()
        {
            return !string.IsNullOrWhiteSpace(_updateInfo.CreateDirs) && ProceedCreateRemove(files: false, create: true);
        }

        /// <summary>
        /// Creates or removes files or dirs which are listed in the corresponding update ini key
        /// </summary>
        /// <param name="files">True when files must be processed else dirs</param>
        /// <param name="create">True when entries must be created else removed</param>
        /// <returns>True when at least one entry was created or removed</returns>
        private bool ProceedCreateRemove(bool files = true, bool create = true)
        {
            var anyProcessed = false;
            string gameDir = ManageSettings.CurrentGameDirPath;

            string value = create
                ? (files ? _updateInfo.CreateFiles : _updateInfo.CreateDirs)
                : (files ? _updateInfo.RemoveFiles : _updateInfo.RemoveDirs);

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            if (create && files)
            {
                value = HideContentCommas(value);
            }

            foreach (var subPath in SplitSubPaths(value))
            {
                try
                {
                    var targetPath = gameDir + Path.DirectorySeparatorChar + subPath;
                    if (create)
                    {
                        if (ProceedCreate(targetPath, files)) anyProcessed = true;
                    }
                    else
                    {
                        if (ProceedRemove(targetPath, subPath, files)) anyProcessed = true;
                    }
                }
                catch (Exception ex)
                {
                    _log.Warn("Failed to " + (create ? "create" : "remove") + " " + (files ? "file" : "dir") + ". SubPath:" + subPath + "\r\nError:" + ex);
                }
            }

            return anyProcessed;
        }

        /// <summary>
        /// Markers of the created file content in the CreateFiles update ini key
        /// </summary>
        private const string CreateFileContentBeginMarker = "|begin ";

        private const string CreateFileContentEndMarker = " end|";

        /// <summary>
        /// Placeholder of the commas which are placed inside the created file content
        /// </summary>
        private const string ContentCommasHiddenString = "%comma%";

        /// <summary>
        /// hide commas in creating file's content because comma using as splitter
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string HideContentCommas(string value)
        {
            MatchCollection contents = Regex.Matches(value, @"\|begin ([^\|]+) end\|");
            for (int i = contents.Count - 1; i >= 0; i--)
            {
                value = value
                    .Remove(contents[i].Index, contents[i].Length)
                    .Insert(contents[i].Index, contents[i].Value.Replace(",", ContentCommasHiddenString));
            }

            return value;
        }

        /// <summary>
        /// Creates the file or dir by the target path.
        /// Path of the file can contain the file content placed between the begin and end markers.
        /// </summary>
        /// <param name="targetPath">Path of the created entry with an optional file content</param>
        /// <param name="files">True when file must be created else dir</param>
        /// <returns>True when entry was created</returns>
        private static bool ProceedCreate(string targetPath, bool files = true)
        {
            string content = string.Empty;
            if (files)
            {
                // has content
                int contentBeginIndex = targetPath.IndexOf(CreateFileContentBeginMarker, StringComparison.Ordinal);
                if (contentBeginIndex != -1)
                {
                    var startIndex = contentBeginIndex + CreateFileContentBeginMarker.Length;
                    var contentLength = targetPath.IndexOf(CreateFileContentEndMarker, StringComparison.Ordinal) - startIndex;
                    content = UnhideCommasAndSpecTextReplace(targetPath.Substring(startIndex, contentLength)); // file's content with unhidden commas
                    targetPath = targetPath.Remove(contentBeginIndex); // file's path
                }
            }

            if (files ? File.Exists(targetPath) : Directory.Exists(targetPath))
            {
                return false;
            }

            if (files)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                File.WriteAllText(targetPath, content.Replace("\\r\\n", "\r\n").Replace("\\n", "\n").Replace("\\r", "\r"));
            }
            else
            {
                Directory.CreateDirectory(targetPath);
            }

            return true;
        }

        /// <summary>
        /// Unhide commas and replace some predefined text variables with actual info text
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string UnhideCommasAndSpecTextReplace(string str)
        {
            return str
                .Replace("%sortmarker%", T._("The marker shows where to sort new zipmods for this sideloader modpack"))
                .Replace(ContentCommasHiddenString, ",")
                ;
        }

        /// <summary>
        /// Splits comma separated list of sub paths and skips empty records
        /// </summary>
        /// <param name="value">Comma separated list of sub paths</param>
        private static IEnumerable<string> SplitSubPaths(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? Enumerable.Empty<string>()
                : value.Split(SubPathSeparators, StringSplitOptions.RemoveEmptyEntries);
        }

        private void SetSkipLists()
        {
            _skipExistDirs = SplitSubPaths(_updateInfo.SkipExistDirs).ToHashSet();
            _skipExistFiles = SplitSubPaths(_updateInfo.SkipExistFiles).ToHashSet();
        }

        private bool ProceedRemoveUnusedSeparators()
        {
            if (_updateInfo.RemoveUnusedSeparators != "true")
            {
                return false;
            }

            var modlist = new ModlistData();
            var bakModsDirPath = Path.Combine(_bakDir, "Mods");

            var anyRemoved = false;
            var modsByName = modlist.Mods.ToDictionary(mod => mod.Name, mod => mod);
            foreach (var separator in new DirectoryInfo(ManageSettings.CurrentGameModsDirPath)
                .EnumerateDirectories("*_separator"))
            {
                if (modsByName.ContainsKey(separator.Name)) continue;

                var bakPath = separator.FullName.Replace(ManageSettings.CurrentGameModsDirPath, bakModsDirPath);
                try
                {
                    separator.MoveTo(bakPath);
                    anyRemoved = true;
                }
                catch (Exception ex)
                {
                    _log.Warn("Failed to remove unusing separator. path:" + separator.FullName + "\r\nError:" + ex);
                }
            }

            return anyRemoved;
        }

        private bool ProceedRemoveFiles()
        {
            return !string.IsNullOrWhiteSpace(_updateInfo.RemoveFiles) && ProceedCreateRemove(files: true, create: false);
        }

        private bool ProceedRemoveDirs()
        {
            return !string.IsNullOrWhiteSpace(_updateInfo.RemoveDirs) && ProceedCreateRemove(files: false, create: false);
        }

        /// <summary>
        /// Moves the game entry to the backup dir when the entry exists
        /// </summary>
        /// <param name="targetPath">Path of the entry in the game dir</param>
        /// <param name="subPath">Sub path of the entry relative to the game dir</param>
        /// <param name="files">True when file must be removed else dir</param>
        /// <returns>True when entry was moved to the backup dir</returns>
        private bool ProceedRemove(string targetPath, string subPath, bool files = true)
        {
            var backupPath = _bakDir + Path.DirectorySeparatorChar + subPath;
            if (!(files ? File.Exists(targetPath) : Directory.Exists(targetPath)))
            {
                return false;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(backupPath));
            MoveEntry(targetPath, backupPath, !files);

            return true;
        }

        private bool ProceedUpdateMO()
        {
            return _updateInfo.UpdateMO == "true" && ProceedUpdateFiles("MO");
        }

        private bool ProceedUpdateData()
        {
            return _updateInfo.UpdateData == "true" && ProceedUpdateFiles("Data");
        }

        /// <summary>
        /// Replaces files of the selected subdir of the game by the newer files of the update
        /// </summary>
        /// <param name="targetDirName">Name of the game subdir which files must be updated</param>
        /// <returns>True when at least one file was processed</returns>
        private bool ProceedUpdateFiles(string targetDirName)
        {
            var updateGameDir = _isRoot ? Path.Combine(_dir.FullName, "Games", _updateInfo.GameFolderName) : _dir.FullName;
            var updateTargetSubDir = Path.Combine(updateGameDir, targetDirName);

            if (!Directory.Exists(updateTargetSubDir))
            {
                return false;
            }

            var anyUpdated = false;

            var updateTargetBackupSubDir = Path.Combine(_bakDir, targetDirName);
            if (!Directory.Exists(updateTargetBackupSubDir))
            {
                Directory.CreateDirectory(updateTargetBackupSubDir);
            }

            // replace files by newer
            foreach (var updateFileInfo in new DirectoryInfo(updateTargetSubDir).EnumerateFiles("*", SearchOption.AllDirectories))
            {
                try
                {
                    // get subpath
                    var targetFileSubPath = updateFileInfo.FullName.Replace(updateGameDir + Path.DirectorySeparatorChar, string.Empty);
                    if (_skipExistFiles.Contains(targetFileSubPath))
                    {
                        // skip if in skip list
                        continue;
                    }

                    var targetFileInfo = new FileInfo(ManageSettings.CurrentGameDirPath + Path.DirectorySeparatorChar + targetFileSubPath);

                    var targetPath = targetFileInfo.FullName;// default target path in game's target subdir

                    bool moveIt = true;
                    if (targetFileInfo.Exists)
                    {
                        var fileBakPath = updateFileInfo.FullName.Replace(updateGameDir, _bakDir);// file's path in bak dir

                        // remove game file to bak if:
                        if ((_writeTimeFiles && updateFileInfo.LastWriteTime > targetFileInfo.LastWriteTime) // update file is newer
                            || (_sizeFiles && updateFileInfo.Length != targetFileInfo.Length) // update file has different size
                            || (_crcFiles && updateFileInfo.GetCrc32() != targetFileInfo.GetCrc32())) // update file has different crc32
                        {
                            // just move current file in buckup because lastwrite time can be newer in actually older file and it will not be replaced by updated
                            Directory.CreateDirectory(Path.GetDirectoryName(fileBakPath));// create parent dir
                            targetFileInfo.MoveTo(fileBakPath); // move exist older target file to bak dir
                        }
                        else
                        {
                            moveIt = false; // dont move update file because target was not removed
                        }
                    }

                    if (moveIt)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(targetPath));// create parent dir
                        updateFileInfo.MoveTo(targetPath); // move update file to target if it is newer or to bak dir if older
                    }

                    anyUpdated = true;
                }
                catch (Exception ex)
                {
                    _log.Warn("Failed to update file. path:" + updateFileInfo.FullName + "\r\nError:" + ex);
                }
            }

            return anyUpdated;
        }

        private bool ProceedUpdateMods()
        {
            return _updateInfo.UpdateMods == "true" && ProceedUpdateDirs("Mods");
        }

        /// <summary>
        /// Replaces dirs of the selected subdir of the game by the newer dirs of the update
        /// </summary>
        /// <param name="dirName">Name of the game subdir which dirs must be updated</param>
        /// <returns>True when at least one dir was processed</returns>
        private bool ProceedUpdateDirs(string dirName)
        {
            var updateGameDir = _isRoot ? Path.Combine(_dir.FullName, "Games", _updateInfo.GameFolderName) : _dir.FullName;
            var updateTargetDir = new DirectoryInfo(Path.Combine(updateGameDir, dirName));

            if (!updateTargetDir.Exists)
            {
                return false;
            }

            var anyUpdated = false;

            // create mods buckup dir
            var targetDirBackupPath = Path.Combine(_bakDir, dirName);
            if (!Directory.Exists(targetDirBackupPath))
            {
                Directory.CreateDirectory(targetDirBackupPath);
            }

            // replace folders by updated
            foreach (var updateSubDir in updateTargetDir.EnumerateDirectories())
            {
                try
                {
                    // get subpath
                    var targetDirSubPath = updateSubDir.FullName.Replace(updateGameDir + Path.DirectorySeparatorChar, string.Empty);
                    if (_skipExistDirs.Contains(targetDirSubPath))
                    {
                        // skip if in skip list
                        continue;
                    }

                    var targetGameSubDir = new DirectoryInfo(ManageSettings.CurrentGameDirPath + Path.DirectorySeparatorChar + targetDirSubPath);

                    if (targetGameSubDir.Exists)
                    {
                        var existsTargetGameSubDir = targetGameSubDir.GetCaseSensitive();
                        if (!IsNewer(updateSubDir.FullName, existsTargetGameSubDir.FullName) // is older
                            && updateSubDir.Name == existsTargetGameSubDir.Name // dir name has exactly same name with same chars case
                            )
                        {
                            // when update dir older just move it in bak and continue
                            //updateModDir.MoveTo(updateModDir.FullName.Replace(updateModsDirPath.FullName, modsBuckupDir));
                            continue;
                        }

                        // move exist older target dir to bak
                        var existsTargetGameModDirBak = existsTargetGameSubDir.FullName.Replace(ManageSettings.CurrentGameModsDirPath, targetDirBackupPath);
                        existsTargetGameSubDir.MoveTo(existsTargetGameModDirBak);
                    }

                    // move new update dir to target
                    updateSubDir.MoveTo(targetGameSubDir.FullName);

                    anyUpdated = true;
                }
                catch (Exception ex)
                {
                    _log.Warn("Failed to update dir. path:" + updateSubDir.FullName + "\r\nError:" + ex);
                }

            }

            return anyUpdated;
        }

        /// <summary>
        /// Compares versions from the meta.ini of the update mod dir and the game mod dir
        /// </summary>
        /// <param name="modDir">Dir of the mod in the update</param>
        /// <param name="currentGameModDir">Dir of the mod in the game</param>
        /// <returns>True when version of the update mod is newer or cannot be compared</returns>
        private static bool IsNewer(string modDir, string currentGameModDir)
        {
            var metaIniUpdatePath = Path.Combine(modDir, "meta.ini");
            var metaIniGamePath = Path.Combine(currentGameModDir, "meta.ini");
            if (!File.Exists(metaIniUpdatePath) || !File.Exists(metaIniGamePath))
            {
                // cannot compare version, is never by default
                return true;
            }

            var updateVersion = ManageIni.GetINIFile(metaIniUpdatePath, false).GetKey("General", "version");
            var gameVersion = ManageIni.GetINIFile(metaIniGamePath, false).GetKey("General", "version");

            return updateVersion.IsNewerOf(gameVersion);
        }
    }
}
