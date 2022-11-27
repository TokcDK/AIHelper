using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage.Functions
{
    internal class InfoEditorForFlp : IFunctionForFlp
    {
        public string Symbol => T._("⚒");

        public string Description => T._("Open mods update info editor");

        public void OnClick(object o, EventArgs e)
        {
            ManageInfoEditor.OpenInfoEditor();
        }
    }
}
