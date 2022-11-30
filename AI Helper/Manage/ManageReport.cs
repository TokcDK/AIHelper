using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace AIHelper.Manage
{
    internal static class ManageReport
    {
        internal class LinksInfos : Dictionary<string, Dictionary<string, string>>
        {
        }
        internal class LinkData
        {
            internal string Base { get; set; } = "";
            internal string Category { get; set; } = "";
            internal string Name { get; set; } = "";
            internal string Description { get; set; } = "";
            internal string Link { get; set; }
        }

        private static readonly string _linksSeparator = "{{link}}";
        internal static void ShowReportFromLinks()
        {
            // get all records data from ini
            var iniInfos = new LinksInfos();
            foreach (var iniFilePath in Directory.GetFiles(ManageSettings.LiksIniInfosDirPath, "*.ini", SearchOption.AllDirectories))
            {
                var ini = ManageIni.GetINIFile(iniFilePath);

                // get all sections
                foreach (var sectionName in ini.EnumerateSectionNames())
                {
                    if (iniInfos.ContainsKey(sectionName)) continue;

                    var sectionData = new Dictionary<string, string>();

                    // add record keys data
                    foreach (var keyName in ini.EnumerateSectionKeyNames(sectionName))
                    {
                        sectionData.Add(keyName, ini.GetKey(sectionName, keyName));
                    }

                    // add record data
                    iniInfos.Add(sectionName, sectionData);
                }
            }

            var langID = "." + ManageSettings.LanuageID.ToLowerInvariant();
            var gameID = "." + ManageSettings.CurrentGame.GameAbbreviation.ToLowerInvariant();

            // build records
            var dataForAdd = new Dictionary<string, LinkData>();
            foreach (var info in iniInfos)
            {
                var data = info;

                var id = data.Key;

                if (!id.EndsWith(gameID)
                    && !id.EndsWith(gameID + langID)
                    && !id.EndsWith(".all")
                    && !id.EndsWith($".all{langID}")
                    ) continue;


                var isLocaleBased = id.EndsWith(langID);

                if (isLocaleBased)
                {
                    var nonLocaleId = id.Remove(id.Length - langID.Length);
                    if (dataForAdd.ContainsKey(nonLocaleId)) dataForAdd.Remove(nonLocaleId);
                }

                var record = data.Value;

                var @base = record.ContainsKey("base") ? record["base"] : "";

                var link = record.ContainsKey("link") ? record["link"] : "";
                link = RestoreString(iniInfos, link, "link", @base);
                if (string.IsNullOrWhiteSpace(link)) continue;

                var name = record.ContainsKey("name") ? record["name"] : "";
            }

            #region old
            //var groupNames = new Dictionary<string, string>();


            //var langID = "<" + ManageSettings.LanuageID + ">";

            //string gameLinksPath = ManageSettings.LinksInfoFilePath;
            //if (string.IsNullOrWhiteSpace(gameLinksPath)) return;

            //string[] strings = File.ReadAllLines(gameLinksPath).Where(line => line.StartsWith(";##", StringComparison.InvariantCulture)).ToArray();
            //foreach (var line in strings)
            //{
            //    var en = Regex.Match(line, ";##([^<]+)");
            //    if (!en.Success) continue;

            //    var t = Regex.Match(line, langID + "([^<]+)");
            //    if (!t.Success) continue;
            //    if (groupNames.ContainsKey(en.Groups[1].Value)) continue;

            //    groupNames.Add(en.Groups[1].Value, t.Groups[1].Value);
            //}

            //string[] links = File.ReadAllLines(gameLinksPath).Where(line => !line.StartsWith(";", StringComparison.InvariantCulture)).ToArray();

            //string lastgroup = string.Empty;
            //var linksInfo = new Dictionary<string, List<string[]>>();

            //// add info
            //foreach (var line in links)
            //{
            //    var info = line.Split(new string[] { _linksSeparator }, StringSplitOptions.None);
            //    if (info.Length != 3) continue;

            //    var category = info[0];
            //    if (!linksInfo.ContainsKey(category)) linksInfo.Add(category, new List<string[]>());

            //    linksInfo[category].Add(new[] { info[1], info[2] });
            //}

            //// create html string
            //var categoriesInfo = new List<string>();
            //foreach (var category in linksInfo)
            //{
            //    var descriptonLink = new List<string>();
            //    foreach (var link in category.Value)
            //    {
            //        descriptonLink.Add(ManageSettings.UpdateReport.HtmlReportCategoryItemTemplate.Replace("%link%", link[1])
            //            .Replace("%text%", new Uri(link[1]).Host.ToUpperInvariant())
            //            .Replace("%description%", TryGetTranslation(link[0], langID))
            //            );
            //    }

            //    categoriesInfo.Add(ManageSettings.UpdateReport.HtmlReportCategoryTemplate.Replace("%category%", groupNames.TryGetValue(category.Key)).Replace("%items%", string.Join("<br>", descriptonLink)));
            //}

            //// create new report contet
            //var reportMessage = File.ReadAllText(ManageSettings.UpdateReport.ReportFilePath)
            // .Replace(ManageSettings.UpdateReport.BgImageLinkPathPattern, ManageSettings.UpdateReport.CurrentGameBgFilePath)
            // .Replace(ManageSettings.UpdateReport.ModsUpdateReportHeaderTextPattern, T._("Links"))
            // .Replace(ManageSettings.UpdateReport.SingleModUpdateReportsTextSectionPattern, string.Join(ManageSettings.UpdateReport.HtmlBetweenModsText, categoriesInfo) + "<br>")
            // .Replace(ManageSettings.UpdateReport.ModsUpdateInfoNoticePattern, ManageSettings.UpdateReport.ModsUpdateInfoNoticeText);

            //var htmlfile = Path.Combine(ManageSettings.CurrentGameLinksInfoDirPath, ManageSettings.CurrentGame.GameAbbreviation + ".html");

            //Directory.CreateDirectory(Path.GetDirectoryName(htmlfile));// fix missing parent directory error

            //File.WriteAllText(htmlfile, reportMessage);
            //using (var process = new System.Diagnostics.Process())
            //{
            //    try
            //    {
            //        process.StartInfo.UseShellExecute = true;
            //        process.StartInfo.FileName = htmlfile;
            //        process.Start();
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(e.Message);
            //    }
            //}
            #endregion old
        }

        private static string RestoreString(LinksInfos iniInfos, string s, string keyName, string baseRecordID)
        {
            if (!s.Contains("%")) return s;

            var matches = Regex.Matches(s, @"\%[^\%]+\%");
            var retS = s;
            foreach (Match match in matches)
            {
                if (string.Equals(match.Value, "%base%", StringComparison.InvariantCultureIgnoreCase))
                {
                    string s1 = ReplaceBase(iniInfos, s, keyName, baseRecordID);
                    if (s1 == null) continue;

                    retS = s1;
                }
                else
                {

                }
            }

            return retS;
        }

        // chack if have base marker
        // check base for empty
        // check if have key
        // get key value

        private static string ReplaceBase(LinksInfos iniInfos, string s, string keyName, string baseRecordID)
        {
            if (string.IsNullOrEmpty(baseRecordID)) return null;
            if (!iniInfos.ContainsKey(baseRecordID)) return null;

            var baseRecodrKeys = iniInfos[baseRecordID];

            // try get key value from baseData
            if (baseRecodrKeys.ContainsKey(keyName))
            {
                var baseKey = baseRecodrKeys[keyName];

                return s.Replace("%base%", baseKey);
            }
            else // have no key!
            {
                // read possible base from parent
                if (!baseRecodrKeys.ContainsKey("base")) return null;
                var baseSub = baseRecodrKeys["base"];

                var s1 = ReplaceBase(iniInfos, s, keyName, baseSub);
                if (s1 == null) return null;

                return s1;
            }
        }

        private static string TryGetValue(this Dictionary<string, string> dictionary, string key)
        {
            if (dictionary.ContainsKey(key)) return dictionary[key];

            return key;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="languageID">string like <ru-RU></param>
        /// <returns></returns>
        private static string TryGetTranslation(this string str, string languageID)
        {
            var o = Regex.Match(str, "^([^<]+)");
            var t = Regex.Match(str, languageID + "([^<]+)");
            if (!t.Success) return o.Groups[1].Value;

            return t.Groups[1].Value;
        }
    }
}
