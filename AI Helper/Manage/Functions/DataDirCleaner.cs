using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AIHelper.Data.Modlist;
using AIHelper.Forms.Other;
using AIHelper.Data.Modlist;
using MAB.DotIgnore;
using NLog;
using static AIHelper.Manage.ManageModOrganizer;

namespace AIHelper.Manage.Functions
{
    internal class DataDirCleaner : FunctionForFlpBase
    {
        readonly Logger _log = LogManager.GetCurrentClassLogger();

        public override string Symbol => "🧹";

        public override string Description => T._("Clean game dir from external files using black and white lists");

        public override void OnClick(object o, EventArgs e)
        {
            Clean();
        }

        /// <summary>
        /// clean Data dir of current selected game from not native files
        /// </summary>
        internal void Clean()
        {
            if (!ManageSettings.IsMoMode) return; // dont touch in common mode
            var options = new DialogFormGeneral
            {
                Location = new Point(ManageSettings.MainForm.Location.X, ManageSettings.MainForm.Location.Y),
                StartPosition = FormStartPosition.Manual
            };

            // option place into form
            var cbxMoveIntoNewMod = new CheckBox();
            cbxMoveIntoNewMod.Text = T._("Move into new mod");
            cbxMoveIntoNewMod.AutoSize = true;
            var cbxIgnoreSymlinks = new CheckBox();
            cbxIgnoreSymlinks.Text = T._("Ignore symlinks");
            cbxIgnoreSymlinks.AutoSize = true;
            cbxIgnoreSymlinks.Checked = true;
            var cbxIgnoreShortcuts = new CheckBox();
            cbxIgnoreShortcuts.Text = T._("Ignore shortcurs");
            cbxIgnoreShortcuts.AutoSize = true;
            options.flpOptions.Controls.Add(cbxMoveIntoNewMod);
            options.flpOptions.Controls.Add(cbxIgnoreSymlinks);
            options.flpOptions.Controls.Add(cbxIgnoreShortcuts);

            //var options = new CleanOptionsDialogForm
            //{
            //    Location = new Point(ManageSettings.MainForm.Location.X, ManageSettings.MainForm.Location.Y),
            //    StartPosition = FormStartPosition.Manual
            //};

            DialogResult result = options.ShowDialog();
            if (result != DialogResult.OK) return;

            // test
            //options.btnStart.Enabled = false;
            //options.btnCancel.Enabled = false;
            //TextBox tbxInfo = new TextBox();
            //tbxInfo.ReadOnly = true;
            //options.flpOptions.Controls.Add(tbxInfo);
            //tbxInfo.Text = T._("Read lists");

            // set vars
            bool isMoveIntoNewMod = cbxMoveIntoNewMod.Checked;
            bool isIgnoreSymlinks = cbxIgnoreSymlinks.Checked;
            bool isIgnoreShortcuts = cbxIgnoreShortcuts.Checked;
            var cleanDataDirInfoPath = ManageSettings.CurrentGameCleanFunctionDirPath;
            var dataDipPath = ManageSettings.CurrentGameDataDirPath;
            int dataDipPathLength = dataDipPath.Length;
            var currentGameAbbr = ManageSettings.CurrentGame.GameAbbreviation;
            var blackListPath = ManageSettings.CurrentGameCleanFunctionBlackListFilePath;
            var whiteListPath = ManageSettings.CurrentGameCleanFunctionWhiteListFilePath;
            var dateTimeSuffix = ManageSettings.DateTimeBasedSuffix;
            var bakDir = Path.Combine(ManageSettings.CurrentGameBakDirPath, ManageSettings.CurrentGameFunctionsDirName, ManageSettings.CurrentGameCleanFunctionDirName, "bak" + dateTimeSuffix);

            options.Dispose(); // release options form

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
                "cubemaps/", // honey select 2
                "manual/",
                "manual_*/",
                "MonoBleedingEdge/",
                "UserData/",
                "DefaultData/",
                "baselib.dll", // room girl
                "GameAssembly.dll",
                "InitSetting.exe",
                "InitSetting*.exe",
                "InitSetting.exe.config",
                "UnityCrashHandler.exe",
                "UnityCrashHandler64.exe",
                "UnityPlayer.dll",
                // com3d2
                "GameData/",
                "GameData_*/",
                "IMG/",
                "Preset/",
                "update.cfg",
                "update.lst",
                "system.dat",
                "uninst.dat",
                "uninst.exe",
            };
            hardcodedWhiteList = hardcodedWhiteList.Where(s => s != "").ToHashSet(); // clean from empty records

