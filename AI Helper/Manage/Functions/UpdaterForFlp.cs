using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage.Functions
{
    internal class UpdaterForFlp : FunctionForFlpBase
    {
        public override string Symbol => T._("🌐");

        public override string Description => T._("Check for updates");

        public override void OnClick(object o, EventArgs e)
        {
            ManageUpdateMods.UpdateMods();
        }
        public override Color? ForeColor => Color.LightBlue;
    }
}
