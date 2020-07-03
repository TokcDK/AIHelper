using AIHelper.Forms.ExtraSettings.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AIHelper
{
    public partial class ExtraSettings : Form
    {
        private bool ExtraSettingsInitOnLoadIsInAction { get => Properties.Settings.Default.ExtraSettingsInitOnLoadIsInAction; set => Properties.Settings.Default.ExtraSettingsInitOnLoadIsInAction = value; }

        public ExtraSettings()
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
                new XUnityAutotranslator()
            };

            foreach (var element in elements)
            {
                if (element.Check())
                {
                    //element.ElementInit();
                    element.Show(this);
                    //var size = element.form.Size;
                }
                else
                {
                    Label l = new Label
                    {
                        AutoSize = true,
                        Text = element.Title + " " + T._("not found"),
                        ForeColor = Color.White
                    };
                    this.Controls.Add(l);
                    l.Show();
                }
            }

        }

        private void ExtraSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
    }
}
