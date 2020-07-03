using AIHelper.Manage;
using System.IO;
using System.Windows.Forms;

namespace AIHelper.Forms.ExtraSettings.Elements
{
    internal class XUnityAutotranslator : Elements
    {
        public XUnityAutotranslator()
        {
        }

        internal override bool Check()
        {
            Properties.Settings.Default.XUAiniPath =
                ManageMO.GetLastMOFileDirPathFromEnabledModsOfActiveMOProfile(new string[2] { Path.Combine(ManageSettings.GetModsPath(), "XUnity.AutoTranslator", "BepInEx", "config", "AutoTranslatorConfig.ini"), Path.Combine(ManageSettings.GetModsPath(), "XUnity.AutoTranslator", "Plugins", "AutoTranslatorConfig.ini") }, new bool[2] { false, false })
                ;

            //return false;
            return !string.IsNullOrWhiteSpace(Properties.Settings.Default.XUAiniPath) && File.Exists(Properties.Settings.Default.XUAiniPath);
        }

        XUnityAutotranslatorForm XUAElement;

        internal override Form form { get => XUAElement;}

        internal override string Title => "XUnity.Autotranslator";

        internal override void Show(Form parentForm)
        {
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.XUAiniPath))
            {
                XUAElement = new XUnityAutotranslatorForm
                {
                    TopLevel = false,
                    BackColor=parentForm.BackColor
                };
                parentForm.Controls.Add(XUAElement);
                XUAElement.Show();
            }
        }
    }
}
