using System.IO;

namespace AIHelper.Manage.Report
{
    /// <summary>
    /// Everything the report pages are built from: the template files, the names
    /// of the markers inside them, the localized texts and the output locations.
    /// Keeping it together means a report only has to describe its own content,
    /// and a theme can be changed without touching any code.
    /// </summary>
    internal static class ReportSettings
    {
        #region Template files

        private const string PageTemplateFileName = "ReportTemplate.html";
        private const string SectionTemplateFileName = "ReportSectionTemplate.html";
        private const string LinkItemTemplateFileName = "ReportLinkItemTemplate.html";
        private const string UpdateItemTemplateFileName = "ReportUpdateItemTemplate.html";

        /// <summary>Overall page of both reports.</summary>
        internal static string PageTemplateFilePath => Path.Combine(ManageSettings.ReportDirPath, PageTemplateFileName);

        /// <summary>One collapsible section of a report.</summary>
        internal static string SectionTemplateFilePath => Path.Combine(ManageSettings.ReportDirPath, SectionTemplateFileName);

        /// <summary>One link card of the "useful links" report.</summary>
        internal static string LinkItemTemplateFilePath => Path.Combine(ManageSettings.ReportDirPath, LinkItemTemplateFileName);

        /// <summary>One entry of the "mods update" report.</summary>
        internal static string UpdateItemTemplateFilePath => Path.Combine(ManageSettings.ReportDirPath, UpdateItemTemplateFileName);

        #endregion

        #region Marker names

        // A marker is written in a template as a percent sign, its name and a
        // second percent sign, for example %link%.

        internal const string BackgroundImageMarker = "BGImageLinkPath";
        internal const string LanguageMarker = "ReportLanguage";
        internal const string ThemeMarker = "ReportTheme";
        internal const string PageTitleMarker = "ReportPageTitle";
        internal const string EyebrowMarker = "ReportEyebrow";
        internal const string HeaderTextMarker = "ReportHeaderText";
        internal const string NoticeMarker = "ReportNoticeText";
        internal const string SectionsMarker = "ReportSections";

        internal const string SectionIdMarker = "sectionId";
        internal const string SectionIconMarker = "icon";
        internal const string SectionTitleMarker = "category";
        internal const string SectionSubtitleMarker = "subtitle";
        internal const string SectionCountMarker = "count";
        internal const string SectionItemsMarker = "items";

        internal const string SearchMarker = "search";
        internal const string LinkDomainMarker = "domain";
        internal const string LinkInitialMarker = "initial";
        internal const string LinkUrlMarker = "link";
        internal const string LinkTitleMarker = "text";
        internal const string LinkDescriptionMarker = "description";
        internal const string LinkHostMarker = "host";

        internal const string UpdateStatusMarker = "status";
        internal const string UpdateIconMarker = "icon";
        internal const string UpdateNameMarker = "name";
        internal const string UpdateMessageMarker = "message";
        internal const string UpdateVersionMarker = "version";
        internal const string UpdateVisitUrlMarker = "visitUrl";
        internal const string UpdateVisitLabelMarker = "visitLabel";
        internal const string UpdateDetailsMarker = "details";

        #endregion

        #region Output files

        private const string UpdateReportHtmlFileName = "report.html";

        /// <summary>File the "mods update" report is written to.</summary>
        internal static string UpdateReportHtmlFilePath =>
            Path.Combine(ManageSettings.CurrentGameModsUpdateDir, UpdateReportHtmlFileName);

        /// <summary>
        /// File the "useful links" report is written to. It sits beside the links
        /// info files it was built from, like it always has.
        /// </summary>
        internal static string LinksReportHtmlFilePath =>
            Path.Combine(ManageSettings.CurrentGameLinksInfoDirPath, ManageSettings.CurrentGame.GameAbbreviation + ".html");

        /// <summary>Plain text fallback of the "useful links" report.</summary>
        internal static string LinksReportTextFilePath =>
            Path.Combine(ManageSettings.CurrentGameLinksInfoDirPath, ManageSettings.CurrentGame.GameAbbreviation + ".txt");

        #endregion

        #region Page values

        /// <summary>Theme a freshly written report starts in.</summary>
        internal const string DefaultTheme = "dark";

        /// <summary>
        /// Wallpaper of the current game as a forward slash path, because the
        /// report markup uses it inside a css url().
        /// </summary>
        internal static string BackgroundImagePath =>
            Path.Combine(ManageSettings.ReportBGDirPath, ManageSettings.CurrentGame.GameAbbreviation + ".jpg")
                .Replace(Path.DirectorySeparatorChar.ToString(), "/");

        /// <summary>Name of the current game, used as the eyebrow of both reports.</summary>
        internal static string GameName => ManageSettings.CurrentGame.GameName;

        #endregion

        #region Localized texts

        /// <summary>Header of the "useful links" report.</summary>
        internal static string LinksReportTitleText => T._("Links");

        /// <summary>Header of the "mods update" report.</summary>
        internal static string UpdateReportTitleText => T._("Update report");

        /// <summary>Hint shown under the header of both reports.</summary>
        internal static string NoticeText => T._("Click on line for more info");

        /// <summary>Shown when an update pass found nothing to update.</summary>
        internal static string NoUpdatesFoundText => T._("No updates found");

        /// <summary>Label of the release page link of an update entry.</summary>
        internal static string VisitLinkText => T._("Website") + ": " + T._("click");

        /// <summary>Message of a folder that was updated.</summary>
        internal static string UpdatedToVersionText => T._("updated to version");

        /// <summary>Message of a folder with a newer version but no file for it.</summary>
        internal static string NoUpdateFileText => T._("have new version but file for update not found");

        /// <summary>Message of a folder that could not be updated.</summary>
        internal static string FailedToUpdateText => T._("Failed to update");

        /// <summary>Prefix of the error detail line of a failed update.</summary>
        internal static string DetailsInText => T._("Details in");

        #endregion
    }
}
