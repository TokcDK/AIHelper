using System;

namespace AIHelper.Manage
{
    /// <summary>
    /// The command line this instance of the application was started with.
    /// </summary>
    /// <remarks>
    /// The application is normally started by the user and given nothing at all. It is started by
    /// aihelper-launch-assistant when a game session ends, and that is the one case where there is
    /// something to do on the way up - see <see cref="RestoreMoProfileOption"/>.
    /// </remarks>
    internal sealed class StartupOptions
    {
        /// <summary>
        /// Mod Organizer profile to put back once the application has started, given by the launch
        /// tool.
        /// </summary>
        internal const string RestoreMoProfileOption = "--restore-mo-profile";

        private StartupOptions(string restoreMoProfileName)
        {
            RestoreMoProfileName = restoreMoProfileName ?? string.Empty;
        }

        /// <summary>
        /// Mod Organizer profile the previous session was working in, or an empty string when the
        /// application was not started by the launch tool.
        /// </summary>
        internal string RestoreMoProfileName { get; }

        /// <summary>Nothing to do on start, which is the case for a launch by the user.</summary>
        internal static StartupOptions Empty { get; } = new StartupOptions(string.Empty);

        /// <summary>
        /// Reads the options out of the arguments the application was started with.
        /// </summary>
        /// <remarks>
        /// An argument this version does not know is ignored rather than refused. A program the user
        /// starts by hand has no business refusing to start over something it does not recognise,
        /// and the application is started by the tool as well, which is free to add arguments this
        /// version does not know yet. A missing value for a known option is treated the same way:
        /// there is then nothing to restore, which is the same as not having been asked.
        /// </remarks>
        internal static StartupOptions Parse(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                return Empty;
            }

            string restoreMoProfileName = string.Empty;

            for (int index = 0; index < args.Length; index++)
            {
                string argument = args[index];
                if (string.IsNullOrEmpty(argument))
                {
                    continue;
                }

                // "--name=value" and "--name value" are both accepted, because the tool quotes each
                // argument separately and either form is what a person would type.
                string name = argument;
                string value = null;

                int separator = argument.IndexOf('=');
                if (separator > 0)
                {
                    name = argument.Substring(0, separator);
                    value = argument.Substring(separator + 1);
                }

                if (!string.Equals(name, RestoreMoProfileOption, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (value == null && index + 1 < args.Length)
                {
                    index++;
                    value = args[index];
                }

                restoreMoProfileName = value == null ? string.Empty : value.Trim();
            }

            return restoreMoProfileName.Length == 0 ? Empty : new StartupOptions(restoreMoProfileName);
        }
    }
}
