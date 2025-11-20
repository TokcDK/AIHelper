using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AIHelper.Manage.ui.themes
{
    internal class DefaultTheme : IUITheme
    {
        public string Name => "Default";
        private readonly Color _bg = Color.FromArgb(255, 72, 85, 86);

        public IReadOnlyList<ElementData> Elements => new List<ElementData>()
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
        }.AsReadOnly();
    }
}
