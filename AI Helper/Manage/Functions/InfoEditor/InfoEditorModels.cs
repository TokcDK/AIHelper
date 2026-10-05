using System.Collections.Generic;
using AIHelper.Data.Modlist;
using AIHelper.Manage.Update;
using INIFileMan;

namespace AIHelper.Manage.Functions.InfoEditor
{
    /// <summary>
    /// One mod shown in the mods update info editor: the mod itself together with the
    /// github coordinates the editor edits for it.
    /// </summary>
    /// <remarks>
    /// The properties a field is bound to are public because the binding resolves them
    /// through <see cref="System.ComponentModel.TypeDescriptor"/>, which only sees public
    /// members. The type itself stays internal: nothing outside this feature uses it.
    /// </remarks>
    internal sealed class UpdateInfoData
    {
        internal UpdateInfoData(ModData mod)
            : this(mod, new GitUpdateInfoData(mod))
        {
        }

        /// <param name="gitInfo">
        /// Coordinates to edit. Passed in when a mod is added, so that the coordinates which
        /// were just looked up are the ones the new block shows.
        /// </param>
        internal UpdateInfoData(ModData mod, GitUpdateInfoData gitInfo)
        {
            Mod = mod;
            GitInfo = gitInfo;
        }

        public ModData Mod { get; }

        public string ModName => Mod.Name;

        /// <summary>
        /// Coordinates the block of this mod edits. A binding reaches them by this name,
        /// which is why it is public.
        /// </summary>
        public GitUpdateInfoData GitInfo { get; }
    }

    /// <summary>
    /// The github coordinates of one mod as the editor holds them while they are being
    /// edited, together with the captions the editor shows for them.
    /// </summary>
    internal sealed class GitUpdateInfoData
    {
        /// <summary>
        /// Host every record of the editor points at. The editor edits one source, and the
        /// host is what identifies it, so it is not an editable field.
        /// </summary>
        public string Site { get; } = UpdateRecordParser.SourceHost;

        public string Owner { get; set; } = "";

        public string Repository { get; set; } = "";

        public string FileStartsWith { get; set; } = "";

        public string FileEndsWith { get; set; } = "";

        public bool VersionFromFile { get; set; } = false;

        /// <summary>
        /// Caption and explanation of every editable field, keyed by property name.
        /// <para>
        /// Built once for the whole type: the catalog does not change while the application
        /// runs, and one dictionary per mod used to be built instead.
        /// </para>
        /// </summary>
        private static readonly Dictionary<string, (string Caption, string Description)> FieldTexts =
            new Dictionary<string, (string Caption, string Description)>
            {
                { nameof(Owner), (T._("Owner"), T._("Repository owner")) },
                { nameof(Repository), (T._("Repository"), T._("Repository name")) },
                { nameof(FileStartsWith), (T._("File starts with"), T._("File to download starts with. Name part before version.")) },
                { nameof(FileEndsWith), (T._("File ends with"), T._("File to download ends with. Name part after version. Usually extension here")) },
                { nameof(VersionFromFile), (T._("Version from file"), T._("Determines if need to search version in file name.")) },
            };

        internal GitUpdateInfoData(ModData mod)
        {
            Mod = mod;
        }

        public ModData Mod { get; }

        public INIFile INI => Mod.MetaIni;

        /// <summary>
        /// Caption and explanation of a field, when the editor has one for it.
        /// </summary>
        /// <param name="propertyName">Name of the field's property.</param>
        /// <param name="caption">Caption to show, or null.</param>
        /// <param name="description">Explanation to show as a tooltip, or null.</param>
        /// <returns>True when the field is one the editor knows how to label.</returns>
        internal static bool TryGetFieldText(string propertyName, out string caption, out string description)
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
        public GithubUpdateRecord ToRecord()
        {
            return new GithubUpdateRecord(Owner, Repository, FileStartsWith, FileEndsWith, VersionFromFile);
        }

        /// <summary>
        /// Stores the coordinates in the mod's meta.ini.
        /// </summary>
        public void Write()
        {
            // A mod without meta.ini is one the editor could not have read either; it is
            // checked here so that a missing file cannot surface as an exception thrown from
            // a control's change event.
            if (INI == null) return;

            MetaIniUpdateRecordWriter.Write(INI, Site, ToRecord());
        }
    }
}
