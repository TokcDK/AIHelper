using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace AIHelperUpdater
{
    /// <summary>
    /// Append-only log of one run.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The tool has no window, and it runs after the application that would have shown one has
    /// closed, so this file is the only witness of what happened to the files. It is written
    /// into RES\update, which the update never moves: a log that the update itself could carry
    /// into the backup would be gone exactly when it is needed.
    /// </para>
    /// <para>
    /// Every write is best effort. A log that cannot be written must not stop an update, so a
    /// failure here is swallowed rather than thrown.
    /// </para>
    /// </remarks>
    internal sealed class UpdateLog : IDisposable
    {
        private readonly object _gate = new object();
        private readonly StreamWriter _writer;

        private UpdateLog(string filePath, StreamWriter writer)
        {
            FilePath = filePath;
            _writer = writer;
        }

        /// <summary>Path the log is written to, whether or not it could be opened.</summary>
        internal string FilePath { get; }

        /// <summary>
        /// Opens the log, appending to whatever previous runs left behind.
        /// </summary>
        /// <remarks>
        /// A log that cannot be opened still yields a usable instance: the update has to run
        /// whether or not it can be recorded, and a null return would put that decision at
        /// every call site instead of here.
        /// </remarks>
        /// <param name="filePath">File to append to.</param>
        internal static UpdateLog Open(string filePath)
        {
            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

                var writer = new StreamWriter(filePath, true, new UTF8Encoding(false)) { AutoFlush = true };
                return new UpdateLog(filePath, writer);
            }
            catch (Exception)
            {
                return new UpdateLog(filePath, null);
            }
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

        public void Dispose()
        {
            var writer = _writer;
            if (writer == null) return;

            try
            {
                writer.Dispose();
            }
            catch (Exception)
            {
                // nothing left to do about a log that will not close
            }
        }

        private void Write(string level, string message)
        {
            var writer = _writer;
            if (writer == null) return;

            try
            {
                var line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                           + " " + level + " " + message;

                lock (_gate)
                {
                    writer.WriteLine(line);
                }
            }
            catch (Exception)
            {
                // a log that cannot be written must not stop an update
            }
        }
    }
}
