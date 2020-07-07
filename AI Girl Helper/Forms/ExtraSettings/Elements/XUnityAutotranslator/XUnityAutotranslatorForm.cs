using AIHelper.Manage;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AIHelper.Forms.ExtraSettings.Elements
{
    public partial class XUnityAutotranslatorForm : Form
    {
        public XUnityAutotranslatorForm()
        {
            InitializeComponent();
        }

        private void XUnityAutotranslatorForm_Load(object sender, EventArgs e)
        {
            AddXUASettings();

            SetLocalizationStrings();

            SetTooltips();
        }

        private void AddXUASettings()
        {
            //Properties.Settings.Default.XUAiniPath =
            //    ManageMO.GetLastMOFileDirPathFromEnabledModsOfActiveMOProfile(new string[2] { Path.Combine(ManageSettings.GetModsPath(), "XUnity.AutoTranslator", "BepInEx", "config", "AutoTranslatorConfig.ini"), Path.Combine(ManageSettings.GetModsPath(), "XUnity.AutoTranslator", "Plugins", "AutoTranslatorConfig.ini") }, new bool[2] { false, false })
            //    ;

            ////если xua ini не найден, отключить элемент
            //if (string.IsNullOrWhiteSpace(Properties.Settings.Default.XUAiniPath) || !File.Exists(Properties.Settings.Default.XUAiniPath))
            //{
            //    XUASettingsPanel.Enabled = false;
            //    return;
            //}
            //else
            //{
            //    XUAGroupBox.Enabled = true;
            //}

            if (Properties.Settings.Default.XUAiniPath.Length > 0)
            {
                var iniValue = ManageINI.GetINIValueIfExist(
                    Properties.Settings.Default.XUAiniPath,
                    "FromLanguage",
                    "General"
                    );

                //запретить элементы, если значение пустое
                if (iniValue.Length == 0)
                {
                    iniValue = "auto";
                    //XUASettingsPanel.Enabled = false;
                    //return;
                }

                XUAFromLanguageComboBox.Items.AddRange(ManageSettings.Languages.ToArray());
                XUALanguageComboBox.Items.AddRange(ManageSettings.Languages.ToArray());
                XUALanguageComboBox.Items.Remove(T._("Auto"));//удаление Авто

                XUAFromLanguageComboBox.SelectedItem = ManageSettings.LanguageEnumFromIdentifier(iniValue);

                if (ManageSettings.IsFirstRun())
                {
                    iniValue = GetLanguageCodeBySystemLanguage();
                }
                else
                {
                    iniValue = ManageINI.GetINIValueIfExist(
                        Properties.Settings.Default.XUAiniPath,
                        "Language",
                        "General"
                        );

                    if (string.IsNullOrWhiteSpace(iniValue))
                    {
                        iniValue = GetLanguageCodeBySystemLanguage();
                    }
                }
                XUALanguageComboBox.SelectedItem = ManageSettings.LanguageEnumFromIdentifier(iniValue);

                XUAEndpointComboBox.Items.AddRange(GetTranslationServicesList());

                iniValue = ManageINI.GetINIValueIfExist(
                    Properties.Settings.Default.XUAiniPath,
                    "Endpoint",
                    "Service"
                    );

                //"GoogleTranslate"
                if (string.IsNullOrWhiteSpace(iniValue))
                {
                    iniValue = "GoogleTranslate";
                }

                XUAEndpointComboBox.SelectedItem = iniValue;

            }
            else
            {
                XUAGroupBox.Enabled = false;
            }
        }

        private static string GetLanguageCodeBySystemLanguage()
        {
            return (CultureInfo.CurrentCulture.Name == "ru") ? "ru" : "en";//return "ru" for russian localization and "en" for all other languages
        }

        private static string[] GetTranslationServicesList()
        {
            return new string[]
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
                    };
        }

        ToolTip THToolTip;

        private void SetLocalizationStrings()
        {
            this.Text = T._("Extra Settings");
            XUALanguageLabel.Text = T._("Language") + ":";
            XUATranslateServiceLabel.Text = T._("Translate Service") + ":";
            XUAcfgFileOpenLinkLabel.Text = T._("Settings");
        }

        private void SetTooltips()
        {
            //http://qaru.site/questions/47162/c-how-do-i-add-a-tooltip-to-a-control
            //THMainResetTableButton
            THToolTip = new ToolTip
            {
                // Set up the delays for the ToolTip.
                AutoPopDelay = 32000,
                InitialDelay = 1000,
                ReshowDelay = 500,
                UseAnimation = true,
                UseFading = true,
                // Force the ToolTip text to be displayed whether or not the form is active.
                //ShowAlways = true
            };

            THToolTip.SetToolTip(XUAcfgFileOpenLinkLabel, T._("Open file with all settings"));
            THToolTip.SetToolTip(XUAHelpLinkLabel, T._("Open XUA documentation"));
            THToolTip.SetToolTip(XUAFromLanguageComboBox, T._("List of source languages. From which translate"));
            THToolTip.SetToolTip(XUALanguageComboBox, T._("List of target languages. To which translate"));
            THToolTip.SetToolTip(XUAEndpointComboBox, T._("List of translation services"));

            THToolTip.SetToolTip(XUAEndpointComboBox,
                T._("From XUA documentation.\nThe supported translators are: ")
                + "\n\n" +
                T._("GoogleTranslate, based on the online Google translation service. Does not require authentication.")
                + "\n\n" +
                T._("GoogleTranslateLegitimate, based on the Google cloud translation API. Requires an API key.\nProvides trial period of 1 year with $300 credits. Enough for 15 million characters translations.")
                + "\n\n" +
                T._("BingTranslate, based on the online Bing translation service. Does not require authentication.\nNo limitations, but unstable.")
                + "\n\n" +
                T._("BingTranslateLegitimate, based on the Azure text translation. Requires an API key.\nFree up to 2 million characters per month.")
                + "\n\n" +
                T._("PapagoTranslate, based on the online Google translation service. Does not require authentication.\nNo limitations, but unstable.")
                + "\n\n" +
                T._("BaiduTranslate, based on Baidu translation service. Requires AppId and AppSecret.\nNot sure on quotas on this one.")
                + "\n\n" +
                T._("YandexTranslate, based on the Yandex translation service. Requires an API key.\nFree up to 1 million characters per day, but max 10 million characters per month.")
                + "\n\n" +
                T._("WatsonTranslate, based on IBM's Watson. Requires a URL and an API key.\nFree up to 1 million characters per month.")
                + "\n\n" +
                T._("LecPowerTranslator15, based on LEC's Power Translator. Does not require authentication, but does require the software installed.")

                );

            ////////////////////////////
        }

        private void XUAHelpLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/bbepis/XUnity.AutoTranslator#index");
        }

        private void XUAFromLanguageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Properties.Settings.Default.ExtraSettingsInitOnLoadIsInAction)
            {
                string value = ManageSettings.LanguageEnumToIdentifier((sender as ComboBox).SelectedItem.ToString());
                ManageINI.WriteINIValue(Properties.Settings.Default.XUAiniPath, "General", "FromLanguage", value);
            }
        }

        private void XUALanguageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Properties.Settings.Default.ExtraSettingsInitOnLoadIsInAction)
            {
                string value = ManageSettings.LanguageEnumToIdentifier((sender as ComboBox).SelectedItem.ToString());
                ManageINI.WriteINIValue(Properties.Settings.Default.XUAiniPath, "General", "Language", value);
            }
        }

        private void XUAEndpointComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Properties.Settings.Default.ExtraSettingsInitOnLoadIsInAction)
            {
                string value = (sender as ComboBox).SelectedItem.ToString();
                ManageINI.WriteINIValue(Properties.Settings.Default.XUAiniPath, "Service", "Endpoint", value);
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
