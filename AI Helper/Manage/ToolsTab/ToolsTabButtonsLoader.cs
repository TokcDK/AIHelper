using System;
using System.Drawing;
using System.Windows.Forms;
using AIHelper.Manage.ToolsTab;

namespace AIHelper.Manage.Functions
{
    internal class ToolsTabButtonsLoader
    {
        internal static void Load()
        {
            var panel = ManageSettings.MainForm.ToolsTabPageBackgroundPanel;

            var flp = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill
            };

            var datas = GetListOfSubClasses.Inherited.GetInterfaceImplimentations<IToolsTabButtonData>();

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

            foreach (var d in datas)
            {
                if (d is IButtonDataDev dev && !dev.IsVisible) continue;

                var b = new Button
                {
                    AutoSize = true,
                    Text = d.Text,
                    ForeColor = Color.White,
                };
                b.Click += new EventHandler((o, e) =>
                {
                    d.OnClick(o, e);
                });
                ttip.SetToolTip(b, d.Description);

                flp.Controls.Add(b);
            }

            panel.Controls.Add(flp);
        }
    }
}
