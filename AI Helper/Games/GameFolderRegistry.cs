using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AIHelper.Manage;
using NLog;

namespace AIHelper.Games
{
    /// <summary>
    /// The game folders the user added, stored as text files in the games base folder:
    /// the file is named after the exe which was picked and holds one folder path per
    /// line.
    /// </summary>
    /// <remarks>
    /// A game folder does not have to be inside the games base folder - the files exist so
    /// that a build which is kept on another disk can be managed where it is. The file
    /// name is a label only, the paths it holds are what counts, and a file which was
    /// written by hand is read exactly like one the app wrote.
    /// </remarks>
    internal static class GameFolderRegistry
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private const string ListFileExtension = ".txt";

        /// <summary>
        /// Reads the game folders which are registered.
        /// </summary>
        /// <returns>
        /// the folders which exist, in the order they are written in the files. A folder
        /// which cannot be used is skipped rather than reported.
        /// </returns>
        internal static IEnumerable<string> ReadGameFolderPaths()
        {
            if (!Directory.Exists(ManageSettings.GamesBaseFolderPath))
            {
                yield break;
            }

            foreach (var listFilePath in Directory.EnumerateFiles(ManageSettings.GamesBaseFolderPath, "*" + ListFileExtension))
            {
                string[] lines;
                try
                {
                    if (new FileInfo(listFilePath).Length == 0)
                    {
                        continue;
                    }

                    lines = File.ReadAllLines(listFilePath);
                }
                catch (Exception ex)
                {
                    // a file which cannot be read must not cost the folders of the other files
                    _log.Warn("Failed to read the list of game folders " + listFilePath + ". Error:\r\n" + ex);
                    continue;
                }

                foreach (var line in lines)
                {
                    string gameDirPath = GetGameDirPath(line);
                    if (gameDirPath == null || !Directory.Exists(gameDirPath))
                    {
                        continue;
                    }

                    yield return gameDirPath;
                }
            }
        }

        /// <summary>
        /// Adds a game folder to the list of the exe it was added by.
        /// </summary>
        /// <param name="gameDirPath">game folder to remember</param>
        /// <param name="exeName">name of the game exe which was picked, without the extension</param>
        /// <returns>true when the folder is registered</returns>
        /// <remarks>
        /// A folder which is registered already is left in place rather than written a
        /// second time, so the file does not grow with every start of the app.
        /// </remarks>
        internal static bool AddGameFolder(string gameDirPath, string exeName)
        {
            string fullGameDirPath = GetGameDirPath(gameDirPath);
            if (fullGameDirPath == null || string.IsNullOrWhiteSpace(exeName))
            {
                return false;
            }

            try
            {
                Directory.CreateDirectory(ManageSettings.GamesBaseFolderPath);

                var listFilePath = GetListFilePath(exeName);
                if (!File.Exists(listFilePath))
                {
                    File.WriteAllText(listFilePath, fullGameDirPath + "\r\n");
                    return true;
                }

                var lines = File.ReadAllLines(listFilePath);
                foreach (var line in lines)
                {
                    if (IsSameGameDirPath(line, fullGameDirPath))
                    {
                        return true; // the folder is registered already
                    }
                }

                File.WriteAllLines(listFilePath, lines.Concat(new[] { fullGameDirPath }).ToArray());
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Failed to add the game folder " + gameDirPath + " to the list of " + exeName + ". Error:\r\n" + ex);
                return false;
            }
        }

        /// <summary>
        /// Full path of a game folder, or null when the text is not a usable path.
        /// </summary>
        /// <param name="path">path as it is written in a list file, or as it was picked</param>
        /// <remarks>
        /// A line which starts with a semicolon is a comment, and a line which is not a
        /// valid path is skipped, because the files are meant to be written by hand as well.
        /// </remarks>
        internal static string GetGameDirPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            string line = path.Trim();
            if (line.StartsWith(";", StringComparison.Ordinal))
            {
                return null;
            }

            try
            {
                return ManageFilesFoldersExtensions.NormalizePath(Path.GetFullPath(line));
            }
            catch (Exception ex)
            {
                // skip when invalid chars in path
                _log.Debug("Invalid game path skipped. Path=" + path + "\r\nError:\r\n" + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Path of the file which holds the game folders of an exe.
        /// </summary>
        private static string GetListFilePath(string exeName)
        {
            return Path.Combine(ManageSettings.GamesBaseFolderPath, exeName + ListFileExtension);
        }

        /// <summary>
        /// true when two lines name the same game folder.
        /// </summary>
        /// <remarks>
        /// A game folder is a folder of this machine, so the case of the path is not part
        /// of its identity and a trailing separator does not make it another folder.
        /// </remarks>
        private static bool IsSameGameDirPath(string left, string right)
        {
            string leftPath = GetGameDirPath(left);
            string rightPath = GetGameDirPath(right);
            if (leftPath == null || rightPath == null)
            {
                return false;
            }

            return string.Equals(
                ManageFilesFoldersExtensions.NormalizeDirectoryPath(leftPath),
                ManageFilesFoldersExtensions.NormalizeDirectoryPath(rightPath),
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
