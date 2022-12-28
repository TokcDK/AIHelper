using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AIHelper.Manage.ui.themes
{
    internal class DarkTheme : IUITheme
    {
        public string Name => "Dark";

        public List<ElementData> Elements => new List<ElementData>()
        {
            new ElementData()
            {
                Type = typeof(Panel),
                BackColor = Color.FromArgb(255, 31,31,31),
            },
            new ElementData()
            {
                Type = typeof(Button),
                BackColor = Color.FromArgb(255, 56,56,56),
                ForeColor = Color.White,
            },
            new ElementData()
            {
                Type = typeof(TabControl),
                BackColor = Color.FromArgb(255, 56,56,56),
                ForeColor = Color.White,
            },
            new ElementData()
            {
                Type = typeof(Label),
                ForeColor = Color.White,
            },
            new ElementData()
            {
                Type = typeof(CheckBox),
                ForeColor = Color.White,
            },
            new ElementData()
            {
                Type = typeof(TextBox),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(255, 56,56,56),
            },
        };
    }
}
