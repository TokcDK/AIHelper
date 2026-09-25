using NLog;
using System;

namespace AIHelper.Install
{
    /// <summary>
    /// Reports how far an install run has come, so one window can show which step is
    /// running and how many entries of it are already processed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The installers are created by reflection and are driven through
    /// <see cref="Types.ModInstallerBase"/>, so they can not be handed a progress
    /// object. They report to this broker instead, and whoever is interested
    /// subscribes to its events (see <c>Forms.Other.InstallProgressForm</c>).
    /// </para>
    /// <para>
    /// Every member is safe to call when nobody listens, which is what happens when
    /// the installers are driven by something else than the install window.
    /// </para>
    /// </remarks>
    internal static class InstallProgress
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Number of steps of the run which is going on, 0 when no run is active.
        /// </summary>
        private static int _stepCount;

        /// <summary>
        /// Depth of the entry loops which are currently running. A step can run
        /// nested loops, because the installer of the 'f' folder delegates the 'm'
        /// sub folder to the installer of the 'm' folder.
        /// </summary>
        private static int _stepScopeDepth;

        /// <summary>
        /// Fired when a run starts, with the number of steps it contains.
        /// </summary>
        internal static event Action<int> RunStarted;

        /// <summary>
        /// Fired when a run ended, no matter whether it succeeded.
        /// </summary>
        internal static event Action RunFinished;

        /// <summary>
        /// Fired when a step starts. Arguments: step number (1 based), step count,
        /// step name.
        /// </summary>
        internal static event Action<int, int, string> StepStarted;

        /// <summary>
        /// Fired while a step runs. Arguments: processed entries, entry count, name of
        /// the entry which is being processed.
        /// </summary>
        internal static event Action<int, int, string> StepProgressChanged;

        /// <summary>
        /// Marks the beginning of a run.
        /// </summary>
        /// <param name="stepCount">Number of steps the run contains</param>
        internal static void BeginRun(int stepCount)
        {
            _stepCount = stepCount < 0 ? 0 : stepCount;
            _stepScopeDepth = 0;

            _log.Info("Install started. Steps: " + _stepCount);

            RunStarted?.Invoke(_stepCount);
        }

        /// <summary>
        /// Marks the end of a run.
        /// </summary>
        internal static void EndRun()
        {
            _stepScopeDepth = 0;

            _log.Info("Install finished.");

            RunFinished?.Invoke();
        }

        /// <summary>
        /// Marks the beginning of a step.
        /// </summary>
        /// <param name="stepNumber">Number of the step, 1 based</param>
        /// <param name="stepName">Name of the step shown to the user</param>
        internal static void BeginStep(int stepNumber, string stepName)
        {
            _log.Info("Step " + stepNumber + "/" + _stepCount + ": " + stepName);

            StepStarted?.Invoke(stepNumber, _stepCount, stepName);
        }

        /// <summary>
        /// Reports how many entries of the running step are processed.
        /// </summary>
        /// <remarks>
        /// Only the outermost entry loop of a step is reported. A step can run a nested
        /// loop (the installer of the 'f' folder delegates the 'm' sub folder to the
        /// installer of the 'm' folder), and its progress would make the bar of the
        /// step jump back and forth. The check lives here rather than at the call site
        /// so a new caller can not get it wrong.
        /// </remarks>
        /// <param name="processedCount">Number of processed entries</param>
        /// <param name="entryCount">Number of entries of the running mask</param>
        /// <param name="entryName">Name of the entry which is being processed</param>
        internal static void ReportStepProgress(int processedCount, int entryCount, string entryName)
        {
            if (_stepScopeDepth != 1) return;

            StepProgressChanged?.Invoke(processedCount, entryCount, entryName);
        }

        /// <summary>
        /// Marks the beginning of the entry loop of one installer.
        /// </summary>
        internal static void EnterStepScope()
        {
            _stepScopeDepth++;
        }

        /// <summary>
        /// Marks the end of the entry loop of one installer.
        /// </summary>
        internal static void ExitStepScope()
        {
            if (_stepScopeDepth > 0) _stepScopeDepth--;
        }
    }
}
