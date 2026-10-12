namespace AIHelperLaunchAssistant
{
    /// <summary>What the user asked the assistant to do instead of waiting.</summary>
    /// <remarks>
    /// There is no "nothing was asked" member: that answer is the absence of a request, and it is
    /// carried as a null <see cref="UserRequest"/> so a caller cannot mistake one for the other.
    /// </remarks>
    internal enum UserRequest
    {
        /// <summary>Start the host now, without waiting for the target to end.</summary>
        StartHostNow,

        /// <summary>Close the assistant without the host being started again.</summary>
        ExitWithoutRestartingHost
    }

    /// <summary>
    /// The assistant's side of the link to whoever is showing it to the user.
    /// </summary>
    /// <remarks>
    /// It carries both directions because they are one conversation: the sequence says what it is doing
    /// so the icon can be shown at the right moment, and reads back what the user asked for while it
    /// waits. Keeping it an interface is what lets the sequence run with no window at all.
    /// </remarks>
    internal interface IAssistantUi
    {
        /// <summary>Called once the target program is running.</summary>
        void TargetStarted(string targetDisplayName);

        /// <summary>Takes the request the user made, clearing it; null when there is none.</summary>
        UserRequest? TakeRequest();
    }

    /// <summary>The user interface when there is none: nothing to show, nothing ever asked.</summary>
    internal sealed class NoAssistantUi : IAssistantUi
    {
        internal static readonly IAssistantUi Instance = new NoAssistantUi();

        private NoAssistantUi()
        {
        }

        public void TargetStarted(string targetDisplayName)
        {
        }

        public UserRequest? TakeRequest()
        {
            return null;
        }
    }
}
