using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AIHelper.Manage.Functions.BindMOProfileToExe
{
    public partial class BindForm : Form
    {
        public BindForm()
        {
            InitializeComponent();
        }

        private List<string> currentGameExeList = new List<string>();
        private List<string> moProfilesList = new List<string>();

        private void ProfilesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ProfilesComboBox.SelectedItem == null || string.IsNullOrEmpty(ProfilesComboBox.SelectedItem.ToString()))
            {
                return;
            }

            var boundExeListPath = Path.Combine(ManageSettings.MoCurrentGameProfilesDirPath, ProfilesComboBox.SelectedItem.ToString(), ManageSettings.MoProfileBoundExesName);

            if (!File.Exists(boundExeListPath))
            {
                return;
            }

            var boundExes = File.ReadAllLines(boundExeListPath)
                .Where(l => !string.IsNullOrWhiteSpace(l) && File.Exists(l))
                .ToArray();
            BoundExesListBox.Items.Clear();
            BoundExesListBox.Items.AddRange(boundExes);
        }

        private void BindForm_Load(object sender, EventArgs e)
        {
            var gameExeName = ManageSettings.Games.Game.GameExeName;
            var gameDirPath = ManageSettings.Games.Game.GameDirInfo.FullName;

            // load profiles
            moProfilesList = new List<string>();
            moProfilesList.AddRange(Directory.GetDirectories(ManageSettings.MoCurrentGameProfilesDirPath).Select(d => Path.GetFileName(d)));
            ProfilesComboBox.DataSource = moProfilesList;

            // load game exes
            currentGameExeList = new List<string>();
            var studioExeName = ManageSettings.Games.Game.GameStudioExeName;
            currentGameExeList.Add(Path.Combine(gameDirPath, ManageSettings.DataDirName, gameExeName + ".exe"));
            currentGameExeList.Add(Path.Combine(gameDirPath, ManageSettings.DataDirName, studioExeName + ".exe"));
            ExesComboBox.DataSource = currentGameExeList;
        }

        private void AddExeButton_Click(object sender, EventArgs e)
        {
            if (ExesComboBox.SelectedItem == null
                || string.IsNullOrEmpty(ExesComboBox.SelectedItem.ToString()))
            {
                return;
            }

            if (BoundExesListBox.Items.Contains(ExesComboBox.SelectedItem.ToString()))
            {
                return;
            }

            BoundExesListBox.Items.Add(ExesComboBox.SelectedItem.ToString());

            WriteBoundExes();
        }

        private void RemoveExeButton_Click(object sender, EventArgs e)
        {
            if (BoundExesListBox.SelectedItem == null)
            {
                return;
            }

            BoundExesListBox.Items.Remove(BoundExesListBox.SelectedItem);

            WriteBoundExes();
        }

        private void WriteBoundExes()
        {
            if (ProfilesComboBox.SelectedItem == null
                || string.IsNullOrEmpty(ProfilesComboBox.SelectedItem.ToString()))
            {
                return;
            }

            var boundExeListPath = Path.Combine(ManageSettings.MoCurrentGameProfilesDirPath, ProfilesComboBox.SelectedItem.ToString(), ManageSettings.MoProfileBoundExesName);
            File.WriteAllLines(boundExeListPath, BoundExesListBox.Items.Cast<string>());
        }
    }
}
