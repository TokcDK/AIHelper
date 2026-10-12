namespace AIHelperLaunchAssistant
{
    /// <summary>
    /// What the assistant reports to whoever started it.
    /// </summary>
    /// <remarks>
    /// The program that starts the assistant is the application itself, so a run that did not do what
    /// it was asked has to be visible without a window: the exit code is the only channel left.
    /// </remarks>
    internal static class ExitCodes
    {
        /// <summary>The target ran and the host was started again, or none of that was asked for.</summary>
        internal const int Success = 0;

        /// <summary>The assistant failed in a way it does not have a code for.</summary>
        internal const int UnexpectedError = 1;

        /// <summary>The command line could not be read.</summary>
        internal const int UsageError = 2;

        /// <summary>The target program could not be started.</summary>
        internal const int TargetStartFailed = 3;

        /// <summary>The target ran, but the host could not be started again.</summary>
        internal const int HostStartFailed = 4;

        /// <summary>The user closed the assistant without the host being started again.</summary>
        internal const int CancelledByUser = 5;
    }
}
