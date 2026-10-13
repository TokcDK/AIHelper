using NLog;
using System;
using System.IO;

namespace AIHelper.Manage.Update
{
    /// <summary>
    /// File system helpers shared by the updater pipeline.
    /// <para>
    /// The updater runs over folders that may be locked by the game, by Mod
    /// Organizer, by an antivirus or by a concurrently running KKManager, so
    /// every operation here is written to never throw: the caller decides what
    /// a failure means. All path comparisons are case-insensitive and
    /// separator-aware, which is required on Windows where
    /// <c>string.Replace</c> based path surgery silently produced wrong paths.
    /// </para>
    /// </summary>
    internal static class UpdateIo
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Deletes a directory if it exists. Never throws.
        /// </summary>
        /// <param name="path">Directory path.</param>
        /// <param name="recursive">
        /// When true the whole tree is removed. When false the call fails
        /// silently if the directory still contains anything.
        /// </param>
        /// <returns>True when the directory does not exist any more.</returns>
        internal static bool TryDeleteDirectory(string path, bool recursive = false)
        {
            if (string.IsNullOrWhiteSpace(path)) return true;

            try
            {
                if (!Directory.Exists(path)) return true;

                Directory.Delete(path, recursive);
                return true;
            }
            catch (DirectoryNotFoundException)
            {
                // already gone
                return true;
            }
            catch (Exception ex)
            {
                _log.Debug("Failed to delete directory '" + path + "'. error:" + Environment.NewLine + ex);
                return !Directory.Exists(path);
            }
        }

        /// <summary>
        /// Deletes a file if it exists. Never throws.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <returns>True when the file does not exist any more.</returns>
        internal static bool TryDeleteFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return true;

