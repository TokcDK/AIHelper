using NLog;
using NLog.Targets;
using System;

namespace AIHelper.Manage.Logging
{
    /// <summary>
    /// NLog target which forwards the log entries of an install to the install window.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The target is added next to the file target of App.config, so an entry reaches
    /// the log file and the window at the same time. The window subscribes to
    /// <see cref="EntryWritten"/> while it is open, which is why the target can stay
    /// registered for the rest of the process and costs nothing when nobody listens.
    /// </para>
    /// <para>
    /// Only <see cref="LogLevel.Info"/> and above reaches the window. The installers call
    /// the mod organizer code, which logs a debug entry for everything it decides against,
    /// and those lines are of no use to the user who is watching an install. The file
    /// target keeps every level, so nothing is lost for a report.
    /// </para>
    /// <para>
    /// Entries are raised on the thread which wrote them, so a handler has to marshal
    /// them to the UI thread itself.
    /// </para>
    /// </remarks>
    [Target("InstallLog")]
    public sealed class InstallLogTarget : TargetWithLayout
    {
        private const string TargetName = "installLog";

        /// <summary>
        /// Lowest level which is forwarded to the window.
        /// </summary>
        private static readonly LogLevel MinimumForwardedLevel = LogLevel.Info;

        /// <summary>
        /// Fired for every log entry with its rendered text.
        /// </summary>
        internal static event Action<string> EntryWritten;

        private static bool _isRegistered;

        protected override void Write(LogEventInfo logEvent)
        {
            // the rule of this target is registered for all levels, so the filter lives
            // here, where it is next to the reason for it
            if (logEvent.Level < MinimumForwardedLevel)
            {
                return;
            }

            var handler = EntryWritten;
            if (handler == null) return;

            handler(RenderLogEvent(Layout, logEvent));
        }

        /// <summary>
        /// Adds the target and its rule to the running NLog configuration, once per
        /// process. Does nothing when NLog has no configuration to extend.
        /// </summary>
        internal static void EnsureRegistered()
        {
            if (_isRegistered) return;

            _isRegistered = true;

            var configuration = LogManager.Configuration;
            if (configuration == null) return;

            var target = new InstallLogTarget
            {
                Name = TargetName,
                Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}${onexception:${newline}${exception:format=tostring}}"
            };

            configuration.AddTarget(target);
            configuration.AddRuleForAllLevels(target);

            // The rules of the loggers which already exist have to be recalculated,
            // otherwise only loggers created from now on would reach the new target.
            LogManager.ReconfigExistingLoggers();
        }
    }
}
