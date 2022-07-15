using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIHelper.Manage.ModeSwitch
{
    class ToMOMode : ModeSwitcherBase
    {
        static Logger _log = LogManager.GetCurrentClassLogger();
        protected override string DialogText =>
            T._("Attention")
            + "\n\n"
            + T._("Conversation to")
            + " "
            + T._("MO mode")
            + "\n\n"
            + T._("This will move all mod files back to Mods folder from Data and will switch to MO mode.\n" +
                "\n" +
                "Continue?");

        protected override void Action()
        {
            SwitchBackToMoMode();
        }

        StringBuilder operationsMade = new StringBuilder();
        string[] moToStandartConvertationOperationsList = null;
        string[] vanillaDataFilesList;
        string[] moddedDataFilesList;
        Dictionary<string, string> zipmodsGuidList;
        bool zipmodsGuidListNotEmpty;
        StringBuilder filesWhichAlreadyHaveSameDestFileInMods;
        bool filesWhichAlreadyHaveSameDestFileInModsIsNotEmpty;
        string dateTimeInFormat;

        protected void SwitchBackToMoMode()
        {
            try
            {
                operationsMade = new StringBuilder();
                moToStandartConvertationOperationsList = File.ReadAllLines(ManageSettings.CurrentGameMoToStandartConvertationOperationsListFilePath);
                ReplaceVarsToPaths(ref moToStandartConvertationOperationsList);
                vanillaDataFilesList = File.ReadAllLines(ManageSettings.CurrentGameVanillaDataFilesListFilePath);
                ReplaceVarsToPaths(ref vanillaDataFilesList);
                moddedDataFilesList = File.ReadAllLines(ManageSettings.CurrentGameModdedDataFilesListFilePath);
                ReplaceVarsToPaths(ref moddedDataFilesList);
                zipmodsGuidList = new Dictionary<string, string>();
                zipmodsGuidListNotEmpty = FillZipModsGUID(zipmodsGuidList);

                //remove normal mode identifier
                SwitchNormalModeIdentifier(false);

                filesWhichAlreadyHaveSameDestFileInModsIsNotEmpty = false;
                filesWhichAlreadyHaveSameDestFileInMods = new StringBuilder();

                //Перемещение файлов модов по списку
                MoveFilesByOperationsList();

                //string destFolderForNewFiles = Path.Combine(ModsPath, "NewAddedFiles");

                //получение даты и времени для дальнейшего использования
                dateTimeInFormat = ManageSettings.DateTimeBasedSuffix;

                MoveExistsModFilesInNewMod();

                //Перемещение новых файлов
                MoveNewCreatedDataFilesToNewMod();

                //перемещение ванильных файлов назад в дата
                MoveVanillaFilesBackToData();

                // remove empty dirs
                ManageFilesFoldersExtensions.DeleteEmptySubfolders(ManageSettings.CurrentGameDataDirPath, false);

                var emptyDirsInfoFilePath = ManageSettings.CurrentGameVanillaDataEmptyFoldersListFilePath;
                if (File.Exists(emptyDirsInfoFilePath))
                {
                    string[] vanillaDataEmptyFoldersList = File.ReadAllLines(emptyDirsInfoFilePath);
                    ReplaceVarsToPaths(ref vanillaDataEmptyFoldersList);

                    // restore empty dirs which was exist before mode switch
                    foreach (var path in vanillaDataEmptyFoldersList)
                    {
                        if (string.IsNullOrWhiteSpace(path))
                        {
                            continue;
                        }

                        if (!Directory.Exists(path))
                        {
                            try
                            {
                                Directory.CreateDirectory(path);
                            }
                            catch { }
                        }
                    }
                }

                //restore sideloader mods from Mods\Mods
                var modsmods = Path.Combine(ManageSettings.CurrentGameModsDirPath, "Mods");
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
                            foreach (var moddir in Directory.EnumerateDirectories(ManageSettings.CurrentGameModsDirPath))
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
                        _log.Debug("error occured while Mods\\Mods file sorting. Error:\r\n" + ex);
                    }
                }

                //чистка файлов-списков
                File.Delete(ManageSettings.CurrentGameMoToStandartConvertationOperationsListFilePath);
                File.Delete(ManageSettings.CurrentGameVanillaDataFilesListFilePath);
                File.Delete(ManageSettings.CurrentGameVanillaDataEmptyFoldersListFilePath);
                File.Delete(ManageSettings.CurrentGameModdedDataFilesListFilePath);
                if (File.Exists(ManageSettings.CurrentGameZipmodsGuidListFilePath))
                {
                    File.Delete(ManageSettings.CurrentGameZipmodsGuidListFilePath);
                }

                ManageFilesFoldersExtensions.DeleteEmptySubfolders(ManageSettings.CurrentGameMOmodeBakDirPath, deleteThisDir: true);

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

                    File.WriteAllLines(ManageSettings.CurrentGameMoToStandartConvertationOperationsListFilePath, moToStandartConvertationOperationsList);
                }

                //recreate normal mode identifier if failed
                SwitchNormalModeIdentifier();

                MessageBox.Show("Failed to switch in MO mode. Error:" + Environment.NewLine + ex);
            }
        }

        private void MoveNewCreatedDataFilesToNewMod()
        {
            //
            //добавление всех файлов из дата, которых нет в списке файлов модов и игры в дата, что был создан сразу после перехода в обычный режим
            //string[] addedFiles = Directory.GetFiles(ManageSettings.GetCurrentGameDataPath(), "*.*", SearchOption.AllDirectories).Where(line => !moddedDataFilesList.Contains(line)).ToArray();
            //задание имени целевой папки для новых модов
            string addedFilesFolderName = "[added]UseFiles" + dateTimeInFormat;
            string destFolderPath = Path.Combine(ManageSettings.CurrentGameModsDirPath, addedFilesFolderName);

            //int addedFilesLength = addedFiles.Length;
            Parallel.ForEach(Directory.GetFiles(ManageSettings.CurrentGameDataDirPath, "*.*", SearchOption.AllDirectories), file =>
            {
                if (moddedDataFilesList.Contains(file))
                {
                    // skip mod files
                    return;
                }

                string destFileName = null;
                try
                {
                    //если zipmod guid присутствует в сохраненных, переместить его на место удаленного
                    string ext;
                    string guid;
                    if ((guid = IsZipmodCanBeMovedByGUID(file)).Length > 0)
                    {
                        if (zipmodsGuidList[guid].Contains("%"))//temp check
                        {
                            _log.Debug("zipmod contains %VAR%:" + zipmodsGuidList[guid]);
                        }

                        var zipmod = ReplaceVarsToPaths(zipmodsGuidList[guid]);

                        if (Path.GetFileName(file) == Path.GetFileName(zipmod))//when zipmod has same name but moved
                        {
                            var targetfolder = zipmod.IsInOverwriteFolder() ?
                                ManageSettings.CurrentGameOverwriteFolderPath : ManageSettings.CurrentGameModsDirPath;
                            destFileName = file.Replace(ManageSettings.CurrentGameDataDirPath, targetfolder
                                );
                        }
                        else//when mod was renamed
                        {
                            if (zipmod.IsInOverwriteFolder())//zipmod in overwrite
                            {
                                var newFilePath = file.Replace(ManageSettings.CurrentGameDataDirPath, ManageSettings.CurrentGameOverwriteFolderPath);
                                if (Directory.Exists(Path.GetDirectoryName(newFilePath)) && newFilePath != file)
                                {
                                    destFileName = newFilePath;
                                }
                            }
                            else//zipmod in Mods
                            {
                                var modPath = zipmod.GetPathInMods();
                                if (Path.GetFileName(modPath).ToUpperInvariant() != "MODS" && Directory.Exists(modPath))
                                {
                                    destFileName = file.Replace(ManageSettings.CurrentGameDataDirPath, modPath);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Debug("Error occured while to MO mode switch:" + Environment.NewLine + ex);
                }

                if (string.IsNullOrEmpty(destFileName))
                {
                    destFileName = file.Replace(ManageSettings.CurrentGameDataDirPath, destFolderPath);
                }
                Directory.CreateDirectory(Path.GetDirectoryName(destFileName));
                file.MoveTo(destFileName);
            });

            //подключить новый мод, если он существует
            if (Directory.Exists(destFolderPath))
            {
                //запись meta.ini
                ManageModOrganizer.WriteMetaIni(
                    destFolderPath
                    ,
                    "UserFiles"
                    ,
                    dateTimeInFormat.Substring(1)
                    ,
                    T._("Sort files if need")
                    ,
                    T._("<br>This files was added in Common mode<br>and moved as mod after convertation in MO mode.<br>Date: ") + dateTimeInFormat.Substring(1)
                    );

                ManageModOrganizer.ActivateDeactivateInsertMod(addedFilesFolderName);
            }
        }

        private string IsZipmodCanBeMovedByGUID(string file)
        {
            string guid;
            string ext;
            if (zipmodsGuidListNotEmpty
                           && file.ToUpperInvariant().Contains("SIDELOADER MODPACK")
                           && ((ext = Path.GetExtension(file).ToUpperInvariant()) == ".ZIPMOD" || ext == ".ZIP")
                           && !string.IsNullOrWhiteSpace(guid = ManageArchive.GetZipmodGuid(file))
                           && zipmodsGuidList.ContainsKey(guid))
            {
                return guid;
            }

            return "";
        }

        private void MoveExistsModFilesInNewMod()
        {
            if (!filesWhichAlreadyHaveSameDestFileInModsIsNotEmpty)
            {
                return;
            }

            Parallel.ForEach(filesWhichAlreadyHaveSameDestFileInMods.ToString().SplitToLines(), fromToPathsLine =>
            {
                if (string.IsNullOrWhiteSpace(fromToPathsLine))
                {
                    return;
                }

                string[] fromToPaths = fromToPathsLine.Split(operationsSplitString, StringSplitOptions.None);

                string targetFolderPath = Path.GetDirectoryName(fromToPaths[1]);

                bool isForOverwriteFolder = ManageStrings.IsStringAContainsStringB(targetFolderPath, ManageSettings.CurrentGameOverwriteFolderPath);
                //поиск имени мода с учетом обработки файлов папки Overwrite
                string modName = targetFolderPath;
                if (isForOverwriteFolder)
                {
                    modName = Path.GetFileName(ManageSettings.CurrentGameOverwriteFolderPath);
                }
                else
                {
                    while (Path.GetDirectoryName(modName) != ManageSettings.CurrentGameModsDirPath)
                    {
                        modName = Path.GetDirectoryName(modName);
                    }
                    modName = Path.GetFileName(modName);
                }

                //Новое имя для новой целевой папки мода
                string originalModPath = isForOverwriteFolder ? ManageSettings.CurrentGameOverwriteFolderPath : Path.Combine(ManageSettings.CurrentGameModsDirPath, modName);
                string newModName = modName + dateTimeInFormat;
                string newModPath = Path.Combine(ManageSettings.CurrentGameModsDirPath, newModName);
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
                    "0." + dateTimeInFormat.Substring(1)
                    ,
                    string.Empty
                    ,
                    note.Replace("\n", "<br>")
                    );
                ManageModOrganizer.ActivateDeactivateInsertMod(modname: newModName, activate: false, recordWithWhichInsert: modName, placeAfter: false);
            });
        }

        private void MoveFilesByOperationsList()
        {
            int operationsLength = moToStandartConvertationOperationsList.Length;
            Parallel.ForEach(moToStandartConvertationOperationsList, operation =>
            {
                if (string.IsNullOrWhiteSpace(operation))
                {
                    return;
                }

                string[] movePaths = operation.Split(operationsSplitString, StringSplitOptions.None);
                if (movePaths.Length != 2)
                {
                    _log.Debug("Something wrong and where was wrong operation parameters count in MoveFilesByOperationsList. Operation value:\r\n" + operation);
                    return;
                }

                if (movePaths[0].CountOf(':') > 1 || movePaths[1].CountOf(':') > 1)
                {
                    // must be one ':' in absolute path or no one if relative path
                    _log.Debug("Something wrong and where was wrong operation parameters value in MoveFilesByOperationsList. Operation value:\r\n" + operation);
                }

                if (IsDirSymlink(movePaths))
                {
                    return;
                }

                var filePathInModsExists = File.Exists(movePaths[0]);
                var filePathInDataExists = File.Exists(movePaths[1]);

                if (!filePathInDataExists) // skip if was deleted in Data
                {
                    return;
                }

                if (filePathInModsExists)
                {
                    //если в Mods на месте планируемого для перемещения назад в Mods файла появился новый файл, то записать информацию о нем в новый мод, чтобы перенести его в новый мод
                    filesWhichAlreadyHaveSameDestFileInMods.AppendLine(movePaths[1] + operationsSplitStringBase + movePaths[0]);
                    filesWhichAlreadyHaveSameDestFileInModsIsNotEmpty = true;

                    return;
                }
                else
                {
                    string modsubfolder = Path.GetDirectoryName(movePaths[0]);
                    Directory.CreateDirectory(modsubfolder);

                    try//ignore move file error if file will be locked and write in log about this
                    {
                        movePaths[1].MoveTo(movePaths[0]);

                        //запись выполненной операции для удаления из общего списка в случае ошибки при переключении из обычного режима
                        operationsMade.AppendLine(operation);

                        MoveBonemodForCards(movePaths);
                    }
                    catch (Exception ex)
                    {
                        _log.Debug("Failed to move file: '" + Environment.NewLine + movePaths[1] + "' " + Environment.NewLine + "Error:" + Environment.NewLine + ex);
                    }
                }
            });
        }

        private bool IsDirSymlink(string[] movePaths)
        {
            if (movePaths[1].IsDirectory() && movePaths[1].IsSymlink(ObjectType.Directory))
            {
                // we created symlink for dir in Data but same symlink in target is still exists
                if (movePaths[0].IsSymlink(ObjectType.Directory))
                {
                    Directory.Delete(movePaths[1]);
                }

                return true;
            }

            return false;
        }

        private void MoveBonemodForCards(string[] movePaths)
        {
            if (!string.Equals(Path.GetExtension(movePaths[1]), ".png")) // only for cards
            {
                return;
            }

            try
            {
                //Move bonemod file both with original
                if (!File.Exists(movePaths[1] + ".bonemod.txt") || File.Exists(movePaths[0] + ".bonemod.txt"))
                {
                    return;
                }

                (movePaths[1] + ".bonemod.txt").MoveTo(movePaths[0] + ".bonemod.txt");
                //запись выполненной операции для удаления из общего списка в случае ошибки при переключении из обычного режима
                operationsMade.AppendLine(movePaths[1] + ".bonemod.txt" + operationsSplitStringBase + movePaths[0] + ".bonemod.txt");
            }
            catch (Exception ex)
            {
                _log.Debug("An error occured while file moving." + "MovePaths[0]=" + movePaths[0] + ";MovePaths[1]=" + movePaths[0] + ".error:\r\n" + ex);
            }
        }

        private bool FillZipModsGUID(Dictionary<string, string> zipmodsGuidList)
        {
            if (!File.Exists(ManageSettings.CurrentGameZipmodsGuidListFilePath))
            {
                return false;
            }

            using (var sr = new StreamReader(ManageSettings.CurrentGameZipmodsGuidListFilePath))
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
            return zipmodsGuidList.Count > 0;
        }
    }
}
