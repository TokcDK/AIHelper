using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage.Functions.ModlistFixes
{
    internal class ModlistFIxesForFlp : IFunctionForFlp
    {
        public string Symbol => "F";

        public string Description => T._("Autodetect and fix issues in the current profile's modlist");

        public void OnClick(object o, EventArgs e)
        {
        }
    }
}
