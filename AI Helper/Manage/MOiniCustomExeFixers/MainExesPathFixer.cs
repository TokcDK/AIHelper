using System;
using System.IO;
using System.Text.RegularExpressions;
using static AIHelper.Manage.ManageModOrganizer;

namespace AIHelper.Manage.MOiniCustomExeFixers
{
    public class MainExesPathFixer : ICustomExePathFixerBase
    {
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

                            var gamePath = Path.Combine(ManageSettings.CurrentGameDirPath.Remove(ManageSettings.CurrentGameDirPath.IndexOf("\\" + ManageSettings.CurrentGameDirName))).TrimEnd('\\').Replace("\\", "\\\\");
                            var fixedPath = $"{gamePath}{path.Substring(index)}";

                            data.Path = data.Path
                                .Remove(match.Index, match.Length)
                                .Insert(match.Index, fixedPath);

                            isAnyChanged = true;
                        }
                        catch { }
                    }

                    if (!isAnyChanged) return;

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
                    if (!data.Path.EndsWith(subdata, StringComparison.InvariantCultureIgnoreCase)) continue;

                    var gamePath = Path.Combine(ManageSettings.CurrentGameDirPath.Remove(ManageSettings.CurrentGameDirPath.IndexOf("\\" + ManageSettings.CurrentGameDirName))).Replace("////", "/").Replace("//", "/").Replace("\\", "/");
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
                    if (!data.Path.EndsWith(subdata, StringComparison.InvariantCultureIgnoreCase)) continue;

                    var fixedPath = $"{ManageSettings.CurrentGameDirPath}/Data";

                    if (Directory.Exists(fixedPath)) data.CustomExeData.Attribute[data.Attribute] = CustomExecutables.NormalizePath(fixedPath);

                    break;
                }
            }
        }
    }
}
