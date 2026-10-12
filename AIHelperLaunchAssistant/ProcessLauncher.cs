using System;
using System.Diagnostics;
using System.IO;

namespace AIHelperLaunchAssistant
{
    /// <summary>Starts programs.</summary>
    /// <remarks>
    /// An interface rather than a plain call to <see cref="Process.Start(ProcessStartInfo)"/> so the
    /// launch sequence can be driven end to end in a test without a program to start.
    /// </remarks>
    internal interface IProcessLauncher
    {
        /// <summary>
        /// Starts a program.
        /// </summary>
        /// <returns>The running process, or <c>null</c> when the operating system started no new one.</returns>
        Process Start(string fileName, string arguments, string workingDirectory);
    }

    internal sealed class ProcessLauncher : IProcessLauncher
    {
        private readonly ToolLog _log;

        internal ProcessLauncher(ToolLog log)
        {
            _log = log;
        }

        public Process Start(string fileName, string arguments, string workingDirectory)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,

                // The shell has to stay out of this. A Mod Organizer shortcut is passed as an argument
                // ("moshortcut://:..."), and UseShellExecute would try to resolve it as a URL instead
                // of handing it to the program as the plain text it is.
                UseShellExecute = false,

                // Windows ignores this for a windowed program, which is what a game, a studio and the
                // host all are. It matters for a console program: without it, starting one would open a
                // console window over the game, and the assistant is supposed to be invisible.
                CreateNoWindow = true,

                WorkingDirectory = ResolveWorkingDirectory(fileName, workingDirectory)
            };

            if (!string.IsNullOrEmpty(arguments))
            {
                startInfo.Arguments = arguments;
            }

            Process process = Process.Start(startInfo);
            if (process == null)
            {
                _log.Warn("No new process was started for \"" + fileName + "\".");
            }

            return process;
        }

        /// <summary>
        /// Where to start the program: the directory it was given, or the program's own folder.
        /// </summary>
        private static string ResolveWorkingDirectory(string fileName, string workingDirectory)
        {
            if (!string.IsNullOrWhiteSpace(workingDirectory))
            {
                return workingDirectory;
            }

            // A bare file name has no folder ("" here) and a drive root has none either (null there);
            // both are handed on as "no working directory" rather than as an invalid path.
            string directory = Path.GetDirectoryName(fileName);
            return string.IsNullOrEmpty(directory) ? null : directory;
        }
    }
}
