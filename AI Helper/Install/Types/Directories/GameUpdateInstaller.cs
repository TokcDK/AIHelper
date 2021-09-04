using AIHelper.Manage;
using AIHelper.Manage.Update;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static AIHelper.Manage.ManageModOrganizerMods;

namespace AIHelper.Install.Types.Directories
{
    class GameUpdateInstaller : DirectoriesInstallerBase
    {
        public override int Order => base.Order / 5;

        DirectoryInfo _dir;
        GameUpdateInfo updateInfo;
        bool IsRoot;
        string _dateTimeSuffix;
        string _backupsDir;
        string _bakDir;
        string _gameUpdateInfoFileName { get => "gameupdate.ini"; }
        HashSet<string> _skipExistDirs;
        HashSet<string> _skipExistFiles;

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

            if (ManageSettings.GetCurrentGameFolderName() != updateInfo.GameFolderName)
            {
                // incorrect game
                MessageBox.Show(updateInfo.GameFolderName + ": " + T._("Incorrect current game"));
                return false;
            }

            bool updated = false;

            IsRoot = updateInfo.IsRoot == "true";

            _dateTimeSuffix = ManageSettings.GetDateTimeBasedSuffix();

            SetSkipLists();

            _backupsDir = Path.Combine(ManageSettings.GetCurrentGameDirPath(), "Buckups");
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
            if (ProceedRemoveUnusedSeparators())
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

        private bool ProceedFixes()
        {
            return FixIniValues();
        }

        private bool FixIniValues()
        {
            return MetaIniNotesTweaks();
        }

