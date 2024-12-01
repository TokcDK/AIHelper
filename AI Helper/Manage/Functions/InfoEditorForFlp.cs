using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using AIHelper.Data.Modlist;
using AIHelper.Manage.ui.themes;
using AIHelper.Manage.Update;
using AIHelper.Manage.Update.Sources;
using AIHelper.Manage.Update.Targets.Mods;
using INIFileMan;

namespace AIHelper.Manage.Functions
{
    internal class InfoEditorForFlp : FunctionForFlpBase
    {
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

        bool _isReading;
        bool _isWriting;

        private static void InitLogTextBox()
        {
            if (Logtb == null || !Logtb.IsAccessible || Logtb.IsDisposed)
            {
                Logtb = new RichTextBox() { ReadOnly = true, Width = 200 };
            }
        }

        static void Log(string v) { Logtb.Text += v + "\r\n"; }

        internal void OpenInfoEditor()
        {
            var modUpdateInfosForm = new Form
            {
                //Size = new System.Drawing.Size(680, 605),
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
            var rowStyleHeader = new RowStyle
            {
                SizeType = SizeType.Absolute,
                Height = 30,
            };
            var rowStyleContent = new RowStyle
            {
                SizeType = SizeType.AutoSize
            };
            modUpdateInfosTableLayoutPanel.RowStyles.Add(rowStyleHeader);
            modUpdateInfosTableLayoutPanel.RowStyles.Add(rowStyleContent);

            // control buttons
            var controlPanelFlowLayoutPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0),
                AutoSize = true,
            };
            modUpdateInfosTableLayoutPanel.SetRow(controlPanelFlowLayoutPanel, 0);
            var addNewModButton = new Button
            {
                Text = T._("Add"),
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Height = 25
            };
            var loadGitInfosButton = new Button
            {
                Text = T._("All"),
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Height = 25
            };
            controlPanelFlowLayoutPanel.Controls.Add(loadGitInfosButton);
            controlPanelFlowLayoutPanel.Controls.Add(addNewModButton);

            var addNewModPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };
            modUpdateInfosTableLayoutPanel.SetRow(addNewModPanel, 1);

            // button events
            addNewModButton.Click += new EventHandler((sender, e) =>
            {
                LoadAddModPanel(modUpdateInfosForm, addNewModPanel);
            });
            loadGitInfosButton.Click += new EventHandler((sender, e) =>
            {
                addNewModPanel.Controls.Clear();
                LoadGitInfos(modUpdateInfosForm, addNewModPanel);
            });

            modUpdateInfosTableLayoutPanel.Controls.Add(controlPanelFlowLayoutPanel);
            modUpdateInfosTableLayoutPanel.Controls.Add(addNewModPanel);
            modUpdateInfosForm.Controls.Add(modUpdateInfosTableLayoutPanel);

            LoadGitInfos(modUpdateInfosForm, addNewModPanel);

            //modsListFlowPanel.Size = new System.Drawing.Size
            //    (modsListFlowPanel.Width * 2
            //    , modsListFlowPanel.Height * 2
            //    );
            //form.Size = new System.Drawing.Size
            //    (modsListFlowPanel.Width
            //    + (modsListFlowPanel.Margin.Horizontal * 2)
            //    , modsListFlowPanel.Height
            //    + (modsListFlowPanel.Margin.Vertical * 2)
            //    );
            modUpdateInfosForm.Size = new System.Drawing.Size(modUpdateInfosForm.Width * 2, modUpdateInfosForm.Height * 2);
            modUpdateInfosForm.Show(ManageSettings.MainForm);

