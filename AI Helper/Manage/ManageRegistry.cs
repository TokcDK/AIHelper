using Microsoft.Win32;
using System.Windows.Forms;

namespace AIHelper.Manage
{
    class ManageRegistry
    {
        public static void FixRegistry(bool auto = true)
        {
            string registryPath = ManageSettings.GetCurrentGameRegistryPath();
            var installDirValue = Registry.GetValue(registryPath, ManageSettings.GetCurrentGameRegistryInstallDirKeyName(), null);
            if (installDirValue == null || installDirValue.ToString() != ManageSettings.GetCurrentGameDataPath())
            {
                Registry.SetValue(registryPath, ManageSettings.GetCurrentGameRegistryInstallDirKeyName(), ManageSettings.GetCurrentGameDataPath());
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
