using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AIHelper.Manage.ui.themes;

namespace AIHelper.Manage.Functions
{
    internal class FunctionsForFlpLoader
    {
        internal static void Load()
        {
            var functions = GetListOfSubClasses.Inherited.GetInterfaceImplimentations<IFunctionForFlp>();

            var panel = ManageSettings.MainForm.FunctionsForFLPTableLayoutPanel;

            var flp = new FlowLayoutPanel();

            var ttip = new ToolTip();

            foreach (var f in functions)
            {
                var l = new Label
                {
                    AutoSize = true,
                    Text = string.IsNullOrWhiteSpace(f.Symbol) ? "f" : f.Symbol,
                };
                IUITheme theme = ManageSettings.CurrentTheme;
                if (f is IFunctionForFlpTextOptions fe)
                {
                    if (fe.Font != null) l.Font = fe.Font;
                    if (fe.ForeColor != null) l.ForeColor = (Color)fe.ForeColor;
                }
                l.Click += new EventHandler((o, e) =>
                {
                    // place last used control as first
                    //int i = 0;
                    //var controls = new Control[flp.Controls.Count];
                    //controls[i++] = o as Control;
                    //foreach (var flpc in flp.Controls)
                    //{
                    //    if (object.Equals(flpc, o)) continue;

                    //    controls[i++] = flpc as Control;
                    //}
                    //flp.Controls.AddRange(controls);

                    f.OnClick(o, e);
                });
                ttip.SetToolTip(l, f.Description);

                flp.Controls.Add(l);
            }

            if (flp.Controls.Count == 0) return;

            // setup for place in center of 1 cell sized panel
            // center flp elements settings got from here: https://stackoverflow.com/a/38824845
            flp.AutoSize = true;
            flp.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flp.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            flp.AutoScroll = true;
            flp.WrapContents = false;

            // Set up the delays for the ToolTip.
            ttip.AutoPopDelay = 32000;
            ttip.InitialDelay = 1000;
            ttip.ReshowDelay = 500;
            ttip.UseAnimation = true;
            ttip.UseFading = true;
            // Force the ToolTip text to be displayed whether or not the form is active.
            ttip.ShowAlways = true;

            panel.Controls.Add(flp);
        }
    }
}
