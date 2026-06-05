using AIHelper.Manage;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace AIHelper.Games.Illusion
{
    public partial class IllusionGameSettingsUserControl : UserControl
    {
        const string SIZE_SETTING_KEY = "Size";
        const string WIDTH_SETTING_KEY = "Width";
        const string HEIGHT_SETTING_KEY = "Height";
        const string QUALITY_SETTING_KEY = "Quality";
        const string FULLSCREEN_SETTING_KEY = "FullScreen";
        const string DISPLAY_SETTING_KEY = "Display";
        const string LANGUAGE_SETTING_KEY = "Language";
        const string DEFAULT_SCREEN_SIZE_LABEL = "1280 x 720 (16 : 9)";
        const string DEFAULT_SCREEN_WIDTH = "1280";
        const string DEFAULT_SCREEN_HEIGHT = "720";
        const int DEFAULT_QUALITY = 2;
        const bool DEFAULT_FULLSCREEN = false;
        const int DEFAULT_DISPLAY = 0;
        const int DEFAULT_LANGUAGE = 0;

        public IllusionGameSettingsUserControl()
        {
            _xmlPath = ManageSettings.CurrentGameSetupXmlFilePathinData;
            AutoScaleMode = AutoScaleMode.Font;
            MinimumSize = new System.Drawing.Size(450, 165);
            Size = new System.Drawing.Size(450, 165);
            BuildLayout();
            LoadXml();
            PopulateControls();
        }

        private readonly string _xmlPath;
        private XDocument _doc;
        private bool _loading;

        // Predefined resolution options
        private static readonly (string Label, int W, int H)[] Resolutions =
        {
            ("854 x 480 (16 : 9)",   854,  480),
            ("800 x 600 (4 : 3)",    800,  600),
            ("1024 x 579 (16 : 9)", 1024,  576),
            ("1024 x 768 (4 : 3)",  1024,  768),
            ("1280 x 720 (16 : 9)", 1280,  720),
            ("1366 x 768 (16 : 9)", 1366,  768),
            ("1536 x 864 (16 : 9)", 1536,  864),
            ("1600 x 900 (16 : 9)", 1600,  900),
            ("1920 x 1080 (16 : 9)",1920, 1080),
            ("2560 x 1440 (16 : 9)",2560, 1440),
            ("3840 x 2160 (16 : 9)",3840, 2160),
        };

        private static readonly string[] QualityItems = { T._("Performance"), T._("Normal"), T._("Quality") };
        private static readonly string[] LanguageItems = { T._("Japanese"), T._("English"), T._("German"), T._("French") };

        // ── Controls ──────────────────────────────────────────────────
        private TableLayoutPanel _table;
        private ComboBox _cboResolution;
        private ComboBox _cboQuality;
        private ComboBox _cboLanguage;
        private ComboBox _cboDisplay;
        private CheckBox _chkFullScreen;
        private LinkLabel _lblOpenInNotepad;
        private Label _lblResolution, _lblQuality, _lblLanguage, _lblDisplay, _lblFullScreen;

        // ── UI Construction ───────────────────────────────────────────
        private void BuildLayout()
        {
            SuspendLayout();

            int maxRows = 6;
            _table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = maxRows,
                Padding = new Padding(8),
            };

            // Column styles: label 38% | control 62%
            _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38f));
            _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62f));

            for (int i = 0; i < maxRows; i++)
                _table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / maxRows));

            // Add controls row by row
            // Row 0 – Open setup xml in notepad
            _lblOpenInNotepad = MakeOpenInNotepadLinkLabel();

            // Row 1 – Resolution
            _lblResolution = MakeLabel(T._("Resolution:"));
            _cboResolution = MakeCombo(Resolutions.Select(r => r.Label).ToArray());
            _cboResolution.SelectedIndexChanged += OnResolutionChanged;

            // Row 2 – Quality
            _lblQuality = MakeLabel(T._("Quality:"));
            _cboQuality = MakeCombo(QualityItems);
            _cboQuality.SelectedIndexChanged += OnSettingChanged;

            // Row 3 – Language
            _lblLanguage = MakeLabel(T._("Language:"));
            _cboLanguage = MakeCombo(LanguageItems);
            _cboLanguage.SelectedIndexChanged += OnSettingChanged;

            // Row 4 – Display
            _lblDisplay = MakeLabel(T._("Display:"));
            _cboDisplay = MakeCombo(Enumerable.Range(0, 4).Select(i => String.Format(T._("Display {0}"), i)).ToArray());
            _cboDisplay.SelectedIndexChanged += OnSettingChanged;

            // Row 5 – FullScreen
            _lblFullScreen = MakeLabel(T._("Full Screen:"));
            _chkFullScreen = new CheckBox
            {
                Dock = DockStyle.Fill,
                Text = string.Empty,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Margin = new Padding(2),
            };
            _chkFullScreen.CheckedChanged += OnSettingChanged;

            _table.Controls.Add(_lblOpenInNotepad, 0, 0);
            int row = 1;
            foreach (var (lbl, ctrl) in new (Control, Control)[]
            {
                (_lblResolution, _cboResolution),
                (_lblQuality,    _cboQuality),
                (_lblLanguage,   _cboLanguage),
                (_lblDisplay,    _cboDisplay),
                (_lblFullScreen, _chkFullScreen),
            })
            {
                _table.Controls.Add(lbl, 0, row);
                _table.Controls.Add(ctrl, 1, row);
                row++;
            }

            Controls.Add(_table);
            ResumeLayout(true);
        }

        private LinkLabel MakeOpenInNotepadLinkLabel()
        {
            var ll = new LinkLabel
            {
                Text = T._("Open game setup file"),
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Margin = new Padding(2),
            };
            ll.Click += (s, e) =>
            {
                try
                {
                    System.Diagnostics.Process.Start("notepad.exe", _xmlPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(T._("Failed to open Notepad: {0}"), ex.Message), T._("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            return ll;
        }

        private static Label MakeLabel(string text) => new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = System.Drawing.ContentAlignment.MiddleRight,
            Margin = new Padding(2),
        };

        private static ComboBox MakeCombo(string[] items)
        {
            var c = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(2),
            };
            c.Items.AddRange(items);
            return c;
        }

        // ── XML I/O ───────────────────────────────────────────────────
        private void LoadXml()
        {
            if (File.Exists(_xmlPath) && new FileInfo(_xmlPath).Length > 0)
            {
                _doc = XDocument.Load(_xmlPath);
            }
            else
            {
                // Create default document
                _doc = GetDefaultSetupXml();
                SaveXml();
            }
        }

        private static XDocument GetDefaultSetupXml()
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

        private void SaveXml()
        {
            var dir = Path.GetDirectoryName(_xmlPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // Write UTF-16
            using (var sw = new StreamWriter(_xmlPath, false, System.Text.Encoding.Unicode))
            {
                _doc.Save(sw);
            }
        }

        private string GetVal(string key) => _doc.Root?.Element(key)?.Value ?? string.Empty;
        private void SetVal(string key, object val) => _doc.Root?.SetElementValue(key, val);


        // ── Populate UI from XML ──────────────────────────────────────
        private void PopulateControls()
        {
            _loading = true;
            try
            {
                // Resolution – match by label
                string sizeLabel = GetVal(SIZE_SETTING_KEY);
                int ri = Array.FindIndex(Resolutions, r => r.Label == sizeLabel);
                _cboResolution.SelectedIndex = ri >= 0 ? ri : 0;

                // Quality
                if (int.TryParse(GetVal(QUALITY_SETTING_KEY), out int q))
                    _cboQuality.SelectedIndex = ManageMath.Clamp(q, 0, QualityItems.Length - 1);

                // Language
                if (int.TryParse(GetVal(LANGUAGE_SETTING_KEY), out int lang))
                    _cboLanguage.SelectedIndex = ManageMath.Clamp(lang, 0, LanguageItems.Length - 1);

                // Display
                if (int.TryParse(GetVal(DISPLAY_SETTING_KEY), out int disp))
                    _cboDisplay.SelectedIndex = ManageMath.Clamp(disp, 0, _cboDisplay.Items.Count - 1);
                // FullScreen
                _chkFullScreen.Checked = GetVal(FULLSCREEN_SETTING_KEY).Equals("true", StringComparison.OrdinalIgnoreCase);
            }
            finally { _loading = false; }
        }

        // ── Event Handlers ────────────────────────────────────────────
        private void OnResolutionChanged(object sender, EventArgs e)
        {
            if (_loading || _cboResolution.SelectedIndex < 0) return;

            var (label, w, h) = Resolutions[_cboResolution.SelectedIndex];
            SetVal(SIZE_SETTING_KEY, label);
            SetVal(WIDTH_SETTING_KEY, w);
            SetVal(HEIGHT_SETTING_KEY, h);
            SaveXml();
        }

        private void OnSettingChanged(object sender, EventArgs e)
        {
            if (_loading) return;

            if (sender == _cboQuality)
                SetVal(QUALITY_SETTING_KEY, _cboQuality.SelectedIndex);
            else if (sender == _cboLanguage)
                SetVal(LANGUAGE_SETTING_KEY, _cboLanguage.SelectedIndex);
            else if (sender == _cboDisplay)
                SetVal(DISPLAY_SETTING_KEY, _cboDisplay.SelectedIndex);
            else if (sender == _chkFullScreen)
                SetVal(FULLSCREEN_SETTING_KEY, _chkFullScreen.Checked.ToString().ToLower());

            SaveXml();
        }
    }
}
