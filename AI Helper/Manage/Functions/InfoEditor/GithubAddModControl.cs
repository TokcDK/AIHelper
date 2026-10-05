using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AIHelper.Data.Modlist;
using AIHelper.Manage.Update;
using NLog;

namespace AIHelper.Manage.Functions.InfoEditor
{
    /// <summary>
    /// The add-mod form of the github source: the fields which describe a mod to add, the
    /// buttons which open what has been entered, and the button which asks for it to be
    /// added.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The form is built on a table rather than measured by hand: the caption column is a
    /// fixed width and the value column takes whatever the tab has, so the fields keep the
    /// width of the window instead of a width guessed before the tab was ever shown.
    /// </para>
    /// <para>
    /// The control reports into the editor's log box through the callback it was given and
    /// never touches the mods folder; it hands what the user typed to the editor as a
    /// request and lets the editor decide what to do with it.
    /// </para>
    /// </remarks>
    internal sealed class GithubAddModControl : UpdateSourceAddControl
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>Directory name offered before a url has been entered.</summary>
        private const string DefaultModDirName = "NewMod";

        /// <summary>Url offered as the shape of what is expected.</summary>
        private const string DefaultModUrl = "https://github.com/Owner/Name";

        /// <summary>Width of the column the captions are laid out in.</summary>
        private const int CaptionColumnWidth = 150;

        /// <summary>Height of the row the buttons sit in.</summary>
        private const int ButtonRowHeight = 30;

        /// <summary>
        /// One field of the form: its caption, the value it holds and the text box the value
        /// is edited in.
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

        private readonly NewModField _modDirNameField;
        private readonly NewModField _urlField;
        private readonly NewModField _fileStartsWithField;
        private readonly NewModField _fileEndsWithField;

        /// <summary>
        /// True while the form is being built. Wiring a field to its value raises the change
        /// events before the user has done anything, and this is what keeps those from being
        /// taken for edits. It belongs to the control rather than to the editor, because the
        /// editor holds one form per source and one form's loading must not mute another's.
        /// </summary>
        private readonly bool _isLoading;

        internal GithubAddModControl(Action<string> log)
            : base(log)
        {
            _modDirNameField = new NewModField(T._("Mod dir name"), DefaultModDirName);
            _urlField = new NewModField(T._("Url"), DefaultModUrl);
            _fileStartsWithField = new NewModField(T._("File starts with"), "");
            _fileEndsWithField = new NewModField(T._("File ends with"), "");

            _isLoading = true;
            SetControl(Build());
            _isLoading = false;
        }

        private Control Build()
        {
            var form = new TableLayoutPanel
            {
                // Sized to its content and docked to the top. A table which fills its page
                // hands the height it does not need to its last row, which would leave the
                // buttons above a band of empty space; the page scrolls instead.
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                Margin = new Padding(0),
            };
            form.ColumnStyles.Add(new ColumnStyle { SizeType = SizeType.Absolute, Width = CaptionColumnWidth });
            form.ColumnStyles.Add(new ColumnStyle { SizeType = SizeType.Percent, Width = 100 });

            var row = 0;

            AddTextField(form, _modDirNameField, ref row);
            AddTextField(form, _urlField, ref row);
            AddTextField(form, _fileStartsWithField, ref row);
            AddTextField(form, _fileEndsWithField, ref row);

            // Kept as a local rather than a field: it is the one control of the form which is
            // neither a text box nor a panel, and the button which reads it is built here too,
            // so nothing outside this method needs to reach it.
            var versionFromFile = new CheckBox
            {
                Text = T._("Version from file"),
                Checked = false,
                AutoSize = true,
                Margin = new Padding(0),
            };
            AddFullWidthRow(form, versionFromFile, ref row);

            AddFullWidthRow(form, BuildButtons(versionFromFile), ref row);

            // Set explicitly: the rows are filled by adding to a cell, and a table whose
            // RowCount was never assigned reports zero however many rows it holds, so the
            // count no longer describes the form.
            form.RowCount = row;

            return form;
        }

        /// <summary>
        /// One caption and value row of the form, wired to the field it edits.
        /// </summary>
        private void AddTextField(TableLayoutPanel form, NewModField field, ref int row)
        {
            var label = new Label
            {
                Text = field.LabelText + ":",
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0),
            };

