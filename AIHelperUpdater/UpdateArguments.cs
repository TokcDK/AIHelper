using System;
using System.Globalization;

namespace AIHelperUpdater
{
    /// <summary>
    /// The command line aihelper-updater is started with by the application it updates.
    /// </summary>
    /// <remarks>
    /// <para>
    /// AI Helper writes the line - with Shared\ArgumentQuoting.cs, the one implementation of the
    /// Windows quoting rules this repository has - and this program reads it back through
    /// Environment.GetCommandLineArgs, which applies exactly those rules. A path with a space in
    /// it, and the application's own name has one, therefore survives the trip.
    /// </para>
    /// <para>
    /// Only two things are passed. Everything else is derived: the application folder is the
    /// folder of the program to start, the payload is RES\update\appupdate inside it, and the
    /// rules file sits beside this program. A path that is already implied is not passed as
    /// well, because that would be a second place for it to be wrong.
    /// </para>
    /// </remarks>
    internal sealed class UpdateArguments
    {
        internal const string HostOption = "--host";
        internal const string HostProcessIdOption = "--host-pid";

        private UpdateArguments(string hostExecutablePath, int hostProcessId)
        {
            HostExecutablePath = hostExecutablePath;
            HostProcessId = hostProcessId;
        }

        /// <summary>Program to start again once the update has been applied.</summary>
        internal string HostExecutablePath { get; }

        /// <summary>Process that has to end before the update may start.</summary>
        internal int HostProcessId { get; }

        /// <summary>True when both things the tool cannot work without were given.</summary>
        internal bool IsValid
        {
            get { return !string.IsNullOrWhiteSpace(HostExecutablePath) && HostProcessId > 0; }
        }

        /// <summary>
        /// Reads the options this program understands out of a command line.
        /// </summary>
        /// <remarks>
        /// An option this program does not know is ignored rather than refused, so that a newer
        /// AI Helper can start an older tool with a longer line without the update being lost to
        /// an argument the tool has no opinion about.
        /// </remarks>
        /// <param name="args">Arguments as the runtime split them.</param>
        internal static UpdateArguments Parse(string[] args)
        {
            string hostExecutablePath = null;
            var hostProcessId = 0;

            if (args != null)
            {
                for (var index = 0; index < args.Length; index++)
                {
                    if (string.Equals(args[index], HostOption, StringComparison.OrdinalIgnoreCase))
                    {
                        hostExecutablePath = NextValue(args, ref index);
                    }
                    else if (string.Equals(args[index], HostProcessIdOption, StringComparison.OrdinalIgnoreCase))
                    {
                        int.TryParse(
                            NextValue(args, ref index),
                            NumberStyles.Integer,
                            CultureInfo.InvariantCulture,
                            out hostProcessId);
                    }
                }
            }

            return new UpdateArguments(hostExecutablePath, hostProcessId);
        }

        /// <summary>Value of the option at <paramref name="index"/>, advancing past it.</summary>
        private static string NextValue(string[] args, ref int index)
        {
            if (index + 1 >= args.Length) return null;

            index++;
            return args[index];
        }
    }
}
