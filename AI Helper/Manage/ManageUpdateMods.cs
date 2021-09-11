using AIHelper.Manage.Update;
using AIHelper.SharedData;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static AIHelper.Manage.ManageModOrganizer;

namespace AIHelper.Manage
{
    class ManageUpdateMods
    {

        public static void UpdateMods()
        {
            UpdateModsInit();

            if (SharedData.GameData.MainForm.UseKKmanagerUpdaterLabel.IsChecked())
                UpdateZipmods();

            if (SharedData.GameData.MainForm.UpdatePluginsLabel.IsChecked())
                UpdateByUpdater();

            UpdateModsFinalize();
        }

        private static async void UpdateByUpdater()
        {
            ManageOther.SwitchFormMinimizedNormalAll(ManageSettings.ListOfFormsForMinimize());

            var updater = new Updater();

            //update plugins in mo mode
            if (Manage.ManageSettings.IsMoMode())
            {
                await updater.Update().ConfigureAwait(true);
            }

            if (updater.UpdatedAny) // make data update only when was updated any. Maybe required after update of MO or BepinEx
            {
                ManageModOrganizer.CleanMoFolder();
                //
                ManageModOrganizer.CheckBaseGamesPy();
                //
                ManageModOrganizer.RedefineGameMoData();
            }

            ManageOther.SwitchFormMinimizedNormalAll(ManageSettings.ListOfFormsForMinimize());
        }

        private static void UpdateZipmods()
        {

            //run zipmod's check if updater found and only for KK, AI, HS2
            if (!GameData.CurrentGame.IsHaveSideloaderMods || !File.Exists(ManageSettings.KkManagerStandaloneUpdaterExePath()))
            {
                return;
            }

            if (!Manage.ManageSettings.IsMoMode())
            {
                //run updater normal
                ManageProcess.RunProgram(ManageSettings.KkManagerStandaloneUpdaterExePath(), "\"" + ManageSettings.GetCurrentGameDataPath() + "\"");
                return;
            }

            //add updater as new exe in mo list if not exists
            //if (!ManageMO.IsMOcustomExecutableTitleByExeNameExists("StandaloneUpdater"))
            {
                var kkManagerStandaloneUpdater = new ManageModOrganizer.CustomExecutables.CustomExecutable
                {
                    Title = "KKManagerStandaloneUpdater",
                    Binary = ManageSettings.KkManagerStandaloneUpdaterExePath(),
                    Arguments = ManageSettings.GetCurrentGameDataPath(),
                    MoTargetMod = ManageSettings.KKManagerFilesModName()
                };

                var kkManagerFilesModPath = Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), ManageSettings.KKManagerFilesModName());
                if (!Directory.Exists(kkManagerFilesModPath))
                {
                    Directory.CreateDirectory(kkManagerFilesModPath);
                    ManageModOrganizer.WriteMetaIni(
                        kkManagerFilesModPath,
                        categoryNames: "",
                        version: "1.0",
                        comments: "",
                        notes: T._("KKManager's and it's Standalone Updater's new created files stored here")
                        //notes: ManageSettings.KKManagerFilesNotes()
                        );

                    ManageModOrganizer.InsertMod(
                        modname: ManageSettings.KKManagerFilesModName(),
                        modAfterWhichInsert: "Low priority_separator"
                        );
                }

                ManageModOrganizer.InsertCustomExecutable(kkManagerStandaloneUpdater);
            }

            zipmodsGuidList = new ZipmodGUIIds(false);

            //activate all mods with Sideloader modpack inside
            ActivateSideloaderMods(zipmodsGuidList);

            FixKKmanagerUpdaterFailedDownloads();

            ManageProcess.RunProgram(ManageSettings.GetAppMOexePath(), "moshortcut://:" + ManageModOrganizer.GetMOcustomExecutableTitleByExeName("StandaloneUpdater"));

            //restore modlist
            ManageModOrganizer.RestoreModlist();

