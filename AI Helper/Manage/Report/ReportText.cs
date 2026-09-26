using System;
using System.Net;
using System.Text.RegularExpressions;

namespace AIHelper.Manage.Report
{
    /// <summary>
    /// Text helpers shared by the report parser and the report renderer: picking
    /// the translation of a multi language value, turning a url into a host name,
    /// making a value safe to embed into markup and turning a name into an id.
    /// </summary>
    internal static class ReportText
    {
        /// <summary>Anything that is not usable in a html id.</summary>
        private static readonly Regex NotInSlugPattern = new Regex("[^A-Za-z0-9]+", RegexOptions.Compiled);

        /// <summary>
        /// Selects the variant of <paramref name="value"/> that belongs to
        /// <paramref name="languageTag"/>. Values carry their translations as
        /// "default&lt;ru-RU&gt;translation"; when there is no variant for the
        /// language the leading default is returned.
        /// </summary>
        /// <param name="value">Value of an info file field.</param>
        /// <param name="languageTag">Language to prefer, for example "ru-RU".</param>
        internal static string SelectLanguageVariant(string value, string languageTag)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            if (!string.IsNullOrWhiteSpace(languageTag))
            {
                var marker = "<" + languageTag + ">";
                var markerIndex = value.IndexOf(marker, StringComparison.Ordinal);

                if (markerIndex >= 0)
                {
                    var variantStart = markerIndex + marker.Length;
                    var variantEnd = value.IndexOf('<', variantStart);
                    var variant = variantEnd < 0
                        ? value.Substring(variantStart)
                        : value.Substring(variantStart, variantEnd - variantStart);

                    if (variant.Length > 0) return variant;
                }
            }

            var defaultEnd = value.IndexOf('<');
            return defaultEnd > 0 ? value.Substring(0, defaultEnd) : value;
        }

        /// <summary>
        /// Returns the host name of <paramref name="url"/>, or the url itself
        /// when it is not a valid url.
        /// </summary>
        internal static string GetHostName(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return string.Empty;

            try
            {
                return new Uri(url).Host;
            }
            catch (UriFormatException)
            {
                return url;
            }
        }

        /// <summary>Makes a value safe to place into report markup.</summary>
        internal static string HtmlEncode(string value)
        {
            return WebUtility.HtmlEncode(value ?? string.Empty);
        }

        /// <summary>
        /// Turns a category name into the id used by its section, so the same
        /// name always produces the same id.
        /// </summary>
        internal static string ToSlug(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "section";

            var slug = NotInSlugPattern.Replace(value, "-").Trim('-');

#pragma warning disable CA1308 // Normalize strings to uppercase - html ids are lower case by convention
            return slug.Length == 0 ? "section" : slug.ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
        }
    }
}
