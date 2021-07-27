using Microsoft.Win32;
using System.Windows.Forms;

namespace AIHelper.Manage
{
    class ManageRegistry
    {
        public static void FixRegistry(bool auto = true)
        {
            string gameName = ManageSettings.GetCurrentGameExeName();
            string regystryPath = @"HKEY_CURRENT_USER\Software\illusion\" + gameName + @"\" + gameName;
            var installDirValue = Registry.GetValue(regystryPath, "INSTALLDIR", null);
            if (installDirValue == null || installDirValue.ToString() != ManageSettings.GetCurrentGameDataPath())
            {
                Registry.SetValue(regystryPath, "INSTALLDIR", ManageSettings.GetCurrentGameDataPath());
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
