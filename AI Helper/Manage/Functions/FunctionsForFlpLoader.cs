using System;
using System.Drawing;
using System.Windows.Forms;
using AIHelper.Manage.ui;
using AIHelper.Manage.ui.themes;

namespace AIHelper.Manage.Functions
{
    /// <summary>
    /// Builds the row of function symbols shown under the launch buttons.
    /// </summary>
    internal static class FunctionsForFlpLoader
    {
        /// <summary>
        /// Explains the symbols. Replaced whenever the row is rebuilt, so the tip
        /// never outlives the labels it describes.
        /// </summary>
        private static UiTooltip _tooltip;

        internal static void Load()
        {
            var panel = ManageSettings.MainForm.FunctionsForFLPTableLayoutPanel;

            // the row is rebuilt when the game changes as well as when the tab is
            // opened, so the previous one has to go first or the symbols pile up
            panel.Controls.Clear();

            if (_tooltip != null)
            {
                _tooltip.Dispose();
                _tooltip = null;
            }

            var functions = GetListOfSubClasses.Inherited.GetInterfaceImplementations<IFunctionForFlp>();
            var palette = ThemesLoader.CurrentPalette;

            var flowPanel = new FlowLayoutPanel
            {
                // centering inside the single cell this is put into
                Anchor = AnchorStyles.None,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                AutoScroll = true,
                WrapContents = false,
                Margin = new Padding(0),
            };

            var tooltip = new UiTooltip();

            foreach (var function in functions)
            {
                // a function which cannot run right now is drawn as unavailable and does not
                // react to a click, so the reason is visible before it is used
                var availability = function as IFunctionForFlpAvailability;
                var isAvailable = availability == null || availability.IsAvailable;

                var symbol = new Label
                {
                    AutoSize = true,
                    Text = string.IsNullOrWhiteSpace(function.Symbol) ? "f" : function.Symbol,
                    // a function picks its own colour to tell it apart from the others,
                    // so the theme must leave this label alone
                    Name = "flpf" + ManageSettings.ThemeLabelColorSetIgnoreNameMark,
                    Cursor = isAvailable ? Cursors.Hand : Cursors.Default,
                    Margin = new Padding(6, 2, 6, 2),
                };

                if (isAvailable)
                {
                    var textOptions = function as IFunctionForFlpTextOptions;
                    if (textOptions != null)
                    {
                        if (textOptions.Font != null) symbol.Font = textOptions.Font;

                        if (textOptions.ForeColor != null)
                        {
                            var color = (Color)textOptions.ForeColor;
                            symbol.ForeColor = palette == null ? color : palette.MakeReadable(color);
                        }
                    }

                    // the loop variable is captured by the handler, so it needs its own copy
                    var clickedFunction = function;
                    symbol.Click += new EventHandler((o, e) => clickedFunction.OnClick(o, e));
                }
                else
                {
                    // the colour of what cannot be used belongs to the theme, so it stays
                    // readable on the surface it is drawn on
                    symbol.ForeColor = palette == null ? SystemColors.GrayText : palette.ButtonDisabledText;
                }

                tooltip.Set(symbol, function.Description);

                flowPanel.Controls.Add(symbol);
            }

            if (flowPanel.Controls.Count == 0)
            {
                tooltip.Dispose();
                return;
            }

            _tooltip = tooltip;

            panel.Controls.Add(flowPanel);

            // the labels are placed by hand, so the panel they sit on is the only
            // thing which needs a colour
            ThemesLoader.ApplyTheme(flowPanel);
        }
    }
}
