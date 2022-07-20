using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AIHelper.Manage
{
    internal static class ManageReport
    {
        private static readonly string _linksSeparator = "{{link}}";
        internal static void ShowReportFromLinks()
        {
            var groupNames = new Dictionary<string, string>();

            var langID = "<" + ManageSettings.LanuageID + ">";
            //langID = "<ru-RU>";

            string gameLinksPath = ManageSettings.LinksInfoFilePath;
            if (string.IsNullOrWhiteSpace(gameLinksPath)) return;

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

            string lastgroup = string.Empty;
            var linksInfo = new Dictionary<string, List<string[]>>();

            // add info
            foreach (var line in links)
            {
                var info = line.Split(new string[] { _linksSeparator }, StringSplitOptions.None);
                if (info.Length != 3) continue;

                var category = info[0];
                if (!linksInfo.ContainsKey(category)) linksInfo.Add(category, new List<string[]>());

                linksInfo[category].Add(new[] { info[1], info[2] });
            }

            // create html string
            var categoriesInfo = new List<string>();
            foreach (var category in linksInfo)
            {
                var descriptonLink = new List<string>();
                foreach (var link in category.Value)
                {
                    descriptonLink.Add(ManageSettings.UpdateReport.HtmlReportCategoryItemTemplate                        .Replace("%link%", link[1])
                        .Replace("%text%", new Uri(link[1]).Host.ToUpperInvariant())
                        .Replace("%description%", TryGetTranslation(link[0], langID))
                        );
                }

                categoriesInfo.Add(ManageSettings.UpdateReport.HtmlReportCategoryTemplate.Replace("%category%", groupNames.TryGetValue(category.Key)).Replace("%items%", string.Join("<br>", descriptonLink)));
            }

            // create new report contet
            var reportMessage = File.ReadAllText(ManageSettings.UpdateReport.ReportFilePath)
             .Replace(ManageSettings.UpdateReport.BgImageLinkPathPattern, ManageSettings.UpdateReport.CurrentGameBgFilePath)
             .Replace(ManageSettings.UpdateReport.ModsUpdateReportHeaderTextPattern, T._("Links"))
             .Replace(ManageSettings.UpdateReport.SingleModUpdateReportsTextSectionPattern, string.Join(ManageSettings.UpdateReport.HtmlBetweenModsText, categoriesInfo) + "<br>")
             .Replace(ManageSettings.UpdateReport.ModsUpdateInfoNoticePattern, ManageSettings.UpdateReport.ModsUpdateInfoNoticeText);

            var htmlfile = Path.Combine(ManageSettings.CurrentGameLinksInfoDirPath, ManageSettings.CurrentGame.GameAbbreviation + ".html");

            Directory.CreateDirectory(Path.GetDirectoryName(htmlfile));// fix missing parent directory error

            File.WriteAllText(htmlfile, reportMessage);
            using (var process = new System.Diagnostics.Process())
            {
                try
                {
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = htmlfile;
                    process.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
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
