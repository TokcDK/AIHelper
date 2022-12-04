using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage.Functions
{
    public interface IFolderTabButtonData
    {
        string Text { get; }
        string Description { get; }

        void OnClick(object o, EventArgs e);
    }
}
