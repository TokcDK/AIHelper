using AIHelper.Manage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace AIHelper.Games.Illusion
{
    public partial class IllusionGameSettingsUserControl : UserControl
    {
        public IllusionGameSettingsUserControl()
        {
            _xmlPath  = ManageSettings.CurrentGameSetupXmlFilePathinData;
            CreateSetupXmlWhenMissing(_xmlPath);

            AutoScaleMode = AutoScaleMode.Font;
            MinimumSize = new System.Drawing.Size(450, 165);
            Size = new System.Drawing.Size(450, 165);
            BuildLayout();
            LoadXml();
            PopulateControls();
        }

        private static void CreateSetupXmlWhenMissing(string setupXmlPath)
        {
            // create the next default xml file if it is missing or empty
            //<?xml version="1.0" encoding="utf-16"?>
            //<Setting>
            //  <Size>1366 x 768 (16 : 9)</Size>
            //  <Width>1366</Width>
            //  <Height>768</Height>
            //  <Quality>2</Quality>
            //  <FullScreen>false</FullScreen>
            //  <Display>0</Display>
            //  <Language>0</Language>
            //</Setting>
            if (!File.Exists(setupXmlPath) || new FileInfo(setupXmlPath).Length == 0)
            {
                var doc = new XDocument(
                    new XDeclaration("1.0", "utf-16", null),
                    new XElement("Setting",
                        new XElement("Size", "1366 x 768 (16 : 9)"),
                        new XElement("Width", 1366),
                        new XElement("Height", 768),
                        new XElement("Quality", 2),
                        new XElement("FullScreen", false),
                        new XElement("Display", 0),
                        new XElement("Language", 0)
                    ));
                // Write UTF-16
                using (var sw = new StreamWriter(setupXmlPath, false, System.Text.Encoding.Unicode))
                {
                    doc.Save(sw);
                }
            }
        }

        private readonly string _xmlPath;
        private XDocument _doc;
        private bool _loading;

        // Predefined resolution options
        private static readonly (string Label, int W, int H)[] Resolutions =
        {
            ("800 x 600 (4 : 3)",    800,  600),
            ("1024 x 768 (4 : 3)",  1024,  768),
            ("1280 x 720 (16 : 9)", 1280,  720),
            ("1366 x 768 (16 : 9)", 1366,  768),
            ("1600 x 900 (16 : 9)", 1600,  900),
            ("1920 x 1080 (16 : 9)",1920, 1080),
            ("2560 x 1440 (16 : 9)",2560, 1440),
        };

        private static readonly string[] QualityItems = { "Low", "Medium", "High" };
        private static readonly string[] LanguageItems = { "English", "German", "French", "Spanish" };

        // ── Controls ──────────────────────────────────────────────────
        private TableLayoutPanel _table;
        private ComboBox _cboResolution;
        private ComboBox _cboQuality;
        private ComboBox _cboLanguage;
        private ComboBox _cboDisplay;
        private CheckBox _chkFullScreen;
        private Label _lblResolution, _lblQuality, _lblLanguage, _lblDisplay, _lblFullScreen;

        // ── UI Construction ───────────────────────────────────────────
        private void BuildLayout()
        {
            SuspendLayout();

            _table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(8),
            };

            // Column styles: label 38% | control 62%
            _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38f));
            _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62f));

            for (int i = 0; i < 5; i++)
                _table.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));

            // Row 0 – Resolution
            _lblResolution = MakeLabel("Resolution:");
            _cboResolution = MakeCombo(Resolutions.Select(r => r.Label).ToArray());
            _cboResolution.SelectedIndexChanged += OnResolutionChanged;

            // Row 1 – Quality
            _lblQuality = MakeLabel("Quality:");
            _cboQuality = MakeCombo(QualityItems);
            _cboQuality.SelectedIndexChanged += OnSettingChanged;

            // Row 2 – Language
            _lblLanguage = MakeLabel("Language:");
            _cboLanguage = MakeCombo(LanguageItems);
            _cboLanguage.SelectedIndexChanged += OnSettingChanged;

            // Row 3 – Display
            _lblDisplay = MakeLabel("Display:");
            _cboDisplay = MakeCombo(Enumerable.Range(0, 4).Select(i => $"Display {i}").ToArray());
            _cboDisplay.SelectedIndexChanged += OnSettingChanged;

            // Row 4 – FullScreen
            _lblFullScreen = MakeLabel("Full Screen:");
            _chkFullScreen = new CheckBox
            {
                Dock = DockStyle.Fill,
                Text = string.Empty,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Margin = new Padding(2),
            };
            _chkFullScreen.CheckedChanged += OnSettingChanged;

            int row = 0;
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
            if (File.Exists(_xmlPath))
            {
                _doc = XDocument.Load(_xmlPath);
            }
            else
            {
                // Create default document
                _doc = new XDocument(
                    new XDeclaration("1.0", "utf-16", null),
                    new XElement("Setting",
                        new XElement("Size", "1366 x 768 (16 : 9)"),
                        new XElement("Width", 1366),
                        new XElement("Height", 768),
                        new XElement("Quality", 2),
                        new XElement("FullScreen", false),
                        new XElement("Display", 0),
                        new XElement("Language", 0)
                    ));
                SaveXml();
            }
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
                string sizeLabel = GetVal("Size");
                int ri = Array.FindIndex(Resolutions, r => r.Label == sizeLabel);
                _cboResolution.SelectedIndex = ri >= 0 ? ri : 0;

                // Quality
                if (int.TryParse(GetVal("Quality"), out int q))
                    _cboQuality.SelectedIndex = IllusionGameSattingsHelper.Clamp(q, 0, QualityItems.Length - 1);

                // Language
                if (int.TryParse(GetVal("Language"), out int lang))
                    _cboLanguage.SelectedIndex = IllusionGameSattingsHelper.Clamp(lang, 0, LanguageItems.Length - 1);

                // Display
                if (int.TryParse(GetVal("Display"), out int disp))
                    _cboDisplay.SelectedIndex = IllusionGameSattingsHelper.Clamp(disp, 0, _cboDisplay.Items.Count - 1);

                // FullScreen
                _chkFullScreen.Checked = GetVal("FullScreen").Equals("true", StringComparison.OrdinalIgnoreCase);
            }
            finally { _loading = false; }
        }

        // ── Event Handlers ────────────────────────────────────────────
        private void OnResolutionChanged(object sender, EventArgs e)
        {
            if (_loading || _cboResolution.SelectedIndex < 0) return;

            var (label, w, h) = Resolutions[_cboResolution.SelectedIndex];
            SetVal("Size", label);
            SetVal("Width", w);
            SetVal("Height", h);
            SaveXml();
        }

        private void OnSettingChanged(object sender, EventArgs e)
        {
            if (_loading) return;

            if (sender == _cboQuality)
                SetVal("Quality", _cboQuality.SelectedIndex);
            else if (sender == _cboLanguage)
                SetVal("Language", _cboLanguage.SelectedIndex);
            else if (sender == _cboDisplay)
                SetVal("Display", _cboDisplay.SelectedIndex);
            else if (sender == _chkFullScreen)
                SetVal("FullScreen", _chkFullScreen.Checked.ToString().ToLower());

            SaveXml();
        }
    }
}