            var textBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(1, 1, 10, 1),
            };
            textBox.DataBindings.Add(new Binding(
                nameof(textBox.Text), field, nameof(field.Value), true, DataSourceUpdateMode.OnPropertyChanged));

            textBox.TextChanged += (o, e) =>
            {
                if (_isLoading) return;

                // compared by identity rather than by caption: the caption is shown text and
                // may be translated, the entry itself is what identifies it
                if (ReferenceEquals(field, _urlField)) SetModNameFromUrl(_urlField.Value, _modDirNameField);
            };
            field.Editor = textBox;

            // The caption and the box take one column each. Spanning the box over the whole
            // width would start its cell past the last column and the layout would put it
            // back into the caption column.
            form.RowStyles.Add(new RowStyle { SizeType = SizeType.AutoSize });
            form.Controls.Add(label, 0, row);
            form.Controls.Add(textBox, 1, row);

            row++;
        }

        /// <summary>
        /// Adds a row which holds one control over the whole width of the form.
        /// </summary>
        private static void AddFullWidthRow(TableLayoutPanel form, Control control, ref int row)
        {
            form.RowStyles.Add(new RowStyle { SizeType = SizeType.AutoSize });
            form.Controls.Add(control, 0, row);
            form.SetColumnSpan(control, form.ColumnCount);

            row++;
        }

        private FlowLayoutPanel BuildButtons(CheckBox versionFromFile)
        {
            var buttonsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Margin = new Padding(0),
            };

            buttonsPanel.Controls.Add(CreateWebButton());
            buttonsPanel.Controls.Add(CreateDirButton());
            buttonsPanel.Controls.Add(CreateAddButton(versionFromFile));

            // The row is given a height of its own rather than measured: the panel is added
            // before it has a size, and an auto sized row would take it as zero.
            buttonsPanel.MinimumSize = new Size(0, ButtonRowHeight);

            return buttonsPanel;
        }

        /// <summary>
        /// Opens the page of the url which has been typed.
        /// </summary>
        private Button CreateWebButton()
        {
            var button = new Button { Text = T._("Web"), AutoSize = true };
            button.Click += (o, e) => ManageProcess.SimpleRunProcess(_urlField.Value);

            return button;
        }

        /// <summary>
        /// Opens the folder the mod would be added as, when it is already there.
        /// </summary>
        private Button CreateDirButton()
        {
            var button = new Button { Text = T._("Dir"), AutoSize = true };
            button.Click += (o, e) =>
            {
                var dirPath = Path.Combine(ManageSettings.CurrentGameModsDirPath, _modDirNameField.Value);
                if (!Directory.Exists(dirPath))
                {
                    Log(T._("Mod dir missing. Mod still is not added!"));
                    return;
                }

                ManageProcess.SimpleRunProcess(dirPath);
            };

            return button;
        }

        /// <summary>
        /// Asks the editor to add the mod the form describes.
        /// </summary>
        private Button CreateAddButton(CheckBox versionFromFile)
        {
            var button = new Button { Text = T._("Add"), AutoSize = true };
            button.Click += (o, e) =>
            {
                if (_isLoading) return;

                var request = BuildRequest(versionFromFile);
                if (request == null) return;

                RaiseAddRequested(request);
            };

            return button;
        }

        /// <summary>
        /// Fills the mod directory name from the repository url the user typed, unless the
        /// name was already set to something of their own.
        /// </summary>
        /// <remarks>
        /// The url offered as an example is refused here rather than being trusted to be
        /// harmless. A binding pushes the stored value into its field when the form is first
        /// realized — which is after the constructor returns, so the loading flag is already
        /// down — and that push raises the same change event as typing does. Taking a name out
        /// of the example would name the folder after the placeholder's own repository.
        /// </remarks>
        private void SetModNameFromUrl(string url, NewModField modDirNameField)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            if (url == DefaultModUrl) return;

            // A name the user has already changed is left alone: the url is there to fill a
            // gap, not to overwrite a choice.
            if (!string.IsNullOrWhiteSpace(modDirNameField.Value)
                && modDirNameField.Value != DefaultModDirName) return;

            try
            {
                if (!GithubUrl.TryParse(url, out string owner, out string repository)) return;

                modDirNameField.Editor.Text = repository;

                Log(T._("Mod name set from url"));
            }
            catch (Exception ex)
            {
                _log.Warn("Failed to set the mod name from the url. Error:\r\n" + ex);
            }
        }

        /// <summary>
        /// Turns the values of the form into the request the editor adds a mod from.
        /// </summary>
        /// <returns>The request, or null when the form cannot be used.</returns>
        /// <remarks>
        /// What is checked here is the form itself — whether the fields say enough to look a
        /// release up. Whether the name may be used as a folder is the editor's check, because
        /// it is the editor which combines it with the mods directory.
        /// </remarks>
        private AddModRequestEventArgs BuildRequest(CheckBox versionFromFile)
        {
            if (string.IsNullOrWhiteSpace(_modDirNameField.Value))
            {
                Log(T._("Mod name is empty"));
                return null;
            }

            if (string.IsNullOrWhiteSpace(_urlField.Value))
            {
                Log(T._("Url s empty"));
                return null;
            }

            if (string.IsNullOrWhiteSpace(_fileStartsWithField.Value))
            {
                Log($"{_fileStartsWithField.LabelText} {T._("value is empty")}");
                return null;
            }

            string owner;
            string repository;
            if (!GithubUrl.TryParse(_urlField.Value, out owner, out repository))
            {
                Log(T._("Url is not recognized"));
                return null;
            }

            // The path is not set here: the mods directory belongs to the editor, and the name
            // is not a path until the editor has checked it against that directory.
            var mod = new ModData
            {
                Name = _modDirNameField.Value,
                Priority = 9999,
            };

            var data = new GithubUpdateData(mod)
            {
                Owner = owner,
                Repository = repository,
                FileStartsWith = _fileStartsWithField.Value,
                FileEndsWith = _fileEndsWithField.Value,
                VersionFromFile = versionFromFile.Checked,
            };

            return new AddModRequestEventArgs(_modDirNameField.Value, data);
        }
    }
}
