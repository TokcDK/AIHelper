using System;
using System.IO;
using System.Text.RegularExpressions;
using NLog;
using static AIHelper.Manage.ManageModOrganizer;

namespace AIHelper.Manage.MOiniCustomExeFixers
{
    internal class MainExesPathFixer : ICustomExePathFixerBase
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The game dir path as an MO ini wants to see it: the part above the game dir name,
        /// without a trailing separator.
        /// </summary>
        /// <remarks>
        /// The ini stores paths as they were on the machine the mod was made on, so a
        /// profile moved to another folder keeps a path pointing at the old place. Both
        /// branches below rebuild the same prefix from the current settings, so it is
        /// worked out once here rather than twice.
        /// </remarks>
        private static string GetGameDirRootPath()
        {
            var currentGameDirPath = ManageSettings.CurrentGameDirPath;
            var gameDirNameIndex = currentGameDirPath.IndexOf("\\" + ManageSettings.CurrentGameDirName, StringComparison.Ordinal);

            if (gameDirNameIndex < 0)
            {
                // The game dir is not inside the path, so there is nothing to strip. Say so
                // instead of handing Remove(-1) an exception.
                _log.Warn("The game dir name '" + ManageSettings.CurrentGameDirName
                    + "' is not a part of the game dir path '" + currentGameDirPath + "'.");

                return currentGameDirPath.TrimEnd('\\');
            }

            return currentGameDirPath.Remove(gameDirNameIndex).TrimEnd('\\');
        }

        public void TryFix(CustomExeFixData data)
        {
            // fix main exe data.Paths
            if (data.Attribute == "arguments")
            {
                if (string.IsNullOrWhiteSpace(data.Path)) return;

                var matches = Regex.Matches(data.Path, @"[A-Za-z]:([\\]{2}[^\\\""]+)+");
                for (int i = matches.Count - 1; i >= 0; i--)
                {
                    var match = matches[i];

                    var path = match.Value;

                    if (File.Exists(path) || Directory.Exists(path)) continue;

                    bool isAnyChanged = false;
                    foreach (var (subdata, slash) in new[]
                    {
                        ("\\\\" + ManageSettings.CurrentGameDirName + "\\\\Data", "/"),
                        ("\\\\" + ManageSettings.CurrentGameDirName + "\\\\Mods", "/"),
                        ("\\\\" + ManageSettings.CurrentGameDirName + "\\\\MO", "/"),
                    })
                    {
                        try
                        {
                            int index = path.ToUpperInvariant().IndexOf(subdata.ToUpperInvariant());
                            if (index == -1) continue;

                            var gamePath = Path.Combine(GetGameDirRootPath()).Replace("\\", "\\\\");
                            var fixedPath = $"{gamePath}{path.Substring(index)}";

                            data.Path = data.Path
                                .Remove(match.Index, match.Length)
                                .Insert(match.Index, fixedPath);

                            isAnyChanged = true;

                            // data.Path has just been rewritten and match.Index still points
                            // into the text as it was before. Matching again would splice a
                            // second, doubled path into it, so one match is fixed once and the
                            // next one is picked up by the enclosing loop.
                            break;
                        }
                        catch (Exception ex)
                        {
                            //not critical: the path is left as is when it cannot be rebuilt
                            _log.Warn("Failed to fix the custom exe path '" + data.Path + "'. Error:\r\n" + ex);
                        }
                    }

                    // One path in the arguments that cannot be rebuilt says nothing about the
                    // others, so the loop goes on with the next match instead of giving up.
                    if (!isAnyChanged) continue;

                    data.CustomExeData.Attribute[data.Attribute] = data.Path;
                }
            }
            else if (data.Attribute == "binary")
            {
                if (File.Exists(data.Path)) return;

                foreach (var (subdata, slash) in new[]
                {
                    ("/" + ManageSettings.CurrentGameDirName + "/data/" + ManageSettings.CurrentGame.GameExeName + ".exe", "/"),
                    ("/" + ManageSettings.CurrentGameDirName + "/data/" + ManageSettings.CurrentGame.GameExeNameX32 + ".exe", "/"),
                    ("/" + ManageSettings.CurrentGameDirName + "/data/" + ManageSettings.CurrentGame.GameExeNameVr + ".exe", "/"),
                    ("/" + ManageSettings.CurrentGameDirName + "/data/" + ManageSettings.CurrentGame.GameStudioExeName + ".exe", "/"),
                    ("/" + ManageSettings.CurrentGameDirName + "/data/" + ManageSettings.CurrentGame.GameStudioExeNameX32 + ".exe", "/"),
                    ("\\" + ManageSettings.CurrentGameDirName + "\\data\\" + ManageSettings.CurrentGame.GameExeName + ".exe", "\\"),
                    ("\\" + ManageSettings.CurrentGameDirName + "\\data\\" + ManageSettings.CurrentGame.GameExeNameX32 + ".exe", "\\"),
                    ("\\" + ManageSettings.CurrentGameDirName + "\\data\\" + ManageSettings.CurrentGame.GameExeNameVr + ".exe", "\\"),
                    ("\\" + ManageSettings.CurrentGameDirName + "\\data\\" + ManageSettings.CurrentGame.GameStudioExeName + ".exe", "\\"),
                    ("\\" + ManageSettings.CurrentGameDirName + "\\data\\" + ManageSettings.CurrentGame.GameStudioExeNameX32 + ".exe", "\\"),
                    ("////" + ManageSettings.CurrentGameDirName + "////data////" + ManageSettings.CurrentGame.GameExeName + ".exe", "////"),
                    ("////" + ManageSettings.CurrentGameDirName + "////data////" + ManageSettings.CurrentGame.GameExeNameX32 + ".exe", "////"),
                    ("////" + ManageSettings.CurrentGameDirName + "////data////" + ManageSettings.CurrentGame.GameExeNameVr + ".exe", "////"),
                    ("////" + ManageSettings.CurrentGameDirName + "////data////" + ManageSettings.CurrentGame.GameStudioExeName + ".exe", "////"),
                    ("////" + ManageSettings.CurrentGameDirName + "////data////" + ManageSettings.CurrentGame.GameStudioExeNameX32 + ".exe", "////"),
                })
                {
                    if (!data.Path.EndsWith(subdata, StringComparison.OrdinalIgnoreCase)) continue;

                    var gamePath = Path.Combine(GetGameDirRootPath()).Replace("////", "/").Replace("//", "/").Replace("\\", "/");
                    var fixedPath = $"{gamePath}/{subdata.Replace(slash, "/")}";

                    if (File.Exists(fixedPath)) data.CustomExeData.Attribute[data.Attribute] = CustomExecutables.NormalizePath(fixedPath);

                    break;
                }
            }
            else if (data.Attribute == "workingDirectory")
            {
                if (Directory.Exists(data.Path)) return;

                foreach (var (subdata, _) in new[]
                {
                    ("/" + ManageSettings.CurrentGameDirName + "/Data", "/"),
                    ("\\" + ManageSettings.CurrentGameDirName + "\\Data", "\\"),
                    ("////" + ManageSettings.CurrentGameDirName + "////Data", "////"),
                })
                {
                    if (!data.Path.EndsWith(subdata, StringComparison.OrdinalIgnoreCase)) continue;

                    var fixedPath = $"{ManageSettings.CurrentGameDirPath}/Data";

                    if (Directory.Exists(fixedPath)) data.CustomExeData.Attribute[data.Attribute] = CustomExecutables.NormalizePath(fixedPath);

                    break;
                }
            }
        }
    }
}
