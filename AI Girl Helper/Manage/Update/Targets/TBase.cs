using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;

namespace AIHelper.Manage.Update.Targets
{
    //Base for targets
    abstract class TBase
    {
        protected updateInfo info;

        protected TBase(updateInfo info)
        {
            this.info = info;

            UpdateFilenameSubPathData = new Dictionary<string, string>
                {
                    { "ScriptLoader.dll", "BepInEx" + Path.DirectorySeparatorChar + "plugins" }
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
                BuckupDirPath = Path.Combine(ManageSettings.GetCurrentGameModsUpdateDir(), "old", info.TargetFolderPath.Name + "_" + info.TargetCurrentVersion);
                if (Directory.Exists(BuckupDirPath))
                {
                    BuckupDirPath += "_" + DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                }

                info.BuckupDirPath = BuckupDirPath;
                //Directory.CreateDirectory(BuckupDirPath);

                //info.TargetFolderPath.FullName.CopyAll(BuckupDirPath);
                MakeBuckup(BuckupDirPath, info.TargetFolderPath.FullName);
                //ZipFile.CreateFromDirectory(info.TargetFolderPath.FullName, OldModBuckupDirPath);


                return Directory.Exists(BuckupDirPath);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// restore mod from buckup
        /// </summary>
        internal virtual void RestoreBuckup()
        {
            RestoreModFromBuckup(BuckupDirPath, info.TargetFolderPath.FullName);
        }

        protected bool PerformUpdate()
        {
            if (!Path.GetFileNameWithoutExtension(info.UpdateFilePath).StartsWith(info.UpdateFileStartsWith, StringComparison.InvariantCultureIgnoreCase)
                //|| !IsLatestVersionNewerOfCurrent(GitLatestVersion, GitCurrentVersion)
                )
            {
                return false;
            }

            //if (!updateData.IsMod)
            //{
            //    return;
            //}

            //var modname = modGitData.CurrentModName; //Path.GetFileName(Path.GetDirectoryName(filePath));
            var UpdatingModDirPath = info.TargetFolderPath.FullName;


            //var OldModBuckupDirPath = Path.Combine(ManageSettings.GetCurrentGameModsUpdateDir(), "old", info.TargetFolderPath.Name + "_" + info.TargetCurrentVersion);
            //if (Directory.Exists(OldModBuckupDirPath))
            //{
            //    OldModBuckupDirPath += "_" + DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            //}

            //Directory.CreateDirectory(OldModBuckupDirPath);

            bool success = false;

            try
            {
                //MakeBuckup(OldModBuckupDirPath, UpdatingModDirPath);

                int code = 0;
                string ext;
                if (!File.Exists(info.UpdateFilePath))
                {
                    code = -1;
                }
                else if ((ext = Path.GetExtension(info.UpdateFilePath).ToUpperInvariant()) == ".ZIP")
                {
                    code = 1;
                }
                else if (ext == ".DLL")
                {
                    code = 2;
                }

                if (code > 0)
                {
                    if (code == 1)
                    {
                        using (ZipArchive archive = ZipFile.OpenRead(info.UpdateFilePath))
                        {
                            archive.ExtractToDirectory(UpdatingModDirPath);
                        }
                    }
                    else if (code == 2)
                    {
                        var FullDllFileName = Path.GetFileName(info.UpdateFilePath);

                        if (UpdateFilenameSubPathData.ContainsKey(FullDllFileName)) //get subpath from predefined list
                        {
                            var targetDir = UpdatingModDirPath + Path.DirectorySeparatorChar + UpdateFilenameSubPathData[FullDllFileName];
                            Directory.CreateDirectory(targetDir);
                            var targetFilePath = Path.Combine(targetDir, FullDllFileName);
                            File.Move(info.UpdateFilePath, targetFilePath);
                        }
                        else
                        {
                            //search dll with same name and get subpath from this dll
                            foreach (var dll in Directory.EnumerateFiles(info.BuckupDirPath, "*.dll", SearchOption.AllDirectories))
                            {
                                if (Path.GetFileName(dll) == FullDllFileName)
                                {
                                    var targetDir = Path.GetDirectoryName(dll).Replace(info.BuckupDirPath, UpdatingModDirPath);
                                    Directory.CreateDirectory(targetDir);
                                    var targetFilePath = Path.Combine(targetDir, FullDllFileName);
                                    File.Move(info.UpdateFilePath, targetFilePath);
                                    break;
                                }
                            }
                        }
                    }
                    success = true;
                }
                if (success)
                {
                    //File.Delete(modGitData.UpdateFilePath);

                    RestoreSomeFiles(info.BuckupDirPath, UpdatingModDirPath);

                    //info.TargetCurrentVersion = info.TargetLastVersion;

                    ManageINI.WriteINIValue(Path.Combine(UpdatingModDirPath, "meta.ini"), "General", "version", info.TargetLastVersion);

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

                ManageLogs.Log("Failed to update mod" + " " + info.TargetFolderPath.Name + ":" + Environment.NewLine + ex);

                return false;
            }

            return true;
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

        protected void RestoreSomeFiles(string OldModBuckupDirPath, string UpdatingModDirPath)
        {
            var RestoreList = new HashSet<string>();
            foreach(var path in info.target.RestorePathsList())
            {
                var p = Path.GetFullPath(Path.Combine(OldModBuckupDirPath, path));
                RestoreList.Add(p);
            }

            //restore some files files
            foreach (var dir in new DirectoryInfo(OldModBuckupDirPath).EnumerateDirectories("*", SearchOption.AllDirectories))
            {
                try
                {
                    if (!Directory.Exists(Path.Combine(UpdatingModDirPath, dir.Name)) && RestoreList.Contains(dir.FullName))
                    {
                        var TargetPath = dir.FullName.Replace(OldModBuckupDirPath, UpdatingModDirPath);
                        Directory.CreateDirectory(Path.GetDirectoryName(TargetPath));
                        if (Directory.Exists(dir.FullName))
                        {
                            dir.FullName.CopyAll(TargetPath);
                        }
                    }
                }
                catch// (Exception ex)
                {
                    //ManageLogs.Log("Update: Failed to move file to bak dir. error:" + Environment.NewLine + ex);
                }
            }

            //restore some files files
            foreach (var file in new DirectoryInfo(OldModBuckupDirPath).EnumerateFiles("*", SearchOption.AllDirectories))
            {
                try
                {
                    string ext;
                    if (!File.Exists(Path.Combine(UpdatingModDirPath, file.Name)) &&
                        (
                            file.Extension == ".ini"
                            || file.Extension == ".cfg"
                            || (file.DirectoryName == OldModBuckupDirPath && file.Extension.IsPictureExtension())
                            || RestoreList.Contains(file.FullName)
                            )
                        )
                    {
                        var TargetFilePath = file.FullName.Replace(OldModBuckupDirPath, UpdatingModDirPath);
                        Directory.CreateDirectory(Path.GetDirectoryName(TargetFilePath));
                        if (File.Exists(file.FullName) && !File.Exists(TargetFilePath))
                        {
                            File.Copy(file.FullName, TargetFilePath);
                        }
                    }
                }
                catch// (Exception ex)
                {
                    //ManageLogs.Log("Update: Failed to move file to bak dir. error:" + Environment.NewLine + ex);
                }
            }
        }

        private static void RestoreModFromBuckup(string OldModBuckupDirPath, string UpdatingModDirPath)
        {
            //restore old dirs
            foreach (var folder in Directory.GetDirectories(OldModBuckupDirPath))
            {
                try
                {
                    var targetPath = folder.Replace(OldModBuckupDirPath, UpdatingModDirPath);
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
            foreach (var file in Directory.GetFiles(OldModBuckupDirPath))
            {
                try
                {
                    var targetPath = file.Replace(OldModBuckupDirPath, UpdatingModDirPath);
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

            ManageFilesFolders.DeleteEmptySubfolders(OldModBuckupDirPath);
        }

        private static void MakeBuckup(string OldModBuckupDirPath, string UpdatingModDirPath)
        {
            Directory.CreateDirectory(OldModBuckupDirPath);

            //buckup old dirs
            foreach (var folder in Directory.GetDirectories(UpdatingModDirPath))
            {
                try
                {
                    var backupPath = folder.Replace(UpdatingModDirPath, OldModBuckupDirPath);
                    Directory.CreateDirectory(Path.GetDirectoryName(backupPath));
                    if (Directory.Exists(folder))
                    {
                        Directory.Move(folder, backupPath);
                    }
                }
                catch (Exception ex)
                {
                    ManageLogs.Log("Update: Filed to move dir to bak dir. error:" + Environment.NewLine + ex);
                }
            }
            //buckup old files
            foreach (var file in Directory.GetFiles(UpdatingModDirPath))
            {
                try
                {
                    if (Path.GetFileName(file) == "meta.ini")
                    {
                        continue;
                    }
                    //string ext;
                    //if ((ext = Path.GetExtension(file)) == ".ini" || ext == ".cfg" || (Path.GetFileName(Path.GetDirectoryName(file)) == UpdatingModDirPath && ext.IsPictureExtension()))
                    //{
                    //    continue;
                    //}
                    var backupPath = file.Replace(UpdatingModDirPath, OldModBuckupDirPath);
                    Directory.CreateDirectory(Path.GetDirectoryName(backupPath));
                    File.Move(file, backupPath);
                }
                catch (Exception ex)
                {
                    ManageLogs.Log("Update: Failed to move file to bak dir. error:" + Environment.NewLine + ex);
                }
            }
        }
    }
}
