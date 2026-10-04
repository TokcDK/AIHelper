using System;
using System.Runtime.InteropServices;

namespace AIHelper.Manage
{
    /// <summary>
    /// The native calls this application needs and the framework does not expose.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The class carries the conventional name on purpose. CA1060 wants native
    /// declarations to sit in a class called <c>NativeMethods</c>, and this project
    /// builds with every analyser enabled while keeping its warning count fixed, so
    /// declaring them anywhere else would add a warning to the build.
    /// </para>
    /// <para>
    /// Only the declarations live here. What the application does with them belongs to
    /// the caller, so the code that needs a call stays with the work it serves rather
    /// than with the platform boundary.
    /// </para>
    /// </remarks>
    internal static class NativeMethods
    {
        // ------------------------------------------------------------------
        // Window appearance
        // ------------------------------------------------------------------

        /// <summary>
        /// Visual style Windows uses for a dark scroll bar.
        /// </summary>
        /// <remarks>
        /// An undocumented but stable name: the same one Explorer itself is drawn
        /// with when the system is in dark mode. Windows 10 1809 and newer know it;
        /// older versions simply keep the normal style.
        /// </remarks>
        internal const string DarkScrollBarTheme = "DarkMode_Explorer";

        /// <summary>
        /// Visual style Windows uses for a normal, light scroll bar.
        /// </summary>
        internal const string LightScrollBarTheme = "Explorer";

        /// <summary>
        /// Asks Windows to draw <paramref name="hWnd"/> - and the scroll bars it owns -
        /// with the visual style named <paramref name="subAppName"/>.
        /// </summary>
        /// <remarks>
        /// Returns a failure code rather than throwing when the style is unknown, so
        /// the result is deliberately not checked: an older Windows which does not
        /// know the dark style just keeps drawing the normal one.
        /// </remarks>
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        internal static extern int SetWindowTheme(IntPtr hWnd, string subAppName, string subIdList);

        /// <summary>
        /// The windows a combo box is made of.
        /// </summary>
        /// <remarks>
        /// Only the handle of the drop down list is used here, because that is the part
        /// which has a scroll bar of its own. It cannot be reached through the control
        /// tree: it is a separate popup window, not a child of the combo box.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        internal struct ComboBoxInfo
        {
            public int Size;

            public int ItemLeft;
            public int ItemTop;
            public int ItemRight;
            public int ItemBottom;

            public int ButtonLeft;
            public int ButtonTop;
            public int ButtonRight;
            public int ButtonBottom;

            public int ButtonState;

            public IntPtr ComboBoxHandle;
            public IntPtr EditHandle;

            /// <summary>
            /// The drop down list, which is the window whose scroll bar has to be
            /// recoloured. Zero until the combo box has a handle.
            /// </summary>
            public IntPtr ListHandle;
        }

        /// <summary>
        /// Fills in <paramref name="info"/> with the windows a combo box is made of.
        /// </summary>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetComboBoxInfo(IntPtr hWnd, ref ComboBoxInfo info);

        /// <summary>
        /// A device context for the whole window, frame included.
        /// </summary>
        /// <remarks>
        /// <see cref="System.Drawing.Graphics.FromHwnd"/> hands back a context for the
        /// client area only, whose origin is the inside of the frame, so a frame cannot be
        /// drawn with it. This one has the window's own origin, which is where the frame
        /// is. It has to be given back with <see cref="ReleaseDeviceContext"/>.
        /// </remarks>
        [DllImport("user32.dll")]
        internal static extern IntPtr GetWindowDC(IntPtr hWnd);

        /// <summary>
        /// Gives back a context taken with <see cref="GetWindowDC"/>.
        /// </summary>
        [DllImport("user32.dll")]
        internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        /// <summary>
        /// Adds the client area of a window to its update region.
        /// </summary>
        internal const uint RedrawInvalidate = 0x0001;

        /// <summary>
        /// Adds the frame of a window to its update region as well.
        /// </summary>
        /// <remarks>
        /// <see cref="System.Windows.Forms.Control.Invalidate()"/> reaches the client
        /// area only, and a frame drawn by the theme lives outside it, so recolouring a
        /// frame would not show until something else happened to repaint it.
        /// </remarks>
        internal const uint RedrawFrame = 0x0400;

        /// <summary>
        /// Marks a window for repainting, frame included when asked for.
        /// </summary>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool RedrawWindow(IntPtr hWnd, IntPtr updateRect, IntPtr updateRegion, uint flags);

        /// <summary>
        /// Show command that brings a window back from minimised without activating it.
        /// </summary>
        internal const int ShowRestore = 9;

        /// <summary>
        /// Shows a window in the state <paramref name="nCmdShow"/> asks for.
        /// </summary>
        /// <remarks>
        /// Windows does not restore a window on its own when a second copy of the
        /// application is started, so the running one has to be told to.
        /// <c>SetForegroundWindow</c> on its own is not enough.
        /// </remarks>
        /// <param name="hWnd">Handle of the window to show.</param>
        /// <param name="nCmdShow">
        /// The state to show it in, <see cref="ShowRestore"/> for a minimised window.
        /// </param>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // ------------------------------------------------------------------
        // Filesystem
        // ------------------------------------------------------------------

        /// <summary>
        /// Makes <paramref name="fileName"/> another name for the file
        /// <paramref name="existingFileName"/> already has.
        /// </summary>
        /// <remarks>
        /// The two names then refer to one set of data on disk, so a file edited through
        /// either of them is seen through both. Removing one name leaves the data in place
        /// as long as the other name still exists.
        /// </remarks>
        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CreateHardLink(
            string lpFileName,
            string lpExistingFileName,
            IntPtr lpSecurityAttributes);

        /// <summary>
        /// Marks a file for deletion on its next close, which is how Windows takes away
        /// the read-only block files downloaded from the internet carry.
        /// </summary>
        /// <remarks>
        /// Returns false instead of throwing when the file is not there, which is the case
        /// the callers treat as nothing to do.
        /// </remarks>
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteFile(string name);
    }
}
