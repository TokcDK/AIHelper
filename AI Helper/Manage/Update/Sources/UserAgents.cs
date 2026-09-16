namespace AIHelper.Manage.Update.Sources
{
    /// <summary>
    /// User agent strings used by the updater when it talks to GitHub.
    /// <para>
    /// These are a frozen snapshot of the browsers that were current when the
    /// updater was written. GitHub does not require a specific agent, but some
    /// CDN edges reject empty or unusual ones, so a real browser string is sent.
    /// Update the values (and the date below) if a request starts being
    /// rejected.
    /// </para>
    /// <para>Snapshot date: 2020-01-04.</para>
    /// </summary>
    internal static class UserAgents
    {
        /// <summary>Latest Chrome on Windows 10.</summary>
        public const string ChromeWin10Latest =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.88 Safari/537.36";

        /// <summary>Chrome on Windows 7, with the Iron brand token.</summary>
        public const string ChromeWin7Latest =
            "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4500.0 Iron Safari/537.36";

        /// <summary>
        /// The agent the updater actually uses. Kept as a separate name because
        /// it is the one tuned to work against GitHub's edge.
        /// </summary>
        public const string ChromeIronWin7 = ChromeWin7Latest;

        /// <summary>Latest Firefox on Windows 10.</summary>
        public const string FirefoxWin10Latest =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:71.0) Gecko/20100101 Firefox/71.0";

        /// <summary>Latest Edge (EdgeHTML) on Windows 10.</summary>
        public const string EdgeWin10Latest =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36 Edge/18.18362";

        /// <summary>
        /// Opera Mini agent. Worked with Google until Google started blocking
        /// it; kept for reference only.
        /// </summary>
        public const string OperaMini =
            "Opera/9.80 (J2ME/MIDP; Opera Mini/5.1.21214/28.2725; U; en) Presto/2.8.119 Version/11.10";
    }
}
