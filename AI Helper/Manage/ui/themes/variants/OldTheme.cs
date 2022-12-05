using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AIHelper.Manage.ui.themes
{
    internal class OldTheme : IUITheme
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
                BackColor = Color.Silver,
                ForeColor = Color.Black,
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
