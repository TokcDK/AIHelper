using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AI_Helper.Manage
{
    class RegistryManage
    {
        public static void FixRegistry(bool auto = true)
        {
            string GameName = SettingsManage.GetCurrentGameEXEName();
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
