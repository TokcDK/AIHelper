namespace AIHelperUpdater
{
    /// <summary>
    /// One line of update-rules.txt: a path the update works with, and which way round it works.
    /// </summary>
    /// <remarks>
    /// A rule is either a path that is moved into the backup before the new version is applied,
    /// or - with a leading <c>!</c> - a path that is left where it is even though it lies inside
    /// one of those. Both are needed for the same reason: what the release ships and what the
    /// user owns are not the same set, and only the file can tell them apart. A theme the user
    /// made lives in the folder the release also ships, and it has to survive the update.
    /// </remarks>
    internal sealed class UpdateRule
    {
        internal UpdateRule(string relativePath, bool isExclusion)
        {
            RelativePath = relativePath;
            IsExclusion = isExclusion;
        }

        /// <summary>Path relative to the application folder.</summary>
        internal string RelativePath { get; }

        /// <summary>True for a <c>!</c> line: a path the backup has to leave alone.</summary>
        internal bool IsExclusion { get; }
    }
}
