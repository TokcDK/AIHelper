using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AIHelper.Manage.Functions.BindMOProfileToExe
{
    public partial class BindForm : Form
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public BindForm()
        {
            InitializeComponent();
        }

        private List<string> currentGameExeList = new List<string>();
        private ProfileBoundExesData profilesList = new ProfileBoundExesData();

        private void ProfilesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ProfilesComboBox.SelectedItem is ProfiledData data)
            {
                BoundExesListBox.DataSource = data.BoundExes;
            }
        }

        private static IEnumerable<string> GetBoundExes(string profileName)
        {
            var boundExeListPath = Path.Combine(ManageSettings.MoCurrentGameProfilesDirPath, profileName, ManageSettings.MoProfileBoundExesName);
            if (!File.Exists(boundExeListPath))
            {
                return Enumerable.Empty<string>();
            }
            try
            {
                // return only existing unique exe file paths
                return File.ReadAllLines(boundExeListPath)
                    .Where(l => !string.IsNullOrWhiteSpace(l) && File.Exists(l))
                    .Distinct();
            }
            catch (Exception ex)  // Catch IOException or general Exception
            {
                // Log if available, or show message in UI context
                _logger.Debug($"Error reading bound exes for {profileName}: {ex.Message}");
                return Enumerable.Empty<string>();
            }
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
            var profileDirs = new DirectoryInfo(ManageSettings.MoCurrentGameProfilesDirPath).GetDirectories()
                                       .OrderByDescending(profileDir => profileDir.LastWriteTime); // order first which was modified last
            foreach (var profileDir in profileDirs)
            {
                var profileName = profileDir.Name;
                var boundExes = GetBoundExes(profileName).ToList();
                profilesList.AddProfileData(profileName, boundExes);
            }

            var bs = new BindingSource
            {
                DataSource = profilesList.GetProfiles(),
                //DataMember = "ProfileName"
            };
            ProfilesComboBox.DataSource = bs;
            ProfilesComboBox.DisplayMember = "ProfileName";
            ProfilesComboBox.SelectedIndex = ProfilesComboBox.FindStringExact(ManageSettings.GetMoSelectedProfileDirName());

            // load game exes
            currentGameExeList = new List<string>();
            var studioExeName = ManageSettings.Games.Game.GameStudioExeName;
            var gameExePath = Path.Combine(gameDirPath, ManageSettings.DataDirName, gameExeName + ".exe");
            if (File.Exists(gameExePath))
            {
                currentGameExeList.Add(gameExePath);
            }
            var studioExePath = Path.Combine(gameDirPath, ManageSettings.DataDirName, studioExeName + ".exe");
            if (File.Exists(studioExePath))
            {
                currentGameExeList.Add(studioExePath);
            }
            ExesComboBox.DataSource = currentGameExeList;
        }

        private void AddExeButton_Click(object sender, EventArgs e)
        {
            if (ExesComboBox.SelectedItem is string exePath
                && ProfilesComboBox.SelectedItem is ProfiledData data
                && !data.BoundExes.Contains(exePath))  // Use pattern matching; check Contains on BindingList
            {
                data.Add(exePath); 
                WriteBoundExes();
            }
        }

        private void RemoveExeButton_Click(object sender, EventArgs e)
        {
            if (BoundExesListBox.SelectedItem is string exePath
                && ProfilesComboBox.SelectedItem is ProfiledData data)
            {
                data.Remove(exePath);
                WriteBoundExes();
            }
        }

        private void WriteBoundExes()
        {
            if (ProfilesComboBox.SelectedItem is ProfiledData data)
            {
                var boundExeListPath = Path.Combine(ManageSettings.MoCurrentGameProfilesDirPath, data.ProfileName, ManageSettings.MoProfileBoundExesName);
                try
                {
                    File.WriteAllLines(boundExeListPath, data.BoundExes);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Error writing bound exes for profile '{data.ProfileName}'");
                    MessageBox.Show($"Error writing bound exes for profile '{data.ProfileName}': {ex.Message}", "Write Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    internal class ProfiledData
    {
        private readonly ProfiledDataFactory _parent;

        public ProfiledData(ProfiledDataFactory parent)
        {
            _parent = parent;
        }

        public string ProfileName { get; set; }
        public BindingList<string> BoundExes { get; set; } = new BindingList<string>();

        public void Add(string exePath)
        {
            _parent.AddExeToProfile(exePath, ProfileName);
        }
        public void Remove(string exePath)
        {
            _parent.RemoveExeFromEverywhere(exePath);
        }
    }

    internal class ProfiledDataFactory
    {
        private readonly Action<string, string> _addExeToProfile;
        private readonly Action<string> _removeExeFromEverywhere;

        public ProfiledDataFactory(Action<string, string> addExeToProfile, Action<string> removeExeFromEverywhere)
        {
            _addExeToProfile = addExeToProfile;
            _removeExeFromEverywhere = removeExeFromEverywhere;
        }

        public void RemoveExeFromEverywhere(string exePath) => _removeExeFromEverywhere(exePath);
        public void AddExeToProfile(string exePath, string profileName) => _addExeToProfile(exePath, profileName);
    }

    internal class ProfileBoundExesData
    {
        // _exeProfilePair made to control the exe containing in other profiles
        // because we can have exe bound only for one profile
        private readonly Dictionary<string, string> _exeProfilePairs = new Dictionary<string, string>();
        private readonly Dictionary<string, ProfiledData> _profileDataReferenced = new Dictionary<string, ProfiledData>();

        private readonly List<ProfiledData> _profileDataList = new List<ProfiledData>();

        private readonly ProfiledDataFactory _factory;

        public ProfileBoundExesData()
        {
            _factory = new ProfiledDataFactory(AddExeToProfile, RemoveExeFromEverywhere);
        }

        public List<ProfiledData> GetProfiles() => _profileDataList;
        private void RemoveExeFromEverywhere(string exePath)
        {
            if (_exeProfilePairs.TryGetValue(exePath, out string profileName))
            {
                if (_profileDataReferenced.TryGetValue(profileName, out ProfiledData profileData))
                {
                    profileData.BoundExes.Remove(exePath);
                }
                _exeProfilePairs.Remove(exePath);
            }
        }

        private void AddExeToProfile(string exePath, string profileName)
        {
            RemoveExeFromEverywhere(exePath);
            if (_profileDataReferenced.TryGetValue(profileName, out ProfiledData profileData))
            {
                profileData.BoundExes.Add(exePath);
                _exeProfilePairs.Add(exePath, profileName);
            }
            else
            {
                throw new ArgumentException($"Profile '{profileName}' does not exist.", nameof(profileName));
            }
        }

        public void AddProfileData(string profileName, List<string> boundExes)
        {
            var filteredExes = FilterExes(profileName, boundExes);
            var profileData = new ProfiledData(_factory)
            {
                ProfileName = profileName,
                BoundExes = new BindingList<string>(filteredExes)
            };
            _profileDataReferenced.Add(profileData.ProfileName, profileData);
            _profileDataList.Add(profileData);
        }

        // filter out exes that are already bound to another profile
        private List<string> FilterExes(string profileName, List<string> boundExes)
        {
            List<string> filteredExes = new List<string>();
            foreach (var exe in boundExes)
            {
                if (_exeProfilePairs.ContainsKey(exe))
                {
                    // Exe is already bound to another profile, skip it
                    continue;
                }
                filteredExes.Add(exe);
                _exeProfilePairs.Add(exe, profileName);
            }

            return filteredExes;
        }
    }
}
