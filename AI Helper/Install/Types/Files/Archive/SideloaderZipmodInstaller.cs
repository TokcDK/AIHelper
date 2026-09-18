using AIHelper.Manage;
using System;
using System.IO;
using System.Linq;
using static AIHelper.Manage.ManageModOrganizer;

namespace AIHelper.Install.Types.Files.Archive
{
    /// <summary>
    /// Installs zipmods: archives with the sideloader manifest which are not extracted
    /// but placed in the mods subfolder of their mod dir
    /// </summary>
    class SideloaderZipmodInstaller : ArchiveInstallerBase
    {
        /// <summary>
        /// Extensions of the files which are moved to the mod dir of the installing zipmod
        /// </summary>
        private static readonly string[] RelatedFilesExtensions = { ".txt", ".png", ".jpg", ".jpeg", ".bmp", ".doc", ".rtf" };

        // zipmods it is archives but they will not be extracted
        public override int Order => base.Order * 5;

        public override string[] Masks => new[] { "*.zipmod" };

        protected override bool Get(FileInfo zipfile)
        {
            var zipmod = GetManifestFromZipFile(zipfile.FullName);

            if (zipmod == null)
            {
                return false;
            }

            string zipArchiveName = Path.GetFileNameWithoutExtension(zipfile.FullName);
            bool gameEmpty = zipmod.game.Length == 0;

            if (!gameEmpty && !string.Equals(zipmod.game, ManageSettings.ZipmodManifestGameNameByCurrentGame, StringComparison.InvariantCultureIgnoreCase))
            {
                _log.Warn("Wrong game: " + zipmod.game + ", zipmod name: " + zipfile.Name);
                File.WriteAllText(zipfile.FullName + ".info.txt", zipfile.Name + " info:\r\n\r\nGame: " + zipmod.game);
                return false;
            }

            // mod name create
            string modName = zipmod.name.Length == 0 ? zipArchiveName : zipmod.name.RemoveInvalidPathChars();
            // add author name to mod name like '[author] modname'
            modName = ManageStrings.AddAuthorToNameIfNeed(modName, zipmod.author);

            // paths setup and removing of the older zipmod file when the zipmod is updated
            var target = ResolveTargetPaths(modName, zipArchiveName, zipfile.FullName);

            // move zipmod in it mod dir in Mods
            Directory.CreateDirectory(target.ModsSubDirPath);
            zipfile.MoveTo(target.TargetZipFile);

            MoveRelatedFilesToModDir(target.ModDirPath, zipArchiveName);

            // write meta.ini in thetarget mod's dir
            ManageModOrganizer.WriteMetaIni(
                target.ModDirPath
                ,
                string.Empty
                ,
                zipmod.version
                ,
                target.IsUpdate ? string.Empty : "Requires: Sideloader plugin"
                ,
                target.IsUpdate ? string.Empty : "<br>Author: " + zipmod.author + "<br><br>" + zipmod.description + "<br><br>" + zipmod.website + (gameEmpty ? "<br>WARNING: Game field for the Sideloader plugin was empty. Check the plugin manually if need." : string.Empty)
                );

            // insert modlist record of the mod
            ManageModOrganizer.ActivateDeactivateInsertMod(Path.GetFileName(target.ModDirPath), true);

            return true;
        }

        /// <summary>
        /// Resolves paths of the mod dir of the zipmod and removes the older zipmod file when the zipmod is updated
        /// </summary>
        /// <param name="modName">Name of the mod dir</param>
        /// <param name="zipArchiveName">Name of the zipmod archive without extension</param>
        /// <param name="zipFilePath">Path of the installing zipmod file</param>
        /// <returns>Paths of the mod dir and its mods subdir with the target zipmod file and the update flag</returns>
        private static (string ModDirPath, string ModsSubDirPath, string TargetZipFile, bool IsUpdate) ResolveTargetPaths(string modName, string zipArchiveName, string zipFilePath)
        {
            string modDirPath = Path.Combine(ManageSettings.CurrentGameModsDirPath, modName);

            if (!Directory.Exists(modDirPath))
            {
                modDirPath = ManageFilesFoldersExtensions.GetResultTargetDirPathWithNameCheck(ManageSettings.CurrentGameModsDirPath, modName);
                string newModsSubDirPath = Path.Combine(modDirPath, "mods");

                return (modDirPath, newModsSubDirPath, Path.Combine(newModsSubDirPath, zipArchiveName + ".zipmod"), false);
            }

            string modsSubDirPath = Path.Combine(modDirPath, "mods");
            string targetZipFile = Path.Combine(modsSubDirPath, zipArchiveName + ".zipmod");

            // check, if target zipmod is exists and if so then just update it
            bool isUpdate = false;
            string anyFileName = string.Empty;
            if (File.Exists(targetZipFile) || (anyFileName = ManageFilesFoldersExtensions.IsAnyFileWithSameExtensionContainsNameOfTheFile(modsSubDirPath, zipArchiveName, "*.zip")).Length > 0)
            {
                if (File.GetLastWriteTime(zipFilePath) > File.GetLastWriteTime(targetZipFile))
                {
                    isUpdate = true;
                    File.Delete(anyFileName.Length > 0 ? anyFileName : targetZipFile);
                }
            }

            return (modDirPath, modsSubDirPath, targetZipFile, isUpdate);
        }

        /// <summary>
        /// Moves files which names are started with the zipmod archive name to the mod dir of the zipmod
        /// </summary>
        /// <param name="modDirPath">Mod dir of the installing zipmod</param>
        /// <param name="zipArchiveName">Name of the zipmod archive without extension</param>
        private static void MoveRelatedFilesToModDir(string modDirPath, string zipArchiveName)
        {
            string[] possibleFilesOfTheMod = Directory.GetFiles(ManageSettings.Install2MoDirPath, "*.*")
                .Where(file => Path.GetFileName(file).Trim().StartsWith(zipArchiveName, StringComparison.InvariantCultureIgnoreCase)
                    && ManageStrings.IsStringAContainsAnyStringFromStringArray(Path.GetExtension(file), RelatedFilesExtensions))
                .ToArray();

            foreach (var possibleFile in possibleFilesOfTheMod)
            {
                if (!File.Exists(possibleFile))
                {
                    continue;
                }

                File.Move(possibleFile, Path.Combine(modDirPath, Path.GetFileName(possibleFile).Replace(zipArchiveName, Path.GetFileNameWithoutExtension(modDirPath))));
            }
        }
    }
}
