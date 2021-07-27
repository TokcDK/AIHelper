using AIHelper.SharedData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace AIHelper.Manage
{
    class ManageModOrganizerMods
    {
        public static void SetMoModsVariables()
        {
            Properties.Settings.Default.BepinExCfgPath = ManageSettings.GetBepInExCfgFilePath();
        }

        public static void CleanBepInExLinksFromData()
        {
            MousfsLoadingFix(true);
            ////удаление файлов BepinEx
            //ManageFilesFolders.DeleteIfSymlink(Path.Combine(Properties.Settings.Default.DataPath, "doorstop_config.ini"));
            //ManageFilesFolders.DeleteIfSymlink(Path.Combine(Properties.Settings.Default.DataPath, "winhttp.dll"));
            //string BepInExDir = Path.Combine(Properties.Settings.Default.DataPath, "BepInEx");
            //if (Directory.Exists(BepInExDir))
            //{
            //    Directory.Delete(BepInExDir, true);
            //}

            ////удаление ссылок на папки плагинов BepinEx
            //ManageFilesFolders.DeleteIfSymlink(Path.Combine(Properties.Settings.Default.DataPath, "UserData", "MaterialEditor"), true);
            //ManageFilesFolders.DeleteIfSymlink(Path.Combine(Properties.Settings.Default.DataPath, "UserData", "Overlays"), true);
        }

        internal static bool IsFileDirExistsInDataOrOverwrite(string filedir, out string source)
        {
            //string filePathInOverwrite =
            //           Path.GetFullPath(
            //               Path.Combine(ManageSettings.GetCurrentGameMOOverwritePath() + Path.DirectorySeparatorChar + filedir)
            //                           );
            if (File.Exists(
                       Path.GetFullPath(
                           Path.Combine(ManageSettings.GetCurrentGameMoOverwritePath() + Path.DirectorySeparatorChar + filedir)
                                       )
                       )
              //||
              //Directory.Exists(
              //    Path.GetFullPath(
              //        Path.Combine(ManageSettings.GetCurrentGameMOOverwritePath() + Path.DirectorySeparatorChar + filedir)
              //                    )
              //                )
              )
            {
                source = "overwrite";
                return true;
            }
            else
            {
                //var FilePathInData = Path.GetFullPath(
                //           Path.Combine(ManageSettings.GetCurrentGameDataPath() + Path.DirectorySeparatorChar + filedir)
                //                       );
                if (

                   File.Exists(
                       Path.GetFullPath(
                           Path.Combine(ManageSettings.GetCurrentGameDataPath() + Path.DirectorySeparatorChar + filedir)
                                       )
                                   )
                   //||
                   //Directory.Exists(
                   //    Path.GetFullPath(
                   //        Path.Combine(ManageSettings.GetCurrentGameDataPath() + Path.DirectorySeparatorChar + filedir)
                   //                    )
                   //                )
                   )
                {
                    source = "data";
                    return true;
                }
            }

            source = string.Empty;
            return false;
        }

        internal static void OpenBepinexLog()
        {
            //https://stackoverflow.com/questions/9993561/c-sharp-open-file-path-starting-with-userprofile
            var gameNameByExe = ManageSettings.GetCurrentGameExeName().Replace("_64", string.Empty).Replace("_32", string.Empty);
            var userprofile = Path.Combine("%USERPROFILE%", "appdata", "locallow", "illusion__" + gameNameByExe.Replace("Trial", string.Empty), gameNameByExe, "output_log.txt");
            var outputLog = Environment.ExpandEnvironmentVariables(userprofile);
            if (File.Exists(outputLog))
            {
                Process.Start("explorer.exe", outputLog);
            }
            else
            {
                if (File.Exists(Path.Combine(ManageSettings.GetCurrentGameDataPath(), ManageSettings.GetCurrentGameExeName() + "_Data", "output_log.txt")))
                {
                    Process.Start("explorer.exe", Path.Combine(ManageSettings.GetCurrentGameDataPath(), ManageSettings.GetCurrentGameExeName() + "_Data", "output_log.txt"));
                }
                else if (File.Exists(Path.Combine(ManageSettings.GetCurrentGameDataPath(), gameNameByExe + "_Data", "output_log.txt")))
                {
                    Process.Start("explorer.exe", Path.Combine(ManageSettings.GetCurrentGameDataPath(), gameNameByExe + "_Data", "output_log.txt"));
                }

            }
        }

        public static void MousfsLoadingFix(bool removeLinks = false)
        {
            if (!ManageSettings.IsMoMode())
            {
                return;
            }

            BepInExPreloadersFix(removeLinks);

            string[,] objectLinkPaths = GameData.CurrentGame.GetDirLinkPaths();

            int objectLinkPathsLength = objectLinkPaths.Length / 2;
            for (int i = 0; i < objectLinkPathsLength; i++)
            {
                var objectPath = objectLinkPaths[i, 0];
                var linkPath = objectLinkPaths[i, 1];

                if (removeLinks)
                {
                    ManageSymLinkExtensions.DeleteIfSymlink(linkPath);
                }
                else
                {
                    try
                    {
                        if (ManageSymLinkExtensions.CreateSymlink
                          (
                           objectPath
                           ,
                           linkPath
                           ,
                           true
                           ,
                           ObjectType.Dir
                          ))
                        {
                        }
                        else
                        {
                            // need when dir in data exists and have content, then content will be move to target
                            ManageFilesFolders.MoveContent(linkPath, objectPath);

                            ManageSymLinkExtensions.CreateSymlink
                                (
                                 objectPath
                                 ,
                                 linkPath
                                 ,
                                 true
                                 ,
                                 ObjectType.Dir
                                );
                        }
                    }
                    catch (Exception ex)
                    {
                        ManageLogs.Log("An error occured while symlink creation:\r\n" + ex);
                    }
                }
            }

            //if (RemoveLinks)
            //{
            //    ManageFilesFolders.DeleteEmptySubfolders(Path.Combine(ManageSettings.GetCurrentGameDataPath(), "BepInEx"));
            //}
            //else if (Directory.Exists(Path.Combine(ManageSettings.GetCurrentGameDataPath(), "BepInEx")))
            //{
            //    ManageFilesFolders.HideFileFolder(Path.Combine(ManageSettings.GetCurrentGameDataPath(), "BepInEx"));
            //}
        }

        private static void BepInExPreloadersFix(bool remove = false)
        {
            string[,] bepInExFilesPaths = GameData.CurrentGame.GetObjectsForMove();

            var done = false;

            int bepInExFilesPathsLength = bepInExFilesPaths.Length / 2;
            for (int i = 0; i < bepInExFilesPathsLength; i++)
            {
                var sourceFilePath = bepInExFilesPaths[i, 0];
                var targetFilePath = bepInExFilesPaths[i, 1];
                if (remove)
                {
                    try
                    {
                        if (File.Exists(targetFilePath) && (File.Exists(sourceFilePath) || ManageSymLinkExtensions.IsSymLink(targetFilePath)))
                        {
                            File.Delete(targetFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        ManageLogs.Log("BepInExPreloadersFix error:" + Environment.NewLine + ex);
                    }
                }
                else
                {
                    try
                    {

                        sourceFilePath = ManageModOrganizer.GetLastMoFileDirPathFromEnabledModsOfActiveMoProfile(sourceFilePath);

                        //skip file if source not exists
                        if (!File.Exists(sourceFilePath))
                        {
                            continue;
                        }

                        //skip file if not in enabled mod
                        if (!File.Exists(sourceFilePath) || !ManageModOrganizerMods.IsInEnabledModOrOverwrite(sourceFilePath))//skip if no active mod found
                        {
                            if (File.Exists(targetFilePath))
                            {
                                File.Delete(targetFilePath);
                            }

                            continue;
                        }

                        if (File.Exists(targetFilePath))
                        {
                            if (
                                ManageSymLinkExtensions.IsSymLink(targetFilePath)
                                ||
                                new FileInfo(targetFilePath).Length != new FileInfo(sourceFilePath).Length
                                ||
                                FileVersionInfo.GetVersionInfo(targetFilePath).ProductVersion != FileVersionInfo.GetVersionInfo(sourceFilePath).ProductVersion
                                )
                            {
                                File.Delete(targetFilePath);
                            }
                            else
                            {
                                continue;
                            }
                        }

                        Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath));

                        File.Copy(sourceFilePath, targetFilePath);
                        done = true;
                    }
                    catch (Exception ex)
                    {
                        ManageLogs.Log("BepInExPreloadersFix error:" + Environment.NewLine + ex);
                    }
                }
            }

            ManageFilesFolders.DeleteEmptySubfolders(Path.Combine(ManageSettings.GetCurrentGameDataPath(), "BepInEx"), true);

            //else if (Directory.Exists(Path.Combine(ManageSettings.GetCurrentGameDataPath(), "BepInEx")))
            //{
            //    ManageFilesFolders.HideFileFolder(Path.Combine(ManageSettings.GetCurrentGameDataPath(), "BepInEx"));
            //}
        }

        /// <summary>
        /// true if file in overwrite folder or in any active mod
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <returns></returns>
        private static bool IsInEnabledModOrOverwrite(string sourceFilePath)
        {
            if (sourceFilePath.Contains(ManageSettings.GetOverwriteFolder()) || sourceFilePath.Contains(ManageSettings.GetCurrentGameMoOverwritePath()))
            {
                return true;
            }

            if (sourceFilePath.Contains(ManageSettings.GetCurrentGameModsPath()))
            {
                //remove Mods path slit and get 1st element as modname
                var noModsPath = sourceFilePath.Replace(ManageSettings.GetCurrentGameModsPath(), string.Empty);
                var splittedPath = noModsPath.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
                var modname = splittedPath[0];

                foreach (var name in ManageModOrganizer.GetModNamesListFromActiveMoProfile())
                {
                    if (modname == name)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static void InstallCsScriptsForScriptLoader(string whereFromInstallDir = "")
        {
            whereFromInstallDir = whereFromInstallDir.Length > 0 ? whereFromInstallDir : Properties.Settings.Default.Install2MODirPath;

            string[] csFiles = Directory.GetFiles(whereFromInstallDir, "*.cs");

            if (csFiles.Length > 0)
            {
                foreach (var csFile in csFiles)
                {
                    string name = Path.GetFileNameWithoutExtension(csFile);
                    string author = string.Empty;
                    string description = string.Empty;
                    string modname = "[script]" + name;
                    string moddir = Path.GetDirectoryName(csFile).Replace(whereFromInstallDir, Path.Combine(Properties.Settings.Default.ModsPath, modname));

                    using (StreamReader sReader = new StreamReader(csFile))
                    {
                        string line;
                        bool readDescriptionMode = false;
                        int i = 0;
                        while (!sReader.EndOfStream || (!readDescriptionMode && i == 10))
                        {
                            line = sReader.ReadLine();

                            if (!readDescriptionMode /*&& Line.Length > 0 уже есть эта проверка в StringEx.IsStringAContainsStringB*/ && ManageStrings.IsStringAContainsStringB(line, "/*"))
                            {
                                readDescriptionMode = true;
                                line = line.Replace("/*", string.Empty);
                                if (line.Length > 0)
                                {
                                    description += line + "<br>";
                                }
                            }
                            else
                            {
                                if (ManageStrings.IsStringAContainsStringB(line, "*/"))
                                {
                                    readDescriptionMode = false;
                                    line = line.Replace("*/", string.Empty);
                                }

                                description += line + "<br>";

                                if (!readDescriptionMode)
                                {
                                    break;
                                }
                            }

                            i++;
                        }
                    }
                    string scriptsdir = Path.Combine(moddir, "scripts");
                    if (!Directory.Exists(scriptsdir))
                    {
                        Directory.CreateDirectory(scriptsdir);
                    }

                    string fileTargetPath = Path.Combine(scriptsdir, name + ".cs");
                    bool isUpdate = false;
                    //резервная копия, если файл существовал
                    if (File.Exists(fileTargetPath))
                    {
                        isUpdate = true;
                        if (File.GetLastWriteTime(csFile) > File.GetLastWriteTime(fileTargetPath))
                        {
                            File.Delete(fileTargetPath);
                            File.Move(csFile, fileTargetPath);
                        }
                        else
                        {
                            File.Delete(csFile);
                        }
                    }
                    else
                    {
                        File.Move(csFile, fileTargetPath);
                    }

                    string fileLastModificationTime = File.GetLastWriteTime(csFile).ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture);
                    //запись meta.ini
                    ManageModOrganizer.WriteMetaIni(
                        moddir
                        ,
                        isUpdate ? string.Empty : ManageModOrganizer.GetCategoryIndexForTheName("ScriptLoader scripts") + ","
                        ,
                        "0." + fileLastModificationTime
                        ,
                        isUpdate ? string.Empty : "Requires: " + "ScriptLoader"
                        ,
                        isUpdate ? string.Empty : "<br>" + "Author" + ": " + author + "<br><br>" + (description.Length > 0 ? description : name)
                        );

                    ManageModOrganizer.ActivateDeactivateInsertMod(modname, false, "ScriptLoader scripts_separator");

                    string[] extrafiles = Directory.GetFiles(whereFromInstallDir, name + "*.*");
                    if (extrafiles.Length > 0)
                    {
                        foreach (var extrafile in extrafiles)
                        {
                            string targetFile = extrafile.Replace(whereFromInstallDir, moddir);

                            if (File.Exists(targetFile))
                            {
                                if (File.GetLastWriteTime(extrafile) > File.GetLastWriteTime(targetFile))
                                {
                                    File.Delete(targetFile);
                                    File.Move(extrafile, targetFile);
                                }
                                else
                                {
                                    File.Delete(extrafile);
                                }
                            }
                            else
                            {
                                File.Move(extrafile, targetFile);
                            }
                        }
                    }
                }
            }
        }

        public static void InstallCardsFrom2Mo(string targetDir = "")
        {
            string proceedDir = targetDir.Length == 0 ? Properties.Settings.Default.Install2MODirPath : targetDir;
            var images = Directory.GetFiles(proceedDir, "*.png");
            int imagesLength = images.Length;
            if (imagesLength > 0)
            {
                //bool IsCharaCard = false;
                for (int imgnum = 0; imgnum < imagesLength; imgnum++)
                {
                    string img = images[imgnum];
                    //var imgdata = Image.FromFile(img);

                    //if (imgdata.Width == 252 && imgdata.Height == 352)
                    //{
                    //    IsCharaCard = true;
                    //}

                    var targetImagePath = ManageFilesFolders.GetResultTargetFilePathWithNameCheck(GetUserDataSubFolder(" Chars", "f"), Path.GetFileNameWithoutExtension(img), ".png");

                    File.Move(img, targetImagePath);
                }
            }
        }

        public static void InstallImagesFromSubfolders()
        {
            foreach (var dir in Directory.GetDirectories(Properties.Settings.Default.Install2MODirPath, "*"))
            {
                //string[] folderTypes = 
                //    {
                //    "f" //папка "f" с женскими карточками внутри
                //    ,
                //    "m" //папка "m" с мужскими карточками внутри
                //    ,
                //    "c" //папка "c" с координатами
                //    ,
                //    "h" //папка "h" с проектами домов
                //    ,
                //    "cf"//папка "cf" с передними фреймами
                //    ,
                //    "cb"//папка "cb" с задними фреймами
                //    ,
                //    "o" //папка "o" с оверлеями
                //    ,
                //    "s" //папка "s" с сценами для стидии
                //    };

                string theDirName = Path.GetFileName(dir);
                if (ManageStrings.IsStringAequalsStringB(theDirName, "f", true))
                {
                    //папка "f" с женскими карточками внутри
                    MoveImagesToTargetContentFolder(dir, "f");
                }
                else if (ManageStrings.IsStringAequalsStringB(theDirName, "m", true))
                {
                    //папка "m" с мужскими карточками внутри
                    MoveImagesToTargetContentFolder(dir, "m");
                }
                else if (ManageStrings.IsStringAequalsStringB(theDirName, "c", true))
                {
                    //папка "c" с координатами
                    MoveImagesToTargetContentFolder(dir, "c");
                }
                else if (ManageStrings.IsStringAequalsStringB(theDirName, "h", true)
                    || ManageStrings.IsStringAequalsStringB(theDirName, "h1", true)
                    || ManageStrings.IsStringAequalsStringB(theDirName, "h2", true)
                    || ManageStrings.IsStringAequalsStringB(theDirName, "h3", true)
                    || ManageStrings.IsStringAequalsStringB(theDirName, "h4", true)
                    )
                {
                    //папка "h" с проектами домов
                    MoveImagesToTargetContentFolder(dir, theDirName);
                }
                else if (ManageStrings.IsStringAequalsStringB(theDirName, "cf", true))
                {
                    //папка "cf" с передними фреймами
                    MoveImagesToTargetContentFolder(dir, "cf");
                }
                else if (ManageStrings.IsStringAequalsStringB(theDirName, "cb", true))
                {
                    //папка "cb" с задними фреймами
                    MoveImagesToTargetContentFolder(dir, "cb");
                }
                else if (ManageStrings.IsStringAequalsStringB(theDirName, "o", true))
                {
                    //папка "o" с оверлеями
                    MoveImagesToTargetContentFolder(dir, "o");
                }
                else if (ManageStrings.IsStringAequalsStringB(theDirName, "s", true))
                {
                    //папка "s" с сценами для стидии
                    MoveImagesToTargetContentFolder(dir, "s");
                }
                else
                {
                    var images = Directory.GetFiles(dir, "*.png");
                    if (images.Length > 0)
                    {
                        //Просто произвольная подпапка, которая будет перемещена как новый мод
                        MoveImagesToTargetContentFolder(dir, "f", true);
                        //bool IsCharaCard = false;
                        //foreach (var img in images)
                        //{
                        //    //var imgdata = Image.FromFile(img);

                        //    //if (imgdata.Width == 252 && imgdata.Height == 352)
                        //    //{
                        //    //    IsCharaCard = true;
                        //    //}
                        //    string TargetPath = Path.Combine(GetCharactersFolder(), Path.GetFileName(img));
                        //    File.Move(img, TargetPath);
                        //}

                        //string theDirName = Path.GetFileName(dir);
                        var cardsModDir = Path.Combine(Properties.Settings.Default.ModsPath, theDirName);
                        //var cardsModDir = GetResultTargetDirPathWithNameCheck(Properties.Settings.Default.ModsPath, Path.GetFileName(dir));

                        //Перемещение файлов в ту же папку, если она существует, вместо создания новой
                        if (Directory.Exists(cardsModDir))
                        {
                            foreach (var file in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories))
                            {
                                string fileTarget = file.Replace(Properties.Settings.Default.Install2MODirPath, Properties.Settings.Default.ModsPath);

                                if (File.Exists(fileTarget))
                                {
                                    fileTarget = ManageFilesFolders.GetResultTargetFilePathWithNameCheck(Path.GetDirectoryName(fileTarget), Path.GetFileNameWithoutExtension(fileTarget), Path.GetExtension(fileTarget));
                                }
                                File.Move(file, fileTarget);
                            }
                            ManageFilesFolders.DeleteEmptySubfolders(dir);
                        }
                        else
                        {
                            Directory.Move(dir, cardsModDir);
                        }

                        //запись meta.ini
                        ManageModOrganizer.WriteMetaIni(
                            cardsModDir
                            ,
                            ManageModOrganizer.GetCategoryIndexForTheName("Characters") + ","
                            ,
                            string.Empty
                            ,
                            string.Empty
                            ,
                            "<br>Author: " + string.Empty + "<br><br>" + Path.GetFileNameWithoutExtension(cardsModDir) + " character cards<br><br>"
                            );

                        ManageModOrganizer.ActivateDeactivateInsertMod(Path.GetFileName(cardsModDir), true, "UserCharacters_separator");
                    }
                }
            }
        }

        public static void MoveImagesToTargetContentFolder(string dir, string contentType, bool moveInThisFolder = false)
        {
            if (Directory.Exists(dir))
            {
                string targetFolder = string.Empty;
                string extension = ".png";
                if (contentType == "f")
                {
                    //TargetFolder = GetCharactersFolder();
                    targetFolder = GetUserDataSubFolder(moveInThisFolder ? dir : " Chars", contentType);
                }
                else if (contentType == "m")
                {
                    //TargetFolder = GetCharactersFolder(true);
                    targetFolder = GetUserDataSubFolder(moveInThisFolder ? dir : " Chars", contentType);
                }
                else if (contentType == "c")
                {
                    //TargetFolder = GetCoordinateFolder();
                    targetFolder = GetUserDataSubFolder(moveInThisFolder ? dir : " Coordinate", contentType);
                }
                else if (contentType == "h"
                    || contentType == "h1"
                    || contentType == "h2"
                    || contentType == "h3"
                    || contentType == "h4"
                    )
                {
                    //TargetFolder = GetCoordinateFolder();
                    targetFolder = GetUserDataSubFolder(moveInThisFolder ? dir : " Housing", contentType);
                }
                else if (contentType == "cf")
                {
                    //TargetFolder = GetCardFrameFolder();
                    targetFolder = GetUserDataSubFolder(moveInThisFolder ? dir : " Cardframes", contentType);
                }
                else if (contentType == "cb")
                {
                    //TargetFolder = GetCardFrameFolder(false);
                    targetFolder = GetUserDataSubFolder(moveInThisFolder ? dir : " Cardframes", contentType);
                }
                else if (contentType == "o")
                {
                    //TargetFolder = GetOverlaysFolder();
                    targetFolder = GetUserDataSubFolder(moveInThisFolder ? dir : " Overlays", contentType);
                }
                else if (contentType == "s")
                {
                    //TargetFolder = GetOverlaysFolder();
                    targetFolder = GetUserDataSubFolder(moveInThisFolder ? dir : " Scenes", contentType);
                }

                //Для всех, сброс png из корневой папки в целевую
                foreach (var target in Directory.GetFiles(dir, "*" + extension))
                {
                    var cardframeTargetFolder = ManageFilesFolders.GetResultTargetFilePathWithNameCheck(targetFolder, Path.GetFileNameWithoutExtension(target), extension);

                    File.Move(target, cardframeTargetFolder);
                }

                if (contentType == "o")
                {
                    ManageArchive.UnpackArchivesToSubfoldersWithSameName(dir, ".zip");
                    foreach (var oSubDir in Directory.GetDirectories(dir, "*"))
                    {
                        string newTarget = ManageFilesFolders.MoveFolderToOneLevelUpIfItAloneAndReturnMovedFolderPath(oSubDir);
                        string targetDirName = Path.GetFileName(newTarget);
                        var resultTargetPath = ManageFilesFolders.GetResultTargetDirPathWithNameCheck(targetFolder, targetDirName);

                        Directory.Move(newTarget, resultTargetPath);
                    }
                }
                else if (contentType == "f")
                {
                    string maleDir = Path.Combine(dir, "m");
                    if (Directory.Exists(maleDir))
                    {
                        foreach (var target in Directory.GetFiles(maleDir, "*.png"))
                        {
                            string name = Path.GetFileName(target);
                            File.Move(target, Path.Combine(dir, name));
                        }
                        Directory.Move(maleDir, maleDir + "_");
                        MoveImagesToTargetContentFolder(dir, "m", moveInThisFolder);
                    }
                }
                else if (contentType == "h"
                    || contentType == "h1"
                    || contentType == "h2"
                    || contentType == "h3"
                    || contentType == "h4"
                    )
                {
                    if (contentType.Length == 2)
                    {
                        foreach (var file in Directory.GetFiles(dir))
                        {
                            File.Move(file, ManageFilesFolders.GetResultTargetFilePathWithNameCheck(targetFolder, Path.GetFileNameWithoutExtension(file), ".png"));
                        }
                    }
                    else
                    {
                        foreach (var typeDir in Directory.GetDirectories(dir))
                        {
                            string hSubDirName = Path.GetFileName(typeDir);
                            foreach (var file in Directory.GetFiles(typeDir))
                            {
                                File.Move(file, ManageFilesFolders.GetResultTargetFilePathWithNameCheck(Path.Combine(targetFolder, hSubDirName), Path.GetFileNameWithoutExtension(file), ".png"));
                            }
                        }

                    }
                }

                ManageFilesFolders.DeleteEmptySubfolders(dir);
            }
        }

        public static string GetUserDataSubFolder(string firstCandidateFolder, string type)
        {
            string[] targetFolders = new string[3]
            {
                firstCandidateFolder.Substring(0,1)== " " ? Path.Combine(Properties.Settings.Default.ModsPath, "OrganizedModPack Downloaded"+firstCandidateFolder) : firstCandidateFolder,
                Path.Combine(Properties.Settings.Default.ModsPath, "MyUserData"),
                Properties.Settings.Default.OverwriteFolder
            };

            string typeFolder = string.Empty;
            string targetFolderName = string.Empty;
            if (type == "f")
            {
                typeFolder = "chara";
                targetFolderName = "female";
            }
            else if (type == "m")
            {
                typeFolder = "chara";
                targetFolderName = "male";
            }
            else if (type == "c")
            {
                targetFolderName = "coordinate";
                //TypeFolder = "";
            }
            else if (type == "h"
                || type == "h1"
                || type == "h2"
                || type == "h3"
                || type == "h4"
                )
            {
                typeFolder = "housing";
                targetFolderName = type.Length == 2 ? "0" + type.Remove(0, 1) : string.Empty;
            }
            else if (type == "cf")
            {
                typeFolder = "cardframe";
                targetFolderName = "Front";
            }
            else if (type == "cb")
            {
                typeFolder = "cardframe";
                targetFolderName = "Back";
            }
            else if (type == "o")
            {
                //TypeFolder = "";
                targetFolderName = "Overlays";
            }
            else if (type == "s")
            {
                typeFolder = "studio";
                targetFolderName = "scene";
            }

            int targetFoldersLength = targetFolders.Length;
            for (int i = 0; i < targetFoldersLength; i++)
            {
                string folder = targetFolders[i];
                if (Directory.Exists(folder))
                {
                    var targetResultDirPath = Path.Combine(folder, "UserData", typeFolder, targetFolderName);
                    if (!Directory.Exists(targetResultDirPath))
                    {
                        Directory.CreateDirectory(targetResultDirPath);
                    }
                    return targetResultDirPath;
                }
            }

            return Path.Combine(Properties.Settings.Default.OverwriteFolder, "UserData", typeFolder, targetFolderName);
        }

        public static void InstallZipArchivesToMods()
        {
            foreach (var zipfile in Directory.GetFiles(Properties.Settings.Default.Install2MODirPath, "*.zip"))
            {
                //следующий, если не существует
                if (!File.Exists(zipfile))
                {
                    continue;
                }

                bool foundZipMod = false;
                bool foundStandardModInZip = false;
                bool foundModsDir = false;
                bool foundcsFiles = false;

                string author = string.Empty;
                string category = string.Empty;
                string version = string.Empty;
                string comment = string.Empty;
                string description = string.Empty;
                string updateModNameFromMeta = string.Empty;
                bool foundUpdateName = false;
                string modFolderForUpdate = string.Empty;
                string zipName = Path.GetFileNameWithoutExtension(zipfile);
                string targetFileAny = string.Empty;

                int filesCount = 0;

                using (ZipArchive archive = ZipFile.OpenRead(zipfile))
                {
                    filesCount = ManageArchive.GetFilesCountInZipArchive(archive);

                    int archiveEntriesCount = archive.Entries.Count;
                    for (int entrieNum = 0; entrieNum < archiveEntriesCount; entrieNum++)
                    {
                        //если один файл, распаковать в подпапку
                        if (filesCount == 1)
                        {
                            archive.ExtractToDirectory(Path.Combine(Properties.Settings.Default.Install2MODirPath, Path.GetFileNameWithoutExtension(zipfile)));
                            break;
                        }

                        string entryName = archive.Entries[entrieNum].Name;
                        string entryFullName = archive.Entries[entrieNum].FullName;

                        int entryFullNameLength = entryFullName.Length;
                        if (!foundZipMod && entryFullNameLength >= 12 && string.Compare(entryFullName.Substring(entryFullNameLength - 12, 12), "manifest.xml", true, CultureInfo.InvariantCulture) == 0) //entryFullName=="manifest.xml"
                        {
                            foundZipMod = true;
                            break;
                        }

                        if (!foundStandardModInZip)
                        {
                            if (
                                   (entryFullNameLength >= 7 && (string.Compare(entryFullName.Substring(entryFullNameLength - 7, 6), "abdata", true, CultureInfo.InvariantCulture) == 0 || string.Compare(entryFullName.Substring(0, 6), "abdata", true, CultureInfo.InvariantCulture) == 0)) //entryFullName=="abdata/"
                                || (entryFullNameLength >= 6 && (string.Compare(entryFullName.Substring(entryFullNameLength - 6, 5), "_data", true, CultureInfo.InvariantCulture) == 0/*тут только проверка на окончание нужна || string.Compare(entryFullName.Substring(0, 5), "_data", true, CultureInfo.InvariantCulture) == 0*/)) //entryFullName=="_data/"
                                || (entryFullNameLength >= 8 && (string.Compare(entryFullName.Substring(entryFullNameLength - 8, 7), "bepinex", true, CultureInfo.InvariantCulture) == 0 || string.Compare(entryFullName.Substring(0, 7), "bepinex", true, CultureInfo.InvariantCulture) == 0)) //entryFullName=="bepinex/"
                                || (entryFullNameLength >= 9 && (string.Compare(entryFullName.Substring(entryFullNameLength - 9, 8), "userdata", true, CultureInfo.InvariantCulture) == 0 || string.Compare(entryFullName.Substring(0, 8), "userdata", true, CultureInfo.InvariantCulture) == 0)) //entryFullName=="userdata/"
                               )
                            {
                                foundStandardModInZip = true;
                            }
                        }

                        //когда найдена папка mods, если найден zipmod
                        if (foundModsDir && !foundStandardModInZip)
                        {
                            if (entryFullNameLength >= 7 && string.Compare(entryFullName.Substring(entryFullNameLength - 7, 7), ".zipmod", true, CultureInfo.InvariantCulture) == 0)//entryFullName==".zipmod"
                            {
                                if (filesCount > 1)
                                {
                                    archive.ExtractToDirectory(Path.Combine(Properties.Settings.Default.Install2MODirPath, Path.GetFileNameWithoutExtension(zipfile)));
                                }
                                else
                                {
                                    archive.Entries[entrieNum].ExtractToFile(Path.Combine(Properties.Settings.Default.Install2MODirPath, entryName));
                                }
                                break;
                            }
                            else if (entryFullNameLength >= 4 && string.Compare(entryFullName.Substring(entryFullNameLength - 4, 4), ".zip", true, CultureInfo.InvariantCulture) == 0)
                            {
                                if (filesCount > 1)
                                {
                                    archive.ExtractToDirectory(Path.Combine(Properties.Settings.Default.Install2MODirPath, Path.GetFileNameWithoutExtension(zipfile)));
                                }
                                else
                                {
                                    archive.Entries[entrieNum].ExtractToFile(Path.Combine(Properties.Settings.Default.Install2MODirPath, entryName + "mod"));
                                }
                                break;
                            }
                        }

                        //если найдена папка mods
                        if (!foundModsDir && entryFullNameLength >= 5 && (string.Compare(entryFullName.Substring(entryFullNameLength - 5, 4), "mods", true, CultureInfo.InvariantCulture) == 0 || string.Compare(entryFullName.Substring(0, 4), "mods", true, CultureInfo.InvariantCulture) == 0))
                        {
                            foundModsDir = true;
                        }

                        //если найден cs
                        if (!foundcsFiles && entryFullNameLength >= 3 && string.Compare(entryFullName.Substring(entryFullNameLength - 3, 3), ".cs", true, CultureInfo.InvariantCulture) == 0)
                        {
                            foundStandardModInZip = false;
                            foundcsFiles = true;
                            break;
                        }

                        //получение информации о моде из dll
                        if (entryFullNameLength >= 4 && string.Compare(entryFullName.Substring(entryFullNameLength - 4, 4), ".dll", true, CultureInfo.InvariantCulture) == 0)
                        {
                            if (description.Length == 0 && version.Length == 0 && author.Length == 0)
                            {
                                string temp = Path.Combine(Properties.Settings.Default.Install2MODirPath, "temp");
                                string entryPath = Path.Combine(temp, entryFullName);
                                string entryDir = Path.GetDirectoryName(entryPath);
                                if (!Directory.Exists(entryDir))
                                {
                                    Directory.CreateDirectory(entryDir);
                                }

                                archive.Entries[entrieNum].ExtractToFile(entryPath);

                                if (File.Exists(entryPath))
                                {
                                    FileVersionInfo dllInfo = FileVersionInfo.GetVersionInfo(entryPath);
                                    description = dllInfo.FileDescription;
                                    version = dllInfo.FileVersion;
                                    //string version = dllInfo.ProductVersion;
                                    string copyright = dllInfo.LegalCopyright;

                                    if (copyright.Length >= 4)
                                    {
                                        //"Copyright © AuthorName 2019"
                                        author = copyright.Remove(copyright.Length - 4, 4).Replace("Copyright © ", string.Empty).Trim();
                                    }

                                    string[] modsList = ManageModOrganizer.GetModNamesListFromActiveMoProfile(false);

                                    foreach (var modFolder in modsList)
                                    {
                                        modFolderForUpdate = Path.Combine(Properties.Settings.Default.ModsPath, modFolder);
                                        string targetfile = Path.Combine(modFolderForUpdate, entryFullName);
                                        targetFileAny = GetTheDllFromSubfolders(modFolderForUpdate, entryName.Remove(entryName.Length - 4, 4), "dll");
                                        if (File.Exists(targetfile) || (targetFileAny.Length > 0 && File.Exists(targetFileAny)))
                                        {
                                            if (targetFileAny.Length > 0)
                                            {
                                                targetfile = targetFileAny;
                                            }

                                            updateModNameFromMeta = ManageIni.GetIniValueIfExist(Path.Combine(modFolderForUpdate, "meta.ini"), "notes", "General");
                                            if (updateModNameFromMeta.Length > 0)
                                            {
                                                int upIndex = updateModNameFromMeta.IndexOf("ompupname:", StringComparison.InvariantCultureIgnoreCase);
                                                if (upIndex > -1)
                                                {
                                                    //get update name
                                                    updateModNameFromMeta = updateModNameFromMeta.Substring(upIndex).Split(':')[1];
                                                    if (updateModNameFromMeta.Length > 0 && zipName.Length >= updateModNameFromMeta.Length && ManageStrings.IsStringAContainsStringB(zipName, updateModNameFromMeta))
                                                    {
                                                        foundUpdateName = true;
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        updateModNameFromMeta = string.Empty;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    //File.Delete(entryPath);
                                    Directory.Delete(temp, true);
                                }
                            }
                        }
                    }
                }

                if (filesCount == 1)
                {
                    File.Move(zipfile, zipfile + ".ExtractedToSubdirAndMustBeInstalled");
                }
                else if (foundZipMod)
                {
                    //если файл имеет расширение zip. Надо, т.к. здесь может быть файл zipmod
                    if (zipfile.Length >= 4 && string.Compare(zipfile.Substring(zipfile.Length - 4, 4), ".zip", true, CultureInfo.InvariantCulture) == 0)
                    {
                        File.Move(zipfile, zipfile + "mod");
                    }
                    InstallZipModsToMods();//будет после установлено соответствующей функцией
                }
                else if (foundStandardModInZip)
                {
                    string targetModDirPath;
                    if (foundUpdateName)
                    {
                        targetModDirPath = Path.Combine(Properties.Settings.Default.Install2MODirPath, zipName + "_temp");
                    }
                    else
                    {
                        targetModDirPath = Path.Combine(Properties.Settings.Default.ModsPath, zipName);
                    }

                    Compressor.Decompress(zipfile, targetModDirPath);

                    if (foundUpdateName)
                    {
                        string[] modfiles = Directory.GetFiles(targetModDirPath, "*.*", SearchOption.AllDirectories);
                        foreach (var file in modfiles)
                        {
                            //ModFolderForUpdate
                            string targetFIle = file.Replace(targetModDirPath, modFolderForUpdate);
                            string targetFileDir = Path.GetDirectoryName(targetFIle);
                            bool targetfileIsNewerOrSame = false;
                            if (File.Exists(targetFIle))
                            {
                                if (File.GetLastWriteTime(file) > File.GetLastWriteTime(targetFIle))
                                {
                                    File.Delete(targetFIle);
                                }
                                else
                                {
                                    targetfileIsNewerOrSame = true;
                                }
                            }
                            else
                            {
                                if (
                                targetFIle.Length >= 4 && targetFIle.Substring(targetFIle.Length - 4, 4) == ".dll"
                                && Path.GetFileNameWithoutExtension(targetFileAny) == Path.GetFileNameWithoutExtension(file)
                                && File.Exists(targetFileAny)
                                   )
                                {
                                    if (File.GetLastWriteTime(file) > File.GetLastWriteTime(targetFileAny))
                                    {
                                        File.Delete(targetFileAny);
                                    }
                                    else
                                    {
                                        targetfileIsNewerOrSame = true;
                                    }
                                }
                            }
                            if (!Directory.Exists(targetFileDir))
                            {
                                Directory.CreateDirectory(targetFileDir);
                            }
                            if (targetfileIsNewerOrSame)
                            {
                                File.Delete(file);
                            }
                            else
                            {
                                File.Move(file, targetFIle);
                            }
                        }
                        Directory.Delete(targetModDirPath, true);
                        //присваивание папки на целевую, после переноса, для дальнейшей работы с ней
                        targetModDirPath = modFolderForUpdate;
                    }

                    File.Move(zipfile, zipfile + (foundUpdateName ? ".InstalledUpdatedMod" : ".InstalledExtractedToMods"));

                    if (version.Length == 0)
                    {
                        version = Regex.Match(zipName, @"\d+(\.\d+)*").Value;
                    }
                    if (!foundUpdateName && author.Length == 0)
                    {
                        author = zipName.StartsWith("[AI][", StringComparison.InvariantCulture) || (zipName.StartsWith("[", StringComparison.InvariantCulture) && !zipName.StartsWith("[AI]", StringComparison.InvariantCulture)) ? zipName.Substring(zipName.IndexOf("[", StringComparison.InvariantCulture) + 1, zipName.IndexOf("]", StringComparison.InvariantCulture) - 1) : string.Empty;
                    }

                    //запись meta.ini
                    ManageModOrganizer.WriteMetaIni(
                        targetModDirPath
                        ,
                        foundUpdateName ? string.Empty : category
                        ,
                        version
                        ,
                        foundUpdateName ? string.Empty : comment
                        ,
                        foundUpdateName ? string.Empty : "<br>Author: " + author + "<br><br>" + (description.Length > 0 ? description : Path.GetFileNameWithoutExtension(zipfile)) + "<br><br>"
                        );

                    ManageModOrganizer.ActivateDeactivateInsertMod(Path.GetFileName(targetModDirPath));
                }
                else if (foundModsDir)
                {
                    if (filesCount > 1)
                    {
                        File.Move(zipfile, zipfile + ".InstalledExtractedToSubfolder");
                    }
                    else
                    {
                        File.Move(zipfile, zipfile + ".InstalledExtractedZipmod");
                    }
                }
                else if (foundcsFiles)
                {
                    //extract to handle as subdir
                    string extractpath = Path.Combine(Properties.Settings.Default.Install2MODirPath, Path.GetFileNameWithoutExtension(zipfile));
                    Compressor.Decompress(zipfile, extractpath);
                    File.Move(zipfile, zipfile + ".InstalledExtractedAsSubfolder");
                }
            }
        }

        public static string GetTheDllFromSubfolders(string dir, string fileName, string extension)
        {
            if (Directory.Exists(dir))
            {
                foreach (var file in Directory.GetFiles(dir, "*." + extension, SearchOption.AllDirectories))
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    if (string.Compare(name, fileName, true, CultureInfo.InvariantCulture) == 0)
                    {
                        return file;
                    }
                }
            }
            return string.Empty;
        }

        public static void InstallModFilesFromSubfolders()
        {
            foreach (var dirIn2Mo in Directory.GetDirectories(Properties.Settings.Default.Install2MODirPath, "*"))
            {
                string name = Path.GetFileName(dirIn2Mo);
                if (ManageStrings.IsStringAequalsStringB(name, "Temp", true)
                    || ManageStrings.IsStringAequalsStringB(name, "f")
                    || ManageStrings.IsStringAequalsStringB(name, "m")
                    || ManageStrings.IsStringAequalsStringB(name, "c")
                    || ManageStrings.IsStringAequalsStringB(name, "cf")
                    || ManageStrings.IsStringAequalsStringB(name, "cb")
                    || ManageStrings.IsStringAequalsStringB(name, "h")
                    || ManageStrings.IsStringAequalsStringB(name, "h1")
                    || ManageStrings.IsStringAequalsStringB(name, "h2")
                    || ManageStrings.IsStringAequalsStringB(name, "h3")
                    || ManageStrings.IsStringAequalsStringB(name, "h4")
                    || ManageStrings.IsStringAequalsStringB(name, "o")
                    || ManageStrings.IsStringAequalsStringB(name, "s")
                    )//path ends with 'Temp'
                {
                    continue;
                }

                //сортировка по подпапкам и переименование файлов
                SortFilesToSubfolders(dirIn2Mo);

                string dir = ManageFilesFolders.MoveFolderToOneLevelUpIfItAloneAndReturnMovedFolderPath(dirIn2Mo);
                name = Path.GetFileName(dir);
                string category = string.Empty;
                string version = string.Empty;
                string author = GetAuthorName(dir, name);//получение имени автора из имени файла или других файлов
                string comment = string.Empty;
                string description = string.Empty;
                string moddir = string.Empty;

                bool anyModFound = false;

                string[] subDirs = Directory.GetDirectories(dir, "*");

                //when was extracted archive where is one folder with same name and this folder contains game files
                //upd. уже сделано выше через ManageFilesFolders.MoveFolderToOneLevelUpIfItAloneAndReturnMovedFolderPath(dirIn2mo)
                //if (subDirs.Length == 1 && Path.GetFileName(subDirs[0]) == name)
                //{
                //    //re-set dir to this one subdir and get dirs from there
                //    dir = subDirs[0];
                //    subDirs = Directory.GetDirectories(dir, "*");
                //}

                var invariantCulture = System.Globalization.CultureInfo.InvariantCulture;

                int subDirsLength = subDirs.Length;
                for (int i = 0; i < subDirsLength; i++)
                {
                    string subdir = subDirs[i];
                    string subdirname = Path.GetFileName(subdir);

                    if (
                           string.Compare(subdirname, "abdata", true, invariantCulture) == 0
                        || string.Compare(subdirname, "userdata", true, invariantCulture) == 0
                        || string.Compare(subdirname, "ai-syoujyotrial_data", true, invariantCulture) == 0
                        || string.Compare(subdirname, "ai-syoujyo_data", true, invariantCulture) == 0
                        || string.Compare(subdirname, "StudioNEOV2_Data", true, invariantCulture) == 0
                        || string.Compare(subdirname, "manual_s", true, invariantCulture) == 0
                        || string.Compare(subdirname, "manual", true, invariantCulture) == 0
                        || string.Compare(subdirname, "MonoBleedingEdge", true, invariantCulture) == 0
                        || string.Compare(subdirname, "DefaultData", true, invariantCulture) == 0
                        || string.Compare(subdirname, "bepinex", true, invariantCulture) == 0
                        || string.Compare(subdirname, "scripts", true, invariantCulture) == 0
                        || string.Compare(subdirname, "mods", true, invariantCulture) == 0
                        )
                    {
                        //CopyFolder.Copy(dir, Path.Combine(Properties.Settings.Default.ModsPath, dir));
                        //Directory.Move(dir, "[installed]" + dir);

                        //имя папки без GetResultTargetDirPathWithNameCheck для того, чтобы обновить существующую, если такая найдется
                        var targetModDIr = Path.Combine(
                            Properties.Settings.Default.ModsPath,
                            (author.Length > 0 && !ManageStrings.IsStringAContainsStringB(name, author))
                                ?
                                "[" + author + "]" + name
                                :
                                name);
                        //var TargetModDIr = ManageFilesFolders.GetResultTargetDirPathWithNameCheck(
                        //    Properties.Settings.Default.ModsPath, 
                        //    (author.Length > 0 && !ManageStrings.IsStringAContainsStringB(name, author))
                        //        ?
                        //        "[" + author + "]" + name
                        //        : 
                        //        name);


                        if (Directory.Exists(targetModDIr))
                        {
                            foreach (var file in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories))
                            {
                                string fileTarget = file.Replace(dir, targetModDIr);
                                if (File.Exists(fileTarget))
                                {
                                    if (File.GetLastWriteTime(file) > File.GetLastWriteTime(fileTarget))
                                    {
                                        File.Delete(fileTarget);
                                        File.Move(file, fileTarget);
                                    }
                                }
                                else
                                {
                                    File.Move(file, fileTarget);
                                }
                            }
                            //ManageFilesFolders.DeleteEmptySubfolders(dir);
                            Directory.Delete(dir, true);
                        }
                        else
                        {
                            Directory.Move(dir, targetModDIr);
                        }

                        moddir = targetModDIr;
                        anyModFound = true;
                        version = Regex.Match(name, @"\d+(\.\d+)*").Value;
                        description = name;
                        break;
                    }
                }

                if (!anyModFound)
                {
                    moddir = dir.Replace(Properties.Settings.Default.Install2MODirPath, Properties.Settings.Default.ModsPath);
                    string targetfilepath = "readme.txt";
                    foreach (var file in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories))
                    {
                        if (Path.GetExtension(file) == ".zipmod")
                        {
                            string newpath = Path.Combine(ManageSettings.GetInstall2MoDirPath(), Path.GetFileName(file));
                            File.Move(file, newpath);
                            InstallZipModsToMods();
                        }
                        else if (Path.GetExtension(file) == ".dll")
                        {
                            string newpath = Path.Combine(ManageSettings.GetInstall2MoDirPath(), Path.GetFileName(file));
                            File.Move(file, newpath);
                            InstallBepinExModsToMods();
                        }
                        else if (string.Compare(Path.GetExtension(file), ".unity3d", true, invariantCulture) == 0)//if extension == .unity3d
                        {
                            //string[] datafiles = Directory.GetFiles(dir, Path.GetFileName(file), SearchOption.AllDirectories);

                            DirectoryInfo dirinfo = new DirectoryInfo(ManageSettings.GetCurrentGameDataPath());

                            var datafiles = dirinfo.GetFiles(Path.GetFileName(file), SearchOption.AllDirectories);

                            if (datafiles.Length > 0)
                            {
                                string selectedfile = datafiles[0].FullName;
                                if (datafiles.Length > 1)
                                {
                                    long size = 0;
                                    for (int f = 0; f < datafiles.Length; f++)
                                    {
                                        if (datafiles[f].Length > size)
                                        {
                                            size = datafiles[f].Length;
                                            selectedfile = datafiles[f].FullName;
                                        }
                                    }
                                }

                                targetfilepath = selectedfile.Replace(ManageSettings.GetCurrentGameDataPath(), moddir);

                                Directory.CreateDirectory(Path.GetDirectoryName(targetfilepath));
                                File.Move(file, targetfilepath);
                            }
                            anyModFound = true;
                        }
                        else if (string.Compare(Path.GetExtension(file), ".cs", true, invariantCulture) == 0)//if extension == .cs
                        {
                            string targetsubdirpath = Path.Combine(moddir, "scripts");
                            if (!Directory.Exists(targetsubdirpath))
                            {
                                Directory.CreateDirectory(targetsubdirpath);
                            }

                            File.Move(file, Path.Combine(targetsubdirpath, Path.GetFileName(file)));
                            if (comment.Length == 0 || !ManageStrings.IsStringAContainsStringB(comment, "Requires: ScriptLoader"))
                            {
                                comment += " Requires: ScriptLoader";
                            }
                            string categoryIndex = ManageModOrganizer.GetCategoryIndexForTheName("ScriptLoader scripts");
                            if (categoryIndex.Length > 0 && (category.Length == 0 || !ManageStrings.IsStringAContainsStringB(category, categoryIndex)))
                            {
                                if (category.Length == 0 || category == "-1,")
                                {
                                    category = categoryIndex + ",";
                                }
                                else
                                {
                                    category += category.EndsWith(",", StringComparison.InvariantCulture) ? categoryIndex : "," + categoryIndex;
                                }
                            }

                            anyModFound = true;
                        }
                    }

                    if (anyModFound)
                    {
                        string[] txts = Directory.GetFiles(dir, "*.txt");
                        string infofile = string.Empty;
                        if (txts.Length > 0)
                        {
                            foreach (string txt in txts)
                            {
                                string txtFileName = Path.GetFileName(txt);

                                if (
                                        string.Compare(txt, "readme.txt", true, invariantCulture) == 0
                                    || string.Compare(txt, "description.txt", true, invariantCulture) == 0
                                    || string.Compare(txt, Path.GetFileName(dir) + ".txt", true, invariantCulture) == 0
                                    || string.Compare(txt, Path.GetFileNameWithoutExtension(targetfilepath) + ".txt", true, invariantCulture) == 0
                                    )
                                {
                                    infofile = txt;
                                }
                            }

                            if (File.Exists(Path.Combine(dir, Path.GetFileName(dir) + ".txt")))
                            {
                                infofile = Path.Combine(dir, Path.GetFileName(dir) + ".txt");
                            }
                            else if (File.Exists(Path.Combine(dir, "readme.txt")))
                            {
                                infofile = Path.Combine(dir, "readme.txt");
                            }
                            else if (File.Exists(Path.Combine(dir, "description.txt")))
                            {
                                infofile = Path.Combine(dir, "description.txt");
                            }
                            else if (File.Exists(Path.Combine(dir, Path.GetFileNameWithoutExtension(targetfilepath) + ".txt")))
                            {
                                infofile = Path.Combine(dir, Path.GetFileNameWithoutExtension(targetfilepath) + ".txt");
                            }
                        }

                        bool d = false;
                        if (infofile.Length > 0)
                        {
                            string[] filecontent = File.ReadAllLines(infofile);
                            for (int l = 0; l < filecontent.Length; l++)
                            {
                                if (d)
                                {
                                    description += filecontent[l] + "<br>";
                                }
                                else if (filecontent[l].StartsWith("name:", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    string s = filecontent[l].Replace("name:", string.Empty);
                                    if (s.Length > 1)
                                    {
                                        name = s;
                                    }
                                }
                                else if (filecontent[l].StartsWith("author:", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    string s = filecontent[l].Replace("author:", string.Empty);
                                    if (s.Length > 1)
                                    {
                                        author = s;
                                    }
                                }
                                else if (filecontent[l].StartsWith("version:", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    string s = filecontent[l].Replace("version:", string.Empty);
                                    if (s.Length > 1)
                                    {
                                        version = s;
                                    }
                                }
                                else if (filecontent[l].StartsWith("description:", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    description += filecontent[l].Replace("description:", string.Empty) + "<br>";
                                    d = true;
                                }
                            }
                            if (File.Exists(infofile))
                            {
                                File.Move(infofile, Path.Combine(moddir, Path.GetFileName(infofile)));
                            }
                        }
                    }
                }

                if (anyModFound)
                {
                    if (author.Length == 0 && ManageFilesFolders.IsAnyFileExistsInTheDir(moddir, ".dll"))
                    {
                        foreach (string dll in Directory.GetFiles(moddir, "*.dll", SearchOption.AllDirectories))
                        {
                            FileVersionInfo dllInfo = FileVersionInfo.GetVersionInfo(dll);

                            if (description.Length == 0)
                            {
                                description = dllInfo.FileDescription;
                            }
                            if (version.Length == 0)
                            {
                                version = dllInfo.FileVersion;
                            }
                            if (version.Length == 0)
                            {
                                version = dllInfo.FileVersion;
                            }
                            if (author.Length == 0)
                            {
                                author = dllInfo.LegalCopyright;
                                //"Copyright © AuthorName 2019"
                                if (!string.IsNullOrEmpty(author))
                                {
                                    if (author.Length >= 4)//удаление года, строка должна быть не менее 4 символов для этого
                                    {
                                        author = author.Remove(author.Length - 4, 4).Replace("Copyright © ", string.Empty).Trim();
                                    }
                                }
                                else
                                {
                                    author = string.Empty;
                                }
                            }
                        }
                    }

                    if (version.Length == 0)
                    {
                        var r = Regex.Match(Path.GetFileName(moddir), @"v(\d+(\.\d+)*)");
                        if (r.Value.Length > 1)
                        {
                            version = r.Value.Substring(1);
                        }
                    }

                    //Задание доп. категорий по наличию папок
                    category = ManageModOrganizer.GetCategoriesForTheFolder(moddir, category);

                    //запись meta.ini
                    ManageModOrganizer.WriteMetaIni(
                        moddir
                        ,
                        category
                        ,
                        version
                        ,
                        comment
                        ,
                        "<br>Author: " + author + "<br><br>" + description
                        );
                    //Utils.IniFile INI = new Utils.IniFile(Path.Combine(dllmoddirpath, "meta.ini"));
                    //INI.WriteINI("General", "category", "\"51,\"");
                    //INI.WriteINI("General", "version", version);
                    //INI.WriteINI("General", "gameName", "Skyrim");
                    //INI.WriteINI("General", "comments", "Requires: BepinEx");
                    //INI.WriteINI("General", "notes", "\"<br>Author: " + author + "<br><br>" + description + "<br><br>" + copyright + " \"");
                    //INI.WriteINI("General", "validated", "true");

                    ManageModOrganizer.ActivateDeactivateInsertMod(Path.GetFileName(moddir));
                }
            }
        }

        public static string GetAuthorName(string subdir = null, string name = null)
        {
            string author = string.Empty;

            if (!string.IsNullOrEmpty(subdir))
            {
                string modsDir = Path.GetFileName(subdir) == "mods" ? subdir : Path.Combine(subdir, "mods");
                if (Directory.Exists(modsDir) && ManageFilesFolders.IsAnyFileExistsInTheDir(modsDir, ".zip", false))
                {
                    foreach (var zipfile in Directory.GetFiles(modsDir, "*.zip"))
                    {
                        using (ZipArchive archive = ZipFile.OpenRead(zipfile))
                        {
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                if (entry.FullName.EndsWith("manifest.xml", StringComparison.OrdinalIgnoreCase))
                                {
                                    string tempDir = Path.Combine(modsDir, "temp");
                                    if (Directory.Exists(tempDir))
                                    {
                                    }
                                    else
                                    {
                                        Directory.CreateDirectory(tempDir);
                                    }

                                    string xmlpath = Path.Combine(tempDir, entry.FullName);
                                    entry.ExtractToFile(xmlpath);

                                    author = ManageXml.ReadXmlValue(xmlpath, "manifest/author", "author");

                                    File.Delete(xmlpath);
                                    ManageFilesFolders.DeleteEmptySubfolders(tempDir);
                                    break;
                                }
                            }
                            if (author.Length > 0)
                            {
                                return author;
                            }
                        }

                    }
                }

                if (/*author.Length == 0 &&*/ ManageFilesFolders.IsAnyFileExistsInTheDir(subdir, ".dll"))
                {
                    foreach (var dllFilePath in Directory.GetFiles(subdir, "*.dll", SearchOption.AllDirectories))
                    {
                        FileVersionInfo dllInfo = FileVersionInfo.GetVersionInfo(dllFilePath);

                        author = dllInfo.LegalCopyright;
                        //"Copyright © AuthorName 2019"
                        if (!string.IsNullOrEmpty(author))
                        {
                            if (author.Length >= 4)//удаление года, строка должна быть не менее 4 символов для этого
                            {
                                author = author.Remove(author.Length - 4, 4).Replace("Copyright © ", string.Empty).Trim();
                            }
                        }
                        else
                        {
                            author = string.Empty;
                        }
                    }
                }

                if (author.Length == 0 && ManageFilesFolders.IsAnyFileExistsInTheDir(subdir, ".txt", false))
                {
                    foreach (var txtFilePath in Directory.GetFiles(subdir, "*.txt"))
                    {
                        using (StreamReader sr = new StreamReader(txtFilePath))
                        {
                            string line = sr.ReadLine();

                            if (line.ToUpperInvariant().StartsWith("BY ", StringComparison.InvariantCultureIgnoreCase) || line.ToUpperInvariant().StartsWith("AUTHOR: ", StringComparison.InvariantCultureIgnoreCase) || line.ToUpperInvariant().StartsWith("AUTHOR ", StringComparison.InvariantCultureIgnoreCase))
                            {
                                author = line.Split(' ')[1];
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(name) && author.Length == 0)
            {
                if (name.StartsWith("[", StringComparison.InvariantCultureIgnoreCase))
                {
                    string[] s = name.Split(']');

                    if (!name.StartsWith("[AI]", StringComparison.InvariantCultureIgnoreCase))
                    {
                        author = s[0].Remove(0, 1);
                    }
                    else if (name.StartsWith("[AI][", StringComparison.InvariantCultureIgnoreCase))
                    {
                        author = s[1].Remove(0, 1);
                    }
                }
            }

            return author;
        }

        public static void SortFilesToSubfolders(string dirIn2Mo)
        {
            foreach (var file in Directory.GetFiles(dirIn2Mo, "*.*"))
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string fileExtension = Path.GetExtension(file);
                string parentFolderPath = Path.GetDirectoryName(file);
                string parentFolderName = Path.GetFileName(Path.GetDirectoryName(file));

                if (!ManageStrings.IsStringAContainsStringB(fileName, parentFolderName) && (fileExtension == ".txt" || fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".bmp" || fileExtension == ".rtd" || fileExtension == ".doc" || fileExtension == ".html"))
                {
                    File.Move(file, Path.Combine(parentFolderPath, parentFolderName + " " + fileName + fileExtension));
                }
                else if (fileExtension == ".zipmod")
                {
                    string modsDirPath = Path.Combine(parentFolderPath, "mods");
                    Directory.CreateDirectory(modsDirPath);
                    File.Move(file, Path.Combine(modsDirPath, fileName + fileExtension));
                }
                else if (fileExtension == ".png")
                {
                    if (ManageStrings.IsStringAContainsStringB(fileName, "AISChaF"))
                    {
                        string aisChaFDirPath = Path.Combine(parentFolderPath, "UserData", "chara", "female");
                        Directory.CreateDirectory(aisChaFDirPath);
                        File.Move(file, Path.Combine(aisChaFDirPath, fileName + fileExtension));

                    }
                    else if (ManageStrings.IsStringAContainsStringB(fileName, "AISChaM"))
                    {
                        string aisChaMDirPath = Path.Combine(parentFolderPath, "UserData", "chara", "male");
                        Directory.CreateDirectory(aisChaMDirPath);
                        File.Move(file, Path.Combine(aisChaMDirPath, fileName + fileExtension));
                    }
                    else if (!ManageStrings.IsStringAContainsStringB(fileName, parentFolderName))
                    {
                        File.Move(file, Path.Combine(parentFolderPath, parentFolderName + " " + fileName + fileExtension));
                    }
                }
            }
        }

        public static void InstallBepinExModsToMods()
        {
            foreach (var dllfile in Directory.GetFiles(Properties.Settings.Default.Install2MODirPath, "*.dll"))
            {
                FileVersionInfo dllInfo = FileVersionInfo.GetVersionInfo(dllfile);
                string name = dllInfo.ProductName;
                string description = dllInfo.FileDescription;
                string version = dllInfo.FileVersion;
                //string version = dllInfo.ProductVersion;
                string copyright = dllInfo.LegalCopyright;

                if (name == null || name.Length == 0)
                {
                    name = Path.GetFileNameWithoutExtension(dllfile);
                }

                string author = string.Empty;
                if (copyright.Length >= 4)
                {
                    //"Copyright © AuthorName 2019"
                    author = copyright.Remove(copyright.Length - 4, 4).Replace("Copyright © ", string.Empty).Trim();
                }

                //добавление имени автора в начало имени папки
                if ((!string.IsNullOrEmpty(name) && name.Substring(0, 1) == "[" && !name.StartsWith("[AI]", StringComparison.InvariantCultureIgnoreCase)) || (name.Length >= 5 && name.Substring(0, 5) == "[AI][") || ManageStrings.IsStringAContainsStringB(name, author))
                {
                }
                else if (author.Length > 0)
                {
                    //проверка на любые невалидные для имени папки символы
                    if (ManageFilesFolders.ContainsAnyInvalidCharacters(author))
                    {
                    }
                    else
                    {
                        name = "[" + author + "]" + name;
                    }
                }

                string dllName = Path.GetFileName(dllfile);
                string dllTargetModDirPath = Path.Combine(Properties.Settings.Default.ModsPath, name);
                string dllTargetModPluginsSubdirPath = Path.Combine(dllTargetModDirPath, "BepInEx", "Plugins");
                string dllTargetPath = Path.Combine(dllTargetModPluginsSubdirPath, dllName);
                bool isUpdate = false;
                if (Directory.Exists(dllTargetModDirPath))
                {
                    if (File.Exists(dllTargetPath) && File.GetLastWriteTime(dllfile) > File.GetLastWriteTime(dllTargetPath))
                    {
                        //обновление существующей dll на более новую
                        isUpdate = true;
                        File.Delete(dllTargetPath);
                    }
                    else
                    {
                        //Проверки существования целевой папки и модификация имени на более уникальное
                        dllTargetModDirPath = ManageFilesFolders.GetResultTargetDirPathWithNameCheck(Properties.Settings.Default.ModsPath, name);
                    }
                }
                else
                {
                    //найти имя мода из списка модов
                    string modNameWithAuthor = ManageModOrganizer.GetModFromModListContainsTheName(name, false);
                    if (modNameWithAuthor.Length == 0)
                    {
                        //если пусто, поискать также имя по имени дллки
                        modNameWithAuthor = ManageModOrganizer.GetModFromModListContainsTheName(dllName.Remove(dllName.Length - 4, 4), false);
                    }
                    if (modNameWithAuthor.Length > 0)
                    {
                        string newModDirPath = Path.Combine(Properties.Settings.Default.ModsPath, modNameWithAuthor);
                        if (Directory.Exists(newModDirPath))
                        {
                            dllTargetModDirPath = newModDirPath;
                            dllTargetModPluginsSubdirPath = Path.Combine(dllTargetModDirPath, "BepInEx", "Plugins");
                            dllTargetPath = Path.Combine(dllTargetModPluginsSubdirPath, dllName);
                            if (File.Exists(dllTargetPath) && File.GetLastWriteTime(dllfile) > File.GetLastWriteTime(dllTargetPath))
                            {
                                //обновление существующей dll на более новую, найдена папка существующего мода, с измененным именем
                                isUpdate = true;
                                File.Delete(dllTargetPath);
                            }
                        }
                    }
                }

                //перемещение zipmod-а в свою подпапку в Mods
                Directory.CreateDirectory(dllTargetModPluginsSubdirPath);
                File.Move(dllfile, dllTargetPath);

                string readme = Path.Combine(Path.GetDirectoryName(dllfile), Path.GetFileNameWithoutExtension(dllfile) + " Readme.txt");
                if (File.Exists(readme))
                {
                    File.Move(readme, Path.Combine(dllTargetModDirPath, Path.GetFileName(readme)));
                }


                //запись meta.ini
                ManageModOrganizer.WriteMetaIni(
                    dllTargetModDirPath
                    ,
                    isUpdate ? string.Empty : ManageModOrganizer.GetCategoryIndexForTheName("Plugins") + ","
                    ,
                    version
                    ,
                    isUpdate ? string.Empty : "Requires: BepinEx"
                    ,
                    isUpdate ? string.Empty : "<br>Author: " + author + "<br><br>" + description + "<br><br>" + copyright
                    );
                //Utils.IniFile INI = new Utils.IniFile(Path.Combine(dllmoddirpath, "meta.ini"));
                //INI.WriteINI("General", "category", "\"51,\"");
                //INI.WriteINI("General", "version", version);
                //INI.WriteINI("General", "gameName", "Skyrim");
                //INI.WriteINI("General", "comments", "Requires: BepinEx");
                //INI.WriteINI("General", "notes", "\"<br>Author: " + author + "<br><br>" + description + "<br><br>" + copyright + " \"");
                //INI.WriteINI("General", "validated", "true");

                ManageModOrganizer.ActivateDeactivateInsertMod(Path.GetFileName(dllTargetModDirPath));
            }
        }

        public static void InstallZipModsToMods()
        {
            string tempDir = Path.Combine(Properties.Settings.Default.Install2MODirPath, "Temp");
            foreach (var zipfile in Directory.GetFiles(Properties.Settings.Default.Install2MODirPath, "*.zipmod"))
            {
                if (!File.Exists(zipfile))
                {
                    continue;
                }

                string guid = string.Empty;
                string name = string.Empty;
                string version = string.Empty;
                string author = string.Empty;
                string description = string.Empty;
                string website = string.Empty;
                string game = string.Empty;

                bool isManifestFound = false;
                using (ZipArchive archive = ZipFile.OpenRead(zipfile))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith("manifest.xml", StringComparison.OrdinalIgnoreCase))
                        {
                            if (Directory.Exists(tempDir))
                            {
                            }
                            else
                            {
                                Directory.CreateDirectory(tempDir);
                            }

                            string xmlpath = Path.Combine(Properties.Settings.Default.Install2MODirPath, "Temp", entry.FullName);
                            entry.ExtractToFile(xmlpath);

                            isManifestFound = true;

                            guid = ManageXml.ReadXmlValue(xmlpath, "manifest/name", string.Empty);
                            name = ManageXml.ReadXmlValue(xmlpath, "manifest/name", string.Empty);
                            version = ManageXml.ReadXmlValue(xmlpath, "manifest/version", "0");
                            author = ManageXml.ReadXmlValue(xmlpath, "manifest/author", "Unknown author");
                            description = ManageXml.ReadXmlValue(xmlpath, "manifest/description", "Unknown description");
                            website = ManageXml.ReadXmlValue(xmlpath, "manifest/website", string.Empty);
                            game = ManageXml.ReadXmlValue(xmlpath, "manifest/game", "AI Girl"); //установил умолчание как "AI Girl"
                            File.Delete(xmlpath);
                            break;
                        }
                    }
                }

                string zipArchiveName = Path.GetFileNameWithoutExtension(zipfile);
                bool gameEmpty = game.Length == 0;
                if (isManifestFound && (gameEmpty || game == "AI Girl"))
                {
                    if (name.Length == 0)
                    {
                        name = Path.GetFileNameWithoutExtension(zipfile);
                    }

                    //добавление имени автора в начало имени папки
                    name = ManageStrings.AddStringBToAIfValid(name, author);
                    //if (name.StartsWith("[AI][") || (name.StartsWith("[") && !name.StartsWith("[AI]")) || ManageStrings.IsStringAContainsStringB(name, author))
                    //{
                    //}
                    //else if (author.Length > 0)
                    //{
                    //    //проверка на любые невалидные для имени папки символы
                    //    if (ManageFilesFolders.ContainsAnyInvalidCharacters(author))
                    //    {
                    //    }
                    //    else
                    //    {
                    //        name = "[" + author + "]" + name;
                    //    }
                    //}

                    //устанавливать имя папки из имени архива, если оно длиннее
                    if (zipArchiveName.Length > name.Length)
                    {
                        name = zipArchiveName;
                        //добавление автора к имени папки, если не пустое и валидное
                        name = ManageStrings.AddStringBToAIfValid(name, author);
                    }
                    //string zipmoddirpath = GetResultTargetDirPathWithNameCheck(Properties.Settings.Default.ModsPath, name);
                    string zipmoddirpath = Path.Combine(Properties.Settings.Default.ModsPath, name);
                    string zipmoddirmodspath = Path.Combine(zipmoddirpath, "mods");
                    string targetZipFile = Path.Combine(zipmoddirmodspath, zipArchiveName + ".zipmod");
                    bool update = false;
                    string anyfname = string.Empty;
                    if (Directory.Exists(zipmoddirpath))
                    {
                        if (File.Exists(targetZipFile) || (anyfname = ManageFilesFolders.IsAnyFileWithSameExtensionContainsNameOfTheFile(zipmoddirmodspath, zipArchiveName, "*.zip")).Length > 0)
                        {
                            if (File.GetLastWriteTime(zipfile) > File.GetLastWriteTime(targetZipFile))
                            {
                                update = true;
                                File.Delete(anyfname.Length > 0 ? anyfname : targetZipFile);
                            }
                        }
                    }
                    else
                    {
                        zipmoddirpath = ManageFilesFolders.GetResultTargetDirPathWithNameCheck(Properties.Settings.Default.ModsPath, name);
                        zipmoddirmodspath = Path.Combine(zipmoddirpath, "mods");
                        targetZipFile = Path.Combine(zipmoddirmodspath, zipArchiveName + ".zipmod");
                    }

                    //перемещение zipmod-а в свою подпапку в Mods
                    Directory.CreateDirectory(zipmoddirmodspath);
                    File.Move(zipfile, targetZipFile);

                    //Перемещение файлов мода, начинающихся с того же имени в папку этого мода
                    string[] possibleFilesOfTheMod = Directory.GetFiles(Properties.Settings.Default.Install2MODirPath, "*.*").Where(file => Path.GetFileName(file).Trim().StartsWith(zipArchiveName, StringComparison.InvariantCultureIgnoreCase) && ManageStrings.IsStringAContainsAnyStringFromStringArray(Path.GetExtension(file), new string[7] { ".txt", ".png", ".jpg", ".jpeg", ".bmp", ".doc", ".rtf" })).ToArray();
                    int possibleFilesOfTheModLength = possibleFilesOfTheMod.Length;
                    if (possibleFilesOfTheModLength > 0)
                    {
                        for (int n = 0; n < possibleFilesOfTheModLength; n++)
                        {
                            if (File.Exists(possibleFilesOfTheMod[n]))
                            {
                                File.Move(possibleFilesOfTheMod[n], Path.Combine(zipmoddirpath, Path.GetFileName(possibleFilesOfTheMod[n]).Replace(zipArchiveName, Path.GetFileNameWithoutExtension(zipmoddirpath))));
                            }
                        }
                    }

                    //запись meta.ini
                    ManageModOrganizer.WriteMetaIni(
                        zipmoddirpath
                        ,
                        string.Empty
                        ,
                        version
                        ,
                        update ? string.Empty : "Requires: Sideloader plugin"
                        ,
                        update ? string.Empty : "<br>Author: " + author + "<br><br>" + description + "<br><br>" + website + (gameEmpty ? "<br>WARNING: Game field for the Sideloader plugin was empty. Check the plugin manually if need." : string.Empty)
                        );
                    //Utils.IniFile INI = new Utils.IniFile(Path.Combine(zipmoddirpath, "meta.ini"));
                    //INI.WriteINI("General", "category", string.Empty);
                    //INI.WriteINI("General", "version", version);
                    //INI.WriteINI("General", "gameName", "Skyrim");
                    //INI.WriteINI("General", "comments", "Requires: Sideloader plugin");
                    //INI.WriteINI("General", "notes", "\"<br>Author: " + author + "<br><br>" + description + "<br><br>" + website + " \"");
                    //INI.WriteINI("General", "validated", "true");

                    ManageModOrganizer.ActivateDeactivateInsertMod(Path.GetFileName(zipmoddirpath), true);
                }
            }
        }

        /// <summary>
        /// return mod path for input path
        /// </summary>
        /// <param name="inputPath"></param>
        /// <returns></returns>
        internal static string GetMoModPathInMods(string inputPath, string defaultPath = null)
        {
            //search Mods Path by step up to parent dir while it will be Mods path
            string modPath;
            var folderPath = modPath = inputPath;
            while (!string.Equals(folderPath/*.TrimEnd(new char[] { '/', '\\' })*/ , ManageSettings.GetCurrentGameModsPath()/*.TrimEnd(new char[] { '/', '\\' })*/, StringComparison.InvariantCultureIgnoreCase))
            {
                modPath = folderPath;
                folderPath = Path.GetDirectoryName(folderPath);
                if (string.Equals(folderPath, ManageSettings.GetCurrentGamePath(), StringComparison.InvariantCultureIgnoreCase))
                {
                    ManageLogs.Log("Warning. Path in Mods not found." + "\r\ninputPath=" + inputPath + "\r\nModPath=" + modPath + "\r\nFolderPath=" + folderPath);
                    return defaultPath;
                }
            }

            if (modPath != null && string.Equals(Path.GetFileName(modPath), "Mods", StringComparison.InvariantCultureIgnoreCase))//temp debug check
            {
                ManageLogs.Log("warning. log path is Mods. ModPath=" + modPath + ". FolderPath=" + folderPath);
            }
            else if (modPath == null)
            {
                ManageLogs.Log("warning. ModPath is null, set to default.(" + modPath + ") FolderPath=" + folderPath);
                modPath = defaultPath;// set to default if null
            }

            return modPath;
        }

        /// <summary>
        /// save guid-zipmodpath in dictionary if targetZipmodPath is exists
        /// </summary>
        /// <param name="targetZipmodPath"></param>
        /// <param name="sourceModZipmodPath"></param>
        /// <param name="zipmodsGuidList"></param>
        internal static void SaveGuidIfZipMod(string targetZipmodPath, string sourceModZipmodPath, Dictionary<string, string> zipmodsGuidList)
        {
            //zipmod GUID save
            string fileInDataFolderExtension;
            if (targetZipmodPath.ToUpperInvariant().Contains("SIDELOADER MODPACK") && ((fileInDataFolderExtension = Path.GetExtension(targetZipmodPath).ToUpperInvariant()) == ".ZIPMOD" || fileInDataFolderExtension == ".ZIP"))
            {
                var guid = ManageArchive.GetZipmodGuid(targetZipmodPath);
                if (guid.Length > 0 && !zipmodsGuidList.ContainsKey(guid))
                {
                    zipmodsGuidList.Add(guid, sourceModZipmodPath);
                }
            }
        }

        /// <summary>
        /// save guid-modpath pair in dictionary
        /// </summary>
        /// <param name="sourceModZipmodPath"></param>
        /// <param name="zipmodsGuidList"></param>
        /// <param name="saveFullPath">if true will be saved full zipmod path</param>
        internal static void SaveGuidIfZipMod(string sourceModZipmodPath, Dictionary<string, string> zipmodsGuidList, bool saveFullPath = true)
        {
            //zipmod GUID save
            string fileInDataFolderExtension;
            if (File.Exists(sourceModZipmodPath) && sourceModZipmodPath.ToUpperInvariant().Contains("SIDELOADER MODPACK") && ((fileInDataFolderExtension = Path.GetExtension(sourceModZipmodPath).ToUpperInvariant()) == ".ZIPMOD" || fileInDataFolderExtension == ".ZIP"))
            {
                var guid = ManageArchive.GetZipmodGuid(sourceModZipmodPath);
                if (guid.Length > 0 && !zipmodsGuidList.ContainsKey(guid))
                {
                    zipmodsGuidList.Add(guid, saveFullPath ? sourceModZipmodPath : ManageModOrganizerMods.GetMoModPathInMods(sourceModZipmodPath));
                }
            }
        }

        /// <summary>
        /// return list of Sideloader Modpack name/Mod path pairs list.
        /// will be added mods which is marked as target with txt file with same name as sideloader modpack dir name.
        /// </summary>
        /// <returns></returns>
        internal static Dictionary<string, string> GetSideloaderModpackTargetDirs()
        {
            Dictionary<string, string> packs = new Dictionary<string, string>();
            HashSet<string> added = new HashSet<string>(10);
            foreach (var dir in Directory.EnumerateDirectories(ManageSettings.GetCurrentGameModsPath()))
            {
                if (!Directory.Exists(Path.Combine(dir, "mods")))
                {
                    continue;
                }

                foreach (var modpackdir in Directory.EnumerateDirectories(Path.Combine(dir, "mods")))
                {
                    var name = Path.GetFileName(modpackdir);
                    //skip if not sideloader modpack
                    if (!name.ToUpperInvariant().Contains("SIDELOADER MODPACK"))
                    {
                        continue;
                    }

                    //skip if txt with same name is missing or modpack already added in list
                    if (!File.Exists(Path.Combine(Path.GetDirectoryName(modpackdir), name + ".txt")) || added.Contains(name))
                    {
                        continue;
                    }

                    name += CheckFemaleMaleUncensor(modpackdir, name);

                    //add to list
                    if (!packs.ContainsKey(name))
                    {
                        added.Add(name);
                        packs.Add(name, Path.GetFileName(dir));
                    }
                }
            }

            return packs;
        }

        /// <summary>
        /// detect female or male uncensors in dir
        /// </summary>
        /// <param name="name"></param>
        /// <returns>F if in dir only female uncensors, M if only male uncensors or empty, else empty</returns>
        private static string CheckFemaleMaleUncensor(string modpackdir, string name = null)
        {
            if (IsUncensorSelector(!string.IsNullOrWhiteSpace(name) ? name : Path.GetFileName(modpackdir)))
            {
                //add female male versions
                var hasFemale = ManageFilesFolders.IsAnyFileExistsInTheDir(modpackdir, "*[Female]*.zipmod", true);
                var hasMale = ManageFilesFolders.IsAnyFileExistsInTheDir(modpackdir, "*[Penis]*.zipmod", true) || ManageFilesFolders.IsAnyFileExistsInTheDir(modpackdir, "*[Balls]*.zipmod", true);
                if (hasFemale && !hasMale)
                {
                    return "F";
                }
                else if (!hasFemale && hasMale)
                {
                    return "M";
                }
            }

            return "";
        }

        internal static bool IsUncensorSelector(string name)
        {
            return name.ToUpperInvariant().StartsWith("SIDELOADER MODPACK - KK_UNCENSORSELECTOR", StringComparison.InvariantCulture);
        }
    }
}
