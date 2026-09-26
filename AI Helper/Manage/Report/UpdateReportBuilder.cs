using System;
using System.Collections.Generic;
using System.Globalization;

namespace AIHelper.Manage.Report
{
    /// <summary>
    /// Builds the "mods update" report page out of the entries an update pass
    /// collected. It is the only place that knows the report is about updates,
    /// the renderer only knows about entries.
    /// </summary>
    internal static class UpdateReportBuilder
    {
        /// <summary>Body of a report of a pass that found nothing to update.</summary>
        private const string NoUpdatesBody = "<p class=\"notice\">";

        /// <summary>
        /// Renders the update report of the current game. An empty entry list
        /// produces the "no updates found" page instead of an empty section.
        /// </summary>
        internal static string BuildHtml(IReadOnlyList<ReportEntry> entries)
        {
            var languageTag = ManageSettings.LanuageID;
            var renderer = new ReportRenderer(ReportTemplates.Load(), languageTag);

            var gameName = ReportSettings.GameName;
            var hasEntries = entries != null && entries.Count > 0;

            var page = new ReportPage
            {
                LanguageTag = languageTag,
                Theme = ReportSettings.DefaultTheme,
                BackgroundImagePath = ReportSettings.BackgroundImagePath,
                PageTitle = ReportSettings.UpdateReportTitleText + " — " + gameName,
                EyebrowText = gameName,
                HeaderText = ReportSettings.UpdateReportTitleText,
                NoticeText = ReportSettings.NoticeText,
                SectionsHtml = hasEntries
                    ? renderer.RenderUpdateSections(
                        gameName,
                        DateTime.Now.ToString("g", CultureInfo.CurrentCulture),
                        entries)
                    : NoUpdatesBody + ReportText.HtmlEncode(ReportSettings.NoUpdatesFoundText) + "</p>"
            };

            return renderer.RenderPage(page);
        }
    }
}
