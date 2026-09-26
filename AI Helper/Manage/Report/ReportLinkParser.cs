using System;
using System.Collections.Generic;
using NLog;

namespace AIHelper.Manage.Report
{
    /// <summary>
    /// Reads the links info file of a game into report models. The format is one
    /// link per line, the group a link belongs to comes first:
    /// <code>
    /// ;##Group&lt;ru-RU&gt;Translated group
    /// Group{{link}}Title&lt;ru-RU&gt;Translated title{{link}}https://example.org
    /// </code>
    /// Lines starting with ";" are comments and are ignored, so the header of the
    /// file can document the format without being reported.
    /// </summary>
    internal static class ReportLinkParser
    {
        /// <summary>Separates the three fields of one link line.</summary>
        internal const string FieldSeparator = "{{link}}";

        private const string CommentPrefix = ";";
        private const string GroupLinePrefix = ";##";
        private const int LinkFieldCount = 3;

        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Parses the lines of a links info file into the categories they
        /// describe, keeping the order the file lists them in.
        /// </summary>
        /// <param name="lines">Lines of a links info file.</param>
        /// <param name="languageTag">Language to prefer for the group names.</param>
        internal static List<ReportLinkCategory> Parse(IEnumerable<string> lines, string languageTag)
        {
            var categories = new List<ReportLinkCategory>();

            if (lines == null) return categories;

            var fileLines = lines as IList<string> ?? new List<string>(lines);
            var groupTitles = ParseGroupTitles(fileLines, languageTag);
            var categoriesByKey = new Dictionary<string, ReportLinkCategory>(StringComparer.Ordinal);

            foreach (var line in fileLines)
            {
                string categoryKey;
                ReportLink link;

                if (!TryParseLinkLine(line, out categoryKey, out link)) continue;

                ReportLinkCategory category;
                if (!categoriesByKey.TryGetValue(categoryKey, out category))
                {
                    string title;
                    category = new ReportLinkCategory(
                        categoryKey,
                        groupTitles.TryGetValue(categoryKey, out title) ? title : categoryKey);

                    categoriesByKey.Add(categoryKey, category);
                    categories.Add(category);
                }

                category.Links.Add(link);
            }

            return categories;
        }

        /// <summary>
        /// Reads the display name of every group the file declares, preferring the
        /// variant of <paramref name="languageTag"/>.
        /// </summary>
        private static Dictionary<string, string> ParseGroupTitles(IEnumerable<string> lines, string languageTag)
        {
            var titles = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var line in lines)
            {
                if (line == null || !line.StartsWith(GroupLinePrefix, StringComparison.Ordinal)) continue;

                var declaration = line.Substring(GroupLinePrefix.Length);
                var keyEnd = declaration.IndexOf('<');

                // a group without a name cannot be matched to a link, so it is skipped
                if (keyEnd == 0) continue;

                var key = keyEnd > 0 ? declaration.Substring(0, keyEnd) : declaration;
                if (key.Length == 0 || titles.ContainsKey(key)) continue;

                titles.Add(key, ReportText.SelectLanguageVariant(declaration, languageTag));
            }

            return titles;
        }

        /// <summary>
        /// Splits one line into its category, title and url. Comment lines and
        /// lines that do not hold exactly three fields are skipped.
        /// </summary>
        private static bool TryParseLinkLine(string line, out string categoryKey, out ReportLink link)
        {
            categoryKey = null;
            link = null;

            if (line == null) return false;

            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith(CommentPrefix, StringComparison.Ordinal)) return false;

            var fields = trimmed.Split(new[] { FieldSeparator }, StringSplitOptions.None);
            if (fields.Length != LinkFieldCount)
            {
                _log.Debug("Report: skipped the links info line '" + trimmed + "'. "
                           + LinkFieldCount + " fields separated by " + FieldSeparator + " are required.");
                return false;
            }

            categoryKey = fields[0].Trim();
            if (categoryKey.Length == 0) return false;

            link = new ReportLink(fields[1].Trim(), fields[2].Trim());
            return true;
        }
    }
}
