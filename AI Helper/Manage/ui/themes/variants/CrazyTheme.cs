using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIHelper.Manage.ui.themes
{
    internal class CrazyTheme : IUITheme
    {
        public string Name => "Crazy";

        public List<ElementData> Elements => new List<ElementData>()
        {
            new ElementData()
            {
                Type = typeof(Panel),
                BackColor = RandomColor,
            },
            new ElementData()
            {
                Type = typeof(Button),
                BackColor = RandomColor,
                ForeColor = RandomColor,
            },
            new ElementData()
            {
                Type = typeof(TabControl),
                BackColor = RandomColor,
                ForeColor = RandomColor,
            },
        };

        readonly Random _rnd = new Random();
        Color RandomColor => Color.FromArgb(255, _rnd.Next(0, 255), _rnd.Next(0, 255), _rnd.Next(0, 255));
    }
}
