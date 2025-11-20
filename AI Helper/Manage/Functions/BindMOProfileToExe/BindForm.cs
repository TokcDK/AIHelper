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
            if(ProfilesComboBox.SelectedItem == null || string.IsNullOrEmpty(ProfilesComboBox.SelectedItem.ToString()))
            {
                return;
            }

            var gameDirPath = ManageSettings.Games.Game.GameDirInfo.FullName;
            var modOrganizerDirPath = Path.Combine(gameDirPath, ManageSettings.AppModOrganizerDirName);
            var moProfilesDir = Path.Combine(modOrganizerDirPath, ManageSettings.MoProfilesDirName);
            var boundExeListPath = Path.Combine(moProfilesDir, ProfilesComboBox.SelectedItem.ToString(), "boundexes.txt");            

            if (!File.Exists(boundExeListPath))
            {
                return;
            }

            var boundExes = File.ReadAllLines(boundExeListPath)
                .Where(l => !string.IsNullOrWhiteSpace(l) && File.Exists(l))
                .ToList();
            BoundExesListBox.DataSource = boundExes;
        }

        private void BindForm_Load(object sender, EventArgs e)
        {
            moProfilesList = new List<string>();
            var gameDirPath = ManageSettings.Games.Game.GameDirInfo.FullName;
            var modOrganizerDirPath = Path.Combine(gameDirPath, ManageSettings.AppModOrganizerDirName);
            var moProfilesDir = Path.Combine(modOrganizerDirPath, ManageSettings.MoProfilesDirName);
            moProfilesList.AddRange(Directory.GetDirectories(moProfilesDir).Select(d => Path.GetFileName(d)));
            ProfilesComboBox.DataSource = moProfilesList;

            currentGameExeList = new List<string>();
            var gameExeName = ManageSettings.Games.Game.GameExeName;
            var studioExeName = ManageSettings.Games.Game.GameStudioExeName;
            currentGameExeList.Add(Path.Combine(gameDirPath, ManageSettings.DataDirName, gameExeName + ".exe"));
            currentGameExeList.Add(Path.Combine(gameDirPath, ManageSettings.DataDirName, studioExeName + ".exe"));

            ExesComboBox.DataSource = currentGameExeList;
        }

        private void AddExeButton_Click(object sender, EventArgs e)
        {
            if(ExesComboBox.SelectedItem == null 
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
            if(BoundExesListBox.SelectedItem == null)
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

            var gameDirPath = ManageSettings.Games.Game.GameDirInfo.FullName;
            var modOrganizerDirPath = Path.Combine(gameDirPath, ManageSettings.AppModOrganizerDirName);
            var moProfilesDir = Path.Combine(modOrganizerDirPath, ManageSettings.MoProfilesDirName);
            var boundExeListPath = Path.Combine(moProfilesDir, ProfilesComboBox.SelectedItem.ToString(), "boundexes.txt");
            var boundExes = new List<string>();
            foreach(var item in BoundExesListBox.Items)
            {
                boundExes.Add(item.ToString());
            }
            File.WriteAllLines(boundExeListPath, boundExes);
        }
    }
}
