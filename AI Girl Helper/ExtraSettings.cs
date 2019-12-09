using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
            //var game = ManageSettings.GetListOfExistsGames()[ManageSettings.GetCurrentGameIndex()];

            //string XUnityAutoTranslatorConfigPath = ManageMO.GetLastMOFileDirPathFromEnabledModsOfActiveMOProfile(
            //    Path.Combine(ManageSettings.GetModsPath(), "XUnity.AutoTranslator", "BepInEx", "config", "AutoTranslatorConfig.ini")
            //    );

            //if (XUnityAutoTranslatorConfigPath.Length>0)
            //{
            //    GroupBox XUnityAutoTranslatorGroupBox = new GroupBox();
            //    Label XUnityAutoTranslatorGroupName = new Label();
            //    XUnityAutoTranslatorGroupName.Text = "test";
            //    this.Controls.Add(XUnityAutoTranslatorGroupBox);
            //    XUnityAutoTranslatorGroupBox.Controls.Add(XUnityAutoTranslatorGroupName);
            //}
        }
    }
}
