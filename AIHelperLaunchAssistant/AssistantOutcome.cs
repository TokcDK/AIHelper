namespace AIHelperLaunchAssistant
{
    /// <summary>How one run of the assistant ended.</summary>
    internal enum AssistantOutcome
    {
        /// <summary>The target ended by itself and the host was started again.</summary>
        TargetExited,

        /// <summary>The user asked for the host while the target was still running.</summary>
        HostRestartedOnRequest,

        /// <summary>The user closed the assistant; the host was not started again.</summary>
        CancelledByUser,

        /// <summary>The target program could not be started.</summary>
        TargetStartFailed,

        /// <summary>The host could not be started again.</summary>
        HostStartFailed
    }
}
