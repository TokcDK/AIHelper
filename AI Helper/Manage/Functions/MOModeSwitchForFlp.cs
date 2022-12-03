using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage.Functions
{
    internal class MOModeSwitchForFlp : FunctionForFlpBase
    {
        public override string Symbol => ManageSettings.IsMoMode? T._("MO"): T._("N");

        public override string Description => ManageSettings.IsMoMode ? T._("Switch to normal mode"): T._("Switch to MO mode");

        public override void OnClick(object o, EventArgs e)
        {
            ManageMOModeSwitch.SwitchBetweenMoAndStandartModes();
        }

        public override Color? ForeColor => ManageSettings.IsMoMode ? Color.Blue: Color.Yellow;
    }
}
