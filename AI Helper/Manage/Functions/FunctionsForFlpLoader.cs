using System;
using System.Drawing;
using System.Windows.Forms;
using AIHelper.Manage.ui.themes;

namespace AIHelper.Manage.Functions
{
    internal class FunctionsForFlpLoader
    {
        internal static void Load()
        {

            var flp = ManageSettings.MainForm.FunctionsFlowLayoutPanel;
            flp.Controls.Clear(); // clear old

            var functions = GetListOfSubClasses.Inherited.GetInterfaceImplimentations<IFunctionForFlp>();

            var ttip = new ToolTip()
            {
                // Set up the delays for the ToolTip.
                AutoPopDelay = 32000,
                InitialDelay = 1000,
                ReshowDelay = 500,
                UseAnimation = true,
                UseFading = true,
                // Force the ToolTip text to be displayed whether or not the form is active.
                ShowAlways = true
            };

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
        }
    }
}