            try
            {
                if (!File.Exists(path)) return true;

                File.Delete(path);
                return true;
            }
            catch (FileNotFoundException)
            {
                return true;
            }
            catch (Exception ex)
            {
                _log.Debug("Failed to delete file '" + path + "'. error:" + Environment.NewLine + ex);
                return !File.Exists(path);
            }
        }

        /// <summary>
        /// Copies a directory tree over another one, overwriting the files that
        /// are already there. Never throws.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Used to refresh a deployed tool from the copy a release ships, which is
        /// a copy over files that are already in place rather than a move: the
        /// destination is the folder the tool runs from, so its files have to be
        /// replaced in place, not cleared out first.
        /// </para>
        /// <para>
        /// A file that only exists in the destination is left alone. The source is
        /// a folder inside a downloaded release and the destination is a folder
        /// that may hold anything, so there is no way to tell "a file this tool no
        /// longer has" from "a file that was never this tool's".
        /// </para>
        /// </remarks>
        /// <param name="sourceDirectoryPath">Directory to copy from.</param>
        /// <param name="destinationDirectoryPath">Directory to copy into.</param>
        /// <returns>True when every file was copied.</returns>
        internal static bool TryCopyDirectory(string sourceDirectoryPath, string destinationDirectoryPath)
        {
            if (string.IsNullOrWhiteSpace(sourceDirectoryPath)
                || string.IsNullOrWhiteSpace(destinationDirectoryPath))
            {
                return false;
            }

            try
            {
                Directory.CreateDirectory(destinationDirectoryPath);

                foreach (var directoryPath in Directory.GetDirectories(sourceDirectoryPath))
                {
                    var childDestinationPath = Path.Combine(
                        destinationDirectoryPath, Path.GetFileName(directoryPath));

                    if (!TryCopyDirectory(directoryPath, childDestinationPath)) return false;
                }

                foreach (var filePath in Directory.GetFiles(sourceDirectoryPath))
                {
                    var destinationFilePath = Path.Combine(
                        destinationDirectoryPath, Path.GetFileName(filePath));

                    File.Copy(filePath, destinationFilePath, true);
                }

                return true;
            }
            catch (Exception ex)
            {
                _log.Debug("Failed to copy directory '" + sourceDirectoryPath + "' to '"
                           + destinationDirectoryPath + "'. error:" + Environment.NewLine + ex);
                return false;
            }
        }

        /// <summary>
        /// Returns the length of a file, or 0 when the file is missing or
        /// unreadable. Replaces <c>new FileInfo(path).Length</c>, which throws
        /// when the file disappears between an existence check and the read.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <returns>File length in bytes, or 0.</returns>
        internal static long GetFileLengthOrZero(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return 0;

            try
            {
                var info = new FileInfo(path);
                return info.Exists ? info.Length : 0;
            }
            catch (Exception ex)
            {
                _log.Debug("Failed to read length of '" + path + "'. error:" + Environment.NewLine + ex);
                return 0;
            }
        }

        /// <summary>
        /// Case-insensitive, separator-aware "is inside" test.
        /// </summary>
        /// <param name="path">Path to test.</param>
        /// <param name="rootPath">Candidate parent directory.</param>
        /// <returns>True when <paramref name="path"/> equals or is below <paramref name="rootPath"/>.</returns>
        internal static bool IsUnderPath(string path, string rootPath)
        {
            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(rootPath)) return false;

            var normalizedPath = NormalizeSeparators(path);
            var normalizedRoot = NormalizeSeparators(rootPath).TrimEnd(Path.DirectorySeparatorChar);

            if (string.Equals(normalizedPath, normalizedRoot, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return normalizedPath.StartsWith(
                normalizedRoot + Path.DirectorySeparatorChar,
                StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Returns the part of <paramref name="path"/> that lies below
        /// <paramref name="rootPath"/>, or the plain file name when the path is
        /// not under the root. Used instead of <c>string.Replace</c> to remap a
        /// path from one root to another while keeping the sub-tree.
        /// </summary>
        /// <param name="path">Source path.</param>
        /// <param name="rootPath">Root to strip.</param>
        /// <returns>Relative sub path (never rooted, never null).</returns>
        internal static string GetRelativeSubPath(string path, string rootPath)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;

            if (string.IsNullOrWhiteSpace(rootPath)) return Path.GetFileName(path);

            var normalizedPath = NormalizeSeparators(path);
            var normalizedRoot = NormalizeSeparators(rootPath).TrimEnd(Path.DirectorySeparatorChar);

            if (normalizedPath.StartsWith(
                normalizedRoot + Path.DirectorySeparatorChar,
                StringComparison.InvariantCultureIgnoreCase))
            {
                return normalizedPath.Substring(normalizedRoot.Length + 1);
            }

            if (string.Equals(normalizedPath, normalizedRoot, StringComparison.InvariantCultureIgnoreCase))
            {
                return string.Empty;
            }

            return Path.GetFileName(path);
        }

        /// <summary>
        /// Remaps <paramref name="path"/> from <paramref name="sourceRoot"/> to
        /// <paramref name="targetRoot"/>, keeping the sub-tree.
        /// </summary>
        /// <param name="path">Source path.</param>
        /// <param name="sourceRoot">Root the path currently lives under.</param>
        /// <param name="targetRoot">Root to move the sub-tree to.</param>
        /// <returns>The remapped path.</returns>
        internal static string RemapPath(string path, string sourceRoot, string targetRoot)
        {
            return Path.Combine(targetRoot, GetRelativeSubPath(path, sourceRoot));
        }

        /// <summary>
        /// Creates the parent directory of <paramref name="path"/> when it is
        /// not a root. Never throws.
        /// </summary>
        /// <param name="path">File or directory path.</param>
        /// <returns>True when the parent directory exists afterwards.</returns>
        internal static bool EnsureParentDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;

            var parent = Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(parent)) return true; // path is already a root

            try
            {
                Directory.CreateDirectory(parent);
                return true;
            }
            catch (Exception ex)
            {
                _log.Debug("Failed to create parent directory of '" + path + "'. error:" + Environment.NewLine + ex);
                return false;
            }
        }

        private static string NormalizeSeparators(string path)
        {
            return path
                .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
                .TrimEnd(Path.DirectorySeparatorChar);
        }
    }
}
