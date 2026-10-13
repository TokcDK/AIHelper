using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NLog;

namespace AIHelper.Manage.Update.SelfUpdate
{
    /// <summary>
    /// Reads the version of AI Helper out of a program file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The version is not kept anywhere of its own - it is what the build stamped into the
    /// executable - so the file is the source of truth, and asking it is the only way to know
    /// which version is installed.
    /// </para>
    /// <para>
    /// Two files are asked, and which answer is used depends on why the question is being asked.
    /// The check asks for the newer of the two, because that is what the user will be running
    /// the next time this application starts: without the waiting update being counted, every
    /// check would see the running copy as old and download the same release again. Everything
    /// else asks about the running program alone.
    /// </para>
    /// </remarks>
    internal static class ApplicationVersion
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Version to compare releases against: the newer of the update waiting to be applied
        /// and the program this process is running.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Both files are asked and the newer answer wins, rather than the waiting update being
        /// preferred because it is waiting. The folder outlives the session that made it, so the
        /// two can be in either order: an application updated by other means in the meantime
        /// runs a version that folder does not hold, and answering with the folder's would offer
        /// the newer version that is already installed again for ever.
        /// </para>
        /// <para>
        /// A version that cannot be read from one of the two is answered with the other, and with
        /// null only when neither can be read.
        /// </para>
        /// </remarks>
        internal static string ReadInstalled()
        {
            var runningVersion = ReadFromFile(System.Windows.Forms.Application.ExecutablePath);
            var pendingVersion = ReadFromFile(SelfUpdatePaths.PendingApplicationExecutablePath);

            if (string.IsNullOrWhiteSpace(pendingVersion)) return runningVersion;
            if (string.IsNullOrWhiteSpace(runningVersion)) return pendingVersion;

            return UpdateTools.IsNewerOf(pendingVersion, runningVersion) ? pendingVersion : runningVersion;
        }

        /// <summary>
        /// Version of the running program, ignoring anything waiting to be applied.
        /// </summary>
        /// <remarks>
        /// This is what "is the waiting update really newer than what is running" is answered
        /// with, and it must not be the version of the waiting update itself.
        /// </remarks>
        internal static string ReadRunning()
        {
            return ReadFromFile(System.Windows.Forms.Application.ExecutablePath);
        }

        /// <summary>
        /// Reads the version out of a program file.
        /// </summary>
        /// <remarks>
        /// The version resource is written by the build and the three fields of it are not
        /// equally trustworthy: ProductVersion and FileVersion carry what the build stamped in,
        /// while AssemblyVersion is what the file can be bound by. ProductVersion is taken first
        /// because it is the one this application writes itself, and FileVersion is the fallback
        /// for a program that has no product version - Mod Organizer, whose version is read the
        /// same way elsewhere.
        /// </remarks>
        /// <param name="filePath">Program to read.</param>
        /// <returns>The version, or null when the file cannot be read or carries none.</returns>
        internal static string ReadFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return null;

            try
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(filePath);

                var version = FirstUsable(versionInfo.ProductVersion, versionInfo.FileVersion);
                if (version != null) return version;

                return FirstUsable(versionInfo.FileMajorPart.ToString(), null) != null
                    ? FormatVersion(versionInfo)
                    : null;
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Self update: failed to read the version of '" + filePath + "'.");
                return null;
            }
        }

        /// <summary>
        /// The first of two version strings that is worth using, or null.
        /// </summary>
        /// <remarks>
        /// A version resource is a string a build tool filled in, and it is not always only a
        /// version: a "+" suffix naming the commit, or a trailing space, would be compared as
        /// part of it. Only the version itself is kept.
        /// </remarks>
        private static string FirstUsable(string preferred, string fallback)
        {
            return TrimToVersion(preferred) ?? TrimToVersion(fallback);
        }

        /// <summary>
        /// Cuts a version string down to its version part, or returns null when there is none.
        /// </summary>
        private static string TrimToVersion(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            var text = value.Trim();

            var end = text.IndexOfAny(new[] { '+', ' ' });
            if (end > 0) text = text.Substring(0, end);

            return text.Length > 0 ? text : null;
        }

        /// <summary>
        /// Builds "major.minor.build.revision" for a file that carries no version string.
        /// </summary>
        private static string FormatVersion(FileVersionInfo versionInfo)
        {
            var version = new Version(
                versionInfo.FileMajorPart,
                versionInfo.FileMinorPart,
                versionInfo.FileBuildPart,
                versionInfo.FilePrivatePart);

            return version.ToString();
        }
    }
}
