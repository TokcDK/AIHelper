using System;

namespace AIHelperLaunchAssistant
{
    /// <summary>
    /// The application the assistant waits for before starting the target, and starts again once the
    /// target has ended.
    /// </summary>
    /// <remarks>
    /// Immutable: it is built once from the command line and only read afterwards.
    /// </remarks>
    internal sealed class HostProgram
    {
        private readonly bool _processNameWasGiven;

        /// <param name="executablePath">
        /// Program to start again, or <c>null</c> to work it out from the assistant's own location.
        /// </param>
        /// <param name="processId">Id of the instance to wait for, or <c>0</c> when it is not pinned to one.</param>
        /// <param name="processName">
        /// Process name to wait for, or <c>null</c> to use the host application's own name.
        /// </param>
        /// <param name="arguments">Arguments for the program when it is started again; empty for none.</param>
        /// <param name="restart">Whether the program is started again when the target ends.</param>
        /// <param name="waitTimeout">
        /// How long to wait for the host to close before starting the target anyway.
        /// <see cref="TimeSpan.Zero"/> waits without a deadline.
        /// </param>
        internal HostProgram(string executablePath, int processId, string processName, string[] arguments, bool restart, TimeSpan waitTimeout)
        {
            ExecutablePath = string.IsNullOrWhiteSpace(executablePath) ? null : executablePath;

            _processNameWasGiven = !string.IsNullOrWhiteSpace(processName);
            ProcessId = processId;
            ProcessName = _processNameWasGiven ? ProcessNames.Normalize(processName) : HostApplicationLocator.HostProcessName;
            Arguments = arguments ?? new string[0];
            Restart = restart;
            WaitTimeout = waitTimeout;
        }

        /// <summary>Program to start again, or <c>null</c> to work it out from the assistant's own location.</summary>
        internal string ExecutablePath { get; }

        /// <summary>Id of the instance to wait for, or <c>0</c> when the wait is by process name.</summary>
        internal int ProcessId { get; }

        /// <summary>Process name to wait for.</summary>
        internal string ProcessName { get; }

        /// <summary>Arguments for the program when it is started again; empty when it takes none.</summary>
        internal string[] Arguments { get; }

        /// <summary>Whether the program is started again when the target ends.</summary>
        internal bool Restart { get; }

        /// <summary>How long to wait for the host to close; <see cref="TimeSpan.Zero"/> for no deadline.</summary>
        internal TimeSpan WaitTimeout { get; }

        /// <summary>The process (or the set of processes) the assistant waits for.</summary>
        internal ProcessTarget ToProcessTarget()
        {
            if (ProcessId > 0)
            {
                // An id with no name to check it against still has to be waited for: refusing to wait
                // would start the target while the host is still running, which is worse than the
                // small chance that the id has since been given to another process.
                return ProcessTarget.ById(ProcessId, _processNameWasGiven ? ProcessName : string.Empty);
            }

            return ProcessTarget.ByName(ProcessName);
        }
    }
}
