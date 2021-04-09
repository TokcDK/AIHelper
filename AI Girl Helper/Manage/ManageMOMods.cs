using AI_Helper.Manage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace AIHelper.Manage
{
    class ManageMOMods
    {
        public static void SetMOModsVariables()
        {
            Properties.Settings.Default.BepinExCfgPath = ManageSettings.GetBepInExCfgFilePath();
        }

        public static void CleanBepInExLinksFromData()
        {
            MOUSFSLoadingFix(true);
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

        internal static bool IsFileDirExistsInDataOROverwrite(string filedir, out string source)
        {
            //string filePathInOverwrite =
            //           Path.GetFullPath(
            //               Path.Combine(ManageSettings.GetCurrentGameMOOverwritePath() + Path.DirectorySeparatorChar + filedir)
            //                           );
            if (File.Exists(
                       Path.GetFullPath(
                           Path.Combine(ManageSettings.GetCurrentGameMOOverwritePath() + Path.DirectorySeparatorChar + filedir)
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
            var gameNameByExe = ManageSettings.GetCurrentGameEXEName().Replace("_64", string.Empty).Replace("_32", string.Empty);
            var USERPROFILE = Path.Combine("%USERPROFILE%", "appdata", "locallow", "illusion__" + gameNameByExe.Replace("Trial", string.Empty), gameNameByExe, "output_log.txt");
            var output_log = Environment.ExpandEnvironmentVariables(USERPROFILE);
            if (File.Exists(output_log))
            {
                Process.Start("explorer.exe", output_log);
            }
            else
            {
                if (File.Exists(Path.Combine(ManageSettings.GetCurrentGameDataPath(), ManageSettings.GetCurrentGameEXEName() + "_Data", "output_log.txt")))
                {
                    Process.Start("explorer.exe", Path.Combine(ManageSettings.GetCurrentGameDataPath(), ManageSettings.GetCurrentGameEXEName() + "_Data", "output_log.txt"));
                }
                else if (File.Exists(Path.Combine(ManageSettings.GetCurrentGameDataPath(), gameNameByExe + "_Data", "output_log.txt")))
                {
                    Process.Start("explorer.exe", Path.Combine(ManageSettings.GetCurrentGameDataPath(), gameNameByExe + "_Data", "output_log.txt"));
                }

            }
        }

        public static void MOUSFSLoadingFix(bool RemoveLinks = false)
        {
            if (!Properties.Settings.Default.MOmode)
            {
                return;
            }

            BepInExPreloadersFix(RemoveLinks);

            string[,] ObjectLinkPaths = ManageSettings.GetListOfExistsGames()[Properties.Settings.Default.CurrentGameListIndex].GetObjectsForSymLinksPaths();

            int ObjectLinkPathsLength = ObjectLinkPaths.Length / 2;
            for (int i = 0; i < ObjectLinkPathsLength; i++)
            {
                if (RemoveLinks)
                {
                    ManageSymLinks.DeleteIfSymlink(ObjectLinkPaths[i, 1]);
                }
                else
                {
                    try
                    {
                        if (ManageSymLinks.Symlink
                          (
                           ObjectLinkPaths[i, 0]
                           ,
                           ObjectLinkPaths[i, 1]
                           ,
                           true
                          ))
                        {
                        }
                        else
                        {
                            ManageFilesFolders.MoveContentOfSourceFolderToTargetFolderAndThenCleanSource(ObjectLinkPaths[i, 1], ObjectLinkPaths[i, 0]);

                            ManageSymLinks.Symlink
                                (
                                 ObjectLinkPaths[i, 0]
                                 ,
                                 ObjectLinkPaths[i, 1]
                                 ,
                                 true
                                );
                        }
                    }
                    catch { }
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

        private static void BepInExPreloadersFix(bool Remove = false)
        {
            string[,] BepInExFilesPaths = ManageSettings.GetListOfExistsGames()[Properties.Settings.Default.CurrentGameListIndex].GetObjectsForMove();

            var Done = false;

            int BepInExFilesPathsLength = BepInExFilesPaths.Length / 2;
            for (int i = 0; i < BepInExFilesPathsLength; i++)
            {
                var SourceFilePath = BepInExFilesPaths[i, 0];
                var TargetFilePath = BepInExFilesPaths[i, 1];
                if (Remove)
                {
                    try
                    {
                        if (File.Exists(TargetFilePath) && (File.Exists(SourceFilePath) || ManageSymLinks.IsSymLink(TargetFilePath)))
                        {
                            File.Delete(TargetFilePath);
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

                        SourceFilePath = ManageMO.GetLastMOFileDirPathFromEnabledModsOfActiveMOProfile(SourceFilePath);

                        //skip file if source not exists
                        if (!File.Exists(SourceFilePath))
                        {
                            continue;
                        }

                        //skip file if not in enabled mod
                        if (!File.Exists(SourceFilePath) || !ManageMOMods.IsInEnabledModOrOverwrite(SourceFilePath))//skip if no active mod found
                        {
                            if (File.Exists(TargetFilePath))
                            {
                                File.Delete(TargetFilePath);
                            }

                            continue;
                        }

                        if (File.Exists(TargetFilePath))
                        {
                            if (
                                ManageSymLinks.IsSymLink(TargetFilePath)
                                ||
                                new FileInfo(TargetFilePath).Length != new FileInfo(SourceFilePath).Length
                                ||
                                FileVersionInfo.GetVersionInfo(TargetFilePath).ProductVersion != FileVersionInfo.GetVersionInfo(SourceFilePath).ProductVersion
                                )
                            {
                                File.Delete(TargetFilePath);
                            }
                            else
                            {
                                continue;
                            }
                        }

                        Directory.CreateDirectory(Path.GetDirectoryName(TargetFilePath));

                        File.Copy(SourceFilePath, TargetFilePath);
                        Done = true;
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
        /// <param name="SourceFilePath"></param>
        /// <returns></returns>
        private static bool IsInEnabledModOrOverwrite(string SourceFilePath)
        {
            if (SourceFilePath.Contains(ManageSettings.GetOverwriteFolder()) || SourceFilePath.Contains(ManageSettings.GetCurrentGameMOOverwritePath()))
            {
                return true;
            }

            if (SourceFilePath.Contains(ManageSettings.GetCurrentGameModsPath()))
            {
                //remove Mods path slit and get 1st element as modname
                var noModsPath = SourceFilePath.Replace(ManageSettings.GetCurrentGameModsPath(), string.Empty);
                var SplittedPath = noModsPath.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
                var modname = SplittedPath[0];

                foreach (var name in ManageMO.GetModNamesListFromActiveMOProfile())
                {
                    if (modname == name)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static void InstallCsScriptsForScriptLoader(string WhereFromInstallDir = "")
        {
            WhereFromInstallDir = WhereFromInstallDir.Length > 0 ? WhereFromInstallDir : Properties.Settings.Default.Install2MODirPath;

            string[] csFiles = Directory.GetFiles(WhereFromInstallDir, "*.cs");

            if (csFiles.Length > 0)
            {
                foreach (var csFile in csFiles)
                {
                    string name = Path.GetFileNameWithoutExtension(csFile);
                    string author = string.Empty;
                    string description = string.Empty;
                    string modname = "[script]" + name;
                    string moddir = Path.GetDirectoryName(csFile).Replace(WhereFromInstallDir, Path.Combine(Properties.Settings.Default.ModsPath, modname));

                    using (StreamReader sReader = new StreamReader(csFile))
                    {
                        string Line;
                        bool readDescriptionMode = false;
                        int i = 0;
                        while (!sReader.EndOfStream || (!readDescriptionMode && i == 10))
                        {
                            Line = sReader.ReadLine();

                            if (!readDescriptionMode /*&& Line.Length > 0 уже есть эта проверка в StringEx.IsStringAContainsStringB*/ && ManageStrings.IsStringAContainsStringB(Line, "/*"))
                            {
                                readDescriptionMode = true;
                                Line = Line.Replace("/*", string.Empty);
                                if (Line.Length > 0)
                                {
                                    description += Line + "<br>";
                                }
                            }
                            else
                            {
                                if (ManageStrings.IsStringAContainsStringB(Line, "*/"))
                                {
                                    readDescriptionMode = false;
                                    Line = Line.Replace("*/", string.Empty);
                                }

                                description += Line + "<br>";

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

                    string FileTargetPath = Path.Combine(scriptsdir, name + ".cs");
                    bool IsUpdate = false;
                    //резервная копия, если файл существовал
                    if (File.Exists(FileTargetPath))
                    {
                        IsUpdate = true;
                        if (File.GetLastWriteTime(csFile) > File.GetLastWriteTime(FileTargetPath))
                        {
                            File.Delete(FileTargetPath);
                            File.Move(csFile, FileTargetPath);
                        }
                        else
                        {
                            File.Delete(csFile);
                        }
                    }
                    else
                    {
                        File.Move(csFile, FileTargetPath);
                    }

                    string FileLastModificationTime = File.GetLastWriteTime(csFile).ToString("yyyyMMddHHmm");
                    //запись meta.ini
                    ManageMO.WriteMetaINI(
                        moddir
                        ,
                        IsUpdate ? string.Empty : ManageMO.GetCategoryIndexForTheName("ScriptLoader scripts") + ","
                        ,
                        "0." + FileLastModificationTime
                        ,
                        IsUpdate ? string.Empty : "Requires: " + "ScriptLoader"
                        ,
                        IsUpdate ? string.Empty : "<br>" + "Author" + ": " + author + "<br><br>" + (description.Length > 0 ? description : name)
                        );

                    ManageMO.ActivateDeactivateInsertMod(modname, false, "ScriptLoader scripts_separator");

                    string[] extrafiles = Directory.GetFiles(WhereFromInstallDir, name + "*.*");
                    if (extrafiles.Length > 0)
                    {
                        foreach (var extrafile in extrafiles)
                        {
                            string targetFile = extrafile.Replace(WhereFromInstallDir, moddir);

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

        public static void InstallCardsFrom2MO(string TargetDir = "")
        {
            string ProceedDir = TargetDir.Length == 0 ? Properties.Settings.Default.Install2MODirPath : TargetDir;
            var images = Directory.GetFiles(ProceedDir, "*.png");
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
                        ManageMO.WriteMetaINI(
                            cardsModDir
                            ,
                            ManageMO.GetCategoryIndexForTheName("Characters") + ","
                            ,
                            string.Empty
                            ,
                            string.Empty
                            ,
                            "<br>Author: " + string.Empty + "<br><br>" + Path.GetFileNameWithoutExtension(cardsModDir) + " character cards<br><br>"
                            );

                        ManageMO.ActivateDeactivateInsertMod(Path.GetFileName(cardsModDir), true, "UserCharacters_separator");
                    }
                }
            }
        }

        public static void MoveImagesToTargetContentFolder(string dir, string ContentType, bool MoveInThisFolder = false)
        {
            if (Directory.Exists(dir))
            {
                string TargetFolder = string.Empty;
                string Extension = ".png";
                if (ContentType == "f")
                {
                    //TargetFolder = GetCharactersFolder();
                    TargetFolder = GetUserDataSubFolder(MoveInThisFolder ? dir : " Chars", ContentType);
                }
                else if (ContentType == "m")
                {
                    //TargetFolder = GetCharactersFolder(true);
                    TargetFolder = GetUserDataSubFolder(MoveInThisFolder ? dir : " Chars", ContentType);
                }
                else if (ContentType == "c")
                {
                    //TargetFolder = GetCoordinateFolder();
                    TargetFolder = GetUserDataSubFolder(MoveInThisFolder ? dir : " Coordinate", ContentType);
                }
                else if (ContentType == "h"
                    || ContentType == "h1"
                    || ContentType == "h2"
                    || ContentType == "h3"
                    || ContentType == "h4"
                    )
                {
                    //TargetFolder = GetCoordinateFolder();
                    TargetFolder = GetUserDataSubFolder(MoveInThisFolder ? dir : " Housing", ContentType);
                }
                else if (ContentType == "cf")
                {
                    //TargetFolder = GetCardFrameFolder();
                    TargetFolder = GetUserDataSubFolder(MoveInThisFolder ? dir : " Cardframes", ContentType);
                }
                else if (ContentType == "cb")
                {
                    //TargetFolder = GetCardFrameFolder(false);
                    TargetFolder = GetUserDataSubFolder(MoveInThisFolder ? dir : " Cardframes", ContentType);
                }
                else if (ContentType == "o")
                {
                    //TargetFolder = GetOverlaysFolder();
                    TargetFolder = GetUserDataSubFolder(MoveInThisFolder ? dir : " Overlays", ContentType);
                }
                else if (ContentType == "s")
                {
                    //TargetFolder = GetOverlaysFolder();
                    TargetFolder = GetUserDataSubFolder(MoveInThisFolder ? dir : " Scenes", ContentType);
                }

                //Для всех, сброс png из корневой папки в целевую
                foreach (var target in Directory.GetFiles(dir, "*" + Extension))
                {
                    var CardframeTargetFolder = ManageFilesFolders.GetResultTargetFilePathWithNameCheck(TargetFolder, Path.GetFileNameWithoutExtension(target), Extension);

                    File.Move(target, CardframeTargetFolder);
                }

                if (ContentType == "o")
                {
                    ManageArchive.UnpackArchivesToSubfoldersWithSameName(dir, ".zip");
                    foreach (var oSubDir in Directory.GetDirectories(dir, "*"))
                    {
                        string newTarget = ManageFilesFolders.MoveFolderToOneLevelUpIfItAloneAndReturnMovedFolderPath(oSubDir);
                        string targetDirName = Path.GetFileName(newTarget);
                        var ResultTargetPath = ManageFilesFolders.GetResultTargetDirPathWithNameCheck(TargetFolder, targetDirName);

                        Directory.Move(newTarget, ResultTargetPath);
                    }
                }
                else if (ContentType == "f")
                {
                    string MaleDir = Path.Combine(dir, "m");
                    if (Directory.Exists(MaleDir))
                    {
                        foreach (var target in Directory.GetFiles(MaleDir, "*.png"))
                        {
                            string name = Path.GetFileName(target);
                            File.Move(target, Path.Combine(dir, name));
                        }
                        Directory.Move(MaleDir, MaleDir + "_");
                        MoveImagesToTargetContentFolder(dir, "m", MoveInThisFolder);
                    }
                }
                else if (ContentType == "h"
                    || ContentType == "h1"
                    || ContentType == "h2"
                    || ContentType == "h3"
                    || ContentType == "h4"
                    )
                {
                    if (ContentType.Length == 2)
                    {
                        foreach (var file in Directory.GetFiles(dir))
                        {
                            File.Move(file, ManageFilesFolders.GetResultTargetFilePathWithNameCheck(TargetFolder, Path.GetFileNameWithoutExtension(file), ".png"));
                        }
                    }
                    else
                    {
                        foreach (var typeDir in Directory.GetDirectories(dir))
                        {
                            string hSubDirName = Path.GetFileName(typeDir);
                            foreach (var file in Directory.GetFiles(typeDir))
                            {
                                File.Move(file, ManageFilesFolders.GetResultTargetFilePathWithNameCheck(Path.Combine(TargetFolder, hSubDirName), Path.GetFileNameWithoutExtension(file), ".png"));
                            }
                        }

                    }
                }

                ManageFilesFolders.DeleteEmptySubfolders(dir);
            }
        }

        public static string GetUserDataSubFolder(string FirstCandidateFolder, string Type)
        {
            string[] TargetFolders = new string[3]
            {
                FirstCandidateFolder.Substring(0,1)== " " ? Path.Combine(Properties.Settings.Default.ModsPath, "OrganizedModPack Downloaded"+FirstCandidateFolder) : FirstCandidateFolder,
                Path.Combine(Properties.Settings.Default.ModsPath, "MyUserData"),
                Properties.Settings.Default.OverwriteFolder
            };

            string TypeFolder = string.Empty;
            string TargetFolderName = string.Empty;
            if (Type == "f")
            {
                TypeFolder = "chara";
                TargetFolderName = "female";
            }
            else if (Type == "m")
            {
                TypeFolder = "chara";
                TargetFolderName = "male";
            }
            else if (Type == "c")
            {
                TargetFolderName = "coordinate";
                //TypeFolder = "";
            }
            else if (Type == "h"
                || Type == "h1"
                || Type == "h2"
                || Type == "h3"
                || Type == "h4"
                )
            {
                TypeFolder = "housing";
                TargetFolderName = Type.Length == 2 ? "0" + Type.Remove(0, 1) : string.Empty;
            }
            else if (Type == "cf")
            {
                TypeFolder = "cardframe";
                TargetFolderName = "Front";
            }
            else if (Type == "cb")
            {
                TypeFolder = "cardframe";
                TargetFolderName = "Back";
            }
            else if (Type == "o")
            {
                //TypeFolder = "";
                TargetFolderName = "Overlays";
            }
            else if (Type == "s")
            {
                TypeFolder = "studio";
                TargetFolderName = "scene";
            }

            int TargetFoldersLength = TargetFolders.Length;
            for (int i = 0; i < TargetFoldersLength; i++)
            {
                string Folder = TargetFolders[i];
                if (Directory.Exists(Folder))
                {
                    var TargetResultDirPath = Path.Combine(Folder, "UserData", TypeFolder, TargetFolderName);
                    if (!Directory.Exists(TargetResultDirPath))
                    {
                        Directory.CreateDirectory(TargetResultDirPath);
                    }
                    return TargetResultDirPath;
                }
            }

            return Path.Combine(Properties.Settings.Default.OverwriteFolder, "UserData", TypeFolder, TargetFolderName);
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

                bool FoundZipMod = false;
                bool FoundStandardModInZip = false;
                bool FoundModsDir = false;
                bool FoundcsFiles = false;

                string author = string.Empty;
                string category = string.Empty;
                string version = string.Empty;
                string comment = string.Empty;
                string description = string.Empty;
                string UpdateModNameFromMeta = string.Empty;
                bool FoundUpdateName = false;
                string ModFolderForUpdate = string.Empty;
                string ZipName = Path.GetFileNameWithoutExtension(zipfile);
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
                        if (!FoundZipMod && entryFullNameLength >= 12 && string.Compare(entryFullName.Substring(entryFullNameLength - 12, 12), "manifest.xml", true) == 0) //entryFullName=="manifest.xml"
                        {
                            FoundZipMod = true;
                            break;
                        }

                        if (!FoundStandardModInZip)
                        {
                            if (
                                   (entryFullNameLength >= 7 && (string.Compare(entryFullName.Substring(entryFullNameLength - 7, 6), "abdata", true) == 0 || string.Compare(entryFullName.Substring(0, 6), "abdata", true) == 0)) //entryFullName=="abdata/"
                                || (entryFullNameLength >= 6 && (string.Compare(entryFullName.Substring(entryFullNameLength - 6, 5), "_data", true) == 0/*тут только проверка на окончание нужна || string.Compare(entryFullName.Substring(0, 5), "_data", true) == 0*/)) //entryFullName=="_data/"
                                || (entryFullNameLength >= 8 && (string.Compare(entryFullName.Substring(entryFullNameLength - 8, 7), "bepinex", true) == 0 || string.Compare(entryFullName.Substring(0, 7), "bepinex", true) == 0)) //entryFullName=="bepinex/"
                                || (entryFullNameLength >= 9 && (string.Compare(entryFullName.Substring(entryFullNameLength - 9, 8), "userdata", true) == 0 || string.Compare(entryFullName.Substring(0, 8), "userdata", true) == 0)) //entryFullName=="userdata/"
                               )
                            {
                                FoundStandardModInZip = true;
                            }
                        }

                        //когда найдена папка mods, если найден zipmod
                        if (FoundModsDir && !FoundStandardModInZip)
                        {
                            if (entryFullNameLength >= 7 && string.Compare(entryFullName.Substring(entryFullNameLength - 7, 7), ".zipmod", true) == 0)//entryFullName==".zipmod"
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
                            else if (entryFullNameLength >= 4 && string.Compare(entryFullName.Substring(entryFullNameLength - 4, 4), ".zip", true) == 0)
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
                        if (!FoundModsDir && entryFullNameLength >= 5 && (string.Compare(entryFullName.Substring(entryFullNameLength - 5, 4), "mods", true) == 0 || string.Compare(entryFullName.Substring(0, 4), "mods", true) == 0))
                        {
                            FoundModsDir = true;
                        }

                        //если найден cs
                        if (!FoundcsFiles && entryFullNameLength >= 3 && string.Compare(entryFullName.Substring(entryFullNameLength - 3, 3), ".cs", true) == 0)
                        {
                            FoundStandardModInZip = false;
                            FoundcsFiles = true;
                            break;
                        }

                        //получение информации о моде из dll
                        if (entryFullNameLength >= 4 && string.Compare(entryFullName.Substring(entryFullNameLength - 4, 4), ".dll", true) == 0)
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

                                    string[] ModsList = ManageMO.GetModNamesListFromActiveMOProfile(false);

                                    foreach (var ModFolder in ModsList)
                                    {
                                        ModFolderForUpdate = Path.Combine(Properties.Settings.Default.ModsPath, ModFolder);
                                        string targetfile = Path.Combine(ModFolderForUpdate, entryFullName);
                                        targetFileAny = GetTheDllFromSubfolders(ModFolderForUpdate, entryName.Remove(entryName.Length - 4, 4), "dll");
                                        if (File.Exists(targetfile) || (targetFileAny.Length > 0 && File.Exists(targetFileAny)))
                                        {
                                            if (targetFileAny.Length > 0)
                                            {
                                                targetfile = targetFileAny;
                                            }

                                            UpdateModNameFromMeta = ManageINI.GetINIValueIfExist(Path.Combine(ModFolderForUpdate, "meta.ini"), "notes", "General");
                                            if (UpdateModNameFromMeta.Length > 0)
                                            {
                                                int upIndex = UpdateModNameFromMeta.IndexOf("ompupname:");
                                                if (upIndex > -1)
                                                {
                                                    //get update name
                                                    UpdateModNameFromMeta = UpdateModNameFromMeta.Substring(upIndex).Split(':')[1];
                                                    if (UpdateModNameFromMeta.Length > 0 && ZipName.Length >= UpdateModNameFromMeta.Length && ManageStrings.IsStringAContainsStringB(ZipName, UpdateModNameFromMeta))
                                                    {
                                                        FoundUpdateName = true;
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        UpdateModNameFromMeta = string.Empty;
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
                else if (FoundZipMod)
                {
                    //если файл имеет расширение zip. Надо, т.к. здесь может быть файл zipmod
                    if (zipfile.Length >= 4 && string.Compare(zipfile.Substring(zipfile.Length - 4, 4), ".zip", true) == 0)
                    {
                        File.Move(zipfile, zipfile + "mod");
                    }
                    InstallZipModsToMods();//будет после установлено соответствующей функцией
                }
                else if (FoundStandardModInZip)
                {
                    string TargetModDirPath;
                    if (FoundUpdateName)
                    {
                        TargetModDirPath = Path.Combine(Properties.Settings.Default.Install2MODirPath, ZipName + "_temp");
                    }
                    else
                    {
                        TargetModDirPath = Path.Combine(Properties.Settings.Default.ModsPath, ZipName);
                    }

                    Compressor.Decompress(zipfile, TargetModDirPath);

                    if (FoundUpdateName)
                    {
                        string[] modfiles = Directory.GetFiles(TargetModDirPath, "*.*", SearchOption.AllDirectories);
                        foreach (var file in modfiles)
                        {
                            //ModFolderForUpdate
                            string TargetFIle = file.Replace(TargetModDirPath, ModFolderForUpdate);
                            string TargetFileDir = Path.GetDirectoryName(TargetFIle);
                            bool targetfileIsNewerOrSame = false;
                            if (File.Exists(TargetFIle))
                            {
                                if (File.GetLastWriteTime(file) > File.GetLastWriteTime(TargetFIle))
                                {
                                    File.Delete(TargetFIle);
                                }
                                else
                                {
                                    targetfileIsNewerOrSame = true;
                                }
                            }
                            else
                            {
                                if (
                                TargetFIle.Length >= 4 && TargetFIle.Substring(TargetFIle.Length - 4, 4) == ".dll"
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
                            if (!Directory.Exists(TargetFileDir))
                            {
                                Directory.CreateDirectory(TargetFileDir);
                            }
                            if (targetfileIsNewerOrSame)
                            {
                                File.Delete(file);
                            }
                            else
                            {
                                File.Move(file, TargetFIle);
                            }
                        }
                        Directory.Delete(TargetModDirPath, true);
                        //присваивание папки на целевую, после переноса, для дальнейшей работы с ней
                        TargetModDirPath = ModFolderForUpdate;
                    }

                    File.Move(zipfile, zipfile + (FoundUpdateName ? ".InstalledUpdatedMod" : ".InstalledExtractedToMods"));

                    if (version.Length == 0)
                    {
                        version = Regex.Match(ZipName, @"\d+(\.\d+)*").Value;
                    }
                    if (!FoundUpdateName && author.Length == 0)
                    {
                        author = ZipName.StartsWith("[AI][") || (ZipName.StartsWith("[") && !ZipName.StartsWith("[AI]")) ? ZipName.Substring(ZipName.IndexOf("[") + 1, ZipName.IndexOf("]") - 1) : string.Empty;
                    }

                    //запись meta.ini
                    ManageMO.WriteMetaINI(
                        TargetModDirPath
                        ,
                        FoundUpdateName ? string.Empty : category
                        ,
                        version
                        ,
                        FoundUpdateName ? string.Empty : comment
                        ,
                        FoundUpdateName ? string.Empty : "<br>Author: " + author + "<br><br>" + (description.Length > 0 ? description : Path.GetFileNameWithoutExtension(zipfile)) + "<br><br>"
                        );

                    ManageMO.ActivateDeactivateInsertMod(Path.GetFileName(TargetModDirPath));
                }
                else if (FoundModsDir)
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
                else if (FoundcsFiles)
                {
                    //extract to handle as subdir
                    string extractpath = Path.Combine(Properties.Settings.Default.Install2MODirPath, Path.GetFileNameWithoutExtension(zipfile));
                    Compressor.Decompress(zipfile, extractpath);
                    File.Move(zipfile, zipfile + ".InstalledExtractedAsSubfolder");
                }
            }
        }

        public static string GetTheDllFromSubfolders(string Dir, string FileName, string Extension)
        {
            if (Directory.Exists(Dir))
            {
                foreach (var file in Directory.GetFiles(Dir, "*." + Extension, SearchOption.AllDirectories))
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    if (string.Compare(name, FileName, true) == 0)
                    {
                        return file;
                    }
                }
            }
            return string.Empty;
        }

        public static void InstallModFilesFromSubfolders()
        {
            foreach (var dirIn2mo in Directory.GetDirectories(Properties.Settings.Default.Install2MODirPath, "*"))
            {
                string name = Path.GetFileName(dirIn2mo);
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
                SortFilesToSubfolders(dirIn2mo);

                string dir = ManageFilesFolders.MoveFolderToOneLevelUpIfItAloneAndReturnMovedFolderPath(dirIn2mo);
                name = Path.GetFileName(dir);
                string category = string.Empty;
                string version = string.Empty;
                string author = GetAuthorName(dir, name);//получение имени автора из имени файла или других файлов
                string comment = string.Empty;
                string description = string.Empty;
                string moddir = string.Empty;

                bool AnyModFound = false;

                string[] subDirs = Directory.GetDirectories(dir, "*");

                //when was extracted archive where is one folder with same name and this folder contains game files
                //upd. уже сделано выше через ManageFilesFolders.MoveFolderToOneLevelUpIfItAloneAndReturnMovedFolderPath(dirIn2mo)
                //if (subDirs.Length == 1 && Path.GetFileName(subDirs[0]) == name)
                //{
                //    //re-set dir to this one subdir and get dirs from there
                //    dir = subDirs[0];
                //    subDirs = Directory.GetDirectories(dir, "*");
                //}

                int subDirsLength = subDirs.Length;
                for (int i = 0; i < subDirsLength; i++)
                {
                    string subdir = subDirs[i];
                    string subdirname = Path.GetFileName(subdir);

                    if (
                           string.Compare(subdirname, "abdata", true) == 0
                        || string.Compare(subdirname, "userdata", true) == 0
                        || string.Compare(subdirname, "ai-syoujyotrial_data", true) == 0
                        || string.Compare(subdirname, "ai-syoujyo_data", true) == 0
                        || string.Compare(subdirname, "StudioNEOV2_Data", true) == 0
                        || string.Compare(subdirname, "manual_s", true) == 0
                        || string.Compare(subdirname, "manual", true) == 0
                        || string.Compare(subdirname, "MonoBleedingEdge", true) == 0
                        || string.Compare(subdirname, "DefaultData", true) == 0
                        || string.Compare(subdirname, "bepinex", true) == 0
                        || string.Compare(subdirname, "scripts", true) == 0
                        || string.Compare(subdirname, "mods", true) == 0
                        )
                    {
                        //CopyFolder.Copy(dir, Path.Combine(Properties.Settings.Default.ModsPath, dir));
                        //Directory.Move(dir, "[installed]" + dir);

                        //имя папки без GetResultTargetDirPathWithNameCheck для того, чтобы обновить существующую, если такая найдется
                        var TargetModDIr = Path.Combine(
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


                        if (Directory.Exists(TargetModDIr))
                        {
                            foreach (var file in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories))
                            {
                                string fileTarget = file.Replace(dir, TargetModDIr);
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
                            Directory.Move(dir, TargetModDIr);
                        }

                        moddir = TargetModDIr;
                        AnyModFound = true;
                        version = Regex.Match(name, @"\d+(\.\d+)*").Value;
                        description = name;
                        break;
                    }
                }

                if (!AnyModFound)
                {
                    moddir = dir.Replace(Properties.Settings.Default.Install2MODirPath, Properties.Settings.Default.ModsPath);
                    string targetfilepath = "readme.txt";
                    foreach (var file in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories))
                    {
                        if (Path.GetExtension(file) == ".zipmod")
                        {
                            string newpath = Path.Combine(ManageSettings.GetInstall2MODirPath(), Path.GetFileName(file));
                            File.Move(file, newpath);
                            InstallZipModsToMods();
                        }
                        else if (Path.GetExtension(file) == ".dll")
                        {
                            string newpath = Path.Combine(ManageSettings.GetInstall2MODirPath(), Path.GetFileName(file));
                            File.Move(file, newpath);
                            InstallBepinExModsToMods();
                        }
                        else if (string.Compare(Path.GetExtension(file), ".unity3d", true) == 0)//if extension == .unity3d
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
                            AnyModFound = true;
                        }
                        else if (string.Compare(Path.GetExtension(file), ".cs", true) == 0)//if extension == .cs
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
                            string categoryIndex = ManageMO.GetCategoryIndexForTheName("ScriptLoader scripts");
                            if (categoryIndex.Length > 0 && (category.Length == 0 || !ManageStrings.IsStringAContainsStringB(category, categoryIndex)))
                            {
                                if (category.Length == 0 || category == "-1,")
                                {
                                    category = categoryIndex + ",";
                                }
                                else
                                {
                                    category += category.EndsWith(",") ? categoryIndex : "," + categoryIndex;
                                }
                            }

                            AnyModFound = true;
                        }
                    }

                    if (AnyModFound)
                    {
                        string[] txts = Directory.GetFiles(dir, "*.txt");
                        string infofile = string.Empty;
                        if (txts.Length > 0)
                        {
                            foreach (string txt in txts)
                            {
                                string txtFileName = Path.GetFileName(txt);

                                if (
                                        string.Compare(txt, "readme.txt", true) == 0
                                    || string.Compare(txt, "description.txt", true) == 0
                                    || string.Compare(txt, Path.GetFileName(dir) + ".txt", true) == 0
                                    || string.Compare(txt, Path.GetFileNameWithoutExtension(targetfilepath) + ".txt", true) == 0
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
                                else if (filecontent[l].StartsWith("name:"))
                                {
                                    string s = filecontent[l].Replace("name:", string.Empty);
                                    if (s.Length > 1)
                                    {
                                        name = s;
                                    }
                                }
                                else if (filecontent[l].StartsWith("author:"))
                                {
                                    string s = filecontent[l].Replace("author:", string.Empty);
                                    if (s.Length > 1)
                                    {
                                        author = s;
                                    }
                                }
                                else if (filecontent[l].StartsWith("version:"))
                                {
                                    string s = filecontent[l].Replace("version:", string.Empty);
                                    if (s.Length > 1)
                                    {
                                        version = s;
                                    }
                                }
                                else if (filecontent[l].StartsWith("description:"))
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

                if (AnyModFound)
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
                    category = ManageMO.GetCategoriesForTheFolder(moddir, category);

                    //запись meta.ini
                    ManageMO.WriteMetaINI(
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

                    ManageMO.ActivateDeactivateInsertMod(Path.GetFileName(moddir));
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
                                    string TempDir = Path.Combine(modsDir, "temp");
                                    if (Directory.Exists(TempDir))
                                    {
                                    }
                                    else
                                    {
                                        Directory.CreateDirectory(TempDir);
                                    }

                                    string xmlpath = Path.Combine(TempDir, entry.FullName);
                                    entry.ExtractToFile(xmlpath);

                                    author = ManageXML.ReadXmlValue(xmlpath, "manifest/author", "author");

                                    File.Delete(xmlpath);
                                    ManageFilesFolders.DeleteEmptySubfolders(TempDir);
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

                if (author.Length == 0 && ManageFilesFolders.IsAnyFileExistsInTheDir(subdir, ".dll"))
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

                            if (line.ToUpper().StartsWith("BY ") || line.ToUpper().StartsWith("AUTHOR: ") || line.ToUpper().StartsWith("AUTHOR "))
                            {
                                author = line.Split(' ')[1];
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(name) && author.Length == 0)
            {
                if (name.StartsWith("["))
                {
                    string[] s = name.Split(']');

                    if (!name.StartsWith("[AI]"))
                    {
                        author = s[0].Remove(0, 1);
                    }
                    else if (name.StartsWith("[AI]["))
                    {
                        author = s[1].Remove(0, 1);
                    }
                }
            }

            return author;
        }

        public static void SortFilesToSubfolders(string dirIn2mo)
        {
            foreach (var file in Directory.GetFiles(dirIn2mo, "*.*"))
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
                        string AISChaFDirPath = Path.Combine(parentFolderPath, "UserData", "chara", "female");
                        Directory.CreateDirectory(AISChaFDirPath);
                        File.Move(file, Path.Combine(AISChaFDirPath, fileName + fileExtension));

                    }
                    else if (ManageStrings.IsStringAContainsStringB(fileName, "AISChaM"))
                    {
                        string AISChaMDirPath = Path.Combine(parentFolderPath, "UserData", "chara", "male");
                        Directory.CreateDirectory(AISChaMDirPath);
                        File.Move(file, Path.Combine(AISChaMDirPath, fileName + fileExtension));
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

                if (name.Length == 0)
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
                if ((!string.IsNullOrEmpty(name) && name.Substring(0, 1) == "[" && !name.StartsWith("[AI]")) || (name.Length >= 5 && name.Substring(0, 5) == "[AI][") || ManageStrings.IsStringAContainsStringB(name, author))
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
                bool IsUpdate = false;
                if (Directory.Exists(dllTargetModDirPath))
                {
                    if (File.Exists(dllTargetPath) && File.GetLastWriteTime(dllfile) > File.GetLastWriteTime(dllTargetPath))
                    {
                        //обновление существующей dll на более новую
                        IsUpdate = true;
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
                    string modNameWithAuthor = ManageMO.GetModFromModListContainsTheName(name, false);
                    if (modNameWithAuthor.Length == 0)
                    {
                        //если пусто, поискать также имя по имени дллки
                        modNameWithAuthor = ManageMO.GetModFromModListContainsTheName(dllName.Remove(dllName.Length - 4, 4), false);
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
                                IsUpdate = true;
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
                ManageMO.WriteMetaINI(
                    dllTargetModDirPath
                    ,
                    IsUpdate ? string.Empty : ManageMO.GetCategoryIndexForTheName("Plugins") + ","
                    ,
                    version
                    ,
                    IsUpdate ? string.Empty : "Requires: BepinEx"
                    ,
                    IsUpdate ? string.Empty : "<br>Author: " + author + "<br><br>" + description + "<br><br>" + copyright
                    );
                //Utils.IniFile INI = new Utils.IniFile(Path.Combine(dllmoddirpath, "meta.ini"));
                //INI.WriteINI("General", "category", "\"51,\"");
                //INI.WriteINI("General", "version", version);
                //INI.WriteINI("General", "gameName", "Skyrim");
                //INI.WriteINI("General", "comments", "Requires: BepinEx");
                //INI.WriteINI("General", "notes", "\"<br>Author: " + author + "<br><br>" + description + "<br><br>" + copyright + " \"");
                //INI.WriteINI("General", "validated", "true");

                ManageMO.ActivateDeactivateInsertMod(Path.GetFileName(dllTargetModDirPath));
            }
        }

        public static void InstallZipModsToMods()
        {
            string TempDir = Path.Combine(Properties.Settings.Default.Install2MODirPath, "Temp");
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

                bool IsManifestFound = false;
                using (ZipArchive archive = ZipFile.OpenRead(zipfile))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith("manifest.xml", StringComparison.OrdinalIgnoreCase))
                        {
                            if (Directory.Exists(TempDir))
                            {
                            }
                            else
                            {
                                Directory.CreateDirectory(TempDir);
                            }

                            string xmlpath = Path.Combine(Properties.Settings.Default.Install2MODirPath, "Temp", entry.FullName);
                            entry.ExtractToFile(xmlpath);

                            IsManifestFound = true;

                            guid = ManageXML.ReadXmlValue(xmlpath, "manifest/name", string.Empty);
                            name = ManageXML.ReadXmlValue(xmlpath, "manifest/name", string.Empty);
                            version = ManageXML.ReadXmlValue(xmlpath, "manifest/version", "0");
                            author = ManageXML.ReadXmlValue(xmlpath, "manifest/author", "Unknown author");
                            description = ManageXML.ReadXmlValue(xmlpath, "manifest/description", "Unknown description");
                            website = ManageXML.ReadXmlValue(xmlpath, "manifest/website", string.Empty);
                            game = ManageXML.ReadXmlValue(xmlpath, "manifest/game", "AI Girl"); //установил умолчание как "AI Girl"
                            File.Delete(xmlpath);
                            break;
                        }
                    }
                }

                string zipArchiveName = Path.GetFileNameWithoutExtension(zipfile);
                bool gameEmpty = game.Length == 0;
                if (IsManifestFound && (gameEmpty || game == "AI Girl"))
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
                    string[] PossibleFilesOfTheMod = Directory.GetFiles(Properties.Settings.Default.Install2MODirPath, "*.*").Where(file => Path.GetFileName(file).Trim().StartsWith(zipArchiveName) && ManageStrings.IsStringAContainsAnyStringFromStringArray(Path.GetExtension(file), new string[7] { ".txt", ".png", ".jpg", ".jpeg", ".bmp", ".doc", ".rtf" })).ToArray();
                    int PossibleFilesOfTheModLength = PossibleFilesOfTheMod.Length;
                    if (PossibleFilesOfTheModLength > 0)
                    {
                        for (int n = 0; n < PossibleFilesOfTheModLength; n++)
                        {
                            if (File.Exists(PossibleFilesOfTheMod[n]))
                            {
                                File.Move(PossibleFilesOfTheMod[n], Path.Combine(zipmoddirpath, Path.GetFileName(PossibleFilesOfTheMod[n]).Replace(zipArchiveName, Path.GetFileNameWithoutExtension(zipmoddirpath))));
                            }
                        }
                    }

                    //запись meta.ini
                    ManageMO.WriteMetaINI(
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

                    ManageMO.ActivateDeactivateInsertMod(Path.GetFileName(zipmoddirpath), true);
                }
            }
        }

        /// <summary>
        /// return mod path for input path
        /// </summary>
        /// <param name="inputPath"></param>
        /// <returns></returns>
        internal static string GetMOModPathInMods(string inputPath)
        {
            //search ModPath
            var ModPath = inputPath;
            var FolderPath = ModPath;
            while (FolderPath.ToUpperInvariant()/*.TrimEnd(new char[] { '/', '\\' })*/ != ManageSettings.GetCurrentGameModsPath().ToUpperInvariant()/*.TrimEnd(new char[] { '/', '\\' })*/)
            {
                ModPath = FolderPath;
                FolderPath = Path.GetDirectoryName(FolderPath);
            }

            if (ModPath != null && Path.GetFileName(ModPath).ToUpperInvariant() == "MODS")//temp debug check
            {
                ManageLogs.Log("warning. log path is Mods. ModPath=" + ModPath + ". FolderPath=" + FolderPath);
            }
            else if (ModPath == null)
            {
                ManageLogs.Log("warning. ModPath is null.(" + ModPath + ") FolderPath=" + FolderPath);
            }

            return ModPath;
        }

        /// <summary>
        /// save guid-zipmodpath in dictionary if targetZipmodPath is exists
        /// </summary>
        /// <param name="targetZipmodPath"></param>
        /// <param name="SourceModZipmodPath"></param>
        /// <param name="ZipmodsGUIDList"></param>
        internal static void SaveGUIDIfZipMod(string targetZipmodPath, string SourceModZipmodPath, System.Collections.Generic.Dictionary<string, string> ZipmodsGUIDList)
        {
            //zipmod GUID save
            string FileInDataFolderExtension;
            if (targetZipmodPath.ToUpperInvariant().Contains("SIDELOADER MODPACK") && ((FileInDataFolderExtension = Path.GetExtension(targetZipmodPath).ToUpperInvariant()) == ".ZIPMOD" || FileInDataFolderExtension == ".ZIP"))
            {
                var guid = ManageArchive.GetZipmodGUID(targetZipmodPath);
                if (guid.Length > 0 && !ZipmodsGUIDList.ContainsKey(guid))
                {
                    ZipmodsGUIDList.Add(guid, SourceModZipmodPath);
                }
            }
        }

        /// <summary>
        /// save guid-modpath pair in dictionary
        /// </summary>
        /// <param name="SourceModZipmodPath"></param>
        /// <param name="ZipmodsGUIDList"></param>
        /// <param name="SaveFullPath">if true will be saved full zipmod path</param>
        internal static void SaveGUIDIfZipMod(string SourceModZipmodPath, System.Collections.Generic.Dictionary<string, string> ZipmodsGUIDList, bool SaveFullPath = true)
        {
            //zipmod GUID save
            string FileInDataFolderExtension;
            if (File.Exists(SourceModZipmodPath) && SourceModZipmodPath.ToUpperInvariant().Contains("SIDELOADER MODPACK") && ((FileInDataFolderExtension = Path.GetExtension(SourceModZipmodPath).ToUpperInvariant()) == ".ZIPMOD" || FileInDataFolderExtension == ".ZIP"))
            {
                var guid = ManageArchive.GetZipmodGUID(SourceModZipmodPath);
                if (guid.Length > 0 && !ZipmodsGUIDList.ContainsKey(guid))
                {
                    ZipmodsGUIDList.Add(guid, SaveFullPath ? SourceModZipmodPath : ManageMOMods.GetMOModPathInMods(SourceModZipmodPath));
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

                    //add to list
                    if(!packs.ContainsKey(name))
                    {
                        added.Add(name);
                        packs.Add(name, Path.GetFileName(dir));
                    }
                }
            }

            return packs;
        }
    }
}
