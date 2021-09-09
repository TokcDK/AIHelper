﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AIHelper.Manage.ModeSwitch
{
    class ToMOMode : ModeSwitcherBase
    {
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
        protected void SwitchBackToMoMode()
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
                bool zipmodsGuidListNotEmpty = FillZipModsGUID(zipmodsGuidList);                

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
                string dateTimeInFormat = ManageSettings.GetDateTimeBasedSuffix();

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
                        string newModName = modName + dateTimeInFormat;
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
                            "0." + dateTimeInFormat.Substring(1)
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
                string addedFilesFolderName = "[added]UseFiles" + dateTimeInFormat;
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
                        dateTimeInFormat.Substring(1)
                        ,
                        T._("Sort files if need")
                        ,
                        T._("<br>This files was added in Common mode<br>and moved as mod after convertation in MO mode.<br>Date: ") + dateTimeInFormat.Substring(1)
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

        private bool FillZipModsGUID(Dictionary<string, string> zipmodsGuidList)
        {
            if (!File.Exists(ManageSettings.GetCurrentGameZipmodsGuidListFilePath()))
            {
                return false;
            }

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
            return zipmodsGuidList.Count > 0;
        }
    }
}
