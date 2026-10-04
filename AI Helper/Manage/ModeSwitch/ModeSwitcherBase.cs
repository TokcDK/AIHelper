using AIHelper.Forms.Other;
using AIHelper.Manage.ui.themes;
using NLog;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIHelper.Manage.ModeSwitch
{
    abstract class ModeSwitcherBase
    {
        protected class ParentSourceModData
        {
            public string Path { get; }
            public bool IsSymlink = false;

            public ParentSourceModData(string path)
            {
                Path = path;
            }
        }

        protected static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Text for switch confirmation dialog
        /// </summary>
        protected abstract string DialogText { get; }

        /// <summary>
        /// Method which will be executed
        /// </summary>
        protected abstract void Action();

        /// <summary>
        /// Asks for confirmation, takes the backup the user asked for and then hands over to
        /// the concrete switcher.
        /// </summary>
        /// <remarks>
        /// The switch itself is currently disabled: <see cref="Action"/> is not invoked, so
        /// this only refreshes the window after the backup. The call site is left in place,
        /// commented out, to keep the place where it belongs.
        /// </remarks>
        internal void Switch()
        {
            ManageSettings.MainForm.OnOffButtons(false);

            bool makeBackup;
            bool doNotSwitch;

            // The dialog owns a window handle and the tooltip for its controls, so it is
            // released as soon as it has been answered.
            using (var dialog = new SwitchModeDialogForm
            {
                Location = new Point(ManageSettings.MainForm.Location.X, ManageSettings.MainForm.Location.Y),
                StartPosition = FormStartPosition.Manual
            })
            {
                ThemesLoader.SetTheme(ManageSettings.CurrentTheme, dialog);

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    ManageSettings.MainForm.OnOffButtons();
                    return;
                }

                // The dialog's answers are the only things which decide whether the Data and
                // Mods dirs are backed up with ntfs hard links and whether the switch runs.
                makeBackup = dialog.MakeBuckupCheckBox.Checked;
                doNotSwitch = dialog.DoNotSwitchCheckBox.Checked;
            }

            // First, create a backup of the game
            if (makeBackup)
                new GameBackuper().CreateDataModsBakOfCurrentGame();

            if (!doNotSwitch)
            {
                // The switch is disabled on purpose. This is where it belongs when it is
                // turned back on, guarded by the dialog's "do not switch" checkbox:
                // Task.Run(() => Action()).ConfigureAwait(true);
            }

            ManageSettings.MainForm.UpdateData();

            ManageSettings.MainForm.OnOffButtons();
        }

        protected virtual bool NeedSkip(string sourceFilePath, ParentSourceModData parentSourceModDir)
        {
            return false;
        }

        protected virtual void PreParseFiles()
        {
        }

        protected bool MOmode { get => ManageSettings.IsMoMode; set => ManageSettings.IsMoMode = value; }

        /// <summary>
        /// Locker for <see cref="AppendLineThreadSafe"/>. The mode switch parsers run inside
        /// <see cref="System.Threading.Tasks.Parallel"/>, so any shared state they touch must be locked.
        /// </summary>
        private readonly object _sharedStateLocker = new object();

        /// <summary>
        /// Append <paramref name="line"/> to <paramref name="target"/> in a thread safe way.
        /// <see cref="StringBuilder"/> is not thread safe, so parallel appends can corrupt its content or throw.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="line"></param>
        protected void AppendLineThreadSafe(StringBuilder target, string line)
        {
            lock (_sharedStateLocker)
            {
                target.AppendLine(line);
            }
        }

        /// <summary>
        /// Execute <paramref name="action"/> while holding the shared state lock, so a parallel parser
        /// cannot interleave a read-modify-write cycle on a shared file (for example modlist.txt)
        /// </summary>
        /// <param name="action"></param>
        protected void RunWithSharedStateLock(Action action)
        {
            lock (_sharedStateLocker)
            {
                action();
            }
        }

        protected static string operationsSplitStringBase { get => "|MovedTo|"; }
        protected string[] operationsSplitString = new string[1] { operationsSplitStringBase };

        /// <summary>
        /// normal mode identifier switcher
        /// </summary>
        /// <param name="create">true=Create/false=Delete</param>
        protected void SwitchNormalModeIdentifier(bool create = true)
        {
            if (create)
            {
                if (!File.Exists(Path.Combine(ManageSettings.CurrentGameDataDirPath, "normal.mode")))
                {
                    File.WriteAllText(Path.Combine(ManageSettings.CurrentGameDataDirPath, "normal.mode"), "The game is in normal mode");
                }
            }
            else
            {
                if (File.Exists(Path.Combine(ManageSettings.CurrentGameDataDirPath, "normal.mode")))
                {
                    File.Delete(Path.Combine(ManageSettings.CurrentGameDataDirPath, "normal.mode"));
                }
            }
        }

        /// <summary>
        /// replace variable to path in string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        protected string ReplaceVarsToPaths(string str)
        {
            return str
            .Replace(ManageSettings.VarCurrentGameDataPath, ManageSettings.CurrentGameDataDirPath)
            .Replace(ManageSettings.VarCurrentGameModsPath, ManageSettings.CurrentGameModsDirPath)
            .Replace(ManageSettings.VarCurrentGameMoOverwritePath, ManageSettings.CurrentGameMoOverwritePath);
        }

        /// <summary>
        /// replace path to variable in string array
        /// </summary>
        /// <param name="sarr"></param>
        protected void ReplacePathsToVars(ref string[] sarr)
        {
            for (int i = 0; i < sarr.Length; i++)
            {
                sarr[i] = sarr[i]
                .Replace(ManageSettings.CurrentGameDataDirPath, ManageSettings.VarCurrentGameDataPath)
                .Replace(ManageSettings.CurrentGameModsDirPath, ManageSettings.VarCurrentGameModsPath)
                .Replace(ManageSettings.CurrentGameMoOverwritePath, ManageSettings.VarCurrentGameMoOverwritePath);
            }
        }

        /// <summary>
        /// replace variable to path in string array
        /// </summary>
        /// <param name="sarr"></param>
        protected void ReplaceVarsToPaths(ref string[] sarr)
        {
            for (int i = 0; i < sarr.Length; i++)
            {
                sarr[i] = sarr[i]
                .Replace(ManageSettings.VarCurrentGameDataPath, ManageSettings.CurrentGameDataDirPath)
                .Replace(ManageSettings.VarCurrentGameModsPath, ManageSettings.CurrentGameModsDirPath)
                .Replace(ManageSettings.VarCurrentGameMoOverwritePath, ManageSettings.CurrentGameMoOverwritePath);
            }
        }

        /// <summary>
        /// replace path to variable in string builder
        /// </summary>
        /// <param name="sb"></param>
        protected void ReplacePathsToVars(ref StringBuilder sb)
        {
            sb = sb
                .Replace(ManageSettings.CurrentGameDataDirPath, ManageSettings.VarCurrentGameDataPath)
                .Replace(ManageSettings.CurrentGameModsDirPath, ManageSettings.VarCurrentGameModsPath)
                .Replace(ManageSettings.CurrentGameMoOverwritePath, ManageSettings.VarCurrentGameMoOverwritePath);
        }

        ///// <summary>
        ///// replace variable to path in string builder
        ///// </summary>
        ///// <param name="sb"></param>
        //static void ReplaceVarsToPaths(ref StringBuilder sb)
        //{
        //    sb = sb
        //        .Replace(ManageSettings.VarCurrentGameDataPath(), ManageSettings.GetCurrentGameDataPath())
        //        .Replace(ManageSettings.VarCurrentGameModsPath(), ManageSettings.GetCurrentGameModsDirPath())
        //        .Replace(ManageSettings.VarCurrentGameMoOverwritePath(), ManageSettings.GetCurrentGameMoOverwritePath());
        //}

        //static bool OfferToSkipTheFileConfirmed(string file)
        //{
        //    DialogResult result = MessageBox.Show(
        //        T._("Path to file is too long!") + Environment.NewLine
        //        + "(" + file + ")" + Environment.NewLine
        //        + T._("Long Path can cause mode switch error and mode will not be switched.") + Environment.NewLine
        //        + T._("Skip it?") + Environment.NewLine
        //        , T._("Too long file path"), MessageBoxButtons.YesNo);

        //    return result == DialogResult.Yes;
        //}

        protected void MoveVanillaFilesBackToData()
        {
            var mOmodeDataFilesBakDirPath = ManageSettings.CurrentGameMOmodeDataFilesBakDirPath;
            if (!Directory.Exists(mOmodeDataFilesBakDirPath)) return;

            Parallel.ForEach(Directory.GetFiles(mOmodeDataFilesBakDirPath, "*.*", SearchOption.AllDirectories), file =>
            {
                if (string.IsNullOrWhiteSpace(file)) return;

                var destFileInDataFolderPath = file.Replace(mOmodeDataFilesBakDirPath, ManageSettings.CurrentGameDataDirPath);
                if (File.Exists(destFileInDataFolderPath)) return;

                var destFileInDataFolderPathFolder = Path.GetDirectoryName(destFileInDataFolderPath);
                Directory.CreateDirectory(destFileInDataFolderPathFolder);

                file.MoveTo(destFileInDataFolderPath);
            });

            //удаление папки, где хранились резервные копии ванильных файлов
            ManageFilesFoldersExtensions.DeleteEmptySubfolders(mOmodeDataFilesBakDirPath);
        }
    }
}
