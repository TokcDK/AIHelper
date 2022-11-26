using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
                var l = new System.Windows.Forms.Label
                {
                    AutoSize = true,
                    Text = f.Symbol
                };
                l.Click += f.OnClick;
                ttip.SetToolTip(l, f.Description);

                flp.Controls.Add(l);
            }
        }
    }
}
