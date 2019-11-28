using AI_Helper.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AI_Helper.Utils
{
    class ManageOther
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

        public static void AutoShortcutAndRegystry()
        {
            CreateShortcuts();
            ManageRegistry.FixRegistry();
        }

        //https://bytescout.com/blog/create-shortcuts-in-c-and-vbnet.html
        public static void CreateShortcuts(bool force = false, bool auto = true)
        {
            if (Properties.Settings.Default.AutoShortcutRegistryCheckBoxChecked || force)
            {
                //AI-Girl Helper
                string shortcutname = ManageSettings.GetCurrentGameFolderName() + " " + T._("Helper");
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
    }
}
