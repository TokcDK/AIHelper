using AIHelper.Games.Settings;
using AIHelper.Manage;
using NLog;
using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace AIHelper.Games.Illusion
{
    /// <summary>
    /// Settings tab of the Illusion games. Edits the <c>UserData\setup.xml</c> of
    /// the currently selected game.
    /// </summary>
    public partial class IllusionGameSettingsUserControl : UserControl
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private const string SIZE_SETTING_KEY = "Size";
        private const string WIDTH_SETTING_KEY = "Width";
        private const string HEIGHT_SETTING_KEY = "Height";
        private const string QUALITY_SETTING_KEY = "Quality";
        private const string FULLSCREEN_SETTING_KEY = "FullScreen";
        private const string DISPLAY_SETTING_KEY = "Display";
        private const string LANGUAGE_SETTING_KEY = "Language";

        private const string DEFAULT_SCREEN_SIZE_LABEL = "1280 x 720 (16 : 9)";
        private const string DEFAULT_SCREEN_WIDTH = "1280";
        private const string DEFAULT_SCREEN_HEIGHT = "720";
        private const int DEFAULT_QUALITY = 2;
        private const bool DEFAULT_FULLSCREEN = false;
        private const int DEFAULT_DISPLAY = 0;
        private const int DEFAULT_LANGUAGE = 0;

        /// <summary>
        /// Number of monitors the game can be told to use.
        /// </summary>
        private const int DISPLAY_COUNT = 4;

        /// <summary>
        /// Row count of the grid: one row of links plus resolution, quality,
        /// language, display and full screen.
        /// </summary>
        private const int LAYOUT_ROWS = 6;

        private static readonly string[] QualityItems = { "Performance", "Normal", "Quality" };
        private static readonly string[] LanguageItems = { T._("Japanese"), T._("English"), T._("German"), T._("French") };

        private readonly SettingsXmlFile _settings;
        private readonly ScreenResolutions _resolutions = new ScreenResolutions();

        private TableLayoutPanel _table;
        private ComboBox _cboResolution;
        private ComboBox _cboQuality;
        private ComboBox _cboLanguage;
        private ComboBox _cboDisplay;
        private CheckBox _chkFullScreen;

        /// <summary>
        /// Set while the controls are filled from the file, so the change
        /// handlers do not write the values straight back.
        /// </summary>
        private bool _loading;

        public IllusionGameSettingsUserControl()
        {
            InitializeComponent();

            _settings = new SettingsXmlFile(ManageSettings.SetupXmlPath, Encoding.Unicode, CreateDefaultSetupXml);

            AutoScaleMode = AutoScaleMode.Font;
            MinimumSize = new System.Drawing.Size(450, 165);
            Size = new System.Drawing.Size(450, 165);

            BuildLayout();
            _settings.Load();
            PopulateControls();
        }

        /// <summary>
        /// Builds the contents of a setup file that does not exist yet.
        /// </summary>
        private static XDocument CreateDefaultSetupXml()
        {
            return new XDocument(
                    new XDeclaration("1.0", "utf-16", null),
                    new XElement("Setting",
                        new XElement(SIZE_SETTING_KEY, DEFAULT_SCREEN_SIZE_LABEL),
                        new XElement(WIDTH_SETTING_KEY, DEFAULT_SCREEN_WIDTH),
                        new XElement(HEIGHT_SETTING_KEY, DEFAULT_SCREEN_HEIGHT),
                        new XElement(QUALITY_SETTING_KEY, DEFAULT_QUALITY),
                        new XElement(FULLSCREEN_SETTING_KEY, DEFAULT_FULLSCREEN),
                        new XElement(DISPLAY_SETTING_KEY, DEFAULT_DISPLAY),
                        new XElement(LANGUAGE_SETTING_KEY, DEFAULT_LANGUAGE)
                    ));
        }

        // ── UI Construction ───────────────────────────────────────────
        private void BuildLayout()
        {
            SuspendLayout();

            _table = SettingsLayout.CreateTable(LAYOUT_ROWS);

            var openLink = SettingsLayout.CreateLink(T._("Open game setup file"), OnOpenSetupFile);
            var reloadLink = SettingsLayout.CreateLink(T._("Reload settings from XML"), OnReloadFromXml);

            _cboResolution = SettingsLayout.CreateDropDown(_resolutions.Labels(), OnResolutionChanged);
            _cboQuality = SettingsLayout.CreateDropDown(QualityItems, OnSettingChanged);
            _cboLanguage = SettingsLayout.CreateDropDown(LanguageItems, OnSettingChanged);
            _cboDisplay = SettingsLayout.CreateDropDown(
                Enumerable.Range(0, DISPLAY_COUNT).Select(i => String.Format(T._("Display {0}"), i)).ToArray(),
                OnSettingChanged);
            _chkFullScreen = SettingsLayout.CreateCheckBox(OnSettingChanged);

            _table.Controls.Add(openLink, 0, 0);
            _table.Controls.Add(reloadLink, 1, 0);
            SettingsLayout.AddRow(_table, 1, T._("Resolution:"), _cboResolution);
            SettingsLayout.AddRow(_table, 2, T._("Quality:"), _cboQuality);
            SettingsLayout.AddRow(_table, 3, T._("Language:"), _cboLanguage);
            SettingsLayout.AddRow(_table, 4, T._("Display:"), _cboDisplay);
            SettingsLayout.AddRow(_table, 5, T._("Full Screen:"), _chkFullScreen);

            Controls.Add(_table);
            ResumeLayout(true);
        }

        // ── Populate UI from the file ─────────────────────────────────
        private void PopulateControls()
        {
            _loading = true;
            try
            {
                SelectResolution();

                if (int.TryParse(_settings.GetValue(QUALITY_SETTING_KEY), out var quality))
                {
                    _cboQuality.SelectedIndex = ManageMath.Clamp(quality, 0, QualityItems.Length - 1);
                }

                if (int.TryParse(_settings.GetValue(LANGUAGE_SETTING_KEY), out var language))
                {
                    _cboLanguage.SelectedIndex = ManageMath.Clamp(language, 0, LanguageItems.Length - 1);
                }

                if (int.TryParse(_settings.GetValue(DISPLAY_SETTING_KEY), out var display))
                {
                    _cboDisplay.SelectedIndex = ManageMath.Clamp(display, 0, _cboDisplay.Items.Count - 1);
                }

                _chkFullScreen.Checked = _settings.GetValue(FULLSCREEN_SETTING_KEY).Equals("true", StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                _loading = false;
            }

            // the changes above are only selection changes, but a resolution that
            // had to be added or repaired is worth writing back
            _settings.Save();
        }

        /// <summary>
        /// Selects the resolution stored in the file, preferring the pixel size
        /// over the label.
        /// </summary>
        private void SelectResolution()
        {
            var width = int.TryParse(_settings.GetValue(WIDTH_SETTING_KEY), out var w) ? w : -1;
            var height = int.TryParse(_settings.GetValue(HEIGHT_SETTING_KEY), out var h) ? h : -1;

            if (width <= 0 || height <= 0)
            {
                // no usable pixel size, fall back to the stored label and make the
                // pixel size consistent with it again
                var byLabel = _resolutions.IndexOfLabel(_settings.GetValue(SIZE_SETTING_KEY));
                _cboResolution.SelectedIndex = byLabel >= 0 ? byLabel : 0;

                var fromLabel = _resolutions[_cboResolution.SelectedIndex];
                _settings.SetValue(WIDTH_SETTING_KEY, fromLabel.Width);
                _settings.SetValue(HEIGHT_SETTING_KEY, fromLabel.Height);
                return;
            }

            var index = _resolutions.IndexOf(width, height);

            if (index < 0)
            {
                // the game uses a resolution the list does not know yet, add it so
                // the user can keep it selected
                index = _resolutions.Add(width, height);
                _cboResolution.Items.Add(_resolutions[index].Label);
            }

            _cboResolution.SelectedIndex = index;

            // the label is what the game reads back, so keep it in sync with the
            // numbers instead of leaving a stale one behind
            _settings.SetValue(SIZE_SETTING_KEY, _resolutions[index].Label);
        }

        // ── Event Handlers ────────────────────────────────────────────
        private void OnOpenSetupFile(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("notepad.exe", _settings.FilePath);
            }
            catch (Exception ex)
            {
                _log.Error("Can not open the game setup file '" + _settings.FilePath + "' in Notepad.\r\n" + ex);
                MessageBox.Show(string.Format(T._("Failed to open Notepad: {0}"), ex.Message), T._("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnReloadFromXml(object sender, EventArgs e)
        {
            _settings.Load();
            PopulateControls();
        }

        private void OnResolutionChanged(object sender, EventArgs e)
        {
            if (_loading || _cboResolution.SelectedIndex < 0) return;

            var resolution = _resolutions[_cboResolution.SelectedIndex];
            _settings.SetValue(SIZE_SETTING_KEY, resolution.Label);
            _settings.SetValue(WIDTH_SETTING_KEY, resolution.Width);
            _settings.SetValue(HEIGHT_SETTING_KEY, resolution.Height);
            _settings.Save();
        }

        private void OnSettingChanged(object sender, EventArgs e)
        {
            if (_loading) return;

            if (sender == _cboQuality)
                _settings.SetValue(QUALITY_SETTING_KEY, _cboQuality.SelectedIndex);
            else if (sender == _cboLanguage)
                _settings.SetValue(LANGUAGE_SETTING_KEY, _cboLanguage.SelectedIndex);
            else if (sender == _cboDisplay)
                _settings.SetValue(DISPLAY_SETTING_KEY, _cboDisplay.SelectedIndex);
            else if (sender == _chkFullScreen)
                _settings.SetValue(FULLSCREEN_SETTING_KEY, _chkFullScreen.Checked.ToString().ToLowerInvariant());

            _settings.Save();
        }
    }
}