            Directory.CreateDirectory(Path.GetDirectoryName(whiteListPath)); // create parent dir
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

            // fill lists
            var whiteList = new IgnoreList(whiteListPath);
            var blackList = new IgnoreList(blackListPath);

            // when move into new mod is set, setup new mod

            var bakDirBaseName = "DataFiles";
            if (isMoveIntoNewMod)
            {
                bakDir = Path.Combine(ManageSettings.CurrentGameModsDirPath, bakDirBaseName + dateTimeSuffix);
            }

            // info vars
            int errors = 0;
            int dirs = 0;
            int files = 0;

            // create bak dir
            Directory.CreateDirectory(bakDir);

            // move files
            //tbxInfo.Text = T._($"Move files: D{dirs}/F{files}/!{errors}");
            foreach (var (list, isIgnored) in new[]
            {
                (whiteList, false),
                (blackList, true),
            })
            {
                // move dirs first
                foreach (var item in new DirectoryInfo(dataDipPath).EnumerateDirectories("*", SearchOption.AllDirectories)
                    .Where(i => (isIgnored ? list.IsIgnored(i) : !list.IsIgnored(i)) && i.Exists && (!isIgnoreSymlinks || !i.IsSymlink())))
                {
                    if (MoveItem(item, bakDir, dataDipPathLength)) { dirs++; } else { errors++; }
                    //tbxInfo.Text = T._($"Move files: D{dirs}/F{files}/!{errors}");
                }
                // move files
                foreach (var item in new DirectoryInfo(dataDipPath).EnumerateFiles("*.*", SearchOption.AllDirectories)
                    .Where(i => (isIgnored ? list.IsIgnored(i) : !list.IsIgnored(i)) && i.Exists && (!isIgnoreSymlinks || !i.IsSymlink()) && (!isIgnoreShortcuts || i.Extension != ".lnk")))
                {
                    if (MoveItem(item, bakDir, dataDipPathLength)) { files++; } else { errors++; }
                    //tbxInfo.Text = T._($"Move files: D{dirs}/F{files}/!{errors}");
                }
            }

            bool isAnyItemsWasMoved = !(dirs == 0 && files == 0 && !bakDir.IsAnyFileExistsInTheDir());

            // remove bak dir when empty
            if (!isAnyItemsWasMoved && bakDir.IsEmptyDir())
            {
                Directory.Delete(bakDir);
            }
            else if (isAnyItemsWasMoved)
            {
                if (isMoveIntoNewMod)
                {
                    var modlist = new ModlistData();

                    var mod = new ModData();
                    mod.IsEnabled = true;
                    mod.Path = bakDir;
                    mod.Name = Path.GetFileName(bakDir);

                    ModData modAfter = default;
                    var existBakDirs = modlist.GetListBy(ModlistData.ModType.ModAny).Where(m => Regex.IsMatch(m.Name, bakDirBaseName + "_[0-9]{14}")).ToArray();
                    if (existBakDirs.Length > 0)
                    {
                        modAfter = existBakDirs.OrderByDescending(m => long.Parse(m.Name.Split('_')[1])).FirstOrDefault();
                    }
                    else mod.Priority = 0;

                    modlist.Insert(mod, modNameToPlaceWith: modAfter != default ? modAfter.Name : "");
                }
            }

            // show result message
            MessageBox.Show(string.Format(T._("Cleaning finished! Moved {0} dirs and {1} files. Failed to move {2} items."), dirs, files, errors) + (isAnyItemsWasMoved ? "\r\n" + T._("Will be opened the folder where files was moved. You can remove all you dont need.") : ""));

            // open bak dir where files and dirs was moved
            if (isAnyItemsWasMoved) Process.Start("explorer.exe", bakDir);
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
