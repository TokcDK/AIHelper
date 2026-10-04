using AIHelper.Install.Types.Directories;
using AIHelper.Manage;
using INIFileMan;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;

namespace AIHelper.Install.UpdateMaker
{
    /// <summary>
    /// Creates the game update package from the selected content of the current game dir
    /// </summary>
    class UpdateMaker
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Name of the ini file which describes what must be packed into the update
        /// </summary>
        private const string MakeUpdateIniFileName = "makeupdate.ini";

        /// <summary>
        /// Name of the makeupdate ini section whose values override the generated update ini
        /// </summary>
        private const string DefaultSectionName = "Default";

        /// <summary>
        /// Sub dir of the game dir where the update package is created
        /// </summary>
        private const string UpdatesDirName = "Updates";

        /// <summary>
        /// Name of the update package dir inside <see cref="UpdatesDirName"/>
        /// </summary>
        private const string UpdateDirName = "Update";

        /// <summary>
        /// Sub dir of the update package dir which holds the packed game content
        /// </summary>
        private const string GamesDirName = "Games";

        /// <summary>
        /// Path of the update package dir created by the last successful <see cref="MakeUpdate"/>
        /// </summary>
        public string UpdateDirPath { get; private set; }

        /// <summary>
        /// Creates the update package when the current game dir contains <see cref="MakeUpdateIniFileName"/>
        /// </summary>
        /// <returns>True when the update package was created</returns>
        public bool MakeUpdate()
        {
            var makeUpdateIniFilePath = Path.Combine(ManageSettings.CurrentGameDirPath, MakeUpdateIniFileName);
            if (!File.Exists(makeUpdateIniFilePath))
            {
                // the game dir holds no update maker info, so this is a normal install run
                return false;
            }

            var updateDir = Path.Combine(ManageSettings.CurrentGameDirPath, UpdatesDirName, UpdateDirName);
            if (!InitUpdateDir(updateDir))
            {
                return false;
            }

            var makeUpdateIni = ManageIni.GetINIFile(makeUpdateIniFilePath);

            // the same parser instances are asked about every content type: they keep no per-run state
            var contentTypeParsers = new ContentTypeParserBase[]
            {
                new ContentTypeParserDirs(makeUpdateIni),
                new ContentTypeParserFiles(makeUpdateIni)
            };

            var updateMakers = new UpdateMakerBase[]
            {
                new UpdateMakerMO(),
                new UpdateMakerMods(),
                new UpdateMakerData()
            };

            var gameUpdateKeys = CopyAndCollectUpdateKeys(updateMakers, contentTypeParsers, updateDir);

            WriteUpdateIni(makeUpdateIni, updateDir, contentTypeParsers, gameUpdateKeys);

            UpdateDirPath = updateDir;

            return true;
        }

        /// <summary>
        /// Creates the update package dir and moves the previous one to the backup dir
        /// </summary>
        /// <param name="updateDir">Path of the update package dir</param>
        /// <returns>True when the dir is ready for the new package</returns>
        private bool InitUpdateDir(string updateDir)
        {
            if (Directory.Exists(updateDir))
            {
                try
                {
                    Directory.Move(updateDir, updateDir + ManageSettings.DateTimeBasedSuffix);
                }
                catch (Exception ex)
                {
                    _log.Error("Failed to move the previous update dir to the backup. Path:" + updateDir + "\r\nError:" + ex);
                    return false;
                }
            }

            try
            {
                Directory.CreateDirectory(updateDir);
            }
            catch (Exception ex)
            {
                _log.Error("Failed to create the update dir. Path:" + updateDir + "\r\nError:" + ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Copies the content of every content type to the update dir and collects the game update keys
        /// </summary>
        /// <param name="updateMakers">Content types which are packed into the update</param>
        /// <param name="contentTypeParsers">Parsers which copy the content of one content type</param>
        /// <param name="updateDir">Path of the update package dir</param>
        /// <returns>Game update keys with the flag which tells the installer to update the content type</returns>
        private static Dictionary<string, string> CopyAndCollectUpdateKeys(
            IEnumerable<UpdateMakerBase> updateMakers,
            IEnumerable<ContentTypeParserBase> contentTypeParsers,
            string updateDir)
        {
            var gameUpdateKeys = new Dictionary<string, string>();
            var updateGameDir = Path.Combine(updateDir, GamesDirName, ManageSettings.CurrentGameDirName);

            foreach (var updateMaker in updateMakers)
            {
                var gameDirOfTheContentType = Path.Combine(ManageSettings.CurrentGameDirPath, updateMaker.DirName);
                var updateDirOfTheContentType = Path.Combine(updateGameDir, updateMaker.DirName);

                var isAnyContentCopied = false;

                foreach (var contentTypeParser in contentTypeParsers)
                {
                    if (!contentTypeParser.IsSelectedFor(updateMaker))
                    {
                        continue;
                    }

                    if (contentTypeParser.CopyContent(updateMaker, gameDirOfTheContentType, updateDirOfTheContentType))
                    {
                        isAnyContentCopied = true;
                    }
                }

                gameUpdateKeys[updateMaker.UpdateKey] = ToIniFlag(isAnyContentCopied);
            }

            return gameUpdateKeys;
        }

        /// <summary>
        /// Writes the game update ini with the collected keys, remove lists and defaults
        /// </summary>
        /// <param name="makeUpdateIni">Ini file with the update maker parameters</param>
        /// <param name="updateDir">Path of the update package dir</param>
        /// <param name="contentTypeParsers">Parsers which copied the content</param>
        /// <param name="gameUpdateKeys">Game update keys collected during the copy</param>
        private static void WriteUpdateIni(
            INIFile makeUpdateIni,
            string updateDir,
            IEnumerable<ContentTypeParserBase> contentTypeParsers,
            IReadOnlyDictionary<string, string> gameUpdateKeys)
        {
            var gameUpdateIni = ManageIni.GetINIFile(Path.Combine(updateDir, ManageSettings.GameUpdateInstallerIniFileName));

            gameUpdateIni.SetKey("", nameof(GameUpdateInfo.GameFolderName), ManageSettings.CurrentGameDirName);
            gameUpdateIni.SetKey("", nameof(GameUpdateInfo.IsRoot), "true");

            // keys which tell the installer which content types must be updated
            foreach (var gameUpdateKey in gameUpdateKeys)
            {
                gameUpdateIni.SetKey("", gameUpdateKey.Key, gameUpdateKey.Value);
            }

            // paths which must be removed from the game during the update
            foreach (var contentTypeParser in contentTypeParsers)
            {
                gameUpdateIni.SetKey("", contentTypeParser.RemoveKeyName, string.Join(",", contentTypeParser.ReadRemoveList()));
            }

            // fill the keys which were not set above with their default values
            foreach (var parameter in new GameUpdateInfo().Keys)
            {
                if (!gameUpdateIni.KeyExists(parameter.Key, ""))
                {
                    gameUpdateIni.SetKey("", parameter.Key, parameter.Value);
                }
            }

            // values of the makeupdate Default section override everything above
            if (makeUpdateIni.SectionExistsAndNotEmpty(DefaultSectionName))
            {
                foreach (var parameter in makeUpdateIni.GetSectionKeyValuePairs(DefaultSectionName))
                {
                    gameUpdateIni.SetKey("", parameter.Key, parameter.Value);
                }
            }

            gameUpdateIni.WriteFile();
        }

        /// <summary>
        /// Formats a flag for the game update ini
        /// </summary>
        private static string ToIniFlag(bool value)
        {
            return value ? "true" : "false";
        }
    }
}
