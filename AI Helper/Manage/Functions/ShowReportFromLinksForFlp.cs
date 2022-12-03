using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage.Functions
{
    internal class ShowReportFromLinksForFlp : FunctionForFlpBase
    {
        public override string Symbol => T._("🔗");

        public override string Description => T._("Open links to useful game related resources");

        public override void OnClick(object o, EventArgs e)
        {
            ManageReport.ShowReportFromLinks();
        }
        public override Color? ForeColor => Color.GreenYellow;
    }
}
