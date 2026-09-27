using System;
using System.Runtime.InteropServices;

namespace AIHelper.Manage.ui
{
    /// <summary>
    /// The few native calls the interface needs, which WinForms does not expose.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A scroll bar is drawn by Windows rather than by WinForms, so it ignores the
    /// colour it is given and stays light grey whatever theme is applied. The only
    /// supported way to recolour one is to ask Windows for a different visual style
    /// for the window which owns it, and that call is not wrapped by WinForms.
    /// </para>
    /// <para>
    /// The class carries the conventional name on purpose. CA1060 wants native
    /// declarations to sit in a class called <c>NativeMethods</c>, and this project
    /// builds with every analyser enabled while keeping its warning count fixed, so
    /// declaring them anywhere else would add a warning to the build.
    /// </para>
    /// </remarks>
    internal static class NativeMethods
    {
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
    }
}