        private bool MetaIniNotesTweaks()
        {
            Dictionary<string, string> modDirNamesValues = new Dictionary<string, string>()
            {
                { ManageSettings.KKManagerFilesModName(), ManageSettings.KKManagerFilesNotes()},
                { "GameUserData", "<!DOCTYPE HTML PUBLIC \\\"-//W3C//DTD HTML 4.0//EN\\\" \\\"http://www.w3.org/TR/REC-html40/strict.dtd\\\">\\n<html><head><meta name=\\\"qrichtext\\\" content=\\\"1\\\" /><style type=\\\"text/css\\\">\\np, li { white-space: pre-wrap; }\\n</style></head><body style=\\\" font-family:'MS Shell Dlg 2'; font-size:8.25pt; font-weight:400; font-style:normal;\\\">\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">New files created by the game will be stored here. Saves, plugins configs and other.</p>\\n<p style=\\\"-qt-paragraph-type:empty; margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\"><br /></p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">\\x41d\\x43e\\x432\\x44b\\x435 \\x444\\x430\\x439\\x43b\\x44b \\x441\\x43e\\x437\\x434\\x430\\x43d\\x43d\\x44b\\x435 \\x438\\x433\\x440\\x43e\\x439 \\x431\\x443\\x434\\x443\\x442 \\x434\\x43e\\x431\\x430\\x432\\x43b\\x44f\\x442\\x44c\\x441\\x44f \\x441\\x44e\\x434\\x430. \\x421\\x43e\\x445\\x440\\x430\\x43d\\x435\\x43d\\x438\\x44f, \\x43d\\x430\\x441\\x442\\x440\\x43e\\x439\\x43a\\x438 \\x43f\\x43b\\x430\\x433\\x438\\x43d\\x43e\\x432 \\x438 \\x434\\x440.</p></body></html>"},
                { "Sideloader Modpack - Exclusive KKS", "<!DOCTYPE HTML PUBLIC \\\"-//W3C//DTD HTML 4.0//EN\\\" \\\"http://www.w3.org/TR/REC-html40/strict.dtd\\\">\\n<html><head><meta name=\\\"qrichtext\\\" content=\\\"1\\\" /><style type=\\\"text/css\\\">\\np, li { white-space: pre-wrap; }\\n</style></head><body style=\\\" font-family:'MS Shell Dlg 2'; font-size:8.25pt; font-weight:400; font-style:normal;\\\">\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">EN</p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">Koikatsu Sunshine exclusive mods.</p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">Warning! More mods more memory consumption.</p>\\n<p style=\\\"-qt-paragraph-type:empty; margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\"><br /></p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">required: Sideloader</p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">------------------------------------------------------------</p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">RU</p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">\\x42d\\x43a\\x441\\x43a\\x43b\\x44e\\x437\\x438\\x432\\x43d\\x44b\\x435 \\x43c\\x43e\\x434\\x44b \\x434\\x43b\\x44f Koikatsu Sunshine.</p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">\\x412\\x43d\\x438\\x43c\\x430\\x43d\\x438\\x435! \\x427\\x435\\x43c \\x431\\x43e\\x43b\\x44c\\x448\\x435 \\x43c\\x43e\\x434\\x43e\\x432, \\x442\\x435\\x43c \\x431\\x43e\\x43b\\x44c\\x448\\x435 \\x43f\\x43e\\x442\\x440\\x435\\x431\\x43b\\x435\\x43d\\x438\\x44f \\x43f\\x430\\x43c\\x44f\\x442\\x438.</p>\\n<p style=\\\"-qt-paragraph-type:empty; margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\"><br /></p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">\\x442\\x440\\x435\\x431\\x443\\x435\\x442\\x441\\x44f: Sideloader</p></body></html>"},
                { "Sideloader Modpack - KK_UncensorSelector", "<!DOCTYPE HTML PUBLIC \\\"-//W3C//DTD HTML 4.0//EN\\\" \\\"http://www.w3.org/TR/REC-html40/strict.dtd\\\">\\n<html><head><meta name=\\\"qrichtext\\\" content=\\\"1\\\" /><style type=\\\"text/css\\\">\\np, li { white-space: pre-wrap; }\\n</style></head><body style=\\\" font-family:'MS Shell Dlg 2'; font-size:8.25pt; font-weight:400; font-style:normal;\\\">\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">EN</p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">Koikatsu Sunshine mods for Uncensor Selector.</p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">Warning! More mods more memory consumption.</p>\\n<p style=\\\"-qt-paragraph-type:empty; margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\"><br /></p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">required: Sideloader and Uncensor Selector</p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">------------------------------------------------------------</p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">RU</p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">\\x42d\\x43a\\x441\\x43a\\x43b\\x44e\\x437\\x438\\x432\\x43d\\x44b\\x435 \\x43c\\x43e\\x434\\x44b \\x434\\x43b\\x44f Koikatsu Sunshine \\x434\\x43b\\x44f Uncensor Selector.</p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">\\x412\\x43d\\x438\\x43c\\x430\\x43d\\x438\\x435! \\x427\\x435\\x43c \\x431\\x43e\\x43b\\x44c\\x448\\x435 \\x43c\\x43e\\x434\\x43e\\x432, \\x442\\x435\\x43c \\x431\\x43e\\x43b\\x44c\\x448\\x435 \\x43f\\x43e\\x442\\x440\\x435\\x431\\x43b\\x435\\x43d\\x438\\x44f \\x43f\\x430\\x43c\\x44f\\x442\\x438.</p>\\n<p style=\\\"-qt-paragraph-type:empty; margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\"><br /></p>\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px; -qt-block-indent:0; text-indent:0px;\\\">\\x442\\x440\\x435\\x431\\x443\\x435\\x442\\x441\\x44f: Sideloader \\x438 Uncensor Selector</p></body></html>"},
            };

            var ret = false;

            foreach (var modDirNameValue in modDirNamesValues)
            {
                try
                {
                    var targetDir = new DirectoryInfo(Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), modDirNameValue.Key));
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
                    ManageLogs.Log("Failed to apply notes default value. Mod name:" + modDirNameValue.Value + "\r\nError:" + ex);
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
            string gameDir = ManageSettings.GetCurrentGameDirPath();
            string value;
            if (create)
            {
                value = (files ? updateInfo.CreateFiles : updateInfo.CreateDirs);
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
                    ManageLogs.Log("Failed to " + (create ? "create" : "remove") + " " + (files ? "file" : "dir") + ". SubPath:" + subPath + "\r\nError:" + ex);
                }
            }

            return ret;
        }

        private bool ProceedCreate(string targetPath, bool files = true)
        {
            var ret = false;

            if ((files && !File.Exists(targetPath)) || (!files && !Directory.Exists(targetPath)))
            {
                if (files)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                    File.WriteAllText(targetPath, "");
                    ret = true;
                }
                else
                {
                    Directory.CreateDirectory(targetPath);
                    ret = true;
                }
            }

            return ret;
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
            foreach (var separator in new DirectoryInfo(ManageSettings.GetCurrentGameModsDirPath()).GetDirectories("*_separator"))
            {
                if (!modlist.ItemByName.ContainsKey(separator.Name))
                {
                    var bakPath = separator.FullName.Replace(ManageSettings.GetCurrentGameModsDirPath(), bakModsDirPath);
                    try
                    {
                        separator.MoveTo(bakPath);
                        ret = true;
                    }
                    catch (Exception ex)
                    {
                        ManageLogs.Log("Failed to remove unusing separator. path:" + separator.FullName + "\r\nError:" + ex);
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

                    var targetFileInfo = new FileInfo(ManageSettings.GetCurrentGameDirPath() + Path.DirectorySeparatorChar + targetFileSubPath);

                    var targetPath = targetFileInfo.FullName;// default target path in game's target subdir

                    bool MoveIt = true;
                    if (targetFileInfo.Exists && (updateFileInfo.LastWriteTime > targetFileInfo.LastWriteTime || updateFileInfo.Length != targetFileInfo.Length))
                    {
                        var fileBakBath = updateFileInfo.FullName.Replace(updateGameDir, _bakDir);// file's path in bak dir

                        // just move current file in buckup because lastwrite time can be newer in actually older file and it will not be replaced by updated
                        Directory.CreateDirectory(Path.GetDirectoryName(fileBakBath));// create parent dir
                        targetFileInfo.MoveTo(fileBakBath); // move exist older target file to bak dir
                    }
                    else
                    {
                        MoveIt = false; // dont move update file because target was not removed
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
                    ManageLogs.Log("Failed to update file. path:" + updateFileInfo.FullName + "\r\nError:" + ex);
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

                    var targetGameSubDir = new DirectoryInfo(ManageSettings.GetCurrentGameDirPath() + Path.DirectorySeparatorChar + targetDirSubPath);

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
                        var existsTargetGameModDirBak = existsTargetGameSubDir.FullName.Replace(ManageSettings.GetCurrentGameModsDirPath(), targetDirBuckupPath);
                        existsTargetGameSubDir.MoveTo(existsTargetGameModDirBak);
                    }

                    // move new update dir to target
                    updateSubDir.MoveTo(targetGameSubDir.FullName);

                    ret = true;
                }
                catch (Exception ex)
                {
                    ManageLogs.Log("Failed to update dir. path:" + updateSubDir.FullName + "\r\nError:" + ex);
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

        Dictionary<string, string> _keys = new Dictionary<string, string>()
        {
            { nameof(GameFolderName), string.Empty },
            { nameof(UpdateData), "false" },
            { nameof(UpdateMods), "false" },
            { nameof(IsRoot), "false" },
            { nameof(UpdateMO), "false" },
            { nameof(RemoveFiles), string.Empty },
            { nameof(RemoveDirs), string.Empty },
            { nameof(RemoveUnusedSeparators), "true" },
            { nameof(SkipExistDirs), string.Empty },
            { nameof(SkipExistFiles), string.Empty },
            { nameof(CreateDirs), string.Empty },
            { nameof(CreateFiles), string.Empty },
        };

        /// <summary>
        /// Name of game's folder name which need to update
        /// </summary>
        internal string GameFolderName { get => _keys[nameof(GameFolderName)]; set => _keys[nameof(GameFolderName)] = value; }
        /// <summary>
        /// Update Data folder
        /// </summary>
        internal string UpdateData { get => _keys[nameof(UpdateData)]; set => _keys[nameof(UpdateData)] = value; }
        /// <summary>
        /// Update mod dirs in Mods folder
        /// </summary>
        internal string UpdateMods { get => _keys[nameof(UpdateMods)]; set => _keys[nameof(UpdateMods)] = value; }
        /// <summary>
        /// Update files in MO dir of the game to newer
        /// </summary>
        internal string UpdateMO { get => _keys[nameof(UpdateMO)]; set => _keys[nameof(UpdateMO)] = value; }
        /// <summary>
        /// Root means aihelper root dir as update dir root
        /// </summary>
        internal string IsRoot { get => _keys[nameof(IsRoot)]; set => _keys[nameof(IsRoot)] = value; }
        /// <summary>
        /// List of files to remove
        /// </summary>
        public string RemoveFiles { get => _keys[nameof(RemoveFiles)]; set => _keys[nameof(RemoveFiles)] = value; }
        /// <summary>
        /// List of dirs to remove
        /// </summary>
        public string RemoveDirs { get => _keys[nameof(RemoveDirs)]; set => _keys[nameof(RemoveDirs)] = value; }
        /// <summary>
        /// Determines if need to remove unused separators which is not updated modlist
        /// </summary>
        public string RemoveUnusedSeparators { get => _keys[nameof(RemoveUnusedSeparators)]; set => _keys[nameof(RemoveUnusedSeparators)] = value; }
        /// <summary>
        /// List of subpaths for dirs which must be skipped and not touched if already exist in current game
        /// </summary>
        public string SkipExistDirs { get => _keys[nameof(SkipExistDirs)]; set => _keys[nameof(SkipExistDirs)] = value; }
        /// <summary>
        /// List of subpaths for files which must be skipped and not touched if already exist in current game
        /// </summary>
        public string SkipExistFiles { get => _keys[nameof(SkipExistFiles)]; set => _keys[nameof(SkipExistFiles)] = value; }

        /// <summary>
        /// Dirs which must be created after update
        /// </summary>
        public string CreateDirs { get => _keys[nameof(CreateDirs)]; set => _keys[nameof(CreateDirs)] = value; }
        /// <summary>
        /// Files which must be created after update
        /// </summary>
        public string CreateFiles { get => _keys[nameof(CreateFiles)]; set => _keys[nameof(CreateFiles)] = value; }

        public GameUpdateInfo(string updateInfoPath)
        {
            Get(updateInfoPath);
        }

        void Get(string updateInfoPath)
        {
            var updateInfoIni = ManageIni.GetINIFile(updateInfoPath);

            var keys = _keys.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                var keyName = keys[i];
                if (updateInfoIni.KeyExists(keyName))
                {
                    _keys[keyName] = updateInfoIni.GetKey("", keyName);
                }
            }
        }
    }
}
