using System;

namespace AIHelper.Manage.Update
{
    /// <summary>
    /// Version string helpers used by the updater to decide whether a mod is
    /// out of date.
    /// </summary>
    internal static class UpdateTools
    {
        /// <summary>
        /// Suffix spellings normalised before two versions are compared, so
        /// that "1.0beta" and "1.0b" are treated as the same release.
        /// </summary>
        static readonly (string From, string To)[] VersionSuffixReplacements =
        {
            ("ALPHA", "A"),
            ("BETA", "B"),
            ("RELEASE", "R"),
            ("PREVIEW", "P"),
            ("TEST", ""),
        };

        /// <summary>Separators that split a version into comparable parts.</summary>
        static readonly char[] VersionSeparators = { '.', ',' };

        /// <summary>
        /// Removes the decorations from a version string: leading "v"/"r"/"_"
        /// characters, trailing ".0" groups and the known release suffix words.
        /// <para>Examples: "v13" → "13", "1.0.0.0" → "1", "1.0BETA" → "1.0B".</para>
        /// </summary>
        /// <param name="version">Version to clean.</param>
        /// <returns>The cleaned version, or an empty string for null/empty input.</returns>
        internal static string CleanVersion(string version)
        {
            if (string.IsNullOrEmpty(version)) return "";

            var result = version.ToUpperInvariant().TrimFileVersion();

            foreach (var replacement in VersionSuffixReplacements)
            {
                result = result.Replace(replacement.From, replacement.To);
            }

            return result;
        }

        /// <summary>
        /// In-place overload of <see cref="CleanVersion(string)"/>, kept for the
        /// existing call sites.
        /// </summary>
        /// <param name="version">Version to clean.</param>
        public static void CleanVersion(ref string version)
        {
            version = CleanVersion(version);
        }

        /// <summary>
        /// Checks whether <paramref name="candidateVersion"/> is newer than
        /// <paramref name="installedVersion"/>.
        /// <para>
        /// Both values are first converted from the Mod Organizer date format
        /// (see <see cref="ManageModOrganizer.ConvertMODateVersion"/>) and then,
        /// unless <paramref name="needClean"/> is false, passed through
        /// <see cref="CleanVersion(string)"/> — pass false when the caller has
        /// already cleaned both values.
        /// </para>
        /// <para>
        /// Each dot/comma separated part is compared as a number first (so that
        /// 10 is newer than 2) and as text otherwise.
        /// </para>
        /// </summary>
        /// <param name="candidateVersion">Version offered by the update source.</param>
        /// <param name="installedVersion">Version currently installed.</param>
        /// <param name="needClean">Whether the values still need cleaning.</param>
        /// <returns>
        /// True when the candidate is newer. An empty candidate is never newer;
        /// an empty installed version is always older.
        /// </returns>
        public static bool IsNewerOf(this string candidateVersion, string installedVersion, bool needClean = true)
        {
            if (string.IsNullOrWhiteSpace(candidateVersion))
            {
                return false;
            }
            else if (string.IsNullOrWhiteSpace(installedVersion))
            {
                return true;
            }

            var candidate = candidateVersion;
            var installed = installedVersion;

            ManageModOrganizer.ConvertMODateVersion(ref candidate);
            ManageModOrganizer.ConvertMODateVersion(ref installed);

            if (needClean)
            {
                candidate = CleanVersion(candidate);
                installed = CleanVersion(installed);
            }

            var candidateParts = candidate.Split(VersionSeparators);
            var installedParts = installed.Split(VersionSeparators);
            var installedLastIndex = installedParts.Length - 1;

            for (int i = 0; i < candidateParts.Length; i++)
            {
                // every part was equal so far and the installed version has fewer parts
                if (i > installedLastIndex)
                {
                    return true;
                }

                // compare as numbers first, because text order puts 10 below 2
                if (int.TryParse(candidateParts[i], out int candidateNumber)
                    && int.TryParse(installedParts[i], out int installedNumber))
                {
                    if (candidateNumber > installedNumber) return true;
                    if (candidateNumber < installedNumber) return false; // numbers differ, installed is newer
                }

                // otherwise compare as text, for example "b" against "a"
                // string.Compare returns an arbitrary signed value, not ±1
                var compareResult = string.Compare(candidateParts[i], installedParts[i], StringComparison.InvariantCulture);
                if (compareResult > 0)
                {
                    return true;
                }
                else if (compareResult < 0)
                {
                    return false;
                }
            }

            return false;
        }
    }
}
