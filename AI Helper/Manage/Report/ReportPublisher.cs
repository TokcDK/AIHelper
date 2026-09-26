using System;
using System.Diagnostics;
using System.IO;
using NLog;

namespace AIHelper.Manage.Report
{
    /// <summary>
    /// Writes a finished report to disk and hands it to the shell, which opens
    /// it in whatever application is registered for the file type. This is the
    /// only part of the report code that touches the outside world, so the
    /// rest of it stays free of side effects.
    /// </summary>
    internal static class ReportPublisher
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Writes the report and opens it in the default application.
        /// </summary>
        /// <param name="filePath">File to write, its folder is created when needed.</param>
        /// <param name="content">Finished report.</param>
        /// <returns>True when the report was written and handed to the shell.</returns>
        internal static bool PublishAndOpen(string filePath, string content)
        {
            if (!TryWrite(filePath, content)) return false;

            return TryOpen(filePath);
        }

        /// <summary>Writes the report, creating the parent folder when needed.</summary>
        internal static bool TryWrite(string filePath, string content)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return false;

            try
            {
                var directoryPath = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                File.WriteAllText(filePath, content);
                return true;
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Report: failed to write '" + filePath + "'.");
                return false;
            }
        }

        /// <summary>Opens the report with the application registered for it.</summary>
        internal static bool TryOpen(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return false;

            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = filePath;
                    process.Start();
                }

                return true;
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Report: failed to open '" + filePath + "'.");
                return false;
            }
        }
    }
}
