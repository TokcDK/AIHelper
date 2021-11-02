using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIHelper.Manage.ModeSwitch
{
    abstract class ModeSwitcherBase
    {
        /// <summary>
        /// Text for switch confirmation dialog
        /// </summary>
        protected abstract string DialogText { get; }

        /// <summary>
        /// Method which will be executed
        /// </summary>
        protected abstract void Action();

        public async void Switch()
        {
            SharedData.GameData.MainForm.OnOffButtons(false);

            DialogResult result = MessageBox.Show(
                DialogText,
                T._("Confirmation"),
                MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                await Task.Run(() => Action()).ConfigureAwait(true);

                try
                {
                    SharedData.GameData.MainForm.MOCommonModeSwitchButton.Text = MOmode ? T._("MOToCommon") : T._("CommonToMO");
                }
                catch
                {
                }

                SharedData.GameData.MainForm.FoldersInit();
            }

            SharedData.GameData.MainForm.OnOffButtons();
        }

        protected virtual bool NeedSkip(string sourceFilePath, string sourceFolder)
        {
            return false;
        }

        protected virtual void PreParseFiles()
        {
        }

        protected bool MOmode { get => ManageSettings.IsMoMode(); set => Properties.Settings.Default.MOmode = value; }

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
                if (!File.Exists(Path.Combine(ManageSettings.GetCurrentGameDataDirPath(), "normal.mode")))
                {
                    File.WriteAllText(Path.Combine(ManageSettings.GetCurrentGameDataDirPath(), "normal.mode"), "The game is in normal mode");
                }
            }
            else
            {
                if (File.Exists(Path.Combine(ManageSettings.GetCurrentGameDataDirPath(), "normal.mode")))
                {
                    File.Delete(Path.Combine(ManageSettings.GetCurrentGameDataDirPath(), "normal.mode"));
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
            .Replace(ManageSettings.VarCurrentGameDataPath(), ManageSettings.GetCurrentGameDataDirPath())
            .Replace(ManageSettings.VarCurrentGameModsPath(), ManageSettings.GetCurrentGameModsDirPath())
            .Replace(ManageSettings.VarCurrentGameMoOverwritePath(), ManageSettings.GetCurrentGameMoOverwritePath());
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
                .Replace(ManageSettings.GetCurrentGameDataDirPath(), ManageSettings.VarCurrentGameDataPath())
                .Replace(ManageSettings.GetCurrentGameModsDirPath(), ManageSettings.VarCurrentGameModsPath())
                .Replace(ManageSettings.GetCurrentGameMoOverwritePath(), ManageSettings.VarCurrentGameMoOverwritePath());
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
                .Replace(ManageSettings.VarCurrentGameDataPath(), ManageSettings.GetCurrentGameDataDirPath())
                .Replace(ManageSettings.VarCurrentGameModsPath(), ManageSettings.GetCurrentGameModsDirPath())
                .Replace(ManageSettings.VarCurrentGameMoOverwritePath(), ManageSettings.GetCurrentGameMoOverwritePath());
            }
        }

        /// <summary>
        /// replace path to variable in string builder
        /// </summary>
        /// <param name="sb"></param>
        protected void ReplacePathsToVars(ref StringBuilder sb)
        {
            sb = sb
                .Replace(ManageSettings.GetCurrentGameDataDirPath(), ManageSettings.VarCurrentGameDataPath())
                .Replace(ManageSettings.GetCurrentGameModsDirPath(), ManageSettings.VarCurrentGameModsPath())
                .Replace(ManageSettings.GetCurrentGameMoOverwritePath(), ManageSettings.VarCurrentGameMoOverwritePath());
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
            var mOmodeDataFilesBakDirPath = ManageSettings.GetCurrentGameMOmodeDataFilesBakDirPath();
            if (!Directory.Exists(mOmodeDataFilesBakDirPath))
            {
                return;
            }

            Parallel.ForEach(Directory.GetFiles(mOmodeDataFilesBakDirPath, "*.*", SearchOption.AllDirectories), file =>
            {
                if (string.IsNullOrWhiteSpace(file))
                {
                    return;
                }

                var destFileInDataFolderPath = file.Replace(mOmodeDataFilesBakDirPath, ManageSettings.GetCurrentGameDataDirPath());
                if (File.Exists(destFileInDataFolderPath))
                {
                    return;
                }

                var destFileInDataFolderPathFolder = Path.GetDirectoryName(destFileInDataFolderPath);
                Directory.CreateDirectory(destFileInDataFolderPathFolder);

                file.MoveTo(destFileInDataFolderPath);
            });

            //удаление папки, где хранились резервные копии ванильных файлов
            ManageFilesFoldersExtensions.DeleteEmptySubfolders(mOmodeDataFilesBakDirPath);
        }
    }
}
