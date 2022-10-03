using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIHelper.Forms.Other;
using MAB.DotIgnore;
using NLog.Fluent;
using System.Windows.Forms;
using System.Drawing;
using NLog;

namespace AIHelper.Manage.Functions
{
    internal class DataDirCleaner
    {
        readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// clean Data dir of current selected game from not native files
        /// </summary>
        internal void Clean()
        {
            if (!ManageSettings.IsMoMode) return; // dont touch in common mode

            var options = new CleanOptionsDialogForm
            {
                Location = new Point(ManageSettings.MainForm.Location.X, ManageSettings.MainForm.Location.Y),
                StartPosition = FormStartPosition.Manual
            };

            DialogResult result = options.ShowDialog();
            if (result != DialogResult.OK) return;

            // set vars
            bool moveIntoNewMod = options.cbxMoveToNewMod.Checked;
            var cleanDataDirInfoPath = ManageSettings.CurrentGameCleanFunctionDirPath;
            var dataDipPath = ManageSettings.CurrentGameDataDirPath;
            int dataDipPathLength = dataDipPath.Length;
            var currentGameAbbr = ManageSettings.CurrentGame.GameAbbreviation;
            var blackListPath = ManageSettings.CurrentGameCleanFunctionBlackListFilePath;
            var whiteListPath = ManageSettings.CurrentGameCleanFunctionWhiteListFilePath;
            var dateTimeSuffix = ManageSettings.DateTimeBasedSuffix;
            var bakDir = Path.Combine(ManageSettings.CurrentGameBakDirPath, ManageSettings.CurrentGameFunctionsDirName, ManageSettings.CurrentGameCleanFunctionDirName, "bak" + dateTimeSuffix);

            var hardcodedWhiteList = new HashSet<string>()
            {
                string.IsNullOrWhiteSpace(ManageSettings.CurrentGameExeName)?"": $"{ManageSettings.CurrentGameExeName}.exe",
                string.IsNullOrWhiteSpace(ManageSettings.CurrentGameExeName)?"": $"{ManageSettings.CurrentGameExeName}_Data/",
                string.IsNullOrWhiteSpace(ManageSettings.StudioExeName)?"": $"{ManageSettings.StudioExeName}.exe",
                string.IsNullOrWhiteSpace(ManageSettings.StudioExeName)?"": $"{ManageSettings.StudioExeName}_Data/",
                string.IsNullOrWhiteSpace(ManageSettings.GameExeNameX32)?"": $"{ManageSettings.GameExeNameX32}.exe",
                string.IsNullOrWhiteSpace(ManageSettings.GameExeNameX32)?"": $"{ManageSettings.GameExeNameX32}_Data/",
                string.IsNullOrWhiteSpace(ManageSettings.GameExeNameVr)?"": $"{ManageSettings.GameExeNameVr}.exe",
                string.IsNullOrWhiteSpace(ManageSettings.GameExeNameVr)?"": $"{ManageSettings.GameExeNameVr}_Data/",
                string.IsNullOrWhiteSpace(ManageSettings.GameStudioExeNameX32)?"": $"{ManageSettings.GameStudioExeNameX32}.exe",
                string.IsNullOrWhiteSpace(ManageSettings.GameStudioExeNameX32)?"": $"{ManageSettings.GameStudioExeNameX32}_Data/",
                "abdata/",
                "*_Data/",
                "mono/",
                "manual/",
                "UserData/",
                "DefaultData/",
                "baselib.dll",
                "GameAssembly.dll",
                "InitSetting.exe",
                "InitSetting.exe.config",
                "UnityCrashHandler.exe",
                "UnityCrashHandler64.exe",
                "UnityPlayer.dll",
            };
            hardcodedWhiteList = hardcodedWhiteList.Where(s => s != "").ToHashSet(); // clean from empty records

            if (File.Exists(whiteListPath))
            {
                // add missing hardcoded masks
                var existsMasks = new HashSet<string>();
                var lines = File.ReadAllLines(whiteListPath);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (line.TrimStart().StartsWith("#")) continue;
                    if (hardcodedWhiteList.Contains(line)) continue;

                    existsMasks.Add(line);
                }

                // check for new hardcoded masks need to add
                var masksToAdd = new HashSet<string>();
                foreach (var mask in hardcodedWhiteList)
                {
                    if (existsMasks.Contains(mask)) continue;

                    masksToAdd.Add(mask);
                }

                // add new masks in the end of file if any need
                if (masksToAdd.Count > 0) File.WriteAllLines(whiteListPath, lines.Concat((new string[] { "\r\n\r\n" }).Concat(masksToAdd)));
            }
            else
            {
                // write hardcoded masks
                File.WriteAllText(whiteListPath, $"# Files and folders patterns which need to be saved\r\n# use .gitignore patterns: https://www.google.com/search?q=.gitignore+pattern\r\n\r\n{string.Join("\r\n", hardcodedWhiteList)}");
            }

            if (!File.Exists(blackListPath)) File.WriteAllText(blackListPath, "# Files and folders patterns which need to be removed\r\nHave higher priority than Whitelist\r\n# use .gitignore patterns: https://www.google.com/search?q=.gitignore+pattern\r\n\r\n");

            options.Dispose(); // release options form

            // fill lists
            var whiteList = new IgnoreList(whiteListPath);
            var blackList = new IgnoreList(blackListPath);

            // info vars
            int failedCount = 0;
            int movedDirsCount = 0;
            int movedFilesCount = 0;

            // create bak dir
            Directory.CreateDirectory(bakDir);

            foreach (var (list, isIgnored) in new[]
            {
                (whiteList, false),
                (blackList, true),
            })
            {
                // move dirs first
                foreach (var item in new DirectoryInfo(dataDipPath).EnumerateDirectories("*", SearchOption.AllDirectories).Where(i => (isIgnored ? list.IsIgnored(i) : !list.IsIgnored(i)) && i.Exists))
                    if (MoveItem(item, bakDir, dataDipPathLength)) { movedDirsCount++; } else { failedCount++; }
                // move files
                foreach (var item in new DirectoryInfo(dataDipPath).EnumerateFiles("*.*", SearchOption.AllDirectories).Where(i => (isIgnored ? list.IsIgnored(i) : !list.IsIgnored(i)) && i.Exists))
                    if (MoveItem(item, bakDir, dataDipPathLength)) { movedFilesCount++; } else { failedCount++; }
            }

            // remove bak dir when empty
            if (movedDirsCount == 0 && movedFilesCount == 0 && !bakDir.IsAnyFileExistsInTheDir()) Directory.Delete(bakDir);

            MessageBox.Show(T._($"Cleaning finished! Moved {movedDirsCount} dirs and {movedFilesCount} files. Failed to move {failedCount} items."));
        }

        bool MoveItem(FileSystemInfo item, string dir2move, int dataDipPathLength)
        {
            try
            {
                var targetPath = $"{dir2move}{item.FullName.Substring(dataDipPathLength)}";
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath)); // create parent dir

                if (item is DirectoryInfo di) { di.MoveTo(targetPath); } else (item as FileInfo).MoveTo(targetPath);

                return true;
            }
            catch (IOException ex)
            {
                _log.Error($"{nameof(DataDirCleaner)}/{nameof(Clean)}: Failed to move dir '{item.FullName}'. Error:\r\n{ex}");
                return false;
            }
        }
    }
}
