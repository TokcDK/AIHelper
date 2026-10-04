using System;
using System.Runtime.InteropServices;

namespace AIHelper.Manage
{
    /// <summary>
    /// Create hardlink for selected file
    /// </summary>
    internal static class ManageHardlinks
    {
        /// <summary>
        /// Makes <paramref name="fileName"/> another name for the file
        /// <paramref name="existingFileName"/> already has.
        /// </summary>
        /// <param name="existingFileName">The name the data already has.</param>
        /// <param name="fileName">The name to add.</param>
        internal static void CreateHardlink(this string existingFileName, string fileName)
        {
            NativeMethods.CreateHardLink(fileName, existingFileName, IntPtr.Zero);
        }
    }
}
