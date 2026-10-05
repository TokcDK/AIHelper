using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using AIHelper.Data.Modlist;
using AIHelper.Manage.ui.themes;
using AIHelper.Manage.Update;

namespace AIHelper.Manage.Functions
{
    /// <summary>
    /// The "Add" tab of the mod info editor: the form which adds a mod from the url of
    /// its release.
    /// </summary>
    /// <remarks>
    /// Kept beside <see cref="InfoEditorForFlp"/> as a part of it rather than as a class of
    /// its own, because the form has no meaning outside the session which owns the mod list
    /// and the log box it reports into. What it does have is a concern of its own — reading
    /// a form and turning it into a request — and that is what lives in this file.
    /// </remarks>
    internal partial class InfoEditorForFlp
    {
        /// <summary>Directory name offered before a url has been entered.</summary>
        const string DefaultModDirName = "NewMod";

        /// <summary>Url offered as the shape of what is expected.</summary>
        const string DefaultModUrl = "https://github.com/Owner/Name";

        /// <summary>
        /// Narrowest the add-mod tab is laid out at while it has not been shown yet and its
        /// real width is still unknown.
        /// </summary>
        const int MinAddModPanelWidth = 200;

        /// <summary>Width of the caption of a field of the form.</summary>
        const int AddModLabelWidth = 150;

        /// <summary>Room a value field leaves for the caption and for its margins.</summary>
        const int AddModValueWidthReserve = 20;

        /// <summary>How much narrower than the tab the log box is kept.</summary>
        const int LogBoxWidthReserve = 10;

        /// <summary>
        /// One field of the add-mod form: its caption, the value it holds and the text box
        /// the value is edited in.
        /// </summary>
        /// <remarks>
        /// The value is public because the text box is bound to it, and a binding only
        /// reaches public members.
        /// </remarks>
        private sealed class NewModField
        {
            internal NewModField(string labelText, string value)
            {
                LabelText = labelText;
                Value = value;
            }

            internal string LabelText { get; }

            /// <summary>Value of the field, kept in step with the text box by a binding.</summary>
            public string Value { get; set; }

            /// <summary>Text box the value is edited in. Set while the form is built.</summary>
            internal TextBox Editor { get; set; }
        }

        /// <summary>
        /// Builds the add-mod tab: the fields describing a mod to add, the buttons which open
        /// what has been entered, and the log box the download reports into.
        /// </summary>
        private void LoadAddModPanel(Control addModContainer)
        {
            // The fields are wired to their values as they are built, which raises the change
            // events before the user has done anything. The flag is what keeps those from
            // being taken for edits.
            bool addModPanelIsLoading = true;

            addModContainer.Controls.Clear();

            var modUpdateInfoMainFlp = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                FlowDirection = FlowDirection.TopDown,
            };

            // Tab page content gets its real width only when the tab is displayed;
            // guard against an uninitialized width so control sizes stay valid.
            int containerWidth = addModContainer.Width > MinAddModPanelWidth ? addModContainer.Width : MinAddModPanelWidth;

            var modDirNameField = new NewModField(T._("Mod dir name"), DefaultModDirName);
            var urlField = new NewModField(T._("Url"), DefaultModUrl);
            var fileStartsWithField = new NewModField(T._("File starts with"), "");
            var fileEndsWithField = new NewModField(T._("File ends with"), "");
            var fields = new[] { modDirNameField, urlField, fileStartsWithField, fileEndsWithField };

            foreach (var field in fields)
            {
                var propertyFlp = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    Margin = new Padding(0),
                };
                var label = new Label
                {
                    Text = field.LabelText + ":",
                    Size = new Size(AddModLabelWidth, ElementHeight),
                };
                var valueTextBox = new TextBox
                {
                    Size = new Size(containerWidth - label.Width - AddModValueWidthReserve, ElementHeight),
                    Margin = new Padding(1, 1, 10, 1),
                };
                valueTextBox.DataBindings.Add(new Binding(
                    nameof(valueTextBox.Text), field, nameof(field.Value), true, DataSourceUpdateMode.OnPropertyChanged));

