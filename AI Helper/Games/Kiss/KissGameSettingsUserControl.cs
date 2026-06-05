using AIHelper.Manage;
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
using System.Xml.Linq;

namespace AIHelper.Games.Kiss
{
    public partial class KissGameSettingsUserControl : UserControl
    {
        public KissGameSettingsUserControl()
        {
            InitializeComponent();

            _xmlPath = ManageSettings.SetupXmlPath;
            AutoScaleMode = AutoScaleMode.Font;
            MinimumSize = new System.Drawing.Size(450, 165);
            Size = new System.Drawing.Size(450, 165);
            BuildLayout();
            LoadXml();
            PopulateControls();
        }

        // Screen category keys
        const string SCREEN_CATEGORY_KEY = "Screen";
        const string FULLSCREEN_SETTING_KEY = "FullScreen";
        const string WIDTH_SETTING_KEY = "ScreenSizeNow.width";
        const string HEIGHT_SETTING_KEY = "ScreenSizeNow.height";
        const string QUALITY_SETTING_KEY = "TextureQuality";
        const int DEFAULT_SCREEN_WIDTH = 1280;
        const int DEFAULT_SCREEN_HEIGHT = 720;
        const string DEFAULT_QUALITY = "High";
        const bool DEFAULT_FULLSCREEN = false;

        private readonly string _xmlPath;
        private XDocument _doc;
        private bool _loading;

        // Predefined resolution options
        private static readonly List<(string Label, int W, int H)> Resolutions = new List<(string Label, int W, int H)>
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

        private static readonly string[] QualityItems = { "Low", "Medium", "High" };

        // ── Controls ──────────────────────────────────────────────────
        private TableLayoutPanel _table;
        private ComboBox _cboResolution;
        private ComboBox _cboQuality;
        private CheckBox _chkFullScreen;
        private LinkLabel _lblOpenInNotepad;
        private LinkLabel _lblReloadFromXml;
        private Label _lblResolution, _lblQuality, _lblFullScreen;

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
            _lblReloadFromXml = MakeReloadLinkLabel();

            // Row 1 – Resolution
            _lblResolution = MakeLabel(T._("Resolution:"));
            _cboResolution = MakeCombo(Resolutions.Select(r => r.Label).ToArray());
            _cboResolution.SelectedIndexChanged += OnResolutionChanged;

            // Row 2 – Quality
            _lblQuality = MakeLabel(T._("Quality:"));
            _cboQuality = MakeCombo(QualityItems);
            _cboQuality.SelectedIndexChanged += OnSettingChanged;

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
            _table.Controls.Add(_lblReloadFromXml, 1, 0);
            int row = 1;
            foreach (var (lbl, ctrl) in new (Control, Control)[]
            {
                (_lblResolution, _cboResolution),
                (_lblQuality,    _cboQuality),
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

        private LinkLabel MakeReloadLinkLabel()
        {
            var ll = new LinkLabel
            {
                Text = T._("Reload settings from XML"),
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Margin = new Padding(2),
            };
            ll.Click += (s, e) =>
            {
                LoadXml();
                PopulateControls();
            };
            return ll;
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
                    new XElement("Screen",
                        new XElement(FULLSCREEN_SETTING_KEY, DEFAULT_FULLSCREEN.ToString().ToLower()),
                        new XElement(WIDTH_SETTING_KEY, DEFAULT_SCREEN_WIDTH),
                        new XElement(HEIGHT_SETTING_KEY, DEFAULT_SCREEN_HEIGHT),
                        new XElement(QUALITY_SETTING_KEY, DEFAULT_QUALITY)
                    )
                )
            );
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

        private string GetVal(string key, string categoryName = "") 
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

            var elem = _doc.Root?.Element(string.IsNullOrEmpty(categoryName) ? SCREEN_CATEGORY_KEY : categoryName)?.Element(key);
            return elem != null ? elem.Value : string.Empty;
        }
        private void SetVal(string key, object val, string categoryName = null) => _doc.Root?.Element(string.IsNullOrEmpty(categoryName) ? SCREEN_CATEGORY_KEY : categoryName)?.SetElementValue(key, val);


        // ── Populate UI from XML ──────────────────────────────────────
        private void PopulateControls()
        {
            _loading = true;
            try
            {
                SetResolution();

                // Quality
                var s = GetVal(QUALITY_SETTING_KEY, SCREEN_CATEGORY_KEY);
                _cboQuality.SelectedItem = s;

                // FullScreen
                _chkFullScreen.Checked = GetVal(FULLSCREEN_SETTING_KEY, SCREEN_CATEGORY_KEY).Equals("true", StringComparison.OrdinalIgnoreCase);
            }
            finally { _loading = false; }
        }

        private void SetResolution()
        {
            // Resolution – first load by Width and Height, if not found then fallback to label matching
            string wStr = GetVal(WIDTH_SETTING_KEY, SCREEN_CATEGORY_KEY);
            int w = int.TryParse(wStr, out int tw) ? tw : -1;
            string hStr = GetVal(HEIGHT_SETTING_KEY, SCREEN_CATEGORY_KEY);
            int h = int.TryParse(hStr, out int th) ? th : -1;
            if (w > 0 && h > 0)
            {
                int ri = Resolutions.FindIndex(r => r.W == w && r.H == h);
                if(ri < 0)
                {
                    string label = $"{w} x {h}";
                    Resolutions.Add((label, w, h));
                    _cboResolution.Items.Add(label);
                    ri = Resolutions.Count - 1;
                }
                _cboResolution.SelectedIndex = ri >= 0 ? ri : 0;
            }
            else
            {
                SetVal(WIDTH_SETTING_KEY, DEFAULT_SCREEN_WIDTH, SCREEN_CATEGORY_KEY);
                SetVal(HEIGHT_SETTING_KEY, DEFAULT_SCREEN_HEIGHT, SCREEN_CATEGORY_KEY);
                SaveXml();

                int ri = Resolutions.FindIndex(r => r.W == DEFAULT_SCREEN_WIDTH && r.H == DEFAULT_SCREEN_HEIGHT);
                _cboResolution.SelectedIndex = ri >= 0 ? ri : 0;

            }
        }

        // ── Event Handlers ────────────────────────────────────────────
        private void OnResolutionChanged(object sender, EventArgs e)
        {
            if (_loading || _cboResolution.SelectedIndex < 0) return;
            var (_, w, h) = Resolutions[_cboResolution.SelectedIndex];
            SetVal(WIDTH_SETTING_KEY, w, SCREEN_CATEGORY_KEY);
            SetVal(HEIGHT_SETTING_KEY, h, SCREEN_CATEGORY_KEY);
            SaveXml();
        }

        private void OnSettingChanged(object sender, EventArgs e)
        {
            if (_loading) return;

            if (sender == _cboQuality)
                SetVal(QUALITY_SETTING_KEY, _cboQuality.SelectedItem, SCREEN_CATEGORY_KEY);
            else if (sender == _chkFullScreen)
                SetVal(FULLSCREEN_SETTING_KEY, _chkFullScreen.Checked.ToString().ToLower(), SCREEN_CATEGORY_KEY);

            SaveXml();
        }
    }
}
