using System;
using System.Globalization;
using System.IO;
using System.Security;
using System.Text;

namespace AIHelperLaunchAssistant
{
    /// <summary>
    /// A log file small enough not to need a logging framework.
    /// </summary>
    /// <remarks>
    /// The application uses NLog, but taking it would drag an assembly and its configuration into a
    /// process whose whole point is to be small. Every write is best effort: the assistant must not
    /// fail because the folder it lives in happens to be read only.
    /// </remarks>
    internal sealed class ToolLog
    {
        internal const string FileName = "aihelper-launch-assistant.log";

        /// <summary>Size at which the log starts over, so a folder that is used for years does not grow forever.</summary>
        private const long MaximumLengthBytes = 64 * 1024;

        private readonly object _sync = new object();
        private readonly string _path;
        private bool _disabled;

        private ToolLog(string path)
        {
            _path = path;
        }

        /// <summary>Opens the log beside the tool, or returns a log that writes nowhere.</summary>
        internal static ToolLog Create(string assistantExecutablePath)
        {
            string directory = string.IsNullOrWhiteSpace(assistantExecutablePath)
                ? null
                : Path.GetDirectoryName(assistantExecutablePath);

            // null at a drive root, "" for a bare name: neither can hold a log file.
            return new ToolLog(string.IsNullOrEmpty(directory) ? null : Path.Combine(directory, FileName));
        }

        internal void Info(string message)
        {
            Write("INFO ", message);
        }

        internal void Warn(string message)
        {
            Write("WARN ", message);
        }

        internal void Error(string message)
        {
            Write("ERROR", message);
        }

        private void Write(string level, string message)
        {
            if (_disabled || _path == null)
            {
                return;
            }

            try
            {
                lock (_sync)
                {
                    RestartIfTooLong();

                    string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                        + " " + level + " " + message + Environment.NewLine;

                    File.AppendAllText(_path, line, Encoding.UTF8);
                }
            }
            catch (IOException)
            {
                _disabled = true;
            }
            catch (UnauthorizedAccessException)
            {
                _disabled = true;
            }
            catch (SecurityException)
            {
                _disabled = true;
            }
        }

        private void RestartIfTooLong()
        {
            var file = new FileInfo(_path);
            if (file.Exists && file.Length > MaximumLengthBytes)
            {
                file.Delete();
            }
        }
    }
}
