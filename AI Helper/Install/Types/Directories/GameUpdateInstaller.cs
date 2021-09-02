﻿using AIHelper.Manage;
using AIHelper.Manage.Update;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

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

            _backupsDir = Path.Combine(ManageSettings.GetCurrentGameDirPath(), "Buckups");
            _bakDir = Path.Combine(_backupsDir, "Backup");
            if (Directory.Exists(_bakDir))
            {
                Directory.Move(_bakDir, _bakDir + _dateTimeSuffix);
            }
            Directory.CreateDirectory(_bakDir);

            if (updateInfo.UpdateMods == "true")
            {
                updated = true;
                ProceedUpdateMods();
            }

            if (updateInfo.UpdateData == "true")
            {
                updated = true;
                ProceedUpdateData();
            }

            if (updateInfo.UpdateMO == "true")
            {
                updated = true;
                ProceedUpdateMO();
            }

            if (updateInfo.RemoveDirs.Length > 0)
            {
                updated = true;
                ProceedRemoveDirs();
            }

            if (updateInfo.RemoveFiles.Length > 0)
            {
                updated = true;
                ProceedRemoveFiles();
            }

            // clean update info and folder
            File.Delete(Path.Combine(dir.FullName, _gameUpdateInfoFileName));
            ManageFilesFolders.DeleteEmptySubfolders(dirPath: dir.FullName, deleteThisDir: true);

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

        private void ProceedRemoveFiles()
        {
            ProceedRemove();
        }

        private void ProceedRemoveDirs()
        {
            ProceedRemove(false);
        }

        private void ProceedRemove(bool files = true)
        {
            string rootDir = IsRoot ? ManageSettings.GetApplicationStartupPath() : ManageSettings.GetCurrentGameDirPath();
            foreach (var SubPath in updateInfo.RemoveDirs.Split(','))
            {
                var sourcePath = rootDir + Path.DirectorySeparatorChar + SubPath;
                var buckupPath = _bakDir + Path.DirectorySeparatorChar + SubPath;
                if ((files && File.Exists(sourcePath)) || (!files && Directory.Exists(sourcePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(buckupPath));
                    if (files)
                    {
                        File.Move(sourcePath, buckupPath);
                    }
                    else
                    {
                        Directory.Move(sourcePath, buckupPath);
                    }
                }
            }
        }

        private void ProceedUpdateMO()
        {
            ProceedUpdateFiles("MO");
        }

        private void ProceedUpdateData()
        {
            ProceedUpdateFiles("Data");
        }

        private void ProceedUpdateFiles(string targetDirName)
        {
            var updateGameDir = IsRoot ? Path.Combine(_dir.FullName, "Games", updateInfo.GameFolderName) : Path.Combine(_dir.FullName);
            var updateTargetSubDir = Path.Combine(updateGameDir, targetDirName);

            if (!Directory.Exists(updateTargetSubDir))
            {
                return;
            }

            var updateTargetBuckupSubDir = Path.Combine(_bakDir, targetDirName);
            if (!Directory.Exists(updateTargetBuckupSubDir))
            {
                Directory.CreateDirectory(updateTargetBuckupSubDir);
            }

            // replace files by newer
            foreach (var updateFileInfo in new DirectoryInfo(updateTargetSubDir).EnumerateFiles("*", SearchOption.AllDirectories))
            {
                var targetFileInfo = new FileInfo(updateFileInfo.FullName.Replace(updateGameDir, ManageSettings.GetCurrentGameDirPath()));

                var targetPath = targetFileInfo.FullName;// default target path in game's target subdir

                if (targetFileInfo.Exists)
                {
                    var fileBakBath = updateFileInfo.FullName.Replace(updateGameDir, _bakDir);// file's path in bak dir

                    if (updateFileInfo.LastWriteTimeUtc > targetFileInfo.LastWriteTimeUtc)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(fileBakBath));// create parent dir
                        targetFileInfo.MoveTo(fileBakBath); // move exist older target file to bak dir
                    }
                    else
                    {
                        targetPath = fileBakBath; // set bak dir path for older target file
                    }
                }

                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));// create parent dir

                updateFileInfo.MoveTo(targetPath); // move update file to target if it is newer or to bak dir if older
            }
        }

        private void ProceedUpdateMods()
        {
            var updateModsDir = IsRoot ? Path.Combine(_dir.FullName, "Games", updateInfo.GameFolderName, "Mods") : Path.Combine(_dir.FullName, "Mods");

            if (!Directory.Exists(updateModsDir))
            {
                return;
            }

            // create mods buckup dir
            var modsBuckupDir = Path.Combine(_bakDir, "Mods");
            if (!Directory.Exists(modsBuckupDir))
            {
                Directory.CreateDirectory(modsBuckupDir);
            }

            // replace folders by updated
            foreach (var updateModDir in Directory.EnumerateDirectories(updateModsDir))
            {
                var targetGameModDir = Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), Path.GetFileName(updateModDir));
                               
                if (Directory.Exists(targetGameModDir))
                {
                    string bakModDir = updateModDir.Replace(updateModsDir, modsBuckupDir);
                    if (!IsNewer(updateModDir, targetGameModDir))
                    {
                        // when update dir older just move it in bak and continue
                        Directory.Move(updateModDir, bakModDir);
                        continue;
                    }

                    // move older target dir to bak
                    Directory.Move(targetGameModDir, bakModDir);
                }

                // move new update dir to target
                Directory.Move(updateModDir, targetGameModDir);
            }
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
