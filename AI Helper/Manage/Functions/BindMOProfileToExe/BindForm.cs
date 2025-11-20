using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private ProfileBoundExesData profilesList = new ProfileBoundExesData();

        private void ProfilesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ProfilesComboBox.SelectedItem == null || !(ProfilesComboBox.SelectedItem is ProfiledData data))
            {
                return;
            }

            // bound the bound exes list box to BoundExesListBox.DataSource
            BoundExesListBox.DataSource = new BindingSource
            {
                DataSource = data.BoundExes
            };
        }

        private static IEnumerable<string> GetBoundExes(string profileName)
        {
            var boundExeListPath = Path.Combine(ManageSettings.MoCurrentGameProfilesDirPath, profileName, ManageSettings.MoProfileBoundExesName);

            if (!File.Exists(boundExeListPath))
            {
                return new List<string>();
            }

            return File.ReadAllLines(boundExeListPath)
                .Where(l => !string.IsNullOrWhiteSpace(l) && File.Exists(l));
        }

        private void BindForm_Load(object sender, EventArgs e)
        {
            this.Text = T._("Bind Mod Organizer Profile to Game Executable");
            label1.Text = T._("Profile");
            label2.Text = T._("Exe");

            var gameExeName = ManageSettings.Games.Game.GameExeName;
            var gameDirPath = ManageSettings.Games.Game.GameDirInfo.FullName;

            // load profiles
            profilesList = new ProfileBoundExesData();
            var profileDirs = Directory.GetDirectories(ManageSettings.MoCurrentGameProfilesDirPath);
            foreach (var profileDir in profileDirs)
            {
                var profileName = Path.GetFileName(profileDir);
                var boundExes = GetBoundExes(profileName).ToList();
                profilesList.AddProfileData(profileName, boundExes);
            }

            var bs = new BindingSource
            {
                DataSource = profilesList.GetProfilesData(),
                //DataMember = "ProfileName"
            };
            ProfilesComboBox.DataSource = bs;
            ProfilesComboBox.DisplayMember = "ProfileName";
            ProfilesComboBox.SelectedIndex = ProfilesComboBox.FindStringExact(ManageSettings.GetMoSelectedProfileDirName());

            // load game exes
            currentGameExeList = new List<string>();
            var studioExeName = ManageSettings.Games.Game.GameStudioExeName;
            currentGameExeList.Add(Path.Combine(gameDirPath, ManageSettings.DataDirName, gameExeName + ".exe"));
            currentGameExeList.Add(Path.Combine(gameDirPath, ManageSettings.DataDirName, studioExeName + ".exe"));
            ExesComboBox.DataSource = currentGameExeList;
        }

        private void AddExeButton_Click(object sender, EventArgs e)
        {
            if (ExesComboBox.SelectedItem == null)
            {
                return;
            }

            string exePath = ExesComboBox.SelectedItem.ToString();
            if (string.IsNullOrEmpty(exePath)
                || !(ProfilesComboBox.SelectedItem is ProfiledData data))
            {
                return;
            }

            if (data.BoundExes.Contains(exePath))
            {
                return;
            }

            data.BoundExes.Add(exePath);
            BoundExesListBox.DataSource = null;
            BoundExesListBox.DataSource = data.BoundExes;

            WriteBoundExes();
        }

        private void RemoveExeButton_Click(object sender, EventArgs e)
        {
            if (BoundExesListBox.SelectedItem == null)
            {
                return;
            }
            var exePath = BoundExesListBox.SelectedItem.ToString();
            if (string.IsNullOrEmpty(exePath)
                || !(ProfilesComboBox.SelectedItem is ProfiledData data))
            {
                return;
            }

            data.BoundExes.Remove(exePath);
            BoundExesListBox.DataSource = null;
            BoundExesListBox.DataSource = data.BoundExes;

            WriteBoundExes();
        }

        private void WriteBoundExes()
        {
            if (ProfilesComboBox.SelectedItem == null
                || !(ProfilesComboBox.SelectedItem is ProfiledData data))
            {
                return;
            }

            var boundExeListPath = Path.Combine(ManageSettings.MoCurrentGameProfilesDirPath, data.ProfileName, ManageSettings.MoProfileBoundExesName);
            File.WriteAllLines(boundExeListPath, data.BoundExes);
        }
    }

    class ProfiledData
    {
        public string ProfileName { get; set; }
        public List<string> BoundExes { get; set; }
    }

    class ProfileBoundExesData
    {
        private readonly Dictionary<string, string> _exeProfilePair = new Dictionary<string, string>();

        private readonly Dictionary<string, List<string>> _profileBoundExes = new Dictionary<string, List<string>>();

        private readonly List<ProfiledData> _profileDataList = new List<ProfiledData>();

        public List<ProfiledData> GetProfilesData() => _profileDataList;

        public IEnumerable<string> Profiles => _profileBoundExes.Keys;

        public bool ProfileContainsExe(string profileName, string exePath)
        {
            return _profileBoundExes.ContainsKey(profileName) && _profileBoundExes[profileName].Contains(exePath);
        }

        public bool AlreadyExistInTheOtherProfile(string exePath, string profileName)
        {
            return _exeProfilePair.ContainsKey(exePath) && _exeProfilePair[exePath] != profileName;
        }

        public void AddExeToProfile(string exePath, string profileName)
        {
            if (_exeProfilePair.TryGetValue(exePath, out string boundProfileName))
            {
                if (boundProfileName != profileName)
                {
                    _profileBoundExes[boundProfileName].Remove(exePath); // remove from old profile
                    if (!_profileBoundExes.ContainsKey(exePath))
                    {
                        _profileBoundExes[profileName].Add(exePath);
                        _exeProfilePair[exePath] = profileName;
                    }
                }
            }
            else
            {
                _profileBoundExes[profileName].Add(exePath);
                _exeProfilePair.Add(exePath, profileName);
            }
        }
        public void RemoveExeFromProfile(string exePath, string profileName)
        {
            if (_exeProfilePair.TryGetValue(exePath, out string boundProfileName))
            {
                if (boundProfileName == profileName)
                {
                    _profileBoundExes[profileName].Remove(exePath);
                    _exeProfilePair.Remove(exePath);
                }
            }
        }

        public void AddProfileData(string profileName, List<string> boundExes)
        {
            var filteredExes = FilterExes(profileName, boundExes);
            _profileBoundExes.Add(profileName, filteredExes);
            _profileDataList.Add(new ProfiledData
            {
                ProfileName = profileName,
                BoundExes = filteredExes
            });
        }

        // filter out exes that are already bound to another profile
        private List<string> FilterExes(string profileName, List<string> boundExes)
        {
            List<string> filteredExes = new List<string>();
            foreach (var exe in boundExes)
            {
                if (_exeProfilePair.ContainsKey(exe))
                {
                    // Exe is already bound to another profile, skip it
                    continue;
                }
                filteredExes.Add(exe);
                _exeProfilePair.Add(exe, profileName);
            }

            return filteredExes;
        }

        internal List<string> GetBoundExes(string profileName)
        {   
            if(string.IsNullOrEmpty(profileName))
            {
                throw new ArgumentNullException(nameof(profileName));
            }

            if (_profileBoundExes.TryGetValue(profileName, out List<string> boundExes))
            {
                return boundExes;
            }

            var emptyBoundExesList = new List<string>();
            _profileBoundExes.Add(profileName, emptyBoundExesList);
            return emptyBoundExesList; // or return an empty list if preferred
        }
    }
}
