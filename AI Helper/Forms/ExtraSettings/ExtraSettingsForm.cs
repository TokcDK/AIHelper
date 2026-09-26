using AIHelper.Forms.ExtraSettings.Elements;
using AIHelper.Forms.ExtraSettings.Elements.BepinEx;
using AIHelper.Manage;
using AIHelper.Manage.ui.themes;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AIHelper
{
    internal partial class ExtraSettingsForm : Form
    {
        private static bool ExtraSettingsInitOnLoadIsInAction { get => ManageSettings.ExtraSettingsInitOnLoadIsInAction; set => ManageSettings.ExtraSettingsInitOnLoadIsInAction = value; }

        public ExtraSettingsForm()
        {
            InitializeComponent();
        }

        private void ExtraSettings_Load(object sender, EventArgs e)
        {
            ExtraSettingsInitOnLoadIsInAction = true;

            InitOnLoad();

            ExtraSettingsInitOnLoadIsInAction = false;
        }

        private void InitOnLoad()
        {
            //AddXUASettings();
            List<Elements> elements = new List<Elements>
            {
                new BepinEx(),
                new XUnityAutotranslator()
            };

            foreach (var element in elements)
            {
                element.Init();
                if (element.Check() && element.ElementToShow != null)
                {
                    ExtraSettingsFlowLayoutPanel.Controls.Add(element.ElementToShow);

                    //the panel is built by the plugin while the program is already
                    //running, so it was not there when the form itself was themed
                    ThemesLoader.ApplyTheme(element.ElementToShow);
                    element.ElementToShow.Show();
                }
                else
                {
                    //a label rather than nothing, so a plugin which is not installed
                    //shows as such instead of leaving an unexplained gap
                    Label l = new Label
                    {
                        AutoSize = true,
                        Margin = new Padding(6, 4, 6, 4),
                        Text = element.Title + " " + T._("not found")
                    };
                    ExtraSettingsFlowLayoutPanel.Controls.Add(l);
                }
            }

            //the labels added above and any control a plugin created are themed here
            ThemesLoader.ApplyTheme(ExtraSettingsFlowLayoutPanel);
        }
    }
}
