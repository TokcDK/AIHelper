using AIHelper.Manage;
using AIHelper.Manage.Update;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static AIHelper.Manage.ManageModOrganizer;

namespace AIHelper.Install.Types.Directories
{
    class GameUpdateInstaller : DirectoriesInstallerBase
    {
        static Logger _log = LogManager.GetCurrentClassLogger();
        public override int Order => base.Order / 5;

        DirectoryInfo _dir;
        GameUpdateInfo updateInfo;
        bool IsRoot;
        string _dateTimeSuffix;
        string _backupsDir;
        string _bakDir;

        static string _gameUpdateInfoFileName { get => ManageSettings.GameUpdateInstallerIniFileName; }
        HashSet<string> _skipExistDirs;
        HashSet<string> _skipExistFiles;
        private bool crcfiles;
        private bool sizeFiles;
        private bool writetimefiles;

        protected override bool Get(DirectoryInfo dir)
        {
            _dir = dir;
            var updateInfoIniFilePath = Path.Combine(dir.FullName, _gameUpdateInfoFileName);
            if (!File.Exists(updateInfoIniFilePath))
            {
                // not update
                return false;
            }

            updateInfo = new GameUpdateInfo(updateInfoIniFilePath);

            if (string.IsNullOrWhiteSpace(updateInfo.GameFolderName))
            {
                // empty game folder name
                return false;
            }

            if (ManageSettings.CurrentGameDirName != updateInfo.GameFolderName)
            {
                // incorrect game
                MessageBox.Show(updateInfo.GameFolderName + ": " + T._("Incorrect current game"));
                return false;
            }

            bool updated = false;
            crcfiles = updateInfo.CRCFiles == "true";
            sizeFiles = updateInfo.SizeFiles == "true";
            writetimefiles = updateInfo.WriteTimeFiles == "true";

            IsRoot = updateInfo.IsRoot == "true";

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
            if (ProceedRenameDirs())
            {
                updated = true;
            }

            // proceed update actions
            if (ProceedRenameFiles())
            {
                updated = true;
            }

            // proceed update actions
            if (ProceedRemoveDirs())
            {
                updated = true;
            }

            // proceed update actions
            if (ProceedRemoveFiles())
            {
                updated = true;
            }

            // proceed update actions
            if (ProceedUpdateMods())
            {
                updated = true;
            }
            // proceed update actions
            if (ProceedUpdateData())
            {
                updated = true;
            }

            // proceed update actions
            if (ProceedUpdateMO())
            {
                updated = true;
            }

            // proceed update actions
            if (ProceedCreateDirs())
            {
                updated = true;
            }

            // proceed update actions
            if (ProceedCreateFiles())
            {
                updated = true;
            }

            // proceed update actions
            if (ProceedRemoveUnusedSeparators())
            {
                updated = true;
            }

            // proceed update actions
            if (ProceedFixes())
            {
                updated = true;
            }

            // clean update info and folder
            //File.Delete(Path.Combine(dir.FullName, _gameUpdateInfoFileName));
            //ManageFilesFolders.DeleteEmptySubfolders(dirPath: dir.FullName, deleteThisDir: true);
            // move dir to bak instead of delete
            dir.MoveTo(Path.Combine(_bakDir, dir.Name) + _dateTimeSuffix);

            if (updated)
            {
                MessageBox.Show(updateInfo.GameFolderName + ": " + T._("Update applied.\n\nWill be opened Backup folder.\nCheck content and remove files and dirs which not need for you animore."));
                Process.Start(_bakDir);
            }
            else
            {
                MessageBox.Show(updateInfo.GameFolderName + ": " + T._("Update not required."));
            }

            return updated;
        }

        private bool ProceedRenameFiles()
        {
            string gameDir = ManageSettings.CurrentGameDirPath;
            string value = updateInfo.RenameFiles;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var ret = false;

            foreach (var data in value.Split(new[] { ", ", "," }, StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    var renData = data.Split(':');
                    if (renData.Length != 2)
                    {
                        continue;
                    }

                    var subPath = renData[0];
                    var renName = renData[1];

                    var targetPath = gameDir + Path.DirectorySeparatorChar + subPath;

                    if (!File.Exists(targetPath))
                    {
                        continue;
                    }

                    ret = true;

                    var newPath = Path.Combine(Path.GetDirectoryName(targetPath), renName);
                    if (File.Exists(newPath))
                    {
                        // move to bak if exists new path
                        File.Move(targetPath, _bakDir + Path.DirectorySeparatorChar + subPath);
                    }
                    else
                    {
                        // rename dir to new
                        File.Move(targetPath, newPath);
                    }
                }
                catch { }
            }

            return ret;
        }

