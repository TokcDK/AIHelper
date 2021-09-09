using System;
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

        protected override bool NeedSkip(string sourceFilePath, string sourceFolder)
        {
            return IsExcludedFileType(sourceFilePath, sourceFolder);
        }

        protected bool IsExcludedFileType(string sourceFilePath, string sourceFolder)
        {
            try
            {
                //skip images and txt in mod root folder
                var fileExtension = Path.GetExtension(sourceFilePath);
                if (string.Equals(fileExtension, ".txt", StringComparison.InvariantCultureIgnoreCase) || fileExtension.IsPictureExtension())
                {
                    if (Path.GetFileName(sourceFilePath.Replace(Path.DirectorySeparatorChar + Path.GetFileName(sourceFilePath), string.Empty)) == Path.GetFileName(sourceFolder))
                    {
                        //пропускать картинки и txt в корне папки мода
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                ManageLogs.Log("error while image skip. error:" + ex);
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
        protected List<string> longPaths;
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

        protected void SwitchToCommonMode()
        {
            moToStandartConvertationOperationsList = new StringBuilder();
            vanillaDataEmptyFoldersList = new StringBuilder();
            zipmodsGuidList = new Dictionary<string, string>();
            longPaths = new List<string>();

            var debugString = "";
            try
            {
                ManageModOrganizer.CleanBepInExLinksFromData();

                if (!ManageSettings.MoIsNew)
                {
                    if (File.Exists(ManageSettings.GetDummyFilePath()) && /*Удалил TESV.exe, который был лаунчером, а не болванкой*/new FileInfo(ManageSettings.GetDummyFilePath()).Length < 10000)
                    {
                        File.Delete(ManageSettings.GetDummyFilePath());
                    }
                }

                debugString = ManageSettings.GetCurrentGameMOmodeDataFilesBakDirPath();
                Directory.CreateDirectory(ManageSettings.GetCurrentGameMOmodeDataFilesBakDirPath());
                moToStandartConvertationOperationsList = new StringBuilder();

                ManageFilesFoldersExtensions.GetEmptySubfoldersPaths(ManageSettings.GetCurrentGameDataPath(), vanillaDataEmptyFoldersList);

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
                vanillaDataFilesList = Directory.GetFiles(ManageSettings.GetCurrentGameDataPath(), "*.*", SearchOption.AllDirectories);

                // OVERWRITE
                var sourceFolder = ManageSettings.GetCurrentGameOverwriteFolderPath();
                frmProgress.Text = T._("Move files") + ":" + Path.GetFileName(sourceFolder);
                ParseFiles(sourceFolder, sourceFolder);

                // MODS
                string[] enabledModsList = ManageModOrganizer.GetModNamesListFromActiveMoProfile();
                if (enabledModsList.Length == 0)
                {
                    MessageBox.Show(T._("There is no enabled mods or files in Overwrite"));
                    return;
                }
                var enabledModsLength = enabledModsList.Length;
                pbProgress.Maximum = enabledModsLength;
                for (int m = 0; m < enabledModsLength; m++)
                {
                    if (m < pbProgress.Maximum)
                    {
                        pbProgress.Value = m;
                    }

                    sourceFolder = Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), enabledModsList[m]);
                    if (string.IsNullOrWhiteSpace(sourceFolder) || !Directory.Exists(sourceFolder))
                    {
                        continue;
                    }

                    frmProgress.Text = T._("Move files") + ":" + Path.GetFileName(sourceFolder);

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
                File.WriteAllText(ManageSettings.GetCurrentGameMoToStandartConvertationOperationsListFilePath(), moToStandartConvertationOperationsList.ToString());
                moToStandartConvertationOperationsList.Clear();

                var dataWithModsFileslist = Directory.GetFiles(ManageSettings.GetCurrentGameDataPath(), "*.*", SearchOption.AllDirectories);
                ReplacePathsToVars(ref dataWithModsFileslist);
                File.WriteAllLines(ManageSettings.GetCurrentGameModdedDataFilesListFilePath(), dataWithModsFileslist);

                ReplacePathsToVars(ref vanillaDataFilesList);
                File.WriteAllLines(ManageSettings.GetCurrentGameVanillaDataFilesListFilePath(), vanillaDataFilesList);

                if (zipmodsGuidList.Count > 0)
                {
                    //using (var file = new StreamWriter(ManageSettings.GetZipmodsGUIDListFilePath()))
                    //{
                    //    foreach (var entry in ZipmodsGUIDList)
                    //    {
                    //        file.WriteLine("{0}{{ZIPMOD}}{1}", entry.Key, entry.Value);
                    //    }
                    //}
                    File.WriteAllLines(ManageSettings.GetCurrentGameZipmodsGuidListFilePath(),
                        zipmodsGuidList.Select(x => x.Key + "{{ZIPMOD}}" + x.Value).ToArray());
                }
                dataWithModsFileslist = null;

                //create normal mode identifier
                SwitchNormalModeIdentifier();


                //записать пути до пустых папок, чтобы при восстановлении восстановить и их
                if (vanillaDataEmptyFoldersList.ToString().Length > 0)
                {
                    ReplacePathsToVars(ref vanillaDataEmptyFoldersList);
                    File.WriteAllText(ManageSettings.GetCurrentGameVanillaDataEmptyFoldersListFilePath(), vanillaDataEmptyFoldersList.ToString());
                }

                MOmode = false;

                MessageBox.Show(T._("All mod files now in Data folder! You can restore MO mode by same button."));
            }
            catch (Exception ex)
            {
                //восстановление файлов в первоначальные папки
                RestoreMovedFilesLocation(moToStandartConvertationOperationsList);

                //clean empty folders except whose was already in Data
                ManageFilesFoldersExtensions.DeleteEmptySubfolders(ManageSettings.GetCurrentGameDataPath(), false, vanillaDataEmptyFoldersList.ToString().SplitToLines().ToArray());

                //сообщить об ошибке
                MessageBox.Show("Mode was not switched. Error:" + Environment.NewLine + ex + "\r\n/debufStr=" + debugString);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFolder"></param>
        /// <param name="parentDir">Parent directory</param>
        /// <returns></returns>
        protected bool ParseDirectories(string sourceFolder, string parentDir)
        {
            Parallel.ForEach(Directory.GetDirectories(sourceFolder), dir =>
            {
                if (dir.IsSymlink(ObjectType.Directory))
                {
                    ParseDirLink(dir, parentDir);
                }
                else
                {
                    ParseFiles(dir, parentDir);
                    ParseDirectories(sourceFolder, parentDir);
                }
            });

            return true;
        }

        protected void ParseDirLink(string dir, string parentDir)
        {
            if (dir.IsValidSymlink())
            {
                var symlinkTarget = Path.GetFullPath(dir.GetSymlinkTarget());

                var targetPath = dir.Replace(parentDir, ManageSettings.GetCurrentGameDataPath()); // we move to data
                symlinkTarget.CreateSymlink(targetPath, isRelative: true, objectType: ObjectType.Directory);
            }
        }

        protected bool ParseFiles(string dir, string parentDir)
        {
            var sourceFilePaths = Directory.GetFiles(dir, "*.*");
            if (sourceFilePaths.Length == 0)
            {
                return false;
            }

            PreParseFiles();

            var sourceFilePathsLength = sourceFilePaths.Length;
            _ = Parallel.For(0, sourceFilePathsLength, f =>
            {
                 var sourceFilePath = sourceFilePaths[f];
                 if (ManageStrings.CheckForLongPath(ref sourceFilePath))
                 {
                     longPaths.Add(sourceFilePath.Remove(0, 4)); // add to lng paths list but with removed long path prefix
                 }

                 if (NeedSkip(sourceFilePath, parentDir))
                 {
                     return;
                 }

                 ParseFile(sourceFilePath, parentDir);
            });

            return true;
        }

        protected void ParseFile(string sourceFilePath, string sourceFolder)
        {
            var dataFilePath = sourceFilePath.Replace(sourceFolder, ManageSettings.GetCurrentGameDataPath());
            if (ManageStrings.CheckForLongPath(ref dataFilePath))
            {
                longPaths.Add(dataFilePath.Remove(0, 4));
            }

            if (File.Exists(dataFilePath))
            {
                var vanillaFileBackupTargetPath = dataFilePath.Replace(ManageSettings.GetCurrentGameDataPath(), ManageSettings.GetCurrentGameMOmodeDataFilesBakDirPath());

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
                    moToStandartConvertationOperationsList.AppendLine(sourceFilePath + operationsSplitStringBase + dataFilePath);//запись об операции будет пропущена, если будет какая-то ошибка
                    ParsedAny = true;
                }
                catch (Exception ex)
                {
                    // when file is not exist in Data, but file in Bak is exist and file in sourceFolder also exists => return file from Bak to Data
                    if (!File.Exists(dataFilePath) && File.Exists(vanillaFileBackupTargetPath) && File.Exists(sourceFilePath))
                    {
                        File.Move(vanillaFileBackupTargetPath, dataFilePath);
                    }

                    ManageLogs.Log("Error occured while to common mode switch:" + Environment.NewLine + ex + "\r\npath=" + bakfolder + "\r\nData path=" + dataFilePath + "\r\nSource dir path=" + sourceFilePath);
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
                    moToStandartConvertationOperationsList.AppendLine(sourceFilePath + operationsSplitStringBase + dataFilePath);//запись об операции будет пропущена, если будет какая-то ошибка
                    ParsedAny = true;
                }
                catch (Exception ex)
                {
                    ManageLogs.Log("Error occured while to common mode switch:" + Environment.NewLine + ex + "\r\npath=" + destFolder + "\r\nData path=" + dataFilePath + "\r\nSource dir path=" + sourceFilePath);
                }
            }
        }

        void RestoreMovedFilesLocation(StringBuilder operations)
        {
            if (operations.Length > 0)
            {
                foreach (string record in operations.ToString().SplitToLines())
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(record))
                        {
                            continue;
                        }

                        var movePaths = record.Split(operationsSplitString, StringSplitOptions.None);

                        if (movePaths.Length != 2)
                        {
                            continue;
                        }

                        var filePathInMods = movePaths[0];
                        var filePathInData = movePaths[1];

                        if (File.Exists(filePathInData))
                        {
                            if (!File.Exists(filePathInMods))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(filePathInMods));

                                filePathInData.MoveTo(filePathInMods);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ManageLogs.Log("error while RestoreMovedFilesLocation. error:\r\n" + ex);
                    }
                }

                //возврат возможных ванильных резервных копий
                MoveVanillaFilesBackToData();
            }
        }
    }
}
