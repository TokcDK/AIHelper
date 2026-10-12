using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AIHelperLaunchAssistant
{
    /// <summary>
    /// Entry point of aihelper-launch-assistant.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The tool lets a game or a studio be started without the application that manages it staying
    /// resident: it waits for AI Helper to close, starts the requested program, keeps an icon in the
    /// notification area while that program runs and starts AI Helper again when it ends.
    /// </para>
    /// <para>
    /// It is deliberately thin - no WinForms, no System.Drawing, no logging framework - because it is
    /// meant to be resident for as long as a game session lasts, and every framework assembly it would
    /// load costs memory the whole time.
    /// </para>
    /// </remarks>
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                return Run(args);
            }
            catch (Exception exception)
            {
                // Nothing below is expected to throw. The assistant is started by another program
                // though, and a silent failure would leave the user with no game and no window, so
                // whatever went wrong is written down before the process goes away.
                ToolLog.Create(ApplicationInfo.ExecutablePath).Error("Unexpected failure: " + exception);
                return ExitCodes.UnexpectedError;
            }
        }

        private static int Run(string[] args)
        {
            ParseResult parse = CommandLineParser.Parse(args);

            if (parse.Status == ParseStatus.HelpRequested)
            {
                ConsoleBridge.Show(CommandLineParser.BuildUsageText());
                return ExitCodes.Success;
            }

            if (parse.Status == ParseStatus.VersionRequested)
            {
                ConsoleBridge.Show(ApplicationInfo.Name + " " + ApplicationInfo.Version + Environment.NewLine);
                return ExitCodes.Success;
            }

            if (!parse.IsSuccess)
            {
                // No message box here: a command line this bad comes from the program that started the
                // assistant far more often than from a person, and a modal box would hang it.
                ConsoleBridge.TryWrite(parse.Message + Environment.NewLine + Environment.NewLine + CommandLineParser.BuildUsageText());
                return ExitCodes.UsageError;
            }

            return Launch(parse.Options);
        }

        private static int Launch(LaunchOptions options)
        {
            string assistantExecutablePath = ApplicationInfo.ExecutablePath;
            ToolLog log = ToolLog.Create(assistantExecutablePath);

            log.Info(ApplicationInfo.Name + " " + ApplicationInfo.Version + " started: target=\"" + options.Target.Path + "\"");

            var assistant = new LaunchAssistant(
                options,
                assistantExecutablePath,
                new ProcessLauncher(log),
                new ProcessWaiter(),
                log);

            AssistantOutcome outcome = options.ShowTrayIcon
                ? RunWithTray(assistant, options, log)
                : assistant.Run(NoAssistantUi.Instance);

            log.Info("Finished: " + outcome + ".");
            return MapExitCode(outcome);
        }

        private static AssistantOutcome RunWithTray(LaunchAssistant assistant, LaunchOptions options, ToolLog log)
        {
            using (var tray = new TrayIcon(options.Host.ProcessName, log))
            {
                if (!tray.TryShow())
                {
                    // No window means no message loop to run, so the sequence is run straight away: the
                    // icon is a convenience, and losing it must not cost the user their game.
                    log.Warn("Running without a notification area icon.");
                    return assistant.Run(NoAssistantUi.Instance);
                }

                // The wait blocks, so it gets a thread of its own: the message loop below has to stay
                // responsive or the icon's menu would never open.
                Task<AssistantOutcome> workflow = Task.Factory.StartNew(
                    () => assistant.Run(tray),
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);

                _ = workflow.ContinueWith(
                    completed => tray.Stop(),
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);

                tray.RunMessageLoop();

                return workflow.GetAwaiter().GetResult();
            }
        }

        private static int MapExitCode(AssistantOutcome outcome)
        {
            switch (outcome)
            {
                case AssistantOutcome.TargetExited:
                case AssistantOutcome.HostRestartedOnRequest:
                    return ExitCodes.Success;

                case AssistantOutcome.CancelledByUser:
                    return ExitCodes.CancelledByUser;

                case AssistantOutcome.TargetStartFailed:
                    return ExitCodes.TargetStartFailed;

                case AssistantOutcome.HostStartFailed:
                    return ExitCodes.HostStartFailed;

                default:
                    return ExitCodes.UnexpectedError;
            }
        }
    }
}
