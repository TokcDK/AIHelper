using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AIHelper.Data.Modlist;
using AIHelper.Manage.ui.themes;
using AIHelper.Manage.Update;
using AIHelper.Manage.Update.Sources;
using INIFileMan;
using NLog;

namespace AIHelper.Manage.Functions
{
    internal class InfoEditorForFlp : FunctionForFlpBase, IDisposable
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public override string Symbol => T._("⚒");

        public override string Description => T._("Open mods update info editor");

        public override void OnClick(object o, EventArgs e)
        {
            OpenInfoEditor();
        }

        public override Color? ForeColor => Color.LightSkyBlue;

        public static RichTextBox Logtb { get; set; }

        readonly int _elHeight = 13;

        readonly List<UpdateInfoData> _updateInfoDatas = new List<UpdateInfoData>();

        // Cache of already built mod info blocks. They are built once and reused,
        // For the new added mods from 'add mod' form the new instances are appended only when a mod was successfully downloaded and added without reread all infos again.
        readonly List<ModInfoBuild> _modInfoBuilds = new List<ModInfoBuild>();

        bool _isReading;
        bool _isWriting;

        // Controls of the currently opened editor (used by filter and by "add mod" flow).
        // They are children of the editor form and are released when a session ends.
        FlowLayoutPanel _modsListFlowPanel;
        TextBox _filterTextBox;

        // The tooltip is a component rather than a control, so the editor form does not
        // dispose it and this object has to.
        ToolTip _toolTip;

        /// <summary>
        /// Releases what an editor session created that the editor form cannot release itself.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// <para>
        /// Releases what an editor session created and lets go of its controls.
        /// </para>
        /// <para>
        /// The tooltip is the one thing the editor form cannot release itself: a
        /// <see cref="ToolTip"/> is a component and not a child control, so it stays alive
        /// after the form closes. The panel and the filter box are children of that form
        /// and are disposed with it; disposing them again here is harmless, because
        /// <see cref="Control.Dispose()"/> is idempotent, and it is what makes the
        /// ownership this object has over them explicit. The cached mod info blocks stay
        /// on <see cref="_modInfoBuilds"/> until the next session clears them.
        /// </para>
        /// </summary>
        /// <param name="disposing">
        /// false when called from a finalizer, where only unmanaged resources may be touched.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            _toolTip?.Dispose();
            _toolTip = null;

            _filterTextBox?.Dispose();
            _filterTextBox = null;

