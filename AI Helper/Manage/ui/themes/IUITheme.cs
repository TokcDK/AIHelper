using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage.ui.themes
{
    public interface IUITheme
    {
        string Name { get; }
        IReadOnlyList<ElementData> Elements { get; }
    }

    public class ElementData
    {
        public Type Type { get; set; }

        public Color ForeColor { get; set; }
        public Color BackColor { get; set; }
        public Font Font { get; set; }
        public Image BackgroundImage { get; set; }
    }
}
