using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using AIHelper.Manage.ModeSwitch.DataFullBuilder.Interfaces;

namespace AIHelper.Manage.ModeSwitch.DataFullBuilder.Services
{
    public sealed class ErrorLogger : IErrorLogger
    {
        private readonly string _logFilePath;
        private readonly ConcurrentQueue<string> _errorQueue;
        private readonly object _flushLock = new object();
        private readonly StringBuilder _buffer;
        private int _errorCount;
        private bool _disposed;

        private const int BufferFlushThreshold = 100;

        public int ErrorCount => _errorCount;

        public ErrorLogger(string logFilePath)
        {
            _logFilePath = logFilePath ?? throw new ArgumentNullException(nameof(logFilePath));
            _errorQueue = new ConcurrentQueue<string>();
            _buffer = new StringBuilder(4096);

            var directory = Path.GetDirectoryName(_logFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Write header
            File.WriteAllText(_logFilePath, 
                $"# DataFullBuilder Error Log{Environment.NewLine}" +
                $"# Created: {DateTime.Now:yyyy-MM-dd HH:mm:ss}{Environment.NewLine}" +
                $"# Format: [Timestamp] [Operation] [Path] - Error{Environment.NewLine}" +
                $"{new string('=', 80)}{Environment.NewLine}",
                Encoding.UTF8);
        }

        public void LogError(string relativePath, string operation, Exception exception)
        {
            LogError(relativePath, operation, exception?.Message ?? "Unknown error");
        }

        public void LogError(string relativePath, string operation, string message)
        {
            Interlocked.Increment(ref _errorCount);

            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] [{operation}] {relativePath} - {message}";
            
            _errorQueue.Enqueue(logEntry);

            if (_errorQueue.Count >= BufferFlushThreshold)
            {
                Flush();
            }
        }

        public void Flush()
        {
            if (_errorQueue.IsEmpty) return;

            lock (_flushLock)
            {
                _buffer.Clear();

                while (_errorQueue.TryDequeue(out var entry))
                {
                    _buffer.AppendLine(entry);
                }

                if (_buffer.Length > 0)
                {
                    File.AppendAllText(_logFilePath, _buffer.ToString(), Encoding.UTF8);
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            
            Flush();
            _disposed = true;
        }
    }
}
