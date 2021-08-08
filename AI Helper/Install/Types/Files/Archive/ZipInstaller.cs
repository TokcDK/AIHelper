using AIHelper.Manage;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace AIHelper.Install.Types.Files.Archive
{
    class ZipInstaller : ArchiveInstallerBase
    {
        public override string Mask => "*.zip";

        protected override void Get(FileInfo zipfile)
        {
            bool foundZipMod = false;
            bool foundStandardModInZip = false;
            bool foundModsDir = false;
            bool foundcsFiles = false;

            string author = string.Empty;
            string category = string.Empty;
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
                                archive.ExtractToDirectory(Path.Combine(ManageSettings.GetInstall2MoDirPath(), zipName));
                            }
                            else
                            {
                                archive.Entries[entrieNum].ExtractToFile(Path.Combine(ManageSettings.GetInstall2MoDirPath(), entryName));
                            }
                            break;
                        }
                        else if (entryFullNameLength >= 4 && string.Compare(entryFullName.Substring(entryFullNameLength - 4, 4), ".zip", true, CultureInfo.InvariantCulture) == 0)
                        {
                            if (filesCount > 1)
                            {
                                archive.ExtractToDirectory(Path.Combine(ManageSettings.GetInstall2MoDirPath(), zipName));
                            }
                            else
                            {
                                archive.Entries[entrieNum].ExtractToFile(Path.Combine(ManageSettings.GetInstall2MoDirPath(), entryName + "mod"));
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
                            string temp = Path.Combine(ManageSettings.GetInstall2MoDirPath(), "temp");
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
                                    targetFileAny = ManageModOrganizerMods.GetTheDllFromSubfolders(modFolderForUpdate, entryName.Remove(entryName.Length - 4, 4), "dll");
                                    if (File.Exists(targetfile) || (targetFileAny.Length > 0 && File.Exists(targetFileAny)))
                                    {
                                        if (targetFileAny.Length > 0)
                                        {
                                            targetfile = targetFileAny;
                                        }

                                        string updateModNameFromMeta = ManageIni.GetIniValueIfExist(Path.Combine(modFolderForUpdate, "meta.ini"), "notes", "General");
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
                zipfile.MoveTo(zipfile + ".ExtractedToSubdirAndMustBeInstalled");
            }
            else if (foundZipMod)
            {
                //если файл имеет расширение zip. Надо, т.к. здесь может быть файл zipmod
                if (zipfile.Length >= 4 && string.Compare(zipfile.FullName.Substring(zipfile.FullName.Length - 4, 4), ".zip", true, CultureInfo.InvariantCulture) == 0)
                {
                    zipfile.MoveTo(zipfile + "mod");
                }
                new SideloaderZipmodInstaller().Install();//будет после установлено соответствующей функцией
            }
            else if (foundStandardModInZip)
            {
                string targetModDirPath;
                if (foundUpdateName)
                {
                    targetModDirPath = Path.Combine(ManageSettings.GetInstall2MoDirPath(), zipName + "_temp");
                }
                else
                {
                    targetModDirPath = Path.Combine(Properties.Settings.Default.ModsPath, zipName);
                }

                Compressor.Decompress(zipfile.FullName, targetModDirPath);

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

                zipfile.MoveTo(zipfile.FullName + (foundUpdateName ? ".InstalledUpdatedMod" : ".InstalledExtractedToMods"));

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
                    foundUpdateName ? string.Empty : "<br>Author: " + author + "<br><br>" + (description.Length > 0 ? description : zipName) + "<br><br>"
                    );

                ManageModOrganizer.ActivateDeactivateInsertMod(Path.GetFileName(targetModDirPath));
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
            }
            else if (foundcsFiles)
            {
                //extract to handle as subdir
                string extractpath = Path.Combine(ManageSettings.GetInstall2MoDirPath(), zipName);
                Compressor.Decompress(zipfile.FullName, extractpath);
                zipfile.MoveTo(zipfile.FullName + ".InstalledExtractedAsSubfolder");
            }
        }
    }
}
