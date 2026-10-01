using AIHelper.Install.Types.Files;
using AIHelper.Install.Types.Files.Archive;
using AIHelper.Manage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace AIHelper.Install.Types.Directories
{
    /// <summary>
    /// Installs mods from the subdirs of the install dir: moves mod dirs to the Mods dir
    /// and installs zipmods, dlls, unity3d files and cs scripts of the not mod dirs
    /// </summary>
    class ModFilesFromDirInstaller : DirectoriesInstallerBase
    {
        /// <summary>
        /// Names of the dirs which must be processed by the cards installers
        /// </summary>
        private static readonly HashSet<string> CardsInstallerDirNames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "f", "m", "c", "cf", "cb", "h", "h1", "h2", "h3", "h4", "o", "s"
        };

        /// <summary>
        /// Names of the dirs which are treated as the game mod dirs
        /// </summary>
        private static readonly HashSet<string> KnownGameModDirNames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "abdata", "userdata", "manual_s", "manual", "MonoBleedingEdge", "DefaultData", "bepinex", "scripts", "mods"
        };

        public override int Order => base.Order / 3;

        public override string ContentDescription => T._("Mod folders: a folder with mod files or mod subfolders");

        protected override bool Get(DirectoryInfo directoryInfo)
        {
            if (directoryInfo == null
                || File.Exists(Path.Combine(directoryInfo.FullName, ManageSettings.GameUpdateInstallerIniFileName)) // return if game update dir
                || File.Exists(Path.Combine(directoryInfo.FullName, ManageSettings.ApplicationProductName + ".exe")) // return if app update dir
                )
            {
                // not parse game update
                return false;
            }

            string dirName = directoryInfo.Name;
            if (string.Equals(dirName, "Temp", StringComparison.InvariantCultureIgnoreCase) // skip temp dir
                || IsDirForCardsInstaller(dirName) // must be parsed with cards installer
                )
            {
                return false;
            }

            //сортировка по подпапкам и переименование файлов
            ManageModOrganizer.SortFilesToSubfolders(directoryInfo.FullName);

            string dir = ManageFilesFoldersExtensions.MoveFolderToOneLevelUpIfItAloneAndReturnMovedFolderPath(directoryInfo.FullName);
            string name = Path.GetFileName(dir);
            string category = string.Empty;
            string version = string.Empty;
            string author = ManageModOrganizer.GetAuthorName(dir, name);//получение имени автора из имени файла или других файлов
            string comment = string.Empty;
            string description = string.Empty;
            string modDir = string.Empty;

            bool anyModFound = false;

            string[] subDirs = Directory.GetDirectories(dir, "*");

            var invariantCulture = CultureInfo.InvariantCulture;

            int subDirsLength = subDirs.Length;
            for (int i = 0; i < subDirsLength; i++)
            {
                string subdir = subDirs[i];
                string subdirname = Path.GetFileName(subdir);

                if (IsGameModValidDir(subdirname))
                {
                    //имя папки без GetResultTargetDirPathWithNameCheck для того, чтобы обновить существующую, если такая найдется
                    var targetModDir = Path.Combine(
                        ManageSettings.CurrentGameModsDirPath,
                        (author.Length > 0 && !ManageStrings.IsStringAContainsStringB(name, author))
                            ?
                            "[" + author + "]" + name
                            :
                            name);

                    if (Directory.Exists(targetModDir))
                    {
                        MergeIntoExistingModDir(dir, targetModDir);
                    }
                    else
                    {
                        Directory.Move(dir, targetModDir);
                    }

                    modDir = targetModDir;
                    anyModFound = true;
                    version = Regex.Match(name, @"\d+(\.\d+)*").Value;
                    description = name;
                    break;
                }
            }

            if (!anyModFound)
            {
                modDir = dir.Replace(ManageSettings.Install2MoDirPath, ManageSettings.CurrentGameModsDirPath);
                string targetfilepath = "readme.txt";
                foreach (var file in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories))
                {
                    if (Path.GetExtension(file) == ".zipmod")
                    {
                        string newpath = Path.Combine(ManageSettings.Install2MoDirPath, Path.GetFileName(file));
                        File.Move(file, newpath);
                        new ZipInstaller().Install();
                    }
                    else if (Path.GetExtension(file) == ".dll")
                    {
                        string newpath = Path.Combine(ManageSettings.Install2MoDirPath, Path.GetFileName(file));
                        File.Move(file, newpath);
                        new BebInExDllInstaller().Install();
                    }
                    else if (string.Compare(Path.GetExtension(file), ".unity3d", true, invariantCulture) == 0)//if extension == .unity3d
                    {
                        DirectoryInfo dirinfo = new DirectoryInfo(ManageSettings.CurrentGameDataDirPath);

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

                            targetfilepath = selectedfile.Replace(ManageSettings.CurrentGameDataDirPath, modDir);

                            Directory.CreateDirectory(Path.GetDirectoryName(targetfilepath));
                            File.Move(file, targetfilepath);
                        }
                        anyModFound = true;
                    }
                    else if (string.Compare(Path.GetExtension(file), ".cs", true, invariantCulture) == 0)//if extension == .cs
                    {
                        string targetsubdirpath = Path.Combine(modDir, "scripts");
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

                if (anyModFound && Directory.GetFiles(dir, "*.txt").Length > 0)
                {
                    string infofile = FindInfoFile(dir, targetfilepath);
                    if (infofile.Length > 0)
                    {
                        description = ReadInfoFile(infofile, ref name, ref author, ref version);

                        if (File.Exists(infofile))
                        {
                            File.Move(infofile, Path.Combine(modDir, Path.GetFileName(infofile)));
                        }
                    }
                }
            }

            if (!anyModFound)
            {
                return false;
            }

            if (author.Length == 0 && ManageFilesFoldersExtensions.IsAnyFileExistsInTheDir(modDir, ".dll"))
            {
                FillInfoFromDlls(modDir, ref author, ref version, ref description);
            }

            if (version.Length == 0)
            {
                var r = Regex.Match(Path.GetFileName(modDir), @"v(\d+(\.\d+)*)");
                if (r.Value.Length > 1)
                {
                    version = r.Value.Substring(1);
                }
            }

            //Задание доп. категорий по наличию папок
            category = ManageModOrganizer.GetCategorieNamesForTheFolder(modDir, category);

            //запись meta.ini
            ManageModOrganizer.WriteMetaIni(
                modDir
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

            ManageModOrganizer.ActivateDeactivateInsertMod(Path.GetFileName(modDir));

            return true;
        }

        /// <summary>
        /// Moves files of the installing mod to the existing mod dir and replaces the older files
        /// </summary>
        /// <param name="dir">Dir of the installing mod</param>
        /// <param name="targetModDir">Existing mod dir of the game</param>
        private static void MergeIntoExistingModDir(string dir, string targetModDir)
        {
            foreach (var file in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories))
            {
                string fileTarget = file.Replace(dir, targetModDir);
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

        /// <summary>
        /// Finds the info file of the mod dir by the known names
        /// </summary>
        /// <param name="dir">Dir of the installing mod</param>
        /// <param name="targetfilepath">Path of the unity3d file which is installed from the mod dir</param>
        /// <returns>Path of the info file or empty string when it is not found</returns>
        private static string FindInfoFile(string dir, string targetfilepath)
        {
            string dirNameInfoFile = Path.Combine(dir, Path.GetFileName(dir) + ".txt");
            if (File.Exists(dirNameInfoFile))
            {
                return dirNameInfoFile;
            }

            string readmeInfoFile = Path.Combine(dir, "readme.txt");
            if (File.Exists(readmeInfoFile))
            {
                return readmeInfoFile;
            }

            string descriptionInfoFile = Path.Combine(dir, "description.txt");
            if (File.Exists(descriptionInfoFile))
            {
                return descriptionInfoFile;
            }

            string targetFileInfoFile = Path.Combine(dir, Path.GetFileNameWithoutExtension(targetfilepath) + ".txt");
            if (File.Exists(targetFileInfoFile))
            {
                return targetFileInfoFile;
            }

            return string.Empty;
        }

        /// <summary>
        /// Reads name, author, version and description from the info file of the mod
        /// </summary>
        /// <param name="infofile">Path of the info file</param>
        /// <param name="name">Name of the mod which can be replaced by the name from the info file</param>
        /// <param name="author">Author of the mod which can be replaced by the author from the info file</param>
        /// <param name="version">Version of the mod which can be replaced by the version from the info file</param>
        /// <returns>Description of the mod from the info file</returns>
        private static string ReadInfoFile(string infofile, ref string name, ref string author, ref string version)
        {
            string description = string.Empty;

            string[] filecontent = File.ReadAllLines(infofile);
            bool descriptionMode = false;
            for (int l = 0; l < filecontent.Length; l++)
            {
                if (descriptionMode)
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
                    descriptionMode = true;
                }
            }

            return description;
        }

        /// <summary>
        /// Fills author, version and description from the info of the dll files of the mod
        /// </summary>
        /// <param name="modDir">Dir of the mod</param>
        /// <param name="author">Author of the mod which must be filled</param>
        /// <param name="version">Version of the mod which must be filled</param>
        /// <param name="description">Description of the mod which must be filled</param>
        private static void FillInfoFromDlls(string modDir, ref string author, ref string version, ref string description)
        {
            foreach (string dll in Directory.GetFiles(modDir, "*.dll", SearchOption.AllDirectories))
            {
                FileVersionInfo dllInfo = FileVersionInfo.GetVersionInfo(dll);

                if (description.Length == 0)
                {
                    description = dllInfo.FileDescription ?? string.Empty;
                }
                if (version.Length == 0)
                {
                    version = dllInfo.FileVersion ?? string.Empty;
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

        /// <summary>
        /// True when the dir must be processed by the cards installers
        /// </summary>
        /// <param name="name">Name of the dir</param>
        private static bool IsDirForCardsInstaller(string name)
        {
            return CardsInstallerDirNames.Contains(name);
        }

        /// <summary>
        /// True when the subdir contains the game mod content
        /// </summary>
        /// <param name="subdirname">Name of the subdir of the installing mod</param>
        private static bool IsGameModValidDir(string subdirname)
        {
            return KnownGameModDirNames.Contains(subdirname)
                || string.Equals(subdirname, ManageSettings.CurrentGameExeName + "_data", StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(subdirname, ManageSettings.StudioExeName + "_data", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
