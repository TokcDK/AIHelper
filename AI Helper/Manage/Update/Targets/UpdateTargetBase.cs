
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace AIHelper.Manage.Update.Targets
{
    //Base for targets
    abstract class UpdateTargetBase
    {
        protected UpdateInfo Info;

        protected UpdateTargetBase(UpdateInfo info)
        {
            this.Info = info;

            UpdateFilenameSubPathData = new Dictionary<string, string>
                {
                    { "ScriptLoader.dll", "BepInEx" + Path.DirectorySeparatorChar + "plugins" },
                    { "GraphicsSettings.dll", "BepInEx" + Path.DirectorySeparatorChar + "plugins" },
                    { "HS2_Heelz.dll", "BepInEx" + Path.DirectorySeparatorChar + "plugins" },
                    { "AI_Heelz.dll", "BepInEx" + Path.DirectorySeparatorChar + "plugins" }
                };
        }

        /// <summary>
        /// List of folders paths with extracted update infos
        /// </summary>
        /// <returns></returns>
        internal abstract Dictionary<string, string> GetUpdateInfos();//list of ModName\UpdateInfo pairs

        /// <summary>
        /// target data for update of some folders when, for example, downloaded dll and need to get correct target folder for it
        /// </summary>
        internal Dictionary<string, string> UpdateFilenameSubPathData;

        /// <summary>
        /// Update folder with new files
        /// </summary>
        /// <returns></returns>
        internal abstract bool UpdateFiles();

        /// <summary>
        /// Path to buckup copy of target folder
        /// </summary>
        internal string BuckupDirPath;

        /// <summary>
        /// Make target folder buckup
        /// </summary>
        internal virtual bool MakeBuckup()
        {
            try
            {
                BuckupDirPath = Path.Combine(ManageSettings.GetUpdatedModsOlderVersionsBuckupDirPath(), Info.TargetFolderPath.Name + "_" + Info.TargetCurrentVersion);
                if (Directory.Exists(BuckupDirPath))
                {
                    BuckupDirPath += ManageSettings.GetDateTimeBasedSuffix();
                }

                Info.BuckupDirPath = BuckupDirPath;
                //Directory.CreateDirectory(BuckupDirPath);

                //set RestoreList
                RestoreList = new HashSet<string>();
                foreach (var path in Info.Target.RestorePathsList())
                {
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        continue;
                    }

                    var p = Path.GetFullPath(Path.Combine(Info.BuckupDirPath, path));
                    RestoreList.Add(p);
                }

                foreach (var path in Info.Target.RestorePathsListExtra())
                {
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        continue;
                    }

                    var dirinfo = new DirectoryInfo(path);
                    if (dirinfo.Exists && dirinfo.IsSymlink() && dirinfo.IsValidSymlink())
                    {
                        var modPath = ManageModOrganizer.GetPathInMods(dirinfo.GetSymlinkTarget());

                        if (Path.GetFileName(modPath) == Info.TargetFolderPath.FullName) // if parsing mod path is same then add for bak
                        {
                            RestoreList.Add(path);
                        }
                    }
                }

                //info.TargetFolderPath.FullName.CopyAll(BuckupDirPath);
                MakeBuckup(BuckupDirPath, Info.TargetFolderPath.FullName);
                //ZipFile.CreateFromDirectory(info.TargetFolderPath.FullName, OldModBuckupDirPath);


                return Directory.Exists(BuckupDirPath);
            }
            catch (Exception ex)
            {
                ManageLogs.Log("An error occured while buckup creation:\r\n" + ex);
                return false;
            }
        }

        /// <summary>
        /// restore mod from buckup
        /// </summary>
        internal virtual void RestoreBuckup()
        {
            RestoreModFromBuckup(BuckupDirPath, Info.TargetFolderPath.FullName);
        }

        protected bool PerformUpdate()
        {
            if (!Path.GetFileNameWithoutExtension(Info.UpdateFilePath).StartsWith(Info.UpdateFileStartsWith, StringComparison.InvariantCultureIgnoreCase)
               && !Path.GetFileNameWithoutExtension(Info.UpdateFilePath).ToUpperInvariant().StartsWith(Path.GetFileNameWithoutExtension(Info.UpdateFileStartsWith).ToUpperInvariant(), StringComparison.InvariantCultureIgnoreCase)
                //|| !IsLatestVersionNewerOfCurrent(GitLatestVersion, GitCurrentVersion)
                )
            {
                return false;
            }

            // Move file from 2mo to downloads when it was manally downloaded in 2mo
            if (Info.UpdateFilePath.StartsWith(ManageSettings.GetInstall2MoDirPath()))
            {
                // set new path in downloads
                var newFilePathInDownloads = Info.UpdateFilePath.Replace(ManageSettings.GetInstall2MoDirPath(), ManageSettings.GetModsUpdateDirDownloadsPath());
                // move file to new path
                File.Move(Info.UpdateFilePath, newFilePathInDownloads);
                // set change path to new
                Info.UpdateFilePath = newFilePathInDownloads;
            }

            //if (!updateData.IsMod)
            //{
            //    return;
            //}

            //var modname = modGitData.CurrentModName; //Path.GetFileName(Path.GetDirectoryName(filePath));
            var updatingModDirPath = Info.TargetFolderPath.FullName;


            //var OldModBuckupDirPath = Path.Combine(ManageSettings.GetCurrentGameModsUpdateDir(), "old", info.TargetFolderPath.Name + "_" + info.TargetCurrentVersion);
            //if (Directory.Exists(OldModBuckupDirPath))
            //{
            //    OldModBuckupDirPath += ManageSettings.GetDateTimeBasedSuffix();
            //}

            //Directory.CreateDirectory(OldModBuckupDirPath);

            bool success = false;

            try
            {
                //MakeBuckup(OldModBuckupDirPath, UpdatingModDirPath);

                int code = 0;
                string ext;
                if (!File.Exists(Info.UpdateFilePath))
                {
                    code = -1;
                }
                else if ((ext = Path.GetExtension(Info.UpdateFilePath).ToUpperInvariant()) == ".ZIP")
                {
                    code = 1;
                }
                else if (ext == ".DLL")
                {
                    code = 2;
                }

                if (code > 0)
                {
                    switch (code)
                    {
                        case 1:
                            {
                                using (ZipArchive archive = ZipFile.OpenRead(Info.UpdateFilePath))
                                {
                                    archive.ExtractToDirectory(updatingModDirPath);
                                }

                                break;
                            }

                        case 2:
                            {
                                var fullDllFileName = Path.GetFileName(Info.UpdateFilePath);
                                string targetDir;
                                if (UpdateFilenameSubPathData.ContainsKey(fullDllFileName)) //get subpath from predefined list
                                {
                                    targetDir = updatingModDirPath + Path.DirectorySeparatorChar + UpdateFilenameSubPathData[fullDllFileName];
                                }
                                else if ((targetDir = GetDllTargetDir(fullDllFileName, updatingModDirPath)).Length > 0)//search dll path in buckup dir
                                {
                                    Directory.CreateDirectory(targetDir);
                                }
                                else if (Info.TargetFolderUpdateInfo[0] == "BepInEx")//default BepInEx plugins dir
                                {
                                    targetDir = Path.Combine(updatingModDirPath, "BepInEx", "plugins");
                                }

                                Directory.CreateDirectory(targetDir);
                                var targetFilePath = Path.Combine(targetDir, fullDllFileName);
                                File.Move(Info.UpdateFilePath, targetFilePath);
                                break;
                            }
                    }
                    success = true;
                }
                if (success)
                {
                    //File.Delete(modGitData.UpdateFilePath);

                    RestoreSomeFiles(Info.BuckupDirPath, updatingModDirPath);

                    //info.TargetCurrentVersion = info.TargetLastVersion;

                    ManageIni.WriteIniValue(Path.Combine(updatingModDirPath, "meta.ini"), "General", "version", Info.TargetLastVersion);

                    //report.Add(
                    //    (IsHTMLReport ? "<p style=\"color:lightgreen\">" : string.Empty)
                    //    + T._("Mod")
                    //    + " "
                    //    + updateData.GitName
                    //    + " "
                    //    + T._("updated to version")
                    //    + " "
                    //    + updateData.GitLatestVersion
                    //    + (IsHTMLReport ? "</p>" : string.Empty)
                    //    + (!string.IsNullOrWhiteSpace(updateData.GitLinkForVisit) ? "{{visit}}" + updateData.GitLinkForVisit : string.Empty));
                }
                else
                {
                    //RestoreModFromBuckup(OldModBuckupDirPath, UpdatingModDirPath);

                    //    report.Add(
                    //        (IsHTMLReport ? "<p style=\"color:orange\">" : string.Empty)
                    //        + T._("Failed to update mod")
                    //        + " "
                    //        + updateData.CurrentModName
                    //        + " "
                    //        + (code == 1 ? " " + T._("Update file not found") : string.Empty)
                    //        + (code == 2 ? " " + T._("Update file not a zip") : string.Empty)
                    //        + "</p>"
                    //        );

                    return false;
                }
            }
            catch (Exception ex)
            {
                //RestoreModFromBuckup(OldModBuckupDirPath, UpdatingModDirPath);

                //report.Add(
                //    (IsHTMLReport ? "<p style=\"color:red\">" : string.Empty)
                //    + T._("Failed to update mod")
                //    + " "
                //    + updateData.CurrentModName
                //    + " (" + ex.Message + ") "
                //    + T._("Details in") + " " + Application.ProductName + ".log"
                //    + "</p>"
                //    );

                ManageLogs.Log("Failed to update mod" + " " + Info.TargetFolderPath.Name + ":" + Environment.NewLine + ex);

                return false;
            }

            return true;
        }

        private string GetDllTargetDir(string fullDllFileName, string updatingModDirPath)
        {
            //search dll with same name and get subpath from this dll
            foreach (var dll in Directory.EnumerateFiles(Info.BuckupDirPath, "*.dll", SearchOption.AllDirectories))
            {
                if (Path.GetFileName(dll) == fullDllFileName)
                {
                    return Path.GetDirectoryName(dll).Replace(Info.BuckupDirPath, updatingModDirPath);
                }
            }
            return "";
        }

        internal abstract void SetCurrentVersion();

        internal virtual string GetParentFolderPath()
        {
            return "";
        }

        /// <summary>
        /// list of files and folder which need to be restore in updated folder
        /// </summary>
        /// <returns></returns>
        internal virtual string[] RestorePathsList()
        {
            return new[] { "" };
        }

        /// <summary>
        /// list of folders from game's list of symlink target object
        /// </summary>
        /// <returns></returns>
        internal virtual string[] RestorePathsListExtra()
        {
            return new[] { "" };
        }

        /// <summary>
        /// file which need to be restored to updating dir
        /// </summary>
        protected HashSet<string> RestoreList;

        /// <summary>
        /// restore some files like cfg
        /// </summary>
        /// <param name="oldModBuckupDirPath"></param>
        /// <param name="updatingModDirPath"></param>
        protected void RestoreSomeFiles(string oldModBuckupDirPath, string updatingModDirPath)
        {
            //restore some files files
            foreach (var dir in new DirectoryInfo(oldModBuckupDirPath).EnumerateDirectories("*", SearchOption.AllDirectories))
            {
                try
                {
                    if (!Directory.Exists(Path.Combine(updatingModDirPath, dir.Name)) && RestoreList.Contains(dir.FullName))
                    {
                        var targetPath = dir.FullName.Replace(oldModBuckupDirPath, updatingModDirPath);
                        Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                        if (Directory.Exists(dir.FullName))
                        {
                            dir.FullName.CopyAll(targetPath);
                        }
                    }
                }
                catch// (Exception ex)
                {
                    //ManageLogs.Log("Update: Failed to move file to bak dir. error:" + Environment.NewLine + ex);
                }
            }

            //restore some files files
            foreach (var file in new DirectoryInfo(oldModBuckupDirPath).EnumerateFiles("*", SearchOption.AllDirectories))
            {
                try
                {
                    string ext;
                    if (!File.Exists(Path.Combine(updatingModDirPath, file.Name)) &&
                        (
                            file.Extension == ".ini"
                            || file.Extension == ".cfg"
                            || (file.DirectoryName == oldModBuckupDirPath && file.Extension.IsPictureExtension())
                            || RestoreList.Contains(file.FullName)
                            )
                        )
                    {
                        var targetFilePath = file.FullName.Replace(oldModBuckupDirPath, updatingModDirPath);
                        Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath));
                        if (File.Exists(file.FullName) && !File.Exists(targetFilePath))
                        {
                            File.Copy(file.FullName, targetFilePath);
                        }
                    }
                }
                catch// (Exception ex)
                {
                    //ManageLogs.Log("Update: Failed to move file to bak dir. error:" + Environment.NewLine + ex);
                }
            }
        }

        private static void RestoreModFromBuckup(string oldModBuckupDirPath, string updatingModDirPath)
        {
            if (string.IsNullOrWhiteSpace(oldModBuckupDirPath) || string.IsNullOrWhiteSpace(updatingModDirPath))
            {
                return;
            }

            //restore old dirs
            foreach (var folder in Directory.GetDirectories(oldModBuckupDirPath))
            {
                try
                {
                    var targetPath = folder.Replace(oldModBuckupDirPath, updatingModDirPath);
                    if (Directory.Exists(targetPath))
                    {
                        Directory.Delete(targetPath, true);
                    }
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                    if (Directory.Exists(folder))
                    {
                        Directory.Move(folder, targetPath);
                    }
                }
                catch (Exception ex)
                {
                    ManageLogs.Log("Restore: Failed to move dir to mod dir. error:" + Environment.NewLine + ex);
                }
            }
            //restore old files
            foreach (var file in Directory.GetFiles(oldModBuckupDirPath))
            {
                try
                {
                    var targetPath = file.Replace(oldModBuckupDirPath, updatingModDirPath);
                    if (File.Exists(targetPath))
                    {
                        File.Delete(targetPath);
                    }
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                    if (File.Exists(file))
                    {
                        File.Move(file, targetPath);
                    }
                }
                catch (Exception ex)
                {
                    ManageLogs.Log("Restore: Failed to move file to mod dir. error:" + Environment.NewLine + ex);
                }
            }

            ManageFilesFoldersExtensions.DeleteEmptySubfolders(oldModBuckupDirPath);
        }

        /// <summary>
        /// move/copy some files to buckup dir
        /// </summary>
        /// <param name="oldModBuckupDirPath"></param>
        /// <param name="updatingModDirPath"></param>
        private void MakeBuckup(string oldModBuckupDirPath, string updatingModDirPath)
        {
            Directory.CreateDirectory(oldModBuckupDirPath);

            //buckup old dirs
            foreach (var folderPath in new DirectoryInfo(updatingModDirPath).EnumerateDirectories())
            {
                try
                {
                    bool isSynlinkTarget = false;
                    var folder = folderPath;
                    if (folder.IsSymlink())
                    {
                        if (!folder.IsValidSymlink())
                        {
                            continue;
                        }

                        isSynlinkTarget = true;
                        folder = new DirectoryInfo(folder.GetSymlinkTarget());
                    }

                    var backupPath = new DirectoryInfo(folder.FullName.Replace(updatingModDirPath, oldModBuckupDirPath));
                    Directory.CreateDirectory(Path.GetDirectoryName(backupPath.FullName));
                    if (folder.Exists)
                    {
                        if (isSynlinkTarget)
                        {
                            folder.CopyAll(backupPath);
                        }
                        else
                        {
                            folder.MoveTo(backupPath.FullName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ManageLogs.Log("Update: Filed to move dir to bak dir. error:" + Environment.NewLine + ex);
                }
            }
            //buckup old files
            foreach (var filePath in new DirectoryInfo(updatingModDirPath).EnumerateFiles())
            {
                try
                {
                    bool isSynlinkTarget = false;
                    var file = filePath;
                    if (file.IsSymlink())
                    {
                        if (!file.IsValidSymlink())
                        {
                            continue;
                        }

                        isSynlinkTarget = true;
                        file = new FileInfo(filePath.GetSymlinkTarget());
                    }

                    //string ext;
                    //if ((ext = Path.GetExtension(file)) == ".ini" || ext == ".cfg" || (Path.GetFileName(Path.GetDirectoryName(file)) == UpdatingModDirPath && ext.IsPictureExtension()))
                    //{
                    //    continue;
                    //}
                    var backupPath = file.FullName.Replace(updatingModDirPath, oldModBuckupDirPath);
                    Directory.CreateDirectory(Path.GetDirectoryName(backupPath));
                    if (RestoreList.Contains(file.FullName) || isSynlinkTarget)
                    {
                        file.CopyTo(backupPath);
                    }
                    else
                    {
                        file.MoveTo(backupPath);
                    }
                }
                catch (Exception ex)
                {
                    ManageLogs.Log("Update: Failed to move file to bak dir. error:" + Environment.NewLine + ex);
                }
            }
        }
    }
}
