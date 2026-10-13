using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AIHelper.Data.Modlist;
using AIHelper.Forms.Other;
using AIHelper.Manage.ui.themes;
using AIHelper.Manage.Update;
using NLog;
using static AIHelper.Manage.ManageModOrganizer;

namespace AIHelper.Manage
{
    /// <summary>
    /// Runs the update pass: AI Helper, Mod Organizer and the plugins through the updater, and
    /// the zipmods through the KKManager standalone updater followed by a sort of what it left
    /// behind.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is the pass and nothing else. What has to be moved and where is
    /// <see cref="ZipmodsSorter"/>, the window which is shown while a step runs is
    /// <see cref="UpdateProgressForm"/>, and the updater - the application, Mod Organizer and
    /// the plugins alike - is <see cref="Updater"/>. What is left here is the order the steps
    /// run in, and the cleanup which has to happen whatever that order came to.
    /// </para>
    /// <para>
    /// The pass changes things which outlive it - the modlist is force enabled and a window
    /// is put on screen - so both are undone in one cleanup instead of at the end of the
    /// happy path. A pass which stopped half way used to leave the window up and every mod
    /// with a sideloader modpack enabled, with nothing on screen to say why.
    /// </para>
    /// </remarks>
    class ManageUpdateMods
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Suffix of the copy of the modlist which is taken before the mods are force
        /// enabled. It is the file <see cref="ManageModOrganizer.RestoreModlist"/> puts back.
        /// </summary>
        private const string ModlistBackupSuffix = ".prezipmodsUpdate";

        /// <summary>
        /// Name without extension of the KKManager standalone updater executable, which is
        /// how Mod Organizer's list of custom executables is searched.
        /// </summary>
        private const string StandaloneUpdaterExeName = "StandaloneUpdater";

        /// <summary>
        /// Title the standalone updater is registered under in Mod Organizer, and therefore
        /// the title of the shortcut the pass starts it through.
        /// </summary>
        private const string StandaloneUpdaterTitle = "KKManagerStandaloneUpdater";

        /// <summary>
        /// Version written into the meta.ini of the mod which collects the files the updater
        /// creates for itself.
        /// </summary>
        private const string KKManagerFilesModVersion = "1.0";

