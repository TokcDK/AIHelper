using System;
using System.Diagnostics;

namespace AIHelperLaunchAssistant
{
    /// <summary>
    /// Identifies the process - or the set of processes - the assistant waits for.
    /// </summary>
    /// <remarks>
    /// A pinned id is the better answer when it is available: waiting for every process with the
    /// host's name also waits for a second copy the user happened to start, and a name can be given to
    /// any executable. The name is still carried alongside an id, because ids are recycled and the one
    /// that was recorded may belong to something else by the time the wait begins.
    /// </remarks>
    internal sealed class ProcessTarget
    {
        private ProcessTarget(int processId, string processName)
        {
            ProcessId = processId;
            ProcessName = ProcessNames.Normalize(processName);
        }

        /// <summary>Id of the process to wait for, or <c>0</c> when the wait is by name.</summary>
        internal int ProcessId { get; }

        /// <summary>Process name, without the ".exe" suffix; empty when the target is only an id.</summary>
        internal string ProcessName { get; }

        /// <summary>Whether the wait is pinned to one process.</summary>
        internal bool HasProcessId
        {
            get { return ProcessId > 0; }
        }

        /// <summary>Waits for one process, whose name is checked as well so a recycled id is not mistaken for it.</summary>
        internal static ProcessTarget ById(int processId, string processName)
        {
            if (processId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(processId), processId, "A process id has to be positive.");
            }

            return new ProcessTarget(processId, processName);
        }

        /// <summary>Waits for every process with this name.</summary>
        internal static ProcessTarget ByName(string processName)
        {
            string normalized = ProcessNames.Normalize(processName);
            if (normalized.Length == 0)
            {
                throw new ArgumentException("A process name is required.", nameof(processName));
            }

            return new ProcessTarget(0, normalized);
        }

        public override string ToString()
        {
            if (HasProcessId)
            {
                return ProcessName.Length == 0
                    ? "process " + ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    : ProcessName + " (process " + ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")";
            }

            return "process named " + ProcessName;
        }
    }

    /// <summary>Process name handling, in one place so the two sides of a name comparison agree.</summary>
    internal static class ProcessNames
    {
        private const string ExecutableSuffix = ".exe";

        /// <summary>
        /// Strips the ".exe" suffix, because that is the form Process.GetProcessesByName expects and
        /// the form Process.ProcessName returns. A name given either way therefore compares equal.
        /// </summary>
        internal static string Normalize(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
            {
                return string.Empty;
            }

            string trimmed = processName.Trim();
            if (trimmed.EndsWith(ExecutableSuffix, StringComparison.OrdinalIgnoreCase))
            {
                trimmed = trimmed.Substring(0, trimmed.Length - ExecutableSuffix.Length);
            }

            return trimmed;
        }

        /// <summary>
        /// Reads the name of a running process, or an empty string when there is no such process.
        /// </summary>
        internal static string TryGetRunningProcessName(int processId)
        {
            try
            {
                using (Process process = Process.GetProcessById(processId))
                {
                    return process.ProcessName;
                }
            }
            catch (ArgumentException)
            {
                // No process with that id.
                return string.Empty;
            }
            catch (InvalidOperationException)
            {
                // The process ended between the lookup and the read.
                return string.Empty;
            }
        }
    }
}
