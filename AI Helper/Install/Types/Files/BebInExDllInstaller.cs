using AIHelper.Manage;
using System;
using System.Diagnostics;
using System.IO;

namespace AIHelper.Install.Types.Files
{
    class BebInExDllInstaller : FilesInstallerBase
    {
        public override string[] Masks => new[] { "*.dll" };

        protected override bool Get(FileInfo dllfile)
        {
            if (dllfile == null)
            {
                return false;
            }

            var ret = false;

            FileVersionInfo dllInfo = FileVersionInfo.GetVersionInfo(dllfile.FullName);
            string name = dllInfo.ProductName;
            string description = dllInfo.FileDescription;
            string version = dllInfo.FileVersion;
            //string version = dllInfo.ProductVersion;
            string copyright = dllInfo.LegalCopyright;

            if (name == null || name.Length == 0)
            {
                name = Path.GetFileNameWithoutExtension(dllfile.FullName);
            }

            string author = string.Empty;
            if (copyright.Length >= 4)
            {
                //"Copyright © AuthorName 2019"
                author = copyright.Remove(copyright.Length - 4, 4).Replace("Copyright © ", string.Empty).Trim();
            }

            //добавление имени автора в начало имени папки
            if ((!string.IsNullOrEmpty(name) && name.Substring(0, 1) == "[" && !name.StartsWith("["+SharedData.GameData.Game.GameAbbreviation+"]", StringComparison.InvariantCultureIgnoreCase)) || (name.Length >= 5 && name.Substring(0, 5) == "["+ SharedData.GameData.Game.GameAbbreviation + "][") || ManageStrings.IsStringAContainsStringB(name, author))
            {
            }
            else if (author.Length > 0)
            {
                //проверка на любые невалидные для имени папки символы
                if (!ManageFilesFoldersExtensions.ContainsAnyInvalidCharacters(author))
                {
                    name = "[" + author + "]" + name;
                }
            }

            string dllName = dllfile.Name;
            string dllTargetModDirPath = Path.Combine(ManageSettings.CurrentGameModsDirPath, name);
            string dllTargetModPluginsSubdirPath = Path.Combine(dllTargetModDirPath, "BepInEx", "Plugins");
            string dllTargetPath = Path.Combine(dllTargetModPluginsSubdirPath, dllName);
            bool isUpdate = false;
            if (Directory.Exists(dllTargetModDirPath))
            {
                if (File.Exists(dllTargetPath) && File.GetLastWriteTime(dllfile.FullName) > File.GetLastWriteTime(dllTargetPath))
                {
                    //обновление существующей dll на более новую
                    isUpdate = true;
                    File.Delete(dllTargetPath);
                }
                else
                {
                    //Проверки существования целевой папки и модификация имени на более уникальное
                    dllTargetModDirPath = ManageFilesFoldersExtensions.GetResultTargetDirPathWithNameCheck(ManageSettings.CurrentGameModsDirPath, name);
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
                    string newModDirPath = Path.Combine(ManageSettings.CurrentGameModsDirPath, modNameWithAuthor);
                    if (Directory.Exists(newModDirPath))
                    {
                        dllTargetModDirPath = newModDirPath;
                        dllTargetModPluginsSubdirPath = Path.Combine(dllTargetModDirPath, "BepInEx", "Plugins");
                        dllTargetPath = Path.Combine(dllTargetModPluginsSubdirPath, dllName);
                        if (File.Exists(dllTargetPath) && File.GetLastWriteTime(dllfile.FullName) > File.GetLastWriteTime(dllTargetPath))
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
            dllfile.MoveTo(dllTargetPath);

            string readme = Path.Combine(dllfile.DirectoryName, Path.GetFileNameWithoutExtension(dllfile.Name) + " Readme.txt");
            if (File.Exists(readme))
            {
                File.Move(readme, Path.Combine(dllTargetModDirPath, Path.GetFileName(readme)));
            }


            //запись meta.ini
            ManageModOrganizer.WriteMetaIni(
                dllTargetModDirPath
                ,
                isUpdate ? string.Empty : "Plugins"
                ,
                version
                ,
                isUpdate ? string.Empty : "Requires: BepinEx"
                ,
                isUpdate ? string.Empty : "<br>Author: " + author + "<br><br>" + description + "<br><br>" + copyright
                );

            ManageModOrganizer.ActivateDeactivateInsertMod(Path.GetFileName(dllTargetModDirPath));

            return ret;
        }
    }
}
