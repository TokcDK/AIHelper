using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AIHelper.Manage.ui.themes
{
    internal class DefaultTheme1 : IUITheme
    {
        public string Name => "Default";

        public List<ElementData> Elements => new List<ElementData>()
        {
            new ElementData()
            {
                Type = typeof(Panel),
                BackColor = Color.Gray,
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
                BackColor = Color.Gray,
                ForeColor = Color.Black,
            },
        };
    }
}
