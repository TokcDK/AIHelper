using AIHelper.Shared;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace AIHelperLaunchAssistant
{
    /// <summary>
    /// The launch sequence itself: wait for the host, start the target, wait for the target, start the
    /// host again.
    /// </summary>
    /// <remarks>
    /// Deliberately free of windows and of the notification area. It takes the user's requests through
    /// <see cref="IAssistantUi"/> and reports what it did through <see cref="ToolLog"/>, so the whole
    /// sequence can be driven with nothing on screen.
    /// </remarks>
    internal sealed class LaunchAssistant
    {
        private readonly LaunchOptions _options;
        private readonly IProcessLauncher _launcher;
        private readonly IProcessWaiter _waiter;
        private readonly ToolLog _log;
        private readonly string _hostExecutablePath;

        internal LaunchAssistant(LaunchOptions options, string assistantExecutablePath, IProcessLauncher launcher, IProcessWaiter waiter, ToolLog log)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options;
            _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));
            _waiter = waiter ?? throw new ArgumentNullException(nameof(waiter));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _hostExecutablePath = options.Host.ExecutablePath ?? HostApplicationLocator.Find(assistantExecutablePath);

            // Reported now rather than when the target ends: finding out that the host cannot be
            // started again hours later, with the game already closed, is finding out too late.
            if (_hostExecutablePath == null && options.Host.Restart)
            {
                _log.Warn("Could not find " + HostApplicationLocator.HostExecutableName + " near \""
                    + assistantExecutablePath + "\"; " + options.Host.ProcessName + " will not be started again.");
            }
        }

        /// <summary>Runs the whole sequence and reports how it ended.</summary>
        internal AssistantOutcome Run(IAssistantUi ui)
        {
            if (ui == null)
            {
                throw new ArgumentNullException(nameof(ui));
            }

            if (!File.Exists(_options.Target.Path))
            {
                _log.Error("The target program does not exist: " + _options.Target.Path);
                return AssistantOutcome.TargetStartFailed;
            }

            AssistantOutcome? stoppedWhileWaitingForHost = WaitForHost(ui);
            if (stoppedWhileWaitingForHost.HasValue)
            {
                return stoppedWhileWaitingForHost.Value;
            }

            Process target = StartTarget();
            if (target == null)
            {
                return RecoverFromTargetStartFailure();
            }

            UserRequest? request;
            using (target)
            {
                ui.TargetStarted(_options.Target.DisplayName);
                _log.Info("Started " + _options.Target.Path + "; waiting for it to end.");

                if (_waiter.WaitForExit(target, TimeSpan.Zero, ui.TakeRequest, out request))
                {
                    _log.Info("The target ended.");
                    return StartHost() ? AssistantOutcome.TargetExited : AssistantOutcome.HostStartFailed;
                }
            }

            // This wait has no deadline, so the only way it stops early is a request from the user.
            return HandleUserRequest(request.Value);
        }

        /// <summary>
        /// Waits for the host to close.
        /// </summary>
        /// <returns>
        /// <c>null</c> when the target may be started; otherwise the outcome the run should report.
        /// </returns>
        private AssistantOutcome? WaitForHost(IAssistantUi ui)
        {
            ProcessTarget host = _options.Host.ToProcessTarget();
            _log.Info("Waiting for " + host + " to end.");

            UserRequest? request;
            if (_waiter.WaitForExit(host, _options.Host.WaitTimeout, ui.TakeRequest, out request))
            {
                return null;
            }

            if (request.HasValue)
            {
                // The user asked for something while the host was still closing.
                return HandleUserRequest(request.Value);
            }

            // The wait stopped with nothing asked, so it ran out of time. Starting the target late is
            // better than not starting it at all: the host may be stuck rather than working.
            _log.Warn("Gave up waiting for " + host + " after " + Describe(_options.Host.WaitTimeout) + "; starting the target anyway.");
            return null;
        }

        private AssistantOutcome HandleUserRequest(UserRequest request)
        {
            if (request == UserRequest.ExitWithoutRestartingHost)
            {
                _log.Info("The user closed the assistant; the host will not be started again.");
                return AssistantOutcome.CancelledByUser;
            }

            _log.Info("The user asked for the host to be started now.");
            return StartHost() ? AssistantOutcome.HostRestartedOnRequest : AssistantOutcome.HostStartFailed;
        }

        private Process StartTarget()
        {
            // The target is usually a game, and letting go of everything this process no longer needs
            // before it starts is the reason the assistant exists instead of the host staying resident.
            GC.Collect();
            GC.WaitForPendingFinalizers();

            try
            {
                return _launcher.Start(_options.Target.Path, _options.Target.Arguments, _options.Target.WorkingDirectory);
            }
            catch (Exception exception) when (IsStartFailure(exception))
            {
                _log.Error("Failed to start " + _options.Target.Path + ": " + exception.Message);
                return null;
            }
        }

        /// <summary>
        /// The target could not be started and the host is already gone. Starting the host again is the
        /// only way out of that: leaving it closed would strand the user with nothing running.
        /// </summary>
        private AssistantOutcome RecoverFromTargetStartFailure()
        {
            StartHost();
            return AssistantOutcome.TargetStartFailed;
        }

        private bool StartHost()
        {
            if (!_options.Host.Restart)
            {
                _log.Info("Starting the host again is disabled; nothing was started.");
                return true;
            }

            if (_hostExecutablePath == null)
            {
                _log.Error("The host executable could not be found, so nothing was started.");
                return false;
            }

            try
            {
                Process host = _launcher.Start(
                    _hostExecutablePath,
                    ArgumentQuoting.Join(_options.Host.Arguments),
                    Path.GetDirectoryName(_hostExecutablePath));

                if (host == null)
                {
                    return false;
                }

                // Disposed straight away: the process keeps running, and only the handle this process
                // has no further use for goes away.
                host.Dispose();

                _log.Info("Started " + _hostExecutablePath + " again.");
                return true;
            }
            catch (Exception exception) when (IsStartFailure(exception))
            {
                _log.Error("Failed to start " + _hostExecutablePath + ": " + exception.Message);
                return false;
            }
        }

        /// <summary>
        /// Whether a failure to start a process is one that can be reported, rather than one that has
        /// to take the assistant down with it.
        /// </summary>
        private static bool IsStartFailure(Exception exception)
        {
            return exception is Win32Exception
                || exception is InvalidOperationException
                || exception is ObjectDisposedException
                || exception is FileNotFoundException
                || exception is DirectoryNotFoundException;
        }

        private static string Describe(TimeSpan timeout)
        {
            return timeout.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " s";
        }
    }
}
