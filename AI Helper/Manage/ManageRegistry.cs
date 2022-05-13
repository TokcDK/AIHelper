using Microsoft.Win32;
using System.Windows.Forms;

namespace AIHelper.Manage
{
    class ManageRegistry
    {
        public static void FixRegistry(bool auto = true)
        {
            string registryPath = ManageSettings.CurrentGameRegistryPath;
            var installDirValue = Registry.GetValue(registryPath, ManageSettings.CurrentGameRegistryInstallDirKeyName, null);
            if (installDirValue == null || installDirValue.ToString() != ManageSettings.CurrentGameDataDirPath)
            {
                Registry.SetValue(registryPath, ManageSettings.CurrentGameRegistryInstallDirKeyName, ManageSettings.CurrentGameDataDirPath);
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
