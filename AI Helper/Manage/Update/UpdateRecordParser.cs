using System;
using System.Text.RegularExpressions;

namespace AIHelper.Manage.Update
{
    /// <summary>
    /// Reads and writes the update record this application stores in meta.ini and
    /// updateinfo.txt.
    /// <para>
    /// The stored shape is
    /// <c>updgit::owner,repository,fileStartsWith,fileEndsWith,versionFromFile::</c>.
    /// Only the first two fields are required, so a record written by hand — or one
    /// left over from an older version of the application — still parses.
    /// </para>
    /// </summary>
    internal static class UpdateRecordParser
    {
        /// <summary>Source id that prefixes every stored record.</summary>
        internal const string SourceId = "updgit";

        /// <summary>Host the stored records point at.</summary>
        internal const string SourceHost = "github.com";

        /// <summary>Separator that opens and closes a stored record.</summary>
        private const string Wrapper = "::";

        /// <summary>Separator between the fields of a record.</summary>
        private static readonly char[] FieldSeparators = { ',' };

        /// <summary>
        /// Pattern of a whole record inside a larger text. The notes of meta.ini are
        /// free-form text the user may have wrapped the record in, so the record has
        /// to be found rather than assumed to be the whole value.
        /// </summary>
        private static readonly Regex RecordRegex = new Regex(
            Regex.Escape(SourceId) + @"::(.+?)::",
            RegexOptions.Compiled | RegexOptions.Singleline,
            TimeSpan.FromSeconds(1));

        /// <summary>
        /// Finds a record inside a text that may contain one.
        /// </summary>
        /// <param name="text">Text to search, for example the notes of meta.ini.</param>
        /// <param name="record">Record that was found.</param>
        /// <returns>True when the text contained a parsable record.</returns>
        internal static bool TryExtract(string text, out GithubUpdateRecord record)
        {
            record = null;

            if (string.IsNullOrWhiteSpace(text)) return false;

            var match = RecordRegex.Match(text);
            if (!match.Success) return false;

            return TryParseFields(match.Groups[1].Value, out record);
        }

        /// <summary>
        /// Parses a value that is expected to be a whole stored record, as written
        /// in updateinfo.txt. A missing wrapper is tolerated.
        /// </summary>
        /// <param name="storedValue">Value read from the file.</param>
        /// <param name="record">Record that was parsed.</param>
        /// <returns>True when the value was a parsable record.</returns>
        internal static bool TryParseStoredValue(string storedValue, out GithubUpdateRecord record)
        {
            record = null;

            if (string.IsNullOrWhiteSpace(storedValue)) return false;

            var value = storedValue.Trim();

            if (value.StartsWith(SourceId + Wrapper, StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(SourceId.Length + Wrapper.Length);
            }

            if (value.EndsWith(Wrapper, StringComparison.Ordinal))
            {
                value = value.Substring(0, value.Length - Wrapper.Length);
            }

            return TryParseFields(value, out record);
        }

        /// <summary>
        /// Formats a record the way this application stores it.
        /// </summary>
        internal static string Format(GithubUpdateRecord record)
        {
            if (record == null) return string.Empty;

            return SourceId + Wrapper
                + record.Owner + ","
                + record.Repository + ","
                + record.FileStartsWith + ","
                + record.FileEndsWith + ","
                + record.VersionFromFile
                + Wrapper;
        }

        /// <summary>
        /// Splits <c>owner,repository[,fileStartsWith[,fileEndsWith[,versionFromFile]]]</c>.
        /// <para>
        /// A missing field is not an error here: whether the record is usable is
        /// <see cref="GithubUpdateRecord.IsValid"/>'s decision, so a record that is too
        /// thin to act on is reported with a reason instead of silently dropped.
        /// </para>
        /// </summary>
        private static bool TryParseFields(string fields, out GithubUpdateRecord record)
        {
            record = null;

            var parts = (fields ?? string.Empty).Split(FieldSeparators);

            if (parts.Length < 2) return false;

            var owner = parts[0].Trim();
            var repository = parts[1].Trim();

            if (owner.Length == 0 || repository.Length == 0) return false;

            var fileStartsWith = parts.Length > 2 ? parts[2].Trim() : string.Empty;
            var fileEndsWith = parts.Length > 3 ? parts[3].Trim() : string.Empty;

            var versionFromFile = false;
            if (parts.Length > 4)
            {
                bool.TryParse(parts[4].Trim(), out versionFromFile);
            }

            record = new GithubUpdateRecord(owner, repository, fileStartsWith, fileEndsWith, versionFromFile);
            return true;
        }
    }
}
