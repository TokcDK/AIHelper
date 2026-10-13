using AIHelper.Shared;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace AIHelper.Manage
{
    /// <summary>
    /// Hands a game or studio launch over to aihelper-launch-assistant.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The tool exists so that this application does not have to stay in memory for the length of a
    /// game session. It is started with the program to run, waits for this process to end, starts the
    /// program, waits for that program and starts this application again when it ends. Nothing of the
    /// game is left to do here after the hand-off, which is the point: the launch is started from
    /// this application and finished by the tool, so only one of the two is ever resident.
    /// </para>
    /// <para>
    /// The tool is native C++ with no window, no notification area icon and no runtime, because it is
    /// the one that stays resident. It is built by the PreBuildEvent in the csproj into the
    /// repository's tools folder, from where postbuild.bat copies it into RES\tools of the output.
    /// </para>
    /// <para>
    /// The tool is a separate program, so the only thing between them is a command line. It is built
    /// here, quoted with <see cref="ArgumentQuoting"/> - the same implementation the tool reads it
    /// with, see the remarks on that file - and nowhere else, so the contract has one side to read.
    /// </para>
    /// </remarks>
    static class ManageLaunchAssistant
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>Folder of the tool inside the application's RES folder.</summary>
        internal const string ToolFolderName = "aihelper-launch-assistant";

        /// <summary>File name of the tool.</summary>
        internal const string ToolFileName = "aihelper-launch-assistant.exe";

        /// <summary>
        /// How long the tool is watched after it is started, to catch a command line it refuses.
        /// </summary>
        /// <remarks>
        /// A tool which is doing its job cannot exit before this process does - it waits for this
        /// process first - so an exit inside this window is a failure, and a failure found here can
        /// still be turned into a launch the old way. The wait is short because the tool reads its
        /// command line and starts waiting straight away; anything slower than this is not the
        /// command line being refused.
        /// </remarks>
        internal const int StartupGraceMilliseconds = 500;

        /// <summary>Where the tool is deployed: RES\tools\aihelper-launch-assistant.</summary>
        internal static string ExecutablePath =>
            Path.Combine(ManageSettings.AppResToolsDirPath, ToolFolderName, ToolFileName);

        /// <summary>Whether the tool is there to be started.</summary>
        internal static bool IsAvailable => File.Exists(ExecutablePath);

        /// <summary>
        /// Starts the tool, which then starts <paramref name="targetExePath"/> as soon as this
        /// process has ended.
        /// </summary>
        /// <param name="targetExePath">Program to start, once this application has closed.</param>
        /// <param name="targetArguments">Command line for that program, or an empty string.</param>
        /// <param name="oldMoProfileName">
        /// Mod Organizer profile to put back once the target has ended, or an empty string.
        /// </param>
        /// <returns>
        /// <c>true</c> when the tool took the launch over, in which case the caller has nothing left
        /// to do but close this application. <c>false</c> when it did not, in which case the launch
        /// still has to be done here.
        /// </returns>
        internal static bool TryHandOff(string targetExePath, string targetArguments, string oldMoProfileName)
        {
            if (!IsAvailable)
            {
                _log.Warn("The launch tool is not installed at \"" + ExecutablePath + "\"; the program is started from here instead.");
                return false;
            }

            string commandLine = BuildCommandLine(
                targetExePath,
                targetArguments,
                GetTargetWorkingDirectory(targetExePath, targetArguments),
                Application.ExecutablePath,
                Process.GetCurrentProcess().Id,
                oldMoProfileName);

            var startInfo = new ProcessStartInfo
            {
                FileName = ExecutablePath,
                Arguments = commandLine,
                WorkingDirectory = Path.GetDirectoryName(ExecutablePath),

                // No shell: the arguments are already a command line and have to reach the tool
                // exactly as they were written, without being taken apart and put together again by
                // something else. No console either, for the same reason the tool is a window
                // application: nothing of a launch may end up on screen over the game.
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (Process tool = Process.Start(startInfo))
                {
                    if (tool == null)
                    {
                        _log.Error("No launch tool process was started for \"" + ExecutablePath + "\".");
                        return false;
                    }

                    if (tool.WaitForExit(StartupGraceMilliseconds))
                    {
                        // It is gone already, and it cannot be gone on its own unless it refused the
                        // command line or could not do what it was asked. Either way the launch is
                        // still owed to the user, and it is done here.
                        _log.Error("The launch tool exited at once with code " + tool.ExitCode + "; the program is started from here instead.");
                        return false;
                    }
                }

                _log.Info("Launch of \"" + targetExePath + "\" handed over to " + ExecutablePath + ".");
                return true;
            }
            catch (Exception exception) when (IsStartFailure(exception))
            {
                _log.Error("Failed to start " + ExecutablePath + ": " + exception.Message);
                return false;
            }
        }

        /// <summary>
        /// Closes this application, which is the next step of the hand-off rather than a side effect
        /// of it: the tool waits for this process to end before it starts the target.
        /// </summary>
        /// <remarks>
        /// Application.Exit is used rather than closing the main window so that every open window is
        /// closed, and so that the closing handlers which save the selected game and the window size
        /// run as they do on any other exit.
        /// </remarks>
        internal static void CloseHost()
        {
            Application.Exit();
        }

        /// <summary>
        /// Builds the command line the tool is started with.
        /// </summary>
        /// <remarks>
        /// Pure and free of the application's state, so that the contract with the tool can be read
        /// in one place: this method and the tool's own command line parser are the two halves of it,
        /// and every value is quoted with the one implementation of the Windows rules the two
        /// programs share.
        /// </remarks>
        internal static string BuildCommandLine(
            string targetExePath,
            string targetArguments,
            string targetWorkingDirectory,
            string hostExePath,
            int hostProcessId,
            string oldMoProfileName)
        {
            var arguments = new List<string> { "--target", targetExePath };

            if (!string.IsNullOrEmpty(targetArguments))
            {
                arguments.Add("--arguments");
                arguments.Add(targetArguments);
            }

            if (!string.IsNullOrEmpty(targetWorkingDirectory))
            {
                arguments.Add("--working-dir");
                arguments.Add(targetWorkingDirectory);
            }

            // The host is named, not found: the tool would work it out from its own location, but
            // this process knows exactly which program it is and has no reason to make the tool guess.
            arguments.Add("--host");
            arguments.Add(hostExePath);

            // Pinned to this instance rather than to a name: a second copy of the application started
            // by the user would otherwise keep the game from starting. The id is the whole of the
            // identity the tool needs - it waits for that process and starts the one named above.
            arguments.Add("--host-pid");
            arguments.Add(hostProcessId.ToString(CultureInfo.InvariantCulture));

            if (!string.IsNullOrEmpty(oldMoProfileName))
            {
                // The profile the user was working in is handed over rather than restored here. This
                // process is about to close and stays closed for the whole game session, so the
                // instance the tool starts again is the only one that can put it back.
                arguments.Add("--host-arg");
                arguments.Add(StartupOptions.RestoreMoProfileOption);
                arguments.Add("--host-arg");
                arguments.Add(oldMoProfileName);
            }

            return ArgumentQuoting.Join(arguments.ToArray());
        }

        /// <summary>
        /// The directory the target has to be started in, or an empty string when its own folder is
        /// right.
        /// </summary>
        /// <remarks>
        /// This is the rule the direct launch applies, kept as it is: a Mod Organizer shortcut is
        /// started in whatever directory this application is in, and everything else in its own
        /// folder. The directory is worked out here and passed on instead of being left to be
        /// inherited, because there is now a second process between this one and the program being
        /// started, and an inherited directory would depend on where that process happened to be.
        /// </remarks>
        internal static string GetTargetWorkingDirectory(string targetExePath, string targetArguments)
        {
            if (ManageSettings.IsMoMode && !string.IsNullOrWhiteSpace(targetArguments))
            {
                return Environment.CurrentDirectory;
            }

            return Path.GetDirectoryName(targetExePath) ?? string.Empty;
        }

        /// <summary>
        /// Whether a failure to start the tool is one that can be reported, rather than one that has
        /// to take the application down with it.
        /// </summary>
        private static bool IsStartFailure(Exception exception)
        {
            return exception is Win32Exception
                || exception is InvalidOperationException
                || exception is FileNotFoundException
                || exception is DirectoryNotFoundException;
        }
    }
}
