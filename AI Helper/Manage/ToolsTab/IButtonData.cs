using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage.ToolsTab
{
    public interface IButtonData
    {
        string Text { get; }
        string Description { get; }

        void OnClick(object o, EventArgs e);
    }
}
