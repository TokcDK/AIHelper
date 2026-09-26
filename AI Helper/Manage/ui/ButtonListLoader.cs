using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AIHelper.Manage.ToolsTab;
using AIHelper.Manage.ui.themes;

namespace AIHelper.Manage.ui
{
    /// <summary>
    /// Builds a tab full of buttons from the button descriptions the assembly
    /// provides.
    /// </summary>
    /// <remarks>
    /// The tools tab and the folders tab were built by two loaders which differed
    /// only in the container they filled and in one filter. They share this one now,
    /// so a button looks and behaves the same on either tab.
    /// </remarks>
    internal static class ButtonListLoader
    {
        /// <summary>
        /// Explains the buttons. Replaced whenever the list is rebuilt, so the tip
        /// never outlives the buttons it describes.
        /// </summary>
        private static UiTooltip _tooltip;

        /// <summary>
        /// Fills <paramref name="host"/> with one button per entry of
        /// <paramref name="buttons"/>.
        /// </summary>
        internal static void Load(Control host, IEnumerable<IButtonData> buttons)
        {
            if (host == null) return;

            host.Controls.Clear();

            if (_tooltip != null)
            {
                _tooltip.Dispose();
                _tooltip = null;
            }

            var flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(12),
            };

            var tooltip = new UiTooltip();

            foreach (var data in buttons)
            {
                var devData = data as IButtonDataDev;
                if (devData != null && !devData.IsVisible) continue;

                var button = new Button
                {
                    Text = data.Text,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Padding = new Padding(12, 6, 12, 6),
                    Margin = new Padding(6),
                    MinimumSize = new Size(0, 34),
                };

                // the loop variable is captured by the handler, so it needs its own copy
                var clickedData = data;
                button.Click += new EventHandler((o, e) => clickedData.OnClick(o, e));

                tooltip.Set(button, data.Description);

                flowPanel.Controls.Add(button);
            }

            _tooltip = tooltip;

            host.Controls.Add(flowPanel);

            // the buttons are built while the window runs, so they have to be themed
            // after they exist
            ThemesLoader.ApplyTheme(flowPanel);
        }
    }
}
