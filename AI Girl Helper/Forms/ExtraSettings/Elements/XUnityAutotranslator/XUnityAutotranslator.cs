using AIHelper.Manage;
using System.IO;
using System.Windows.Forms;

namespace AIHelper.Forms.ExtraSettings.Elements
{
    internal class XUnityAutotranslator : Elements, System.IDisposable
    {
        public XUnityAutotranslator()
        {
            XUAElement = new XUnityAutotranslatorForm
            {
                TopLevel = false,
                //BackColor = parentForm.BackColor
            };
            ElementToPlace = XUAElement;
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

        //internal override T ElementToPlace { get => XUAElement; set => XUAElement = value; }

        internal override string Title => "XUnity.Autotranslator";

        internal override Form ElementToShow { get => XUAElement; set => XUAElement=value as XUnityAutotranslatorForm; }

        internal override void Show(Form parentForm)
        {
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.XUAiniPath))
            {
                parentForm.Controls.Add(XUAElement);
                XUAElement.Show();
            }
        }

        public void Dispose()
        {
            XUAElement.Dispose();
        }
    }
}
