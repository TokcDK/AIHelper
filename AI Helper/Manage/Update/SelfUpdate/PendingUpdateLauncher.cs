using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using AIHelper.Shared;
using NLog;

namespace AIHelper.Manage.Update.SelfUpdate
{
    /// <summary>
    /// Hands the waiting update over to aihelper-updater.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The tool exists because the files of an update cannot be replaced by the program that is
    /// running from them. It is started with the program to start again, waits for this process
    /// to end, moves the current files into a backup, moves the waiting update over the
    /// application and starts the application again. Nothing of the update is left to do here
    /// after the hand-off, which is the point: this process is about to end and the tool is what
    /// outlives the files it replaces.
    /// </para>
    /// <para>
    /// This is the same shape as the hand-off to aihelper-launch-assistant, and deliberately so:
    /// both are a small program started by this one, told what to start when this one is gone,
    /// and given the process id of this instance so that it waits for the right process and not
    /// for a program by name.
    /// </para>
    /// <para>
    /// The tool is a separate program, so the only thing between them is a command line. It is
    /// built here, quoted with <see cref="ArgumentQuoting"/> - the same implementation the tool
    /// reads it with, see the remarks on that file - and nowhere else.
    /// </para>
    /// </remarks>
    internal static class PendingUpdateLauncher
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>Command line option naming the program to start again.</summary>
        internal const string HostOption = "--host";

        /// <summary>Command line option naming the process to wait for.</summary>
        internal const string HostProcessIdOption = "--host-pid";

        /// <summary>
        /// How long the tool is watched after it is started, to catch a command line it refuses.
        /// </summary>
        /// <remarks>
        /// A tool which is doing its job cannot exit before this process does - it waits for this
        /// process first - so an exit inside this window is a failure, and a failure found here is
        /// still something the caller can report. The wait is short because the tool reads its
        /// command line and starts waiting straight away; anything slower than this is not the
        /// command line being refused.
        /// </remarks>
        internal const int StartupGraceMilliseconds = 500;

        /// <summary>Whether the tool is there to be started.</summary>
        internal static bool IsAvailable
        {
            get { return File.Exists(SelfUpdatePaths.UpdaterExecutablePath); }
        }

        /// <summary>
        /// Starts the tool, which then applies the waiting update as soon as this process has
        /// ended.
        /// </summary>
        /// <returns>
        /// <c>true</c> when the tool took the update over, in which case the caller has nothing
        /// left to do but close this application. <c>false</c> when it did not, in which case the
        /// update has not been applied and the application has to keep running.
        /// </returns>
        internal static bool TryHandOff()
        {
            if (!IsAvailable)
            {
                _log.Error("Self update: the tool is not installed at \""
                           + SelfUpdatePaths.UpdaterExecutablePath + "\"; the update is not applied.");
                return false;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = SelfUpdatePaths.UpdaterExecutablePath,
                Arguments = BuildCommandLine(Application.ExecutablePath, Process.GetCurrentProcess().Id),
                WorkingDirectory = SelfUpdatePaths.UpdaterToolDirectory,

                // No shell: the arguments are already a command line and have to reach the tool
                // exactly as they were written, without being taken apart and put together again
                // by something else. No console either, for the same reason the tool is a window
                // application: the update happens after this application has closed, and nothing
                // of it may end up on screen.
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (var tool = Process.Start(startInfo))
                {
                    if (tool == null)
                    {
                        _log.Error("Self update: no process was started for \""
                                   + SelfUpdatePaths.UpdaterExecutablePath + "\".");
                        return false;
                    }

                    if (tool.WaitForExit(StartupGraceMilliseconds))
                    {
                        // It is gone already, and it cannot be gone on its own unless it refused
                        // the command line or found nothing to apply. Either way the update was
                        // not applied, and closing now would leave the user with an application
                        // that is not there.
                        _log.Error("Self update: the tool exited at once with code " + tool.ExitCode
                                   + "; the update is not applied.");
                        return false;
                    }
                }

                _log.Info("Self update: the update was handed over to "
                          + SelfUpdatePaths.UpdaterExecutablePath + ".");
                return true;
            }
            catch (Exception exception) when (IsStartFailure(exception))
            {
                _log.Error("Self update: failed to start " + SelfUpdatePaths.UpdaterExecutablePath
                           + ": " + exception.Message);
                return false;
            }
        }

        /// <summary>
        /// Closes this application, which is the next step of the hand-off rather than a side
        /// effect of it: the tool waits for this process to end before it touches anything.
        /// </summary>
        /// <remarks>
        /// Application.Exit is used rather than closing the main window so that every open window
        /// is closed, and so that the closing handlers which save the selected game and the window
        /// size run as they do on any other exit.
        /// </remarks>
        internal static void CloseHost()
        {
            Application.Exit();
        }

        /// <summary>
        /// Builds the command line the tool is started with.
        /// </summary>
        /// <remarks>
        /// Pure and free of the application's state, so that the contract with the tool can be
        /// read in one place: this method and the tool's own command line parser are the two
        /// halves of it. Everything else the tool needs - the application folder, the payload,
        /// the rules file, the backup - is derived from these two values and is not passed.
        /// </remarks>
        /// <param name="hostExePath">Program to start again once the update has been applied.</param>
        /// <param name="hostProcessId">Process that has to end before the update may start.</param>
        internal static string BuildCommandLine(string hostExePath, int hostProcessId)
        {
            return ArgumentQuoting.Join(new[]
            {
                HostOption,
                hostExePath,

                // Pinned to this instance rather than to a name: the user is free to start a
                // second copy of the application while the tool waits, and a tool that waited
                // for "the application" would then apply the update under it.
                HostProcessIdOption,
                hostProcessId.ToString(CultureInfo.InvariantCulture)
            });
        }

        /// <summary>
        /// Whether a failure to start the tool is one that can be reported, rather than one that
        /// has to take the application down with it.
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
