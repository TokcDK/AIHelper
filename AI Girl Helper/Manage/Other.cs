using AI_Girl_Helper.Manage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AI_Girl_Helper.Utils
{
    class Other
    {
        public static void CheckBoxChangeColor(CheckBox checkBox)
        {
            if (checkBox.Checked)
            {
                checkBox.ForeColor = Color.FromArgb(192, 255, 192);
            }
            else
            {
                checkBox.ForeColor = Color.White;
            }
        }

        /// <summary>
        /// Проверки существования целевой папки и модификация имени на уникальное
        /// </summary>
        /// <param name="ParentFolder"></param>
        /// <param name="TargetFolder"></param>
        /// <returns></returns>
        public static string GetResultTargetDirPathWithNameCheck(string ParentFolder, string TargetFolder)
        {
            string ResultTargetDirPath = Path.Combine(ParentFolder, TargetFolder);
            int i = 0;
            while (Directory.Exists(ResultTargetDirPath))
            {
                i++;
                ResultTargetDirPath = Path.Combine(ParentFolder, TargetFolder + " (" + i + ")");
            }
            return ResultTargetDirPath;
        }

        public static string IsAnyFileWithSameExtensionContainsNameOfTheFile(string zipmoddirmodspath, string zipname, string extension)
        {
            if (Directory.Exists(zipmoddirmodspath))
            {
                foreach (var path in Directory.GetFiles(zipmoddirmodspath, extension))
                {
                    string name = Path.GetFileNameWithoutExtension(path);
                    if (StringEx.IsStringAContainsStringB(name, zipname))
                    {
                        return path;
                    }
                }
            }
            return string.Empty;
        }

        public static void AutoShortcutAndRegystry()
        {
            CreateShortcuts();
            RegistryManage.FixRegistry();
        }

        //https://bytescout.com/blog/create-shortcuts-in-c-and-vbnet.html
        public static void CreateShortcuts(bool force = false, bool auto = true)
        {
            if (Properties.Settings.Default.AutoShortcutRegistryCheckBoxChecked || force)
            {
                //AI-Girl Helper
                string shortcutname = SettingsManage.GetCurrentGameName() + " " + T._("Helper");
                string targetpath = Application.ExecutablePath;
                string arguments = string.Empty;
                string workingdir = Path.GetDirectoryName(targetpath);
                string description = T._("Run") + " " + shortcutname;
                string iconlocation = Application.ExecutablePath;
                Shortcut.Create(shortcutname, targetpath, arguments, workingdir, description, iconlocation);

                //AI-Girl Trial
                //shortcutname = "AI-Girl Trial";
                //targetpath = Path.Combine(MOPath, "ModOrganizer.exe");
                //arguments = "\"moshortcut://:AI-SyoujyoTrial\"";
                //workingdir = Path.GetDirectoryName(targetpath);
                //description = "Run " + shortcutname + " with ModOrganizer";
                //iconlocation = Path.Combine(DataPath, "AI-SyoujyoTrial.exe");
                //Shortcut.Create(shortcutname, targetpath, arguments, workingdir, description, iconlocation);

                ////Mod Organizer
                //shortcutname = "ModOrganizer AI-Shoujo Trial";
                //targetpath = Path.Combine(MOPath, "ModOrganizer.exe");
                //arguments = string.Empty;
                //workingdir = Path.GetDirectoryName(targetpath);
                //description = shortcutname;
                //Shortcut.Create(shortcutname, targetpath, arguments, workingdir, description);

                if (!Directory.Exists(Properties.Settings.Default.OverwriteFolderLink) && Directory.Exists(Properties.Settings.Default.OverwriteFolder))
                {
                    CreateSymlink.Folder(Properties.Settings.Default.OverwriteFolder, Properties.Settings.Default.OverwriteFolderLink);
                }
                if (!auto)
                {
                    MessageBox.Show(T._("Shortcut") + " " + T._("created") + "!");
                }
            }
        }

        public static void MakeDummyFiles()
        {
            //Create dummy file and add hidden attribute
            if (!File.Exists(SettingsManage.GetDummyFile()))
            {
                File.WriteAllText(SettingsManage.GetDummyFile(), "dummy file need to execute mod organizer");
                FileFolderOperations.HideFileFolder(SettingsManage.GetDummyFile(), true);
            }
        }
    }
}
