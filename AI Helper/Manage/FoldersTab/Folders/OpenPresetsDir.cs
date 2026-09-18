using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using AIHelper.Manage.Functions;
using NLog;

namespace AIHelper.Manage.FoldersTab
{
    internal class OpenPresetsDir : IFolderTabButtonData
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public virtual string Text => T._("Cards");

        public virtual string Description => T._("Open dir with characters cards");

        public void OnClick(object o, EventArgs e)
        {
            OpenPresetDirs(IsMO);
        }

        protected virtual bool IsMO { get; } = false;

        private static async void OpenPresetDirs(bool IsMO = false)
        {
            //NOTE: this stays async void because OnClick is a synchronous event handler and cannot await it.
            //An exception thrown after the first await of an async void method cannot be caught by the caller
            //and would crash the process, so the whole body must be guarded here.
            bool areButtonsDisabled = false;

            try
            {
                await Task.Run(() => ManageOther.WaitIfGameIsChanging()).ConfigureAwait(true);

                string CharacterDirSubpath = ManageSettings.Games.Game.CharacterPresetsFolderSubPath;

                string exePath = IsMO ? Path.Combine(ManageSettings.AppModOrganizerDirPath, "explorer++", "Explorer++.exe") : "explorer.exe";
                string presetsDirPath = IsMO && ManageSettings.IsMoMode ? Path.Combine(ManageSettings.CurrentGameDataDirPath) + "\\" + CharacterDirSubpath : ManageSettings.GetUserfilesDirectoryPath(CharacterDirSubpath);

                Directory.CreateDirectory(presetsDirPath);

                if (IsMO && ManageSettings.IsMoMode)
                {
                    ManageSettings.MainForm.OnOffButtons(false);
                    areButtonsDisabled = true;

                    string customExeTitleName = "Cards";
                    string gameUserDataModName = ManageSettings.GameUserDataModName;
                    // Set new app open presets dir profile in mo
                    var explorerPresetsDir = new ManageModOrganizer.CustomExecutables.CustomExecutable
                    {
                        Title = customExeTitleName,
                        Binary = exePath,
                        Arguments = presetsDirPath,
                        MoTargetMod = gameUserDataModName
                    };

                    var GameUserDataFilesModPath = Path.Combine(ManageSettings.CurrentGameModsDirPath, gameUserDataModName);
                    if (!Directory.Exists(GameUserDataFilesModPath))
                    {
                        Directory.CreateDirectory(GameUserDataFilesModPath);
                        ManageModOrganizer.WriteMetaIni(
                            GameUserDataFilesModPath,
                            categoryNames: "UserData",
                            version: "1.0",
                            comments: "",
                            notes: T._("New files created by the game will be stored here. Saves, plugins configs and other.")
                            //notes: ManageSettings.KKManagerFilesNotes()
                            );

                        ManageModOrganizer.InsertMod(
                            modname: gameUserDataModName,
                            modAfterWhichInsert: "UserData_separator"
                            );
                    }

                    ManageModOrganizer.InsertCustomExecutable(explorerPresetsDir);

                    exePath = ManageSettings.AppMOexePath;
                    presetsDirPath = "moshortcut://:\"" + customExeTitleName + "\"";

                    ManageProcess.RunProgramAndWaitHidden(exePath, presetsDirPath);

                    ManageSettings.MainForm.OnOffButtons();
                    areButtonsDisabled = false;
                }
                else
                {
                    Process.Start(exePath, presetsDirPath);
                }
            }
            catch (Exception ex)
            {
                _log.Error("Failed to open the presets dir. Error:\r\n" + ex);
            }
            finally
            {
                //never leave the main form disabled after a failure
                if (areButtonsDisabled) ManageSettings.MainForm.OnOffButtons();
            }
        }
    }
    internal class OpenPresetsDirMO : OpenPresetsDir
    {
        public override string Text => base.Text + "MO";

        public override string Description => base.Description + " " + T._("using Mod Organizer");

        protected override bool IsMO => true;
    }
}
