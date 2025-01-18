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
        protected StringBuilder vanillaDataEmptyFolders;
        /// <summary>
        /// список guid zipmod-ов
        /// </summary>
        protected Dictionary<string, string> zipmodsGUIDs;
        /// <summary>
        /// список выполненных операций с файлами.
        /// </summary>
        protected StringBuilder moToStandartConvertationOperations;
        /// <summary>
        /// True if any files was parsed
        /// </summary>
        protected bool ParsedAny;
        private readonly object moToStandartConvertationOperationsListLocker = new object();

        protected void SwitchToCommonMode()
        {
            // First, create a backup of the game
            if (MakeBuckup)
                new GameBackuper().CreateDataModsBakOfCurrentGame();

            moToStandartConvertationOperations = new StringBuilder();
            vanillaDataEmptyFolders = new StringBuilder();
            zipmodsGUIDs = new Dictionary<string, string>();
            longPaths = new ConcurrentBag<string>();

            try
            {
                // Clean BepInEx links from data
                ManageModOrganizer.CleanBepInExLinksFromData();

                if (!ManageSettings.MoIsNew)
                {
                    // Delete TESV.exe, which was the game launcher and not a placeholder
                    if (File.Exists(ManageSettings.DummyFilePath) && new FileInfo(ManageSettings.DummyFilePath).Length < 10000)
                    {
                        File.Delete(ManageSettings.DummyFilePath);
                    }
                }

                // Create the backup directory
                Directory.CreateDirectory(ManageSettings.CurrentGameMOmodeDataFilesBakDirPath);

                // Get the paths of empty subfolders in the game data directory
                ManageFilesFoldersExtensions.GetEmptySubfoldersPaths(ManageSettings.CurrentGameDataDirPath, vanillaDataEmptyFolders);

                // Initialize progress bar
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

                // Get vanilla data files
                var vanillaDataFiles = Directory.GetFiles(ManageSettings.CurrentGameDataDirPath, "*.*", SearchOption.AllDirectories);

                // Parse the OVERWRITE folder
                var sourceFolder = ManageSettings.CurrentGameOverwriteFolderPath;
                frmProgress.Text = T._("Parsing") + ":" + Path.GetFileName(sourceFolder);
                ParseDirectories(sourceFolder, sourceFolder);

                // Parse the enabled mods
                var enabledModNames = ManageModOrganizer.GetModNamesListFromActiveMoProfile();
                if (!ParsedAny && enabledModNames.Length == 0)
                {
                    MessageBox.Show(T._("There are no enabled mods or files in the Overwrite folder"));
                    return;
                }
                var numEnabledMods = enabledModNames.Length;
                pbProgress.Maximum = numEnabledMods;
                for (int i = 0; i < numEnabledMods; i++)
                {
                    if (i < pbProgress.Maximum)
                    {
                        pbProgress.Value = i;
                    }

                    sourceFolder = Path.Combine(ManageSettings.CurrentGameModsDirPath, enabledModNames[i]);
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
                    MessageBox.Show(T._("There are no files to move"));
                    return;
                }

                ReplacePathsToVars(ref moToStandartConvertationOperations);
                File.WriteAllText(ManageSettings.CurrentGameMoToStandartConvertationOperationsListFilePath, moToStandartConvertationOperations.ToString());
                moToStandartConvertationOperations.Clear();

                // Write the paths of the modded data files to a file
                var dataWithModsFiles = Directory.GetFiles(ManageSettings.CurrentGameDataDirPath, "*.*", SearchOption.AllDirectories);
                ReplacePathsToVars(ref dataWithModsFiles);
                File.WriteAllLines(ManageSettings.CurrentGameModdedDataFilesListFilePath, dataWithModsFiles);

                // Write the paths of the vanilla data files to a file
                ReplacePathsToVars(ref vanillaDataFiles);
                File.WriteAllLines(ManageSettings.CurrentGameVanillaDataFilesListFilePath, vanillaDataFiles);

                if (zipmodsGUIDs.Count > 0)
                {
                    // Write the zipmods GUID list to a file
                    File.WriteAllLines(ManageSettings.CurrentGameZipmodsGuidListFilePath,
                        zipmodsGUIDs.Select(x => x.Key + "{{ZIPMOD}}" + x.Value).ToArray());
                }
                dataWithModsFiles = null;

                // Create the normal mode identifier
                SwitchNormalModeIdentifier();

                // Write the paths of empty folders to a file, so they can be restored later
                if (vanillaDataEmptyFolders.ToString().Length > 0)
                {
                    ReplacePathsToVars(ref vanillaDataEmptyFolders);
                    File.WriteAllText(ManageSettings.CurrentGameVanillaDataEmptyFoldersListFilePath, vanillaDataEmptyFolders.ToString());
                }

                MOmode = false;

                MessageBox.Show(T._("All mod files are now in the Data folder! You can restore MO mode by clicking the same button."));
            }
            catch (Exception ex)
            {
                // Restore moved files to their original locations
                RestoreMovedFilesLocation(moToStandartConvertationOperations);

                // Get the list of empty directories
                var emptyDirsList = ReplaceVarsToPaths(vanillaDataEmptyFolders.ToString());
                // Clean empty folders except those that were already in the Data folder
                ManageFilesFoldersExtensions.DeleteEmptySubfolders(ManageSettings.CurrentGameDataDirPath, false, emptyDirsList.SplitToLines().ToHashSet(), false);

                // Display an error message
                MessageBox.Show("The mode was not switched. Error:" + Environment.NewLine + ex);
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
                ParseDirectoryA(dir, parentDirPath);
            });

            return true;
        }

        private void ParseDirectoryA(string dir, string parentDirPath)
        {
            if (dir.IsSymlink(ObjectType.Directory))
            {
                ParseDirLink(dir, parentDirPath);
            }
            else
            {
                ParseDirectories(dir, parentDirPath);
            }
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
                    moToStandartConvertationOperations.AppendLine(dir + operationsSplitStringBase + targetPath); // add symlink operation
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
                ParseFileA(dir, parentDirPath, f);
            });

            return true;
        }

        private void ParseFileA(string dir, string parentDirPath, string filePath)
        {
            var sourceFilePath = filePath;
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

                    ManageModOrganizer.SaveGuidIfZipMod(sourceFilePath, zipmodsGUIDs);

                    sourceFilePath.MoveTo(dataFilePath);
                    lock (moToStandartConvertationOperationsListLocker)
                    {
                        moToStandartConvertationOperations.AppendLine(sourceFilePath + operationsSplitStringBase + dataFilePath);//запись об операции будет пропущена, если будет какая-то ошибка
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

                    ManageModOrganizer.SaveGuidIfZipMod(sourceFilePath, zipmodsGUIDs);

                    sourceFilePath.MoveTo(dataFilePath);//перенос файла из папки мода в Data
                    lock (moToStandartConvertationOperationsListLocker)
                    {
                        moToStandartConvertationOperations.AppendLine(sourceFilePath + operationsSplitStringBase + dataFilePath);//запись об операции будет пропущена, если будет какая-то ошибка
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
