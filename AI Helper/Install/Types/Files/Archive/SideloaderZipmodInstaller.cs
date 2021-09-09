using AIHelper.Manage;
using System;
using System.IO;
using System.Linq;
using static AIHelper.Manage.ManageModOrganizer;

namespace AIHelper.Install.Types.Files.Archive
{
    class SideloaderZipmodInstaller : ArchiveInstallerBase
    {
        // zipmods it is archives but they will not be extracted
        public override int Order => base.Order * 5;

        public override string[] Masks => new[] { "*.zipmod" };

        protected override bool Get(FileInfo zipfile)
        {
            SideloaderZipmodInfo zipmod = GetManifestFromZipFile(zipfile.FullName);

            if (zipmod == null)
            {
                return false;
            }

            string zipArchiveName = Path.GetFileNameWithoutExtension(zipfile.FullName);
            bool gameEmpty = zipmod.game.Length == 0;

            if (!gameEmpty && !string.Equals(zipmod.game, ManageSettings.GetZipmodManifestGameNameByCurrentGame(), StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            // mod name create
            string modName = zipmod.name.Length == 0 ? zipArchiveName : zipmod.name.RemoveInvalidPathChars();
            // add author name to mod name like '[author] modname'
            modName = ManageStrings.AddAuthorToNameIfNeed(modName, zipmod.author);
            //устанавливать имя папки из имени архива, если оно длиннее
            // заархивировано, т.к. имя из манифеста обычно чище
            //if (zipArchiveName.Length > modName.Length)
            //{
            //    modName = zipArchiveName;
            //    //добавление автора к имени папки, если не пустое и валидное
            //    modName = ManageStrings.AddAuthorToNameIfNeed(zipmod.name, zipmod.author);
            //}

            // paths setup
            //string zipmoddirpath = GetResultTargetDirPathWithNameCheck(ManageSettings.GetCurrentGameModsPath(), name);
            string modDirPath = Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), modName);
            string modsSubDirPath = Path.Combine(modDirPath, "mods");
            string targetZipFile = Path.Combine(modsSubDirPath, zipArchiveName + ".zipmod");

            // check, if target zipmod is exists and if so then just update it
            bool update = false;
            string anyFileName = string.Empty;
            if (Directory.Exists(modDirPath))
            {
                if (File.Exists(targetZipFile) || (anyFileName = ManageFilesFoldersExtensions.IsAnyFileWithSameExtensionContainsNameOfTheFile(modsSubDirPath, zipArchiveName, "*.zip")).Length > 0)
                {
                    if (File.GetLastWriteTime(zipfile.FullName) > File.GetLastWriteTime(targetZipFile))
                    {
                        update = true;
                        File.Delete(anyFileName.Length > 0 ? anyFileName : targetZipFile);
                    }
                }
            }
            else
            {
                modDirPath = ManageFilesFoldersExtensions.GetResultTargetDirPathWithNameCheck(ManageSettings.GetCurrentGameModsDirPath(), modName);
                modsSubDirPath = Path.Combine(modDirPath, "mods");
                targetZipFile = Path.Combine(modsSubDirPath, zipArchiveName + ".zipmod");
            }

            // move zipmod in it mod dir in Mods
            Directory.CreateDirectory(modsSubDirPath);
            File.Move(zipfile.FullName, targetZipFile);

            // Move files starting with same name to the same mod folder
            string[] possibleFilesOfTheMod = Directory.GetFiles(ManageSettings.GetInstall2MoDirPath(), "*.*").Where(file => Path.GetFileName(file).Trim().StartsWith(zipArchiveName, StringComparison.InvariantCultureIgnoreCase) && ManageStrings.IsStringAContainsAnyStringFromStringArray(Path.GetExtension(file), new string[7] { ".txt", ".png", ".jpg", ".jpeg", ".bmp", ".doc", ".rtf" })).ToArray();
            int possibleFilesOfTheModLength = possibleFilesOfTheMod.Length;
            if (possibleFilesOfTheModLength > 0)
            {
                for (int n = 0; n < possibleFilesOfTheModLength; n++)
                {
                    if (File.Exists(possibleFilesOfTheMod[n]))
                    {
                        File.Move(possibleFilesOfTheMod[n], Path.Combine(modDirPath, Path.GetFileName(possibleFilesOfTheMod[n]).Replace(zipArchiveName, Path.GetFileNameWithoutExtension(modDirPath))));
                    }
                }
            }

            // write meta.ini in thetarget mod's dir
            ManageModOrganizer.WriteMetaIni(
                modDirPath
                ,
                string.Empty
                ,
                zipmod.version
                ,
                update ? string.Empty : "Requires: Sideloader plugin"
                ,
                update ? string.Empty : "<br>Author: " + zipmod.author + "<br><br>" + zipmod.description + "<br><br>" + zipmod.website + (gameEmpty ? "<br>WARNING: Game field for the Sideloader plugin was empty. Check the plugin manually if need." : string.Empty)
                );

            // insert modlist record of the mod
            ManageModOrganizer.ActivateDeactivateInsertMod(Path.GetFileName(modDirPath), true);

            return true;
        }
    }
}
