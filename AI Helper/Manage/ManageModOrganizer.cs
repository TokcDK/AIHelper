using AIHelper.Games;
using AIHelper.SharedData;
using CheckForEmptyDir;
using INIFileMan;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AIHelper.Manage.ManageModOrganizer;

namespace AIHelper.Manage
{
    static class ManageModOrganizer
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();
        internal static bool IsInOverwriteFolder(this string filePath)
        {
            return filePath.ToUpperInvariant().Contains(ManageSettings.CurrentGameOverwriteFolderPath.ToUpperInvariant());
        }

        public static void SetMoModsVariables()
        {
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

        public static void CopyModOrganizerUserFiles(string moDirAltName)
        {
            //var game = Data.CurrentGame.GetCurrentGameIndex()];
            string gameMoDirPath = Path.Combine(ManageSettings.CurrentGameDirPath, "MO");
            string gameMoDirPathAlt = Path.Combine(ManageSettings.CurrentGameDirPath, moDirAltName);

            // dirs and files required for work
            var subPaths = new Dictionary<string, ObjectType>
            {
                { "Profiles", ObjectType.Directory },
                { "Overwrite", ObjectType.Directory },
                { "categories.dat", ObjectType.File },
                { "ModOrganizer.ini", ObjectType.File }
            };

            foreach (var subPath in subPaths)
            {
                try
                {
                    var altpath = Path.GetFullPath(Path.Combine(gameMoDirPathAlt, subPath.Key));
                    var workpath = Path.GetFullPath(Path.Combine(gameMoDirPath, subPath.Key));
                    if (subPath.Value == ObjectType.Directory && Directory.Exists(altpath) && (!Directory.Exists(workpath) || !ManageFilesFoldersExtensions.IsAnyFileExistsInTheDir(workpath, allDirectories: true)))
                    {
                        ManageFilesFoldersExtensions.CopyAll(altpath, workpath);
                    }
                    else if (subPath.Value == ObjectType.File && File.Exists(altpath) && !File.Exists(workpath))
                    {
                        File.Copy(altpath, workpath);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error("An error occured while MO files coping. error:\r\n" + ex);
                }
            }
        }

        /// <summary>
        /// When <paramref name="ConvertFromMODate"/> is true it will convert <paramref name="versionString"/> looking like d9.10.2011 mo date version format to 2011.10.9
        /// When <paramref name="ConvertFromMODate"/> is false it will convert <paramref name="versionString"/> looking like 2011.10.9 to d9.10.2011 mo date version format
        /// </summary>
        /// <param name="versionString"></param>
        /// <param name="ConvertFromMODate"></param>
        internal static void ConvertMODateVersion(ref string versionString, bool ConvertFromMODate = true)
        {
            var dateRegex = new Regex("^" + (ConvertFromMODate ? "d" : "") + @"([0-9]{" + (ConvertFromMODate ? "1,2" : "2,4") + @"})\.([0-9]{1,2})\.([0-9]{" + (ConvertFromMODate ? "2,4" : "1,2") + @"})");
            var dateRegexMatch = dateRegex.Match(versionString);
            if (dateRegexMatch.Success)
            {
                versionString = (ConvertFromMODate ? "" : "d") + dateRegexMatch.Groups[3].Value.TrimStart('0') + "." + dateRegexMatch.Groups[2].Value.TrimStart('0') + "." + dateRegexMatch.Groups[1].Value.TrimStart('0');
            }
        }

        internal static bool IsFileDirExistsInDataOrOverwrite(string filedir, out string source)
        {
            //string filePathInOverwrite =
            //           Path.GetFullPath(
            //               Path.Combine(ManageSettings.GetCurrentGameMOOverwritePath() + Path.DirectorySeparatorChar + filedir)
            //                           );
            if (File.Exists(
                       Path.GetFullPath(
                           Path.Combine(ManageSettings.CurrentGameMoOverwritePath + Path.DirectorySeparatorChar + filedir)
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
                           Path.Combine(ManageSettings.CurrentGameDataDirPath + Path.DirectorySeparatorChar + filedir)
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
            var gameNameByExe = ManageSettings.CurrentGameExeName.Replace("_64", string.Empty).Replace("_32", string.Empty);
            var userprofile = Path.Combine("%USERPROFILE%", "appdata", "locallow", "illusion__" + gameNameByExe.Replace("Trial", string.Empty), gameNameByExe, "output_log.txt");
            var outputLog = Environment.ExpandEnvironmentVariables(userprofile);
            if (File.Exists(outputLog))
            {
                Process.Start("explorer.exe", outputLog);
            }
            else
            {
                if (File.Exists(Path.Combine(ManageSettings.CurrentGameDataDirPath, ManageSettings.CurrentGameExeName + "_Data", "output_log.txt")))
                {
                    Process.Start("explorer.exe", Path.Combine(ManageSettings.CurrentGameDataDirPath, ManageSettings.CurrentGameExeName + "_Data", "output_log.txt"));
                }
                else if (File.Exists(Path.Combine(ManageSettings.CurrentGameDataDirPath, gameNameByExe + "_Data", "output_log.txt")))
                {
                    Process.Start("explorer.exe", Path.Combine(ManageSettings.CurrentGameDataDirPath, gameNameByExe + "_Data", "output_log.txt"));
                }

            }
        }

        public static void MOUSFSLoadingFix(bool removeLinks = false)
        {
            if (!ManageSettings.IsMoMode) return;

            BepInExPreloadersFix(removeLinks);

            string[,] objectLinkPaths = ManageSettings.Games.Game.DirLinkPaths;

            int objectLinkPathsLength = objectLinkPaths.Length / 2;
            for (int i = 0; i < objectLinkPathsLength; i++)
            {
                // try to find last not empty dir path
                var objectPath = new DirectoryInfo(ManageModOrganizer.GetLastPath(objectLinkPaths[i, 0], isDir: true, tryFindWithContent: true)); ;

                var linkPath = new DirectoryInfo(objectLinkPaths[i, 1]);

                if (removeLinks)
                {
                    ManageSymLinkExtensions.DeleteIfSymlink(linkPath.FullName);
                }
                else
                {
                    if (linkPath.Exists && linkPath.IsSymlink())
                    {
                        if (linkPath.IsValidSymlink(objectPath.FullName))
                        {
                            continue;
                        }
                    }

                    try
                    {
                        if (ManageSymLinkExtensions.CreateSymlink
                          (
                           objectPath.FullName
                           ,
                           linkPath.FullName
                           ,
                           true
                           ,
                           ObjectType.Directory
                          ))
                        {
                        }
                        else
                        {
                            // need when dir in data exists and have content, then content will be move to target
                            ManageFilesFoldersExtensions.MoveContent(linkPath.FullName, objectPath.FullName);

                            ManageSymLinkExtensions.CreateSymlink
                                (
                                 objectPath.FullName
                                 ,
                                 linkPath.FullName
                                 ,
                                 true
                                 ,
                                 ObjectType.Directory
                                );
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error("An error occured while symlink creation:\r\n" + ex);
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
            string[,] bepInExFilesPaths = ManageSettings.Games.Game.ObjectsForMove;

            int bepInExFilesPathsLength = bepInExFilesPaths.Length / 2;
            for (int i = 0; i < bepInExFilesPathsLength; i++)
            {                
                var sourceFilePath = bepInExFilesPaths[i, 0]; // source file, which to copy
                var targetFilePath = bepInExFilesPaths[i, 1]; // target file path, where to copy
                if (remove)
                {
                    // remove files mode
                    try
                    {
                        if (File.Exists(targetFilePath) && (File.Exists(sourceFilePath) 
                            || ManageSymLinkExtensions.IsSymlink(targetFilePath))) File.Delete(targetFilePath);
                    }
                    catch (Exception ex)
                    {
                        _log.Error("BepInExPreloadersFix error:" + Environment.NewLine + ex);
                    }
                }
                else
                {
                    // add file mode
                    try
                    {

                        sourceFilePath = ManageModOrganizer.GetLastPath(sourceFilePath); // get path in last mod in MO load order

                        //skip file if source not exists
                        //if (!File.Exists(sourceFilePath))
                        //{
                        //    continue;
                        //}

                        //skip file if not in enabled mod
                        bool isNotExistsSource = false;
                        if ((isNotExistsSource = !File.Exists(sourceFilePath)) || !ManageModOrganizer.IsInEnabledModOrOverwrite(sourceFilePath))//skip if no active mod found
                        {
                            if (File.Exists(targetFilePath)) File.Delete(targetFilePath);

                            if(isNotExistsSource) _log.Warn($"{nameof(BepInExPreloadersFix)}, Source path is not exists: {sourceFilePath}");

                            continue;
                        }

                        if (File.Exists(targetFilePath))
                        {
                            // chack if latest version of the target file when exists
                            if (
                                ManageSymLinkExtensions.IsSymlink(targetFilePath)
                                ||
                                new FileInfo(targetFilePath).Length != new FileInfo(sourceFilePath).Length
                                ||
                                FileVersionInfo.GetVersionInfo(targetFilePath).ProductVersion != FileVersionInfo.GetVersionInfo(sourceFilePath).ProductVersion
                                )
                            {
                                File.Delete(targetFilePath); // remove when old or invalid
                            }
                            else
                            {
                                continue; // do not touch when actual and valid
                            }
                        }

                        // copy actual version of the file
                        Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath));
                        File.Copy(sourceFilePath, targetFilePath);
                    }
                    catch (Exception ex)
                    {
                        _log.Error("BepInExPreloadersFix error:" + Environment.NewLine + ex);
                    }
                }
            }

            // clean empty dirs in bepinex fir
            ManageFilesFoldersExtensions.DeleteEmptySubfolders(Path.Combine(ManageSettings.CurrentGameDataDirPath, "BepInEx"), true);

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
            if (sourceFilePath.Contains(ManageSettings.CurrentGameOverwriteFolderPath) || sourceFilePath.Contains(ManageSettings.CurrentGameMoOverwritePath))
            {
                return true;
            }

            if (sourceFilePath.Contains(ManageSettings.CurrentGameModsDirPath))
            {
                //remove Mods path slit and get 1st element as modname
                var noModsPath = sourceFilePath.Replace(ManageSettings.CurrentGameModsDirPath, string.Empty);
                var splittedPath = noModsPath.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
                var modname = splittedPath[0];

                foreach (var name in ManageModOrganizer.EnumerateModNamesListFromActiveMoProfile())
                {
                    if (modname == name)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static string GetUserDataSubFolder(string firstCandidateFolder, string type)
        {
            string[] targetFolders = new string[3]
            {
                firstCandidateFolder.Substring(0,1)== " " ? Path.Combine(ManageSettings.CurrentGameModsDirPath, "OrganizedModPack Downloaded"+firstCandidateFolder) : firstCandidateFolder,
                Path.Combine(ManageSettings.CurrentGameModsDirPath, "MyUserData"),
                ManageSettings.CurrentGameOverwriteFolderPath
            };

            string typeFolder = string.Empty;
            string targetFolderName = string.Empty;
            switch (type)
            {
                case "f":
                    typeFolder = "chara";
                    targetFolderName = "female";
                    break;
                case "m":
                    typeFolder = "chara";
                    targetFolderName = "male";
                    break;
                case "c":
                    targetFolderName = "coordinate";
                    //TypeFolder = "";
                    break;
                case "h":
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                    typeFolder = "housing";
                    targetFolderName = type.Length == 2 ? "0" + type.Remove(0, 1) : string.Empty;
                    break;
                case "cf":
                    typeFolder = "cardframe";
                    targetFolderName = "Front";
                    break;
                case "cb":
                    typeFolder = "cardframe";
                    targetFolderName = "Back";
                    break;
                case "o":
                    //TypeFolder = "";
                    targetFolderName = "Overlays";
                    break;
                case "s":
                    typeFolder = "studio";
                    targetFolderName = "scene";
                    break;
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

            return Path.Combine(ManageSettings.CurrentGameOverwriteFolderPath, "UserData", typeFolder, targetFolderName);
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

        public static string GetAuthorName(string subdir = null, string name = null)
        {
            string author = string.Empty;

            if (!string.IsNullOrEmpty(subdir))
            {
                string modsDir = Path.GetFileName(subdir) == "mods" ? subdir : Path.Combine(subdir, "mods");
                if (Directory.Exists(modsDir) && ManageFilesFoldersExtensions.IsAnyFileExistsInTheDir(modsDir, ".zip", false))
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
                                    ManageFilesFoldersExtensions.DeleteEmptySubfolders(tempDir);
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

                if (/*author.Length == 0 &&*/ ManageFilesFoldersExtensions.IsAnyFileExistsInTheDir(subdir, ".dll"))
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

                if (author.Length == 0 && ManageFilesFoldersExtensions.IsAnyFileExistsInTheDir(subdir, ".txt", false))
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

                    if (!name.StartsWith("[" + ManageSettings.Games.Game.GameAbbreviation + "]", StringComparison.InvariantCultureIgnoreCase))
                    {
                        author = s[0].Remove(0, 1);
                    }
                    else if (name.StartsWith("[" + ManageSettings.Games.Game.GameAbbreviation + "][", StringComparison.InvariantCultureIgnoreCase))
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

        /// <summary>
        /// will get instance of SideloaderZipmodInfo from <paramref name="zipfile"/>'s manifest.xml if it is exists there
        /// </summary>
        /// <param name="zipfile"></param>
        /// <returns></returns>
        internal static SideloaderZipmodInfo GetManifestFromZipFile(string zipfile)
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipfile))
            {
                using (var manifest = archive.GetZipEntryStream("manifest.xml"))
                {
                    if (manifest != null)
                    {
                        return new SideloaderZipmodInfo(manifest);
                    }
                }
            }

            return null;
        }

        internal class SideloaderZipmodInfo
        {
            /// <summary>
            /// zipmod unique id
            /// </summary>
            internal string guid;
            /// <summary>
            /// name of mod
            /// </summary>
            internal string name;
            /// <summary>
            /// varsion of mod
            /// </summary>
            internal string version;
            /// <summary>
            /// mod author
            /// </summary>
            internal string author;
            /// <summary>
            /// mod's description
            /// </summary>
            internal string description;
            /// <summary>
            /// author's website
            /// </summary>
            internal string website;
            /// <summary>
            /// game id for whichthis zipmod is
            /// </summary>
            internal string game;

            public SideloaderZipmodInfo(string manifestXmlPath)
            {
                ReadManifestValues(manifestXmlPath);
            }

            public SideloaderZipmodInfo(Stream manifest)
            {
                using (var reader = new StreamReader(manifest))
                {
                    ReadManifestValues(reader.ReadToEnd(), true);
                }
            }

            void ReadManifestValues(string manifestXml, bool isXmlString = false)
            {
                guid = ManageXml.ReadXmlValue(manifestXml, "manifest/name", string.Empty, isXmlString: isXmlString);
                name = ManageXml.ReadXmlValue(manifestXml, "manifest/name", string.Empty, isXmlString: isXmlString);
                version = ManageXml.ReadXmlValue(manifestXml, "manifest/version", "0", isXmlString: isXmlString);
                author = ManageXml.ReadXmlValue(manifestXml, "manifest/author", "Unknown author", isXmlString: isXmlString);
                description = ManageXml.ReadXmlValue(manifestXml, "manifest/description", "Unknown description", isXmlString: isXmlString);
                website = ManageXml.ReadXmlValue(manifestXml, "manifest/website", string.Empty, isXmlString: isXmlString);
                game = ManageXml.ReadXmlValue(manifestXml, "manifest/game", ManageSettings.ZipmodManifestGameNameByCurrentGame, isXmlString: isXmlString); //установил умолчание как "AI Girl"
            }
        }

        /// <summary>
        /// return mod path for input path in Mods
        /// </summary>
        /// <param name="inputPath"></param>
        /// <returns></returns>
        internal static string GetPathInMods(this string inputPath, string defaultPath = null)
        {
            //search Mods Path by step up to parent dir while it will be Mods path
            string modPath;
            var folderPath = modPath = inputPath;
            while (!string.Equals(folderPath/*.TrimEnd(new char[] { '/', '\\' })*/ , ManageSettings.CurrentGameModsDirPath/*.TrimEnd(new char[] { '/', '\\' })*/, StringComparison.InvariantCultureIgnoreCase)
                //&& !string.Equals(folderPath, ManageSettings.GetOverwriteFolder(), StringComparison.InvariantCultureIgnoreCase)
                )
            {
                modPath = folderPath;
                folderPath = Path.GetDirectoryName(folderPath);
                if (string.Equals(folderPath, ManageSettings.CurrentGameDirPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    _log.Warn("Warning. Path in Mods not found." + "\r\ninputPath=" + inputPath + "\r\nModPath=" + modPath + "\r\nFolderPath=" + folderPath);
                    return defaultPath;
                }
            }

            if (modPath != string.Empty &&
                (string.Equals(Path.GetFileName(modPath), Path.GetFileName(ManageSettings.CurrentGameModsDirPath), StringComparison.InvariantCultureIgnoreCase)//temp debug check
                                                                                                                                                               //|| string.Equals(Path.GetFileName(modPath), Path.GetFileName(ManageSettings.GetOverwriteFolder()), StringComparison.InvariantCultureIgnoreCase)
                )
                )
            {
                _log.Warn("warning. log path is Mods. ModPath=" + modPath + ". FolderPath=" + folderPath);
            }
            else if (modPath == string.Empty)
            {
                _log.Warn("warning. ModPath is null, set to default.(" + modPath + ") FolderPath=" + folderPath);
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
                    zipmodsGuidList.Add(guid, saveFullPath ? sourceModZipmodPath : ManageModOrganizer.GetPathInMods(sourceModZipmodPath));
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
            HashSet<string> addedModPackNames = new HashSet<string>(10);
            foreach (var dir in Directory.EnumerateDirectories(ManageSettings.CurrentGameModsDirPath))
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

                    var oldName = name;
                    name += CheckFemaleMaleUncensor(modpackdir, name);

                    var modpackName = Path.GetFileName(dir);

                    // skip when same modpack name already added as target dir
                    // but dont skip when it is uncensor selectr Male Female splitted zipmods of one modpack
                    if (oldName == name && addedModPackNames.Contains(modpackName))
                    {
                        continue;
                    }

                    //add to list
                    if (!packs.ContainsKey(name))
                    {
                        added.Add(name);
                        packs.Add(name, modpackName);
                        addedModPackNames.Add(modpackName);
                    }
                }
            }

            return packs;
        }

        /// <summary>
        /// Fills <paramref name="inputList"/> with paths of <paramref name="fileName"/> found in <paramref name="inputDir"/>
        /// </summary>
        /// <param name="inputDir">Dir where to search</param>
        /// <param name="fileName">File name which to search</param>
        /// <param name="inputList">List where to add found paths</param>
        internal static void AddExistFilePathsUsing(this List<string> inputList, string inputDir, string fileName)
        {
            var filePath = Path.Combine(inputDir, fileName);
            if (File.Exists(filePath))
            {
                inputList.Add(filePath);
            }

            // parallel iterate mods because mod order is not important here
            Parallel.ForEach(Directory.EnumerateDirectories(inputDir), dir =>
            {
                inputList.AddExistFilePathsUsing(dir, fileName);
            });
        }

        /// <summary>
        /// make symbolik links from fileinfo infos
        /// </summary>
        internal static void MakeLinks()
        {
            if (File.Exists(ManageSettings.OverallLinkInfoFilePath))
            {
                ParseLinkInfo(new FileInfo(ManageSettings.OverallLinkInfoFilePath), out List<string> _, true);
            }
            else
            {
                return;
            }

            //var linkInfosList = new List<string>();
            //var infoFileName = ManageSettings.GetLinkInfoFileName();
            ////check overwrite
            //linkInfosList.AddExistFilePathsUsing(ManageSettings.GetCurrentGameOverwriteFolderPath(), infoFileName);

            //var modsLinkInfoFile = Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), infoFileName);
            ////create link for mods root info file
            ////ParseLinkInfo(modsLinkInfoFile);
            //if (File.Exists(modsLinkInfoFile))
            //{
            //    linkInfosList.Add(modsLinkInfoFile);
            //}

            ////check mod dirs
            //Parallel.ForEach(ManageModOrganizer.EnumerateModNamesListFromActiveMoProfile(), modName =>
            //{
            //    var dir = Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), modName);
            //    linkInfosList.AddExistFilePathsUsing(dir, infoFileName);
            //});

            //// create links
            //// parallel iterate mods because mod order is not important here
            //var sw = new StreamWriter(ManageSettings.GetOverallLinkInfoFilePath());
            //Parallel.ForEach(linkInfosList, file =>
            //{
            //    if (ParseLinkInfo(new FileInfo(file), out List<string> newLines, false))
            //    {
            //        sw.WriteLineAsync(string.Join(Environment.NewLine, newLines));// write lines in new file
            //    }
            //});
            //sw.Dispose();
        }

        private static bool ParseLinkInfo(FileInfo linkinfo, out List<string> newLines, bool overall = false)
        {
            newLines = new List<string>();
            if (!linkinfo.Exists || linkinfo.Length == 0)
            {
                // skip if link info file is empty
                return false;
            }

            int minDataSize = 2;
            int maxDataSize = 3;
            if (overall)
            {
                minDataSize = 3;
                maxDataSize = 4;
            }

            var ret = false;

            using (StreamReader reader = new StreamReader(linkinfo.FullName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";"))
                    {
                        // skip empty lines and comments
                        continue;
                    }

                    var info = line.Split(',');
                    var infoLength = info.Length;
                    if (infoLength < minDataSize || infoLength > maxDataSize)
                    {
                        continue;
                    }
                    bool HasTargetLinkNameSet = infoLength == maxDataSize; // if set target link name, use it else use object name

                    bool IsDir = info[0] == "d";
                    if (!IsDir && info[0] != "f")
                    {
                        // if type is incorrect or info invalid
                        continue;
                    }


                    if (HasTargetLinkNameSet && ManageFilesFoldersExtensions.ContainsAnyInvalidCharacters(info[minDataSize]))
                    {
                        // if target link name has invalid chars
                        //continue;

                        // skip name because it invalid
                        HasTargetLinkNameSet = false;
                        info = info.Take(minDataSize).ToArray();
                    }

                    info[1] = ConvertVars(info[1]);
                    if (ManageFilesFoldersExtensions.ContainsAnyInvalidCharacters(info[1]))
                    {
                        // if object path has invalid chars
                        continue;
                    }

                    var targetObjectPath = info[1].IndexOf(".\\") != -1 ? Path.GetFullPath((overall ? Path.GetDirectoryName(info[2]) : linkinfo.Directory.FullName) + Path.DirectorySeparatorChar + info[1]) // get full path if it was relative. current directory will be linkinfo file's directory
                        : info[1]; // path is not relative

                    // skip if target object is not exists
                    if (!targetObjectPath.Exists(IsDir))
                    {
                        continue;
                    }

                    var targetObjectType = IsDir ? ObjectType.Directory : ObjectType.File;

                    if (targetObjectPath.IsSymlink(targetObjectType))
                    {
                        if (targetObjectPath.IsValidSymlink())
                        {
                            // get symlink target if object is symlink
                            targetObjectPath = targetObjectPath.GetSymlinkTarget(targetObjectType);
                        }
                        else
                        {
                            _log.Warn("Warning! Link info file has invalid simlink as target of path. File:" + linkinfo.FullName + "\r\n");
                            // skip if target object is invalid symlink
                            continue;
                        }
                    }

                    string linkParentDirPath = overall ? (info[minDataSize - 1].IndexOf(".\\") != -1 ? Path.GetDirectoryName(Path.GetFullPath(Path.Combine(ManageSettings.CurrentGameDirPath, info[minDataSize - 1]))) // if relative path then get full path from current game dir
                        : Path.GetDirectoryName(info[minDataSize - 1]))
                        : linkinfo.Directory.FullName; // for old linkinfo
                    var symlinkPath = Path.Combine(linkParentDirPath,
                    HasTargetLinkNameSet ? info[minDataSize] : (overall ? Path.GetFileName(info[minDataSize - 1]) : Path.GetFileName(targetObjectPath))); // get object name or name from info is was set

                    if (string.Equals(symlinkPath.Replace(ManageSettings.CurrentGameParentDirPath, string.Empty), symlinkPath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        //reject because target link is not located in current game's dir
                        return false;
                    }

                    if (!overall)
                    {
                        newLines.Add((IsDir ? "d" : "f") + "," + targetObjectPath + "," + symlinkPath + (HasTargetLinkNameSet ? "," + info[minDataSize] : string.Empty));
                    }

                    if (symlinkPath.Exists(IsDir))
                    {
                        bool isLink;
                        if ((isLink = symlinkPath.IsSymlink(targetObjectType) && symlinkPath.GetSymlinkTarget(targetObjectType) == targetObjectPath) || (!IsDir || !symlinkPath.IsEmptyDir()))
                        {
                            ret = true;
                            // skip if not synlink or exists symlink with valid target
                            continue;
                        }
                        else
                        {
                            // delene invalid symlink
                            if (IsDir)
                            {
                                Directory.Delete(symlinkPath);
                            }
                            else
                            {
                                File.Delete(symlinkPath);
                            }
                        }
                    }

                    // create symlink
                    targetObjectPath.CreateSymlink(symlinkPath
                        , isRelative: false
                        , objectType: targetObjectType);

                    ret = true;
                    //return; // info found, stop read file // commented because can be several links now
                }

                return ret;
            }
        }

        /// <summary>
        /// convert vars to paths
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        private static string ConvertVars(string inputString)
        {
            inputString = GetTargetGamePath(inputString);

            return inputString;
        }

        /// <summary>
        /// replace var like %game:Koikatsu% to first found dir path
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        private static string GetTargetGamePath(string inputString)
        {
            //example: %game:Koikatsu%
            if (inputString.Contains("%game:"))
            {
                var regexMatch = Regex.Match(inputString, "%game:([^%]+)%");

                var gameDirName = regexMatch.Groups[1].Value;
                foreach (var game in ManageSettings.Games.Games)
                {
                    if (game.GameDirInfo.Name == gameDirName)
                    {
                        // return path with the variable %game:Gamename% replaced to 1st game found path
                        return inputString.Replace("%game:" + gameDirName + "%", game.GameDirInfo.FullName);
                    }
                }

                return inputString.Replace("%game:" + gameDirName + "%", gameDirName);// else just replace by extracted name

            }

            return inputString;
        }

        //private static void ParseLinkInfo(FileInfo linkinfo)
        //{
        //    if (!linkinfo.Exists || linkinfo.Length == 0)
        //    {
        //        // skip if link info file is empty
        //        return;
        //    }

        //    using (StreamReader reader = new StreamReader(linkinfo.FullName))
        //    {
        //        string line;
        //        while ((line = reader.ReadLine()) != null)
        //        {
        //            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";"))
        //            {
        //                // skip empty lines and comments
        //                continue;
        //            }

        //            var info = line.Split(',');
        //            var infoLength = info.Length;
        //            if (infoLength < 2 || infoLength > 3)
        //            {
        //                continue;
        //            }
        //            bool HasTargetLinkNameSet = infoLength == 3; // if set target link name, use it else use object name

        //            bool IsDir = info[0] == "d";
        //            if (!IsDir && info[0] != "f")
        //            {
        //                // if type is incorrect or info invalid
        //                continue;
        //            }

        //            if (HasTargetLinkNameSet && ManageFilesFoldersExtensions.ContainsAnyInvalidCharacters(info[2]))
        //            {
        //                // if target link name has invalid chars
        //                continue;
        //            }

        //            var targetObjectPath = info[1].IndexOf(".\\") != -1 ? Path.GetFullPath(linkinfo.Directory.FullName + Path.DirectorySeparatorChar + info[1]) // get full path if it was relative. current directory will be linkinfo file's directory
        //                : info[1]; // path is not relative

        //            if (ManageFilesFoldersExtensions.ContainsAnyInvalidCharacters(targetObjectPath))
        //            {
        //                // if object path has invalid chars
        //                continue;
        //            }

        //            // skip if target object is not exists
        //            if (!targetObjectPath.Exists(IsDir))
        //            {
        //                continue;
        //            }

        //            var targetObjectType = IsDir ? ObjectType.Directory : ObjectType.File;

        //            if (targetObjectPath.IsSymlink(targetObjectType))
        //            {
        //                if (targetObjectPath.IsValidSymlink())
        //                {
        //                    // get symlink target if object is symlink
        //                    targetObjectPath = targetObjectPath.GetSymlinkTarget(targetObjectType);
        //                }
        //                else
        //                {
        //                    _log.Debug("Warning! Link info file has invalid simlink as target of path. File:" + linkinfo.FullName + "\r\n");
        //                    // skip if target object is invalid symlink
        //                    continue;
        //                }
        //            }

        //            var symlinkPath = Path.Combine(linkinfo.Directory.FullName,
        //                HasTargetLinkNameSet ? info[2] : Path.GetFileName(targetObjectPath)); // get object name or name from info is was set

        //            if (symlinkPath.Exists(IsDir))
        //            {
        //                bool isLink;
        //                if ((isLink = symlinkPath.IsSymlink(targetObjectType) && symlinkPath.GetSymlinkTarget(targetObjectType) == targetObjectPath) || (!IsDir || !symlinkPath.IsEmptyDir()))
        //                {
        //                    // skip if not synlink or exists symlink with valid target
        //                    continue;
        //                }
        //                else
        //                {
        //                    // delene invalid symlink
        //                    if (IsDir)
        //                    {
        //                        Directory.Delete(symlinkPath);
        //                    }
        //                    else
        //                    {
        //                        File.Delete(symlinkPath);
        //                    }
        //                }
        //            }

        //            // create symlink
        //            targetObjectPath.CreateSymlink(symlinkPath
        //                , isRelative: false
        //                , objectType: targetObjectType);

        //            //return; // info found, stop read file // commented because can be several links now
        //        }
        //    }
        //}

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
                var hasFemale = ManageFilesFoldersExtensions.IsAnyFileExistsInTheDir(modpackdir, "*[Female]*.zipmod", true);
                var hasMale = ManageFilesFoldersExtensions.IsAnyFileExistsInTheDir(modpackdir, "*[Penis]*.zipmod", true) || ManageFilesFoldersExtensions.IsAnyFileExistsInTheDir(modpackdir, "*[Balls]*.zipmod", true);
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

        internal class ModlistProfileInfo
        {
            internal string ModlistPath = "";
            internal List<ProfileModlistRecord> Items;
            internal Dictionary<string, ProfileModlistRecord> ItemByName;

            internal const string ListDescriptionMarker = "# This file was automatically generated by Mod Organizer.";
            internal const string SeparatorMarker = "_separator";

            /// <summary>
            /// init and load content of modlist for current profile
            /// </summary>
            public ModlistProfileInfo()
            {
                Items = new List<ProfileModlistRecord>();
                ItemByName = new Dictionary<string, ProfileModlistRecord>();
                Load();
            }

            /// <summary>
            /// init and load content of modlist for selected path
            /// </summary>
            public ModlistProfileInfo(string modListPath)
            {
                Items = new List<ProfileModlistRecord>();
                ItemByName = new Dictionary<string, ProfileModlistRecord>();
                Load(modListPath);
            }

            /// <summary>
            /// modlist load
            /// </summary>
            void Load(string modListPath = null)
            {
                ModlistPath = modListPath ?? ManageSettings.CurrentMoProfileModlistPath;

                if (!File.Exists(ModlistPath))
                {
                    return;
                }

                var modlistContent = File.ReadAllLines(ManageSettings.CurrentMoProfileModlistPath);
                Array.Reverse(modlistContent);

                var modPriority = 0;
                ProfileModlistRecord lastSeparator = null;
                foreach (var line in modlistContent)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.InvariantCulture))
                    {
                        continue;
                    }

                    var mod = new ProfileModlistRecord();
                    mod.Priority = modPriority;
                    mod.IsEnabled = line[0] == '+';
                    var indexOfSeparatorMarker = line.IndexOf(SeparatorMarker, StringComparison.InvariantCulture);
                    mod.IsSeparator = indexOfSeparatorMarker > -1;
                    mod.Name = line.Substring(1);
                    mod.Path = Path.Combine(ManageSettings.CurrentGameModsDirPath, mod.Name);
                    mod.IsExist = Directory.Exists(mod.Path);
                    mod.ParentSeparator = lastSeparator;


                    if (!mod.IsSeparator && lastSeparator != null)
                    {
                        lastSeparator.Items.Add(mod); // add subitems references to understand which items is under the group separator
                    }

                    // reset separator if was changed
                    if (mod.IsSeparator && lastSeparator != mod.ParentSeparator)
                    {
                        lastSeparator = mod;
                    }


                    Items.Add(mod);
                    ItemByName.Add(mod.Name, mod);

                    modPriority++;
                }
            }

            internal void Insert(ProfileModlistRecord profileModlistRecord, string modToPlaceWith = "", bool placeAfter = true)
            {
                ProfileModlistRecord existsItem = GetItemByName(profileModlistRecord.Name);
                if (existsItem != null)
                {
                    // activate or deactivate item if it already exists
                    existsItem.IsEnabled = profileModlistRecord.IsEnabled;
                    return;
                }

                // add by parent separator
                if (profileModlistRecord.ParentSeparator != null && profileModlistRecord.ParentSeparator.Name.Length > 0)
                {
                    bool added = false;
                    var modPriority = 0;

                    bool groupFound = false;
                    var newItems = new List<ProfileModlistRecord>();
                    ProfileModlistRecord foundGroup = null;
                    foreach (var item in Items)
                    {
                        if (!added && !groupFound && item.IsSeparator && item.Name == profileModlistRecord.ParentSeparator.Name)
                        {
                            foundGroup = item;
                            groupFound = true;
                        }
                        else if (!added && groupFound && item.IsSeparator && item.Name != profileModlistRecord.ParentSeparator.Name)
                        {
                            // set parent group, new priority and increase priority number
                            foundGroup.Items.Add(profileModlistRecord); // add inserting item in list of under separator
                            profileModlistRecord.ParentSeparator = foundGroup;
                            profileModlistRecord.Priority = modPriority++;

                            newItems.Add(profileModlistRecord);
                            added = true;
                        }

                        item.Priority = modPriority;
                        newItems.Add(item);

                        modPriority++;
                    }

                    if (!added) // add in the end when was not added
                    {
                        profileModlistRecord.Priority = modPriority;
                        profileModlistRecord.ParentSeparator = null;
                        newItems.Add(profileModlistRecord);
                    }

                    Items = newItems;

                    return;
                }

                //add by modToPlaceWith
                if (!string.IsNullOrWhiteSpace(modToPlaceWith))
                {
                    bool added = false;
                    var modPriority = 0;
                    bool placeMeNow = false;

                    var newItems = new List<ProfileModlistRecord>();
                    foreach (var item in Items)
                    {
                        if (!added && placeMeNow)
                        {
                            profileModlistRecord.Priority = modPriority++;
                            newItems.Add(profileModlistRecord);
                            added = true;
                        }
                        else if (!added && !placeMeNow && item.Name == profileModlistRecord.Name)
                        {
                            if (placeAfter)
                            {
                                placeMeNow = true;
                            }
                            else
                            {
                                profileModlistRecord.Priority = modPriority++;
                                newItems.Add(profileModlistRecord);
                                added = true;
                            }
                        }

                        item.Priority = modPriority++;
                        newItems.Add(item);
                    }

                    if (!added) // add in the end when was not added
                    {
                        profileModlistRecord.Priority = modPriority;
                        profileModlistRecord.ParentSeparator = null;
                        newItems.Add(profileModlistRecord);
                    }

                    Items = newItems;

                    return;
                }


                // add by priority
                // insert at the end if mod priority is not set or more of max
                if (profileModlistRecord.Priority == -1 || profileModlistRecord.Priority >= Items.Count)
                {
                    profileModlistRecord.Priority = Items[Items.Count - 1].Priority + 1; // correct priority
                    Items.Add(profileModlistRecord);
                    return;
                }
                Items.Insert(profileModlistRecord.Priority - 1, profileModlistRecord);
            }

            private ProfileModlistRecord GetItemByName(string itemName)
            {
                if (ItemByName.ContainsKey(itemName))
                {
                    return ItemByName[itemName];
                }

                return null;
            }

            /// <summary>
            /// get list of mods from all items by selected mod type
            /// </summary>
            /// <param name="modType">Enabled, Disabled, Separator</param>
            /// <returns>list of mods by mod type</returns>
            internal List<ProfileModlistRecord> GetListBy(ModType modType)
            {
                return GetBy(modType).ToList();
            }

            /// <summary>
            /// get mods from all items by selected mod type
            /// </summary>
            /// <param name="modType">Enabled, Disabled, Separators</param>
            /// <param name="exists">True by default. Determines if add only existing mod folders</param>
            /// <returns>list of mods by mod type</returns>
            internal IEnumerable<ProfileModlistRecord> GetBy(ModType modType, bool exists = true)
            {
                foreach (var mod in Items)
                {
                    switch (modType)
                    {
                        case ModType.Separator when mod.IsSeparator:
                        case ModType.ModAny when !mod.IsSeparator:
                        case ModType.ModEnabled when !mod.IsSeparator && mod.IsEnabled:
                        case ModType.ModDisabled when !mod.IsSeparator && !mod.IsEnabled:
                            if (!exists || mod.IsExist) // mod exists or exists is false
                                yield return mod;
                            break;
                    }
                }
            }

            /// <summary>
            /// mod type
            /// </summary>
            internal enum ModType
            {
                /// <summary>
                /// any enabled or disabled mods
                /// </summary>
                ModAny,
                /// <summary>
                /// only enabled mods
                /// </summary>
                ModEnabled,
                /// <summary>
                /// only disabled mods
                /// </summary>
                ModDisabled,
                /// <summary>
                /// only separators
                /// </summary>
                Separator
            }

            /// <summary>
            /// save modlist
            /// </summary>
            internal void Save()
            {
                if (!File.Exists(ModlistPath))
                {
                    return;
                }

                var writeItems = new List<ProfileModlistRecord>(Items);
                writeItems.Reverse();
                using (var newModlist = new StreamWriter(ModlistPath))
                {
                    newModlist.WriteLine(ListDescriptionMarker);
                    foreach (var item in writeItems)
                    {
                        newModlist.WriteLine((item.IsEnabled ? "+" : "-") + item.Name);
                    }
                }
            }
        }

        internal static string GetExeNameByTitle(string customExeTitleName, INIFile ini = null)
        {
            if (ini == null)
            {
                ini = ManageIni.GetINIFile(ManageSettings.AppMOiniFilePath);
            }

            var customs = new CustomExecutables(ini);
            foreach (var customExe in customs.List)
            {
                if (customExe.Value.Title == customExeTitleName)
                {
                    if (File.Exists(customExe.Value.Binary))
                    {
                        return Path.GetFileNameWithoutExtension(customExe.Value.Binary);
                    }
                }
            }

            return "";
        }

        internal class ProfileModlistRecord
        {
            /// <summary>
            /// priority in modlist
            /// </summary>
            internal int Priority = -1;
            /// <summary>
            /// name of mod dir or name of separator dir
            /// </summary>
            internal string Name;
            /// <summary>
            /// path to folder
            /// </summary>
            internal string Path;
            /// <summary>
            /// true when enabled. separators is always disabled
            /// </summary>
            internal bool IsEnabled;
            /// <summary>
            /// true for separators
            /// </summary>
            internal bool IsSeparator;
            /// <summary>
            /// true when folder is exists in mods
            /// </summary>
            internal bool IsExist;

            /// <summary>
            /// parent separator
            /// </summary>
            internal ProfileModlistRecord ParentSeparator;
            /// <summary>
            /// child items for separator
            /// </summary>
            internal List<ProfileModlistRecord> Items = new List<ProfileModlistRecord>();
        }

        internal class ZipmodGUIIds
        {
            internal Dictionary<string, ZipmodInfo> GUIDList;
            readonly Dictionary<string, string> cachedGUIDList;

            /// <summary>
            /// Sideloader modpacks zipmod file infos are storing here
            /// </summary>
            /// <param name="LoadInfos">Load infos from active modlist right after init</param>
            public ZipmodGUIIds(bool LoadInfos = true)
            {
                GUIDList = new Dictionary<string, ZipmodInfo>();
                cachedGUIDList = new Dictionary<string, string>();
                if (LoadInfos)
                {
                    LoadAll();
                }
            }

            void LoadAll()
            {
                var modlist = new ModlistProfileInfo();

                ReadCachedGUID(cachedGUIDList);

                Parallel.ForEach(modlist.Items, item =>
                {
                    if (!item.IsExist || item.IsSeparator)
                    {
                        return;
                    }

                    if (!Directory.Exists(Path.Combine(item.Path, "mods")))
                    {
                        return;
                    }

                    foreach (var packDir in Directory.EnumerateDirectories(Path.Combine(item.Path, "mods"), "Sideloader Modpack*"))
                    {
                        Parallel.ForEach(Directory.EnumerateFiles(packDir, "*.zip*", SearchOption.AllDirectories), zipmod =>
                        {
                            Load(zipmod);
                        });
                    }
                });

                Task.Run(() => WriteCachedGUID(GUIDList)).ConfigureAwait(false); // not need to wait while will be writed
            }

            private readonly object GUIDListAddLock = new object();

            internal void Load(string zipmodPath)
            {
                //zipmod GUID save
                if (!File.Exists(zipmodPath) || (!string.Equals(Path.GetExtension(zipmodPath), ".ZIPMOD", StringComparison.InvariantCultureIgnoreCase) && !string.Equals(Path.GetExtension(zipmodPath), ".ZIP", StringComparison.InvariantCultureIgnoreCase)))
                {
                    return;
                }

                string guid = cachedGUIDList.ContainsKey(zipmodPath) ? cachedGUIDList[zipmodPath] : ManageArchive.GetZipmodGuid(zipmodPath);
                if (string.IsNullOrWhiteSpace(guid))
                {
                    return;
                }

                var zipmodInfo = new ZipmodInfo
                {
                    GUID = guid,
                    FileInfo = new FileInfo(zipmodPath),
                    SubPath = GetRelativeSubPathInMods(zipmodPath)
                };

                lock (GUIDListAddLock)
                {
                    if (!GUIDList.ContainsKey(guid))
                    {
                        GUIDList.Add(guid, zipmodInfo);
                    }
                }
            }
        }

        internal class ZipmodInfo
        {
            internal string GUID;
            internal FileInfo FileInfo;
            internal string SubPath;
        }

        static void WriteCachedGUID(Dictionary<string, ZipmodInfo> gUIDList)
        {
            var list = new Dictionary<string, ZipmodInfo>(gUIDList); // get copy of list because it will be removed

            Directory.CreateDirectory(Path.GetDirectoryName(ManageSettings.CachedGUIDFilePath));

            var sw = new StreamWriter(ManageSettings.CachedGUIDFilePath);
            foreach (var pair in list)
            {
                sw.WriteLine(pair.Value.FileInfo.FullName + cachedCUIDFileSplitter + pair.Key);
            }
            sw.Dispose();
        }

        const string cachedCUIDFileSplitter = "|>|";

        /// <summary>
        /// Fill cached guid list from file
        /// </summary>
        /// <returns></returns>
        private static bool ReadCachedGUID(Dictionary<string, string> cachedGUIDList)
        {
            if (!File.Exists(ManageSettings.CachedGUIDFilePath))
            {
                cachedGUIDList = new Dictionary<string, string>();
                return false;
            }

            foreach (var line in File.ReadAllLines(ManageSettings.CachedGUIDFilePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var data = line.Split(new[] { cachedCUIDFileSplitter }, StringSplitOptions.None);

                if (data.Length != 2)
                {
                    continue;
                }

                cachedGUIDList.Add(data[0], data[1]);
            }

            return true;
        }

        /// <summary>
        /// return relative subpath for input path
        /// </summary>
        /// <param name="zipmodPath"></param>
        /// <returns></returns>
        private static string GetRelativeSubPathInMods(string zipmodPath)
        {
            return zipmodPath.Replace(zipmodPath.GetPathInMods(), "");
        }

        internal static string MOremoveByteArray(string mOSelectedProfileDirPath)
        {
            if (mOSelectedProfileDirPath.StartsWith("@ByteArray(", StringComparison.InvariantCulture))
            {
                return mOSelectedProfileDirPath
                    .Remove(mOSelectedProfileDirPath.Length - 1, 1)
                    .Replace("@ByteArray(", string.Empty);
            }
            else
            {
                return mOSelectedProfileDirPath;
            }
        }

        public static void RedefineGameMoData()
        {
            //MOini
            ManageSettings.AppMOiniFilePath.ReCreateFileLinkWhenNotValid(ManageSettings.MOiniPathForSelectedGame, true);
            //Categories
            ManageSettings.AppMOcategoriesFilePath.ReCreateFileLinkWhenNotValid(ManageSettings.CurrentGameMOcategoriesFilePath, true);
        }

        internal static string GetMoVersion()
        {
            var exeversion = System.Diagnostics.FileVersionInfo.GetVersionInfo(ManageSettings.AppMOexePath);
            return exeversion.FileVersion;
        }

        public static void SetModOrganizerIniSettingsForTheGame()
        {
            RedefineGameMoData();

            //менять настройки МО только когда игра меняется
            if (!ManageSettings.CurrentGameIsChanging && Path.GetDirectoryName(ManageSettings.AppModOrganizerDirPath) != ManageSettings.ApplicationStartupPath)
            {
                return;
            }

            INIFile ini = ManageIni.GetINIFile(ManageSettings.ModOrganizerIniPath);

            SetCommonIniValues(ini);

            //await Task.Run(() => SetCustomExecutablesIniValues(INI)).ConfigureAwait(true);
            FixCustomExecutablesIniValues();
            ManageSettings.SetModOrganizerINISettingsForTheGame = false;

            //ManageSettings.CurrentGameIsChanging = false;
        }

        /// <summary>
        /// returns filename
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns></returns>
        internal static string MakeMoProfileModlistFileBuckup(string suffix = "")
        {
            var modlistPath = ManageSettings.CurrentMoProfileModlistPath;
            var lastBackup = GetLastBak(suffix);
            if (lastBackup == null || (File.Exists(modlistPath) && File.Exists(lastBackup) && File.ReadAllText(lastBackup) != File.ReadAllText(modlistPath)))
            {
                var targetFile = modlistPath + "." + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss", CultureInfo.InvariantCulture) + suffix;
                File.Copy(modlistPath, targetFile);
                return targetFile;
            }
            return string.Empty;
        }

        /// <summary>
        /// get last modlist buckup path
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns></returns>
        private static string GetLastBak(string suffix)
        {
            FileInfo last = null;
            foreach (var file in new DirectoryInfo(ManageSettings.MoSelectedProfileDirPath).EnumerateFiles("modlist.txt.*" + suffix))
            {
                if (last == null || last.LastWriteTime < file.LastWriteTime)
                {
                    last = file;
                }
            }
            return last?.FullName;
        }

        /// <summary>
        /// changes incorrect relative paths "../Data" fo custom executable for absolute Data path of currents game
        /// </summary>
        /// <param name="ini"></param>
        internal static void FixCustomExecutablesIniValues(INIFile ini = null)
        {
            if (ini == null)
            {
                ini = ManageIni.GetINIFile(ManageSettings.ModOrganizerIniPath);
            }

            var customExecutables = new CustomExecutables(ini);
            var customsToRemove = new List<string>();
            foreach (var record in customExecutables.List)
            {
                if (string.IsNullOrWhiteSpace(record.Value.Binary))
                {
                    customsToRemove.Add(record.Key); // add invalid custom to remove list
                    continue;
                }

                foreach (var attribute in new[] { "binary", "workingDirectory" })
                {
                    try
                    {
                        string fullPath = "";
                        if (record.Value.Attribute[attribute].Length > 0)
                        {
                            fullPath = Path.GetFullPath(record.Value.Attribute[attribute]);
                        }
                        bool isFile = attribute == "binary";
                        if ((isFile && File.Exists(fullPath)) || (!isFile && Directory.Exists(fullPath)))
                        {
                            //not need to change if path is exists
                        }
                        else
                        {
                            if (record.Value.Attribute[attribute].StartsWith("..", StringComparison.InvariantCulture))
                            {
                                //suppose relative path was from MO dir ..\%MODir%
                                //replace .. to absolute path of current game directory
                                var targetcorrectedrelative = record.Value.Attribute[attribute]
                                        .Remove(0, 2).Insert(0, ManageSettings.CurrentGameDirPath);

                                //replace other slashes
                                var targetcorrectedabsolute = Path.GetFullPath(targetcorrectedrelative);

                                FixExplorerPlusPlusPath(ref targetcorrectedabsolute);

                                record.Value.Attribute[attribute] = CustomExecutables.NormalizePath(targetcorrectedabsolute);
                            }
                            else
                            {
                                var newPath = record.Value.Attribute[attribute].Replace("/", "\\");
                                if (FixExplorerPlusPlusPath(ref newPath))
                                {
                                    record.Value.Attribute[attribute] = CustomExecutables.NormalizePath(newPath);
                                }
                            }
                        }
                    }
                    catch
                    {
                        _log.Error("FixCustomExecutablesIniValues:Error while path fix.\r\nKey=" + record.Key + "\r\nPath=" + record.Value.Binary);
                    }
                }
            }

            //remove invalid customs
            foreach (var custom in customsToRemove)
            {
                customExecutables.List.Remove(custom);
            }

            customExecutables.Save();
        }

        /// <summary>
        /// check if the <paramref name="game"/> is valid to use with the app
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static bool IsValidGame(this GameBase game)
        {
            return game != null
                   &&
                   Directory.Exists(game.GamePath)
                   &&
                   game.HasMainGameExe() // has main exe to play
                   &&
                   game.HasModOrganizerIni() // has mo ini
                   &&
                   game.HasAnyWorkProfile() // has atleast one profile
                   &&
                   game.CheckCategoriesDat()
                   ;
        }

        /// <summary>
        /// check if the <paramref name="game"/> has ModOrganizer.ini file in MO folder
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static bool HasModOrganizerIni(this GameBase game)
        {
            return File.Exists(Path.Combine(game.GamePath, ManageSettings.AppModOrganizerDirName, "ModOrganizer.ini"));
        }

        /// <summary>
        /// will create categories.dat file in mo folder of the <paramref name="game"/> if missing
        /// </summary>
        /// <param name="game"></param>
        internal static bool CheckCategoriesDat(this GameBase game)
        {
            string categories;
            if (!File.Exists(categories = Path.Combine(game.GamePath, ManageSettings.AppModOrganizerDirName, "categories.dat")))
            {
                Directory.CreateDirectory(Path.Combine(game.GamePath, ManageSettings.AppModOrganizerDirName));

                File.WriteAllText(categories, string.Empty);
            }

            return true;
        }

        /// <summary>
        /// check if the <paramref name="game"/> has main execute file
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static bool HasMainGameExe(this GameBase game)
        {
            return File.Exists(Path.Combine(ManageSettings.CurrentGameParentDirPath, game.DataPath, game.GameExeName + ".exe"));
        }

        /// <summary>
        /// check if MO dir of input <paramref name="game"/> has any profile with modlist.txt
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static bool HasAnyWorkProfile(this GameBase game)
        {
            foreach (var profileDir in Directory.EnumerateDirectories(Path.Combine(game.GamePath, ManageSettings.AppModOrganizerDirName, "Profiles")))
            {
                if (File.Exists(Path.Combine(profileDir, "modlist.txt")))
                {
                    return true;
                }
            }
            return false;
            //!Path.Combine(game.GetGamePath(), GetAppModOrganizerDirName(), "Profiles").IsNullOrEmptyDirectory(mask: "modlist.txt", recursive: true, preciseMask: true) // modlist.txt must exist atleast one
        }

        /// <summary>
        /// fixes alternative path for explorer++ to path to native mo dir
        /// </summary>
        /// <param name="inputPath"></param>
        private static bool FixExplorerPlusPlusPath(ref string inputPath)
        {
            var altModOrganizerDirNameForEpp = Regex.Match(inputPath, @"\\([^\\]+)\\explorer\+\+");
            if (altModOrganizerDirNameForEpp.Success && altModOrganizerDirNameForEpp.Result("$1").StartsWith("MO", StringComparison.InvariantCultureIgnoreCase))
            {
                inputPath = inputPath
                    .Remove(altModOrganizerDirNameForEpp.Index, altModOrganizerDirNameForEpp.Length)
                    .Insert(altModOrganizerDirNameForEpp.Index, @"\..\..\MO\explorer++");

                inputPath = Path.GetFullPath(inputPath);

                return true;
            }

            return false;
        }

        internal class CustomExecutables
        {
            /// <summary>
            /// list of custom executables
            /// </summary>
            internal Dictionary<string, CustomExecutable> List;
            INIFile _ini;

            /// <summary>
            /// Init new custom executables and load from default ModOrganizer.ini path for the selected game.
            /// </summary>
            internal CustomExecutables()
            {
                LoadFrom(ManageIni.GetINIFile(ManageSettings.AppMOiniFilePath));
            }

            /// <summary>
            /// Init new custom executables and load from specified ini.
            /// </summary>
            /// <param name="ini"></param>
            internal CustomExecutables(INIFile ini)
            {
                LoadFrom(ini);
            }

            private int _loadedListCustomsCount;

            bool NeedSetCurrentProfileSettingsIniSet = true;
            INIFile settingsIni;

            /// <summary>
            /// load customs in list
            /// </summary>
            /// <param name="ini"></param>
            internal void LoadFrom(INIFile ini)
            {
                if (List != null && List.Count > 0) // already loaded
                {
                    return;
                }

                ini.Configuration.AssigmentSpacer = ""; // no spaces for mo ini

                this._ini = ini; // set ini reference

                List = new Dictionary<string, CustomExecutable>();
                foreach (var keyData in ini.GetSectionKeyValuePairs("customExecutables"))
                {
                    var numName = keyData.Key.Split('\\');//numName[0] - number of customexecutable , numName[0] - name of attribute
                    if (numName.Length != 2)
                    {
                        continue;
                    }
                    var customNumber = numName[0];
                    var customKeyName = numName[1];

                    if (!List.ContainsKey(customNumber))
                    {
                        List.Add(customNumber, new CustomExecutable());
                    }

                    List[customNumber].Attribute[customKeyName] = keyData.Value;
                }

                _loadedListCustomsCount = List.Count;

                //Normalize values
                foreach (var customExecutable in List)
                {
                    foreach (var attributeKey in customExecutable.Value.Attribute.Keys.ToList())
                    {
                        ApplyNormalize(customExecutable.Value, attributeKey);
                    }

                    if (NeedSetCurrentProfileSettingsIniSet)
                    {
                        NeedSetCurrentProfileSettingsIniSet = false;

                        settingsIni = ManageIni.GetINIFile(ManageSettings.MoSelectedProfileSettingsPath);
                    }
                    if (!NeedSetCurrentProfileSettingsIniSet)
                    {
                        if (settingsIni.KeyExists(customExecutable.Value.Title, "custom_overwrites"))
                        {
                            customExecutable.Value.MoTargetMod = settingsIni.GetKey("custom_overwrites", customExecutable.Value.Title);
                        }
                    }
                }

            }

            /// <summary>
            /// keys frol custom executables list to remove while Save execution
            /// </summary>
            internal HashSet<string> listToRemoveKeys = new HashSet<string>();

            /// <summary>
            /// Save ini with changed customs
            /// </summary>
            internal void Save()
            {
                if (_ini == null)
                {
                    return;
                }

                bool changed = false;
                bool sectionCleared = _loadedListCustomsCount != List.Count || listToRemoveKeys.Count > 0;
                if (sectionCleared)
                {
                    changed = true;
                    _ini.ClearSection("customExecutables");
                }

                int customExecutableNumber = 0; // use new executable number when section was cleared and need to renumber executable numbers
                foreach (var customExecutable in List)
                {
                    if (sectionCleared)
                    {
                        if (listToRemoveKeys.Contains(customExecutable.Key)) // skip custom exe to be saved
                        {
                            changed = true;
                            continue;
                        }

                        customExecutableNumber++;
                    }

                    foreach (var attributeKey in customExecutable.Value.Attribute.Keys.ToList())
                    {
                        string keyName = (sectionCleared ? customExecutableNumber + "" : customExecutable.Key) + "\\" + attributeKey;

                        // normalize values
                        var valueBeforeNormalize = customExecutable.Value.Attribute[attributeKey];
                        ApplyNormalize(customExecutable.Value, attributeKey);

                        if (sectionCleared || customExecutable.Value.Attribute[attributeKey] != valueBeforeNormalize || !_ini.KeyExists(keyName, "customExecutables") || _ini.GetKey("customExecutables", keyName) != customExecutable.Value.Attribute[attributeKey]) // write only if not equal
                        {
                            changed = true;
                            _ini.SetKey("customExecutables", keyName, customExecutable.Value.Attribute[attributeKey]);
                        }
                    }

                    // set target mod if exists
                    if (!string.IsNullOrWhiteSpace(customExecutable.Value.MoTargetMod) && settingsIni.GetKey(customExecutable.Value.Title, "custom_overwrites") != customExecutable.Value.MoTargetMod)
                    {
                        settingsIni.SetKey("custom_overwrites", customExecutable.Value.Title, customExecutable.Value.MoTargetMod);
                    }
                }

                if (changed)
                {
                    _ini.SetKey("customExecutables", "size", (sectionCleared ? customExecutableNumber : List.Count) + "");
                }
            }

            /// <summary>
            /// apply normalization of values
            /// </summary>
            /// <param name="customExecutable"></param>
            /// <param name="attributeKey"></param>
            private static void ApplyNormalize(CustomExecutable customExecutable, string attributeKey)
            {
                switch (attributeKey)
                {
                    case "binary":
                    case "workingDirectory":
                        customExecutable.Attribute[attributeKey] = NormalizePath(customExecutable.Attribute[attributeKey]);
                        break;
                    case "arguments":
                        customExecutable.Attribute[attributeKey] = NormalizeArguments(customExecutable.Attribute[attributeKey]);
                        break;
                    case "toolbar":
                    case "ownicon":
                    case "hide":
                        customExecutable.Attribute[attributeKey] = NormalizeBool(customExecutable.Attribute[attributeKey]);
                        break;
                }
            }

            /// <summary>
            /// normalize boolean for mod organizer ini custom executable
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            protected static string NormalizeBool(string value)
            {
                if (!string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) && !string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
                {
                    //false if value not one of false or true
                    return "false";
                }

                return value.Trim();
            }

            /// <summary>
            /// normalize path for mod organizer ini custom executable
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            internal static string NormalizePath(string value)
            {
                return value.Replace('\\', '/');
            }

            /// <summary>
            /// normalize argumants for mod organizer ini custom executable
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            protected static string NormalizeArguments(string value)
            {
                value = value.Trim();

                if (string.IsNullOrEmpty(value) || value == "\\\"\\\"")
                {
                    return value;
                }

                int startIndex = 0;
                int endIndex = value.Length - 1;

                bool IsBackSlashOrQuote = false;
                for (int i = endIndex; i >= startIndex; i--)
                {
                    if (value[i] == '\\' || (!IsBackSlashOrQuote && value[i] == '\"'))
                    {
                        IsBackSlashOrQuote = !IsBackSlashOrQuote;

                        if (i == 0 && value[i] == '\"')
                        {
                            value = value.Insert(i, "\\");
                        }
                    }
                    else
                    {
                        if (IsBackSlashOrQuote)
                        {
                            value = value.Insert(i + 1, "\\");
                            IsBackSlashOrQuote = !IsBackSlashOrQuote;
                        }
                    }
                }

                var startsQuoted = value.StartsWith("\\\"", StringComparison.InvariantCulture);
                var endsQuoted = value.EndsWith("\\\"", StringComparison.InvariantCulture);

                return (startsQuoted ? "" : "\\\"") + value + (endsQuoted ? "" : "\\\"");
            }

            internal class CustomExecutable
            {
                /// <summary>
                /// target mod for mod organizer new created files of the exe file
                /// </summary>
                internal string MoTargetMod = "";

                /// <summary>
                /// <list type="s">
                ///     <listheader>
                ///         <description>List of possible attributes:</description>
                ///     </listheader>
                ///     <item>
                ///         <term>title</term>
                ///         <description>(Required!) title of custom executable</description>
                ///     </item>
                ///     <item>
                ///         <term>binary</term>
                ///         <description>(Required!) path to the exe</description>
                ///     </item>
                ///     <item>
                ///         <term>workingDirectory</term>
                ///         <description>(Optional) Working directory. By defaule willbe directory where is binary located.</description>
                ///     </item>
                ///     <item>
                ///         <term>arguments</term>
                ///         <description>(Optional) Arguments for binary. Empty by default.</description>
                ///     </item>
                ///     <item>
                ///         <term>toolbar</term>
                ///         <description>(Optional) Enable toolbar. False by default.</description>
                ///     </item>
                ///     <item>
                ///         <term>ownicon</term>
                ///         <description>(Optional) Use own icon for the exe, else will be icon of MO. False by default.</description>
                ///     </item>
                ///     <item>
                ///         <term>hide</term>
                ///         <description>(Optional) Hide the custom exe? False by default.</description>
                ///     </item>
                ///     <item>
                ///         <term>steamAppID</term>
                ///         <description>(Optional) Steam app id for binary. Empty by default.</description>
                ///     </item>
                /// </list>
                /// </summary>
                /// <example>
                ///     example of use:
                ///     <code>
                ///         customExecutable.attribute["title"] = "New custom exe"
                ///     </code>
                /// </example>
                internal Dictionary<string, string> Attribute = new Dictionary<string, string>()
                {
                    { "title" , null},
                    { "binary" , null},
                    { "workingDirectory" , string.Empty},
                    { "arguments" , string.Empty},
                    { "toolbar" , "false"},
                    { "ownicon" , "false"},
                    { "hide" , "false"},
                    { "steamAppID" , string.Empty},
                };


#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CA1822 // Mark members as static
                /// <summary>
                /// (Required!) title of custom executable
                /// </summary>
                internal string Title { get => Attribute["title"]; set => Attribute["title"] = value; }

                /// <summary>
                /// (Required!) path to the exe
                /// </summary>
                internal string Binary { get => Attribute["binary"]; set => Attribute["binary"] = NormalizePath(value); }

                /// <summary>
                /// (Optional) Working directory. By defaule willbe directory where is binary located.
                /// </summary>
                internal string WorkingDirectory { get => Attribute["workingDirectory"]; set => Attribute["workingDirectory"] = NormalizePath(value); }

                /// <summary>
                /// (Optional) Arguments for binary. Empty by default.
                /// </summary>
                internal string Arguments { get => Attribute["arguments"]; set => Attribute["arguments"] = NormalizeArguments(value); }

                /// <summary>
                /// (Optional) Enable toolbar. False by default.
                /// </summary>
                internal string Toolbar { get => Attribute["toolbar"]; set => Attribute["toolbar"] = NormalizeBool(value); }

                /// <summary>
                /// (Optional) Use own icon for the exe, else will be icon of MO. False by default.
                /// </summary>
                internal string Ownicon { get => Attribute["ownicon"]; set => Attribute["ownicon"] = NormalizeBool(value); }

                /// <summary>
                /// (Optional) Hide the custom exe? False by default.
                /// </summary>
                internal string Hide { get => Attribute["hide"]; set => Attribute["hide"] = NormalizeBool(value); }

                /// <summary>
                /// (Optional) Steam app id for binary. Empty by default.
                /// </summary>
                internal string SteamAppId { get => Attribute["steamAppID"]; set => Attribute["steamAppID"] = value; }

#pragma warning restore CA1822 // Mark members as static
#pragma warning restore IDE1006 // Naming Styles
            }
        }

        //private static void SetCustomExecutablesIniValues(INIFile INI)
        //{
        //    if (ManageSettings.SetModOrganizerINISettingsForTheGame)
        //    {
        //        return;
        //    }

        //    ManageSettings.SetModOrganizerINISettingsForTheGame = true;

        //    string[,] IniValues = new string[,] { };

        //    if (!ManageSettings.MOIsNew)
        //    {
        //        IniValues = new string[,]
        //            {
        //            //customExecutables
        //            {
        //                ManageSettings.GetCurrentGameEXEName()
        //                ,
        //                @"1\title"
        //            }
        //        ,
        //            {
        //                Path.Combine(ManageSettings.GetCurrentGameDataPath(), ManageSettings.GetCurrentGameEXEName() + ".exe")
        //                ,
        //                @"1\binary"
        //            }
        //        ,
        //            {
        //                ManageSettings.GetCurrentGameDataPath()
        //                ,
        //                @"1\workingDirectory"
        //            }
        //        ,
        //            {
        //                "true"
        //                ,
        //                @"1\ownicon"
        //            }
        //        ,
        //            {
        //                ManageSettings.GetINISettingsEXEName()
        //                ,
        //                @"2\title"
        //            }
        //        ,
        //            {
        //                ManageSettings.GetSettingsEXEPath()
        //                ,
        //                @"2\binary"
        //            }
        //        ,
        //            {
        //                ManageSettings.GetCurrentGameDataPath()
        //                ,
        //                @"2\workingDirectory"
        //            }
        //        ,
        //            {
        //                "true"
        //                ,
        //                @"2\ownicon"
        //            }
        //        //,
        //        //    {
        //        //        "Explore Virtual Folder"
        //        //        ,
        //        //        @"3\title"
        //        //    }
        //        //,
        //        //    {
        //        //        Path.Combine(SettingsManage.GetMOdirPath(), "explorer++", "Explorer++.exe")
        //        //        ,
        //        //        @"3\binary"
        //        //    }
        //        //,
        //        //    {
        //        //        SettingsManage.GetDataPath()
        //        //        ,
        //        //        @"3\arguments"
        //        //    }
        //        //,
        //        //    {
        //        //        Path.Combine(SettingsManage.GetMOdirPath(), "explorer++")
        //        //        ,
        //        //        @"3\workingDirectory"
        //        //    }
        //        //,
        //        //    {
        //        //        "true"
        //        //        ,
        //        //        @"3\ownicon"
        //        //    }
        //        //,
        //        //    {
        //        //        "Skyrim"
        //        //        ,
        //        //        @"4\title"
        //        //    }
        //        //,
        //        //    {
        //        //        Path.Combine(SettingsManage.GetDataPath(), SettingsManage.GetCurrentGameEXEName() + ".exe")
        //        //        ,
        //        //        @"4\binary"
        //        //    }
        //        //,
        //        //    {
        //        //        SettingsManage.GetDataPath()
        //        //        ,
        //        //        @"4\workingDirectory"
        //        //    }
        //        ,
        //            {
        //                Path.Combine(ManageSettings.GetStudioEXEName())
        //                ,
        //                @"3\title"
        //            }
        //        ,
        //            {
        //                Path.Combine(ManageSettings.GetCurrentGameDataPath(),ManageSettings.GetStudioEXEName()+".exe")
        //                ,
        //                @"3\binary"
        //            }
        //        ,
        //            {
        //                ManageSettings.GetCurrentGameDataPath()
        //                ,
        //                @"3\workingDirectory"
        //            }
        //        ,
        //            {
        //                "true"
        //                ,
        //                @"3\ownicon"
        //            }
        //        };
        //    }


        //    Dictionary<string, string> IniValuesDict = new Dictionary<string, string>();

        //    string[,] iniParameters =
        //        {
        //            {
        //               "title"
        //               ,
        //               "rs"
        //            }
        //        ,
        //            {
        //                "binary"
        //            ,
        //                "rs"
        //            }
        //        ,
        //            {
        //                "workingDirectory"
        //            ,
        //                "rs"
        //            }
        //        ,
        //            {
        //                "arguments"
        //            ,
        //                "s"
        //            }
        //        ,
        //            {
        //                "toolbar"
        //            ,
        //                "b"
        //            }
        //        ,
        //            {
        //                "ownicon"
        //            ,
        //                "b"
        //            }
        //        ,
        //            {
        //                "steamAppID"
        //            ,
        //                "s"
        //            }
        //        ,
        //            {
        //                "hide"
        //            ,
        //                "b"
        //            }
        //    };

        //    int resultindex = 0;//конечное количество добавленных exe
        //    if (!ManageSettings.MOIsNew)
        //    {
        //        //заполнить словарь значениями массива строк
        //        resultindex = 1;//конечное количество добавленных exe
        //        int IniValuesLength = IniValues.Length / 2;//длина массива, деленая на 2 т.к. каждый элемент состоит из двух
        //        int exclude = 0;//будет равен индексу, который надо пропустить
        //        int skippedCnt = 0;//счет количества пропущенных, нужно для правильного подсчета resultindex
        //        for (int i = 0; i < IniValuesLength; i++)
        //        {
        //            var curindex = int.Parse(IniValues[i, 1].Split(new string[1] { @"\" }, StringSplitOptions.None)[0], CultureInfo.InvariantCulture);
        //            var parameterName = IniValues[i, 1].Split(new string[1] { @"\" }, StringSplitOptions.None)[1];

        //            if (curindex == exclude)
        //            {
        //                continue;
        //            }
        //            else if (exclude != 0)
        //            {
        //                exclude = 0;
        //            }
        //            //Если название пустое, то пропускать все значения с таким индексом
        //            if (parameterName == "title" && IniValues[i, 0].Length == 0)
        //            {
        //                exclude = curindex;
        //                skippedCnt++;
        //                continue;
        //            }

        //            if (curindex - skippedCnt > resultindex && exclude == 0)
        //            {
        //                resultindex++;
        //            }

        //            var ResultparameterName = resultindex + @"\" + parameterName;

        //            if (!IniValuesDict.ContainsKey(ResultparameterName))
        //            {
        //                IniValuesDict.Add(ResultparameterName, IniValues[i, 0]);
        //            }
        //        }
        //    }

        //    var ExecutablesCount = resultindex;
        //    //try
        //    //{
        //    //    ExecutablesCount = int.Parse(IniValues[IniValuesLength - 1, 1].Split(new string[1] { @"\" }, StringSplitOptions.None)[0]);
        //    //}
        //    //catch
        //    //{
        //    //    return;//если не число, выйти
        //    //}

        //    AddCustomExecutables(INI, out Dictionary<string, string> customExecutables, ref ExecutablesCount);

        //    if (customExecutables.Count > 0)
        //    {
        //        //очистка секции
        //        INI.ClearSection("customExecutables", false);
        //        foreach (var pair in customExecutables)
        //        {
        //            if (!IniValuesDict.ContainsKey(pair.Key))
        //            {
        //                IniValuesDict.Add(pair.Key, pair.Value);
        //            }
        //        }

        //        int cnt = 1;
        //        int iniParametersLength = iniParameters.Length / 2;
        //        while (cnt <= ExecutablesCount)
        //        {
        //            for (int i = 0; i < iniParametersLength; i++)
        //            {
        //                string key = cnt + @"\" + iniParameters[i, 0];
        //                string IniValue;
        //                bool keyExists = IniValuesDict.ContainsKey(key);
        //                string subquote = key.EndsWith(@"\arguments", StringComparison.InvariantCulture) && keyExists ? "\\\"" : string.Empty;
        //                if (keyExists)
        //                {
        //                    IniValue = subquote + IniValuesDict[key].Replace(@"\", key.EndsWith(@"\arguments", StringComparison.InvariantCulture) ? @"\\" : "/") + subquote;
        //                }
        //                else
        //                {
        //                    IniValue = subquote + (iniParameters[i, 1].EndsWith("b", StringComparison.InvariantCulture) ? "false" : string.Empty) + subquote;
        //                }

        //                INI.WriteINI("customExecutables", key, IniValue, false);
        //            }
        //            cnt++;
        //        }

        //        //Hardcoded exe Game exe and Explorer++
        //        //ExecutablesCount += 2;

        //        INI.WriteINI("customExecutables", "size", ExecutablesCount.ToString(CultureInfo.InvariantCulture));
        //    }
        //    else
        //    {
        //        INI.SaveINI(true, true);
        //    }

        //    ManageSettings.SetModOrganizerINISettingsForTheGame = false;
        //}

        /// <summary>
        /// true if MO have base plugin
        /// </summary>
        /// <returns></returns>
        internal static bool IsMo23OrNever()
        {
            var ver = GetMoVersion();
            return ver != null && ver.Length > 2 && ver[0] == '2' && int.Parse(ver[2].ToString(), CultureInfo.InvariantCulture) > 2;
        }

        //private static void AddCustomExecutables(INIFile INI, out Dictionary<string, string> customExecutables, ref int ExecutablesCount)
        //{
        //    customExecutables = new Dictionary<string, string>();

        //    var exists = INI.ReadSectionValuesToArray("customExecutables");

        //    var exeEqual = 0;
        //    var OneexeNotEqual = false;
        //    var executablesCount = ExecutablesCount;
        //    Dictionary<string, string> customs = new Dictionary<string, string>();

        //    if (!ManageSettings.MOIsNew)
        //    {
        //        var CurrentGame = ManageSettings.Games.CurrentGame;
        //        if (CurrentGame.GetGameEXENameX32().Length > 0 && File.Exists(Path.Combine(ManageSettings.GetCurrentGameDataPath(), CurrentGame.GetGameEXENameX32() + ".exe")))
        //        {
        //            executablesCount++;
        //            exeEqual++;
        //            customs.Add(executablesCount + @"\title", CurrentGame.GetGameEXENameX32());
        //            customs.Add(executablesCount + @"\binary", Path.Combine(ManageSettings.GetCurrentGameDataPath(), CurrentGame.GetGameEXENameX32() + ".exe"));
        //            customs.Add(executablesCount + @"\workingDirectory", ManageSettings.GetCurrentGameDataPath());
        //        }
        //        if (CurrentGame.GetGameStudioEXENameX32().Length > 0 && File.Exists(Path.Combine(ManageSettings.GetCurrentGameDataPath(), CurrentGame.GetGameStudioEXENameX32() + ".exe")))
        //        {
        //            executablesCount++;
        //            customs.Add(executablesCount + @"\title", CurrentGame.GetGameStudioEXENameX32());
        //            customs.Add(executablesCount + @"\binary", Path.Combine(ManageSettings.GetCurrentGameDataPath(), CurrentGame.GetGameStudioEXENameX32() + ".exe"));
        //            customs.Add(executablesCount + @"\workingDirectory", ManageSettings.GetCurrentGameDataPath());
        //        }
        //    }
        //    else
        //    {
        //        foreach (var value in ManagePython.GetExecutableInfosFromPyPlugin())
        //        {
        //            if (string.IsNullOrWhiteSpace(value.Key) || !File.Exists(value.Value))
        //            {
        //                continue;
        //            }

        //            executablesCount++;
        //            customs.Add(executablesCount + @"\title", value.Key);
        //            customs.Add(executablesCount + @"\binary", value.Value);
        //            customs.Add(executablesCount + @"\arguments", string.Empty);
        //            customs.Add(executablesCount + @"\workingDirectory", Path.GetDirectoryName(value.Value));
        //            customs.Add(executablesCount + @"\ownicon", "true");
        //        }
        //    }

        //    string[] pathExclusions =
        //        {
        //        "BepInEx" + Path.DirectorySeparatorChar + "plugins",
        //        //"Lec.ExtProtocol",
        //        //"ezTransXP.ExtProtocol",
        //        ".ExtProtocol",
        //        //"Common.ExtProtocol.Executor",
        //        "UnityCrashHandler64",
        //        Path.DirectorySeparatorChar + "IPA",
        //        "WideSliderPatch"
        //        };

        //    {
        //        //Добавление exe из Data
        //        //Parallel.ForEach(Directory.EnumerateFileSystemEntries(ManageSettings.GetDataPath(), "*.exe", SearchOption.AllDirectories),
        //        //    exePath =>
        //        //    {
        //        //        try
        //        //        {
        //        //            if (exeEqual > 9)
        //        //            {
        //        //                return;
        //        //            }

        //        //            var exeName = Path.GetFileNameWithoutExtension(exePath);
        //        //            if (exeName.Length > 0 && !customs.Values.Contains(exeName) && !ManageStrings.IsStringAContainsAnyStringFromStringArray(exePath, pathExclusions, true))
        //        //            {
        //        //                executablesCount++;
        //        //                if (!OneexeNotEqual)
        //        //                {
        //        //                    if (exists.Contains(exePath))
        //        //                    {
        //        //                        exeEqual++;
        //        //                    }
        //        //                    else
        //        //                    {
        //        //                        exeEqual = 0;
        //        //                        OneexeNotEqual = true;
        //        //                    }
        //        //                }

        //        //                customs.Add(executablesCount + @"\title", exeName);
        //        //                customs.Add(executablesCount + @"\binary", exePath);
        //        //            }
        //        //        }
        //        //        catch { }

        //        //    });
        //    }

        //    foreach (var exePath in Directory.EnumerateFiles(ManageSettings.GetCurrentGameDataPath(), "*.exe", SearchOption.AllDirectories))
        //    {
        //        try
        //        {
        //            if (exeEqual > 9)
        //            {
        //                return;
        //            }

        //            var exeName = Path.GetFileNameWithoutExtension(exePath);
        //            if (exeName.Length > 0 && !customs.Values.Contains(exeName) && Path.GetDirectoryName(exePath) != ManageSettings.GetCurrentGameDataPath() && !ManageStrings.IsStringAContainsAnyStringFromStringArray(exePath, pathExclusions, true))
        //            {
        //                executablesCount++;
        //                if (!OneexeNotEqual)
        //                {
        //                    if (exists.Contains(exePath))
        //                    {
        //                        exeEqual++;
        //                    }
        //                    else
        //                    {
        //                        exeEqual = 0;
        //                        OneexeNotEqual = true;
        //                    }
        //                }

        //                customs.Add(executablesCount + @"\title", exeName);
        //                customs.Add(executablesCount + @"\binary", exePath);
        //            }
        //        }
        //        catch { }
        //    }

        //    {
        //        //foreach (var exePath in Directory.EnumerateFiles(ManageSettings.GetDataPath(), "*.exe", SearchOption.AllDirectories))
        //        //{
        //        //    string exeName = Path.GetFileNameWithoutExtension(exePath);
        //        //    if (exeName.Length > 0 && !customExecutables.Values.Contains(exeName) && !ManageStrings.IsStringAContainsAnyStringFromStringArray(exePath, pathExclusions, true))
        //        //    {
        //        //        ExecutablesCount++;
        //        //        customExecutables.Add(ExecutablesCount + @"\title", exeName);
        //        //        customExecutables.Add(ExecutablesCount + @"\binary", exePath);
        //        //    }
        //        //}
        //        //Добавление exe из Mods
        //        //Parallel.ForEach(Directory.EnumerateFileSystemEntries(ManageSettings.GetModsPath(), "*.exe", SearchOption.AllDirectories),
        //        //    exePath =>
        //        //    {
        //        //        try
        //        //        {
        //        //            if (exeEqual > 9)
        //        //            {
        //        //                return;
        //        //            }

        //        //            string exeName = Path.GetFileNameWithoutExtension(exePath);
        //        //            if (exeName.Length > 0 && !customs.Values.Contains(exeName) && !ManageStrings.IsStringAContainsAnyStringFromStringArray(exePath, pathExclusions, true))
        //        //            {
        //        //                executablesCount++;
        //        //                if (!OneexeNotEqual)
        //        //                {
        //        //                    if (exists.Contains(exePath))
        //        //                    {
        //        //                        exeEqual++;
        //        //                    }
        //        //                    else
        //        //                    {
        //        //                        exeEqual = 0;
        //        //                        OneexeNotEqual = true;
        //        //                    }
        //        //                }

        //        //                customs.Add(executablesCount + @"\title", exeName);
        //        //                customs.Add(executablesCount + @"\binary", exePath);
        //        //            }
        //        //        }
        //        //        catch { }

        //        //    });
        //        //foreach (var exePath in Directory.EnumerateFiles(ManageSettings.GetModsPath(), "*.exe", SearchOption.AllDirectories))
        //        //{
        //        //    string exeName = Path.GetFileNameWithoutExtension(exePath);
        //        //    if (Path.GetFileNameWithoutExtension(exePath).Length > 0 && !customExecutables.Values.Contains(exeName) && !ManageStrings.IsStringAContainsAnyStringFromStringArray(exePath, pathExclusions, true))
        //        //    {
        //        //        ExecutablesCount++;
        //        //        customExecutables.Add(ExecutablesCount + @"\title", exeName);
        //        //        customExecutables.Add(ExecutablesCount + @"\binary", exePath);
        //        //    }
        //        //}
        //    }

        //    foreach (var exePath in Directory.EnumerateFiles(ManageSettings.GetCurrentGameModsPath(), "*.exe", SearchOption.AllDirectories))
        //    {
        //        try
        //        {
        //            if (exeEqual > 9)
        //            {
        //                return;
        //            }

        //            string exeName = Path.GetFileNameWithoutExtension(exePath);
        //            if (exeName.Length > 0 && !customs.Values.Contains(exeName) && !ManageStrings.IsStringAContainsAnyStringFromStringArray(exePath, pathExclusions, true))
        //            {
        //                executablesCount++;
        //                if (!OneexeNotEqual)
        //                {
        //                    if (exists.Contains(exePath))
        //                    {
        //                        exeEqual++;
        //                    }
        //                    else
        //                    {
        //                        exeEqual = 0;
        //                        OneexeNotEqual = true;
        //                    }
        //                }

        //                customs.Add(executablesCount + @"\title", exeName);
        //                customs.Add(executablesCount + @"\binary", exePath);
        //            }
        //        }
        //        catch { }
        //    }

        //    //добавление hardcoded exe
        //    //executablesCount++;
        //    //customs.Add(executablesCount + @"\title", "Skyrim");
        //    //customs.Add(executablesCount + @"\binary", Path.Combine(ManageSettings.ApplicationStartupPath, "TESV.exe"));
        //    //customs.Add(executablesCount + @"\workingDirectory", ManageSettings.ApplicationStartupPath);
        //    //customs.Add(executablesCount + @"\ownicon", "true");
        //    //executablesCount++;
        //    //customs.Add(executablesCount + @"\title", "Explore Virtual Folder");
        //    //customs.Add(executablesCount + @"\binary", Path.Combine(ManageSettings.GetMOdirPath(), "explorer++", "Explorer++.exe"));
        //    //customs.Add(executablesCount + @"\workingDirectory", Path.Combine(ManageSettings.GetMOdirPath(), "explorer++"));
        //    //customs.Add(executablesCount + @"\arguments", ManageSettings.GetCurrentGameDataPath());
        //    //customs.Add(executablesCount + @"\ownicon", "true");

        //    customExecutables = customs;
        //    ExecutablesCount = executablesCount;
        //}

        private static void SetCommonIniValues(INIFile ini)
        {
            {
                //string[,] iniValuesOld =
                //    {
                //        //General
                //        {
                //            "@ByteArray("+ManageSettings.GetCurrentGamePath()+")"
                //            ,
                //            "General"
                //            ,
                //            "gamePath"
                //        }
                //    //,
                //    //    {
                //    //        GetSelectedProfileName()
                //    //        ,
                //    //        "General"
                //    //        ,
                //    //        "@ByteArray("+"selected_profile"+")"
                //    //    }


                //    //,
                //    //    {
                //    //        "1"
                //    //        ,
                //    //        "General"
                //    //        ,
                //    //        @"selected_executable"
                //    //    }


                //    //,
                //    //    //customExecutables
                //    //    {
                //    //        SettingsManage.GetCurrentGameEXEName()
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"1\title"
                //    //    }
                //    //,
                //    //    {
                //    //        Path.Combine(SettingsManage.GetDataPath(), SettingsManage.GetCurrentGameEXEName() + ".exe")
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"1\binary"
                //    //    }
                //    //,
                //    //    {
                //    //        SettingsManage.GetDataPath()
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"1\workingDirectory"
                //    //    }
                //    //,
                //    //    {
                //    //        SettingsManage.GetINISettingsEXEName()
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"2\title"
                //    //    }
                //    //,
                //    //    {
                //    //        SettingsManage.GetSettingsEXEPath()
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"2\binary"
                //    //    }
                //    //,
                //    //    {
                //    //        SettingsManage.GetDataPath()
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"2\workingDirectory"
                //    //    }
                //    //,
                //    //    {
                //    //        Path.Combine(SettingsManage.GetMOdirPath(), "explorer++", "Explorer++.exe")
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"3\binary"
                //    //    }
                //    //,
                //    //    {
                //    //        SettingsManage.GetDataPath()
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"3\arguments"
                //    //    }
                //    //,
                //    //    {
                //    //        Path.Combine(SettingsManage.GetMOdirPath(), "explorer++")
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"3\workingDirectory"
                //    //    }
                //    //,
                //    //    {
                //    //        Path.Combine(SettingsManage.GetCurrentGamePath(), "TESV.exe")
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"4\binary"
                //    //    }
                //    //,
                //    //    {
                //    //        SettingsManage.GetCurrentGamePath()
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"4\workingDirectory"
                //    //    }
                //    //,
                //    //    {
                //    //        Path.Combine(SettingsManage.GetStudioEXEName())
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"5\title"
                //    //    }
                //    //,
                //    //    {
                //    //        Path.Combine(SettingsManage.GetDataPath(),SettingsManage.GetStudioEXEName()+".exe")
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"5\binary"
                //    //    }
                //    //,
                //    //    {
                //    //        SettingsManage.GetDataPath()
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"5\workingDirectory"
                //    //    }
                //    ,
                //        //Settings
                //        {
                //            ManageSettings.GetCurrentGameModsPath()
                //            ,
                //            "Settings"
                //            ,
                //            @"mod_directory"
                //        }
                //    ,
                //        {
                //            Path.Combine(ManageSettings.GetCurrentGamePath(), "Downloads")
                //            ,
                //            "Settings"
                //            ,
                //            @"download_directory"
                //        }
                //    ,
                //        {
                //            Path.Combine(ManageSettings.GetCurrentGameMOPath(), "profiles")
                //            ,
                //            "Settings"
                //            ,
                //            @"profiles_directory"
                //        }
                //    ,
                //        {
                //            Path.Combine(ManageSettings.GetCurrentGameMOPath(), "overwrite")
                //            ,
                //            "Settings"
                //            ,
                //            @"overwrite_directory"
                //        }
                //    ,
                //        {
                //            CultureInfo.InvariantCulture.Name.Split('-')[0]
                //            ,
                //            "Settings"
                //            ,
                //            @"language"
                //        }

                //    };
            }

            string[,] iniValues =
                {
                    //General
                    {
                        "@ByteArray("+ManageSettings.CurrentGameDirPath+")"
                        ,
                        "General"
                        ,
                        "gamePath"
                    }
                ,
                    //Settings
                    {
                        ManageSettings.CurrentGameModsDirPath
                        ,
                        "Settings"
                        ,
                        "mod_directory"
                    }
                ,
                    {
                        Path.Combine(ManageSettings.CurrentGameDirPath, "Downloads")
                        ,
                        "Settings"
                        ,
                        "download_directory"
                    }
                ,
                    {
                        Path.Combine(ManageSettings.CurrentGameModOrganizerDirPath, "profiles")
                        ,
                        "Settings"
                        ,
                        "profiles_directory"
                    }
                ,
                    {
                        Path.Combine(ManageSettings.CurrentGameModOrganizerDirPath, "overwrite")
                        ,
                        "Settings"
                        ,
                        "overwrite_directory"
                    }
                ,
                    {
                        CultureInfo.CurrentCulture.Name.Split('-')[0]
                        ,
                        "Settings"
                        ,
                        "language"
                    }
                ,
                    {
                        "false"
                        ,
                        "Settings"
                        ,
                        "check_for_updates" // disable check for updates by mo to prevent autoupdate and possible data lost. Update button will make buckups before update and will update more safe.
                    }

                };

            int iniValuesLength = iniValues.Length / 3;

            bool changed = false;
            for (int i = 0; i < iniValuesLength; i++)
            {
                string subquote = iniValues[i, 2].EndsWith(@"\arguments", StringComparison.InvariantCulture) ? "\\\"" : string.Empty;
                string keyValue = iniValues[i, 0].Replace(@"\", @"\\") + subquote;
                string sectionName = iniValues[i, 1];
                string keyName = iniValues[i, 2];

                if (ini.GetKey(sectionName, keyName) != keyValue)
                {
                    changed = true;
                    ini.SetKey(sectionName, keyName, keyValue, false);
                }
            }

            if (changed)
            {
                ini.WriteFile();
            }
        }

        //internal static Dictionary<string, string> GetMOcustomExecutablesList()
        //{
        //    var INI = ManageIni.GetINIFile(ManageSettings.GetMOiniPath());
        //    var customs = INI.ReadSectionKeyValuePairsToDictionary("customExecutables");
        //    var retDict = new Dictionary<string, string>();
        //    foreach (var pair in customs)
        //    {
        //        if (pair.Key.EndsWith(@"\title", StringComparison.InvariantCulture))
        //        {
        //            retDict.Add(pair.Value, customs[pair.Key.Split('\\')[0] + @"\binary"]);
        //        }
        //    }
        //    return retDict;
        //}

        /// <summary>
        /// get title of custom executable by it exe name
        /// </summary>
        /// <param name="exename"></param>
        /// <param name="ini"></param>
        /// <returns></returns>
        internal static string GetMOcustomExecutableTitleByExeName(string exename, INIFile ini = null, bool newMethod = true)
        {
            if (ini == null)
            {
                ini = ManageIni.GetINIFile(ManageSettings.AppMOiniFilePath);
            }

            if (newMethod)
            {
                var customs = new CustomExecutables(ini);
                foreach (var customExe in customs.List)
                {
                    if (Path.GetFileNameWithoutExtension(customExe.Value.Binary) == exename)
                    {
                        if (File.Exists(customExe.Value.Binary))
                        {
                            return customExe.Value.Title;
                        }
                    }
                }
            }
            else
            {
                var customs = ini.GetSectionValuesToDictionary("customExecutables");
                foreach (var pair in customs)
                {
                    if (pair.Key.EndsWith(@"\binary", StringComparison.InvariantCulture))
                        if (Path.GetFileNameWithoutExtension(pair.Value) == exename)
                            if (File.Exists(pair.Value))
                            {
                                return customs[pair.Key.Split('\\')[0] + @"\title"];
                            }
                }
            }

            return exename;
        }

        /// <summary>
        /// inserts in MO.ini new custom executable
        /// required exeParams 0 is exe title, required exeParams 1 is exe bynary path
        /// optional exeParams 2 is arguments, optional exeParams 4 is working directory
        /// </summary>
        /// <param name="newCustomExecutable"></param>
        internal static void InsertCustomExecutable(CustomExecutables.CustomExecutable newCustomExecutable, INIFile ini = null, bool insertOnlyMissingBinary = true)
        {
            if (ini == null)
            {
                ini = ManageIni.GetINIFile(ManageSettings.AppMOiniFilePath);
            }

            var customExcutables = new CustomExecutables(ini);
            string customToUpdate = "";
            if (insertOnlyMissingBinary)
            {
                foreach (var exe in customExcutables.List)
                {
                    if (exe.Value.Binary == newCustomExecutable.Binary && exe.Value.Title == newCustomExecutable.Title) // return if exe found and have same title
                    {
                        customToUpdate = exe.Key; // memorize found custom exe number
                        break;
                    }
                }
            }

            if (customToUpdate.Length == 0)
            {
                customExcutables.List.Add(customExcutables.List.Count + 1 + "", newCustomExecutable);
            }
            else
            {
                // update same record from input instead of add new
                foreach (var attributeName in customExcutables.List[customToUpdate].Attribute.Keys.ToList())
                {
                    if (customExcutables.List[customToUpdate].Attribute[attributeName] != newCustomExecutable.Attribute[attributeName])
                    {
                        customExcutables.List[customToUpdate].Attribute[attributeName] = newCustomExecutable.Attribute[attributeName];
                    }
                }
            }

            customExcutables.Save();
        }

        /// <summary>
        /// get MO custom executables count
        /// </summary>
        /// <param name="customs"></param>
        /// <returns></returns>
        internal static int GetMOiniCustomExecutablesCount(Dictionary<string, string> customs = null)
        {
            customs = customs ?? ManageIni.GetINIFile(ManageSettings.AppMOiniFilePath).GetSectionValuesToDictionary("customExecutables");

            if (customs.Count == 0)//check if caustoms is exists
            {
                return 0;
            }

            int ind = 1;
            if (!customs.ContainsKey(ind + @"\binary"))//return 0 if there is no binary in list
            {
                return 0;
            }

            while (customs.ContainsKey(ind + @"\binary"))//iterate while binary with index is exist
            {
                ind++;
            }
            return ind - 1;
        }

        /// <summary>
        /// returns true if program custom exe is exists in MO list
        /// </summary>
        /// <param name="exename"></param>
        /// <returns></returns>
        internal static bool IsMOcustomExecutableTitleByExeNameExists(string exename)
        {
            var customs = ManageIni.GetINIFile(ManageSettings.AppMOiniFilePath).GetSectionValuesToDictionary("customExecutables");
            foreach (var pair in customs)
            {
                if (pair.Key.EndsWith(@"\binary", StringComparison.InvariantCulture))
                    if (Path.GetFileNameWithoutExtension(pair.Value) == exename)
                        if (File.Exists(pair.Value))
                        {
                            return true;
                        }
            }
            return false;
        }

        public static void ActivateMod(string modname)
        {
            ActivateDeactivateInsertMod(modname);
        }

        /// <summary>
        /// check symlink for usedata dir when target is not exists or symlink is not valid. overwrite for mo mode and data for common mode
        /// </summary>
        internal static void CheckMoUserdata()
        {
            try
            {
                DirectoryInfo objectDir;
                //overwrite dir of mo folder for mo mode and data folder for common mode
                if (ManageSettings.IsMoMode)
                {
                    objectDir = new DirectoryInfo(ManageSettings.CurrentGameMoOverwritePath);
                }
                else
                {
                    objectDir = new DirectoryInfo(ManageSettings.CurrentGameDataDirPath);
                }

                //create target object dir when it is not exists
                if (!objectDir.Exists)
                {
                    objectDir.Create();
                }

                //symlink path for MOUserData in game's dir
                DirectoryInfo symlinkPath = new DirectoryInfo(ManageSettings.OverwriteFolderLink);

                //delete if target is not exists or not symlink and recreate
                if (!symlinkPath.IsValidSymlinkTargetEquals(objectDir.FullName))
                {
                    if (symlinkPath.Exists)
                    {
                        symlinkPath.Delete();
                    }

                    objectDir.CreateSymlink(symlinkPath.FullName, true);
                }
            }
            catch (Exception ex)
            {
                _log.Error("An error occured in CheckMoUserdata. error:" + ex);
            }
        }

        public static void DeactivateMod(string modname)
        {
            ActivateDeactivateInsertMod(modname, false);
        }

        /// <summary>
        /// Insert <paramref name="modname"/> in the current mod organizer's profile's modlist.txt.
        /// If <paramref name="modAfterWhichInsert"/> is not set new <paramref name="modname"/> will be placed at the end of modlist.
        /// </summary>
        /// <param name="modname"></param>
        /// <param name="activate">If need the <paramref name="modname"/> was activated in modlist</param>
        /// <param name="modAfterWhichInsert">Name of mod after which must be inserted else will me inserted in the end od list.</param>
        /// <param name="placeAfter"> Will determine before or after of <paramref name="modAfterWhichInsert"/> new <paramref name="modname"/> must be inserted</param>
        public static void InsertMod(string modname, bool activate = true, string modAfterWhichInsert = "", bool placeAfter = true)
        {
            ActivateDeactivateInsertMod(modname, activate, modAfterWhichInsert, placeAfter);
        }

        /// <summary>
        /// Insert <paramref name="modname"/> in the current mod organizer's profile's modlist.txt.
        /// </summary>
        /// <param name="modname"></param>
        /// <param name="activate">If need the <paramref name="modname"/> was activated in modlist</param>
        /// <param name="recordWithWhichInsert">Name of mod after which must be inserted else will me inserted in the end od list.</param>
        /// <param name="placeAfter"> Will determine before or after of <paramref name="recordWithWhichInsert"/> new <paramref name="modname"/> must be inserted</param>
        public static void ActivateDeactivateInsertMod(string modname, bool activate = true, string recordWithWhichInsert = "", bool placeAfter = true)
        {
            // insert new
            var modList = new ModlistProfileInfo();

            // just activate/deactivate if exists
            if (modList.ItemByName.ContainsKey(modname))
            {
                modList.ItemByName[modname].IsEnabled = activate;
                modList.Save();
                return;
            }

            var record = new ProfileModlistRecord
            {
                IsEnabled = activate,
                IsSeparator = false,
                Name = modname,
                Path = Path.Combine(ManageSettings.CurrentGameModsDirPath, modname)
            };
            record.IsExist = Directory.Exists(record.Path);


            if (!string.IsNullOrWhiteSpace(recordWithWhichInsert)
                && recordWithWhichInsert.EndsWith("_separator", comparisonType: StringComparison.InvariantCulture))
            {
                record.ParentSeparator = new ProfileModlistRecord
                {
                    IsSeparator = true,
                    Name = recordWithWhichInsert,
                    Path = Path.Combine(ManageSettings.CurrentGameModsDirPath, recordWithWhichInsert)
                };
                record.ParentSeparator.IsExist = Directory.Exists(record.ParentSeparator.Path);
            }

            modList.Insert(record, (record.ParentSeparator == null && !string.IsNullOrWhiteSpace(recordWithWhichInsert) ? recordWithWhichInsert : ""), placeAfter);
            modList.Save();

            //if (modname.Length > 0)
            //{
            //    string currentMOprofile = ManageSettings.GetMoSelectedProfileDirName();

            //    if (currentMOprofile.Length == 0)
            //    {
            //    }
            //    else
            //    {
            //        string profilemodlistpath = ManageSettings.GetCurrentMoProfileModlistPath();

            //        InsertLineInModlistFile(profilemodlistpath, (activate ? "+" : "-") + modname, 1, recordWithWhichInsert, placeAfter);
            //    }
            //}
        }

        //https://social.msdn.microsoft.com/Forums/vstudio/en-US/8f713e50-0789-4bf6-865f-c87cdebd0b4f/insert-line-to-text-file-using-streamwriter-using-csharp?forum=csharpgeneral
        /// <summary>
        /// Inserts line in file in set position
        /// </summary>
        /// <param name="moModlistPath"></param>
        /// <param name="line"></param>
        /// <param name="position"></param>
        public static void InsertLineInModlistFile(string moModlistPath, string line, int position = 1, string insertWithThisMod = "", bool placeAfter = true)
        {
            if (moModlistPath.Length > 0 && File.Exists(moModlistPath) && line.Length > 0)
            {
                string[] fileLines = File.ReadAllLines(moModlistPath);
                bool isEnabled;
                if (!(isEnabled = fileLines.Contains("+" + line.Remove(0, 1))) && !fileLines.Contains("-" + line.Remove(0, 1)))
                {
                    int fileLinesLength = fileLines.Length;
                    bool insertWithMod = insertWithThisMod.Length > 0;
                    position = insertWithMod ? fileLinesLength : position;
                    using (StreamWriter writer = new StreamWriter(moModlistPath))
                    {
                        for (int lineNumber = 0; lineNumber < position; lineNumber++)
                        {
                            if (insertWithMod && fileLines[lineNumber].Length > 0 && string.Compare(fileLines[lineNumber].Remove(0, 1), insertWithThisMod, true, CultureInfo.InvariantCulture) == 0)
                            {
                                if (placeAfter)
                                {
                                    position = lineNumber;
                                }
                                else
                                {
                                    writer.WriteLine(fileLines[lineNumber]);
                                    position = lineNumber + 1;
                                }
                                break;

                            }
                            else
                            {
                                writer.WriteLine(fileLines[lineNumber]);
                            }
                        }

                        writer.WriteLine(line);

                        if (position < fileLinesLength)
                        {
                            for (int lineNumber = position; lineNumber < fileLinesLength; lineNumber++)
                            {
                                writer.WriteLine(fileLines[lineNumber]);
                            }
                        }
                    }
                }
                else
                {
                    var prefix = "-";
                    if (isEnabled)
                    {
                        prefix = "+";
                    }
                    fileLines = fileLines.Replace(prefix + line.Remove(0, 1), line);
                    File.WriteAllLines(moModlistPath, fileLines);
                }
            }
        }

        /// <summary>
        /// Writes required parameters in meta.ini
        /// </summary>
        /// <param name="moddir"></param>
        /// <param name="categoryNames">names of categories splitted by ','</param>
        /// <param name="version"></param>
        /// <param name="comments"></param>
        /// <param name="notes"></param>
        public static void WriteMetaIni(string moddir, string categoryNames = "", string version = "", string comments = "", string notes = "")
        {
            new MetaIniInfo().Set(moddir, categoryNames, version, comments, notes);
        }

        /// <summary>
        /// restore modlist which was not restored after zipmods update
        /// </summary>
        internal static void RestoreModlist()
        {
            if (File.Exists(ManageSettings.CurrentMoProfileModlistPath + ".prezipmodsUpdate"))
            {
                try
                {
                    File.Move(ManageSettings.CurrentMoProfileModlistPath, ManageSettings.CurrentMoProfileModlistPath + ".tmp");

                    if (!File.Exists(ManageSettings.CurrentMoProfileModlistPath))
                        File.Move(ManageSettings.CurrentMoProfileModlistPath + ".prezipmodsUpdate", ManageSettings.CurrentMoProfileModlistPath);

                    if (File.Exists(ManageSettings.CurrentMoProfileModlistPath))
                    {
                        new FileInfo(ManageSettings.CurrentMoProfileModlistPath + ".tmp").DeleteReadOnly();
                    }
                }
                catch (Exception ex)
                {
                    _log.Error("RestoreModlist error:\r\n" + ex);
                }
            }
        }

        /// <summary>
        /// return list of mo names in active profile folder
        /// </summary>
        /// <param name="onlyEnabled"></param>
        /// <returns></returns>
        public static string[] GetModNamesListFromActiveMoProfile(bool onlyEnabled = true)
        {
            return EnumerateModNamesListFromActiveMoProfile(onlyEnabled).ToArray();

            //string[] lines;
            //if (onlyEnabled)
            //{
            //    //все строки с + в начале
            //    lines = File.ReadAllLines(profilemodlistpath).Where(line => line.Length > 0 && line[0] == '+').ToArray();
            //}
            //else
            //{
            //    //все строки кроме сепараторов
            //    lines = File.ReadAllLines(profilemodlistpath).Where(line => line.Length > 0 && line[0] != '#' && (line.Length < 10 || !line.EndsWith("_separator", StringComparison.InvariantCulture))).ToArray();
            //}
            ////Array.Reverse(lines); //убрал, т.к. дулаю архив с резервными копиями
            //int linesLength = lines.Length;
            //for (int l = 0; l < linesLength; l++)
            //{
            //    //remove +-
            //    lines[l] = lines[l].Remove(0, 1);
            //}

            //return lines;
        }

        /// <summary>
        /// return list of mo names in active profile folder
        /// </summary>
        /// <param name="onlyEnabled"></param>
        /// <returns></returns>
        public static IEnumerable<string> EnumerateModNamesListFromActiveMoProfile(bool onlyEnabled = true)
        {
            string currentMOprofile = ManageSettings.GetMoSelectedProfileDirName();

            if (currentMOprofile.Length == 0)
            {
                yield break;
            }

            string profilemodlistpath = ManageSettings.CurrentMoProfileModlistPath;

            //фикс на случай несовпадения выбранной игры и профиля в MO ini
            if (!File.Exists(profilemodlistpath))
            {
                RedefineGameMoData();
                currentMOprofile = ReGetcurrentMOprofile(currentMOprofile);
                if (currentMOprofile.Length == 0)
                {
                    yield break;
                }

                profilemodlistpath = ManageSettings.CurrentMoProfileModlistPath;
            }

            if (!File.Exists(profilemodlistpath))
            {
                yield break;
            }

            foreach (var modInfo in new ModlistProfileInfo().GetBy(modType: onlyEnabled ? ModlistProfileInfo.ModType.ModEnabled : ModlistProfileInfo.ModType.ModAny))
            {
                yield return modInfo.Name;
            }
        }

        private static string ReGetcurrentMOprofile(string currentMOprofile)
        {
            while (!Directory.Exists(Path.Combine(ManageSettings.CurrentGameModOrganizerDirPath, "profiles", currentMOprofile)))
            {
                if (Directory.Exists(Path.Combine(ManageSettings.CurrentGameModOrganizerDirPath, "profiles", ManageSettings.CurrentGameExeName)))
                {
                    return ManageSettings.CurrentGameExeName;
                }
                else if (Directory.Exists(Path.Combine(ManageSettings.CurrentGameModOrganizerDirPath, "profiles", ManageSettings.CurrentGameDisplayingName)))
                {
                    return ManageSettings.CurrentGameDisplayingName;
                }
                else if (Directory.Exists(Path.Combine(ManageSettings.CurrentGameModOrganizerDirPath, "profiles", ManageSettings.CurrentGameDirName)))
                {
                    return ManageSettings.CurrentGameDirName;
                }
                else if (Directory.GetDirectories(Path.Combine(ManageSettings.CurrentGameModOrganizerDirPath, "profiles")).Length == 0)
                {
                    MessageBox.Show(T._("No profiles found for Current game") + " " + ManageSettings.CurrentGameDisplayingName);
                }
                else
                {
                    return Path.GetDirectoryName(Directory.GetDirectories(Path.Combine(ManageSettings.CurrentGameModOrganizerDirPath, "profiles"))[0]);
                }
            }
            Application.Exit();
            return "Default";
        }

        public static string GetLastMoFileDirPathFromEnabledModsOfActiveMoProfile(string[] pathInMods, bool[] isDir)
        {
            string path = string.Empty;
            int d = 0;
            try
            {
                if (pathInMods == null)
                {
                    return path;
                }

                foreach (var pathCandidate in pathInMods)
                {
                    path = pathCandidate;

                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        if ((isDir[d] && Directory.Exists(pathCandidate)) || (!isDir[d] && File.Exists(pathCandidate)))
                        {
                            return pathCandidate;
                        }
                        else
                        {
                            path = GetLastPath(pathCandidate, isDir[d]);
                            if (!string.IsNullOrWhiteSpace(path) && path != pathCandidate)
                            {
                                return path;
                            }
                        }
                    }

                    d++;
                }

                return path;
            }
            catch (Exception ex)
            {
                _log.Error("An error occured while path get:\r\n" + ex + "\r\npath=" + path);
            }

            return path;
        }

        /// <summary>
        /// Return last path for <paramref name="inputPath"/> in active Mod Organizer game profile.
        /// Can be set <paramref name="isDir"/> if object path is directory (file path by default).
        /// Can be set <paramref name="onlyEnabled"/> to determine if need to search only in enabled or all mods.
        /// </summary>
        /// <param name="inputPath">input path to dir or file</param>
        /// <param name="isDir">search dir else file</param>
        /// <param name="onlyEnabled">find only in enabled mods</param>
        /// <param name="tryFindWithContent">try to find not empty dir</param>
        /// <returns>Path in mod with hightest priority. When <paramref name="isDir"/> and <paramref name="tryFindWithContent"/> will try to find not empty dir. When not found will return <paramref name="inputPath"/></returns>
        public static string GetLastPath(string inputPath, bool isDir = false, bool onlyEnabled = true, bool tryFindWithContent = false)
        {
            if (string.IsNullOrWhiteSpace(inputPath))
            {
                return inputPath;
            }

            string firstFoundPath = string.Empty;
            try
            {
                string modsOverwrite = inputPath.Contains(ManageSettings.CurrentGameMoOverwritePath) ? ManageSettings.CurrentGameMoOverwritePath : ManageSettings.CurrentGameModsDirPath;

                //искать путь только для ссылки в Mods или в Data
                if (!ManageStrings.IsStringAContainsStringB(inputPath, modsOverwrite) && !ManageStrings.IsStringAContainsStringB(inputPath, ManageSettings.CurrentGameDataDirPath))
                    return inputPath;

                //отсеивание первого элемента с именем мода
                //string subpath = string.Empty;

                string[] pathInModsElements = inputPath
                    .Replace(modsOverwrite, string.Empty)
                    .Split(Path.DirectorySeparatorChar)
                    .Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

                //для мода в mods пропустить имя этого мода
                if (modsOverwrite == ManageSettings.CurrentGameModsDirPath)
                {
                    pathInModsElements = pathInModsElements.Skip(1).ToArray();
                }

                //foreach (var element in pathInModsElements)
                //{
                //    if (i > 1)
                //    {
                //        subpath += i > 2 ? Path.DirectorySeparatorChar.ToString() : string.Empty;
                //        subpath += element;
                //    }
                //    i++;
                //}
                string subpath = string.Join(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture), pathInModsElements);

                if (!ManageSettings.IsMoMode)
                {
                    return Path.Combine(ManageSettings.CurrentGameDataDirPath, subpath);
                }

                //check in Overwrite 1st
                string overwritePath = ManageSettings.CurrentGameMoOverwritePath + Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture) + subpath;

                if (IsLastPathFound(overwritePath, ref firstFoundPath, isDir, tryFindWithContent))
                {
                    return overwritePath;
                }

                //поиск по списку модов
                string modsPath = ManageSettings.CurrentGameModsDirPath;
                foreach (var modName in ManageModOrganizer.EnumerateModNamesListFromActiveMoProfile(onlyEnabled))
                {
                    string possiblePath = Path.Combine(modsPath, modName) + Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture) + subpath;

                    if (IsLastPathFound(possiblePath, ref firstFoundPath, isDir, tryFindWithContent))
                    {
                        return possiblePath;
                    }
                }
            }
            catch
            {
            }

            return firstFoundPath.Length > 0 ? firstFoundPath : inputPath;
        }

        private static bool IsLastPathFound(string possiblePath, ref string firstFoundPath, bool isDir, bool tryFindWithContent)
        {
            if (isDir)
            {
                if (Directory.Exists(possiblePath))
                {
                    if (tryFindWithContent && possiblePath.IsEmptyDir())
                    {
                        if (firstFoundPath.Length == 0)
                        {
                            // set found path if not set
                            firstFoundPath = possiblePath;
                        }
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (File.Exists(possiblePath))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets setup.xml path from latest enabled mod like must be in Mod Organizer
        /// </summary>
        /// <returns></returns>
        public static string GetSetupXmlPathForCurrentProfile()
        {
            if (ManageSettings.IsMoMode)
            {
                return GetLastPath(ManageSettings.CurrentGameSetupXmlFilePath);
            }
            else
            {
                return ManageSettings.CurrentGameSetupXmlFilePathinData;
            }
        }

        public static string GetMetaParameterValue(string metaFilePath, string neededValue)
        {
            return ManageIni.GetIniValueIfExist(metaFilePath, neededValue);

            //using (StreamReader sr = new StreamReader(MetaFilePath))
            //{
            //    string line;
            //    while (!sr.EndOfStream)
            //    {
            //        line = sr.ReadLine();

            //        if (line.Length > 0)
            //        {
            //            if (line.StartsWith(NeededValue + "=", StringComparison.InvariantCulture))
            //            {
            //                return line.Remove(0, (NeededValue + "=").Length);
            //            }
            //        }
            //    }
            //}

            //return string.Empty;
        }

        public static string GetModFromModListContainsTheName(string name, bool onlyFromEnabledMods = true)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                int nameLength = name.Length;
                foreach (var modname in ManageModOrganizer.EnumerateModNamesListFromActiveMoProfile(onlyFromEnabledMods))
                {
                    if (modname.Length >= nameLength && ManageStrings.IsStringAContainsStringB(modname, name))
                    {
                        return modname;
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// для МО до версии 2.3 -создание болванки, для 2.3 и новее - удаление, если есть
        /// </summary>
        public static void DummyFiles()
        {
            if (ManageSettings.MoIsNew)
            {
                // remove dummy file
                if (File.Exists(ManageSettings.DummyFilePath))
                {
                    File.Delete(ManageSettings.DummyFilePath);
                }

                var ini = ManageIni.GetINIFile(ManageSettings.ModOrganizerIniPath);

                RemoveCustomExecutable("Skyrim", ini);

                // change gameName to specific mo plugin set
                if (ini.GetKey("General", "gameName") == "Skyrim")
                {
                    ini.SetKey("General", "gameName", GetMoBasicGamePluginGameName());
                }
            }
            else
            {
                //Create dummy file and add hidden attribute
                if (!File.Exists(ManageSettings.DummyFilePath))
                {
                    File.Copy(ManageSettings.DummyFileResPath, ManageSettings.DummyFilePath);
                    //new FileInfo(ManageSettings.GetDummyFilePath()).Create().Close();
                    //File.WriteAllText(ManageSettings.GetDummyFilePath(), "dummy file need to execute mod organizer");
                    ManageFilesFoldersExtensions.HideFileFolder(ManageSettings.DummyFilePath, true);
                }
            }
        }

        /// <summary>
        /// remove custom executable by selected attribute
        /// </summary>
        /// <param name="ini"></param>
        /// <param name="AttributeToCheck"></param>
        private static void RemoveCustomExecutable(string keyToFind, INIFile ini = null, string AttributeToCheck = "title")
        {
            if (ini == null)
            {
                ini = ManageIni.GetINIFile(ManageSettings.ModOrganizerIniPath);
            }

            var customs = new CustomExecutables(ini);
            foreach (var custom in customs.List)
            {
                if (custom.Value.Attribute[AttributeToCheck] == keyToFind)
                {
                    customs.listToRemoveKeys.Add(custom.Key);
                }
            }

            customs.Save();
        }

        public static string GetCategoryNameForTheIndex(string inputCategoryIndex, CategoriesInfo categoriesList = null)
        {
            return GetCategoryIndexNameBase(inputCategoryIndex, categoriesList, true);
        }

        public static string GetCategoryIndexForTheName(string inputCategoryNames, CategoriesInfo categoriesList = null)
        {
            return GetCategoryIndexNameBase(inputCategoryNames, categoriesList, false);
        }

        /// <summary>
        /// GetName = true means will be returned categorie name by index else will be returned index by name
        /// </summary>
        /// <param name="inputNamesOrIds"></param>
        /// <param name="inputCategoriesList"></param>
        /// <param name="getName"></param>
        /// <returns></returns>
        public static string GetCategoryIndexNameBase(string inputNamesOrIds, CategoriesInfo inputCategoriesList = null, bool getName = false)
        {
            var categoriesList = inputCategoriesList ?? new CategoriesInfo();

            List<string> categoryParts = new List<string>();
            foreach (var part in inputNamesOrIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                foreach (var category in categoriesList.Records)
                {
                    if ((getName ? category.ID : category.Name) == part)
                    {
                        categoryParts.Add(getName ? category.Name : category.ID);
                    }
                }
            }

            return categoryParts.Count == 0 ? "" : categoryParts.Count > 1 ? string.Join(",", categoryParts) : categoryParts[0].Length > 0 ? categoryParts[0] + "," : "";
        }

        public static string GetCategorieNamesForTheFolder(string modDir, string defaultCategory, CategoriesInfo categoriesList = null)
        {
            string resultCategory = defaultCategory;

            if (categoriesList == null)
            {
                categoriesList = new CategoriesInfo();
            }

            string[,] categorieRules =
            {
                { Path.Combine(modDir, "BepInEx", "Plugins"), GetCategoryIndexForTheName("Plugins",categoriesList), "dll" } //Plug-ins 51
                ,
                { Path.Combine(modDir, "UserData"), GetCategoryIndexForTheName("UserFiles",categoriesList), "*" } //UserFiles 53
                ,
                { Path.Combine(modDir, "UserData", "chara"), GetCategoryIndexForTheName("Characters",categoriesList), "png" } //Characters 54
                ,
                { Path.Combine(modDir, "UserData", "studio", "scene"), GetCategoryIndexForTheName("Studio scenes",categoriesList), "png"} //Studio scenes 57
                ,
                { Path.Combine(modDir, "Mods"), GetCategoryIndexForTheName("Sideloader",categoriesList), "zip" } //Sideloader 60
                ,
                { Path.Combine(modDir, "scripts"), GetCategoryIndexForTheName("ScriptLoader scripts",categoriesList), "cs"} //ScriptLoader scripts 86
                ,
                { Path.Combine(modDir, "UserData", "coordinate"), GetCategoryIndexForTheName("Coordinate",categoriesList), "png"} //Coordinate 87
                ,
                { Path.Combine(modDir, "UserData", "Overlays"), GetCategoryIndexForTheName("Overlay",categoriesList), "png"} //Overlay 88
                ,
                { Path.Combine(modDir, "UserData", "housing"), GetCategoryIndexForTheName("Housing",categoriesList), "png"} //Housing 89
                ,
                { Path.Combine(modDir, "UserData", "housing"), GetCategoryIndexForTheName("Cardframe",categoriesList), "png"} //Cardframe 90
            };

            int categorieRulesLength = categorieRules.Length / 3;
            for (int i = 0; i < categorieRulesLength; i++)
            {
                string dir = categorieRules[i, 0];
                string categorieNum = categorieRules[i, 1];
                string extension = categorieRules[i, 2];
                if (
                    (
                        (defaultCategory.Length > 0
                        && !defaultCategory.Contains("," + categorieNum)
                        && !defaultCategory.Contains(categorieNum + ",")
                        )
                     || defaultCategory.Length == 0
                    )
                    && Directory.Exists(dir)
                    && !dir.IsNullOrEmptyDirectory()
                    && ManageFilesFoldersExtensions.IsAnyFileExistsInTheDir(dir, extension)
                   )
                {
                    if (resultCategory.Length > 0)
                    {
                        if (resultCategory.Substring(resultCategory.Length - 1, 1) == ",")
                        {
                            resultCategory += categorieNum;
                        }
                        else
                        {
                            resultCategory += "," + categorieNum;
                        }
                    }
                    else
                    {
                        resultCategory = categorieNum + ",";
                    }
                }
            }

            return resultCategory;
        }

        internal static void MoIniFixes()
        {
            if (!File.Exists(ManageSettings.ModOrganizerIniPath)) return;

            var ini = ManageIni.GetINIFile(ManageSettings.ModOrganizerIniPath);
            //if (INI == null) return;

            string gameName;
            //updated game name
            if (string.IsNullOrWhiteSpace(gameName = ini.GetKey("General", "gameName")) || gameName != ManageSettings.MoCurrentGameName)
            {
                ini.SetKey("General", "gameName", ManageSettings.MoCurrentGameName, false);
            }

            //clear pluginBlacklist section of MO ini to prevent plugin_python.dll exist there
            if (ini.SectionExistsAndNotEmpty("pluginBlacklist"))
            {
                ini.DeleteSection("pluginBlacklist", false);
            }

            bool selectedExecutableNeedToSet = true;
            var customs = new CustomExecutables(ini);
            foreach (var custom in customs.List)
            {
                // fix spaces in exe title to prevent errors because it
                if (custom.Value.Title.IndexOf(' ') != -1)
                {
                    custom.Value.Title = custom.Value.Title.Replace(' ', '_');
                }

                // Set selected_executable number to game exe
                if (selectedExecutableNeedToSet && Path.GetFileNameWithoutExtension(custom.Value.Binary) == CustomExecutables.NormalizePath(ManageSettings.CurrentGameExeName))
                {
                    selectedExecutableNeedToSet = false;
                    var index = custom.Key;
                    ini.SetKey("General", "selected_executable", index, false);
                    ini.SetKey("Widgets", "MainWindow_executablesListBox_index", index, false);
                }

                // set target mod for kkmanager exe's
                if (!string.Equals(custom.Value.MoTargetMod, ManageSettings.KKManagerFilesModName, StringComparison.InvariantCultureIgnoreCase)
                    && Directory.Exists(Path.Combine(ManageSettings.CurrentGameModsDirPath, ManageSettings.KKManagerFilesModName))
                    && ManageModOrganizer.EnumerateModNamesListFromActiveMoProfile(true).Any(n => n == ManageSettings.KKManagerFilesModName)
                    &&
                    (Path.GetFileName(custom.Value.Binary) == ManageSettings.KkManagerExeName
                    ||
                    Path.GetFileName(custom.Value.Binary) == ManageSettings.KkManagerStandaloneUpdaterExeName)
                    )
                {
                    custom.Value.MoTargetMod = ManageSettings.KKManagerFilesModName;
                }
            }

            customs.Save();

            ini.SetKey("PluginPersistance", @"Python%20Proxy\tryInit", "false");
        }

        /// <summary>
        /// clean MO folder from some useless files for illusion games
        /// </summary>
        internal static void CleanMoFolder()
        {
            var moFilesForClean = new[]
            {
                    @"MOFolder\plugins\bsa_*.dll",
                    //@"MOFolder\plugins\bsa_extractor.dll",
                    //@"MOFolder\plugins\bsa_packer.dll",
                    @"MOFolder\plugins\check_fnis.dll",
                    @"MOFolder\plugins\DDSPreview.py",
                    @"MOFolder\plugins\FNIS*.py",
                    //@"MOFolder\plugins\FNISPatches.py",
                    //@"MOFolder\plugins\FNISTool.py",
                    //@"MOFolder\plugins\FNISToolReset.py",
                    @"MOFolder\plugins\Form43Checker.py",
                    @"MOFolder\plugins\game_*.dll",
                    //@"MOFolder\plugins\game_enderal.dll",
                    //@"MOFolder\plugins\game_fallout3.dll",
                    //@"MOFolder\plugins\game_fallout4.dll",
                    //@"MOFolder\plugins\game_fallout4vr.dll",
                    //@"MOFolder\plugins\game_falloutNV.dll",
                    //@"MOFolder\plugins\game_morrowind.dll",
                    //@"MOFolder\plugins\game_oblivion.dll",
                    //@"MOFolder\plugins\game_skyrimse.dll",
                    //@"MOFolder\plugins\game_skyrimvr.dll",
                    //@"MOFolder\plugins\game_ttw.dll",
                    //@"MOFolder\plugins\game_skyrim.dll",
                    //@"MOFolder\plugins\game_enderalse.dll",
                    @"MOFolder\plugins\installer_bain.dll",
                    @"MOFolder\plugins\installer_bundle.dll",
                    @"MOFolder\plugins\installer_fomod.dll",
                    @"MOFolder\plugins\installer_ncc.dll",
                    @"MOFolder\plugins\installer_omod.dll",
                    @"MOFolder\plugins\preview_bsa.dll",
                    @"MOFolder\plugins\ScriptExtenderPluginChecker.py",
                    @"MOFolder\plugins\installer_fomod_csharp.dll",
                    @"MOFolder\plugins\data\OMODFramework*.*",
                    @"MOFolder\plugins\data\DDS\",
                    !GetMoVersion().StartsWith("2.3",StringComparison.InvariantCulture)?@"MOFolder\plugins\modorganizer-basic_games\":""
            };
            var mOfolderPath = ManageSettings.AppModOrganizerDirPath;
            foreach (var file in moFilesForClean)
            {
                var path = file.Replace("MOFolder", mOfolderPath);
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                if (path.EndsWith("\\", StringComparison.InvariantCulture) && Directory.Exists(path))
                {
                    var trimmedPath = path.TrimEnd('\\');
                    var searchDir = new DirectoryInfo(Path.GetDirectoryName(trimmedPath));
                    if (!searchDir.Exists)
                    {
                        continue;
                    }
                    var searchPattern = Path.GetFileName(trimmedPath);
                    foreach (var foundDir in searchDir.EnumerateDirectories(searchPattern))
                    {
                        try
                        {
                            foundDir.Attributes = FileAttributes.Normal;
                            foundDir.Delete(true);
                        }
                        catch (Exception ex)
                        {
                            _log.Error("An error occured while MO dir cleaning from useless files. Error:\r\n" + ex);
                        }
                    }
                }
                else
                {
                    var searchDir = new DirectoryInfo(Path.GetDirectoryName(path));
                    if (!searchDir.Exists)
                    {
                        continue;
                    }
                    var searchPattern = Path.GetFileName(path);
                    foreach (var foundFile in searchDir.EnumerateFiles(searchPattern))
                    {
                        try
                        {
                            foundFile.Attributes = FileAttributes.Normal;
                            foundFile.Delete();
                        }
                        catch (Exception ex)
                        {
                            _log.Error("An error occured while MO dir cleaning from useless files. Error:\r\n" + ex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// create game detection py files if missing
        /// </summary>
        internal static void CheckBaseGamesPy()
        {
            var moTargetBaseGamesPluginGamesDirPath = ManageSettings.MoBaseGamesPluginGamesDirPath;
            var moSourceBaseGamesPluginGamesDirPath = ManageSettings.AppResBasicGamesDir;
            if (!Directory.Exists(moTargetBaseGamesPluginGamesDirPath)) return;

            var pyname = ManageSettings.Games.Game.BasicGamePluginName;
            if (string.IsNullOrWhiteSpace(pyname)) return;

            var TargetPyInfo = new FileInfo(Path.Combine(moTargetBaseGamesPluginGamesDirPath, pyname + ".py"));
            var SourcePyInfo = new FileInfo(Path.Combine(moSourceBaseGamesPluginGamesDirPath, pyname + ".py"));            

            if (SourcePyInfo.Exists && (!TargetPyInfo.Exists || SourcePyInfo.Length != TargetPyInfo.Length))
            {
                TargetPyInfo.Directory.Create();
                SourcePyInfo.CopyTo(TargetPyInfo.FullName, true);
            }
        }

        /// <summary>
        /// get GameName value frome basicgame plugin
        /// </summary>
        /// <returns></returns>
        internal static string GetMoBasicGamePluginGameName()
        {
            Match gameName = Regex.Match(File.ReadAllText(Path.Combine(ManageSettings.MoBaseGamesPluginGamesDirPath, ManageSettings.Games.Game.BasicGamePluginName + ".py")), @"GameName \= \""([^\""]+)\""");

            if (gameName == null) return "";

            return gameName.Result("$1");
        }
    }

    internal static class CustomExecutablesExtensions
    {
        /// <summary>
        /// Add new <paramref name="customExecutable"/> in the <paramref name="customExecutables"/>.
        /// </summary>
        /// <param name="customExecutables">Custom executables</param>
        /// <param name="customExecutable">Executable to add.</param>
        /// <param name="performSave">True if need to save right after add.</param>
        internal static void Add(this CustomExecutables customExecutables, CustomExecutables.CustomExecutable customExecutable, bool performSave = false)
        {
            customExecutables.List.Add((customExecutables.List.Count + 1) + "", customExecutable);
            if (performSave)
            {
                customExecutables.Save();
            }
        }

        /// <summary>
        /// Check if <paramref name="customExecutables"/> contains <paramref name="binaryPath"/>
        /// </summary>
        /// <param name="customExecutables"></param>
        /// <param name="binaryPath"></param>
        /// <returns></returns>
        internal static bool ContainsBinary(this CustomExecutables customExecutables, string binaryPath)
        {
            if (customExecutables == null || customExecutables.List == null || string.IsNullOrWhiteSpace(binaryPath))
            {
                return false;
            }

            foreach (var custom in customExecutables.List)
            {
                if (custom.Value.Binary == CustomExecutables.NormalizePath(binaryPath))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if <paramref name="customExecutables"/> contains <paramref name="titleName"/>
        /// </summary>
        /// <param name="customExecutables"></param>
        /// <param name="titleName"></param>
        /// <returns></returns>
        internal static bool ContainsTitle(this CustomExecutables customExecutables, string titleName)
        {
            if (customExecutables == null || customExecutables.List == null || string.IsNullOrWhiteSpace(titleName))
            {
                return false;
            }

            foreach (var custom in customExecutables.List)
            {
                if (custom.Value.Binary == CustomExecutables.NormalizePath(titleName))
                {
                    return true;
                }
            }

            return false;
        }
    }

    class MetaIniInfo
    {
        INIFile _ini;
        public MetaIniParams Get(string moddir)
        {
            string metaPath = Path.Combine(moddir, "meta.ini");
            if (!File.Exists(metaPath))
            {
                return null;
            }

            _ini = ManageIni.GetINIFile(metaPath);

            return new MetaIniParams(
                _ini.GetKey("General", "gameName"),
                _ini.GetKey("General", "category").Trim('\"'),
                _ini.GetKey("General", "version"),
                _ini.GetKey("General", "comments"),
                _ini.GetKey("General", "notes"),
                _ini.GetKey("General", "url")
                );
        }

        public void Set(string moddir, string categoryNames = "", string version = "", string comments = "", string notes = "")
        {
            if (!Directory.Exists(moddir))
            {
                return;
            }

            string metaPath = Path.Combine(moddir, "meta.ini");
            if (!File.Exists(metaPath))
            {
                File.WriteAllText(metaPath, ManageSettings.DefaultMetaIni);
            }

            INIFile ini = ManageIni.GetINIFile(metaPath);

            if (!ini.KeyExists("category", "General") || (categoryNames.Replace(",", "").Length > 0 && ini.GetKey("General", "category").Trim('\"').Length == 0))
            {
                ini.SetKey("General", "category", "\"" + GetCategoryIndexForTheName(categoryNames) + "\"");
            }

            if (version.Length > 0)
            {
                ManageModOrganizer.ConvertMODateVersion(ref version, false);
                ini.SetKey("General", "version", version);
            }

            ini.SetKey("General", "gameName", ManageSettings.MoCurrentGameName);

            if (comments.Length > 0)
            {
                ini.SetKey("General", "comments", comments.ToHexForMetaIni());
            }

            if (notes.Length > 0)
            {
                ini.SetKey("General", "notes", ("\"" + notes.Replace(Environment.NewLine, "<br>") + "\"").ToHexForMetaIni());
            }

            ini.SetKey("General", "validated", "true");
        }

        public class MetaIniParams
        {
            public string GameName;
            public string Category;
            public string Version;
            public string Comments;
            public string Notes;
            public string Url;

            public MetaIniParams(string gameName, string category, string version, string comments, string notes, string url)
            {
                GameName = gameName;
                Category = category;
                Version = version;
                Comments = comments;
                Notes = notes;
                Url = url;
            }
        }
    }

    class CategoriesInfo
    {
        public List<CategoryLineInfo> Records = new List<CategoryLineInfo>();

        string _categoriesFilePath;
        int RecordsInitCount;

        public CategoriesInfo(string categoriesFilePath = null)
        {
            Get(categoriesFilePath ?? ManageSettings.CurrentGameMOcategoriesFilePath);
        }

        public string Add(string categoryName, bool doSave = false)
        {
            Records.Add(new CategoryLineInfo(new[] { Records.Count + 1 + "", categoryName, "", "0" }));

            return Records.Count + 1 + "";
        }

        private void Get(string categoriesFilePath)
        {
            if (!File.Exists(categoriesFilePath))
            {
                return;
            }

            _categoriesFilePath = categoriesFilePath;

            using (StreamReader reader = new StreamReader(categoriesFilePath))
            {
                string[] lineData;
                while (!reader.EndOfStream)
                {
                    lineData = reader.ReadLine().Split('|');
                    if (lineData == null || lineData.Length != 4)
                    {
                        continue;
                    }

                    Records.Add(new CategoryLineInfo(lineData));
                }
            }

            RecordsInitCount = Records.Count;
        }

        internal void Save()
        {
            if (RecordsInitCount == Records.Count)
            {
                // not changed
                return;
            }

            using (StreamWriter writer = new StreamWriter(_categoriesFilePath))
            {
                foreach (var record in Records)
                {
                    writer.WriteLine(record.ID + "|" + record.Name + "|" + record.NexisID + "|" + record.ParentID);
                }
            }
        }

        public class CategoryLineInfo
        {
            public string ID;
            public string Name;
            public string NexisID = "";
            public string ParentID = "0";

            /// <summary>
            /// ID + Name + NexisID + ParentID
            /// </summary>
            /// <param name="categoryInfo"></param>
            public CategoryLineInfo(string[] categoryInfo)
            {
                ID = categoryInfo[0];
                Name = categoryInfo[1];
                NexisID = categoryInfo[2];
                ParentID = categoryInfo[3];
            }
        }
    }
}
