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

        protected static Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Text for switch confirmation dialog
        /// </summary>
        protected abstract string DialogText { get; }

        /// <summary>
        /// Method which will be executed
        /// </summary>
        protected abstract void Action();

        /// <summary>
        /// When switch to normal mode make backup of Data and Mods dirs using ntfs hard links
        /// </summary>
        public bool MakeBuckup = true;

        public async void Switch()
        {
            ManageSettings.MainForm.OnOffButtons(false);

            SwitchModeDialogForm dialog = new SwitchModeDialogForm
            {
                Location = new Point(ManageSettings.MainForm.Location.X, ManageSettings.MainForm.Location.Y),
                StartPosition = FormStartPosition.Manual
            };

            ThemesLoader.SetTheme(ManageSettings.CurrentTheme, dialog);
            DialogResult result = dialog.ShowDialog();
            if (result != DialogResult.OK)
            {
                ManageSettings.MainForm.OnOffButtons();
                return;
            }

            MakeBuckup = dialog.MakeBuckupCheckBox.Checked;

            if (!dialog.DoNotSwitchCheckBox.Checked)
            {
                // commented for tests
                // await Task.Run(() => Action()).ConfigureAwait(true);
            }

            //try
            //{
            //    ManageSettings.MainForm.MOCommonModeSwitchButton.Text = MOmode ? T._("MOToCommon") : T._("CommonToMO");
            //}
            //catch (Exception ex)
            //{
            //    _log.Info("An error occured while change mode switch button text: \r\n" + ex);
            //}

            ManageSettings.MainForm.UpdateData();

            //DialogResult result = MessageBox.Show(
            //    DialogText,
            //    T._("Confirmation"),
            //    MessageBoxButtons.OKCancel);
            //if (result == DialogResult.OK)
            //{
            //    await Task.Run(() => Action()).ConfigureAwait(true);

            //    try
            //    {
            //        ManageSettings.MainForm.MOCommonModeSwitchButton.Text = MOmode ? T._("MOToCommon") : T._("CommonToMO");
            //    }
            //    catch
            //    {
            //    }

            //    ManageSettings.MainForm.FoldersInit();
            //}

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
