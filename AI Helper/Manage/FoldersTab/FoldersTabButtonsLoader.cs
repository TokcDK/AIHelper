using System;
using System.Drawing;
using System.Windows.Forms;

namespace AIHelper.Manage.Functions
{
    internal class FoldersTabButtonsLoader
    {
        internal static void Load()
        {
            var panel = ManageSettings.MainForm.FoldersTabPageBackgroundPanel;

            var flp = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill
            };

            var datas = GetListOfSubClasses.Inherited.GetInterfaceImplimentations<IFolderTabButtonData>();

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
