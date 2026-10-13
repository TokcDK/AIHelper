using System;
using System.IO;

namespace AIHelperUpdater
{
    /// <summary>
    /// Entry point of aihelper-updater.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The program is started by AI Helper when a downloaded update is waiting, and it is
    /// started *before* the application closes - the application starts it and then ends, and
    /// this process is what outlives the files it is about to replace. Its whole job is the
    /// sequence below, in this order and for these reasons:
    /// </para>
    /// <list type="number">
    /// <item>
    /// wait for the application to end. Every file the update replaces is locked while it runs,
    /// so nothing may be touched first. The wait is bounded: an application that will not close
    /// is one the user is still using, and the tool then leaves without changing anything
    /// rather than pulling files out from under it.
    /// </item>
    /// <item>
    /// apply the update: the files listed by update-rules.txt are moved into a backup, and the
    /// payload is then moved over the application.
    /// </item>
    /// <item>
    /// start the application again. This happens whatever the update came to, because the
    /// alternative is a closed application with nothing on screen to say why.
    /// </item>
    /// </list>
    /// <para>
    /// The program shows nothing and says nothing on screen, like the launch assistant and for
    /// the same reason: it runs while the application is not running, so there is no window to
    /// belong to. Everything it has to say goes into RES\update\aihelper-updater.log, and a
    /// failure is also left in a marker file the application reads on its next start - which is
    /// how the user is told, in the language the application is set to.
    /// </para>
    /// </remarks>
    internal static class Program
    {
        /// <summary>Exit code reported when the command line could not be understood.</summary>
        internal const int BadCommandLineExitCode = 2;

        /// <summary>Exit code reported when the application was still running.</summary>
        internal const int ApplicationStillRunningExitCode = 3;

        /// <summary>Exit code reported when the update could not be applied.</summary>
        internal const int UpdateFailedExitCode = 1;

        /// <summary>
        /// How long the application is given to close.
        /// </summary>
        /// <remarks>
        /// Closing runs the application's own end-of-session work - the selected game and the
        /// window geometry are written out on the way down - so the wait cannot be short. It
        /// only has to be short enough that a user who cancelled the close is not left with a
        /// process that appears to be doing something.
        /// </remarks>
        private static readonly TimeSpan ApplicationExitTimeout = TimeSpan.FromSeconds(60);

        private static int Main(string[] args)
        {
            var arguments = UpdateArguments.Parse(args);
            if (!arguments.IsValid) return BadCommandLineExitCode;

            var applicationDirectory = Path.GetDirectoryName(arguments.HostExecutablePath);
            if (string.IsNullOrEmpty(applicationDirectory)) return BadCommandLineExitCode;

            var layout = new UpdateLayout(applicationDirectory, ExecutableDirectory());

            using (var log = UpdateLog.Open(layout.LogFilePath))
            {
                log.Info("aihelper-updater started for '" + arguments.HostExecutablePath
                         + "' (pid " + arguments.HostProcessId + ").");

                if (!HostProcessWatcher.WaitForExit(
                        arguments.HostProcessId,
                        Path.GetFileNameWithoutExtension(arguments.HostExecutablePath),
                        ApplicationExitTimeout,
                        log))
                {
                    return ApplicationStillRunningExitCode;
                }

                var applied = new ApplicationUpdate(layout, log)
                    .Apply(Path.GetFileName(arguments.HostExecutablePath));

                // Whatever happened, the application is put back on screen before this process
                // ends. An update that failed has to be visible as an application that is still
                // there, not as a program that disappeared.
                HostProcessWatcher.Start(arguments.HostExecutablePath, applicationDirectory, log);

                return applied ? 0 : UpdateFailedExitCode;
            }
        }

        /// <summary>
        /// Folder this program runs from.
        /// </summary>
        /// <remarks>
        /// Taken from the assembly location rather than from the current directory, which is
        /// whatever the process that started this one happened to be in.
        /// </remarks>
        private static string ExecutableDirectory()
        {
            return Path.GetDirectoryName(typeof(Program).Assembly.Location);
        }
    }
}
