using System;
using System.Diagnostics;
using System.IO;

namespace AIHelperUpdater
{
    /// <summary>
    /// Waits for the application to end, and starts it again afterwards.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Both halves exist because the update is started from the application that is being
    /// replaced. It hands the work over and closes - so the first thing this program does is
    /// wait for that process, and the last thing it does is start it again.
    /// </para>
    /// <para>
    /// The wait is on the process id it was given, and not on the name: the user may have a
    /// second copy somewhere, and waiting for "an AI Helper" to end when the one that is closing
    /// has already closed would make the tool wait for a program that is not involved. The name
    /// is used for one thing only - confirming that the id still belongs to the process that was
    /// handed over, because an id can be reused the moment a process ends.
    /// </para>
    /// </remarks>
    internal static class HostProcessWatcher
    {
        /// <summary>
        /// Waits for a process to end.
        /// </summary>
        /// <param name="processId">Id of the process to wait for.</param>
        /// <param name="expectedProcessName">
        /// Name the id is expected to still belong to, or null to skip that check.
        /// </param>
        /// <param name="timeout">How long to wait before giving up.</param>
        /// <param name="log">Where the outcome is recorded.</param>
        /// <returns>
        /// True when the process is no longer running - which includes having already ended, the
        /// case the tool most often sees.
        /// </returns>
        internal static bool WaitForExit(int processId, string expectedProcessName, TimeSpan timeout, UpdateLog log)
        {
            Process host = null;

            try
            {
                host = Process.GetProcessById(processId);
            }
            catch (ArgumentException)
            {
                // No process with that id: it ended before this program got here, which is the
                // normal case - the application starts the tool and closes immediately.
                log.Info("process " + processId + " has already ended.");
                return true;
            }
            catch (InvalidOperationException)
            {
                log.Info("process " + processId + " has already ended.");
                return true;
            }
            catch (Exception ex)
            {
                log.Error("process " + processId + " could not be opened: " + ex.Message);
                return false;
            }

            using (host)
            {
                if (!BelongsTo(host, expectedProcessName, log)) return true;

                log.Info("waiting for process " + processId + " to end (up to "
                         + (int)timeout.TotalSeconds + "s).");

                try
                {
                    if (host.WaitForExit((int)timeout.TotalMilliseconds))
                    {
                        log.Info("process " + processId + " ended.");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    log.Error("waiting for process " + processId + " failed: " + ex.Message);
                    return false;
                }
            }

            log.Warn("process " + processId + " is still running after "
                     + (int)timeout.TotalSeconds + "s; nothing was changed.");
            return false;
        }

        /// <summary>
        /// Starts a program again, detached from this one.
        /// </summary>
        /// <remarks>
        /// The shell is used to start it so that the new process is not tied to this one: this
        /// process is about to end, and the application has to outlive it.
        /// </remarks>
        /// <param name="executablePath">Program to start.</param>
        /// <param name="workingDirectory">Directory to start it in.</param>
        /// <param name="log">Where the outcome is recorded.</param>
        internal static void Start(string executablePath, string workingDirectory, UpdateLog log)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    UseShellExecute = true
                };

                if (Directory.Exists(workingDirectory)) startInfo.WorkingDirectory = workingDirectory;

                using (var started = Process.Start(startInfo))
                {
                    if (started == null)
                    {
                        log.Error("'" + executablePath + "' could not be started.");
                        return;
                    }
                }

                log.Info("started '" + executablePath + "' again.");
            }
            catch (Exception ex)
            {
                log.Error("'" + executablePath + "' could not be started: " + ex.Message);
            }
        }

        /// <summary>
        /// True when the id still belongs to the process that was handed over.
        /// </summary>
        /// <remarks>
        /// An id that has been reused would otherwise be waited on for as long as the unrelated
        /// program that took it happens to run. A name that cannot be read means the process has
        /// ended, and that is what the wait wanted anyway.
        /// </remarks>
        private static bool BelongsTo(Process process, string expectedProcessName, UpdateLog log)
        {
            if (string.IsNullOrWhiteSpace(expectedProcessName)) return true;

            string actualName;
            try
            {
                actualName = process.ProcessName;
            }
            catch (Exception)
            {
                log.Info("process " + process.Id + " ended before it could be looked at.");
                return false;
            }

            if (string.Equals(actualName, expectedProcessName, StringComparison.OrdinalIgnoreCase)) return true;

            log.Warn("process " + process.Id + " is '" + actualName + "' and not '" + expectedProcessName
                     + "'; the application has already ended.");
            return false;
        }
    }
}
