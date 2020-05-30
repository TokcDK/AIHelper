using Microsoft.Win32;
using System.Windows.Forms;

namespace AIHelper.Manage
{
    class ManageRegistry
    {
        public static void FixRegistry(bool auto = true)
        {
            string GameName = ManageSettings.GetCurrentGameEXEName();
            string RegystryPath = @"HKEY_CURRENT_USER\Software\illusion\" + GameName + @"\" + GameName;
            var InstallDirValue = Registry.GetValue(RegystryPath, "INSTALLDIR", null);
            if (InstallDirValue == null || InstallDirValue.ToString() != Properties.Settings.Default.DataPath)
            {
                Registry.SetValue(RegystryPath, "INSTALLDIR", Properties.Settings.Default.DataPath);
                if (!auto)
                {
                    MessageBox.Show(T._("Registry fixed! Install dir was set to Data dir."));
                }
            }
            else
            {
                if (!auto)
                {
                    MessageBox.Show(T._("Registry was already fixed"));
                }
            }
        }
    }
}
