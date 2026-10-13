namespace AIHelper.Manage.Update.SelfUpdate
{
    /// <summary>
    /// What a check of AI Helper's own update came to.
    /// </summary>
    /// <remarks>
    /// Three outcomes and not two, because "nothing newer is published" and "a newer version is
    /// waiting" are both successes and both have something to say to the user, while a failure
    /// has to be told apart from them to be reported as one.
    /// </remarks>
    internal enum SelfUpdateOutcome
    {
        /// <summary>Nothing newer than the installed version is published.</summary>
        UpToDate,

        /// <summary>A newer release was downloaded and is waiting to be applied.</summary>
        Ready,

        /// <summary>The release could not be looked up, downloaded or unpacked.</summary>
        Failed
    }
}
