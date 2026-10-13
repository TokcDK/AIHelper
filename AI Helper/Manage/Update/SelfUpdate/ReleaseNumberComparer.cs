using System;
using ModUpdater.Core.Infrastructure.Versioning;

namespace AIHelper.Manage.Update.SelfUpdate
{
    /// <summary>
    /// Compares two versions of AI Helper written as a release number: <c>yy.M.d.N</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the rule the application's own version is compared by, and it exists because the
    /// rules the update engine already has are both wrong for it. The host's rule
    /// (<see cref="UpdateTools.IsNewerOf"/>) turns a date-shaped version into a Mod Organizer
    /// date first and then compares the parts as text, which is a rule for mod folders. The
    /// lenient rule (<see cref="LenientVersionComparer"/>) compares two versions that do not
    /// parse as text, and text order puts "26.9.28.1" above "26.10.13.3" - the update would be
    /// offered backwards.
    /// </para>
    /// <para>
    /// A version that does not parse therefore gets the answer "not newer" rather than a guess.
    /// The two ways of being wrong are not equally bad: a missed update leaves the user on a
    /// working version, while a wrong answer installs an older one over a newer one.
    /// </para>
    /// <para>
    /// The trailing part is dropped before parsing because a tag is allowed to carry one -
    /// "26.10.13.3-1" is a release of the same version - while <see cref="Version"/> refuses
    /// anything after the fourth number.
    /// </para>
    /// </remarks>
    internal sealed class ReleaseNumberComparer : IVersionComparer
    {
        /// <summary>
        /// Characters a version may be followed by without the rest being part of it.
        /// </summary>
        private static readonly char[] TrailingParts = { '-', '+', ' ' };

        /// <summary>
        /// Compares the installed version against the offered one.
        /// </summary>
        /// <param name="current">Version installed, or the pending one when there is one.</param>
        /// <param name="latest">Version the release offers.</param>
        /// <returns>Positive when <paramref name="latest"/> is newer, zero otherwise.</returns>
        public int Compare(string current, string latest)
        {
            var currentNumber = ParseReleaseNumber(current);
            var latestNumber = ParseReleaseNumber(latest);

            // Either side being unreadable is "cannot tell", which is answered as "not newer".
            if (currentNumber == null || latestNumber == null) return 0;

            // Version orders a missing part below a present one, so a four-part release is
            // newer than the same three-part version - which is what the counter at the end
            // of the version is for.
            return latestNumber.CompareTo(currentNumber);
        }

        /// <summary>
        /// Reads a release number, or returns null when the text is not one.
        /// </summary>
        /// <param name="value">Version text.</param>
        private static Version ParseReleaseNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            var text = value.Trim().TrimStart('v', 'V');

            var trailingPart = text.IndexOfAny(TrailingParts);
            if (trailingPart > 0) text = text.Substring(0, trailingPart);

            // Version needs at least two parts, while a release number of a single part is
            // still a number.
            if (text.IndexOf('.') < 0) text += ".0";

            Version number;
            return Version.TryParse(text, out number) ? number : null;
        }
    }
}
