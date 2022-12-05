using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AIHelper.Manage.ui.themes
{
    internal class DefaultTheme1 : IUITheme
    {
        public string Name => "Default";
        Color _bg = Color.FromArgb(255, 42, 57, 80);

        public List<ElementData> Elements => new List<ElementData>()
        {
            new ElementData()
            {
                Type = typeof(Panel),
                BackColor = _bg,
            },
            new ElementData()
            {
                Type = typeof(Button),
                BackColor = Color.FromArgb(255, 115, 96, 124),
                ForeColor = Color.White,
            },
            new ElementData()
            {
                Type = typeof(TabControl),
                BackColor = _bg,
                ForeColor = Color.Black,
            },
        };
    }
}
