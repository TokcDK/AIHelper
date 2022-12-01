﻿using System.Collections.Generic;
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
            var gameAllID = ".all";

            // build records
            var dataForAdd = new Dictionary<string, LinkData>();

            foreach (var info in iniInfos)
            {
                var data = info;

                var id = data.Key;
                var record = data.Value;

                // relink .all->.game => .notlang->.lang => .all.lang->.game.lang
                bool relinkedToLocalBased = false;
                bool alreadyHaveTheId = false;
                foreach (var (sourceSuffix, targetSuffix, b) in new[]
                {
                    (gameAllID, gameID, true), // default record to gamebased record
                    (langID, langID, false), // not lang based record to lang baed record
                    ($"{gameAllID}{langID}", $"{gameID}{langID}", true), // default lang based to game lang based
                })
                {
                    if (id.EndsWith(sourceSuffix) != b) continue;

                    var newId = (b ? $"{id.Remove(id.Length - sourceSuffix.Length)}" : id) + targetSuffix;

                    // break if id already added
                    if (dataForAdd.ContainsKey(newId))
                    {
                        alreadyHaveTheId = true;
                        break;
                    }

                    if (!iniInfos.ContainsKey(newId)) continue;

                    // when have the id, relink to it
                    id = newId;
                    record = iniInfos[newId];

                    relinkedToLocalBased = true; // set to not make checks below
                }
                if (alreadyHaveTheId) continue; // skip the id because already added

                //if (id.EndsWith(gameAllID))
                //{
                //    // relink to gamebased id

                //    var gameBasedId = $"{id.Remove(id.Length - gameAllID.Length)}{gameID}";

                //    if (dataForAdd.ContainsKey(gameBasedId)) continue;

                //    if (iniInfos.ContainsKey(gameBasedId))
                //    {
                //        id = gameBasedId;
                //        record = iniInfos[gameBasedId];

                //        relinkedToLocalBased = true;
                //    }
                //}

                //if (!id.EndsWith(langID))
                //{
                //    // relink to localebased id

                //    var langBasedId = $"{id}{langID}";

                //    if (dataForAdd.ContainsKey(langBasedId)) continue;

                //    if (iniInfos.ContainsKey(langBasedId))
                //    {
                //        id = langBasedId;
                //        record = iniInfos[langBasedId];

                //        relinkedToLocalBased = true;
                //    }
                //}
                //else if (id.EndsWith($"{gameAllID}{langID}"))
                //{
                //    // relink to gamebased id

                //    var gameLangBasedId = $"{id.Remove(id.Length - $"{gameAllID}{langID}".Length)}{gameID}{langID}";

                //    if (dataForAdd.ContainsKey(gameLangBasedId)) continue;

                //    if (iniInfos.ContainsKey(gameLangBasedId))
                //    {
                //        id = gameLangBasedId;
                //        record = iniInfos[gameLangBasedId];

                //        relinkedToLocalBased = true;
                //    }
                //}

                if (!id.EndsWith(gameID)
                    && !id.EndsWith($"{gameID}{langID}")
                    && !id.EndsWith(gameAllID)
                    && !id.EndsWith($"{gameAllID}{langID}")
                    ) continue;

                //var isLocaleBased = id.EndsWith(langID);
                //if (isLocaleBased)
                //{
                //    var nonLocaleId = id.Remove(id.Length - langID.Length);
                //    if (dataForAdd.ContainsKey(nonLocaleId)) dataForAdd.Remove(nonLocaleId);
                //}

                var @base = record.ContainsKey("base") ? record["base"] : "";
                if (@base == id)
                {
                    bool checkBase = true;
                    if (@base.EndsWith(langID))
                    {
                        // remove lang id
                        @base = @base.Remove(@base.Length- langID.Length);
                        if (iniInfos.ContainsKey(@base)) checkBase = false;
                    }
                    if(checkBase && @base.EndsWith(gameID))
                    {
                        // remove game id
                        @base = @base.Remove(@base.Length - gameID.Length);
                        if (iniInfos.ContainsKey(@base)) checkBase = false;
                    }
                    if(checkBase && @base.EndsWith(gameAllID))
                    {
                        // remove game id
                        @base = @base.Remove(@base.Length - gameAllID.Length);
                        if (iniInfos.ContainsKey(@base)) checkBase = false;
                    }
                    if (checkBase) @base = ""; // reset if not found valid id for base
                }

                string link = GetKeyValue(iniInfos, record, "link", @base);
                if (string.IsNullOrWhiteSpace(link)) continue;

                var name = GetKeyValue(iniInfos, record, "name", @base);
                if (string.IsNullOrWhiteSpace(name)) name = id;

                var description = GetKeyValue(iniInfos, record, "description", @base);
                if (string.IsNullOrWhiteSpace(description)) description = T._("Open" + " '" + link + "'");

                var category = GetKeyValue(iniInfos, record, "category", @base);
                if (string.IsNullOrWhiteSpace(category)) category = T._("Other");

                var foAdd = new LinkData
                {
                    Name = name,
                    Description = description,
                    Category = category,
                    Link = link
                };

                if (!dataForAdd.ContainsKey(id)) dataForAdd.Add(id, foAdd);
            }

            var dddd = "fghfghfgh";

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

        private static string GetKeyValue(LinksInfos iniInfos, Dictionary<string, string> record, string keyName, string parentId)
        {
            bool haveKey = record.ContainsKey(keyName);
            if (haveKey)
            {
                return CheckReplaceMarkers(iniInfos, record[keyName], keyName, parentId);
            }
            else if (!string.IsNullOrWhiteSpace(parentId))
            {
                return GetKeyValueFromBase(iniInfos, keyName, parentId);
            }

            return "";
        }

        private static string GetKeyValueFromBase(LinksInfos iniInfos, string keyName, string parentRecordId)
        {
            if (!iniInfos.ContainsKey(parentRecordId)) return "";

            var parentRecord = iniInfos[parentRecordId];

            var parentBase = parentRecord.ContainsKey("base") ? parentRecord["base"] : "";

            bool haveKey = parentRecord.ContainsKey(keyName);
            if (haveKey)
            {
                // have key and key value have no replcacer markers
                return CheckReplaceMarkers(iniInfos, parentRecord[keyName], keyName, parentBase);
            }
            else if (!string.IsNullOrWhiteSpace(parentBase))
            {
                // the record have no key name, search it in parent
                return GetKeyValueFromBase(iniInfos, keyName, parentBase);
            }

            return "";
        }

        private static string CheckReplaceMarkers(LinksInfos iniInfos, string s, string keyName, string baseRecordID)
        {
            if (!s.Contains("%")) return s;

            var matches = Regex.Matches(s, @"\%[^\%]+\%");
            var retS = s;
            foreach (Match match in matches)
            {
                if (match.Value == "%base%")
                {
                    string s1 = ReplaceBase(iniInfos, s, keyName, baseRecordID);
                    if (s1 == null) continue;

                    retS = s1;
                }
                else if (match.Value.Contains("/")) // %base/keyname%
                {
                    var basekey = match.Value.Trim('%').Split('/');
                    var theParentId = basekey[0];
                    var theKey = basekey[1];

                    string s1 = ReplaceBase(iniInfos, s, theKey, theParentId == "base" ? baseRecordID : theParentId);
                    if (s1 == null) continue;

                    retS = s1;
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
