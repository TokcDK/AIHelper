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
        Color BackColorPanel { get; }
        Color BackColorButton { get; }
        Color ForeColorButton { get; }
    }
}
