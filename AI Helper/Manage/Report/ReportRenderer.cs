using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AIHelper.Manage.Report
{
    /// <summary>
    /// Turns report models into the final markup by filling the templates. It
    /// knows nothing about files, settings or the current game, so the whole
    /// presentation can be reasoned about on its own.
    /// </summary>
    internal class ReportRenderer
    {
        private readonly ReportTemplates _templates;
        private readonly string _languageTag;

        internal ReportRenderer(ReportTemplates templates, string languageTag)
        {
            if (templates == null) throw new ArgumentNullException(nameof(templates));

            _templates = templates;
            _languageTag = languageTag ?? string.Empty;
        }

        /// <summary>Fills the page shell around an already rendered body.</summary>
        internal string RenderPage(ReportPage page)
        {
            if (page == null) throw new ArgumentNullException(nameof(page));

            var markers = new ReportMarkers()
                .Set(ReportSettings.LanguageMarker, ReportText.HtmlEncode(page.LanguageTag))
                .Set(ReportSettings.ThemeMarker, ReportText.HtmlEncode(page.Theme))
                .Set(ReportSettings.PageTitleMarker, ReportText.HtmlEncode(page.PageTitle))
                .Set(ReportSettings.BackgroundImageMarker, ReportText.HtmlEncode(page.BackgroundImagePath))
                .Set(ReportSettings.EyebrowMarker, ReportText.HtmlEncode(page.EyebrowText))
                .Set(ReportSettings.HeaderTextMarker, ReportText.HtmlEncode(page.HeaderText))
                .Set(ReportSettings.NoticeMarker, ReportText.HtmlEncode(page.NoticeText))
                .Set(ReportSettings.SectionsMarker, page.SectionsHtml);

            return _templates.Page.Fill(markers);
        }

        /// <summary>Renders the link categories of the "useful links" report.</summary>
        internal string RenderLinkSections(IReadOnlyList<ReportLinkCategory> categories)
        {
            var sections = new StringBuilder();

            if (categories == null) return sections.ToString();

            foreach (var category in categories)
            {
                var items = new StringBuilder();
                foreach (var link in category.Links)
                {
                    items.AppendLine(RenderLinkItem(link));
                }

                sections.AppendLine(RenderSection(
                    category.Key,
                    ReportIcons.Link,
                    category.Title,
                    string.Empty,
                    category.Links.Count,
                    items.ToString()));
            }

            return sections.ToString();
        }

        /// <summary>Renders the entries of the "mods update" report as one section.</summary>
        internal string RenderUpdateSections(string title, string subtitle, IReadOnlyList<ReportEntry> entries)
        {
            var sections = new StringBuilder();

            if (entries == null || entries.Count == 0) return sections.ToString();

            var items = new StringBuilder();
            foreach (var entry in entries)
            {
                items.AppendLine(RenderUpdateItem(entry));
            }

            sections.AppendLine(RenderSection(
                title,
                ReportIcons.Update,
                title,
                subtitle,
                entries.Count,
                items.ToString()));

            return sections.ToString();
        }

        /// <summary>Renders one link card.</summary>
        internal string RenderLinkItem(ReportLink link)
        {
            if (link == null) throw new ArgumentNullException(nameof(link));

            var host = ReportText.GetHostName(link.Url);
            var title = host.ToUpperInvariant();
            var description = ReportText.SelectLanguageVariant(link.RawTitle, _languageTag);

            var markers = new ReportMarkers()
                .Set(ReportSettings.SearchMarker, ReportText.HtmlEncode(BuildSearchText(title, description, host, link.Url)))
                .Set(ReportSettings.LinkDomainMarker, ReportText.HtmlEncode(Uri.EscapeDataString(host)))
                .Set(ReportSettings.LinkInitialMarker, ReportText.HtmlEncode(GetInitial(host)))
                .Set(ReportSettings.LinkUrlMarker, ReportText.HtmlEncode(link.Url))
                .Set(ReportSettings.LinkTitleMarker, ReportText.HtmlEncode(title))
                .Set(ReportSettings.LinkDescriptionMarker, ReportText.HtmlEncode(description))
                .Set(ReportSettings.LinkHostMarker, ReportText.HtmlEncode(host));

            return _templates.LinkItem.Fill(markers);
        }

        /// <summary>Renders one entry of the "mods update" report.</summary>
        internal string RenderUpdateItem(ReportEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            var markers = new ReportMarkers()
                .Set(ReportSettings.UpdateStatusMarker, GetStatusClass(entry.Status))
                .Set(ReportSettings.UpdateIconMarker, GetStatusIcon(entry.Status))
                .Set(ReportSettings.SearchMarker, ReportText.HtmlEncode(BuildSearchText(
                    entry.Name, entry.Message, entry.CurrentVersion, entry.TargetVersion)))
                .Set(ReportSettings.UpdateNameMarker, ReportText.HtmlEncode(entry.Name))
                .Set(ReportSettings.UpdateMessageMarker, ReportText.HtmlEncode(entry.Message))
                .Set(ReportSettings.UpdateVersionMarker, ReportText.HtmlEncode(GetVersionText(entry)))
                .Set(ReportSettings.UpdateVisitUrlMarker, ReportText.HtmlEncode(entry.SourceLink))
                .Set(ReportSettings.UpdateVisitLabelMarker, ReportText.HtmlEncode(ReportSettings.VisitLinkText))
                .Set(ReportSettings.UpdateDetailsMarker, ReportText.HtmlEncode(entry.Details));

            return _templates.UpdateItem.Fill(markers);
        }

        /// <summary>
        /// Renders one collapsible section. The icon is markup and is passed
        /// through unchanged, every other value is escaped first.
        /// </summary>
        private string RenderSection(string sectionId, string icon, string title, string subtitle, int itemCount, string itemsHtml)
        {
            var markers = new ReportMarkers()
                .Set(ReportSettings.SectionIdMarker, ReportText.HtmlEncode(ReportText.ToSlug(sectionId)))
                .Set(ReportSettings.SectionIconMarker, icon)
                .Set(ReportSettings.SectionTitleMarker, ReportText.HtmlEncode(title))
                .Set(ReportSettings.SectionSubtitleMarker, ReportText.HtmlEncode(subtitle))
                .Set(ReportSettings.SectionCountMarker, itemCount.ToString(CultureInfo.InvariantCulture))
                .Set(ReportSettings.SectionItemsMarker, itemsHtml);

            return _templates.Section.Fill(markers);
        }

        /// <summary>
        /// Text a card is matched against by the search of the page. Lower case,
        /// because the script compares it with a lower cased search term.
        /// </summary>
        private static string BuildSearchText(params string[] parts)
        {
#pragma warning disable CA1308 // Normalize strings to uppercase - the page search lower cases the term
            return string.Join(" ", parts).ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
        }

        /// <summary>First letter of a host, used by the favicon fallback of a card.</summary>
        private static string GetInitial(string host)
        {
            return string.IsNullOrEmpty(host) ? "?" : host.Substring(0, 1).ToUpperInvariant();
        }

        /// <summary>Version transition of an entry, empty when no version is known.</summary>
        private static string GetVersionText(ReportEntry entry)
        {
            if (string.IsNullOrWhiteSpace(entry.TargetVersion)) return string.Empty;
            if (string.IsNullOrWhiteSpace(entry.CurrentVersion)) return entry.TargetVersion;

            return entry.CurrentVersion + " \u2192 " + entry.TargetVersion;
        }

        /// <summary>Stylesheet modifier of an entry card, driven by the status.</summary>
        private static string GetStatusClass(ReportEntryStatus status)
        {
            switch (status)
            {
                case ReportEntryStatus.Success: return "ok";
                case ReportEntryStatus.Warning: return "warn";
                default: return "err";
            }
        }

        /// <summary>Symbol shown in front of an entry, driven by the status.</summary>
        private static string GetStatusIcon(ReportEntryStatus status)
        {
            switch (status)
            {
                case ReportEntryStatus.Success: return "\u2714";
                case ReportEntryStatus.Warning: return "!";
                default: return "\u2716";
            }
        }

        /// <summary>
        /// Inline icons of the report sections. They are markup rather than
        /// escaped text, so they live next to the renderer that places them.
        /// </summary>
        private static class ReportIcons
        {
            internal const string Link =
                "<svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\">"
                + "<rect x=\"3\" y=\"5\" width=\"12\" height=\"15\" rx=\"2\"/><path d=\"M8 9h2M8 13h4\"/>"
                + "<path d=\"M17 7.5 20.4 8a1.6 1.6 0 0 1 1.3 1.9l-1.8 8.4\"/></svg>";

            internal const string Update =
                "<svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\">"
                + "<path d=\"M12 3v12\"/><path d=\"m7 10 5 5 5-5\"/><path d=\"M5 21h14\"/></svg>";
        }
    }
}
