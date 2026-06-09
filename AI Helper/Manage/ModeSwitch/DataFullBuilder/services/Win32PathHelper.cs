using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AIHelper.Manage.ModeSwitch.DataFullBuilder.Services
{
    public static class Win32PathHelper
    {
        #region Win32 API Constants and Imports

        private const uint FILE_READ_ATTRIBUTES = 0x0080;
        private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
        private const uint OPEN_EXISTING = 3;
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;
        private const uint FILE_SHARE_DELETE = 0x00000004;
        private const uint VOLUME_NAME_DOS = 0x0;

        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateFileW(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern uint GetFinalPathNameByHandleW(
            IntPtr hFile,
            StringBuilder lpszFilePath,
            uint cchFilePath,
            uint dwFlags);

        #endregion

        public const string LongPathPrefix = @"\\?\";
        public const string UNCPathPrefix = @"\\?\UNC\";

        /// <summary>
        /// Gets the drive root of a path, resolving any symbolic links or junction points.
        /// Unlike Path.GetPathRoot, this method follows symbolic links to determine
        /// the actual physical location's drive root.
        /// </summary>
        /// <param name="path">The path to analyze.</param>
        /// <returns>The root of the drive where the path actually resides.</returns>
        /// <exception cref="ArgumentNullException">Thrown when path is null or empty.</exception>
        public static string GetDriveRoot(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            // Normalize to full path
            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(path);
            }
            catch (Exception)
            {
                // If we can't get full path, return standard root
                return Path.GetPathRoot(path);
            }

            // Try to resolve symlinks walking up the directory tree
            string resolvedPath = ResolveToExistingPath(fullPath);

            return Path.GetPathRoot(resolvedPath ?? fullPath);
        }

        /// <summary>
        /// Walks up the directory tree to find an existing path and resolves it.
        /// </summary>
        private static string ResolveToExistingPath(string path)
        {
            string currentPath = path;

            while (!string.IsNullOrEmpty(currentPath))
            {
                // Try to resolve if path exists
                if (File.Exists(currentPath) || Directory.Exists(currentPath))
                {
                    string resolved = GetFinalPathName(currentPath);
                    if (!string.IsNullOrEmpty(resolved))
                    {
                        return resolved;
                    }
                }

                // Move to parent directory
                string parentPath = Path.GetDirectoryName(currentPath);

                // Reached root or can't go higher
                if (string.IsNullOrEmpty(parentPath) || parentPath == currentPath)
                {
                    break;
                }

                currentPath = parentPath;
            }

            return null;
        }

        /// <summary>
        /// Gets the final path name after resolving all symbolic links and junctions.
        /// </summary>
        private static string GetFinalPathName(string path)
        {
            // FILE_FLAG_BACKUP_SEMANTICS is required to open directories
            uint flags = FILE_FLAG_BACKUP_SEMANTICS;

            IntPtr handle = CreateFileW(
                path,
                FILE_READ_ATTRIBUTES,
                FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE,
                IntPtr.Zero,
                OPEN_EXISTING,
                flags,
                IntPtr.Zero);

            if (handle == INVALID_HANDLE_VALUE)
            {
                return null;
            }

            try
            {
                var sb = new StringBuilder(512);

                uint result = GetFinalPathNameByHandleW(
                    handle,
                    sb,
                    (uint)sb.Capacity,
                    VOLUME_NAME_DOS);

                if (result == 0)
                {
                    return null;
                }

                // Buffer too small - resize and retry
                if (result > sb.Capacity)
                {
                    sb.Capacity = (int)result + 1;
                    result = GetFinalPathNameByHandleW(
                        handle,
                        sb,
                        (uint)sb.Capacity,
                        VOLUME_NAME_DOS);

                    if (result == 0 || result > sb.Capacity)
                    {
                        return null;
                    }
                }

                return NormalizePath(sb.ToString());
            }
            finally
            {
                CloseHandle(handle);
            }
        }

        /// <summary>
        /// Removes the extended path prefix returned by GetFinalPathNameByHandle.
        /// </summary>
        public static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            // UNC path: \\?\UNC\server\share -> \\server\share
            if (path.StartsWith(UNCPathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return @"\\" + path.Substring(8);
            }

            // Local path: \\?\C:\folder -> C:\folder
            if (path.StartsWith(LongPathPrefix, StringComparison.Ordinal))
            {
                return path.Substring(4);
            }

            return Path.GetFullPath(path)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}
