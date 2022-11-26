using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage.Functions
{
    internal class UpdaterFlp : IFunctionForFlp
    {
        public string Symbol => T._("🌐");

        public string Description => T._("Check for updates");

        public void OnClick(object o, EventArgs e)
        {
            ManageUpdateMods.UpdateMods();
        }
    }
}
