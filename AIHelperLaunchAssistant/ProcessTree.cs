using System;
using System.Runtime.InteropServices;

namespace AIHelperLaunchAssistant
{
    /// <summary>
    /// Reads the shape of the running process tree.
    /// </summary>
    internal static class ProcessTree
    {
        private static readonly int EntrySize = Marshal.SizeOf(typeof(Win32.ProcessEntry32));

        /// <summary>
        /// Finds the parent of <paramref name="processId"/>.
        /// </summary>
        /// <remarks>
        /// The assistant is normally started by the very instance it has to wait for, so the parent id
        /// is a better answer than the process name: it is that instance, not any AI Helper the user
        /// happens to have open. A parent id is only true for the moment it is read - the parent may
        /// have exited and its id been reused since - which is why the caller checks the name as well.
        /// </remarks>
        internal static bool TryGetParentProcessId(int processId, out int parentProcessId)
        {
            parentProcessId = 0;

            IntPtr snapshot = Win32.CreateToolhelp32Snapshot(Win32.SnapshotProcesses, 0);
            if (snapshot == Win32.InvalidHandleValue)
            {
                return false;
            }

            try
            {
                var entry = new Win32.ProcessEntry32();
                entry.Size = (uint)EntrySize;

                if (!Win32.Process32First(snapshot, ref entry))
                {
                    return false;
                }

                do
                {
                    if (entry.ProcessId == (uint)processId)
                    {
                        parentProcessId = (int)entry.ParentProcessId;
                        return parentProcessId > 0;
                    }

                    // Process32Next writes through the same structure, and the size has to be set again
                    // for every call on a structure that has already been filled in once.
                    entry.Size = (uint)EntrySize;
                }
                while (Win32.Process32Next(snapshot, ref entry));
            }
            finally
            {
                Win32.CloseHandle(snapshot);
            }

            return false;
        }
    }
}
