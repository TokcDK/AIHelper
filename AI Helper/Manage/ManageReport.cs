using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AIHelper.Manage
{
    internal static class ManageReport
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private static readonly string _linksSeparator = "{{link}}";
        private static readonly string HeaderText = T._("Links");

        internal static void ShowReportFromLinks()
        {
            //langID = "<ru-RU>";
            var langID = "<" + ManageSettings.LanuageID + ">";

            var groupNames = new Dictionary<string, string>();

            var links = BuildLinks(langID, groupNames);
            if (links.Length == 0) return;

            // <CategoryName, <linkTitle,Url>>
            Dictionary<string, List<string[]>> linksInfo = BuildLinksInfo(links);

            if (!BuildAndOpenHtmlReport(linksInfo, langID, groupNames))
            {
                BuildAndOpenTxtReport(linksInfo);
            }

        }

        private static string[] BuildLinks(string langID, Dictionary<string, string> groupNames)
        {
            string gameLinksPath = ManageSettings.LinksInfoFilePath;
            if (string.IsNullOrWhiteSpace(gameLinksPath)) return Array.Empty<string>();

            string[] strings = File.ReadAllLines(gameLinksPath).Where(line => line.StartsWith(";##", StringComparison.InvariantCulture)).ToArray();
            foreach (var line in strings)
            {
                var en = Regex.Match(line, ";##([^<]+)");
                if (!en.Success) continue;

                var t = Regex.Match(line, langID + "([^<]+)");
                if (!t.Success) continue;
                if (groupNames.ContainsKey(en.Groups[1].Value)) continue;

                groupNames.Add(en.Groups[1].Value, t.Groups[1].Value);
            }

            string[] links = File.ReadAllLines(gameLinksPath).Where(line => !line.StartsWith(";", StringComparison.InvariantCulture)).ToArray();

            return links;
        }

        private static Dictionary<string, List<string[]>> BuildLinksInfo(string[] links)
        {
            Dictionary<string, List<string[]>> linksInfo = new Dictionary<string, List<string[]>>();

            // add info
            foreach (var line in links)
            {
                var info = line.Split(new string[] { _linksSeparator }, StringSplitOptions.None);
                if (info.Length != 3) continue;

                var category = info[0];
                if (!linksInfo.ContainsKey(category)) linksInfo.Add(category, new List<string[]>());

                linksInfo[category].Add(new[] { info[1], info[2] });
            }

            return linksInfo;
        }

        private static bool BuildAndOpenHtmlReport(Dictionary<string, List<string[]>> linksInfo, string langID, Dictionary<string, string> groupNames)
        {
            var categoriesInfo = new List<string>();
            foreach (var category in linksInfo)
            {
                var descriptonLink = new List<string>();
                int i = 0;
                foreach (var link in category.Value)
                {
                    if (link.Length != 2)
                    {
                        _log.Debug("Useful links report html: Invalid titleUrlData format. Must be Title-Url pair. Index:{0}, Length: {1}, Values: {2}", i, link.Length, string.Join(", ", link));
                    }

                    descriptonLink.Add(ManageSettings.UpdateReport.HtmlReportCategoryItemTemplate.Replace("%link%", link[1])
                        .Replace("%text%", new Uri(link[1]).Host.ToUpperInvariant())
                        .Replace("%description%", TryGetTranslation(link[0], langID))
                        );

                    i++;
                }

                categoriesInfo.Add(ManageSettings.UpdateReport.HtmlReportCategoryTemplate.Replace("%category%", groupNames.TryGetValue(category.Key)).Replace("%items%", string.Join("<br>", descriptonLink)));
            }

            // create new report contet
            var reportMessage = File.ReadAllText(ManageSettings.UpdateReport.ReportFilePath)
             .Replace(ManageSettings.UpdateReport.BgImageLinkPathPattern, ManageSettings.UpdateReport.CurrentGameBgFilePath)
             .Replace(ManageSettings.UpdateReport.ModsUpdateReportHeaderTextPattern, HeaderText)
             .Replace(ManageSettings.UpdateReport.SingleModUpdateReportsTextSectionPattern, string.Join(ManageSettings.UpdateReport.HtmlBetweenModsText, categoriesInfo) + "<br>")
             .Replace(ManageSettings.UpdateReport.ModsUpdateInfoNoticePattern, ManageSettings.UpdateReport.ModsUpdateInfoNoticeText);

            var htmlfile = Path.Combine(ManageSettings.CurrentGameLinksInfoDirPath, ManageSettings.CurrentGame.GameAbbreviation + ".html");

            Directory.CreateDirectory(Path.GetDirectoryName(htmlfile));// fix missing parent directory error

            File.WriteAllText(htmlfile, reportMessage);
            try
            {
                Process.Start(htmlfile);
            }
            catch (Exception e)
            {
                _log.Debug("Useful links report: Failed to open html. Error: {1}", e.Message);
                return false;
            }

            return true;
        }

        private static void BuildAndOpenTxtReport(Dictionary<string, List<string[]>> linksInfo)
        {
            var txt = new StringBuilder();
            txt.AppendLine(ManageSettings.CurrentGame.GameName);
            txt.AppendLine();
            txt.AppendLine();
            foreach (var category in linksInfo)
            {
                txt.AppendLine(category.Key + ":"); // add category name

                int i = 0;
                foreach (var titleUrlData in category.Value)
                {
                    if (titleUrlData.Length != 2)
                    {
                        _log.Debug("Useful links report txt: Invalid titleUrlData format. Must be Title-Url pair. Index:{0}, Length: {1}, Values: {2}", i, titleUrlData.Length, string.Join(", ", titleUrlData));
                    }
                    txt.AppendLine($"  {titleUrlData[0]}: {titleUrlData[1]}");

                    i++;
                }

                txt.AppendLine();
            }

            var txtReportFilePath = Path.Combine(ManageSettings.CurrentGameLinksInfoDirPath, ManageSettings.CurrentGame.GameAbbreviation + ".txt");

            File.WriteAllText(txtReportFilePath, txt.ToString());

            try
            {
                Process.Start(txtReportFilePath);
            }
            catch (Exception e)
            {
                _log.Debug("Useful links report: Failed to open txt. Error: {1}", e.Message);
            }
        }

        private static string TryGetValue(this Dictionary<string, string> dictionary, string key)
        {
            if (dictionary.TryGetValue(key, out string value)) return value;

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
