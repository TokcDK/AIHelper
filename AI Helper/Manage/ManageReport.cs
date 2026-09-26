using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AIHelper.Manage.Report;
using NLog;

namespace AIHelper.Manage
{
    /// <summary>
    /// The "useful links" report of the current game: the links are read from the
    /// per game links info file, rendered through the theme templates and opened
    /// in the default application for html. When the page cannot be produced the
    /// links are written as plain text instead, so they are never lost.
    /// <para>
    /// The work itself is delegated: <see cref="ReportLinkParser"/> reads the info
    /// file, <see cref="ReportRenderer"/> fills the templates and
    /// <see cref="ReportPublisher"/> writes and opens the result.
    /// </para>
    /// </summary>
    internal static class ManageReport
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        internal static void ShowReportFromLinks()
        {
            var linksInfoFilePath = ManageSettings.LinksInfoFilePath;
            if (string.IsNullOrWhiteSpace(linksInfoFilePath)) return;

            var languageTag = ManageSettings.LanuageID;

            var categories = ReadCategories(linksInfoFilePath, languageTag);
            if (categories.Count == 0) return;

            if (!PublishHtmlReport(categories, languageTag))
            {
                PublishTextReport(categories);
            }
        }

        /// <summary>
        /// Reads and parses the links info file. A file that cannot be read is
        /// logged and treated as empty.
        /// </summary>
        private static List<ReportLinkCategory> ReadCategories(string linksInfoFilePath, string languageTag)
        {
            try
            {
                return ReportLinkParser.Parse(File.ReadAllLines(linksInfoFilePath), languageTag);
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Report: failed to read the links info file '" + linksInfoFilePath + "'.");
                return new List<ReportLinkCategory>();
            }
        }

        /// <summary>Renders the links into the report page and opens it.</summary>
        /// <returns>True when the page was written and handed to the shell.</returns>
        private static bool PublishHtmlReport(IReadOnlyList<ReportLinkCategory> categories, string languageTag)
        {
            var renderer = new ReportRenderer(ReportTemplates.Load(), languageTag);
            var gameName = ReportSettings.GameName;

            var page = new ReportPage
            {
                LanguageTag = languageTag,
                Theme = ReportSettings.DefaultTheme,
                BackgroundImagePath = ReportSettings.BackgroundImagePath,
                PageTitle = ReportSettings.LinksReportTitleText + " — " + gameName,
                EyebrowText = gameName,
                HeaderText = ReportSettings.LinksReportTitleText,
                NoticeText = ReportSettings.NoticeText,
                SectionsHtml = renderer.RenderLinkSections(categories)
            };

            return ReportPublisher.PublishAndOpen(
                ReportSettings.LinksReportHtmlFilePath,
                renderer.RenderPage(page));
        }

        /// <summary>
        /// Writes the links as plain text, used when the report page cannot be
        /// produced.
        /// </summary>
        private static void PublishTextReport(IReadOnlyList<ReportLinkCategory> categories)
        {
            var text = new StringBuilder();
            text.AppendLine(ManageSettings.CurrentGame.GameName);
            text.AppendLine();
            text.AppendLine();

            foreach (var category in categories)
            {
                text.AppendLine(category.Key + ":"); // group name as written in the info file

                foreach (var link in category.Links)
                {
                    text.AppendLine("  " + link.RawTitle + ": " + link.Url);
                }

                text.AppendLine();
            }

            ReportPublisher.PublishAndOpen(ReportSettings.LinksReportTextFilePath, text.ToString());
        }
    }
}
