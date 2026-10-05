using System.Collections.Generic;
using AIHelper.Data.Modlist;
using AIHelper.Manage.Update;

namespace AIHelper.Manage.Functions.InfoEditor
{
    /// <summary>
    /// The github coordinates of one mod as the editor holds them while they are being
    /// edited, together with the captions the editor shows for them.
    /// </summary>
    internal sealed class GithubUpdateData : UpdateSourceData
    {
        /// <summary>
        /// Caption and explanation of every editable field, keyed by property name.
        /// </summary>
        /// <remarks>
        /// Built once for the whole type: the catalog does not change while the application
        /// runs, and one dictionary per mod used to be built instead.
        /// </remarks>
        private static readonly Dictionary<string, (string Caption, string Description)> FieldTexts =
            new Dictionary<string, (string Caption, string Description)>
            {
                { nameof(Owner), (T._("Owner"), T._("Repository owner")) },
                { nameof(Repository), (T._("Repository"), T._("Repository name")) },
                { nameof(FileStartsWith), (T._("File starts with"), T._("File to download starts with. Name part before version.")) },
                { nameof(FileEndsWith), (T._("File ends with"), T._("File to download ends with. Name part after version. Usually extension here")) },
                { nameof(VersionFromFile), (T._("Version from file"), T._("Determines if need to search version in file name.")) },
            };

        internal GithubUpdateData(ModData mod)
            : base(mod, GithubUpdateSource.Instance)
        {
        }

        public string Owner { get; set; } = "";

        public string Repository { get; set; } = "";

        public string FileStartsWith { get; set; } = "";

        public string FileEndsWith { get; set; } = "";

        public bool VersionFromFile { get; set; } = false;

        internal override string FilterText
        {
            get { return string.Join(" ", new[] { Owner, Repository, FileStartsWith, FileEndsWith }); }
        }

        internal override bool PointsAtSameFileAs(UpdateSourceData other)
        {
            // Data of another source can never be the same release file, so the comparison
            // is between two github records or it is no match at all.
            if (!(other is GithubUpdateData otherGithub)) return false;

            return ToRecord().PointsAtSameFileAs(otherGithub.ToRecord());
        }

        internal override bool TryGetFieldText(string propertyName, out string caption, out string description)
        {
            caption = null;
            description = null;

            (string Caption, string Description) text;
            if (!FieldTexts.TryGetValue(propertyName, out text)) return false;

            caption = text.Caption;
            description = text.Description;

            return true;
        }

        /// <summary>
        /// The coordinates in the shape this application stores them.
        /// </summary>
        internal GithubUpdateRecord ToRecord()
        {
            return new GithubUpdateRecord(Owner, Repository, FileStartsWith, FileEndsWith, VersionFromFile);
        }

        internal override void Write()
        {
            // A mod without meta.ini is one the editor could not have read either; it is
            // checked here so that a missing file cannot surface as an exception thrown from
            // a control's change event.
            var ini = Mod.MetaIni;
            if (ini == null) return;

            MetaIniUpdateRecordWriter.Write(ini, UpdateRecordParser.SourceHost, ToRecord());
        }
    }
}