        /// <summary>
        /// Runs the pass, after asking which parts of it to run.
        /// </summary>
        public static async Task UpdateMods()
        {
            // the dialog is kept for the whole pass rather than for the question alone: the
            // updater reads the choices the user made from it
            using (var updateOptions = new UpdateOptionsDialogForm
            {
                Location = new Point(ManageSettings.MainForm.Location.X, ManageSettings.MainForm.Location.Y),
                StartPosition = FormStartPosition.Manual
            })
            {
                ThemesLoader.SetTheme(ManageSettings.CurrentTheme, updateOptions);

                if (updateOptions.ShowDialog() != DialogResult.OK) return;

                ManageSettings.MainForm.AIGirlHelperTabControl.Enabled = false;

                try
                {
                    // The updater covers the application, Mod Organizer and the plugins in one
                    // window, so it is started when either of the two steps it answers for was
                    // chosen. A pass with nothing in it opens no window at all.
                    if (updateOptions.UpdatePluginsCheckBox.Checked || updateOptions.UpdateAppCheckBox.Checked)
                    {
                        await UpdateByUpdater(updateOptions).ConfigureAwait(true);
                    }

                    if (updateOptions.UpdateZipmodsCheckBox.Checked)
                    {
                        await UpdateZipmods().ConfigureAwait(true);
                    }
                }
                finally
                {
                    // the window is usable again whatever the pass did. Leaving it disabled
                    // is what makes a failure look like the application has frozen.
                    ManageSettings.MainForm.UpdateData();
                    ManageSettings.MainForm.AIGirlHelperTabControl.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Updates the application, Mod Organizer and the plugins through the updater.
        /// </summary>
        /// <remarks>
        /// The pass is made of what this installation can actually update: the mods need a managed
        /// installation, while the application needs neither Mod Organizer nor a game. The updater
        /// works that out from the mode, so this is one call whether or not Mod Organizer is here.
        /// </remarks>
        private static async Task UpdateByUpdater(UpdateOptionsDialogForm updateOptions)
        {
            ManageOther.SwitchFormMinimizedNormalAll(ManageSettings.ListOfFormsForMinimize);

            try
            {
                var updater = new Updater(updateOptions);

                await updater.Update().ConfigureAwait(true);

                if (updater.UpdatedAny && ManageSettings.IsMoMode)
                {
                    // the data of the managed installation is rebuilt only when something was
                    // updated, because it is the update which can make it stale - and only when
                    // there is a managed installation to rebuild
                    ManageModOrganizer.CleanMoFolder();
                    ManageModOrganizer.CheckBaseGamesPy();
                    ManageModOrganizer.RedefineGameMoData();
                }
            }
            finally
            {
                ManageOther.SwitchFormMinimizedNormalAll(ManageSettings.ListOfFormsForMinimize);
            }
        }

        /// <summary>
        /// Runs the KKManager standalone updater and puts what it leaves behind where it
        /// belongs.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Everything from the moment the window is opened runs inside one cleanup, so that
        /// the window is closed and the modlist is put back whether the pass finished,
        /// failed, or was stopped from inside the updater.
        /// </para>
        /// <para>
        /// The sort runs even when the updater was stopped early. What it wrote before that
        /// is still in the wrong place, and putting it right is the whole point of the sort.
        /// </para>
        /// </remarks>
        private static Task UpdateZipmods()
        {
            // the check and the sort are both about sideloader modpacks, which only some of
            // the games have
            if (!ManageSettings.Games.Game.IsHaveSideloaderMods) return Task.CompletedTask;

            // without the updater there is nothing to run
            if (!File.Exists(ManageSettings.KkManagerStandaloneUpdaterExePath)) return Task.CompletedTask;

            if (!ManageSettings.IsMoMode)
            {
                // there is no mod organizer, so the updater is pointed at the game data
                // directory and what it writes there stays there
                RunUpdater(
                    ManageSettings.KkManagerStandaloneUpdaterExePath,
                    "\"" + ManageSettings.CurrentGameDataDirPath + "\"");

                return Task.CompletedTask;
            }

            // read once: the property creates the overwrite folder as a side effect, and
            // every step of the pass would otherwise read it again
            var sortDirPaths = ManageSettings.KKManagerUpdateSortDirs.ToArray();

            EnsureStandaloneUpdaterIsRegistered();

            UpdateProgressForm progressForm = null;
            var isModlistModified = false;

            try
            {
                progressForm = UpdateProgressForm.Open(ManageSettings.MainForm, T._("Update zipmods"));

                progressForm.SetStatus(T._("Getting zipmods info") + "..");

                // which modpack a zipmod belongs to is decided from the zipmods which are
                // installed, so the list has to be built before the updater changes anything
                var zipmodsGuidList = new ZipmodGUIIds();

                progressForm.SetStatus(T._("Preactivate mods with Sideloader packs") + "..");

                // the updater writes into the mods, so the mods it writes into have to be
                // enabled first
                BackupModlist();
                isModlistModified = true;

                EnableSideloaderMods();

                // a download which failed earlier leaves the updater looking for a directory
                // it did not create itself, and it refuses to start without one
                Directory.CreateDirectory(ManageSettings.KKManagerDownloadsTempDir);

                progressForm.SetStatus(T._("Update zipmods") + "..");

                RunUpdater(
                    ManageSettings.AppMOexePath,
                    "moshortcut://:" + ManageModOrganizer.GetMOcustomExecutableTitleByExeName(StandaloneUpdaterExeName));

                // put back before the sort, so the mods are enabled only for as long as the
                // updater runs
                ManageModOrganizer.RestoreModlist();
                isModlistModified = false;

                progressForm.SetStatus(T._("Sorting zipmods") + "..");

                new ZipmodsSorter(
                    zipmodsGuidList,
                    sortDirPaths,
                    ManageSettings.CurrentGameModsDirPath,
                    ManageSettings.CurrentGameDataDirPath,
                    ManageSettings.DateTimeBasedSuffix)
                    .Sort();
            }
            finally
            {
                // the window goes first: it is the visible half of the cleanup, and it must
                // not be left behind by a failure in the other half
                if (progressForm != null) progressForm.CloseSafely();

                // the only way back for a pass which stopped before the restore above
                if (isModlistModified) ManageModOrganizer.RestoreModlist();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Runs a program hidden, waits for it, and writes down how it exited.
        /// </summary>
        /// <remarks>
        /// The exit code is recorded rather than acted on. The updater is a separate program
        /// with its own idea of what a stopped update is, and everything it wrote before
        /// stopping still has to be sorted - but a run which ended early is exactly what is
        /// worth knowing about when this pass is reported as having gone wrong.
        /// </remarks>
        private static void RunUpdater(string fileName, string arguments)
        {
            ManageProcess.RunProgramAndWaitHidden(fileName, arguments, out int exitCode);

            _log.Info("'" + fileName + "' finished with exit code " + exitCode + ".");
        }

        /// <summary>
        /// Registers the standalone updater as a custom executable of Mod Organizer, and
        /// makes sure the mod its own files go into exists.
        /// </summary>
        /// <remarks>
        /// Both are what makes the updater runnable from here: the shortcut this pass starts
        /// is the one Mod Organizer builds from that entry, and the updater refuses to run
        /// when the mod it writes into is missing.
        /// </remarks>
        private static void EnsureStandaloneUpdaterIsRegistered()
        {
            var kkManagerStandaloneUpdater = new ManageModOrganizer.CustomExecutables.CustomExecutable
            {
                Title = StandaloneUpdaterTitle,
                Binary = ManageSettings.KkManagerStandaloneUpdaterExePath,
                Arguments = ManageSettings.CurrentGameDataDirPath,
                MoTargetMod = ManageSettings.KKManagerFilesModName
            };

            var kkManagerFilesModPath = Path.Combine(ManageSettings.CurrentGameModsDirPath, ManageSettings.KKManagerFilesModName);

            if (!Directory.Exists(kkManagerFilesModPath))
            {
                Directory.CreateDirectory(kkManagerFilesModPath);

                ManageModOrganizer.WriteMetaIni(
                    kkManagerFilesModPath,
                    categoryNames: "",
                    version: KKManagerFilesModVersion,
                    comments: "",
                    notes: T._("KKManager's and it's Standalone Updater's new created files stored here")
                    );

                ManageModOrganizer.InsertMod(
                    modname: ManageSettings.KKManagerFilesModName,
                    modAfterWhichInsert: "Low priority_separator"
                    );
            }

            ManageModOrganizer.InsertCustomExecutable(kkManagerStandaloneUpdater);
        }

        /// <summary>
        /// Copies the modlist, so that it can be put back when the pass is over.
        /// </summary>
        /// <remarks>
        /// A copy left behind by a pass which did not finish is overwritten rather than
        /// refused. The modlist which is current now is the one which has to be put back, and
        /// refusing would make the next update after a failure impossible to run at all.
        /// </remarks>
        private static void BackupModlist()
        {
            var modlistPath = ManageSettings.CurrentMoProfileModlistPath;

            File.Copy(modlistPath, modlistPath + ModlistBackupSuffix, overwrite: true);
        }

        /// <summary>
        /// Enables the mods which carry a sideloader modpack, so that the updater can write
        /// into them.
        /// </summary>
        /// <remarks>
        /// The modlist has to have been copied first: the enabled state is put back from that
        /// copy when the pass is over, and there is no other record of what it was.
        /// </remarks>
        private static void EnableSideloaderMods()
        {
            var modlist = new ModlistData();

            var kkManagerFilesMod = modlist.Mods.FirstOrDefault(m => m.Name == ManageSettings.KKManagerFilesModName);
            if (kkManagerFilesMod != null && !kkManagerFilesMod.IsEnabled) kkManagerFilesMod.IsEnabled = true;

            Parallel.ForEach(modlist.Mods, item =>
            {
                if (item.IsEnabled
                || !item.IsExist
                || item.Type is ModType.Separator) return;

                if (Directory.Exists(Path.Combine(item.Path, "mods"))
                || Directory.Exists(Path.Combine(item.Path, "UserData", "chara"))) item.IsEnabled = true;
            });

            modlist.Save();
        }
    }
}
