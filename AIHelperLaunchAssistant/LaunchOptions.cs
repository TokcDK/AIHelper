namespace AIHelperLaunchAssistant
{
    /// <summary>
    /// One launch request, as it was described on the command line.
    /// </summary>
    /// <remarks>
    /// Immutable on purpose: it is built once by <see cref="CommandLineParser"/> and only read
    /// afterwards, so the sequence cannot be steered by anything but the arguments it was given.
    /// </remarks>
    internal sealed class LaunchOptions
    {
        internal LaunchOptions(TargetProgram target, HostProgram host, bool showTrayIcon)
        {
            Target = target;
            Host = host;
            ShowTrayIcon = showTrayIcon;
        }

        /// <summary>Program to start once the host application has closed.</summary>
        internal TargetProgram Target { get; }

        /// <summary>Application to wait for, and to start again when the target ends.</summary>
        internal HostProgram Host { get; }

        /// <summary>Whether the assistant shows an icon in the notification area while the target runs.</summary>
        internal bool ShowTrayIcon { get; }
    }
}
