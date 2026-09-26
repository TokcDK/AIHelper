using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AIHelper.Manage.ui.themes;

namespace AIHelper.Manage.ui
{
    /// <summary>
    /// Gives the scroll bars of a control the colour of the theme.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A scroll bar is drawn by Windows, not by WinForms, so it ignores the colour it
    /// is given and stayed light grey in every theme, including the dark ones. There is
    /// no managed way to recolour one; the window which owns the scroll bar has to be
    /// asked for a different visual style instead.
    /// </para>
    /// <para>
    /// A scroll bar only exists once its control has a window handle, and the theme is
    /// often applied before that - a window which has not been shown yet has no handle
    /// at all. The style is therefore asked for again whenever a handle appears, and
    /// the handler reads the theme which is current at that moment rather than
    /// remembering the one which was current when it was attached, so a handle created
    /// after a theme change still gets the right style.
    /// </para>
    /// <para>
    /// This covers the controls whose scroll bars are visible in this application: the
    /// scrollable panels, the text boxes, the lists and the drop down list of a combo
    /// box. A window's own border scroll bars are a platform feature and are not
    /// touched.
    /// </para>
    /// </remarks>
    internal static class ThemedScrollBars
    {
        /// <summary>
        /// Asks Windows to draw the scroll bars of <paramref name="control"/> to match
        /// a light or a dark theme.
        /// </summary>
        /// <remarks>
        /// Safe to call repeatedly and safe to call before the control has a handle,
        /// which is what makes it usable from a theme pass.
        /// </remarks>
        internal static void Apply(Control control, bool isDark)
        {
            if (control == null) return;

            ApplyToHandle(control, GetStyleName(isDark));

            // attached once, and re-attached with the event removed first, the same way
            // the tab strip wires its drawing
            control.HandleCreated -= OnHandleCreated;
            control.HandleCreated += OnHandleCreated;
        }

        private static string GetStyleName(bool isDark)
        {
            return isDark ? NativeMethods.DarkScrollBarTheme : NativeMethods.LightScrollBarTheme;
        }

        /// <summary>
        /// Gives a control which has just been created a scroll bar in the current
        /// theme, rather than the one which was current when the theme was applied.
        /// </summary>
        private static void OnHandleCreated(object sender, EventArgs e)
        {
            var control = sender as Control;
            if (control == null) return;

            var palette = ThemesLoader.CurrentPalette;
            if (palette == null) return;

            ApplyToHandle(control, GetStyleName(palette.IsDark));
        }

        private static void ApplyToHandle(Control control, string styleName)
        {
            if (!control.IsHandleCreated) return;

            NativeMethods.SetWindowTheme(control.Handle, styleName, null);

            var comboBox = control as ComboBox;
            if (comboBox != null)
            {
                ApplyToComboBoxList(comboBox, styleName);
            }
        }

        /// <summary>
        /// Recolours the scroll bar of the list a combo box drops down.
        /// </summary>
        /// <remarks>
        /// The list is a popup window of its own rather than a child of the combo box,
        /// so it is reached through the combo box info rather than through the control
        /// tree. It does not exist before the combo box has a handle, and the call
        /// simply fails when it is not there yet.
        /// </remarks>
        private static void ApplyToComboBoxList(ComboBox comboBox, string styleName)
        {
            var info = new NativeMethods.ComboBoxInfo();
            info.Size = Marshal.SizeOf(typeof(NativeMethods.ComboBoxInfo));

            if (!NativeMethods.GetComboBoxInfo(comboBox.Handle, ref info)) return;
            if (info.ListHandle == IntPtr.Zero) return;

            NativeMethods.SetWindowTheme(info.ListHandle, styleName, null);
        }
    }
}
