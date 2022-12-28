using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AIHelper.Manage.ui.themes
{
    internal class ForestTheme : IUITheme
    {
        public string Name => "Forest";

        public List<ElementData> Elements => new List<ElementData>()
        {
            new ElementData()
            {
                Type = typeof(Panel),
                BackColor = Color.FromArgb(255, 45, 94, 46),
            },
            new ElementData()
            {
                Type = typeof(Button),
                BackColor = Color.FromArgb(255, 152, 188, 98),
                ForeColor = Color.Black,
            },
            new ElementData()
            {
                Type = typeof(TabControl),
                BackColor = Color.FromArgb(255, 152, 188, 98),
                ForeColor = Color.Black,
            },
            new ElementData()
            {
                Type = typeof(Label),
                ForeColor = Color.Black,
            },
            new ElementData()
            {
                Type = typeof(CheckBox),
                ForeColor = Color.Black,
            },
            new ElementData()
            {
                Type = typeof(TextBox),
                ForeColor = Color.Black,
                BackColor = Color.FromArgb(255, 152, 188, 98),
            },
        };
    }
}
