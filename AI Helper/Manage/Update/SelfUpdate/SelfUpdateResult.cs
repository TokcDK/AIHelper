namespace AIHelper.Manage.Update.SelfUpdate
{
    /// <summary>
    /// What a check of AI Helper's own update came to, and what the caller needs to say about
    /// it.
    /// </summary>
    /// <remarks>
    /// The version is carried back rather than looked up by the caller, because the release that
    /// was downloaded is known only here - the folder it went into says the same thing, but only
    /// while it is still there. The error is the reason for a failure, meant for the log; the
    /// message the user sees is built by the caller, which is the side that knows the language
    /// of the interface.
    /// </remarks>
    internal sealed class SelfUpdateResult
    {
        private SelfUpdateResult(SelfUpdateOutcome outcome, string version, string error)
        {
            Outcome = outcome;
            Version = version;
            Error = error;
        }

        /// <summary>What the check came to.</summary>
        internal SelfUpdateOutcome Outcome { get; }

        /// <summary>Version that was downloaded, or null when nothing was.</summary>
        internal string Version { get; }

        /// <summary>Why the check failed, or null when it did not.</summary>
        internal string Error { get; }

        /// <summary>Nothing newer is published.</summary>
        internal static SelfUpdateResult UpToDate()
        {
            return new SelfUpdateResult(SelfUpdateOutcome.UpToDate, null, null);
        }

        /// <summary>
        /// <paramref name="version"/> was downloaded and is waiting to be applied.
        /// </summary>
        internal static SelfUpdateResult Ready(string version)
        {
            return new SelfUpdateResult(SelfUpdateOutcome.Ready, version, null);
        }

        /// <summary>The check failed for the given reason.</summary>
        internal static SelfUpdateResult Failed(string error)
        {
            return new SelfUpdateResult(SelfUpdateOutcome.Failed, null, error);
        }
    }
}
