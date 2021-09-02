using AIHelper.Install.Types.Files;
using AIHelper.Install.Types.Files.Archive;
using AIHelper.Manage;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace AIHelper.Install.Types.Directories
{
    class ModFilesFromDirInstaller : DirectoriesInstallerBase
    {
        public override int Order => base.Order / 3;

        protected override bool Get(DirectoryInfo directoryInfo)
        {
            string name = directoryInfo.Name;
            if (!ManageStrings.IsStringAequalsStringB(name, "Temp", true)
                && !ManageStrings.IsStringAequalsStringB(name, "f")
                && !ManageStrings.IsStringAequalsStringB(name, "m")
                && !ManageStrings.IsStringAequalsStringB(name, "c")
                && !ManageStrings.IsStringAequalsStringB(name, "cf")
                && !ManageStrings.IsStringAequalsStringB(name, "cb")
                && !ManageStrings.IsStringAequalsStringB(name, "h")
                && !ManageStrings.IsStringAequalsStringB(name, "h1")
                && !ManageStrings.IsStringAequalsStringB(name, "h2")
                && !ManageStrings.IsStringAequalsStringB(name, "h3")
                && !ManageStrings.IsStringAequalsStringB(name, "h4")
                && !ManageStrings.IsStringAequalsStringB(name, "o")
                && !ManageStrings.IsStringAequalsStringB(name, "s")
                )//path ends with 'Temp'
            {
                return false;
            }

            var ret = false;

            //сортировка по подпапкам и переименование файлов
            ManageModOrganizerMods.SortFilesToSubfolders(directoryInfo.FullName);

            string dir = ManageFilesFolders.MoveFolderToOneLevelUpIfItAloneAndReturnMovedFolderPath(directoryInfo.FullName);
            name = Path.GetFileName(dir);
            string category = string.Empty;
            string version = string.Empty;
            string author = ManageModOrganizerMods.GetAuthorName(dir, name);//получение имени автора из имени файла или других файлов
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
                    //CopyFolder.Copy(dir, Path.Combine(ManageSettings.GetCurrentGameModsPath(), dir));
                    //Directory.Move(dir, "[installed]" + dir);

                    //имя папки без GetResultTargetDirPathWithNameCheck для того, чтобы обновить существующую, если такая найдется
                    var targetModDIr = Path.Combine(
                        ManageSettings.GetCurrentGameModsDirPath(),
                        (author.Length > 0 && !ManageStrings.IsStringAContainsStringB(name, author))
                            ?
                            "[" + author + "]" + name
                            :
                            name);
                    //var TargetModDIr = ManageFilesFolders.GetResultTargetDirPathWithNameCheck(
                    //    ManageSettings.GetCurrentGameModsPath(), 
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
                moddir = dir.Replace(ManageSettings.GetInstall2MoDirPath(), ManageSettings.GetCurrentGameModsDirPath());
                string targetfilepath = "readme.txt";
                foreach (var file in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories))
                {
                    if (Path.GetExtension(file) == ".zipmod")
                    {
                        string newpath = Path.Combine(ManageSettings.GetInstall2MoDirPath(), Path.GetFileName(file));
                        File.Move(file, newpath);
                        new ZipInstaller().Install();
                    }
                    else if (Path.GetExtension(file) == ".dll")
                    {
                        string newpath = Path.Combine(ManageSettings.GetInstall2MoDirPath(), Path.GetFileName(file));
                        File.Move(file, newpath);
                        new BebInExDllInstaller().Install();
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
                category = ManageModOrganizer.GetCategorieNamesForTheFolder(moddir, category);

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

                ret = true;
            }

            return ret;
        }
    }
}
