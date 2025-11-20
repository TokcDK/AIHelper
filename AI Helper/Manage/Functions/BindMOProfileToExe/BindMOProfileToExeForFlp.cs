using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GetListOfSubClasses;
using System.Windows.Forms;
using AIHelper.Games;
using System.Reflection;
using System.Drawing;

namespace AIHelper.Manage.Functions.BindMOProfileToExe
{
    internal class BindMOProfileToExeForFlp : FunctionForFlpBase
    {
        public override string Symbol => T._("࿊"); //࿊

        public override string Description => T._("Bind the Game or Studio to run with specific Mod Organizer profile.");

        public override void OnClick(object o, EventArgs e)
        {
            BindExeToMOProfile();
        }

        public override Color? ForeColor => Color.Yellow;

        internal static void BindExeToMOProfile()
        {

            // create a list of Mod Organizer profile names for current game
            var moProfilesList = new List<string>();
            var gameDirPath = ManageSettings.Games.Game.GameDirInfo.FullName;
            var modOrganizerDirPath = Path.Combine(gameDirPath, ManageSettings.AppModOrganizerDirName);
            var moProfilesDir = Path.Combine(modOrganizerDirPath, ManageSettings.MoProfilesDirName);

            foreach (var profileDir in Directory.GetDirectories(moProfilesDir))
            {
                moProfilesList.Add(Path.GetFileName(profileDir));
            }

            var currentGameExeList = new List<string>();
            var gameExeName = ManageSettings.Games.Game.GameExeName;
            var studioExeName = ManageSettings.Games.Game.GameStudioExeName;
            currentGameExeList.Add(Path.Combine(gameDirPath, ManageSettings.DataDirName, gameExeName + ".exe"));
            currentGameExeList.Add(Path.Combine(gameDirPath, ManageSettings.DataDirName, studioExeName + ".exe"));

            // open a form to select exe and mod organzer profile to bind
            var bindForm = new Form();
            bindForm.Text = T._("Bind exe to Mod Organizer profile");
            var moProfilesComboBox = new ComboBox();
            var gameExesComboBox = new ComboBox();
            var bindButton = new Button();
            bindButton.Text = T._("Add");
            var boundExeListBox = new ListBox();

            // place and TableLayoutPanel to bindForm controls and setup the three columns for the tlp
            var tlp = new TableLayoutPanel();
            tlp.ColumnCount = 3;
            tlp.RowCount = 1;
            tlp.Dock = DockStyle.Fill;
            // set the tlp first and third columns to autosize and the second column to absolute size 20

            // add a flow layout panel with horisontal flow direction to the tlp first column
            var flp1 = new FlowLayoutPanel();
            flp1.FlowDirection = FlowDirection.TopDown;
            // add mo profiles combobox and exes combobox to the flp1
            moProfilesComboBox.DataSource = moProfilesList;
            flp1.Controls.Add(new Label() { Text = T._("Select Mod Organizer profile:") });
            flp1.Controls.Add(moProfilesComboBox);
            flp1.Controls.Add(new Label() { Text = T._("Select game exe:") });
            gameExesComboBox.DataSource = currentGameExeList;
            flp1.Controls.Add(gameExesComboBox);

            bindButton.Click += (sender, args) =>
            {

            };

            // add the flp1 to the tlp first column

            tlp.Controls.Add(flp1, 0, 0);


            bindForm.Controls.Add(tlp);
        }
    }
}
