namespace AIHelper.Games
{
    /// <summary>
    /// One "object path - link/target path" pair of a game preset.
    /// </summary>
    /// <remarks>
    /// This replaces the <c>string[,]</c> tables that were used before. With a
    /// named type the two paths can not be swapped by accident and the meaning
    /// of each one is visible at the call site, which the old <c>[i, 0]</c> /
    /// <c>[i, 1]</c> indexing only explained in comments. That matters here:
    /// <see cref="Manage.ManageSymLinkExtensions.CreateSymlink"/> takes the real
    /// object first and the place the link is created second, and getting the
    /// two the wrong way round is a silent no-op rather than an error.
    /// </remarks>
    public sealed class PathPair
    {
        public PathPair(string source, string target)
        {
            Source = source;
            Target = target;
        }

        /// <summary>
        /// The path the content lives at now. For a link pair this is the real
        /// object, for a copy pair it is the file to copy from.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// The path the content is linked to, or copied to.
        /// </summary>
        public string Target { get; }

        public override string ToString()
        {
            return Source + " -> " + Target;
        }
    }
}
