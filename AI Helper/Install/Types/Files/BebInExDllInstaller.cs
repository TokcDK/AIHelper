using AIHelper.Manage;
using System;
using System.Diagnostics;
using System.IO;

namespace AIHelper.Install.Types.Files
{
    /// <summary>
    /// Installs dll plugins to the BepInEx/Plugins subdir of their mod dir
    /// </summary>
    class BebInExDllInstaller : FilesInstallerBase
    {
        public override string[] Masks => new[] { "*.dll" };

        protected override bool Get(FileInfo dllfile)
        {
            if (dllfile == null)
            {
                return false;
            }

            var dllInfo = GetDllInfo(dllfile);

            //добавление имени автора в начало имени папки
            string modName = AddAuthorToNameIfNeed(dllInfo.Name, dllInfo.Author);

            var target = ResolveTarget(dllfile, modName);

            // readme is placed near the dll in the install dir, so its path must be resolved before the move
            string readmePath = Path.Combine(dllfile.DirectoryName, Path.GetFileNameWithoutExtension(dllfile.Name) + " Readme.txt");

            //перемещение dll в свою подпапку в Mods
            Directory.CreateDirectory(target.PluginsSubDirPath);
            dllfile.MoveTo(target.FilePath);

            if (File.Exists(readmePath))
            {
                File.Move(readmePath, Path.Combine(target.ModDirPath, Path.GetFileName(readmePath)));
            }

            //запись meta.ini
            ManageModOrganizer.WriteMetaIni(
                target.ModDirPath
                ,
                target.IsUpdate ? string.Empty : "Plugins"
                ,
                dllInfo.Version
                ,
                target.IsUpdate ? string.Empty : "Requires: BepinEx"
                ,
                target.IsUpdate ? string.Empty : "<br>Author: " + dllInfo.Author + "<br><br>" + dllInfo.Description + "<br><br>" + dllInfo.Copyright
                );

            ManageModOrganizer.ActivateDeactivateInsertMod(Path.GetFileName(target.ModDirPath));

            return true;
        }

        /// <summary>
        /// Resolves paths of the target mod dir of the dll and removes the older dll from the game
        /// </summary>
        /// <param name="dllfile">Installing dll</param>
        /// <param name="modName">Name of the mod dir of the dll</param>
        /// <returns>Paths of the mod dir and its plugins subdir with the target dll file and the update flag</returns>
        private static (string ModDirPath, string PluginsSubDirPath, string FilePath, bool IsUpdate) ResolveTarget(FileInfo dllfile, string modName)
        {
            string dllName = dllfile.Name;
            string modDirPath = Path.Combine(ManageSettings.CurrentGameModsDirPath, modName);
            string pluginsSubDirPath = Path.Combine(modDirPath, "BepInEx", "Plugins");
            string filePath = Path.Combine(pluginsSubDirPath, dllName);
            bool isUpdate = false;

            if (Directory.Exists(modDirPath))
            {
                if (File.Exists(filePath) && File.GetLastWriteTime(dllfile.FullName) > File.GetLastWriteTime(filePath))
                {
                    //обновление существующей dll на более новую
                    isUpdate = true;
                    File.Delete(filePath);
                }
                else
                {
                    //Проверки существования целевой папки и модификация имени на более уникальное
                    modDirPath = ManageFilesFoldersExtensions.GetResultTargetDirPathWithNameCheck(ManageSettings.CurrentGameModsDirPath, modName);
                    pluginsSubDirPath = Path.Combine(modDirPath, "BepInEx", "Plugins");
                    filePath = Path.Combine(pluginsSubDirPath, dllName);
                }
            }
            else
            {
                //найти имя мода из списка модов
                string modNameWithAuthor = ManageModOrganizer.GetModFromModListContainsTheName(modName, false);
                if (modNameWithAuthor.Length == 0)
                {
                    //если пусто, поискать также имя по имени дллки
                    modNameWithAuthor = ManageModOrganizer.GetModFromModListContainsTheName(Path.GetFileNameWithoutExtension(dllName), false);
                }

                if (modNameWithAuthor.Length > 0)
                {
                    string newModDirPath = Path.Combine(ManageSettings.CurrentGameModsDirPath, modNameWithAuthor);
                    if (Directory.Exists(newModDirPath))
                    {
                        modDirPath = newModDirPath;
                        pluginsSubDirPath = Path.Combine(modDirPath, "BepInEx", "Plugins");
                        filePath = Path.Combine(pluginsSubDirPath, dllName);
                        if (File.Exists(filePath) && File.GetLastWriteTime(dllfile.FullName) > File.GetLastWriteTime(filePath))
                        {
                            //обновление существующей dll на более новую, найдена папка существующего мода, с измененным именем
                            isUpdate = true;
                            File.Delete(filePath);
                        }
                    }
                }
            }

            return (modDirPath, pluginsSubDirPath, filePath, isUpdate);
        }

        /// <summary>
        /// Reads name, author and version info of the dll
        /// </summary>
        /// <param name="dllfile">Installing dll</param>
        private static DllInfo GetDllInfo(FileInfo dllfile)
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(dllfile.FullName);

            string name = versionInfo.ProductName;
            if (string.IsNullOrEmpty(name))
            {
                name = Path.GetFileNameWithoutExtension(dllfile.FullName);
            }

            string copyright = versionInfo.LegalCopyright ?? string.Empty;
            string author = string.Empty;
            if (copyright.Length >= 4)
            {
                //"Copyright © AuthorName 2019"
                author = copyright.Remove(copyright.Length - 4, 4).Replace("Copyright © ", string.Empty).Trim();
            }

            return new DllInfo
            {
                Name = name,
                Author = author,
                Version = versionInfo.FileVersion ?? string.Empty,
                Description = versionInfo.FileDescription ?? string.Empty,
                Copyright = copyright
            };
        }

        /// <summary>
        /// Adds the author name to the beginning of the mod dir name when the name does not contain it already
        /// </summary>
        /// <param name="name">Name of the mod dir</param>
        /// <param name="author">Author name of the dll</param>
        private static string AddAuthorToNameIfNeed(string name, string author)
        {
            bool nameContainsAuthor =
                   (!string.IsNullOrEmpty(name) && name[0] == '[' && !name.StartsWith("[" + ManageSettings.CurrentGame.GameAbbreviation + "]", StringComparison.InvariantCultureIgnoreCase))
                || (name.Length >= 5 && name.Substring(0, 5) == "[" + ManageSettings.CurrentGame.GameAbbreviation + "][")
                || ManageStrings.IsStringAContainsStringB(name, author);

            //проверка на любые невалидные для имени папки символы
            if (!nameContainsAuthor && author.Length > 0 && !ManageFilesFoldersExtensions.ContainsAnyInvalidCharacters(author))
            {
                return "[" + author + "]" + name;
            }

            return name;
        }

        /// <summary>
        /// Info which is read from the installing dll
        /// </summary>
        private sealed class DllInfo
        {
            /// <summary>
            /// Name of the mod dir of the dll
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Author name from the copyright of the dll
            /// </summary>
            public string Author { get; set; }

            /// <summary>
            /// Version of the dll
            /// </summary>
            public string Version { get; set; }

            /// <summary>
            /// Description of the dll
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// Copyright of the dll
            /// </summary>
            public string Copyright { get; set; }
        }
    }
}