            //restore zipmods to source mods
            MoveZipModsFromOverwriteToSourceMod();
        }

        /// <summary>
        /// prevents failed kkmanager updater file creation when run from mo
        /// </summary>
        private static void FixKKmanagerUpdaterFailedDownloads()
        {
            Directory.CreateDirectory(ManageSettings.KKManagerDownloadsTempDir());
        }

        private static void UpdateModsInit()
        {
            SharedData.GameData.MainForm.AIGirlHelperTabControl.Enabled = false;
        }

        private static void UpdateModsFinalize()
        {
            SharedData.GameData.MainForm.FoldersInit();

            SharedData.GameData.MainForm.AIGirlHelperTabControl.Enabled = true;
        }

        private static void MoveZipModsFromOverwriteToSourceMod()
        {
            //progressForm = new Form
            //{
            //    StartPosition = FormStartPosition.CenterScreen,
            //    Size = new Size(400, 50),
            //    Text = T._("Sideloader dirs sorting") + "..",
            //    FormBorderStyle = FormBorderStyle.FixedToolWindow
            //};

            //pBar = new ProgressBar
            //{
            //    Dock = DockStyle.Bottom
            //};

            //progressForm.Controls.Add(pBar);
            //progressForm.Show();

            SortZipmodsPacks();

            //if (pBar.Value < pBar.Maximum)
            //{
            //    pBar.Value = pBar.Maximum == 100 ? pBar.Maximum - 1 : pBar.Value + 1;
            //}
            //progressForm.Text = T._("Sorting") + ":" + "Sideloader ModPack - Community UserData";
            SortCommunityUserDataPack();

            AfterZipmodsUpdateCleanDirs();

            //progressForm.Dispose();
        }

        private static void AfterZipmodsUpdateCleanDirs()
        {
            foreach (var sortingDirPath in ManageSettings.GetKKManagerUpdateSortDirs())
            {
                // clean temp dir
                CleanKKManagerTempDir(sortingDirPath);
            }

            // clean temp in data
            CleanKKManagerTempDir(ManageSettings.GetCurrentGameDataPath());
        }

        private static void CleanKKManagerTempDir(string targetDir)
        {
            ManageFilesFoldersExtensions.DeleteEmptySubfolders(targetDir);
        }

        private static void SortCommunityUserDataPack()
        {
            //sort community userdata
            var communityUserDataPack = new DirectoryInfo(Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), "Sideloader ModPack - Community UserData"));

            Parallel.ForEach(ManageSettings.GetKKManagerUpdateSortDirs(), sortingDirPath =>
            {
                var charaDir = new DirectoryInfo(Path.Combine(sortingDirPath, "UserData", "chara"));
                if (!charaDir.Exists)
                {
                    return;
                }

                Parallel.ForEach(new[] { "female", "male" }, genderDirName =>
                {
                    var genderDir = new DirectoryInfo(Path.Combine(charaDir.FullName, genderDirName));
                    if (!genderDir.Exists)
                    {
                        return;
                    }

                    Parallel.ForEach(genderDir.EnumerateDirectories("[Community] *"), charaCommunityDir =>
                    {
                        Parallel.ForEach(charaCommunityDir.EnumerateFiles("*.*", searchOption: SearchOption.AllDirectories), charaFile =>
                        {
                            try
                            {
                                var targetPath = new FileInfo(charaFile.FullName.Replace(sortingDirPath, communityUserDataPack.FullName));

                                MoveSideloaderPackFile(charaFile, targetPath);
                            }
                            catch (Exception ex)
                            {
                                ManageLogs.Log("Failed to sort chara card: " + charaFile.FullName + "\r\nerror:\r\n" + ex);
                            }
                        });
                    });

                    var infoFile = new FileInfo(Path.Combine(genderDir.FullName, "Want your cards or scenes in BetterRepack.txt"));
                    if (infoFile.Exists)
                    {
                        try
                        {
                            var targetPath = new FileInfo(infoFile.FullName.Replace(sortingDirPath, communityUserDataPack.FullName));

                            MoveSideloaderPackFile(infoFile, targetPath);
                        }
                        catch (Exception ex)
                        {
                            ManageLogs.Log("Failed to sort chara card: " + infoFile.FullName + "\r\nerror:\r\n" + ex);
                        }
                    }
                });
            });
        }

        private static void MoveSideloaderPackFile(FileInfo sourcePath, FileInfo targetPath)
        {
            if (targetPath.Exists)
            {
                if (targetPath.Length == sourcePath.Length && targetPath.GetCrc32() == sourcePath.GetCrc32())
                {
                    sourcePath.DeleteReadOnly();
                }
                else
                {
                    targetPath = targetPath.GetNewTargetName();
                }
            }

            if (!targetPath.Directory.Exists)
            {
                targetPath.Directory.Create();
            }
            sourcePath.MoveTo(targetPath.FullName);
        }

        static ZipmodGUIIds zipmodsGuidList;
        //static Form progressForm;
        //static ProgressBar pBar;
        private static void SortZipmodsPacks()
        {
            Parallel.ForEach(ManageSettings.GetKKManagerUpdateSortDirs(), sortingDirPath =>
            {

                var sortingDir = new DirectoryInfo(Path.Combine(sortingDirPath, "mods"));

                if (!sortingDir.Exists)
                {
                    return;
                }

                //var modpackFilters = new Dictionary<string, string>
                //{
                //    { "Sideloader Modpack - KK_UncensorSelector", @"^\[KK\]\[Female\].*$" }//sideloader dir name /files regex filter
                //};

                var dirs = sortingDir.GetDirectories().Where(dirpath => dirpath.Name.StartsWith("Sideloader Modpack", comparisonType: StringComparison.InvariantCultureIgnoreCase) && !ManageSettings.GetKKManagerUpdateSortDirs().Contains(dirpath.Name));

                if (!dirs.Any())
                {
                    return;
                }

                var modpacks = GetSideloaderModpackTargetDirs();

                //pBar.Invoke((Action)(() => pBar.Maximum = dirs.Count() + 1));
                //pBar.Invoke((Action)(() => pBar.Value = 0));

                var timeInfo = ManageSettings.GetDateTimeBasedSuffix();

                Parallel.ForEach(dirs, dir =>
                {
                    //if (pBar.Value < pBar.Maximum)
                    //{
                    //    pBar.Invoke((Action)(() => pBar.Value++));
                    //}

                    //if (!dir.ToUpperInvariant().Contains("SIDELOADER MODPACK"))
                    //{
                    //    continue;
                    //}

                    //set sideloader dir name
                    var sideloaderDirName = dir.Name;

                    //progressForm.Text = T._("Sorting") + ":" + sideloaderDirName;

                    var isUnc = IsUncensorSelector(sideloaderDirName);
                    var isMaleUnc = isUnc && modpacks.ContainsKey(sideloaderDirName + "M");
                    var isFeMaleUnc = isUnc && modpacks.ContainsKey(sideloaderDirName + "F");
                    var isSortingModPack = modpacks.ContainsKey(sideloaderDirName) || isMaleUnc || isFeMaleUnc;
                    Parallel.ForEach(dir.EnumerateFiles("*.*", SearchOption.AllDirectories), f =>
                    {
                        try
                        {
                            var file = f.FullName;

                            // Check if TargetIsInSideloader by guid
                            var guid = ManageArchive.GetZipmodGuid(file);
                            bool isguid = guid.Length > 0 && zipmodsGuidList.GUIDList.ContainsKey(guid);
                            string targetModPath = isguid ? zipmodsGuidList.GUIDList[guid].FileInfo.FullName.GetPathInMods() : "";
                            var pathElements = !string.IsNullOrWhiteSpace(targetModPath) ? file.Replace(sortingDirPath, "").Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries) : null;
                            var targetzipModDirName = pathElements != null && pathElements.Length > 1 ? pathElements[1] : ""; // %modpath%\mods\%sideloadermodpackdir%
                            var targetIsInSideloader = targetzipModDirName.ToUpperInvariant().Contains("SIDELOADER MODPACK"); // dir in mods is sideloader

                            if (isguid && !targetIsInSideloader/*do not touch sideloader files and let them be updated properly*/)//move by guid
                            {
                                var target = file.Replace(sortingDir.FullName, targetModPath);


                                bool isTargetExists = File.Exists(target);
                                bool isSourceExists = File.Exists(file);

                                ManageStrings.CheckForLongPath(ref file);
                                ManageStrings.CheckForLongPath(ref target);

                                if (!isSourceExists)
                                {
                                    return;
                                }
                                else if (isTargetExists)
                                {
                                    var targetinfo = new FileInfo(target);
                                    var sourceinfo = new FileInfo(file);
                                    if (targetinfo.Length == sourceinfo.Length && targetinfo.GetCrc32() == sourceinfo.GetCrc32())
                                    {
                                        sourceinfo.Delete();
                                        return;
                                    }
                                    else if (targetinfo.LastWriteTime < sourceinfo.LastWriteTime)
                                    {
                                        var tfiletarget = file.Replace(sortingDir.FullName, targetModPath + timeInfo);

                                        Directory.CreateDirectory(Path.GetDirectoryName(tfiletarget));
                                        if (!File.Exists(tfiletarget))
                                            File.Move(target, tfiletarget); // move older target file
                                    }
                                    else
                                    {
                                        target = file.Replace(sortingDir.FullName, targetModPath + timeInfo);
                                    }
                                }

                                Directory.CreateDirectory(Path.GetDirectoryName(target));
                                try
                                {
                                    File.Move(file, target);
                                }
                                catch (IOException ex)
                                {
                                    ManageLogs.Log("An error occured while file move. error:\r\n" + ex + "\r\nfile=" + file + "\r\ntarget file=" + target);
                                }
                            }
                            else if (isSortingModPack)
                            {
                                var sortM = isMaleUnc && Path.GetExtension(file).StartsWith(".zip", StringComparison.CurrentCultureIgnoreCase) && (Path.GetFileNameWithoutExtension(file).Contains("[Penis]") || Path.GetFileNameWithoutExtension(file).Contains("[Balls]"));
                                var sortF = isFeMaleUnc && Path.GetExtension(file).StartsWith(".zip", StringComparison.CurrentCultureIgnoreCase) && Path.GetFileNameWithoutExtension(file).Contains("[Female]");

                                var targetModName = modpacks[sideloaderDirName + (sortF ? "F" : sortM ? "M" : "")];

                                //get target path for the zipmod
                                var target = ManageSettings.GetCurrentGameModsDirPath()
                                    + Path.DirectorySeparatorChar + targetModName //mod name
                                    + Path.DirectorySeparatorChar + "mods" //mods dir
                                    + Path.DirectorySeparatorChar + sideloaderDirName // sideloader dir name
                                    + file.Replace(dir.FullName, ""); // file subpath in sideloader dir

                                bool isTargetExists = File.Exists(target);
                                bool isSourceExists = File.Exists(file);

                                ManageStrings.CheckForLongPath(ref file);
                                ManageStrings.CheckForLongPath(ref target);

                                if (!isSourceExists)
                                {
                                    return;
                                }
                                else if (isTargetExists)
                                {
                                    var targetinfo = new FileInfo(target);
                                    var sourceinfo = new FileInfo(file);
                                    if (targetinfo.Length == sourceinfo.Length && targetinfo.GetCrc32() == sourceinfo.GetCrc32())
                                    {
                                        sourceinfo.Delete();
                                        return;
                                    }
                                    else if (targetinfo.LastWriteTime < sourceinfo.LastWriteTime)
                                    {
                                        var tfiletarget = ManageSettings.GetCurrentGameModsDirPath()
                                            + Path.DirectorySeparatorChar + targetModName + timeInfo //mod name
                                            + Path.DirectorySeparatorChar + "mods" //mods dir
                                            + Path.DirectorySeparatorChar + sideloaderDirName // sideloader dir name
                                            + file.Replace(dir.FullName, ""); // file subpath in sideloader dir

                                        Directory.CreateDirectory(Path.GetDirectoryName(tfiletarget));
                                        if (!File.Exists(tfiletarget))
                                            File.Move(target, tfiletarget); // move older target file
                                    }
                                    else
                                    {
                                        target = ManageSettings.GetCurrentGameModsDirPath()
                                            + Path.DirectorySeparatorChar + targetModName + timeInfo //mod name
                                            + Path.DirectorySeparatorChar + "mods" //mods dir
                                            + Path.DirectorySeparatorChar + sideloaderDirName // sideloader dir name
                                            + file.Replace(dir.FullName, ""); // file subpath in sideloader dir
                                    }
                                }


                                Directory.CreateDirectory(Path.GetDirectoryName(target));//create parent dir

                                try
                                {
                                    File.Move(file, target);//move file to the marked mod
                                }
                                catch (IOException ex)
                                {
                                    ManageLogs.Log("An error occured while file move. error:\r\n" + ex + "\r\nfile=" + file + "\r\ntarget file=" + target);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ManageLogs.Log("Failed to sort file " + f + "\r\nerror:\r\n" + ex);
                        }
                    });
                });
            });
        }

        /// <summary>
        /// activate all mods with sideloader modpack inside mods folder
        /// </summary>
        private static void ActivateSideloaderMods(ZipmodGUIIds zipmodsGuidList = null)
        {
            bool getZipmodId = zipmodsGuidList != null;

            // buckup modlist
            File.Copy(ManageSettings.GetCurrentMoProfileModlistPath(), ManageSettings.GetCurrentMoProfileModlistPath() + ".prezipmodsUpdate");

            var modlist = new ModlistProfileInfo();

            Parallel.ForEach(modlist.Items, (item, state) =>
            {
                if (!item.IsExist || item.IsSeparator)
                {
                    return;
                }

                if (Directory.Exists(Path.Combine(item.Path, "mods")))
                {
                    item.IsEnabled = true;

                    if (getZipmodId)
                    {
                        foreach (var packDir in Directory.EnumerateDirectories(Path.Combine(item.Path, "mods"), "Sideloader Modpack*"))
                        {
                            Parallel.ForEach(Directory.EnumerateFiles(packDir, "*.zip*", SearchOption.AllDirectories), zipmod =>
                            {
                                zipmodsGuidList.Load(zipmod);
                                //SaveGuidIfZipMod(zipmod, zipmodsGuidList);
                            });
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else if (!item.IsEnabled && Directory.Exists(Path.Combine(item.Path, "UserData", "chara")))
                {
                    item.IsEnabled = true;
                }
            });

            modlist.Save();
        }
    }
}
