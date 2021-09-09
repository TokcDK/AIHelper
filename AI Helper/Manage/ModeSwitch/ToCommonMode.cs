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

                        var movePaths = record.Split(new string[] { "|MovedTo|" }, StringSplitOptions.None);

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
