using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIHelper.Manage.ModeSwitch
{
    class ToCommonMode : ModeSwitcherBase
    {
        protected override string DialogText =>
                    T._("Attention")
                    + "\n\n"
                    + T._("Conversation to")
                    + " " + T._("Common mode")
                    + "\n\n"
                    + T._("This will move using mod files from Mods folder to Data folder to make it like common installation variant.\nYou can restore it later back to MO mode.\n\nContinue?");

        protected override void Action()
        {
            SwitchToCommonMode();
        }

        protected override void PreParseFiles()
        {
        }

        protected override bool NeedSkip(string sourceFilePath, string parentFolder)
        {
            return IsExcludedFileType(sourceFilePath, parentFolder);
        }

        protected bool IsExcludedFileType(string sourceFilePath, string parentFolder)
        {
            try
            {
                //skip images and txt in mod root folder
                var fileExtension = Path.GetExtension(sourceFilePath);
                if (string.Equals(fileExtension, ".txt", StringComparison.InvariantCultureIgnoreCase) || fileExtension.IsPictureExtension())
                {
                    if (Path.GetFileName(sourceFilePath.Replace(Path.DirectorySeparatorChar + Path.GetFileName(sourceFilePath), string.Empty)) == Path.GetFileName(parentFolder))
                    {
                        //пропускать картинки и txt в корне папки мода
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("error while image skip. error:" + ex);
            }

            //skip meta.ini
            if (Path.GetFileName(sourceFilePath) == "meta.ini")
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// list of files with log paths
        /// </summary>
        protected ConcurrentBag<string> longPaths;
        /// <summary>
        /// File paths list from vanilla Data folder with no mods
        /// </summary>
        protected string[] vanillaDataFilesList;
        /// <summary>
        /// List of empty folder paths in vanilla Data directory with no mods
        /// </summary>
        protected StringBuilder vanillaDataEmptyFoldersList;
        /// <summary>
        /// список guid zipmod-ов
        /// </summary>
        protected Dictionary<string, string> zipmodsGuidList;
        /// <summary>
        /// список выполненных операций с файлами.
        /// </summary>
        protected StringBuilder moToStandartConvertationOperationsList;
        /// <summary>
        /// True if any files was parsed
        /// </summary>
        protected bool ParsedAny;
        private readonly object moToStandartConvertationOperationsListLocker = new object();

        protected void SwitchToCommonMode()
        {
            // first make game's buckup
            //if (ManageSettings.MainForm.ModeSwitchCreateBuckupLabel.IsChecked())
            if (MakeBuckup)
                new GameBackuper().CreateDataModsBakOfCurrentGame();

            moToStandartConvertationOperationsList = new StringBuilder();
            vanillaDataEmptyFoldersList = new StringBuilder();
            zipmodsGuidList = new Dictionary<string, string>();
            longPaths = new ConcurrentBag<string>();

            try
            {
                ManageModOrganizer.CleanBepInExLinksFromData();

                if (!ManageSettings.MoIsNew)
                {
                    if (File.Exists(ManageSettings.DummyFilePath) && /*Удалил TESV.exe, который был лаунчером, а не болванкой*/new FileInfo(ManageSettings.DummyFilePath).Length < 10000)
                    {
                        File.Delete(ManageSettings.DummyFilePath);
                    }
                }

                Directory.CreateDirectory(ManageSettings.CurrentGameMOmodeDataFilesBakDirPath);

                ManageFilesFoldersExtensions.GetEmptySubfoldersPaths(ManageSettings.CurrentGameDataDirPath, vanillaDataEmptyFoldersList);

                var frmProgress = new Form
                {
                    Size = new Size(200, 50),
                    StartPosition = FormStartPosition.CenterScreen,
                    FormBorderStyle = FormBorderStyle.FixedToolWindow
                };
                var pbProgress = new ProgressBar
                {
                    Dock = DockStyle.Bottom
                };

                frmProgress.Controls.Add(pbProgress);
                frmProgress.Show();

                //DATA files
                vanillaDataFilesList = Directory.GetFiles(ManageSettings.CurrentGameDataDirPath, "*.*", SearchOption.AllDirectories);

                // OVERWRITE
                var sourceFolder = ManageSettings.CurrentGameOverwriteFolderPath;
                frmProgress.Text = T._("Parsing") + ":" + Path.GetFileName(sourceFolder);
                ParseDirectories(sourceFolder, sourceFolder);

                // MODS
                var enabledModNamesList = ManageModOrganizer.GetModNamesListFromActiveMoProfile();
                if (!ParsedAny && enabledModNamesList.Length == 0)
                {
                    MessageBox.Show(T._("There is no enabled mods or files in Overwrite"));
                    return;
                }
                var enabledModsLength = enabledModNamesList.Length;
                pbProgress.Maximum = enabledModsLength;
                for (int m = 0; m < enabledModsLength; m++)
                {
                    if (m < pbProgress.Maximum)
                    {
                        pbProgress.Value = m;
                    }

                    sourceFolder = Path.Combine(ManageSettings.CurrentGameModsDirPath, enabledModNamesList[m]);
                    if (!Directory.Exists(sourceFolder))
                    {
                        continue;
                    }

                    frmProgress.Text = T._("Parsing") + ":" + Path.GetFileName(sourceFolder);

                    ParseDirectories(sourceFolder, sourceFolder);
                }

                pbProgress.Dispose();
                frmProgress.Dispose();

                if (!ParsedAny)
                {
                    MessageBox.Show(T._("Nothing to move"));
                    return;
                }

                ReplacePathsToVars(ref moToStandartConvertationOperationsList);
                File.WriteAllText(ManageSettings.CurrentGameMoToStandartConvertationOperationsListFilePath, moToStandartConvertationOperationsList.ToString());
                moToStandartConvertationOperationsList.Clear();

                var dataWithModsFileslist = Directory.GetFiles(ManageSettings.CurrentGameDataDirPath, "*.*", SearchOption.AllDirectories);
                ReplacePathsToVars(ref dataWithModsFileslist);
                File.WriteAllLines(ManageSettings.CurrentGameModdedDataFilesListFilePath, dataWithModsFileslist);

                ReplacePathsToVars(ref vanillaDataFilesList);
                File.WriteAllLines(ManageSettings.CurrentGameVanillaDataFilesListFilePath, vanillaDataFilesList);

                if (zipmodsGuidList.Count > 0)
                {
                    //using (var file = new StreamWriter(ManageSettings.GetZipmodsGUIDListFilePath()))
                    //{
                    //    foreach (var entry in ZipmodsGUIDList)
                    //    {
                    //        file.WriteLine("{0}{{ZIPMOD}}{1}", entry.Key, entry.Value);
                    //    }
                    //}
                    File.WriteAllLines(ManageSettings.CurrentGameZipmodsGuidListFilePath,
                        zipmodsGuidList.Select(x => x.Key + "{{ZIPMOD}}" + x.Value).ToArray());
                }
                dataWithModsFileslist = null;

                //create normal mode identifier
                SwitchNormalModeIdentifier();


                //записать пути до пустых папок, чтобы при восстановлении восстановить и их
                if (vanillaDataEmptyFoldersList.ToString().Length > 0)
                {
                    ReplacePathsToVars(ref vanillaDataEmptyFoldersList);
                    File.WriteAllText(ManageSettings.CurrentGameVanillaDataEmptyFoldersListFilePath, vanillaDataEmptyFoldersList.ToString());
                }

                MOmode = false;

                MessageBox.Show(T._("All mod files now in Data folder! You can restore MO mode by same button."));
            }
            catch (Exception ex)
            {
                //восстановление файлов в первоначальные папки
                RestoreMovedFilesLocation(moToStandartConvertationOperationsList);

                var emptyDirsList = ReplaceVarsToPaths(vanillaDataEmptyFoldersList.ToString());
                //clean empty folders except whose was already in Data
                ManageFilesFoldersExtensions.DeleteEmptySubfolders(ManageSettings.CurrentGameDataDirPath, false, emptyDirsList.SplitToLines().ToHashSet(), false);

                //сообщить об ошибке
                MessageBox.Show("Mode was not switched. Error:" + Environment.NewLine + ex);
            }
        }

        /// <summary>
        /// Parse files and dirs in <paramref name="sourceFolder"/> using <paramref name="parentDirPath"/>
        /// </summary>
        /// <param name="sourceFolder"></param>
        /// <param name="parentDirPath">Parent directory</param>
        /// <returns></returns>
        protected bool ParseDirectories(string sourceFolder, string parentDirPath)
        {
            ParseFiles(sourceFolder, parentDirPath); // parse files of this directory

            Parallel.ForEach(Directory.EnumerateDirectories(sourceFolder), dir =>
            {
                if (dir.IsSymlink(ObjectType.Directory))
                {
                    ParseDirLink(dir, parentDirPath);
                }
                else
                {
                    ParseDirectories(dir, parentDirPath);
                }
            });

            return true;
        }

        /// <summary>
        /// Parse <paramref name="dir"/>'s symlink using <paramref name="parentDirPath"/>
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="parentDirPath"></param>
        protected void ParseDirLink(string dir, string parentDirPath)
        {
            if (dir.IsValidSymlink(objectType: ObjectType.Directory))
            {
                var symlinkTarget = Path.GetFullPath(dir.GetSymlinkTarget(ObjectType.Directory));

                var targetPath = dir.Replace(parentDirPath, ManageSettings.CurrentGameDataDirPath); // we move to data
                symlinkTarget.CreateSymlink(targetPath, isRelative: Path.GetPathRoot(targetPath) == Path.GetPathRoot(symlinkTarget), objectType: ObjectType.Directory);

                // we not deleted symlink in the dir
                lock (moToStandartConvertationOperationsListLocker)
                {
                    moToStandartConvertationOperationsList.AppendLine(dir + operationsSplitStringBase + targetPath); // add symlink operation
                }

                ParsedAny = true;
            }
            else
            {
                var invalidSymlinkDirMarkerPath = dir + "_InvalidSymLink";
                if (!Directory.Exists(invalidSymlinkDirMarkerPath))
                {
                    Directory.CreateDirectory(invalidSymlinkDirMarkerPath);
                }
            }
        }

        /// <summary>
        /// Parse files in <paramref name="dir"/> using <paramref name="parentDirPath"/>
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="parentDirPath"></param>
        /// <returns></returns>
        protected bool ParseFiles(string dir, string parentDirPath)
        {
            //var sourceFilePaths = Directory.GetFiles(dir, "*.*");
            //if (sourceFilePaths.Length == 0)
            //{
            //    return false;
            //}

            PreParseFiles();

            //var sourceFilePathsLength = sourceFilePaths.Length;
            Parallel.ForEach(Directory.EnumerateFiles(dir, "*.*"), f =>
            {
                var sourceFilePath = f;
                if (ManageStrings.CheckForLongPath(ref sourceFilePath))
                {
                    longPaths.Add(sourceFilePath.Substring(4)); // add to long paths list but with removed long path prefix
                }

                if (NeedSkip(sourceFilePath, parentDirPath))
                {
                    return;
                }

                if (sourceFilePath.IsSymlink(ObjectType.File))
                {
                    ParseFileLink(dir, parentDirPath);
                }
                else
                {
                    ParseFile(sourceFilePath, parentDirPath);
                }
            });

            return true;
        }

        static void ParseFileLink(string linkPath, string parentDirPath)
        {
            if (linkPath.IsValidSymlink(objectType: ObjectType.File))
            {
                var symlinkTarget = Path.GetFullPath(linkPath.GetSymlinkTarget(ObjectType.File));

                var targetPath = linkPath.Replace(parentDirPath, ManageSettings.CurrentGameDataDirPath);
                symlinkTarget.CreateSymlink(targetPath, isRelative: Path.GetPathRoot(targetPath) == Path.GetPathRoot(symlinkTarget), objectType: ObjectType.File);
            }
            else
            {
                var invalidSymlinkDirMarkerPath = linkPath + ".InvalidSymLink";
                if (!File.Exists(invalidSymlinkDirMarkerPath))
                {
                    File.WriteAllText(invalidSymlinkDirMarkerPath, "Symlink file '" + linkPath + "' is invalid!");
                }
            }
        }

        /// <summary>
        /// Parse <paramref name="sourceFilePath"/> using <paramref name="parentDirPath"/>
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="parentDirPath"></param>
        protected void ParseFile(string sourceFilePath, string parentDirPath)
        {
            var dataFilePath = sourceFilePath.Replace(parentDirPath, ManageSettings.CurrentGameDataDirPath);
            if (ManageStrings.CheckForLongPath(ref dataFilePath))
            {
                longPaths.Add(dataFilePath.Substring(4));
            }

            if (File.Exists(dataFilePath))
            {
                var vanillaFileBackupTargetPath = dataFilePath.Replace(ManageSettings.CurrentGameDataDirPath, ManageSettings.CurrentGameMOmodeDataFilesBakDirPath);

                if (File.Exists(vanillaFileBackupTargetPath) || !vanillaDataFilesList.Contains(dataFilePath))
                {
                    return;
                }

                var bakfolder = Path.GetDirectoryName(vanillaFileBackupTargetPath);
                try
                {
                    Directory.CreateDirectory(bakfolder);

                    dataFilePath.MoveTo(vanillaFileBackupTargetPath);//перенос файла из Data в Bak, если там не было

                    ManageModOrganizer.SaveGuidIfZipMod(sourceFilePath, zipmodsGuidList);

                    sourceFilePath.MoveTo(dataFilePath);
                    lock (moToStandartConvertationOperationsListLocker)
                    {
                        moToStandartConvertationOperationsList.AppendLine(sourceFilePath + operationsSplitStringBase + dataFilePath);//запись об операции будет пропущена, если будет какая-то ошибка
                    }

                    ParsedAny = true;
                }
                catch (Exception ex)
                {
                    // when file is not exist in Data, but file in Bak is exist and file in sourceFolder also exists => return file from Bak to Data
                    if (!File.Exists(dataFilePath) && File.Exists(vanillaFileBackupTargetPath) && File.Exists(sourceFilePath))
                    {
                        File.Move(vanillaFileBackupTargetPath, dataFilePath);
                    }

                    _log.Error("Error occured while to common mode switch:" + Environment.NewLine + ex + "\r\npath=" + bakfolder + "\r\nData path=" + dataFilePath + "\r\nSource dir path=" + sourceFilePath);
                }
            }
            else
            {
                var destFolder = Path.GetDirectoryName(dataFilePath);
                try
                {
                    Directory.CreateDirectory(destFolder);

                    ManageModOrganizer.SaveGuidIfZipMod(sourceFilePath, zipmodsGuidList);

                    sourceFilePath.MoveTo(dataFilePath);//перенос файла из папки мода в Data
                    lock (moToStandartConvertationOperationsListLocker)
                    {
                        moToStandartConvertationOperationsList.AppendLine(sourceFilePath + operationsSplitStringBase + dataFilePath);//запись об операции будет пропущена, если будет какая-то ошибка
                    }

                    ParsedAny = true;
                }
                catch (Exception ex)
                {
                    _log.Error("Error occured while to common mode switch:" + Environment.NewLine + ex + "\r\npath=" + destFolder + "\r\nData path=" + dataFilePath + "\r\nSource dir path=" + sourceFilePath);
                }
            }
        }

        void RestoreMovedFilesLocation(StringBuilder operations)
        {
            if (operations.Length == 0)
            {
                return;
            }

            Parallel.ForEach(operations.ToString().SplitToLines(), record =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(record))
                    {
                        return;
                    }

                    var movePaths = record.Split(operationsSplitString, StringSplitOptions.None);

                    if (movePaths.Length != 2)
                    {
                        return;
                    }

                    var filePathInMods = movePaths[0];
                    var filePathInData = movePaths[1];

                    if (!File.Exists(filePathInData))
                    {
                        return;
                    }

                    if (File.Exists(filePathInMods))
                    {
                        return;
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(filePathInMods));

                    filePathInData.MoveTo(filePathInMods);
                }
                catch (Exception ex)
                {
                    _log.Error("error while RestoreMovedFilesLocation. error:\r\n" + ex);
                }
            });

            //возврат возможных ванильных резервных копий
            MoveVanillaFilesBackToData();
        }
    }
}
