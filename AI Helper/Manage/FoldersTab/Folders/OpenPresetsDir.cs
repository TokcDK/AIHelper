using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using AIHelper.Manage.Functions;

namespace AIHelper.Manage.FoldersTab
{
    internal class OpenPresetsDir : IFolderTabButtonData
    {
        public virtual string Text => T._("Cards");

        public virtual string Description => T._("Open dir with characters cards");

        public void OnClick(object o, EventArgs e)
        {
            OpenPresetDirs(IsMO);
        }

        protected virtual bool IsMO { get; } = false;

        private static async void OpenPresetDirs(bool IsMO = false)
        {
            await Task.Run(() => ManageOther.WaitIfGameIsChanging()).ConfigureAwait(true);

            string CharacterDirSubpath = ManageSettings.Games.Game.CharacterPresetsFolderSubPath;

            string exePath = IsMO ? Path.Combine(ManageSettings.AppModOrganizerDirPath, "explorer++", "Explorer++.exe") : "explorer.exe";
            string presetsDirPath = IsMO && ManageSettings.IsMoMode ? Path.Combine(ManageSettings.CurrentGameDataDirPath) + "\\" + CharacterDirSubpath : ManageSettings.GetUserfilesDirectoryPath(CharacterDirSubpath);

            Directory.CreateDirectory(presetsDirPath);

            if (IsMO && ManageSettings.IsMoMode)
            {
                ManageSettings.MainForm.OnOffButtons(false);

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
            }
            else
            {
                Process.Start(exePath, presetsDirPath);
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
