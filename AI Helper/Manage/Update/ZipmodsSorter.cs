using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace AIHelper.Manage.Update
{
    /// <summary>
    /// Moves the zipmods the KKManager updater left behind into the modpack they belong to.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The updater cannot write into a mod: it is an external program which knows the game
    /// data folder and the folder Mod Organizer calls the overwrite, and nothing else. What
    /// it downloads therefore lands in those two places, and putting it where it belongs is
    /// this application's job.
    /// </para>
    /// <para>
    /// Two rules decide the destination. A zipmod whose GUID is already known - it is in a
    /// modpack that is installed - goes back into that modpack. A zipmod which is part of a
    /// sideloader modpack that is not installed yet goes into the mod that carries the pack
    /// it names, and an uncensor selector pack is split further by a marker in the file
    /// name. Anything else is left where it is.
    /// </para>
    /// <para>
    /// The class holds only what it moves and where: the sort directories, the mods
    /// directory the destinations are built from, the directory which is cleaned up
    /// afterwards and the date suffix used to keep an older file next to a newer one. It
    /// reads no settings and touches no window, so it can be driven from a test as well as
    /// from the update pass.
    /// </para>
    /// </remarks>
    internal sealed class ZipmodsSorter
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>Directory inside a sort directory which holds the modpacks.</summary>
        private const string ModsDirName = "mods";

        /// <summary>
        /// Prefix of a sideloader modpack directory, compared without case.
        /// </summary>
        private const string SideloaderModpackDirPrefix = "Sideloader Modpack";

        /// <summary>
        /// Text which marks a directory as a sideloader modpack. It is compared upper case,
        /// so it is written the way it is compared.
        /// </summary>
        private const string SideloaderModpackMarker = "SIDELOADER MODPACK";

        /// <summary>
        /// Mod which collects the character cards and scenes of the community packs.
        /// </summary>
        private const string CommunityUserDataPackName = "Sideloader ModPack - Community UserData";

        /// <summary>Directory of the community pack inside the game data directory.</summary>
        private static readonly string[] CommunityUserDataSubDirs = { "UserData", "chara" };

        /// <summary>Directories of the community pack which hold the cards of one gender.</summary>
        private static readonly string[] GenderDirNames = { "female", "male" };

        /// <summary>Prefix of a community card directory, used to find the cards to move.</summary>
        private const string CommunityCardDirPattern = "[Community] *";

        /// <summary>File which explains how to get a card into the community packs.</summary>
        private const string CommunityInfoFileName = "Want your cards or scenes in BetterRepack.txt";

        /// <summary>
        /// Suffix which marks the male part of an uncensor selector modpack, and the marker
        /// its files carry.
        /// </summary>
        private const string MaleSuffix = "M";

        /// <summary>Suffix which marks the female part of an uncensor selector modpack.</summary>
        private const string FemaleSuffix = "F";

        /// <summary>Marker of a zipmod which belongs to the male part of an uncensor selector pack.</summary>
        private static readonly string[] MaleUncensorMarkers = { "[Penis]", "[Balls]" };

        /// <summary>Marker of a zipmod which belongs to the female part of an uncensor selector pack.</summary>
        private const string FemaleUncensorMarker = "[Female]";

        private readonly ManageModOrganizer.ZipmodGUIIds _zipmodGuids;
        private readonly IReadOnlyList<string> _sortDirPaths;
        private readonly HashSet<string> _sortDirNames;
        private readonly string _modsDirPath;
        private readonly string _gameDataDirPath;
        private readonly string _timeSuffix;

        /// <param name="zipmodGuids">
        /// The zipmod GUIDs which are installed, used to send a file back to the modpack it
        /// came from.
        /// </param>
        /// <param name="sortDirPaths">
        /// Directories the updater writes into, and therefore the ones which are searched for
        /// files to move.
        /// </param>
        /// <param name="modsDirPath">Mods directory of the game, which the destinations are built from.</param>
        /// <param name="gameDataDirPath">
        /// Game data directory. It is cleaned up together with the sort directories, because
        /// the updater writes its temporary folders there as well.
        /// </param>
        /// <param name="timeSuffix">
        /// Suffix appended to the name of an older file so that it can stay next to the newer
        /// one instead of being overwritten.
        /// </param>
        internal ZipmodsSorter(
            ManageModOrganizer.ZipmodGUIIds zipmodGuids,
            IReadOnlyList<string> sortDirPaths,
            string modsDirPath,
            string gameDataDirPath,
            string timeSuffix)
        {
            if (zipmodGuids == null) throw new ArgumentNullException(nameof(zipmodGuids));
            if (sortDirPaths == null) throw new ArgumentNullException(nameof(sortDirPaths));
            if (modsDirPath == null) throw new ArgumentNullException(nameof(modsDirPath));
            if (gameDataDirPath == null) throw new ArgumentNullException(nameof(gameDataDirPath));

            _zipmodGuids = zipmodGuids;
            _sortDirPaths = sortDirPaths;
            _modsDirPath = modsDirPath;
            _gameDataDirPath = gameDataDirPath;
            _timeSuffix = timeSuffix ?? string.Empty;

            // The names are what the filter below compares a candidate against, not the
            // paths. Comparing a directory name with a path could never match, which is how
            // the guard that keeps a sort directory from being sorted into itself ended up
            // doing nothing at all.
            _sortDirNames = new HashSet<string>(
                sortDirPaths.Select(Path.GetFileName),
                StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Moves everything the updater left in the sort directories, then cleans up.
        /// </summary>
        /// <remarks>
        /// A failure to move one file is logged and the rest continues: a single zipmod that
        /// cannot be moved is not a reason to leave the others behind. A failure to read a
        /// directory is not caught, because the caller reports it and there is nothing left
        /// to do afterwards.
        /// </remarks>
        internal void Sort()
        {
            SortSideloaderModpacks();
            SortCommunityUserDataPack();
            CleanEmptySubfolders();
        }

        /// <summary>
        /// Moves the zipmods of every sideloader modpack the updater wrote out into the mod
        /// which carries that pack.
        /// </summary>
        private void SortSideloaderModpacks()
        {
            Parallel.ForEach(_sortDirPaths, sortDirPath =>
            {
                var sortModsDir = new DirectoryInfo(Path.Combine(sortDirPath, ModsDirName));
                if (!sortModsDir.Exists) return;

                // A sort directory is never a modpack itself, so it is skipped even though
                // its name would have to look like a pack for the prefix test to let it
                // through.
                var modpackDirs = sortModsDir.GetDirectories().Where(dir =>
                    dir.Name.StartsWith(SideloaderModpackDirPrefix, StringComparison.InvariantCultureIgnoreCase)
                    && !_sortDirNames.Contains(dir.Name));

                if (!modpackDirs.Any()) return;

                var modpackTargets = ManageModOrganizer.GetSideloaderModpackTargetDirs();

                Parallel.ForEach(modpackDirs, modpackDir =>
                {
                    var modpackDirName = modpackDir.Name;

                    // An uncensor selector pack is shipped as one directory but belongs to
                    // two mods, one per gender, so which of them a file goes to is decided
                    // per file below.
                    var isUncensorSelector = ManageModOrganizer.IsUncensorSelector(modpackDirName);
                    var isMaleUncensorSelector = isUncensorSelector && modpackTargets.ContainsKey(modpackDirName + MaleSuffix);
                    var isFemaleUncensorSelector = isUncensorSelector && modpackTargets.ContainsKey(modpackDirName + FemaleSuffix);
                    var isSortingModpack = modpackTargets.ContainsKey(modpackDirName)
                        || isMaleUncensorSelector
                        || isFemaleUncensorSelector;

                    Parallel.ForEach(modpackDir.EnumerateFiles("*.*", SearchOption.AllDirectories), fileInfo =>
                    {
                        try
                        {
                            SortSideloaderModpackFile(
                                sortDirPath, sortModsDir, modpackDir, modpackDirName, modpackTargets,
                                isMaleUncensorSelector, isFemaleUncensorSelector, isSortingModpack, fileInfo);
                        }
                        catch (Exception ex)
                        {
                            _log.Error($"Failed to sort file '{fileInfo.FullName}'\r\nError:\r\n{ex}");
                        }
                    });
                });
            });
        }

        /// <summary>
        /// Decides where one file of a sideloader modpack belongs and moves it there.
        /// </summary>
        /// <remarks>
        /// A file whose GUID is known goes back to the modpack it is installed in, unless that
        /// modpack is itself a sideloader pack - those are owned by the updater and are left
        /// alone so that it can replace them as a whole.
        /// </remarks>
        private void SortSideloaderModpackFile(
            string sortDirPath,
            DirectoryInfo sortModsDir,
            DirectoryInfo modpackDir,
            string modpackDirName,
            Dictionary<string, string> modpackTargets,
            bool isMaleUncensorSelector,
            bool isFemaleUncensorSelector,
            bool isSortingModpack,
            FileInfo fileInfo)
        {
            var filePath = fileInfo.FullName;

            var guid = ManageArchive.GetZipmodGuid(filePath);
            var isKnownGuid = guid.Length > 0 && _zipmodGuids.GUIDList.ContainsKey(guid);

            var knownTargetModPath = isKnownGuid
                ? _zipmodGuids.GUIDList[guid].FileInfo.FullName.GetPathInMods()
                : string.Empty;

            var pathElements = !string.IsNullOrWhiteSpace(knownTargetModPath)
                ? filePath.Replace(sortDirPath, "").Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)
                : null;

            // %modpath%\mods\%sideloadermodpackdir%
            var targetModpackDirName = pathElements != null && pathElements.Length > 1 ? pathElements[1] : string.Empty;
            var targetIsInSideloader = targetModpackDirName.ToUpperInvariant().Contains(SideloaderModpackMarker);

            if (isKnownGuid && !targetIsInSideloader)
            {
                MoveZipmodFile(
                    filePath,
                    filePath.Replace(sortModsDir.FullName, knownTargetModPath),
                    filePath.Replace(sortModsDir.FullName, knownTargetModPath + _timeSuffix));
                return;
            }

            if (!isSortingModpack) return;

            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var isZip = Path.GetExtension(filePath).StartsWith(".zip", StringComparison.InvariantCultureIgnoreCase);

            var sortToMale = isMaleUncensorSelector && isZip
                && MaleUncensorMarkers.Any(marker => fileName.Contains(marker));
            var sortToFemale = isFemaleUncensorSelector && isZip && fileName.Contains(FemaleUncensorMarker);

            var targetModName = modpackTargets[modpackDirName + (sortToFemale ? FemaleSuffix : sortToMale ? MaleSuffix : "")];

            // the path of the file inside the modpack directory, which is kept so that the
            // pack keeps its own structure
            var fileSubPathInModpackDir = filePath.Replace(modpackDir.FullName, "");

            MoveZipmodFile(
                filePath,
                GetModpackTargetFilePath(targetModName, modpackDirName, fileSubPathInModpackDir, string.Empty),
                GetModpackTargetFilePath(targetModName, modpackDirName, fileSubPathInModpackDir, _timeSuffix));
        }

        /// <summary>
        /// Moves the character cards and scenes of the community packs into the mod that
        /// collects them.
        /// </summary>
        private void SortCommunityUserDataPack()
        {
            var communityUserDataPackPath = Path.Combine(_modsDirPath, CommunityUserDataPackName);

            Parallel.ForEach(_sortDirPaths, sortDirPath =>
            {
                var charaDirPath = Path.Combine(new[] { sortDirPath }.Concat(CommunityUserDataSubDirs).ToArray());
                var charaDir = new DirectoryInfo(charaDirPath);
                if (!charaDir.Exists) return;

                Parallel.ForEach(GenderDirNames, genderDirName =>
                {
                    var genderDir = new DirectoryInfo(Path.Combine(charaDir.FullName, genderDirName));
                    if (!genderDir.Exists) return;

                    Parallel.ForEach(genderDir.EnumerateDirectories(CommunityCardDirPattern), cardDir =>
                    {
                        Parallel.ForEach(cardDir.EnumerateFiles("*.*", SearchOption.AllDirectories), cardFile =>
                        {
                            MoveIntoCommunityPack(sortDirPath, communityUserDataPackPath, cardFile, "chara card");
                        });
                    });

                    // the note which explains how to submit a card sits next to the card
                    // directories and belongs to the pack as well
                    var infoFile = new FileInfo(Path.Combine(genderDir.FullName, CommunityInfoFileName));
                    if (infoFile.Exists)
                    {
                        MoveIntoCommunityPack(sortDirPath, communityUserDataPackPath, infoFile, "chara info file");
                    }
                });
            });
        }

        /// <summary>
        /// Moves one community pack file, logging and swallowing the failure so that the rest
        /// of the cards are still moved.
        /// </summary>
        private void MoveIntoCommunityPack(string sortDirPath, string communityPackPath, FileInfo sourceFile, string what)
        {
            try
            {
                var targetPath = new FileInfo(sourceFile.FullName.Replace(sortDirPath, communityPackPath));

                MoveFile(sourceFile, targetPath);
            }
            catch (Exception ex)
            {
                _log.Error("Failed to sort " + what + ": " + sourceFile.FullName + "\r\nerror:\r\n" + ex);
            }
        }

        /// <summary>
        /// Moves a file of a community pack, removing the source when the destination already
        /// holds the same content.
        /// </summary>
        private static void MoveFile(FileInfo sourceFile, FileInfo targetFile)
        {
            if (targetFile.Exists)
            {
                if (targetFile.Length == sourceFile.Length && targetFile.GetCrc32() == sourceFile.GetCrc32())
                {
                    // the same file was already moved in an earlier pass
                    sourceFile.DeleteReadOnly();
                    return;
                }

                targetFile = targetFile.GetNewTargetName();
            }

            targetFile.Directory.Create();
            sourceFile.MoveTo(targetFile.FullName);
        }

        /// <summary>
        /// Moves a zipmod file to its target path.
        /// <para>
        /// When the target file already exists, identical files are deduplicated (the source
        /// file is removed), an older target file is moved aside to
        /// <paramref name="olderTargetFilePath"/>, and otherwise the source file is moved to
        /// <paramref name="olderTargetFilePath"/> so that the target file is kept.
        /// </para>
        /// </summary>
        /// <param name="filePath">Source zipmod file path.</param>
        /// <param name="targetFilePath">Required target zipmod file path.</param>
        /// <param name="olderTargetFilePath">Target file path with the date suffix, used for the older file.</param>
        private static void MoveZipmodFile(string filePath, string targetFilePath, string olderTargetFilePath)
        {
            var isTargetExists = File.Exists(targetFilePath);
            var isSourceExists = File.Exists(filePath);

            ManageStrings.CheckForLongPath(ref filePath);
            ManageStrings.CheckForLongPath(ref targetFilePath);
            ManageStrings.CheckForLongPath(ref olderTargetFilePath);

            if (!isSourceExists)
            {
                return;
            }

            if (isTargetExists)
            {
                var targetInfo = new FileInfo(targetFilePath);
                var sourceInfo = new FileInfo(filePath);

                if (targetInfo.Length == sourceInfo.Length && targetInfo.GetCrc32() == sourceInfo.GetCrc32())
                {
                    // same file already moved before
                    sourceInfo.Delete();
                    return;
                }

                if (targetInfo.LastWriteTime < sourceInfo.LastWriteTime)
                {
                    // keep the older target file
                    Directory.CreateDirectory(Path.GetDirectoryName(olderTargetFilePath));
                    if (!File.Exists(olderTargetFilePath))
                    {
                        File.Move(targetFilePath, olderTargetFilePath);
                    }
                }
                else
                {
                    // keep the target file and place the source file near it
                    targetFilePath = olderTargetFilePath;
                }
            }

            Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath));

            try
            {
                File.Move(filePath, targetFilePath);
            }
            catch (IOException ex)
            {
                _log.Error($"An error occurred while moving the file. Error:\r\n{ex}\r\nFile='{filePath}'\r\nTarget File='{targetFilePath}'");
            }
        }

        /// <summary>
        /// Builds the path a zipmod of a sideloader modpack is moved to.
        /// </summary>
        /// <param name="targetModName">Name of the mod which is the target of the modpack.</param>
        /// <param name="modpackDirName">Name of the modpack directory inside the mods directory of the target mod.</param>
        /// <param name="fileSubPath">Path of the file inside the modpack directory.</param>
        /// <param name="targetModNameSuffix">Suffix for the target mod name, used for the date marked mod name.</param>
        private string GetModpackTargetFilePath(string targetModName, string modpackDirName, string fileSubPath, string targetModNameSuffix)
        {
            return _modsDirPath
                + Path.DirectorySeparatorChar + targetModName + targetModNameSuffix // mod name
                + Path.DirectorySeparatorChar + ModsDirName
                + Path.DirectorySeparatorChar + modpackDirName // modpack directory name
                + fileSubPath; // path of the file inside the modpack directory
        }

        /// <summary>
        /// Removes the empty directories the updater left in the sort directories and in the
        /// game data directory.
        /// </summary>
        /// <remarks>
        /// The directories themselves go as well when they are left empty: they are working
        /// directories which the tools that write into them create again on demand.
        /// </remarks>
        private void CleanEmptySubfolders()
        {
            foreach (var sortDirPath in _sortDirPaths)
            {
                ManageFilesFoldersExtensions.DeleteEmptySubfolders(sortDirPath, deleteThisDir: true);
            }

            ManageFilesFoldersExtensions.DeleteEmptySubfolders(_gameDataDirPath, deleteThisDir: true);
        }
    }
}
