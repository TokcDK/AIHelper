﻿using NLog;
using SharpCompress.Common;
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
        // 1. Обойти все папки модов одну за другой через метод ParseFolder(inputDirPath, sourceModDirPath), где sourceModDirPath это путь к папке мода, при первом использовании inputDirPath и sourceModDirPath равны
        // 2. В ParseFolder(inputDirPath, sourceModDirPath) сделать следующее:
        //  2.1 Если по пути inputDirPath находится invalid symbolic link на папку - пропустить
        //  2.2 Обойти все файлы и для каждого файла вызвать ParseFile(inputFilePath, sourceModDirPath)
        //   2.2.1 В ParseFile(inputFilePath, sourceModDirPath) сделать следующее:
        //    2.2.1.0 Если по пути inputFilePath находится invalid symbolic link на файл - пропустить
        //    2.2.1.1 Получить targetFilePath заменив в inputFilePath часть пути равную sourceModDirPath на путь к папке Data из переменной currentGameDataPath
        //    2.2.1.2 Если по пути targetFilePath уже присутствует файл - переместить его в папку backupDataPath в той же подпапке и только после этого обрабатывать файл
        //    2.2.1.3 Если inputFilePath на другом диске - создать для него символическую ссылку по пути targetFilePath с symbolik link target указывающий на inputFilePath
        //    2.2.1.4 Если inputFilePath это символическая ссылка - пересоздать символическую ссылку на новом месте с той же symbolik link target как и у inputFilePath, преобразовав путь в абсолютный
        //
        //  2.3 Далее в ParseFolder(inputDirPath, sourceModDirPath) обойти все папки и для каждой папки вызвать ParseFolder(inputDirPath, sourceModDirPath), где inputDirPath это путь к папке, а sourceModDirPath c тем же значением sourceModDirPath

        const string ZIPMOD_RECORD_SPLITTER = "{{ZIPMOD}}";

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

        protected override bool NeedSkip(string sourceFilePath, ParentSourceModData parentSourceModDir)
        {
            return IsExcludedFileType(sourceFilePath, parentSourceModDir);
        }

        protected bool IsExcludedFileType(string sourceFilePath, ParentSourceModData parentSourceModDir)
        {
            try
            {
                //skip images and txt in mod root folder
                var fileExtension = Path.GetExtension(sourceFilePath);
                if (string.Equals(fileExtension, ".txt", StringComparison.InvariantCultureIgnoreCase) || fileExtension.IsPictureExtension())
                {
                    if (Path.GetFileName(sourceFilePath.Replace(Path.DirectorySeparatorChar + Path.GetFileName(sourceFilePath), string.Empty)) == Path.GetFileName(parentSourceModDir.Path))
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
        protected HashSet<string> vanillaDataFilesList;
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
        protected HashSet<string> MovedModFiles = new HashSet<string>();
        /// <summary>
        /// True if any files was parsed
        /// </summary>
        protected bool ParsedAny;
        private readonly object moToStandartConvertationOperationsListLocker = new object();

        protected void SwitchToCommonMode()
        {
            InitVars();

            try
            {
                MoveFilesToData();

                MOmode = false;

                MessageBox.Show(T._("All mod files are now in the Data folder! You can restore MO mode by clicking the same button."));
            }
            catch (Exception ex)
            {
                RevertChangesBackToMOmode();

                // Display an error message
                MessageBox.Show("The mode was not switched. Error:" + Environment.NewLine + ex);
            }
        }

        private void InitVars()
        {
            moToStandartConvertationOperations = new StringBuilder();
            vanillaDataEmptyFolders = new StringBuilder();
            zipmodsGUIDs = new Dictionary<string, string>();
            longPaths = new ConcurrentBag<string>();
        }

        private void MoveFilesToData()
        {// Clean BepInEx links from data
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

            // Get vanilla data files
            var vanillaDataFiles = GetVanillaDataFilesList();

            MoveAllEnabledDirsAndFiles();

            if (!ParsedAny)
            {
                MessageBox.Show(T._("There are no files to move"));
                return;
            }

            WriteRevertToMOModeData(vanillaDataFiles);
        }

        private string[] GetVanillaDataFilesList()
        {
            var vanillaDataFiles = Directory.GetFiles(ManageSettings.CurrentGameDataDirPath, "*.*", SearchOption.AllDirectories);
            vanillaDataFilesList = vanillaDataFiles.ToHashSet();

            return vanillaDataFiles;
        }

        private void WriteRevertToMOModeData(string[] vanillaDataFiles)
        {
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

            WriteZipmodGUIDs();

            dataWithModsFiles = null;
            vanillaDataFiles = null;

            // Create the normal mode identifier
            SwitchNormalModeIdentifier();

            // Write the paths of empty folders to a file, so they can be restored later
            if (vanillaDataEmptyFolders.ToString().Length > 0)
            {
                ReplacePathsToVars(ref vanillaDataEmptyFolders);
                File.WriteAllText(ManageSettings.CurrentGameVanillaDataEmptyFoldersListFilePath, vanillaDataEmptyFolders.ToString());
            }
        }

        private void WriteZipmodGUIDs()
        {
            if (zipmodsGUIDs.Count == 0)
            {
                return;
            }

            // Write the zipmods GUID list to a file
            File.WriteAllLines(ManageSettings.CurrentGameZipmodsGuidListFilePath,
                zipmodsGUIDs.Select(x => x.Key + ZIPMOD_RECORD_SPLITTER + x.Value).ToArray());
        }

        private void MoveAllEnabledDirsAndFiles()
        {
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

            MoveOverwriteDirsAndFiles(frmProgress, pbProgress);

            MoveEnabledModsDirsAndFiles(frmProgress, pbProgress);

            pbProgress.Dispose();
            frmProgress.Dispose();
        }

        private void MoveEnabledModsDirsAndFiles(Form frmProgress, ProgressBar pbProgress)
        {
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

                string sourceFolder = Path.Combine(ManageSettings.CurrentGameModsDirPath, enabledModNames[i]);
                if (!Directory.Exists(sourceFolder))
                {
                    continue;
                }

                frmProgress.Text = T._("Parsing") + ":" + Path.GetFileName(sourceFolder);

                var modDirData = new ParentSourceModData(sourceFolder)
                {
                    IsSymlink = sourceFolder.IsSymlink(ObjectType.Directory)
                };
                ParseDirectoryFiles(sourceFolder, modDirData);
            }
        }

        private void MoveOverwriteDirsAndFiles(Form frmProgress, ProgressBar pbProgress)
        {
            // Parse the OVERWRITE folder
            string sourceFolder = ManageSettings.CurrentGameOverwriteFolderPath;
            frmProgress.Text = T._("Parsing") + ":" + Path.GetFileName(sourceFolder);

            var parentSourceModData = new ParentSourceModData(sourceFolder);

            ParseDirectoryFiles(sourceFolder, parentSourceModData);
        }

        private void RevertChangesBackToMOmode()
        {
            // Restore moved files to their original locations
            RestoreMovedFilesLocation(moToStandartConvertationOperations);

            // Get the list of empty directories
            var emptyDirsList = ReplaceVarsToPaths(vanillaDataEmptyFolders.ToString());
            // Clean empty folders except those that were already in the Data folder
            ManageFilesFoldersExtensions.DeleteEmptySubfolders(ManageSettings.CurrentGameDataDirPath, false, emptyDirsList.SplitToLines().ToHashSet(), false);
        }

        /// <summary>
        /// Parse files and dirs in <paramref name="sourceFolderPath"/> using <paramref name="parentSourceModDir"/>
        /// </summary>
        /// <param name="sourceFolderPath"></param>
        /// <param name="parentSourceModDir">Parent directory</param>
        /// <returns></returns>
        protected bool ParseDirectoryFiles(string sourceFolderPath, ParentSourceModData parentSourceModDir)
        {
            ParseFiles(sourceFolderPath, parentSourceModDir); // parse files of this directory

            bool isModSymlink = parentSourceModDir.IsSymlink;

            Parallel.ForEach(Directory.EnumerateDirectories(sourceFolderPath), dir =>
            {
                ParseDirectoryByType(dir, parentSourceModDir);
            });

            return true;
        }

        private void ParseDirectoryByType(string dir, ParentSourceModData parentSourceModDir)
        {
            if (dir.IsSymlink(ObjectType.Directory))
            {
                ParseDirLink(dir, parentSourceModDir);
            }
            else
            {
                ParseDirectoryFiles(dir, parentSourceModDir);
            }
        }

        /// <summary>
        /// Parse <paramref name="dir"/>'s symlink using <paramref name="parentDirPath"/>
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="parentDirPath"></param>
        protected void ParseDirLink(string dir, ParentSourceModData parentSourceModDir)
        {
            if (dir.IsValidSymlink(objectType: ObjectType.Directory))
            {
                var symlinkTarget = Path.GetFullPath(dir.GetSymlinkTarget(ObjectType.Directory));

                var targetPath = dir.Replace(parentSourceModDir.Path, ManageSettings.CurrentGameDataDirPath); // we move to data
                symlinkTarget.CreateSymlink(targetPath, isRelative: Path.GetPathRoot(targetPath) == Path.GetPathRoot(symlinkTarget), objectType: ObjectType.Directory);

                // we not deleted symlink in the dir
                AppendOperation(dir, targetPath);

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

        private void AppendOperation(string sourcePath, string targetPath)
        {
            lock (moToStandartConvertationOperationsListLocker)
            {
                moToStandartConvertationOperations.AppendLine(sourcePath + operationsSplitStringBase + targetPath); // add symlink operation
                MovedModFiles.Add(targetPath);
            }
        }

        /// <summary>
        /// Parse files in <paramref name="parentDir"/> using <paramref name="parentSourceModDir"/>
        /// </summary>
        /// <param name="parentDir"></param>
        /// <param name="parentSourceModDir"></param>
        /// <returns></returns>
        protected bool ParseFiles(string parentDir, ParentSourceModData parentSourceModDir)
        {
            //var sourceFilePaths = Directory.GetFiles(dir, "*.*");
            //if (sourceFilePaths.Length == 0)
            //{
            //    return false;
            //}

            PreParseFiles();

            //var sourceFilePathsLength = sourceFilePaths.Length;
            Parallel.ForEach(Directory.EnumerateFiles(parentDir, "*.*"), f =>
            {
                ParseFileA(f, parentSourceModDir);
            });

            return true;
        }

        private void ParseFileA(string filePath, ParentSourceModData parentSourceModDir)
        {
            var sourceFilePath = filePath;
            if (ManageStrings.CheckForLongPath(ref sourceFilePath))
            {
                longPaths.Add(sourceFilePath.Substring(4)); // add to long paths list but with removed long path prefix
            }

            if (NeedSkip(sourceFilePath, parentSourceModDir))
            {
                return;
            }

            ParseFile(sourceFilePath, parentSourceModDir);
        }

        static void ParseFileLink(string fileSymLinkPath, ParentSourceModData parentSourceModDir)
        {
            if (fileSymLinkPath.IsValidSymlink(objectType: ObjectType.File))
            {
                var symlinkTarget = Path.GetFullPath(fileSymLinkPath.GetSymlinkTarget(ObjectType.File));

                var targetFilePath = fileSymLinkPath.Replace(parentSourceModDir.Path, ManageSettings.CurrentGameDataDirPath);
                symlinkTarget.CreateSymlink(targetFilePath, isRelative: Path.GetPathRoot(targetFilePath) == Path.GetPathRoot(symlinkTarget), objectType: ObjectType.File);
            }
            else
            {
                var invalidSymlinkDirMarkerPath = fileSymLinkPath + ".InvalidSymLink";
                if (!File.Exists(invalidSymlinkDirMarkerPath))
                {
                    File.WriteAllText(invalidSymlinkDirMarkerPath, "Symlink file '" + fileSymLinkPath + "' is invalid!");
                }
            }
        }

        /// <summary>
        /// Parse <paramref name="sourceFilePath"/> using <paramref name="parentSourceModDir"/>
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="parentSourceModDir"></param>
        protected void ParseFile(string sourceFilePath, ParentSourceModData parentSourceModDir)
        {
            string parentDirPath = Path.GetDirectoryName(sourceFilePath);
            if (parentSourceModDir.IsSymlink && parentSourceModDir.Path == Path.GetDirectoryName(sourceFilePath))
            {
                // the files and dirs under symlink mod can be on another disk
                // need only make symlink to the dir
                string targetFilePath = sourceFilePath.Replace(parentDirPath, ManageSettings.CurrentGameDataDirPath);

                ParseFileCheckInData(sourceFilePath, targetFilePath, forceCreateSymlink: true);

                return;
            }

            if (sourceFilePath.IsSymlink(ObjectType.File))
            {
                // parse as file symlink instead
                ParseFileLink(sourceFilePath, parentSourceModDir);

                return;
            }

            var dataFilePath = sourceFilePath.Replace(parentSourceModDir.Path, ManageSettings.CurrentGameDataDirPath);
            if (ManageStrings.CheckForLongPath(ref dataFilePath))
            {
                longPaths.Add(dataFilePath.Substring(4));
            }

            ParseFileCheckInData(sourceFilePath, dataFilePath);
        }

        private void ParseFileCheckInData(string sourceFilePath, string targetFilePath, bool forceCreateSymlink = false)
        {
            if (File.Exists(targetFilePath))
            {
                ParseFileExistInData(sourceFilePath, targetFilePath, forceCreateSymlink);
            }
            else
            {
                ParseFileMissingInData(sourceFilePath, targetFilePath, forceCreateSymlink);
            }
        }

        private void ParseFileMissingInData(string sourceFilePath, string dataFilePath, bool forceCreateSymlink = false)
        {
            var destFolder = forceCreateSymlink ? "" : Path.GetDirectoryName(dataFilePath);
            if (!forceCreateSymlink)
            {
                Directory.CreateDirectory(destFolder);
            }
            try
            {

                MoveFileToData(sourceFilePath, dataFilePath, forceCreateSymlink);

                ParsedAny = true;
            }
            catch (Exception ex)
            {
                _log.Error("Error occured while to common mode switch:" + Environment.NewLine + ex + "\r\npath=" + destFolder + "\r\nData path=" + dataFilePath + "\r\nSource dir path=" + sourceFilePath);
            }
        }

        private void MoveFileToData(string sourceFilePath, string dataFilePath, bool forceCreateSymlink)
        {
            ManageModOrganizer.SaveGuidIfZipMod(sourceFilePath, zipmodsGUIDs);

            if (!forceCreateSymlink)
            {
                sourceFilePath.MoveTo(dataFilePath);//перенос файла из папки мода в Data
            }
            else
            {
                // create symlink
                sourceFilePath.CreateSymlink(dataFilePath, isRelative: false, objectType: ObjectType.File);
            }

            AppendOperation(sourceFilePath, dataFilePath);//запись об операции будет пропущена, если будет какая-то ошибка
        }

        private void ParseFileExistInData(string sourceFilePath, string dataFilePath, bool forceCreateSymlink = false)
        {
            var vanillaFileBackupTargetPath = dataFilePath.Replace(ManageSettings.CurrentGameDataDirPath, ManageSettings.CurrentGameMOmodeDataFilesBakDirPath);

            if (File.Exists(vanillaFileBackupTargetPath) || !vanillaDataFilesList.Contains(dataFilePath))
            {
                // skip if file already was moved from mod with higher priority and was already exist in data, was created bak path
                return;
            }

            var bakfolder = forceCreateSymlink ? "" : Path.GetDirectoryName(vanillaFileBackupTargetPath);
            if (!forceCreateSymlink)
            {
                Directory.CreateDirectory(bakfolder);
            }
            try
            {
                dataFilePath.MoveTo(vanillaFileBackupTargetPath); // перенос файла из Data в Bak, если там не было

                MoveFileToData(sourceFilePath, dataFilePath, forceCreateSymlink);

                AppendOperation(sourceFilePath, dataFilePath);// запись об операции будет пропущена, если будет какая-то ошибка

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

        void RestoreMovedFilesLocation(StringBuilder operations)
        {
            if (operations.Length == 0)
            {
                return;
            }

            Parallel.ForEach(operations.ToString().SplitToLines(), record =>
            {
                RestoreFileByRecord(record);
            });

            //возврат возможных ванильных резервных копий
            MoveVanillaFilesBackToData();
        }

        private void RestoreFileByRecord(string record)
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
        }
    }
}
