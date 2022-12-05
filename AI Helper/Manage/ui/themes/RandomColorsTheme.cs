using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage.ui.themes
{
    internal class RandomColorsTheme : IUITheme
    {
        readonly Random _rnd = new Random();

        public Color BackColorPanel => Color.FromArgb(255, _rnd.Next(0, 255), _rnd.Next(0, 255), _rnd.Next(0, 255));

        public Color BackColorButton => Color.FromArgb(255, _rnd.Next(0, 255), _rnd.Next(0, 255), _rnd.Next(0, 255));

        public Color ForeColorButton => Color.FromArgb(255, _rnd.Next(0, 255), _rnd.Next(0, 255), _rnd.Next(0, 255));
    }
}
