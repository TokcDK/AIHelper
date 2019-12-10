using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AI_Helper.Manage;

namespace AI_Helper
{
    public partial class ExtraSettings : Form
    {
        public ExtraSettings()
        {
            InitializeComponent();
        }

        private void ExtraSettings_Load(object sender, EventArgs e)
        {
            Properties.Settings.Default.XUAiniPath =
                ManageMO.GetLastMOFileDirPathFromEnabledModsOfActiveMOProfile(Path.Combine(ManageSettings.GetModsPath(), "XUnity.AutoTranslator", "BepInEx", "config", "AutoTranslatorConfig.ini"))
                ;

            if (Properties.Settings.Default.XUAiniPath.Length > 0)
            {

                XUAFromLanguageComboBox.Items.AddRange(ManageSettings.Languages.ToArray());
                XUALanguageComboBox.Items.AddRange(ManageSettings.Languages.ToArray());

                string iniValue = ManageINI.GetINIValueIfExist(
                    Properties.Settings.Default.XUAiniPath,
                    "FromLanguage",
                    "General"
                    );
                XUAFromLanguageComboBox.SelectedItem = ManageSettings.LanguageEnumFromIdentifier(iniValue);

                iniValue = ManageINI.GetINIValueIfExist(
                    Properties.Settings.Default.XUAiniPath,
                    "Language",
                    "General"
                    );
                XUALanguageComboBox.SelectedItem = ManageSettings.LanguageEnumFromIdentifier(iniValue);

                XUAEndpointComboBox.Items.AddRange(
                    new string[]
                    {
                    "GoogleTranslate"
                    ,
                    "GoogleTranslateLegitimate"
                    ,
                    "BingTranslate"
                    ,
                    "BingTranslateLegitimate"
                    ,
                    "PapagoTranslate"
                    ,
                    "BaiduTranslate"
                    ,
                    "YandexTranslate"
                    ,
                    "WatsonTranslate"
                    ,
                    "LecPowerTranslator15"
                    }
                );

                iniValue = ManageINI.GetINIValueIfExist(
                    Properties.Settings.Default.XUAiniPath,
                    "Endpoint",
                    "Service"
                    );
                XUAEndpointComboBox.SelectedItem = iniValue;

            }
            else
            {
                XUAGroupBox.Enabled = false;
            }
        }

        private void XUAcfgFileOpenLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(Properties.Settings.Default.XUAiniPath))
            {
                Process.Start(Properties.Settings.Default.XUAiniPath);
            }
        }
    }
}
