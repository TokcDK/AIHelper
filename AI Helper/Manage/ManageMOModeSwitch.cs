using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIHelper.Manage
{
    class ManageMOModeSwitch
    {
        static bool MOmode { get => ManageSettings.IsMoMode(); set => Properties.Settings.Default.MOmode = value; }

        internal static async void SwitchBetweenMoAndStandartModes()
        {
            SharedData.GameData.MainForm.OnOffButtons(false);
            if (MOmode)
            {
                DialogResult result = MessageBox.Show(
                    T._("Attention")
                    + "\n\n"
                    + T._("Conversation to")
                    + " " + T._("Common mode")
                    + "\n\n"
                    + T._("This will move using mod files from Mods folder to Data folder to make it like common installation variant.\nYou can restore it later back to MO mode.\n\nContinue?")
                    , T._("Confirmation"), MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    using (ProgressBar mo2CommonProgressBar = new ProgressBar())
                    {
                        mo2CommonProgressBar.Style = ProgressBarStyle.Marquee;
                        mo2CommonProgressBar.MarqueeAnimationSpeed = 50;
                        mo2CommonProgressBar.Dock = DockStyle.Bottom;
                        mo2CommonProgressBar.Height = 10;

                        SharedData.GameData.MainForm.Controls.Add(mo2CommonProgressBar);

                        await Task.Run(() => SwitchToCommonMode()).ConfigureAwait(true);

                        SharedData.GameData.MainForm.Controls.Remove(mo2CommonProgressBar);

                        SharedData.GameData.MainForm.OnOffButtons();
                    }

                    try
                    {
                        SharedData.GameData.MainForm.MOCommonModeSwitchButton.Text = MOmode ? T._("MOToCommon") : T._("CommonToMO");
                    }
                    catch
                    {
                    }

                    //Directory.Delete(ModsPath, true);
                    //Directory.Move(MODirPath, Path.Combine(AppResDir, Path.GetFileName(MODirPath)));
                    //SharedData.GameData.MainForm.MOCommonModeSwitchButton.Enabled = true;
                    //LanchModeInfoLinkLabel.Enabled = true;
                    //File.Move(Path.Combine(MODirPath, "ModOrganizer.exe"), Path.Combine(MODirPath, "ModOrganizer.exe.GameInCommonModeNow"));
                    //обновление информации о конфигурации папок игры
                    SharedData.GameData.MainForm.FoldersInit();
                }
            }
            else
            {
                DialogResult result = MessageBox.Show(T._("Attention") + "\n\n" + T._("Conversation to") + " " + T._("MO mode") + "\n\n" + T._("This will move all mod files back to Mods folder from Data and will switch to MO mode.\n\nContinue?"), T._("Confirmation"), MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    //SharedData.GameData.MainForm.MOCommonModeSwitchButton.Enabled = false;
                    //LanchModeInfoLinkLabel.Enabled = false;

                    using (ProgressBar mo2CommonProgressBar = new ProgressBar())
                    {
                        mo2CommonProgressBar.Style = ProgressBarStyle.Marquee;
                        mo2CommonProgressBar.MarqueeAnimationSpeed = 50;
                        mo2CommonProgressBar.Dock = DockStyle.Bottom;
                        mo2CommonProgressBar.Height = 10;

                        SharedData.GameData.MainForm.Controls.Add(mo2CommonProgressBar);

                        await Task.Run(() => SwitchBackToMoMode()).ConfigureAwait(true);

                        SharedData.GameData.MainForm.Controls.Remove(mo2CommonProgressBar);
                    }
                    SharedData.GameData.MainForm.MOCommonModeSwitchButton.Text = T._("MOToCommon");
                    //SharedData.GameData.MainForm.MOCommonModeSwitchButton.Enabled = true;
                    //LanchModeInfoLinkLabel.Enabled = true;

                    //создание ссылок на файлы bepinex
                    //BepinExLoadingFix();
                    //обновление информации о конфигурации папок игры
                    SharedData.GameData.MainForm.FoldersInit();

                }
            }

            SharedData.GameData.MainForm.OnOffButtons();
        }

        private static void SwitchBackToMoMode()
        {
            StringBuilder operationsMade = new StringBuilder();
            string[] moToStandartConvertationOperationsList = null;
            try
            {
                moToStandartConvertationOperationsList = File.ReadAllLines(ManageSettings.GetCurrentGameMoToStandartConvertationOperationsListFilePath());
                ReplaceVarsToPaths(ref moToStandartConvertationOperationsList);
                var operationsSplitString = new string[] { "|MovedTo|" };
                var vanillaDataFilesList = File.ReadAllLines(ManageSettings.GetCurrentGameVanillaDataFilesListFilePath());
                ReplaceVarsToPaths(ref vanillaDataFilesList);
                var moddedDataFilesList = File.ReadAllLines(ManageSettings.GetCurrentGameModdedDataFilesListFilePath());
                ReplaceVarsToPaths(ref moddedDataFilesList);
                Dictionary<string, string> zipmodsGuidList = new Dictionary<string, string>();
                bool zipmodsGuidListNotEmpty = false;
                if (File.Exists(ManageSettings.GetCurrentGameZipmodsGuidListFilePath()))
                {
                    using (var sr = new StreamReader(ManageSettings.GetCurrentGameZipmodsGuidListFilePath()))
                    {
                        while (!sr.EndOfStream)
                        {
                            var line = sr.ReadLine();

                            if (!string.IsNullOrWhiteSpace(line) && line.Contains("{{ZIPMOD}}"))
                            {
                                var guidPathPair = ReplaceVarsToPaths(line).Split(new[] { "{{ZIPMOD}}" }, StringSplitOptions.None);
                                zipmodsGuidList.Add(guidPathPair[0], guidPathPair[1]);
                            }
                        }
                    }
                    zipmodsGuidListNotEmpty = zipmodsGuidList.Count > 0;
                }

                //remove normal mode identifier
                SwitchNormalModeIdentifier(false);

                StringBuilder filesWhichAlreadyHaveSameDestFileInMods = new StringBuilder();
                bool filesWhichAlreadyHaveSameDestFileInModsIsNotEmpty = false;

                //Перемещение файлов модов по списку
                int operationsLength = moToStandartConvertationOperationsList.Length;
                for (int o = 0; o < operationsLength; o++)
                {
                    if (string.IsNullOrWhiteSpace(moToStandartConvertationOperationsList[o]))
                    {
                        continue;
                    }

                    string[] movePaths = moToStandartConvertationOperationsList[o].Split(operationsSplitString, StringSplitOptions.None);

                    var filePathInModsExists = File.Exists(movePaths[0]);
                    var filePathInDataExists = File.Exists(movePaths[1]);

                    if (!filePathInDataExists)
                    {
                        continue;
                    }

                    if (!filePathInModsExists)
                    {
                        string modsubfolder = Path.GetDirectoryName(movePaths[0]);
                        if (!Directory.Exists(modsubfolder))
                        {
                            Directory.CreateDirectory(modsubfolder);
                        }

                        try//ignore move file error if file will be locked and write in log about this
                        {
                            movePaths[1].MoveTo(movePaths[0]);

                            //запись выполненной операции для удаления из общего списка в случае ошибки при переключении из обычного режима
                            operationsMade.AppendLine(moToStandartConvertationOperationsList[o]);

                            try
                            {
                                //Move bonemod file both with original
                                if (File.Exists(movePaths[1] + ".bonemod.txt"))
                                {
                                    (movePaths[1] + ".bonemod.txt").MoveTo(movePaths[0] + ".bonemod.txt");
                                }
                                //запись выполненной операции для удаления из общего списка в случае ошибки при переключении из обычного режима
                                operationsMade.AppendLine(movePaths[1] + ".bonemod.txt" + "|MovedTo|" + movePaths[0] + ".bonemod.txt");
                            }
                            catch (Exception ex)
                            {
                                ManageLogs.Log("An error occured while file moving." + "MovePaths[0]=" + movePaths[0] + ";MovePaths[1]=" + movePaths[0] + ".error:\r\n" + ex);
                            }
                        }
                        catch (Exception ex)
                        {
                            ManageLogs.Log("Failed to move file: '" + Environment.NewLine + movePaths[1] + "' " + Environment.NewLine + "Error:" + Environment.NewLine + ex);
                        }
                    }
                    else
                    {
                        //если в Mods на месте планируемого для перемещения назад в Mods файла появился новый файл, то записать информацию о нем в новый мод, чтобы перенести его в новый мод
                        filesWhichAlreadyHaveSameDestFileInMods.AppendLine(movePaths[1] + "|MovedTo|" + movePaths[0]);
                        filesWhichAlreadyHaveSameDestFileInModsIsNotEmpty = true;
                    }
                }

                //string destFolderForNewFiles = Path.Combine(ModsPath, "NewAddedFiles");

                //получение даты и времени для дальнейшего использования
                string dateTimeInFormat = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

                if (filesWhichAlreadyHaveSameDestFileInModsIsNotEmpty)
                {
                    foreach (string fromToPathsLine in filesWhichAlreadyHaveSameDestFileInMods.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (string.IsNullOrWhiteSpace(fromToPathsLine))
                        {
                            continue;
                        }

                        string[] fromToPaths = fromToPathsLine.Split(new string[] { "|MovedTo|" }, StringSplitOptions.None);

                        string targetFolderPath = Path.GetDirectoryName(fromToPaths[1]);

                        bool isForOverwriteFolder = ManageStrings.IsStringAContainsStringB(targetFolderPath, ManageSettings.GetCurrentGameOverwriteFolderPath());
                        //поиск имени мода с учетом обработки файлов папки Overwrite
                        string modName = targetFolderPath;
                        if (isForOverwriteFolder)
                        {
                            modName = Path.GetFileName(ManageSettings.GetCurrentGameOverwriteFolderPath());
                        }
                        else
                        {
                            while (Path.GetDirectoryName(modName) != ManageSettings.GetCurrentGameModsDirPath())
                            {
                                modName = Path.GetDirectoryName(modName);
                            }
                            modName = Path.GetFileName(modName);
                        }

                        //Новое имя для новой целевой папки мода
                        string originalModPath = isForOverwriteFolder ? ManageSettings.GetCurrentGameOverwriteFolderPath() : Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), modName);
                        string newModName = modName + "_" + dateTimeInFormat;
                        string newModPath = Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), newModName);
                        targetFolderPath = targetFolderPath.Replace(originalModPath, newModPath);

                        string targetFileName = Path.GetFileNameWithoutExtension(fromToPaths[1]);
                        string targetFileExtension = Path.GetExtension(fromToPaths[1]);
                        string targetPath = Path.Combine(targetFolderPath, targetFileName + targetFileExtension);

                        //создать подпапку для файла
                        if (!Directory.Exists(targetFolderPath))
                        {
                            Directory.CreateDirectory(targetFolderPath);
                        }

                        //переместить файл в новую для него папку
                        fromToPaths[0].MoveTo(targetPath);

                        //ВОЗМОЖНО ЗДЕСЬ ПРОБЛЕМА В КОДЕ, ПРИ КОТОРОЙ ДЛЯ КАЖДОГО ФАЙЛА БУДЕТ СОЗДАНА ОТДЕЛЬНАЯ ПАПКА С МОДОМ
                        //НУЖНО ДОБАВИТЬ ЗАПИСЬ И ПОДКЛЮЧЕНИЕ НОВОГО МОДА ТОЛЬКО ПОСЛЕ ТОГО, КАК ВСЕ ФАЙЛЫ ИЗ НЕГО ПЕРЕМЕЩЕНЫ

                        //записать в папку мода замечание с объяснением наличия этого мода
                        string note = T._(
                            "Files in same paths already exist in original mod folder!\n\n" +
                            " This folder was created in time of conversion from Common mode to MO mode\n" +
                            " and because in destination place\n" +
                            " where mod file must be moved already was other file with same name.\n" +
                            " It could happen if content of the mod folder was updated\n" +
                            " when game was in common mode and was made same file in same place.\n" +
                            " Please check files here and if this files need for you\n" +
                            " then activate this mod or move files to mod folder with same name\n" +
                            " and if this files obsolete or just not need anymore then delete this mod folder."
                            );
                        File.WriteAllText(Path.Combine(newModPath, "NOTE!.txt"), note);

                        //запись meta.ini с замечанием
                        ManageModOrganizer.WriteMetaIni(
                            newModPath
                            ,
                            string.Empty
                            ,
                            "0." + dateTimeInFormat
                            ,
                            string.Empty
                            ,
                            note.Replace("\n", "<br>")
                            );
                        ManageModOrganizer.ActivateDeactivateInsertMod(modname: newModName, activate: false, recordWithWhichInsert: modName, placeAfter: false);
                    }
                }

                //Перемещение новых файлов
                //
                //добавление всех файлов из дата, которых нет в списке файлов модов и игры в дата, что был создан сразу после перехода в обычный режим
                string[] addedFiles = Directory.GetFiles(ManageSettings.GetCurrentGameDataPath(), "*.*", SearchOption.AllDirectories).Where(line => !moddedDataFilesList.Contains(line)).ToArray();
                //задание имени целевой папки для новых модов
                string addedFilesFolderName = "[added]UseFiles_" + dateTimeInFormat;
                string destFolderPath = Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), addedFilesFolderName);

                int addedFilesLength = addedFiles.Length;
                for (int f = 0; f < addedFilesLength; f++)
                {
                    string destFileName = null;
                    try
                    {
                        //если zipmod guid присутствует в сохраненных, переместить его на место удаленного
                        string ext;
                        string guid;
                        if (zipmodsGuidListNotEmpty
                            && addedFiles[f].ToUpperInvariant().Contains("SIDELOADER MODPACK")
                            && ((ext = Path.GetExtension(addedFiles[f]).ToUpperInvariant()) == ".ZIPMOD" || ext == ".ZIP")
                            && !string.IsNullOrWhiteSpace(guid = ManageArchive.GetZipmodGuid(addedFiles[f]))
                            && zipmodsGuidList.ContainsKey(guid)
                            )
                        {
                            if (zipmodsGuidList[guid].Contains("%"))//temp check
                            {
                                ManageLogs.Log("zipmod contains %VAR%:" + zipmodsGuidList[guid]);
                            }

                            var zipmod = ReplaceVarsToPaths(zipmodsGuidList[guid]);

                            if (Path.GetFileName(addedFiles[f]) == Path.GetFileName(zipmod))//when zipmod has same name but moved
                            {
                                var targetfolder = zipmod.IsInOverwriteFolder() ?
                                    ManageSettings.GetCurrentGameOverwriteFolderPath() : ManageSettings.GetCurrentGameModsDirPath();
                                destFileName = addedFiles[f].Replace(ManageSettings.GetCurrentGameDataPath(), targetfolder
                                    );
                            }
                            else//when mod was renamed
                            {
                                if (zipmod.IsInOverwriteFolder())//zipmod in overwrite
                                {
                                    var newFilePath = addedFiles[f].Replace(ManageSettings.GetCurrentGameDataPath(), ManageSettings.GetCurrentGameOverwriteFolderPath());
                                    if (Directory.Exists(Path.GetDirectoryName(newFilePath)) && newFilePath != addedFiles[f])
                                    {
                                        destFileName = newFilePath;
                                    }
                                }
                                else//zipmod in Mods
                                {
                                    var modPath = zipmod.GetPathInMods();
                                    if (Path.GetFileName(modPath).ToUpperInvariant() != "MODS" && Directory.Exists(modPath))
                                    {
                                        destFileName = addedFiles[f].Replace(ManageSettings.GetCurrentGameDataPath(), modPath);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ManageLogs.Log("Error occured while to MO mode switch:" + Environment.NewLine + ex);
                    }

                    if (string.IsNullOrEmpty(destFileName))
                    {
                        destFileName = addedFiles[f].Replace(ManageSettings.GetCurrentGameDataPath(), destFolderPath);
                    }
                    Directory.CreateDirectory(Path.GetDirectoryName(destFileName));
                    addedFiles[f].MoveTo(destFileName);
                }

                //подключить новый мод, если он существует
                if (Directory.Exists(destFolderPath))
                {
                    //запись meta.ini
                    ManageModOrganizer.WriteMetaIni(
                        destFolderPath
                        ,
                        "UserFiles"
                        ,
                        dateTimeInFormat
                        ,
                        T._("Sort files if need")
                        ,
                        T._("<br>This files was added in Common mode<br>and moved as mod after convertation in MO mode.<br>Date: ") + dateTimeInFormat
                        );

                    ManageModOrganizer.ActivateDeactivateInsertMod(addedFilesFolderName);
                }

                //перемещение ванильных файлов назад в дата
                MoveVanillaFilesBackToData();

                //очистка пустых папок в Data
                if (File.Exists(ManageSettings.GetCurrentGameVanillaDataEmptyFoldersListFilePath()))
                {
                    //удалить все, за исключением добавленных ранее путей до пустых папок
                    string[] vanillaDataEmptyFoldersList = File.ReadAllLines(ManageSettings.GetCurrentGameVanillaDataEmptyFoldersListFilePath());
                    ReplaceVarsToPaths(ref vanillaDataEmptyFoldersList);
                    ManageFilesFoldersExtensions.DeleteEmptySubfolders(ManageSettings.GetCurrentGameDataPath(), false, vanillaDataEmptyFoldersList);
                }
                else
                {
                    ManageFilesFoldersExtensions.DeleteEmptySubfolders(ManageSettings.GetCurrentGameDataPath(), false);
                }

                //restore sideloader mods from Mods\Mods
                var modsmods = Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), "Mods");
                var targetmodpath = modsmods;
                if (false && Directory.Exists(modsmods)) //need to update overwrite check and scan only active mods or just move content to overwrite
                {
                    try//skip if was error
                    {
                        //scan Mods\Mods subdir
                        foreach (var dir in new DirectoryInfo(modsmods).EnumerateDirectories())
                        {
                            //parse only sideloader folders
                            if (!dir.Name.ToUpperInvariant().Contains("SIDELOADER MODPACK"))
                            {
                                continue;
                            }

                            //search target mod path where exists 'dir' folder
                            foreach (var moddir in Directory.EnumerateDirectories(ManageSettings.GetCurrentGameModsDirPath()))
                            {
                                //skip separators
                                if (moddir.EndsWith("_separator", StringComparison.InvariantCulture) || moddir == modsmods)
                                {
                                    continue;
                                }

                                //check if subfolder dir.name exists
                                if (Directory.Exists(Path.Combine(moddir, dir.Name)))
                                {
                                    targetmodpath = moddir;
                                    break;
                                }
                            }

                            //move files from 'dir' to target mod
                            if (targetmodpath != modsmods)//skip if target mod folder not changed
                                foreach (var file in Directory.EnumerateFiles(dir.FullName))
                                {
                                    var targetfilepath = file.Replace(dir.FullName, targetmodpath);
                                    if (!File.Exists(targetfilepath))//skip if exists target file path
                                    {
                                        File.Move(file, targetfilepath);
                                    }
                                }
                        }

                        //cleanup empty dirs
                        ManageFilesFoldersExtensions.DeleteEmptySubfolders(modsmods, true);
                    }
                    catch (Exception ex)
                    {
                        ManageLogs.Log("error occured while Mods\\Mods file sorting. Error:\r\n" + ex);
                    }
                }

                //чистка файлов-списков
                File.Delete(ManageSettings.GetCurrentGameMoToStandartConvertationOperationsListFilePath());
                File.Delete(ManageSettings.GetCurrentGameVanillaDataFilesListFilePath());
                File.Delete(ManageSettings.GetCurrentGameVanillaDataEmptyFoldersListFilePath());
                File.Delete(ManageSettings.GetCurrentGameModdedDataFilesListFilePath());
                if (File.Exists(ManageSettings.GetCurrentGameZipmodsGuidListFilePath()))
                {
                    File.Delete(ManageSettings.GetCurrentGameZipmodsGuidListFilePath());
                }

                ManageFilesFoldersExtensions.DeleteEmptySubfolders(ManageSettings.GetCurrentGameMOmodeBakDirPath(), deleteThisDir: true);

                MOmode = true;

                MessageBox.Show(T._("Mod Organizer mode restored! All mod files moved back to Mods folder. If in Data folder was added new files they also moved in Mods folder as new mod, check and sort it if need"));

            }
            catch (Exception ex)
            {
                //обновление списка операций с файлами, для удаления уже выполненных и записи обновленного списка
                if (operationsMade.ToString().Length > 0 && moToStandartConvertationOperationsList != null && moToStandartConvertationOperationsList.Length > 0)
                {
                    foreach (string operationsMadeLine in operationsMade.ToString().SplitToLines())
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(operationsMadeLine))
                            {
                                continue;
                            }

                            moToStandartConvertationOperationsList = moToStandartConvertationOperationsList.Where(operationsLine => operationsLine != operationsMadeLine).ToArray();
                        }
                        catch
                        {
                        }
                    }

                    File.WriteAllLines(ManageSettings.GetCurrentGameMoToStandartConvertationOperationsListFilePath(), moToStandartConvertationOperationsList);
                }

                //recreate normal mode identifier if failed
                SwitchNormalModeIdentifier();

                MessageBox.Show("Failed to switch in MO mode. Error:" + Environment.NewLine + ex);
            }
        }

        /// <summary>
        /// normal mode identifier switcher
        /// </summary>
        /// <param name="create">true=Create/false=Delete</param>
        private static void SwitchNormalModeIdentifier(bool create = true)
        {
            if (create)
            {
                if (!File.Exists(Path.Combine(ManageSettings.GetCurrentGameDataPath(), "normal.mode")))
                {
                    File.WriteAllText(Path.Combine(ManageSettings.GetCurrentGameDataPath(), "normal.mode"), "The game is in normal mode");
                }
            }
            else
            {
                if (File.Exists(Path.Combine(ManageSettings.GetCurrentGameDataPath(), "normal.mode")))
                {
                    File.Delete(Path.Combine(ManageSettings.GetCurrentGameDataPath(), "normal.mode"));
                }
            }
        }

        private static void SwitchToCommonMode()
        {
            var enabledModsList = ManageModOrganizer.GetModNamesListFromActiveMoProfile();

            if (enabledModsList.Length == 0)
            {
                MessageBox.Show(T._("There is no enabled mods in Mod Organizer"));
                return;
            }

            //список выполненных операций с файлами.
            var moToStandartConvertationOperationsList = new StringBuilder();
            //список пустых папок в data до ереноса файлов модов
            StringBuilder vanillaDataEmptyFoldersList = new StringBuilder();
            //список файлов в data без модов
            string[] vanillaDataFilesList;
            //список guid zipmod-ов
            Dictionary<string, string> zipmodsGuidList = new Dictionary<string, string>();
            List<string> longPaths = new List<string>();


            var enabledModsLength = enabledModsList.Length;

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

                //получение всех файлов из Data
                vanillaDataFilesList = Directory.GetFiles(ManageSettings.GetCurrentGameDataPath(), "*.*", SearchOption.AllDirectories);

                //получение всех файлов из папки Overwrite и их обработка
                var filesInOverwrite = Directory.GetFiles(ManageSettings.GetCurrentGameOverwriteFolderPath(), "*.*", SearchOption.AllDirectories);
                if (filesInOverwrite.Length > 0)
                {
                    //if (Path.GetFileName(FilesInOverwrite[0]).Contains("Overwrite"))
                    //{
                    //    OverwriteFolder = OverwriteFolder.Replace("overwrite", "Overwrite");
                    //}

                    string dataFilePath;
                    var filesInOverwriteLength = filesInOverwrite.Length;

                    using (var frmProgress = new Form())
                    {
                        frmProgress.Text = T._("Move files from Overwrite folder");
                        frmProgress.Size = new Size(200, 50);
                        frmProgress.StartPosition = FormStartPosition.CenterScreen;
                        frmProgress.FormBorderStyle = FormBorderStyle.FixedToolWindow;
                        using (var pbProgress = new ProgressBar())
                        {
                            pbProgress.Maximum = filesInOverwriteLength;
                            pbProgress.Dock = DockStyle.Bottom;
                            frmProgress.Controls.Add(pbProgress);
                            frmProgress.Show();
                            for (int n = 0; n < filesInOverwriteLength; n++)
                            {
                                if (n < pbProgress.Maximum)
                                {
                                    pbProgress.Value = n;
                                }

                                var sourceFilePath = filesInOverwrite[n];

                                if (ManageStrings.CheckForLongPath(ref sourceFilePath))
                                {
                                    longPaths.Add(sourceFilePath.Remove(0, 4));
                                }
                                //if (FileInOverwrite.Length > 259)
                                //{
                                //    if (OfferToSkipTheFileConfirmed(FileInOverwrite))
                                //    {
                                //        continue;
                                //    }
                                //    //    FileInOverwrite = @"\\?\" + FileInOverwrite;
                                //}

                                dataFilePath = sourceFilePath.Replace(ManageSettings.GetCurrentGameOverwriteFolderPath(), ManageSettings.GetCurrentGameDataPath());
                                if (ManageStrings.CheckForLongPath(ref dataFilePath))
                                {
                                    longPaths.Add(dataFilePath.Remove(0, 4));
                                }
                                //if (FileInDataFolder.Length > 259)
                                //{
                                //    FileInDataFolder = @"\\?\" + FileInDataFolder;
                                //}
                                if (File.Exists(dataFilePath))
                                {
                                    var vanillaFileBackupTargetPath = dataFilePath.Replace(ManageSettings.GetCurrentGameDataPath(), ManageSettings.GetCurrentGameMOmodeDataFilesBakDirPath());
                                    //if (FileInBakFolderWhichIsInRES.Length > 259)
                                    //{
                                    //    FileInBakFolderWhichIsInRES = @"\\?\" + FileInBakFolderWhichIsInRES;
                                    //}
                                    if (!File.Exists(vanillaFileBackupTargetPath) && vanillaDataFilesList.Contains(dataFilePath))
                                    {
                                        var bakfolder = Path.GetDirectoryName(vanillaFileBackupTargetPath);
                                        try
                                        {
                                            if (SharedData.GameData.MainForm.IsDebug)
                                                debugString = bakfolder + ":bakfolder,l1922";
                                            Directory.CreateDirectory(bakfolder);

                                            dataFilePath.MoveTo(vanillaFileBackupTargetPath);//перенос файла из Data в Bak, если там не было

                                            ManageModOrganizer.SaveGuidIfZipMod(sourceFilePath, zipmodsGuidList);

                                            sourceFilePath.MoveTo(dataFilePath);

                                            moToStandartConvertationOperationsList.AppendLine(sourceFilePath + "|MovedTo|" + dataFilePath);//запись об операции будет пропущена, если будет какая-то ошибка

                                        }
                                        catch (Exception ex)
                                        {
                                            //когда файла в дата нет, файл в бак есть и есть файл в папке Overwrite - вернуть файл из bak назад
                                            if (!File.Exists(dataFilePath) && File.Exists(vanillaFileBackupTargetPath) && File.Exists(sourceFilePath))
                                            {
                                                File.Move(vanillaFileBackupTargetPath, dataFilePath);
                                            }

                                            ManageLogs.Log("Error occured while to common mode switch:" + Environment.NewLine + ex + "\r\nparent dir=" + bakfolder + "\r\nData path=" + dataFilePath + "\r\nOverwrite path=" + sourceFilePath);
                                        }
                                    }
                                }
                                else
                                {
                                    var destFolder = Path.GetDirectoryName(dataFilePath);
                                    try
                                    {
                                        if (SharedData.GameData.MainForm.IsDebug)
                                            debugString = destFolder + ":destFolder,l2068";
                                        Directory.CreateDirectory(destFolder);

                                        ManageModOrganizer.SaveGuidIfZipMod(sourceFilePath, zipmodsGuidList);

                                        sourceFilePath.MoveTo(dataFilePath);//перенос файла из папки мода в Data
                                        moToStandartConvertationOperationsList.AppendLine(sourceFilePath + "|MovedTo|" + dataFilePath);//запись об операции будет пропущена, если будет какая-то ошибка
                                    }
                                    catch (Exception ex)
                                    {
                                        ManageLogs.Log("Error occured while to common mode switch:" + Environment.NewLine + ex + "\r\npath=" + destFolder + "\r\nData path=" + dataFilePath + "\r\nOverwrite path=" + sourceFilePath);
                                    }
                                }
                            }
                        }
                    }
                }
                using (var frmProgress = new Form())
                {
                    frmProgress.Text = T._("Move files from Mods folder");
                    frmProgress.Size = new Size(200, 50);
                    frmProgress.StartPosition = FormStartPosition.CenterScreen;
                    frmProgress.FormBorderStyle = FormBorderStyle.FixedToolWindow;
                    using (var pbProgress = new ProgressBar())
                    {
                        pbProgress.Maximum = enabledModsLength;
                        pbProgress.Dock = DockStyle.Bottom;
                        frmProgress.Controls.Add(pbProgress);
                        frmProgress.Show();
                        for (int n = 0; n < enabledModsLength; n++)
                        {
                            if (n < pbProgress.Maximum)
                            {
                                pbProgress.Value = n;
                            }

                            var modFolder = Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), enabledModsList[n]);
                            if (modFolder.Length > 0 && Directory.Exists(modFolder))
                            {
                                var modFiles = Directory.GetFiles(modFolder, "*.*", SearchOption.AllDirectories);
                                if (modFiles.Length > 0)
                                {
                                    var modFilesLength = modFiles.Length;
                                    string dataFilePath;

                                    var metaskipped = false;
                                    for (int f = 0; f < modFilesLength; f++)
                                    {
                                        //"\\?\" - prefix to ignore 260 path cars limit

                                        var sourceFilePath = modFiles[f];
                                        if (ManageStrings.CheckForLongPath(ref sourceFilePath))
                                        {
                                            longPaths.Add(sourceFilePath.Remove(0, 4));
                                        }
                                        //if (FileOfMod.Length > 259)
                                        //{
                                        //    if (OfferToSkipTheFileConfirmed(FileOfMod))
                                        //    {
                                        //        continue;
                                        //    }
                                        //    //FileOfMod = @"\\?\" + FileOfMod;
                                        //}

                                        try
                                        {
                                            //skip images and txt in mod root folder
                                            var fileExtension = Path.GetExtension(sourceFilePath);
                                            if (string.Equals(fileExtension, ".txt", StringComparison.InvariantCultureIgnoreCase) || fileExtension.IsPictureExtension())
                                            {
                                                if (Path.GetFileName(sourceFilePath.Replace(Path.DirectorySeparatorChar + Path.GetFileName(sourceFilePath), string.Empty)) == enabledModsList[n])
                                                {
                                                    //пропускать картинки и txt в корне папки мода
                                                    continue;
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            ManageLogs.Log("error while image skip. error:" + ex);
                                        }

                                        //skip meta.ini
                                        if (metaskipped)
                                        {
                                        }
                                        else if (Path.GetFileName(sourceFilePath) == "meta.ini" /*ModFile.Length >= 8 && string.Compare(ModFile.Substring(ModFile.Length - 8, 8), "meta.ini", true, CultureInfo.InvariantCulture) == 0*/)
                                        {
                                            metaskipped = true;//для ускорения проверки, когда meta будет найден, будет делать быструю проверку bool переменной
                                            continue;
                                        }

                                        //SharedData.GameData.MainForm.MOCommonModeSwitchButton.Text = "..." + EnabledModsLength + "/" + N + ": " + f + "/" + ModFilesLength;
                                        dataFilePath = sourceFilePath.Replace(modFolder, ManageSettings.GetCurrentGameDataPath());
                                        if (ManageStrings.CheckForLongPath(ref dataFilePath))
                                        {
                                            longPaths.Add(dataFilePath.Remove(0, 4));
                                        }
                                        //if (FileInDataFolder.Length > 259)
                                        //{
                                        //    FileInDataFolder = @"\\?\" + FileInDataFolder;
                                        //}

                                        if (File.Exists(dataFilePath))
                                        {
                                            var fileInBakFolderWhichIsInRes = dataFilePath.Replace(ManageSettings.GetCurrentGameDataPath(), ManageSettings.GetCurrentGameMOmodeDataFilesBakDirPath());
                                            //if (FileInBakFolderWhichIsInRES.Length > 259)
                                            //{
                                            //    FileInBakFolderWhichIsInRES = @"\\?\" + FileInBakFolderWhichIsInRES;
                                            //}
                                            if (!File.Exists(fileInBakFolderWhichIsInRes) && vanillaDataFilesList.Contains(dataFilePath))
                                            {
                                                var bakfolder = Path.GetDirectoryName(fileInBakFolderWhichIsInRes);
                                                try
                                                {
                                                    if (SharedData.GameData.MainForm.IsDebug)
                                                        debugString = bakfolder + ":bakfolder,l2183";
                                                    Directory.CreateDirectory(bakfolder);

                                                    dataFilePath.MoveTo(fileInBakFolderWhichIsInRes);//перенос файла из Data в Bak, если там не было

                                                    ManageModOrganizer.SaveGuidIfZipMod(sourceFilePath, zipmodsGuidList);

                                                    sourceFilePath.MoveTo(dataFilePath);//перенос файла из папки мода в Data
                                                    moToStandartConvertationOperationsList.AppendLine(sourceFilePath + "|MovedTo|" + dataFilePath);//запись об операции будет пропущена, если будет какая-то ошибка
                                                }
                                                catch (Exception ex)
                                                {
                                                    //когда файла в дата нет, файл в бак есть и есть файл в папке мода - вернуть файл из bak назад
                                                    if (!File.Exists(dataFilePath) && File.Exists(fileInBakFolderWhichIsInRes) && File.Exists(sourceFilePath))
                                                    {
                                                        File.Move(fileInBakFolderWhichIsInRes, dataFilePath);
                                                    }

                                                    ManageLogs.Log("Error occured while to common mode switch:" + Environment.NewLine + ex + "\r\npath=" + bakfolder + "\r\nData path=" + dataFilePath + "\r\nMods path=" + sourceFilePath);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var destFolder = Path.GetDirectoryName(dataFilePath);
                                            try
                                            {
                                                if (SharedData.GameData.MainForm.IsDebug)
                                                    debugString = destFolder + ":destFolder,l2208";
                                                Directory.CreateDirectory(destFolder);

                                                ManageModOrganizer.SaveGuidIfZipMod(sourceFilePath, zipmodsGuidList);

                                                sourceFilePath.MoveTo(dataFilePath);//перенос файла из папки мода в Data
                                                moToStandartConvertationOperationsList.AppendLine(sourceFilePath + "|MovedTo|" + dataFilePath);//запись об операции будет пропущена, если будет какая-то ошибка
                                            }
                                            catch (Exception ex)
                                            {
                                                ManageLogs.Log("Error occured while to common mode switch:" + Environment.NewLine + ex + "\r\npath=" + destFolder + "\r\nData path=" + dataFilePath + "\r\nMods path=" + sourceFilePath);
                                            }
                                        }

                                        //MoveWithReplace(ModFile, DestFilePath[f]);
                                    }
                                    //Directory.Delete(ModFolder, true);
                                }
                            }
                        }
                    }
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
        /// replace variable to path in string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string ReplaceVarsToPaths(string str)
        {
            return str
            .Replace(ManageSettings.VarCurrentGameDataPath(), ManageSettings.GetCurrentGameDataPath())
            .Replace(ManageSettings.VarCurrentGameModsPath(), ManageSettings.GetCurrentGameModsDirPath())
            .Replace(ManageSettings.VarCurrentGameMoOverwritePath(), ManageSettings.GetCurrentGameMoOverwritePath());
        }

        /// <summary>
        /// replace path to variable in string array
        /// </summary>
        /// <param name="sarr"></param>
        private static void ReplacePathsToVars(ref string[] sarr)
        {
            for (int i = 0; i < sarr.Length; i++)
            {
                sarr[i] = sarr[i]
                .Replace(ManageSettings.GetCurrentGameDataPath(), ManageSettings.VarCurrentGameDataPath())
                .Replace(ManageSettings.GetCurrentGameModsDirPath(), ManageSettings.VarCurrentGameModsPath())
                .Replace(ManageSettings.GetCurrentGameMoOverwritePath(), ManageSettings.VarCurrentGameMoOverwritePath());
            }
        }

        /// <summary>
        /// replace variable to path in string array
        /// </summary>
        /// <param name="sarr"></param>
        private static void ReplaceVarsToPaths(ref string[] sarr)
        {
            for (int i = 0; i < sarr.Length; i++)
            {
                sarr[i] = sarr[i]
                .Replace(ManageSettings.VarCurrentGameDataPath(), ManageSettings.GetCurrentGameDataPath())
                .Replace(ManageSettings.VarCurrentGameModsPath(), ManageSettings.GetCurrentGameModsDirPath())
                .Replace(ManageSettings.VarCurrentGameMoOverwritePath(), ManageSettings.GetCurrentGameMoOverwritePath());
            }
        }

        /// <summary>
        /// replace path to variable in string builder
        /// </summary>
        /// <param name="sb"></param>
        private static void ReplacePathsToVars(ref StringBuilder sb)
        {
            sb = sb
                .Replace(ManageSettings.GetCurrentGameDataPath(), ManageSettings.VarCurrentGameDataPath())
                .Replace(ManageSettings.GetCurrentGameModsDirPath(), ManageSettings.VarCurrentGameModsPath())
                .Replace(ManageSettings.GetCurrentGameMoOverwritePath(), ManageSettings.VarCurrentGameMoOverwritePath());
        }

        /// <summary>
        /// replace variable to path in string builder
        /// </summary>
        /// <param name="sb"></param>
        private static void ReplaceVarsToPaths(ref StringBuilder sb)
        {
            sb = sb
                .Replace(ManageSettings.VarCurrentGameDataPath(), ManageSettings.GetCurrentGameDataPath())
                .Replace(ManageSettings.VarCurrentGameModsPath(), ManageSettings.GetCurrentGameModsDirPath())
                .Replace(ManageSettings.VarCurrentGameMoOverwritePath(), ManageSettings.GetCurrentGameMoOverwritePath());
        }

        private static bool OfferToSkipTheFileConfirmed(string file)
        {
            DialogResult result = MessageBox.Show(
                T._("Path to file is too long!") + Environment.NewLine
                + "(" + file + ")" + Environment.NewLine
                + T._("Long Path can cause mode switch error and mode will not be switched.") + Environment.NewLine
                + T._("Skip it?") + Environment.NewLine
                , T._("Too long file path"), MessageBoxButtons.YesNo);

            return result == DialogResult.Yes;
        }

        private static void RestoreMovedFilesLocation(StringBuilder operations)
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

        private static void MoveVanillaFilesBackToData()
        {
            var mOmodeDataFilesBakDirPath = ManageSettings.GetCurrentGameMOmodeDataFilesBakDirPath();
            if (Directory.Exists(mOmodeDataFilesBakDirPath))
            {
                var filesInMOmodeDataFilesBak = Directory.GetFiles(mOmodeDataFilesBakDirPath, "*.*", SearchOption.AllDirectories);
                int filesInMOmodeDataFilesBakLength = filesInMOmodeDataFilesBak.Length;
                for (int f = 0; f < filesInMOmodeDataFilesBakLength; f++)
                {
                    if (string.IsNullOrWhiteSpace(filesInMOmodeDataFilesBak[f]))
                    {
                        continue;
                    }

                    var destFileInDataFolderPath = filesInMOmodeDataFilesBak[f].Replace(mOmodeDataFilesBakDirPath, ManageSettings.GetCurrentGameDataPath());
                    if (!File.Exists(destFileInDataFolderPath))
                    {
                        var destFileInDataFolderPathFolder = Path.GetDirectoryName(destFileInDataFolderPath);
                        if (!Directory.Exists(destFileInDataFolderPathFolder))
                        {
                            Directory.CreateDirectory(destFileInDataFolderPathFolder);
                        }
                        filesInMOmodeDataFilesBak[f].MoveTo(destFileInDataFolderPath);
                    }
                }

                //удаление папки, где хранились резервные копии ванильных файлов
                ManageFilesFoldersExtensions.DeleteEmptySubfolders(mOmodeDataFilesBakDirPath);
            }
        }
    }
}
