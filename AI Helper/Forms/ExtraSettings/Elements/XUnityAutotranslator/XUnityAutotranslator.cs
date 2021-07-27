using AIHelper.Manage;
using System.IO;
using System.Windows.Forms;

namespace AIHelper.Forms.ExtraSettings.Elements
{
    internal class XUnityAutotranslator : Elements, System.IDisposable
    {
        public XUnityAutotranslator()
        {
            _xuaElement = new XUnityAutotranslatorForm
            {
                TopLevel = false,
                //BackColor = parentForm.BackColor
            };
            ElementToPlace = _xuaElement;
        }

        internal override bool Check()
        {
            Properties.Settings.Default.XUAiniPath =
                ManageModOrganizer.GetLastMoFileDirPathFromEnabledModsOfActiveMoProfile(new string[2] { Path.Combine(ManageSettings.GetCurrentGameModsPath(), "XUnity.AutoTranslator", "BepInEx", "config", "AutoTranslatorConfig.ini"), Path.Combine(ManageSettings.GetCurrentGameModsPath(), "XUnity.AutoTranslator", "Plugins", "AutoTranslatorConfig.ini") }, new bool[2] { false, false })
                ;

            //return false;
            return !string.IsNullOrWhiteSpace(Properties.Settings.Default.XUAiniPath) && File.Exists(Properties.Settings.Default.XUAiniPath);
        }

        XUnityAutotranslatorForm _xuaElement;

        //internal override T ElementToPlace { get => XUAElement; set => XUAElement = value; }

        internal override string Title => "XUnity.Autotranslator";

        internal override Form ElementToShow { get => _xuaElement; set => _xuaElement = value as XUnityAutotranslatorForm; }

        internal override void Show(Form parentForm)
        {
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.XUAiniPath))
            {
                parentForm.Controls.Add(_xuaElement);
                _xuaElement.Show();
            }
        }

        public void Dispose()
        {
            _xuaElement.Dispose();
        }
    }
}
