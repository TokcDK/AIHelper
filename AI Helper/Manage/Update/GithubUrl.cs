using System;
using System.Text.RegularExpressions;

namespace AIHelper.Manage.Update
{
    /// <summary>
    /// Reads and builds the GitHub repository urls this application stores.
    /// <para>
    /// Two places need this: the editor's add-mod form, where the url is typed, and
    /// the editor's mod list, where it is read back out of the <c>url</c> key of
    /// meta.ini. Each of them used to carry a regular expression of its own, and the
    /// two did not agree — one demanded the scheme, the other accepted arbitrary text
    /// in front of the host, and neither stopped the repository name at a query
    /// string.
    /// </para>
    /// </summary>
    internal static class GithubUrl
    {
        /// <summary>
        /// Pattern of a repository url: an optional scheme, the host, the owner and the
        /// repository name. Anything after the repository name — a deeper path, a query
        /// string, a fragment — is ignored rather than folded into the name.
        /// </summary>
        private static readonly Regex RepositoryRegex = new Regex(
            @"^(?:https?://)?" + Regex.Escape(UpdateRecordParser.SourceHost) + @"/(?<owner>[^/?#\s]+)/(?<repository>[^/?#\s]+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase,
            TimeSpan.FromSeconds(1));

        /// <summary>
        /// Splits a repository url into its owner and its repository name.
        /// </summary>
        /// <param name="url">Url to read. The scheme may be missing.</param>
        /// <param name="owner">Owner that was read, or an empty string.</param>
        /// <param name="repository">Repository that was read, or an empty string.</param>
        /// <returns>True when the url points at a repository of the source host.</returns>
        internal static bool TryParse(string url, out string owner, out string repository)
        {
            owner = string.Empty;
            repository = string.Empty;

            if (string.IsNullOrWhiteSpace(url)) return false;

            var match = RepositoryRegex.Match(url.Trim());
            if (!match.Success) return false;

            owner = match.Groups["owner"].Value;
            repository = match.Groups["repository"].Value;

            return owner.Length > 0 && repository.Length > 0;
        }

        /// <summary>
        /// The page of a repository, with the scheme added when the host does not carry
        /// one already.
        /// </summary>
        /// <param name="site">Host of the repository, or null/empty for the default one.</param>
        /// <param name="owner">Owner of the repository.</param>
        /// <param name="repository">Repository name.</param>
        internal static string BuildRepositoryUrl(string site, string owner, string repository)
        {
            if (string.IsNullOrWhiteSpace(site)) site = UpdateRecordParser.SourceHost;

            var host = site.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? site : "https://" + site;

            return host + "/" + owner + "/" + repository;
        }
    }
}