                valueTextBox.TextChanged += (o, e) =>
                {
                    if (addModPanelIsLoading) return;

                    // compared by identity rather than by caption: the caption is shown text
                    // and may be translated, the entry itself is what identifies it
                    if (ReferenceEquals(field, urlField)) SetModNameFromUrl(urlField.Value, modDirNameField);
                };
                field.Editor = valueTextBox;

                propertyFlp.Controls.Add(label);
                propertyFlp.Controls.Add(valueTextBox);

                var labelWidth = label.Width + (label.Margin.Horizontal * 2);
                var labelHeight = label.Height + (label.Margin.Vertical * 2);
                var boxWidth = valueTextBox.Width + (valueTextBox.Margin.Horizontal * 2);
                var boxHeight = valueTextBox.Height + (valueTextBox.Margin.Vertical * 2);
                propertyFlp.Size = new Size(labelWidth + boxWidth, labelHeight + boxHeight);

                modUpdateInfoMainFlp.Controls.Add(propertyFlp);
            }

            var versionFromFile = new CheckBox
            {
                Text = T._("Version from file"),
                Checked = false,
                Size = new Size(containerWidth - AddModValueWidthReserve, ElementHeight + 5),
                TextAlign = ContentAlignment.MiddleLeft,
            };
            modUpdateInfoMainFlp.Controls.Add(versionFromFile);

            var openWebButton = new Button
            {
                Text = T._("Web")
            };
            openWebButton.Click += (o, e) => ManageProcess.SimpleRunProcess(urlField.Value);
            modUpdateInfoMainFlp.Controls.Add(openWebButton);

            var openDirButton = new Button
            {
                Text = T._("Dir")
            };
            openDirButton.Click += (o, e) =>
            {
                var dirPath = Path.Combine(ManageSettings.CurrentGameModsDirPath, modDirNameField.Value);
                if (!Directory.Exists(dirPath))
                {
                    Log(T._("Mod dir missing. Mod still is not added!"));
                    return;
                }

                ManageProcess.SimpleRunProcess(dirPath);
            };
            modUpdateInfoMainFlp.Controls.Add(openDirButton);

            var downloadAndAddMod = new Button
            {
                Text = T._("Add")
            };
            downloadAndAddMod.Click += async (o, e) =>
            {
                if (addModPanelIsLoading) return;

                await AddModFromFormAsync(
                    modDirNameField, urlField, fileStartsWithField, fileEndsWithField, versionFromFile)
                    .ConfigureAwait(true);
            };
            modUpdateInfoMainFlp.Controls.Add(downloadAndAddMod);

            var logBox = EnsureLogBox();
            logBox.Width = containerWidth - LogBoxWidthReserve;
            modUpdateInfoMainFlp.Controls.Add(logBox);

            addModContainer.Controls.Add(modUpdateInfoMainFlp);

            ThemesLoader.SetTheme(ManageSettings.CurrentTheme, addModContainer);

