using AIHelper.Manage;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace AIHelper.Install.Types.Files.Archive
{
    class ZipInstaller : ArchiveInstallerBase
    {
        public override string[] Masks => new[] { "*.zip" };

        protected override bool Get(FileInfo zipfile)
        {
            var ret = false;

            bool foundZipMod = false;
            bool foundStandardModInZip = false;
            bool foundModsDir = false;
            bool foundcsFiles = false;

            string author = string.Empty;
            string category = "Mods";
            string version = string.Empty;
            string comment = string.Empty;
            string description = string.Empty;
            bool foundUpdateName = false;
            string modFolderForUpdate = string.Empty;
            string zipName = Path.GetFileNameWithoutExtension(zipfile.FullName);
            string targetFileAny = string.Empty;

            int filesCount = 0;

            using (ZipArchive archive = ZipFile.OpenRead(zipfile.FullName))
            {
                filesCount = ManageArchive.GetFilesCountInZipArchive(archive);

                int archiveEntriesCount = archive.Entries.Count;
                for (int entrieNum = 0; entrieNum < archiveEntriesCount; entrieNum++)
                {
                    //если один файл, распаковать в подпапку
                    if (filesCount == 1)
                    {
                        archive.ExtractToDirectory(Path.Combine(ManageSettings.GetInstall2MoDirPath(), zipName));
                        break;
                    }

                    string entryName = archive.Entries[entrieNum].Name;
                    string entryFullName = archive.Entries[entrieNum].FullName;

                    int entryFullNameLength = entryFullName.Length;
                    if (!foundZipMod && string.Equals(Path.GetFileName(entryFullName), "manifest.xml", StringComparison.InvariantCultureIgnoreCase)) //entryFullName=="manifest.xml"
                    {
                        foundZipMod = true;
                        break;
                    }

                    if (!foundStandardModInZip && IsStandardModInZip(entryFullName))
                    {
                        foundStandardModInZip = true;
                    }

                    //found mods dir, and zipmod
                    if (foundModsDir && !foundStandardModInZip && FoundZipmod(archive: archive, entryFullName: entryFullName, filesCount: filesCount, entrieNum: entrieNum, zipName: zipName, entryName: entryName))
                    {
                        break;
                    }

                    //found mods dir
                    if (!foundModsDir && string.Equals(Path.GetFileName(entryFullName), "mods", StringComparison.InvariantCultureIgnoreCase))
                    {
                        foundModsDir = true;
                    }

                    //found cs
                    if (!foundcsFiles && string.Equals(Path.GetExtension(entryFullName), ".cs", StringComparison.InvariantCultureIgnoreCase))
                    {
                        foundStandardModInZip = false;
                        foundcsFiles = true;
                        break;
                    }

                    //found dll
                    if (!string.Equals(Path.GetExtension(entryFullName), ".dll", StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    if (description.Length != 0 || version.Length != 0 || author.Length != 0)
                    {
                        continue;
                    }

                    string temp = Path.Combine(ManageSettings.GetInstall2MoDirPath(), "temp");
                    string entryPath = Path.Combine(temp, entryFullName);
                    string entryDir = Path.GetDirectoryName(entryPath);
                    if (!Directory.Exists(entryDir))
                    {
                        Directory.CreateDirectory(entryDir);
                    }

                    archive.Entries[entrieNum].ExtractToFile(entryPath);

                    if (!File.Exists(entryPath))
                    {
                        continue;
                    }

                    var dllInfo = FileVersionInfo.GetVersionInfo(entryPath);
                    description = dllInfo.FileDescription;
                    version = dllInfo.FileVersion;
                    //string version = dllInfo.ProductVersion;
                    string copyright = dllInfo.LegalCopyright;

                    if (copyright.Length >= 4)
                    {
                        //"Copyright © AuthorName 2019"
                        author = copyright.Remove(copyright.Length - 4, 4).Replace("Copyright © ", string.Empty).Trim();
                    }

                    foreach (var modFolder in ManageModOrganizer.EnumerateModNamesListFromActiveMoProfile(false))
                    {
                        modFolderForUpdate = Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), modFolder);
                        string targetfile = Path.Combine(modFolderForUpdate, entryFullName);
                        targetFileAny = ManageModOrganizer.GetTheDllFromSubfolders(modFolderForUpdate, entryName.Remove(entryName.Length - 4, 4), "dll");
                        if (!File.Exists(targetfile) && (targetFileAny.Length == 0 || !File.Exists(targetFileAny)))
                        {
                            continue;
                        }

                        if (targetFileAny.Length > 0)
                        {
                            targetfile = targetFileAny;
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
                            foundUpdateName = true;
                            break;
                        }
                        else
                        {
                            updateModNameFromMeta = string.Empty;
                        }
                    }

                    //File.Delete(entryPath);
                    Directory.Delete(temp, true);
                }
            }

            if (filesCount == 1)
            {
                zipfile.MoveTo(zipfile + ".Extracted" + ManageSettings.GetDateTimeBasedSuffix());
                ret = true;
            }
            else if (foundZipMod)
            {
                //если файл имеет расширение zip. Надо, т.к. здесь может быть файл zipmod
                if (string.Equals(zipfile.Extension, ".zip", StringComparison.InvariantCultureIgnoreCase))
                {
                    zipfile.MoveTo(zipfile.FullName + "mod");
                }
                ret = new SideloaderZipmodInstaller().Install();//будет после установлено соответствующей функцией
            }
            else if (foundStandardModInZip)
            {
                var modName = zipName;
                ExtractVersion(ref version, ref modName);

                string targetModDirPath = ManageFilesFoldersExtensions.GetResultTargetDirPathWithNameCheck(foundUpdateName ? ManageSettings.GetInstall2MoDirPath() : ManageSettings.GetCurrentGameModsDirPath(), modName + (foundUpdateName ? "_temp" : ""));

                Compressor.Decompress(zipfile.FullName, targetModDirPath);

                if (foundUpdateName)
                {
                    ParseFoundUpdateName(ref targetModDirPath, modFolderForUpdate, targetFileAny);
                }

                zipfile.MoveTo(zipfile.FullName + (foundUpdateName ? ".InstalledUpdatedMod" : ".InstalledExtractedToMods"));

                if (!foundUpdateName && author.Length == 0)
                {
                    author = zipName.StartsWith("[" + SharedData.GameData.CurrentGame.GetGamePrefix() + "][", StringComparison.InvariantCulture) || (zipName.StartsWith("[", StringComparison.InvariantCulture) && !zipName.StartsWith("[" + SharedData.GameData.CurrentGame.GetGamePrefix() + "]", StringComparison.InvariantCulture)) ? zipName.Substring(zipName.IndexOf("[", StringComparison.InvariantCulture) + 1, zipName.IndexOf("]", StringComparison.InvariantCulture) - 1) : string.Empty;
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
                    foundUpdateName ? string.Empty : "<br>Author: " + author + "<br><br>" + (description.Length > 0 ? description : zipName) + "<br><br>"
                    );

                ManageModOrganizer.ActivateDeactivateInsertMod(Path.GetFileName(targetModDirPath));

                ret = true;
            }
            else if (foundModsDir)
            {
                if (filesCount > 1)
                {
                    zipfile.MoveTo(zipfile.FullName + ".InstalledExtractedToSubfolder");
                }
                else
                {
                    zipfile.MoveTo(zipfile.FullName + ".InstalledExtractedZipmod");
                }

                ret = true;
            }
            else if (foundcsFiles)
            {
                //extract to handle as subdir
                string extractpath = ManageFilesFoldersExtensions.GetResultTargetDirPathWithNameCheck(ManageSettings.GetInstall2MoDirPath(), zipName);
                Compressor.Decompress(zipfile.FullName, extractpath);
                zipfile.MoveTo(zipfile.FullName + ".InstalledExtractedAsSubfolder");

                ret = true;
            }

            return ret;
        }

        private void ExtractVersion(ref string version, ref string modName)
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
                version = Regex.Match(modName, @"[0-9]+(\.[0-9]+)*").Value;
                modName = modName.Replace(m.Value, string.Empty).TrimEnd('_', '-').Trim();
            }
        }

        private bool FoundZipmod(ZipArchive archive, string entryFullName, int filesCount, int entrieNum, string zipName, string entryName)
        {
            if (string.Equals(Path.GetExtension(entryFullName), ".zipmod", StringComparison.InvariantCultureIgnoreCase))//entryFullName==".zipmod"
            {
                if (filesCount > 1)
                {
                    archive.ExtractToDirectory(Path.Combine(ManageSettings.GetInstall2MoDirPath(), zipName));
                }
                else
                {
                    archive.Entries[entrieNum].ExtractToFile(Path.Combine(ManageSettings.GetInstall2MoDirPath(), entryName));
                }
                return true;
            }
            else if (string.Equals(Path.GetExtension(entryFullName), ".zip", StringComparison.InvariantCultureIgnoreCase))
            {
                if (filesCount > 1)
                {
                    archive.ExtractToDirectory(Path.Combine(ManageSettings.GetInstall2MoDirPath(), zipName));
                }
                else
                {
                    archive.Entries[entrieNum].ExtractToFile(Path.Combine(ManageSettings.GetInstall2MoDirPath(), entryName + "mod"));
                }
                return true;
            }

            return false;
        }

        private bool IsStandardModInZip(string entryFullName)
        {
            entryFullName = entryFullName.ToUpperInvariant();
            return
                   string.Equals(Path.GetFileName(entryFullName), "ABDATA") //entryFullName=="abdata/"
                || entryFullName.ToUpperInvariant().StartsWith("ABDATA/")
                || entryFullName.EndsWith("_DATA")/*тут только проверка на окончание нужна || string.Compare(entryFullName.Substring(0, 5), "_data", true, CultureInfo.InvariantCulture) == 0*/ //entryFullName=="_data/"
                || entryFullName.ToUpperInvariant().StartsWith("BEPINEX/")
                || string.Equals(Path.GetFileName(entryFullName), "BEPINEX") //entryFullName=="userdata/"
                || string.Equals(Path.GetFileName(entryFullName), "USERDATA") //entryFullName=="userdata/"
                || entryFullName.ToUpperInvariant().StartsWith("USERDATA/")
                ;
        }

        private void ParseFoundUpdateName(ref string targetModDirPath, string modFolderForUpdate, string targetFileAny)
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
                    if (string.Equals(Path.GetExtension(targetFIle), ".dll", StringComparison.InvariantCultureIgnoreCase)
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
    }
}
