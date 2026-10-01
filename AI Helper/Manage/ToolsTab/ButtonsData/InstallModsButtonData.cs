using System;
using AIHelper.Forms.Other;
using AIHelper.Install;
using AIHelper.Install.Cards;
using AIHelper.Install.Types;
using AIHelper.Install.UpdateMaker;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AIHelper.Manage.Functions;
using AIHelper.Install.Types.Directories;
using NLog;

namespace AIHelper.Manage.ToolsTab.ButtonsData
{
    internal class InstallModsButtonData : IToolsTabButtonData, IButtonDataAvailability
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public string Text => T._("Install");

        public string Description => T._("Install mods and userdata, placed in") + " " + ManageSettings.ModsInstallDirName + (ManageSettings.IsMoMode ? 
            T._(" to MO format in Mods when possible") : 
            T._(" to the game folder when possible")
            );

        /// <summary>
        /// Installing only makes sense through Mod Organizer: it needs Mod Organizer to be
        /// installed, and it offers to convert the game to MO mode when it is not there yet.
        /// </summary>
        public bool IsAvailable => ManageModOrganizer.IsInstalled;

        public void OnClick(object o, EventArgs e)
        {
            Install();
        }

        internal static void Install()
        {
            try
            {
                if (new UpdateMaker().MakeUpdate())
                {
                    MessageBox.Show("Made update instead");
                    return;
                }

                List<ModInstallerBase> installers = GetListOfSubClasses.Inherited.GetListOfInheritedSubClasses<ModInstallerBase>().OrderBy(o => o.Order).ToList();

                ManageSettings.MainForm.OnOffButtons(false);

                try
                {
                    //impossible to correctly update mods in common mode
                    if (!ManageSettings.IsMoMode)
                    {
                        DialogResult result = MessageBox.Show(T._("Attention") + "\n\n" + T._("Impossible to correctly install/update mods\n\n in standart mode because files was moved in Data.") + "\n\n" + T._("Switch to MO mode?"), T._("Confirmation"), MessageBoxButtons.OKCancel);
                        if (result == DialogResult.OK)
                        {
                            ManageMOModeSwitch.SwitchBetweenMoAndStandartModes();
                        }
                        else
                        {
                            return;
                        }
                    }

                    // The window owns the install. It does nothing until its own install
                    // button is pressed, so no file is moved before the user asks for it,
                    // and it shows the progress of every step and the log while it runs.
                    // It is told what the install folder accepts, which it lists before
                    // the run so the user can check the folder against it.
                    using (var installForm = new InstallProgressForm(DescribeInstallableContent(installers), () => InstallModFilesAndCleanEmptyFolder(installers)))
                    {
                        installForm.ShowDialog(ManageSettings.MainForm);
                    }

                    //обновление информации о конфигурации папок игры
                    ManageSettings.MainForm.UpdateData();
                }
                finally
                {
                    //never leave the main form disabled
                    ManageSettings.MainForm.OnOffButtons();
                }
            }
            catch (Exception ex)
            {
                _log.Error("Failed to install mods. Error:\r\n" + ex);

                MessageBox.Show(T._("Failed to install mods.") + Environment.NewLine + ex.Message);

                //never leave the main form disabled after a failure
                ManageSettings.MainForm.OnOffButtons();
            }
        }

        /// <summary>
        /// Lines which describe what can be placed in the install folder, taken from the
        /// installers themselves so the shown list can not go out of date.
        /// </summary>
        /// <param name="installers">Installers of the run, in the order they run in</param>
        /// <returns>One line per installer</returns>
        private static List<string> DescribeInstallableContent(List<ModInstallerBase> installers)
        {
            var lines = new List<string>();

            foreach (var installer in installers)
            {
                lines.Add(installer.ContentDescription);
            }

            return lines;
        }

        /// <summary>
        /// Opens the folder the user drops the files to install into.
        /// </summary>
        private static void OpenInstallFolder()
        {
            try
            {
                Directory.CreateDirectory(ManageSettings.Install2MoDirPath);

                Process.Start("explorer.exe", ManageSettings.Install2MoDirPath);
            }
            catch (Exception ex)
            {
                _log.Error("Failed to open the install folder. Error:\r\n" + ex);
            }
        }

        private static bool IsInstallDirHasAnyRequiredFileFrom(List<ModInstallerBase> installers)
        {
            foreach (var installer in installers)
            {
                var IsDirInstaller = installer is DirectoriesInstallerBase;
                foreach (var mask in installer.Masks)
                {
                    if ((IsDirInstaller && ManageFilesFoldersExtensions.IsAnySubDirExistsInTheDir(ManageSettings.Install2MoDirPath, mask))
                        || (!IsDirInstaller && ManageFilesFoldersExtensions.IsAnyFileExistsInTheDir(ManageSettings.Install2MoDirPath, mask, allDirectories: false)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void InstallModFilesAndCleanEmptyFolder(List<ModInstallerBase> installers)
        {
            //the cards filed by their content during the previous run must not make
            //the card installers skip files of this run
            CardTargetRegistry.Clear();

            InstallProgress.BeginRun(installers.Count);

            try
            {
                for (int i = 0; i < installers.Count; i++)
                {
                    var installer = installers[i];

                    // the installer type name is what tells the user which kind of
                    // content the step handles
                    InstallProgress.BeginStep(i + 1, installer.GetType().Name);

                    var installedSomething = installer.Install();

                    _log.Info("Step " + (i + 1) + "/" + installers.Count + " finished: " + installer.GetType().Name
                        + (installedSomething ? " (installed something)" : " (nothing to install)"));
                }
            }
            finally
            {
                InstallProgress.EndRun();
            }

            //string installMessage = T._("Installing");
            //InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage));
            //new RarExtractor().Install();
            //InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage + "."));
            //new SevenZipExtractor().Install();
            //InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage + ".."));
            //new CsScriptsInstaller().Install();

            //InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage + "..."));
            //new ZipInstaller().Install();

            //InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage));
            //new BebInExDllInstaller().Install();

            //InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage + "."));
            //new SideloaderZipmod().Install();

            //InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage + ".."));
            //new PngInstaller().Install();

            //InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage + "..."));
            //new ModFilesFromDir().Install();

            //InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage));
            //new CardsFromDirsInstaller().Install();

            //InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = installMessage + "."));
            ManageFilesFoldersExtensions.DeleteEmptySubfolders(ManageSettings.Install2MoDirPath, false);

            Directory.CreateDirectory(ManageSettings.Install2MoDirPath);

            //InstallInModsButton.Invoke((Action)(() => InstallInModsButton.Text = T._("Install from") + " " + ManageSettings.ModsInstallDirName()));
        }
    }
}
