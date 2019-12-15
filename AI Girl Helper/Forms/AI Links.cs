﻿using AIHelper.Manage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AIHelper
{
    public partial class LinksForm : Form
    {
        public LinksForm()
        {
            InitializeComponent();
            SetLocalization();
        }

        private void SetLocalization()
        {
            //LinksCharactersGroupBox.Text = T._("Characters");
        }

        //private void BooruLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        //{
        //    Process.Start("https://aigirl.booru.org/index.php?page=post&s=list&tags=all");
        //}

        //private void KenzatoLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        //{
        //    Process.Start("https://kenzato.uk/booru/category/AICARD");
        //}

        //private void IllusionDiscordLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        //{
        //    Process.Start("https://discord.gg/TyQtXkf");
        //}

        //private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        //{
        //    Process.Start("https://pastebin.com/QRRKtC45");
        //}

        //private void IllusionOficialUploader_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        //{
        //    Process.Start("https://mega.nz/#F!XOpkWahD!d0CpOSqiwww-M9QAVBjBSw");        
        //}

        private void LinksForm_Load(object sender, System.EventArgs e)
        {
            GetLinksListAndAddLinks();
        }

        private readonly string linksSeparator = "{{link}}";
        private void GetLinksListAndAddLinks()
        {
            Dictionary<string, string> GroupNames = new Dictionary<string, string>
            {
                { "Characters", T._("Characters") },
                { "Mods", T._("Mods") },
                { "Authors", T._("Authors") },
                { "Resources", T._("Resources") },
                { "Other", T._("Other") }
            };

            string GameLinksPath = Path.Combine(Properties.Settings.Default.AppResDir, "links", ManageSettings.GetCurrentGameFolderName() + ".txt");
            if (!File.Exists(GameLinksPath))
            {
                GameLinksPath = Path.Combine(Properties.Settings.Default.AppResDir, "links", "Default.txt");
            }
            if (File.Exists(GameLinksPath))
            {
                //https://stackoverflow.com/questions/16959122/displaying-multiple-linklabels-in-a-form
                string[] links = File.ReadAllLines(GameLinksPath).Where(line => !line.StartsWith(";")).ToArray();
                int groupcnt = 0;
                string lastgroup = string.Empty;
                foreach (var line in links)
                {
                    string group = line.Split(new string[] { linksSeparator }, StringSplitOptions.None)[0];
                    if (lastgroup != group)
                    {
                        lastgroup = group;
                        groupcnt++;
                    }
                }
                lastgroup = string.Empty;

                TableLayoutPanel panel = new TableLayoutPanel
                {
                    RowCount = links.Length + groupcnt,
                    ColumnCount = 1
                };
                //panel.MaximumSize = new System.Drawing.Size(354, 138);
                int currentRow = 0;

                foreach (var link in links)
                {
                    string[] linkParts = link.Split(new string[] { linksSeparator }, StringSplitOptions.None);

                    LinkLabel linkLabel = new LinkLabel
                    {
                        Text = linkParts[1]
                    };
                    linkLabel.Links.Add(0, linkParts[1].Length, linkParts[2]);
                    linkLabel.LinkClicked += OnLinkClicked;

                    if (lastgroup != linkParts[0])
                    {
                        lastgroup = linkParts[0];
                        Label lblGroup = new Label
                        {
                            Text = GroupNames.Keys.Contains(linkParts[0]) ? GroupNames[linkParts[0]] : linkParts[0]
                        };

                        panel.Controls.Add(lblGroup, 0, currentRow++);
                        panel.Controls[currentRow - 1].Height = 13;
                        panel.Controls[currentRow - 1].Width = 330;
                    }

                    panel.Controls.Add(linkLabel, 0, currentRow++);
                    panel.Controls[currentRow - 1].Height = 13;
                    panel.Controls[currentRow - 1].Width = 330;
                }
                //panel.AutoSize = true;
                this.Controls.Add(panel);
                //panel.HorizontalScroll.Visible = false;
                panel.AutoScroll = true;
                panel.MaximumSize = new System.Drawing.Size(354, 138);
                //panel.Size = new System.Drawing.Size(354, 138);
                panel.Dock = DockStyle.Fill;
            }
            else
            {
                //this.Controls.Add(lbl);
            }

        }

        void OnLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string link = e.Link.LinkData.ToString();
            Process.Start(link);
        }
    }
}