            addModPanelIsLoading = false;
        }

        /// <summary>
        /// Fills the mod directory name from the repository url the user typed, unless the
        /// name was already set to something of their own.
        /// </summary>
        private void SetModNameFromUrl(string url, NewModField modDirNameField)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            // A name the user has already changed is left alone: the url is there to fill a
            // gap, not to overwrite a choice.
            if (!string.IsNullOrWhiteSpace(modDirNameField.Value)
                && modDirNameField.Value != DefaultModDirName) return;

            try
            {
                string owner;
                string repository;
                if (!GithubUrl.TryParse(url, out owner, out repository)) return;

                modDirNameField.Editor.Text = repository;

                Log(T._("Mod name set from url"));
            }
            catch (Exception ex)
            {
                _log.Warn("Failed to set the mod name from the url. Error:\r\n" + ex);
            }
        }

        /// <summary>
        /// Handles the "Add" button: checks what the form holds, then asks for the mod to be
        /// downloaded and added.
        /// </summary>
        private async Task AddModFromFormAsync(
            NewModField modDirNameField,
            NewModField urlField,
            NewModField fileStartsWithField,
            NewModField fileEndsWithField,
            CheckBox versionFromFile)
        {
            try
            {
                var newRecord = BuildNewModRecord(
                    modDirNameField, urlField, fileStartsWithField, fileEndsWithField, versionFromFile);
                if (newRecord == null) return;

                var targetDirPathInfo = new DirectoryInfo(
                    Path.Combine(ManageSettings.CurrentGameModsDirPath, modDirNameField.Value));
                if (targetDirPathInfo.Exists)
                {
                    Log(T._("Target mod dir is exists!"));
                    return;
                }

                foreach (var modInfoBlock in _modInfoBlocks)
                {
                    if (!newRecord.PointsAtSameFileAs(modInfoBlock.InfoData.GitInfo.ToRecord())) continue;

                    Log(T._("Mod '{0}' has the same Owner, Repository, start/end", modInfoBlock.InfoData.ModName));
                    return;
                }

                var modData = new ModData
                {
                    Path = targetDirPathInfo.FullName,
                    Name = modDirNameField.Value,
                    Priority = 9999
                };

                var ginfo = new GitUpdateInfoData(modData)
                {
                    Owner = newRecord.Owner,
                    Repository = newRecord.Repository,
                    FileStartsWith = newRecord.FileStartsWith,
                    FileEndsWith = newRecord.FileEndsWith,
                    VersionFromFile = newRecord.VersionFromFile
                };

                await AddModAsync(newRecord, ginfo).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                Log(T._("Error: ") + ex.Message);
            }
        }

        /// <summary>
        /// Turns the values of the add-mod form into the record the download needs.
        /// </summary>
        /// <returns>The record, or null when the form cannot be used.</returns>
        private GithubUpdateRecord BuildNewModRecord(
            NewModField modDirNameField,
            NewModField urlField,
            NewModField fileStartsWithField,
            NewModField fileEndsWithField,
            CheckBox versionFromFile)
        {
            if (string.IsNullOrWhiteSpace(modDirNameField.Value))
            {
                Log(T._("Mod name is empty"));
                return null;
            }

            if (!IsValidModDirName(modDirNameField.Value))
            {
                Log(T._("Mod dir name is not a valid folder name"));
                return null;
            }

            if (string.IsNullOrWhiteSpace(urlField.Value))
            {
                Log(T._("Url s empty"));
                return null;
            }

            if (string.IsNullOrWhiteSpace(fileStartsWithField.Value))
            {
                Log($"{fileStartsWithField.LabelText} {T._("value is empty")}");
                return null;
            }

            string owner;
            string repository;
            if (!GithubUrl.TryParse(urlField.Value, out owner, out repository))
            {
                Log(T._("Url is not recognized"));
                return null;
            }

            return new GithubUpdateRecord(
                owner,
                repository,
                fileStartsWithField.Value,
                fileEndsWithField.Value,
                versionFromFile.Checked);
        }

        /// <summary>
        /// True when the name can be used as the folder of a mod inside the mods directory.
        /// </summary>
        /// <remarks>
        /// The name is typed by the user and is combined with the mods directory, and
        /// <see cref="Path.Combine(string, string)"/> returns the second path unchanged when
        /// it is rooted, so a name carrying a drive or a separator would put the mod
        /// somewhere else entirely.
        /// </remarks>
        private static bool IsValidModDirName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            if (name != name.Trim()) return false;
            if (name == "." || name == "..") return false;
            if (Path.IsPathRooted(name)) return false;
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return false;

            return true;
        }
    }
}
