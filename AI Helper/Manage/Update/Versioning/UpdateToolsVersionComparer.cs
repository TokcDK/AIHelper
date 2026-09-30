using ModUpdater.Core.Infrastructure.Versioning;

namespace AIHelper.Manage.Update.Versioning
{
    /// <summary>
    /// Decides which of two versions is the newer one, using the comparison this
    /// application has always used.
    /// <para>
    /// The engine defines the question and leaves the answer to the host, because
    /// only the host knows how the versions it deals with are written. Here they
    /// come from two sources that disagree: Mod Organizer writes a date based
    /// version into meta.ini, while releases are tagged with anything the author
    /// felt like. <see cref="UpdateTools.IsNewerOf"/> already knows about both —
    /// it normalises the Mod Organizer date form and the usual release suffix
    /// words, and compares each dot separated part as a number when it can, so
    /// that 1.10 counts as newer than 1.9.
    /// </para>
    /// </summary>
    internal sealed class UpdateToolsVersionComparer : IVersionComparer
    {
        /// <summary>
        /// Contract of <see cref="IVersionComparer"/>: a positive result means the
        /// second value — the version found online — is the newer one.
        /// </summary>
        public int Compare(string current, string latest)
        {
            return UpdateTools.IsNewerOf(latest, current) ? 1 : 0;
        }
    }
}