            _modsListFlowPanel?.Dispose();
            _modsListFlowPanel = null;
        }

        private static void InitLogTextBox()
        {
            if (Logtb == null || !Logtb.IsAccessible || Logtb.IsDisposed)
            {
                Logtb = new RichTextBox() { ReadOnly = true, Width = 200 };
            }
        }

        static void Log(string v) { Logtb.Text += v + "\r\n"; }

        private static ToolTip CreateToolTip()
        {
            return new ToolTip
            {
                AutoPopDelay = 32000,
                InitialDelay = 1000,
                ReshowDelay = 500,
                UseAnimation = true,
                UseFading = true,
                ShowAlways = true
            };
        }

        internal void OpenInfoEditor()
        {
            // A fresh editor session: reset caches so mod infos are read once per form.
            _modInfoBuilds.Clear();
            _updateInfoDatas.Clear();

            var modUpdateInfosForm = new Form
            {
                Text = T._("Mods update info settings") + " (" + ManageSettings.CurrentGameDisplayingName + ")",
                StartPosition = FormStartPosition.CenterScreen
            };

            var modUpdateInfosTableLayoutPanel = new TableLayoutPanel
            {
                RowCount = 2,
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };
            modUpdateInfosTableLayoutPanel.RowStyles.Add(new RowStyle
            {
                SizeType = SizeType.Absolute,
                Height = 30,
            });
            modUpdateInfosTableLayoutPanel.RowStyles.Add(new RowStyle
            {
                SizeType = SizeType.Percent,
                Height = 100,
            });

            // Header: filter field which dynamically filters all displayed mod infos.
            var filterFlowLayoutPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
            };
            var filterLabel = new Label
            {
                Text = T._("Filter") + ":",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
            };
            _filterTextBox = new TextBox
            {
                Width = 320,
                Anchor = AnchorStyles.Left,
            };
            _filterTextBox.TextChanged += (o, e) => ApplyFilter();
            filterFlowLayoutPanel.Controls.Add(filterLabel);
            filterFlowLayoutPanel.Controls.Add(_filterTextBox);
            modUpdateInfosTableLayoutPanel.SetRow(filterFlowLayoutPanel, 0);

            // Content: tab control with tab pages instead of the two buttons.
            var modUpdateInfosTabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
            };

            var allModsTabPage = new TabPage { Text = T._("All"), UseVisualStyleBackColor = true };
            var addModTabPage = new TabPage { Text = T._("Add"), UseVisualStyleBackColor = true };

            _modsListFlowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Margin = new Padding(0),
            };
            allModsTabPage.Controls.Add(_modsListFlowPanel);

            modUpdateInfosTabControl.TabPages.Add(allModsTabPage);
            modUpdateInfosTabControl.TabPages.Add(addModTabPage);

            // Build "Add mod" tab page lazily on its first selection.
            modUpdateInfosTabControl.SelectedIndexChanged += (o, e) =>
            {
                if (modUpdateInfosTabControl.SelectedTab == addModTabPage && addModTabPage.Controls.Count == 0)
                {
                    LoadAddModPanel(addModTabPage);
                }
            };

            modUpdateInfosTableLayoutPanel.SetRow(modUpdateInfosTabControl, 1);
            modUpdateInfosTableLayoutPanel.Controls.Add(filterFlowLayoutPanel);
            modUpdateInfosTableLayoutPanel.Controls.Add(modUpdateInfosTabControl);
            modUpdateInfosForm.Controls.Add(modUpdateInfosTableLayoutPanel);

            // Build and cache all mod infos once.
            LoadGitInfos(modUpdateInfosForm, _modsListFlowPanel);

            modUpdateInfosForm.Size = new System.Drawing.Size(modUpdateInfosForm.Width * 2, modUpdateInfosForm.Height * 2);

            // The editor is shown modeless, so its session ends with the form rather than
            // with this method. The tooltip is not a child of that form and has to be
            // released explicitly.
            modUpdateInfosForm.FormClosed += (o, e) => Dispose();
            modUpdateInfosForm.Show(ManageSettings.MainForm);

            _isReading = false;
        }

        private void ApplyFilter()
        {
            if (_modsListFlowPanel == null) return;

            var filterText = _filterTextBox == null ? "" : _filterTextBox.Text.Trim();

            _modsListFlowPanel.SuspendLayout();
            foreach (var modInfoBuild in _modInfoBuilds)
            {
                modInfoBuild.Control.Visible =
                    filterText.Length == 0
                    || modInfoBuild.Filter.IndexOf(filterText, System.StringComparison.InvariantCultureIgnoreCase) >= 0;
            }
            _modsListFlowPanel.ResumeLayout();
        }

        class NewModPropData
        {
            public NewModPropData(string labelText, string textBoxText)
            {
                LabelText = labelText;
                TextBoxText = textBoxText;
            }

            public string LabelText { get; }
            public string TextBoxText { get; set; }

            public TextBox TB;
        }

        private void LoadAddModPanel(Control addModContainer)
        {
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
            int containerWidth = addModContainer.Width > 200 ? addModContainer.Width : 200;

            var defaultModDirName = "NewMod";
            var modnamePropData = new NewModPropData(T._("Mod dir name"), defaultModDirName);
            var urlPropData = new NewModPropData("Url", "https://github.com/Owner/Name");
            var startsWithPropData = new NewModPropData(T._("File starts with"), "");
            var endsWithPropData = new NewModPropData(T._("File ends with"), "");
            var newModDatas = new NewModPropData[]
            {
            modnamePropData,
            urlPropData,
            startsWithPropData,
            endsWithPropData,
            };
            foreach (var newModData in newModDatas)
            {
                var propertyFlp = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    Margin = new Padding(0),
                };
                var l = new Label
                {
                    Text = newModData.LabelText + ":",
                    Size = new System.Drawing.Size(150, _elHeight),
                };
                var urlTextBox = new TextBox
                {
                    Size = new System.Drawing.Size(containerWidth - l.Width - 20, _elHeight),
                    Margin = new Padding(1, 1, 10, 1),
                };
                urlTextBox.DataBindings.Add(new Binding(nameof(urlTextBox.Text), newModData, nameof(newModData.TextBoxText), true, DataSourceUpdateMode.OnPropertyChanged));
                urlTextBox.TextChanged += new System.EventHandler((o, e) =>
                {
                    if (addModPanelIsLoading) return;

                    if (newModData.LabelText == urlPropData.LabelText)
                    {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(urlTextBox.Text)) return;
                            if (!string.IsNullOrWhiteSpace(modnamePropData.TextBoxText)
                            && modnamePropData.TextBoxText != defaultModDirName) return;

                            var urlGithubMatch = Regex.Match(urlTextBox.Text, @"(^.*https?\:\/\/)?github\.com\/([^\/]+)\/([^\/\? ]+).*$", RegexOptions.IgnoreCase);
                            if (!urlGithubMatch.Success) return;
                            if (urlGithubMatch.Groups.Count != 4) return;

                            var ownerName = urlGithubMatch.Groups[2].Value;
                            if (string.IsNullOrWhiteSpace(ownerName)) return;
                            var repositoryName = urlGithubMatch.Groups[3].Value;
                            if (string.IsNullOrWhiteSpace(repositoryName)) return;

                            // set repository name as mod name
                            modnamePropData.TB.Text = repositoryName;

                            Log(T._("Mod name set from url"));
                        }
                        catch (Exception ex)
                        {
                            _log.Warn("Failed to set the mod name from the url. Error:\r\n" + ex);
                            return;
                        }
                    }

                    urlTextBox.DataBindings[0].WriteValue();
                });
                newModData.TB = urlTextBox;

                propertyFlp.Controls.Add(l);
                propertyFlp.Controls.Add(urlTextBox);

                var lWidth = l.Width + (l.Margin.Horizontal * 2);
                var lHeight = l.Height + (l.Margin.Vertical * 2);
                var tbWidth = urlTextBox.Width + (urlTextBox.Margin.Horizontal * 2);
                var tbHeight = urlTextBox.Height + (urlTextBox.Margin.Vertical * 2);
                var ltbWidth = lWidth + tbWidth;
                var ltbHeight = lHeight + tbHeight;
                propertyFlp.Size = new System.Drawing.Size(ltbWidth, ltbHeight);

                modUpdateInfoMainFlp.Controls.Add(propertyFlp);
            }

            var verFromFile = new CheckBox
            {
                Text = T._("Version from file"),
                Checked = false,
                Size = new System.Drawing.Size(containerWidth - 20, _elHeight + 5),
                Anchor = AnchorStyles.Left,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            modUpdateInfoMainFlp.Controls.Add(verFromFile);

            var tryLoadInfo = new Button
            {
                Text = "Web"
            };
            tryLoadInfo.Click += new EventHandler((o, e) =>
            {
                Process.Start(urlPropData.TextBoxText);
            });
            modUpdateInfoMainFlp.Controls.Add(tryLoadInfo);

            var tryOpenDir = new Button
            {
                Text = T._("Dir")
            };
            tryOpenDir.Click += new EventHandler((o, e) =>
            {
                var dirPath = Path.Combine(ManageSettings.CurrentGameModsDirPath, modnamePropData.TextBoxText);
                if (!Directory.Exists(dirPath))
                {
                    Log(T._("Mod dir missing. Mod still is not added!"));
                    return;
                }

                Process.Start(dirPath);
            });
            modUpdateInfoMainFlp.Controls.Add(tryOpenDir);

            var DownloadAndAddMod = new Button
            {
                Text = T._("Add")
            };
            DownloadAndAddMod.Click += new EventHandler((o, e) =>
            {
                if (addModPanelIsLoading) return;

                // load archive and add

                try
                {
                    string targetModDirName = modnamePropData.TextBoxText;
                    string targetUrl = urlPropData.TextBoxText;
                    string startsWith = startsWithPropData.TextBoxText;
                    string endsWith = endsWithPropData.TextBoxText;

                    if (string.IsNullOrWhiteSpace(modnamePropData.TextBoxText))
                    {
                        Log(T._("Mod name is empty"));
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(targetUrl))
                    {
                        Log(T._("Url s empty"));
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(startsWith))
                    {
                        Log($"{startsWithPropData.LabelText} {T._("value is empty")}");
                        return;
                    }

                    var targetUrlGithubMatch = Regex.Match(targetUrl,
                        @"(^.*https?\:\/\/)?github\.com\/([^\/]+)\/([^\/\? ]+).*$",
                        RegexOptions.IgnoreCase);
                    if (!targetUrlGithubMatch.Success)
                    {
                        Log(T._("Url is not recognized"));
                        return;
                    }
                    if (targetUrlGithubMatch.Groups.Count != 4)
                    {
                        Log(T._("Url is not recognized"));
                        return;
                    }

                    var ownerName = targetUrlGithubMatch.Groups[2].Value;
                    var repositoryName = targetUrlGithubMatch.Groups[3].Value;
                    if (string.IsNullOrWhiteSpace(ownerName))
                    {
                        Log(T._("Url is not recognized"));
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(repositoryName))
                    {
                        Log(T._("Url is not recognized"));
                        return;
                    }

                    var targetDirPathInfo = new DirectoryInfo(Path.Combine(ManageSettings.CurrentGameModsDirPath, targetModDirName));
                    if (targetDirPathInfo.Exists)
                    {
                        Log(T._("Target mod dir is exists!"));
                        return;
                    }

                    var newRecord = new GithubUpdateRecord(
                        ownerName,
                        repositoryName,
                        startsWith,
                        endsWithPropData.TextBoxText,
                        verFromFile.Checked);

                    foreach (var modInfo in _updateInfoDatas)
                    {
                        if (!newRecord.PointsAtSameFileAs(ToUpdateRecord(modInfo.GitInfo))) continue;

                        Log(T._($"Mod '{modInfo.ModName}' have same Owner,Repository,start/end"));
                        return;
                    }

                    var modData = new ModData
                    {
                        Path = targetDirPathInfo.FullName,
                        Name = modnamePropData.TextBoxText,
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

                    GetGithubFile(newRecord, ginfo);
                }
                catch (Exception ex)
                {
                    Log(T._("Error: " + ex.Message));
                    return;
                }
            });
            modUpdateInfoMainFlp.Controls.Add(DownloadAndAddMod);

            InitLogTextBox();
            Logtb.Width = containerWidth - 10;
            modUpdateInfoMainFlp.Controls.Add(Logtb);

            addModContainer.Controls.Add(modUpdateInfoMainFlp);

            ThemesLoader.SetTheme(ManageSettings.CurrentTheme, addModContainer);

            addModPanelIsLoading = false;
        }

        /// <summary>
        /// Downloads the release the record points at and adds it as a mod.
        /// <para>
        /// The lookup and the download are the update engine's; what happens here is
        /// the ordering the editor needs — resolve the version first, then put the
        /// files in place, and only then write the record into meta.ini, so that a
        /// failed download never leaves a mod that claims to be updatable.
        /// </para>
        /// </summary>
        private async void GetGithubFile(GithubUpdateRecord record, GitUpdateInfoData ginfo)
        {
            //NOTE: this stays async void because the enclosing LoadAddModPanel is synchronous, so the caller
            //cannot await it. An exception thrown after the first await of an async void method cannot be caught
            //by the caller and would crash the process, so the whole body must be guarded here.
            try
            {
                using (var engine = new UpdateEngineFactory())
                {
                    var probe = new GithubUpdateProbe(engine.GithubSource);

                    var latest = await probe
                        .FindLatestAsync(record, CancellationToken.None)
                        .ConfigureAwait(true);

                    if (latest == null || string.IsNullOrWhiteSpace(latest.VersionString))
                    {
                        Log(T._("Cant get last version"));
                        return;
                    }

                    var targetFolderPath = ginfo.Mod.Path;

                    if (!await probe
                            .DownloadAndInstallAsync(latest, targetFolderPath, CancellationToken.None)
                            .ConfigureAwait(true))
                    {
                        Log(T._("Cant download file!"));
                        return;
                    }

                    ginfo.Mod.MetaIni = new INIFile(Path.Combine(targetFolderPath, "meta.ini"), true);

                    ginfo.Write();

                    Log(T._("Mod succesfully downloaded and added!"));

                    // The mod was successfully downloaded and added: build one new
                    // ModInfoBuild with the entered values and append it to the cache.
                    AddNewModInfoBuild(ginfo);
                }
            }
            catch (Exception ex)
            {
                Log(T._("Error: ") + ex.Message);
                _log.Error("Failed to download and add the mod. Error:\r\n" + ex);
            }
        }

        /// <summary>
        /// Reads a record out of the fields the editor holds, so that two entries
        /// feeding off the same release can be recognised through the comparison the
        /// record itself offers instead of by comparing four fields by hand.
        /// </summary>
        private static GithubUpdateRecord ToUpdateRecord(GitUpdateInfoData gitInfo)
        {
            return new GithubUpdateRecord(
                gitInfo.Owner,
                gitInfo.Repository,
                gitInfo.FileStartsWith,
                gitInfo.FileEndsWith,
                gitInfo.VersionFromFile);
        }

        private void AddNewModInfoBuild(GitUpdateInfoData ginfo)
        {
            if (_modsListFlowPanel == null) return;

            if (_toolTip == null) _toolTip = CreateToolTip();

            var infoData = new UpdateInfoData(ginfo.Mod)
            {
                GitInfo = ginfo,
                ToolTip = _toolTip,
            };

            _isReading = true;
            var modInfoBuild = new ModInfoBuild(infoData, _toolTip, _elHeight, WriteValue);
            _isReading = false;

            _modInfoBuilds.Add(modInfoBuild);
            _updateInfoDatas.Add(infoData);
            _modsListFlowPanel.Controls.Add(modInfoBuild.Control);

            ThemesLoader.SetTheme(ManageSettings.CurrentTheme, modInfoBuild.Control);

            ApplyFilter();
        }

        private void LoadGitInfos(Form f, FlowLayoutPanel modsListFlowPanel)
        {
            // Already read ModInfoBuild instances are cached: do not rebuild them.
            if (_modInfoBuilds.Count > 0)
            {
                ApplyFilter();
                return;
            }

            _toolTip = CreateToolTip();

            _isReading = true;

            var mods = new ModlistData();
            var sectionName = ManageSettings.AiMetaIniSectionName;
            var keyName = ManageSettings.AiMetaIniKeyUpdateName;
            foreach (var mod in mods
                .Mods
                .Where(m =>
                !(m.Type is ModType.Separator)
                && !(m.Type is ModType.Overwrite)
                ))
            {
                var ini = mod.MetaIni;
                if (ini == null) continue; // skip when no ini

                var hasUrl = ini.KeyExists("url", "General");
                var hasUpdateInfo = ini.KeyExists(keyName, sectionName);
                if (!hasUrl && !hasUpdateInfo) continue;

                var url = ini.GetKey("General", "url");
                var updateInfo = ini.GetKey(sectionName, keyName);

                var urlInfoIsEmpty = string.IsNullOrWhiteSpace(url);
                var updateInfoIsEmpty = string.IsNullOrWhiteSpace(updateInfo);
                if (urlInfoIsEmpty && updateInfoIsEmpty) continue;

                var urlMatch = Regex.Match(
                    url,
                    $@"(https?:\/\/){UpdateRecordParser.SourceHost}\/([^\/]+)\/([^\/]+)",
                    RegexOptions.IgnoreCase);

                var infoData = new UpdateInfoData(mod)
                {
                    ToolTip = _toolTip
                };

                // A stored record is only read when it really is one. The value may
                // equally well be a plain note, and taking "first,second" out of a
                // sentence would invent an owner and a repository.
                GithubUpdateRecord storedRecord = null;
                var hasStoredRecord = !updateInfoIsEmpty
                                      && updateInfo.TrimStart().StartsWith(
                                          UpdateRecordParser.SourceId, StringComparison.OrdinalIgnoreCase)
                                      && UpdateRecordParser.TryParseStoredValue(updateInfo, out storedRecord);

                if (hasStoredRecord)
                {
                    infoData.GitInfo.Owner = storedRecord.Owner;
                    infoData.GitInfo.Repository = storedRecord.Repository;
                    infoData.GitInfo.FileStartsWith = storedRecord.FileStartsWith;
                    infoData.GitInfo.FileEndsWith = storedRecord.FileEndsWith;
                    infoData.GitInfo.VersionFromFile = storedRecord.VersionFromFile;
                }
                else if (urlMatch.Success)
                {
                    infoData.GitInfo.Owner = urlMatch.Groups[2].Value;
                    infoData.GitInfo.Repository = urlMatch.Groups[3].Value.Split('/')[0];
                }
                else
                {
                    continue;
                }

                var modInfoBuild = new ModInfoBuild(infoData, _toolTip, _elHeight, WriteValue);

                _modInfoBuilds.Add(modInfoBuild);
                _updateInfoDatas.Add(infoData);
                modsListFlowPanel.Controls.Add(modInfoBuild.Control);

                var curModFlpWidth = modInfoBuild.Control.Width + (modInfoBuild.Control.Margin.Horizontal * 2);
                var curModFlpHeight = modInfoBuild.Control.Height + (modInfoBuild.Control.Margin.Vertical * 2);
                f.Size = new System.Drawing.Size
                    (
                    curModFlpWidth > f.Width ? curModFlpWidth : f.Width
                    , curModFlpHeight > f.Height ? curModFlpHeight : f.Height
                    );
            }

            ThemesLoader.SetTheme(ManageSettings.CurrentTheme, f);
        }

        private void WriteValue(Control cb, UpdateInfoData infoData)
        {
            if (_isReading) return;
            if (_isWriting) return;

            _isWriting = true;

            Task.Delay(1000).ContinueWith(t =>
            {
                cb.DataBindings[0].WriteValue(); // force write property valuue before try write ini
                infoData.GitInfo.Write();

                _isWriting = false;
            });
        }

        /// <summary>
        /// Builds the UI block of a single mod github info and keeps it together
        /// with the text used to filter it.
        /// </summary>
        private class ModInfoBuild
        {
            private readonly UpdateInfoData _infoData;
            private readonly ToolTip _toolTip;
            private readonly int _elHeight;
            private readonly Action<Control, UpdateInfoData> _writeValue;

            public ModInfoBuild(UpdateInfoData infoData, ToolTip toolTip, int elHeight, Action<Control, UpdateInfoData> writeValue)
            {
                _infoData = infoData;
                _toolTip = toolTip;
                _elHeight = elHeight;
                _writeValue = writeValue;

                Control = Build();
            }

            /// <summary>The control element with all added mod info elements.</summary>
            public Control Control { get; }

            public UpdateInfoData InfoData => _infoData;

            /// <summary>
            /// Getter-only text used to filter the mod: merged "Mod dir name",
            /// "url", github "Owner" and github "repository".
            /// </summary>
            public string Filter =>
                string.Join(" ", new[] { _infoData.ModName, Url, _infoData.GitInfo.Owner, _infoData.GitInfo.Repository });

            private string Url
            {
                get
                {
                    var site = _infoData.GitInfo.Site;
                    if (string.IsNullOrWhiteSpace(site)) site = "github.com";
                    if (!site.StartsWith("http", System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        site = "https://" + site;
                    }

                    return $"{site}/{_infoData.GitInfo.Owner}/{_infoData.GitInfo.Repository}";
                }
            }

            private FlowLayoutPanel Build()
            {
                var currentModFlowPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    AutoScroll = true,
                    Size = new System.Drawing.Size(10, 10),
                    Margin = new Padding(10),
                    BorderStyle = BorderStyle.Fixed3D,
                };

                var mnameLabel = new Label
                {
                    AutoSize = true,
                    Text = $"{T._("Name")}: {_infoData.ModName}",
                    Size = new System.Drawing.Size(100, _elHeight * 3),
                    Margin = new Padding(0),
                };
                _toolTip.SetToolTip(mnameLabel, T._("Mod dir name"));
                currentModFlowPanel.Controls.Add(mnameLabel);

                var mnameWidth = mnameLabel.Width + (mnameLabel.Margin.Horizontal * 2);
                var mnameHeight = mnameLabel.Height + (mnameLabel.Margin.Vertical * 2);
                currentModFlowPanel.Size = new System.Drawing.Size
                        (mnameWidth > currentModFlowPanel.Width ? mnameWidth : currentModFlowPanel.Width
                        , mnameHeight > currentModFlowPanel.Height ? mnameHeight : currentModFlowPanel.Height
                        );

                var githubDataFlowPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.TopDown,
                    AutoScroll = true,
                    Size = new System.Drawing.Size(10, 10),
                    Margin = new Padding(0),
                    BorderStyle = BorderStyle.Fixed3D,
                };

                AddGithubData(githubDataFlowPanel);
                AddButtons(githubDataFlowPanel);

                githubDataFlowPanel.Size = new System.Drawing.Size(githubDataFlowPanel.Width + 10, githubDataFlowPanel.Height + 15);
                currentModFlowPanel.Size = new System.Drawing.Size
                    (
                    currentModFlowPanel.Width + githubDataFlowPanel.Width + 15
                    , currentModFlowPanel.Height + githubDataFlowPanel.Height + 15
                    );
                currentModFlowPanel.Size = new System.Drawing.Size(currentModFlowPanel.Width + 15, 300);

                currentModFlowPanel.Controls.Add(githubDataFlowPanel);

                return currentModFlowPanel;
            }

            private void AddGithubData(FlowLayoutPanel githubDataFlowPanel)
            {
                var repNameLabel = new Label
                {
                    Text = "Github",
                    Size = new System.Drawing.Size(100, _elHeight * 3),
                    Margin = new Padding(0),
                };
                githubDataFlowPanel.Controls.Add(repNameLabel);
                var mnameWidth = repNameLabel.Width + (repNameLabel.Margin.Horizontal * 2);
                var mnameHeight = repNameLabel.Height + (repNameLabel.Margin.Vertical * 2);
                githubDataFlowPanel.Size = new System.Drawing.Size
                        (mnameWidth > githubDataFlowPanel.Width ? mnameWidth : githubDataFlowPanel.Width
                        , mnameHeight > githubDataFlowPanel.Height ? mnameHeight : githubDataFlowPanel.Height
                        );

                foreach (var propertyInfo in _infoData.GitInfo.GetType().GetProperties())
                {
                    if (propertyInfo.Name == nameof(_infoData.GitInfo.Site)) continue;
                    if (propertyInfo.Name == nameof(_infoData.GitInfo.Strings)) continue;
                    if (propertyInfo.Name == nameof(_infoData.GitInfo.INI)) continue;
                    if (!propertyInfo.CanWrite) continue;

                    var propertyType = propertyInfo.GetMethod.ReturnType;

                    if (propertyType == typeof(string))
                    {
                        var thePropertyFlowPanel = new FlowLayoutPanel
                        {
                            FlowDirection = FlowDirection.LeftToRight,
                            Margin = new Padding(0)
                        };
                        var l = new Label
                        {
                            Text = _infoData.GitInfo.Strings[propertyInfo.Name].t + ":",
                            Size = new System.Drawing.Size(150, _elHeight),
                            Margin = new Padding(0),
                        };
                        var tb = new TextBox
                        {
                            Size = new System.Drawing.Size(200, _elHeight),
                            Margin = new Padding(0),
                        };
                        tb.DataBindings.Add(new Binding("Text", _infoData, $"{nameof(_infoData.GitInfo)}.{propertyInfo.Name}", true, DataSourceUpdateMode.OnPropertyChanged));
                        tb.TextChanged += (o, e) => _writeValue(tb, _infoData);

                        thePropertyFlowPanel.Controls.Add(l);
                        thePropertyFlowPanel.Controls.Add(tb);

                        _toolTip.SetToolTip(l, _infoData.GitInfo.Strings[propertyInfo.Name].d);
                        _toolTip.SetToolTip(tb, _infoData.GitInfo.Strings[propertyInfo.Name].d);

                        var lWidth = l.Width + (l.Margin.Horizontal * 2);
                        var lHeight = l.Height + (l.Margin.Vertical * 2);
                        var tbWidth = tb.Width + (tb.Margin.Horizontal * 2);
                        var tbHeight = tb.Height + (tb.Margin.Vertical * 2);
                        var ltbWidth = lWidth + tbWidth;
                        var ltbHeight = lHeight + tbHeight;
                        thePropertyFlowPanel.Size = new System.Drawing.Size(ltbWidth, ltbHeight);

                        githubDataFlowPanel.Controls.Add(thePropertyFlowPanel);

                        var thePropertyFlowPanelWidth = thePropertyFlowPanel.Width + (thePropertyFlowPanel.Margin.Horizontal * 2);
                        var thePropertyFlowPanelHeight = thePropertyFlowPanel.Height + (thePropertyFlowPanel.Margin.Vertical * 2);
                        githubDataFlowPanel.Size = new System.Drawing.Size
                            (
                            thePropertyFlowPanelWidth > githubDataFlowPanel.Width ? thePropertyFlowPanelWidth : githubDataFlowPanel.Width
                            , githubDataFlowPanel.Height + thePropertyFlowPanelHeight
                            );
                    }
                    else if (propertyType == typeof(bool))
                    {
                        var cb = new CheckBox
                        {
                            Text = _infoData.GitInfo.Strings[propertyInfo.Name].t,
                            Size = new System.Drawing.Size(300, _elHeight + 10),
                            Margin = new Padding(0)
                        };
                        cb.DataBindings.Add(new Binding("Checked", _infoData, $"{nameof(_infoData.GitInfo)}.{propertyInfo.Name}", true, DataSourceUpdateMode.OnPropertyChanged));
                        cb.CheckedChanged += (o, e) => _writeValue(cb, _infoData);

                        var lWidth = cb.Width + (cb.Margin.Horizontal * 2);
                        var lHeight = cb.Height + (cb.Margin.Vertical * 2);

                        githubDataFlowPanel.Controls.Add(cb);
                        githubDataFlowPanel.Size = new System.Drawing.Size
                            (
                            lWidth > githubDataFlowPanel.Width ? lWidth : githubDataFlowPanel.Width
                            , githubDataFlowPanel.Height + lHeight
                            );

                        _toolTip.SetToolTip(cb, _infoData.GitInfo.Strings[propertyInfo.Name].d);
                    }
                    else continue;
                }
            }

            private void AddButtons(FlowLayoutPanel currentModFlowPanel)
            {
                var buttonsFlp = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    Margin = new Padding(0),
                    BorderStyle = BorderStyle.Fixed3D,
                };

                foreach (var buttonData in new IModUpdateInfoButton[2]
                {
                new OpenWebPage(),
                new OpenModDir(),
                })
                {
                    var openWebPageButton = new Button()
                    {
                        AutoSize = true,
                        Size = new System.Drawing.Size(40, _elHeight),
                        Text = buttonData.Text,
                    };
                    _toolTip.SetToolTip(openWebPageButton, buttonData.Description);
                    openWebPageButton.Click += (o, e) =>
                    {
                        Process.Start(buttonData.GetProcessStartTarget(_infoData));
                    };
                    buttonsFlp.Controls.Add(openWebPageButton);

                    var buttonsFlpWidth = buttonsFlp.Width + (buttonsFlp.Margin.Horizontal * 2);
                    var buttonsFlpHeight = buttonsFlp.Height + (buttonsFlp.Margin.Vertical * 2);
                    currentModFlowPanel.Size = new System.Drawing.Size
                        (
                        buttonsFlpWidth > currentModFlowPanel.Width ? buttonsFlpWidth : currentModFlowPanel.Width
                        , currentModFlowPanel.Height + buttonsFlpHeight
                        );
                }

                currentModFlowPanel.Controls.Add(buttonsFlp);
            }
        }

        public interface IModUpdateInfoButton
        {
            string Text { get; }
            string Description { get; }

            string GetProcessStartTarget(UpdateInfoData infoData);
        }

        public class OpenWebPage : IModUpdateInfoButton
        {
            public string Text => T._("Web");

            public string Description => T._("Open web page of the mod");

            string IModUpdateInfoButton.GetProcessStartTarget(UpdateInfoData infoData)
            {
                var site = $"{(infoData.GitInfo.Site.StartsWith("http", System.StringComparison.InvariantCultureIgnoreCase) ? "" : "https://")}{infoData.GitInfo.Site}";
                var sub = $"/{infoData.GitInfo.Owner}/{infoData.GitInfo.Repository}/releases/latest";

                return $"{site}{sub}";
            }
        }

        public class OpenModDir : IModUpdateInfoButton
        {
            public string Text => T._("Dir");

            public string Description => T._("Open directory of the mod");

            string IModUpdateInfoButton.GetProcessStartTarget(UpdateInfoData infoData) => infoData.GitInfo.INI.INIPath.DirectoryName;
        }

        public class UpdateInfoData
        {
            public UpdateInfoData(ModData mod)
            {
                Mod = mod;
                GitInfo = new GitUpdateInfoData(mod);
            }

            public ModData Mod { get; }
            public string ModName { get => Mod.Name; }
            INIFile INI { get => Mod.MetaIni; }
            public GitUpdateInfoData GitInfo { get; set; }
            public ToolTip ToolTip { get; internal set; }
        }

        public class GitUpdateInfoData
        {
            public string Site { get; } = UpdateRecordParser.SourceHost;

            public string Owner { get; set; } = "";
            public string Repository { get; set; } = "";
            public string FileStartsWith { get; set; } = "";
            public string FileEndsWith { get; set; } = "";
            public bool VersionFromFile { get; set; } = false;

            internal Dictionary<string, (string t, string d)> Strings { get; } = new Dictionary<string, (string t, string d)>()
        {
            {nameof(Owner), (T._("Owner"), T._("Repository owner")) },
            {nameof(Repository), (T._("Repository"), T._("Repository name")) },
            {nameof(FileStartsWith), (T._("File starts with"), T._("File to download starts with. Name part before version.")) },
            {nameof(FileEndsWith), (T._("File ends with"), T._("File to download ends with. Name part after version. Usually extension here")) },
            {nameof(VersionFromFile), (T._("Version from file"), T._("Determines if need to search version in file name.")) },
        };

            public GitUpdateInfoData(ModData mod) { Mod = mod; }

            public ModData Mod { get; }
            public INIFile INI { get => Mod.MetaIni; }

            public void Write()
            {
                bool changed = false;

                bool hasGitHubUrlData = Owner.Length > 0 && Repository.Length > 0;
                if (hasGitHubUrlData && FileStartsWith.Length > 0)
                {
                    // Written through the parser, so the shape stored by the editor is
                    // the one shape every reader of the file agrees on.
                    var newInfo = UpdateRecordParser.Format(
                        new GithubUpdateRecord(Owner, Repository, FileStartsWith, FileEndsWith, VersionFromFile));

                    var currentInfo = INI.GetKey(ManageSettings.AiMetaIniSectionName, ManageSettings.AiMetaIniKeyUpdateName);

                    if (!string.Equals(currentInfo, newInfo, System.StringComparison.InvariantCultureIgnoreCase)
                        && !string.Equals(currentInfo.Replace(", ", ","), newInfo, System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        INI.SetKey(ManageSettings.AiMetaIniSectionName, ManageSettings.AiMetaIniKeyUpdateName, newInfo, false);

                        changed = true;
                    }
                }

                var url = INI.GetKey("General", "url");
                if (hasGitHubUrlData && (url == null || url.Length == 0))
                {
                    //only add the scheme when Site does not already carry one, otherwise the result
                    //becomes "https://https://..."
                    var site = (Site.StartsWith("http", System.StringComparison.InvariantCultureIgnoreCase) ? "" : "https://") + Site;
                    INI.SetKey("General", "url", $"{site}/{Owner}/{Repository}", false);

                    changed = true;
                }

                if (changed) INI.WriteFile();
            }
        }
    }
}
