using AIHelper.Manage.Update;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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

        protected override void Get(DirectoryInfo dir)
        {
            _dir = dir;
            var updateInfoIniFilePath = Path.Combine(dir.FullName, "gameupdateinfo.ini");
            if (!File.Exists(updateInfoIniFilePath))
            {
                // not update
                return;
            }

            updateInfo = new GameUpdateInfo(updateInfoIniFilePath);

            if (string.IsNullOrWhiteSpace(updateInfo.GameFolderName))
            {
                // empty game folder name
                return;
            }

            if (Manage.ManageSettings.GetCurrentGameFolderName() != updateInfo.GameFolderName)
            {
                // incorrect game
                MessageBox.Show(updateInfo.GameFolderName + ": " + T._("Incorrect current game"));
                return;
            }

            bool updated = false;

            IsRoot = updateInfo.IsRoot == "true";

            _dateTimeSuffix = "_" + DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

            _backupsDir = Path.Combine(Manage.ManageSettings.GetCurrentGameFolderName(), "Buckups");
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

            if(updated)
            {
                MessageBox.Show(updateInfo.GameFolderName + ": " + T._("Update applied.\n\nWill be opened Backup folder.\nCheck content and remove files and dirs which not need for you animore."));
                Process.Start(_bakDir);
            }
            else
            {
                MessageBox.Show(updateInfo.GameFolderName + ": " + T._("Update not required."));
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
            var dataDir = IsRoot ? Path.Combine(_dir.FullName, "Games", updateInfo.GameFolderName, targetDirName) : Path.Combine(_dir.FullName, targetDirName);

            if (!Directory.Exists(dataDir))
            {
                return;
            }

            var dataBuckupDir = Path.Combine(_bakDir, targetDirName);
            if (!Directory.Exists(dataBuckupDir))
            {
                Directory.CreateDirectory(dataBuckupDir);
            }

            // replace files by newer
            foreach (var fileInfo in new DirectoryInfo(dataDir).EnumerateFiles("*", SearchOption.AllDirectories))
            {
                var targetFileInfo = new FileInfo(fileInfo.Name.Replace(dataDir, Manage.ManageSettings.GetCurrentGameDataPath()));
                var targetPath = targetFileInfo.FullName;
                if (targetFileInfo.Exists)
                {
                    targetPath = fileInfo.LastWriteTimeUtc > targetFileInfo.LastWriteTimeUtc
                        ? targetPath.Replace(Manage.ManageSettings.GetCurrentGameDataPath(), dataBuckupDir)
                        : fileInfo.FullName.Replace(dataDir, dataBuckupDir);
                }

                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                fileInfo.MoveTo(targetPath);
            }
        }

        private void ProceedUpdateMods()
        {
            var modsDir = IsRoot ? Path.Combine(_dir.FullName, "Games", updateInfo.GameFolderName, "Mods") : Path.Combine(_dir.FullName, "Mods");

            if (!Directory.Exists(modsDir))
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
            foreach (var modDir in Directory.EnumerateDirectories(modsDir))
            {
                var targetGameModDir = Path.Combine(Manage.ManageSettings.GetCurrentGameModsDirPath(), Path.GetFileName(modDir));
                if (Directory.Exists(targetGameModDir))
                {
                    if (!IsNewer(modDir, targetGameModDir))
                    {
                        Directory.Move(modDir, modDir.Replace(modsDir, modsBuckupDir));
                        continue;
                    }

                    Directory.Move(targetGameModDir, targetGameModDir.Replace(Manage.ManageSettings.GetCurrentGameModsDirPath(), modsBuckupDir));
                }
                else
                {
                    Directory.Move(modDir, targetGameModDir);
                }
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

            var UpdateVersion = Manage.ManageIni.GetINIFile(metaIniUpdatePath, false).GetKey("General", "version");
            var GameVersion = Manage.ManageIni.GetINIFile(metaIniGamePath, false).GetKey("General", "version");

            return UpdateVersion.IsNewerOf(GameVersion);
        }
    }

    class GameUpdateInfo
    {

        Dictionary<string, string> keys = new Dictionary<string, string>()
        {
            { nameof(GameFolderName), string.Empty },
            { nameof(UpdateData), "false" },
            { nameof(UpdateMods), "false" },
            { nameof(IsRoot), "false" },
            { nameof(UpdateMO), "false" },
        };

        /// <summary>
        /// Name of game's folder name which need to update
        /// </summary>
        internal string GameFolderName { get => keys[nameof(GameFolderName)]; set => keys[nameof(GameFolderName)] = value; }
        /// <summary>
        /// Update Data folder
        /// </summary>
        internal string UpdateData { get => keys[nameof(UpdateData)]; set => keys[nameof(UpdateData)] = value; }
        /// <summary>
        /// Update mod dirs in Mods folder
        /// </summary>
        internal string UpdateMods { get => keys[nameof(UpdateMods)]; set => keys[nameof(UpdateMods)] = value; }
        /// <summary>
        /// Update files in MO dir of the game to newer
        /// </summary>
        internal string UpdateMO { get => keys[nameof(UpdateMO)]; set => keys[nameof(UpdateMO)] = value; }
        /// <summary>
        /// Root means aihelper root dir as update dir root
        /// </summary>
        internal string IsRoot { get => keys[nameof(IsRoot)]; set => keys[nameof(IsRoot)] = value; }

        public GameUpdateInfo(string updateInfoPath)
        {
            Get(updateInfoPath);
        }

        void Get(string updateInfoPath)
        {
            var updateInfoIni = Manage.ManageIni.GetINIFile(updateInfoPath);

            foreach (var key in keys)
            {
                if (updateInfoIni.KeyExists(key.Key))
                {
                    keys[key.Key] = updateInfoIni.GetKey("", key.Key);
                }
            }
        }
    }
}
