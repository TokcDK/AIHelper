using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIHelper.Manage
{
    class ManageOther
    {
        internal static async void WaitIfGameIsChanging()
        {
            if (!ManageSettings.IsMoMode())
            {
                return;
            }

            await Task.Run(() => WaitIfGameIsChanging(1000)).ConfigureAwait(true);
        }

        private static void WaitIfGameIsChanging(int waittime, int maxLoops = 60)
        {
            int loopsCount = 0;
            while (ManageSettings.IsMoMode() && (Properties.Settings.Default.SetModOrganizerINISettingsForTheGame || Properties.Settings.Default.CurrentGameIsChanging) && loopsCount < maxLoops)
            {
                Thread.Sleep(waittime);
                loopsCount++;
            }
        }

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
                string shortcutname = /*ManageSettings.GetCurrentGameFolderName() +*/ "AI " + T._("Helper");
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

                ManageModOrganizer.CheckMoUserdata();

                if (!auto)
                {
                    MessageBox.Show(T._("Shortcut") + " " + T._("created") + "!");
                }
            }
        }

        internal static void SwitchFormMinimizedNormalAll(Form[] forms)
        {
            foreach (var form in forms)
            {
                SwitchFormMinimizedNormal(form);
            }
        }

        internal static void SwitchFormMinimizedNormal(Form form)
        {
            //http://www.cyberforum.ru/windows-forms/thread31052.html
            if (form == null || form.IsDisposed)
            {
            }
            else if (form.WindowState == FormWindowState.Normal)
            {
                form.WindowState = FormWindowState.Minimized;

                SharedData.GameData.MainForm.DisposeTooltips(); // tooltips is not need here
            }
            else if (form.WindowState == FormWindowState.Minimized)
            {
                form.WindowState = FormWindowState.Normal;

                SharedData.GameData.MainForm.SetTooltips(); // set tooltips again
            }
        }

        //        internal static void ReportMessageForm(string title, string message)
        //        {
        ////#pragma warning disable CA2000 // Dispose objects before losing scope
        ////            Form ReportForm = new Form
        ////            {
        ////                Text = title,
        ////                //ReportForm.Size = new System.Drawing.Size(500,700);
        ////                AutoSize = true,
        ////                FormBorderStyle = FormBorderStyle.FixedDialog,
        ////                StartPosition = FormStartPosition.CenterScreen
        ////            };
        ////#pragma warning restore CA2000 // Dispose objects before losing scope
        ////            RichTextBox ReportTB = new RichTextBox
        ////            {
        ////                Size = new System.Drawing.Size(700, 900),
        ////                WordWrap = true,
        ////                Dock = DockStyle.Fill,
        ////                ReadOnly = true,
        ////                //ReportTB.BackColor = System.Drawing.Color.Gray;
        ////                Text = message,
        ////                ScrollBars = RichTextBoxScrollBars.Both
        ////            };

        ////            ReportForm.Controls.Add(ReportTB);
        ////            ReportForm.Show();
        //        }
    }
}
