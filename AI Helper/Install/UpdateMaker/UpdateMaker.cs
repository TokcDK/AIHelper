using AIHelper.Install.Types.Directories;
using AIHelper.Manage;
using INIFileMan;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AIHelper.Install.UpdateMaker
{
    /// <summary>
    /// Creates the game update package from the selected content of the current game dir
    /// </summary>
    class UpdateMaker
    {
        /// <summary>
        /// Game update keys which must be written in the game update ini
        /// </summary>
        private readonly Dictionary<string, string> _gameUpdateKeys = new Dictionary<string, string>();

        public bool MakeUpdate()
        {
            var updateMakeInfoFilePath = Path.Combine(ManageSettings.CurrentGameDirPath, "makeupdate.ini");
            if (!File.Exists(updateMakeInfoFilePath))
            {
                return false;
            }

            var updateMakers = new List<UpdateMakerBase>
            {
                new UpdateMakerMO(),
                new UpdateMakerMods(),
                new UpdateMakerData()
            };

            var infoIni = ManageIni.GetINIFile(updateMakeInfoFilePath);
            var updateDir = Path.Combine(ManageSettings.CurrentGameDirPath, "Updates", "Update");
            if (!InitUpdateDir(updateDir)) return false;

            var contentTypeParsers = new ContentTypeParserBase[]
            {
                new ContentTypeParserDirs(infoIni),
                new ContentTypeParserFiles(infoIni)
            };

            CopyAndAddUpdatePaths(updateMakers, contentTypeParsers, updateDir);

            WriteUpdateIni(infoIni, updateDir, contentTypeParsers);

            Process.Start(updateDir);

            return true;
        }

        /// <summary>
        /// Creates the update dir and moves the old update dir in the backup one
        /// </summary>
        private bool InitUpdateDir(string updateDir)
        {
            var dateTimeSuffix = ManageSettings.DateTimeBasedSuffix;
            if (Directory.Exists(updateDir))
            {
                try
                {
                    Directory.Move(updateDir, updateDir + dateTimeSuffix);
                }
                catch
                {
                    return false;
                }
            }
            Directory.CreateDirectory(updateDir);

            return true;
        }

        /// <summary>
        /// Copies content of every content type to the update dir and collects the game update keys
        /// </summary>
        private void CopyAndAddUpdatePaths(List<UpdateMakerBase> updateMakers, ContentTypeParserBase[] contentTypeParsers, string updateDir)
        {
            var updateGameDir = Path.Combine(updateDir, "Games", ManageSettings.CurrentGameDirName);

            foreach (var updateMaker in updateMakers)
            {
                var gameDir = Path.Combine(ManageSettings.CurrentGameDirPath, updateMaker.DirName);
                var updateDirOfTheContentType = Path.Combine(updateGameDir, updateMaker.DirName);
                var keyName = "Update" + updateMaker.DirName;

                foreach (var contentTypeParser in contentTypeParsers)
                {
                    contentTypeParser.Maker = updateMaker;

                    if (!contentTypeParser.IsNeedToCopy) continue;

                    contentTypeParser.CopyIncludedContent(gameDir, updateDirOfTheContentType);

                    UpdateGameUpdateKey(keyName, updateMaker);
                }
            }
        }

        /// <summary>
        /// Adds or updates the game update key of the content type
        /// </summary>
        private void UpdateGameUpdateKey(string keyName, UpdateMakerBase updateMaker)
        {
            var keyValue = updateMaker.IsAnyFileCopied.ToString().ToLowerInvariant();

            if (!_gameUpdateKeys.ContainsKey(keyName))
            {
                _gameUpdateKeys.Add(keyName, keyValue);
            }
            else if (updateMaker.IsAnyFileCopied && _gameUpdateKeys[keyName] == "false")
            {
                _gameUpdateKeys[keyName] = "true";
            }
        }

        /// <summary>
        /// Writes the game update ini with the collected keys, remove lists and defaults
        /// </summary>
        private void WriteUpdateIni(INIFile ini, string updateDir, ContentTypeParserBase[] contentTypeParsers)
        {
            var gameUpdateIni = ManageIni.GetINIFile(Path.Combine(updateDir, ManageSettings.GameUpdateInstallerIniFileName));
            gameUpdateIni.SetKey("", "GameFolderName", ManageSettings.CurrentGameDirName);
            gameUpdateIni.SetKey("", "IsRoot", "true");
            // add keys
            foreach (var parameter in _gameUpdateKeys)
            {
                gameUpdateIni.SetKey("", parameter.Key, parameter.Value);
            }
            // add other missing keys and their default values
            foreach (var parameter in new GameUpdateInfo().Keys)
            {
                if (!gameUpdateIni.KeyExists(parameter.Key, ""))
                {
                    gameUpdateIni.SetKey("", parameter.Key, parameter.Value);
                }
            }
            // set removelists
            foreach (var contentTypeParser in contentTypeParsers)
            {
                gameUpdateIni.SetKey("", contentTypeParser.RemoveKeyName, string.Join(",", contentTypeParser.RemoveList));
            }
            // set defaults from maker
            if (ini.SectionExistsAndNotEmpty("Default"))
            {
                foreach (var parameter in ini.GetSectionKeyValuePairs("Default"))
                {
                    gameUpdateIni.SetKey("", parameter.Key, parameter.Value);
                }
            }

            gameUpdateIni.WriteFile();
        }
    }
}
