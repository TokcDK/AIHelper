using AIHelper.SharedData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;

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

        public static void MOUSFSLoadingFix(bool removeLinks = false)
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
                var objectPath = new DirectoryInfo(ManageModOrganizer.GetLastPath(objectLinkPaths[i, 0], isDir: true));
                var linkPath = new DirectoryInfo(objectLinkPaths[i, 1]);

                if (removeLinks)
                {
                    ManageSymLinkExtensions.DeleteIfSymlink(linkPath.FullName);
                }
                else
                {
                    if (linkPath.IsSymlink())
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
                           ObjectType.Dir
                          ))
                        {
                        }
                        else
                        {
                            // need when dir in data exists and have content, then content will be move to target
                            ManageFilesFolders.MoveContent(linkPath.FullName, objectPath.FullName);

                            ManageSymLinkExtensions.CreateSymlink
                                (
                                 objectPath.FullName
                                 ,
                                 linkPath.FullName
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

                        sourceFilePath = ManageModOrganizer.GetLastPath(sourceFilePath);

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

            if (sourceFilePath.Contains(ManageSettings.GetCurrentGameModsDirPath()))
            {
                //remove Mods path slit and get 1st element as modname
                var noModsPath = sourceFilePath.Replace(ManageSettings.GetCurrentGameModsDirPath(), string.Empty);
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

        public static string GetUserDataSubFolder(string firstCandidateFolder, string type)
        {
            string[] targetFolders = new string[3]
            {
                firstCandidateFolder.Substring(0,1)== " " ? Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), "OrganizedModPack Downloaded"+firstCandidateFolder) : firstCandidateFolder,
                Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), "MyUserData"),
                ManageSettings.GetOverwriteFolder()
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

            return Path.Combine(ManageSettings.GetOverwriteFolder(), "UserData", typeFolder, targetFolderName);
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
                game = ManageXml.ReadXmlValue(manifestXml, "manifest/game", ManageSettings.GetZipmodManifestGameNameByCurrentGame(), isXmlString: isXmlString); //установил умолчание как "AI Girl"
            }
        }

        /// <summary>
        /// return mod path for input path in Mods
        /// </summary>
        /// <param name="inputPath"></param>
        /// <returns></returns>
        internal static string GetMoModPathInMods(string inputPath, string defaultPath = null)
        {
            //search Mods Path by step up to parent dir while it will be Mods path
            string modPath;
            var folderPath = modPath = inputPath;
            while (!string.Equals(folderPath/*.TrimEnd(new char[] { '/', '\\' })*/ , ManageSettings.GetCurrentGameModsDirPath()/*.TrimEnd(new char[] { '/', '\\' })*/, StringComparison.InvariantCultureIgnoreCase)
                //&& !string.Equals(folderPath, ManageSettings.GetOverwriteFolder(), StringComparison.InvariantCultureIgnoreCase)
                )
            {
                modPath = folderPath;
                folderPath = Path.GetDirectoryName(folderPath);
                if (string.Equals(folderPath, ManageSettings.GetCurrentGameDirPath(), StringComparison.InvariantCultureIgnoreCase))
                {
                    ManageLogs.Log("Warning. Path in Mods not found." + "\r\ninputPath=" + inputPath + "\r\nModPath=" + modPath + "\r\nFolderPath=" + folderPath);
                    return defaultPath;
                }
            }

            if (modPath != null &&
                (string.Equals(Path.GetFileName(modPath), Path.GetFileName(ManageSettings.GetCurrentGameModsDirPath()), StringComparison.InvariantCultureIgnoreCase)//temp debug check
                                                                                                                                                                    //|| string.Equals(Path.GetFileName(modPath), Path.GetFileName(ManageSettings.GetOverwriteFolder()), StringComparison.InvariantCultureIgnoreCase)
                )
                )
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
            foreach (var dir in Directory.EnumerateDirectories(ManageSettings.GetCurrentGameModsDirPath()))
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

        internal class ProfileModlist
        {
            internal string ModlistPath = "";
            internal List<ProfileModlistRecord> Items;
            internal Dictionary<string, ProfileModlistRecord> ItemByName;

            internal const string ListDescriptionMarker = "# This file was automatically generated by Mod Organizer.";
            internal const string SeparatorMarker = "_separator";

            /// <summary>
            /// init and load content of modlist for current profile
            /// </summary>
            public ProfileModlist()
            {
                Items = new List<ProfileModlistRecord>();
                ItemByName = new Dictionary<string, ProfileModlistRecord>();
                Load();
            }

            /// <summary>
            /// init and load content of modlist for selected path
            /// </summary>
            public ProfileModlist(string modListPath)
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
                ModlistPath = modListPath ?? ManageSettings.GetCurrentMoProfileModlistPath();

                if (!File.Exists(ModlistPath))
                {
                    return;
                }

                var modlistContent = File.ReadAllLines(ManageSettings.GetCurrentMoProfileModlistPath());
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
                    mod.Path = Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), mod.Name);
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

            /// <summary>
            /// Sideloader modpacks zipmod file infos are storing here
            /// </summary>
            /// <param name="LoadInfos">Load infos from active modlist right after init</param>
            public ZipmodGUIIds(bool LoadInfos = true)
            {
                GUIDList = new Dictionary<string, ZipmodInfo>();
                if (LoadInfos)
                {
                    LoadAll();
                }
            }

            void LoadAll()
            {
                var modlist = new ProfileModlist();

                foreach (var item in modlist.Items)
                {
                    if (!item.IsExist || item.IsSeparator)
                    {
                        continue;
                    }

                    if (Directory.Exists(Path.Combine(item.Path, "mods")))
                    {
                        foreach (var packDir in Directory.EnumerateDirectories(Path.Combine(item.Path, "mods"), "Sideloader Modpack*"))
                        {
                            foreach (var zipmod in Directory.EnumerateFiles(packDir, "*.zip*", SearchOption.AllDirectories))
                            {
                                Load(zipmod);
                            }
                        }
                    }
                }
            }

            internal void Load(string zipmodPath)
            {
                //zipmod GUID save
                if (File.Exists(zipmodPath) && (string.Equals(Path.GetExtension(zipmodPath), ".ZIPMOD", StringComparison.CurrentCultureIgnoreCase) || string.Equals(Path.GetExtension(zipmodPath), ".ZIPMOD", StringComparison.CurrentCultureIgnoreCase)))
                {
                    var guid = ManageArchive.GetZipmodGuid(zipmodPath);
                    if (guid.Length > 0 && !GUIDList.ContainsKey(guid))
                    {
                        var zipmodInfo = new ZipmodInfo();
                        zipmodInfo.GUID = guid;
                        zipmodInfo.FileInfo = new FileInfo(zipmodPath);
                        zipmodInfo.SubPath = GetRelativeSubPathInMods(zipmodPath);

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

        /// <summary>
        /// return relative subpath for input path
        /// </summary>
        /// <param name="zipmodPath"></param>
        /// <returns></returns>
        private static string GetRelativeSubPathInMods(string zipmodPath)
        {
            return zipmodPath.Replace(GetMoModPathInMods(zipmodPath), "");
        }
    }
}
