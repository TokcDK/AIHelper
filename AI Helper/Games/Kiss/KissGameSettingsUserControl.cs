using AIHelper.Games.Settings;
using AIHelper.Manage;
using NLog;
using System;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace AIHelper.Games.Kiss
{
    /// <summary>
    /// Settings tab of the Kiss games. Edits the <c>config.xml</c> of the
    /// currently selected game.
    /// </summary>
    public partial class KissGameSettingsUserControl : UserControl
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Category the screen settings live in.
        /// </summary>
        private const string SCREEN_CATEGORY_KEY = "Screen";

        private const string FULLSCREEN_SETTING_KEY = "FullScreen";
        private const string WIDTH_SETTING_KEY = "ScreenSizeNow.width";
        private const string HEIGHT_SETTING_KEY = "ScreenSizeNow.height";
        private const string QUALITY_SETTING_KEY = "TextureQuality";

        private const int DEFAULT_SCREEN_WIDTH = 1280;
        private const int DEFAULT_SCREEN_HEIGHT = 720;
        private const string DEFAULT_QUALITY = "High";
        private const bool DEFAULT_FULLSCREEN = false;

        /// <summary>
        /// Row count of the grid. The Kiss games have fewer settings than the
        /// Illusion ones, the spare rows keep this tab the same height.
        /// </summary>
        private const int LAYOUT_ROWS = 6;

        private static readonly string[] QualityItems = { "Low", "Medium", "High" };

        private readonly SettingsXmlFile _settings;
        private readonly ScreenResolutions _resolutions = new ScreenResolutions();

        private TableLayoutPanel _table;
        private ComboBox _cboResolution;
        private ComboBox _cboQuality;
        private CheckBox _chkFullScreen;

        /// <summary>
        /// Set while the controls are filled from the file, so the change
        /// handlers do not write the values straight back.
        /// </summary>
        private bool _loading;

        public KissGameSettingsUserControl()
        {
            InitializeComponent();

            // the CM3D2 config is UTF-8, a file that already exists keeps its own encoding
            _settings = new SettingsXmlFile(ManageSettings.SetupXmlPath, new UTF8Encoding(false), CreateDefaultSetupXml);

            AutoScaleMode = AutoScaleMode.Font;
            MinimumSize = new System.Drawing.Size(450, 165);
            Size = new System.Drawing.Size(450, 165);

            BuildLayout();
            _settings.Load();
            PopulateControls();
        }

        /// <summary>
        /// Builds the contents of a config file that does not exist yet.
        /// </summary>
        private static XDocument CreateDefaultSetupXml()
        {
            //<?xml version="1.0" encoding="utf-8" standalone="no"?>
            //<!--CM3D2 Config-->
            //<Config Version="22501">
            //  <System>
            //    <SysButtonShowAlways>true</SysButtonShowAlways>
            //  </System>
            //  <Screen>
            //    <FullScreen>false</FullScreen>
            //    <ScreenSizeNow.width>1280</ScreenSizeNow.width>
            //    <ScreenSizeNow.height>720</ScreenSizeNow.height>
            //    <Antialias>X2</Antialias>
            //    <ShadowQuality>Medium</ShadowQuality>
            //    <TextureQuality>High</TextureQuality>
            //    <VSync>false</VSync>
            //    <TargetFPS>60</TargetFPS>
            //    <ViewFps>false</ViewFps>
            //    <Bloom>true</Bloom>
            //    <BloomValue>50</BloomValue>
            //    <ScreenShotSuperSize>X1</ScreenShotSuperSize>
            //    <ManAlpha>50</ManAlpha>
            //  </Screen>
            //</Config>
            return new XDocument(
                new XDeclaration("1.0", "utf-8", "no"),
                new XElement("Config",
                    new XAttribute("Version", "1510"),
                    new XElement("System",
                        new XElement("SysButtonShowAlways", "true")
                    ),
                    new XElement(SCREEN_CATEGORY_KEY,
                        new XElement(FULLSCREEN_SETTING_KEY, DEFAULT_FULLSCREEN.ToString().ToLowerInvariant()),
                        new XElement(WIDTH_SETTING_KEY, DEFAULT_SCREEN_WIDTH),
                        new XElement(HEIGHT_SETTING_KEY, DEFAULT_SCREEN_HEIGHT),
                        new XElement(QUALITY_SETTING_KEY, DEFAULT_QUALITY)
                    )
                )
            );
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
            _chkFullScreen = SettingsLayout.CreateCheckBox(OnSettingChanged);

            _table.Controls.Add(openLink, 0, 0);
            _table.Controls.Add(reloadLink, 1, 0);
            SettingsLayout.AddRow(_table, 1, T._("Resolution:"), _cboResolution);
            SettingsLayout.AddRow(_table, 2, T._("Quality:"), _cboQuality);
            SettingsLayout.AddRow(_table, 3, T._("Full Screen:"), _chkFullScreen);

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

                _cboQuality.SelectedItem = _settings.GetValue(SCREEN_CATEGORY_KEY, QUALITY_SETTING_KEY);

                _chkFullScreen.Checked = _settings.GetValue(SCREEN_CATEGORY_KEY, FULLSCREEN_SETTING_KEY)
                    .Equals("true", StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                _loading = false;
            }

            // the changes above are only selection changes, but a resolution that
            // had to be added is worth writing back
            _settings.Save();
        }

        /// <summary>
        /// Selects the resolution stored in the file, falling back to the default
        /// one when the stored size is unusable.
        /// </summary>
        private void SelectResolution()
        {
            var width = int.TryParse(_settings.GetValue(SCREEN_CATEGORY_KEY, WIDTH_SETTING_KEY), out var w) ? w : -1;
            var height = int.TryParse(_settings.GetValue(SCREEN_CATEGORY_KEY, HEIGHT_SETTING_KEY), out var h) ? h : -1;

            if (width <= 0 || height <= 0)
            {
                _settings.SetValue(SCREEN_CATEGORY_KEY, WIDTH_SETTING_KEY, DEFAULT_SCREEN_WIDTH);
                _settings.SetValue(SCREEN_CATEGORY_KEY, HEIGHT_SETTING_KEY, DEFAULT_SCREEN_HEIGHT);

                var defaultIndex = _resolutions.IndexOf(DEFAULT_SCREEN_WIDTH, DEFAULT_SCREEN_HEIGHT);
                _cboResolution.SelectedIndex = defaultIndex >= 0 ? defaultIndex : 0;
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
                _log.Error("Can not open the game config file '" + _settings.FilePath + "' in Notepad.\r\n" + ex);
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
            _settings.SetValue(SCREEN_CATEGORY_KEY, WIDTH_SETTING_KEY, resolution.Width);
            _settings.SetValue(SCREEN_CATEGORY_KEY, HEIGHT_SETTING_KEY, resolution.Height);
            _settings.Save();
        }

        private void OnSettingChanged(object sender, EventArgs e)
        {
            if (_loading) return;

            if (sender == _cboQuality)
                _settings.SetValue(SCREEN_CATEGORY_KEY, QUALITY_SETTING_KEY, _cboQuality.SelectedItem);
            else if (sender == _chkFullScreen)
                _settings.SetValue(SCREEN_CATEGORY_KEY, FULLSCREEN_SETTING_KEY, _chkFullScreen.Checked.ToString().ToLowerInvariant());

            _settings.Save();
        }
    }
}
