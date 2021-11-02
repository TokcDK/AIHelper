using System;
using System.Runtime.InteropServices;

namespace AIHelper.Manage
{
    static class ManageHardlinks
    {
        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        static extern bool CreateHardLink(
        string lpFileName,
        string lpExistingFileName,
        IntPtr lpSecurityAttributes
        );

        /// <summary>
        /// Create hardlink for selected file
        /// </summary>
        /// <param name="existingFileName"></param>
        /// <param name="fileName"></param>
        internal static void CreateHardlink(this string existingFileName, string fileName)
        {
            CreateHardLink(fileName, existingFileName, IntPtr.Zero);
        }
    }
}
