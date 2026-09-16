using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AIHelper.Manage.Update.Targets
{
    /// <summary>
    /// Target for Mod Organizer itself. MO is not a mod: it has no parent
    /// folder, its update record key is an absolute path, and it is updated by
    /// running its own installer instead of extracting an archive.
    /// </summary>
    class Mo : UpdateTargetBase
    {
        /// <summary>How long the MO installer may run before it is killed.</summary>
        const int InstallerTimeoutMilliseconds = 10 * 60 * 1000;

        /// <summary>Installer switches that keep the update silent and unattended.</summary>
        const string InstallerSilentArguments = "/noicons /nocancel /norestart /silent";

        /// <summary>
        /// Update record of the MO target: owner, repository, file name prefix.
        /// The prefix keeps the source from picking up unrelated release assets.
        /// </summary>
        const string MoUpdateRecord = "ModOrganizer2,modorganizer,Mod.Organizer-";

        /// <summary>
        /// Files of the MO installation that must survive an update. The paths
        /// are relative to the MO folder (and therefore to its backup copy).
        /// </summary>
        static readonly string[] BaseFilesToBackup =
        {
            "ModOrganizer.ini",
            "categories.dat",
            "explorer++/Explorer++RU.dll",
            "explorer++/config.xml",
        };

        public Mo(UpdateInfo info) : base(info)
        {
        }

        /// <summary>
        /// Folder of the Mod Organizer installation. Read on demand so the
        /// target does not depend on <see cref="GetUpdateInfos"/> having run.
        /// </summary>
        private static string MoDirPath
        {
            get { return ManageSettings.AppModOrganizerDirPath; }
        }

        /// <summary>
        /// Get MO update info.
        /// </summary>
        /// <returns>
        /// A single entry whose key is the absolute MO directory path, or null
        /// when the current source cannot update MO.
        /// </returns>
        internal override Dictionary<string, string> GetUpdateInfos()
        {
            if (!Info.Source.Title.ToUpperInvariant().Contains("GITHUB"))
            {
                return null;
            }

            return new Dictionary<string, string>
            {
                { MoDirPath, MoUpdateRecord }
            };
        }

        /// <summary>
        /// Runs the MO installer to replace the current installation.
        /// </summary>
        /// <returns>True when the installer finished successfully.</returns>
        internal override bool UpdateFiles()
        {
            if (!File.Exists(Info.UpdateFilePath))
            {
                _log.Warn("Update: the MO installer '" + Info.UpdateFilePath + "' does not exist.");
                return false;
            }

            try
            {
                using (var installer = new Process())
                {
                    installer.StartInfo.FileName = Info.UpdateFilePath;
                    installer.StartInfo.Arguments = "/dir=\"" + MoDirPath + "\" " + InstallerSilentArguments;

                    installer.Start();

                    if (!installer.WaitForExit(InstallerTimeoutMilliseconds))
                    {
                        _log.Warn("Update: the MO installer did not finish in "
                                  + (InstallerTimeoutMilliseconds / 60000) + " minutes and was killed.");

                        KillInstaller(installer);

                        UpdateIo.TryDeleteFile(Info.UpdateFilePath); //maybe corrupted
                        return false;
                    }

                    if (installer.ExitCode != 0)
                    {
                        _log.Warn("Update: the MO installer exited with code " + installer.ExitCode
                                  + ". The existing installation was left untouched.");

                        UpdateIo.TryDeleteFile(Info.UpdateFilePath); //maybe corrupted
                        return false;
                    }

                    // restore the preserved files only after a successful install
                    RestoreSomeFiles(Info.BackupDirPath, MoDirPath);
                    ManageModOrganizer.RestoreMissingModOrganizerBasicGamePluginFiles();

                    return true;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Update: failed to run the MO installer '" + Info.UpdateFilePath + "'.");

                UpdateIo.TryDeleteFile(Info.UpdateFilePath); //maybe corrupted
                return false;
            }
        }

        /// <summary>
        /// Tells the source that the MO update file is the installer executable.
        /// </summary>
        internal override void PrepareUpdateRequest()
        {
            Info.UpdateFileEndsWith = ".exe";
        }

        internal override void SetCurrentVersion()
        {
            var moExePath = Path.Combine(MoDirPath, "ModOrganizer.exe");

            if (!File.Exists(moExePath)) return;

            var versionInfo = FileVersionInfo.GetVersionInfo(moExePath);

            // ProductVersion is null when the resource is missing, fall back to FileVersion
            Info.TargetCurrentVersion = !string.IsNullOrWhiteSpace(versionInfo.ProductVersion)
                ? versionInfo.ProductVersion
                : (versionInfo.FileVersion ?? string.Empty);
        }

        internal override string[] RestorePathsList()
        {
            return BaseFilesToBackup;
        }

        internal override bool MakeBackup()
        {
            ManageModOrganizer.PreserveModOrganizerBasicGamePluginFiles();
            return base.MakeBackup();
        }

        private void KillInstaller(Process installer)
        {
            try
            {
                installer.Kill();
            }
            catch (Exception ex)
            {
                _log.Debug(ex, "Update: failed to kill the MO installer.");
            }
        }
    }
}
