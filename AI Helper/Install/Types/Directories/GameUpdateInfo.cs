using AIHelper.Manage;
using System.Collections.Generic;
using System.Linq;

namespace AIHelper.Install.Types.Directories
{
    /// <summary>
    /// Parameters of the game update which are read from the game update ini file
    /// </summary>
    sealed class GameUpdateInfo
    {
        /// <summary>
        /// Names of the supported parameters and their default values
        /// </summary>
        public readonly Dictionary<string, string> Keys = new Dictionary<string, string>()
        {
            { nameof(GameFolderName), string.Empty },
            { nameof(UpdateData), "false" },
            { nameof(UpdateMods), "false" },
            { nameof(IsRoot), "false" },
            { nameof(UpdateMO), "false" },
            { nameof(RenameFiles), string.Empty },
            { nameof(RenameDirs), string.Empty },
            { nameof(RemoveFiles), string.Empty },
            { nameof(RemoveDirs), string.Empty },
            { nameof(RemoveUnusedSeparators), "false" },
            { nameof(SkipExistDirs), string.Empty },
            { nameof(SkipExistFiles), string.Empty },
            { nameof(CreateDirs), string.Empty },
            { nameof(CreateFiles), string.Empty },
            { nameof(ApplyFixes), "false" },
            { nameof(CRCFiles), "false" },
            { nameof(SizeFiles), "true" },
            { nameof(WriteTimeFiles), "true" },
        };

        /// <summary>
        /// Name of game's folder name which need to update
        /// </summary>
        public string GameFolderName { get => Keys[nameof(GameFolderName)]; set => Keys[nameof(GameFolderName)] = value; }
        /// <summary>
        /// Update Data folder
        /// </summary>
        public string UpdateData { get => Keys[nameof(UpdateData)]; set => Keys[nameof(UpdateData)] = value; }
        /// <summary>
        /// Update mod dirs in Mods folder
        /// </summary>
        public string UpdateMods { get => Keys[nameof(UpdateMods)]; set => Keys[nameof(UpdateMods)] = value; }
        /// <summary>
        /// Update files in MO dir of the game to newer
        /// </summary>
        public string UpdateMO { get => Keys[nameof(UpdateMO)]; set => Keys[nameof(UpdateMO)] = value; }
        /// <summary>
        /// Root means aihelper root dir as update dir root
        /// </summary>
        public string IsRoot { get => Keys[nameof(IsRoot)]; set => Keys[nameof(IsRoot)] = value; }
        /// <summary>
        /// List of files to rename
        /// </summary>
        public string RenameFiles { get => Keys[nameof(RenameFiles)]; set => Keys[nameof(RenameFiles)] = value; }
        /// <summary>
        /// List of dirs to rename
        /// </summary>
        public string RenameDirs { get => Keys[nameof(RenameDirs)]; set => Keys[nameof(RenameDirs)] = value; }
        /// <summary>
        /// List of files to remove
        /// </summary>
        public string RemoveFiles { get => Keys[nameof(RemoveFiles)]; set => Keys[nameof(RemoveFiles)] = value; }
        /// <summary>
        /// List of dirs to remove
        /// </summary>
        public string RemoveDirs { get => Keys[nameof(RemoveDirs)]; set => Keys[nameof(RemoveDirs)] = value; }
        /// <summary>
        /// Determines if need to remove unused separators which is not updated modlist
        /// </summary>
        public string RemoveUnusedSeparators { get => Keys[nameof(RemoveUnusedSeparators)]; set => Keys[nameof(RemoveUnusedSeparators)] = value; }
        /// <summary>
        /// List of subpaths for dirs which must be skipped and not touched if already exist in current game
        /// </summary>
        public string SkipExistDirs { get => Keys[nameof(SkipExistDirs)]; set => Keys[nameof(SkipExistDirs)] = value; }
        /// <summary>
        /// List of subpaths for files which must be skipped and not touched if already exist in current game
        /// </summary>
        public string SkipExistFiles { get => Keys[nameof(SkipExistFiles)]; set => Keys[nameof(SkipExistFiles)] = value; }

        /// <summary>
        /// Dirs which must be created after update
        /// </summary>
        public string CreateDirs { get => Keys[nameof(CreateDirs)]; set => Keys[nameof(CreateDirs)] = value; }
        /// <summary>
        /// Files which must be created after update
        /// </summary>
        public string CreateFiles { get => Keys[nameof(CreateFiles)]; set => Keys[nameof(CreateFiles)] = value; }

        /// <summary>
        /// various game fixes and tweaks
        /// </summary>
        public string ApplyFixes { get => Keys[nameof(ApplyFixes)]; set => Keys[nameof(ApplyFixes)] = value; }
        /// <summary>
        /// check crc32 while check if file need to update. (slow)
        /// </summary>
        public string CRCFiles { get => Keys[nameof(CRCFiles)]; set => Keys[nameof(CRCFiles)] = value; }
        /// <summary>
        /// check size while check if file need to update
        /// </summary>
        public string SizeFiles { get => Keys[nameof(SizeFiles)]; set => Keys[nameof(SizeFiles)] = value; }
        /// <summary>
        /// check last write time while check if file need to update
        /// </summary>
        public string WriteTimeFiles { get => Keys[nameof(WriteTimeFiles)]; set => Keys[nameof(WriteTimeFiles)] = value; }

        /// <summary>
        /// Creates the game update info and reads it from <paramref name="updateInfoPath"/> when the path is set
        /// </summary>
        /// <param name="updateInfoPath">Path of the game update ini file</param>
        public GameUpdateInfo(string updateInfoPath = "")
        {
            if (!string.IsNullOrWhiteSpace(updateInfoPath))
            {
                LoadFromIni(updateInfoPath);
            }
        }

        /// <summary>
        /// Reads values of the supported parameters from the game update ini file
        /// </summary>
        /// <param name="updateInfoPath">Path of the game update ini file</param>
        private void LoadFromIni(string updateInfoPath)
        {
            var updateInfoIni = ManageIni.GetINIFile(updateInfoPath);

            foreach (var keyName in Keys.Keys.ToArray())
            {
                if (updateInfoIni.KeyExists(keyName))
                {
                    Keys[keyName] = updateInfoIni.GetKey("", keyName);
                }
            }
        }
    }
}
