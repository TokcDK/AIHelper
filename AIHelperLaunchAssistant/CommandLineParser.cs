using AIHelper.Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AIHelperLaunchAssistant
{
    /// <summary>
    /// Turns a raw command line into a <see cref="LaunchOptions"/>.
    /// </summary>
    /// <remarks>
    /// Pure: it starts nothing, reads nothing from disk and keeps no state, which is what makes the
    /// whole command line contract testable without a process or a window. Anything that can only be
    /// answered by looking at the machine - whether the target exists, whether the host can be found -
    /// is left to the run itself.
    /// </remarks>
    internal static class CommandLineParser
    {
        /// <summary>Everything after this argument belongs to the target program.</summary>
        internal const string ArgumentSeparator = "--";

        internal static ParseResult Parse(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                return ParseResult.Help();
            }

            string targetPath = null;
            string targetArguments = null;
            string workingDirectory = null;
            string hostPath = null;
            int hostProcessId = 0;
            string hostProcessName = null;
            bool restartHost = true;
            bool showTrayIcon = true;
            TimeSpan hostWaitTimeout = TimeSpan.Zero;
            var hostArguments = new List<string>();
            var trailing = new List<string>();

            for (int index = 0; index < args.Length; index++)
            {
                string argument = args[index];

                if (argument == ArgumentSeparator)
                {
                    for (int rest = index + 1; rest < args.Length; rest++)
                    {
                        trailing.Add(args[rest]);
                    }

                    break;
                }

                string name;
                string inlineValue;
                SplitOption(argument, out name, out inlineValue);

                string value;
                string error;

                switch (name)
                {
                    case "--help":
                    case "-h":
                    case "-?":
                        return ParseResult.Help();

                    case "--version":
                    case "-v":
                        return ParseResult.Version();

                    case "--target":
                    case "-t":
                        if (!TryTakeValue(args, ref index, inlineValue, name, out value, out error))
                        {
                            return ParseResult.Error(error);
                        }

                        targetPath = value;
                        break;

                    case "--arguments":
                    case "-a":
                        if (!TryTakeValue(args, ref index, inlineValue, name, out value, out error))
                        {
                            return ParseResult.Error(error);
                        }

                        targetArguments = value;
                        break;

                    case "--working-dir":
                    case "-d":
                        if (!TryTakeValue(args, ref index, inlineValue, name, out value, out error))
                        {
                            return ParseResult.Error(error);
                        }

                        workingDirectory = value;
                        break;

                    case "--host":
                        if (!TryTakeValue(args, ref index, inlineValue, name, out value, out error))
                        {
                            return ParseResult.Error(error);
                        }

                        hostPath = value;
                        break;

                    case "--host-arg":
                        if (!TryTakeValue(args, ref index, inlineValue, name, out value, out error))
                        {
                            return ParseResult.Error(error);
                        }

                        hostArguments.Add(value);
                        break;

                    case "--host-pid":
                        if (!TryTakeValue(args, ref index, inlineValue, name, out value, out error))
                        {
                            return ParseResult.Error(error);
                        }

                        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out hostProcessId)
                            || hostProcessId <= 0)
                        {
                            return ParseResult.Error("Option " + name + " needs a positive process id, not \"" + value + "\".");
                        }

                        break;

                    case "--host-name":
                        if (!TryTakeValue(args, ref index, inlineValue, name, out value, out error))
                        {
                            return ParseResult.Error(error);
                        }

                        hostProcessName = value;
                        break;

                    case "--host-wait":
                        if (!TryTakeValue(args, ref index, inlineValue, name, out value, out error))
                        {
                            return ParseResult.Error(error);
                        }

                        int seconds;
                        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out seconds)
                            || seconds < 0)
                        {
                            return ParseResult.Error("Option " + name + " needs a number of seconds, not \"" + value + "\".");
                        }

                        hostWaitTimeout = TimeSpan.FromSeconds(seconds);
                        break;

                    case "--no-restart":
                        if (RejectValue(name, inlineValue, out error))
                        {
                            return ParseResult.Error(error);
                        }

                        restartHost = false;
                        break;

                    case "--no-tray":
                        if (RejectValue(name, inlineValue, out error))
                        {
                            return ParseResult.Error(error);
                        }

                        showTrayIcon = false;
                        break;

                    default:
                        return ParseResult.Error(DescribeUnknownArgument(argument));
                }
            }

            if (trailing.Count > 0)
            {
                if (targetPath != null)
                {
                    return ParseResult.Error("The target program was given twice: with --target and after \"" + ArgumentSeparator + "\".");
                }

                targetPath = trailing[0];

                if (trailing.Count > 1)
                {
                    if (targetArguments != null)
                    {
                        return ParseResult.Error("Arguments for the target program were given twice: with --arguments and after \"" + ArgumentSeparator + "\".");
                    }

                    // Re-quoted rather than concatenated: the arguments arrive already split, and
                    // joining them back with plain spaces would lose the ones that contained a space.
                    targetArguments = ArgumentQuoting.Join(trailing.GetRange(1, trailing.Count - 1).ToArray());
                }
            }

            if (string.IsNullOrWhiteSpace(targetPath))
            {
                return ParseResult.Error("No target program was given. Name it with --target, or after \"" + ArgumentSeparator + "\".");
            }

            var target = new TargetProgram(targetPath, targetArguments, workingDirectory);
            var host = new HostProgram(hostPath, hostProcessId, hostProcessName, hostArguments.ToArray(), restartHost, hostWaitTimeout);

            return ParseResult.Success(new LaunchOptions(target, host, showTrayIcon));
        }

        internal static string BuildUsageText()
        {
            var builder = new StringBuilder();

            builder.AppendLine(ApplicationInfo.Name + " " + ApplicationInfo.Version);
            builder.AppendLine();
            builder.AppendLine("Starts a program without the host application staying resident: waits for the host to");
            builder.AppendLine("close, starts the target, keeps an icon in the notification area while the target runs");
            builder.AppendLine("and starts the host again when the target ends.");
            builder.AppendLine();
            builder.AppendLine("Usage:");
            builder.AppendLine("  " + ApplicationInfo.ExecutableName + " [options] -- <target> [<target arguments>...]");
            builder.AppendLine();
            builder.AppendLine("Options:");
            builder.AppendLine("  -t, --target <path>      Program to start once the host has closed.");
            builder.AppendLine("  -a, --arguments <text>   Command line for the target program.");
            builder.AppendLine("  -d, --working-dir <dir>  Directory to start the target in (default: its own folder).");
            builder.AppendLine("      --host <path>        Program to start again when the target ends");
            builder.AppendLine("                           (default: found next to this tool).");
            builder.AppendLine("      --host-arg <value>   One argument for the host program; may be repeated.");
            builder.AppendLine("      --host-pid <pid>     Id of the host instance to wait for");
            builder.AppendLine("                           (default: the process that started this tool).");
            builder.AppendLine("      --host-name <name>   Process name of the host (default: \"" + HostApplicationLocator.HostProcessName + "\").");
            builder.AppendLine("      --host-wait <sec>    Stop waiting for the host after this long (default: forever).");
            builder.AppendLine("      --no-restart         Do not start the host again.");
            builder.AppendLine("      --no-tray            Do not show a notification area icon.");
            builder.AppendLine("  -h, --help               Show this text.");
            builder.AppendLine("  -v, --version            Show the version.");
            builder.AppendLine();
            builder.AppendLine("Exit codes: 0 done, 1 unexpected error, 2 bad command line, 3 target not started,");
            builder.AppendLine("            4 host not started again, 5 closed by the user.");

            return builder.ToString();
        }

        /// <summary>
        /// Splits "--name=value" into its two halves. An argument that is not an option, or an option
        /// without an equals sign, comes back whole with no value.
        /// </summary>
        private static void SplitOption(string argument, out string name, out string inlineValue)
        {
            name = argument;
            inlineValue = null;

            if (argument.Length < 2 || argument[0] != '-')
            {
                return;
            }

            int separator = argument.IndexOf('=');
            if (separator <= 0)
            {
                return;
            }

            name = argument.Substring(0, separator);
            inlineValue = argument.Substring(separator + 1);
        }

        /// <summary>
        /// Reads the value of an option, either from "--name=value" or from the argument after it.
        /// </summary>
        private static bool TryTakeValue(string[] args, ref int index, string inlineValue, string optionName, out string value, out string error)
        {
            error = null;

            if (inlineValue != null)
            {
                value = inlineValue;
            }
            else if (index + 1 < args.Length)
            {
                index++;
                value = args[index];
            }
            else
            {
                value = null;
                error = "Option " + optionName + " needs a value.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                value = null;
                error = "Option " + optionName + " needs a value that is not empty.";
                return false;
            }

            return true;
        }

        /// <summary>Refuses a value given to an option that is a plain switch.</summary>
        private static bool RejectValue(string optionName, string inlineValue, out string error)
        {
            if (inlineValue == null)
            {
                error = null;
                return false;
            }

            error = "Option " + optionName + " does not take a value.";
            return true;
        }

        private static string DescribeUnknownArgument(string argument)
        {
            if (argument.Length > 0 && argument[0] == '-')
            {
                return "Unknown option \"" + argument + "\".";
            }

            return "Unexpected value \"" + argument + "\". Name the target program with --target, or put it after \"" + ArgumentSeparator + "\".";
        }
    }
}