        private bool ProceedRenameDirs()
        {
            string gameDir = ManageSettings.CurrentGameDirPath;
            string value = updateInfo.RenameDirs;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var ret = false;

            foreach (var data in value.Split(new[] { ", ", "," }, StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    var renData = data.Split(':');
                    if (renData.Length != 2)
                    {
                        continue;
                    }

                    var subPath = renData[0];
                    var renName = renData[1];

                    var targetPath = gameDir + Path.DirectorySeparatorChar + subPath;

                    if (!Directory.Exists(targetPath))
                    {
                        continue;
                    }

                    ret = true;

                    var newPath = Path.Combine(Path.GetDirectoryName(targetPath), renName);
                    if (Directory.Exists(newPath))
                    {
                        // move to bak if exists new path
                        Directory.Move(targetPath, _bakDir + Path.DirectorySeparatorChar + subPath);
                    }
                    else
                    {
                        // rename dir to new
                        Directory.Move(targetPath, newPath);
                    }
                }
                catch { }
            }

            return ret;
        }

        private bool ProceedFixes()
        {
            return updateInfo.ApplyFixes == "true" && FixIniValues();
        }

        private static bool FixIniValues()
        {
            return MetaIniNotesTweaks();
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
                    if (!targetDir.Exists)
                    {
                        continue;
                    }

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
                    _log.Warn("Failed to apply notes default value. Mod name:" + modDirNameValue.Value + "\r\nError:" + ex);
                }
            }

