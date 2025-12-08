using System.Collections.Generic;

namespace AIHelper.Manage.ModeSwitch.DataFullBuilder.Interfaces
{
    public interface IFileSystemHelper
    {
        bool DirectoryExists(string path);
        bool FileExists(string path);
        bool IsSymbolicLink(string path);
        bool IsValidSymbolicLink(string path);
        bool CreateHardLink(string linkPath, string targetPath);
        bool CreateSymbolicLink(string linkPath, string targetPath, bool isDirectory);
        bool CanReadFile(string path);
        string GetDriveRoot(string path);
        IEnumerable<string> EnumerateFiles(string path, bool recursive);
        IEnumerable<string> EnumerateDirectories(string path, bool recursive);
        void CreateDirectory(string path);
        string NormalizePath(string path);
        string GetLongPath(string path);
        string GetRelativePath(string basePath, string fullPath);
    }
}
