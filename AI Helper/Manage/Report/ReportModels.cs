using System.Collections.Generic;

namespace AIHelper.Manage.Report
{
    /// <summary>
    /// One link of the "useful links" report, as written in the per game links
    /// info file.
    /// </summary>
    internal class ReportLink
    {
        internal ReportLink(string rawTitle, string url)
        {
            RawTitle = rawTitle ?? string.Empty;
            Url = url ?? string.Empty;
        }

        /// <summary>
        /// Title exactly as it appears in the info file. It may carry per
        /// language variants behind a "&lt;language&gt;" marker.
        /// </summary>
        internal string RawTitle { get; }

        /// <summary>Address the link points at.</summary>
        internal string Url { get; }
    }

    /// <summary>
    /// A named group of links. Every category becomes one collapsible section
    /// of the report.
    /// </summary>
    internal class ReportLinkCategory
    {
        internal ReportLinkCategory(string key, string title)
        {
            Key = key ?? string.Empty;
            Title = title ?? string.Empty;
            Links = new List<ReportLink>();
        }

        /// <summary>Group name as written in the info file, for example "Characters".</summary>
        internal string Key { get; }

        /// <summary>Localized group name, or <see cref="Key"/> when there is no translation.</summary>
        internal string Title { get; }

        /// <summary>Links of this group, in file order.</summary>
        internal List<ReportLink> Links { get; }
    }

    /// <summary>Outcome of one folder of an update pass.</summary>
    internal enum ReportEntryStatus
    {
        /// <summary>The folder was updated to a newer version.</summary>
        Success,

        /// <summary>A newer version exists but no file for it was found.</summary>
        Warning,

        /// <summary>The update failed.</summary>
        Failed
    }

    /// <summary>
    /// One entry of the "mods update" report. The updater only collects the
    /// facts here, the markup is added later by <see cref="ReportRenderer"/>.
    /// </summary>
    internal class ReportEntry
    {
        internal ReportEntry(ReportEntryStatus status, string name, string message)
        {
            Status = status;
            Name = name ?? string.Empty;
            Message = message ?? string.Empty;
            CurrentVersion = string.Empty;
            TargetVersion = string.Empty;
            Details = string.Empty;
            SourceLink = string.Empty;
        }

        internal ReportEntryStatus Status { get; }

        /// <summary>Folder name of the mod.</summary>
        internal string Name { get; }

        /// <summary>Localized description of what happened, without the versions.</summary>
        internal string Message { get; }

        /// <summary>Version that was installed before the update, may be empty.</summary>
        internal string CurrentVersion { get; set; }

        /// <summary>Version the folder was updated to, may be empty.</summary>
        internal string TargetVersion { get; set; }

        /// <summary>Extra lines of an entry, currently the error text of a failure.</summary>
        internal string Details { get; set; }

        /// <summary>Release page of the mod, may be empty.</summary>
        internal string SourceLink { get; set; }
    }

    /// <summary>
    /// Everything that is the same for every report page, whatever the report
    /// is about. The body is passed in already rendered.
    /// </summary>
    internal class ReportPage
    {
        internal ReportPage()
        {
            LanguageTag = "en-US";
            Theme = "dark";
            BackgroundImagePath = string.Empty;
            PageTitle = string.Empty;
            EyebrowText = string.Empty;
            HeaderText = string.Empty;
            NoticeText = string.Empty;
            SectionsHtml = string.Empty;
        }

        /// <summary>Value of the lang attribute, for example "ru-RU".</summary>
        internal string LanguageTag { get; set; }

        /// <summary>Value of the initial data-theme attribute.</summary>
        internal string Theme { get; set; }

        /// <summary>Wallpaper of the current game, as a forward slash path.</summary>
        internal string BackgroundImagePath { get; set; }

        /// <summary>Document title.</summary>
        internal string PageTitle { get; set; }

        /// <summary>Small label shown above the header.</summary>
        internal string EyebrowText { get; set; }

        /// <summary>Main header of the page.</summary>
        internal string HeaderText { get; set; }

        /// <summary>Hint shown under the header.</summary>
        internal string NoticeText { get; set; }

        /// <summary>Rendered body of the report.</summary>
        internal string SectionsHtml { get; set; }
    }
}
