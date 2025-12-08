using AIHelper.Manage.ModeSwitch.DataFullBuilder.Interfaces;
using NCode.ReparsePoints;
using System;
using System.Collections.Generic;
using System.IO;

namespace AIHelper.Manage.ModeSwitch.DataFullBuilder.Services
{
    public sealed class FileSystemHelper : IFileSystemHelper
    {
        private string LongPathPrefix => Win32PathHelper.LongPathPrefix;
        private string UNCPathPrefix => Win32PathHelper.UNCPathPrefix;

        private const int MaxPath = 260;

        public bool DirectoryExists(string path)
        {
            try
            {
                return Directory.Exists(GetLongPath(path));
            }
            catch
            {
                return false;
            }
        }

        public bool FileExists(string path)
        {
            try
            {
                return File.Exists(GetLongPath(path));
            }
            catch
            {
                return false;
            }
        }

        public bool IsSymbolicLink(string path)
        {
            try
            {
                var longPath = GetLongPath(path);
                var fileInfo = new FileInfo(longPath);
                return fileInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
            }
            catch
            {
                return false;
            }
        }

        public bool IsValidSymbolicLink(string path)
        {
            try
            {
                if (!IsSymbolicLink(path)) return true;

                var longPath = GetLongPath(path);
                
                // Try to resolve the link target
                if (File.Exists(longPath))
                {
                    using (var fs = File.OpenRead(longPath))
                    {
                        return true;
                    }
                }
                
                if (Directory.Exists(longPath))
                {
                    Directory.GetFiles(longPath);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
        public bool CreateHardLink(string linkPath, string targetPath)
        {
            // Uses external method as specified
            return CreateHardlink(linkPath, targetPath);
        }

        public bool CreateSymbolicLink(string linkPath, string targetPath, bool isDirectory)
        {
            // Uses external method as specified
            return CreateSymlink(linkPath, targetPath, isDirectory);
        }

        // External methods - implementation provided externally
        private static bool CreateHardlink(string target, string source)
        {
            ReparsePointFactory.Provider.CreateLink(target, source, LinkType.HardLink);

            return File.Exists(target);
        }

        private static bool CreateSymlink(string target, string source, bool isFolder)
        {
            ReparsePointFactory.Provider.CreateLink(target, source, LinkType.Symbolic);

            return File.Exists(target);
        }

        public bool CanReadFile(string path)
        {
            try
            {
                var longPath = GetLongPath(path);
                using (var fs = new FileStream(longPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public string GetDriveRoot(string path)
        {
            try
            {
                var normalizedPath = NormalizePath(path);
                var driveRoot = Win32PathHelper.GetDriveRoot(normalizedPath);
                return driveRoot.ToUpperInvariant(); // Path.GetPathRoot(normalizedPath)?.ToUpperInvariant() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public IEnumerable<string> EnumerateFiles(string path, bool recursive)
        {
            var longPath = GetLongPath(path);
            var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            
            return EnumerateSafe(
                () => Directory.EnumerateFiles(longPath, "*", option),
                path);
        }

        public IEnumerable<string> EnumerateDirectories(string path, bool recursive)
        {
            var longPath = GetLongPath(path);
            var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            
            return EnumerateSafe(
                () => Directory.EnumerateDirectories(longPath, "*", option),
                path);
        }

        private IEnumerable<string> EnumerateSafe(Func<IEnumerable<string>> enumerator, string basePath)
        {
            IEnumerator<string> iterator;
            try
            {
                iterator = enumerator().GetEnumerator();
            }
            catch
            {
                yield break;
            }

            while (true)
            {
                string current;
                try
                {
                    if (!iterator.MoveNext()) break;
                    current = iterator.Current;
                }
                catch
                {
                    continue;
                }

                yield return current;
            }

            iterator?.Dispose();
        }

        public void CreateDirectory(string path)
        {
            var longPath = GetLongPath(path);
            Directory.CreateDirectory(longPath);
        }

        public string NormalizePath(string path)
        {
            return Win32PathHelper.NormalizePath(path);
        }

        public string GetLongPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            
            // Already has prefix
            if (path.StartsWith(LongPathPrefix)) return path;
            
            // UNC paths
            if (path.StartsWith(@"\\"))
            {
                return UNCPathPrefix + path.Substring(2);
            }

            var normalizedPath = NormalizePath(path);
            
            // Only add prefix for long paths
            if (normalizedPath.Length >= MaxPath)
            {
                return LongPathPrefix + normalizedPath;
            }

            return normalizedPath;
        }

        public string GetRelativePath(string basePath, string fullPath)
        {
            var normalizedBase = NormalizePath(basePath);
            var normalizedFull = NormalizePath(fullPath);

            if (!normalizedFull.StartsWith(normalizedBase, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"Path '{fullPath}' is not under base path '{basePath}'");
            }

            var relativePath = normalizedFull.Substring(normalizedBase.Length);
            return relativePath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}
