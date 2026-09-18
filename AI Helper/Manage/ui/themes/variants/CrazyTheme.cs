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

        public IReadOnlyList<ElementData> Elements => new List<ElementData>()
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
        }.AsReadOnly();

        readonly Random _rnd = new Random();
        //Next(0, 256): Next's upper bound is exclusive, so 255 was never produced and no channel could be fully bright
        Color RandomColor => Color.FromArgb(255, _rnd.Next(0, 256), _rnd.Next(0, 256), _rnd.Next(0, 256));
    }
}
