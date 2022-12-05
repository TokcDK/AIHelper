using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage.ui.themes
{
    internal class DefaultTheme1 : IUITheme
    {
        public Color BackColorPanel => Color.FromArgb(255, 19, 54, 55);

        public Color BackColorButton => Color.FromArgb(255, 160, 107, 123);

        public Color ForeColorButton => Color.White;
    }
}