            _isReading = false;
        }

        class newModPropData
        {
            public newModPropData(string labelText, string textBoxText)
            {
                LabelText = labelText;
                TextBoxText = textBoxText;
            }

            public string LabelText { get; }
            public string TextBoxText { get; set; }

            public TextBox TB;
        }

        private void LoadAddModPanel(Form modUpdateInfosForm, Panel addNewModPanel)
        {
            bool addModPanelIsLoading = true;

            addNewModPanel.Controls.Clear();

            var modUpdateInfoMainFlp = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                //AutoScroll = true,
                Margin = new Padding(0),
                FlowDirection = FlowDirection.TopDown,
            };

            var defaultModDirName = "NewMod";
            var modnamePropData = new newModPropData(T._("Mod dir name"), defaultModDirName);
            var urlPropData = new newModPropData("Url", "https://github.com/Owner/Name");
            var startsWithPropData = new newModPropData(T._("File starts with"), "");
            var endsWithPropData = new newModPropData(T._("File ends with"), "");
            var newModDatas = new newModPropData[]
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
                    ForeColor = Color.White,
                    Size = new System.Drawing.Size(150, _elHeight),
                };
                var urlTextBox = new TextBox
                {
                    Size = new System.Drawing.Size(addNewModPanel.Width - l.Width - 20, _elHeight),
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
                        catch { return; }
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
                ForeColor = Color.White,
                Size = new System.Drawing.Size(addNewModPanel.Width - 20, _elHeight + 5),
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
                    if (string.IsNullOrWhiteSpace(modnamePropData.TextBoxText))
                    {
                        Log(T._("Mod name is empty"));
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(urlPropData.TextBoxText))
                    {
                        Log(T._("Url s empty"));
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(startsWithPropData.TextBoxText))
                    {
                        Log($"{startsWithPropData.LabelText} {T._("value is empty")}");
                        return;
                    }

                    var targetModDirName = modnamePropData.TextBoxText;
                    var targetUrl = urlPropData.TextBoxText;
                    var startsWith = startsWithPropData.TextBoxText;

                    var m = Regex.Match(targetUrl,
                        @"(^.*https?\:\/\/)?github\.com\/([^\/]+)\/([^\/\? ]+).*$",
                        RegexOptions.IgnoreCase);
                    if (!m.Success)
                    {
                        Log(T._("Url is not recognized"));
                        return;
                    }
                    if (m.Groups.Count != 4)
                    {
                        Log(T._("Url is not recognized"));
                        return;
                    }

                    var owner = m.Groups[2].Value;
                    if (string.IsNullOrWhiteSpace(owner))
                    {
                        Log(T._("Url is not recognized"));
                        return;
                    }
                    var rep = m.Groups[3].Value;
                    if (string.IsNullOrWhiteSpace(rep))
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

                    var info = new Update.UpdateInfo
                    {
                        TargetFolderPath = targetDirPathInfo,
                        TargetFolderUpdateInfo = new string[3] { owner, rep, startsWith }
                    };

                    if (!string.IsNullOrWhiteSpace(endsWithPropData.TextBoxText))
                    {
                        info.TargetFolderUpdateInfo = info.TargetFolderUpdateInfo.Concat(new string[1] { endsWithPropData.TextBoxText }).ToArray();
                    }
                    info.TargetFolderUpdateInfo = info.TargetFolderUpdateInfo.Concat(new string[1] { verFromFile.Checked.ToString() }).ToArray();

                    var ghub = new Github(info);

                    info.TargetLastVersion = ghub.GetLastVersion();

                    if (info.TargetLastVersion.Length == 0)
                    {
                        Log(T._("Cant get last version"));
                        return;
                    }

                    // clean version for more correct comprasion
                    UpdateTools.CleanVersion(ref info.TargetLastVersion);

                    //if it is last version then run update
                    if (!info.TargetLastVersion.IsNewerOf("0", false))
                    {
                        Log(T._("Version is lesser. Check entered data."));
                        return;
                    }

                    var modData = new ModData
                    {
                        Path = info.TargetFolderPath.FullName,
                        Name = modnamePropData.TextBoxText,
                        Priority = 9999
                    };

                    var ginfo = new GitUpdateInfoData(modData)
                    {
                        Owner = owner,
                        Repository = rep,
                        FileStartsWith = startsWith,
                        FileEndsWith = endsWithPropData.TextBoxText,
                        VersionFromFile = verFromFile.Checked
                    };

                    GetGHubFile(ghub, ginfo);
                }
                catch (Exception ex)
                {
                    Log(T._("Error: " + ex.Message));
                    return;
                }
            });
            modUpdateInfoMainFlp.Controls.Add(DownloadAndAddMod);

            InitLogTextBox();
            Logtb.Width = modUpdateInfoMainFlp.Width - 10;
            modUpdateInfoMainFlp.Controls.Add(Logtb);

            addNewModPanel.Controls.Add(modUpdateInfoMainFlp);

            ThemesLoader.SetTheme(ManageSettings.CurrentTheme, addNewModPanel);

            addModPanelIsLoading = false;
        }

        private static async void GetGHubFile(Github ghub, GitUpdateInfoData ginfo)
        {
            bool getfileIsTrue = await ghub.GetFile().ConfigureAwait(true); // download latest file
            if (!getfileIsTrue)
            {
                Log(T._("Cant download file!"));
                return;
            }

            var t = new ModsMeta(ghub.Info);

            if (!t.UpdateFiles())
            {
                Log(T._("Cant move file into mods dir"));
                return;
            }

            ginfo.Mod.MetaIni = new INIFile(Path.Combine(ghub.Info.TargetFolderPath.FullName, "meta.ini"), true);

            ginfo.Write();

            Log(T._("Mod succesfully downloaded and added!"));
        }

        private void LoadGitInfos(Form f, Panel p)
        {
            var modsListFlowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                //FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                //Size = new System.Drawing.Size(500, 300),
                Margin = new Padding(0)
            };

            p.Controls.Add(modsListFlowPanel);

            var ttip = new ToolTip
            {
                // Set up the delays for the ToolTip.
                AutoPopDelay = 32000,
                InitialDelay = 1000,
                ReshowDelay = 500,
                UseAnimation = true,
                UseFading = true,
                // Force the ToolTip text to be displayed whether or not the form is active.
                ShowAlways = true
            };

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

                var github = new Github(null);
                var urlMatch = Regex.Match(url, $@"(https?:\/\/){github.Url}\/([^\/]+)\/([^\/]+)", RegexOptions.IgnoreCase);
                var infoData = new UpdateInfoData(mod)
                {
                    ToolTip = ttip
                };

                if (!updateInfoIsEmpty && updateInfo.StartsWith(github.InfoId))
                {
                    var extractedInfo = updateInfo
                        .Remove(updateInfo.Length - 2)
                        .Remove(0, github.InfoId.Length + 2).Split(',');

                    var owner = extractedInfo[0];
                    var repName = extractedInfo[1];
                    var start = extractedInfo[2];
                    var end = "";
                    if (extractedInfo.Length > 3)
                    {
                        end = extractedInfo[3];
                    }
                    var isFromFile = false;
                    if (extractedInfo.Length > 4 && bool.TryParse(extractedInfo[4], out bool b))
                    {
                        isFromFile = b;
                    }

                    infoData.GitInfo.Owner = owner.Trim();
                    infoData.GitInfo.Repository = repName.Trim();
                    infoData.GitInfo.FileStartsWith = start.Trim();
                    infoData.GitInfo.FileEndsWith = end.Trim();
                    infoData.GitInfo.VersionFromFile = isFromFile;
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

                var currentModFlowPanel = new FlowLayoutPanel
                {
                    //Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    AutoScroll = true,
                    Size = new System.Drawing.Size(10, 10),
                    Margin = new Padding(10),
                    BorderStyle = BorderStyle.Fixed3D,
                };
                var mnameLabel = new Label
                {
                    AutoSize = true,
                    Text = $"{T._("Name")}: {infoData.ModName}",
                    Size = new System.Drawing.Size(100, _elHeight * 3),
                    Margin = new Padding(0),
                };
                infoData.ToolTip.SetToolTip(mnameLabel, T._("Mod dir name"));
                currentModFlowPanel.Controls.Add(mnameLabel);
                var mnameWidth = mnameLabel.Width + (mnameLabel.Margin.Horizontal * 2);
                var mnameHeight = mnameLabel.Height + (mnameLabel.Margin.Vertical * 2);
                currentModFlowPanel.Size = new System.Drawing.Size
                        (mnameWidth > currentModFlowPanel.Width ? mnameWidth : currentModFlowPanel.Width
                        , mnameHeight > currentModFlowPanel.Height ? mnameHeight : currentModFlowPanel.Height
                        );

                var githubDataFlowPanel = new FlowLayoutPanel
                {
                    //Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.TopDown,
                    AutoScroll = true,
                    Size = new System.Drawing.Size(10, 10),
                    Margin = new Padding(0),
                    BorderStyle = BorderStyle.Fixed3D,
                };

                AddGithubData(githubDataFlowPanel, infoData);

                AddButtons(githubDataFlowPanel, infoData);

                githubDataFlowPanel.Size = new System.Drawing.Size(githubDataFlowPanel.Width + 10, githubDataFlowPanel.Height + 15);
                currentModFlowPanel.Size = new System.Drawing.Size
                    (
                    currentModFlowPanel.Width + githubDataFlowPanel.Width + 15
                    , currentModFlowPanel.Height + githubDataFlowPanel.Height + 15
                    );
                currentModFlowPanel.Size = new System.Drawing.Size(currentModFlowPanel.Width + 15, 300 /*currentModFlowPanel.Height + 15*/);

                currentModFlowPanel.Controls.Add(githubDataFlowPanel);
                modsListFlowPanel.Controls.Add(currentModFlowPanel);

                //var curModFlpWidth = currentModFlowPanel.Width + (currentModFlowPanel.Margin.Horizontal * 2);
                //var curModFlpHeight = currentModFlowPanel.Height + (currentModFlowPanel.Margin.Vertical * 2);
                //modsListFlowPanel.Size = new System.Drawing.Size
                //    (
                //    curModFlpWidth > modsListFlowPanel.Width ? curModFlpWidth : modsListFlowPanel.Width
                //    , curModFlpHeight > modsListFlowPanel.Height ? curModFlpHeight : modsListFlowPanel.Height
                //    );

                var curModFlpWidth = currentModFlowPanel.Width + (currentModFlowPanel.Margin.Horizontal * 2);
                var curModFlpHeight = currentModFlowPanel.Height + (currentModFlowPanel.Margin.Vertical * 2);
                f.Size = new System.Drawing.Size
                    (
                    curModFlpWidth > f.Width ? curModFlpWidth : f.Width
                    , curModFlpHeight > f.Height ? curModFlpHeight : f.Height
                    );

                _updateInfoDatas.Add(infoData);
            }

            ThemesLoader.SetTheme(ManageSettings.CurrentTheme, f);
        }

        private void AddGithubData(FlowLayoutPanel githubDataFlowPanel, UpdateInfoData infoData)
        {
            var repNameLabel = new Label
            {
                //AutoSize = true,
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
            foreach (var propertyInfo in infoData.GitInfo.GetType().GetProperties())
            {
                if (propertyInfo.Name == nameof(infoData.GitInfo.Site)) continue;
                if (propertyInfo.Name == nameof(infoData.GitInfo.Strings)) continue;
                if (propertyInfo.Name == nameof(infoData.GitInfo.INI)) continue;
                if (!propertyInfo.CanWrite) continue;

                var propertyType = propertyInfo.GetMethod.ReturnType;

                if (propertyType == typeof(string))
                {
                    var thePropertyFlowPanel = new FlowLayoutPanel
                    {
                        //Dock = DockStyle.Fill,
                        FlowDirection = FlowDirection.LeftToRight,
                        //AutoScroll = true,
                        //Size = new System.Drawing.Size(300, 20),
                        //AutoSize = true,
                        Margin = new Padding(0)
                    };
                    var l = new Label
                    {
                        //AutoSize = true,
                        Text = infoData.GitInfo.Strings[propertyInfo.Name].t + ":",
                        Size = new System.Drawing.Size(150, _elHeight),
                        Margin = new Padding(0),
                    };
                    var tb = new TextBox
                    {
                        //AutoSize = true,
                        Size = new System.Drawing.Size(200, _elHeight),
                        Margin = new Padding(0),
                    };
                    tb.DataBindings.Add(new Binding("Text", infoData, $"{nameof(infoData.GitInfo)}.{propertyInfo.Name}", true, DataSourceUpdateMode.OnPropertyChanged));
                    tb.TextChanged += new System.EventHandler((o, e) => { WriteValue(tb, infoData); });

                    thePropertyFlowPanel.Controls.Add(l);
                    thePropertyFlowPanel.Controls.Add(tb);

                    infoData.ToolTip.SetToolTip(l, infoData.GitInfo.Strings[propertyInfo.Name].d);
                    infoData.ToolTip.SetToolTip(tb, infoData.GitInfo.Strings[propertyInfo.Name].d);

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
                        //AutoSize = true,
                        Text = infoData.GitInfo.Strings[propertyInfo.Name].t,
                        Size = new System.Drawing.Size(300, _elHeight + 10),
                        Margin = new Padding(0)
                    };
                    cb.DataBindings.Add(new Binding("Checked", infoData, $"{nameof(infoData.GitInfo)}.{propertyInfo.Name}", true, DataSourceUpdateMode.OnPropertyChanged));
                    cb.CheckedChanged += new System.EventHandler((o, e) => { WriteValue(cb, infoData); });

                    var lWidth = cb.Width + (cb.Margin.Horizontal * 2);
                    var lHeight = cb.Height + (cb.Margin.Vertical * 2);

                    githubDataFlowPanel.Controls.Add(cb);
                    githubDataFlowPanel.Size = new System.Drawing.Size
                        (
                        lWidth > githubDataFlowPanel.Width ? lWidth : githubDataFlowPanel.Width
                        , githubDataFlowPanel.Height + lHeight
                        );

                    infoData.ToolTip.SetToolTip(cb, infoData.GitInfo.Strings[propertyInfo.Name].d);
                }
                else continue;
            }
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

        private void AddButtons(FlowLayoutPanel currentModFlowPanel, UpdateInfoData infoData)
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
                var dirPath = infoData.Mod.Path;
                var openWebPageButton = new Button()
                {
                    AutoSize = true,
                    Size = new System.Drawing.Size(40, _elHeight),
                    Text = buttonData.Text,
                };
                infoData.ToolTip.SetToolTip(openWebPageButton, buttonData.Description);
                openWebPageButton.Click += new System.EventHandler((o, e) =>
                {
                    Process.Start(buttonData.GetProcessStartTarget(infoData));
                });
                buttonsFlp.Controls.Add(openWebPageButton);
                currentModFlowPanel.Controls.Add(buttonsFlp);

                var buttonsFlpWidth = buttonsFlp.Width + (buttonsFlp.Margin.Horizontal * 2);
                var buttonsFlpHeight = buttonsFlp.Height + (buttonsFlp.Margin.Vertical * 2);
                currentModFlowPanel.Size = new System.Drawing.Size
                    (
                    buttonsFlpWidth > currentModFlowPanel.Width ? buttonsFlpWidth : currentModFlowPanel.Width
                    , currentModFlowPanel.Height + buttonsFlpHeight
                    );
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
            public string Site { get; } = "github.com";
            public string Marker { get; } = "updgit";

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
                    var fileEnds = FileEndsWith.Length > 0 ? "," + FileEndsWith : "";
                    var verFromFile = (VersionFromFile && fileEnds.Length > 0 ? "," + VersionFromFile : "").ToLowerInvariant();
                    var newInfo = $"{Marker}::{Owner},{Repository},{FileStartsWith}{fileEnds}{verFromFile}::";

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
                    var site = (Site.StartsWith("http", System.StringComparison.InvariantCultureIgnoreCase) ? "https://" : "") + Site;
                    INI.SetKey("General", "url", $"https://{site}/{Owner}/{Repository}", false);

                    changed = true;
                }

                if (changed) INI.WriteFile();
            }
        }
    }
}
