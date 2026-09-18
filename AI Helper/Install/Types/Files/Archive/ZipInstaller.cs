using AIHelper.Manage;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace AIHelper.Install.Types.Files.Archive
{
    /// <summary>
    /// Installs zip archives: extracts mods, zipmods and scripts to the proper places of the Mods dir
    /// </summary>
    class ZipInstaller : ArchiveInstallerBase
    {
        public override string[] Masks => new[] { "*.zip" };

        protected override bool Get(FileInfo zipfile)
        {
            string zipName = Path.GetFileNameWithoutExtension(zipfile.FullName);

            ZipScanResult scan = null;
            int filesCount;

            using (ZipArchive archive = ZipFile.OpenRead(zipfile.FullName))
            {
                filesCount = ManageArchive.GetFilesCountInZipArchive(archive);

                if (filesCount == 1)
                {
                    //если один файл, распаковать в подпапку
                    archive.ExtractToDirectory(Path.Combine(ManageSettings.Install2MoDirPath, zipName));
                }
                else
                {
                    scan = ScanArchive(archive, zipName, filesCount);
                }
            }

            if (filesCount == 1)
            {
                zipfile.MoveTo(zipfile + ".Extracted" + ManageSettings.DateTimeBasedSuffix);
                return true;
            }

            if (scan.FoundZipMod)
            {
                //если файл имеет расширение zip. Надо, т.к. здесь может быть файл zipmod
                if (string.Equals(zipfile.Extension, ".zip", StringComparison.InvariantCultureIgnoreCase))
                {
                    zipfile.MoveTo(zipfile.FullName + "mod");
                }
                return new SideloaderZipmodInstaller().Install();//будет после установлено соответствующей функцией
            }

            if (scan.FoundStandardMod)
            {
                return InstallStandardMod(zipfile, zipName, scan);
            }

            if (scan.FoundModsDir)
            {
                zipfile.MoveTo(zipfile.FullName + (filesCount > 1 ? ".InstalledExtractedToSubfolder" : ".InstalledExtractedZipmod"));

                return true;
            }

            if (scan.FoundCsFiles)
            {
                //extract to handle as subdir
                string extractpath = ManageFilesFoldersExtensions.GetResultTargetDirPathWithNameCheck(ManageSettings.Install2MoDirPath, zipName);
                Compressor.Decompress(zipfile.FullName, extractpath);
                zipfile.MoveTo(zipfile.FullName + ".InstalledExtractedAsSubfolder");

                return true;
            }

            return false;
        }

        /// <summary>
        /// Scans entries of the archive and detects the type of its content
        /// </summary>
        /// <param name="archive">Opened archive</param>
        /// <param name="zipName">Name of the archive file without extension</param>
        /// <param name="filesCount">Count of files in the archive</param>
        /// <returns>Detected content of the archive</returns>
        private static ZipScanResult ScanArchive(ZipArchive archive, string zipName, int filesCount)
        {
            var scan = new ZipScanResult();

            int archiveEntriesCount = archive.Entries.Count;
            for (int entryNum = 0; entryNum < archiveEntriesCount; entryNum++)
            {
                ZipArchiveEntry entry = archive.Entries[entryNum];
                string entryName = entry.Name;
                string entryFullName = entry.FullName;

                if (!scan.FoundZipMod && string.Equals(Path.GetFileName(entryFullName), "manifest.xml", StringComparison.InvariantCultureIgnoreCase)) //entryFullName=="manifest.xml"
                {
                    scan.FoundZipMod = true;
                    break;
                }

                if (!scan.FoundStandardMod && IsStandardModInZip(entryFullName))
                {
                    scan.FoundStandardMod = true;
                }

                //found mods dir, and zipmod
                if (scan.FoundModsDir && !scan.FoundStandardMod && FoundZipmod(archive: archive, entryFullName: entryFullName, filesCount: filesCount, entry: entry, zipName: zipName, entryName: entryName))
                {
                    break;
                }

                //found mods dir
                if (!scan.FoundModsDir && string.Equals(Path.GetFileName(entryFullName), "mods", StringComparison.InvariantCultureIgnoreCase))
                {
                    scan.FoundModsDir = true;
                }

                //found cs
                if (!scan.FoundCsFiles && string.Equals(Path.GetExtension(entryFullName), ".cs", StringComparison.InvariantCultureIgnoreCase))
                {
                    scan.FoundStandardMod = false;
                    scan.FoundCsFiles = true;
                    break;
                }

                //found dll
                if (!string.Equals(Path.GetExtension(entryFullName), ".dll", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                if (scan.Description.Length != 0 || scan.Version.Length != 0 || scan.Author.Length != 0)
                {
                    continue;
                }

                ScanDllEntry(scan, entry, entryFullName, entryName, zipName);
            }

            return scan;
        }

        /// <summary>
        /// Reads info from the dll entry and detects the name of the mod which must be updated by the archive
        /// </summary>
        /// <param name="scan">Result of the archive scan which must be filled</param>
        /// <param name="entry">Dll entry of the archive</param>
        /// <param name="entryFullName">Full name of the dll entry in the archive</param>
        /// <param name="entryName">Name of the dll entry</param>
        /// <param name="zipName">Name of the archive file without extension</param>
        private static void ScanDllEntry(ZipScanResult scan, ZipArchiveEntry entry, string entryFullName, string entryName, string zipName)
        {
            string temp = Path.Combine(ManageSettings.Install2MoDirPath, "temp");
            string entryPath = Path.Combine(temp, entryFullName);
            string entryDir = Path.GetDirectoryName(entryPath);
            if (!Directory.Exists(entryDir))
            {
                Directory.CreateDirectory(entryDir);
            }

            entry.ExtractToFile(entryPath);

            if (!File.Exists(entryPath))
            {
                return;
            }

            var dllInfo = FileVersionInfo.GetVersionInfo(entryPath);
            scan.Description = dllInfo.FileDescription ?? string.Empty;
            scan.Version = dllInfo.FileVersion ?? string.Empty;
            //string version = dllInfo.ProductVersion;
            string copyright = dllInfo.LegalCopyright ?? string.Empty;

            if (copyright.Length >= 4)
            {
                //"Copyright © AuthorName 2019"
                scan.Author = copyright.Remove(copyright.Length - 4, 4).Replace("Copyright © ", string.Empty).Trim();
            }

            foreach (var modFolder in ManageModOrganizer.EnumerateModNamesListFromActiveMoProfile(false))
            {
                string modFolderForUpdate = Path.Combine(ManageSettings.CurrentGameModsDirPath, modFolder);
                string targetFile = Path.Combine(modFolderForUpdate, entryFullName);
                string targetFileAny = ManageModOrganizer.GetTheDllFromSubfolders(modFolderForUpdate, entryName.Remove(entryName.Length - 4, 4), "dll");
                if (!File.Exists(targetFile) && (targetFileAny.Length == 0 || !File.Exists(targetFileAny)))
                {
                    continue;
                }

                string updateModNameFromMeta = ManageIni.GetIniValueIfExist(Path.Combine(modFolderForUpdate, "meta.ini"), "notes", "General");
                if (updateModNameFromMeta.Length == 0)
                {
                    continue;
                }

                int upIndex = updateModNameFromMeta.IndexOf("ompupname:", StringComparison.InvariantCultureIgnoreCase);
                if (upIndex == -1)
                {
                    continue;
                }

                //get update name
                updateModNameFromMeta = updateModNameFromMeta.Substring(upIndex).Split(':')[1];
                if (updateModNameFromMeta.Length > 0 && zipName.Length >= updateModNameFromMeta.Length && ManageStrings.IsStringAContainsStringB(zipName, updateModNameFromMeta))
                {
                    scan.FoundUpdateName = true;
                    scan.ModFolderForUpdate = modFolderForUpdate;
                    scan.TargetFileAny = targetFileAny;
                    break;
                }
            }

            //File.Delete(entryPath);
            Directory.Delete(temp, true);
        }

        /// <summary>
        /// Extracts the archive as a standard mod, updates the existing mod when needed and writes the mod meta.ini
        /// </summary>
        /// <param name="zipfile">Installing archive</param>
        /// <param name="zipName">Name of the archive file without extension</param>
        /// <param name="scan">Result of the archive scan</param>
        /// <returns>True when the mod was installed</returns>
        private static bool InstallStandardMod(FileInfo zipfile, string zipName, ZipScanResult scan)
        {
            string version = scan.Version;
            string modName = zipName;
            ExtractVersion(ref version, ref modName);

            string targetModDirPath = ManageFilesFoldersExtensions.GetResultTargetDirPathWithNameCheck(
                scan.FoundUpdateName ? ManageSettings.Install2MoDirPath : ManageSettings.CurrentGameModsDirPath,
                modName + (scan.FoundUpdateName ? "_temp" : string.Empty));

            Compressor.Decompress(zipfile.FullName, targetModDirPath);

            if (scan.FoundUpdateName)
            {
                ParseFoundUpdateName(ref targetModDirPath, scan.ModFolderForUpdate, scan.TargetFileAny);
            }

            zipfile.MoveTo(zipfile.FullName + (scan.FoundUpdateName ? ".InstalledUpdatedMod" : ".InstalledExtractedToMods"));

            string author = scan.Author;
            if (!scan.FoundUpdateName && author.Length == 0)
            {
                author = zipName.StartsWith("[" + ManageSettings.CurrentGame.GameAbbreviation + "][", StringComparison.InvariantCulture)
                    || (zipName.StartsWith("[", StringComparison.InvariantCulture) && !zipName.StartsWith("[" + ManageSettings.CurrentGame.GameAbbreviation + "]", StringComparison.InvariantCulture))
                    ? zipName.Substring(zipName.IndexOf("[", StringComparison.InvariantCulture) + 1, zipName.IndexOf("]", StringComparison.InvariantCulture) - 1)
                    : string.Empty;
            }

            // category and comment are not detected for the standard mods and are set to the defaults
            string category = "Mods";
            string comment = string.Empty;

            //запись meta.ini
            ManageModOrganizer.WriteMetaIni(
                targetModDirPath
                ,
                scan.FoundUpdateName ? string.Empty : category
                ,
                version
                ,
                scan.FoundUpdateName ? string.Empty : comment
                ,
                scan.FoundUpdateName ? string.Empty : "<br>Author: " + author + "<br><br>" + (scan.Description.Length > 0 ? scan.Description : zipName) + "<br><br>"
                );

            ManageModOrganizer.ActivateDeactivateInsertMod(Path.GetFileName(targetModDirPath));

            return true;
        }

        /// <summary>
        /// Moves files of the extracted update mod to the mod dir of the game and deletes files which are older than the game ones
        /// </summary>
        /// <param name="targetModDirPath">Dir of the extracted update mod</param>
        /// <param name="modFolderForUpdate">Dir of the mod in the game which must be updated</param>
        /// <param name="targetFileAny">Path of the dll of the game mod</param>
        private static void ParseFoundUpdateName(ref string targetModDirPath, string modFolderForUpdate, string targetFileAny)
        {
            string[] modfiles = Directory.GetFiles(targetModDirPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in modfiles)
            {
                //ModFolderForUpdate
                string targetFile = file.Replace(targetModDirPath, modFolderForUpdate);
                string targetFileDir = Path.GetDirectoryName(targetFile);
                bool targetFileIsNewerOrSame = false;
                if (File.Exists(targetFile))
                {
                    if (File.GetLastWriteTime(file) > File.GetLastWriteTime(targetFile))
                    {
                        File.Delete(targetFile);
                    }
                    else
                    {
                        targetFileIsNewerOrSame = true;
                    }
                }
                else
                {
                    if (string.Equals(Path.GetExtension(targetFile), ".dll", StringComparison.InvariantCultureIgnoreCase)
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
                            targetFileIsNewerOrSame = true;
                        }
                    }
                }
                if (!Directory.Exists(targetFileDir))
                {
                    Directory.CreateDirectory(targetFileDir);
                }
                if (targetFileIsNewerOrSame)
                {
                    File.Delete(file);
                }
                else
                {
                    File.Move(file, targetFile);
                }
            }
            Directory.Delete(targetModDirPath, true);
            //присваивание папки на целевую, после переноса, для дальнейшей работы с ней
            targetModDirPath = modFolderForUpdate;
        }

        /// <summary>
        /// Extracts the version from the mod name and removes it from the name
        /// </summary>
        /// <param name="version">Version which is detected in the mod name when it is not set</param>
        /// <param name="modName">Name of the mod which must be cleaned from the version</param>
        private static void ExtractVersion(ref string version, ref string modName)
        {
            if (version.Length == 0)
            {
                var m = Regex.Match(modName, @"([0-9]{4})([\.\-_])([0-9]{2})\2([0-9]{2})");
                if (m.Success)
                {
                    version = "d" + m.Groups[4].Value + "." + m.Groups[3].Value.TrimStart('0') + "." + m.Groups[1].Value;
                    modName = modName.Replace(m.Value, string.Empty).TrimEnd('_', '-').Trim();
                }
                else
                {
                    m = Regex.Match(modName, @"([0-9]{2})([\.\-_])([0-9]{2})\2([0-9]{4})");
                    if (m.Success)
                    {
                        version = "d" + m.Groups[1].Value + "." + m.Groups[3].Value.TrimStart('0') + "." + m.Groups[4].Value;
                        modName = modName.Replace(m.Value, string.Empty).TrimEnd('_', '-').Trim();
                    }
                }
            }
            if (version.Length == 0)
            {
                var m = Regex.Match(modName, @"[0-9]+(\.[0-9]+)*");
                version = m.Value;
                modName = modName.Replace(m.Value, string.Empty).TrimEnd('_', '-').Trim();
            }
        }

        /// <summary>
        /// Extracts the zipmod or nested zip entry of the archive
        /// </summary>
        /// <param name="archive">Opened archive</param>
        /// <param name="entryFullName">Full name of the entry in the archive</param>
        /// <param name="filesCount">Count of files in the archive</param>
        /// <param name="entry">Entry which must be extracted</param>
        /// <param name="zipName">Name of the archive file without extension</param>
        /// <param name="entryName">Name of the entry</param>
        /// <returns>True when the entry is a zipmod or a nested zip</returns>
        private static bool FoundZipmod(ZipArchive archive, string entryFullName, int filesCount, ZipArchiveEntry entry, string zipName, string entryName)
        {
            if (string.Equals(Path.GetExtension(entryFullName), ".zipmod", StringComparison.InvariantCultureIgnoreCase))//entryFullName==".zipmod"
            {
                if (filesCount > 1)
                {
                    archive.ExtractToDirectory(Path.Combine(ManageSettings.Install2MoDirPath, zipName));
                }
                else
                {
                    entry.ExtractToFile(Path.Combine(ManageSettings.Install2MoDirPath, entryName));
                }
                return true;
            }
            else if (string.Equals(Path.GetExtension(entryFullName), ".zip", StringComparison.InvariantCultureIgnoreCase))
            {
                if (filesCount > 1)
                {
                    archive.ExtractToDirectory(Path.Combine(ManageSettings.Install2MoDirPath, zipName));
                }
                else
                {
                    entry.ExtractToFile(Path.Combine(ManageSettings.Install2MoDirPath, entryName + "mod"));
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// True when the entry is a part of the standard mod
        /// </summary>
        /// <param name="entryFullName">Full name of the entry in the archive</param>
        private static bool IsStandardModInZip(string entryFullName)
        {
            string entryFullNameUpper = entryFullName.ToUpperInvariant();
            return
                   string.Equals(Path.GetFileName(entryFullNameUpper), "ABDATA") //entryFullName=="abdata/"
                || entryFullNameUpper.StartsWith("ABDATA/")
                || entryFullNameUpper.EndsWith("_DATA")/*тут только проверка на окончание нужна || string.Compare(entryFullName.Substring(0, 5), "_data", true, CultureInfo.InvariantCulture) == 0*/ //entryFullName=="_data/"
                || entryFullNameUpper.StartsWith("BEPINEX/")
                || string.Equals(Path.GetFileName(entryFullNameUpper), "BEPINEX") //entryFullName=="bepinex/"
                || string.Equals(Path.GetFileName(entryFullNameUpper), "USERDATA") //entryFullName=="userdata/"
                || entryFullNameUpper.StartsWith("USERDATA/")
                ;
        }

        /// <summary>
        /// Result of the archive content scan
        /// </summary>
        private sealed class ZipScanResult
        {
            /// <summary>
            /// Archive contains the sideloader zipmod manifest
            /// </summary>
            public bool FoundZipMod { get; set; }

            /// <summary>
            /// Archive contains the standard mod content
            /// </summary>
            public bool FoundStandardMod { get; set; }

            /// <summary>
            /// Archive contains the mods dir
            /// </summary>
            public bool FoundModsDir { get; set; }

            /// <summary>
            /// Archive contains cs scripts
            /// </summary>
            public bool FoundCsFiles { get; set; }

            /// <summary>
            /// True when the archive contains the update of the existing mod
            /// </summary>
            public bool FoundUpdateName { get; set; }

            /// <summary>
            /// Name of the mod dir which must be updated
            /// </summary>
            public string ModFolderForUpdate { get; set; } = string.Empty;

            /// <summary>
            /// Path of the dll of the mod which must be updated
            /// </summary>
            public string TargetFileAny { get; set; } = string.Empty;

            /// <summary>
            /// Author from the dll info of the archive
            /// </summary>
            public string Author { get; set; } = string.Empty;

            /// <summary>
            /// Version from the dll info of the archive
            /// </summary>
            public string Version { get; set; } = string.Empty;

            /// <summary>
            /// Description from the dll info of the archive
            /// </summary>
            public string Description { get; set; } = string.Empty;
        }
    }
}
