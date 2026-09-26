using System;
using System.Windows.Forms;
using AIHelper.Manage.ui.themes;

namespace AIHelper.Manage.ui
{
    /// <summary>
    /// The tool tip of a window, set up once and themed with the rest of it.
    /// </summary>
    /// <remarks>
    /// Every window used to build its own <see cref="ToolTip"/> with the same seven
    /// properties and its own copy of the code which removes it again. The delays
    /// live here now, so they are the same everywhere and a window only has to say
    /// what each control means.
    /// </remarks>
    internal sealed class UiTooltip : IDisposable
    {
        private readonly ToolTip _toolTip;

        internal UiTooltip()
        {
            _toolTip = new ToolTip
            {
                // the window shows long explanations, so the tip has to stay long
                // enough to be read and must not disappear while the pointer rests on
                // the control
                AutoPopDelay = 32000,
                InitialDelay = 1000,
                ReshowDelay = 500,
                UseAnimation = true,
                UseFading = true,
                ShowAlways = true,
            };

            ApplyTheme();
        }

        /// <summary>
        /// Explains a control. A control without a description gets no tip.
        /// </summary>
        internal void Set(Control control, string description)
        {
            if (control == null || string.IsNullOrEmpty(description)) return;

            _toolTip.SetToolTip(control, description);
        }

        /// <summary>
        /// Recolours the tip for the theme in use.
        /// </summary>
        internal void ApplyTheme()
        {
            var palette = ThemesLoader.CurrentPalette;
            if (palette == null) return;

            _toolTip.BackColor = palette.SurfaceAlt;
            _toolTip.ForeColor = palette.Text;
        }

        public void Dispose()
        {
            _toolTip.RemoveAll();
            _toolTip.Dispose();
        }
    }
}
