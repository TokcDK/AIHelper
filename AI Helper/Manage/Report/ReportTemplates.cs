using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using NLog;

namespace AIHelper.Manage.Report
{
    /// <summary>
    /// The values that are substituted into one report template. A marker is
    /// written in a template as a percent sign, its name and a second percent
    /// sign, for example %link%.
    /// </summary>
    internal class ReportMarkers
    {
        private static readonly Regex MarkerPattern = new Regex("%([A-Za-z0-9_]+)%", RegexOptions.Compiled);

        /// <summary>Html comments, which a template may use to document itself.</summary>
        private static readonly Regex CommentPattern =
            new Regex("<!--.*?-->", RegexOptions.Compiled | RegexOptions.Singleline);

        /// <summary>The stand in that keeps a comment out of the way while filling.</summary>
        private static readonly Regex CommentPlaceholderPattern =
            new Regex("\u0001([0-9]+)\u0001", RegexOptions.Compiled);

        private readonly Dictionary<string, string> _values = new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>
        /// Adds or replaces the value of one marker. The name is given without
        /// the percent signs.
        /// </summary>
        internal ReportMarkers Set(string markerName, string value)
        {
            _values[markerName] = value ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Replaces every known marker in a single pass, so a value that happens
        /// to contain a percent sign can never be mistaken for a marker.
        /// Markers without a value are left in place, which keeps a theme that
        /// drops one from breaking the report.
        /// <para>
        /// Markers inside an html comment are never replaced. A template can
        /// therefore list its own markers in a comment without that list being
        /// filled in and duplicating the content those markers stand for.
        /// </para>
        /// </summary>
        internal string Apply(string template)
        {
            if (string.IsNullOrEmpty(template)) return string.Empty;

            var comments = new List<string>();

            var withoutComments = CommentPattern.Replace(template, match =>
            {
                comments.Add(match.Value);
                return "\u0001" + (comments.Count - 1).ToString(CultureInfo.InvariantCulture) + "\u0001";
            });

            var filled = MarkerPattern.Replace(withoutComments, match =>
            {
                string value;
                return _values.TryGetValue(match.Groups[1].Value, out value) ? value : match.Value;
            });

            if (comments.Count == 0) return filled;

            return CommentPlaceholderPattern.Replace(filled, match =>
                comments[int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture)]);
        }
    }

    /// <summary>One report template file and where it came from.</summary>
    internal class ReportTemplate
    {
        private readonly string _content;

        internal ReportTemplate(string content, string sourcePath)
        {
            _content = content ?? string.Empty;
            SourcePath = sourcePath ?? string.Empty;
        }

        /// <summary>Path the template was read from, empty for the built-in fallback.</summary>
        internal string SourcePath { get; }

        /// <summary>Returns the template with every known marker filled in.</summary>
        internal string Fill(ReportMarkers markers)
        {
            return markers.Apply(_content);
        }
    }

    /// <summary>
    /// The set of templates a report is built from. A template file that is
    /// missing, unreadable or stripped of its main marker falls back to a
    /// minimal built-in layout, so a broken theme never stops the report.
    /// </summary>
    internal class ReportTemplates
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private ReportTemplates(ReportTemplate page, ReportTemplate section, ReportTemplate linkItem, ReportTemplate updateItem)
        {
            Page = page;
            Section = section;
            LinkItem = linkItem;
            UpdateItem = updateItem;
        }

        internal ReportTemplate Page { get; }

        internal ReportTemplate Section { get; }

        internal ReportTemplate LinkItem { get; }

        internal ReportTemplate UpdateItem { get; }

        /// <summary>Loads the templates of the current theme.</summary>
        internal static ReportTemplates Load()
        {
            return new ReportTemplates(
                LoadTemplate(ReportSettings.PageTemplateFilePath, ReportSettings.SectionsMarker, FallbackPage),
                LoadTemplate(ReportSettings.SectionTemplateFilePath, ReportSettings.SectionItemsMarker, FallbackSection),
                LoadTemplate(ReportSettings.LinkItemTemplateFilePath, ReportSettings.LinkUrlMarker, FallbackLinkItem),
                LoadTemplate(ReportSettings.UpdateItemTemplateFilePath, ReportSettings.UpdateNameMarker, FallbackUpdateItem));
        }

        /// <summary>
        /// Reads one template file. The built-in fallback is used when the file
        /// is gone, cannot be read, or no longer contains its main marker.
        /// </summary>
        private static ReportTemplate LoadTemplate(string filePath, string requiredMarker, string fallback)
        {
            if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
            {
                try
                {
                    var content = File.ReadAllText(filePath);

                    if (content.IndexOf("%" + requiredMarker + "%", StringComparison.Ordinal) >= 0)
                    {
                        return new ReportTemplate(content, filePath);
                    }

                    _log.Warn("Report: template '" + filePath + "' has no %" + requiredMarker
                              + "% marker, the built-in layout is used instead.");
                }
                catch (Exception ex)
                {
                    _log.Warn(ex, "Report: failed to read the template '" + filePath + "'.");
                }
            }

            return new ReportTemplate(fallback, string.Empty);
        }

        // The fallbacks are deliberately plain: they only keep the report usable
        // when the theme files are missing, they are not meant to be pretty.
        private const string FallbackPage =
            "<!DOCTYPE html><html lang=\"%ReportLanguage%\"><head><meta charset=\"utf-8\">"
            + "<title>%ReportPageTitle%</title></head>"
            + "<body style=\"background:#111;color:#eee;font-family:Segoe UI,sans-serif\">"
            + "<h1>%ReportHeaderText%</h1><p>%ReportNoticeText%</p>%ReportSections%</body></html>";

        private const string FallbackSection =
            "<details open><summary><b>%category%</b> (%count%)</summary>%items%</details>";

        private const string FallbackLinkItem =
            "<p><a href=\"%link%\" target=\"_blank\" rel=\"noopener noreferrer\">%text%</a> - %description% (%host%)</p>";

        private const string FallbackUpdateItem =
            "<p><b>%name%</b> - %message% %version% %details%</p>";
    }
}