            return ret;
        }

        private bool ProceedCreateFiles()
        {
            return !string.IsNullOrWhiteSpace(updateInfo.CreateFiles) && ProceedCreateRemove(files: true, create: true);
        }

        private bool ProceedCreateDirs()
        {
            return !string.IsNullOrWhiteSpace(updateInfo.CreateDirs) && ProceedCreateRemove(files: false, create: true);
        }

        private bool ProceedCreateRemove(bool files = true, bool create = true)
        {
            var ret = false;
            string gameDir = ManageSettings.CurrentGameDirPath;
            string value;
            if (create)
            {
                value = (files ? updateInfo.CreateFiles : updateInfo.CreateDirs);
                if (files)
                {
                    value = HideContentCommas(value);
                }
            }
            else
            {
                value = (files ? updateInfo.RemoveFiles : updateInfo.RemoveDirs);
            }

            foreach (var subPath in value.Split(new[] { ", ", "," }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (string.IsNullOrWhiteSpace(subPath))
                {
                    continue;
                }

                try
                {
                    var targetPath = gameDir + Path.DirectorySeparatorChar + subPath;
                    if (create)
                    {
                        if (ProceedCreate(targetPath, files))
                        {
                            ret = true;
                        }
                    }
                    else
                    {
                        if (ProceedRemove(targetPath, subPath, files))
                        {
                            ret = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Warn("Failed to " + (create ? "create" : "remove") + " " + (files ? "file" : "dir") + ". SubPath:" + subPath + "\r\nError:" + ex);
                }
            }

            return ret;
        }

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
                ;
            }

            return value;
        }

        static string CreateFileContendBeginMarker { get => "|begin "; }
        static string CreateFileContendEndMarker { get => " end|"; }
        static string ContentCommasHiddenString { get => "%comma%"; }

        private static bool ProceedCreate(string targetPath, bool files = true)
        {
            string content = "";
            if (files)
            {
                // has content
                int contentBeginIndex = targetPath.IndexOf(CreateFileContendBeginMarker);
                if (contentBeginIndex != -1)
                {
                    var startIndex = contentBeginIndex + CreateFileContendBeginMarker.Length;
                    var contentLength = targetPath.IndexOf(CreateFileContendEndMarker) - startIndex;
                    content = UnhideCommasAndSpecTextReplace(targetPath.Substring(startIndex, contentLength)); // file's content with unhidden commas
                    targetPath = targetPath.Remove(contentBeginIndex); // file's path
                }
            }

            if (targetPath.Exists(!files) /*&& (!files || (content.Length == 0 || File.ReadAllText(targetPath) == content))*/)
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

        private void SetSkipLists()
        {
            if (!string.IsNullOrWhiteSpace(updateInfo.SkipExistDirs))
            {
                _skipExistDirs = updateInfo.SkipExistDirs.Split(new[] { ", ", "," }, StringSplitOptions.RemoveEmptyEntries).ToHashSet();
            }
            else
            {
                _skipExistDirs = new HashSet<string>();
            }
            if (!string.IsNullOrWhiteSpace(updateInfo.SkipExistFiles))
            {
                _skipExistFiles = updateInfo.SkipExistFiles.Split(new[] { ", ", "," }, StringSplitOptions.RemoveEmptyEntries).ToHashSet();
            }
            else
            {
                _skipExistFiles = new HashSet<string>();
            }
        }

        private bool ProceedRemoveUnusedSeparators()
        {
            if (updateInfo.RemoveUnusedSeparators != "true")
            {
                return false;
            }

            var modlist = new ModlistProfileInfo();
            var bakModsDirPath = Path.Combine(_bakDir, "Mods");

            var ret = false;
            foreach (var separator in new DirectoryInfo(ManageSettings.CurrentGameModsDirPath).GetDirectories("*_separator"))
            {
                if (!modlist.ItemByName.ContainsKey(separator.Name))
                {
                    var bakPath = separator.FullName.Replace(ManageSettings.CurrentGameModsDirPath, bakModsDirPath);
                    try
                    {
                        separator.MoveTo(bakPath);
                        ret = true;
                    }
                    catch (Exception ex)
                    {
                        _log.Warn("Failed to remove unusing separator. path:" + separator.FullName + "\r\nError:" + ex);
                    }
                }
            }

            return ret;
        }

        private bool ProceedRemoveFiles()
        {
            return !string.IsNullOrWhiteSpace(updateInfo.RemoveFiles) && ProceedCreateRemove(files: true, create: false);
        }

        private bool ProceedRemoveDirs()
        {
            return !string.IsNullOrWhiteSpace(updateInfo.RemoveDirs) && ProceedCreateRemove(files: false, create: false);
        }

        private bool ProceedRemove(string targetPath, string subPath, bool files = true)
        {
            var ret = false;

            var buckupPath = _bakDir + Path.DirectorySeparatorChar + subPath;
            if ((files && File.Exists(targetPath)) || (!files && Directory.Exists(targetPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(buckupPath));
                if (files)
                {
                    ret = true;
                    File.Move(targetPath, buckupPath);
                }
                else
                {
                    ret = true;
                    Directory.Move(targetPath, buckupPath);
                }
            }

            return ret;
        }

        private bool ProceedUpdateMO()
        {
            return updateInfo.UpdateMO == "true" && ProceedUpdateFiles("MO");
        }

        private bool ProceedUpdateData()
        {
            return updateInfo.UpdateData == "true" && ProceedUpdateFiles("Data");
        }

        private bool ProceedUpdateFiles(string targetDirName)
        {
            var updateGameDir = IsRoot ? Path.Combine(_dir.FullName, "Games", updateInfo.GameFolderName) : Path.Combine(_dir.FullName);
            var updateTargetSubDir = Path.Combine(updateGameDir, targetDirName);

            if (!Directory.Exists(updateTargetSubDir))
            {
                return false;
            }

            var ret = false;

            var updateTargetBuckupSubDir = Path.Combine(_bakDir, targetDirName);
            if (!Directory.Exists(updateTargetBuckupSubDir))
            {
                Directory.CreateDirectory(updateTargetBuckupSubDir);
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

                    bool MoveIt = true;
                    if (targetFileInfo.Exists)
                    {
                        var fileBakBath = updateFileInfo.FullName.Replace(updateGameDir, _bakDir);// file's path in bak dir

                        // remove game file to bak if:
                        if ((writetimefiles && updateFileInfo.LastWriteTime > targetFileInfo.LastWriteTime) // update file is newer
                            || (sizeFiles && updateFileInfo.Length != targetFileInfo.Length) // update file has different size
                            || (crcfiles && updateFileInfo.GetCrc32() != targetFileInfo.GetCrc32())) // update file has different crc32
                        {
                            // just move current file in buckup because lastwrite time can be newer in actually older file and it will not be replaced by updated
                            Directory.CreateDirectory(Path.GetDirectoryName(fileBakBath));// create parent dir
                            targetFileInfo.MoveTo(fileBakBath); // move exist older target file to bak dir
                        }
                        else
                        {
                            MoveIt = false; // dont move update file because target was not removed
                        }
                    }

                    if (MoveIt)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(targetPath));// create parent dir
                        updateFileInfo.MoveTo(targetPath); // move update file to target if it is newer or to bak dir if older
                    }

                    ret = true;
                }
                catch (Exception ex)
                {
                    _log.Warn("Failed to update file. path:" + updateFileInfo.FullName + "\r\nError:" + ex);
                }
            }

            return ret;
        }

        private bool ProceedUpdateMods()
        {
            return updateInfo.UpdateMods == "true" && ProceedUpdateDirs("Mods");
        }

        private bool ProceedUpdateDirs(string dirName)
        {
            var updateGameDir = IsRoot ? Path.Combine(_dir.FullName, "Games", updateInfo.GameFolderName) : _dir.FullName;
            var updateTargetDir = new DirectoryInfo(Path.Combine(updateGameDir, dirName));

            if (!updateTargetDir.Exists)
            {
                return false;
            }

            var ret = false;

            // create mods buckup dir
            var targetDirBuckupPath = Path.Combine(_bakDir, dirName);
            if (!Directory.Exists(targetDirBuckupPath))
            {
                Directory.CreateDirectory(targetDirBuckupPath);
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
                        var existsTargetGameModDirBak = existsTargetGameSubDir.FullName.Replace(ManageSettings.CurrentGameModsDirPath, targetDirBuckupPath);
                        existsTargetGameSubDir.MoveTo(existsTargetGameModDirBak);
                    }

                    // move new update dir to target
                    updateSubDir.MoveTo(targetGameSubDir.FullName);

                    ret = true;
                }
                catch (Exception ex)
                {
                    _log.Warn("Failed to update dir. path:" + updateSubDir.FullName + "\r\nError:" + ex);
                }

            }

            return ret;
        }

        private static bool IsNewer(string modDir, string currentGameModDir)
        {
            var metaIniUpdatePath = Path.Combine(modDir, "meta.ini");
            var metaIniGamePath = Path.Combine(currentGameModDir, "meta.ini");
            if (!File.Exists(metaIniUpdatePath) || !File.Exists(metaIniGamePath))
            {
                // cannot compare version, is never by default
                return true;
            }

            var UpdateVersion = ManageIni.GetINIFile(metaIniUpdatePath, false).GetKey("General", "version");
            var GameVersion = ManageIni.GetINIFile(metaIniGamePath, false).GetKey("General", "version");

            return UpdateVersion.IsNewerOf(GameVersion);
        }
    }

    class GameUpdateInfo
    {
        internal readonly Dictionary<string, string> Keys = new Dictionary<string, string>()
        {
            { nameof(GameFolderName), string.Empty },
            { nameof(UpdateData), "false" },
            { nameof(UpdateMods), "false" },
            { nameof(IsRoot), "false" },
            { nameof(UpdateMO), "false" },
            { nameof(RenameFiles), string.Empty },
            { nameof(RenameDirs), string.Empty },
            { nameof(RemoveFiles), string.Empty },
            { nameof(RemoveDirs), string.Empty },
            { nameof(RemoveUnusedSeparators), "false" },
            { nameof(SkipExistDirs), string.Empty },
            { nameof(SkipExistFiles), string.Empty },
            { nameof(CreateDirs), string.Empty },
            { nameof(CreateFiles), string.Empty },
            { nameof(ApplyFixes), "false" },
            { nameof(CRCFiles), "false" },
            { nameof(SizeFiles), "true" },
            { nameof(WriteTimeFiles), "true" },
        };

        /// <summary>
        /// Name of game's folder name which need to update
        /// </summary>
        internal string GameFolderName { get => Keys[nameof(GameFolderName)]; set => Keys[nameof(GameFolderName)] = value; }
        /// <summary>
        /// Update Data folder
        /// </summary>
        internal string UpdateData { get => Keys[nameof(UpdateData)]; set => Keys[nameof(UpdateData)] = value; }
        /// <summary>
        /// Update mod dirs in Mods folder
        /// </summary>
        internal string UpdateMods { get => Keys[nameof(UpdateMods)]; set => Keys[nameof(UpdateMods)] = value; }
        /// <summary>
        /// Update files in MO dir of the game to newer
        /// </summary>
        internal string UpdateMO { get => Keys[nameof(UpdateMO)]; set => Keys[nameof(UpdateMO)] = value; }
        /// <summary>
        /// Root means aihelper root dir as update dir root
        /// </summary>
        internal string IsRoot { get => Keys[nameof(IsRoot)]; set => Keys[nameof(IsRoot)] = value; }
        /// <summary>
        /// List of files to remove
        /// </summary>
        public string RenameFiles { get => Keys[nameof(RenameFiles)]; set => Keys[nameof(RenameFiles)] = value; }
        /// <summary>
        /// List of dirs to remove
        /// </summary>
        public string RenameDirs { get => Keys[nameof(RenameDirs)]; set => Keys[nameof(RenameDirs)] = value; }
        /// <summary>
        /// List of files to remove
        /// </summary>
        public string RemoveFiles { get => Keys[nameof(RemoveFiles)]; set => Keys[nameof(RemoveFiles)] = value; }
        /// <summary>
        /// List of dirs to remove
        /// </summary>
        public string RemoveDirs { get => Keys[nameof(RemoveDirs)]; set => Keys[nameof(RemoveDirs)] = value; }
        /// <summary>
        /// Determines if need to remove unused separators which is not updated modlist
        /// </summary>
        public string RemoveUnusedSeparators { get => Keys[nameof(RemoveUnusedSeparators)]; set => Keys[nameof(RemoveUnusedSeparators)] = value; }
        /// <summary>
        /// List of subpaths for dirs which must be skipped and not touched if already exist in current game
        /// </summary>
        public string SkipExistDirs { get => Keys[nameof(SkipExistDirs)]; set => Keys[nameof(SkipExistDirs)] = value; }
        /// <summary>
        /// List of subpaths for files which must be skipped and not touched if already exist in current game
        /// </summary>
        public string SkipExistFiles { get => Keys[nameof(SkipExistFiles)]; set => Keys[nameof(SkipExistFiles)] = value; }

        /// <summary>
        /// Dirs which must be created after update
        /// </summary>
        public string CreateDirs { get => Keys[nameof(CreateDirs)]; set => Keys[nameof(CreateDirs)] = value; }
        /// <summary>
        /// Files which must be created after update
        /// </summary>
        public string CreateFiles { get => Keys[nameof(CreateFiles)]; set => Keys[nameof(CreateFiles)] = value; }

        /// <summary>
        /// various game fixes and tweaks
        /// </summary>
        public string ApplyFixes { get => Keys[nameof(ApplyFixes)]; set => Keys[nameof(ApplyFixes)] = value; }
        /// <summary>
        /// check crc32 while check if file need to update. (slow)
        /// </summary>
        public string CRCFiles { get => Keys[nameof(CRCFiles)]; set => Keys[nameof(CRCFiles)] = value; }
        /// <summary>
        /// check size while check if file need to update
        /// </summary>
        public string SizeFiles { get => Keys[nameof(SizeFiles)]; set => Keys[nameof(SizeFiles)] = value; }
        /// <summary>
        /// check last write time while check if file need to update
        /// </summary>
        public string WriteTimeFiles { get => Keys[nameof(SizeFiles)]; set => Keys[nameof(SizeFiles)] = value; }

        public GameUpdateInfo(string updateInfoPath = "")
        {
            if (updateInfoPath.Length > 0)
            {
                Get(updateInfoPath);
            }
        }

        void Get(string updateInfoPath)
        {
            var updateInfoIni = ManageIni.GetINIFile(updateInfoPath);

            var keys = Keys.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                var keyName = keys[i];
                if (updateInfoIni.KeyExists(keyName))
                {
                    Keys[keyName] = updateInfoIni.GetKey("", keyName);
                }
            }
        }
    }
}
