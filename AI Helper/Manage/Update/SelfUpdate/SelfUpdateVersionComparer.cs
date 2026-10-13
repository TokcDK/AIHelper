using ModUpdater.Core.Infrastructure.Versioning;

namespace AIHelper.Manage.Update.SelfUpdate
{
    /// <summary>
    /// The rule AI Helper's own update decides by: is the release on GitHub newer than the
    /// version installed here.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Two shapes are compared, and which one a version is decides how. A release tag is
    /// allowed to be a SemVer string - "26.9.28-1" is what a feed that publishes versions
    /// properly writes - and a pre-release tail has exactly one correct place in the order,
    /// which only a SemVer parser knows. Everything else, which is what the build of this
    /// application writes into its own executable, is a release number and is compared by
    /// <see cref="ReleaseNumberComparer"/>.
    /// </para>
    /// <para>
    /// The composition is here rather than at each call site so that "which version is newer"
    /// has one answer for the whole feature, and so that the answer can be tested without a
    /// repository, a download or a window.
    /// </para>
    /// </remarks>
    internal sealed class SelfUpdateVersionComparer : IVersionComparer
    {
        /// <summary>
        /// Rule used for the values <see cref="SemanticVersionComparer"/> cannot parse.
        /// </summary>
        /// <remarks>
        /// The engine's default fallback is deliberately not used: it compares unparsable
        /// versions as text, which reads "26.9.28.1" as newer than "26.10.13.3". See the
        /// remarks on <see cref="ReleaseNumberComparer"/>.
        /// </remarks>
        private readonly IVersionComparer _inner = new SemanticVersionComparer(new ReleaseNumberComparer());

        /// <summary>
        /// Compares the installed version against the offered one.
        /// </summary>
        /// <param name="current">Version installed, or the pending one when there is one.</param>
        /// <param name="latest">Version the release offers.</param>
        /// <returns>Positive when <paramref name="latest"/> is newer, zero otherwise.</returns>
        public int Compare(string current, string latest)
        {
            return _inner.Compare(current, latest);
        }
    }
}
