using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AIHelper.Manage.ui.themes
{
    internal class OldTheme1 : IUITheme
    {
        public string Name => "Old";
        Color _bg = Color.Gray;

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
                BackColor = Color.FromArgb(255, 102, 115, 117),
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
