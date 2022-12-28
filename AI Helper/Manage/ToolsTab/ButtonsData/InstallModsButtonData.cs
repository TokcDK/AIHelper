using System;
using AIHelper.Install.Types;
using AIHelper.Install.UpdateMaker;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AIHelper.Manage.Functions;
using AIHelper.Install.Types.Directories;

namespace AIHelper.Manage.ToolsTab.ButtonsData
{
    internal class InstallModsButtonData : IToolsTabButtonData
    {
        public string Text => T._("Install");

        public string Description => T._("Install mods and userdata, placed in") + " " + ManageSettings.ModsInstallDirName + (ManageSettings.IsMoMode ? 
            T._(" to MO format in Mods when possible") : 
            T._(" to the game folder when possible")
            );

        public void OnClick(object o, EventArgs e)
        {
            Install();
        }

        internal static async void Install()
        {
            if (new UpdateMaker().MakeUpdate())
            {
                MessageBox.Show("Made update instead");
                return;
            }

            List<ModInstallerBase> installers = GetListOfSubClasses.Inherited.GetListOfinheritedSubClasses<ModInstallerBase>().OrderBy(o => o.Order).ToList();

            //if (Directory.Exists(Install2MoDirPath) && (Directory.GetFiles(Install2MoDirPath, "*.rar").Length > 0 || Directory.GetFiles(Install2MoDirPath, "*.7z").Length > 0 || Directory.GetFiles(Install2MoDirPath, "*.png").Length > 0 || Directory.GetFiles(Install2MoDirPath, "*.cs").Length > 0 || Directory.GetFiles(Install2MoDirPath, "*.dll").Length > 0 || Directory.GetFiles(Install2MoDirPath, "*.zipmod").Length > 0 || Directory.GetFiles(Install2MoDirPath, "*.zip").Length > 0 || Directory.GetDirectories(Install2MoDirPath, "*").Length > 0))
            if (!IsInstallDirHasAnyRequiredFileFrom(installers))
            {
                MessageBox.Show(T._("No compatible for installation formats found in install folder.\n\nIt must be archvives, game files or folders with game files.\n\nWill be opened installation dir where you can drop files for installation."));
            }
            else
            {
                ManageSettings.MainForm.OnOffButtons(false);

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
                        ManageSettings.MainForm.OnOffButtons();
                        return;
                    }
                }

                await Task.Run(() => InstallModFilesAndCleanEmptyFolder(installers)).ConfigureAwait(true);

                //this.Text = T._("Install from") + " " + ManageSettings.ModsInstallDirName;

                ManageSettings.MainForm.OnOffButtons();

                //обновление информации о конфигурации папок игры
                ManageSettings.MainForm.UpdateData();

                MessageBox.Show(T._("All possible mods installed. Install all rest in install folder manually."));
            }

            Directory.CreateDirectory(ManageSettings.Install2MoDirPath);
            Process.Start("explorer.exe", ManageSettings.Install2MoDirPath);
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

            foreach (var installer in installers) installer.Install();

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
