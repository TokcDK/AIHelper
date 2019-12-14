using System.Runtime.InteropServices;

namespace AIHelper
{
    public static class CreateSymlink
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern bool CreateSymbolicLink(
        string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        enum SymbolicLink
        {
            File = 0,
            Directory = 1
        }

        public static void Folder(string folderforwichcreate, string symlinkwherecreate)
        {
            CreateSymbolicLink(symlinkwherecreate, folderforwichcreate, SymbolicLink.Directory);
        }

        public static void File(string fileforwichcreate, string symlinkwherecreate)
        {
            CreateSymbolicLink(symlinkwherecreate, fileforwichcreate, SymbolicLink.File);
        }
    }
}
