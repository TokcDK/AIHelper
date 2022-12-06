using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using AIHelper.Data.Modlist;
using AIHelper.Manage.Update.Sources;
using INIFileMan;
using IniParser.Model;

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

        readonly int _elHeight = 13;

        readonly List<UpdateInfoData> _updateInfoDatas = new List<UpdateInfoData>();

        bool _isReading;
        bool _isWriting;

        internal void OpenInfoEditor()
        {
            var f = new Form
            {
                //Size = new System.Drawing.Size(680, 605),
                Text = T._("Mods update info settings") + " (" + ManageSettings.CurrentGameDisplayingName + ")",
                StartPosition = FormStartPosition.CenterScreen
            };
            var p = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };
            var modsListFlowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                //FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                //Size = new System.Drawing.Size(500, 300),
                Margin = new Padding(0)
            };

            p.Controls.Add(modsListFlowPanel);
            f.Controls.Add(p);

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
            foreach (var mod in mods.ModsPlusOverwrite)
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

            //modsListFlowPanel.Size = new System.Drawing.Size
            //    (modsListFlowPanel.Width * 2
            //    , modsListFlowPanel.Height * 2
            //    );
            //f.Size = new System.Drawing.Size
            //    (modsListFlowPanel.Width
            //    + (modsListFlowPanel.Margin.Horizontal * 2)
            //    , modsListFlowPanel.Height
            //    + (modsListFlowPanel.Margin.Vertical * 2)
            //    );
            f.Size = new System.Drawing.Size(f.Width * 2, f.Height * 2);
            f.Show(ManageSettings.MainForm);

            _isReading = false;

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
                        Size = new System.Drawing.Size(300, _elHeight+10),
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
                if (hasGitHubUrlData && url.Length == 0)
                {
                    var site = (Site.StartsWith("http", System.StringComparison.InvariantCultureIgnoreCase) ? "https://" : "") + Site;
                    INI.SetKey(ManageSettings.AiMetaIniSectionName, ManageSettings.AiMetaIniKeyUpdateName, $"{site}/{Owner}/{Repository}", false);

                    changed = true;
                }

                if (changed) INI.WriteFile();
            }
        }
    }
}
